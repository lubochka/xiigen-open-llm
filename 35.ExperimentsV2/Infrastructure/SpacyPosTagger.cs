// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Diagnostics;
using System.Text.Json;

namespace ExperimentsV2;

// SpacyPosTagger calls the Python script spacy_pos_tagger.py via subprocess.
// Sends sentences to stdin, receives POS tags from stdout as JSON.
public sealed class SpacyPosTagger : IDisposable
{
    private readonly Process _process;
    private bool _disposed;

    public SpacyPosTagger(string scriptPath)
    {
        if (!File.Exists(scriptPath))
            throw new FileNotFoundException($"Script not found: {scriptPath}");

        _process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName               = "python",
                Arguments              = $"\"{scriptPath}\"",
                RedirectStandardInput  = true,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
            }
        };
        _process.Start();
    }

    // Get POS tags for a single sentence.
    public OperationResult<IReadOnlyList<string>> GetPosTags(string sentence)
    {
        try
        {
            string sanitized = sentence.Replace('\n', ' ').Replace('\r', ' ');
            _process.StandardInput.WriteLine(sanitized);
            _process.StandardInput.Flush();

            string? line = _process.StandardOutput.ReadLine();
            if (line == null)
                return OperationResult<IReadOnlyList<string>>.Fail("SpaCy did not respond");

            var tags = JsonSerializer.Deserialize<List<string>>(line);
            if (tags == null || tags.Count == 0)
                return OperationResult<IReadOnlyList<string>>.Fail("Empty SpaCy response");

            return OperationResult<IReadOnlyList<string>>.Ok(tags.AsReadOnly());
        }
        catch (Exception ex)
        {
            return OperationResult<IReadOnlyList<string>>.Fail($"SpaCy error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { _process.StandardInput.Close(); } catch { }
        try { _process.WaitForExit(2000); } catch { }
        try { _process.Kill(); } catch { }
        _process.Dispose();
    }
}
