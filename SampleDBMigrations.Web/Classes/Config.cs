using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SampleDBMigrations.Web
{
    public static class Config
    {
        /// <summary>
        /// Main DB SQL Connection String, read from the Web.config.
        /// </summary>
        public static string MainDBConnectionString
        {
            get
            {
                return System.Configuration.ConfigurationManager.ConnectionStrings["MainDB"].ConnectionString;
            }
        }
    }
}