using System;

namespace Metrics.Web.Models;

public class VersionInfo
{
    public Version? AssemblyVersion { get; set; }
    public string? FileVersion { get; set; }
    public string? InformationalVersion { get; set; }

    public string CleanInformationalVersion
    {
        get
        {
            if (string.IsNullOrEmpty(InformationalVersion))
                return InformationalVersion ?? "Unknown";

            // Handle pattern: "0.1.0-beta17+819e9bb.819e9bb687984ebad94e70dc94db877db4a87859"
            if (InformationalVersion.Contains('+') && InformationalVersion.Contains('.'))
            {
                var parts = InformationalVersion.Split('+');
                var versionPart = parts[0];
                var shaPart = parts[1];

                // If SHA part contains a dot, split and take only the part before the dot
                if (shaPart.Contains('.'))
                {
                    var shaParts = shaPart.Split('.');
                    return $"{versionPart}+{shaParts[0]}"; // Returns "0.1.0-beta17+819e9bb"
                }
            }

            return InformationalVersion;
        }
    }
}
