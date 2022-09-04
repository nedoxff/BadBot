using System.Text.RegularExpressions;

namespace BadBot.Processor.Extensions;

public enum LinkType
{
    Invalid,
    Youtube,
    Other
}

public static class StringExtensions
{
    private const string YoutubeRegex =
        @"(?:https?:\/\/)?(?:www\.)?youtu(?:\.be\/|be.com\/\S*(?:watch|embed|shorts)(?:(?:(?=\/[-a-zA-Z0-9_]{11,}(?!\S))\/)|(?:\S*v=|v\/)))[-a-zA-Z0-9_]{11}";

    public static bool IsValidUrl(this string str)
    {
        return Uri.TryCreate(str, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    public static LinkType GetLinkType(this string str)
    {
        if (!IsValidUrl(str)) return LinkType.Invalid;
        if (Regex.IsMatch(str, YoutubeRegex)) return LinkType.Youtube;
        return LinkType.Other;
    }
}