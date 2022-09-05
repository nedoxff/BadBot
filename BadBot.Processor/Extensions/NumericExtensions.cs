namespace BadBot.Processor.Extensions;

public static class NumericExtensions
{
    public static double Map(this double input, double inputStart, double inputEnd, double outputStart,
        double outputEnd)
    {
        return outputStart + (outputEnd - outputStart) / (inputEnd - inputStart) * (input - inputStart);
    }
}