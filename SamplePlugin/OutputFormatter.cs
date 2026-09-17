using System;
using Dalamud.Game.Text.SeStringHandling;

namespace WhoPulled;

internal static class OutputFormatter
{
    internal static SeString Format(Configuration configuration, string player, string target, string rank)
    {
        var template = configuration.OutputFormat
            .Replace("{Player}", player, StringComparison.Ordinal)
            .Replace("{Target}", target, StringComparison.Ordinal)
            .Replace("{Rank}", rank, StringComparison.Ordinal);
        var builder = new SeStringBuilder();

        if (configuration.DefaultColorKey != 0)
        {
            builder.AddUiForeground(configuration.DefaultColorKey);
        }

        var position = 0;
        while (position < template.Length)
        {
            var colorStart = template.IndexOf("{color:", position, StringComparison.Ordinal);
            var colorEnd = template.IndexOf("{/color}", position, StringComparison.Ordinal);
            var nextToken = colorStart < 0 ? colorEnd : colorEnd < 0 ? colorStart : Math.Min(colorStart, colorEnd);

            if (nextToken < 0)
            {
                builder.AddText(template[position..]);
                break;
            }

            if (nextToken > position)
            {
                builder.AddText(template[position..nextToken]);
            }

            if (nextToken == colorEnd)
            {
                builder.AddUiForegroundOff();
                position = nextToken + "{/color}".Length;
                continue;
            }

            var valueStart = nextToken + "{color:".Length;
            var valueEnd = template.IndexOf('}', valueStart);
            if (valueEnd < 0 || !ushort.TryParse(template[valueStart..valueEnd], out var colorKey))
            {
                builder.AddText(template[nextToken..(valueStart)]);
                position = valueStart;
                continue;
            }

            builder.AddUiForeground(colorKey);
            position = valueEnd + 1;
        }

        if (configuration.DefaultColorKey != 0)
        {
            builder.AddUiForegroundOff();
        }

        return builder.BuiltString;
    }
}
