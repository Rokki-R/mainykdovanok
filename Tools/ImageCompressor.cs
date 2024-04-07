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
                    // Resize the image
                    img.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(width, height),
                        Mode = ResizeMode.Max
                    }));

                    // Compress the image
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
                // Load watermark image
                using (var watermarkStream = File.OpenRead(watermarkImagePath))
                using (var watermark = Image.Load(watermarkStream))
                {
                    // Calculate position to overlay watermark (e.g., bottom right corner)
                    var position = new Point(0, 0);

                    // Overlay watermark on the image
                    img.Mutate(x => x.DrawImage(watermark, position, 1f));

                    // Convert image back to byte array
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
