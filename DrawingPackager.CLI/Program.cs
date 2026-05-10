using DrawingPackager.Core.Packaging;
using DrawingPackager.SolidEdge;

namespace DrawingPackager.CLI
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            if (args.Length == 0 || IsHelp(args[0]))
            {
                WriteUsage();
                return args.Length == 0 ? 1 : 0;
            }

            return args[0].ToLowerInvariant() switch
            {
                "package" => await PackageAsync(args),
                _ => UnknownCommand(args[0])
            };
        }

        private static async Task<int> PackageAsync(string[] args)
        {
            if (args.Length < 3)
            {
                Console.Error.WriteLine("Missing required arguments.");
                WriteUsage();
                return 1;
            }

            var request = new PackageRequest(
                args[1],
                args[2],
                new PackageOptions
                {
                    ExportPdf = true,
                    CopySourceDrawing = true
                });

            var service = new DrawingPackageService(new SolidEdgeDrawingAutomationService());
            var result = await service.PackageAsync(request);

            foreach (var message in result.Messages)
            {
                Console.WriteLine(message);
            }

            if (result.PackageFolder is not null)
            {
                Console.WriteLine($"Package folder: {result.PackageFolder}");
            }

            return result.Success ? 0 : 1;
        }

        private static int UnknownCommand(string command)
        {
            Console.Error.WriteLine($"Unknown command: {command}");
            WriteUsage();
            return 1;
        }

        private static bool IsHelp(string value)
        {
            return value is "-h" or "--help" or "help";
        }

        private static void WriteUsage()
        {
            Console.WriteLine("DrawingPackager CLI");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  DrawingPackager.CLI package <drawing.dft> <output-folder>");
        }
    }
}
