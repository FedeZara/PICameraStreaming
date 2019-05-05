using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
/*!
\file IPMACMapper.cs
\brief Implementazione della classe IPMACMapper
\version 1.0
*/
namespace client_app
{
    /*!
    \class IPMACMapper
    \brief Classe che permette il recupero di un indirizzo IP da un indirizzo MAC
    */
    public static class IPMACMapper
    {
        //*! \var list
        //*! \brief Lista contenente oggetti di tipo IPandMac
        private static List<IPAndMac> list;
        /*!
        \fn ExecuteCommandLine
        \brief Esegue un comando cmd e ne ritorna un eventuale output
        \param[in] file = file o comando da eseguire 
        \param[in] arguments = argomenti da passare alla esecuzione in linea di comando
        \param[out] process.StandardOutput = output del comando
        */
        private static StreamReader ExecuteCommandLine(String file, String arguments = "")
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.FileName = file;
            startInfo.Arguments = arguments;

            Process process = Process.Start(startInfo);

            return process.StandardOutput;
        }
        /*!
        \fn InitializeGetIPsAndMac
        \brief Utilizza il comando arp per effettuare un mappatura IP e MAC, salvandone poi il risultato in list
        */
        private static void InitializeGetIPsAndMac()
        {
            if (list != null)
                return;

            var arpStream = ExecuteCommandLine("arp", "-a");
            List<string> result = new List<string>();
            while (!arpStream.EndOfStream)
            {
                var line = arpStream.ReadLine().Trim();
                result.Add(line);
            }

            list = result.Where(x => !string.IsNullOrEmpty(x) && (x.Contains("dinamico") || x.Contains("statico")))
                .Select(x =>
                {
                    string[] parts = x.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    return new IPAndMac { IP = parts[0].Trim(), MAC = parts[1].Trim() };
                }).ToList();
        }
        /*!
       \fn FindIPFromMacAddress
       \brief Ritorna un IP associato ad un mac fornito
       \param[in] macAddress = Indirizzo MAC da cui si vuole trovare l'ip
       \param[out] item.IP = IP associato all'indirizzo MAC fornito
       */
        public static string FindIPFromMacAddress(string macAddress)
        {
            InitializeGetIPsAndMac();
            IPAndMac item = list.SingleOrDefault(x => x.MAC == macAddress);
            if (item == null)
                return null;
            return item.IP;
        }
        /*!
     \fn FindMacFromIPAddress
     \brief Ritorna un MAC associato ad un IP fornito
     \param[in] ip = Indirizzo ip da cui si vuole trovare il MAC
     \param[out] item.MAC = MAC associato all'indirizzo IP fornito
     */

        public static string FindMacFromIPAddress(string ip)
        {
            InitializeGetIPsAndMac();
            IPAndMac item = list.SingleOrDefault(x => x.IP == ip);
            if (item == null)
                return null;
            return item.MAC;
        }

        private class IPAndMac
        {
            //*! \var IP
            //*! \brief Proprietà(stringa) della classe che contiene l'ip
            public string IP { get; set; }
            //*! \var MAC
            //*! \brief Proprietà(stringa) della classe che contiene il MAC
            public string MAC { get; set; }
        }

    }
}
