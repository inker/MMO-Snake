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
            Msg = message;
        }

        public override string ToString() => Msg;      
        string Msg { get; }
    }

    public class ClientServer
    {
        public event EventHandler OnMessage;
        static string ServerURL = "ws://tron-inker.c9.io";
        WebSocket Socket = new WebSocket(ServerURL);
        Game Game;
        string Message {
            set
            {
                OnMessage(this, new MyEventArgs(value));
            }
        }

        public ClientServer(Game game)
        {
            Game = game;
            Socket.Opened += OnSocketOpen;
            Socket.Error += HandleSocketError;
            Socket.DataReceived += HandleData;
            Socket.MessageReceived += HandleMessage;
            Socket.Closed += OnSocketClosed;
        }

        public void Connect()
        {
            CloseSocket();
            Message = "Connecting to server...";
            if (Socket.State != WebSocketState.Open)
            {
                Socket.Open();
            }
        }

        void OnSocketClosed(object sender, EventArgs e)
        {
            Message = "Connection to server lost. You've probably killed yourself.";
        }

        void OnSocketOpen(object sender, EventArgs e)
        {
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
            // remake to:
            // 0 - id
            // 1 - score
            // 2 - color
            // 3 - action (0 - move, 1 - exit, 
            // 4~ - info
            var bytes = e.Data;
            byte id = bytes[0];
            int score = bytes[1];
            score = (score << 8) | bytes[2];
            int color = bytes[3];
            bool nitro = bytes[4] > 0;

            var opponent = Game.GetOrMakePlayer(id, color, score, nitro);
            var oppSnake = opponent.Snake;
            for (int i = 5; i < bytes.Length; i += 2)
            {
                oppSnake.Add(new Vec2(bytes[i], bytes[i + 1]));
            }
        }

        void HandleMessage(object sender, MessageReceivedEventArgs e)
        {
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
        }

        public void ReportSituation()
        {
            var packet = new List<byte>(Game.Snake.Count << 1);
            packet.Add((byte)(Game.Score >> 8));
            packet.Add((byte)Game.Score);
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
