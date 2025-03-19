using System;
using System.Text.RegularExpressions;
namespace Metrics.Common.Utils;

public static partial class MetricsRegexPatternChecker
{
    [GeneratedRegex(@"^\d{4}-\d{2}$")]
    public static partial Regex KpiPeriodNameRegex();


    public static bool IsValidPeriodNameFormat(string value)
    {
        return KpiPeriodNameRegex().IsMatch(value);
    }
}
