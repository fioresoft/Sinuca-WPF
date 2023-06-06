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

namespace BouncingBalls02
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly int m_cBalls = 8;
        public static Ball[]? balls = new Ball[m_cBalls];
        private static DispatcherTimer timer = new DispatcherTimer();
        static Random rnd = new Random();
        public static Hole[]? holes = new Hole[6];

        public MainWindow()
        {
            InitializeComponent();

            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            rnd = new Random();
            Ball.canvas = myCanvas;
            Ball.progress = myProgress;
            Ball.label = myLabel;
            Ball.toolBar = myToolBar;
            Ball.m_bt8 = ball8;
        }
        private void myWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
            InitBallsRandom(m_cBalls);
            InitHoles();
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
            if(balls != null)
            {
                foreach(Ball b in balls)
                {
                    if(b != null && b.sprite != null)
                        myCanvas.Children.Remove(b.sprite);
                }
            }
            if(balls == null)
            {
                balls = new Ball[count];
            }
            for (int i = 0; i < count; i++)
            {
                balls[i] = new Ball((i + 1) * rnd.Next(100), (i + 1) * rnd.Next(100),
                    (float)rnd.Next(200), (float)rnd.Next(200), i);
            }
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
                        if (balls[i].is_shock(balls[j]))
                        {
                            balls[i].do_shock(balls[j]);
                        }
                    }
                }
                balls[i].Move(0.1f);
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
            InitBallsRandom(8);
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
            InitHoles();
            InitBallsRandom(8);
        }

        private void myWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Thread.CurrentThread.IsBackground = true;
            timer.Stop();
        }
    }
}

public class Ball
{
    public Ellipse sprite;
    static public Canvas canvas;
    static public ProgressBar progress;
    static public Label label;
    static public ToolBar toolBar;
    static public Button m_bt8;
    public Vector2 m_p;                 // centre of the circle/sprite
    public Vector2 m_v;
    public static float m_radius = 15;
    int i;
    public bool m_gone = false;
    static int CUE_LEN = 500;
    public bool m_has_cue = false;
    public static int m_cue_angle = 0;
    private static Line m_cue;
    public static int m_table_friction = 2; // %
    public static int m_wall_friction = 5; // %
    private float m_cue_speed = 0;
    private DateTime m_RightMouseButtonDownTime;

    // from center (m_p) to top-left of sprite
    public Vector2 c_to_tl()
    {
        Vector2 p = new Vector2();
        p.X = m_p.X - (float)sprite.Width / 2;
        p.Y = m_p.Y - (float)sprite.Height / 2;
        return p;
    }
    // from top-left of sprite to centre of sprite
    //public Vector2 tl_to_c()
    //{
    //    Vector2 p = new Vector2();
    //    p = m_p;
    //    p.X += (float)sprite.Width / 2;
    //    p.Y += (float)sprite.Height / 2;
    //    return p;
    //}
    /// <summary>
    // projection of p in the direction of u
    /// </summary>
    /// <param name="p"></param>
    /// <param name="u"></param>
    /// <returns></returns>
    static Vector2 proj(Vector2 p, Vector2 u) => ((Vector2.Dot(p, u) / Vector2.Dot(u, u))) * u;

    public Ball()
    {

    }

    public Ball(float x, float y, float vx, float vy, int i)
    {
        this.m_p.X = x;
        this.m_p.Y = y;
        this.m_v.X = vx;
        this.m_v.Y = vy;
        sprite = new Ellipse();
        sprite.Width = m_radius * 2;
        sprite.Height = m_radius * 2;
        sprite.MouseLeftButtonDown += Sprite_MouseDown;
        sprite.MouseMove += Sprite_MouseMove;
        Random r = new Random();
        sprite.Fill = new SolidColorBrush(MainWindow.MakeRandomColor());
        canvas.Children.Add(sprite);
        sprite.SetValue(Canvas.LeftProperty, (double)(c_to_tl().X));
        sprite.SetValue(Canvas.TopProperty, (double)(c_to_tl().Y));
        this.i = i;
    }

    private void Sprite_MouseMove(object sender, MouseEventArgs e)
    {
           Vector2 r = new Vector2();
            Point ptMouse;
            ptMouse = e.GetPosition(canvas);
        foreach (Ball ball in MainWindow.balls)
        {
            if (ball.m_has_cue)
            {
                Vector2 vector = new Vector2((float)ptMouse.X - m_p.X, (float)ptMouse.Y - m_p.Y);
                vector *= 5;
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    progress.Value = vector.Length();
                    //m_cue_speed = vector.Length();
                    if(Mouse.Captured == null)
                    {
                        Mouse.Capture(ball.sprite);
                    }
                    label.Content = progress.Value;
                    break;
                }
                if (e.RightButton == MouseButtonState.Released)
                {
                    progress.Value = 0;
                    m_cue_speed = vector.Length();
                    if (Mouse.Captured == ball.sprite)
                        Mouse.Capture(null);
                    else
                        break;
                    Vector2 cue = new Vector2();
                    cue.X = (float)(m_cue.X2 - m_cue.X1);
                    cue.Y = (float)(m_cue.Y2 - m_cue.Y1);
                    cue /= cue.Length();
                    cue *= m_cue_speed;
                    ball.m_v = cue;
                    ball.m_has_cue = false;
                    canvas.Children.Remove(m_cue);
                    Ball.m_cue_angle = 0;
                    label.Content = 0;
                    break;
                }
            }
        }
    }

    private void Sprite_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if(sender is Ellipse)
        {
            for(int i =0; i < MainWindow.balls.Length; i++)
            {
                if (MainWindow.balls[i].is_point_over(e.GetPosition(canvas)))
                {
                    Ball ball = MainWindow.balls[i];
                    ball.m_has_cue = true;
                    ball.draw_cue();
                }
                else
                    MainWindow.balls[i].m_has_cue=false;
            }
        }
    }

    bool is_point_over(Point point)
    { 
        float d  = (float)Math.Sqrt(Math.Pow(m_p.X - point.X, 2) +  Math.Pow(m_p.Y - point.Y, 2));
        return d <= m_radius;
    }

    public void Move(float dt)
    {
        bool bounced = false;
        m_v *= (float)((100 - m_table_friction) / 100.0);
        if (m_v.Length() < m_radius)
            m_v *= 0;
        m_p += m_v * dt;
        
        for(int hole = 0; hole < MainWindow.holes.Length; hole++)
        {
            if (MainWindow.holes[hole].is_in_hole(this))
            {
                m_gone = true;
                canvas.Children.Remove(sprite);
                if(canvas.Children.Count == 0)
                {
                    m_bt8.IsEnabled = true;
                }
            }
        }

        if (!m_gone)
        {
            if (bounced = bounce())
            {
                m_v *= (float)((100 - m_wall_friction) / 100.0);
                if (m_v.Length() < m_radius)
                    m_v *= 0;
            }
            sprite.SetValue(Canvas.LeftProperty, (double)c_to_tl().X);
            sprite.SetValue(Canvas.TopProperty, (double)c_to_tl().Y);
        }
        DockPanel wnd = canvas.Parent as DockPanel;
        MainWindow mainWindow = wnd.Parent as MainWindow;
        if (!MainWindow.balls_stopped())
        { 
            mainWindow.Title = "Moving...";
        }
        else
        {
            mainWindow.Title = "Stopped.";
        }
        
    }
    bool bounce()
    {
        bool bounced = false;

        if (m_p.X <= m_radius)
        {
            m_p.X = m_radius;
            m_v.X *= -1;
            bounced = true;
        }
        else if (m_p.X >= canvas.Width - m_radius)
        {
            m_p.X = (float)(canvas.Width - m_radius);
            m_v.X *= -1;
            bounced = true;
        }
        if (m_p.Y <= m_radius)
        {
            m_p.Y = m_radius;
            m_v.Y *= -1;
            bounced = true;
        }
        if (m_p.Y >= canvas.Height - m_radius)
        {
            m_p.Y = (float)(canvas.Height - m_radius);
            m_v.Y *= -1;
            bounced = true;
        }
        return bounced;
    }
    public bool is_shock(Ball other)
    {
        double SumOfRadiiSquared = (m_radius + Ball.m_radius) * (m_radius + Ball.m_radius);
        double d_squared = (Math.Pow((m_p.X - other.m_p.X), 2) + Math.Pow((m_p.Y - other.m_p.Y), 2));
        if (d_squared < SumOfRadiiSquared)
        {
            this.sprite.Fill = new SolidColorBrush(MainWindow.MakeRandomColor());
            other.sprite.Fill = new SolidColorBrush(MainWindow.MakeRandomColor());
            Console.WriteLine($"SHOCK: {i}-{other.i}");
            return true;
        }

        return false;

    }
    /*
     * Vector r = b2.m_p - m_p;
	Vector v1r,v1n;
	Vector v2r,v2n;
	Vector tmp;

	v1r = m_v.proj(r);
	v1n = m_v - v1r;
	v2r = b2.m_v.proj(r);
	v2n = b2.m_v - v2r;
	tmp = v1r;
	v1r = v2r;
	v2r = tmp;

	if (!b2.m_ghost) {
		m_v = (v1r + v1n);// *m_ball_elasticity;
		b2.m_v = (v2r + v2n);// *m_ball_elasticity;

		//////
		if (r.modulo() < m_r + b2.m_r) {
			Move(350);
		}
		//////
	}
    */
    public void do_shock(Ball other)
    {
        Vector2 r = other.m_p - m_p;
        Vector2 v1r, v1n;
        Vector2 v2r, v2n;
        Vector2 tmp;

        v1r = proj(m_v, r);
        v1n = m_v - v1r;
        v2r = proj(other.m_v, r);
        v2n = other.m_v - v2r;
        tmp = v1r;
        v1r = v2r;
        v2r = tmp;

        m_v = (v1r + v1n);
        other.m_v = (v2r + v2n);

    }

    public bool is_overlapping(Ball other)
    {
        double SumOfRadiiSquared = (m_radius + Ball.m_radius) * (m_radius + Ball.m_radius);
        double d_squared = (Math.Pow((m_p.X - other.m_p.X), 2) + Math.Pow((m_p.Y - other.m_p.Y), 2));
        if (d_squared < SumOfRadiiSquared)
        {
            Console.WriteLine($"OVERLAPPING: {i}-{other.i}");
            return true;
        }
        return false;
    }

    public void draw_cue()
    {
        Point p1 = new Point();
        Point p2 = new Point();

        double angle_rad = (m_cue_angle * Math.PI) / 180;
        p1.X = m_p.X + m_radius * Math.Cos(angle_rad);
        p1.Y = m_p.Y - m_radius * Math.Sin(angle_rad);
        p2.X = m_p.X + (m_radius + CUE_LEN) * Math.Cos(angle_rad);
        p2.Y = m_p.Y - (m_radius + CUE_LEN) * Math.Sin(angle_rad);

        Line line = new Line
        {
            X1 = p1.X,
            Y1 = p1.Y,
            X2 = p2.X,
            Y2 = p2.Y,
            Stroke = Brushes.Black,
        };
        if(m_cue != null)
            canvas.Children.Remove(m_cue);
        m_cue = line;
        canvas.Children.Add(line);
    }
}

public class Hole
{
    public Ellipse sprite;
    public Vector2 m_c;
    public static float m_r = 50;
    int hole;
    public Hole(Ellipse sprite,int hole)
    {
        this.sprite = sprite;
        this.hole = hole;
    }
    public bool is_in_hole(Ball ball)
    {
        /*
         *  Vector r = b.m_p - m_c;
	        LONG d = (LONG)r.modulo();
	        return d < m_r - b.m_r;
        */
    Vector2 r = ball.m_p - m_c;
        float d = r.Length();
        bool debug = d < m_r - Ball.m_radius;
        if (debug)
        {
            debug = true;
        }
        return debug;
    }
}

