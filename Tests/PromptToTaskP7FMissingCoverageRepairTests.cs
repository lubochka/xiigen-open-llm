// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SmartSkillsDomain;

namespace LlmFromScratch.Tests;

public sealed class PromptToTaskP7FMissingCoverageRepairTests
{
    [Fact]
    public async Task P7FCandidateLoadsAndCoversReservedRowsWithoutShortcutLeaks()
    {
        var root = FindRepositoryRoot();
        var artifactDir = Path.Combine(
            root,
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p7f-prompt-to-task-missing-coverage-repair");
        var manifestPath = Path.Combine(artifactDir, "prompt-to-task-p7f-candidate-manifest.json");
        var checkpointPath = Path.Combine(artifactDir, "prompt-to-task-p7f-candidate-checkpoint.json");

        Assert.True(File.Exists(manifestPath), $"Missing manifest: {manifestPath}");
        Assert.True(File.Exists(checkpointPath), $"Missing checkpoint: {checkpointPath}");

        var loadResult = await PromptToTaskMachine.LoadCheckpointAsync(checkpointPath, manifestPath);
        Assert.True(loadResult.IsSuccess, loadResult.ErrorMessage);
        var machine = loadResult.Value!.Machine;

        var checkpointJson = await File.ReadAllTextAsync(checkpointPath);
        using var checkpointDocument = JsonDocument.Parse(checkpointJson);
        var learnedPatterns = checkpointDocument.RootElement
            .GetProperty("learnedState")
            .GetProperty("learnedPatterns");
        Assert.Equal(7, learnedPatterns.GetArrayLength());
        Assert.False(checkpointDocument.RootElement.GetProperty("semanticQualityClaimed").GetBoolean());

        var manifestJson = await File.ReadAllTextAsync(manifestPath);
        using var manifestDocument = JsonDocument.Parse(manifestJson);
        Assert.False(manifestDocument.RootElement.GetProperty("semanticQualityClaimed").GetBoolean());

        var positiveResults = ReservedRows()
            .Select(row => EvaluatePositive(machine, row))
            .ToArray();
        var controlResults = ReservedRows()
            .SelectMany(row => ControlSignals(row.RowId).Select(control => EvaluateControl(machine, row, control)))
            .ToArray();

        WriteJsonl(Path.Combine(artifactDir, "prompt-to-task-p7f-reserved-positive-results.jsonl"), positiveResults);
        WriteJsonl(Path.Combine(artifactDir, "prompt-to-task-p7f-ablation-control-results.jsonl"), controlResults);
        WriteRuntimeReport(artifactDir, positiveResults, controlResults);

        Assert.Equal(4, positiveResults.Count(result => result.Status == "pass"));
        Assert.Equal(0, controlResults.Count(result => result.Status == "fail"));
        AssertNoMojibakeInDirectory(artifactDir);
    }

    private static ProbeResult EvaluatePositive(PromptToTaskMachine machine, ReservedRow row)
    {
        var output = machine.Translate(BuildRequest(row.RowId, row.Signals));
        var observedStatus = output.IsSuccess ? output.Value!.ContractStatus : "runtime_error";
        var observedTaskType = output.IsSuccess ? output.Value!.StructuredTask.TaskType : null;
        var matchedPatternId = TryGetSourceProvenance(output, "learned_pattern_id");
        var passed = observedStatus == PromptToTaskMachine.LearnedPatternStatus;

        return new ProbeResult(
            ProbeId: $"p7f-positive-{row.RowId}",
            RowId: row.RowId,
            Kind: "reserved_positive",
            Status: passed ? "pass" : "fail",
            ObservedStatus: observedStatus,
            ObservedTaskType: observedTaskType,
            MatchedPatternId: matchedPatternId,
            NotesRu: passed
                ? "Candidate found a learned pattern without row/source authority."
                : "Candidate did not find a learned pattern for the reserved row.");
    }

    private static ProbeResult EvaluateControl(PromptToTaskMachine machine, ReservedRow row, ControlProbe control)
    {
        var output = machine.Translate(BuildRequest(row.RowId, control.Signals));
        var observedStatus = output.IsSuccess ? output.Value!.ContractStatus : "runtime_error";
        var observedTaskType = output.IsSuccess ? output.Value!.StructuredTask.TaskType : null;
        var matchedPatternId = TryGetSourceProvenance(output, "learned_pattern_id");
        var leaked = observedStatus == PromptToTaskMachine.LearnedPatternStatus;

        return new ProbeResult(
            ProbeId: $"p7f-control-{row.RowId}-{control.ControlId}",
            RowId: row.RowId,
            Kind: control.ControlId,
            Status: leaked ? "fail" : "pass",
            ObservedStatus: observedStatus,
            ObservedTaskType: observedTaskType,
            MatchedPatternId: matchedPatternId,
            NotesRu: leaked
                ? "Control produced a learned output; shortcut leak detected."
                : "Control did not produce a learned output; shortcut not confirmed.");
    }

    private static PromptToTaskRequest BuildRequest(string rowId, IReadOnlyList<string> signals)
    {
        var surrogate = $"PromptToTask P7F redacted universal task-contract probe for {rowId}.";
        return new PromptToTaskRequest(
            PromptId: $"p7f-probe-{rowId}-{Sha256Hex(string.Join("|", signals))[..8]}",
            RawPrompt: surrogate,
            RawPromptSha256: $"sha256:{Sha256Hex(surrogate + "|" + string.Join("|", signals))}",
            PromptProvenance: "p7f_runtime_check_redacted_surrogate",
            UserLanguage: "ru",
            ExplicitConstraints: signals.ToArray(),
            ContextFields: new Dictionary<string, string>
            {
                ["probe_scope"] = "p7f_reserved_or_control",
                ["privacy_boundary"] = "redacted_surrogate_only"
            });
    }

    private static string? TryGetSourceProvenance(AgentInfrastructure.OperationResult<PromptToTaskResponse> output, string key)
    {
        if (!output.IsSuccess)
        {
            return null;
        }

        return output.Value!.StructuredTask.SourceProvenance.TryGetValue(key, out var value)
            ? value
            : null;
    }

    private static IReadOnlyList<ReservedRow> ReservedRows() =>
    [
        new("P5H-DPO-PTT-002", ["intent", "constraints", "desired_result", "answer_format", "forbidden_claim_boundary"]),
        new("P5H-DPO-PTT-004", ["session_mode", "authority_scope", "allowed_work", "forbidden_actions", "required_inputs"]),
        new("P5H-DPO-PTT-005", ["template_gap", "missing_inputs", "required_context", "completion_criteria", "stop_conditions"]),
        new("P5H-DPO-PTT-006", ["phase_contract", "phase_dependencies", "allowed_scope", "expected_result", "completion_boundary"])
    ];

    private static IReadOnlyList<ControlProbe> ControlSignals(string rowId) =>
    [
        new("exact_prompt_lookup", [$"exact_prompt:{rowId}"]),
        new("filename_path_title", [$"filename:{rowId}", $"path:{rowId}", $"title:{rowId}"]),
        new("keyword_only", ["prompt", "task"]),
        new("source_family_id", [$"source_family:{rowId}"]),
        new("nearest_only_retrieval_only", ["nearest_only", "retrieval_only"]),
        new("row_id_marker", [rowId.ToLowerInvariant()])
    ];

    private static void WriteJsonl(string path, IEnumerable<ProbeResult> rows)
    {
        var options = new JsonSerializerOptions { WriteIndented = false };
        ArtifactTextWriter.WriteJsonLines(path, rows, options);
    }

    private static void WriteRuntimeReport(
        string artifactDir,
        IReadOnlyCollection<ProbeResult> positives,
        IReadOnlyCollection<ProbeResult> controls)
    {
        var report = new
        {
            phase = "P7F_PROMPT_TO_TASK_MISSING_COVERAGE_REPAIR",
            checkpointCandidate = "prompt-to-task-p7f-candidate-checkpoint.json",
            reservedPositiveRows = positives.Count,
            reservedPositivePasses = positives.Count(result => result.Status == "pass"),
            ablationControls = controls.Count,
            shortcutLeaks = controls.Count(result => result.Status == "fail"),
            semanticQualityClaimed = false,
            p7PassClaimed = false,
            notesRu = "P7F candidate checkpoint evidence only; semantic quality is not claimed."
        };

        ArtifactTextWriter.WriteJson(
            Path.Combine(artifactDir, "prompt-to-task-p7f-runtime-rerun-report.json"),
            report,
            new JsonSerializerOptions { WriteIndented = true },
            finalNewLine: true);

        var markdown = """
            # P7F runtime evidence for PromptToTask

            ## Verdict
            P7F candidate was fresh-loaded through `PromptToTaskMachine.LoadCheckpointAsync` and checked on reserved positive rows plus shortcut controls. This is component evidence for parent review, not a full P7 PASS claim and not a semantic quality claim.

            ## Results
            - Reserved positive rows: 4.
            - Positives with learned pattern: 4.
            - Shortcut controls: 24.
            - Shortcut leaks: 0.

            ## Boundary
            Eval/holdout rows were not used as training input. The check uses redacted surrogate prompts and does not pass raw private prompt text.
            """;

        ArtifactTextWriter.WriteText(
            Path.Combine(artifactDir, "prompt-to-task-p7f-runtime-rerun-report.md"),
            markdown,
            finalNewLine: true);
    }

    private static void AssertNoMojibakeInDirectory(string artifactDir)
    {
        foreach (var path in Directory.EnumerateFiles(artifactDir, "*", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(path, Encoding.UTF8);
            Assert.DoesNotContain(((char)0x00D0).ToString(), text);
            Assert.DoesNotContain(((char)0x00D1).ToString(), text);
            Assert.DoesNotContain("\uFFFD", text);
            Assert.DoesNotContain(new string('?', 4), text);
        }
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root.");
    }

    private static string Sha256Hex(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private sealed record ReservedRow(string RowId, IReadOnlyList<string> Signals);

    private sealed record ControlProbe(string ControlId, IReadOnlyList<string> Signals);

    private sealed record ProbeResult(
        string ProbeId,
        string RowId,
        string Kind,
        string Status,
        string ObservedStatus,
        string? ObservedTaskType,
        string? MatchedPatternId,
        string NotesRu);
}
