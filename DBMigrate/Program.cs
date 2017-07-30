using DbUp;
using Microsoft.Extensions.CommandLineUtils;
using System;

namespace DBMigrate
{
    class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: false);
            app.Name = "DB Updater";
            app.FullName = "Database Updater application";
            app.Description = "Runs the databae migrations";
            app.ShortVersionGetter = () => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            app.LongVersionGetter = () => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            app.HelpOption("-? | -h | --help");

            CreateDatabaseUpdateCommand(app);

            app.OnExecute(() =>
            {
                Console.WriteLine("");
                Console.WriteLine("Oops, looks like you have not specified a command. Have some help. ;)");
                Console.WriteLine("");
                app.ShowHelp();

                return 1;
            });

            var result = app.Execute(args);

            return result;
        }

        #region Private Methods

        private static void CreateDatabaseUpdateCommand(CommandLineApplication app)
        {
            var command = app.Command("RunDBMigrate", config =>
            {
                config.Description = "Run the database migration";
                config.HelpOption("-? | -h | --help");

                CommandOption server = CreateServerOption(config);
                CommandOption database = CreateDatabaseOption(config);
                CommandOption username = CreateUsernameOption(config);
                CommandOption password = CreatePasswordOption(config);
                CommandOption trustedConnection = CreateTrustedConnectionOption(config);

                CommandOption wait = CreateWaitOption(config);

                CommandOption scriptFolder = CreateScriptFolderOption(config);

                config.OnExecute(() =>
                {
                    // TODO Validate args here.
                    Console.WriteLine("Database Migration started at {0:yyyy/MM/dd HH:mm:ss}", DateTime.Now);

                    Console.WriteLine(
                      "Command line parameters: Server: {0}, Database: {1}, Username: {2}, TrustedConnection: {3}, Wait: {4}",
                      server.Value(), database.Value(), username.Value(), trustedConnection.HasValue(), wait.HasValue());

                    var connectionString = GetConnectionString(server.Value(), database.Value(), username.Value(), password.Value(), trustedConnection.HasValue());
                    var returnValue = 0;

                    try
                    {
                        var scriptsFolder = scriptFolder.Value();

                        var upgradeBuilder = DeployChanges.To
                            .SqlDatabase(connectionString)
                            .WithScriptsFromFileSystem(scriptsFolder)
                            .WithTransactionPerScript();

                        upgradeBuilder.Configure(c => { c.ScriptExecutor.ExecutionTimeoutSeconds = 30 * 60; });

                        var upgrader = upgradeBuilder.LogToConsole()
                            .LogScriptOutput()
                            .Build();

                        var result = upgrader.PerformUpgrade();

                        if (!result.Successful)
                        {
                            returnValue = 1;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(result.Error);
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Success!");
                            Console.ResetColor();
                        }
                    }
                    catch (Exception ex)
                    {
                        returnValue = 1;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(ex);
                        Console.ResetColor();
                    }

                    Console.WriteLine("Database Migration completed at {0:yyyy/MM/dd HH:mm:ss}", DateTime.Now);

                    if (wait.HasValue())
                    {
                        Console.ReadLine();
                    }

                    return returnValue;
                });
            });
        }

        private static string GetConnectionString(string server, string database, string username, string password, bool useTrustedConnection)
        {
            var connectionString = string.Format("Data Source={0};Initial Catalog={1}; User ID={2};Password={3};Integrated Security=false;",
                           server, database, username, password);

            if (useTrustedConnection)
            {
                connectionString = string.Format("Data Source={0};Initial Catalog={1};Integrated Security=true;",
                    server, database);
            }

            return connectionString;
        }

        private static CommandOption CreateScriptFolderOption(CommandLineApplication config)
        {
            return config.Option(
                "-f |--folder",
                "The path to the Script folder",
                CommandOptionType.SingleValue);
        }

        private static CommandOption CreateWaitOption(CommandLineApplication config)
        {
            return config.Option(
                "-w |--wait",
                "True to wait for user input to close",
                CommandOptionType.NoValue);
        }

        #region Db Options

        private static CommandOption CreateTrustedConnectionOption(CommandLineApplication config)
        {
            return config.Option(
                "-t |--trusted",
                "True to use a trusted connection",
                CommandOptionType.NoValue);
        }

        private static CommandOption CreatePasswordOption(CommandLineApplication config)
        {
            return config.Option(
                "-p |--password <password>",
                "The password",
                CommandOptionType.SingleValue);
        }

        private static CommandOption CreateUsernameOption(CommandLineApplication config)
        {
            return config.Option(
                "-u |--username <username>",
                "The username",
                CommandOptionType.SingleValue);
        }

        private static CommandOption CreateDatabaseOption(CommandLineApplication config)
        {
            return config.Option(
                "-d |--database <database>",
                "The database name",
                CommandOptionType.SingleValue);
        }

        private static CommandOption CreateServerOption(CommandLineApplication config)
        {
            return config.Option(
                "-s |--server <server>",
                "The server name",
            CommandOptionType.SingleValue);
        }

        #endregion

        #endregion
    }
}
