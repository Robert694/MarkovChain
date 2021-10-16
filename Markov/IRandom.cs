namespace Markov
{
    public interface IRandom
    {
        int Random(int max);
        int Random(int min, int max);
        double Random();
    }
}