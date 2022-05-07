using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPProfileDownloader
{
    public static class ErrorQueue
    {
        private static List<string> errors = new List<string>();
        public static void Add(string message)
        {
            if (!errors.Exists(item => item == message))
            {
                errors.Add(message);
            }
        }
        public static List<string> PurgeQueue()
        {
            List<string> result = errors;
            errors = new List<string>();
            return result;
        }
    }
}
