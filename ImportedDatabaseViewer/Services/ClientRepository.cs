using ImportedDatabaseViewer.Models;
using Microsoft.Data.SqlClient;

namespace ImportedDatabaseViewer.Services
{
    public class ClientRepository
    {
        private readonly string _connectionString;
        public ClientRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Соединение 'DefaultConnection' не найдено."); ;
        }

        async public Task<List<Client>> GetAllAsync()
        {
            List<Client> clients = new List<Client>();
            using var connection = new SqlConnection(_connectionString);

            using var clientsC = new SqlCommand(@" SELECT CardCode, LastName, FirstName, SurName, PhoneMobile, Email,
            GenderId, Birthday, City, Pincode, Bonus, Turnover FROM dbo.Clients ORDER BY CardCode", connection);
            await connection.OpenAsync();
            await using var reader = await clientsC.ExecuteReaderAsync();
            while(await reader.ReadAsync())
            {
                clients.Add(MapClient(reader));
            }


            return clients;
        }

        private static Client MapClient(SqlDataReader reader)
        {
            Client client = new Client()
            {
                CardCode=reader.GetInt32(0),
                LastName=reader.GetString(1),
                FirstName=reader.GetString(2),
                SurName= reader.IsDBNull(3) ? null : reader.GetString(3),
                PhoneMobile=reader.GetString(4),
                Email= reader.IsDBNull(5) ? null : reader.GetString(5),
                GenderId=reader.GetString(6),
                Birthday=reader.GetDateTime(7),
                City=reader.IsDBNull(8) ? null : reader.GetString(8),
                Pincode=reader.IsDBNull(9) ? null : reader.GetInt32(9),
                Bonus= reader.IsDBNull(10) ? null : reader.GetInt32(10),
                Turnover= reader.IsDBNull(11) ? null : reader.GetInt32(11)
            };

            return client;
        }
    }
}
