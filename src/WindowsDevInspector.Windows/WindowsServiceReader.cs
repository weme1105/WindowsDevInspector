using System.ComponentModel;
using System.ServiceProcess;

namespace WindowsDevInspector.Windows;

public sealed class WindowsServiceReader : IServiceReader
{
    public ServiceReadResult ReadService(string serviceName)
    {
        try
        {
            using ServiceController controller = new(serviceName);

            return new ServiceReadResult
            {
                Exists = true,
                Status = controller.Status.ToString()
            };
        }
        catch (Exception exception) when (exception is InvalidOperationException or Win32Exception)
        {
            return new ServiceReadResult
            {
                Exists = false,
                ErrorMessage = exception.Message
            };
        }
    }
}
