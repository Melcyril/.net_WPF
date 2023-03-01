using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace TchatWPF
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //SpeechSynthesizer pour ajouter la sortie à un flux qui contient les données audio au format Waveform.
        private SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();

        //Le programme de socket client C # est la deuxième partie du programme de socket serveur C #
        public Socket MonSocketClient;

        //Un thread ou fil ou tâche est similaire à un processus car tous deux représentent l'exécution d'un ensemble d'instructions
        //du langage machine d'un processeur. Du point de vue de l'utilisateur, ces exécutions semblent se dérouler en parallèle
        public Thread MonThread;

        //Delegate est un type qui encapsule sans risque une méthode ; il est similaire à un pointeur de fonction en C et C++.
        //Toutefois, à la différence des pointeurs de fonction C, les délégués sont orientés objet, de type sécurisé et sûrs.
        //Le type d'un délégué est défini par le nom du délégué. L’exemple suivant déclare un délégué nommé Del qui peut encapsuler
        //une méthode acceptant une chaîne (string) comme argument et qui retourne void :
        public delegate void dEcrit(string texte);

        //public delegate void dReception(Byte[] bytes);


        //es documents dynamiques sont conçus pour optimiser l’affichage et la lisibilité.Au lieu d’avoir une disposition prédéfinie,
        //ces documents dynamiques ajustent et refluent dynamiquement leur contenu en fonction des variables d’exécution telles 
        //que la taille de la fenêtre, la résolution de l’appareil et les préférences facultatives de l’utilisateur.
        //En outre, les documents dynamiques offrent des fonctionnalités de document avancées, telles que la pagination et les colonnes.
        FlowDocument flowDocument = new FlowDocument();

        public MainWindow()
        {
            InitializeComponent();
            Pseudo.Text = "";
            IP.Text = "10.115.145.66";
            Port.Text = "6060";
            DetectionRam();
            ListePeripherique();
            ListeConnexions();
            ListeRegistre();
            ListeDisques();
            ListeTCP();
            NomEtIP();
            //speechSynthesizer.SelectVoiceByHints(VoiceGender.Male, VoiceAge.Adult);
            //speechSynthesizer.SelectVoice("Microsoft Paul Desktop");

            //SpeakAsync méthode génèrent de la parole de manière asynchrone.
            speechSynthesizer.SpeakAsync("Bienvenue, " + Pseudo.Text);
        }


        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            MonSocketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //Verifie si l'ip est valide(=4), en splittant l'ip sur le point  
            char separator = Convert.ToChar('.');
            string[] numCompte = IP.Text.Split(separator);
            if(numCompte.Length != 4)
            {
                EcritureMessage("Adresse IP non valide.");
                return;
            }


            try
            {
                string ip = IP.Text;
                //Fournit une adresse IP(Internet Protocol)
                IPAddress adresse = IPAddress.Parse(ip);

                //Représente un point de terminaison du réseau comme une adresse IP et un numéro de port
                IPEndPoint monEP = new IPEndPoint(adresse, Convert.ToInt32(Port.Text));

                //Connection de l'utilisateur
                MonSocketClient.Connect(monEP);
            }
            catch (Exception ex)
            {
                EcritureMessage(ex.Message);
            }
            TraitementConnexion();
        }
        private void TraitementConnexion()
        {
            if(Pseudo.Text != "" && MonSocketClient != null)
            {
                
                IP.IsEnabled = false;
                Port.IsEnabled = false;
                Reponse.IsEnabled = true;
                Pseudo.IsEnabled = false;
                connecter.IsEnabled = false;
                deconnecter.IsEnabled = true;
                envoyer.IsEnabled = true;

                //Message encodé en ut8
                //GetBytes-> En cas de substitution dans une classe dérivée, encode tous les caractères de la chaîne spécifiée en séquence d'octet
                Byte[] Msg = Encoding.UTF8.GetBytes(Pseudo.Text);

                int Envoi = MonSocketClient.Send(Msg);
                MonThread = new Thread(ThreadLecture);
                MonThread.Start();
            }
        }
        private void ThreadLecture()
        {
            while (MonSocketClient.Connected)
            {
                Byte[] Octets = new Byte[100000];
                string Message = "";
                //int Recu = 0;
                try
                {
                    var stream = new NetworkStream(MonSocketClient);

                    int bytesRead = stream.Read(Octets, 0, Octets.Length);
                    byte[] theData = Octets.Take(bytesRead).ToArray();
                    Message = Encoding.UTF8.GetString(theData);
                    //Recu = MonSocketClient.Receive(Octets);
                }
                catch (Exception)
                {
                    EcritureMessage("Connexion perdue, arrêt de la reception des données....");
                }
                //string Message = Encoding.UTF8.GetString(Octets);
                //Message = Message.Substring(0, Recu);

                EcritureMessage(Message);
            }
        }
        public void EcritureMessage (string texte)
        {
            try
            {
                this.Dispatcher.Invoke(new dEcrit(Ecrit), texte);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public void Ecrit(string texte)
        {
            //Reponse.Text = texte.Substring(0, texte.Length-2) ;

            Paragraph paragraph = new Paragraph();
            string[] result = texte.Split(':');
            
            if (result.Count() > 1)
            {

                    BitmapImage bitmapImage = new BitmapImage(new Uri("http://www.xn--icne-wqa.com/images/icones/3/0/face-wink-3.png"));
                    Bold commentaire = new Bold(new Run(result[1]));
                    Run identite = new Run(" " + result[0] + ": ");
                    Image smiley = new Image();
                    smiley.Source = bitmapImage;
                    smiley.Width = 60;
                    smiley.Height = 60;
                    paragraph.Inlines.Add(smiley);
                    paragraph.Inlines.Add(identite);
                    paragraph.Inlines.Add(commentaire);
            }
            else
            {
                paragraph.Inlines.Add(new Run(texte));
            }
            paragraph.Margin = new Thickness { Left = 5, Top = 0, Right = 0, Bottom = 0 };
            if(flowDocument.Blocks.FirstBlock != null)
            {
                flowDocument.Blocks.InsertAfter(flowDocument.Blocks.LastBlock, paragraph);
            }
            else
            {
                flowDocument.Blocks.Add(paragraph);
            }
            ChatTB.Document = flowDocument;
            ChatTB.ScrollToEnd();
            speechSynthesizer.GetInstalledVoices();
            speechSynthesizer.SpeakAsync(texte);
        }
        public void Envoyer(object sender, RoutedEventArgs e)
        {
            if (Pseudo.Text != "" && MonSocketClient != null)
            {
                string texte = Reponse.Text;
                if(texte != "" && texte != "Tapez votre message...")
                try
                {
                    this.Dispatcher.Invoke(new dEcrit(EnvoyerMessage), texte);
                    Reponse.Text = "Tapez votre message...";
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }
        public void EnvoyerMessage(string texte)
        {
            
                Byte[] Octets = Encoding.UTF8.GetBytes(Reponse.Text);
                
                int Envoi = 0;
                Envoi = MonSocketClient.Send(Octets);
         }

        private void deconnecter_Click(object sender, RoutedEventArgs e)
        {
            MonSocketClient.Disconnect(false);
            
                IP.IsEnabled = false;
                Port.IsEnabled = false;
                Reponse.IsEnabled = true;
                Pseudo.IsEnabled = false;
                connecter.IsEnabled = true;
                deconnecter.IsEnabled = false;
                envoyer.IsEnabled = true;
            EcritureMessage("Vous êtes déconnecté.");
        }

        private void Envoyer_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (Pseudo.Text != "" && MonSocketClient != null)
                {
                    string texte = Reponse.Text;
                    try
                    {
                        this.Dispatcher.Invoke(new dEcrit(EnvoyerMessage), texte);
                        Reponse.Text = "";
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                    }
                }
            }
        }

        private void Reponse_GotFocus(object sender, RoutedEventArgs e)
        {
            Reponse.Text = "";
        }
        private void DetectionRam()
        {
            ObjectQuery wql = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(wql);
            ManagementObjectCollection results = searcher.Get();

            double ramTotal;
            double ramDispo ;
            foreach (ManagementObject result in results)
            {
                ramTotal = Convert.ToDouble(result["TotalVisibleMemorySize"]);
                PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes", true);
                ramTotal = Math.Round(ramTotal / (1024 * 1024),2);
                ramDispo = Math.Round((Convert.ToDouble(ramCounter.NextValue())/1024), 2);
                RAMinstall.Text = ramDispo+"GB libres sur "+ramTotal + "GB au total";
            }
        }
        private void ListePeripherique()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPSignedDriver");
            foreach (ManagementObject obj in searcher.Get())
            {
                string periph;

                //Device name  
                periph = obj["DeviceName"]?.ToString();
                ListePeriph.Text += periph + "\r\n";
            }
        }
        private void ListeConnexions()
        {

            DirectoryEntry a = new DirectoryEntry();
            a.Path = "WinNT://WORKGROUP";


            foreach (DirectoryEntry item in a.Children)
            {
                if ((item.Name != "Schema"))
                {
                    IPLocales.Text += item.Name + " : \r\n";
                }
            }


            IPAddress[] ipLocales = Dns.GetHostAddresses("exchange.ad.afpanet");

            foreach (IPAddress ipaddresse in ipLocales)
            {
                string hote = "";
                try
                {
                    hote = Dns.GetHostEntry(ipaddresse.ToString()).HostName;
                    
                }
                catch(SocketException e)
                {

                }
                if (hote != "")
                {
                    IPLocales.Text += hote +" : "+ ipaddresse.ToString() + "\r\n";
                }
                else
                {
                    IPLocales.Text += ipaddresse.ToString() + "\r\n";
                }
            }
        }
        public void ListeRegistre()
        {
            RegistryKey cleRoot = Registry.ClassesRoot;
            RegistryKey cleCurrentUser = Registry.CurrentUser;
            RegistryKey cleLocalMachine = Registry.LocalMachine;
            RegistryKey cleUsers = Registry.Users;
            RegistryKey cleCurrentConfig = Registry.CurrentConfig;

            //int a = cleRoot.ValueCount + cleCurrentUser.ValueCount + cleLocalMachine.ValueCount + cleUsers.ValueCount + cleCurrentConfig.ValueCount;

            List<RegistryKey> listeRegistre = new List<RegistryKey>();
            listeRegistre.Add(cleRoot);
            listeRegistre.Add(cleCurrentUser);
            listeRegistre.Add(cleLocalMachine);
            listeRegistre.Add(cleUsers);
            listeRegistre.Add(cleCurrentConfig);

            foreach(RegistryKey liste in listeRegistre)
            {
                foreach (var subkeyName in liste.GetSubKeyNames())
                {
                    cleRegistre.Text += subkeyName + "\r\n";
                }    
            }
        }
        public void ListeDisques()
        {
            DriveInfo[] listeDisques = DriveInfo.GetDrives();
            foreach (var disque in listeDisques)
            {
                
                if(disque.DriveType == DriveType.CDRom)
                {
                    Disques.Text += "Lecteur(DVD-Rom) " + disque.Name;
                }
                else
                {
                    double espaceLibre = Math.Round(Convert.ToDouble(disque.TotalFreeSpace)/(1024*1024*1024), 2);
                    double tailletotale = Math.Round(Convert.ToDouble(disque.TotalSize)/(1024*1024*1024), 2);
                    Disques.Text += "Lecteur(HDD) " + disque.Name + ", mémoire disponible : " + espaceLibre + "GB/" +tailletotale + "GB Total\r\n";
                }

            }
        }
        public void ListeTCP()
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            foreach (TcpConnectionInformation connection in connections)
            {
                    ActiveConn.Text += connection.LocalEndPoint.ToString()+ " <--> " +connection.RemoteEndPoint.ToString() +"\r\n";               
            }
        }

        private void Snake_Click(object sender, RoutedEventArgs e)
        {
            Snake snake = new Snake();
            snake.Show();
        }
        private void NomEtIP()
        {

            string strHostName = Dns.GetHostName();

            string IPv4 = Dns.GetHostEntry(strHostName).AddressList[1].ToString();
            string IPv6 = Dns.GetHostEntry(strHostName).AddressList[0].ToString();

            LB_nomMachine.Text = Environment.MachineName + "\r\n" + "IPv4 : " + IPv4 + "\r\n" + "IPv6 : " + IPv6;
            NomUtilisateur.Text = Environment.UserName;

        }
        public void ListeVoix()
        {
            //foreach (InstalledVoice item in speechSynthesizer.GetInstalledVoices())
            //{
            //    VoiceInfo info = item.VoiceInfo;
            //    Console.WriteLine(" Name:          " + info.Name);
            //    Console.WriteLine(" Culture:       " + info.Culture);
            //    Console.WriteLine(" Age:           " + info.Age);
            //    Console.WriteLine(" Gender:        " + info.Gender);
            //    Console.WriteLine(" Description:   " + info.Description);
            //    Console.WriteLine(" ID:            " + info.Id);
            //    Console.WriteLine(" Enabled:       " + item.Enabled);
            //}
            using (SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer())
            {

                // Get information about supported audio formats.  
                string AudioFormats = "";
                foreach (SpeechAudioFormatInfo fmt in speechSynthesizer.Voice.SupportedAudioFormats)
                {
                    AudioFormats += String.Format("{0}\n",
                    fmt.EncodingFormat.ToString());
                }

                // Write information about the voice to the console.  
                Console.WriteLine(" Name:          " + speechSynthesizer.Voice.Name);
                Console.WriteLine(" Culture:       " + speechSynthesizer.Voice.Culture);
                Console.WriteLine(" Age:           " + speechSynthesizer.Voice.Age);
                Console.WriteLine(" Gender:        " + speechSynthesizer.Voice.Gender);
                Console.WriteLine(" Description:   " + speechSynthesizer.Voice.Description);
                Console.WriteLine(" ID:            " + speechSynthesizer.Voice.Id);
                if (speechSynthesizer.Voice.SupportedAudioFormats.Count != 0)
                {
                    Console.WriteLine(" Audio formats: " + AudioFormats);
                }
                else
                {
                    Console.WriteLine(" No supported audio formats found");
                }

                // Get additional information about the voice.  
                string AdditionalInfo = "";
                foreach (string key in speechSynthesizer.Voice.AdditionalInfo.Keys)
                {
                    AdditionalInfo += String.Format("  {0}: {1}\n",
                      key, speechSynthesizer.Voice.AdditionalInfo[key]);
                }

                Console.WriteLine(" Additional Info - " + AdditionalInfo);
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MonSocketClient.Disconnect(false);
        }
    }
}



