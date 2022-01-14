using System.Diagnostics;

namespace Bcdl.Console;

internal static class Helpers
{
    public static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Debugger.Break();
    }
}
