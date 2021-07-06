
using System.Text;
using PacketDotNet;

namespace RedesTrabalho
{
    public class HTTPInfo
    {
        public string httpVersion;
        public string httpRequestType;
        public string host = string.Empty;
    }

    class HTTPprocess
    {
        public struct HTTPVersions
        {
            public const string httpv09 = "HTTP/0.9";
            public const string httpv10 = "HTTP/1.0";
            public const string httpv11 = "HTTP/1.1";
            public const string httpv20 = "HTTP/2.0";
            public const string httpv30 = "HTTP/3.0";
            public const string none = "none";
        }

        public struct HTTPRequestTypes
        {
            public const string status = "STATUS";
            public const string head = "HEAD";
            public const string get = "GET";
            public const string post = "POST";
            public const string put = "PUT";
            public const string delete = "DELETE";
            public const string trace = "TRACE";
            public const string options = "OPTIONS";
            public const string connect = "CONNECT";
            public const string patch = "PATCH";
        }


        public static HTTPInfo ProcessPacket(IPPacket pIP, TcpPacket pTCP)
        {
            if (pIP == null || pTCP == null) return null;

            if (pTCP.HasPayloadData)
            {
                if (pTCP.PayloadData.Length == 0) return null;

                HTTPInfo HTTPInfo = GetHTTPInfos(pTCP.PayloadData);
                if (HTTPInfo != null)
                    return HTTPInfo;

                return CheckSequence(pIP, pTCP);
            }

            return null;
        }

        private static HTTPInfo GetHTTPInfos(byte[] payLoadData)
        {
            string s = Encoding.ASCII.GetString(payLoadData);

            HTTPInfo httpInfo = ReadHTTPHeader(s); 

            if (httpInfo.httpVersion == HTTPVersions.none) return null;


            return httpInfo;
        }

        private static string GetHTTPVersion(string s)
        {
            //if(s.IndexOf(HTTPVersions.httpv11, StringComparison.OrdinalIgnoreCase) >= 0) return HTTPVersions.httpv11;

            if (s.Contains(HTTPVersions.httpv11)) return HTTPVersions.httpv11;
            else if (s.Contains(HTTPVersions.httpv20)) return HTTPVersions.httpv20;
            else if (s.Contains(HTTPVersions.httpv09)) return HTTPVersions.httpv09;
            else if (s.Contains(HTTPVersions.httpv30)) return HTTPVersions.httpv30;

            return HTTPVersions.none;
        }

        private static string GetHTTPRequestType(string s)
        {
            if (s.Contains(HTTPRequestTypes.get)) return HTTPRequestTypes.get;
            else if (s.Contains(HTTPRequestTypes.post)) return HTTPRequestTypes.post;
            else if (s.Contains(HTTPRequestTypes.connect)) return HTTPRequestTypes.connect;
            else if (s.Contains(HTTPRequestTypes.delete)) return HTTPRequestTypes.delete;
            else if (s.Contains(HTTPRequestTypes.put)) return HTTPRequestTypes.put;
            else if (s.Contains(HTTPRequestTypes.options)) return HTTPRequestTypes.options;
            else if (s.Contains(HTTPRequestTypes.trace)) return HTTPRequestTypes.trace;
            else if (s.Contains(HTTPRequestTypes.head)) return HTTPRequestTypes.head;
            else if (s.Contains(HTTPRequestTypes.patch)) return HTTPRequestTypes.patch;

            return HTTPRequestTypes.status;
        }

        private static string GetHost(string s)
        {
            if (s.Contains("Host:")) return s.Substring("Host:".Length);
            return string.Empty;
        }

        public static HTTPInfo ReadHTTPHeader(string s)
        {
            HTTPInfo httpInfo = new HTTPInfo();
            string[] words = s.Split('\n');
            bool versionSet = false, requestSet = false, hostSet = false;

            foreach (string str in words)
            {
                if (versionSet && requestSet && hostSet) break;

                if (!hostSet)
                {
                    httpInfo.host = GetHost(str);
                    if (httpInfo.host != string.Empty) hostSet = true;
                }
                if (!versionSet)
                {
                    httpInfo.httpVersion = GetHTTPVersion(str);
                    if (httpInfo.httpVersion != HTTPVersions.none) versionSet = true;
                }
                if (!requestSet)
                {
                    httpInfo.httpRequestType = GetHTTPRequestType(str);
                    if (httpInfo.httpRequestType != HTTPRequestTypes.status) requestSet = true;
                }
            }
            return httpInfo;
        }

        private static HTTPInfo CheckSequence(IPPacket pIP,TcpPacket pTCP)
        {

            for(int i = PackageCapture.inst.packages.Count - 1; i >= 0; i--)
            {
                var ip = PackageCapture.inst.packages[i].Extract<IPPacket>();
                var tcp = PackageCapture.inst.packages[i].Extract<TcpPacket>();

                if (CheckAdresses(pTCP, pIP, tcp, ip) && CheckSeqAck(pTCP,tcp))
                {
                    HTTPInfo httpInfo = GetHTTPInfos(tcp.PayloadData);
                    if (httpInfo != null) return httpInfo;
                }
            }

            return null;
        }

        private static bool CheckAdresses(TcpPacket pTCP,IPPacket pIP, TcpPacket tcp, IPPacket ip)
        {
            if (pIP.SourceAddress.Equals(ip.SourceAddress) && pIP.DestinationAddress.Equals(ip.DestinationAddress))
            {
                if (pTCP.SourcePort == tcp.SourcePort && pTCP.DestinationPort == tcp.DestinationPort)
                        return true;
            }
            return false;
        }

        private static bool CheckSeqAck(TcpPacket pTCP,TcpPacket tcp)
        {
            if (pTCP.SequenceNumber > tcp.SequenceNumber || pTCP.AcknowledgmentNumber > tcp.AcknowledgmentNumber)
                return true;
            return false;
        }

        public static void GetHTTPInfoTxt(TcpPacket pTCP,IPPacket pIP,ref StringBuilder sb)
        {
            HTTPInfo httpInfo = GetHTTPInfos(pTCP.PayloadData);
            if(httpInfo == null)
            {
                httpInfo = CheckSequence(pIP, pTCP);
            }

            if(httpInfo != null)
            {
                sb.Append($"Versão HTTP: {httpInfo.httpVersion}\n\n");
                sb.Append($"Requisição HTTP: {httpInfo.httpRequestType}\n\n");

                if (httpInfo.host != string.Empty)
                    sb.Append($"Host: {httpInfo.host}\n\n");
            }
            
            
            sb.Append(Encoding.ASCII.GetString(pTCP.PayloadData));
        }

        

    }
}
