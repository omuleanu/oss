using System;

namespace Oss
{
    public class Logger : ILogger
    {
        public int Level;

        public void Info(string format, params object[] args)
        {
            var message = string.Format(format, args);
            Log(message);
        }

        public void Log(string message, ConsoleColor color = ConsoleColor.White, int level = 0)
        {
            if (level >= Level)
            {
                if (color != ConsoleColor.White)
                {
                    Console.ForegroundColor = color;
                }

                Console.WriteLine(message);
                Console.ResetColor();
            }
        }

        public void Info(string message)
        {
            Log(message);
        }

        public void Error(string message)
        {
            Log(message, ConsoleColor.Magenta, 10);
        }

        public void Message(string message)
        {
            Log(message, level: 10);
        }

        public void Warn(string message)
        {
            Log(message, ConsoleColor.Yellow, 9);
        }

        public void Noise()
        {
            Console.Beep();
        }
    }
}