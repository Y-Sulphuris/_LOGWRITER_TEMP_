using System;

namespace LogWriter
{
    public class LogRecord {
        private string ip;
        private string u, l;
        private DateTime date;
        private string r;
        private int s;
        private int b;

        public LogRecord(string ip, string u, string l, DateTime date, string r, int s, int b) {
            this.ip = ip;
            this.u = u;
            this.l = l;
            this.date = date;
            this.r = r;
            this.s = s;
            this.b = b;
        }

        public string toSqlValuesString() {
            return $"('{ip}', '{u}', '{l}', '{date}', '{r}', {s}, {b})";
        }
        
        public string ToJson() {
            return $"{{\"ip\": \"{ip}\", \"user:\"\"{u}\", \"logname\": \"{l}\", \"time\": \"{date}\", \"first_line\": \"{r}\", \"status\": \"{s}\", \"bytes\": \"{b}\"}}";
        }
    }
}