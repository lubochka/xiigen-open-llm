// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Text;
using System.Text.Json;

internal static class ArtifactTextWriter
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public static void WriteJson<T>(string path, T value, JsonSerializerOptions options, bool finalNewLine = false)
    {
        WriteText(path, JsonSerializer.Serialize(value, options), finalNewLine);
    }

    public static Task WriteJsonAsync<T>(string path, T value, JsonSerializerOptions options, bool finalNewLine = false)
    {
        return WriteTextAsync(path, JsonSerializer.Serialize(value, options), finalNewLine);
    }

    public static void WriteJsonLines<T>(string path, IEnumerable<T> rows, JsonSerializerOptions options)
    {
        var text = string.Join("\n", rows.Select(row => JsonSerializer.Serialize(row, options)));
        WriteText(path, text, finalNewLine: true);
    }

    public static Task WriteJsonLinesAsync<T>(string path, IEnumerable<T> rows, JsonSerializerOptions options)
    {
        var text = string.Join("\n", rows.Select(row => JsonSerializer.Serialize(row, options)));
        return WriteTextAsync(path, text, finalNewLine: true);
    }

    public static void WriteText(string path, string text, bool finalNewLine = false)
    {
        File.WriteAllText(path, Normalize(text, finalNewLine), Utf8NoBom);
    }

    public static Task WriteTextAsync(string path, string text, bool finalNewLine = false)
    {
        return File.WriteAllTextAsync(path, Normalize(text, finalNewLine), Utf8NoBom);
    }

    private static string Normalize(string text, bool finalNewLine)
    {
        var normalized = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal);
        var cleanLines = normalized
            .Split('\n')
            .Select(line => line.TrimEnd(' ', '\t'));
        var clean = string.Join("\n", cleanLines);
        return finalNewLine ? clean.TrimEnd('\n') + "\n" : clean;
    }
}
