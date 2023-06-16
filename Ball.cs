using System;
using System.Diagnostics;
using System.Windows;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BouncingBalls02
{
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
        public int m_i;
        public bool m_gone = false;
        static int CUE_LEN = 500;
        public bool m_has_cue = false;
        public static int m_cue_angle = 0;
        private static Line m_cue;
        public static int m_table_friction = 2; // %
        public static int m_wall_friction = 5; // %
        private float m_cue_speed = 0;
        private DateTime m_RightMouseButtonDownTime;
        public bool is_placing { get; set; } = false;
        public static IGameEvents gameEvents = null;
        public bool m_bInHole = false;


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

        public Ball(Color color, int i)
        {
            sprite = new Ellipse();
            sprite.Width = m_radius * 2;
            sprite.Height = m_radius * 2;
            sprite.MouseLeftButtonDown += Sprite_MouseDown;
            sprite.MouseMove += Sprite_MouseMove;
            sprite.MouseLeftButtonUp += Sprite_LeftMouseUp;
            sprite.MouseRightButtonUp += Sprite_MouseRightButtonUp;
            sprite.Fill = new SolidColorBrush(color);
            canvas.Children.Add(sprite);
            sprite.SetValue(Canvas.LeftProperty, (double)(c_to_tl().X));
            sprite.SetValue(Canvas.TopProperty, (double)(c_to_tl().Y));
            this.m_i = i;
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
            sprite.MouseLeftButtonUp += Sprite_LeftMouseUp;
            sprite.MouseRightButtonUp += Sprite_MouseRightButtonUp;
            Random r = new Random();
            sprite.Fill = new SolidColorBrush(MainWindow.MakeRandomColor());
            canvas.Children.Add(sprite);
            sprite.SetValue(Canvas.LeftProperty, (double)(c_to_tl().X));
            sprite.SetValue(Canvas.TopProperty, (double)(c_to_tl().Y));
            this.m_i = i;
        }

        private void Sprite_MouseMove(object sender, MouseEventArgs e)
        {
            Vector2 r = new Vector2();
            Point ptMouse;
            ptMouse = e.GetPosition(canvas);
            foreach (Ball ball in MainWindow.balls)
            {
                if (ball.is_placing)
                {
                    ball.m_p.X = (float)ptMouse.X;
                    ball.m_p.Y = (float)ptMouse.Y;

                    break;
                }
                if (ball.m_has_cue)
                {
                    Vector2 vector = new Vector2((float)ptMouse.X - m_p.X, (float)ptMouse.Y - m_p.Y);
                    vector *= 5;
                    if (e.RightButton == MouseButtonState.Pressed)
                    {
                        progress.Value = vector.Length();
                        //m_cue_speed = vector.Length();
                        if (Mouse.Captured == null)
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

                        if (m_i == 0)
                        {
                            gameEvents.OnBallHitByCue(0);
                        }
                        break;
                    }
                }
            }
        }

        private void Sprite_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Ellipse)
            {
                for (int i = 0; i < MainWindow.balls.Length; i++)
                {
                    if (MainWindow.balls[i].is_placing)
                    {
                        Point ptMouse = e.GetPosition(canvas);
                        //MainWindow.balls[i].is_placing = false;
                        //Mouse.Capture(null);
                        MainWindow.balls[i].m_p.X = (float)ptMouse.X;
                        MainWindow.balls[i].m_p.Y = (float)ptMouse.Y;
                        break;
                    }
                    if (MainWindow.balls[i].is_point_over(e.GetPosition(canvas)))
                    {
                        Ball ball = MainWindow.balls[i];
                        ball.m_has_cue = true;
                        ball.draw_cue();
                    }
                    else
                        MainWindow.balls[i].m_has_cue = false;
                }
            }
        }

        private void Sprite_LeftMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (is_placing)
            {
                Mouse.Capture(null);
                is_placing = false;
            }
        }

        private void Sprite_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            //m_bHitting = true;
        }

        bool is_point_over(Point point)
        {
            float d = (float)Math.Sqrt(Math.Pow(m_p.X - point.X, 2) + Math.Pow(m_p.Y - point.Y, 2));
            return d <= m_radius;
        }

        public void Move(float dt)
        {
            bool bounced = false;
            m_v *= (float)((100 - m_table_friction) / 100.0);
            if (m_v.Length() < m_radius)
                m_v *= 0;
            m_p += m_v * dt;

            bool bInHole = false;
            for (int hole = 0; !m_gone && hole < MainWindow.holes.Length; hole++)
            {
                if (MainWindow.holes[hole].is_in_hole(this))
                {
                    m_gone = true;
                    bInHole = true;
                    canvas.Children.Remove(sprite);
                    if (canvas.Children.Count == 0)
                    {
                        m_bt8.IsEnabled = true;
                    }
                    break;
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
            if (!MainWindow.balls_stopped())
            {
                gameEvents.OnBallsMoving();
            }
            else
            {
                if ((m_gone && !m_bInHole) /*&& MainWindow.m_bHitting != false*/)
                {
                    gameEvents.OnBallInHole(this);
                    //MainWindow.m_hit = null;
                    //MainWindow.m_bHitting = false;
                    //bInHole = false;
                }
                //else if (bInHole)
                //{
                //    gameEvents.OnFault(m_i);
                //}
                else if (!bInHole && MainWindow.m_bHitting)
                {
                    gameEvents.OnBallOutHole(m_i);
                }
                gameEvents.OnBallsStopped();
                bInHole = false;
                //m_bHit = false;
                //m_bHitting = false; 
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
                //this.sprite.Fill = new SolidColorBrush(MainWindow.MakeRandomColor());
                //other.sprite.Fill = new SolidColorBrush(MainWindow.MakeRandomColor());
                Debug.Write($"SHOCK: {m_i}-{other.m_i} ");
                gameEvents.OnBallHit(this, other);

                //else if (m_bHitting)
                //{
                //    m_bHitting = false;
                //    m_bHit = false;
                //}
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
                Debug.WriteLine($"OVERLAPPING: {m_i}-{other.m_i}");
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
            if (m_cue != null)
                canvas.Children.Remove(m_cue);
            m_cue = line;
            canvas.Children.Add(line);
        }
    }

}
