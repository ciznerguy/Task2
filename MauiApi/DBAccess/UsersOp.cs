using Microsoft.Data.Sqlite;
using MauiModels.Models;

namespace MauiApi.DBAccess
{
    public class UserDataAccess
    {
        private readonly string _connectionString = "Data Source=Database/users.db";

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var users = new List<User>();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var sql = "SELECT * FROM MyUsers WHERE Role = 0";
            using var command = new SqliteCommand(sql, connection);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                users.Add(MapReaderToUser(reader));
            }

            return users;
        }

        public async Task<User?> GetUserByCredentialsAsync(string username, string password)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var sql = "SELECT * FROM MyUsers WHERE Username = @Username AND Password = @Password";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Password", password);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToUser(reader);
            }

            return null; // לא נמצא
        }

        public async Task AddUserAsync(User user)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                INSERT INTO MyUsers (FullName, Username, Password, Phone, Email, BirthDate, Role)
                VALUES (@FullName, @Username, @Password, @Phone, @Email, @BirthDate, @Role)";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@FullName", user.FullName);
            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@Password", user.Password);
            command.Parameters.AddWithValue("@Phone", user.Phone);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@BirthDate", user.BirthDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@Role", user.Role);

            await command.ExecuteNonQueryAsync();

            command.CommandText = "SELECT last_insert_rowid();";
            user.Id = Convert.ToInt32((long)(await command.ExecuteScalarAsync()));
        }

        public async Task DeleteUserAsync(int userId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var sql = "DELETE FROM MyUsers WHERE Id = @Id";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", userId);

            await command.ExecuteNonQueryAsync();
        }

        private User MapReaderToUser(SqliteDataReader reader)
        {
            return new User
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                Password = reader.GetString(reader.GetOrdinal("Password")),
                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                BirthDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("BirthDate"))),
                Role = reader.GetInt32(reader.GetOrdinal("Role"))
            };
        }
    }
}
