using MSEdgeBot.Classes.Automation;
using MSEdgeBot.DataStore;
using System;
using System.Threading.Tasks;

namespace MSEdgeBot
{
    class Program
    {
        public static DBEngine Db;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Freshy EdgeUpdate Bot!");
            Console.WriteLine("Release version: " + TelegramBotSettings.BOT_VERSION);

            Console.WriteLine("\nInitializing Directories...");
            InitializeDirectory();

            Console.WriteLine("\nInitializing Database...");
            Db = new DBEngine();

            Console.WriteLine("\nInitializing TDLIB engine...");
            TDLibHost tdHost = new TDLibHost();
            Console.WriteLine("\nTDLIB engine ready!");
			
            Task.Factory.StartNew(o => SearchAutomation.PreExecute(), null, TaskCreationOptions.LongRunning);

            string cmd = "";

            do
            {
                cmd = Console.ReadKey().KeyChar.ToString().ToLower();
            } while (cmd != "q");
        }

        private static void InitializeDirectory()
        {
            foreach (var dir in TelegramBotSettings.DIRS_TO_INITALIZE)
            {
                try
                {
                    if (!System.IO.Directory.Exists(dir))
                        System.IO.Directory.CreateDirectory(dir);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[INITIALIZATION ERROR] {ex.Message}");
                    throw ex;
                }
            }
        }
    }
}
