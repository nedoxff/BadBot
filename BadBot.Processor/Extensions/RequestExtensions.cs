using System.Reflection;
using BadBot.Processor.Models.Modifiers;
using BadBot.Processor.Models.Requests;
using Serilog;

namespace BadBot.Processor.Extensions;

public static class RequestExtensions
{
    private static readonly Dictionary<string, Type> _modifiers = new();

    public static void RegisterModifiers()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes()
            .Where(x => x.BaseType is { IsGenericType: true } && x.BaseType.BaseType == typeof(Modifier));
        foreach (var type in types)
            _modifiers[type.Name.Replace("Modifier", "")] = type;
        Log.Information("Registered {ModifierCount} modifiers: {Modifiers}", _modifiers.Count,
            string.Join(", ", _modifiers.Keys));
    }

    public static Modifier ToModifier(this Request request)
    {
        if (!_modifiers.ContainsKey(request.ModifierType))
            throw new Exception("Cannot convert the request to a modifier (is the ModifierType invalid?)");
        var obj = Activator.CreateInstance(_modifiers[request.ModifierType])!.To<Modifier>();
        obj!.RawRequest = request;
        obj!.Id = request.Id;
        return obj;
    }
}