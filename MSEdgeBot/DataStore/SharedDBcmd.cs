using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace MSEdgeBot.DataStore
{
    internal static class SharedDBcmd
    {
        readonly static DBEngine _db;
        static SharedDBcmd() => _db = Program.Db;

        public static void TraceError(string errorMessage, [CallerMemberName] string memberName = "") 
            => _db.RunCommand("INSERT INTO Errors(mDateTime, errorMessage) VALUES (datetime('now'), @errorMessage)", new KeyValuePair<string, object>("errorMessage", errorMessage + " | ON " + memberName + "()"));
        
        public static bool UpdateExists(string ring, string version) 
            => _db.GetBoolCommand("SELECT CASE WHEN EXISTS (SELECT * FROM Updates WHERE Ring = @ring AND Version = @version) THEN CAST (1 AS BIT) ELSE CAST (0 AS BIT) END", new KeyValuePair<string, object>("ring", ring), new KeyValuePair<string, object>("version", version));
        
        public static void AddNewUpdate(string ring, string version, string filename, long filesize, string sha256, string url) 
            => _db.RunCommand("INSERT INTO Updates(Ring, Version, FileName, FileSize, SHA256, Url, AddedAt) VALUES (@ring, @version, @filename, @filesize, @sha256, @url, @addedat)", new KeyValuePair<string, object>("ring", ring), new KeyValuePair<string, object>("version", version), new KeyValuePair<string, object>("filename", filename), new KeyValuePair<string, object>("filesize", filesize), new KeyValuePair<string, object>("sha256", sha256), new KeyValuePair<string, object>("url", url), new KeyValuePair<string, object>("addedat", DateTime.UtcNow));
    }
}
