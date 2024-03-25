using mainykdovanok.Models;
using mainykdovanok.ViewModels.Device;
using mainykdovanok.Tools;
using MySql.Data.MySqlClient;
using Serilog;
using System.Data.Common;
using System.Data;

namespace mainykdovanok.Repositories.Image
{
    public class ImageRepo
    {
        private Serilog.ILogger _logger;
        private readonly string _connectionString;

        public ImageRepo()
        {
            CreateLogger();

            _connectionString = Environment.GetEnvironmentVariable("DATABASE_CONN_STRING");
        }

        public ImageRepo(string connectionString)
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
        public async Task<bool> InsertImages(DeviceModel device)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            foreach (IFormFile image in device.Images)
            {
                byte[] data = await ImageCompressor.ResizeCompressImage(image, 640, 480);

                data = await ImageCompressor.AddWatermark(data, "ClientApp\\public\\images\\watermark.png"); // Provide path to watermark image

                using MySqlCommand command = new MySqlCommand(
                    "INSERT INTO device_image (image, fk_device) VALUES (@image, @fk_device)", connection);

                // Add parameters
                command.Parameters.AddWithValue("@image", data);
                command.Parameters.AddWithValue("@fk_device", device.Id);

                await command.ExecuteNonQueryAsync();
            }

            return true;
        }

        public async Task<List<DeviceImageViewModel>> GetByDevice(int id)
        {
            List<DeviceImageViewModel> images = new List<DeviceImageViewModel>();

            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "SELECT id, image FROM device_image WHERE fk_device=@id", connection);
            command.Parameters.AddWithValue("@id", id);

            using DbDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                int dataLength = (int)reader.GetBytes(1, 0, null, 0, int.MaxValue);
                byte[] imageData = new byte[dataLength];
                reader.GetBytes(1, 0, imageData, 0, dataLength);

                DeviceImageViewModel image = new()
                {
                    Id = reader.GetInt32("id"),
                    Data = imageData,
                };
                images.Add(image);
            }
            return images;
        }

        public async Task<List<DeviceImageViewModel>> GetByDeviceFirst(int id)
        {
            List<DeviceImageViewModel> image = new List<DeviceImageViewModel>();

            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "SELECT id, image FROM device_image WHERE fk_device=@id", connection);
            command.Parameters.AddWithValue("@id", id);

            using DbDataReader reader = await command.ExecuteReaderAsync();
            await reader.ReadAsync();

            int dataLength = (int)reader.GetBytes(1, 0, null, 0, int.MaxValue);
            byte[] imageData = new byte[dataLength];
            reader.GetBytes(1, 0, imageData, 0, dataLength);

            var firstImage = new DeviceImageViewModel()
            {
                Id = reader.GetInt32("id"),
                Data = imageData
            };

            image.Add(firstImage);

            return image;
        }

        public async Task<bool> Delete(List<int> ids)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "DELETE FROM device_image WHERE id = @id", connection);

            foreach (int id in ids)
            {
                // Clear parameters before adding new ones
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@id", id);
                await command.ExecuteNonQueryAsync();
            }

            return true;
        }

    }
}
