using mainykdovanok.Models;
using mainykdovanok.ViewModels;
using mainykdovanok.ViewModels.Item;
using MySql.Data.MySqlClient;
using Serilog;
using System.Data;
using System.Data.Common;

namespace mainykdovanok.Repositories.Comment
{
    public class CommentRepo
    {
        private Serilog.ILogger _logger;
        private readonly string _connectionString;

        public CommentRepo()
        {
            CreateLogger();

            _connectionString = Environment.GetEnvironmentVariable("DATABASE_CONN_STRING");
        }

        public CommentRepo(string connectionString)
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

        public async Task<List<CommentViewModel>> GetAllItemComments(int itemId)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "SELECT c.id, c.comment, c.fk_user, c.fk_item, c.date, u.name, u.surname " +
                "FROM item_comments AS c " +
                "INNER JOIN users AS u ON c.fk_user = u.user_id " +
                "WHERE c.fk_item = @itemId", connection);
            command.Parameters.AddWithValue("@itemId", itemId);

            using DbDataReader reader = await command.ExecuteReaderAsync();

            List<CommentViewModel> results = new List<CommentViewModel>();

            while (await reader.ReadAsync())
            {
                int id = reader.GetInt32("id");
                string comment = reader.GetString("comment");
                int userId = reader.GetInt32("fk_user");
                int fetchedItemId = reader.GetInt32("fk_item");
                string userName = reader.GetString("name");
                string userSurname = reader.GetString("surname");
                DateTime dateTime = Convert.ToDateTime(reader["date"]);

                CommentViewModel result = new CommentViewModel
                {
                    Id = id,
                    Comment = comment,
                    UserId = userId,
                    ItemId = fetchedItemId,
                    UserName = userName,
                    UserSurname = userSurname,
                    PostDateTime = dateTime
                };
                results.Add(result);
            }

            return results;
        }

        public async Task<bool> InsertComment(CommentModel comment)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "INSERT INTO item_comments (comment, fk_user, fk_item, date) VALUES (@comment, @fk_user, @fk_item, @date)", connection);

            // Add parameters
            command.Parameters.AddWithValue("@comment", comment.Comment);
            command.Parameters.AddWithValue("@fk_user", comment.UserId);
            command.Parameters.AddWithValue("@fk_item", comment.ItemId);
            command.Parameters.AddWithValue("@date", comment.PostedDateTime);

            await command.ExecuteNonQueryAsync();

            return true;
        }

    }
}
