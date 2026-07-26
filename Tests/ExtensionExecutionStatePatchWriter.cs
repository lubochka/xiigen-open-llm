// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;

public static class ExtensionExecutionStatePatchWriter
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

    private static readonly JsonSerializerOptions DefaultJsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public static string DefaultPatchPath(string artifactDir, string phaseId)
        => Path.Combine(
            artifactDir,
            "execution-state-patches",
            $"{ToFileName(phaseId)}-state-patch.json");

    public static string WritePatch(
        string statePath,
        string artifactDir,
        string phaseId,
        IReadOnlyDictionary<string, object?> patchFields,
        object? report,
        object? validation,
        JsonSerializerOptions? jsonOptions = null)
    {
        Directory.CreateDirectory(Path.Combine(artifactDir, "execution-state-patches"));
        var patchPath = DefaultPatchPath(artifactDir, phaseId);
        var stateFile = new FileInfo(statePath);
        var patch = new
        {
            schema = "xiigen.extension_execution_state_patch.v1",
            phase_id = phaseId,
            source_state_ref = Path.GetFullPath(statePath),
            source_state_file_bytes = stateFile.Exists ? stateFile.Length : 0,
            state_file_read_strategy = "metadata_only_no_json_parse",
            state_file_write_strategy = "compact_patch_artifact_no_giant_rewrite",
            giant_state_mutated = false,
            patch_fields = patchFields,
            report,
            validation
        };
        var options = jsonOptions ?? DefaultJsonOptions;
        var json = JsonSerializer.Serialize(patch, options).Replace("\r\n", "\n", StringComparison.Ordinal);
        File.WriteAllText(patchPath, json + "\n", Utf8NoBom);
        return patchPath;
    }

    public static JsonElement ReadPatchFields(string patchPath)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(patchPath, Encoding.UTF8));
        return document.RootElement.GetProperty("patch_fields").Clone();
    }

    private static string ToFileName(string phaseId)
        => phaseId.ToLowerInvariant().Replace('_', '-').Replace(':', '-');
}
