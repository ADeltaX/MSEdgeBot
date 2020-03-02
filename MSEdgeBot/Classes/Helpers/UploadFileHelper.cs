using System;
using System.Threading.Tasks;

namespace MSEdgeBot.Classes.Helpers
{
    public static class UploadFileHelper
    {
        static int numMaxTries = 6;
        public static async Task UploadFileAsync(string path, string captionHtml, int numTries = 0)
        {
            if (numTries >= numMaxTries)
                return;

            try
            {
                await TDLibHost.TDLibHostBot.UploadFileAndMessage(path, captionHtml);
            }
            catch (Exception)
            {
                try
                {
                    await Task.Delay(2000);

                    await TDLibHost.TDLibHostBot.Close();
                    
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("TDLIB PLEASE -> " + ex.Message);
                }

                try
                {
                    await TDLibHost.TDLibHostBot.CreateInstance();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("TDLIB PLEASE - PART 2 -> " + ex.Message);
                }

                await UploadFileAsync(path, captionHtml, ++numTries);
            }
        }
    }
}
