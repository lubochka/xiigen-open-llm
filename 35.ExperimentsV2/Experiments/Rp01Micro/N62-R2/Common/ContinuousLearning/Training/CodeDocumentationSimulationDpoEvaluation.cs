// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
using System.Diagnostics; // Use the same actual monotonic stage clock as training.
using System.Text.Json; // The independent panel remains an explicitly pinned caller input.
using AgentInfrastructure; // Preserve existing typed results at the host boundary.
using AgentInfrastructure.Logical; // Reuse the exact material preparation owner for assessment.
using DPO; // Reuse the public response-token-normalized Student likelihood implementation.
using DPO.Models; // Use existing masks and author kinds without granting mutation rights.
using TensorCore.Core; // Count the actual shifted response targets in canonical masks.
using TransformerModel; // Validate/import the complete saved candidate schema before assessment.

namespace ContinuousLearning; // Evaluation is an explicit separate command, never part of the training critical path.

public sealed record SimulationDpoEvaluationOptions(string RepositoryRoot, string RunId, string PanelPath, string PanelSha256, // The independent owner provides a new frozen panel; no built-in Gold is read.
    string? CandidatePath, string? CandidateSha256, CodeDocumentationStudentGenerationSettings Settings, // Null candidate selects the same immutable saved B0 for control.
    Func<CancellationToken, SimulationDpoModelContext> LoadModels,
    SimulationDpoDomain Domain = SimulationDpoDomain.CodeDocumentation,
    Func<SimulationDpoReadOnlyStudent, SimulationDpoInputRow, CodeDocumentationStudentGenerationSettings, CancellationToken, object>? TranslationGeneration = null); // The trusted host composes the ordinary typed translation owner without a 14→27 dependency.

public static partial class CodeDocumentationSimulationDpoExperiment // This partial adds only measurements and raw ordinary-consumer generation.
{ // It contains no grader, optimizer, external judge, panel authoring or acceptance decision.
    public static OperationResult<string> Evaluate(SimulationDpoEvaluationOptions options, CancellationToken token = default) // Caller freezes panel/settings before main training.
    { // Refuse malformed destinations before reading any panel bodies.
        if (options is null || !Enum.IsDefined(options.Domain) || options.Domain == SimulationDpoDomain.EnRu && (options.CandidatePath is null || options.TranslationGeneration is null) ||
            !IsHash(options.PanelSha256) || options.RunId.Length is < 1 or > 128 || // The ENRU control must explicitly select C50; null/B0 is forbidden.
            !options.RunId.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_') || options.Settings.MaxNewTokens is < 0 or > 256 || // Fixed finite generation budget is part of the reported decoder binding.
            (options.CandidatePath is null) != (options.CandidateSha256 is null) || (options.CandidateSha256 is { } pin && !IsHash(pin))) // Both candidate path and exact pin are required together.
            return OperationResult<string>.Fail("Evaluation arguments are invalid.", "SIMULATION_DPO_EVALUATION_INVALID"); // No new training authority is issued.
        var root = Path.GetFullPath(options.RepositoryRoot); // The thin CLI supplies the fixed canonical root.
        var parent = Path.Combine(root, "training-artifacts", "experiments", "code-documentation-simulation"); // Keep raw answers/measurements in the existing private candidate surface.
        var directory = Path.Combine(parent, options.RunId); // A completed/partial assessment is never overwritten or silently resumed.
        if (Directory.Exists(directory) || File.Exists(directory) || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(parent)) return OperationResult<string>.Fail("Evaluation output exists or its boundary is invalid.", "SIMULATION_DPO_EVALUATION_EXISTS"); // No automatic call replay after interruption.
        Directory.CreateDirectory(parent); SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(directory); // Restrict only this new owned output.
        using var journal = new RunJournal(directory, options.RunId); // Each issued/completed case gets an immediate durable event.
        var started = Stopwatch.GetTimestamp(); var utcStart = DateTimeOffset.UtcNow; // Whole-window cost includes loading/materialization and failure.
        var scoringIssued = 0; var scoringCompleted = 0; var generationIssued = 0; var generationCompleted = 0; // Report missingness rather than inventing completed calls.
        var scores = new List<object>(); string? error = null; bool? stateUnchanged = null; // No result is considered a quality verdict here.
        try // A panel or native runtime error produces a narrow durable result.
        { // Panel contents are not selected, rewritten or tuned by this tool.
            IReadOnlyList<SimulationDpoInputRow> rows; IReadOnlyList<DpoVectorBatch> batches; SimulationDpoModelContext models;
            SimulationDpoReadOnlyStudent? translationStudent = null;
            using (journal.Stage("load_and_materialize", "evaluation")) // Measure actual input/import work for each arm.
            { // The exact caller panel is read once, with no discovery of historical exams.
                var bytes = File.ReadAllBytes(options.PanelPath); if (bytes.Length > 1024 * 1024 || Hash(bytes) != options.PanelSha256) throw new InvalidDataException("SIMULATION_DPO_PANEL_PIN_INVALID"); // Refuse changed or oversized panels.
                var lines = Strict.GetString(bytes).Split('\n'); var imported = new List<SimulationDpoInputRow>(); // Separate labels/metadata never enter model inputs.
                for (var index = 0; index < lines.Length; index++) // Preserve all physical records except the conventional final empty newline.
                { // Every record must supply exact text rather than generated fallback answers.
                    if (index == lines.Length - 1 && lines[index].Length == 0) break; using var document = JsonDocument.Parse(lines[index]); // Parse one exact independent case.
                    var row = document.RootElement; imported.Add(new(Text(row, "id"), Text(row, "prompt"), Text(row, "chosen"), Text(row, "rejected"))); // The panel schema needs only four text properties; other evidence remains with its owner.
                    if (options.Domain == SimulationDpoDomain.EnRu) _ = SimulationEnRuPrompt.Extract(imported[^1].Prompt);
                } // No case is silently skipped after parse/validation errors.
                if (imported.Count is < 1 or > 100 || imported.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() != imported.Count) throw new InvalidDataException("SIMULATION_DPO_PANEL_INVALID"); // Keep assessment finite and identities unambiguous.
                rows = imported.AsReadOnly(); models = options.LoadModels(token); // Control and treatment use the identical accepted saved-root tokenizer/profile.
                if (options.CandidatePath is { } candidate) // The caller may select only an existing isolated candidate, never arbitrary replacement model state.
                { // Validate exact file bytes and the full canonical base+LoRA schema before import.
                    var absolute = Path.GetFullPath(candidate); var prefix = Path.GetFullPath(parent) + Path.DirectorySeparatorChar; // Candidate custody stays inside this existing experiment owner.
                    if (!absolute.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(absolute) || Hash(File.ReadAllBytes(absolute)) != options.CandidateSha256) throw new InvalidDataException("SIMULATION_DPO_CANDIDATE_PIN_INVALID"); // A path name alone proves nothing.
                    if (options.Domain == SimulationDpoDomain.EnRu)
                        translationStudent = SimulationEnRuContinuation.OpenStudent(root, models, absolute, options.CandidateSha256!, token);
                    else
                    {
                        using var stream = File.OpenRead(absolute); _ = Require(LlmModel.ValidateFullCheckpointArtifact(stream, models.Policy.Config, new FullCheckpointLoraTopology(models.LoraConfig.Rank, models.LoraConfig.TargetModules), stream.Length, token));
                        stream.Position = 0; models.Policy.LoadCheckpoint(stream);
                    }
                } // Null candidate leaves the actual verified saved B0 control unchanged.
                batches = Require(LogicalDpoPreparedBatchMaterializer.Prepare(rows.Select(row => new LogicalDpoPreferenceExample(row.Prompt, row.Chosen, row.Rejected) // Exact same prompt/answer segmentation as the training objective.
                { ChosenAuthorKind = DpoPairSideAuthorKind.DeterministicGenerator, RejectedAuthorKind = DpoPairSideAuthorKind.DeterministicGenerator }).ToArray(), models.Tokenizer)); // The independent programmatic panel uses the existing truthful generator kind; this is no training permission.
                foreach (var batch in batches) _ = DpoExactPairMaterial.CaptureAndValidate(batch.Batch, models.Policy.Config.VocabSize, models.Policy.Config.ContextLen, models.Tokenizer.ComputeStateSha256(), DpoExactPairMaterial.Version); // Validate capacity and all prompt/response/padding masks.
                WriteNew(Path.Combine(directory, "intent.json"), new { options.RunId, options.Domain, options.PanelPath, options.PanelSha256, options.CandidatePath, candidateSha256 = options.CandidateSha256 ?? models.SourceWeightsSha256, options.Settings, settingsSha256 = CodeDocumentationStudentGeneration.SettingsSha256(options.Settings), tokenizerSha256 = models.Tokenizer.ComputeStateSha256(), pairCount = rows.Count, generationContract = options.Domain == SimulationDpoDomain.EnRu ? SimulationEnRuPrompt.Version : CodeDocumentationStudentGeneration.ContractVersion, role = "measurement_only_no_training_admission", automaticRetry = false }); // Bind actual chosen inputs/settings before Student calls.
            } // Whole load time stays separate from Student scoring/generation.
            var before = Snapshot(models.Policy); var parameters = models.Policy.Parameters().ToArray(); var flags = parameters.Select(x => x.Value.RequiresGrad).ToArray(); // Observe real model state and temporary inference flags.
            try // Disable autograd at actual input leaves, rather than assuming training:false alone suppresses graph construction.
            { // Evaluation never changes parameter values or trains prompt/panel labels.
                foreach (var parameter in parameters) parameter.Value.RequiresGrad = false; // Existing Tensor operations then have no trainable parameter leaves.
                var handle = options.Domain == SimulationDpoDomain.CodeDocumentation ? CodeDocumentationStudentHandle.BindVerified(models.Policy, models.Tokenizer, new("private_verified_evaluation", "csharp-code-documentation", "rp01-csharp-small", "v1", null, null, options.CandidateSha256 ?? models.SourceWeightsSha256)) : null;
                for (var index = 0; index < rows.Count; index++) // Paired scores remain matched by the independent opaque case ID.
                { // The independent owner, not the code publisher, later interprets law correctness.
                    token.ThrowIfCancellationRequested(); var row = rows[index]; var batch = batches[index].Batch; // Use prepared exact material without re-tokenizing metadata.
                    float chosen; float rejected; // Existing public likelihood returns response-token-normalized log probability.
                    using (journal.Stage("student_candidate_scoring_total", row.Id)) // This window contains exactly two real Student forward calls.
                    { // Actual issue/completion accounting survives a failed second forward.
                        scoringIssued++; journal.Event("score_issued", row.Id + ":chosen", new { ordinal = scoringIssued }); chosen = DpoTrainer.EvaluateAverageNextTokenLogProbability(models.Policy, batch.ChosenIds, batch.ChosenResponseMask!, token); scoringCompleted++; // Same shifted response targets as exact DPO, normalized for heldout reporting only.
                        scoringIssued++; journal.Event("score_issued", row.Id + ":rejected", new { ordinal = scoringIssued }); rejected = DpoTrainer.EvaluateAverageNextTokenLogProbability(models.Policy, batch.RejectedIds, batch.RejectedResponseMask!, token); scoringCompleted++; // The training objective itself remains summed response likelihood.
                    } // Never call this evaluator-only interval training or optimizer time.
                    var chosenTargets = TargetCount(batch.ChosenResponseMask!); var rejectedTargets = TargetCount(batch.RejectedResponseMask!); // Count the actual i+1 masked targets used by the canonical scorer.
                    if (!float.IsFinite(chosen) || !float.IsFinite(rejected) || chosenTargets == 0 || rejectedTargets == 0) throw new InvalidDataException("SIMULATION_DPO_SCORE_INVALID"); // Missing/nonfinite scores cannot produce a positive signal.
                    scores.Add(new { id = row.Id, chosenMean = chosen, rejectedMean = rejected, normalizedMargin = (double)chosen - rejected, chosenTargets, rejectedTargets, chosenSum = (double)chosen * chosenTargets, rejectedSum = (double)rejected * rejectedTargets, sumGap = (double)chosen * chosenTargets - (double)rejected * rejectedTargets }); // Both length-normalized and summed gaps are explicit.
                    WriteNew(Path.Combine(directory, $"score-{index:D4}.json"), scores[^1]); // Save each returned score immediately; never regenerate a missing case automatically.
                    if (options.Settings.MaxNewTokens > 0) // Free generation is explicitly selected and bounded in the frozen command.
                    { // Raw answers go only to the owner-only result, without a built-in correctness grade.
                        generationIssued++; journal.Event("generation_issued", row.Id, new { ordinal = generationIssued }); // Persist issue before the real ordinary-consumer call.
                        using var stage = journal.Stage("student_generation_total", row.Id); // Actual generation cost is separate from predictive scoring.
                        object generation = options.Domain == SimulationDpoDomain.EnRu
                            ? options.TranslationGeneration!(translationStudent!, row, options.Settings, token)
                            : CodeDocumentationStudentGeneration.Generate(handle!, new(row.Id, handle!.Identity.DomainSlug, row.Prompt), options.Settings, token); // ENRU reaches the real ordinary typed translation flow, with exactly one rebuilt prefix.
                        generationCompleted++; WriteNew(Path.Combine(directory, $"generation-{index:D4}.json"), new { id = row.Id, generation }); // No teacher calls or answer lookup replaces Student output.
                    } // MaxNewTokens=0 records the generation arm as not requested.
                } // Panel authorship and scientific grading remain outside this code executor's authority.
            } // Inference flags are restored even after cancellation or a failed case.
            finally { for (var index = 0; index < parameters.Length; index++) parameters[index].Value.RequiresGrad = flags[index]; } // Restore runtime flags without changing saved values.
            stateUnchanged = Same(before, Snapshot(models.Policy)); if (stateUnchanged != true || Hash(File.ReadAllBytes(models.SourceWeightsPath)) != models.SourceWeightsSha256) throw new InvalidOperationException("SIMULATION_DPO_EVALUATION_MUTATED_STATE"); // Verify actual in-memory and immutable source preservation.
        } // Scores/raw answers alone make no learning or promotion claim.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { error = exception.GetType().Name + ":" + exception.Message; journal.Event("failure", options.RunId, new { error }); } // Unknown/missing calls are explicit in the final receipt.
        var path = Path.Combine(directory, "evaluation-receipt.json"); // One new result is durable even for partial assessment.
        WriteNew(path, new { options.RunId, options.Domain, options.PanelSha256, candidateSha256 = options.CandidateSha256, utcStart, utcEnd = DateTimeOffset.UtcNow, wholeRunSeconds = Elapsed(started), scoringIssued, scoringCompleted, generationIssued, generationCompleted, stateUnchanged, scores, error, optimizerCalls = 0, externalTeacherCalls = 0, externalEvaluatorCalls = 0, qualityVerdict = "NOT_GRADED_BY_CODE_EXECUTOR" }); // Independent examiner later joins opaque IDs to the frozen law rubric.
        return error is null ? OperationResult<string>.Ok(path) : OperationResult<string>.Fail("Evaluation failed; inspect the preserved receipt.", error); // Do not silently repeat model calls.
    } // No closed panel is hardcoded or authored by this runner.
    private static int TargetCount(Tensor mask) => mask.Data.Skip(1).Count(value => value > 0); // Mirror only the public scorer's documented shifted target count, not its logsoftmax implementation.
} // All mathematical model evaluation is performed by the existing Student APIs.
