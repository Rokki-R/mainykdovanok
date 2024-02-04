﻿using MySql.Data.MySqlClient;
using mainykdovanok.Models;
using mainykdovanok.Repositories.Image;
using mainykdovanok.ViewModels.Item;
using mainykdovanok.ViewModels.User;
using Org.BouncyCastle.Cms;
using Serilog;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace mainykdovanok.Repositories.Item
{
    public class ItemRepo
    {
        private Serilog.ILogger _logger;
        private readonly string _connectionString;
        private readonly ImageRepo _imageRepo;

        public ItemRepo()
        {
            CreateLogger();

            _connectionString = Environment.GetEnvironmentVariable("DATABASE_CONN_STRING");
            _imageRepo = new ImageRepo();
        }

        public ItemRepo(string connectionString)
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

        public async Task<List<ItemViewModel>> GetAll()
        {
            var items = new List<ItemViewModel>();

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand("SELECT items.id, items.name, items.description, " +
                "items.fk_user, items.location, items.end_datetime, item_type.type as type " +
                "FROM items " +
                "LEFT JOIN item_type ON items.fk_type = item_type.id " +
                "WHERE items.fk_status = 1", connection))
                {
                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var item = new ItemViewModel()
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                UserId = Convert.ToInt32(reader["fk_user"]),
                                Name = reader["name"].ToString(),
                                Description = reader["description"].ToString(),
                                Type = reader["type"].ToString(),
                                Location = reader["location"].ToString(),
                                EndDateTime = Convert.ToDateTime(reader["end_datetime"])
                            };

                            item.Images = await _imageRepo.GetByItemFirst(item.Id);

                            items.Add(item);
                        }
                    }
                }
            }

            return items;
        }

        public async Task<ItemViewModel> GetFullById(int itemId)
        {
            var item = new ItemViewModel();

            var images = await _imageRepo.GetByItem(itemId);
            var questions = await GetQuestions(itemId);
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand("SELECT items.*, item_type.type AS item_type, item_categories.name AS category_name, item_status.name AS status_name, " +
                    "COUNT(item_lottery_participants.id) AS participants_count " +
                    "FROM items " +
                    "JOIN item_type ON items.fk_type = item_type.id " +
                    "JOIN item_categories ON items.fk_category = item_categories.id " +
                    "JOIN item_status ON items.fk_status = item_status.id " +
                    "LEFT JOIN item_lottery_participants ON items.id = item_lottery_participants.fk_item " +
                    "WHERE items.id = @itemId " +
                    "GROUP BY items.id, item_type.type, item_categories.name", connection))
                {
                    command.Parameters.AddWithValue("@itemId", itemId);
                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        await reader.ReadAsync();

                        item.Id = Convert.ToInt32(reader["id"]);
                        item.UserId = Convert.ToInt32(reader["fk_user"]);
                        item.Name = reader["name"].ToString();
                        item.Description = reader["description"].ToString();
                        item.Status = reader["status_name"].ToString();
                        item.Type = reader["item_type"].ToString();
                        item.Participants = Convert.ToInt32(reader["participants_count"]);
                        item.Location = reader["location"].ToString();
                        item.Category = reader["category_name"].ToString();
                        item.CreationDateTime = Convert.ToDateTime(reader["creation_datetime"]);
                        item.EndDateTime = Convert.ToDateTime(reader["end_datetime"]);
                        item.WinnerId = reader["fk_winner"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["fk_winner"]);
                        item.Images = images;
                        item.Questions = questions;

                        return item;
                    }
                }
            }
        }

        public async Task<List<ItemViewModel>> GetAllByUser(int userId)
        {
            List<ItemViewModel> items = new List<ItemViewModel>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                using (MySqlCommand command = new MySqlCommand("SELECT items.*, item_type.type AS item_type, categories.name AS category_name, status.name AS status_name, " +
                    "COUNT(item_lottery_participants.id) AS participants_count " +
                    "FROM items " +
                    "JOIN item_type ON items.fk_type = item_type.id " +
                    "JOIN categories ON items.fk_category = categories.id " +
                    "JOIN status ON items.fk_status = status.id " +
                    "LEFT JOIN item_lottery_participants ON items.id = item_lottery_participants.fk_item " +
                    "WHERE items.fk_user = @userId " +
                    "GROUP BY items.id, item_type.type, categories.name ", connection))
                {
                    await connection.OpenAsync();
                    command.Parameters.AddWithValue("@userId", userId);

                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var item = new ItemViewModel()
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                UserId = Convert.ToInt32(reader["fk_user"]),
                                Name = reader["name"].ToString(),
                                Description = reader["description"].ToString(),
                                Status = reader["status_name"].ToString(),
                                Type = reader["item_type"].ToString(),
                                Participants = Convert.ToInt32(reader["participants_count"]),
                                Location = reader["location"].ToString(),
                                Category = reader["category_name"].ToString(),
                                CreationDateTime = Convert.ToDateTime(reader["creation_datetime"]),
                                EndDateTime = Convert.ToDateTime(reader["end_datetime"]),
                                WinnerId = reader["fk_winner"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["fk_winner"]),
                                Images = await _imageRepo.GetByItem(Convert.ToInt32(reader["id"])),
                                Questions = await GetQuestions(Convert.ToInt32(reader["id"]))
                            };

                            items.Add(item);
                        }
                    }

                    return items;
                }
            }
        }

        public async Task<List<ItemViewModel>> GetAllByCategory(int categoryId)
        {
            List<ItemViewModel> items = new List<ItemViewModel>();


            using (var connection = new MySqlConnection(_connectionString))
            {
                using (MySqlCommand command = new MySqlCommand("SELECT items.*, item_type.type AS item_type, " +
                    "item_categories.name AS category_name, item_status.name AS status_name, " +
                    "COUNT(item_lottery_participants.id) AS participants_count " +
                    "FROM items " +
                    "JOIN item_type ON items.fk_type = item_type.id " +
                    "JOIN item_categories ON items.fk_category = categories.id " +
                    "JOIN item_status ON items.fk_status = status.id " +
                    "LEFT JOIN item_lottery_participants ON items.id = ad_lottery_participants.fk_item " +
                    "WHERE items.fk_category = @categoryId AND items.fk_status = 1  " +
                    "GROUP BY items.id, item_type.type, item_categories.name", connection))
                {
                    await connection.OpenAsync();
                    command.Parameters.AddWithValue("@categoryId", categoryId);

                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var item = new ItemViewModel()
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                UserId = Convert.ToInt32(reader["fk_user"]),
                                Name = reader["name"].ToString(),
                                Description = reader["description"].ToString(),
                                Status = reader["status_name"].ToString(),
                                Type = reader["item_type"].ToString(),
                                Participants = Convert.ToInt32(reader["participants_count"]),
                                Location = reader["location"].ToString(),
                                Category = reader["category_name"].ToString(),
                                CreationDateTime = Convert.ToDateTime(reader["creation_datetime"]),
                                EndDateTime = Convert.ToDateTime(reader["end_datetime"]),
                                Images = await _imageRepo.GetByItem(Convert.ToInt32(reader["id"])),
                                Questions = await GetQuestions(Convert.ToInt32(reader["id"]))
                            };

                            items.Add(item);
                        }
                    }

                    return items;
                }
            }
        }

        public async Task<int> Create(ItemModel item)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "INSERT INTO items (name, description, location, fk_category, fk_user, fk_status, fk_type, end_datetime) " +
                "VALUES (@Name, @Description, @Location, @Category, @User, @Status, @Type, @EndDate)", connection);

            command.Parameters.AddWithValue("@Name", item.Name);
            command.Parameters.AddWithValue("@Description", item.Description);
            command.Parameters.AddWithValue("@Location", item.Location);
            command.Parameters.AddWithValue("@Category", item.Category);
            command.Parameters.AddWithValue("@User", item.User);
            command.Parameters.AddWithValue("@Status", item.Status);
            command.Parameters.AddWithValue("@Type", item.Type);
            command.Parameters.AddWithValue("@EndDate", item.EndDate);

            await command.ExecuteNonQueryAsync();

            command.CommandText = "SELECT LAST_INSERT_ID()";
            int id = Convert.ToInt32(await command.ExecuteScalarAsync());

            return id;
        }

        public async Task<ItemViewModel> Find(int id)
        {
            ItemViewModel item = new ItemViewModel();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("SELECT id, fk_type FROM items WHERE id=@id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        await reader.ReadAsync();

                        item.Id = reader.GetInt32("id");
                        item.Name = reader.GetString("fk_type");

                        return item;
                    }
                }
            }
        }
        public async Task<List<ItemQuestionsViewModel>> GetQuestions(int itemId)
        {
            List<ItemQuestionsViewModel> questions = new List<ItemQuestionsViewModel>();
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "SELECT question, id FROM questions where fk_item=@itemId", connection);
            command.Parameters.AddWithValue("@itemId", itemId);

            using DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                int id = reader.GetInt32("id");
                string text = reader.GetString("question");
                ItemQuestionsViewModel question = new ItemQuestionsViewModel { Id = id, Question = text };
                questions.Add(question);
            }

            return questions;
        }
        public async Task<bool> InsertQuestions(ItemModel item)
        {
            try
            {
                using MySqlConnection connection = GetConnection();
                await connection.OpenAsync();

                foreach (string question in item.Questions)
                {
                    using MySqlCommand command = new MySqlCommand(
                        "INSERT INTO questions (question, fk_item) VALUES (@question, @fk_item)", connection);

                    // Add parameters
                    command.Parameters.AddWithValue("@question", question);
                    command.Parameters.AddWithValue("@fk_item", item.Id);

                    await command.ExecuteNonQueryAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error saving questions to database!");
                return false;
            }
        }
        public async Task<bool> DeleteItem(int id)
        {
            using MySqlConnection connection = GetConnection();
            using MySqlCommand command = new MySqlCommand(
                "DELETE FROM items WHERE id=@Id", connection);

            command.Parameters.AddWithValue("@Id", id);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return true;
        }

        public async Task<bool> EnterLottery(int itemId, int userId)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "INSERT INTO item_lottery_participants " +
                "(fk_item, fk_user) VALUES (@fk_item, @fk_user)", connection);
            command.Parameters.AddWithValue("@fk_item", itemId);
            command.Parameters.AddWithValue("@fk_user", userId);

            await command.ExecuteNonQueryAsync();
            return true;
        }
        public async Task<bool> LeaveLottery(int itemId, int userId)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "DELETE FROM item_lottery_participants " +
                "WHERE fk_item = @fk_item AND fk_user = @fk_user ", connection);
            command.Parameters.AddWithValue("@fk_item", itemId);
            command.Parameters.AddWithValue("@fk_user", userId);

            await command.ExecuteNonQueryAsync();
            return true;
        }

        public async Task<bool> IsUserParticipatingInLottery(int itemId, int userId)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "SELECT id FROM item_lottery_participants " +
                "WHERE fk_item = @fk_item AND fk_user = @fk_user ", connection);
            command.Parameters.AddWithValue("@fk_item", itemId);
            command.Parameters.AddWithValue("@fk_user", userId);


            using DbDataReader reader = await command.ExecuteReaderAsync();
            return reader.HasRows;
        }

        public async Task<List<UserViewModel>> GetLotteryParticipants(int itemId)
        {
            List<UserViewModel> lotteryParticipants = new List<UserViewModel>();

            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand("SELECT users.user_id, users.name, users.surname, users.email " +
                "FROM users " +
                "JOIN item_lottery_participants ON users.user_id = item_lottery_participants.fk_user " +
                "WHERE item_lottery_participants.fk_item = @itemId", connection);
            command.Parameters.AddWithValue("@itemId", itemId);

            using DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                UserViewModel user = new UserViewModel();
                user.Id = reader.GetInt32("user_id");
                user.Name = reader.GetString("name");
                user.Surname = reader.GetString("surname");
                user.Email = reader.GetString("email");

                lotteryParticipants.Add(user);
            }
            return lotteryParticipants;
        }


        public async Task<List<ItemLotteryViewModel>> GetDueLotteries()
        {
            List<ItemLotteryViewModel> lotteriesList = new List<ItemLotteryViewModel>();
            DateTime dateTimeNow = DateTime.Now;

            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using (MySqlCommand command = new MySqlCommand("SELECT items.id, items.fk_user AS UserId, items.name AS Name, items.description AS Description, COUNT(item_lottery_participants.id) " +
                "AS Participants, items.location AS Location, item_categories.name AS Category " +
                "FROM items" +
                "JOIN item_categories ON items.fk_category = item_categories.id " +
                "LEFT JOIN item_lottery_participants ON items.id = item_lottery_participants.fk_item " +
                "WHERE items.end_datetime <= @dateTimeNow AND items.fk_status = 1 AND items.fk_type = 1 " +
                "GROUP BY items.id, items.fk_user, items.name, items.description, items.location, item_categories.name", connection))
            {
                command.Parameters.AddWithValue("@dateTimeNow", dateTimeNow);

                using DbDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    ItemLotteryViewModel item = new ItemLotteryViewModel();
                    item.Id = reader.GetInt32("id");
                    item.UserId = reader.GetInt32("UserId");
                    item.Name = reader.GetString("Name");
                    item.Description = reader.GetString("Description");
                    item.Participants = reader.GetInt32("Participants");
                    item.Location = reader.GetString("Location");
                    item.Category = reader.GetString("Category");

                    lotteriesList.Add(item);
                }
            }
            return lotteriesList;
        }

        public async Task<List<ItemViewModel>> GetPastEndDateItems()
        {
            List<ItemViewModel> items = new List<ItemViewModel>();

            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "SELECT items.id, items.name, items.description, items.location, items.end_datetime, items.fk_status, items.fk_user, items.fk_winner, item_status.name AS status_name " +
                "FROM items " +
                "JOIN item_status ON items.fk_status = status.id " +
                "WHERE end_datetime < NOW() AND fk_status = 1", connection);

            using (DbDataReader reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    ItemViewModel item = new ItemViewModel
                    {
                        Id = reader.GetInt32("id"),
                        Name = reader.GetString("name"),
                        Description = reader.GetString("description"),
                        Location = reader.GetString("location"),
                        EndDateTime = reader.GetDateTime("end_datetime"),
                        Status = reader.GetString("status_name"),
                        UserId = reader.GetInt32("fk_user")
                    };
                    items.Add(item);
                }
            }
            return items;
        }

        public async Task<int> DrawLotteryWinner(int itemId)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "SELECT fk_user FROM item_lottery_participants " +
                "WHERE fk_item = @fk_item " +
                "ORDER BY RAND() " +
                "LIMIT 1", connection);
            command.Parameters.AddWithValue("@fk_item", itemId);

            using DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                return reader.GetInt32("fk_user");
            }
            throw new Exception("Failed to draw lottery winner from database!");
        }

        public async Task<bool> UpdateItemStatus(int itemId, int newStatusId)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "UPDATE items " +
                "SET fk_status = @fk_status " +
                "WHERE id = @id", connection);
            command.Parameters.AddWithValue("@fk_status", newStatusId);
            command.Parameters.AddWithValue("@id", itemId);

            await command.ExecuteNonQueryAsync();
            return true;
        }

        public async Task<bool> SetItemWinner(int itemId, int winnerId)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "UPDATE items " +
                "SET fk_winner = @fk_winner " +
                "WHERE id = @id", connection);
            command.Parameters.AddWithValue("@fk_winner", winnerId);
            command.Parameters.AddWithValue("@id", itemId);

            await command.ExecuteNonQueryAsync();
            return true;
        }
    }
}
