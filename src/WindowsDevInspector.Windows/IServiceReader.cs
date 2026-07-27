namespace WindowsDevInspector.Windows;

public interface IServiceReader
{
    ServiceReadResult ReadService(string serviceName);
}
