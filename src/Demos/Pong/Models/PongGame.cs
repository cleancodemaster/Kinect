﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Threading;

namespace Kinect.WPF.nPong.Models
{
    public class PongGame
    {
        #region EventHandlers
        public event EventHandler Updated;
        protected virtual void OnUpdated()
        {
            var handler = this.Updated;

            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        public event EventHandler<ScoreEventArgs> Scored;
        protected virtual void OnScored(Models.Paddle.Side side, Ball ball)
        {
            var handler = this.Scored;

            if (handler != null)
            {
                handler(this, new ScoreEventArgs(side, ball));
            }
        }
        #endregion

        #region PongGame Members
        public Rectangle Boundry { get; set; }
        private DispatcherTimer _gameLoop;
        public ObservableCollection<Ball> Balls { get; set; }
        public ObservableCollection<Paddle> Paddles { get; set; }
        public int ScoreLeft { get; set; }
        public int ScoreRight { get; set; }

        public bool IsRunning
        {
            get { return _gameLoop.IsEnabled; }
        }
        #endregion

        #region Singleton
        private static readonly PongGame _instance = new PongGame();

        private PongGame()
        {
            this.Initialize();
        }

        public static PongGame Instance
        {
            get { return _instance; }
        }
        #endregion

        public void Start()
        {
            _gameLoop.Start();
        }

        public void Stop()
        {
            _gameLoop.Stop();
        }

        private void Initialize()
        {
            ScoreLeft = 0;
            ScoreRight = 0;
            Balls = new ObservableCollection<Ball>();
            Paddles = new ObservableCollection<Paddle>();
            _gameLoop = new DispatcherTimer();
            _gameLoop.Interval = TimeSpan.FromMilliseconds(1);
            _gameLoop.Tick += _gameLoop_Process;
        }

        void _gameLoop_Process(object sender, EventArgs e)
        {
            Paddles.AsParallel().ForAll(p => p.Move());
            Balls.AsParallel().ForAll(b => b.Move());
            OnUpdated();
        }

        public void AddBall()
        {
            var ball = new Ball(25, new Random(DateTime.Now.Millisecond).Next(360),2);
            Balls.Add(ball);
            ball.Scored += ball_Scored;
        }

        public void Reset()
        {
            _gameLoop.Stop();
            Balls.Clear();
            Paddles.Clear();
            ScoreLeft = 0;
            ScoreRight = 0;
        }

        void ball_Scored(object sender, ScoreEventArgs e)
        {
            if (e.Side == Paddle.Side.Left)
            {
                ScoreLeft += 1;
            }
            else if (e.Side == Paddle.Side.Right)
            {
                ScoreRight += 1;
            }
            OnScored(e.Side, e.Ball);
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                Balls.Remove(e.Ball);
                AddBall();
            });
        }

        public void AddPaddle(Paddle.Side _side, bool isComputerControlled, int kinectUserId)
        {
            var x = new Paddle(_side, isComputerControlled, kinectUserId);
            Paddles.Add(x);
        }

    }
}