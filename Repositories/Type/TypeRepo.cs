using MySql.Data.MySqlClient;
using mainykdovanok.ViewModels.Item;
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

        public async Task<List<ItemTypeViewModel>> GetAll()
        {
            List<ItemTypeViewModel> types = new List<ItemTypeViewModel>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                using (var command = new MySqlCommand("SELECT * FROM item_type", connection))
                {
                    await connection.OpenAsync();
                    var dataTable = new DataTable();
                    using (var dataAdapter = new MySqlDataAdapter(command))
                    {
                        dataAdapter.Fill(dataTable);
                    }

                    types = (from DataRow dt in dataTable.Rows
                             select new ItemTypeViewModel()
                             {
                                 Id = Convert.ToInt32(dt["id"]),
                                 Name = dt["type"].ToString()
                             }).ToList();

                    return types;
                }
            }
        }
    }
}
