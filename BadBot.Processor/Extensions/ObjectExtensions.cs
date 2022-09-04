namespace BadBot.Processor.Extensions;

public static class ObjectExtensions
{
    public static T To<T>(this object obj) where T : class
    {
        return obj as T;
    }
}