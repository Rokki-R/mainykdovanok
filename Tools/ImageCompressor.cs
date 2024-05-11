using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace mainykdovanok.Tools
{
    public static class ImageCompressor
    {
        public async static Task<byte[]> ConvertToByteArray(IFormFile image)
        {
            using (var memoryStream = new MemoryStream())
            {
                await image.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public async static Task<byte[]> ResizeCompressImage(IFormFile image, int width, int height, int quality = 90)
        {
            using (var memoryStream = new MemoryStream())
            {
                await image.CopyToAsync(memoryStream);
                var imageData = memoryStream.ToArray();

                using (var imageStream = new MemoryStream(imageData))
                using (var img = Image.Load(imageStream))
                {
                    img.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(width, height),
                        Mode = ResizeMode.Max
                    }));

                    using (var outputStream = new MemoryStream())
                    {
                        var encoder = new JpegEncoder
                        {
                            Quality = quality
                        };
                        img.Save(outputStream, encoder);

                        return outputStream.ToArray();
                    }
                }
            }
        }

        public async static Task<byte[]> AddWatermark(byte[] imageData, string watermarkImagePath)
        {
            using (var imageStream = new MemoryStream(imageData))
            using (var img = Image.Load(imageStream))
            {
                using (var watermarkStream = File.OpenRead(watermarkImagePath))
                using (var watermark = Image.Load(watermarkStream))
                {
                    var position = new Point(
                        (img.Width - watermark.Width) / 2,
                        (img.Height - watermark.Height) / 2
                    );

                    img.Mutate(x => x.DrawImage(watermark, position, 1f));

                    using (var outputStream = new MemoryStream())
                    {
                        img.Save(outputStream, new JpegEncoder());
                        return outputStream.ToArray();
                    }
                }
            }
        }

    }
}
