namespace System.NumericsX.OpenStack.Gngine.Framework.Async
{
    public interface INetworkSystem
    {
        void ServerSendReliableMessage(int clientNum, BitMsg msg);
        void ServerSendReliableMessageExcluding(int clientNum, BitMsg msg);
        int ServerGetClientPing(int clientNum);
        int ServerGetClientPrediction(int clientNum);
        int ServerGetClientTimeSinceLastPacket(int clientNum);
        int ServerGetClientTimeSinceLastInput(int clientNum);
        int ServerGetClientOutgoingRate(int clientNum);
        int ServerGetClientIncomingRate(int clientNum);
        float ServerGetClientIncomingPacketLoss(int clientNum);
        void ClientSendReliableMessage(BitMsg msg);
        int ClientPrediction { get; }
        int ClientTimeSinceLastPacket { get; }
        int ClientOutgoingRate { get; }
        int ClientIncomingRate { get; }
        float ClientIncomingPacketLoss { get; }
    }
}
