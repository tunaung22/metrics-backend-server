﻿using System.Text.RegularExpressions;

namespace Metrics.Shared.Utils;

public static class StringUtils
{
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) { return input; }

        var startUnderscores = Regex.Match(input, @"^_+");

        return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
    }

}
