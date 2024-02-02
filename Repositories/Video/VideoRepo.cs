using mainykdovanok.Models;
using mainykdovanok.Tools;
using MySql.Data.MySqlClient;
using Serilog;

namespace mainykdovanok.Repositories.Video
{
    public class VideoRepo
    {
        private Serilog.ILogger _logger;
        private readonly string _connectionString;

        public VideoRepo()
        {
            CreateLogger();

            _connectionString = Environment.GetEnvironmentVariable("DATABASE_CONN_STRING");
        }

        public VideoRepo(string connectionString)
        {
            CreateLogger();

            _connectionString = connectionString;
        }

        private void CreateLogger()
        {
            _logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
        }

        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public async Task<bool> InsertVideo(VideoModel video)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            if (video.File != null)
            {
                IFormFile videoFile = video.File;

                // Remove the await keyword here
                byte[] compressedVideoData = await VideoCompressor.ResizeCompressVideo(videoFile);

                using MySqlCommand command = new MySqlCommand(
                    "INSERT INTO motivational_videos (video_data, fk_item, fk_user) VALUES (@video, @fk_item, @fk_user)", connection);

                // Add parameters
                command.Parameters.AddWithValue("@video", compressedVideoData);
                command.Parameters.AddWithValue("@fk_item", video.Item);
                command.Parameters.AddWithValue("@fk_user", video.User);

                await command.ExecuteNonQueryAsync();
            }

            return true;
        }




    }
}
