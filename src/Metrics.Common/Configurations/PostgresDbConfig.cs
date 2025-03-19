using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Common.Configurations
{
    public class PostgresDbConfig
    {
        public string PgSchema { get; set; } = null!;
        public int PgPoolSize { get; set; } = 100;
    }

}

/**
        Host	                your_host	    The hostname or IP address of the PostgreSQL server.
        Port	                5432	        The port number on which the PostgreSQL server is listening (default is 5432).
        Database	            your_database	The name of the database to connect to.
        Username	            your_username	The username to connect to the database.
        Password	            your_password	The password for the specified username.
        Pooling	                true	        Enable connection pooling (default is true).
        MinPoolSize	            0	            Minimum number of connections in the pool.
        MaxPoolSize	            100	            Maximum number of connections in the pool.
        Timeout	                15	            Time in seconds to wait for a connection to open (default is 15 seconds).
        CommandTimeout	        30	            Time in seconds to wait for a command to execute (default is 30 seconds).
        SSL Mode	            Prefer	        SSL mode for the connection (options: Disable, Allow, Prefer, Require, VerifyCA, VerifyFull).
        Application Name	    your_app_name	Name of the application connecting to the database, useful for monitoring.
        Search Path	            public	        The schema search path to use for the connection.
        Keep Alive	            60	            Time in seconds to keep the connection alive (default is 60 seconds).
        Integrated Security     true or false	Use Windows Authentication (set to true for integrated security).
*/