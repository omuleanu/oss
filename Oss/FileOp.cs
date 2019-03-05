using System;
using System.IO;
using System.Threading.Tasks;

namespace Oss
{
    public class FileOp
    {
        private readonly ILogger logger = new Logger();

        public string[] ReadLines(string path)
        {
            string[] res = null;
            Try(() =>
            {
                res = File.ReadAllLines(path);
            });

            return res;
        }

        public string ReadText(string path)
        {
            string res = null;
            Try(() =>
            {
                res = File.ReadAllText(path);
            });

            return res;
        }

        public void WriteText(string path, string content)
        {
            Try(() => File.WriteAllText(path, content));
        }

        private void Try(Action action, int attempt = 0)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                logger.Error(e.Message);

                if (attempt > 2)
                {
                    logger.Noise();
                    logger.Error("can't access file");
                    throw;
                }

                logger.Message("trying again ...");
                Task.Delay(100 * attempt).Wait();
                Try(action, attempt + 1);
            }
        }
    }
}