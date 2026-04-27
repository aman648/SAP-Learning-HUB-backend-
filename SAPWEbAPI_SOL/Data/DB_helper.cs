using System.Data;
using MySql.Data.MySqlClient;
namespace SAPWEbAPI_SOL.Data;

public class DB_helper
{
    private string connectionString;

    public DB_helper(IConfiguration config)
    {
        connectionString = config.GetConnectionString("DefaultConnection");
        
    }

    public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string query)
    {
        return await ExecuteQueryAsync(query, parameters: null);
    }

    public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(
        string query,
        Dictionary<string, object>? parameters)
    {
        using var conn = new MySqlConnection(connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(query, conn);
        if (parameters != null)
        {
            foreach (var param in parameters)
                cmd.Parameters.AddWithValue(param.Key, param.Value);
        }
        using var reader = await cmd.ExecuteReaderAsync();

        var result = new List<Dictionary<string, object>>();

        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.GetValue(i);
            }

            result.Add(row);
        }

        return result;
    }

    public async Task<object?> ExecuteScalarAsync(string query, Dictionary<string, object>? parameters = null)
    {
        using var conn = new MySqlConnection(connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(query, conn);
        if (parameters != null)
        {
            foreach (var param in parameters)
                cmd.Parameters.AddWithValue(param.Key, param.Value);
        }

        return await cmd.ExecuteScalarAsync();
    }
    public async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters)
    {
        using var conn = new MySqlConnection(connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(query, conn);

        foreach (var param in parameters)
            cmd.Parameters.AddWithValue(param.Key, param.Value);

        return await cmd.ExecuteNonQueryAsync();
    }
    
}
