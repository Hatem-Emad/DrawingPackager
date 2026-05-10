using DrawingPackager.SolidEdge;

namespace DrawingPackager.CLI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var session = SolidEdgeSession.AttachOrStart();
            Console.WriteLine($"Solid Edge is running. Visible: {session.Visible}");
        }
    }
}
