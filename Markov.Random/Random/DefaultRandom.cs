namespace Markov.Random
{
    public class DefaultRandom : IRandom
    {
        public DefaultRandom() : this(new System.Random())
        {
        }

        public DefaultRandom(System.Random r)
        {
            _rng = r;
        }

        private readonly System.Random _rng;

        public int Random(int max)
        {
            return _rng.Next(max);
        }

        public int Random(int min, int max)
        {
            return _rng.Next(min, max);
        }

        public double Random()
        {
            return _rng.NextDouble();
        }
    }
}
