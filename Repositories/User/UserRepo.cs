using System.Data;
using System.Data.Common;
using System.Reflection;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Cms;
using Serilog;
using mainykdovanok.Models;
using mainykdovanok.ViewModels.User;
using System.Xml.Linq;

namespace mainykdovanok.Repositories.User
{
    public class UserRepo
    {
        private Serilog.ILogger _logger;
        private readonly string _connectionString;
        public UserRepo()
        {
            CreateLogger();

            _connectionString = Environment.GetEnvironmentVariable("DATABASE_CONN_STRING");
        }

        public UserRepo(string connectionString)
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

        /// <summary>
        /// Returns data as a List of objects.
        /// </summary>
        /// <typeparam name="T">The type of the objects to return</typeparam>
        /// <typeparam name="U">The type of the parameters used in the SQL query</typeparam>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="parameters">The parameters used in the SQL query</param>
        /// <returns>List of objects of type T</returns>
        public async Task<List<T>> LoadData<T, U>(string sql, U parameters)
        {
            using MySqlConnection connection = GetConnection();
            using MySqlCommand command = new MySqlCommand(sql, connection);
            AddParameters(command, parameters);

            await connection.OpenAsync();
            try
            {
                using MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

                List<T> result = new List<T>();

                while (await reader.ReadAsync())
                {
                    T row = Activator.CreateInstance<T>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        PropertyInfo property = row.GetType().GetProperty(reader.GetName(i));
                        if (property != null && !reader.IsDBNull(i))
                        {
                            property.SetValue(row, reader.GetValue(i), null);
                        }
                    }
                    result.Add(row);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading data from database!");
                return null;
            }
        }

        /// <summary>
        /// Returns query results as a DataTable.
        /// </summary>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="parameters">List of parameters to append to the query</param>
        /// <returns>DataTable containing the query results</returns>
        public async Task<DataTable> LoadData<U>(string sql, U parameters)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        AddParameters(command, parameters);

                        await connection.OpenAsync();
                        var dataTable = new DataTable();
                        using (var dataAdapter = new MySqlDataAdapter(command))
                        {
                            dataAdapter.Fill(dataTable);
                        }
                        return dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading data from database!");
                return null;
            }
        }

        /// <summary>
        /// Executes a query that modifies data in the database.
        /// </summary>
        /// <typeparam name="U">Type of parameter object</typeparam>
        /// <param name="sql">SQL statement to execute</param>
        /// <param name="parameters">Object containing parameter values</param>
        public async Task<bool> SaveData<U>(string sql, U parameters)
        {
            try
            {
                using MySqlConnection connection = GetConnection();
                using MySqlCommand command = new MySqlCommand(sql, connection);
                AddParameters(command, parameters);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error saving data to database!");
                return false;
            }
        }

        /// <summary>
        /// Adds parameters to a database command object.
        /// </summary>
        /// <typeparam name="U">Type of parameter object</typeparam>
        /// <param name="command">Command object to add parameters to</param>
        /// <param name="parameters">Object containing parameter values</param>
        private void AddParameters<U>(IDbCommand command, U parameters)
        {
            if (parameters != null)
            {
                foreach (PropertyInfo property in parameters.GetType().GetProperties())
                {
                    IDbDataParameter parameter = command.CreateParameter();
                    parameter.ParameterName = property.Name;
                    parameter.Value = property.GetValue(parameters) ?? DBNull.Value;
                    command.Parameters.Add(parameter);
                }
            }
        }
        public async Task<string> GetUserEmail(int userId)
        {
            try
            {
                MySqlConnection connection = GetConnection();
                await connection.OpenAsync();

                using MySqlCommand command = new MySqlCommand(
                    "SELECT email FROM user " +
                    "WHERE user_id = @id", connection);
                command.Parameters.AddWithValue("@id", userId);

                using DbDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    return reader.GetString("email");
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting user email!");
                return null;
            }
        }

        public async Task<bool> IncrementUserQuantityOfDevicesWon(int? userId)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "UPDATE user " +
                "SET devices_won = devices_won + 1 " +
                "WHERE user_id = @id", connection);
            command.Parameters.AddWithValue("@id", userId);

            await command.ExecuteNonQueryAsync();
            return true;
        }

        public async Task<bool> IncrementUserQuantityOfDevicesGifted(int userId)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "UPDATE user " +
                "SET devices_gifted = devices_gifted + 1 " +
                "WHERE user_id = @id", connection);
            command.Parameters.AddWithValue("@id", userId);

            await command.ExecuteNonQueryAsync();
            return true;
        }

        public async Task<UserModel> GetUser(string name)
        {
            MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "SELECT user_id, name, surname, email FROM user " +
                "WHERE CONCAT(name, ' ', surname) = @name", connection);
            command.Parameters.AddWithValue("@name", name);

            using (DbDataReader reader = await command.ExecuteReaderAsync())
            {
                await reader.ReadAsync();

                UserModel user = new UserModel()
                {
                    Id = Convert.ToInt32(reader["user_id"]),
                    Name = reader["name"].ToString(),
                    Surname = reader["surname"].ToString(),
                    Email = reader["email"].ToString()
                };

                return user;
            }
        }

        public async Task<UserViewModel> GetUserById(int userId)
        {
            MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "SELECT user_id, name, surname, email, devices_gifted, devices_won FROM user " +
                "WHERE user_id = @id", connection);
            command.Parameters.AddWithValue("@id", userId);

            using (DbDataReader reader = await command.ExecuteReaderAsync())
            {
                await reader.ReadAsync();

                UserViewModel user = new UserViewModel()
                {
                    Id = Convert.ToInt32(reader["user_id"]),
                    Name = reader["name"].ToString(),
                    Surname = reader["surname"].ToString(),
                    Email = reader["email"].ToString(),
                    devicesGifted = Convert.ToInt32(reader["devices_gifted"]),
                    devicesWon = Convert.ToInt32(reader["devices_won"]),

                };

                return user;
            }
        }

        public async Task<bool> CheckEmailExists(string email)
        {
            try
            {
                using MySqlConnection connection = GetConnection();
                string sql = "SELECT COUNT(*) FROM user WHERE email = @Email";
                using MySqlCommand command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Email", email);

                await connection.OpenAsync();
                int count = Convert.ToInt32(await command.ExecuteScalarAsync());

                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error checking email existence in database!");
                return false;
            }
        }

        public async Task<List<UserViewModel>> GetUsers()
        {
            var users = new List<UserViewModel>();

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand("SELECT user.user_id, user.name, user.surname, " +
                     "user.email, user.user_role, user.devices_won, user.devices_gifted, user_status.status as status " +
                     "FROM user " +
                     "LEFT JOIN user_status ON user.fk_user_status = user_status.id "
                     , connection))
                {
                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {

                            UserViewModel user = new UserViewModel()
                            {
                                Id = Convert.ToInt32(reader["user_id"]),
                                Name = reader["name"].ToString(),
                                Surname = reader["surname"].ToString(),
                                Email = reader["email"].ToString(),
                                Role = Convert.ToInt32(reader["user_role"]),
                                devicesGifted = Convert.ToInt32(reader["devices_gifted"]),
                                devicesWon = Convert.ToInt32(reader["devices_won"]),
                                Status = reader["status"].ToString()

                            };

                            users.Add(user);

                        }
                    }
                }
            }
            return users;
        }

        public async Task<bool> UpdateUserStatus(int userId, int newStatusId)
        {
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new MySqlCommand(
                "UPDATE user " +
                "SET fk_user_status = @fk_user_status " +
                "WHERE user_id = @id", connection);
            command.Parameters.AddWithValue("@fk_user_status", newStatusId);
            command.Parameters.AddWithValue("@id", userId);

            await command.ExecuteNonQueryAsync();
            return true;
        }
    }
}
