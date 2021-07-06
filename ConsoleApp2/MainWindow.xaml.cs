using EO.WebBrowser.DOM;
using System.Windows.Controls;
using System.Windows;
using Windows.UI.Xaml;

namespace RedesTrabalho
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            FillList();
        }

        private void FillList()
        {
            GridView gridView = new GridView();
            DevicesList.View = gridView;

            gridView.Columns.Add(new GridViewColumn
            {
                //DisplayMemberBinding = new Binding("columnTitle"),
                Header = "Dispositivos"
            });

            string[] devices = PackageCapture.GetAvaliableDevices();
            foreach(string s in devices)
            {
                DevicesList.Items.Add(s);
            }

        }

        private void OnClickStart(object sender, RoutedEventArgs e)
        {
            if(DevicesList.SelectedItem != null)
            {
                if (DevicesList.SelectedItems.Count > 1) return;

                PackageCapture.inst = new PackageCapture();
                PackageCapture.inst.capturaWindow = new CapturaWindow(DevicesList.SelectedIndex);
                PackageCapture.inst.capturaWindow.ShowDialog();
            }
        }
    }
}
