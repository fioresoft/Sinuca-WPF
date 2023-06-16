using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Numerics;
using BouncingBalls02;
using MyPropertyDlg3;
using System.Threading;
using System.Diagnostics;
using System.Media;
using System.Runtime.InteropServices;

namespace BouncingBalls02
{
    public enum GameType
    {
        GM_RANDOM,
        GM_SINUCA,
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window,IGame,IGameEvents
    {
        public static readonly int m_cBalls = 8;
        public static Ball[]? balls = new Ball[m_cBalls];
        public static DispatcherTimer timer = new DispatcherTimer();
        static Random rnd = new Random();
        public static Hole[]? holes = new Hole[6];
        private static GameType m_gameType;
        private int m_turnBall = 1;     // bola da vez
        private bool m_bColored = true; // pode encaçapar bola colorida?
        private int m_player1 = 0;
        private int m_player2 = 0;
        private int m_currentPlayer = 1;
        private static Color[] colors = new Color[8]{Colors.White, Colors.Red, Colors.Yellow,
                Colors.Maroon,Colors.LightGreen,Colors.Blue,Colors.Pink,Colors.Black};
        private string? m_pathSound = System.IO.Path.GetFullPath(".\\Sounds\\");
        public static bool m_bHitting = false; // cue hit ball white
        public static Ball? m_hit = null;    // cue hit ball white which hit colored ball
        private static GameType m_nextGame = GameType.GM_SINUCA;

        [DllImport("winmm.dll")]
        private static extern bool PlaySound(string lpszName, int hModule, int dwFlags);


        public MainWindow()
        {
            InitializeComponent();

            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Tick += Timer_Tick;
            rnd = new Random();
            Ball.canvas = myCanvas;
            Ball.progress = myProgress;
            Ball.label = myLabel;
            Ball.toolBar = myToolBar;
            Ball.m_bt8 = ball8;
            Ball.gameEvents = this;
            Hole.gameEvents = this;
            
        }
        private void myWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
            //InitBallsRandom(m_cBalls);
            m_currentPlayer = 2;
            OnChangePlayer();
            Init(GameType.GM_SINUCA);
            timer.Start();
            myCanvas.KeyDown += Canvas_KeyDown;
            FocusManager.GetFocusedElement(this);
            FocusManager.GetFocusedElement(myCanvas);
        }

        private void LoadSettings()
        {
            Dlg.options.Load();

            Ball.m_radius = Dlg.options.ball_radius;
            Hole.m_r = Dlg.options.hole_radius;
            Ball.m_table_friction = Dlg.options.table_friction;
            Ball.m_wall_friction = Dlg.options.wall_friction;
        }
        private void InitBallsRandom(int count)
        {
            m_gameType = GameType.GM_RANDOM;
            if (balls != null)
            {
                foreach (Ball b in balls)
                {
                    if (b != null && b.sprite != null)
                        myCanvas.Children.Remove(b.sprite);
                }
            }
            if (balls == null)
            {
                balls = new Ball[count];
            }
            for (int i = 0; i < count; i++)
            {
                balls[i] = new Ball((i + 1) * rnd.Next(100), (i + 1) * rnd.Next(100),
                    (float)rnd.Next(200), (float)rnd.Next(200), i);
            }
        }
        private void InitBallsSinuca()
        {
            m_gameType = GameType.GM_SINUCA;

            if (balls == null)
            {
                balls = new Ball[8];
            }

            if (balls != null)
            {
                foreach (Ball b in balls)
                {
                    if (b != null && b.sprite != null)
                        myCanvas.Children.Remove(b.sprite);
                }
            }
            
            for (int i = 0; i < 8; i++)
            {
                balls[i] = new Ball(colors[i], i);
                switch (i)
                {
                    // TODO ***********REMOVE MAGIC NUMBERS**********
                    case 0:
                        balls[i].m_p = GetBall0Pt();
                        balls[i].is_placing = true;
                        break;
                    case 1: // red
                        balls[i].m_p = GetBall1Pt();
                        break;
                    case 2: // yellow
                        balls[i].m_p = GetBall2Pt();
                        break;
                    case 3: // maroon
                        balls[i].m_p = GetBall3Pt();
                        break;
                    case 4: // green
                        balls[i].m_p = GetBall4Pt();
                        break;
                    case 5: // blue
                        balls[i].m_p = GetBall5Pt();
                        break;
                    case 6: // pink
                        balls[i].m_p = GetBall6Pt();
                        break;
                    case 7: // black
                        balls[i].m_p = GetBall7Pt();
                        break;

                }
            }
        }
        private Vector2 GetBall0Pt()
        {
            return new Vector2(33.031f, (float)(137.225f + pathD.Height / 2));
        }
        private Vector2 GetBall1Pt()
        {
            return new Vector2(holes[1].m_c.X + (holes[2].m_c.X - holes[1].m_c.X) / 2,
                137.225f + 140 - 2 * Ball.m_radius);
        }
        private Vector2 GetBall2Pt()
        {
            return new Vector2((float)lineD.Point.X, 137.225f + 140 - 2 * Ball.m_radius);
        }
        private Vector2 GetBall3Pt()
        {
            return new Vector2((float)lineD.Point.X,
                137.225f + 70 - Ball.m_radius);
        }
        private Vector2 GetBall4Pt()
        {
            return new Vector2((float)lineD.Point.X, 137.225f);
        }
        private Vector2 GetBall5Pt()
        {
            return new Vector2(holes[1].m_c.X, 137.225f + 70 - Ball.m_radius);
        }
        private Vector2 GetBall6Pt()
        {
            return new Vector2(holes[1].m_c.X + (holes[2].m_c.X - holes[1].m_c.X) / 2,
                137.225f + 70 - Ball.m_radius);
        }
        private Vector2 GetBall7Pt()
        {
            return new Vector2(holes[1].m_c.X + (holes[2].m_c.X - holes[1].m_c.X) / 2 + 100,
                137.225f + 70 - Ball.m_radius);
        }
        public static Color MakeRandomColor()
        {
            return Color.FromRgb((byte)MainWindow.rnd.Next(256),
            (byte)MainWindow.rnd.Next(256), (byte)MainWindow.rnd.Next(256));
        }
        private void InitHoles()
        {
            int i, j, hole;
            float cx = (float)(myCanvas.Width / 2);
            float cy = (float)(myCanvas.Height);

            foreach(Hole h in holes)
            {
                if(h != null && h.sprite != null)
                    myCanvas.Children.Remove(h.sprite);
            }
            holes = null;
            if(holes == null || holes.Length == 0)
            {
                holes = new Hole[6];
            }

            for (hole = 0, i = 0; i < 2; i++)
            {
                for (j = 0; j < 3; j++, hole++)
                {
                    Ellipse sprite = new Ellipse
                    {
                        Width = 2 * Hole.m_r,
                        Height = 2 * Hole.m_r,
                        //Fill = new SolidColorBrush(Colors.Black),
                        Stroke = new SolidColorBrush(Colors.Black),
                    };
                    myCanvas.Children.Add(sprite);
                    holes[hole] = new Hole(sprite, hole);
                    holes[hole].m_c.X = (cx * j);
                    holes[hole].m_c.Y = (cy * i);
                    sprite.SetValue(Canvas.LeftProperty,(double) holes[hole].m_c.X - sprite.Width/2);
                    sprite.SetValue(Canvas.TopProperty, (double)holes[hole].m_c.Y - sprite.Height/2);
                }
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            for (int i = 0; i < MainWindow.m_cBalls; i++)
            {
                for (int j = 0; j < m_cBalls; j++)
                {
                    if (i != j && !balls[i].m_gone && !balls[j].m_gone)
                    {
                        if (balls[j].is_shock(balls[i]))
                        {
                            balls[j].do_shock(balls[i]);
                            //balls[j].Move(0.1f);
                        }
                    }
                }
                balls[i].Move(0.05f);
            }
        }

        private void rot5_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < MainWindow.balls.Length; i++)
            {
                if (balls[i].m_has_cue)
                {
                    Ball.m_cue_angle += 5;
                    balls[i].draw_cue();
                }
            }
        }

        private void RotMinus5_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < MainWindow.balls.Length; i++)
            {
                if (balls[i].m_has_cue)
                {
                    Ball.m_cue_angle -= 5;
                    balls[i].draw_cue();
                }
            }
        }

        private void rot1_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < MainWindow.balls.Length; i++)
            {
                if (balls[i].m_has_cue)
                {
                    Ball.m_cue_angle += 1;
                    balls[i].draw_cue();
                }
            }
        }

        private void RotMinus1_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < MainWindow.balls.Length; i++)
            {
                if (balls[i].m_has_cue)
                {
                    Ball.m_cue_angle -= 1;
                    balls[i].draw_cue();
                }
            }
        }
        public static bool balls_stopped()
        {
            bool res = true;
            int gone = 0;
            for (int i = 0; i < balls.Length; i++)
            {
                if (!balls[i].m_gone)
                {
                    if (balls[i].m_v.Length() != 0)
                    {
                        res = false;
                    }
                }
                else
                    gone++;
            }
            if (gone == m_cBalls)
                Ball.m_bt8.IsEnabled = true;
            return res;
        }
        private void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    for (int i = 0; i < MainWindow.balls.Length; i++)
                    {
                        if (MainWindow.balls[i].m_has_cue)
                        {
                            Ball.m_cue_angle += 1;
                            MainWindow.balls[i].draw_cue();
                        }
                    }
                    break;
                case Key.Right:
                    for (int i = 0; i < MainWindow.balls.Length; i++)
                    {
                        if (MainWindow.balls[i].m_has_cue)
                        {
                            Ball.m_cue_angle -= 1;
                            MainWindow.balls[i].draw_cue();
                        }
                    }
                    break;
                default:
                    e.Handled = false;
                    break;
            }
        }

        private void ball8_Click(object sender, RoutedEventArgs e)
        {
            Init(m_nextGame);
        }

        private void menuOptions_Click(object sender, RoutedEventArgs e)
        {
            Dlg dlg = new Dlg();

            if(dlg.ShowDialog() == true)
            {
                Ball.m_radius = Dlg.options.ball_radius;
                Hole.m_r = Dlg.options.hole_radius;
                Ball.m_table_friction = Dlg.options.table_friction;
                Ball.m_wall_friction = Dlg.options.wall_friction;
                Reset();
            }
        }

        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void menuHelp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("There are no rules, just 8 random balls\nTo use" +
                " the keyboard hit tab till you leave the menu\nTo shoot the ball first click on it" +
                " then right click and drag");
        }

        private void menuAbout_Click(object sender, RoutedEventArgs e)
        {
            About dlg = new About();
            dlg.ShowDialog();
        }
        private void Reset()
        {
            Init(m_gameType);
        }

        private void myWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Thread.CurrentThread.IsBackground = true;
            timer.Stop();
        }

        public void Init(GameType gameType)
        {
            InitHoles();
            switch(gameType)
            {
                case GameType.GM_RANDOM:
                    InitBallsRandom(8);
                    break;
                case GameType.GM_SINUCA:
                    InitBallsSinuca();
                    ellipseTurn.Fill = new SolidColorBrush(colors[1]);
                    tbPlayer1.Text = "0";
                    tbPlayer2.Text = "0";
                    tbPlayer1.Background = new SolidColorBrush(Colors.Crimson);
                    break;
            }
        }

        public void OnBallInHole(Ball ball)
        {
            PlaySound(m_pathSound + "pocket.wav",
                0, 1);
            ball.m_bInHole = true;
            if (m_gameType == GameType.GM_RANDOM)
                return;
            //Debug.Assert(m_hit?.m_i == i);
            
            // TODO uncomment hack
            //else if (!m_bHitting /*&& m_hit == null*/)
            //{
            //    Debug.WriteLine("\nm_bHitting era falso e m_hit==null");
            //    return;
            //}
            if(ball.m_i == m_turnBall)
            {
                Debug.WriteLine($"\nturn ball: {m_turnBall}");
                OnScore(ball.m_i);
                m_turnBall++;
                m_bColored = true;
                if(m_turnBall == 8)
                {
                    OnEnd();
                }
                else
                {
                    ellipseTurn.Fill = new SolidColorBrush(colors[m_turnBall]);
                }
            }
            else if(ball.m_i == 0)
            {
                Debug.WriteLine($"\nwhite ball: {ball.m_i}");
                OnChangePlayer();
                OnPlaceBall(ball.m_i);
            }
            else if (m_bColored)
            {
                Debug.WriteLine($"\ncolored ball: {ball.m_i}");
                m_bColored = false;
                OnScore(ball.m_i);
                myCanvas.Children.Remove(ball.sprite);
                balls[ball.m_i].m_gone = true;
                OnPlaceBall(ball.m_i);
            }
            else
            {
                Debug.WriteLine($"\nother ball: {ball.m_i}");
                OnScore(-ball.m_i);
                OnChangePlayer();
                OnPlaceBall(ball.m_i);
            }
            m_hit = null;
            m_bHitting = false;
            Debug.WriteLine("m_bHitting set to false at OnBallInHole");

        }

        public void OnBallOutHole(int i)
        {
            if (m_bHitting)
            {
                OnChangePlayer();
                m_bHitting = false;
                m_hit = null;
                Debug.WriteLine("m_bHitting set to false at OnBallOutHole");
            }
        }
        public void OnEnd()
        {
            int winner = this.m_player1 > this.m_player2 ? 1 : 2;
            MessageBox.Show("player " + winner + " wins");
        }
        public void OnScore(int i)
        {
            if(m_currentPlayer == 1)
            {
                this.m_player1 += i;
                tbPlayer1.Text = this.m_player1.ToString();
            }
            else
            {
                this.m_player2 += i;
                tbPlayer2.Text = this.m_player2.ToString();
            }
        }
        public void OnChangePlayer()
        {
            m_currentPlayer = m_currentPlayer == 1? 2: 1;
            tbStatus.Text = "player " + m_currentPlayer + " turn." +
                "ball " + m_turnBall;
            if (m_bColored)
            {
                tbStatus.Text += " or colored";
            }
            if (m_currentPlayer == 1)
            {
                tbPlayer1.Background = new SolidColorBrush(Colors.Crimson);
                tbPlayer2.Background = new SolidColorBrush(Colors.White);
            }
            else
            {
                tbPlayer1.Background= new SolidColorBrush(Colors.Black);
                tbPlayer2.Background = new SolidColorBrush(Colors.Crimson);
            }
            Debug.WriteLine("\nOnChangePlayer");
            Debug.WriteLine(m_pathSound);  
            PlaySound(m_pathSound + "hit.wav",
                0, 1);
        }
        public void OnPlaceBall(int i)
        {
            balls[i].m_gone = false;
            balls[i].m_p = new Vector2(100, 100);
            balls[i].m_v *= 0;
            balls[i].is_placing = true;
            myCanvas.Children.Add(balls[i].sprite);
            Mouse.Capture(balls[i].sprite);
            tbStatus.Text = "player " + this.m_currentPlayer + " place ball " + i;
            m_hit = null;
            m_bHitting = false;
            balls[i].m_bInHole = false;
        }
        public void OnFault(int i)
        {
            OnChangePlayer();
            OnPlaceBall(i);
        }
        public void OnBallHitByCue(int i)
        {
            PlaySound(m_pathSound + "cue_shoot.wav",
                0, 1);
            //if (i == 0)
            //{
            //    m_bHitting = true;
            //}
            m_bHitting =true;
            Debug.WriteLine("m_bHitting set to true");
        }
        public void OnBallHit(Ball i, Ball j)
        {
            PlaySound(m_pathSound + "low_ball.wav",
                0, 1);
            if (m_bHitting)
            {
                m_hit = i.m_i == 0 ? j : i;
            }
        }
        public void OnBallsMoving()
        {
            Title = "moving...";
        }
        public void OnBallsStopped()
        {
            //m_hit = null;
            //m_bHitting = false;
            Title = "Stopped.";
        }

        private void myCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            foreach (Ball ball in balls)
            {
                //if (ball.is_placing)
                //{
                //    ball.is_placing = false;
                //    Mouse.Capture(null);
                //}
            }
        }

        private void menuSinuca_Click(object sender, RoutedEventArgs e)
        {
            menuSinuca.IsChecked = true;
            menuRandom.IsChecked = false;
            m_nextGame = GameType.GM_SINUCA;
        }

        private void menuRandom_Click(object sender, RoutedEventArgs e)
        {
            menuSinuca.IsChecked = false;
            menuRandom.IsChecked = true;
            m_nextGame = GameType.GM_RANDOM;
        }
    }
}



