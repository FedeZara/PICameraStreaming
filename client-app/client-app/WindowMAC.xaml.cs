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
/*!
\file WindowMAC.xaml.cs
\brief Implementazione della classe parziale WindowMAC
\version 1.0
*/

namespace client_app
{
    /*!
    \class WindowMAC
    \brief Classe per la finestra di dialogo dove inserire il MAC della Raspberry con cui si vuole comunicare
    */
    public partial class WindowMAC : Window
    {
        //! \var ValoriAccettabiliMac
        //! \brief Lista di valori usabili nell'inserimento del MAC
        List<char> ValoriAccettabiliMac = new List<char> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'A', 'B', 'C', 'D', 'E', 'F' };
       
       /*!
       \fn WindowMac
       \brief Costruttore della classe WindowMac
       */
        public WindowMAC()
        {
            InitializeComponent();
            MACadd.Text = MainWindow.macPi;
        }

        //! \fn Click
        //! \brief Nel caso il MAC inserito sia valido viene salvato, altrimenti si chiede all'utente di inserirne un altro
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

        /*!
          \fn MACValido
          \brief Controlla la validità di un MAC fornito
          \param[in] mac = MAC di cui si vuole controllare la validità
          \param[out] bool che indica la correttezza
        */
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
