using System.Collections.Generic;
using System.Text;
using SharpPcap;

namespace RedesTrabalho
{
    public class PackageCapture
    {
        public static PackageCapture inst;

        public CapturaWindow capturaWindow;

        public static CaptureDeviceList devices;

        public List<PacketDotNet.Packet> packages = new List<PacketDotNet.Packet>();

       
        private void Device_OnPacketArrival(object s, PacketCapture e)
        {
            var packet = PacketDotNet.Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data);
            var ip = packet.Extract<PacketDotNet.IPPacket>();
            var tcp = packet.Extract<PacketDotNet.TcpPacket>();
            
            HTTPInfo httpInfo = HTTPprocess.ProcessPacket(ip, tcp);
            
            if (httpInfo != null)
            {
                packages.Add(packet);
                     
                capturaWindow.AddPacket(new PacketDisplayInfo()
                {
                    srcIp = ip.SourceAddress.ToString(),
                    destIp = ip.DestinationAddress.ToString(),
                    n = packages.Count,
                    time = e.GetPacket().Timeval.Date.ToString(),
                    httpV = httpInfo.httpVersion,
                    req = httpInfo.httpRequestType,
                    tam = packet.TotalPacketLength//e.GetPacket().Data.Length
                });
            }
            
        }

        public void StartCapture(int deviceIndex)
        {
            var device = devices[deviceIndex];

            device.OnPacketArrival += new PacketArrivalEventHandler(Device_OnPacketArrival);

            int readTimeoutMilliseconds = 1000;
            device.Open(DeviceModes.Promiscuous, readTimeoutMilliseconds);

            device.StartCapture();
        }

        public void StopCapture(int deviceIndex)
        {
            devices[deviceIndex].StopCapture();
            devices[deviceIndex].Close();
        }

        public string GetPacketInfos(int pIndex)
        {
            StringBuilder sb = new StringBuilder();
            PacketDotNet.Packet p = packages[pIndex];
            var ip = p.Extract<PacketDotNet.IPPacket>();
            var tcp = p.Extract<PacketDotNet.TcpPacket>();

            sb.Append($"Ip origem: {ip.SourceAddress}\n\n");
            sb.Append($"Ip destino: {ip.DestinationAddress}\n\n");
            sb.Append($"Porta origem: {tcp.SourcePort}\n\n");
            sb.Append($"Porta destino: {tcp.DestinationPort}\n\n");

            sb.Append($"Tamanho da janela: {tcp.WindowSize}\n\n");
            sb.Append($"Número de sequência: {tcp.SequenceNumber}\n\n");

            sb.Append($"Número de reconhecimento: {tcp.AcknowledgmentNumber}\n\n");

            sb.Append($"Flag ACK: {(tcp.Acknowledgment ? "1" : "0")} \n\n");
            sb.Append($"Flag SYN: {(tcp.Synchronize ? "1" : "0")} \n\n");
            sb.Append($"Flag FIN: {(tcp.Finished ? "1" : "0")} \n\n");
            sb.Append($"Flag RST: {(tcp.Reset ? "1" : "0")} \n\n");
            
            sb.Append($"Tamanho do pacote: {p.TotalPacketLength} bytes\n\n");

            HTTPprocess.GetHTTPInfoTxt(tcp, ip,ref sb);

            return sb.ToString();
        }

        public static string[] GetAvaliableDevices()
        {
            devices = CaptureDeviceList.Instance;
            string[] devicesInfo = new string[devices.Count];

            for(int i = 0; i < devices.Count; i++)
            {
                devicesInfo[i] = $"{i+1} : {devices[i].Description} : {devices[i].Name}";
            }

            return devicesInfo;
        }
    }
}

