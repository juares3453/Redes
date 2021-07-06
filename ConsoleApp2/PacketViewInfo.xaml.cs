using EO.WebBrowser.DOM;
using System.Windows.Input;


namespace RedesTrabalho
{
    /// <summary>
    /// Lógica interna para PacketViewInfo.xaml
    /// </summary>

    public partial class PacketViewInfo : Window
    {
        public PacketViewInfo(int pIndex)
        {
            InitializeComponent();
            ShowInfo(pIndex);
        }

        private void ShowInfo(int pIndex)
        {
            PacketText.Text = PackageCapture.inst.GetPacketInfos(pIndex);
        }
    }
}
