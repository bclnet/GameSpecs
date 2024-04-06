namespace System.NumericsX.OpenStack.System
{
    public static partial class SysW
    {
        //        static WSADATA winsockdata;
        //        static bool winsockInitialized = false;

        //        static readonly CVar net_ip = new("net_ip", "localhost", CVAR.SYSTEM, "local IP address");
        //        static readonly CVar net_port = new("net_port", "0", CVAR.SYSTEM | CVAR.INTEGER, "local IP port number");
        //        static readonly CVar net_forceLatency = new("net_forceLatency", "0", CVAR.SYSTEM | CVAR.INTEGER, "milliseconds latency");
        //        static readonly CVar net_forceDrop = new("net_forceDrop", "0", CVAR.SYSTEM | CVAR.INTEGER, "percentage packet loss");

        //        static SOCKET ip_socket;

        //        public struct net_interface
        //        {
        //            public uint ip;
        //            public uint mask;
        //        }

        //        const int MAX_INTERFACES = 32;
        //        static int num_interfaces = 0;
        //        static net_interface[] netint = new net_interface[MAX_INTERFACES];

        //        public static string NET_ErrorString()
        //        {
        //            var code = WSAGetLastError();
        //            switch (code)
        //            {
        //                case WSAEINTR: return "WSAEINTR";
        //                case WSAEBADF: return "WSAEBADF";
        //                case WSAEACCES: return "WSAEACCES";
        //                case WSAEDISCON: return "WSAEDISCON";
        //                case WSAEFAULT: return "WSAEFAULT";
        //                case WSAEINVAL: return "WSAEINVAL";
        //                case WSAEMFILE: return "WSAEMFILE";
        //                case WSAEWOULDBLOCK: return "WSAEWOULDBLOCK";
        //                case WSAEINPROGRESS: return "WSAEINPROGRESS";
        //                case WSAEALREADY: return "WSAEALREADY";
        //                case WSAENOTSOCK: return "WSAENOTSOCK";
        //                case WSAEDESTADDRREQ: return "WSAEDESTADDRREQ";
        //                case WSAEMSGSIZE: return "WSAEMSGSIZE";
        //                case WSAEPROTOTYPE: return "WSAEPROTOTYPE";
        //                case WSAENOPROTOOPT: return "WSAENOPROTOOPT";
        //                case WSAEPROTONOSUPPORT: return "WSAEPROTONOSUPPORT";
        //                case WSAESOCKTNOSUPPORT: return "WSAESOCKTNOSUPPORT";
        //                case WSAEOPNOTSUPP: return "WSAEOPNOTSUPP";
        //                case WSAEPFNOSUPPORT: return "WSAEPFNOSUPPORT";
        //                case WSAEAFNOSUPPORT: return "WSAEAFNOSUPPORT";
        //                case WSAEADDRINUSE: return "WSAEADDRINUSE";
        //                case WSAEADDRNOTAVAIL: return "WSAEADDRNOTAVAIL";
        //                case WSAENETDOWN: return "WSAENETDOWN";
        //                case WSAENETUNREACH: return "WSAENETUNREACH";
        //                case WSAENETRESET: return "WSAENETRESET";
        //                case WSAECONNABORTED: return "WSWSAECONNABORTEDAEINTR";
        //                case WSAECONNRESET: return "WSAECONNRESET";
        //                case WSAENOBUFS: return "WSAENOBUFS";
        //                case WSAEISCONN: return "WSAEISCONN";
        //                case WSAENOTCONN: return "WSAENOTCONN";
        //                case WSAESHUTDOWN: return "WSAESHUTDOWN";
        //                case WSAETOOMANYREFS: return "WSAETOOMANYREFS";
        //                case WSAETIMEDOUT: return "WSAETIMEDOUT";
        //                case WSAECONNREFUSED: return "WSAECONNREFUSED";
        //                case WSAELOOP: return "WSAELOOP";
        //                case WSAENAMETOOLONG: return "WSAENAMETOOLONG";
        //                case WSAEHOSTDOWN: return "WSAEHOSTDOWN";
        //                case WSASYSNOTREADY: return "WSASYSNOTREADY";
        //                case WSAVERNOTSUPPORTED: return "WSAVERNOTSUPPORTED";
        //                case WSANOTINITIALISED: return "WSANOTINITIALISED";
        //                case WSAHOST_NOT_FOUND: return "WSAHOST_NOT_FOUND";
        //                case WSATRY_AGAIN: return "WSATRY_AGAIN";
        //                case WSANO_RECOVERY: return "WSANO_RECOVERY";
        //                case WSANO_DATA: return "WSANO_DATA";
        //                default: return "NO ERROR";
        //            }
        //        }

        //        public static void Net_NetadrToSockadr(Netadr a, sockaddr s)
        //        {
        //            memset(s, 0, sizeof(s));

        //            if (a.type == NA.BROADCAST)
        //            {
        //                ((sockaddr_in)s).sin_family = AF_INET;
        //                ((sockaddr_in)s).sin_addr.s_addr = INADDR_BROADCAST;
        //            }

        //            else if (a.type == NA.IP || a.type == NA.LOOPBACK)
        //            {
        //                ((sockaddr_in)s).sin_family = AF_INET;
        //                ((sockaddr_in)s).sin_addr.s_addr = (int)a.ip;
        //            }

        //    ((sockaddr_in)s).sin_port = htons((short)a.port);
        //        }

        //        public static void Net_SockadrToNetadr(sockaddr s, out Netadr a)
        //        {
        //            uint ip;
        //            if (s.sa_family == AF_INET)
        //            {
        //                ip = ((sockaddr_in)s).sin_addr.s_addr;
        //                a.ip = ip;
        //                a.port = htons(((sockaddr_in)s).sin_port);
        //                // we store in network order, that loopback test is host order..
        //                ip = ntohl(ip);
        //                a.type = ip == INADDR_LOOPBACK ? NA.LOOPBACK : NA.IP;
        //            }
        //        }

        //        static bool Net_ExtractPort(string src, string buf, int bufsize, int port)
        //        {
        //            char* p;
        //            strncpy(buf, src, bufsize);
        //            p = buf; p += Min(bufsize - 1, (int)strlen(src)); *p = '\0';
        //            p = strchr(buf, ':');
        //            if (!p)
        //                return false;
        //            *p = '\0';
        //            *port = strtol(p + 1, NULL, 10);
        //            if (errno == ERANGE)
        //                return false;
        //            return true;
        //        }

        //        static bool Net_StringToSockaddr(string s, out sockaddr sadr, bool doDNSResolve)
        //        {
        //            hostent h;
        //            char buf[256];
        //            int port;

        //            memset(sadr, 0, sizeof( * sadr) );

        //            ((sockaddr_in)sadr).sin_family = AF_INET;
        //            ((sockaddr_in)sadr).sin_port = 0;

        //            if (s[0] >= '0' && s[0] <= '9')
        //            {
        //                ulong ret = inet_addr(s);
        //                if (ret != INADDR_NONE)
        //                    ((sockaddr_in)sadr).sin_addr = ret;
        //                else
        //                {
        //                    // check for port
        //                    if (!Net_ExtractPort(s, buf, sizeof(buf), &port))
        //                        return false;
        //                    ret = inet_addr(buf);
        //                    if (ret == INADDR_NONE)
        //                        return false;
        //                    ((sockaddr_in)sadr).sin_addr = ret;
        //                    ((sockaddr_in)sadr).sin_port = htons(port);
        //                }
        //            }
        //            else if (doDNSResolve)
        //            {
        //                // try to remove the port first, otherwise the DNS gets confused into multiple timeouts failed or not failed, buf is expected to contain the appropriate host to resolve
        //                if (Net_ExtractPort(s, buf, sizeof(buf), &port))
        //                    ((sockaddr_in)sadr).sin_port = htons(port);
        //                h = gethostbyname(buf);
        //                if (h == 0)
        //                    return false;
        //                ((sockaddr_in)sadr).sin_addr = h.h_addr_list[0];
        //            }

        //            return true;
        //        }

        //        static int NET_IPSocket(string net_interface, int port, Netadr bound_to)
        //        {
        //            SOCKET newsocket;

        //            sockaddr_in address;
        //            ulong _true = 1;
        //            int i = 1;
        //            int err;

        //            if (net_interface)
        //                common.DPrintf($"Opening IP socket: {net_interface}:{port}\n");
        //            else
        //                common.DPrintf($"Opening IP socket: localhost:{port}\n");

        //            if ((newsocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP)) == INVALID_SOCKET)
        //            {
        //                err = WSAGetLastError();
        //                if (err != WSAEAFNOSUPPORT)
        //                    common.Printf($"WARNING: UDP_OpenSocket: socket: {NET_ErrorString()}\n");
        //                return 0;
        //            }

        //            // make it non-blocking
        //            if (ioctlsocket(newsocket, FIONBIO, &_true) == SOCKET_ERROR)
        //            {
        //                common.Printf($"WARNING: UDP_OpenSocket: ioctl FIONBIO: {NET_ErrorString()}\n");
        //                return 0;
        //            }

        //            // make it broadcast capable
        //            if (setsockopt(newsocket, SOL_SOCKET, SO_BROADCAST, (char*)&i, sizeof(i)) == SOCKET_ERROR)
        //            {
        //                common.Printf($"WARNING: UDP_OpenSocket: setsockopt SO_BROADCAST: {NET_ErrorString()}\n");
        //                return 0;
        //            }

        //            if (!net_interface || !net_interface[0] || string.Equals(net_interface, "localhost", System.StringComparison.OrdinalIgnoreCase))
        //                address.sin_addr.s_addr = INADDR_ANY;
        //            else
        //                Net_StringToSockaddr(net_interface, (sockaddr)address, true);

        //            address.sin_port = port == PORT_ANY ? 0 : htons(port);

        //            address.sin_family = AF_INET;

        //            if (bind(newsocket, (sockaddr)address, sizeof(address)) == SOCKET_ERROR)
        //            {
        //                common.Printf($"WARNING: UDP_OpenSocket: bind: {NET_ErrorString()}\n");
        //                closesocket(newsocket);
        //                return 0;
        //            }

        //            // if the port was PORT_ANY, we need to query again to know the real port we got bound to (this used to be in Port::InitForPort)
        //            if (bound_to)
        //            {
        //                int len = sizeof(address);
        //                getsockname(newsocket, (sockaddr)address, len);
        //                Net_SockadrToNetadr((sockaddr)address, bound_to);
        //            }

        //            return newsocket;
        //        }

        //        static bool Net_WaitForUDPPacket(int netSocket, int timeout)
        //        {
        //            int ret;
        //            fd_set set;

        //            timeval tv;

        //            if (!netSocket)
        //                return false;

        //            if (timeout <= 0)
        //                return true;

        //            FD_ZERO(set);
        //            FD_SET(netSocket, set);

        //            tv.tv_sec = 0;
        //            tv.tv_usec = timeout * 1000;

        //            ret = select(netSocket + 1, set, null, null, tv);

        //            if (ret == -1)
        //            {
        //                common.DPrintf("Net_WaitForUPDPacket select(): {strerror(errno)}\n");
        //                return false;
        //            }

        //            // timeout with no data
        //            if (ret == 0)
        //                return false;

        //            return true;
        //        }

        //        static bool Net_GetUDPPacket(int netSocket, netadr_t net_from, byte[] data, ref int size, int maxSize)
        //        {
        //            int ret;

        //            sockaddr from;
        //            int fromlen;
        //            int err;

        //            if (!netSocket)
        //                return false;

        //            fromlen = sizeof(from);
        //            ret = recvfrom(netSocket, data, maxSize, 0, (sockaddr)from, fromlen);
        //            if (ret == SOCKET_ERROR)
        //            {
        //                err = WSAGetLastError();

        //                if (err == WSAEWOULDBLOCK || err == WSAECONNRESET)
        //                    return false;
        //                char buf[1024];
        //                sprintf(buf, "Net_GetUDPPacket: {NET_ErrorString()}\n");
        //                OutputDebugString(buf);
        //                return false;
        //            }

        //            if (netSocket == ip_socket)
        //            {
        //                memset(((sockaddr_in)from).sin_zero, 0, 8);
        //            }

        //            Net_SockadrToNetadr(from, net_from);

        //            if (ret == maxSize)
        //            {
        //                char buf[1024];
        //                sprintf(buf, $"Net_GetUDPPacket: oversize packet from {Sys_NetAdrToString(net_from)}\n");
        //                OutputDebugString(buf);
        //                return false;
        //            }

        //            size = ret;

        //            return true;
        //        }


        //        statuc void Net_SendUDPPacket(int netSocket, int length, byte[] data, Netadr to)
        //        {
        //            int ret;

        //            sockaddr addr;

        //            if (!netSocket)
        //                return;

        //            Net_NetadrToSockadr(to, addr);
        //            ret = sendto(netSocket, data, length, 0, addr, sizeof(addr));
        //            if (ret == SOCKET_ERROR)
        //            {
        //                int err = WSAGetLastError();

        //                // wouldblock is silent
        //                if (err == WSAEWOULDBLOCK)
        //                    return;

        //                // some PPP links do not allow broadcasts and return an error
        //                if ((err == WSAEADDRNOTAVAIL) && (to.type == NA.BROADCAST))
        //                    return;

        //                char buf[1024];
        //                sprintf(buf, $"Net_SendUDPPacket: {NET_ErrorString()}\n");
        //                OutputDebugString(buf);
        //            }
        //        }


        //        static void Sys_InitNetworking()
        //        {
        //            int r;

        //            r = WSAStartup(MAKEWORD(1, 1), &winsockdata);
        //            if (r != 0)
        //            {
        //                common.Printf("WARNING: Winsock initialization failed, returned {r}\n");
        //                return;
        //            }

        //            winsockInitialized = true;
        //            common.Printf("Winsock Initialized\n");

        //            PIP_ADAPTER_INFO pAdapterInfo;
        //            PIP_ADAPTER_INFO pAdapter = null;
        //            DWORD dwRetVal = 0;
        //            PIP_ADDR_STRING pIPAddrString;
        //            ULONG ulOutBufLen;
        //            bool foundloopback;

        //            num_interfaces = 0;
        //            foundloopback = false;

        //            pAdapterInfo = (IP_ADAPTER_INFO)malloc(sizeof(IP_ADAPTER_INFO));
        //            if (!pAdapterInfo)
        //                common.FatalError($"Sys_InitNetworking: Couldn't malloc({(uint)sizeof(IP_ADAPTER_INFO)})");
        //            ulOutBufLen = sizeof(IP_ADAPTER_INFO);

        //            // Make an initial call to GetAdaptersInfo to get the necessary size into the ulOutBufLen variable
        //            if (GetAdaptersInfo(pAdapterInfo, &ulOutBufLen) == ERROR_BUFFER_OVERFLOW)
        //            {
        //                free(pAdapterInfo);
        //                pAdapterInfo = (IP_ADAPTER_INFO*)malloc(ulOutBufLen);
        //                if (!pAdapterInfo)
        //                    common.FatalError($"Sys_InitNetworking: Couldn't malloc({ulOutBufLen})");
        //            }

        //            if ((dwRetVal = GetAdaptersInfo(pAdapterInfo, &ulOutBufLen)) != NO_ERROR)
        //                // happens if you have no network connection
        //                common.Printf($"Sys_InitNetworking: GetAdaptersInfo failed ({dwRetVal}).\n");
        //            else
        //            {
        //                pAdapter = pAdapterInfo;
        //                while (pAdapter)
        //                {
        //                    common.Printf("Found interface: %s %s - ", pAdapter.AdapterName, pAdapter.Description);
        //                    pIPAddrString = &pAdapter.IpAddressList;
        //                    while (pIPAddrString)
        //                    {
        //                        ulong ip_a, ip_m;
        //                        if (string.Equals("127.0.0.1", pIPAddrString.IpAddress.String, System.StringComparison.OrdinalIgnoreCase))
        //                            foundloopback = true;
        //                        ip_a = ntohl(inet_addr(pIPAddrString.IpAddress.String));
        //                        ip_m = ntohl(inet_addr(pIPAddrString.IpMask.String));
        //                        //skip null netmasks
        //                        if (!ip_m)
        //                        {
        //                            common.Printf($"{pIPAddrString.IpAddress.String} Null netmask - skipped\n");
        //                            pIPAddrString = pIPAddrString.Next;
        //                            continue;
        //                        }
        //                        common.Printf($"{pIPAddrString.IpAddress.String}/{pIPAddrString.IpMask.String}\n");
        //                        netint[num_interfaces].ip = ip_a;
        //                        netint[num_interfaces].mask = ip_m;
        //                        num_interfaces++;
        //                        if (num_interfaces >= MAX_INTERFACES)
        //                        {
        //                            common.Printf($"Sys_InitNetworking: MAX_INTERFACES({MAX_INTERFACES}) hit.\n");
        //                            free(pAdapterInfo);
        //                            return;
        //                        }
        //                        pIPAddrString = pIPAddrString.Next;
        //                    }
        //                    pAdapter = pAdapter.Next;
        //                }
        //            }
        //            // for some retarded reason, win32 doesn't count loopback as an adapter...
        //            if (!foundloopback && num_interfaces < MAX_INTERFACES)
        //            {
        //                common.Printf("Sys_InitNetworking: adding loopback interface\n");
        //                netint[num_interfaces].ip = ntohl(inet_addr("127.0.0.1"));
        //                netint[num_interfaces].mask = ntohl(inet_addr("255.0.0.0"));
        //                num_interfaces++;
        //            }
        //            free(pAdapterInfo);
        //        }

        //        static void Sys_ShutdownNetworking()
        //        {
        //            if (!winsockInitialized)
        //                return;
        //            WSACleanup();
        //            winsockInitialized = false;
        //        }


        //        static bool Sys_StringToNetAdr(string s, out Netadr a, bool doDNSResolve)
        //        {
        //            if (!Net_StringToSockaddr(s, out var sadr, doDNSResolve))
        //            {
        //                a = default;
        //                return false;
        //            }

        //            Net_SockadrToNetadr(sadr, out a);
        //            return true;
        //        }

        //        static bool Sys_IsLANAddress(Netadr adr)
        //        {
        //#if ID_NOLANADDRESS
        //            common.Printf("Sys_IsLANAddress: ID_NOLANADDRESS\n");
        //            return false;
        //#endif
        //            if (adr.type == NA.LOOPBACK)
        //                return true;

        //            if (adr.type != NA.IP)
        //                return false;

        //            if (num_interfaces != 0)
        //            {
        //                var p_ip = (ulong)adr.ip[0];
        //                var ip = ntohl(p_ip);
        //                for (var i = 0; i < num_interfaces; i++)
        //                    if ((netint[i].ip & netint[i].mask) == (ip & netint[i].mask))
        //                        return true;
        //            }
        //            return false;
        //        }

        //        // Compares without the port
        //        static bool Sys_CompareNetAdrBase(Netadr a, Netadr b)
        //        {
        //            if (a.type != b.type)
        //                return false;

        //            if (a.type == NA.LOOPBACK)
        //                return true;

        //            if (a.type == NA.IP)
        //                return a.ip[0] == b.ip[0] && a.ip[1] == b.ip[1] && a.ip[2] == b.ip[2] && a.ip[3] == b.ip[3];

        //            common.Printf("Sys_CompareNetAdrBase: bad address type\n");
        //            return false;
        //        }

    }
}
//        //=============================================================================


//        class udpMsg
//        {
//            const int MAX_UDP_MSG_SIZE = 1400;
//            byte[] data[MAX_UDP_MSG_SIZE];
//            Netadr address;
//            int size;
//            uint time;
//            udpMsg next;
//        }

//        class UDPLag
//        {
//            UDPLag()
//            {
//                sendFirst = sendLast = recieveFirst = recieveLast = null;
//            }

//            udpMsg sendFirst;
//            udpMsg sendLast;
//            udpMsg recieveFirst;
//            udpMsg recieveLast;
//            BlockAlloc<udpMsg, 64> udpMsgAllocator;
//        }

//        static readonly UDPLag[] udpPorts = new UDPLag[65536];

//        /*
//        ==================
//        idPort::idPort
//        ==================
//        */
//        idPort::idPort() {
//    netSocket = 0;
//    memset(&bound_to, 0, sizeof(bound_to));
//    }

//    /*
//    ==================
//    idPort::~idPort
//    ==================
//    */
//    idPort::~idPort()
//    {
//        Close();
//    }

//    /*
//    ==================
//    InitForPort
//    ==================
//    */
//    bool idPort::InitForPort(int portNumber)
//    {
//        netSocket = NET_IPSocket(net_ip.GetString(), portNumber, &bound_to);
//        if (netSocket <= 0)
//        {
//            netSocket = 0;
//            memset(&bound_to, 0, sizeof(bound_to));
//            return false;
//        }

//        udpPorts[bound_to.port] = new idUDPLag;

//        return true;
//    }

//    /*
//    ==================
//    idPort::Close
//    ==================
//    */
//    void idPort::Close()
//    {
//        if (netSocket)
//        {
//            if (udpPorts[bound_to.port])
//            {
//                delete udpPorts[bound_to.port];
//                udpPorts[bound_to.port] = NULL;
//            }
//            closesocket(netSocket);
//            netSocket = 0;
//            memset(&bound_to, 0, sizeof(bound_to));
//        }
//    }

//    /*
//    ==================
//    idPort::GetPacket
//    ==================
//    */
//    bool idPort::GetPacket(netadr_t &from, void* data, int &size, int maxSize)
//    {
//        udpMsg_t* msg;
//        bool ret;

//        while (1)
//        {

//            ret = Net_GetUDPPacket(netSocket, from, (char*)data, size, maxSize);
//            if (!ret)
//            {
//                break;
//            }

//            if (net_forceDrop.GetInteger() > 0)
//            {
//                if (rand() < net_forceDrop.GetInteger() * RAND_MAX / 100)
//                {
//                    continue;
//                }
//            }

//            packetsRead++;
//            bytesRead += size;

//            if (net_forceLatency.GetInteger() > 0)
//            {

//                assert(size <= MAX_UDP_MSG_SIZE);
//                msg = udpPorts[bound_to.port].udpMsgAllocator.Alloc();
//                memcpy(msg.data, data, size);
//                msg.size = size;
//                msg.address = from;
//                msg.time = Sys_Milliseconds();
//                msg.next = NULL;
//                if (udpPorts[bound_to.port].recieveLast)
//                {
//                    udpPorts[bound_to.port].recieveLast.next = msg;
//                }
//                else
//                {
//                    udpPorts[bound_to.port].recieveFirst = msg;
//                }
//                udpPorts[bound_to.port].recieveLast = msg;
//            }
//            else
//            {
//                break;
//            }
//        }

//        if (net_forceLatency.GetInteger() > 0 || (udpPorts[bound_to.port] && udpPorts[bound_to.port].recieveFirst))
//        {

//            msg = udpPorts[bound_to.port].recieveFirst;
//            if (msg && msg.time <= Sys_Milliseconds() - net_forceLatency.GetInteger())
//            {
//                memcpy(data, msg.data, msg.size);
//                size = msg.size;
//                from = msg.address;
//                udpPorts[bound_to.port].recieveFirst = udpPorts[bound_to.port].recieveFirst.next;
//                if (!udpPorts[bound_to.port].recieveFirst)
//                {
//                    udpPorts[bound_to.port].recieveLast = NULL;
//                }
//                udpPorts[bound_to.port].udpMsgAllocator.Free(msg);
//                return true;
//            }
//            return false;

//        }
//        else
//        {
//            return ret;
//        }
//    }

//    /*
//    ==================
//    idPort::GetPacketBlocking
//    ==================
//    */
//    bool idPort::GetPacketBlocking(netadr_t &from, void* data, int &size, int maxSize, int timeout)
//    {

//        Net_WaitForUDPPacket(netSocket, timeout);

//        if (GetPacket(from, data, size, maxSize))
//        {
//            return true;
//        }

//        return false;
//    }

//    /*
//    ==================
//    idPort::SendPacket
//    ==================
//    */
//    void idPort::SendPacket( const netadr_t to, const void* data, int size)
//    {
//        udpMsg_t* msg;

//        if (to.type == NA_BAD)
//        {
//            common.Warning("idPort::SendPacket: bad address type NA_BAD - ignored");
//            return;
//        }

//        packetsWritten++;
//        bytesWritten += size;

//        if (net_forceDrop.GetInteger() > 0)
//        {
//            if (rand() < net_forceDrop.GetInteger() * RAND_MAX / 100)
//            {
//                return;
//            }
//        }

//        if (net_forceLatency.GetInteger() > 0 || (udpPorts[bound_to.port] && udpPorts[bound_to.port].sendFirst))
//        {

//            assert(size <= MAX_UDP_MSG_SIZE);
//            msg = udpPorts[bound_to.port].udpMsgAllocator.Alloc();
//            memcpy(msg.data, data, size);
//            msg.size = size;
//            msg.address = to;
//            msg.time = Sys_Milliseconds();
//            msg.next = NULL;
//            if (udpPorts[bound_to.port].sendLast)
//            {
//                udpPorts[bound_to.port].sendLast.next = msg;
//            }
//            else
//            {
//                udpPorts[bound_to.port].sendFirst = msg;
//            }
//            udpPorts[bound_to.port].sendLast = msg;

//            for (msg = udpPorts[bound_to.port].sendFirst; msg && msg.time <= Sys_Milliseconds() - net_forceLatency.GetInteger(); msg = udpPorts[bound_to.port].sendFirst)
//            {
//                Net_SendUDPPacket(netSocket, msg.size, msg.data, msg.address);
//                udpPorts[bound_to.port].sendFirst = udpPorts[bound_to.port].sendFirst.next;
//                if (!udpPorts[bound_to.port].sendFirst)
//                {
//                    udpPorts[bound_to.port].sendLast = NULL;
//                }
//                udpPorts[bound_to.port].udpMsgAllocator.Free(msg);
//            }

//        }
//        else
//        {
//            Net_SendUDPPacket(netSocket, size, data, to);
//        }
//    }


//    //=============================================================================

//    /*
//    ==================
//    idTCP::idTCP
//    ==================
//    */
//    idTCP::idTCP() {
//    fd = 0;
//    memset(&address, 0, sizeof(address));
//}

///*
//==================
//idTCP::~idTCP
//==================
//*/
//idTCP::~idTCP() {
//    Close();
//}

///*
//==================
//idTCP::Init
//==================
//*/
//bool idTCP::Init( const char* host, short port)
//{
//    unsigned long _true = 1;

//    struct sockaddr sadr;

//if (!Sys_StringToNetAdr(host, &address, true))
//{
//    common.Printf("Couldn't resolve server name \"%s\"\n", host);
//    return false;
//}
//address.type = NA_IP;
//if (!address.port)
//{
//    address.port = port;
//}
//common.Printf("\"%s\" resolved to %i.%i.%i.%i:%i\n", host,
//                address.ip[0], address.ip[1], address.ip[2], address.ip[3], address.port);
//Net_NetadrToSockadr(&address, &sadr);

//if (fd)
//{
//    common.Warning("idTCP::Init: already initialized?");
//}

//if ((fd = socket(AF_INET, SOCK_STREAM, 0)) == INVALID_SOCKET)
//{
//    fd = 0;
//    common.Printf("ERROR: idTCP::Init: socket: %s\n", NET_ErrorString());
//    return false;
//}

//if (connect(fd, &sadr, sizeof(sadr)) == SOCKET_ERROR)
//{
//    common.Printf("ERROR: idTCP::Init: connect: %s\n", NET_ErrorString());
//    closesocket(fd);
//    fd = 0;
//    return false;
//}

//// make it non-blocking
//if (ioctlsocket(fd, FIONBIO, &_true) == SOCKET_ERROR)
//{
//    common.Printf("ERROR: idTCP::Init: ioctl FIONBIO: %s\n", NET_ErrorString());
//    closesocket(fd);
//    fd = 0;
//    return false;
//}

//common.DPrintf("Opened TCP connection\n");
//return true;
//}

///*
//==================
//idTCP::Close
//==================
//*/
//void idTCP::Close()
//{
//    if (fd)
//    {
//        closesocket(fd);
//    }
//    fd = 0;
//}

///*
//==================
//idTCP::Read
//==================
//*/
//int idTCP::Read(void* data, int size)
//{
//    int nbytes;

//    if (!fd)
//    {
//        common.Printf("idTCP::Read: not initialized\n");
//        return -1;
//    }

//    if ((nbytes = recv(fd, (char*)data, size, 0)) == SOCKET_ERROR)
//    {
//        if (WSAGetLastError() == WSAEWOULDBLOCK)
//        {
//            return 0;
//        }
//        common.Printf("ERROR: idTCP::Read: %s\n", NET_ErrorString());
//        Close();
//        return -1;
//    }

//    // a successful read of 0 bytes indicates remote has closed the connection
//    if (nbytes == 0)
//    {
//        common.DPrintf("idTCP::Read: read 0 bytes - assume connection closed\n");
//        return -1;
//    }

//    return nbytes;
//}

///*
//==================
//idTCP::Write
//==================
//*/
//int idTCP::Write(void* data, int size)
//{
//    int nbytes;

//    if (!fd)
//    {
//        common.Printf("idTCP::Write: not initialized\n");
//        return -1;
//    }

//    if ((nbytes = send(fd, (char*)data, size, 0)) == SOCKET_ERROR)
//    {
//        common.Printf("ERROR: idTCP::Write: %s\n", NET_ErrorString());
//        Close();
//        return -1;
//    }

//    return nbytes;
//}
