using System;
using System.Drawing;
using WebSocket4Net;

namespace Snake
{
    class MessageEventArgs : EventArgs
    {
        public MessageEventArgs(string message) : base()
        {
            Message = message;
        }

        public string Message { get; }
    }

    public class ClientServer
    {
        public event EventHandler OnMessage;
        static string ServerURL = "ws://tron-inker.c9.io";
        WebSocket Socket = new WebSocket(ServerURL);
        Gameplay Game;
        string _message;
        string Message {
            get { return _message; }
            set
            {
                _message = value;
                OnMessage(this, new MessageEventArgs(value));
            }
        }

        public ClientServer(Gameplay game)
        {
            Game = game;
            Socket.Opened += OnSocketOpen;
            Socket.Error += HandleSocketError;
            Socket.MessageReceived += (s, e) => Message = e.Message;
            Socket.DataReceived += HandleData;
            Socket.Closed += (s, e) => Message = (e as ClosedEventArgs).Reason;
        }

        public void Connect()
        {
            Message = "Connecting to server...";
            try
            {
                CloseSocket();
                Socket.Open();
            }
            catch (Exception e)
            {
                // swallow
            }
        }

        void OnSocketOpen(object sender, EventArgs e)
        {
            Socket.Send("gdfFDgert4t$T3DSffert34#fg1");
            Message = "Connection to server established";
            //Game.Score = 0;
            ReportSituation();
        }

        void HandleSocketError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Message = "Error while connecting to server (" + e.Exception.Message + "). Retrying...";
            if (Socket.State == WebSocketState.Open)
            {
                Socket.Close();
            }
            Connect();
        }

        void HandleData(object sender, DataReceivedEventArgs e)
        {
            // 0 - action (2 - move, 0 - enter, 1 - exit, 3 - food, 4 - grid size, 5 - color,
            // 6 - speeds (normal, nitro), 7 - points (food, nitro penalty) )
            
            // 1 - id
            // 2, 3 - score
            // 4 - color
            // 5 - nitro
            // 6~ - info
            var data = e.Data;
            byte id = data[1];

            switch (data[0])
            {
                case 0:
                    break;
                case 1:
                    Game.Opponents.Remove(id);
                    break;
                case 2:
                    int score = data[2];
                    score = (score << 8) | data[3];
                    //int color = data[4];
                    bool nitro = data[5] > 0;
                    var opponent = Game.GetOrMakeOpponent(id, score, nitro);
                    var oppSnake = opponent.Snake;
                    for (int i = 6; i < data.Length; i += 2)
                    {
                        oppSnake.Add(new Vec2(data[i], data[i + 1]));
                    }
                    break;
                case 3:
                    Game.Food.X = data[6];
                    Game.Food.Y = data[7];
                    break;
                case 4:
                    // assign new value to trigger setter
                    Game.Grid = new Vec2(data[6], data[7]);
                    break;
                case 5:
                    var a = data;
                    var b = Game.Opponents;
                    (id == 255 ? Game : Game.Opponents[id]).Color = Color.FromArgb(data[6], data[7], data[8]);
                    break;
                case 6:
                    Game.InitialSpeed = data[6];
                    break;
                case 7:
                    Game.FoodPoints = data[6];
                    break;
                default:
                    break;
            }
        }

        byte[] MakePacket(byte action, int size)
        {
            var packet = new byte[size];
            packet[0] = action;
            packet[1] = 255; // fake id
            packet[2] = (byte)(Game.Score >> 8);
            packet[3] = (byte)Game.Score;
            packet[4] = 0;
            packet[5] = (byte)(Game.Nitro ? 1 : 0);
            return packet;
        }

        public void ReportSituation()
        {
            int i = 6;
            var packet = MakePacket(2, i + Game.Snake.Count * 2);
            foreach (var p in Game.Snake)
            {
                packet[i++] = (byte)p.X;
                packet[i++] = (byte)p.Y;
            }
            if (Socket.State == WebSocketState.Open)
            {
                Socket.Send(packet, 0, packet.Length);
            }
        }

        public void ReportNewFood()
        {
            if (Socket.State == WebSocketState.Open)
            { 
                var packet = MakePacket(3, 8);
                packet[6] = (byte)Game.Food.X;
                packet[7] = (byte)Game.Food.Y;
                Socket.Send(packet, 0, packet.Length);
            }
        }

        public void CloseSocket(string reason = "Connection to server lost")
        {
            if (Socket.State == WebSocketState.Open)
            {
                Socket.Close(reason);
            }
        }
    }
}