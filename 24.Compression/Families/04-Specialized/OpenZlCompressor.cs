// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;

namespace Compression;

// OpenZL-style structured compressor: detects CSV/JSON structure, rearranges
// data into columnar layout so repeated values cluster together, then applies
// a general-purpose compressor on the reordered bytes.
// Columnar layout dramatically improves compression on tabular data.
// Reference: OpenZL concept (column-major byte rearrangement before entropy coding)
public sealed class OpenZlCompressor : ICompressor
{
    private readonly ICompressor _inner;
    private readonly OpenZlStructureAnalyzer _analyzer = new();

    // inner — the compressor applied after columnar rearrangement (e.g., Brotli, Zstd).
    public OpenZlCompressor(ICompressor inner) => _inner = inner;

    public async Task<OperationResult<CompressionResult>> CompressAsync(
        byte[] input, CompressionOptions options)
    {
        if (input.Length == 0)
            return OperationResult<CompressionResult>.Fail("Empty input.", "EMPTY_INPUT");

        var text   = Encoding.UTF8.GetString(input);
        var report = _analyzer.Analyze(text);

        byte[] payload;
        byte   mode; // 0 = passthrough, 1 = CSV columnar, 2 = JSON columnar

        if (report.IsColumnarFriendly && report.Format == OpenZlStructureAnalyzer.DataFormat.Csv)
        {
            payload = ColumnarizeCsv(text, report);
            mode    = 1;
        }
        else if (report.IsColumnarFriendly && report.Format == OpenZlStructureAnalyzer.DataFormat.Json)
        {
            payload = ColumnarizeJson(text, report);
            mode    = 2;
        }
        else
        {
            payload = input;
            mode    = 0;
        }

        var result = await _inner.CompressAsync(payload, options);
        if (!result.IsSuccess)
            return result;

        // Prepend 1-byte mode flag + 4-byte original length.
        var compressed = result.Value!.CompressedBytes;
        var output     = new byte[1 + 4 + compressed.Length];
        output[0] = mode;
        output[1] = (byte)input.Length;
        output[2] = (byte)(input.Length >> 8);
        output[3] = (byte)(input.Length >> 16);
        output[4] = (byte)(input.Length >> 24);
        Buffer.BlockCopy(compressed, 0, output, 5, compressed.Length);

        return OperationResult<CompressionResult>.Ok(new CompressionResult
        {
            CompressedBytes = output,
            OriginalLength  = input.Length,
            AlgorithmName   = "OpenZL"
        });
    }

    public async Task<OperationResult<byte[]>> DecompressAsync(byte[] compressed, int expectedOriginalLength)
    {
        if (compressed.Length < 5)
            return OperationResult<byte[]>.Fail("Too short.", "CORRUPT");

        byte mode  = compressed[0];
        int origLen = compressed[1] | (compressed[2] << 8) | (compressed[3] << 16) | (compressed[4] << 24);

        byte[] innerData = compressed[5..];
        var result = await _inner.DecompressAsync(innerData, 0);
        if (!result.IsSuccess)
            return result;

        byte[] payload = result.Value!;

        if (mode == 0)
            return OperationResult<byte[]>.Ok(payload);

        // Reverse the columnar rearrangement.
        string text = mode == 1
            ? DecolumnizeCsv(payload)
            : DecolumnizeJson(payload);

        return OperationResult<byte[]>.Ok(Encoding.UTF8.GetBytes(text));
    }

    // Rearrange CSV rows → column-major bytes so similar values cluster.
    // Format: header row unchanged, then all values for col0, then col1, etc.
    private static byte[] ColumnarizeCsv(string text, OpenZlStructureAnalyzer.StructureReport report)
    {
        var lines   = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var header  = lines[0];
        var rows    = lines.Skip(1)
                          .Select(l => l.Split(','))
                          .Where(r => r.Length == report.ColumnCount)
                          .ToList();

        var sb = new StringBuilder();
        sb.AppendLine(header);
        sb.AppendLine(rows.Count.ToString()); // row count for reconstruction

        for (int c = 0; c < report.ColumnCount; c++)
        {
            foreach (var row in rows)
                sb.AppendLine(c < row.Length ? row[c] : "");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string DecolumnizeCsv(byte[] payload)
    {
        var text    = Encoding.UTF8.GetString(payload);
        var lines   = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return text;

        var header  = lines[0];
        if (!int.TryParse(lines[1], out int rowCount)) return text;

        var headers  = header.Split(',');
        int colCount = headers.Length;

        // Remaining lines: colCount * rowCount values.
        var values = lines.Skip(2).ToArray();
        var rows   = new string[rowCount][];
        for (int r = 0; r < rowCount; r++)
            rows[r] = new string[colCount];

        for (int c = 0; c < colCount; c++)
        {
            for (int r = 0; r < rowCount; r++)
            {
                int idx = c * rowCount + r;
                rows[r][c] = idx < values.Length ? values[idx] : "";
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine(header);
        foreach (var row in rows)
            sb.AppendLine(string.Join(",", row));

        return sb.ToString();
    }

    // Rearrange JSON array → key-grouped lines for columnar compression.
    private static byte[] ColumnarizeJson(string text, OpenZlStructureAnalyzer.StructureReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine(report.ColumnCount.ToString());

        foreach (var col in report.Columns)
        {
            sb.AppendLine(col.Name);
            // Extract all values for this key using simple string scanning.
            string key   = col.Name;
            string search = $"\"{key}\":";
            int idx = 0;
            var vals = new List<string>();

            while (true)
            {
                int found = text.IndexOf(search, idx, StringComparison.Ordinal);
                if (found < 0) break;
                int vStart = found + search.Length;
                while (vStart < text.Length && char.IsWhiteSpace(text[vStart])) vStart++;
                int vEnd = vStart;
                if (vEnd < text.Length && text[vEnd] == '"')
                {
                    vEnd++;
                    while (vEnd < text.Length && text[vEnd] != '"') vEnd++;
                    vals.Add(text[(vStart + 1)..vEnd]);
                    vEnd++;
                }
                else
                {
                    while (vEnd < text.Length && text[vEnd] != ',' && text[vEnd] != '}') vEnd++;
                    vals.Add(text[vStart..vEnd].Trim());
                }
                idx = vEnd;
            }

            sb.AppendLine(vals.Count.ToString());
            foreach (var v in vals) sb.AppendLine(v);
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string DecolumnizeJson(byte[] payload)
    {
        var lines = Encoding.UTF8.GetString(payload)
                        .Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 1) return "[]";

        if (!int.TryParse(lines[0], out int colCount))
            return Encoding.UTF8.GetString(payload);

        var columns = new List<(string Key, string[] Values)>();
        int li = 1;

        for (int c = 0; c < colCount && li < lines.Length; c++)
        {
            string key = lines[li++];
            if (!int.TryParse(lines[li++], out int count)) break;
            var vals = new string[count];
            for (int r = 0; r < count && li < lines.Length; r++)
                vals[r] = lines[li++];
            columns.Add((key, vals));
        }

        if (columns.Count == 0) return "[]";

        int rowCount = columns[0].Values.Length;
        var sb       = new StringBuilder("[");

        for (int r = 0; r < rowCount; r++)
        {
            sb.Append('{');
            for (int c = 0; c < columns.Count; c++)
            {
                var (key, vals) = columns[c];
                string val      = r < vals.Length ? vals[r] : "";
                sb.Append($"\"{key}\":\"{val}\"");
                if (c < columns.Count - 1) sb.Append(',');
            }
            sb.Append('}');
            if (r < rowCount - 1) sb.Append(',');
        }

        sb.Append(']');
        return sb.ToString();
    }
}
