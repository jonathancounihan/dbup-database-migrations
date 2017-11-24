using DbUp;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace SampleDBMigrations.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            CheckIfDbMaintenanceIsRequiredAndSetGlobal();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var context = new HttpContextWrapper(Context);

            if ((bool)Application[UIConstants.InMaintenaceMode])
            {
                if (!HttpContext.Current.Request.Path.Contains("MaintenanceMode"))
                {
                    HttpContext.Current.RewritePath("MaintenanceMode/WithFeedback");
                }
            }
        }

        private void CheckIfDbMaintenanceIsRequiredAndSetGlobal()
        {
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MainDB"].ConnectionString;
            var scriptFolderPath = HttpRuntime.AppDomainAppPath + @"bin\SqlScripts\MainDB\";

            if (System.IO.Directory.Exists(scriptFolderPath))
            {
                var upgrader = DeployChanges.To
                    .SqlDatabase(connectionString).LogScriptOutput()
                    .WithScriptsFromFileSystem(scriptFolderPath)
                    .Build();

                var isUpgradeRequired = upgrader.IsUpgradeRequired();

                Application[UIConstants.InMaintenaceMode] = isUpgradeRequired;
            }
            else
            {
                Application[UIConstants.InMaintenaceMode] = true;
            }
        }
    }
}