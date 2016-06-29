using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace Bot.Core {

    public class MessageSender {
        private TextWriter Output { get; }

        private ConcurrentDictionary<Guid, string> MessageQueue { get; }

        private string Channel { get; }

        private int MessagesSent { get; set; }

        private const int MaxMessages = 20; //100 if mod

        private const int GlobalTime = 30000;//30s

        private Task GlobalTimer { get; set; }

        private CancellationTokenSource TokenSource { get; }

        public MessageSender(TextWriter output, string channel) {
            this.Output = output;
            this.MessageQueue = new ConcurrentDictionary<Guid, string>();
            this.Channel = channel;
            this.MessagesSent = 0;
            TokenSource = new CancellationTokenSource();
            var token = TokenSource.Token;
            Task.Factory.StartNew(() => SendMessagesJob(token), token);
        }

        ~MessageSender() {
            TokenSource.Cancel();
        }

        public void AddMessage(string message) {
            MessageQueue.TryAdd(Guid.NewGuid(), message);
        }

        private async Task SendMessagesJob(CancellationToken token) {
            var running = true;
            while (running) { //run forever sending messages
                if (token.IsCancellationRequested) {
                    running = false;
                }
                if (!MessageQueue.Any()) {
                    await Task.Delay(1000);
                } else {
                    var messageItem = MessageQueue.LastOrDefault();
                    var message = messageItem.Value;
                    if (GlobalTimer == null || GlobalTimer.IsCompleted) {
                        GlobalTimer = Task.Run(() => StartGlobalTimer());
                    }
                    if (MessagesSent >= MaxMessages) {
                        await GlobalTimer;
                    }
                    Output.Write("PRIVMSG " + Channel + " :" + message + "\r\n");
                    Output.Flush();

                    string val;
                    MessageQueue.TryRemove(messageItem.Key, out val);
                    MessagesSent++;
                }
            }
        }

        private async Task StartGlobalTimer() {
            await Task.Delay(GlobalTime);
            MessagesSent = 0;
        }

    }
}