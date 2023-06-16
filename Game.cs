using System;


namespace BouncingBalls02
{
    public interface IGame
    {
        public void Init(GameType gameType);

    }
    public interface IGameEvents
    {
        public void OnBallInHole(Ball ball);
        public void OnBallOutHole(int i);
        public void OnEnd();
        public void OnScore(int i);
        public void OnChangePlayer();
        public void OnPlaceBall(int i);
        public void OnFault(int i);
        public void OnBallHitByCue(int i);
        public void OnBallHit(Ball i, Ball j);
        public void OnBallsMoving();
        public void OnBallsStopped();
    }
}
