using System;
using System.Globalization;
using System.Text;

namespace LogWriter
{
    public class Utils {
        public static LogRecord scant(string log, string format) {
            // default values
            DateTime date = DateTime.MinValue;
            string ip = "0.0.0.0";
            string r = "UNKNOWN";
            string l = "-";
            string u = "-";
            int s = -1;
            int b = -1;
            
            for (int log_i = 0, f_i = 0;; log_i -= f_i++ - f_i) // log_i++; f_i++
            {
                if (log_i == log.Length != (f_i == format.Length)) { // xor
                    throw new ArgumentException("illegal format");
                }
                if (log_i == log.Length) { // && f_i == format.Length because of previous check
                    break;
                }
                char lch = log[log_i];
                char fch = format[f_i];
                if (fch == '%') {
                    char cmd = format[++f_i];
                    if (cmd == '>') {
                        cmd = format[++f_i];
                    }
                    switch (cmd) {
                        case 'h':
                            ip = parseToNextSpace(log, ref log_i);
                            break;
                        case 'u':
                            u = parseToNextSpace(log, ref log_i);
                            break;
                        case 'l':
                            l = parseToNextSpace(log, ref log_i);
                            break;
                        case 't':
                            date = parseDate(log, ref log_i);
                            break;
                        case 'r':
                            r = parseUntilNext(log, ref log_i, format[f_i+1]);
                            break;
                        case 's':
                            s = int.Parse(parseToNextSpace(log, ref log_i));
                            break;
                        case 'b':
                            b = int.Parse(parseToNextSpace(log, ref log_i));
                            break;
                        default:
                            throw new ArgumentException("illegal format: Unknown format " + cmd);
                    }
                }
                else
                {
                    if (fch != lch) throw new ArgumentException($"illegal format: {fch} != {lch}");
                }
            }

            return new LogRecord(ip, u, l,  date, r, s, b);
        }

        private static String parseUntilNext(string log, ref int i, char fch) {
            StringBuilder str = new StringBuilder();
            while (i < log.Length && log[i] != fch) str.Append(log[i++]);
            --i;
            return str.ToString();
        }

        private static string parseToNextSpace(string log, ref int i) {
            return parseUntilNext(log, ref i, ' ');
        }
        
        private static DateTime parseDate(string log, ref int i) {
            String format = LW.m_config.date_format;

            StringBuilder str = new StringBuilder();
            i++;
            while (i < log.Length && log[i] != ']') str.Append(log[i++]);
            string datestr = str.ToString();
            
            try {
                return DateTime.ParseExact(datestr, format, CultureInfo.InvariantCulture);
            } catch (FormatException) {
                Console.Error.WriteLine("Invalid date: " + datestr + " (expected: " + format + ")");
                throw;
            }
        }
    }
}