public static class SharedRandom
{
    static readonly System.Random random = new();

    public static int Next(int maxValue)
    {
        return random.Next(maxValue);
    }
}
