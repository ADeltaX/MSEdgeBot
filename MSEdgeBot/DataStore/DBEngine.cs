using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MSEdgeBot.DataStore
{
    internal sealed class DBEngine
    {
        private readonly SqliteConnection _mDbConnection;

        internal DBEngine()
        {
            if (!System.IO.File.Exists(TelegramBotSettings.DATABASE_FILENAME))
                System.IO.File.Create(TelegramBotSettings.DATABASE_FILENAME).Close();

            _mDbConnection = new SqliteConnection($"Data Source={TelegramBotSettings.DATABASE_FILENAME};");
            _mDbConnection.Open();
            Console.WriteLine("DB Connection opened!");

            //creazione tabelle
            RunCommand(@"CREATE TABLE IF NOT EXISTS Updates (id INTEGER PRIMARY KEY, Ring NVARCHAR NOT NULL, Version NVARCHAR NOT NULL, FileName NVARCHAR NOT NULL, FileSize INTEGER, SHA256 TEXT, Url TEXT, AddedAt DATETIME);
CREATE TABLE IF NOT EXISTS Errors(idError INTEGER PRIMARY KEY, mDateTime datetime NOT NULL, errorMessage NVARCHAR);");
        }

        internal void RunCommand(string sql, params KeyValuePair<string, object>[] parameteres)
        {
            try
            {
                using (var command = new SqliteCommand(sql, _mDbConnection))
                {
                    foreach (var param in parameteres)
                        command.Parameters.Add(new SqliteParameter(param.Key, param.Value));

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Ovvero: " + sql);
            }
        }

        internal bool GetBoolCommand(string sql, params KeyValuePair<string, object>[] parameteres)
        {
            try
            {
                using (var command = new SqliteCommand(sql, _mDbConnection))
                {
                    foreach (var param in parameteres)
                        command.Parameters.Add(new SqliteParameter(param.Key, param.Value));

                    using (var reader = command.ExecuteReader())
                        while (reader.Read())
                            return reader.GetBoolean(0);
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        internal string GetStringCommand(string sql, params KeyValuePair<string, object>[] parameteres)
        {
            try
            {
                using (var command = new SqliteCommand(sql, _mDbConnection))
                {
                    foreach (var param in parameteres)
                        command.Parameters.Add(new SqliteParameter(param.Key, param.Value));

                    using (var reader = command.ExecuteReader())
                        while (reader.Read())
                            return reader.GetString(0);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        internal int GetIntCommand(string sql, params KeyValuePair<string, object>[] parameteres)
        {
            try
            {
                using (var command = new SqliteCommand(sql, _mDbConnection))
                {
                    foreach (var param in parameteres)
                        command.Parameters.Add(new SqliteParameter(param.Key, param.Value));

                    using (var reader = command.ExecuteReader())
                        while (reader.Read())
                            return reader.GetInt32(0);
                }

                return -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }

        internal List<int> GetListIntCommand(string sql, params KeyValuePair<string, object>[] parameteres)
        {
            List<int> ints = new List<int>();

            try
            {
                using (var command = new SqliteCommand(sql, _mDbConnection))
                {
                    foreach (var param in parameteres)
                        command.Parameters.Add(new SqliteParameter(param.Key, param.Value));

                    using (var reader = command.ExecuteReader())
                        while (reader.Read())
                            ints.Add(reader.GetInt32(0));
                }

                return ints;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ints;
            }
        }

        internal List<KeyValuePair<long, string>> GetListKVLongStringCommand(string sql, params KeyValuePair<string, object>[] parameteres)
        {
            List<KeyValuePair<long, string>> valuesList = new List<KeyValuePair<long, string>>();

            try
            {
                using (var command = new SqliteCommand(sql, _mDbConnection))
                {
                    foreach (var param in parameteres)
                        command.Parameters.Add(new SqliteParameter(param.Key, param.Value));

                    using (var reader = command.ExecuteReader())
                        while (reader.Read())
                            valuesList.Add(new KeyValuePair<long, string>(reader.GetInt64(0), reader.GetString(1)));
                }

                return valuesList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return valuesList;
            }
        }

        internal List<ValueTuple<string, string, string>> GetListTupleStringCommand(string sql, params KeyValuePair<string, object>[] parameteres)
        {
            List<ValueTuple<string, string, string>> valuesList = new List<ValueTuple<string, string, string>>();

            try
            {
                using (var command = new SqliteCommand(sql, _mDbConnection))
                {
                    foreach (var param in parameteres)
                        command.Parameters.Add(new SqliteParameter(param.Key, param.Value));

                    using (var reader = command.ExecuteReader())
                        while (reader.Read())
                            valuesList.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2)));
                }

                return valuesList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return valuesList;
            }
        }
    }
}
