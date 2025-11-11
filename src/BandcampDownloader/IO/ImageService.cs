using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace BandcampDownloader.IO;

internal interface IImageService
{
    Task ConvertToJpegAsync(string inputImageFilePath, string outputJpegFilePath, CancellationToken cancellationToken);
    Task ResizeImage(string inputImageFilePath, string outputImageFilePath, int maxWidth, int maxHeight, CancellationToken cancellationToken);
}

internal sealed class ImageService : IImageService
{
    public async Task ConvertToJpegAsync(string inputImageFilePath, string outputJpegFilePath, CancellationToken cancellationToken)
    {
        using var image = await Image.LoadAsync(inputImageFilePath, cancellationToken);

        var jpegEncoder = new JpegEncoder
        {
            Quality = 90,
        };

        await image.SaveAsJpegAsync(outputJpegFilePath, jpegEncoder, cancellationToken);
    }

    public async Task ResizeImage(string inputImageFilePath, string outputImageFilePath, int maxWidth, int maxHeight, CancellationToken cancellationToken)
    {
        using var image = await Image.LoadAsync(inputImageFilePath, cancellationToken);

        var resizeOptions = new ResizeOptions
        {
            Mode = ResizeMode.Min,
            Size = new Size(maxWidth, maxHeight),
        };
        image.Mutate(x => x.Resize(resizeOptions));

        await image.SaveAsync(outputImageFilePath, cancellationToken);
    }
}
