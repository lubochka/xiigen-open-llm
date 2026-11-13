using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace OssEquivalents;

// Saves and restores state machine checkpoints to SQLite.
// Each checkpoint stores the current state name and the full state dictionary as JSON.
// This is the .NET equivalent of LangGraph's MemorySaver / SqliteSaver.
public sealed class StateCheckpointer : IAsyncDisposable
{
    private readonly string _connectionString;

    public StateCheckpointer(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
        EnsureSchema();
    }

    private void EnsureSchema()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd  = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS checkpoints (
                id          INTEGER PRIMARY KEY AUTOINCREMENT,
                state_name  TEXT    NOT NULL,
                state_json  TEXT    NOT NULL,
                saved_at    TEXT    NOT NULL
            );
            """;
        cmd.ExecuteNonQuery();
    }

    // Save a checkpoint after a state transition.
    public async Task<OperationResult<bool>> SaveAsync(
        string                     stateName,
        Dictionary<string, object> stateData)
    {
        try
        {
            await using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO checkpoints (state_name, state_json, saved_at)
                VALUES (@name, @json, @ts);
                """;
            cmd.Parameters.AddWithValue("@name", stateName);
            cmd.Parameters.AddWithValue("@json", JsonSerializer.Serialize(stateData));
            cmd.Parameters.AddWithValue("@ts",   DateTimeOffset.UtcNow.ToString("O"));
            await cmd.ExecuteNonQueryAsync();
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Fail(ex.Message, "DB_ERROR");
        }
    }

    // Load the most recent checkpoint (latest saved_at).
    public async Task<OperationResult<(string StateName, Dictionary<string, object> StateData)>> LoadLatestAsync()
    {
        await using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT state_name, state_json FROM checkpoints ORDER BY id DESC LIMIT 1;";
        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return OperationResult<(string, Dictionary<string, object>)>.Fail(
                "No checkpoints saved yet.", "EMPTY");

        string name = reader.GetString(0);
        string json = reader.GetString(1);

        var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json)
                   ?? new Dictionary<string, object>();

        return OperationResult<(string, Dictionary<string, object>)>.Ok((name, data));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
