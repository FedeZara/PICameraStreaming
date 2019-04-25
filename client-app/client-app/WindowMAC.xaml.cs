using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace client_app
{
    /// <summary>
    /// Logica di interazione per WindowMAC.xaml
    /// </summary>
    public partial class WindowMAC : Window
    {
        List<char> ValoriAccettabiliMac = new List<char> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'A', 'B', 'C', 'D', 'E', 'F' };

        public WindowMAC()
        {
            InitializeComponent();
            MACadd.Text = MainWindow.macPi;
        }

        void Click(object sender, RoutedEventArgs e)
        {
            if (!MACValido(MACadd.Text))
            {
                infoP.Content = "Inserisci un indirizzo MAC valido!";
                infoP.Foreground = new SolidColorBrush(Color.FromRgb(203, 50, 52));
            } else
            {
                MainWindow.macPi = MACadd.Text;
                Close();
            }
        }

        private bool MACValido(string mac)
        {
            if (mac.Length != 17)
                return false;

            for(int i = 0; i < 17; i++)
            {
                if ((i + 1) % 3 == 0)
                {
                    if (!(mac[i] == '-'))
                        return false;
                }
                else
                    if (!ValoriAccettabiliMac.Contains(mac[i]))
                        return false;
            }

            return true;
        }
    }
}
