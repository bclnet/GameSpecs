using System.Diagnostics;
using System.Runtime.CompilerServices;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Framework.Async
{
    public class MsgQueue
    {
        const int MAX_MSG_QUEUE_SIZE = 16384;      // must be a power of 2

        byte[] buffer = new byte[MAX_MSG_QUEUE_SIZE];
        int first;          // sequence number of first message in queue
        int last;           // sequence number of last message in queue
        int startIndex;     // index pointing to the first byte of the first message
        int endIndex;       // index pointing to the first byte after the last message

        public MsgQueue()
            => Init(0);

        public void Init(int sequence)
        {
            first = last = sequence;
            startIndex = endIndex = 0;
        }

        public bool Add(byte[] data, int offset, int size)
        {
            if (SpaceLeft < size + 8) return false;
            int sequence = last;
            WriteShort(size);
            WriteInt(sequence);
            WriteData(data, offset, size);
            last++;
            return true;
        }

        public bool Get(byte[] data, out int size)
        {
            if (first == last) { size = 0; return false; }
            int sequence;
            size = ReadShort();
            sequence = ReadInt();
            ReadData(data, size);
            Debug.Assert(sequence == first);
            first++;
            return true;
        }

        public int TotalSize
            => startIndex <= endIndex
            ? endIndex - startIndex
            : MAX_MSG_QUEUE_SIZE - startIndex + endIndex;

        public int SpaceLeft
            => startIndex <= endIndex
            ? MAX_MSG_QUEUE_SIZE - (endIndex - startIndex) - 1
            : (startIndex - endIndex) - 1;

        public int First
            => first;

        public int Last
            => last;

        public void CopyToBuffer(byte[] buf, int offset)
        {
            if (startIndex <= endIndex) Unsafe.CopyBlock(ref buf[offset], ref buffer[startIndex], (uint)(endIndex - startIndex));
            else
            {
                Unsafe.CopyBlock(ref buf[offset], ref buffer[startIndex], (uint)(MAX_MSG_QUEUE_SIZE - startIndex));
                Unsafe.CopyBlock(ref buf[offset + MAX_MSG_QUEUE_SIZE - startIndex], ref buffer[offset], (uint)endIndex);
            }
        }

        void WriteByte(byte b)
        {
            buffer[endIndex] = b;
            endIndex = (endIndex + 1) & (MAX_MSG_QUEUE_SIZE - 1);
        }
        byte ReadByte()
        {
            var b = buffer[startIndex];
            startIndex = (startIndex + 1) & (MAX_MSG_QUEUE_SIZE - 1);
            return b;
        }

        void WriteShort(int s)
        {
            WriteByte((byte)((s >> 0) & 255));
            WriteByte((byte)((s >> 8) & 255));
        }
        int ReadShort()
            => ReadByte() | (ReadByte() << 8);

        void WriteInt(int l)
        {
            WriteByte((byte)((l >> 0) & 255));
            WriteByte((byte)((l >> 8) & 255));
            WriteByte((byte)((l >> 16) & 255));
            WriteByte((byte)((l >> 24) & 255));
        }
        int ReadInt()
            => ReadByte() | (ReadByte() << 8) | (ReadByte() << 16) | (ReadByte() << 24);

        void WriteData(byte[] data, int offset, int size)
        {
            for (var i = 0; i < size; i++) WriteByte(data[offset + i]);
        }
        void ReadData(byte[] data, int size)
        {
            if (data != null) for (var i = 0; i < size; i++) data[i] = ReadByte();
            else for (var i = 0; i < size; i++) ReadByte();
        }
    }

    public class MsgChannel
    {
        const int MAX_PACKETLEN = 1400;     // max size of a network packet
        const int FRAGMENT_SIZE = MAX_PACKETLEN - 100;
        const int FRAGMENT_BIT = 1 << 31;

        static CVar net_channelShowPackets = new("net_channelShowPackets", "0", CVAR.SYSTEM | CVAR.BOOL, "show all packets");
        static CVar net_channelShowDrop = new("net_channelShowDrop", "0", CVAR.SYSTEM | CVAR.BOOL, "show dropped packets");

        public const int MAX_MESSAGE_SIZE = 16384;      // max length of a message, which may
                                                        // be fragmented into multiple packets
        public const int CONNECTIONLESS_MESSAGE_ID = -1;            // id for connectionless messages
        public const int CONNECTIONLESS_MESSAGE_ID_MASK = 0x7FFF;       // value to mask away connectionless message id

        Netadr remoteAddress;   // address of remote host
        int id;             // our identification used instead of port number
        int maxRate;        // maximum number of bytes that may go out per second
        VCompressor compressor;      // compressor used for data compression

        // variables to control the outgoing rate
        int lastSendTime;   // last time data was sent out
        int lastDataBytes;  // bytes left to send at last send time

        // variables to keep track of the rate
        int outgoingRateTime;
        int outgoingRateBytes;
        int incomingRateTime;
        int incomingRateBytes;

        // variables to keep track of the compression ratio
        float outgoingCompression;
        float incomingCompression;

        // variables to keep track of the incoming packet loss
        float incomingReceivedPackets;
        float incomingDroppedPackets;
        int incomingPacketLossTime;

        // sequencing variables
        int outgoingSequence;
        int incomingSequence;

        // outgoing fragment buffer
        bool unsentFragments;
        int unsentFragmentStart;
        byte[] unsentBuffer = new byte[MAX_MESSAGE_SIZE];
        BitMsg unsentMsg;

        // incoming fragment assembly buffer
        int fragmentSequence;
        int fragmentLength;
        byte[] fragmentBuffer = new byte[MAX_MESSAGE_SIZE];

        // reliable messages
        MsgQueue reliableSend;
        MsgQueue reliableReceive;

        public MsgChannel()
           => id = -1;

        // Opens a channel to a remote system.
        public void Init(Netadr adr, int id)
        {
            this.remoteAddress = adr;
            this.id = id;
            this.maxRate = 50000;
            this.compressor = VCompressor.AllocRunLength_ZeroBased();

            lastSendTime = 0;
            lastDataBytes = 0;
            outgoingRateTime = 0;
            outgoingRateBytes = 0;
            incomingRateTime = 0;
            incomingRateBytes = 0;
            incomingReceivedPackets = 0f;
            incomingDroppedPackets = 0f;
            incomingPacketLossTime = 0;
            outgoingCompression = 0f;
            incomingCompression = 0f;
            outgoingSequence = 1;
            incomingSequence = 0;
            unsentFragments = false;
            unsentFragmentStart = 0;
            fragmentSequence = 0;
            fragmentLength = 0;
            reliableSend.Init(1);
            reliableReceive.Init(0);
        }

        public void Shutdown()
            => compressor = null;

        public void ResetRate()
        {
            lastSendTime = 0;
            lastDataBytes = 0;
            outgoingRateTime = 0;
            outgoingRateBytes = 0;
            incomingRateTime = 0;
            incomingRateBytes = 0;
        }

        // Gets or Sets the maximum outgoing rate.
        public int MaxOutgoingRate
        {
            get => maxRate;
            set => maxRate = value;
        }

        // Returns the address of the entity at the other side of the channel.
        public Netadr RemoteAddress
            => remoteAddress;

        // Returns the average outgoing rate over the last second.
        public int OutgoingRate
            => outgoingRateBytes;

        // Returns the average incoming rate over the last second.
        public int IncomingRate
            => incomingRateBytes;

        // Returns the average outgoing compression ratio over the last second.
        public float OutgoingCompression
            => outgoingCompression;

        // Returns the average incoming compression ratio over the last second.
        public float IncomingCompression
            => incomingCompression;

        // Returns the average incoming packet loss over the last 5 seconds.
        public float IncomingPacketLoss
            => incomingReceivedPackets == 0f && incomingDroppedPackets == 0f
            ? 0f
            : incomingDroppedPackets * 100f / (incomingReceivedPackets + incomingDroppedPackets);

        // Returns true if the channel is ready to send new data based on the maximum rate.
        public bool ReadyToSend(int time)
        {
            if (maxRate == 0) return true;
            var deltaTime = time - lastSendTime;
            if (deltaTime > 1000) return true;
            return (lastDataBytes - deltaTime * maxRate / 1000) <= 0;
        }

        // Sends an unreliable message, in order and without duplicates.
        // Sends a message to a connection, fragmenting if necessary A 0 length will still generate a packet.
        public int SendMessage(NetPort port, int time, BitMsg msg)
        {
            if (remoteAddress.type == NA.BAD) return -1;

            if (unsentFragments) { common.Error("MsgChannel::SendMessage: called with unsent fragments left"); return -1; }

            var totalLength = 4 + reliableSend.TotalSize + 4 + msg.Size;

            if (totalLength > MAX_MESSAGE_SIZE) { common.Printf($"MsgChannel::SendMessage: message too large, length = {totalLength}\n"); return -1; }

            unsentMsg.InitW(unsentBuffer);
            unsentMsg.BeginWriting();

            // fragment large messages
            if (totalLength >= FRAGMENT_SIZE)
            {
                unsentFragments = true;
                unsentFragmentStart = 0;

                // write out the message data
                WriteMessageData(unsentMsg, msg);

                // send the first fragment now
                SendNextFragment(port, time);

                return outgoingSequence;
            }

            // write the header
            unsentMsg.WriteShort(id);
            unsentMsg.WriteInt(outgoingSequence);

            // write out the message data
            WriteMessageData(unsentMsg, msg);

            // send the packet
            port.SendPacket(remoteAddress, unsentMsg.DataW, unsentMsg.Size);

            // update rate control variables
            UpdateOutgoingRate(time, unsentMsg.Size);

            if (net_channelShowPackets.Bool) common.Printf($"{id} send {unsentMsg.Size:4} : s = {outgoingSequence - 1} ack = {incomingSequence}\n");

            outgoingSequence++;

            return outgoingSequence - 1;
        }

        // Sends the next fragment if the last message was too large to send at once.
        // Sends one fragment of the current message.
        public void SendNextFragment(NetPort port, int time)
        {
            BitMsg msg = new(); byte[] msgBuf = new byte[MAX_PACKETLEN];
            int fragLength;

            if (remoteAddress.type == NA.BAD) return;

            if (!unsentFragments) return;

            // write the packet
            msg.InitW(msgBuf);
            msg.WriteShort(id);
            msg.WriteInt(outgoingSequence | FRAGMENT_BIT);

            fragLength = FRAGMENT_SIZE;
            if (unsentFragmentStart + fragLength > unsentMsg.Size) fragLength = unsentMsg.Size - unsentFragmentStart;

            msg.WriteShort(unsentFragmentStart);
            msg.WriteShort(fragLength);
            msg.WriteData(unsentMsg.DataW, unsentFragmentStart, fragLength);

            // send the packet
            port.SendPacket(remoteAddress, msg.DataW, msg.Size);

            // update rate control variables
            UpdateOutgoingRate(time, msg.Size);

            if (net_channelShowPackets.Bool) common.Printf($"{id} send {msg.Size:4} : s = {outgoingSequence - 1} fragment = {unsentFragmentStart},{fragLength}\n");

            unsentFragmentStart += fragLength;

            // this exit condition is a little tricky, because a packet that is exactly the fragment length still needs to send
            // a second packet of zero length so that the other side can tell there aren't more to follow
            if (unsentFragmentStart == unsentMsg.Size && fragLength != FRAGMENT_SIZE) { outgoingSequence++; unsentFragments = false; }
        }

        // Returns true if there are unsent fragments left.
        public bool UnsentFragmentsLeft
            => unsentFragments;

        // Processes the incoming message. Returns true when a complete message is ready for further processing. In that case the read pointer of msg
        // points to the first byte ready for reading, and sequence is set to the sequence number of the message.
        // Returns false if the message should not be processed due to being out of order or a fragment.
        // msg must be large enough to hold MAX_MESSAGE_SIZE, because if this is the final fragment of a multi-part message, the entire thing will be copied out.
        public bool Process(Netadr from, int time, BitMsg msg, out int sequence)
        {
            int fragStart, fragLength, dropped;
            bool fragmented;
            BitMsg fragMsg = new();

            // the IP port can't be used to differentiate them, because some address translating routers periodically change UDP port assignments
            if (remoteAddress.port != from.port) { common.Printf("MsgChannel::Process: fixing up a translated port\n"); remoteAddress.port = from.port; }

            // update incoming rate
            UpdateIncomingRate(time, msg.Size);

            // get sequence numbers
            sequence = msg.ReadInt();

            // check for fragment information
            if ((sequence & FRAGMENT_BIT) != 0) { sequence &= ~FRAGMENT_BIT; fragmented = true; }
            else fragmented = false;

            // read the fragment information
            if (fragmented) { fragStart = msg.ReadShort(); fragLength = msg.ReadShort(); }
            else { fragStart = 0; fragLength = 0; }      // stop warning message

            if (net_channelShowPackets.Bool)
                common.Printf(fragmented
                    ? $"{id} recv {msg.Size:4} : s = {sequence} fragment = {fragStart},{fragLength}\n"
                    : $"{id} recv {msg.Size:4} : s = {sequence}\n");

            // discard out of order or duplicated packets
            if (sequence <= incomingSequence)
            {
                if (net_channelShowDrop.Bool || net_channelShowPackets.Bool) common.Printf($"{remoteAddress}: out of order packet {sequence} at {incomingSequence}\n");
                return false;
            }

            // dropped packets don't keep this message from being used
            dropped = sequence - (incomingSequence + 1);
            if (dropped > 0)
            {
                if (net_channelShowDrop.Bool || net_channelShowPackets.Bool) common.Printf("{SysW.NetAdrToString(remoteAddress)}: dropped {dropped} packets at {sequence}\n");
                UpdatePacketLoss(time, 0, dropped);
            }

            // if the message is fragmented
            if (fragmented)
            {
                // make sure we have the correct sequence number
                if (sequence != fragmentSequence) { fragmentSequence = sequence; fragmentLength = 0; }

                // if we missed a fragment, dump the message
                if (fragStart != fragmentLength)
                {
                    if (net_channelShowDrop.Bool || net_channelShowPackets.Bool) common.Printf($"{remoteAddress}: dropped a message fragment at seq {sequence}\n");
                    // we can still keep the part that we have so far, so we don't need to clear fragmentLength
                    UpdatePacketLoss(time, 0, 1);
                    return false;
                }

                // copy the fragment to the fragment buffer
                if (fragLength < 0 || fragLength > msg.RemaingData || fragmentLength + fragLength > MAX_MESSAGE_SIZE)
                {
                    if (net_channelShowDrop.Bool || net_channelShowPackets.Bool) common.Printf($"{remoteAddress}: illegal fragment length\n");
                    UpdatePacketLoss(time, 0, 1);
                    return false;
                }

                Unsafe.CopyBlock(ref fragmentBuffer[fragmentLength], ref msg.DataW[msg.ReadCount], (uint)fragLength);

                fragmentLength += fragLength;

                UpdatePacketLoss(time, 1, 0);

                // if this wasn't the last fragment, don't process anything
                if (fragLength == FRAGMENT_SIZE) return false;
            }
            else
            {
                Unsafe.CopyBlock(ref fragmentBuffer[0], ref msg.DataW[msg.ReadCount], (uint)msg.RemaingData);
                fragmentLength = msg.RemaingData;
                UpdatePacketLoss(time, 1, 0);
            }

            fragMsg.InitW(fragmentBuffer, fragmentLength);
            fragMsg.Size = fragmentLength;
            fragMsg.BeginReading();

            incomingSequence = sequence;

            // read the message data
            return ReadMessageData(msg, fragMsg);
        }

        // Sends a reliable message, in order and without duplicates.
        public bool SendReliableMessage(BitMsg msg)
        {
            Debug.Assert(remoteAddress.type != NA.BAD);
            if (remoteAddress.type == NA.BAD) return false;
            var result = reliableSend.Add(msg.DataW, 0, msg.Size);
            if (!result) { common.Warning("MsgChannel::SendReliableMessage: overflowed"); return false; }
            return result;
        }

        // Returns true if a new reliable message is available and stores the message.
        public bool GetReliableMessage(BitMsg msg)
        {
            var result = reliableReceive.Get(msg.DataW, out var size);
            msg.Size = size;
            msg.BeginReading();
            return result;
        }

        // Removes any pending outgoing or incoming reliable messages.
        public void ClearReliableMessages()
        {
            reliableSend.Init(1);
            reliableReceive.Init(0);
        }

        void WriteMessageData(BitMsg o, BitMsg msg)
        {
            BitMsg tmp = new(); byte[] tmpBuf = new byte[MAX_MESSAGE_SIZE];

            tmp.InitW(tmpBuf);

            // write acknowledgement of last received reliable message
            tmp.WriteInt(reliableReceive.Last);

            // write reliable messages
            reliableSend.CopyToBuffer(tmp.DataW, tmp.Size);
            tmp.Size = tmp.Size + reliableSend.TotalSize;
            tmp.WriteShort(0);

            // write data
            tmp.WriteData(msg.DataW, 0, msg.Size);

            // write message size
            o.WriteShort(tmp.Size);

            // compress message
            var file = new VFile_BitMsg(o);
            compressor.Init(file, true, 3);
            compressor.Write(tmp.DataW, tmp.Size);
            compressor.FinishCompress();
            outgoingCompression = compressor.CompressionRatio;
        }

        bool ReadMessageData(BitMsg o, BitMsg msg)
        {
            int reliableAcknowledge, reliableMessageSize, reliableSequence;

            // read message size
            o.Size = msg.ReadShort();

            // decompress message
            var file = new VFile_BitMsg(msg);
            compressor.Init(file, false, 3);
            compressor.Read(o.DataR, o.Size);
            incomingCompression = compressor.CompressionRatio;
            o.BeginReading();

            // read acknowledgement of sent reliable messages
            reliableAcknowledge = o.ReadInt();

            // remove acknowledged reliable messages
            while (reliableSend.First <= reliableAcknowledge) if (!reliableSend.Get(null, out reliableMessageSize)) break;

            // read reliable messages
            reliableMessageSize = o.ReadShort();
            while (reliableMessageSize != 0)
            {
                if (reliableMessageSize <= 0 || reliableMessageSize > o.Size - o.ReadCount) { common.Printf($"{remoteAddress}: bad reliable message\n"); return false; }
                reliableSequence = o.ReadInt();
                if (reliableSequence == reliableReceive.Last + 1) reliableReceive.Add(o.DataR, o.ReadCount, reliableMessageSize);
                o.ReadData(null, reliableMessageSize);
                reliableMessageSize = o.ReadShort();
            }

            return true;
        }

        void UpdateOutgoingRate(int time, int size)
        {
            // update the outgoing rate control variables
            var deltaTime = time - lastSendTime;
            if (deltaTime > 1000) lastDataBytes = 0;
            else { lastDataBytes -= (deltaTime * maxRate) / 1000; if (lastDataBytes < 0) lastDataBytes = 0; }
            lastDataBytes += size;
            lastSendTime = time;

            // update outgoing rate variables
            if (time - outgoingRateTime > 1000)
            {
                outgoingRateBytes -= outgoingRateBytes * (time - outgoingRateTime - 1000) / 1000;
                if (outgoingRateBytes < 0) outgoingRateBytes = 0;
            }
            outgoingRateTime = time - 1000;
            outgoingRateBytes += size;
        }

        void UpdateIncomingRate(int time, int size)
        {
            // update incoming rate variables
            if (time - incomingRateTime > 1000)
            {
                incomingRateBytes -= incomingRateBytes * (time - incomingRateTime - 1000) / 1000;
                if (incomingRateBytes < 0) incomingRateBytes = 0;
            }
            incomingRateTime = time - 1000;
            incomingRateBytes += size;
        }

        void UpdatePacketLoss(int time, int numReceived, int numDropped)
        {
            // update incoming packet loss variables
            if (time - incomingPacketLossTime > 5000)
            {
                var scale = (time - incomingPacketLossTime - 5000) * (1f / 5000f);
                incomingReceivedPackets -= incomingReceivedPackets * scale;
                if (incomingReceivedPackets < 0f) incomingReceivedPackets = 0f;
                incomingDroppedPackets -= incomingDroppedPackets * scale;
                if (incomingDroppedPackets < 0f) incomingDroppedPackets = 0f;
            }
            incomingPacketLossTime = time - 5000;
            incomingReceivedPackets += numReceived;
            incomingDroppedPackets += numDropped;
        }
    }
}
