// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ProjectWikiDomain;
using Xunit.Abstractions;

public sealed class WorkLeftRagEvidenceIndexTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private const string ExpectedEvidenceSha256 = "eb872a2cc24a0615382afdfbd47752ff224ef70a0e992de26af226cc73a6caca";
    private const string ExpectedManifestSha256 = "17994726c4ab6d4bbab04d3add5eed39b6f7f75b4234a016c1f95bb3fd828f5b";
    private const int ExpectedSourceCount = 20;
    private const int ExpectedRecordCount = 776;
    private const int ExpectedMarkdownHeadingChunkCount = 222;
    private const int ExpectedNdjsonRecordCount = 554;

    private static readonly string WikiRoot = Path.Combine(
        TestSuiteBatchManifest.FindRepositoryRoot(),
        ".xiigen",
        "wiki-domain");

    private static readonly string EvidencePath = Path.Combine(
        WikiRoot,
        "evidence",
        "evidence-index.jsonl");

    private static readonly string ManifestPath = Path.Combine(
        WikiRoot,
        "evidence",
        "workleft-source-manifest.json");

    private static readonly string GeneratorPath = Path.Combine(
        WikiRoot,
        "evidence",
        "GenerateWorkLeftEvidenceIndex.ps1");

    private static readonly SourceExpectation[] Sources =
    [
        new(@"14.08.26WorkLeft-DEC-04-runtime-blocked-executor-playbook.md", @"C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\14.08.26WorkLeft-DEC-04-runtime-blocked-executor-playbook.md", "40ee2ed84691694746b57f67210d9b67de88574f61a1979dc4eebae4278122f5"),
        new(@"13.08.26WorkLeft-DEC-01-smart-app-control-runtime-path.md", @"C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\13.08.26WorkLeft-DEC-01-smart-app-control-runtime-path.md", "534d62e49a190583da707872a728097df3ff0a40ba144ababe369b61d50b7c05"),
        new(@"13.08.26WorkLeft-PLAN-HUMAN-01.md", @"C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\13.08.26WorkLeft-PLAN-HUMAN-01.md", "29befe2589ad9a7240e9ed429cc7eacfdf7e5b3a57bbedd05840a5f41b3bd003"),
        new(@"13.08.26WorkLeft-DEC-02-architect-adjudication.md", @"C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\13.08.26WorkLeft-DEC-02-architect-adjudication.md", "98fabc41759fe19ea857ef2ea9a3ba663a0bbab0987d0d3c8c255753ed0a9229"),
        new(@"13.08.26WorkLeft-03-luba-decisions.md", @"C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\13.08.26WorkLeft-03-luba-decisions.md", "173171820f315a0b28e7c8c5465a223a214c673f42745b470b839da5735170b6"),
        new(@"13.08.26WorkLeft-01-code-changes.ndjson", @"C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\13.08.26WorkLeft-01-code-changes.ndjson", "a363967ff61d7596887167853358149c158244d37a957cbfe7c69eb8ab64248f"),
        new(@"13.08.26WorkLeft-01-code-changes.md", @"C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\13.08.26WorkLeft-01-code-changes.md", "65f9fb253df902950f0a24cd6cb6b1f776b00834cf98c4465a449cdd3525728e"),
        new(@"13.08.26WorkLeft-00-ИТОГ.md", @"C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\13.08.26WorkLeft-00-ИТОГ.md", "34a4271346fe74e16f86fab93406e6bbcad8b6a32b75fb9d3692205a063b639e"),
        new(@"13.08.26WorkLeft-02-teaching-chinese-model.md", @"C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\13.08.26WorkLeft-02-teaching-chinese-model.md", "76ef57a9a6b2fcc0bd2035122539c2b2422c395c0898db68476d0c65a1d2e45b"),
        new(@"13.08.26WorkLeft-02-teaching-chinese-model.ndjson", @"C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\13.08.26WorkLeft-02-teaching-chinese-model.ndjson", "196ef62343f1c321127cee0370bf5cef2c096633016455cd41ee168bc8020842"),
        new(@"13.08.26WorkLeft-MECH-01-mechanism-validation-current-tree.md", @"C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\13.08.26WorkLeft-MECH-01-mechanism-validation-current-tree.md", "a4763c517d3d28e8551f19c3936948a15c9999e4a76ae4b017af75c43bf8908e"),
        new(@"13.08.26WorkLeft-RAG-MASTER-INDEX.md", @"C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\13.08.26WorkLeft-RAG-MASTER-INDEX.md", "b92be92f71773d4d38a7fc5bc1966ac9ccf4e12cf420916e510bb7469c2655b7"),
        new(@"13.08.26WorkLeft-RAG-04-vs-vscode-continue.md", @"C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\13.08.26WorkLeft-RAG-04-vs-vscode-continue.md", "62609c9e29c1cffe4d277d49362b4444b9166d8aaf26194023355381647d9f4e"),
        new(@"13.08.26WorkLeft-RAG-03-merge-and-branches.md", @"C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\13.08.26WorkLeft-RAG-03-merge-and-branches.md", "5de24a5e3b308e442555fe4402fa9413665043b355f1ab1059143ea28470b170"),
        new(@"13.08.26WorkLeft-RAG-02-design-to-code.md", @"C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\13.08.26WorkLeft-RAG-02-design-to-code.md", "022991cee8915d3782974f29c07c675a3de89aee75b8c7c8c5f4eaccddd2b2ac"),
        new(@"13.08.26WorkLeft-RAG-01-dpo-legal-build.md", @"C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\13.08.26WorkLeft-RAG-01-dpo-legal-build.md", "cd880ff7e9c77a74ef5bd264c2d0e5ade68d050e126b70c547f90f0b901bef4f"),
        new(@"13.08.26WorkLeft-RAG-02-design-to-code.ndjson", @"C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\13.08.26WorkLeft-RAG-02-design-to-code.ndjson", "4efeb399f865c93acfc6fea93063f3060d8d62cbe63de90a9a849ba63e3dd3a6"),
        new(@"13.08.26WorkLeft-RAG-01-dpo-legal-build.ndjson", @"C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\13.08.26WorkLeft-RAG-01-dpo-legal-build.ndjson", "7c80b27c5e78e933eae825863111733b04e895f408a6be72ceb313a763676b77"),
        new(@"13.08.26WorkLeft-RAG-03-merge-and-branches.ndjson", @"C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\13.08.26WorkLeft-RAG-03-merge-and-branches.ndjson", "e301acef2475be0e84e714b2a0147a3aca2cc0f5775d68419a1b0450b87626b7"),
        new(@"13.08.26WorkLeft-RAG-04-vs-vscode-continue.ndjson", @"C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\13.08.26WorkLeft-RAG-04-vs-vscode-continue.ndjson", "8b14fc7b5e082ef9621cb62bbff32c748ceb30368bc9f7ff5d998959ad640cbc"),
    ];

    private readonly ITestOutputHelper _output;

    public WorkLeftRagEvidenceIndexTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task WorkLeftEvidenceIndex_LoadsSearchesRetrievesCitedCorpus()
    {
        Assert.Equal(ExpectedEvidenceSha256, Sha256File(EvidencePath));
        Assert.Equal(ExpectedManifestSha256, Sha256File(ManifestPath));

        foreach (var source in Sources)
        {
            Assert.Equal(source.Sha256, Sha256File(source.Path));
        }

        using var manifest = JsonDocument.Parse(File.ReadAllText(ManifestPath, Encoding.UTF8));
        Assert.Equal("workleft://sources", manifest.RootElement.GetProperty("source_root").GetString());
        Assert.Equal("workleft://evidence/evidence-index.jsonl", manifest.RootElement.GetProperty("output_evidence_index").GetString());
        Assert.Equal("workleft://evidence/workleft-source-manifest.json", manifest.RootElement.GetProperty("output_manifest").GetString());
        Assert.DoesNotContain(@"C:\Users\lubao", manifest.RootElement.GetRawText(), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(@"C:\\Users\\lubao", manifest.RootElement.GetRawText(), StringComparison.OrdinalIgnoreCase);
        var manifestSources = manifest.RootElement.GetProperty("sources").EnumerateArray().ToArray();
        Assert.Equal(ExpectedSourceCount, manifestSources.Length);
        foreach (var expected in Sources)
        {
            var manifestSource = manifestSources.Single(source => source.GetProperty("path").GetString() == expected.PublicSourcePath);
            Assert.Equal(expected.Basename, manifestSource.GetProperty("basename").GetString());
            Assert.Equal(expected.Sha256, manifestSource.GetProperty("expected_sha256").GetString());
            Assert.Equal(expected.Sha256, manifestSource.GetProperty("actual_sha256").GetString());
            Assert.True(manifestSource.GetProperty("evidence_record_count").GetInt32() > 0);
        }

        var lines = File.ReadAllLines(EvidencePath, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();
        Assert.All(lines, line =>
        {
            Assert.DoesNotContain(@"C:\Users\lubao", line, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(@"C:\\Users\\lubao", line, StringComparison.OrdinalIgnoreCase);
        });

        var records = new List<ProjectWikiEvidenceRecord>();
        foreach (var line in lines)
        {
            using var parsed = JsonDocument.Parse(line);
            Assert.Equal(JsonValueKind.Object, parsed.RootElement.ValueKind);
            var record = JsonSerializer.Deserialize<ProjectWikiEvidenceRecord>(line, JsonOptions);
            Assert.NotNull(record);
            Assert.False(string.IsNullOrWhiteSpace(record!.EvidenceId));
            Assert.False(string.IsNullOrWhiteSpace(record.Meaning));
            Assert.False(string.IsNullOrWhiteSpace(record.SourcePath));
            Assert.False(string.IsNullOrWhiteSpace(record.DocPath));
            Assert.True(record.SourceLine > 0);
            records.Add(record);
        }

        Assert.Equal(ExpectedRecordCount, records.Count);
        Assert.Equal(ExpectedRecordCount, records.Select(record => record.EvidenceId).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(ExpectedRecordCount, manifest.RootElement.GetProperty("totals").GetProperty("evidence_record_count").GetInt32());
        Assert.Equal(ExpectedMarkdownHeadingChunkCount, manifest.RootElement.GetProperty("totals").GetProperty("markdown_chunk_count").GetInt32());
        Assert.Equal(ExpectedNdjsonRecordCount, manifest.RootElement.GetProperty("totals").GetProperty("ndjson_record_count").GetInt32());
        Assert.Equal(ExpectedRecordCount, manifest.RootElement.GetProperty("totals").GetProperty("unique_evidence_id_count").GetInt32());
        Assert.Equal(
            ExpectedMarkdownHeadingChunkCount,
            records.Count(record => record.Parameters.Contains("content_kind=markdown_heading_chunk")));
        Assert.Equal(
            ExpectedNdjsonRecordCount,
            records.Count(record => record.Parameters.Contains("content_kind=ndjson_record")));

        Assert.All(records, record =>
        {
            Assert.False(string.IsNullOrWhiteSpace(record.EvidenceId));
            Assert.False(string.IsNullOrWhiteSpace(record.ProjectId));
            Assert.False(string.IsNullOrWhiteSpace(record.DocumentationVersion));
            Assert.False(string.IsNullOrWhiteSpace(record.SymbolId));
            Assert.False(string.IsNullOrWhiteSpace(record.RelationSymbolId));
            Assert.False(string.IsNullOrWhiteSpace(record.DocPath));
            Assert.False(string.IsNullOrWhiteSpace(record.SourcePath));
            Assert.True(record.SourceLine > 0);
            Assert.False(string.IsNullOrWhiteSpace(record.DocumentationLevel));
            Assert.False(string.IsNullOrWhiteSpace(record.TargetFramework));
            Assert.False(string.IsNullOrWhiteSpace(record.CSharpLanguageVersion));
            Assert.False(string.IsNullOrWhiteSpace(record.Meaning));
            Assert.False(string.IsNullOrWhiteSpace(record.Returns));
            Assert.False(string.IsNullOrWhiteSpace(Citation(record)));
            Assert.DoesNotContain(@"C:\Users\lubao", record.SourcePath, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(@"C:\\Users\\lubao", record.SourcePath, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(record.Parameters, parameter => parameter.StartsWith("source_basename=", StringComparison.Ordinal));
            Assert.Contains(record.Parameters, parameter => parameter.StartsWith("source_sha256=", StringComparison.Ordinal));
            Assert.Contains(record.Parameters, parameter => parameter.StartsWith("source_line_start=", StringComparison.Ordinal));
            Assert.Contains(record.Parameters, parameter => parameter.StartsWith("source_line_end=", StringComparison.Ordinal));
            Assert.Contains(record.Parameters, parameter => parameter.StartsWith("content_kind=", StringComparison.Ordinal));
            Assert.Contains(record.Parameters, parameter => parameter.StartsWith("heading_path=", StringComparison.Ordinal));
            Assert.Contains(record.Parameters, parameter => parameter.StartsWith("classification=", StringComparison.Ordinal));
            Assert.Contains(record.Parameters, parameter => parameter.StartsWith("routing_tags=", StringComparison.Ordinal));
        });

        var coveredHashes = records
            .SelectMany(record => record.Parameters)
            .Where(parameter => parameter.StartsWith("source_sha256=", StringComparison.Ordinal))
            .Select(parameter => parameter["source_sha256=".Length..])
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(20, coveredHashes.Length);
        Assert.All(Sources, source => Assert.Contains(source.Sha256, coveredHashes));
        Assert.All(Sources, source => Assert.Contains(records, record => record.SourcePath == source.PublicSourcePath));

        var index = ProjectEvidenceIndex.LoadFromWikiDomain(WikiRoot);
        Assert.True(index.IsSuccess, index.ErrorMessage);
        Assert.Equal(records.Count, index.Value!.Records.Count);

        var retriever = new WikiContextRetriever(index.Value);
        var expectations = new[]
        {
            new QueryExpectation(
                "runtime blocked Smart App Control WSL2 Ubuntu 24.04 .NET 9",
                "14.08.26WorkLeft-DEC-04-runtime-blocked-executor-playbook.md"),
            new QueryExpectation(
                "owner pending decisions default 58 architect adjudication",
                "13.08.26WorkLeft-DEC-02-architect-adjudication.md"),
            new QueryExpectation(
                "W-153 capability mutation admission issuer",
                "13.08.26WorkLeft-01-code-changes.ndjson"),
            new QueryExpectation(
                "training not started W-166",
                "13.08.26WorkLeft-02-teaching-chinese-model.ndjson"),
            new QueryExpectation(
                "merge branches evidence canon",
                "13.08.26WorkLeft-RAG-03-merge-and-branches.md"),
            new QueryExpectation(
                "design to code source map",
                "13.08.26WorkLeft-RAG-02-design-to-code.md"),
        };

        foreach (var expectation in expectations)
        {
            var hits = index.Value.Search(expectation.Query, topK: 5);
            Assert.True(hits.IsSuccess, hits.ErrorMessage);
            Assert.NotEmpty(hits.Value!);
            Assert.Contains(
                hits.Value!.Take(5),
                hit => SourceBasename(hit.Evidence.SourcePath) == expectation.ExpectedBasename);
            Assert.All(hits.Value!, hit => Assert.False(string.IsNullOrWhiteSpace(Citation(hit.Evidence))));

            var context = await retriever.RetrieveAsync(new ProjectWikiQuestionRequest(
                "llm_mvp_core-DpoImplementation",
                expectation.Query,
                WikiRoot,
                "workleft-rag-2026-08-16",
                TopK: 5,
                AllowSemantic: false));

            Assert.True(context.IsSuccess, context.ErrorMessage);
            Assert.Equal(ProjectWikiRetrievalMode.Mode3LexicalEvidence, context.Value!.Mode);
            Assert.False(context.Value.SemanticUnavailable);
            Assert.Empty(context.Value.Warnings);
            Assert.Contains(
                context.Value.EvidenceHits.Take(5),
                hit => SourceBasename(hit.Evidence.SourcePath) == expectation.ExpectedBasename);
            Assert.All(context.Value.EvidenceHits, hit => Assert.False(string.IsNullOrWhiteSpace(Citation(hit.Evidence))));

            var top = context.Value.EvidenceHits[0].Evidence;
            _output.WriteLine($"{expectation.Query} -> {SourceBasename(top.SourcePath)}:{top.SourceLine} [{top.EvidenceId}]");
        }

        VerifyScratchGenerationIsByteIdentical();
    }

    [Fact]
    public void ProjectEvidenceIndex_LoadsWikiRootWhenPathIsSpelledAsWindowsDriveFromWsl()
    {
        var windowsSpelledWikiRoot = ToWindowsDriveSpelling(WikiRoot);

        var index = ProjectEvidenceIndex.LoadFromWikiDomain(windowsSpelledWikiRoot);

        Assert.True(index.IsSuccess, index.ErrorMessage);
        Assert.Equal(ExpectedRecordCount, index.Value!.Records.Count);
    }

    private void VerifyScratchGenerationIsByteIdentical()
    {
        var scratchRoot = Path.Combine(
            Path.GetTempPath(),
            $"workleft-rag-generator-{Guid.NewGuid():N}");
        var scratchGeneratorPath = Path.Combine(scratchRoot, Path.GetFileName(GeneratorPath));

        try
        {
            Directory.CreateDirectory(scratchRoot);
            File.Copy(GeneratorPath, scratchGeneratorPath);
            Assert.Equal(Sha256File(GeneratorPath), Sha256File(scratchGeneratorPath));

            var result = RunPowerShellFile(scratchGeneratorPath, scratchRoot);
            Assert.True(result.ExitCode == 0, $"Scratch generator exit code {result.ExitCode}. Output:{Environment.NewLine}{result.Output}");
            Assert.Contains("SourceCount", result.Output, StringComparison.Ordinal);
            Assert.Contains(ExpectedSourceCount.ToString(), result.Output, StringComparison.Ordinal);
            Assert.Contains("EvidenceRecordCount", result.Output, StringComparison.Ordinal);
            Assert.Contains(ExpectedRecordCount.ToString(), result.Output, StringComparison.Ordinal);
            Assert.DoesNotContain("ParserError", result.Output, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("hash mismatch", result.Output, StringComparison.OrdinalIgnoreCase);

            var scratchEvidencePath = Path.Combine(scratchRoot, "evidence-index.jsonl");
            var scratchManifestPath = Path.Combine(scratchRoot, "workleft-source-manifest.json");
            Assert.True(File.Exists(scratchEvidencePath));
            Assert.True(File.Exists(scratchManifestPath));
            Assert.Equal(ExpectedEvidenceSha256, Sha256File(scratchEvidencePath));
            Assert.Equal(File.ReadAllText(EvidencePath, Encoding.UTF8), File.ReadAllText(scratchEvidencePath, Encoding.UTF8));

            Assert.Equal(ExpectedManifestSha256, Sha256File(scratchManifestPath));
            Assert.Equal(File.ReadAllBytes(ManifestPath), File.ReadAllBytes(scratchManifestPath));
        }
        finally
        {
            if (Directory.Exists(scratchRoot))
            {
                Directory.Delete(scratchRoot, recursive: true);
            }

            Assert.False(Directory.Exists(scratchRoot));
        }
    }

    private static CommandResult RunPowerShellFile(string scriptPath, string workingDirectory)
    {
        try
        {
            return RunPowerShellFileWith("pwsh.exe", scriptPath, workingDirectory);
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return RunPowerShellFileWith("powershell.exe", scriptPath, workingDirectory);
        }
    }

    private static CommandResult RunPowerShellFileWith(string fileName, string scriptPath, string workingDirectory)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -File {QuoteArgument(scriptPath)}",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var output = new StringBuilder();
        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                output.AppendLine(args.Data);
            }
        };
        process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                output.AppendLine(args.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        Assert.True(process.WaitForExit(milliseconds: 60_000), "Scratch generator did not exit within 60 seconds.");
        process.WaitForExit();
        return new CommandResult(process.ExitCode, output.ToString());
    }

    private static string QuoteArgument(string value)
        => $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    private static string Citation(ProjectWikiEvidenceRecord record)
        => string.IsNullOrWhiteSpace(record.SourcePath) || record.SourceLine <= 0
            ? ""
            : $"{record.SourcePath}:{record.SourceLine}";

    private static string SourceBasename(string sourcePath)
    {
        var normalized = sourcePath.Replace('\\', '/');
        var lastSlash = normalized.LastIndexOf('/');
        return lastSlash < 0 ? normalized : normalized[(lastSlash + 1)..];
    }

    private static string ResolveWorkLeftSourcePath(string path, string basename)
    {
        var hostPath = OperatingSystem.IsWindows() || !TryMapWindowsDrivePathToWslMount(path, out var mappedPath)
            ? path
            : mappedPath;

        if (File.Exists(hostPath))
            return hostPath;

        var sourceRoot = Path.GetDirectoryName(hostPath);
        var archivedPath = string.IsNullOrWhiteSpace(sourceRoot)
            ? hostPath
            : Path.Combine(sourceRoot, "archive", basename);
        if (File.Exists(archivedPath))
            return archivedPath;

        // Русский комментарий: WorkLeft-источники живут на Windows-диске.
        // При запуске теста из WSL открываем тот же файл через /mnt/<disk>,
        // а после архивирования берем тот же исторический документ из archive.
        return hostPath;
    }

    private static string ToWindowsDriveSpelling(string path)
    {
        if (OperatingSystem.IsWindows())
            return path;

        var normalized = path.Replace('\\', '/');
        if (!normalized.StartsWith("/mnt/", StringComparison.OrdinalIgnoreCase) ||
            normalized.Length < "/mnt/c/".Length ||
            !char.IsLetter(normalized["/mnt/".Length]) ||
            normalized["/mnt/c".Length] != '/')
        {
            return path;
        }

        var drive = char.ToUpperInvariant(normalized["/mnt/".Length]);
        return $"{drive}:\\{normalized["/mnt/c/".Length..].Replace('/', '\\')}";
    }

    private static bool TryMapWindowsDrivePathToWslMount(string path, out string mappedPath)
    {
        mappedPath = "";
        if (path.Length < 3 || !char.IsLetter(path[0]) || path[1] != ':' || path[2] is not ('\\' or '/'))
            return false;

        var drive = char.ToLowerInvariant(path[0]);
        var tail = path[3..].Replace('\\', '/').TrimStart('/');
        mappedPath = string.IsNullOrEmpty(tail)
            ? $"/mnt/{drive}"
            : $"/mnt/{drive}/{tail}";
        return true;
    }

    private static string Sha256File(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private sealed record SourceExpectation(string Basename, string HostPath, string Sha256)
    {
        public string PublicSourcePath => $"workleft://{Basename}";

        public string Path { get; } = ResolveWorkLeftSourcePath(HostPath, Basename);
    }

    private sealed record QueryExpectation(string Query, string ExpectedBasename);

    private sealed record CommandResult(int ExitCode, string Output);
}
