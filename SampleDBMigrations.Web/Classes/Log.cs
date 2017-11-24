using DbUp.Engine.Output;
using System.Text;

namespace SampleDBMigrations.Web
{
    public class Log : IUpgradeLog
    {
        private StringBuilder sb = new StringBuilder();

        public void WriteError(string format, params object[] args)
        {
            sb.AppendLine(string.Format(format, args));
        }

        public void WriteInformation(string format, params object[] args)
        {
            sb.AppendLine(string.Format(format, args));
        }

        public void WriteWarning(string format, params object[] args)
        {
            sb.AppendLine(string.Format(format, args));
        }

        public string GetLog()
        {
            var logtext = sb.ToString();
            return logtext;
        }
    }
}