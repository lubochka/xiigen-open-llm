// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentAlgorithms;

// ─── Rules parse result ───────────────────────────────────────────────────────

public sealed record RulesParseResult(
    Dictionary<string, string>   Sections,
    IReadOnlyList<string>        AllowedTools,
    IReadOnlyList<string>        DeniedTools);

// ─── Rules file parser ────────────────────────────────────────────────────────

// Parses AGENTS.md or CLAUDE.md instruction files.
// Extracts named sections (each Markdown heading introduces a section)
// and any @allow / @deny lines that define tool permissions.
public sealed class RulesFileParser
{
    // Parse the raw text of an instruction file.
    public OperationResult<RulesParseResult> Parse(string fileContent)
    {
        if (string.IsNullOrWhiteSpace(fileContent))
            return OperationResult<RulesParseResult>.Fail("File content is empty.", "EMPTY_CONTENT");

        var sections    = new Dictionary<string, string>(StringComparer.Ordinal);
        var allowTools  = new List<string>();
        var denyTools   = new List<string>();

        string currentSection = "preamble";
        var sectionLines = new List<string>();

        void FlushSection()
        {
            sections[currentSection] = string.Join('\n', sectionLines).Trim();
            sectionLines.Clear();
        }

        // Normalise CRLF (Windows) and lone CR (old Mac) to plain LF before splitting
        // so that raw string literals compiled from CRLF source files do not leave
        // a trailing \r on each section name and tool directive.
        foreach (var line in fileContent.ReplaceLineEndings("\n").Split('\n'))
        {
            // Markdown heading → new section.
            if (line.StartsWith('#'))
            {
                FlushSection();
                currentSection = line.TrimStart('#', ' ');
                continue;
            }

            // @allow tool_name directive.
            if (line.TrimStart().StartsWith("@allow ", StringComparison.OrdinalIgnoreCase))
            {
                allowTools.Add(line.TrimStart()["@allow ".Length..].Trim());
                continue;
            }

            // @deny tool_name directive.
            if (line.TrimStart().StartsWith("@deny ", StringComparison.OrdinalIgnoreCase))
            {
                denyTools.Add(line.TrimStart()["@deny ".Length..].Trim());
                continue;
            }

            sectionLines.Add(line);
        }

        FlushSection();

        return OperationResult<RulesParseResult>.Ok(
            new RulesParseResult(sections, allowTools, denyTools));
    }
}
