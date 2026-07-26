// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Diagnostics;
using AgentInfrastructure;
using LoRA;
using SemanticCompilerAdapter;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

// This trainer turns a visible knowledge edge into a probationary learned unit and a retryable flow.
public sealed class DefaultLogicalCapabilityTrainer : ILogicalCapabilityTrainer
{
    // Learned units registered by this trainer fill the generated-capability slot.
    public const string GeneratedCapabilityUnitRole = "generated-capability";

    // Generated flows are registered after training so the retry path can execute the new capability.
    public const string GeneratedCapabilityFlowIdPrefix = "generated-capability-flow";

    // Generated scenarios mirror the generated flow id so registry lookup remains traceable.
    public const string GeneratedCapabilityScenarioIdPrefix = "generated-capability";

    // Version applied to first-generation runtime capabilities.
    public const string GeneratedCapabilityVersion = "1.0.0";

    private const int DefaultDpoMaxStepCount = 5;
    private const int DefaultDpoConvergenceWindowSteps = 1;
    private readonly string _checkpointRoot;
    private readonly Dictionary<string, CachedTeachingStack> _teachingCache = new(StringComparer.Ordinal);
    private readonly Dictionary<string, GeneratedCapabilityCheckpointManifest> _manifestCacheByUnitId = new(StringComparer.Ordinal);
    private readonly LlmModel _referenceModel = new(BuildTinyConfig());
    private readonly ILogicalTeachingIdleGate _idleGate;
    private readonly ITrustScorer? _trustScorer;
    private bool _manifestCacheHydrated;

    // Create the trainer with a checkpoint root outside the repository by default.
    public DefaultLogicalCapabilityTrainer(string? checkpointRoot = null, ILogicalTeachingIdleGate? idleGate = null, ITrustScorer? trustScorer = null)
    {
        _checkpointRoot = string.IsNullOrWhiteSpace(checkpointRoot)
            ? Path.Combine(Path.GetTempPath(), "llm-mvp-capability-training")
            : checkpointRoot;
        _idleGate = idleGate ?? (ILogicalTeachingIdleGate?)AutonomousLearningIdleGate.TryCreate() ?? new AlwaysReadyLogicalTeachingIdleGate();
        _trustScorer = trustScorer;
    }

    // Release live teaching stacks and manifest indexes so long-running batch runners
    // can start the next physical batch from disk without retaining model objects.
    public void ClearCache()
    {
        _teachingCache.Clear();
        _manifestCacheByUnitId.Clear();
        _manifestCacheHydrated = false;
    }

    // Detects DPO convergence from window-level loss changes.
    public sealed class ConvergenceMonitor
    {
        private readonly float _threshold;
        private readonly int _windowSteps;
        private float _previousLoss = float.MaxValue;
        private int _stableWindowCount;

        // Create a monitor using the E11 loss-delta threshold and measurement window.
        public ConvergenceMonitor(float threshold = 0.002f, int windowSteps = 25)
        {
            _threshold = MathF.Max(0f, threshold);
            _windowSteps = Math.Max(1, windowSteps);
        }

        // Return true after two consecutive windows move less than the threshold.
        public bool ShouldStop(float currentLoss)
        {
            StepsTaken += _windowSteps;
            if (_previousLoss == float.MaxValue)
            {
                _previousLoss = currentLoss;
                return false;
            }

            if (MathF.Abs(_previousLoss - currentLoss) < _threshold)
                _stableWindowCount++;
            else
                _stableWindowCount = 0;

            _previousLoss = currentLoss;
            return _stableWindowCount >= 2;
        }

        // Steps represented by the measured windows.
        public int StepsTaken { get; private set; }
    }

    // Train from the observed knowledge-edge branches and register the generated capability.
    public async Task<OperationResult<LogicalCapabilityTrainingResult>> TrainAndRegisterAsync(
        LogicalCapabilityTrainingRequest request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared envelope.
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalCapabilityTrainingResult>.Fail("Cancelled.", "CANCELLED");

        // Validate the request shape.
        if (request is null)
            return OperationResult<LogicalCapabilityTrainingResult>.Fail("Capability training request is required.", "CAPABILITY_TRAINING_REQUEST_REQUIRED");
        if (request.KnowledgeEdgeBranches is null || request.KnowledgeEdgeBranches.Length == 0)
            return OperationResult<LogicalCapabilityTrainingResult>.Fail("At least one knowledge-edge branch is required.", "KNOWLEDGE_EDGE_BRANCH_REQUIRED");
        if (request.Topology is null || request.Flows is null || request.Units is null)
            return OperationResult<LogicalCapabilityTrainingResult>.Fail("Topology, flow, and unit registries are required.", "CAPABILITY_REGISTRIES_REQUIRED");
        if ((request.OriginalRequest.Flags ?? new LogicalExecutionFlags()).UseIdleLearningScheduling)
        {
            var idle = _idleGate.CanTrainNow(request);
            if (!idle.IsSuccess)
                return OperationResult<LogicalCapabilityTrainingResult>.Fail(idle.ErrorMessage, idle.ErrorCode);
            if (!idle.Value)
            {
                var deferred = new LogicalCapabilityTrainingResult(
                    false,
                    [],
                    [],
                    [],
                    0,
                    [],
                    "Teaching deferred by D-14 idle-learning gate.");
                return OperationResult<LogicalCapabilityTrainingResult>.Ok(deferred);
            }
        }

        // Track runtime and registered artifacts for the returned teaching evidence.
        var stopwatch = Stopwatch.StartNew();
        var registeredFlowIds = new List<string>();
        var registeredSubPhaseIds = new List<string>();
        var registeredUnitIds = new List<string>();
        var evidenceIds = new List<string>();
        var notes = new List<string>();
        var branchTimings = new List<LogicalCapabilityTrainingBranchTiming>();
        var domainAdapterTag = request.Domain.ToString();
        var compiledPrompt = request.CompiledPrompt ?? CompileForTeaching(request.OriginalRequest.UserRequest);
        var compiledText = !string.IsNullOrWhiteSpace(request.CompiledText)
            ? request.CompiledText!.Trim()
            : compiledPrompt?.CanonicalText;
        var batchedDpoStepCount = 0;

        // Validate and collect every branch first so one same-domain row can teach in one batched DPO step.
        var branchPlans = new List<TrainingBranchPlan>();
        foreach (var branch in request.KnowledgeEdgeBranches)
        {
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<LogicalCapabilityTrainingResult>.Fail("Cancelled.", "CANCELLED");

            // Extract the routed sub-phase from the branch metrics.
            var subPhaseId = ExtractSubPhaseId(branch);
            if (string.IsNullOrWhiteSpace(subPhaseId))
            {
                notes.Add($"Skipped branch '{branch.FlowId}' because it did not name a sub-phase.");
                continue;
            }

            // The trainer can only install a flow when the topology contains the target sub-phase.
            var subPhase = request.Topology.ResolveSubPhase(subPhaseId);
            if (!subPhase.IsSuccess)
            {
                notes.Add($"Skipped sub-phase '{subPhaseId}' because it is not registered in topology.");
                continue;
            }

            // Build or continue the local teaching stack for this sub-phase.
            var trainingText = BuildTrainingTextCore(compiledPrompt, compiledText, request.OriginalRequest.UserRequest, branch, subPhase.Value!.SubPhase);
            branchPlans.Add(new TrainingBranchPlan(
                branch,
                subPhaseId,
                subPhase.Value.SubPhase,
                trainingText.Text,
                BuildChosenCapabilityText(subPhase.Value.SubPhase),
                $"Do not generate a capability for sub-phase {subPhase.Value.SubPhase.SubPhaseId}.",
                trainingText.TeachingVector));
        }

        if (branchPlans.Count > 0)
        {
            var requestFlags = request.OriginalRequest.Flags ?? new LogicalExecutionFlags();
            var replayManifests = FindReplayManifests(domainAdapterTag, compiledText, requestFlags.UseReplayFusionRanking);
            var totalKnownRows = Math.Max(1, CountGeneratedCapabilityManifests() + branchPlans.Count);
            var distinctClasses = Math.Max(1, branchPlans.Select(plan => plan.SubPhaseId).Distinct(StringComparer.Ordinal).Count());
            var rareRows = Math.Max(1, replayManifests.Length + 1);
            var currentRepeatCount = LogicalTeachingDatasetWeighter.ComputeCurrentRepeatCount(totalKnownRows, distinctClasses, rareRows);
            if (replayManifests.Length > 0)
                currentRepeatCount = Math.Max(currentRepeatCount, 2);

            var combinedTrainingText = string.Join(" ", branchPlans.Select(plan => plan.TrainingText));
            var buildStackStopwatch = Stopwatch.StartNew();
            var teachingStack = BuildTeachingStack(request.Units, domainAdapterTag, combinedTrainingText, compiledPrompt);
            buildStackStopwatch.Stop();
            if (!teachingStack.IsSuccess)
            {
                notes.Add($"Teaching stack creation failed for domain '{domainAdapterTag}': {teachingStack.ErrorMessage}");
            }
            else
            {
                var tokenizer = teachingStack.Value!.Tokenizer;
                var policyModel = teachingStack.Value.PolicyModel;
                var referenceModelStopwatch = Stopwatch.StartNew();
                var referenceModel = _referenceModel;
                referenceModelStopwatch.Stop();
                var batchCheckpointDirectory = Path.Combine(_checkpointRoot, Sanitize(request.RunId), Sanitize(domainAdapterTag), "domain-batch");

                // Execute one DPO teaching unit for every current branch in this domain row.
                var dpoStopwatch = Stopwatch.StartNew();
                var dpoResult = await ExecuteDpoTeachingAsync(
                    request.RunId,
                    policyModel,
                    referenceModel,
                    tokenizer,
                    batchCheckpointDirectory,
                    branchPlans,
                    teachingStack.Value.ParentPolicyCheckpointPath,
                    replayManifests,
                    currentRepeatCount,
                    ReadDpoMaxStepCount(request.OriginalRequest.Metadata),
                    ReadDpoConvergenceWindowSteps(request.OriginalRequest.Metadata),
                    cancellationToken);
                dpoStopwatch.Stop();
                if (!dpoResult.IsSuccess)
                {
                    notes.Add($"Batched DPO teaching failed for domain '{domainAdapterTag}': {dpoResult.ErrorMessage}");
                }
                else
                {
                    evidenceIds.AddRange(dpoResult.Value!.Output.MetricFilePaths);
                    batchedDpoStepCount = dpoResult.Value.Output.DpoStepCount;
                    var domainClassifierRegistration = RegisterTrainedDomainClassifier(request, compiledPrompt, compiledText);
                    if (domainClassifierRegistration.IsSuccess)
                    {
                        registeredUnitIds.Add(domainClassifierRegistration.Value!.UnitId);
                        evidenceIds.Add($"domain_classifier_training:{domainClassifierRegistration.Value.UnitId}");
                        notes.Add($"domain_classifier_training=registered; unit_id={domainClassifierRegistration.Value.UnitId}; domain={domainAdapterTag}");
                    }
                    else
                    {
                        notes.Add($"domain_classifier_training=skipped; reason={domainClassifierRegistration.ErrorMessage}");
                    }

                    var trustState = AutonomousLearningTrustPromotionScorer.ScoreCapabilityTrust(
                        request,
                        branchPlans.Select(plan => new TrainingBranchPlanView(plan.SubPhaseId, plan.SubPhase.PredicateIds, plan.SubPhase.DomainIds)).ToArray(),
                        replayManifests,
                        _trustScorer);
                    var registrationPlans = BuildRegistrationPlans(branchPlans);
                    notes.Add($"domain={domainAdapterTag}; batch_branches={branchPlans.Count}; registered_sub_phases={registrationPlans.Count}; semantic_compression=enabled; current_repeat={currentRepeatCount}; replay_count={Math.Max(0, dpoResult.Value.Output.PreferenceCount - (branchPlans.Count * currentRepeatCount))}; batched_dpo_steps={dpoResult.Value.Output.DpoStepCount}; d09_max_weight={LogicalTeachingDatasetWeighter.ComputeMaxWeight(totalKnownRows, distinctClasses, rareRows):0.###}");
                    notes.Add($"trust_scorer=project16; trust_state={trustState}");
                    var sharedBuildStackMs = buildStackStopwatch.Elapsed.TotalMilliseconds / registrationPlans.Count;
                    var sharedReferenceModelMs = referenceModelStopwatch.Elapsed.TotalMilliseconds / registrationPlans.Count;
                    var sharedDpoMs = dpoStopwatch.Elapsed.TotalMilliseconds / registrationPlans.Count;

                    foreach (var plan in registrationPlans)
                    {
                        var branchStopwatch = Stopwatch.StartNew();
                        var saveArtifactsMs = 0.0;
                        var wrapUnitMs = 0.0;
                        var saveManifestMs = 0.0;
                        var registerUnitMs = 0.0;
                        var registerFlowMs = 0.0;
                        var subPhaseId = plan.SubPhaseId;
                        var subPhase = plan.SubPhase;
                        var checkpointDirectory = Path.Combine(_checkpointRoot, Sanitize(request.RunId), Sanitize(domainAdapterTag), Sanitize(subPhaseId));
                        var unitId = $"{GeneratedCapabilityUnitRole}-{Sanitize(domainAdapterTag)}-{Sanitize(subPhaseId)}-{Sanitize(request.RunId)}";
                        var flowId = $"{GeneratedCapabilityFlowIdPrefix}-{Sanitize(subPhaseId)}";
                        var scenarioId = $"{GeneratedCapabilityScenarioIdPrefix}-{Sanitize(subPhaseId)}";
                        var tokenizerPath = Path.Combine(checkpointDirectory, "tokenizer.json");
                        var policyCheckpointPath = Path.Combine(checkpointDirectory, "policy.bin");
                        var manifestPath = Path.Combine(checkpointDirectory, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName);

                        // Persist the taught model and tokenizer so later teaching rows can load the capability directly.
                        var saveArtifactsStopwatch = Stopwatch.StartNew();
                        Directory.CreateDirectory(checkpointDirectory);
                        tokenizer.Save(tokenizerPath);
                        policyModel.SaveCheckpoint(policyCheckpointPath);
                        saveArtifactsStopwatch.Stop();
                        saveArtifactsMs = saveArtifactsStopwatch.Elapsed.TotalMilliseconds;
                        evidenceIds.Add(tokenizerPath);
                        evidenceIds.Add(policyCheckpointPath);

                        // Wrap the taught model as an executable learned KNN unit.
                        var wrapUnitStopwatch = Stopwatch.StartNew();
                        var learnedUnit = TransformerEmbeddingKnnLogicalUnit<string>.Train(
                            unitId,
                            GeneratedCapabilityUnitRole,
                            domainAdapterTag,
                            policyModel,
                            tokenizer,
                            BuildKnnTrainingExamples(replayManifests, branchPlans));
                        wrapUnitStopwatch.Stop();
                        wrapUnitMs = wrapUnitStopwatch.Elapsed.TotalMilliseconds;
                        if (!learnedUnit.IsSuccess)
                        {
                            notes.Add($"Learned unit wrapping failed for '{subPhaseId}': {learnedUnit.ErrorMessage}");
                            continue;
                        }

                        var manifest = BuildGeneratedCapabilityManifest(
                            unitId,
                            flowId,
                            scenarioId,
                            tokenizerPath,
                            policyCheckpointPath,
                            plan.TrainingText,
                            domainAdapterTag,
                            subPhase,
                            teachingStack.Value.ParentManifest,
                            teachingStack.Value.TrainingStepCount,
                            teachingStack.Value.Version,
                            trustState,
                            request.OriginalRequest.UserRequest);
                        var saveManifestStopwatch = Stopwatch.StartNew();
                        var manifestSave = TeachingRegistryPersistence.SaveGeneratedCapabilityManifest(manifest, manifestPath);
                        saveManifestStopwatch.Stop();
                        saveManifestMs = saveManifestStopwatch.Elapsed.TotalMilliseconds;
                        if (!manifestSave.IsSuccess)
                        {
                            notes.Add($"Capability manifest save failed for '{subPhaseId}': {manifestSave.ErrorMessage}");
                            continue;
                        }

                        evidenceIds.Add(manifestSave.Value!);
                        _manifestCacheByUnitId[manifest.UnitId] = manifest;
                        _teachingCache[manifest.UnitFamilyId] = new CachedTeachingStack(policyModel, tokenizer, manifest);

                        // Register the learned unit at probationary trust so retry execution can evaluate it immediately.
                        var registerUnitStopwatch = Stopwatch.StartNew();
                        var unitRegistration = request.Units.Register<string, string>(
                            unitId,
                            GeneratedCapabilityUnitRole,
                            domainAdapterTag,
                            teachingStack.Value.Version,
                            trustState,
                            isLearned: true,
                            learnedUnit.Value!);
                        registerUnitStopwatch.Stop();
                        registerUnitMs = registerUnitStopwatch.Elapsed.TotalMilliseconds;
                        if (!unitRegistration.IsSuccess)
                        {
                            notes.Add($"Unit registration failed for '{subPhaseId}': {unitRegistration.ErrorMessage}");
                            continue;
                        }

                        registeredUnitIds.Add(unitRegistration.Value!.UnitId);
                        registeredSubPhaseIds.Add(subPhaseId);
                        evidenceIds.Add(unitRegistration.Value.UnitId);

                        // Register a generated flow factory after the learned unit so the orchestrator retry selects it.
                        var registerFlowStopwatch = Stopwatch.StartNew();
                        var flowRegistration = request.Flows.RegisterFactory(
                            flowId,
                            scenarioId,
                            teachingStack.Value.Version,
                            subPhase.PhaseId,
                            subPhaseId,
                            BuildTaskType(subPhase),
                            subPhase.DomainIds,
                            new GeneratedCapabilityFlowFactory(
                                subPhase.PhaseId,
                                subPhaseId,
                                BuildTaskType(subPhase),
                                subPhase.DomainIds,
                                subPhase.PredicateIds,
                                domainAdapterTag,
                                request.OriginalRequest.UserRequest));
                        registerFlowStopwatch.Stop();
                        registerFlowMs = registerFlowStopwatch.Elapsed.TotalMilliseconds;
                        if (!flowRegistration.IsSuccess)
                        {
                            notes.Add($"Flow registration failed for '{subPhaseId}': {flowRegistration.ErrorMessage}");
                            continue;
                        }

                        registeredFlowIds.Add(flowRegistration.Value!.FlowId);
                        evidenceIds.Add(flowRegistration.Value.FlowId);
                        branchStopwatch.Stop();
                        branchTimings.Add(new LogicalCapabilityTrainingBranchTiming(
                            subPhaseId,
                            unitId,
                            teachingStack.Value.ParentManifest?.UnitId,
                            teachingStack.Value.TrainingStepCount,
                            teachingStack.Value.UsedWarmCache,
                            sharedBuildStackMs,
                            sharedReferenceModelMs,
                            sharedDpoMs,
                            saveArtifactsMs,
                            wrapUnitMs,
                            saveManifestMs,
                            registerUnitMs,
                            registerFlowMs,
                            branchStopwatch.Elapsed.TotalMilliseconds + sharedBuildStackMs + sharedReferenceModelMs + sharedDpoMs));
                    }
                }
            }
        }

        stopwatch.Stop();

        // Report whether anything executable was actually registered.
        var capabilityRegistered = registeredUnitIds.Count > 0 || registeredFlowIds.Count > 0;
        var result = new LogicalCapabilityTrainingResult(
            capabilityRegistered,
            registeredFlowIds.Distinct(StringComparer.Ordinal).ToArray(),
            registeredSubPhaseIds.Distinct(StringComparer.Ordinal).ToArray(),
            registeredUnitIds.Distinct(StringComparer.Ordinal).ToArray(),
            stopwatch.Elapsed.TotalMilliseconds,
            evidenceIds.Distinct(StringComparer.Ordinal).ToArray(),
            notes.Count == 0
                ? "Generated probationary learned capability and retryable flow."
                : string.Join(" | ", notes))
        {
            Domain = domainAdapterTag,
            BatchedBranchCount = branchPlans.Count,
            BatchedDpoStepCount = batchedDpoStepCount,
            BranchTimings = branchTimings.ToArray()
        };

        return OperationResult<LogicalCapabilityTrainingResult>.Ok(result);
    }

    // Execute one DPO preference row through the logical teaching unit.
    private static async Task<OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>> ExecuteDpoTeachingAsync(
        string runId,
        LlmModel policyModel,
        LlmModel referenceModel,
        BpeTokenizer tokenizer,
        string checkpointDirectory,
        IReadOnlyList<TrainingBranchPlan> branchPlans,
        string? parentPolicyCheckpointPath,
        IReadOnlyList<GeneratedCapabilityCheckpointManifest> replayManifests,
        int currentRepeatCount,
        int maxDpoStepCount,
        int convergenceWindowSteps,
        CancellationToken cancellationToken)
    {
        var preferences = AutonomousLearningReplayMixer.BuildPreferences(
            branchPlans.Select(plan => new LogicalDpoPreferenceExample(plan.TrainingText, plan.ChosenText, plan.RejectedText, plan.TeachingVector)).ToArray(),
            replayManifests,
            currentRepeatCount);
        var request = new LogicalDpoTrainingRequest(
            policyModel,
            referenceModel,
            tokenizer,
            checkpointDirectory,
            preferences,
            BuildCapabilityLoraConfig(),
            parentPolicyCheckpointPath,
            MaxStepCount: maxDpoStepCount,
            ConvergenceWindowSteps: convergenceWindowSteps);

        return await new LogicalDpoTrainingUnit().ExecuteAsync(new LogicalUnitRequest<LogicalDpoTrainingRequest>(
            runId,
            LogicalDpoTrainingUnit.Id,
            LogicalDpoTrainingUnit.Role,
            LogicalDpoTrainingUnit.DomainTag,
            request,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));
    }

    // Build the model/tokenizer stack, continuing from the active domain-adapter checkpoint when one exists.
    private OperationResult<TeachingStack> BuildTeachingStack(
        InMemoryLogicalUnitRegistry units,
        string domainAdapterTag,
        string trainingText,
        SemanticTeachingPromptRepresentation? compiledPrompt)
    {
        try
        {
            var config = BuildTinyConfig();
            var parent = FindActiveParentManifest(units, domainAdapterTag);
            if (parent is null)
            {
                return OperationResult<TeachingStack>.Ok(new TeachingStack(
                    new LlmModel(config),
                    BuildTokenizer(compiledPrompt, trainingText),
                    null,
                    GeneratedCapabilityVersion,
                    1,
                    null,
                    false));
            }

            var unitFamilyId = BuildUnitFamilyId(domainAdapterTag);
            if (_teachingCache.TryGetValue(unitFamilyId, out var cached) &&
                string.Equals(cached.Manifest.UnitId, parent.UnitId, StringComparison.Ordinal))
            {
                return OperationResult<TeachingStack>.Ok(new TeachingStack(
                    cached.PolicyModel,
                    cached.Tokenizer,
                    parent,
                    IncrementPatchVersion(parent.Version),
                    Math.Max(1, parent.TrainingStepCount) + 1,
                    null,
                    true));
            }

            if (!File.Exists(parent.PolicyCheckpointPath))
                return OperationResult<TeachingStack>.Fail($"Parent policy checkpoint not found: {parent.PolicyCheckpointPath}", "PARENT_POLICY_CHECKPOINT_NOT_FOUND");
            if (!File.Exists(parent.TokenizerPath))
                return OperationResult<TeachingStack>.Fail($"Parent tokenizer checkpoint not found: {parent.TokenizerPath}", "PARENT_TOKENIZER_NOT_FOUND");

            var tokenizer = BpeTokenizer.Load(parent.TokenizerPath);
            var policyModel = new LlmModel(config);
            var trainingStepCount = Math.Max(1, parent.TrainingStepCount) + 1;
            return OperationResult<TeachingStack>.Ok(new TeachingStack(
                policyModel,
                tokenizer,
                parent,
                IncrementPatchVersion(parent.Version),
                trainingStepCount,
                parent.PolicyCheckpointPath,
                false));
        }
        catch (Exception ex)
        {
            return OperationResult<TeachingStack>.Fail(ex.Message, "TEACHING_STACK_CREATION_FAILED");
        }
    }

    // Find the manifest for the newest usable generated capability in this domain adapter.
    private GeneratedCapabilityCheckpointManifest? FindActiveParentManifest(
        InMemoryLogicalUnitRegistry units,
        string domainAdapterTag)
    {
        var manifests = LoadGeneratedCapabilityManifests();
        var activeDescriptor = units.ListDescriptors()
            .Where(descriptor =>
                descriptor.UnitRole == GeneratedCapabilityUnitRole &&
                descriptor.DomainTag == domainAdapterTag &&
                descriptor.InputType == typeof(string) &&
                descriptor.OutputType == typeof(string) &&
                descriptor.TrustState >= LogicalUnitTrustState.Probationary &&
                descriptor.TrustState != LogicalUnitTrustState.Rejected)
            .OrderByDescending(descriptor => descriptor.RegistrationOrder)
            .FirstOrDefault();
        if (activeDescriptor is not null)
        {
            var activeManifest = manifests
                .LastOrDefault(manifest => string.Equals(manifest.UnitId, activeDescriptor.UnitId, StringComparison.Ordinal));
            if (activeManifest is not null)
                return activeManifest;
        }

        return manifests
            .Where(manifest =>
                string.Equals(manifest.UnitRole, GeneratedCapabilityUnitRole, StringComparison.Ordinal) &&
                string.Equals(manifest.DomainTag, domainAdapterTag, StringComparison.Ordinal) &&
                !string.Equals(manifest.TrustState, LogicalUnitTrustState.Rejected.ToString(), StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(manifest => manifest.CreatedAtUtc)
            .ThenByDescending(manifest => manifest.UnitId, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    // Load generated capability manifests from the configured checkpoint root.
    private GeneratedCapabilityCheckpointManifest[] LoadGeneratedCapabilityManifests()
    {
        EnsureManifestCacheHydrated();
        return _manifestCacheByUnitId.Values
            .OrderBy(manifest => manifest.CreatedAtUtc)
            .ThenBy(manifest => manifest.UnitId, StringComparer.Ordinal)
            .ToArray();
    }

    private int CountGeneratedCapabilityManifests()
    {
        EnsureManifestCacheHydrated();
        return _manifestCacheByUnitId.Count;
    }

    private void EnsureManifestCacheHydrated()
    {
        if (_manifestCacheHydrated)
            return;

        _manifestCacheHydrated = true;
        if (string.IsNullOrWhiteSpace(_checkpointRoot) || !Directory.Exists(_checkpointRoot))
            return;

        foreach (var manifestPath in Directory.GetFiles(_checkpointRoot, TeachingRegistryPersistence.GeneratedCapabilityManifestFileName, SearchOption.AllDirectories))
        {
            var json = File.ReadAllText(manifestPath);
            var manifest = System.Text.Json.JsonSerializer.Deserialize<GeneratedCapabilityCheckpointManifest>(json);
            if (manifest is not null)
                _manifestCacheByUnitId[manifest.UnitId] = manifest;
        }
    }

    // Return previous manifests for the same domain adapter, optionally ranked by E34V2 replay fusion.
    private GeneratedCapabilityCheckpointManifest[] FindReplayManifests(
        string domainAdapterTag,
        string? compiledText,
        bool useReplayFusionRanking)
    {
        var candidates = LoadGeneratedCapabilityManifests()
            .Where(manifest => string.Equals(manifest.DomainTag, domainAdapterTag, StringComparison.Ordinal))
            .OrderByDescending(manifest => manifest.CreatedAtUtc)
            .Take(10)
            .ToArray();

        if (!useReplayFusionRanking || string.IsNullOrWhiteSpace(compiledText))
            return candidates.Take(3).ToArray();

        return ReplayFusionRanker.Rank(candidates, compiledText, topK: 3).ToArray();
    }

    private static int ReadDpoMaxStepCount(IReadOnlyDictionary<string, object> metadata)
    {
        if (metadata.TryGetValue("dpo.max_steps", out var value) &&
            int.TryParse(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture), out var parsed))
            return Math.Max(1, parsed);

        return DefaultDpoMaxStepCount;
    }

    private static int ReadDpoConvergenceWindowSteps(IReadOnlyDictionary<string, object> metadata)
    {
        if (metadata.TryGetValue("dpo.convergence_window_steps", out var value) &&
            int.TryParse(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture), out var parsed))
            return Math.Max(1, parsed);

        return DefaultDpoConvergenceWindowSteps;
    }

    // Extract the routed or missing sub-phase from one branch.
    private static string ExtractSubPhaseId(LogicalFlowResult branch)
    {
        if (branch.Metrics.TryGetValue("missing_sub_phase", out var missingSubPhase) && !string.IsNullOrWhiteSpace(missingSubPhase))
            return missingSubPhase.Trim();
        if (branch.Metrics.TryGetValue("routed_sub_phase_id", out var routedSubPhase) && !string.IsNullOrWhiteSpace(routedSubPhase))
            return routedSubPhase.Trim();
        if (branch.Metrics.TryGetValue("sub_phase_id", out var subPhase) && !string.IsNullOrWhiteSpace(subPhase))
            return subPhase.Trim();

        return string.Empty;
    }

    // Build one compact row of teaching text from the compiled prompt and branch evidence.
    private static SemanticTrainingText BuildTrainingText(
        SemanticTeachingPromptRepresentation? compiledPrompt,
        LogicalFlowResult branch,
        LogicalSubPhase subPhase)
    {
        return BuildTrainingTextCore(compiledPrompt, compiledPrompt?.CanonicalText, compiledPrompt?.RawText ?? string.Empty, branch, subPhase);
    }

    private static SemanticTrainingText BuildTrainingTextCore(
        SemanticTeachingPromptRepresentation? compiledPrompt,
        string? compiledText,
        string userRequest,
        LogicalFlowResult branch,
        LogicalSubPhase subPhase)
    {
        var promptSide = !string.IsNullOrWhiteSpace(compiledText)
            ? compiledText.Trim()
            : CompileForTrainingText(userRequest);
        var raw = string.Join(" ",
            "semantic_prompt:",
            promptSide,
            "branch_result:",
            branch.FinalHumanResult,
            "sub_phase:",
            subPhase.SubPhaseId,
            subPhase.Name,
            subPhase.Purpose,
            string.Join(" ", subPhase.PredicateIds));
        var vector = compiledPrompt?.TeachingVector ?? TrainingVectorBuilder.Build(promptSide);
        var vectorText = string.Join(",", vector.Select(value => value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)));
        return new SemanticTrainingText($"{raw} semantic_compressed: {promptSide} teaching_vector_60: {vectorText}".Trim(), vector);
    }

    private static string CompileForTrainingText(string userRequest)
    {
        var compiled = CompileForTeaching(userRequest ?? string.Empty);
        return compiled?.CanonicalText ?? userRequest?.Trim() ?? string.Empty;
    }

    // Build the positive B side for the one-row generated capability.
    private static string BuildChosenCapabilityText(LogicalSubPhase subPhase)
    {
        return $"Generated capability for sub-phase {subPhase.SubPhaseId}: {subPhase.Name}. Execute the requested work through task type {BuildTaskType(subPhase)}.";
    }

    // Convert a sub-phase name into a stable task type when no explicit flow metadata exists.
    private static string BuildTaskType(LogicalSubPhase subPhase)
    {
        return Sanitize(subPhase.Name);
    }

    // Build the durable manifest used by cumulative teaching sessions to load prior row capability.
    private static GeneratedCapabilityCheckpointManifest BuildGeneratedCapabilityManifest(
        string unitId,
        string flowId,
        string scenarioId,
        string tokenizerPath,
        string policyCheckpointPath,
        string trainingText,
        string domainAdapterTag,
        LogicalSubPhase subPhase,
        GeneratedCapabilityCheckpointManifest? parentManifest,
        int trainingStepCount,
        string version,
        LogicalUnitTrustState trustState,
        string originalPrompt)
    {
        var config = BuildTinyConfig();
        var loraConfig = BuildCapabilityLoraConfig();
        return new GeneratedCapabilityCheckpointManifest(
            unitId,
            GeneratedCapabilityUnitRole,
            domainAdapterTag,
            version,
            trustState.ToString(),
            flowId,
            scenarioId,
            subPhase.PhaseId,
            subPhase.SubPhaseId,
            BuildTaskType(subPhase),
            trainingText,
            BuildChosenCapabilityText(subPhase),
            $"capability-row-{Sanitize(subPhase.SubPhaseId)}",
            subPhase.PredicateIds,
            subPhase.DomainIds,
            Path.GetFullPath(policyCheckpointPath),
            Path.GetFullPath(tokenizerPath),
            config.VocabSize,
            config.ContextLen,
            config.HiddenDim,
            config.NumLayers,
            config.NumHeads,
            config.NumKvHeads,
            config.FfnHiddenDim,
            loraConfig.Rank,
            loraConfig.Alpha,
            loraConfig.Dropout,
            loraConfig.TargetModules,
            DateTimeOffset.UtcNow,
            UnitFamilyId: BuildUnitFamilyId(domainAdapterTag),
            ParentUnitId: parentManifest?.UnitId,
            ParentPolicyCheckpointPath: parentManifest?.PolicyCheckpointPath,
            ParentTokenizerPath: parentManifest?.TokenizerPath,
            TrainingStepCount: trainingStepCount,
            OriginalPrompt: originalPrompt?.Trim() ?? string.Empty);
    }

    // Build the domain adapter retrieval memory from replay rows plus the current teaching row.
    private static TransformerKnnTrainingExample<string>[] BuildKnnTrainingExamples(
        IReadOnlyList<GeneratedCapabilityCheckpointManifest> replayManifests,
        IReadOnlyList<TrainingBranchPlan> branchPlans)
    {
        var examples = replayManifests
            .Reverse()
            .Select(manifest => new TransformerKnnTrainingExample<string>(
                manifest.ExampleId,
                manifest.TrainingText,
                manifest.OutputText,
                manifest.PredicateIds,
                manifest.DomainTags))
            .ToList();

        examples.AddRange(branchPlans.Select(plan => new TransformerKnnTrainingExample<string>(
            $"capability-row-{Sanitize(plan.SubPhaseId)}",
            plan.TrainingText,
            plan.ChosenText,
            plan.SubPhase.PredicateIds,
            plan.SubPhase.DomainIds)));

        return examples.ToArray();
    }

    private OperationResult<LogicalUnitDescriptor> RegisterTrainedDomainClassifier(
        LogicalCapabilityTrainingRequest request,
        SemanticTeachingPromptRepresentation? compiledPrompt,
        string? compiledText)
    {
        compiledPrompt ??= !string.IsNullOrWhiteSpace(compiledText)
            ? CompileForTeaching(compiledText!)
            : null;

        if (compiledPrompt is null)
            return OperationResult<LogicalUnitDescriptor>.Fail("No compiled prompt was available for domain classifier training.", "COMPILED_PROMPT_REQUIRED");

        var predicateProfile = DomainClassifierUnit.EncodePredicateProfile(
                compiledPrompt.Compilation.DevelopmentPredicateIds.Concat(compiledPrompt.Compilation.PipelinePredicates))
            .ToArray();
        if (predicateProfile.Length == 0)
            return OperationResult<LogicalUnitDescriptor>.Fail("Compiled prompt did not contain trainable predicate ids.", "DOMAIN_CLASSIFIER_PREDICATES_REQUIRED");

        var examples = LoadDomainClassifierTrainingExamples()
            .Append(new DomainClassifierTrainingExample(request.RunId, request.Domain, predicateProfile, DateTimeOffset.UtcNow))
            .GroupBy(example => $"{example.Domain}:{string.Join(",", example.Predicates)}", StringComparer.Ordinal)
            .Select(group => group.Last())
            .ToArray();
        var saveExamples = SaveDomainClassifierTrainingExamples(examples);
        if (!saveExamples.IsSuccess)
            return saveExamples.MapError<LogicalUnitDescriptor>();

        var classifier = new DomainClassifierUnit();
        classifier.Train(examples.Select(example => (example.Predicates, example.Domain)).ToArray());
        var wrapper = new CompiledDomainClassifierUnit(classifier);
        var unitId = $"domain-classifier-{Sanitize(request.Domain.ToString())}-{Sanitize(request.RunId)}";
        return request.Units.Register<DomainClassifierRequest, DomainClassification>(
            unitId,
            CompiledDomainClassifierUnit.Role,
            CompiledDomainClassifierUnit.DomainTag,
            GeneratedCapabilityVersion,
            LogicalUnitTrustState.Probationary,
            isLearned: true,
            wrapper);
    }

    private DomainClassifierTrainingExample[] LoadDomainClassifierTrainingExamples()
    {
        var path = DomainClassifierTrainingPath();
        if (!File.Exists(path))
            return [];

        var json = File.ReadAllText(path);
        return System.Text.Json.JsonSerializer.Deserialize<DomainClassifierTrainingExample[]>(json) ?? [];
    }

    private OperationResult<string> SaveDomainClassifierTrainingExamples(IReadOnlyList<DomainClassifierTrainingExample> examples)
    {
        var path = DomainClassifierTrainingPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(examples));
        return OperationResult<string>.Ok(path);
    }

    private string DomainClassifierTrainingPath()
    {
        return Path.Combine(_checkpointRoot, "domain-classifier-training.json");
    }

    private static IReadOnlyList<TrainingBranchPlan> BuildRegistrationPlans(IReadOnlyList<TrainingBranchPlan> branchPlans)
    {
        return branchPlans
            .GroupBy(plan => plan.SubPhaseId, StringComparer.Ordinal)
            .Select(group =>
            {
                var first = group.First();
                var trainingText = string.Join(" ", group
                    .Select(plan => plan.TrainingText)
                    .Where(text => !string.IsNullOrWhiteSpace(text))
                    .Distinct(StringComparer.Ordinal));
                return group.Count() == 1 || string.IsNullOrWhiteSpace(trainingText)
                    ? first
                    : first with { TrainingText = trainingText };
            })
            .OrderBy(plan => plan.SubPhaseId, StringComparer.Ordinal)
            .ToArray();
    }

    // Keep capability adapters tiny enough for cumulative teaching on ordinary hardware.
    private static LoraConfig BuildCapabilityLoraConfig()
    {
        return new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] };
    }

    // Train a tiny tokenizer with enough vocabulary headroom for merged prompt tokens.
    private static BpeTokenizer BuildTokenizer(SemanticTeachingPromptRepresentation? compiledPrompt, string text)
    {
        var tokenizer = new BpeTokenizer();
        var seedText = compiledPrompt?.TokenizerSeedText ??
            $"{SemanticTeachingPromptCompiler.CanonicalPredicateSeed} capability generated learned flow selected rejected";
        var stableMergeSeed = string.Join(" ", Enumerable.Range(0, 512).Select(index => $"semantic_seed_{index:000}"));
        tokenizer.Train($"{seedText} {text} {stableMergeSeed}", 512);
        return tokenizer;
    }

    private static SemanticTeachingPromptRepresentation? CompileForTeaching(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return null;

        var compiled = new SemanticTeachingPromptCompiler().Compile(prompt);
        return compiled.IsSuccess ? compiled.Value : null;
    }

    // Keep capability training small enough to run on ordinary developer machines.
    private static ModelConfig BuildTinyConfig()
    {
        return new ModelConfig
        {
            VocabSize = 512,
            ContextLen = 64,
            HiddenDim = 32,
            NumLayers = 4,
            NumHeads = 4,
            NumKvHeads = 2,
            FfnHiddenDim = 64
        };
    }

    // Build a stable logical unit family id for the domain adapter capability.
    private static string BuildUnitFamilyId(string domainAdapterTag)
    {
        return $"{GeneratedCapabilityUnitRole}-{Sanitize(domainAdapterTag)}";
    }

    // Increment semantic patch version for the next checkpoint in a unit-family chain.
    private static string IncrementPatchVersion(string version)
    {
        var parts = (version ?? string.Empty).Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 3 &&
            int.TryParse(parts[0], out var major) &&
            int.TryParse(parts[1], out var minor) &&
            int.TryParse(parts[2], out var patch))
            return $"{major}.{minor}.{patch + 1}";

        return "1.0.1";
    }

    // Sanitize ids so generated registry and checkpoint names are stable.
    private static string Sanitize(string value)
    {
        var safeValue = string.IsNullOrWhiteSpace(value) ? "unknown" : value.Trim();
        var chars = safeValue.Select(ch => char.IsLetterOrDigit(ch) ? char.ToLowerInvariant(ch) : '-').ToArray();
        return string.Join("-", new string(chars).Split('-', StringSplitOptions.RemoveEmptyEntries));
    }

    private sealed record TeachingStack(
        LlmModel PolicyModel,
        BpeTokenizer Tokenizer,
        GeneratedCapabilityCheckpointManifest? ParentManifest,
        string Version,
        int TrainingStepCount,
        string? ParentPolicyCheckpointPath,
        bool UsedWarmCache);

    private sealed record CachedTeachingStack(
        LlmModel PolicyModel,
        BpeTokenizer Tokenizer,
        GeneratedCapabilityCheckpointManifest Manifest);

    private sealed record TrainingBranchPlan(
        LogicalFlowResult Branch,
        string SubPhaseId,
        LogicalSubPhase SubPhase,
        string TrainingText,
        string ChosenText,
        string RejectedText,
        float[] TeachingVector);

    private sealed record SemanticTrainingText(string Text, float[] TeachingVector);

    private sealed record DomainClassifierTrainingExample(
        string RunId,
        SemanticCompiler29.Domain Domain,
        int[] Predicates,
        DateTimeOffset CreatedAtUtc);
}

// This factory creates a generated capability flow that selects probationary learned units for retry validation.
public sealed class GeneratedCapabilityFlowFactory : ILogicalFlowFactory<string>
{
    private readonly string _phaseId;
    private readonly string _subPhaseId;
    private readonly string _unitDomainTag;
    private readonly string _taskType;
    private readonly string[] _domainTags;
    private readonly string[] _predicateIds;
    private readonly string _originalPrompt;

    public GeneratedCapabilityFlowFactory(
        string phaseId,
        string subPhaseId,
        string taskType,
        string[] domainTags,
        string[] predicateIds,
        string? unitDomainTag = null,
        string? originalPrompt = null)
    {
        _phaseId = phaseId;
        _subPhaseId = subPhaseId;
        _unitDomainTag = string.IsNullOrWhiteSpace(unitDomainTag) ? subPhaseId : unitDomainTag;
        _taskType = taskType;
        _domainTags = domainTags ?? [];
        _predicateIds = predicateIds ?? [];
        _originalPrompt = originalPrompt ?? string.Empty;
    }

    // Create a flow that uses the newest probationary-or-better generated capability unit.
    public OperationResult<ILogicalFlow<string>> Create(LogicalFlowFactoryRequest request)
    {
        if (request is null)
            return OperationResult<ILogicalFlow<string>>.Fail("Flow factory request is required.", "FLOW_FACTORY_REQUEST_REQUIRED");

        if (!request.Flags.UseLearnedUnitWhenAvailable)
            return OperationResult<ILogicalFlow<string>>.Ok(new UniversalNineNodeDevelopmentFlow(_phaseId, _subPhaseId, _taskType, _domainTags, _predicateIds));

        var learnedUnit = request.Units.ResolveLatest<string, string>(
            DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole,
            _unitDomainTag,
            LogicalUnitTrustState.Probationary);
        if (!learnedUnit.IsSuccess)
            return OperationResult<ILogicalFlow<string>>.Ok(new UniversalNineNodeDevelopmentFlow(_phaseId, _subPhaseId, _taskType, _domainTags, _predicateIds));

        return OperationResult<ILogicalFlow<string>>.Ok(new GeneratedCapabilityFlow(
            _phaseId,
            _subPhaseId,
            _taskType,
            _domainTags,
            _predicateIds,
            _unitDomainTag,
            learnedUnit.Value!,
            request.Topology,
            _originalPrompt));
    }
}

// This flow proves a generated learned unit can become executable on the orchestrator retry path.
public sealed class GeneratedCapabilityFlow : ILogicalFlow<string>
{
    private readonly string _phaseId;
    private readonly string _subPhaseId;
    private readonly string _unitDomainTag;
    private readonly string _taskType;
    private readonly string[] _domainTags;
    private readonly string[] _predicateIds;
    private readonly ILogicalUnit<string, string> _unit;
    private readonly ILogicalTopologyRegistry? _topology;
    private readonly string _originalPrompt;

    public GeneratedCapabilityFlow(
        string phaseId,
        string subPhaseId,
        string taskType,
        string[] domainTags,
        string[] predicateIds,
        string unitDomainTag,
        ILogicalUnit<string, string> unit,
        ILogicalTopologyRegistry? topology = null,
        string? originalPrompt = null)
    {
        _phaseId = phaseId;
        _subPhaseId = subPhaseId;
        _unitDomainTag = unitDomainTag;
        _taskType = taskType;
        _domainTags = domainTags ?? [];
        _predicateIds = predicateIds ?? [];
        _unit = unit;
        _topology = topology;
        _originalPrompt = originalPrompt ?? string.Empty;
    }

    // Execute the learned capability unit and return its output as a flow result.
    public async Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<string> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalFlowResult>.Fail("Cancelled.", "CANCELLED");
        if (string.IsNullOrWhiteSpace(request.Input))
            return OperationResult<LogicalFlowResult>.Fail("Flow input cannot be empty.", "EMPTY_FLOW_INPUT");

        var compiled = new SemanticTeachingPromptCompiler().Compile(request.Input);
        var queryText = compiled.IsSuccess ? compiled.Value!.CanonicalText : request.Input;

        var unitResult = await _unit.ExecuteAsync(new LogicalUnitRequest<string>(
            request.RunId,
            DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole,
            DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole,
            _unitDomainTag,
            queryText,
            request.Flags,
            request.Metadata), cancellationToken);
        if (!unitResult.IsSuccess)
            return OperationResult<LogicalFlowResult>.Fail(unitResult.ErrorMessage, unitResult.ErrorCode);

        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["phase_id"] = _phaseId,
            ["sub_phase_id"] = _subPhaseId,
            ["task_type"] = _taskType,
            ["domain_adapter_tag"] = _unitDomainTag,
            ["generated_capability_flow"] = true.ToString(),
            ["learned_unit_selected"] = true.ToString(),
            ["learned_unit_id"] = unitResult.Value!.UnitId,
            ["learned_unit_confidence"] = unitResult.Value.Confidence.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
        };

        var evidenceIds = unitResult.Value.EvidenceIds
            .Concat([$"generated-capability-flow-{_subPhaseId}"])
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var summary = new LogicalFlowSummary(
            request.FlowId,
            1,
            0,
            1,
            "generated_capability",
            false,
            true,
            false,
            "USE_PROBATIONARY_LEARNED_CAPABILITY",
            unitResult.Value.Confidence,
            $"sub_phase={_subPhaseId}",
            "Generated learned capability executed from registry after training.",
            0,
            0);

        var humanResult = TranslateResult(unitResult.Value.Output);

        var result = new LogicalFlowResult(
            request.FlowId,
            request.ScenarioId,
            "PASS",
            humanResult,
            summary,
            metrics,
            evidenceIds,
            unitResult.Value.Warnings);

        return OperationResult<LogicalFlowResult>.Ok(result);
    }

    private string TranslateResult(string output)
    {
        if (_topology is null)
            return CapabilityResultTranslator.TranslateFromKnnOutput(output, _originalPrompt);

        var subPhase = _topology.ResolveSubPhase(_subPhaseId);
        return subPhase.IsSuccess
            ? CapabilityResultTranslator.Translate(output, subPhase.Value!.SubPhase, _originalPrompt)
            : CapabilityResultTranslator.TranslateFromKnnOutput(output, _originalPrompt);
    }
}
