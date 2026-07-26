// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OssEquivalents;

// ─── Skill metadata ───────────────────────────────────────────────────────────

// The frontmatter fields from a SKILL.md file, loaded at startup (fast, small).
public sealed record SkillMetadata(
    string Name,
    string Description,
    string FilePath,
    string Version = "1.0");

// ─── Skill file loader ────────────────────────────────────────────────────────

// Loads all SKILL.md files from a directory.
// At startup: parses only frontmatter (fast).
// On activation: loads the full Markdown content on demand.
public sealed class SkillFileLoader
{
    private readonly FrontmatterParser _parser;
    private readonly Dictionary<string, SkillMetadata> _metadata
        = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _fullContent
        = new(StringComparer.OrdinalIgnoreCase);

    public SkillFileLoader(FrontmatterParser parser)
        => _parser = parser;

    // Scan a directory for .md files and load their frontmatter.
    public async Task<OperationResult<IReadOnlyList<SkillMetadata>>> LoadDirectoryAsync(
        string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            return OperationResult<IReadOnlyList<SkillMetadata>>.Fail(
                $"Directory not found: {directoryPath}", "DIR_NOT_FOUND");

        var files = Directory.GetFiles(directoryPath, "*.md", SearchOption.TopDirectoryOnly);

        foreach (var filePath in files)
        {
            string text   = await File.ReadAllTextAsync(filePath);
            var    result = _parser.Parse(text);
            if (!result.IsSuccess) continue;

            var (meta, _) = result.Value;

            string name = meta.TryGetValue("name", out var n)    ? n.ToString()! : Path.GetFileNameWithoutExtension(filePath);
            string desc = meta.TryGetValue("description", out var d) ? d.ToString()! : "";
            string ver  = meta.TryGetValue("version", out var v)  ? v.ToString()! : "1.0";

            _metadata[name] = new SkillMetadata(name, desc, filePath, ver);
        }

        return OperationResult<IReadOnlyList<SkillMetadata>>.Ok([.. _metadata.Values]);
    }

    // Load the full Markdown content for a skill by name (on-demand lazy loading).
    public async Task<OperationResult<string>> LoadFullContentAsync(string skillName)
    {
        if (_fullContent.TryGetValue(skillName, out var cached))
            return OperationResult<string>.Ok(cached);

        if (!_metadata.TryGetValue(skillName, out var meta))
            return OperationResult<string>.Fail($"Skill '{skillName}' not found.", "NOT_FOUND");

        string text   = await File.ReadAllTextAsync(meta.FilePath);
        var    result = _parser.Parse(text);
        if (!result.IsSuccess)
            return OperationResult<string>.Fail(result.ErrorMessage, result.ErrorCode);

        string content = result.Value.Content;
        _fullContent[skillName] = content;
        return OperationResult<string>.Ok(content);
    }

    public IReadOnlyDictionary<string, SkillMetadata> LoadedMetadata => _metadata;
}
