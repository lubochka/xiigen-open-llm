// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
using System.Diagnostics; // Measure monotonic stage windows as well as UTC boundaries.
using System.Runtime.InteropServices; // Hash actual float bits without formatting numbers.
using System.Security.Cryptography; // Bind every input and saved state to actual bytes.
using System.Text; // Refuse lossy text decoding and keep exact source strings.
using System.Text.Json; // Import metadata separately from the three Student text fields.
using AgentInfrastructure; // Reuse the existing typed refusal contract.
using AgentInfrastructure.Logical; // Reuse preparation and neutral content projection.
using DPO; // The existing trainer owns gradients, optimizer, rollback and checkpoint publication.
using DPO.Models; // Reuse existing generator admission and exact response-only material.
using LoRA; // Reuse project recipient topology and RNG identities.
using Tokenizer; // No new tokenizer is introduced by this runner.
using TransformerModel; // Load and reopen the canonical complete checkpoint schema.

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Rp01SimulationDpo")] // Only the exact trusted offline host can enter the mutation-authority composition below.

namespace ContinuousLearning; // Keep trusted admission composition in its existing owner assembly.

public sealed record SimulationDpoModelContext(LlmModel Policy, LlmModel Reference, BpeTokenizer Tokenizer, // Separate loaded storage is required for the frozen reference.
    LoraConfig LoraConfig, LoraPairRandomSeedAuthority Seeds, string SourceWeightsPath, string SourceWeightsSha256, // These identify actual verified saved B0 bytes.
    Func<CancellationToken, LlmModel> CreateReadbackModel); // The CLI uses the same canonical saved-root loader for the fresh destination.

internal sealed record SimulationDpoRunOptions(string RepositoryRoot, string RunId, string Mode, // Only the trusted offline host can supply mutable-run inputs.
    string InputPath, string InputSha256, int ExpectedPairCount, int BatchSize, int MaximumUpdates, double? WallBudgetSeconds, // The finite main schedule is frozen by the caller before execution.
    Func<CancellationToken, SimulationDpoModelContext> LoadModels,
    SimulationDpoDomain Domain = SimulationDpoDomain.CodeDocumentation); // Only the explicit ENRU arm continues the pinned research state.

public sealed record SimulationDpoInputRow(string Id, string Prompt, string Chosen, string Rejected); // Student receives only these three strings; ID is journal-only.

/// <summary>A bounded offline experiment through the existing generator admission, exact material and project-LoRA trainer.</summary>
public static partial class CodeDocumentationSimulationDpoExperiment // This is not a production publication or model-adoption endpoint.
{ // No external teacher/evaluator is constructed or invoked here.
    private static readonly UTF8Encoding Strict = new(false, true); // Invalid source text must refuse before mutation.
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true }; // Human-inspectable durable receipts use existing web naming.
    public const string ReplayError = "SIMULATION_DPO_ATTEMPT_ALREADY_ISSUED"; // A different run ID cannot replay a possibly consumed dataset.

    public static OperationResult<IReadOnlyList<SimulationDpoInputRow>> Inspect(string inputPath, string expectedSha256, int expectedCount,
        SimulationDpoDomain domain = SimulationDpoDomain.CodeDocumentation) // Read-only import has no model or artifact effects.
    { // The allowed source is genuinely programmatically materialized simulation data.
        try // Preserve a typed refusal for malformed bytes rather than accepting a partial file.
        { // Read and pin the complete input once; never trim or rewrite pair text.
            if (!Enum.IsDefined(domain) || domain == SimulationDpoDomain.EnRu &&
                (expectedSha256 != SimulationEnRuContinuation.DatasetSha256 || expectedCount != 500)) return InvalidInput();
            var bytes = File.ReadAllBytes(inputPath); // Caller names the exact source; no corpus discovery occurs.
            if (!IsHash(expectedSha256) || bytes.Length > 8 * 1024 * 1024 || Hash(bytes) != expectedSha256) return InvalidInput(); // Exact input pin is mandatory.
            var lines = Strict.GetString(bytes).Split('\n'); // JSON escapes preserve literal prompt newlines.
            var rows = new List<SimulationDpoInputRow>(); // Metadata does not enter this text-only carrier.
            var ids = new HashSet<string>(StringComparer.Ordinal); // Opaque IDs are checked independently of content.
            var prompts = new HashSet<string>(StringComparer.Ordinal); // Duplicate prompts would overweight repeated records.
            var triplets = new HashSet<string>(StringComparer.Ordinal); // Content-bound fingerprints also reject duplicate triplets.
            for (var index = 0; index < lines.Length; index++) // Every physical record is either parsed or refused.
            { // Permit only the final empty newline, not missing interior records.
                if (index == lines.Length - 1 && lines[index].Length == 0) break; // A standard trailing newline is not a row.
                using var document = JsonDocument.Parse(lines[index]); // Invalid JSON never becomes a skipped training example.
                var row = document.RootElement; // Validate source role before constructing any trainer material.
                var allowed = new[] { "id", "dataset_version", "domain", "pair_kind", "provenance", "prompt", "chosen", "rejected", "metadata" }; // No extra origin/model/source framework is accepted.
                if (row.ValueKind != JsonValueKind.Object || row.EnumerateObject().Any(x => !allowed.Contains(x.Name)) || // Reject unknown fields instead of silently relabelling arbitrary responses.
                    row.EnumerateObject().Select(x => x.Name).Distinct(StringComparer.Ordinal).Count() != row.EnumerateObject().Count() || // Repeated JSON keys are ambiguous even if their values agree.
                    row.GetProperty("provenance").GetString() != "simulation" ||
                    row.GetProperty("domain").GetString() != (domain == SimulationDpoDomain.EnRu ? "en-ru" : "csharp") ||
                    row.GetProperty("pair_kind").GetString() != (domain == SimulationDpoDomain.EnRu ? "TRANSLATION" : "CODE_TO_DOC") ||
                    domain == SimulationDpoDomain.EnRu && row.GetProperty("dataset_version").GetString() != "simulation-enru-500-v1") return InvalidInput();
                var value = new SimulationDpoInputRow(Text(row, "id"), Text(row, "prompt"), Text(row, "chosen"), Text(row, "rejected")); // Keep raw strings exactly as authored.
                if (domain == SimulationDpoDomain.EnRu) _ = SimulationEnRuPrompt.Extract(value.Prompt); // The same single-prefix builder is used by the translation consumer.
                if (value.Id.Length > 256 || value.Id.Any(char.IsControl) || value.Chosen == value.Rejected || // A tie is not a binary preference.
                    !ids.Add(value.Id) || !prompts.Add(value.Prompt)) return InvalidInput(); // Reject duplicates before any model load.
                var admitted = DpoTrainerPairCustoms.CreateAdmitted(value.Prompt, value.Chosen, value.Rejected, // Reuse content-framed fingerprints rather than concatenation hashes.
                    DpoPairSideAuthorKind.DeterministicGenerator, DpoPairSideAuthorKind.DeterministicGenerator, "simulation"); // The actual deterministic materializer is the existing author kind.
                if (!triplets.Add(admitted.Admission.ContentFingerprintSha256)) return InvalidInput(); // Content identity is independent of row naming.
                rows.Add(value); // Only accepted exact texts reach the immutable import result.
            } // No subset is silently promoted to the requested dataset.
            return rows.Count == expectedCount && expectedCount is > 0 and <= 500 // Bound this experiment without changing production wrapper caps.
                ? OperationResult<IReadOnlyList<SimulationDpoInputRow>>.Ok(Array.AsReadOnly(rows.ToArray())) : InvalidInput(); // All expected rows must be present.
        } // Read-only inspection does not issue a mutation lease.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { return InvalidInput(); } // Fail closed on malformed input/I/O without dumping private text.
    } // Import success is not permission or learning evidence.

    internal static OperationResult<string> Run(SimulationDpoRunOptions options, CancellationToken cancellationToken = default) // Mutation-authority composition is restricted to the existing trusted host boundary.
    { // Validate the schedule and paths before creating any attempt.
        if (!ValidOptions(options)) return OperationResult<string>.Fail("Finite simulation experiment arguments are invalid.", "SIMULATION_DPO_ARGUMENTS_INVALID"); // Calibration is one excluded pair and one update; main is exactly one 500-pair epoch.
        var root = Path.GetFullPath(options.RepositoryRoot); // Canonical repository ownership is supplied by the thin trusted CLI.
        var experimentRoot = Path.Combine(root, "training-artifacts", "experiments", "code-documentation-simulation"); // All learned state stays inside the canonical repository.
        var runRoot = Path.Combine(experimentRoot, options.RunId); // A run cannot replace another run's output.
        var issuedPath = Path.Combine(experimentRoot, (options.Domain == SimulationDpoDomain.EnRu ? "en-ru-" : "") + options.Mode + "-" + options.InputSha256 + ".issued.json"); // Content/domain/arm identity survives changed run IDs; legacy C# marker names stay valid.
        if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(experimentRoot) || File.Exists(issuedPath)) return RefuseReplay(); // Refuse before any input/model work if the effect may already exist.
        if (Directory.Exists(runRoot) || File.Exists(runRoot)) return OperationResult<string>.Fail("The run directory already exists; reconcile its history.", "SIMULATION_DPO_RUN_EXISTS"); // Interrupted attempts are never reset.
        Directory.CreateDirectory(experimentRoot); // This creates no worktree or alternate source tree.
        SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(runRoot); // Restrict only this newly owned candidate/journal directory.
        using var journal = new RunJournal(runRoot, options.RunId); // The journal opens create-new and flushes each observation.
        var wholeStart = Stopwatch.GetTimestamp(); var utcStart = DateTimeOffset.UtcNow; // Include failed load and materialization costs.
        SimulationDpoModelContext? models = null; // Effect readback remains optional if loading never succeeded.
        var initialStep = options.Domain == SimulationDpoDomain.EnRu ? SimulationEnRuContinuation.SourceStep : 0;
        var issued = 0; var completed = 0; var presentations = 0; var error = (string?)null; // Completed counts NEW updates; global step includes the verified source.
        var consumedIds = new HashSet<string>(StringComparer.Ordinal); // Count unique actually completed row IDs separately from presentations.
        var stepTimings = new List<DpoStepVectorTiming>(options.MaximumUpdates); // One small in-memory observation per issued call, including a possible thrown call.
        var timingJournalWriteFailures = 0; // Optional timing I/O cannot turn a returned committed update into a trainer failure.
        object? effect = null; string? checkpoint = null; DpoTrainingOptimizerSettings? optimizerSettings = null; // A null result is never labelled a saved candidate or measured optimizer configuration.
        try // Every nonfatal runtime refusal is durably reported with the actual issued/completed boundary.
        { // No production model pointer or accepted checkpoint is changed.
            WriteNew(Path.Combine(runRoot, "intent.json"), new { options.RunId, options.Mode, options.Domain, options.InputPath, options.InputSha256, options.ExpectedPairCount, options.BatchSize, options.MaximumUpdates, options.WallBudgetSeconds, initialGlobalStep = initialStep, finalGlobalStep = initialStep + options.MaximumUpdates, referencePolicy = initialStep == 50 ? "separate_frozen_actual_C50_copy" : "separate_frozen_B0_copy", learningRate = 1e-5f, dpoBeta = .1f, epochs = 1, provenance = "simulation", authority = "Current human work order authorizes these generated simulation pairs and an isolated development candidate.", productionPublication = false, sourcePolicy = "existing DeterministicGenerator only" }); // Describe real authorization, never a fabricated license/passport.
            IReadOnlyList<SimulationDpoInputRow> rows; IReadOnlyList<DpoVectorBatch> batches; DpoTrainingContentBatch content; // One materialized batch list survives all updates.
            using (journal.Stage("load_and_materialize", "load")) // UTC plus Stopwatch measures the actual deferred load and preparation.
            { // The canonical source and tokenizer are loaded before deriving any tensor masks.
                rows = Require(Inspect(options.InputPath, options.InputSha256, options.ExpectedPairCount, options.Domain)); // Exact count/hash/duplicate validation runs before model allocation.
                if (options.Domain == SimulationDpoDomain.EnRu) _ = SimulationEnRuContinuation.ReadState(root); // Refuse missing/partial full research state before allocating Student.
                cancellationToken.ThrowIfCancellationRequested(); models = options.LoadModels(cancellationToken); // This callback invokes the existing saved-root loader, not random baseline construction.
                if (options.Domain == SimulationDpoDomain.EnRu) models = SimulationEnRuContinuation.ImportSource(root, models, cancellationToken); // Canonical root supplies topology only; both actual starting copies are complete C50.
                if (Hash(File.ReadAllBytes(models.SourceWeightsPath)) != models.SourceWeightsSha256 || // Recheck the actual immutable B0 bytes used by both copies.
                    ReferenceEquals(models.Policy, models.Reference) || models.LoraConfig.Dropout != 0) throw new InvalidOperationException("SIMULATION_DPO_SAVED_ROOT_INVALID"); // This bounded path never replaces root/dropout semantics.
                foreach (var parameter in models.Reference.Parameters()) parameter.Value.RequiresGrad = false; // The reference is an independent frozen copy for the entire main run.
                var governance = new TrainingDataGovernanceDecision(true, false, new[] { "sha256:" + options.InputSha256 }); // The digest identifies the exact human-authorized programmatic source.
                var rich = rows.Select(row => new DpoPair(row.Prompt, row.Chosen, row.Rejected, options.Domain == SimulationDpoDomain.EnRu ? "translation" : "code-documentation", utcStart) // Existing governance validates the exact pairs, without external-teacher passports.
                { GovernanceDecision = governance, ChosenAuthorship = new(DpoPairSideAuthorKind.DeterministicGenerator), RejectedAuthorship = new(DpoPairSideAuthorKind.DeterministicGenerator) }).ToArray(); // These are existing internal typed fields, never new payload metadata.
                _ = Require(TrainingDataGovernanceAdmission.Validate(rich)); content = Require(DpoPairTrainingContentAuthority.Project(rich)); // Retain the existing license/quarantine/evidence checks and content projector.
                var preferences = rich.Select(row => new LogicalDpoPreferenceExample(row.Prompt, row.Chosen, row.Rejected) // Metadata and labels cannot enter Student tokenization.
                { ChosenAuthorKind = row.ChosenAuthorship!.AuthorKind, RejectedAuthorKind = row.RejectedAuthorship!.AuthorKind }).ToArray(); // Both typed authors survive the normal preparation bridge.
                batches = Require(LogicalDpoPreparedBatchMaterializer.Prepare(preferences, models.Tokenizer)); // Existing PrepareExecution -> BuildBatch -> exact UTF-8 material, once.
                for (var index = 0; index < batches.Count; index++) // Prove every response mask before requesting mutation rights.
                { // Existing canonical validation enforces lossless segments, vocabulary, capacity, content and padding.
                    var batch = DpoExactPairMaterial.CaptureAndValidate(batches[index].Batch, models.Policy.Config.VocabSize, models.Policy.Config.ContextLen, models.Tokenizer.ComputeStateSha256(), DpoExactPairMaterial.Version); // No prompt truncation or prompt targets.
                    var material = batch.ExactMaterial!; // This is returned only after exact material validation.
                    journal.Event("material", rows[index].Id, new { material.DigestSha256, material.PromptLength, chosenResponseTokens = material.ChosenLength - material.PromptLength, rejectedResponseTokens = material.RejectedLength - material.PromptLength, material.SequenceLength }); // Token counts describe actual prepared inputs.
                } // No gradient work occurs in material validation.
            } // The stage includes tokenizer materialization and canonical B0 loads.
            var loaded = models!; // Only the successful load branch reaches training composition.
            var beforePolicy = Snapshot(loaded.Policy); var beforeReference = Snapshot(loaded.Reference); // Hash all actual named parameter bits before mutation.
            var adapters = Enumerable.Range(0, loaded.Policy.Blocks.Count).SelectMany(block => loaded.LoraConfig.TargetModules.Select(target => (LoraLinear)loaded.Policy.GetAttachedProjectionAdapter(block, target)!)).ToArray(); // Use only actually installed existing project recipients.
            var recipient = DpoProjectLoraRecipient.Capture(loaded.Policy, loaded.Reference, adapters, loaded.LoraConfig); // Existing recipient verifies frozen base/reference and allowed keyed LoRA parameters.
            var profile = DpoExecutionProfile.Capture(batches[0].Batch, recipient.Bindings); // Reuse canonical exact material/framing/objective and recipient identity.
            var legal = Require(LegalTrainingTeacherAdmissionAuthority.Create([], 0, RandomNumberGenerator.GetBytes(32))); // Existing generic programmatic route has no external allowlist and a zero call budget.
            var live = true; // Scope the existing revocable admission owner to this one native execution.
            var authority = ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(legal.ValidateBatch, () => live, null, // Reuse actual validators; no fake provider, model, license or digest.
                legal.SealLogicalCapabilityReplay, legal.VerifyLogicalCapabilityReplay, legal.SealLogicalCapabilityExecutableManifest, legal.VerifyLogicalCapabilityExecutableManifest, legal.ValidateBatch); // The existing content-bound admission policy remains authoritative.
            using var lease = Require(options.Domain == SimulationDpoDomain.EnRu
                ? ((IServerOwnedDpoTrainingLeaseBroker)authority).AcquireTranslationTrainingLease(content, options.RunId)
                : ((IServerOwnedDpoTrainingLeaseBroker)authority).AcquireCodeDocumentationTrainingLease(content, options.RunId)); // Reuse the actual domain-owned namespaces.
            var mutation = Require(lease.IssueDpoTrainerMutationAdmissionCapability()); // Only the existing live lease can mint mutation authority.
            var topology = LoraRandomStateIdentity.ComputeTopologySha256(loaded.Policy.Config, loaded.LoraConfig, loaded.Seeds); // RNG state retains the production identity frame.
            var loraIdentity = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(loaded.LoraConfig, loaded.Seeds); // Preserve the seeded adapter contract.
            var continuation = options.Domain == SimulationDpoDomain.EnRu ? SimulationEnRuContinuation.ReadState(root, topology, loraIdentity, profile) : null;
            var checkpointRoot = Path.Combine(runRoot, "checkpoints"); SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(checkpointRoot); // Candidate state is private and isolated from canonical accepted checkpoint pointers.
            var activeStep = 0; // Actual callback IDs remain attached to their transactional StepVector call.
            var observer = new DpoTrainerPublicationObserver // Existing publication retains its rollback, create-new files and ownership proofs.
            { // Inner checkpoint serialization is measured only at the actual existing callback boundary.
                ArtifactOwnershipProven = proof => journal.Event("artifact", "step-" + proof.Step, new { proof.Kind, proof.Path, proof.Sha256, proof.Length }), // Keep independently inspectable actual file ownership identities.
                BeforePolicySerialization = _ => journal.Begin("checkpoint_serialization", $"step-{activeStep:D6}"), // This timestamp precedes the actual full checkpoint write.
                AfterPolicySerializationBeforeProof = _ => journal.End("checkpoint_serialization", $"step-{activeStep:D6}", "COMPLETED"), // It excludes later move/proof cost; the outer step still includes that cost.
                BeforePolicyFinalMove = bundle => // Persist the real portable optimizer and RNG before the final policy/metrics commit marker.
                { // A persistence failure triggers the trainer's existing in-memory rollback.
                    using var stage = journal.Stage("optimizer_and_rng_save", "step-" + bundle.Step); // This is serialization/readback, never optimizer-only compute.
                    var saved = Require(RepositoryOwnedDpoOptimizerStateFile.WriteAndReadBack(checkpointRoot, bundle.Step, bundle.OptimizerState, cancellationToken)); // Actual optimizer slots and step are saved/reopened by the canonical file owner.
                    if (bundle.RandomState is null) throw new InvalidOperationException("SIMULATION_DPO_RANDOM_STATE_MISSING"); // Do not silently discard resumable state.
                    var random = Require(RepositoryOwnedDpoRandomStateFile.WriteCreateNew(Path.Combine(checkpointRoot, RepositoryOwnedDpoRandomStateFile.BuildFileName(bundle.Step)), bundle.RandomState)); // Actual adapter cursors survive process exit.
                    WriteNew(Path.Combine(checkpointRoot, $"state-{bundle.Step:D6}.json"), new { bundle.Step, saved.OptimizerStateSha256, saved.OptimizerStateByteLength, random.Sha256, random.ByteLength, bundle.TrialWeightsSha256, bundle.TrialWeightsByteLength, bundle.ExecutionProfile }); // Bind actual persisted state without fabricating a production manifest.
                }, // Policy and metrics still publish atomically through existing trainer ownership rules.
                PublicationFailed = () => journal.Event("publication_failed", "active-step", new { effect = "RECONCILE_BEFORE_RETRY" }) // Preserve partial state evidence; no automatic new run or deletion occurs.
            }; // A fresh trainer is created once, never once per pair or minibatch.
            var trainer = DpoTrainer.CreateForProjectLora(loaded.Policy, loaded.Reference, checkpointRoot, mutation, recipient, profile, continuation?.Proof, observer, // The existing trainer owns DPO math and allowed-parameter updates.
                step => loaded.Policy.CaptureRandomState(topology, loraIdentity, step), state => loaded.Policy.RestoreRandomState(state, new(topology, loraIdentity))); // Real RNG capture/rollback remains continuous.
            optimizerSettings = trainer.OptimizerSettings; // Record the actual existing optimizer values used by this constructed trainer.
            if (options.Domain == SimulationDpoDomain.EnRu && optimizerSettings != new DpoTrainingOptimizerSettings(1e-5f, .9f, .95f, .01f, 1e-8f))
                throw new InvalidOperationException("SIMULATION_ENRU_OPTIMIZER_SETTINGS_MISMATCH");
            if (continuation is not null)
            {
                if (!trainer.TryRestoreContinuationState(continuation.Random, continuation.Optimizer) ||
                    !trainer.ExportOptimizerState().ToBytes().SequenceEqual(continuation.Optimizer.ToBytes()) ||
                    !JsonSerializer.SerializeToUtf8Bytes(loaded.Policy.CaptureRandomState(topology, loraIdentity, initialStep)).SequenceEqual(JsonSerializer.SerializeToUtf8Bytes(continuation.Random)))
                    throw new InvalidOperationException("SIMULATION_ENRU_CONTINUATION_RESTORE_INVALID");
                journal.Event("continuation_restored", "step-000050", new { continuation.Proof.SourceIdentity, optimizerStep = trainer.ExportOptimizerState().Step, randomStep = continuation.Random.RandomStateStep, weightsSha256 = loaded.SourceWeightsSha256, referenceWeightsSha256 = loaded.SourceWeightsSha256 });
            }
            WriteNew(issuedPath, new { options.RunId, options.Mode, options.Domain, options.InputSha256, initialGlobalStep = initialStep, pairIds = rows.Select(x => x.Id), utc = DateTimeOffset.UtcNow, status = "ISSUED_EFFECT_UNKNOWN_UNTIL_COMPLETED_JOURNAL", automaticReplay = false }); // Durable create-new once guard precedes the first possible optimizer effect.
            using var heartbeat = new Timer(_ => { var process = Process.GetCurrentProcess(); Console.WriteLine(JsonSerializer.Serialize(new { activeRun = options.RunId, pid = process.Id, elapsedSeconds = Elapsed(wholeStart), issued = Volatile.Read(ref issued), completed = Volatile.Read(ref completed), workingSetBytes = process.WorkingSet64, cpuSeconds = process.TotalProcessorTime.TotalSeconds })); }, null, TimeSpan.Zero, TimeSpan.FromSeconds(15)); // Observe only this owned process; no timeout or foreign process control.
            for (var offset = 0; offset < rows.Count; offset += options.BatchSize) // Exactly one finite epoch sees every row once on success.
            { // Cancellation/budget are checked between transactional optimizer calls.
                cancellationToken.ThrowIfCancellationRequested(); // No automatic deadline is imposed on calibration.
                if (options.WallBudgetSeconds is { } budget && Elapsed(wholeStart) >= budget) throw new OperationCanceledException("SIMULATION_DPO_WALL_BUDGET_REACHED"); // Main's caller-frozen budget stops before the next update, never replays a partial step.
                var step = initialStep + completed + 1; var count = Math.Min(options.BatchSize, rows.Count - offset); // Global state continues while NEW updates remain separately bounded.
                if (completed >= options.MaximumUpdates || trainer.ExportOptimizerState().Step != step - 1) throw new InvalidOperationException("SIMULATION_DPO_UPDATE_BUDGET_EXCEEDED");
                var id = $"step-{step:D6}"; var selected = batches.Skip(offset).Take(count).ToArray(); // The same materialized exact tensor carriers enter the existing trainer.
                WriteNew(Path.Combine(runRoot, id + ".issued.json"), new { options.RunId, step, pairIds = rows.Skip(offset).Take(count).Select(x => x.Id), utc = DateTimeOffset.UtcNow, optimizerBefore = trainer.ExportOptimizerState().Step }); // Durable issuance distinguishes unknown effects after interruption.
                var timing = new DpoStepVectorTiming(); stepTimings.Add(timing); // Allocate observation before any update effect.
                activeStep = step; issued++; // Count actual calls only immediately before entering StepVector.
                DpoMetrics metrics; // Outer timing includes trainer transaction, forwards, backwards, optimizer and checkpoint/state publication.
                using (journal.Stage("training_step_total", id)) metrics = trainer.StepVector(selected, step, timing); // Existing StepVector calls ApplyOptimizer once for the entire selected minibatch.
                completed++; presentations += count; foreach (var row in rows.Skip(offset).Take(count)) consumedIds.Add(row.Id); // Only returned committed updates count as completed presentations.
                if (!journal.TryStepTiming(id, timing)) timingJournalWriteFailures++; // Observe after counters advance; a diagnostic write failure cannot undo the returned effect.
                WriteNew(Path.Combine(runRoot, id + ".completed.json"), new { options.RunId, step, pairCount = count, optimizerAfter = trainer.ExportOptimizerState().Step, metrics, utc = DateTimeOffset.UtcNow }); // This is observed training effect, not a heldout quality verdict.
            } // Reference and optimizer objects remain unchanged across update boundaries.
            live = false; // Prevent further mutation issuance through the scoped existing authority.
            var finalStep = initialStep + completed;
            checkpoint = Path.Combine(checkpointRoot, DpoTrainer.BuildPolicyCheckpointFileName(finalStep)); // The actual GLOBAL update identifies every saved state file.
            using (journal.Stage("checkpoint_optimizer_rng_reopen", "final")) // Readback has its own cost rather than pretending it was optimizer compute.
            { // Reopen actual full LoRA checkpoint plus portable optimizer/RNG using existing codecs.
                using var stream = File.OpenRead(checkpoint); // The existing published private candidate is the only reopened weights path.
                _ = Require(LlmModel.ValidateFullCheckpointArtifact(stream, loaded.Policy.Config, new FullCheckpointLoraTopology(loaded.LoraConfig.Rank, loaded.LoraConfig.TargetModules), stream.Length, cancellationToken)); // Validate all base and adapter keys/shapes before import.
                stream.Position = 0; var fresh = loaded.CreateReadbackModel(cancellationToken); fresh.LoadCheckpoint(stream); // Import into independent canonical loaded-root storage; no random baseline is trained.
                var optimizer = Require(RepositoryOwnedDpoOptimizerStateFile.OpenExisting(checkpointRoot, finalStep, cancellationToken: cancellationToken)); // Reopen actual optimizer, not just a manifest string.
                var randomPath = Path.Combine(checkpointRoot, RepositoryOwnedDpoRandomStateFile.BuildFileName(finalStep)); // Reopen the exact existing RNG codec.
                var random = Require(RepositoryOwnedDpoRandomStateFile.OpenAndValidate(randomPath, Hash(File.ReadAllBytes(randomPath)), new FileInfo(randomPath).Length, finalStep, topology, loraIdentity)); // Validate format, step and topology.
                fresh.RestoreRandomState(random, new(topology, loraIdentity)); // Real state import proves cursor readability; this performs no optimizer call.
                var afterPolicy = Snapshot(loaded.Policy); var afterReference = Snapshot(loaded.Reference); // Inspect actual in-memory policy and frozen reference after training.
                effect = new { parameters = Effects(beforePolicy, afterPolicy), referenceUnchanged = Same(beforeReference, afterReference), freshWeightsEqualLive = Same(afterPolicy, Snapshot(fresh)), optimizerStep = optimizer.State.Step, optimizerSlots = optimizer.State.Entries.Select(x => x.Key).ToArray(), optimizerEqualLive = optimizer.State.ToBytes().SequenceEqual(trainer.ExportOptimizerState().ToBytes()), randomStateReopened = true }; // Parameter effect, persistence and quality remain distinct claims.
                if (!Same(beforeReference, afterReference) || !Same(afterPolicy, Snapshot(fresh)) || optimizer.State.Step != finalStep || !optimizer.State.ToBytes().SequenceEqual(trainer.ExportOptimizerState().ToBytes()) || // Require actual saved/live equivalence.
                    beforePolicy.Where(x => x.Value.IsBase).Any(x => x.Value.Hash != afterPolicy[x.Key].Hash) || // Every base parameter stays bitwise frozen.
                    !beforePolicy.Where(x => !x.Value.IsBase).Any(x => x.Value.Hash != afterPolicy[x.Key].Hash)) throw new InvalidOperationException("SIMULATION_DPO_EFFECT_OR_READBACK_INVALID"); // A zero-LoRA-effect run cannot masquerade as training.
            } // A reopened candidate is held privately and is not adopted.
            if (Hash(File.ReadAllBytes(loaded.SourceWeightsPath)) != loaded.SourceWeightsSha256) throw new InvalidOperationException("SIMULATION_DPO_SOURCE_CHANGED"); // Immutable source bytes must still match B0.
            if (options.Domain == SimulationDpoDomain.EnRu) SimulationEnRuContinuation.VerifySource(root);
        } // Success means only finite training plus verified private saved effect.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) // Retain failures/cancellation without resetting attempt identity.
        {
            error = exception is DpoMutationAdmissionException admission ? admission.ErrorCode : exception.GetType().Name + ":" + exception.Message;
            if (stepTimings.Count > 0 && stepTimings[^1].Threw &&
                !journal.TryStepTiming($"step-{initialStep + issued:D6}", stepTimings[^1])) timingJournalWriteFailures++; // Preserve completed phase work from a thrown call without replacing its original exception.
            journal.Event("failure", options.RunId, new { error, issued, completed });
        } // Actual errors stay observable and no external raw content is dumped.
        var receipt = Path.Combine(runRoot, "receipt.json"); // One durable final observation accompanies successful and failed native executions.
        WriteNew(receipt, new { options.RunId, options.Mode, options.Domain, initialGlobalStep = initialStep, finalGlobalStep = initialStep + completed, completedNewUpdates = completed, provenance = "simulation", utcStart, utcEnd = DateTimeOffset.UtcNow, wholeRunSeconds = Elapsed(wholeStart), status = error is null ? "TRAINED_PRIVATE_CANDIDATE_NOT_ADOPTED" : "FAILED_RECONCILE_BEFORE_RETRY", error, options.ExpectedPairCount, optimizerSettings, pairPresentations = presentations, uniquePairIdsCompleted = consumedIds.Count, stepVectorCallsIssued = issued, stepVectorCallsCompleted = completed, optimizerCallsObservedByCompletedSteps = completed, optimizerEffectForIncompleteCall = issued == completed ? "NONE_MISSING" : "UNKNOWN", checkpoint, effect, sourceWeightsSha256After = models is null ? null : Hash(File.ReadAllBytes(models.SourceWeightsPath)), teacherCalls = 0, externalEvaluatorCalls = 0, studentGenerationCalls = 0, candidateScoringCalls = 0, stepVectorTiming = BuildStepTimingReport(stepTimings, timingJournalWriteFailures), uninstrumented = new { optimizerOnlyMilliseconds = (double?)null, reason = "ADAMW_KERNEL_ONLY_NOT_INSTRUMENTED" }, zeroCallStages = new { student_generation_total = "NOT_APPLICABLE", student_candidate_scoring_total = "NOT_APPLICABLE", external_evaluator_total = "NOT_APPLICABLE" }, pid = Environment.ProcessId, backend = "existing managed CPU transformer and AdamW", processCpuSeconds = Process.GetCurrentProcess().TotalProcessorTime.TotalSeconds, peakWorkingSetBytes = Process.GetCurrentProcess().PeakWorkingSet64 }); // Never label an outer step optimizer-only or claim an unmeasured speedup.
        return error is null ? OperationResult<string>.Ok(receipt) : OperationResult<string>.Fail("Simulation run failed; inspect its durable receipt before any retry.", error); // No main run or new-ID retry is automatic.
    } // Existing accepted model trees, historical experiments and closed evaluation inputs are untouched.

    private static bool ValidOptions(SimulationDpoRunOptions value) // Finite schedule admission is local to this explicitly selected development runner.
        => value is not null && Enum.IsDefined(value.Domain) && value.Mode is "calibration" or "main" &&
            (value.Domain != SimulationDpoDomain.EnRu || value.Mode == "main" && value.InputSha256 == SimulationEnRuContinuation.DatasetSha256 && value.BatchSize == 10 && value.MaximumUpdates == 50) && // The exact trusted C50→100 arm has no calibration or alternate schedule.
            value.RunId.Length is > 0 and <= 128 && value.RunId.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_') && IsHash(value.InputSha256) && // Run IDs cannot traverse output paths.
            value.ExpectedPairCount == (value.Mode == "calibration" ? 1 : 500) && value.BatchSize > 0 && value.BatchSize <= value.ExpectedPairCount && // Calibration cannot include main500.
            value.MaximumUpdates == (value.ExpectedPairCount + value.BatchSize - 1) / value.BatchSize && // One epoch has an exact predeclared optimizer count.
            (value.Mode == "calibration" ? value.BatchSize == 1 && value.MaximumUpdates == 1 && value.WallBudgetSeconds is null : value.WallBudgetSeconds is null || value.WallBudgetSeconds is > 0 && double.IsFinite(value.WallBudgetSeconds.Value)); // Main keeps the finite epoch/update schedule; a wall-clock budget is optional and positive finite when supplied.
    private static string Text(JsonElement row, string name) // Preserve exact Unicode while bounding malformed source fields.
    { var text = row.GetProperty(name).GetString(); if (string.IsNullOrEmpty(text) || Strict.GetByteCount(text) > 65536) throw new InvalidDataException("SIMULATION_DPO_TEXT_INVALID"); return text; } // No trim, normalization, concatenation or truncation occurs.
    private static OperationResult<IReadOnlyList<SimulationDpoInputRow>> InvalidInput() => OperationResult<IReadOnlyList<SimulationDpoInputRow>>.Fail("Pinned simulation input is invalid, duplicated or incomplete.", "SIMULATION_DPO_INPUT_INVALID"); // A partial import is always a refusal.
    private static OperationResult<string> RefuseReplay() => OperationResult<string>.Fail("This dataset/arm was issued before; reconcile the existing journal instead of replaying it.", ReplayError); // Changing run IDs cannot erase unknown effects.
    private static T Require<T>(OperationResult<T> result) => result.IsSuccess && result.Value is { } value ? value : throw new InvalidOperationException(result.ErrorCode + ":" + result.ErrorMessage); // Existing refusals stop the live path.
    private static bool IsHash(string value) => value.Length == 64 && value.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f'); // SHA pins use canonical lowercase hex.
    private static string Hash(ReadOnlySpan<byte> bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(); // Identity always comes from observed bytes.
    private static double Elapsed(long start) => Stopwatch.GetElapsedTime(start).TotalSeconds; // Monotonic elapsed time is independent of UTC clock changes.
    private static object BuildStepTimingReport(IReadOnlyList<DpoStepVectorTiming> observations, int journalWriteFailures)
    {
        var attempts = observations.Select(x => x.Snapshot()).ToArray();
        DpoPhaseTimingSnapshot Sum(Func<DpoStepVectorTimingSnapshot, DpoPhaseTimingSnapshot> select)
        {
            var phases = attempts.Select(select).ToArray();
            var completed = phases.Sum(x => x.CompletedCalls);
            return new(phases.Sum(x => x.StartedCalls), completed,
                completed == 0 ? null : phases.Sum(x => x.CompletedMilliseconds ?? 0));
        }
        return new
        {
            schema = "step-vector-phase-timing-v1", journalWriteFailures,
            accounting = "COMPLETED_HELPERS_IN_ALL_ATTEMPTS_INCLUDING_ROLLED_BACK_WORK",
            relationship = "CONTAINED_IN_STEP_VECTOR_AND_TRAINING_STEP_TOTAL_DO_NOT_ADD",
            forwardScope = "ONE_CHOSEN_OR_REJECTED_FORWARD_PLUS_LIKELIHOOD_REDUCTION_PER_HELPER_CALL",
            backwardScope = "ONE_CHOSEN_OR_REJECTED_LOGIT_GRADIENT_AND_BACKPROP_PER_HELPER_CALL",
            applyOptimizerScope = "RECIPIENT_VALIDATION_PARAMETER_SELECTION_AND_OPTIMIZER_EXCLUDES_FOLLOWING_ZERO_GRAD",
            referenceForward = Sum(x => x.ReferenceForward), policyForward = Sum(x => x.PolicyForward),
            backward = Sum(x => x.Backward), applyOptimizer = Sum(x => x.ApplyOptimizer), attempts
        };
    }
    private static void WriteNew(string path, object value) // Durable journals and receipts never replace existing records.
    { using var file = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read, FileOptions.WriteThrough); file.Write(JsonSerializer.SerializeToUtf8Bytes(value, Json)); file.Flush(true); } // Actual file ownership/custody is checked by the canonical boundary.
    private sealed record ParameterSnapshot(string Hash, bool IsBase, int Count, float[]? AdapterValues); // Keep small adapter values for numeric deltas; hash all base bits.
    private static Dictionary<string, ParameterSnapshot> Snapshot(LlmModel model) // Observe actual named state, not a checkpoint filename.
    { var bases = model.BaseParameters.ToHashSet(ReferenceEqualityComparer.Instance); return model.GetCheckpointParameterEntries().ToDictionary(x => x.Key, x => new ParameterSnapshot(Hash(MemoryMarshal.AsBytes(x.Parameter.Value.Data.AsSpan())), bases.Contains(x.Parameter), x.Parameter.Value.Data.Length, bases.Contains(x.Parameter) ? null : x.Parameter.Value.Data.ToArray()), StringComparer.Ordinal); } // Base snapshots are streamed through SHA256 without another full model copy.
    private static bool Same(Dictionary<string, ParameterSnapshot> first, Dictionary<string, ParameterSnapshot> second) // Require exact names/count and bit identity for every parameter.
        => first.Count == second.Count && first.All(x => second.TryGetValue(x.Key, out var other) && x.Value.Hash == other.Hash); // Equality here is persistence/freeze evidence, never quality.
    private static object[] Effects(Dictionary<string, ParameterSnapshot> before, Dictionary<string, ParameterSnapshot> after) // Report per-key state effects rather than calling a changed archive learning gain.
        => before.Select(x => { var current = after[x.Key]; var differences = x.Value.AdapterValues?.Zip(current.AdapterValues!, (a, b) => (double)b - a).ToArray(); if (differences?.Any(d => !double.IsFinite(d)) == true) throw new InvalidOperationException("SIMULATION_DPO_NONFINITE_DELTA"); return (object)new { key = x.Key, x.Value.IsBase, elements = x.Value.Count, beforeSha256 = x.Value.Hash, afterSha256 = current.Hash, changed = x.Value.Hash != current.Hash, adapterMaximumAbsoluteDelta = differences?.Max(d => Math.Abs(d)), adapterL2Delta = differences is null ? (double?)null : Math.Sqrt(differences.Sum(d => d * d)) }; }).ToArray(); // Actual finite numeric LoRA effect is separate from a heldout signal.

    private sealed class RunJournal : IDisposable // Stage observations remain inspectable even when a native process is interrupted.
    { // The file is owner-only and each line is flushed independently.
        private readonly FileStream _stream; private readonly string _runId; private readonly object _gate = new(); // Serialize actual journal writes inside this one native process.
        private readonly Dictionary<string, (long Start, DateTimeOffset Utc)> _open = new(); // Track only started stages; missing ends stay visible after interruption.
        internal RunJournal(string root, string runId) { _runId = runId; _stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(Path.Combine(root, "stages.jsonl"), FileMode.CreateNew, FileAccess.Write, FileShare.Read, FileOptions.WriteThrough); } // Create-new prevents replacing history.
        internal void Event(string stage, string id, object detail) { lock (_gate) { var bytes = JsonSerializer.SerializeToUtf8Bytes(new { runId = _runId, stage, id, utc = DateTimeOffset.UtcNow, monotonicTimestamp = Stopwatch.GetTimestamp(), detail }); _stream.Write(bytes); _stream.WriteByte((byte)'\n'); _stream.Flush(true); } } // Full stage cost is measured in the actual executed path.
        internal void Begin(string stage, string id) { lock (_gate) { _open.Add(stage + ":" + id, (Stopwatch.GetTimestamp(), DateTimeOffset.UtcNow)); Event(stage, id, new { status = "STARTED" }); } } // A started-only record is not a completed timing interval.
        internal void End(string stage, string id, string status) { lock (_gate) { var start = _open[stage + ":" + id]; _open.Remove(stage + ":" + id); Event(stage, id, new { status, utcStart = start.Utc, elapsedMilliseconds = Stopwatch.GetElapsedTime(start.Start).TotalMilliseconds }); } } // UTC boundaries and monotonic durations are both explicit.
        internal IDisposable Stage(string stage, string id) { Begin(stage, id); return new StageScope(() => End(stage, id, "ENDED_CHECK_RESULT")); } // Disposing a failed scope records an ended window, not a success claim.
        internal bool TryStepTiming(string id, DpoStepVectorTiming timing)
        {
            try { Event("training_step_phases", id, timing.Snapshot()); return true; }
            catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { return false; } // Optional diagnostics must preserve returned/failed trainer semantics.
        }
        public void Dispose() => _stream.Dispose(); // Do not delete any journal or partial candidate at shutdown.
        private sealed class StageScope(Action end) : IDisposable { public void Dispose() => end(); } // One lexical scope corresponds to one observed stage interval.
    } // Timing writes occur after each call, never per pair inside the trainer transaction.
} // This tool path grants neither production adoption nor broad C# competence.
