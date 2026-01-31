public static class SharedRandom
{
    static readonly System.Random random = new();

    /// <summary>
    /// Returns a non-negative random integer that is less than the specified maximum.
    /// </summary>
    /// <param name="maxValue">The exclusive upper bound of the random number to be generated. Must be greater than or equal to 0.</param>
    /// <returns>
    /// A 32-bit signed integer that is greater than or equal to 0, and less than maxValue; that is, the range of return values ordinarily includes 0 but not maxValue.
    /// However, if maxValue equals 0, 0 is returned.
    /// </returns>
    public static int Next(int maxValue)
    {
        return random.Next(maxValue);
    }
}
