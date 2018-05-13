using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheMiddle.CommandLine;
using TheMiddle.Interceptors;
using TheMiddle.Monitors;

namespace TheMiddle
{
    internal static class Program
    {

        private static Task ConsoleInterruptionTask;

        static int Main(string[] args)
        {
            var argList = new List<CommandArgument>();
            var doubleDashArgBuilder = (StringBuilder) null;
            foreach (var a in args)
            {
                if (doubleDashArgBuilder != null)
                {
                    doubleDashArgBuilder.Append(" ");
                    if (a.Contains(" "))
                    {
                        doubleDashArgBuilder.Append('"');
                        doubleDashArgBuilder.Append(a);
                        doubleDashArgBuilder.Append('"');
                    }
                    else
                    {
                        doubleDashArgBuilder.Append(a);
                    }
                    continue;
                }
                if (a == "--")
                {
                    doubleDashArgBuilder = new StringBuilder();
                    continue;
                }
                argList.Add(CommandLineParser.ParseArgument(a));
            }
            var doubleDashArg = (string) null;
            if (doubleDashArgBuilder != null)
            {
                doubleDashArg = doubleDashArgBuilder.ToString(1, doubleDashArgBuilder.Length - 1);
            }
            var consoleInterruptionTcs = new TaskCompletionSource<bool>();
            Console.CancelKeyPress += (sender, e) =>
            {
                if (consoleInterruptionTcs.TrySetResult(true))
                    e.Cancel = true;
            };
            ConsoleInterruptionTask = consoleInterruptionTcs.Task;
            return MainAsync(new CommandArguments(argList), doubleDashArg).GetAwaiter().GetResult();
        }

        static async Task<int> MainAsync(CommandArguments args, string doubleDashArg)
        {
            if (args.Count == 0 || new[] {"?", "h", "help"}.Any(args.NamedArguments.ContainsKey))
            {
                Console.WriteLine(Prompts.HelpMessage);
                return 0;
            }
            var dumpFileName = (string) args.Requires("d");
            var applicationFileName = (string) args.Requires("a");
            using (var dumpStream = File.OpenWrite(dumpFileName))
            {
                var monitor = new DumpedStreamMonitorProvider(dumpStream);
                using (var cin = Console.OpenStandardInput())
                using (var bcin = new BufferedStream(cin))
                using (var cout = Console.OpenStandardOutput())
                using (var cerr = Console.OpenStandardError())
                using (var mcin = new MonitoredStream(bcin, monitor.CreateStreamMonitor("STDIN")))
                using (var mcout = new MonitoredStream(cout, monitor.CreateStreamMonitor("STDOUT")))
                using (var mcerr = new MonitoredStream(cerr, monitor.CreateStreamMonitor("STDERR")))
                using (var interceptor = new StdIOInterceptor(applicationFileName, doubleDashArg, mcin, mcout, mcerr))
                {
                    await interceptor.StartAsync();
                    await Task.WhenAny(interceptor.ProcessExitTask, ConsoleInterruptionTask);
                    await interceptor.StopAsync();
                }
            }
            return 0;
        }
    }
}
