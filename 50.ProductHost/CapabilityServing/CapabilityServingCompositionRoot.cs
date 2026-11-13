// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProductHost.CapabilityServing;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using Inference;
using Microsoft.Data.Sqlite;
using OrchestratorModel.Logical;

public sealed class CapabilityServingCompositionRoot
{
    public const string ServingBoundary =
        "persisted_generated_capability_serving_only_no_training_no_teacher_no_dpo_no_product_ready";

    private const int MaximumRequestLength = 32_768;
    private const int MaximumIdentityLength = 256;
    private const int MaximumGenerationTokens = 512;
    private const float GenerationTemperature = 0.8f;
    private const float GenerationTopP = 0.95f;
    public const string GenerationInferenceMode =
        "verified_checkpoint_provenance_bound_seeded_autoregressive_kv_cache";
    internal const long MaximumSnapshotBytes = 8 * 1024 * 1024;
    internal const long MaximumTokenizerBytes = 64 * 1024 * 1024;
    internal const long MaximumPolicyCheckpointBytes = 1024L * 1024 * 1024;
    internal const int MaximumRegistryEntries = 256;
    internal const int MaximumArrayEntries = 256;
    internal const int MaximumLoraTargets = 32;
    internal const int MaximumVocabularySize = 1_000_000;
    internal const int MaximumContextLength = 131_072;
    internal const int MaximumHiddenDimension = 32_768;
    internal const int MaximumLayers = 256;
    internal const int MaximumHeads = 256;
    internal const int MaximumFfnDimension = 131_072;
    internal const int MaximumLoraRank = 4_096;
    internal const long MaximumEstimatedModelParameters = 100_000_000;
    internal const long MaximumAggregateArtifactBytes = 512L * 1024 * 1024;

    private readonly CapabilityServingAssistantRunner _runAssistant;
    private readonly IReadOnlyDictionary<string, CapabilityServingAuthorizedBinding>
        _authorizedBindingsByScenario;
    private readonly IReadOnlyDictionary<string, CapabilityServingGenerationRuntime>
        _generationRuntimesByScenario;
    private long _assistantInvocationCount;

    private CapabilityServingCompositionRoot(
        TeachingSession session,
        TeachingRegistrySnapshot snapshot,
        IReadOnlyDictionary<string, CapabilityServingAuthorizedBinding>
            authorizedBindingsByScenario,
        IReadOnlyDictionary<string, CapabilityServingGenerationRuntime>
            generationRuntimesByScenario,
        CapabilityServingAssistantRunner? runnerOverride)
    {
        _runAssistant = runnerOverride ?? session.Assistant.RunAsync;
        _authorizedBindingsByScenario = authorizedBindingsByScenario;
        _generationRuntimesByScenario = generationRuntimesByScenario;
        SnapshotId = snapshot.SnapshotId;
        BundleId = snapshot.BundleId;
        SnapshotRowIndex = snapshot.RowIndex;
        RestartInstanceId = Guid.NewGuid().ToString("N");
    }

    public string SnapshotId { get; }
    public string BundleId { get; }
    public int SnapshotRowIndex { get; }
    public string RestartInstanceId { get; }
    public int ScenarioCount => _authorizedBindingsByScenario.Count;
    public long AssistantInvocationCount => Interlocked.Read(ref _assistantInvocationCount);
    public string EntrypointTraceId => "50.ProductHost->15.OrchestratorModel->18.CapabilityTraining";

    public static OperationResult<CapabilityServingCompositionRoot> Create(
        CapabilityServingHostConfiguration configuration)
        => CreateCore(
            configuration,
            replayVerificationCapability: null,
            afterSnapshotValidated: null);

    public static OperationResult<CapabilityServingCompositionRoot> Create(
        CapabilityServingHostConfiguration configuration,
        LogicalCapabilityReplayVerificationCapability
            replayVerificationCapability)
        => replayVerificationCapability is null
            ? OperationResult<CapabilityServingCompositionRoot>.Fail(
                "A server-issued replay verification capability is required.",
                LogicalCapabilityReplayVerificationCapability.RequiredErrorCode)
            : CreateCore(
                configuration,
                replayVerificationCapability,
                afterSnapshotValidated: null);

    public static OperationResult<CapabilityServingCompositionRoot> Create(
        CapabilityServingHostConfiguration configuration,
        ServerOwnedLegalTrainingTeacherAdmissionAuthority
            legalTeacherAdmissionAuthority)
    {
        if (legalTeacherAdmissionAuthority is null)
        {
            return OperationResult<CapabilityServingCompositionRoot>.Fail(
                "A server-owned legal teacher admission authority is required.",
                LogicalCapabilityReplayVerificationCapability.RequiredErrorCode);
        }

        var verificationCapability = legalTeacherAdmissionAuthority
            .IssueReplayVerificationCapability();
        return verificationCapability.IsSuccess &&
               verificationCapability.Value is not null
            ? Create(configuration, verificationCapability.Value)
            : verificationCapability
                .MapError<CapabilityServingCompositionRoot>();
    }

    internal static OperationResult<CapabilityServingCompositionRoot> CreateForTesting(
        CapabilityServingHostConfiguration configuration,
        Action<TeachingRegistrySnapshot>? afterSnapshotValidated = null,
        Action<TeachingRegistrySnapshot>? afterArtifactsLeased = null,
        CapabilityServingAssistantRunner? runnerOverride = null,
        CapabilityServingGenerationRunner? generationRunnerOverride = null,
        LogicalCapabilityReplayVerificationCapability?
            replayVerificationCapability = null)
        => CreateCore(
            configuration,
            replayVerificationCapability,
            afterSnapshotValidated,
            afterArtifactsLeased,
            runnerOverride,
            generationRunnerOverride);

    private static OperationResult<CapabilityServingCompositionRoot> CreateCore(
        CapabilityServingHostConfiguration configuration,
        LogicalCapabilityReplayVerificationCapability?
            replayVerificationCapability,
        Action<TeachingRegistrySnapshot>? afterSnapshotValidated,
        Action<TeachingRegistrySnapshot>? afterArtifactsLeased = null,
        CapabilityServingAssistantRunner? runnerOverride = null,
        CapabilityServingGenerationRunner? generationRunnerOverride = null)
    {
        if (configuration is null)
        {
            return OperationResult<CapabilityServingCompositionRoot>.Fail(
                "Capability-serving configuration is required.",
                "CAPABILITY_SERVING_CONFIGURATION_REQUIRED");
        }

        CapabilityServingArtifactLease snapshotLease;
        try
        {
            CapabilityServingArtifactLease.ValidatePrivateDirectory(configuration.StateRoot);
            CapabilityServingArtifactLease.ValidatePrivateDirectory(configuration.CheckpointRoot);
            snapshotLease = CapabilityServingArtifactLease.OpenDeferred(
                configuration.SnapshotPath,
                configuration.StateRoot,
                MaximumSnapshotBytes);
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            return OperationResult<CapabilityServingCompositionRoot>.Fail(
                "Capability-serving snapshot could not be loaded.",
                "CAPABILITY_SERVING_SNAPSHOT_INVALID");
        }

        using (snapshotLease)
        {
            var loaded = TeachingRegistryPersistence.LoadBounded(
                snapshotLease.Stream,
                MaximumSnapshotBytes);
            if (!loaded.IsSuccess || loaded.Value is null)
            {
                return OperationResult<CapabilityServingCompositionRoot>.Fail(
                    "Capability-serving snapshot could not be loaded.",
                    "CAPABILITY_SERVING_SNAPSHOT_INVALID");
            }

            var snapshot = loaded.Value;
            var validation = ValidateSnapshot(configuration, snapshot);
            if (!validation.IsSuccess || validation.Value is null)
                return validation.MapError<CapabilityServingCompositionRoot>();
            var authorizedBindings = validation.Value;

            afterSnapshotValidated?.Invoke(snapshot);

            var leases = new List<CapabilityServingArtifactLease>();
            var databaseExisted = File.Exists(configuration.SessionDatabasePath);
            var storageCreated = false;
            try
            {
                var aggregateArtifactBytes = snapshotLease.Length;
                var tokenizerPaths = snapshot.GeneratedCapabilities
                    .Select(manifest => Path.GetFullPath(manifest.TokenizerPath))
                    .ToHashSet(PathComparer);
                foreach (var checkpointPath in snapshot.CheckpointPaths
                             .Select(Path.GetFullPath)
                             .Distinct(PathComparer))
                {
                    var lease = CapabilityServingArtifactLease.OpenDeferred(
                        checkpointPath,
                        configuration.CheckpointRoot,
                        tokenizerPaths.Contains(checkpointPath)
                            ? MaximumTokenizerBytes
                            : MaximumPolicyCheckpointBytes);
                    leases.Add(lease);
                    aggregateArtifactBytes = checked(
                        aggregateArtifactBytes + lease.Length);
                    if (aggregateArtifactBytes > MaximumAggregateArtifactBytes)
                    {
                        return OperationResult<CapabilityServingCompositionRoot>.Fail(
                            "Capability-serving cumulative artifact budget was exceeded.",
                            "CAPABILITY_SERVING_ARTIFACT_BUDGET_EXCEEDED");
                    }
                }

                snapshotLease.CompleteVerification();
                foreach (var lease in leases)
                    lease.CompleteVerification();
                snapshotLease.VerifyUnchanged();
                var heldArtifacts = leases.ToDictionary(
                    lease => lease.RequestedPath,
                    lease => new TeachingRegistryArtifactSource(
                        lease.Stream,
                        lease.MaximumBytes),
                    PathComparer);
                afterArtifactsLeased?.Invoke(snapshot);

                SessionStateStore storage;
                try
                {
                    if (!databaseExisted)
                    {
                        CreatePrivateSessionDatabasePlaceholder(
                            configuration.SessionDatabasePath);
                        storageCreated = true;
                    }
                    storage = new SessionStateStore(configuration.SessionDatabasePath);
                    CapabilityServingArtifactLease.SetPrivateFileMode(
                        configuration.SessionDatabasePath);
                    CapabilityServingArtifactLease.ValidatePrivateFile(
                        configuration.SessionDatabasePath);
                }
                catch (Exception exception) when (IsNonFatal(exception))
                {
                    if (!CleanupCreatedStorage(
                            configuration.SessionDatabasePath,
                            databaseExisted))
                    {
                        return OperationResult<CapabilityServingCompositionRoot>.Fail(
                            "Capability-serving session storage cleanup failed.",
                            "CAPABILITY_SERVING_STORAGE_CLEANUP_FAILED");
                    }
                    return OperationResult<CapabilityServingCompositionRoot>.Fail(
                        "Capability-serving session storage could not be opened.",
                        "CAPABILITY_SERVING_STORAGE_INVALID");
                }

                var opened = replayVerificationCapability is null
                    ? TeachingSessionBuilder.OpenReadOnlyServing(
                        storage,
                        snapshot.BundleId,
                        configuration.CheckpointRoot,
                        configuration.SnapshotPath,
                        snapshot,
                        heldArtifacts)
                    : TeachingSessionBuilder.OpenReadOnlyServing(
                        storage,
                        snapshot.BundleId,
                        configuration.CheckpointRoot,
                        configuration.SnapshotPath,
                        snapshot,
                        heldArtifacts,
                        replayVerificationCapability);
                if (!opened.IsSuccess || opened.Value is null)
                {
                    if (storageCreated &&
                        !CleanupCreatedStorage(
                            configuration.SessionDatabasePath,
                            databaseExisted))
                    {
                        return OperationResult<CapabilityServingCompositionRoot>.Fail(
                            "Capability-serving session storage cleanup failed.",
                            "CAPABILITY_SERVING_STORAGE_CLEANUP_FAILED");
                    }
                    return OperationResult<CapabilityServingCompositionRoot>.Fail(
                        "Capability-serving registry could not be restored.",
                        "CAPABILITY_SERVING_REGISTRY_RESTORE_FAILED");
                }

                snapshotLease.VerifyUnchanged();
                foreach (var lease in leases)
                    lease.VerifyUnchanged();

                var liveDescriptors = opened.Value.Flows.ListDescriptors();
                var liveUnitDescriptors = opened.Value.Units.ListDescriptors();
                foreach (var expected in authorizedBindings.Values)
                {
                    if (HasExactLiveBinding(
                            expected,
                            liveUnitDescriptors,
                            liveDescriptors))
                    {
                        continue;
                    }

                    if (storageCreated &&
                        !CleanupCreatedStorage(
                            configuration.SessionDatabasePath,
                            databaseExisted))
                    {
                        return OperationResult<CapabilityServingCompositionRoot>.Fail(
                            "Capability-serving session storage cleanup failed.",
                            "CAPABILITY_SERVING_STORAGE_CLEANUP_FAILED");
                    }
                    return OperationResult<CapabilityServingCompositionRoot>.Fail(
                        "Capability-serving registry is missing a persisted flow.",
                        "CAPABILITY_SERVING_FLOW_RESTORE_FAILED");
                }

                var generationRuntimes = BuildGenerationRuntimes(
                    snapshot,
                    opened.Value.Units,
                    authorizedBindings,
                    generationRunnerOverride);
                if (!generationRuntimes.IsSuccess || generationRuntimes.Value is null)
                {
                    if (storageCreated &&
                        !CleanupCreatedStorage(
                            configuration.SessionDatabasePath,
                            databaseExisted))
                    {
                        return OperationResult<CapabilityServingCompositionRoot>.Fail(
                            "Capability-serving session storage cleanup failed.",
                            "CAPABILITY_SERVING_STORAGE_CLEANUP_FAILED");
                    }
                    return generationRuntimes.MapError<CapabilityServingCompositionRoot>();
                }

                return OperationResult<CapabilityServingCompositionRoot>.Ok(
                    new CapabilityServingCompositionRoot(
                        opened.Value,
                        snapshot,
                        authorizedBindings,
                        generationRuntimes.Value,
                        runnerOverride));
            }
            catch (Exception exception) when (IsNonFatal(exception))
            {
                if (storageCreated &&
                    !CleanupCreatedStorage(
                        configuration.SessionDatabasePath,
                        databaseExisted))
                {
                    return OperationResult<CapabilityServingCompositionRoot>.Fail(
                        "Capability-serving session storage cleanup failed.",
                        "CAPABILITY_SERVING_STORAGE_CLEANUP_FAILED");
                }
                return OperationResult<CapabilityServingCompositionRoot>.Fail(
                    "Capability-serving artifact boundary is invalid.",
                    "CAPABILITY_SERVING_ARTIFACT_INVALID");
            }
            finally
            {
                foreach (var lease in leases)
                    lease.Dispose();
            }
        }
    }

    public async Task<OperationResult<LogicalAssistantRunResult>> ExecuteAsync(
        CapabilityServingRequest request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled();
        if (request is null)
        {
            return OperationResult<LogicalAssistantRunResult>.Fail(
                "Capability-serving request is required.",
                "CAPABILITY_SERVING_REQUEST_REQUIRED");
        }

        if (!IsSafePayload(request.UserRequest, MaximumRequestLength))
        {
            return OperationResult<LogicalAssistantRunResult>.Fail(
                "Capability-serving user request is invalid.",
                "CAPABILITY_SERVING_USER_REQUEST_INVALID");
        }

        if (!IsSafeIdentity(request.ScenarioId))
        {
            return OperationResult<LogicalAssistantRunResult>.Fail(
                "Capability-serving scenario id is invalid.",
                "CAPABILITY_SERVING_SCENARIO_INVALID");
        }

        if (!_authorizedBindingsByScenario.TryGetValue(
                request.ScenarioId,
                out var authorizedBinding))
        {
            return OperationResult<LogicalAssistantRunResult>.Fail(
                "Capability-serving scenario is not authorized.",
                "CAPABILITY_SERVING_SCENARIO_NOT_AUTHORIZED");
        }

        var flags = new LogicalExecutionFlags
        {
            UseCompiledPredicateRepresentation = true,
            UseDomainScopedRetrieval = true,
            UseLowConfidenceLessonSignal = true,
            UseLearnedUnitWhenAvailable = true,
            UseTraceEvidence = true,
            UseContextCarryForward = false,
            UseIdleLearningScheduling = false,
            UseE35DependencyDetection = false,
            UseSkillRetriever = false,
            UseReplayFusionRanking = false,
            UseArbiterAuthority = true,
            UseKnowledgeEdgeDetection = false,
            UseInternalFirstExecution = true,
            RequireExternalAiApproval = false,
            RejectArbiterContradictions = true,
            UseDeterministicFixtureBaseline = false,
            WriteEvidenceArtifacts = false,
            CompareAgainstOracle = false,
            FailOnMetricDrift = false
        };

        OperationResult<LogicalAssistantRunResult> result;
        try
        {
            Interlocked.Increment(ref _assistantInvocationCount);
            result = await _runAssistant(
                request.UserRequest,
                request.ScenarioId,
                flags,
                new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["source"] = "product_host_capability_serving",
                    ["serving_boundary"] = ServingBoundary,
                    ["snapshot_id"] = SnapshotId
                },
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            return OperationResult<LogicalAssistantRunResult>.Fail(
                "Capability-serving execution failed.",
                "CAPABILITY_SERVING_EXECUTION_FAILED");
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();
        if (!result.IsSuccess || result.Value is null)
            return result;

        if (!string.Equals(result.Value.ScenarioId, request.ScenarioId, StringComparison.Ordinal) ||
            !string.Equals(
                result.Value.SelectedFlowId,
                authorizedBinding.FlowId,
                StringComparison.Ordinal))
        {
            return OperationResult<LogicalAssistantRunResult>.Fail(
                "Capability-serving route did not preserve its persisted identity.",
                "CAPABILITY_SERVING_ROUTE_BOUNDARY_VIOLATION");
        }

        var applied = result.Value.AppliedFlags;
        if (!applied.UseLearnedUnitWhenAvailable ||
            applied.UseIdleLearningScheduling ||
            applied.UseKnowledgeEdgeDetection ||
            applied.WriteEvidenceArtifacts ||
            applied.CompareAgainstOracle ||
            applied.FailOnMetricDrift)
        {
            return OperationResult<LogicalAssistantRunResult>.Fail(
                "Capability-serving execution crossed the no-training boundary.",
                "CAPABILITY_SERVING_FLAG_BOUNDARY_VIOLATION");
        }

        var metrics = result.Value.Metrics;
        if (metrics is null ||
            !HasExactMetric(metrics, "generated_capability_flow", bool.TrueString) ||
            !HasExactMetric(metrics, "retrieval_unit_selected", bool.TrueString) ||
            !HasExactMetric(metrics, "learned_unit_selected", bool.FalseString) ||
            !HasExactMetric(
                metrics,
                "retrieval_unit_id",
                authorizedBinding.UnitId))
        {
            return OperationResult<LogicalAssistantRunResult>.Fail(
                "Capability-serving execution did not use its authorized generated capability.",
                "CAPABILITY_SERVING_EXECUTABLE_BOUNDARY_VIOLATION");
        }

        return result;
    }

    // Generate directly from the exact restored model and tokenizer bound to one authorized scenario.
    public async Task<OperationResult<CapabilityGenerationResult>> GenerateAsync(
        CapabilityGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return GenerationCancelled();
        if (request is null)
        {
            return OperationResult<CapabilityGenerationResult>.Fail(
                "Capability-generation request is required.",
                "CAPABILITY_GENERATION_REQUEST_REQUIRED");
        }

        // Authorize the exact scenario before exposing any scenario-specific runtime state.
        if (!IsSafeIdentity(request.ScenarioId))
        {
            return OperationResult<CapabilityGenerationResult>.Fail(
                "Capability-generation scenario id is invalid.",
                "CAPABILITY_GENERATION_SCENARIO_INVALID");
        }
        if (!_authorizedBindingsByScenario.ContainsKey(request.ScenarioId))
        {
            return OperationResult<CapabilityGenerationResult>.Fail(
                "Capability-generation scenario is not authorized.",
                "CAPABILITY_GENERATION_SCENARIO_NOT_AUTHORIZED");
        }

        if (!IsSafePayload(request.UserRequest, MaximumRequestLength))
        {
            return OperationResult<CapabilityGenerationResult>.Fail(
                "Capability-generation user request is invalid.",
                "CAPABILITY_GENERATION_USER_REQUEST_INVALID");
        }
        if (request.MaxNewTokens is < 1 or > MaximumGenerationTokens)
        {
            return OperationResult<CapabilityGenerationResult>.Fail(
                "Capability-generation token limit is invalid.",
                "CAPABILITY_GENERATION_MAX_NEW_TOKENS_INVALID");
        }
        if (!_generationRuntimesByScenario.TryGetValue(
                request.ScenarioId,
                out var runtime))
        {
            return OperationResult<CapabilityGenerationResult>.Fail(
                "Capability generation requires verified artifact provenance.",
                "CAPABILITY_GENERATION_PROVENANCE_REQUIRED");
        }

        IReadOnlyList<int> promptTokens;
        try
        {
            promptTokens = runtime.Runtime.Tokenizer.Encode(request.UserRequest);
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            return OperationResult<CapabilityGenerationResult>.Fail(
                "Capability-generation prompt could not be tokenized.",
                "CAPABILITY_GENERATION_FAILED");
        }

        if (promptTokens.Count == 0)
        {
            return OperationResult<CapabilityGenerationResult>.Fail(
                "Capability-generation prompt produced no tokens.",
                "CAPABILITY_GENERATION_PROMPT_EMPTY");
        }
        if ((long)promptTokens.Count + request.MaxNewTokens - 1 >
            runtime.Runtime.Model.Config.ContextLen)
        {
            return OperationResult<CapabilityGenerationResult>.Fail(
                "Capability-generation context budget was exceeded.",
                "CAPABILITY_GENERATION_CONTEXT_BUDGET_EXCEEDED");
        }

        var gateEntered = false;
        try
        {
            await runtime.Gate.WaitAsync(cancellationToken);
            gateEntered = true;
            cancellationToken.ThrowIfCancellationRequested();

            var topK = Math.Min(50, runtime.Runtime.Model.Config.VocabSize);
            var generatedText = await Task.Run(
                () => runtime.Runner(
                    runtime.Generator,
                    request.UserRequest,
                    request.MaxNewTokens,
                    GenerationTemperature,
                    topK,
                    GenerationTopP,
                    request.Seed,
                    cancellationToken),
                CancellationToken.None);
            cancellationToken.ThrowIfCancellationRequested();

            return OperationResult<CapabilityGenerationResult>.Ok(
                new CapabilityGenerationResult(
                    generatedText,
                    runtime.ScenarioId,
                    runtime.FlowId,
                    runtime.UnitId,
                    request.Seed,
                    request.MaxNewTokens,
                    runtime.TokenizerSha256,
                    runtime.PolicyCheckpointSha256,
                    runtime.RuntimeProvenanceSha256,
                    GenerationInferenceMode,
                    CapabilityServingReady: true,
                    ProductReady: false,
                    PerformsTraining: false,
                    DpoReady: false,
                    TeacherReady: false,
                    ServingBoundary));
        }
        catch (OperationCanceledException exception)
            when (cancellationToken.IsCancellationRequested &&
                  exception.CancellationToken == cancellationToken)
        {
            return GenerationCancelled();
        }
        catch (ArgumentException exception) when (IsNonFatal(exception))
        {
            var mappedCode = MapTextGeneratorArgumentCode(exception.Message);
            return mappedCode is null
                ? GenerationFailed()
                : OperationResult<CapabilityGenerationResult>.Fail(
                    "Capability generation was rejected.",
                    mappedCode);
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            return GenerationFailed();
        }
        finally
        {
            if (gateEntered)
                runtime.Gate.Release();
        }
    }

    private static OperationResult<IReadOnlyDictionary<string, CapabilityServingGenerationRuntime>>
        BuildGenerationRuntimes(
            TeachingRegistrySnapshot snapshot,
            InMemoryLogicalUnitRegistry units,
            IReadOnlyDictionary<string, CapabilityServingAuthorizedBinding> authorizedBindings,
            CapabilityServingGenerationRunner? runnerOverride)
    {
        var runtimes = new Dictionary<string, CapabilityServingGenerationRuntime>(
            StringComparer.Ordinal);
        foreach (var manifest in snapshot.GeneratedCapabilities)
        {
            if (!HasCompleteGenerationProvenance(manifest))
                continue;

            if (!authorizedBindings.TryGetValue(manifest.ScenarioId, out var binding))
                return InvalidGenerationRuntime();

            var resolved = units.ResolveLatestById<string, string>(manifest.UnitId);
            if (!resolved.IsSuccess ||
                resolved.Value is null ||
                !IsExactLiveUnitDescriptor(resolved.Value.Descriptor, binding) ||
                resolved.Value.Unit.GetType() !=
                    typeof(TransformerEmbeddingKnnLogicalUnit<string>) ||
                resolved.Value.Unit is not TransformerEmbeddingKnnLogicalUnit<string> unit)
            {
                return InvalidGenerationRuntime();
            }

            var retainedRuntime = unit.GetServingRuntime();
            var generator = new TextGenerator(retainedRuntime.Model, retainedRuntime.Tokenizer);
            if (!runtimes.TryAdd(
                    manifest.ScenarioId,
                    new CapabilityServingGenerationRuntime(
                        manifest.ScenarioId,
                        manifest.FlowId,
                        manifest.UnitId,
                        manifest.TokenizerSha256!,
                        manifest.PolicyCheckpointSha256!,
                        manifest.RuntimeProvenanceSha256!,
                        retainedRuntime,
                        generator,
                        runnerOverride ?? RunTextGenerator)))
            {
                return InvalidGenerationRuntime();
            }
        }

        return OperationResult<
            IReadOnlyDictionary<string, CapabilityServingGenerationRuntime>>.Ok(runtimes);
    }

    private static string RunTextGenerator(
        TextGenerator generator,
        string prompt,
        int maxNewTokens,
        float temperature,
        int topK,
        float topP,
        int seed,
        CancellationToken cancellationToken)
        => generator.Generate(
            prompt,
            maxNewTokens,
            temperature,
            topK,
            topP,
            seed,
            cancellationToken);

    private static OperationResult<IReadOnlyDictionary<string, CapabilityServingAuthorizedBinding>>
        ValidateSnapshot(
        CapabilityServingHostConfiguration configuration,
        TeachingRegistrySnapshot snapshot)
    {
        if (!IsSafeIdentity(snapshot.SnapshotId) ||
            !IsSafeIdentity(snapshot.BundleId) ||
            snapshot.RowIndex < 0 ||
            snapshot.Units is null ||
            snapshot.Units.Length > MaximumRegistryEntries ||
            snapshot.Flows is null ||
            snapshot.Flows.Length > MaximumRegistryEntries ||
            snapshot.GeneratedCapabilities is null ||
            snapshot.GeneratedCapabilities.Length == 0 ||
            snapshot.GeneratedCapabilities.Length > MaximumRegistryEntries ||
            snapshot.CheckpointPaths is null ||
            snapshot.CheckpointPaths.Length == 0 ||
            snapshot.CheckpointPaths.Length > MaximumRegistryEntries)
        {
            return InvalidSnapshot();
        }

        var allUnitIds = new HashSet<string>(StringComparer.Ordinal);
        var allFlowIds = new HashSet<string>(StringComparer.Ordinal);
        var manifestUnitIds = new HashSet<string>(StringComparer.Ordinal);
        var manifestFlowIds = new HashSet<string>(StringComparer.Ordinal);
        var scenarioIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var unit in snapshot.Units)
        {
            if (unit is null ||
                !IsSafeIdentity(unit.UnitId) ||
                !IsSafeIdentity(unit.UnitRole) ||
                !IsSafeIdentity(unit.DomainTag) ||
                !IsSafeIdentity(unit.Version) ||
                !IsSafeIdentity(unit.TrustState) ||
                !IsSafeText(unit.InputTypeName, 2_048) ||
                !IsSafeText(unit.OutputTypeName, 2_048) ||
                !IsSafeIdentity(unit.UnitKind) ||
                unit.RegistrationOrder < 0 ||
                !allUnitIds.Add(unit.UnitId))
            {
                return InvalidSnapshot();
            }
        }

        foreach (var flow in snapshot.Flows)
        {
            if (flow is null ||
                !IsSafeIdentity(flow.FlowId) ||
                !IsSafeIdentity(flow.ScenarioId) ||
                !IsSafeIdentity(flow.Version) ||
                !IsSafeText(flow.InputTypeName, 2_048) ||
                !IsSafeIdentity(flow.PhaseId) ||
                !IsSafeIdentity(flow.SubPhaseId) ||
                !IsSafeIdentity(flow.TaskType) ||
                !AreSafeIdentities(flow.DomainTags, MaximumArrayEntries) ||
                flow.RegistrationOrder < 0 ||
                !IsSafeIdentity(flow.FlowKind) ||
                !allFlowIds.Add(flow.FlowId))
            {
                return InvalidSnapshot();
            }
        }

        long aggregateModelParameters = 0;
        var authorizedBindings =
            new Dictionary<string, CapabilityServingAuthorizedBinding>(
                StringComparer.Ordinal);
        foreach (var manifest in snapshot.GeneratedCapabilities)
        {
            if (manifest is null ||
                !IsSafeIdentity(manifest.UnitId) ||
                !IsSafeIdentity(manifest.UnitRole) ||
                !IsSafeIdentity(manifest.DomainTag) ||
                !IsSafeIdentity(manifest.Version) ||
                !IsSafeIdentity(manifest.TrustState) ||
                !IsSafeIdentity(manifest.FlowId) ||
                !IsSafeIdentity(manifest.ScenarioId) ||
                !IsSafeIdentity(manifest.PhaseId) ||
                !IsSafeIdentity(manifest.SubPhaseId) ||
                !IsSafeIdentity(manifest.TaskType) ||
                !IsSafePayload(manifest.TrainingText, MaximumRequestLength) ||
                !IsSafePayload(manifest.OutputText, MaximumRequestLength) ||
                !IsSafeIdentity(manifest.ExampleId) ||
                !AreSafeIdentities(manifest.PredicateIds, MaximumArrayEntries) ||
                !AreSafeIdentities(manifest.DomainTags, MaximumArrayEntries) ||
                !IsCanonicalLoraTargets(manifest.LoraTargetModules) ||
                !manifestUnitIds.Add(manifest.UnitId) ||
                !manifestFlowIds.Add(manifest.FlowId) ||
                !scenarioIds.Add(manifest.ScenarioId) ||
                !IsBoundedModelIdentity(manifest) ||
                !configuration.IsOwnedCheckpointArtifact(manifest.PolicyCheckpointPath) ||
                !configuration.IsOwnedCheckpointArtifact(manifest.TokenizerPath))
            {
                return InvalidSnapshot();
            }

            if (!TryEstimateModelParameters(manifest, out var estimatedParameters))
                return InvalidSnapshot();
            try
            {
                aggregateModelParameters = checked(
                    aggregateModelParameters + estimatedParameters);
            }
            catch (OverflowException)
            {
                return InvalidSnapshot();
            }
            if (aggregateModelParameters > MaximumEstimatedModelParameters)
                return InvalidSnapshot();

            if (!TryParseServingTrustState(manifest.TrustState, out var trustState) ||
                trustState is not (
                    LogicalUnitTrustState.Probationary or
                    LogicalUnitTrustState.Trusted))
            {
                return InvalidSnapshot();
            }

            var matchingUnits = snapshot.Units
                .Where(unit => string.Equals(
                    unit.UnitId,
                    manifest.UnitId,
                    StringComparison.Ordinal))
                .ToArray();
            var matchingFlows = snapshot.Flows
                .Where(flow => string.Equals(
                    flow.FlowId,
                    manifest.FlowId,
                    StringComparison.Ordinal))
                .ToArray();
            if (matchingUnits.Length != 1 ||
                matchingFlows.Length != 1 ||
                !IsExactServingUnitBinding(
                    matchingUnits[0],
                    manifest,
                    trustState) ||
                !IsExactServingFlowBinding(matchingFlows[0], manifest))
            {
                return InvalidSnapshot();
            }

            if (!authorizedBindings.TryAdd(
                    manifest.ScenarioId,
                    new CapabilityServingAuthorizedBinding(
                        manifest.ScenarioId,
                        manifest.FlowId,
                        manifest.UnitId,
                        manifest.UnitRole,
                        manifest.DomainTag,
                        manifest.Version,
                        trustState,
                        manifest.PhaseId,
                        manifest.SubPhaseId,
                        manifest.TaskType,
                        [.. manifest.DomainTags])))
            {
                return InvalidSnapshot();
            }
        }

        if (snapshot.Units.Count(unit => string.Equals(
                unit.UnitKind,
                TeachingRegistrySnapshotKinds.GeneratedCapabilityUnit,
                StringComparison.Ordinal)) != snapshot.GeneratedCapabilities.Length ||
            snapshot.Flows.Count(flow => string.Equals(
                flow.FlowKind,
                TeachingRegistrySnapshotKinds.GeneratedCapabilityFlow,
                StringComparison.Ordinal)) != snapshot.GeneratedCapabilities.Length)
        {
            return InvalidSnapshot();
        }

        if (snapshot.CheckpointPaths.Any(path => !configuration.IsOwnedCheckpointArtifact(path)))
            return InvalidSnapshot();

        var expectedCheckpointPaths = snapshot.GeneratedCapabilities
            .SelectMany(manifest => new[]
            {
                Path.GetFullPath(manifest.PolicyCheckpointPath),
                Path.GetFullPath(manifest.TokenizerPath)
            })
            .ToHashSet(PathComparer);
        var declaredCheckpointPaths = snapshot.CheckpointPaths
            .Select(Path.GetFullPath)
            .ToArray();
        if (declaredCheckpointPaths.Distinct(PathComparer).Count() != declaredCheckpointPaths.Length ||
            !expectedCheckpointPaths.SetEquals(declaredCheckpointPaths))
        {
            return InvalidSnapshot();
        }

        return OperationResult<
            IReadOnlyDictionary<string, CapabilityServingAuthorizedBinding>>.Ok(
            authorizedBindings);
    }

    private static bool IsBoundedModelIdentity(
        GeneratedCapabilityCheckpointManifest manifest)
        => manifest.VocabSize is > 0 and <= MaximumVocabularySize &&
           manifest.ContextLen is > 0 and <= MaximumContextLength &&
           manifest.HiddenDim is > 0 and <= MaximumHiddenDimension &&
           manifest.NumLayers is > 0 and <= MaximumLayers &&
           manifest.NumHeads is > 0 and <= MaximumHeads &&
           manifest.NumKvHeads is > 0 and <= MaximumHeads &&
           manifest.FfnHiddenDim is > 0 and <= MaximumFfnDimension &&
           manifest.LoraRank is > 0 and <= MaximumLoraRank &&
           manifest.HiddenDim % manifest.NumHeads == 0 &&
           manifest.NumHeads % manifest.NumKvHeads == 0 &&
           float.IsFinite(manifest.LoraAlpha) &&
           manifest.LoraAlpha > 0 &&
           float.IsFinite(manifest.LoraDropout) &&
           manifest.LoraDropout >= 0 &&
           manifest.LoraDropout < 1 &&
           manifest.TrainingStepCount >= 0 &&
           TryEstimateModelParameters(manifest, out var estimatedParameters) &&
           estimatedParameters <= MaximumEstimatedModelParameters &&
           IsOptionalSafePayload(manifest.OriginalPrompt, MaximumRequestLength) &&
           IsOptionalSafeText(manifest.UnitFamilyId, MaximumIdentityLength) &&
           IsOptionalSafeText(manifest.ParentUnitId, MaximumIdentityLength) &&
           IsOptionalSafeText(manifest.ParentPolicyCheckpointPath, 32_768) &&
           IsOptionalSafeText(manifest.ParentTokenizerPath, 32_768) &&
           IsOptionalSafeText(manifest.ServerBaseUrl, 2_048) &&
           IsOptionalSafeText(manifest.ServerUnitEndpoint, 2_048) &&
           IsOptionalSafeText(manifest.ManifestSha256, MaximumIdentityLength) &&
           IsOptionalSafeText(manifest.TrainingBranch, MaximumIdentityLength) &&
           IsOptionalSafeText(manifest.TrainingRunId, MaximumIdentityLength) &&
           IsOptionalSafeText(manifest.SourceDatasetSha256, MaximumIdentityLength);

    private static bool TryEstimateModelParameters(
        GeneratedCapabilityCheckpointManifest manifest,
        out long estimatedParameters)
    {
        estimatedParameters = 0;
        if (manifest.VocabSize <= 0 ||
            manifest.HiddenDim <= 0 ||
            manifest.NumLayers <= 0 ||
            manifest.NumHeads <= 0 ||
            manifest.NumKvHeads <= 0 ||
            manifest.FfnHiddenDim <= 0 ||
            manifest.LoraRank <= 0 ||
            manifest.HiddenDim % manifest.NumHeads != 0 ||
            manifest.NumHeads % manifest.NumKvHeads != 0 ||
            !IsCanonicalLoraTargets(manifest.LoraTargetModules))
        {
            return false;
        }

        try
        {
            var vocabulary = (long)manifest.VocabSize;
            var hidden = (long)manifest.HiddenDim;
            var layers = (long)manifest.NumLayers;
            var heads = (long)manifest.NumHeads;
            var keyValueHeads = (long)manifest.NumKvHeads;
            var headDimension = hidden / heads;
            var feedForward = (long)manifest.FfnHiddenDim;
            var rank = (long)manifest.LoraRank;

            var queryOutput = checked(heads * headDimension);
            var keyValueOutput = checked(keyValueHeads * headDimension);
            var tokenEmbedding = checked(vocabulary * hidden);
            var languageModelHead = checked(hidden * vocabulary);

            var queryProjection = checked(hidden * queryOutput);
            var keyProjection = checked(hidden * keyValueOutput);
            var valueProjection = checked(hidden * keyValueOutput);
            var outputProjection = checked(hidden * hidden);
            var attentionPerLayer = checked(
                checked(queryProjection + keyProjection) +
                checked(valueProjection + outputProjection));

            var gateProjection = checked(hidden * feedForward);
            var upProjection = checked(hidden * feedForward);
            var downProjection = checked(feedForward * hidden);
            var feedForwardPerLayer = checked(
                checked(gateProjection + upProjection) + downProjection);
            var normalizationPerLayer = checked(2L * hidden);

            long loraPerLayer = 0;
            foreach (var target in manifest.LoraTargetModules)
            {
                var outputFeatures = target switch
                {
                    "q_proj" => queryOutput,
                    "k_proj" => keyValueOutput,
                    "v_proj" => keyValueOutput,
                    "o_proj" => hidden,
                    _ => throw new InvalidDataException(
                        "LoRA target escaped ProductHost validation.")
                };
                var adapterA = checked(rank * hidden);
                var adapterB = checked(outputFeatures * rank);
                loraPerLayer = checked(
                    loraPerLayer + checked(adapterA + adapterB));
            }

            var completeLayer = checked(
                checked(attentionPerLayer + feedForwardPerLayer) +
                checked(normalizationPerLayer + loraPerLayer));
            var transformer = checked(layers * completeLayer);
            estimatedParameters = checked(
                checked(tokenEmbedding + languageModelHead) +
                checked(transformer + hidden));
            return estimatedParameters > 0;
        }
        catch (Exception exception)
            when (exception is OverflowException or InvalidDataException)
        {
            estimatedParameters = 0;
            return false;
        }
    }

    private static bool IsCanonicalLoraTargets(string[]? targets)
    {
        if (targets is null ||
            targets.Length == 0 ||
            targets.Length > MaximumLoraTargets)
        {
            return false;
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var target in targets)
        {
            if (target is not ("q_proj" or "k_proj" or "v_proj" or "o_proj") ||
                !seen.Add(target))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsExactServingUnitBinding(
        TeachingUnitRegistryEntry unit,
        GeneratedCapabilityCheckpointManifest manifest,
        LogicalUnitTrustState trustState)
        => string.Equals(unit.UnitId, manifest.UnitId, StringComparison.Ordinal) &&
           string.Equals(
               unit.UnitRole,
               DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole,
               StringComparison.Ordinal) &&
           string.Equals(unit.UnitRole, manifest.UnitRole, StringComparison.Ordinal) &&
           string.Equals(unit.DomainTag, manifest.DomainTag, StringComparison.Ordinal) &&
           string.Equals(unit.Version, manifest.Version, StringComparison.Ordinal) &&
           string.Equals(unit.TrustState, manifest.TrustState, StringComparison.Ordinal) &&
           TryParseServingTrustState(unit.TrustState, out var unitTrustState) &&
           unitTrustState == trustState &&
           string.Equals(
               unit.InputTypeName,
               TypeName(typeof(string)),
               StringComparison.Ordinal) &&
           string.Equals(
               unit.OutputTypeName,
               TypeName(typeof(string)),
               StringComparison.Ordinal) &&
           string.Equals(
               unit.UnitKind,
               TeachingRegistrySnapshotKinds.GeneratedCapabilityUnit,
               StringComparison.Ordinal);

    private static bool IsExactServingFlowBinding(
        TeachingFlowRegistryEntry flow,
        GeneratedCapabilityCheckpointManifest manifest)
        => string.Equals(flow.FlowId, manifest.FlowId, StringComparison.Ordinal) &&
           string.Equals(flow.ScenarioId, manifest.ScenarioId, StringComparison.Ordinal) &&
           string.Equals(flow.Version, manifest.Version, StringComparison.Ordinal) &&
           string.Equals(
               flow.InputTypeName,
               TypeName(typeof(string)),
               StringComparison.Ordinal) &&
           string.Equals(flow.PhaseId, manifest.PhaseId, StringComparison.Ordinal) &&
           string.Equals(flow.SubPhaseId, manifest.SubPhaseId, StringComparison.Ordinal) &&
           string.Equals(flow.TaskType, manifest.TaskType, StringComparison.Ordinal) &&
           flow.DomainTags.SequenceEqual(
               manifest.DomainTags,
               StringComparer.Ordinal) &&
           string.Equals(
               flow.FlowKind,
               TeachingRegistrySnapshotKinds.GeneratedCapabilityFlow,
               StringComparison.Ordinal);

    private static bool HasExactLiveBinding(
        CapabilityServingAuthorizedBinding binding,
        IReadOnlyList<LogicalUnitDescriptor> liveUnits,
        IReadOnlyList<LogicalFlowDescriptor> liveFlows)
    {
        var matchingUnits = liveUnits.Where(unit =>
            string.Equals(unit.UnitId, binding.UnitId, StringComparison.Ordinal) &&
            string.Equals(unit.UnitRole, binding.UnitRole, StringComparison.Ordinal) &&
            string.Equals(unit.DomainTag, binding.DomainTag, StringComparison.Ordinal) &&
            string.Equals(unit.Version, binding.Version, StringComparison.Ordinal) &&
            unit.TrustState == binding.TrustState &&
            !unit.IsLearned &&
            unit.InputType == typeof(string) &&
            unit.OutputType == typeof(string)).ToArray();
        var matchingFlows = liveFlows.Where(flow =>
            string.Equals(flow.FlowId, binding.FlowId, StringComparison.Ordinal) &&
            string.Equals(flow.ScenarioId, binding.ScenarioId, StringComparison.Ordinal) &&
            string.Equals(flow.Version, binding.Version, StringComparison.Ordinal) &&
            flow.InputType == typeof(string) &&
            string.Equals(flow.PhaseId, binding.PhaseId, StringComparison.Ordinal) &&
            string.Equals(flow.SubPhaseId, binding.SubPhaseId, StringComparison.Ordinal) &&
            string.Equals(flow.TaskType, binding.TaskType, StringComparison.Ordinal) &&
            flow.DomainTags.SequenceEqual(
                binding.DomainTags,
                StringComparer.Ordinal)).ToArray();
        return matchingUnits.Length == 1 && matchingFlows.Length == 1;
    }

    private static bool IsExactLiveUnitDescriptor(
        LogicalUnitDescriptor descriptor,
        CapabilityServingAuthorizedBinding binding)
        => string.Equals(descriptor.UnitId, binding.UnitId, StringComparison.Ordinal) &&
           string.Equals(descriptor.UnitRole, binding.UnitRole, StringComparison.Ordinal) &&
           string.Equals(descriptor.DomainTag, binding.DomainTag, StringComparison.Ordinal) &&
           string.Equals(descriptor.Version, binding.Version, StringComparison.Ordinal) &&
           descriptor.TrustState == binding.TrustState &&
           !descriptor.IsLearned &&
           descriptor.InputType == typeof(string) &&
           descriptor.OutputType == typeof(string) &&
           descriptor.RegistrationOrder > 0;

    private static bool HasCompleteGenerationProvenance(
        GeneratedCapabilityCheckpointManifest manifest)
        => manifest.RopeTheta is not null &&
           manifest.RmsNormEps is not null &&
           float.IsFinite(manifest.RopeTheta.Value) &&
           manifest.RopeTheta.Value > 0f &&
           float.IsFinite(manifest.RmsNormEps.Value) &&
           manifest.RmsNormEps.Value > 0f &&
           RuntimeProvenance.IsSha256Hex(manifest.TokenizerSha256) &&
           RuntimeProvenance.IsSha256Hex(manifest.PolicyCheckpointSha256) &&
           RuntimeProvenance.IsSha256Hex(manifest.RuntimeProvenanceSha256);

    private static bool TryParseServingTrustState(
        string? value,
        out LogicalUnitTrustState trustState)
        => Enum.TryParse(value, ignoreCase: false, out trustState) &&
           Enum.IsDefined(trustState);

    private static string TypeName(Type type)
        => type.AssemblyQualifiedName ?? type.FullName ?? type.Name;

    private static bool HasExactMetric(
        IReadOnlyDictionary<string, string> metrics,
        string key,
        string expectedValue)
        => metrics.TryGetValue(key, out var actualValue) &&
           string.Equals(actualValue, expectedValue, StringComparison.Ordinal);

    private static bool AreSafeIdentities(string[]? values, int maximumCount)
        => values is not null &&
           values.Length > 0 &&
           values.Length <= maximumCount &&
           values.All(IsSafeIdentity);

    private static bool IsOptionalSafeText(string? value, int maximumLength)
        => string.IsNullOrEmpty(value) || IsSafeText(value, maximumLength);

    private static bool IsOptionalSafePayload(string? value, int maximumLength)
        => string.IsNullOrEmpty(value) || IsSafePayload(value, maximumLength);

    private static StringComparer PathComparer => OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    private static void CreatePrivateSessionDatabasePlaceholder(string databasePath)
    {
        var options = new FileStreamOptions
        {
            Mode = FileMode.CreateNew,
            Access = FileAccess.ReadWrite,
            Share = FileShare.None,
            Options = FileOptions.WriteThrough,
            BufferSize = 1
        };
        if (!OperatingSystem.IsWindows())
        {
            options.UnixCreateMode =
                UnixFileMode.UserRead | UnixFileMode.UserWrite;
        }

        using var stream = new FileStream(databasePath, options);
        stream.Flush(flushToDisk: true);
        CapabilityServingArtifactLease.ValidatePrivateFile(databasePath);
    }

    private static bool CleanupCreatedStorage(string databasePath, bool databaseExisted)
    {
        if (databaseExisted)
            return true;

        try
        {
            SqliteConnection.ClearAllPools();
            foreach (var path in new[]
                     {
                         databasePath,
                         databasePath + "-shm",
                         databasePath + "-wal",
                         databasePath + "-journal"
                     })
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            return !File.Exists(databasePath) &&
                   !File.Exists(databasePath + "-shm") &&
                   !File.Exists(databasePath + "-wal") &&
                   !File.Exists(databasePath + "-journal");
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            return false;
        }
    }

    private static OperationResult<
        IReadOnlyDictionary<string, CapabilityServingAuthorizedBinding>>
        InvalidSnapshot()
        => OperationResult<
            IReadOnlyDictionary<string, CapabilityServingAuthorizedBinding>>.Fail(
            "Capability-serving snapshot is invalid.",
            "CAPABILITY_SERVING_SNAPSHOT_INVALID");

    private static OperationResult<LogicalAssistantRunResult> Cancelled()
        => OperationResult<LogicalAssistantRunResult>.Fail("Cancelled.", "CANCELLED");

    private static OperationResult<CapabilityGenerationResult> GenerationCancelled()
        => OperationResult<CapabilityGenerationResult>.Fail("Cancelled.", "CANCELLED");

    private static OperationResult<CapabilityGenerationResult> GenerationFailed()
        => OperationResult<CapabilityGenerationResult>.Fail(
            "Capability generation failed.",
            "CAPABILITY_GENERATION_FAILED");

    private static OperationResult<IReadOnlyDictionary<string, CapabilityServingGenerationRuntime>>
        InvalidGenerationRuntime()
        => OperationResult<
            IReadOnlyDictionary<string, CapabilityServingGenerationRuntime>>.Fail(
            "Capability-generation runtime binding is invalid.",
            "CAPABILITY_GENERATION_RUNTIME_INVALID");

    private static string? MapTextGeneratorArgumentCode(string? message)
        => message switch
        {
            "TEXT_GENERATOR_PROMPT_EMPTY" => "CAPABILITY_GENERATION_PROMPT_EMPTY",
            "TEXT_GENERATOR_PROMPT_CONTEXT_LENGTH_EXCEEDED" or
            "TEXT_GENERATOR_CONTEXT_BUDGET_EXCEEDED" =>
                "CAPABILITY_GENERATION_CONTEXT_BUDGET_EXCEEDED",
            "TEXT_GENERATOR_MAX_NEW_TOKENS_NEGATIVE" =>
                "CAPABILITY_GENERATION_MAX_NEW_TOKENS_INVALID",
            "TEXT_GENERATOR_LOGITS_RANK_INVALID" or
            "TEXT_GENERATOR_LOGITS_BATCH_INVALID" or
            "TEXT_GENERATOR_LOGITS_QUERY_LENGTH_INVALID" or
            "TEXT_GENERATOR_LOGITS_VOCAB_SIZE_INVALID" =>
                "CAPABILITY_GENERATION_FAILED",
            _ => null
        };

    private static bool IsSafeIdentity(string? value)
        => IsSafeText(value, MaximumIdentityLength) &&
           value!.All(character => !char.IsWhiteSpace(character));

    private static bool IsSafeText(string? value, int maximumLength)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= maximumLength &&
           value.IndexOf('\0') < 0 &&
           !value.Any(char.IsControl);

    private static bool IsSafePayload(string? value, int maximumLength)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= maximumLength &&
           value.IndexOf('\0') < 0 &&
           !value.Any(character =>
               char.IsControl(character) &&
               character is not ('\r' or '\n' or '\t'));

    private static bool IsNonFatal(Exception exception)
        => SnapshotExceptionGraphClassifier.IsNonFatal(exception);
}

internal sealed record CapabilityServingAuthorizedBinding(
    string ScenarioId,
    string FlowId,
    string UnitId,
    string UnitRole,
    string DomainTag,
    string Version,
    LogicalUnitTrustState TrustState,
    string PhaseId,
    string SubPhaseId,
    string TaskType,
    string[] DomainTags);

public sealed record CapabilityServingRequest(
    string UserRequest,
    string ScenarioId);

public sealed record CapabilityGenerationRequest(
    string UserRequest,
    string ScenarioId,
    int MaxNewTokens,
    int Seed);

public sealed record CapabilityGenerationResult(
    string GeneratedText,
    string ScenarioId,
    string FlowId,
    string UnitId,
    int Seed,
    int MaxNewTokens,
    string TokenizerSha256,
    string PolicyCheckpointSha256,
    string RuntimeProvenanceSha256,
    string InferenceMode,
    bool CapabilityServingReady,
    bool ProductReady,
    bool PerformsTraining,
    bool DpoReady,
    bool TeacherReady,
    string Boundary);

internal sealed class CapabilityServingGenerationRuntime
{
    internal CapabilityServingGenerationRuntime(
        string scenarioId,
        string flowId,
        string unitId,
        string tokenizerSha256,
        string policyCheckpointSha256,
        string runtimeProvenanceSha256,
        TransformerEmbeddingKnnRuntime runtime,
        TextGenerator generator,
        CapabilityServingGenerationRunner runner)
    {
        ScenarioId = scenarioId;
        FlowId = flowId;
        UnitId = unitId;
        TokenizerSha256 = tokenizerSha256;
        PolicyCheckpointSha256 = policyCheckpointSha256;
        RuntimeProvenanceSha256 = runtimeProvenanceSha256;
        Runtime = runtime;
        Generator = generator;
        Runner = runner;
        Gate = new SemaphoreSlim(1, 1);
    }

    internal string ScenarioId { get; }
    internal string FlowId { get; }
    internal string UnitId { get; }
    internal string TokenizerSha256 { get; }
    internal string PolicyCheckpointSha256 { get; }
    internal string RuntimeProvenanceSha256 { get; }
    internal TransformerEmbeddingKnnRuntime Runtime { get; }
    internal TextGenerator Generator { get; }
    internal CapabilityServingGenerationRunner Runner { get; }
    internal SemaphoreSlim Gate { get; }
}

internal delegate Task<OperationResult<LogicalAssistantRunResult>>
    CapabilityServingAssistantRunner(
        string userRequest,
        string? scenarioId,
        LogicalExecutionFlags? flags,
        IReadOnlyDictionary<string, object>? metadata,
        CancellationToken cancellationToken);

internal delegate string CapabilityServingGenerationRunner(
    TextGenerator generator,
    string prompt,
    int maxNewTokens,
    float temperature,
    int topK,
    float topP,
    int seed,
    CancellationToken cancellationToken);
