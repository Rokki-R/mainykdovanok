using mainykdovanok.ViewModels;
using MySql.Data.MySqlClient;
using Serilog;
using System.Data;

namespace mainykdovanok.Repositories.Category
{
    public class CategoryRepo
    {
        private Serilog.ILogger _logger;
        private readonly string _connectionString;

        public CategoryRepo()
        {
            CreateLogger();

            _connectionString = Environment.GetEnvironmentVariable("DATABASE_CONN_STRING");
        }

        public CategoryRepo(string connectionString)
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

        public async Task<List<DeviceCategoryViewModel>> GetAll()
        {
            List<DeviceCategoryViewModel> categories = new List<DeviceCategoryViewModel>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                using (var command = new MySqlCommand("SELECT * FROM device_category", connection))
                {
                    await connection.OpenAsync();
                    var dataTable = new DataTable();
                    using (var dataAdapter = new MySqlDataAdapter(command))
                    {
                        dataAdapter.Fill(dataTable);
                    }

                    categories = (from DataRow dt in dataTable.Rows
                                  select new DeviceCategoryViewModel()
                                  {
                                      Id = Convert.ToInt32(dt["id"]),
                                      Name = dt["name"].ToString()
                                  }).ToList();

                    return categories;
                }
            }
        }
    }
}
