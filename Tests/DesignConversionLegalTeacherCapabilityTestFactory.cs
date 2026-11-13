// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;

internal static class DesignConversionLegalTeacherCapabilityTestFactory
{
    internal const string DefaultModelId = "qwen3.5:9b";
    internal const string DefaultExpectedOllamaManifestDigest =
        "dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd";

    internal static ServerOwnedLegalTrainingTeacherAdmissionAuthority CreateAdmitted(
        int maxTeacherCalls = 100,
        string modelId = DefaultModelId,
        string? expectedOllamaManifestDigest = null,
        string? expectedOllamaModelLayerDigest = null)
    {
        var configuration = CreateTeacherConfiguration(
            maxTeacherCalls,
            modelId,
            expectedOllamaManifestDigest,
            expectedOllamaModelLayerDigest);
        return ServerOwnedLegalTrainingTeacherAdmissionAuthority
            .IssueFromAdmittedConfiguration(configuration, static () => true);
    }

    internal static ServerOwnedLegalTrainingTeacherAdmissionAuthority
        CreateAdmittedWithPermitObserver(
            IServerOwnedLegalTrainingTeacherAdmissionLeaseObserverForTesting
                concurrencyObserverForTesting,
            int maxTeacherCalls = 100,
            string modelId = DefaultModelId,
            string? expectedOllamaManifestDigest = null,
            string? expectedOllamaModelLayerDigest = null)
    {
        ArgumentNullException.ThrowIfNull(concurrencyObserverForTesting);
        var configuration = CreateTeacherConfiguration(
            maxTeacherCalls,
            modelId,
            expectedOllamaManifestDigest,
            expectedOllamaModelLayerDigest);
        var authority = configuration.CreateTeacherPolicyBudgetAuthority();
        if (!authority.IsSuccess || authority.Value is null)
        {
            throw new InvalidOperationException(
                $"Test legal-teacher configuration admission failed: {authority.ErrorCode}");
        }

        return ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
            authority.Value.ValidateBatch,
            static () => true,
            concurrencyObserverForTesting,
            authority.Value.SealLogicalCapabilityReplay,
            authority.Value.VerifyLogicalCapabilityReplay,
            authority.Value.SealLogicalCapabilityExecutableManifest,
            authority.Value.VerifyLogicalCapabilityExecutableManifest,
            authority.Value.ValidateBatch);
    }

    internal static RevocableAdmissionHarness CreateRevocable(
        int maxTeacherCalls = 100,
        string modelId = DefaultModelId,
        string? expectedOllamaManifestDigest = null,
        string? expectedOllamaModelLayerDigest = null)
        => new(CreateTeacherConfiguration(
            maxTeacherCalls,
            modelId,
            expectedOllamaManifestDigest,
            expectedOllamaModelLayerDigest));

    internal static Task<OperationResult<DesignConversionTrainingPipelineResult>>
        RunStrictPipelineAsync(
            DesignConversionTrainingPipeline pipeline,
            DesignConversionMachine machine,
            IReadOnlyList<DesignConversionDpoTrainingRecord> records,
            DesignConversionTrainingPipelineOptions options,
            CancellationToken ct = default)
        => pipeline.RunAsync(
            CreateAdmitted(),
            machine,
            records,
            options,
            ct);

    internal static Task<OperationResult<LearningBatchResult>>
        TrainMachineStrictAsync(
            DesignConversionMachine machine,
            LearningBatch<DesignConversionDomain.DesignConversionDomain> batch,
            CancellationToken ct = default)
        => DesignConversionMachineTrainingAuthority.ExecuteExactAsync(
            CreateAdmitted(),
            machine,
            batch,
            batch.TrainingItems
                .Select((_, index) =>
                    (TrainingTeacherProvenance?)
                    TrainingTeacherProvenance.Deterministic(
                        string.Concat(
                            "test:design-conversion-machine:",
                            batch.BatchId,
                            ":",
                            index.ToString(
                                System.Globalization.CultureInfo.InvariantCulture))))
                .ToArray(),
            ct);

    internal static async Task<OperationResult<
        DesignConversionLocalContinueTrainingProbeReport>>
        ContinueTrainingStrictAsync(
            DesignConversionLocalFreshLoadProbeReport freshLoadProbe,
            DesignConversionTrainingBatch continuationBatch,
            IReadOnlyCollection<string> heldOutRecordIdentityHashes,
            string continuedCheckpointPath,
            bool datasetCountGatePassed,
            CancellationToken ct = default)
    {
        var machineBatch = new LearningBatch<
            DesignConversionDomain.DesignConversionDomain>(
                continuationBatch.BatchId,
                DesignConversionMachine.DomainTag,
                continuationBatch.TrainingItems,
                continuationBatch.SourceIds);
        var provenance = machineBatch.TrainingItems
            .Select((_, index) =>
                (TrainingTeacherProvenance?)
                TrainingTeacherProvenance.Deterministic(
                    string.Concat(
                        "test:design-conversion-local-continue:",
                        machineBatch.BatchId,
                        ":",
                        index.ToString(
                            System.Globalization.CultureInfo.InvariantCulture))))
            .ToArray();
        var finalOptimizer = DesignConversionDpoTrainingContentAuthority
            .ProjectFinalOptimizerInputs(
                machineBatch.TrainingItems,
                provenance,
                static item => item.Prompt,
                static item => item.ChosenResponse,
                static item => item.RejectedResponse,
                static _ => null,
                static _ => null,
                static item => string.Concat(
                    "test:design-conversion-local-continue|item=",
                    item.ItemId));
        if (!finalOptimizer.IsSuccess || finalOptimizer.Value is null)
        {
            return finalOptimizer.MapError<
                DesignConversionLocalContinueTrainingProbeReport>();
        }

        var leaseResult = CreateAdmitted().AcquireTrainingLease(
            finalOptimizer.Value.Content,
            finalOptimizer.Value.Provenance);
        if (!leaseResult.IsSuccess || leaseResult.Value is null)
        {
            return leaseResult.MapError<
                DesignConversionLocalContinueTrainingProbeReport>();
        }

        using var lease = leaseResult.Value;
        var admission = DesignConversionMachineTrainingAdmission.Mint(
            finalOptimizer.Value,
            machineBatch,
            lease);
        if (!admission.IsSuccess || admission.Value is null)
        {
            return admission.MapError<
                DesignConversionLocalContinueTrainingProbeReport>();
        }

        return await DesignConversionLocalContinueTrainingProbe
            .VerifyFreshLoadedCheckpointAdmittedAsync(
                freshLoadProbe,
                continuationBatch,
                heldOutRecordIdentityHashes,
                continuedCheckpointPath,
                datasetCountGatePassed,
                admission.Value,
                ct);
    }

    internal static async Task<OperationResult<DesignConversionTrainingRunResult>>
        RunStrictRunnerAsync(
            DesignConversionTrainingRunner runner,
            DesignConversionMachine machine,
            IReadOnlyList<DesignConversionDpoTrainingRecord> records,
            DesignConversionTrainingPipelineOptions options,
            CancellationToken ct = default)
    {
        var prepared = runner.PrepareTrainingRecords(records, options);
        if (!prepared.IsSuccess || prepared.Value is null)
            return prepared.MapError<DesignConversionTrainingRunResult>();

        ServerOwnedLegalTrainingTeacherAdmissionLease? lease = null;
        DesignConversionPipelineTrainingBinding? trainingBinding = null;
        try
        {
            if (prepared.Value.Pipeline.FinalOptimizer is { } finalOptimizer)
            {
                var admitted = CreateAdmitted().AcquireTrainingLease(
                    finalOptimizer.Content,
                    finalOptimizer.Provenance);
                if (!admitted.IsSuccess || admitted.Value is null)
                    return admitted.MapError<DesignConversionTrainingRunResult>();
                lease = admitted.Value;
                var binding = DesignConversionPipelineTrainingBinding
                    .MintExact(finalOptimizer, lease);
                if (!binding.IsSuccess || binding.Value is null)
                    return binding.MapError<DesignConversionTrainingRunResult>();
                trainingBinding = binding.Value;
            }

            return await runner.RunPreparedTrainingRecordsAsync(
                machine,
                trainingBinding,
                prepared.Value,
                ct);
        }
        finally
        {
            lease?.Dispose();
        }
    }

    private static ContinuousLearningTp1DpoConfiguration CreateTeacherConfiguration(
        int maxTeacherCalls,
        string modelId,
        string? expectedOllamaManifestDigest,
        string? expectedOllamaModelLayerDigest)
    {
        var configuration = ContinuousLearningTp1DpoConfiguration.Create(
            modelId,
            expectedOllamaManifestDigest ?? DefaultExpectedOllamaManifestDigest,
            expectedOllamaModelLayerDigest ?? new string('c', 64),
            maxTeacherCalls,
            $"sha256:{new string('a', 64)}",
            $"sha256:{new string('b', 64)}",
            Enumerable.Range(1, 32)
                .Select(static value => checked((byte)value))
                .ToArray());
        if (!configuration.IsSuccess || configuration.Value is null)
        {
            throw new InvalidOperationException(
                $"Test legal-teacher configuration admission failed: {configuration.ErrorCode}");
        }

        return configuration.Value;
    }

    internal sealed class RevocableAdmissionHarness
    {
        private int _isLive = 1;

        internal RevocableAdmissionHarness(
            ContinuousLearningTp1DpoConfiguration configuration)
            => Authority = ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .IssueFromAdmittedConfiguration(
                    configuration,
                    () => Volatile.Read(ref _isLive) != 0);

        internal ServerOwnedLegalTrainingTeacherAdmissionAuthority Authority { get; }

        internal void LoseAdmissionLive()
            => Volatile.Write(ref _isLive, 0);

        internal Task BeginRevocationAsync()
            => Authority.BeginRevocation();
    }

    internal sealed class PermitCountingObserverForTesting(
        Action<string>? eventSink = null)
        : IServerOwnedLegalTrainingTeacherAdmissionLeaseObserverForTesting
    {
        private readonly object _gate = new();
        private readonly List<int> _activePermitCountTransitions = [];
        private int _grantCount;
        private int _leaseDisposeAttemptCount;

        internal int GrantCount => Volatile.Read(ref _grantCount);

        internal int LeaseDisposeAttemptCount =>
            Volatile.Read(ref _leaseDisposeAttemptCount);

        internal IReadOnlyList<int> ActivePermitCountTransitions
        {
            get
            {
                lock (_gate)
                    return _activePermitCountTransitions.ToArray();
            }
        }

        public void OnFinalMutationPermitGrantUnderGateForTesting()
        {
            Interlocked.Increment(ref _grantCount);
            eventSink?.Invoke("permit:grant");
        }

        public void OnMutationPermitActiveCountChangedForTesting(
            int activePermitCount)
        {
            lock (_gate)
                _activePermitCountTransitions.Add(activePermitCount);
            eventSink?.Invoke($"permit:active:{activePermitCount}");
        }

        public void OnLeaseDisposeAttemptedBeforeGateForTesting()
        {
            Interlocked.Increment(ref _leaseDisposeAttemptCount);
            eventSink?.Invoke("permit:lease_dispose_attempt");
        }
    }

}
