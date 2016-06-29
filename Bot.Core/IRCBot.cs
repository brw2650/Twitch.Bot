using System;
using System.IO;
using System.Net.Sockets;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Core {

    public class IRCBot {
        private const string Name = "beanbotxz";
        private const string Server = "irc.chat.twitch.tv";
        private const int Port = 6667;
        private const string Channel = "#speckxz";
        private const string OAuth = "oauth:axvvfuhks5fvncj2ip5qpdjumyzxsd";
        private TextReader Input;
        private TextWriter Output;
        private MessageSender Sender { get; set; }

        public void Run() {
            TcpClient sock = new TcpClient();

            //Connect to irc Server and get Input and Output text streams from TcpClient.
            sock.ConnectAsync(Server, Port).Wait();
            if (!sock.Connected) {
                Console.WriteLine("Failed to connect!");
                return;
            }
            Input = new StreamReader(sock.GetStream());
            Output = new StreamWriter(sock.GetStream());

            //Starting USER and Name login commands 
            Output.Write(
                "PASS " + OAuth + "\r\n" + "NICK " + Name + "\r\n"
            );
            Output.Flush();

            Sender = new MessageSender(Output, Channel);
            SendMessage("Hello");

            //Process each line received from irc Server
            for (var buf = Input.ReadLine(); ; buf = Input.ReadLine()) {

                //Display received irc message
                Console.WriteLine(buf);

                //Send pong reply to any ping messages
                if (buf.StartsWith("PING ")) {
                    Output.Write(buf.Replace("PING", "PONG") + "\r\n");
                    Output.Flush();
                }

                if (buf[0] != ':')
                    continue;

                /* IRC commands come in one of these formats:
                 * :Name!USER@HOST COMMAND ARGS ... :DATA\r\n
                 * :SERVER COMAND ARGS ... :DATA\r\n
                 */

                //After Server sends 001 command, we can set mode to bot and join a Channelnel
                if (buf.Split(' ')[1] == "001") {
                    Output.Write(
                        "MODE " + Name + " +B\r\n" +
                        "JOIN " + Channel + "\r\n"
                    );
                    Output.Flush();
                }

                if (buf.Contains(":?")) {
                    var query = buf.Split('?').Last().Trim();
                    Task.Run(() => RunFilmQuery(query));
                }
            }
        }

        private void SendMessage(string message) {
            Sender.AddMessage(message);
        }

        private async Task RunFilmQuery(string query) {
            var fService = new FilmService();
            var res = await fService.GetFilmInfo(query);
            if (res.Response) {
                SendMessage("Type: " + res.Type);
                SendMessage("Title: " + res.Title);
                SendMessage("Plot: " + res.Plot);
                SendMessage("Released: " + res.Released);
                SendMessage("Actors: " + res.Actors);
            } else {
                SendMessage("Not found");
            }
        }

    }
}