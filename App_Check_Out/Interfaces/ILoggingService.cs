using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;

namespace APP.READ_MESSAGES.Libraries
{
    public  interface ILoggingService
    {
        public void LoggingAppOutput(string message, bool file_out = false, bool tele_out = false);
        public int InsertLogTelegramDirect(string message);
      
    }
}
