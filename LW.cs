using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Text;

namespace LogWriter
{
    internal class LW
    {
        public static readonly Config m_config = new Config(File.ReadAllLines("config.properties"));

        public static void Main(string[] args)
        {
            // "dd/MMM/YYYY:HH:mm:ss Z"
            //var record = Utils.scant("192.168.2.20 - - [28/Jul/2006:10:27:10 -0300] \"GET /cgi-bin/try/ HTTP/1.0\" 200 3395", m_config.log_format);
            //Console.WriteLine(record);
            //Console.WriteLine(DateTime.ParseExact("[28/Jul/2006:10:22:04 -0300]", m_config.date_format, CultureInfo.InvariantCulture));
            
            if (args.Length > 0 && args[0] == "parse")
            {
                if (args.Length > 1)
                {
                    Console.Error.WriteLine("Unknown arguments after 'parse'");
                    Environment.Exit(-1);
                }

                parse();
            } else {
                filters f = new filters();
                List<string> showOnly = new List<string>();
                for (int i = 0; i < args.Length; i++) {
                    string flag = args[i];
                    switch (flag) {
                        case "-ip":
                            f.ipFilter = args[++i];
                            break;
                        case "-u":
                            f.userFiler = args[++i];
                            break;
                        case "-minDate":
                            f.minDateFilter = DateTime.ParseExact(args[++i], "dd.MM.yyyy", CultureInfo.InvariantCulture);
                            break;
                        case "-maxDate":
                            f.maxDateFilter = DateTime.ParseExact(args[++i], "dd.MM.yyyy", CultureInfo.InvariantCulture);
                            break;
                        case "-s":
                            f.statusFilter = int.Parse(args[++i]);
                            break;
                        default:
                            if (availableShowOnly.Contains(flag)) {
                                showOnly.Add(flag);
                            } else {
                                Console.WriteLine("Unknown flag: " + flag + ".");
                                Console.WriteLine("Available filter flags: -ip, -u, -minDate, -maxDate, -s");
                                Console.WriteLine("Available tables to view: " + String.Join(", ", availableShowOnly.ToArray()));
                                Environment.Exit(0);
                            }
                            break;
                    }
                }

                if (showOnly.Count == 0) {
                    showOnly.AddRange(availableShowOnly);
                }
                printAll(f, showOnly);
            }
        }

        private static readonly List<string> availableShowOnly = new List<string>(new[]{"ip","user","name","date","first_line","status","size"});

        static readonly string connectionString = "Data Source=logs.sqlite;Version=3;";
        private static void printAll(filters filters, List<string> showOnly) {
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionString);
            m_dbConnection.Open();

            StringBuilder cmdBuilder = new StringBuilder("select\n");
            for (int i = 0; i < showOnly.Count; i++) {
                string se = showOnly[i]; // O(1) в любом случае
                if (se == "date") {
                    cmdBuilder.Append("strftime(log_date) as \"date\"");
                } else cmdBuilder.Append("log_" + se + " as " + "\"" + se + "\"");
                if (i != showOnly.Count-1) {
                    cmdBuilder.Append(",\n");
                }
            }

            cmdBuilder.Append(" from log where not length(log_ip) = 0 ");
            if (filters.ipFilter != null) {
                cmdBuilder.Append($"AND ip = '{filters.ipFilter}' ");
            }
            if (filters.userFiler != null) {
                cmdBuilder.Append($"AND user = '{filters.userFiler}' ");
            }
            if (filters.statusFilter != null) {
                cmdBuilder.Append($"AND status = {filters.statusFilter} ");
            }
            if (filters.minDateFilter != null) {
                cmdBuilder.Append($"AND '{filters.minDateFilter}' < date ");
            }
            if (filters.maxDateFilter != null) {
                cmdBuilder.Append($"AND date < '{filters.maxDateFilter}' ");
            }



            SQLiteDataReader r;
            try
            {
                r = new SQLiteCommand(cmdBuilder.ToString(), m_dbConnection).ExecuteReader();
            } catch (Exception e) {
                //Console.WriteLine(cmdBuilder.ToString());
                //Console.WriteLine(e.Message);
                //Console.WriteLine(e.StackTrace);
                
                Console.WriteLine("Database not found, parsing...");
                m_dbConnection.Close();
                parse();
                try {
                    m_dbConnection = new SQLiteConnection(connectionString);
                    m_dbConnection.Open();
                    r = new SQLiteCommand(cmdBuilder.ToString(), m_dbConnection).ExecuteReader();
                } catch (Exception) {
                    Console.WriteLine("Unknown error");
                    throw;
                }
            }

            foreach (string se in showOnly) {
                Console.Write(se + "\t");
                switch (se)
                {
                    case "date":
                    case "first_line":
                        Console.Write("\t\t");
                        break;
                    case "ip":
                        Console.Write('\t');
                        break;
                }
            }

            Console.WriteLine();
            while (r.Read()) {
                foreach (string se in showOnly) {
                    Console.Write(r[se] + "\t");
                }

                Console.WriteLine();
            }

            r.Close();
            m_dbConnection.Close();
        }

        struct filters {
            public string ipFilter;
            public string userFiler;
            public DateTime? maxDateFilter;
            public DateTime? minDateFilter;
            public int? statusFilter;
        }

        static void parse() {
            // this creates a zero-byte file
            SQLiteConnection.CreateFile("logs.sqlite");
            
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionString);
            m_dbConnection.Open();
            
            SQLiteCommand command = new SQLiteCommand("Create Table log (serial serial, log_ip varchar(16), log_user varchar(64), log_name varchar(64), log_date date, log_first_line varchar(64), log_status int, log_size int)", m_dbConnection);
            command.ExecuteNonQuery();
            
            
            foreach (String fname in Directory.GetFiles(m_config.log_files_dir)) {
                if (!fname.EndsWith($".{m_config.log_extension}")) {
                    continue;
                }
                
                foreach (string line in File.ReadAllLines(fname)) {
                    try {
                        LogRecord record = Utils.scant(line, m_config.log_format);
                        string cmd =
                            "Insert into log (log_ip, log_user, log_name, log_date, log_first_line, log_status, log_size) " +
                            $"values {record.toSqlValuesString()}";
                        command = new SQLiteCommand(cmd, m_dbConnection);
                        command.ExecuteNonQuery();
                    } catch (Exception) { 
                        Console.Error.WriteLine("Log parsing error: '" + line + "' (skipped)"); 
                        //Console.Error.WriteLine(e.StackTrace);
                    }
                }
            }

            Console.WriteLine("Parsed successfully");
        }
    }
}