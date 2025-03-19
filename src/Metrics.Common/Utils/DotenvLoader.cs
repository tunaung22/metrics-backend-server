using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Common.Utils;

public static class DotenvLoader
{
    public static void Load(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        foreach (var i in File.ReadAllLines(path))
        {
            // env=development
            var part = i.Split('=', StringSplitOptions.RemoveEmptyEntries);

            if (part.Length == 2)
            {
                Environment.SetEnvironmentVariable(part[0], part[1]);
            }
        }
    }
}