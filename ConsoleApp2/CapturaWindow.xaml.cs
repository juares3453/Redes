using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace RedesTrabalho
{
    /// <summary>
    /// Lógica interna para CapturaWindow.xaml
    /// </summary>
    public partial class CapturaWindow : Window
    {
        private int deviceId;
        private bool isCapturing;

        public CapturaWindow(int deviceId)
        {
            this.deviceId = deviceId;
            isCapturing = false;

            InitializeComponent();

            InicializeCaptureList();
        }

        private void InicializeCaptureList()
        {
            for(int i = 0; i < PacketDisplayInfo.varNames.Length; i++)
            {
                DataGridTextColumn textColumn = new DataGridTextColumn();
                textColumn.Header = PacketDisplayInfo.labelNames[i];
                textColumn.Binding = new Binding(PacketDisplayInfo.varNames[i]);
                PackagesList.Columns.Add(textColumn);

                PackagesList.Columns[i].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            }
            PackagesList.IsReadOnly = true;
        }

        private void OnClickCaptura(object sender, RoutedEventArgs e)
        {
            if (isCapturing)
            {
                CapturaBtn.Content = "INICIAR CAPTURA";
                PackageCapture.inst.StopCapture(deviceId);
            }
            else
            {
                CapturaBtn.Content = "PARAR CAPTURA";
                PackageCapture.inst.StartCapture(deviceId);
            }

            isCapturing = !isCapturing;

        }

        public void AddPacket(PacketDisplayInfo p)
        {
            PackagesList.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                PackagesList.Items.Add(p);
            }));
        }

        private void OnClickItem(object sender, MouseButtonEventArgs e)
        {
            DataGrid dg = (DataGrid)sender;
            if(dg.SelectedItem != null)
            {
                new PacketViewInfo(dg.SelectedIndex).ShowDialog();
            }
        }

        private void OnClickClear(object sender, RoutedEventArgs e)
        {
            PackagesList.Items.Clear();
            PackageCapture.inst.packages.Clear();
        }
    }

    public class PacketDisplayInfo
    {
        public static string[] varNames = {"n", "srcIp", "destIp", "time", "httpV","req","tam" };
        public static string[] labelNames = {"Nro","IP Origem","IP Destino","Data","Versão HTTP","Requisição","Tamanho"};

        public int n { get; set; }
        public string srcIp { get; set; }
        public string destIp { get; set; }
        public string time { get; set; }

        public string httpV { get; set; }

        public string req { get; set; }

        public int tam { get; set; }
    }
}
