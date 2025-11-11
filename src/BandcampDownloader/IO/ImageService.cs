using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace BandcampDownloader.IO;

internal interface IImageService
{
    Task<Stream> ConvertToJpegAsync(Stream inputImage, CancellationToken cancellationToken);
    Task<Stream> ResizeImage(Stream inputImage, int maxWidth, int maxHeight, CancellationToken cancellationToken);
}

internal sealed class ImageService : IImageService
{
    public async Task<Stream> ConvertToJpegAsync(Stream inputImage, CancellationToken cancellationToken)
    {
        inputImage.Position = 0;
        using var image = await Image.LoadAsync(inputImage, cancellationToken);

        var jpegEncoder = new JpegEncoder
        {
            Quality = 90,
        };

        var outputStream = new MemoryStream();
        await image.SaveAsJpegAsync(outputStream, jpegEncoder, cancellationToken);

        return outputStream;
    }

    public async Task<Stream> ResizeImage(Stream inputImage, int maxWidth, int maxHeight, CancellationToken cancellationToken)
    {
        inputImage.Position = 0;
        using var image = await Image.LoadAsync(inputImage, cancellationToken);

        var resizeOptions = new ResizeOptions
        {
            Mode = ResizeMode.Min,
            Size = new Size(maxWidth, maxHeight),
        };
        image.Mutate(x => x.Resize(resizeOptions));

        var outputStream = new MemoryStream();
        if (image.Metadata.DecodedImageFormat != null)
        {
            await image.SaveAsync(outputStream, image.Metadata.DecodedImageFormat, cancellationToken);
        }
        else
        {
            // This shouldn't happen hopefully, but if it does, fallback to Jpeg
            await image.SaveAsJpegAsync(outputStream, cancellationToken);
        }

        return outputStream;
    }
}
