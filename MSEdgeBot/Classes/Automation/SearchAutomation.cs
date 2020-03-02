using EdgeUpdateAPI;
using EdgeUpdateAPI.Classes;
using MSEdgeBot.Classes.Helpers;
using MSEdgeBot.DataStore;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Timers;

namespace MSEdgeBot.Classes.Automation
{
    public static class SearchAutomation
    {
        private const string messageEdgeUpdate = @"📣 New update in <b>{0}</b>
📜 Version <b>{1}</b>

SHA1 <code>{2}</code>";

        public static bool IsRunning = false;

        //oh c'mon, if it fails more than 11 times (and we are online) == someone's server sucks (or .NET sucks, but I doubt it)
        private const int _numMaxDLAttempts = 12;
        private static Timer _timerUpdates;

        private static readonly EdgeUpdate _edgeUpdate = new EdgeUpdate();
        private static readonly string DownloadFolder = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "dw";

        private static async Task SearchRun(string name, string ringName)
        {
            var res = await _edgeUpdate.GetLatestFiles(name);
            if (res.Success && !SharedDBcmd.UpdateExists(ringName, res.Value.Version))
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"[{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}] !!! New update !!! {ringName} - Version: {res.Value.Version}");
                Console.ResetColor();

                var edgeFileFull = res.Value.EdgeFile.Where(file => file.EdgeFileUpdateType == EdgeFileUpdateType.Full).First();
                var SHA1Hex = BitConverter.ToString(edgeFileFull.Sha1).Replace("-", "");
                var SHA256Hex = BitConverter.ToString(edgeFileFull.Sha256).Replace("-", "");

                SharedDBcmd.AddNewUpdate(ring: ringName, version: res.Value.Version,
                                                filename: edgeFileFull.FileName, filesize: edgeFileFull.Size,
                                                sha256: SHA256Hex, url: edgeFileFull.Url);

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"[{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}] <<< Downloading from MSEdge fe3 server");
                Console.ResetColor();
                await Download(name, res.Value.Version, edgeFileFull);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}] >>> Uploading to Telegram via TDLib");
                Console.ResetColor();
                await UploadAndSendMessage(edgeFileFull, string.Format(messageEdgeUpdate, ringName, res.Value.Version, SHA1Hex));

            }
        }

        private static async Task UploadAndSendMessage(EdgeFile fileElement, string captionHtml)
        {
            string file = Path.Combine(DownloadFolder, fileElement.FileName);

            if (!File.Exists(file))
            {
                SharedDBcmd.TraceError($"Internal error: File is not downloaded => " + file);
                Console.WriteLine("File is not downloaded => " + file);
                return;
            }

            await UploadFileHelper.UploadFileAsync(file, captionHtml);

        }

        private static async Task Download(string name, string version, EdgeFile fileElement, int numTries = 0)
        {
            if (numTries >= _numMaxDLAttempts)
                return;

            try
            {
                string filename = Path.Combine(DownloadFolder, fileElement.FileName);

                if (File.Exists(filename))
                {
                    byte[] fileHash = GetSHA1HashFile(filename);

                    //We already have this update, no need to download it again
                    if (fileHash.SequenceEqual(fileElement.Sha1))
                    {
                        Console.WriteLine($"[{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}] --- The file is already downloaded and valid, using this...");
                        return;
                    }
                    else
                    {
                        File.Delete(filename);
                    }
                }

                using (WebClient wb = new WebClient())
                {
                    await wb.DownloadFileTaskAsync(fileElement.Url, filename);
                    byte[] fileHash = GetSHA1HashFile(filename);

                    if (!fileHash.SequenceEqual(fileElement.Sha1))
                    {
                        if (File.Exists(filename))
                            File.Delete(filename);

                        throw new DataMisalignedException("Rip SHA1 hash");
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMessageIter = ex.Message;

                var currException = ex;

                while ((currException = currException.InnerException) != null)
                    errorMessageIter += "\n\nINNER EXCEPTION: " + currException.Message;

                SharedDBcmd.TraceError($"Internal error: {errorMessageIter}");

                Console.WriteLine("[ERROR] Failed downloading... retrying...");

                //Generate a new link.... just in case
                var res = await _edgeUpdate.GetFiles(name, version);

                if (res.Success)
                {
                    var edgeFileFull = res.Value.EdgeFile.Where(file => file.EdgeFileUpdateType == EdgeFileUpdateType.Full).First();
                    await Download(name, version, edgeFileFull, ++numTries);
                }
                else
                {
                    await Download(name, version, fileElement, ++numTries);
                }
            }
        }

        public static byte[] GetSHA1HashFile(string fileLocation)
        {
            try
            {
                using (FileStream fs = new FileStream(fileLocation, FileMode.Open, FileAccess.Read))
                using (var cryptoProvider = new SHA1CryptoServiceProvider())
                    return cryptoProvider.ComputeHash(fs);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool Execute()
        {
            if (IsRunning)
                return false;

            try
            {
                Console.WriteLine($"[{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}] Doing searches.");

                IsRunning = true;

                SearchRun(EdgeUpdate.Canary_Win_x86, "Canary (x86)").GetAwaiter().GetResult();
                SearchRun(EdgeUpdate.Canary_Win_x64, "Canary (x64)").GetAwaiter().GetResult();
                SearchRun(EdgeUpdate.Canary_Win_arm64, "Canary (ARM64)").GetAwaiter().GetResult();

                SearchRun(EdgeUpdate.Dev_Win_x86, "Dev (x86)").GetAwaiter().GetResult();
                SearchRun(EdgeUpdate.Dev_Win_x64, "Dev (x64)").GetAwaiter().GetResult();
                SearchRun(EdgeUpdate.Dev_Win_arm64, "Dev (ARM64)").GetAwaiter().GetResult();

                SearchRun(EdgeUpdate.Beta_Win_x86, "Beta (x86)").GetAwaiter().GetResult();
                SearchRun(EdgeUpdate.Beta_Win_x64, "Beta (x64)").GetAwaiter().GetResult();
                SearchRun(EdgeUpdate.Beta_Win_arm64, "Beta (ARM64)").GetAwaiter().GetResult();

                SearchRun(EdgeUpdate.Stable_Win_x86, "Stable (x86)").GetAwaiter().GetResult();
                SearchRun(EdgeUpdate.Stable_Win_x64, "Stable (x64)").GetAwaiter().GetResult();
                SearchRun(EdgeUpdate.Stable_Win_arm64, "Stable (ARM64)").GetAwaiter().GetResult();
                
                IsRunning = false;

                Console.WriteLine($"[{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}] Finished doing searches.");
                return true;
            }
            catch (Exception ex)
            {
                //RIP
                Console.WriteLine($"[Error] {ex.Message}");
                SharedDBcmd.TraceError($"Internal error: {ex.Message}");
                IsRunning = false;
                return false;
            }
        }

        public static void PreExecute()
        {
            //Launch an immediate search
            Execute();

            var periodTimeSpan = TimeSpan.FromSeconds(60);
            _timerUpdates = new Timer();
            _timerUpdates.Interval = periodTimeSpan.TotalMilliseconds;
            _timerUpdates.Elapsed += (s, e) => {
                Console.WriteLine("Standard period-triggered scan");
                Execute();
            };
            _timerUpdates.Start();
        }
    }
}
