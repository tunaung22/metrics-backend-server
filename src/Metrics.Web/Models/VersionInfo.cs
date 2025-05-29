using System;

namespace Metrics.Web.Models;

public class VersionInfo
{
    public Version? AssemblyVersion { get; set; }
    public string? FileVersion { get; set; }
    public string? InformationalVersion { get; set; }
}
