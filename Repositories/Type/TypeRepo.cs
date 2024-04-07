using MySql.Data.MySqlClient;
using mainykdovanok.ViewModels.Device;
using Serilog;
using System.Data;

namespace mainykdovanok.Repositories.Type
{
    public class TypeRepo
    {
        private Serilog.ILogger _logger;
        private readonly string _connectionString;

        public TypeRepo()
        {
            CreateLogger();

            _connectionString = Environment.GetEnvironmentVariable("DATABASE_CONN_STRING");
        }

        public TypeRepo(string connectionString)
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

        public async Task<List<DeviceTypeViewModel>> GetAll()
        {
            List<DeviceTypeViewModel> types = new List<DeviceTypeViewModel>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                using (var command = new MySqlCommand("SELECT * FROM device_type", connection))
                {
                    await connection.OpenAsync();
                    var dataTable = new DataTable();
                    using (var dataAdapter = new MySqlDataAdapter(command))
                    {
                        dataAdapter.Fill(dataTable);
                    }

                    types = (from DataRow dt in dataTable.Rows
                             select new DeviceTypeViewModel()
                             {
                                 Id = Convert.ToInt32(dt["id"]),
                                 Name = dt["type"].ToString()
                             }).ToList();

                    return types;
                }
            }
        }

        public async Task<DeviceTypeViewModel> GetType(int id)
        {
            DeviceTypeViewModel type = null;

            using (var connection = new MySqlConnection(_connectionString))
            {
                using (var command = new MySqlCommand("SELECT * FROM device_type WHERE id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    await connection.OpenAsync();
                    var reader = await command.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        type = new DeviceTypeViewModel
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Name = reader["type"].ToString()
                        };
                    }
                }
            }

            return type;
        }

    }
}
