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
        public WindowMAC()
        {
            InitializeComponent();
            infoLabel.Content = "Inserisci l'indirizzo MAC";
        }

        void Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
