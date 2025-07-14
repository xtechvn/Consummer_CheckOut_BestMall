using APP_CHECKOUT.Utilities.Lib;
using System.Configuration;
using Telegram.Bot;

namespace APP.READ_MESSAGES.Libraries
{
    public  class LoggingService : ILoggingService
    {

        public string env = "Product";
        public string name = "APP_CHECKOUT";

        public LoggingService() {

            env = ConfigurationManager.AppSettings["BotSetting_environment"];
            name = ConfigurationManager.AppSettings["BotSetting_Name"];

        }

    
        public  void LoggingAppOutput( string message, bool file_out = false,bool tele_out=false)
        {
            string log = "[" + name + "] - [" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "]: " + message + ""; 
            if (file_out)
            {
                WriteAppLog(log);
            }
            if (tele_out)
            {
                InsertLogTelegramDirect(log);
            }
        }
        private  bool WriteAppLog(string log)
        {
            try
            {
                string app_path = Directory.GetCurrentDirectory().Replace(@"\bin\Debug\netcoreapp3.1", "") + @"\log";
                if (!Directory.Exists(app_path))
                {
                    Directory.CreateDirectory(app_path);
                }
                string file_name = "general" + DateTime.Now.ToString("yyMMdd") + ".list";
                string path_excuted = app_path + @"\" +file_name;
                if (!File.Exists(path_excuted))
                {
                    File.Create(path_excuted);
                }
                File.AppendAllText(path_excuted, log);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public int InsertLogTelegramDirect(string message)
        {
            var rs = 1;
            try
            {
                LogHelper.InsertLogTelegram(message);
            }
            catch (Exception)
            {
                rs = -1;
            }
            return rs;
        }
       
    }
}
