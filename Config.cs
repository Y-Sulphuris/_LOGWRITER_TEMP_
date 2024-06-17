using System;
using System.Diagnostics.CodeAnalysis;

namespace LogWriter
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Config {
        public readonly String log_files_dir;
        public readonly String log_extension;
        public readonly String log_format;
        public readonly String date_format;

        public Config(String[] lines) {
            foreach (String line in lines) {
                if (line.Length == 0 || line.StartsWith("#")) continue;

                String[] tok = line.Split(new[] {'='}, 2);
                if (tok.Length < 2) {
                    Console.WriteLine("Invalid config syntax at: " + line);
                    Environment.Exit(0);
                }
                switch (tok[0]) {
                    case "log_files_dir":
                        tryToSet(ref log_files_dir, tok);
                        break;
                    case "log_extension": 
                        tryToSet(ref log_extension, tok);
                        break;
                    case "log_format":
                        tryToSet(ref log_format, tok);
                        break;
                    case "date_format":
                        tryToSet(ref date_format, tok);
                        break;
                }
            }
        }

        private void tryToSet(ref String property, in String[] tok) {
            if (property == null) 
                property = tok[1];
            else {
                Console.WriteLine($@"Invalid config: duplicated property '{tok[0]}'");
                Environment.Exit(0);
            }
        }
    }
}