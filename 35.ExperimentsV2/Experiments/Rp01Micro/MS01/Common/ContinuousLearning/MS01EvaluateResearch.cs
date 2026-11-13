// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// MS01 card E-SFT-RESPONSE-MASK: the closed copy-only evaluation entry.
// The body is the native Evaluate with one allowed location adaptation: the output root is the
// assigned attempt root plus arm and state, and the candidate must live inside that same arm's
// checkpoints. Every other guard of the native method is preserved verbatim: panel pin, candidate
// pin and full typed checkpoint validation, materializer and exact-material capture, the no-grad
// flags, the issued/completed accounting, the state-unchanged check and the durable receipt.
// The native Evaluate itself is untouched and accepts no arbitrary root.

using System.Diagnostics;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DPO;
using DPO.Models;
using TensorCore.Core;
using TransformerModel;

namespace ContinuousLearning;

public static partial class CodeDocumentationSimulationDpoExperiment
{
    private const string MS01AttemptRoot =
        "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MS01/results/attempt-0001";

    public static OperationResult<string> EvaluateResearch(
        SimulationDpoEvaluationOptions options,
        string fixedStudyRoot,
        string fixedArm,
        string stateName,
        CancellationToken token = default)
    {
        // The only adaptation: the destination is the assigned attempt root, arm and state.
        if (options is null || !Enum.IsDefined(options.Domain) || options.Domain == SimulationDpoDomain.EnRu && (options.CandidatePath is null || options.TranslationGeneration is null) ||
            !IsHash(options.PanelSha256) || options.RunId.Length is < 1 or > 128 ||
            !options.RunId.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_') || options.Settings.MaxNewTokens is < 0 or > 256 ||
            (options.CandidatePath is null) != (options.CandidateSha256 is null) || (options.CandidateSha256 is { } pin && !IsHash(pin)))
            return OperationResult<string>.Fail("Evaluation arguments are invalid.", "SIMULATION_DPO_EVALUATION_INVALID");
        if (!string.Equals(Path.GetFullPath(fixedStudyRoot).Replace('\\', '/').TrimEnd('/'), MS01AttemptRoot, StringComparison.OrdinalIgnoreCase) ||
            fixedArm is not ("A" or "B") || stateName is not ("A0" or "A1" or "B0" or "B1") ||
            stateName[0] != fixedArm[0] || options.CandidatePath is null || options.CandidateSha256 is null)
            return OperationResult<string>.Fail("Research evaluation location is not the assigned one.", "MS01_EVALUATION_LOCATION_INVALID");

        var root = Path.GetFullPath(options.RepositoryRoot);
        var armRoot = Path.Combine(MS01AttemptRoot, fixedArm);
        var parent = Path.Combine(armRoot, "evaluation");
        var directory = Path.Combine(parent, stateName);
        if (Directory.Exists(directory) || File.Exists(directory) || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(parent))
            return OperationResult<string>.Fail("Evaluation output exists or its boundary is invalid.", "SIMULATION_DPO_EVALUATION_EXISTS");
        Directory.CreateDirectory(parent); SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(directory);
        using var journal = new RunJournal(directory, options.RunId);
        var started = Stopwatch.GetTimestamp(); var utcStart = DateTimeOffset.UtcNow;
        var scoringIssued = 0; var scoringCompleted = 0; var generationIssued = 0; var generationCompleted = 0;
        var scores = new List<object>(); string? error = null; bool? stateUnchanged = null;
        try
        {
            IReadOnlyList<SimulationDpoInputRow> rows; IReadOnlyList<DpoVectorBatch> batches; SimulationDpoModelContext models;
            using (journal.Stage("load_and_materialize", "evaluation"))
            {
                var bytes = File.ReadAllBytes(options.PanelPath);
                if (bytes.Length > 1024 * 1024 || Hash(bytes) != options.PanelSha256) throw new InvalidDataException("SIMULATION_DPO_PANEL_PIN_INVALID");
                var lines = Strict.GetString(bytes).Split('\n'); var imported = new List<SimulationDpoInputRow>();
                for (var index = 0; index < lines.Length; index++)
                {
                    if (index == lines.Length - 1 && lines[index].Length == 0) break;
                    using var document = JsonDocument.Parse(lines[index]);
                    var row = document.RootElement;
                    imported.Add(new(Text(row, "id"), Text(row, "prompt"), Text(row, "chosen"), Text(row, "rejected")));
                }

                if (imported.Count is < 1 or > 100 || imported.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() != imported.Count)
                    throw new InvalidDataException("SIMULATION_DPO_PANEL_INVALID");
                rows = imported.AsReadOnly(); models = options.LoadModels(token);

                // The candidate must be the arm's own checkpoint, validated as a full typed artifact.
                var candidate = options.CandidatePath!;
                var absolute = Path.GetFullPath(candidate);
                var prefix = Path.GetFullPath(Path.Combine(armRoot, "checkpoints")) + Path.DirectorySeparatorChar;
                if (!absolute.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
                    SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(absolute) ||
                    Hash(File.ReadAllBytes(absolute)) != options.CandidateSha256)
                    throw new InvalidDataException("SIMULATION_DPO_CANDIDATE_PIN_INVALID");
                using (var stream = File.OpenRead(absolute))
                {
                    _ = Require(LlmModel.ValidateFullCheckpointArtifact(stream, models.Policy.Config,
                        new FullCheckpointLoraTopology(models.LoraConfig.Rank, models.LoraConfig.TargetModules), stream.Length, token));
                    stream.Position = 0; models.Policy.LoadCheckpoint(stream);
                }

                batches = Require(LogicalDpoPreparedBatchMaterializer.Prepare(rows.Select(row => new LogicalDpoPreferenceExample(row.Prompt, row.Chosen, row.Rejected)
                { ChosenAuthorKind = DpoPairSideAuthorKind.DeterministicGenerator, RejectedAuthorKind = DpoPairSideAuthorKind.DeterministicGenerator }).ToArray(), models.Tokenizer));
                foreach (var batch in batches)
                    _ = DpoExactPairMaterial.CaptureAndValidate(batch.Batch, models.Policy.Config.VocabSize, models.Policy.Config.ContextLen, models.Tokenizer.ComputeStateSha256(), DpoExactPairMaterial.Version);
                WriteNew(Path.Combine(directory, "intent.json"), new
                {
                    options.RunId, options.Domain, options.PanelPath, options.PanelSha256, options.CandidatePath,
                    candidateSha256 = options.CandidateSha256 ?? models.SourceWeightsSha256, options.Settings,
                    settingsSha256 = CodeDocumentationStudentGeneration.SettingsSha256(options.Settings),
                    tokenizerSha256 = models.Tokenizer.ComputeStateSha256(), pairCount = rows.Count,
                    generationContract = CodeDocumentationStudentGeneration.ContractVersion,
                    role = "measurement_only_no_training_admission", automaticRetry = false,
                    researchLocation = new { attemptRoot = MS01AttemptRoot, arm = fixedArm, state = stateName }
                });
            }

            var before = Snapshot(models.Policy); var parameters = models.Policy.Parameters().ToArray();
            var flags = parameters.Select(x => x.Value.RequiresGrad).ToArray();
            try
            {
                foreach (var parameter in parameters) parameter.Value.RequiresGrad = false;
                var handle = CodeDocumentationStudentHandle.BindVerified(models.Policy, models.Tokenizer,
                    new("private_verified_evaluation", "csharp-code-documentation", "rp01-csharp-small", "v1", null, null,
                        options.CandidateSha256 ?? models.SourceWeightsSha256));
                for (var index = 0; index < rows.Count; index++)
                {
                    token.ThrowIfCancellationRequested(); var row = rows[index]; var batch = batches[index].Batch;
                    float chosen; float rejected;
                    using (journal.Stage("student_candidate_scoring_total", row.Id))
                    {
                        scoringIssued++; journal.Event("score_issued", row.Id + ":chosen", new { ordinal = scoringIssued });
                        chosen = DpoTrainer.EvaluateAverageNextTokenLogProbability(models.Policy, batch.ChosenIds, batch.ChosenResponseMask!, token);
                        scoringCompleted++;
                        scoringIssued++; journal.Event("score_issued", row.Id + ":rejected", new { ordinal = scoringIssued });
                        rejected = DpoTrainer.EvaluateAverageNextTokenLogProbability(models.Policy, batch.RejectedIds, batch.RejectedResponseMask!, token);
                        scoringCompleted++;
                    }

                    var chosenTargets = TargetCount(batch.ChosenResponseMask!); var rejectedTargets = TargetCount(batch.RejectedResponseMask!);
                    if (!float.IsFinite(chosen) || !float.IsFinite(rejected) || chosenTargets == 0 || rejectedTargets == 0)
                        throw new InvalidDataException("SIMULATION_DPO_SCORE_INVALID");
                    scores.Add(new
                    {
                        id = row.Id, chosenMean = chosen, rejectedMean = rejected, normalizedMargin = (double)chosen - rejected,
                        chosenTargets, rejectedTargets, chosenSum = (double)chosen * chosenTargets,
                        rejectedSum = (double)rejected * rejectedTargets,
                        sumGap = (double)chosen * chosenTargets - (double)rejected * rejectedTargets
                    });
                    WriteNew(Path.Combine(directory, $"score-{index:D4}.json"), scores[^1]);
                    if (options.Settings.MaxNewTokens > 0)
                    {
                        generationIssued++; journal.Event("generation_issued", row.Id, new { ordinal = generationIssued });
                        using var stage = journal.Stage("student_generation_total", row.Id);
                        object generation = CodeDocumentationStudentGeneration.Generate(handle,
                            new(row.Id, handle.Identity.DomainSlug, row.Prompt), options.Settings, token);
                        generationCompleted++;
                        WriteNew(Path.Combine(directory, $"generation-{index:D4}.json"), new { id = row.Id, generation });
                    }
                }
            }
            finally { for (var index = 0; index < parameters.Length; index++) parameters[index].Value.RequiresGrad = flags[index]; }

            stateUnchanged = Same(before, Snapshot(models.Policy));
            if (stateUnchanged != true || Hash(File.ReadAllBytes(models.SourceWeightsPath)) != models.SourceWeightsSha256)
                throw new InvalidOperationException("SIMULATION_DPO_EVALUATION_MUTATED_STATE");
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            error = exception.GetType().Name + ":" + exception.Message;
            journal.Event("failure", options.RunId, new { error });
        }

        var path = Path.Combine(directory, "evaluation-receipt.json");
        WriteNew(path, new
        {
            options.RunId, options.Domain, options.PanelSha256, candidateSha256 = options.CandidateSha256,
            arm = fixedArm, state = stateName, utcStart, utcEnd = DateTimeOffset.UtcNow, wholeRunSeconds = Elapsed(started),
            scoringIssued, scoringCompleted, generationIssued, generationCompleted, stateUnchanged, scores, error,
            optimizerCalls = 0, externalTeacherCalls = 0, externalEvaluatorCalls = 0,
            qualityVerdict = "NOT_GRADED_BY_CODE_EXECUTOR"
        });
        return error is null
            ? OperationResult<string>.Ok(path)
            : OperationResult<string>.Fail("Evaluation failed; inspect the preserved receipt.", error);
    }
}
