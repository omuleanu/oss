using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Oss
{
    public class ParseConfig
    {
        public Dictionary<string, string> Files { get; set; }
        public bool NoWatch { get; set; }
        public string[] Flags { get; set; }
    }

    class Program
    {
        static readonly ILogger logger = new Logger();

        static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                logger.Error("no arguments specified");
                return;
            }

            MergeActionCheckAndApply(args);

            ConfigActionCheckAndApply(args);

            Parse(args);
        }

        static void Parse(string[] args)
        {
            if (args[0] != "parse") return;

            var i = 1;

            if (args.Length < 2)
            {
                logger.Error("parse arguments are [nowatch] frompath topath or [nowatch] frompath:topath ...");
                return;
            }

            var watch = true;
            if (args[i] == "-nowatch")
            {
                i++;
                watch = false;
            }

            if (args[i] == "-file")
            {
                i++;
                var path = args[i];
                var lines = new FileOp().ReadLines(path);
                var dir = new FileInfo(path).Directory.FullName;

                MultipleParse(lines, dir);
            }
            else if (!args[i].Contains(":"))
            {
                var frompath = args[i++];
                var topath = args[i];

                var parser = new OssParser();
                var res = parser.Parse(frompath, topath, watch);

                HandleRes(res);
            }
            else
            {
                // multiple parse
                var pairs = args.Skip(i).ToArray();
                MultipleParse(pairs);
            }

            void MultipleParse(string[] pairs, string dir = null)
            {
                foreach (var pair in pairs)
                {
                    var arr = pair.Split(':');
                    var frompath = arr[0];
                    var topath = arr[1];

                    var parser = new OssParser();
                    var res = parser.Parse(frompath, topath, watch, dir);

                    HandleRes(res);
                }
            }

            Console.WriteLine("hit enter to stop watching");
            Console.ReadLine();
        }

        static void HandleRes(ParseRes res)
        {
            if (res.Errors > 0)
            {
                Console.WriteLine($"{res.Errors} errors found");
                Console.ReadLine();
            }
        }

        static void MergeActionCheckAndApply(string[] args)
        {
            if (args[0] != "merge") return;

            if (args.Length < 3)
            {
                logger.Error("merge arguments are frompath topath [respath] \r\n e.g. merge a.css b.css");
                return;
            }

            var frompath = args[1];
            var topath = args[2];
            var respath = topath;

            if (args.Length > 3)
            {
                respath = args[3];
            }

            var merger = new Merger();
            var fromstr = File.ReadAllText(frompath);
            var tostr = File.ReadAllText(topath);

            var res = merger.MergeVars(fromstr, tostr, out var removed);

            if (removed.Count > 0)
            {
                var svars = string.Join(",", removed.Select(o => o.VarName));

                Console.WriteLine($"{removed.Count} variables will be removed: {svars}");
                Console.WriteLine("are you sure you want to proceed ? (y/n)");

                var key = Console.ReadKey();
                if (key.Key != ConsoleKey.Y)
                {
                    Console.WriteLine("merge canceled");
                    return;
                }
            }

            if (File.Exists(respath))
            {
                var cont = File.ReadAllText(respath);
                File.WriteAllText(respath + ".bak", cont);
            }

            File.WriteAllText(respath, res);
        }

        static void ConfigActionCheckAndApply(string[] args)
        {
            if (args[0] != "useconfig") return;

            if (args.Length != 2)
            {
                logger.Error("after useconfig you need to specify only 1 argument the config file name");
                return;
            }

            var fileName = args[1];

            if (!File.Exists(fileName))
            {
                logger.Error("can't find file " + fileName);
            }

            var content = File.ReadAllText(fileName);
            var parseConfig = JsonSerializer.Deserialize<ParseConfig>(content);
            var dirToWatch = new FileInfo(fileName).Directory.FullName;
            
            ConfigParse(parseConfig, dirToWatch);

            void ConfigParse(ParseConfig config, string dir = null)
            {
                foreach (var kv in config.Files)
                {
                    var parser = new OssParser { Flags = config.Flags };
                    var res = parser.Parse(kv.Key, kv.Value, !config.NoWatch, dir);
                    HandleRes(res);
                }
            }
        }
    }
}