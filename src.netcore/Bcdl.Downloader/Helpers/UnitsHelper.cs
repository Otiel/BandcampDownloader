namespace Bcdl.Downloader.Helpers;

internal static class UnitsHelper
{
    private const int BYTES_PER_KILOBYTE = 1_000;

    public static double BytesToKiloBytes(double bytes)
    {
        return bytes / BYTES_PER_KILOBYTE;
    }
}
