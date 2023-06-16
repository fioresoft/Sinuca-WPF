using System;
using System.Numerics;
using System.Windows.Shapes;

namespace BouncingBalls02
{
    public class Hole
    {
        public Ellipse sprite;
        public Vector2 m_c;
        public static float m_r = 50;
        int hole;
        public static IGameEvents gameEvents = null;
        public Hole(Ellipse sprite, int hole)
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
                ball.m_gone = true;
                //gameEvents.OnBallInHole(ball.m_i);
            }
            return debug;
        }
    }

}
