using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfClient
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Socket MonSocketClient;
        public Thread MonThread;
        public delegate void dEcrit(string texte);
        FlowDocument flowDocument = new FlowDocument();
        public MainWindow()
        {
            InitializeComponent();
            txt_Pseudo.Text = Environment.UserName.ToUpper();
        }

        private void Connexion_Click(object sender, RoutedEventArgs e)
        {
            MonSocketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            char separator = Convert.ToChar(".");
            string[] numCpte = txt_IP.Text.Split(separator);
            if (numCpte.Length != 4)
            {
                EcritureMessage("Adresse Ip non valide");
                return;
            }
            try
            {
                string ip = txt_IP.Text;
                IPAddress adresse = IPAddress.Parse(ip);
                IPEndPoint MonEP = new IPEndPoint(adresse, Convert.ToInt32(txt_Port.Text));
                MonSocketClient.Connect(MonEP);
                TraitementConnexion();
            }
            catch(Exception)
            {
                throw;
            }
        }

        public void TraitementConnexion()
        {
            if(txt_Pseudo.Text!="" && MonSocketClient!=null)
            {
                Byte[] Msg = System.Text.Encoding.UTF8.GetBytes(txt_Pseudo.Text);
                txt_IP.IsEnabled = false;
                txt_Port.IsEnabled = false;
                txt_Message.IsEnabled = false;
                bt_Connexion.IsEnabled = false;
                bt_Deconnexion.IsEnabled = false;
                bt_Envoyer.IsEnabled = true;

                int Envoi = MonSocketClient.Send(Msg);
                MonThread = new Thread(ThreadLecture);
                MonThread.Start();
               

            }
        }
        public void ThreadLecture()
        {
            while(MonSocketClient.Connected)
            {
                Byte[] Octets = new Byte[100000];
                int Recu = 0;
                try
                {
                    Recu = MonSocketClient.Receive(Octets);
                }
                catch(Exception)
                {
                    EcritureMessage("Connexion perdue, arrêt de la reception des données ...");
                }

                string Message = System.Text.Encoding.UTF8.GetString(Octets);
                Message = Message.Substring(0, Recu);
                EcritureMessage(Message);
            }
        }
        public void Ecrit(string texte)
        {
            //txt_Message.Text = texte;
            Paragraph paragraph = new Paragraph();
            string[] result = texte.Split(':');
            if(result.Count()>1)
            {
                BitmapImage bitmapImage=new BitmapImage(new Uri())
            }
        }
        public void EcritureMessage(string texte)
        {
           
            try
            {
                this.Dispatcher.Invoke(new dEcrit(Ecrit), texte);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Bt_Envoyer_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
