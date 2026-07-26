// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;

public sealed class CodeDocumentationBatchFileRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly CodeDocumentationTrainingRunner _runner;

    public CodeDocumentationBatchFileRunner(CodeDocumentationTrainingRunner? runner = null)
    {
        _runner = runner ?? new CodeDocumentationTrainingRunner();
    }

    public async Task<OperationResult<CodeDocumentationTrainingRunResult>> RunAsync(
        string trainPairsPath,
        string heldOutRowsPath,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(trainPairsPath))
            return OperationResult<CodeDocumentationTrainingRunResult>.Fail("TRAIN-PAIRS.jsonl path is required.", "TRAIN_PAIRS_PATH_REQUIRED");

        if (string.IsNullOrWhiteSpace(heldOutRowsPath))
            return OperationResult<CodeDocumentationTrainingRunResult>.Fail("HELDOUT-ROWS.jsonl path is required.", "HELDOUT_ROWS_PATH_REQUIRED");

        if (!File.Exists(trainPairsPath))
            return OperationResult<CodeDocumentationTrainingRunResult>.Fail($"TRAIN-PAIRS.jsonl not found: {trainPairsPath}", "TRAIN_PAIRS_NOT_FOUND");

        if (!File.Exists(heldOutRowsPath))
            return OperationResult<CodeDocumentationTrainingRunResult>.Fail($"HELDOUT-ROWS.jsonl not found: {heldOutRowsPath}", "HELDOUT_ROWS_NOT_FOUND");

        var trainRows = await LoadRowsAsync(trainPairsPath, ct);
        if (!trainRows.IsSuccess)
            return trainRows.MapError<CodeDocumentationTrainingRunResult>();

        var heldOutRows = await LoadRowsAsync(heldOutRowsPath, ct);
        if (!heldOutRows.IsSuccess)
            return heldOutRows.MapError<CodeDocumentationTrainingRunResult>();

        var trainExamples = ConvertRows(trainRows.Value!);
        if (!trainExamples.IsSuccess)
            return trainExamples.MapError<CodeDocumentationTrainingRunResult>();

        var heldOutExamples = ConvertRows(heldOutRows.Value!);
        if (!heldOutExamples.IsSuccess)
            return heldOutExamples.MapError<CodeDocumentationTrainingRunResult>();

        return await _runner.TrainAndEvaluateAsync(trainExamples.Value!, heldOutExamples.Value!, ct);
    }

    private static async Task<OperationResult<CodeDocumentationBatchRow[]>> LoadRowsAsync(
        string path,
        CancellationToken ct)
    {
        var rows = new List<CodeDocumentationBatchRow>();
        var lineNumber = 0;
        await foreach (var line in File.ReadLinesAsync(path, ct))
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
                continue;

            CodeDocumentationBatchRow? row;
            try
            {
                row = JsonSerializer.Deserialize<CodeDocumentationBatchRow>(line, JsonOptions);
            }
            catch (JsonException ex)
            {
                return OperationResult<CodeDocumentationBatchRow[]>.Fail($"Invalid JSONL at {path}:{lineNumber}: {ex.Message}", "INVALID_CODE_DOC_JSONL");
            }

            if (row is null)
                return OperationResult<CodeDocumentationBatchRow[]>.Fail($"Empty JSONL row at {path}:{lineNumber}.", "EMPTY_CODE_DOC_JSONL_ROW");

            rows.Add(row);
        }

        return OperationResult<CodeDocumentationBatchRow[]>.Ok(rows.ToArray());
    }

    private static OperationResult<CodeDocumentationTrainingExample[]> ConvertRows(
        IReadOnlyList<CodeDocumentationBatchRow> rows)
    {
        var examples = new List<CodeDocumentationTrainingExample>();
        foreach (var row in rows)
        {
            var converted = ConvertRow(row);
            if (!converted.IsSuccess)
                return converted.MapError<CodeDocumentationTrainingExample[]>();

            examples.Add(converted.Value!);
        }

        return OperationResult<CodeDocumentationTrainingExample[]>.Ok(examples.ToArray());
    }

    private static OperationResult<CodeDocumentationTrainingExample> ConvertRow(CodeDocumentationBatchRow row)
    {
        if (string.IsNullOrWhiteSpace(row.ExampleId))
            return OperationResult<CodeDocumentationTrainingExample>.Fail("example_id is required.", "EXAMPLE_ID_REQUIRED");

        if (string.IsNullOrWhiteSpace(row.CodeText))
            return OperationResult<CodeDocumentationTrainingExample>.Fail($"code_text is required for {row.ExampleId}.", "CODE_TEXT_REQUIRED");

        if (string.Equals(row.Chosen?.Trim(), row.Rejected?.Trim(), StringComparison.Ordinal))
            return OperationResult<CodeDocumentationTrainingExample>.Fail($"chosen equals rejected for {row.ExampleId}.", "CHOSEN_EQUALS_REJECTED");

        if (string.IsNullOrWhiteSpace(row.Rejected) || !row.Rejected.Contains("[WRONG:", StringComparison.Ordinal))
            return OperationResult<CodeDocumentationTrainingExample>.Fail($"rejected must contain [WRONG:] for {row.ExampleId}.", "MISSING_WRONG_ANNOTATION");

        var request = new CodeDocumentationRequest(
            row.ExampleId,
            row.SourceId ?? "",
            row.Repository ?? "",
            row.CommitSha ?? "",
            row.FilePath ?? "",
            row.SymbolId ?? "",
            row.CodeText,
            row.Calls ?? [],
            row.KnownCallees ?? [],
            row.AppSide ?? "",
            row.RuntimeFamily ?? "",
            row.TargetFramework ?? "",
            row.CSharpLanguageVersion ?? "",
            row.LanguageVersionConfidence ?? "",
            row.ProjectStyle ?? "",
            row.WebFramework ?? "",
            row.WebFrameworkVersion ?? "",
            row.FrontendFramework ?? "",
            row.FrontendFrameworkVersion ?? "",
            row.StackLabel ?? "",
            row.ArtifactKind ?? "",
            row.DocumentationLevel ?? "");

        return OperationResult<CodeDocumentationTrainingExample>.Ok(new CodeDocumentationTrainingExample(
            row.ExampleId,
            request,
            string.IsNullOrWhiteSpace(row.ExpectedUnitId)
                ? OrchestrationRoutingCurriculum.DocumentationDotNetUnitId
                : row.ExpectedUnitId,
            row.ExpectVersionGatePass,
            row.ExpectTransitGatePass,
            string.IsNullOrWhiteSpace(row.CorrectReason)
                ? "The row asks for version-aware bottom-up legacy .NET documentation."
                : row.CorrectReason,
            string.IsNullOrWhiteSpace(row.WrongRouteConsequence)
                ? "wrong specialist routing loses version-aware code documentation and caller/callee transit."
                : row.WrongRouteConsequence,
            row.ExpectSourceGatePass));
    }
}

public sealed record CodeDocumentationBatchRow
{
    public string ExampleId { get; init; } = "";
    public string? SourceId { get; init; }
    public string? Repository { get; init; }
    public string? CommitSha { get; init; }
    public string? FilePath { get; init; }
    public string? SymbolId { get; init; }
    public string CodeText { get; init; } = "";
    public CodeDocumentationCallEdge[]? Calls { get; init; }
    public CodeDocumentationCalleeSummary[]? KnownCallees { get; init; }
    public string? AppSide { get; init; }
    public string? RuntimeFamily { get; init; }
    public string? TargetFramework { get; init; }
    [JsonPropertyName("csharp_language_version")]
    public string? CSharpLanguageVersion { get; init; }
    public string? LanguageVersionConfidence { get; init; }
    public string? ProjectStyle { get; init; }
    public string? WebFramework { get; init; }
    public string? WebFrameworkVersion { get; init; }
    public string? FrontendFramework { get; init; }
    public string? FrontendFrameworkVersion { get; init; }
    public string? StackLabel { get; init; }
    public string? ArtifactKind { get; init; }
    public string? DocumentationLevel { get; init; }
    public string Chosen { get; init; } = "";
    public string Rejected { get; init; } = "";
    public string? ExpectedUnitId { get; init; }
    public bool ExpectVersionGatePass { get; init; } = true;
    public bool ExpectTransitGatePass { get; init; } = true;
    public string? CorrectReason { get; init; }
    public string? WrongRouteConsequence { get; init; }
    public bool ExpectSourceGatePass { get; init; } = true;
}
