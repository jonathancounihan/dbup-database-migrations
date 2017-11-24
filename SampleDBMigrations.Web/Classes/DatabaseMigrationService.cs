using DbUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SampleDBMigrations.Web
{
    public class DatabaseMigrationService
    {
        #region Public Methods

        public string ExecuteMigration(string pathScriptFolder)
        {
            var logger = new Log();

            DbUp.Engine.UpgradeEngine upgrader = GetDbUpgrader(pathScriptFolder, logger);

            var result = upgrader.PerformUpgrade();

            return logger.GetLog();
        }

        public bool CheckForIsUpgradeRequired(string pathScriptFolder)
        {
            var logger = new Log();
            return GetDbUpgrader(pathScriptFolder, logger).IsUpgradeRequired();
        }

        #endregion

        private DbUp.Engine.UpgradeEngine GetDbUpgrader(string pathScriptFolder, Log logger)
        {
            var connectionString = Config.MainDBConnectionString;

            var upgrader = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsFromFileSystem(pathScriptFolder)
                .WithTransactionPerScript()
                .LogTo(logger)
                .LogScriptOutput()
                .Build();

            return upgrader;
        }
    }
}