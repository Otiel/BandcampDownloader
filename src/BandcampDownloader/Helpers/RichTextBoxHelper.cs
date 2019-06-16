using System.Windows.Controls;

namespace BandcampDownloader {

    internal static class RichTextBoxHelper {

        /// <summary>
        /// Returns true if scroll position of the current RichTextBox is at the end; false otherwise.
        /// </summary>
        public static bool IsScrolledToEnd(this RichTextBox richTextBox) {
            return richTextBox.VerticalOffset > richTextBox.ExtentHeight - richTextBox.ViewportHeight - 10;
        }
    }
}