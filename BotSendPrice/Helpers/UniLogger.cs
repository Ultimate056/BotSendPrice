using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSavePrices
{
    public static class UniLogger
    {
        private static BlockingCollection<string> _blockingCollection;
        private static string _filename = Directory.GetCurrentDirectory() + @"\log\log" + DateTime.Now.ToString("dd MM yy HH mm ss") + ".txt";
        private static Task _task;

        static UniLogger()
        {
            _blockingCollection = new BlockingCollection<string>();

            _task = Task.Factory.StartNew(() =>
            {
                using (var streamWriter = new StreamWriter(_filename, true, Encoding.UTF8))
                {
                    streamWriter.AutoFlush = true;

                    foreach (var s in _blockingCollection.GetConsumingEnumerable())
                        streamWriter.WriteLine(s);
                }
            },
            TaskCreationOptions.LongRunning);
        }

        public static void WriteLog(string action, int errorCode, string errorDescription)
        {
            _blockingCollection.Add($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff")} действие: {action}, код: {errorCode.ToString()}, описание: { errorDescription} ");
            //_blockingCollection.Add(@"[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff") + "] тип: " + errorCode.ToString() + " сообщение: " + errorDescription);
        }

        public static void Flush()
        {
            _blockingCollection.CompleteAdding();
            _task.Wait();
        }
    }
}
