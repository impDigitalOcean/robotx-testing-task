using ImportedDatabaseViewer.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ImportedDatabaseViewer.Services
{
    public class ClientRepository
    {
        private readonly string _connectionString;
        public struct PagedData
        {
            List<Client> clients;
            int total;
            int page;

            public PagedData(List<Client> clients, int total, int page)
            {
                this.clients = clients;
                this.total = total;
                this.page = page;
            }
        }
        
        public ClientRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Соединение 'DefaultConnection' не найдено."); ;
        }

        public async Task<PagedData> GetPagedAsync(int page, int pageSize = 20)
        {
            List<Client> clients = new List<Client>();
            await using var connection = new SqlConnection(_connectionString);

            await using var clientsC = new SqlCommand(@"SELECT CardCode, LastName, FirstName, SurName, PhoneMobile, Email,
            GenderId, Birthday, City, Pincode, Bonus, Turnover FROM dbo.Clients ORDER BY CardCode
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY", connection);
            clientsC.Parameters.AddWithValue("Skip", (page - 1) * pageSize);
            clientsC.Parameters.AddWithValue("Take", pageSize);

            await using var countC = new SqlCommand(@"SELECT COUNT(*) FROM dbo.Clients", connection);
            await connection.OpenAsync();

            await using var reader = await clientsC.ExecuteReaderAsync();
            while(await reader.ReadAsync())
            {
                clients.Add(MapClient(reader));
            }


            var countR = await countC.ExecuteScalarAsync();
            int count = countR != null ? Convert.ToInt32(countR) : 0;

            return new PagedData(clients, count, page);
        }

        public async Task UpdateAsync(Client client)
        {
            await using var connection = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand(@"
            UPDATE dbo.Clients SET
            LastName=@LastName, FirstName=@FirstName, SurName=@SurName,
            PhoneMobile=@PhoneMobile, Email=@Email, GenderId=@GenderId,
            Birthday=@Birthday, City=@City, Pincode=@Pincode,
            Bonus=@Bonus, Turnover=@Turnover
            WHERE CardCode=@CardCode", connection);

            cmd.Parameters.Add("@CardCode", SqlDbType.Int).Value = client.CardCode;
            cmd.Parameters.Add("@LastName", SqlDbType.NVarChar, 255).Value = client.LastName;
            cmd.Parameters.Add("@FirstName", SqlDbType.NVarChar, 255).Value = client.FirstName;
            cmd.Parameters.Add("@SurName", SqlDbType.NVarChar, 255).Value = client.SurName ?? (object)DBNull.Value;
            cmd.Parameters.Add("@PhoneMobile", SqlDbType.NVarChar, 255).Value = client.PhoneMobile;
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 300).Value = client.Email ?? (object)DBNull.Value;
            cmd.Parameters.Add("@GenderId", SqlDbType.NVarChar, 10).Value = client.GenderId;
            cmd.Parameters.Add("@Birthday", SqlDbType.Date).Value = client.Birthday;
            cmd.Parameters.Add("@City", SqlDbType.NVarChar, 255).Value = client.City ?? (object)DBNull.Value;
            cmd.Parameters.Add("@Pincode", SqlDbType.Int).Value = client.Pincode ?? (object)DBNull.Value;
            cmd.Parameters.Add("@Bonus", SqlDbType.Int).Value = client.Bonus ?? (object)DBNull.Value;
            cmd.Parameters.Add("@Turnover", SqlDbType.Int).Value = client.Turnover ?? (object)DBNull.Value;

            await connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
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
