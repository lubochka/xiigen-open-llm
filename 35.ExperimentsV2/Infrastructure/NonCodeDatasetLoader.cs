// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Net.Http;
using System.Text;

namespace ExperimentsV2;

// Provides non-code datasets for E08: English prose, SQL queries, and JSON documents.
// Moby Dick is downloaded from Project Gutenberg once and cached on disk.
public static class NonCodeDatasetLoader
{
    private const string MobyDickUrl   = "https://gutenberg.org/files/2701/2701-0.txt";
    private const string CacheFileName = "moby_dick.txt";

    // Download (once) and return the full text of Moby Dick.
    public static OperationResult<string> LoadEnglishText(string cacheDir)
    {
        try
        {
            Directory.CreateDirectory(cacheDir);
            string cachePath = Path.Combine(cacheDir, CacheFileName);

            if (!File.Exists(cachePath))
            {
                Console.Write("  Downloading Moby Dick from gutenberg.org... ");
                using var client = new HttpClient();
                client.Timeout   = TimeSpan.FromSeconds(60);
                string text      = client.GetStringAsync(MobyDickUrl).GetAwaiter().GetResult();
                File.WriteAllText(cachePath, text, Encoding.UTF8);
                Console.WriteLine($"OK ({text.Length:N0} chars, cache: {cachePath})");
            }

            return OperationResult<string>.Ok(File.ReadAllText(cachePath, Encoding.UTF8));
        }
        catch (Exception ex)
        {
            return OperationResult<string>.Fail($"LoadEnglishText: {ex.Message}");
        }
    }

    // Generate deterministic SQL SELECT/JOIN/GROUP-BY queries.
    public static IReadOnlyList<string> GenerateSqlQueries(int count = 500)
    {
        var tables  = new[] { "users", "orders", "products", "payments", "sessions",
                               "logs",  "events", "accounts", "items",   "categories" };
        var columns = new[] { "id", "name", "email", "status", "amount",
                               "created_at", "updated_at", "user_id", "type", "value" };
        var ops     = new[] { "=", "!=", ">", "<", ">=", "<=" };
        var aggs    = new[] { "COUNT(*)", "SUM(amount)", "AVG(value)", "MAX(id)", "MIN(created_at)" };

        var queries = new List<string>(count);
        var rng     = new Random(42);

        for (int n = 0; n < count; n++)
        {
            string t1  = tables[rng.Next(tables.Length)];
            string t2  = tables[rng.Next(tables.Length)];
            string col = columns[rng.Next(columns.Length)];
            string op  = ops[rng.Next(ops.Length)];
            int    val = rng.Next(1, 1000);

            string q = (n % 4) switch
            {
                0 => $"SELECT {col}, {columns[rng.Next(columns.Length)]} FROM {t1} " +
                     $"WHERE {col} {op} {val};",
                1 => $"SELECT {aggs[rng.Next(aggs.Length)]} FROM {t1} " +
                     $"GROUP BY {col} HAVING COUNT(*) > {val % 10};",
                2 => $"SELECT a.{col}, b.{columns[rng.Next(columns.Length)]} " +
                     $"FROM {t1} a INNER JOIN {t2} b ON a.id = b.{t1[..Math.Min(4, t1.Length)]}_id " +
                     $"WHERE a.status = 'active';",
                _ => $"SELECT DISTINCT {col} FROM {t1} ORDER BY {col} LIMIT {val % 100 + 1};"
            };
            queries.Add(q);
        }
        return queries;
    }

    // Generate deterministic JSON documents of varying depth and type mix.
    public static IReadOnlyList<string> GenerateJsonDocuments(int count = 200)
    {
        var keys = new[] { "id", "name", "type", "value", "status", "email",
                            "count", "active", "tags", "metadata", "items", "config" };

        var docs = new List<string>(count);
        var rng  = new Random(42);

        for (int n = 0; n < count; n++)
        {
            int numKeys = rng.Next(3, 8);
            var sb = new StringBuilder("{");
            for (int k = 0; k < numKeys; k++)
            {
                if (k > 0) sb.Append(',');
                string key = keys[rng.Next(keys.Length)];
                sb.Append($"\"{key}\":");

                switch (rng.Next(4))
                {
                    case 0: sb.Append($"\"{key}_{n}\""); break;
                    case 1: sb.Append(rng.Next(1, 10000)); break;
                    case 2: sb.Append(rng.Next(2) == 0 ? "true" : "false"); break;
                    default:
                        int arrLen = rng.Next(2, 5);
                        sb.Append('[');
                        for (int a = 0; a < arrLen; a++)
                        {
                            if (a > 0) sb.Append(',');
                            sb.Append($"\"{key}_{a}\"");
                        }
                        sb.Append(']');
                        break;
                }
            }
            sb.Append('}');
            docs.Add(sb.ToString());
        }
        return docs;
    }
}
