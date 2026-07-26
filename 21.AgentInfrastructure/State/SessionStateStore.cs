// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace AgentInfrastructure;

// Stage 07 — Session state persistence.
// Saves and restores full agent sessions using a SQLite database.
//
// Schema:
//   sessions  — id TEXT, task TEXT, created_at TEXT
//   messages  — session_id TEXT, role TEXT, content TEXT, seq INTEGER
public sealed class SessionStateStore : ISessionStorage
{
    private readonly string _connectionString;

    public SessionStateStore(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
        EnsureSchema();
    }

    // Create tables if they do not exist.
    private void EnsureSchema()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS sessions (
                id         TEXT PRIMARY KEY,
                task       TEXT NOT NULL,
                created_at TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS messages (
                session_id TEXT    NOT NULL,
                seq        INTEGER NOT NULL,
                role       TEXT    NOT NULL,
                content    TEXT    NOT NULL,
                PRIMARY KEY (session_id, seq)
            );
            CREATE TABLE IF NOT EXISTS todos (
                session_id   TEXT    NOT NULL,
                seq          INTEGER NOT NULL,
                description  TEXT    NOT NULL,
                is_completed INTEGER NOT NULL,
                PRIMARY KEY (session_id, seq)
            );
            """;
        cmd.ExecuteNonQuery();
    }

    public async Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
    {
        await using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        await using var tx = conn.BeginTransaction();

        try
        {
            // Upsert the session row.
            await using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = """
                    INSERT INTO sessions (id, task, created_at)
                    VALUES (@id, @task, @ts)
                    ON CONFLICT(id) DO UPDATE SET task = @task;
                    """;
                cmd.Parameters.AddWithValue("@id",   sessionId);
                cmd.Parameters.AddWithValue("@task", state.CurrentTask);
                cmd.Parameters.AddWithValue("@ts",   DateTimeOffset.UtcNow.ToString("O"));
                await cmd.ExecuteNonQueryAsync();
            }

            // Delete old messages and todos for this session.
            await using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "DELETE FROM messages WHERE session_id = @id; DELETE FROM todos WHERE session_id = @id;";
                cmd.Parameters.AddWithValue("@id", sessionId);
                await cmd.ExecuteNonQueryAsync();
            }

            // Insert messages.
            for (int i = 0; i < state.MessageHistory.Count; i++)
            {
                var msg = state.MessageHistory[i];
                await using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = "INSERT INTO messages (session_id, seq, role, content) VALUES (@sid, @seq, @role, @content);";
                cmd.Parameters.AddWithValue("@sid",     sessionId);
                cmd.Parameters.AddWithValue("@seq",     i);
                cmd.Parameters.AddWithValue("@role",    msg.Role);
                cmd.Parameters.AddWithValue("@content", msg.Content);
                await cmd.ExecuteNonQueryAsync();
            }

            // Insert todos.
            for (int i = 0; i < state.TodoList.Count; i++)
            {
                var todo = state.TodoList[i];
                await using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = "INSERT INTO todos (session_id, seq, description, is_completed) VALUES (@sid, @seq, @desc, @done);";
                cmd.Parameters.AddWithValue("@sid",  sessionId);
                cmd.Parameters.AddWithValue("@seq",  i);
                cmd.Parameters.AddWithValue("@desc", todo.Description);
                cmd.Parameters.AddWithValue("@done", todo.IsCompleted ? 1 : 0);
                await cmd.ExecuteNonQueryAsync();
            }

            tx.Commit();
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return OperationResult<bool>.Fail(ex.Message, "DB_ERROR");
        }
    }

    public async Task<OperationResult<AgentState>> LoadAsync(string sessionId)
    {
        await using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        // Load the session row.
        string task;
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT task FROM sessions WHERE id = @id;";
            cmd.Parameters.AddWithValue("@id", sessionId);
            var scalar = await cmd.ExecuteScalarAsync();
            if (scalar is null)
                return OperationResult<AgentState>.Fail($"Session '{sessionId}' not found.", "NOT_FOUND");
            task = (string)scalar;
        }

        // Load messages in order.
        var messages = new List<ConversationMessage>();
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT role, content FROM messages WHERE session_id = @id ORDER BY seq;";
            cmd.Parameters.AddWithValue("@id", sessionId);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                messages.Add(new ConversationMessage(reader.GetString(0), reader.GetString(1)));
        }

        // Load todos.
        var todos = new List<TodoItem>();
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT description, is_completed FROM todos WHERE session_id = @id ORDER BY seq;";
            cmd.Parameters.AddWithValue("@id", sessionId);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                todos.Add(new TodoItem(reader.GetString(0), reader.GetInt32(1) != 0));
        }

        var state = new AgentState
        {
            SessionId      = sessionId,
            CurrentTask    = task,
            MessageHistory = messages,
            TodoList       = todos
        };

        return OperationResult<AgentState>.Ok(state);
    }

    public async Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
    {
        await using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var ids = new List<string>();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id FROM sessions ORDER BY created_at;";
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            ids.Add(reader.GetString(0));

        return OperationResult<IReadOnlyList<string>>.Ok(ids);
    }
}
