using System.Security.Principal;

namespace WindowsDevInspector.ElevatedWorker;

public static class Elevation
{
    public static bool IsProcessElevated()
    {
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new(identity);

        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
