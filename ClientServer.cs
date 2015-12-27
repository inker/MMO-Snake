using System;
using System.Collections.Generic;
using System.Linq;
using WebSocket4Net;

namespace Snake
{
    class MyEventArgs : EventArgs
    {
        public MyEventArgs(string message) : base()
        {
            Message = message;
        }

        public override string ToString() => Message;      
        string Message { get; set; }
    }
    public class ClientServer
    {
        public event EventHandler OnMessage;
        static string ServerURL = "ws://tron-inker.c9.io";
        WebSocket Socket;
        Game Game;
        string Msg { set { OnMessage(this, new MyEventArgs(value)); } }

        public ClientServer(Game game)
        {
            Game = game;
        }

        public void Connect()
        {
            Msg = "Connecting to server...";
            Socket = new WebSocket(ServerURL);
            Socket.Opened += OnSocketOpen;
            Socket.Error += HandleSocketError;
            Socket.MessageReceived += HandleMessage;
            Socket.Closed += (s, e) => Msg = "Connection to server lost. You've probably killed yourself.";
            Socket.Open();
        }

        private void OnSocketOpen(object sender, EventArgs e)
        {
            Msg = "Connection to server established";
            //Game.Score = 0;
            ReportSituation();
        }

        private void HandleSocketError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Msg = "Error while connecting to server (" + e.Exception.Message + "). Retrying...";
            if (Socket.State == WebSocketState.Open)
            {
                Socket.Close();
            }
            Connect();
        }

        private void HandleMessage(object sender, MessageReceivedEventArgs e)
        {
            // remake to:
            // 0 - id
            // 1 - score
            // 2 - color
            // 3 - action (0 - move, 1 - exit, 
            // 4~ - info
            var msg = e.Message;
            if (msg.StartsWith("food:"))
            {
                var arr = msg.Substring(5)
                    .Split(',')
                    .Select(i => byte.Parse(i))
                    .ToArray();
                Game.Food.X = arr[0];
                Game.Food.Y = arr[1];
            }
            else if (msg.StartsWith("color:"))
            {
                Game.ColorNum = int.Parse(msg.Substring(6));
            }
            else if (msg.StartsWith("exit:"))
            {
                byte id = byte.Parse(msg.Substring(5));
                Game.Opponents.Remove(id);
            }
            else
            {
                byte[] bytes = msg.Select(c => (byte)c).ToArray();
                byte id = bytes[0];
                int score = (bytes[1] << 8) | bytes[2];
                int color = bytes[3];
                bool nitro = bytes[4] > 0;
                Player opponent;
                if (Game.Opponents.TryGetValue(id, out opponent))
                {
                    opponent.Score = score;
                    opponent.ColorNum = color;
                    opponent.Nitro = nitro;
                    opponent.Snake.Clear();
                }
                else
                {
                    opponent = new Player(new List<Vec2>(), color, score, nitro);
                    Game.Opponents.Add(id, opponent);
                }
                var oppSnake = opponent.Snake;
                for (int i = 4; i < bytes.Length; i += 2)
                {
                    oppSnake.Add(new Vec2(bytes[i], bytes[i + 1]));
                }
            }
        }

        public void ReportSituation()
        {
            var packet = new List<byte>(Game.Snake.Count << 1);
            packet.Add((byte)Game.Score);
            packet.Add((byte)(Game.Score >> 8));
            packet.Add((byte)Game.ColorNum);
            packet.Add((byte)(Game.Nitro ? 1 : 0));
            foreach (var p in Game.Snake)
            {
                packet.Add((byte)p.X);
                packet.Add((byte)p.Y);
            }
            if (Socket.State == WebSocketState.Open)
            {
                Socket.Send(packet.ToArray(), 0, packet.Count);
            }
        }

        public void ReportNewFood()
        {
            if (Socket.State == WebSocketState.Open)
            {
                Socket.Send(string.Format("food:{0},{1}", Game.Food.X, Game.Food.Y));
            }
        }

        public void CloseSocket()
        {
            if (Socket.State == WebSocketState.Open)
            {
                Socket.Close();
            }
        }
    }
}
