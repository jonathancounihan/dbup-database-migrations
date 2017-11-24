using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;

namespace SampleDBMigrations.Web
{
    public class MaintenanceModeController : Controller
    {
        private const string relativeFolderPath = @"bin\SqlScripts\MainDB\";

        private DatabaseMigrationService DatabaseMigrationService = new DatabaseMigrationService();

        #region Public Methods

        public ActionResult UpdateDb(string UpgradeCode)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent("Not authorized");

            if (HttpContext.Request.IsLocal || UpgradeCode == "ThisIsASecretCodeWhichIsUsuallyInTheWeb.config")
            {
                var scriptFolderPath = HttpRuntime.AppDomainAppPath + relativeFolderPath;
                if (System.IO.Directory.Exists(scriptFolderPath))
                {
                    var watch = new Stopwatch();

                    watch.Start();
                    var result = DatabaseMigrationService.ExecuteMigration(scriptFolderPath);
                    HttpContext.Application[UIConstants.InMaintenaceMode] = DatabaseMigrationService.CheckForIsUpgradeRequired(scriptFolderPath);
                    watch.Stop();

                    var parsedView = result.Replace("\r\n", "<br/>");
                    parsedView += "<br/> Total Run Time: " + watch.Elapsed.Seconds + " seconds";

                    return Content(parsedView, "text/html");
                }

                return RedirectToAction("ServerError", "Error", new { errorMessage = "Cannot find database migration scripts folder!" });
            }

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

            return Content("You are not authorized to execute this method", "text/html");
        }

        // GET: MaintenanceMode
        public ActionResult WithFeedback()
        {
            var inMaintenaceMode = (bool)HttpContext.Application[UIConstants.InMaintenaceMode];

            if (inMaintenaceMode)
            {
                var scriptFolderPath = HttpRuntime.AppDomainAppPath + relativeFolderPath;
                if (System.IO.Directory.Exists(scriptFolderPath))
                {
                    inMaintenaceMode = DatabaseMigrationService.CheckForIsUpgradeRequired(scriptFolderPath);

                    HttpContext.Application[UIConstants.InMaintenaceMode] = inMaintenaceMode;
                }
            }

            if (!inMaintenaceMode)
            {
                return RedirectToAction("Index", "Home");
            }

            if (HttpContext.Request.IsLocal)
            {
                return View("MaintenanceWithFeedback");
            }
            else
            {
                return View("MaintenanceSiteOffline");
            }
        }

        #endregion Public Methods
    }
}