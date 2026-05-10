using System.Runtime.InteropServices;

namespace DrawingPackager.SolidEdge;

public sealed class SolidEdgeSession
{
    private const string ProgramId = "SolidEdge.Application";

    private SolidEdgeSession(dynamic application)
    {
        Application = application;
    }

    public dynamic Application { get; }

    public bool Visible
    {
        get => Application.Visible;
        set => Application.Visible = value;
    }

    public static SolidEdgeSession AttachToRunning()
    {
        CLSIDFromProgID(ProgramId, out var classId);
        GetActiveObject(ref classId, IntPtr.Zero, out var application);
        return new SolidEdgeSession(application);
    }

    public static SolidEdgeSession StartNew(bool visible = true)
    {
        var type = Type.GetTypeFromProgID(ProgramId)
            ?? throw new InvalidOperationException("Solid Edge is not registered on this machine.");

        var application = Activator.CreateInstance(type)
            ?? throw new InvalidOperationException("Solid Edge could not be started.");

        var session = new SolidEdgeSession(application);
        session.Visible = visible;
        return session;
    }

    public static SolidEdgeSession AttachOrStart(bool visible = true)
    {
        try
        {
            var session = AttachToRunning();
            session.Visible = visible;
            return session;
        }
        catch (COMException)
        {
            return StartNew(visible);
        }
    }

    [DllImport("ole32.dll", CharSet = CharSet.Unicode)]
    private static extern int CLSIDFromProgID(string programId, out Guid classId);

    [DllImport("oleaut32.dll", PreserveSig = false)]
    private static extern void GetActiveObject(ref Guid classId, IntPtr reserved, [MarshalAs(UnmanagedType.IUnknown)] out object application);
}
