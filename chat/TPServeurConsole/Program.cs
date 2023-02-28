using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TPServeurConsole
{
    class Program
    {
        public string port = "6060";
        string txt;
        string texte_connexion;
        bool heure_connexion;
        public static List<Client> ListeClient { get; private set; }

        public static void Main()
        {
            ServerSocket();
        }
        public static void ServerSocket()
        {
            Socket SocketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint monEP = new IPEndPoint(IPAddress.Any, 6060);
            SocketServer.Bind(monEP);
            SocketServer.Listen(1000);
            Console.WriteLine("Socket serveur initialisé sur le port 6060");
            ListeClient = new List<Client>();

            while (true)
            {
                Console.WriteLine("En attente d'une connexion");
                Socket socketEnvoi = SocketServer.Accept();

                Program prg = new Program();
                prg.TraitementConnexion(socketEnvoi);
            }
        }
        public void TraitementConnexion(Socket socketEnvoi)
        {
            Console.WriteLine("Socket client connecté, création d'un thread");
            Client clt = new Client(socketEnvoi);
            ListeClient.Add(clt);
            Thread ThreadClient = new Thread(clt.TraitementClient);
            ThreadClient.Start();

        }
        public void Broadcast(string Message)
        {
            Console.WriteLine("Broadcast: " + Message);
            foreach (var clientConnecte in ListeClient)
            {
                clientConnecte.EnvoiMessage(Message);
            }
        }
        public void removeClient(Client clt)
        {
            ListeClient.Remove(clt);
        }
        public void addClient(Client clt)
        {
            ListeClient.Add(clt);
        }
    }
    class Client
    {
        public Socket _socketClient;
        public string _pseudo;
        public Client(Socket socket)
        {
            _socketClient = socket;
        }
        public void TraitementClient()
        {
            Console.WriteLine("ThreadClient est lancé");
            byte[] octets = new byte[100000];
            int Recu;
            try
            {
               Recu = _socketClient.Receive(octets);
            }
            catch (Exception)
            {
                Console.WriteLine("Erreur pendant la réception du pseudo");
                return;
            }

            string Message = Encoding.UTF8.GetString(octets);
            Message = Message.Substring(0, Recu);
            if (Recu > 20)
            {
                _pseudo = Message.Substring(0, 20);
            }
            else
            {
                _pseudo = Message.Substring(0, Recu);
            }
            Program prg = new Program();
            prg.Broadcast( _pseudo + " identifié sur le réseau.");
            while (_socketClient.Connected)
            {
                try
                {
                    string Msg;

                    //var stream = new NetworkStream(MonSocketClient);

                    //int bytesRead = stream.Read(Octets, 0, Octets.Length);
                    //byte[] theData = Octets.Take(bytesRead).ToArray();
                    //Message = Encoding.UTF8.GetString(theData);

                    Recu = 0;
                    Recu = _socketClient.Receive(octets);
                    if (Recu > 0)
                    {

                        Msg = Encoding.UTF8.GetString(octets);
                        Msg = Msg.Substring(0, Recu);
                        if (Msg == "/ListeUser")
                        {
                            Client clientPourListe = new Client(_socketClient);
                            clientPourListe.EnvoiListe();
                            return;
                        }

                        prg.Broadcast(_pseudo + " a dit : " + Msg);

                    }
                    else
                    {
                        Client clt2 = new Client(_socketClient);
                        prg.removeClient(clt2);
                        _socketClient.Close();
                        prg.Broadcast(_pseudo + " déconnecté");
                        return;
                    }
                }
                catch (Exception)
                {
                    Client clt2 = new Client(_socketClient);
                    prg.removeClient(clt2);
                    _socketClient.Close();
                    prg.Broadcast(_pseudo + " déconnecté");
                    return;
                }
            }
            if (_socketClient.Connected == false)
            {
                Client clt2 = new Client(_socketClient);
                prg.removeClient(clt2);
                _socketClient.Close();
                prg.Broadcast(_pseudo + " déconnecté");
                return;
            }
        }
        public void EnvoiMessage(string Message)
        {
            try
            {
                Byte[] Msg = Encoding.UTF8.GetBytes(Message);
                int Envoi = _socketClient.Send(Msg);
                Console.WriteLine(Envoi + " octets envoyés au client " + _pseudo);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Message non remis: " + ex.Message);
                _socketClient.Close();
                Client clt = new Client(_socketClient);
                Program prg = new Program();
                prg.removeClient(clt);
            }
        }
        public void EnvoiListe()
        {
            //Byte[] listeClients = { };

            List<Client> ListeClient = Program.ListeClient;

            byte[] entete = Encoding.UTF8.GetBytes("Liste des connectés : ");
            int envoiEntete = _socketClient.Send(entete);

            foreach (var clientConnecte in ListeClient)
            {
                //string pseudo = clientConnecte._pseudo;
                //listeClients.Add(pseudo);

                //listeClients.Add(clientConnecte._pseudo);
                byte[] byteClient = Encoding.UTF8.GetBytes(clientConnecte._pseudo);
                int Envoi = _socketClient.Send(byteClient);
            }
        }
    }
}
