using System;
using System.IO;
using System.Text.RegularExpressions;

namespace BandcampDownloader.IO;

internal static class FileHelper
{
    public static string ToAllowedFileName(this string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);

        fileName = fileName.ReplaceInvalidPathCharacters('_');

        // Remove trailing dot(s)
        fileName = Regex.Replace(fileName, @"\.+$", "");

        // Replace whitespace(s) by ' '
        fileName = Regex.Replace(fileName, @"\s+", " ");

        // Remove trailing whitespace(s) /!\ Must be last
        fileName = Regex.Replace(fileName, @"\s+$", "");

        return fileName;
    }

    private static string ReplaceInvalidPathCharacters(this string path, char replaceBy)
    {
        foreach (var invalidCharacter in Path.GetInvalidPathChars())
        {
            path = path.Replace(invalidCharacter, replaceBy);
        }

        foreach (var invalidCharacter in Path.GetInvalidFileNameChars())
        {
            path = path.Replace(invalidCharacter, replaceBy);
        }

        return path;
    }
}
