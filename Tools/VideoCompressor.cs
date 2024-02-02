using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using Microsoft.AspNetCore.Mvc;

namespace mainykdovanok.Tools
{
    public class VideoCompressor
    {
        public async static Task<byte[]> ResizeCompressVideo(IFormFile video)
        {
            using (var memoryStream = new MemoryStream())
            {
                await video.CopyToAsync(memoryStream);
                var videoData = memoryStream.ToArray();

                // Implement your video compression logic here
                // You may use external libraries or tools specific to video compression

                // For example, you can use FFmpeg for video compression:
                // var compressedVideoData = FFmpegCompressor.Compress(videoData, width, height);

                // For the purpose of this example, we'll return the original video data
                var compressedVideoData = videoData;

                return compressedVideoData;
            }
        }
    }
}
