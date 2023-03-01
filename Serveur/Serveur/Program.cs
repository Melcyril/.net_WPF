using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Serveur
{
    class Program
    {
        public string port = "6060";
        string txt;
        string texte_connexion;
        bool heure_connexion;
        public static List<Client> ListClient { get; set; }


        static void Main() { ServeurSocket(); }

        public static void ServeurSocket()
        {
            Socket MonScoketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint monEP = new IPEndPoint(IPAddress.Any, 6060);
            MonScoketServer.Bind(monEP);
            MonScoketServer.Listen(1000);
            Console.WriteLine("Socket server initialisé sur le port 6060");
            ListClient = new List<Client>();
            while (true)
            {
                Console.WriteLine("en attente de connexion");
                Socket SockerEnvoi = MonScoketServer.Accept();

                Program prg = new Program();
                prg.TraitementConnexion(SockerEnvoi);
            }
        }
        public void Broadcast(string Message)
        {
            Console.WriteLine("Broadcast" + Message);
            foreach (var ClientConnecte in ListClient)
            {
                ClientConnecte.EnvoiMessage(Message);
            }
        }

        public void TraitementConnexion(Socket SocketEnvoi)
        {
            Console.WriteLine("Socket client connecté, create d'un thred");
            Client clt = new Client(SocketEnvoi);
            ListClient.Add(clt);
            Thread ThreadClient = new Thread(clt.TraitementClient);
            ThreadClient.Start();
        }
        public class Client
        {
            public Socket _SocketClient;
            public string _Pseudo;
            public Client(Socket sock)
            {
                _SocketClient = sock;
            }
            public void TraitementClient()
            {
                Console.WriteLine("Thread client lance");
                byte[] Octets = new byte[100000];
                int Recu;
                try
                {
                     Recu = _SocketClient.Receive(Octets); 
                }
                catch (Exception)
                {

                    Console.WriteLine("erreur pendant la reception du pseudo");
                    return;
                }
                string Message = System.Text.Encoding.UTF8.GetString(Octets);
                Message = Message.Substring(0, Recu);
                if (Recu > 9)
                {
                    _Pseudo = Message.Substring(0, 9);
                }
                else
                {
                    _Pseudo = Message.Substring(0, Recu);
                }
                Program prg = new Program();
                prg.Broadcast(_Pseudo + "Identifie sur le reseau");
                while (_SocketClient.Connected)
                {
                    try
                    {
                        string Msg;
                        Recu = _SocketClient.Receive(Octets);
                        if (Recu > 0)
                        {
                            Msg = System.Text.Encoding.UTF8.GetString(Octets);
                            Msg = Msg.Substring(0, Recu);
                            prg.Broadcast(_Pseudo + "a dit:" + Msg);
                        }
                        else
                        {
                            Client clt2 = new Client(_SocketClient);
                            prg.removeClient(clt2);
                            _SocketClient.Close();
                            prg.Broadcast(_Pseudo + "deconnecté");
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        Client clt2 = new Client(_SocketClient);
                        prg.removeClient(clt2);
                        _SocketClient.Close();
                        prg.Broadcast(_Pseudo + "deconnecté");
                        return;
                    }
                }
                if (_SocketClient.Connected == false)
                {
                    Client clt2 = new Client(_SocketClient);
                    prg.removeClient(clt2);
                    _SocketClient.Close();
                    prg.Broadcast(_Pseudo + "deconnecté");
                    return;
                }
            }
            public void EnvoiMessage(string Message)
            {
                try
                {
                    Byte[] Msg = System.Text.Encoding.UTF8.GetBytes(Message);
                    int Envoi = _SocketClient.Send(Msg);
                    Console.WriteLine(Envoi + "Octets envoie au client" + _Pseudo);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Mesage non remi" + ex.Message);
                    _SocketClient.Close();
                    Client clt = new Client(_SocketClient);
                    Program prg = new Program();
                    prg.removeClient(clt);
                }
            }
        }
        public void AddClient(Client clt)
        {
            ListClient.Add(clt);
        }

        public void removeClient(Client clt)
        {
            ListClient.Remove(clt);
        }
    }
}
