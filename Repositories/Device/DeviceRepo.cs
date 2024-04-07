using MySql.Data.MySqlClient;
using mainykdovanok.Models;
using mainykdovanok.Repositories.Image;
using mainykdovanok.ViewModels.Device;
using mainykdovanok.ViewModels.User;
using Org.BouncyCastle.Cms;
using Serilog;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace mainykdovanok.Repositories.Device
{
    public class DeviceRepo
    {
        private Serilog.ILogger _logger;
        private readonly string _connectionString;
        private readonly ImageRepo _imageRepo;

        public DeviceRepo()
        {
            CreateLogger();

            _connectionString = Environment.GetEnvironmentVariable("DATABASE_CONN_STRING");
            _imageRepo = new ImageRepo();
        }

        public DeviceRepo(string connectionString)
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

        public async Task<List<DeviceViewModel>> GetAll()
        {
            var devices = new List<DeviceViewModel>();

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand("SELECT device_ad.id, device_ad.name, device_ad.description, " +
                "device_ad.fk_user, device_ad.location, device_ad.winner_draw_datetime, device_type.type as type " +
                "FROM device_ad " +
                "LEFT JOIN device_type ON device_ad.fk_type = device_type.id " +
                "WHERE device_ad.fk_status = 1", connection))
                {
                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var device = new DeviceViewModel()
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                UserId = Convert.ToInt32(reader["fk_user"]),
                                Name = reader["name"].ToString(),
                                Description = reader["description"].ToString(),
                                Type = reader["type"].ToString(),
                                Location = reader["location"].ToString(),
                                WinnerDrawDateTime = Convert.ToDateTime(reader["winner_draw_datetime"])
                            };

                            device.Images = await _imageRepo.GetByDeviceFirst(device.Id);

                            devices.Add(device);
                        }
                    }
                }
            }

            return devices;
        }

        public async Task<DeviceViewModel> GetFullById(int deviceId)
        {
            var device = new DeviceViewModel();

            var images = await _imageRepo.GetByDevice(deviceId);
            var questions = await GetQuestions(deviceId);
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand("SELECT device_ad.*, device_type.type AS device_type, device_category.name AS category_name, device_status.name AS status_name, " +
                    "COUNT(device_lottery_participant.id) AS participants_count " +
                    "FROM device_ad " +
                    "JOIN device_type ON device_ad.fk_type = device_type.id " +
                    "JOIN device_category ON device_ad.fk_category = device_category.id " +
                    "JOIN device_status ON device_ad.fk_status = device_status.id " +
                    "LEFT JOIN device_lottery_participant ON device_ad.id = device_lottery_participant.fk_device " +
                    "WHERE device_ad.id = @deviceId " +
                    "GROUP BY device_ad.id, device_type.type, device_category.name", connection))
                {
                    command.Parameters.AddWithValue("@deviceId", deviceId);
                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        await reader.ReadAsync();

                        device.Id = Convert.ToInt32(reader["id"]);
                        device.UserId = Convert.ToInt32(reader["fk_user"]);
                        device.Name = reader["name"].ToString();
                        device.Description = reader["description"].ToString();
                        device.Status = reader["status_name"].ToString();
                        device.Type = reader["device_type"].ToString();
                        device.Participants = Convert.ToInt32(reader["participants_count"]);
                        device.Location = reader["location"].ToString();
                        device.Category = reader["category_name"].ToString();
                        device.CreationDateTime = Convert.ToDateTime(reader["creation_datetime"]);
                        device.WinnerDrawDateTime = reader["winner_draw_datetime"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["winner_draw_datetime"]);
                        device.WinnerId = reader["fk_winner"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["fk_winner"]);
                        device.Images = images;
                        device.Questions = questions;

                        return device;
                    }
                }
            }
        }

        public async Task<List<DeviceViewModel>> GetAllByUser(int userId)
        {
            List<DeviceViewModel> devices = new List<DeviceViewModel>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                using (MySqlCommand command = new MySqlCommand("SELECT device_ad.*, device_type.type AS device_type, device_category.name AS category_name, device_status.name AS status_name, " +
                    "COUNT(device_lottery_participant.id) AS participants_count " +
                    "FROM device_ad " +
                    "JOIN device_type ON device_ad.fk_type = device_type.id " +
                    "JOIN device_category ON device_ad.fk_category = device_category.id " +
                    "JOIN device_status ON device_ad.fk_status = device_status.id " +
                    "LEFT JOIN device_lottery_participant ON device_ad.id = device_lottery_participant.fk_device " +
                    "WHERE device_ad.fk_user = @userId AND device_ad.fk_status = 1 " +
                    "GROUP BY device_ad.id, device_type.type, device_category.name ", connection))
                {
                    await connection.OpenAsync();
                    command.Parameters.AddWithValue("@userId", userId);

                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var device = new DeviceViewModel()
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                UserId = Convert.ToInt32(reader["fk_user"]),
                                Name = reader["name"].ToString(),
                                Description = reader["description"].ToString(),
                                Status = reader["status_name"].ToString(),
                                Type = reader["device_type"].ToString(),
                                Participants = Convert.ToInt32(reader["participants_count"]),
                                Location = reader["location"].ToString(),
                                Category = reader["category_name"].ToString(),
                                CreationDateTime = Convert.ToDateTime(reader["creation_datetime"]),
                                WinnerDrawDateTime = Convert.ToDateTime(reader["winner_draw_datetime"]),
                                WinnerId = reader["fk_winner"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["fk_winner"]),
                                Images = await _imageRepo.GetByDevice(Convert.ToInt32(reader["id"])),
                            };

                            devices.Add(device);
                        }
                    }

                    return devices;
                }
            }
        }

        public async Task<List<DeviceViewModel>> GetUserWonDevices(int userId)
        {
            List<DeviceViewModel> devices = new List<DeviceViewModel>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                using (MySqlCommand command = new MySqlCommand("SELECT device_ad.*, device_type.type AS device_type, device_category.name AS category_name, device_status.name AS status_name, " +
                    "COUNT(device_lottery_participant.id) AS participants_count " +
                    "FROM device_ad " +
                    "JOIN device_type ON device_ad.fk_type = device_type.id " +
                    "JOIN device_category ON device_ad.fk_category = device_category.id " +
                    "JOIN device_status ON device_ad.fk_status = device_status.id " +
                    "LEFT JOIN device_lottery_participant ON device_ad.id = device_lottery_participant.fk_device " +
                    "WHERE device_ad.fk_winner = @userId AND (device_ad.fk_status = 2 OR device_ad.fk_status = 3 OR device_ad.fk_status = 5) " +
                    "GROUP BY device_ad.id, device_type.type, device_category.name ", connection))
                {
                    await connection.OpenAsync();
                    command.Parameters.AddWithValue("@userId", userId);

                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var device = new DeviceViewModel()
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                UserId = Convert.ToInt32(reader["fk_user"]),
                                Name = reader["name"].ToString(),
                                Description = reader["description"].ToString(),
                                Status = reader["status_name"].ToString(),
                                Type = reader["device_type"].ToString(),
                                Participants = Convert.ToInt32(reader["participants_count"]),
                                Location = reader["location"].ToString(),
                                Category = reader["category_name"].ToString(),
                                CreationDateTime = Convert.ToDateTime(reader["creation_datetime"]),
                                WinnerDrawDateTime = Convert.ToDateTime(reader["winner_draw_datetime"]),
                                WinnerId = reader["fk_winner"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["fk_winner"]),
                                Images = await _imageRepo.GetByDevice(Convert.ToInt32(reader["id"])),
                            };

                            devices.Add(device);
                        }
                    }

                    return devices;
                }
            }
        }

        public async Task<List<DeviceViewModel>> GetAllByCategory(int categoryId)
        {
            List<DeviceViewModel> devices = new List<DeviceViewModel>();


            using (var connection = new MySqlConnection(_connectionString))
            {
                using (MySqlCommand command = new MySqlCommand("SELECT device_ad.*, device_type.type AS device_type, device_category.name AS category_name, device_status.name AS status_name, " +
                    "COUNT(device_lottery_participant.id) AS participants_count " +
                    "FROM device_ad " +
                    "JOIN device_type ON device_ad.fk_type = device_type.id " +
                    "JOIN device_category ON device_ad.fk_category = device_category.id " +
                    "JOIN device_status ON device_ad.fk_status = device_status.id " +
                    "LEFT JOIN device_lottery_participant ON device_ad.id = device_lottery_participant.fk_device " +
                    "WHERE device_ad.fk_category = @categoryId AND device_ad.fk_status = 1 " +
                    "GROUP BY device_ad.id, device_type.type, device_category.name ", connection))
                {
                    await connection.OpenAsync();
                    command.Parameters.AddWithValue("@categoryId", categoryId);

                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var device = new DeviceViewModel()
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                UserId = Convert.ToInt32(reader["fk_user"]),
                                Name = reader["name"].ToString(),
                                Description = reader["description"].ToString(),
                                Status = reader["status_name"].ToString(),
                                Type = reader["device_type"].ToString(),
                                Participants = Convert.ToInt32(reader["participants_count"]),
                                Location = reader["location"].ToString(),
                                Category = reader["category_name"].ToString(),
                                CreationDateTime = Convert.ToDateTime(reader["creation_datetime"]),
                                WinnerDrawDateTime = Convert.ToDateTime(reader["winner_draw_datetime"]),
                                Images = await _imageRepo.GetByDevice(Convert.ToInt32(reader["id"])),
                            };

                            devices.Add(device);
                        }
                    }

                    return devices;
                }
            }
        }

        public async Task<int> Create(DeviceModel device)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "INSERT INTO device_ad (name, description, location, fk_category, fk_user, fk_status, fk_type, winner_draw_datetime) " +
                "VALUES (@Name, @Description, @Location, @Category, @User, @Status, @Type, @WinnerDrawDate)", connection);

            command.Parameters.AddWithValue("@Name", device.Name);
            command.Parameters.AddWithValue("@Description", device.Description);
            command.Parameters.AddWithValue("@Location", device.Location);
            command.Parameters.AddWithValue("@Category", device.Category);
            command.Parameters.AddWithValue("@User", device.User);
            command.Parameters.AddWithValue("@Status", device.Status);
            command.Parameters.AddWithValue("@Type", device.Type);
            command.Parameters.AddWithValue("@WinnerDrawDate", device.WinnerDrawDate);

            await command.ExecuteNonQueryAsync();

            command.CommandText = "SELECT LAST_INSERT_ID()";
            int id = Convert.ToInt32(await command.ExecuteScalarAsync());

            return id;
        }

        public async Task<DeviceViewModel> Find(int id)
        {
            DeviceViewModel device = new DeviceViewModel();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("SELECT id, name FROM device_ad WHERE id=@id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        await reader.ReadAsync();

                        device.Id = reader.GetInt32("id");
                        device.Name = reader.GetString("name");

                        return device;
                    }
                }
            }
        }
      
        public async Task<bool> DeleteDevice(int id)
        {
            using MySqlConnection connection = GetConnection();
            using MySqlCommand command = new MySqlCommand(
                "DELETE FROM device_ad WHERE id=@Id", connection);

            command.Parameters.AddWithValue("@Id", id);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return true;
        }

        public async Task<bool> EnterLottery(int deviceId, int userId)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "INSERT INTO device_lottery_participant " +
                "(fk_device, fk_user) VALUES (@fk_device, @fk_user)", connection);
            command.Parameters.AddWithValue("@fk_device", deviceId);
            command.Parameters.AddWithValue("@fk_user", userId);

            await command.ExecuteNonQueryAsync();
            return true;
        }
        public async Task<bool> LeaveLottery(int deviceId, int userId)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "DELETE FROM device_lottery_participant " +
                "WHERE fk_device = @fk_device AND fk_user = @fk_user ", connection);
            command.Parameters.AddWithValue("@fk_device", deviceId);
            command.Parameters.AddWithValue("@fk_user", userId);

            await command.ExecuteNonQueryAsync();
            return true;
        }

        public async Task<bool> IsUserParticipatingInLottery(int deviceId, int userId)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "SELECT id FROM device_lottery_participant " +
                "WHERE fk_device = @fk_device AND fk_user = @fk_user ", connection);
            command.Parameters.AddWithValue("@fk_device", deviceId);
            command.Parameters.AddWithValue("@fk_user", userId);


            using DbDataReader reader = await command.ExecuteReaderAsync();
            return reader.HasRows;
        }

        public async Task<List<UserModel>> GetLotteryParticipants(int deviceId)
        {
            List<UserModel> lotteryParticipants = new List<UserModel>();

            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand("SELECT user.user_id, user.name, user.surname, user.email " +
                "FROM user " +
                "JOIN device_lottery_participant ON user.user_id = device_lottery_participant.fk_user " +
                "WHERE device_lottery_participant.fk_device = @deviceId", connection);
            command.Parameters.AddWithValue("@deviceId", deviceId);

            using DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                UserModel user = new UserModel();
                user.Id = reader.GetInt32("user_id");
                user.Name = reader.GetString("name");
                user.Surname = reader.GetString("surname");
                user.Email = reader.GetString("email");

                lotteryParticipants.Add(user);
            }
            return lotteryParticipants;
        }
        public async Task<UserViewModel> GetDeviceOwnerByDeviceId(int deviceId)
        {
            UserViewModel deviceOwner = new UserViewModel();

            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            string query = "SELECT u.user_id AS UserId, u.name AS Name, u.surname AS Surname, u.email AS Email " +
                           "FROM device_ad AS i " +
                           "JOIN user AS u ON i.fk_user = u.user_id " +
                           "WHERE i.id = @deviceId";

            using MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@deviceId", deviceId);

            using (DbDataReader reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    deviceOwner.Id = reader.GetInt32("UserId");
                    deviceOwner.Name = reader.GetString("Name");
                    deviceOwner.Surname = reader.GetString("Surname");
                    deviceOwner.Email = reader.GetString("Email");
                }
            }

            return deviceOwner;
        }

        public async Task<List<DeviceLotteryViewModel>> GetDueLotteries()
        {
            List<DeviceLotteryViewModel> lotteriesList = new List<DeviceLotteryViewModel>();
            DateTime dateTimeNow = DateTime.Now;

            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using (MySqlCommand command = new MySqlCommand("SELECT device_ad.id, device_ad.fk_user AS UserId, device_ad.name AS Name, device_ad.description AS Description, COUNT(device_lottery_participant.id) " +
                "AS Participants, device_ad.location AS Location, device_category.name AS Category " +
                "FROM device_ad " +
                "JOIN device_category ON device_ad.fk_category = device_category.id " +
                "LEFT JOIN device_lottery_participant ON device_ad.id = device_lottery_participant.fk_device " +
                "WHERE device_ad.winner_draw_datetime <= @dateTimeNow AND device_ad.fk_status = 1 AND device_ad.fk_type = 1 " +
                "GROUP BY device_ad.id, device_ad.fk_user, device_ad.name, device_ad.description, device_ad.location, device_category.name", connection))
            {
                command.Parameters.AddWithValue("@dateTimeNow", dateTimeNow);

                using DbDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    DeviceLotteryViewModel device = new DeviceLotteryViewModel();
                    device.Id = reader.GetInt32("id");
                    device.UserId = reader.GetInt32("UserId");
                    device.Name = reader.GetString("Name");
                    device.Description = reader.GetString("Description");
                    device.Participants = reader.GetInt32("Participants");
                    device.Location = reader.GetString("Location");
                    device.Category = reader.GetString("Category");

                    lotteriesList.Add(device);
                }
            }
            return lotteriesList;
        }

        public async Task<int> DrawLotteryWinner(int deviceId)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "SELECT fk_user FROM device_lottery_participant " +
                "WHERE fk_device = @fk_device " +
                "ORDER BY RAND() " +
                "LIMIT 1", connection);
            command.Parameters.AddWithValue("@fk_device", deviceId);

            using DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                return reader.GetInt32("fk_user");
            }
            throw new Exception("Failed to draw lottery winner from database!");
        }

        public async Task<bool> UpdateDeviceStatus(int deviceId, int newStatusId)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "UPDATE device_ad " +
                "SET fk_status = @fk_status " +
                "WHERE id = @id", connection);
            command.Parameters.AddWithValue("@fk_status", newStatusId);
            command.Parameters.AddWithValue("@id", deviceId);

            await command.ExecuteNonQueryAsync();
            return true;
        }

        public async Task<bool> SetDeviceWinner(int deviceId, int winnerId)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "UPDATE device_ad " +
                "SET fk_winner = @fk_winner " +
                "WHERE id = @id", connection);
            command.Parameters.AddWithValue("@fk_winner", winnerId);
            command.Parameters.AddWithValue("@id", deviceId);

            await command.ExecuteNonQueryAsync();
            return true;
        }

        public async Task<bool> SetExchangeWinners(int deviceId, int winnerId, int posterUserId, int userDeviceId)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command1 = new MySqlCommand(
                "UPDATE device_ad " +
                "SET fk_winner = @fk_winner, fk_status = 2 " +
                "WHERE id = @id", connection);
            command1.Parameters.AddWithValue("@fk_winner", winnerId);
            command1.Parameters.AddWithValue("@id", deviceId);

            using MySqlCommand command2 = new MySqlCommand(
                "UPDATE device_ad " +
                "SET fk_winner = @fk_winner, fk_status = 2 " +
                "WHERE id = @id", connection);
            command2.Parameters.AddWithValue("@fk_winner", posterUserId);
            command2.Parameters.AddWithValue("@id", userDeviceId);

            await command1.ExecuteNonQueryAsync();
            await command2.ExecuteNonQueryAsync();
            return true;
        }


        public async Task<string> GetDeviceName(int deviceId)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(
                    "SELECT name " +
                    "FROM device_ad " +
                    "WHERE id = @deviceId ", connection))
                {
                    command.Parameters.AddWithValue("@deviceId", deviceId);
                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        await reader.ReadAsync();

                        string name = reader["name"].ToString();

                        return name;
                    }
                }
            }
        }

        public async Task<List<DeviceViewModel>> Search(string searchWord)
        {
            List<DeviceViewModel> foundDevices = new List<DeviceViewModel>();
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
               "SELECT id, name, description, fk_user, fk_status, winner_draw_datetime FROM device_ad " +
                "WHERE (name LIKE CONCAT('%', @searchWord, '%') OR description LIKE CONCAT('%', @searchWord, '%')) " +
                "AND fk_status = 1", connection);
            command.Parameters.AddWithValue("@searchWord", searchWord);

            using DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                DeviceViewModel Device = new()
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    Description = reader.GetString("description"),
                    UserId = reader.GetInt32("fk_user"),
                    Images = await _imageRepo.GetByDeviceFirst(reader.GetInt32("id")),
                    WinnerDrawDateTime = reader.GetDateTime("winner_draw_datetime")
                };
                foundDevices.Add(Device);
            }
            return foundDevices;
        }

        public async Task<List<ExchangeViewModel>> GetOffers(int deviceId)
        {
            List<ExchangeViewModel> results = new List<ExchangeViewModel>();

            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();


            using MySqlCommand command = new MySqlCommand(
                "SELECT e.fk_offered_device, e.offer_message, i.name, i.description, i.location, i.winner_draw_datetime, CONCAT(u.name, ' ', u.surname) AS user " +
                "FROM device_exchange_offer AS e " +
                "INNER JOIN device_ad AS i ON i.id = e.fk_offered_device " +
        "JOIN user AS u ON i.fk_user = u.user_id " +
                "WHERE e.fk_main_device = @deviceId ",
                connection);

            command.Parameters.AddWithValue("@deviceId", deviceId);


            using (DbDataReader reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    ExchangeViewModel result = new ExchangeViewModel
                    {
                        Id = reader.GetInt32("fk_offered_device"),
                        Message = reader.GetString("offer_message"),
                        Name = reader.GetString("name"),
                        Description = reader.GetString("description"),
                        Location = reader.GetString("location"),
                        WinnerDrawDateTime = reader.GetDateTime("winner_draw_datetime"),
                        Images = await _imageRepo.GetByDevice(Convert.ToInt32(reader["fk_offered_device"])),
                        User = reader.GetString("user"),
                    };
                    results.Add(result);
                }
            }

            return results;

        }

        public async Task<bool> SubmitExchangeOffer(int deviceId, ExchangeOfferModel offer)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                    "INSERT INTO device_exchange_offer (fk_main_device, fk_offered_device, offer_message) VALUES (@fk_main_device, @fk_offered_device, @offer_message)", connection);

            // Add parameters
            command.Parameters.AddWithValue("@fk_main_device", deviceId);
            command.Parameters.AddWithValue("@fk_offered_device", offer.SelectedDevice);
            command.Parameters.AddWithValue("@offer_message", offer.Message);

            await command.ExecuteNonQueryAsync();
            return true;
        }

        public async Task<bool> InsertLetter(int deviceId, MotivationalLetterModel letter, int userId)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "INSERT INTO motivational_letter (letter, fk_user, fk_device) VALUES (@letter, @fk_user, @fk_device)", connection);

            // Add parameters
            command.Parameters.AddWithValue("@letter", letter.Letter);
            command.Parameters.AddWithValue("@fk_user", userId);
            command.Parameters.AddWithValue("@fk_device", deviceId);

            await command.ExecuteNonQueryAsync();

            return true;
        }

        public async Task<bool> HasSubmittedLetter(int deviceId, int userId)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "SELECT COUNT(*) FROM motivational_letter WHERE fk_device = @deviceId AND fk_user = @userId", connection);

            command.Parameters.AddWithValue("@deviceId", deviceId);
            command.Parameters.AddWithValue("@userId", userId);

            int count = Convert.ToInt32(await command.ExecuteScalarAsync());

            return count > 0;
        }

        public async Task<bool> HasSubmittedAnswers(int deviceId, int userId)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "SELECT COUNT(*) FROM answer WHERE fk_device = @deviceId AND fk_user = @userId", connection);

            command.Parameters.AddWithValue("@deviceId", deviceId);
            command.Parameters.AddWithValue("@userId", userId);

            int count = Convert.ToInt32(await command.ExecuteScalarAsync());

            return count > 0;
        }

        public async Task<Dictionary<string, List<MotivationalLetterViewModel>>> GetLetters(int deviceId)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "SELECT l.id AS id, l.letter, CONCAT(u.name, ' ', u.surname) AS user " +
                "FROM motivational_letter AS l " +
                "INNER JOIN user AS u ON l.fk_user = u.user_id " +
                "WHERE l.fk_device=@deviceId", connection);
            command.Parameters.AddWithValue("@deviceId", deviceId);

            using DbDataReader reader = await command.ExecuteReaderAsync();

            List<MotivationalLetterViewModel> results = new List<MotivationalLetterViewModel>();

            while (await reader.ReadAsync())
            {
                int id = reader.GetInt32("id");
                string letter = reader.GetString("letter");
                string user = reader.GetString("user");
                MotivationalLetterViewModel result = new MotivationalLetterViewModel { Id = id, Letter = letter, User = user };
                results.Add(result);
            }

            Dictionary<string, List<MotivationalLetterViewModel>> groupedResults = results.GroupBy(r => r.User)
                .ToDictionary(g => g.Key, g => g.ToList());

            return groupedResults;
        }

        public async Task<bool> InsertQuestions(DeviceModel device)
        {
            try
            {
                using MySqlConnection connection = GetConnection();
                await connection.OpenAsync();

                foreach (string question in device.Questions)
                {
                    using MySqlCommand command = new MySqlCommand(
                        "INSERT INTO question (question, fk_device) VALUES (@question, @fk_device)", connection);

                    // Add parameters
                    command.Parameters.AddWithValue("@question", question);
                    command.Parameters.AddWithValue("@fk_device", device.Id);

                    await command.ExecuteNonQueryAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error saving question to database!");
                return false;
            }
        }

        public async Task<List<DeviceQuestionViewModel>> GetQuestions(int deviceId)
        {
            List<DeviceQuestionViewModel> questions = new List<DeviceQuestionViewModel>();
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "SELECT question, id FROM question where fk_device=@deviceId", connection);
            command.Parameters.AddWithValue("@deviceId", deviceId);

            using DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                int id = reader.GetInt32("id");
                string text = reader.GetString("question");
                DeviceQuestionViewModel question = new DeviceQuestionViewModel { Id = id, Question = text };
                questions.Add(question);
            }

            return questions;
        }

        public async Task<List<AnswerModel>> GetAnswers(int deviceId)
        {
            List<AnswerModel> answers = new List<AnswerModel>();
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "SELECT id, answer FROM answer WHERE fk_device = @deviceId", connection);
            command.Parameters.AddWithValue("@deviceId", deviceId);

            using DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                string answer_text= reader.GetString("answer");
                AnswerModel answer = new AnswerModel { Text = answer_text };
                answers.Add(answer);
            }

            return answers;
        }


        public async Task<Dictionary<string, List<QuestionnaireViewModel>>> GetQuestionsAndAnswers(int deviceId)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "SELECT a.id AS id, a.answer, q.question, CONCAT(u.name, ' ', u.surname) AS user " +
                "FROM answer AS a " +
                "INNER JOIN question AS q ON a.fk_question = q.id " +
                "INNER JOIN user AS u ON a.fk_user = u.user_id " +
                "WHERE a.fk_device=@deviceId", connection);
            command.Parameters.AddWithValue("@deviceId", deviceId);

            using DbDataReader reader = await command.ExecuteReaderAsync();

            List<QuestionnaireViewModel> results = new List<QuestionnaireViewModel>();
            while (await reader.ReadAsync())
            {
                int id = reader.GetInt32("id");
                string text = reader.GetString("question");
                string answer = reader.GetString("answer");
                string user = reader.GetString("user");
                QuestionnaireViewModel result = new QuestionnaireViewModel { Id = id, Question = text, Answer = answer, User = user };
                results.Add(result);
            }

            Dictionary<string, List<QuestionnaireViewModel>> groupedResults = results.GroupBy(r => r.User)
                .ToDictionary(g => g.Key, g => g.ToList());

            return groupedResults;
        }


        public async Task<bool> InsertAnswers(int deviceId, List<AnswerModel> answers, int userId)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            foreach (AnswerModel answer in answers)
            {
                using MySqlCommand command = new MySqlCommand(
                    "INSERT INTO answer (answer, fk_question, fk_user, fk_device) VALUES (@answer, @fk_question, @fk_user, @fk_device)", connection);

                // Add parameters
                command.Parameters.AddWithValue("@answer", answer.Text);
                command.Parameters.AddWithValue("@fk_question", answer.Question);
                command.Parameters.AddWithValue("@fk_user", userId);
                command.Parameters.AddWithValue("@fk_device", deviceId);

                await command.ExecuteNonQueryAsync();
            }

            return true;
        }

        public async Task<bool> Update(DeviceModel device)
        {
            using MySqlConnection connection = GetConnection();
            using MySqlCommand command = new MySqlCommand(
                "UPDATE device_ad SET name=@Name, description=@Description, fk_category=@Category, fk_type=@Type WHERE id=@Id", connection);

            command.Parameters.AddWithValue("@Id", device.Id);
            command.Parameters.AddWithValue("@Name", device.Name);
            command.Parameters.AddWithValue("@Description", device.Description);
            command.Parameters.AddWithValue("@Category", device.Category);
            command.Parameters.AddWithValue("@Type", device.Type);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return true;
        }

        public async Task<bool> DeleteQuestions(int deviceId)
        {
            try
            {
                using MySqlConnection connection = GetConnection();
                await connection.OpenAsync();

                using MySqlCommand command = new MySqlCommand(
                    "DELETE FROM question WHERE fk_device = @deviceId", connection);

                // Add parameter
                command.Parameters.AddWithValue("@deviceId", deviceId);

                await command.ExecuteNonQueryAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting question from database!");
                return false;
            }
        }


        private async Task InsertQuestions(int deviceId, List<DeviceQuestionViewModel> updatedQuestions)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            foreach (var question in updatedQuestions)
            {
                using MySqlCommand command = new MySqlCommand(
                    "INSERT INTO question (fk_device, question) VALUES (@deviceId, @questionText)", connection);
                command.Parameters.AddWithValue("@deviceId", deviceId);
                command.Parameters.AddWithValue("@questionText", question.Question);

                await command.ExecuteNonQueryAsync();
            }
        }



    }
}
