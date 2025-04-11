using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Shared.Configurations;

public class DatabaseSettings
{
    public PostgresDbConfig PostgresDbconfig { get; set; } = null!;
}
