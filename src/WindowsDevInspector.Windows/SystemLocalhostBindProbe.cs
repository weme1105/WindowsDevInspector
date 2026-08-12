using System.Net;
using System.Net.Sockets;

namespace WindowsDevInspector.Windows;

public sealed class SystemLocalhostBindProbe : ILocalhostBindProbe
{
    public LocalhostBindProbeResult Probe()
    {
        TcpListener? listener = null;

        try
        {
            listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();

            int port = listener.LocalEndpoint is IPEndPoint endpoint ? endpoint.Port : 0;
            return new LocalhostBindProbeResult
            {
                Succeeded = true,
                Port = port
            };
        }
        catch (SocketException ex)
        {
            return new LocalhostBindProbeResult
            {
                Succeeded = false,
                ErrorMessage = ex.Message
            };
        }
        finally
        {
            listener?.Stop();
        }
    }
}
