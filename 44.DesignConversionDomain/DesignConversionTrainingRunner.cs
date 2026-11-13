// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;

public sealed record DesignConversionTrainingRunResult(
    DesignConversionDpoBuildResult DpoBuild,
    DesignConversionDpoDatasetManifest DatasetManifest,
    DesignConversionDpoDatasetCountGate DatasetCountGate,
    DesignConversionTrainingBoundaryDecision BoundaryDecision,
    DesignConversionTrainingPipelineResult PipelineResult,
    IReadOnlyList<string> Warnings)
{
    [System.Text.Json.Serialization.JsonPropertyName("safeProjectionSchemaVersion")]
    public string SafeProjectionSchemaVersion =>
        DesignConversionSafeTrainingProjection.SchemaVersion;
}

internal sealed record DesignConversionPreparedTrainingRecordExecution(
    DesignConversionDpoBuildResult Build,
    DesignConversionTrainingPipelineOptions Options,
    DesignConversionCandidatePayloadCustodyStore.TrainingCapability?
        CustodyCapability,
    DesignConversionTrainingPipeline.PreparedPipelineExecution Pipeline);

internal sealed record
    DesignConversionDualTargetTeacherExternalConsumptionRunResult(
        DesignConversionTrainingRunResult Run,
        DesignConversionExternalConsumptionCommitEvidence
            ExternalConsumption);

public sealed class DesignConversionTrainingRunner
{
    private readonly DesignConversionDpoGenerator _generator;
    private readonly DesignConversionTrainingPipeline _pipeline;

    public DesignConversionTrainingRunner(
        DesignConversionDpoGenerator? generator = null,
        DesignConversionTrainingPipeline? pipeline = null)
    {
        _generator = generator ?? new DesignConversionDpoGenerator();
        _pipeline = pipeline ?? new DesignConversionTrainingPipeline();
    }

    [Obsolete("Use the legal-teacher-authority overload.")]
    public Task<OperationResult<DesignConversionTrainingRunResult>> RunAsync(
        DesignConversionMachine machine,
        IReadOnlyList<DesignConversionStoredDpoExample> examples,
        DesignConversionTrainingPipelineOptions options,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<DesignConversionTrainingRunResult>.Fail(
                "DesignConversion training runner cancelled.",
                "CANCELLED"));

        return Task.FromResult(LegalTeacherConfigurationRequired());
    }

    public async Task<OperationResult<DesignConversionTrainingRunResult>> RunAsync(
        ServerOwnedLegalTrainingTeacherAdmissionAuthority legalTeacherAdmissionAuthority,
        DesignConversionMachine machine,
        IReadOnlyList<DesignConversionStoredDpoExample> examples,
        DesignConversionTrainingPipelineOptions options,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionTrainingRunResult>.Fail(
                "DesignConversion training runner cancelled.",
                "CANCELLED");

        if (legalTeacherAdmissionAuthority is null)
            return LegalTeacherConfigurationRequired();

        var exampleSnapshot = SnapshotExamples(examples);
        if (!exampleSnapshot.IsSuccess)
            return exampleSnapshot.MapError<DesignConversionTrainingRunResult>();

        var optionsSnapshot = DesignConversionTrainingPipeline.SnapshotOptions(options);
        if (!optionsSnapshot.IsSuccess)
            return optionsSnapshot.MapError<DesignConversionTrainingRunResult>();

        var build = _generator.BuildTrainReadyRecords(exampleSnapshot.Value!);
        if (!build.IsSuccess)
            return build.MapError<DesignConversionTrainingRunResult>();

        var recordSnapshot = DesignConversionTrainingPipeline.SnapshotTrainingRecords(
            build.Value!.AllRecords);
        if (!recordSnapshot.IsSuccess)
            return recordSnapshot.MapError<DesignConversionTrainingRunResult>();

        var pipeline = await _pipeline.RunAsync(
            legalTeacherAdmissionAuthority,
            machine,
            recordSnapshot.Value!,
            optionsSnapshot.Value!,
            ct);
        if (!pipeline.IsSuccess)
            return pipeline.MapError<DesignConversionTrainingRunResult>();

        var pipelineValue = pipeline.Value!;
        var warnings = BuildRunnerWarnings(build.Value.Warnings, pipelineValue);
        var publicBuild = ToPublicSafeBuild(build.Value);
        var publicPipeline = ToPublicSafePipeline(pipelineValue);

        return OperationResult<DesignConversionTrainingRunResult>.Ok(new DesignConversionTrainingRunResult(
            DpoBuild: publicBuild,
            DatasetManifest: pipelineValue.DatasetManifest,
            DatasetCountGate: pipelineValue.DatasetCountGate,
            BoundaryDecision: pipelineValue.BoundaryDecision,
            PipelineResult: publicPipeline,
            Warnings: warnings));
    }

    [Obsolete("Use the mutation-permit overload.")]
    internal Task<OperationResult<DesignConversionTrainingRunResult>> RunTrainingRecordsAsync(
        DesignConversionMachine machine,
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        DesignConversionTrainingPipelineOptions options,
        CancellationToken ct = default,
        IReadOnlyList<DesignConversionCandidatePayloadCustodyStore.CommittedEvidence>?
            committedCustodyEvidence = null)
        => Task.FromResult(ct.IsCancellationRequested
            ? OperationResult<DesignConversionTrainingRunResult>.Fail(
                "DesignConversion training runner cancelled.",
                "CANCELLED")
            : LegalTeacherConfigurationRequired());

    [Obsolete("Use typed prepared execution with exact-content admission.")]
    internal Task<OperationResult<DesignConversionTrainingRunResult>> RunTrainingRecordsAsync(
        DesignConversionMachine machine,
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        DesignConversionTrainingPipelineOptions options,
        Func<OperationResult<IDisposable>> acquireMutationPermit,
        CancellationToken ct = default,
        IReadOnlyList<DesignConversionCandidatePayloadCustodyStore.CommittedEvidence>?
            committedCustodyEvidence = null)
        => Task.FromResult(ct.IsCancellationRequested
            ? OperationResult<DesignConversionTrainingRunResult>.Fail(
                "DesignConversion training runner cancelled.",
                "CANCELLED")
            : LegalTeacherConfigurationRequired());

    internal OperationResult<DesignConversionPreparedTrainingRecordExecution>
        PrepareTrainingRecords(
            IReadOnlyList<DesignConversionDpoTrainingRecord> records,
            DesignConversionTrainingPipelineOptions options,
            IReadOnlyList<DesignConversionCandidatePayloadCustodyStore.CommittedEvidence>?
                committedCustodyEvidence = null)
    {
        if (records is null)
        {
            return OperationResult<
                DesignConversionPreparedTrainingRecordExecution>.Fail(
                "DesignConversion DPO training records are required.",
                "DESIGN_CONVERSION_DPO_TRAINING_RECORDS_REQUIRED");
        }
        var optionsSnapshot = DesignConversionTrainingPipeline.SnapshotOptions(
            options);
        if (!optionsSnapshot.IsSuccess || optionsSnapshot.Value is null)
        {
            return optionsSnapshot.MapError<
                DesignConversionPreparedTrainingRecordExecution>();
        }

        var accepted = records
            .Where(DesignConversionTrainingMeasurementGatePolicy.IsRecordAllowedForTrainingCandidate)
            .OrderByDescending(record => record.UsefulnessScore)
            .ThenBy(record => record.RecordId, StringComparer.Ordinal)
            .ToArray();
        var blocked = records
            .Where(record => !DesignConversionTrainingMeasurementGatePolicy.IsRecordAllowedForTrainingCandidate(record))
            .OrderBy(record => record.RecordId, StringComparer.Ordinal)
            .ToArray();
        var protectedAccepted = accepted
            .Where(DesignConversionTrainingPipeline.IsCustodyProtectedRecord)
            .ToArray();
        if (protectedAccepted.Any(record =>
                !DesignConversionCandidatePayloadCustodyStore
                    .IsExactStoreBackedRecord(record, committedCustodyEvidence)))
        {
            return OperationResult<
                DesignConversionPreparedTrainingRecordExecution>.Fail(
                "DesignConversion accepted teacher records require exact store-backed payload custody.",
                "DESIGN_CONVERSION_TRAINING_CUSTODY_AUTHORITY_REQUIRED");
        }
        var build = new DesignConversionDpoBuildResult(
            AcceptedRecords: accepted,
            QuarantinedRecords: blocked,
            Warnings: BuildResolvedRecordWarnings(
                accepted,
                committedCustodyEvidence));

        DesignConversionCandidatePayloadCustodyStore.TrainingCapability?
            custodyCapability = null;
        if ((committedCustodyEvidence?.Count ?? 0) > 0)
        {
            var capability = DesignConversionCandidatePayloadCustodyStore
                .MintTrainingCapability(committedCustodyEvidence);
            if (!capability.IsSuccess)
            {
                return capability.MapError<
                    DesignConversionPreparedTrainingRecordExecution>();
            }
            custodyCapability = capability.Value;
        }

        var pipeline = _pipeline.PrepareExecution(
            build.AllRecords,
            optionsSnapshot.Value);
        if (!pipeline.IsSuccess || pipeline.Value is null)
        {
            return pipeline.MapError<
                DesignConversionPreparedTrainingRecordExecution>();
        }
        return OperationResult<
            DesignConversionPreparedTrainingRecordExecution>.Ok(
            new DesignConversionPreparedTrainingRecordExecution(
                build,
                optionsSnapshot.Value,
                custodyCapability,
                pipeline.Value));
    }

    internal OperationResult<DesignConversionPreparedTrainingRecordExecution>
        PrepareAdmittedCorpus(DesignConversionDpoAdmittedCorpus corpus)
    {
        if (corpus is null)
        {
            return OperationResult<
                DesignConversionPreparedTrainingRecordExecution>.Fail(
                    "A live exact DesignConversion corpus admission is required.",
                    DesignConversionDpoCorpusAdmissionAuthority.InvalidErrorCode);
        }

        var pipeline = _pipeline.PrepareAdmittedCorpusExecution(corpus);
        if (!pipeline.IsSuccess || pipeline.Value is null)
        {
            return pipeline.MapError<
                DesignConversionPreparedTrainingRecordExecution>();
        }

        var build = new DesignConversionDpoBuildResult(
            AcceptedRecords: corpus.Records,
            QuarantinedRecords: [],
            Warnings:
            [
                "DESIGN_CONVERSION_SERVER_OWNED_CORPUS_ADMISSION",
                "SOURCE_RECORDS_REMAIN_REVIEW_ONLY_NOT_TRAINING_GRADE"
            ]);
        return OperationResult<
            DesignConversionPreparedTrainingRecordExecution>.Ok(
                new DesignConversionPreparedTrainingRecordExecution(
                    build,
                    corpus.Options,
                    null,
                    pipeline.Value));
    }

    internal async Task<OperationResult<DesignConversionTrainingRunResult>>
        RunPreparedTrainingRecordsAsync(
            DesignConversionMachine machine,
            DesignConversionPipelineTrainingBinding? trainingBinding,
            DesignConversionPreparedTrainingRecordExecution prepared,
            CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            return OperationResult<DesignConversionTrainingRunResult>.Fail(
                "DesignConversion training runner cancelled.",
                "CANCELLED");
        }
        if (prepared is null ||
            prepared.Pipeline.FinalOptimizer is null &&
            trainingBinding is not null ||
            prepared.Pipeline.FinalOptimizer is not null &&
            (trainingBinding is null ||
             !trainingBinding.MatchesExactPreparedFinal(
                 prepared.Pipeline.FinalOptimizer)))
            return LegalTeacherConfigurationRequired();

        var pipeline = await _pipeline.RunValidatedAsync(
            machine,
            prepared.Build.AllRecords,
            prepared.Options,
            prepared.CustodyCapability,
            trainingBinding,
            prepared.Pipeline,
            ct);
        if (!pipeline.IsSuccess)
            return pipeline.MapError<DesignConversionTrainingRunResult>();

        var pipelineValue = pipeline.Value!;
        var warnings = BuildRunnerWarnings(prepared.Build.Warnings, pipelineValue);
        var publicBuild = ToPublicSafeBuild(prepared.Build);
        var publicPipeline = ToPublicSafePipeline(pipelineValue);

        return OperationResult<DesignConversionTrainingRunResult>.Ok(new DesignConversionTrainingRunResult(
            DpoBuild: publicBuild,
            DatasetManifest: pipelineValue.DatasetManifest,
            DatasetCountGate: pipelineValue.DatasetCountGate,
            BoundaryDecision: pipelineValue.BoundaryDecision,
            PipelineResult: publicPipeline,
            Warnings: warnings));
    }

    internal async Task<OperationResult<DesignConversionTrainingRunResult>>
        RunPreparedCommittedDualTargetTeacherLocalContinuationAsync(
            DesignConversionMachine machine,
            DesignConversionPipelineTrainingBinding? trainingBinding,
            DesignConversionPreparedTrainingRecordExecution prepared,
            DesignConversionExternalConsumptionCommitEvidence
                committedExternalConsumption,
            CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            return OperationResult<DesignConversionTrainingRunResult>.Fail(
                "DesignConversion training runner cancelled.",
                "CANCELLED");
        }

        if (machine is null ||
            prepared is null ||
            prepared.Pipeline.FinalOptimizer is null ||
            trainingBinding is null ||
            committedExternalConsumption is null ||
            !trainingBinding.MatchesExactPreparedFinal(
                prepared.Pipeline.FinalOptimizer))
        {
            return LegalTeacherConfigurationRequired();
        }

        var pipeline = await _pipeline
            .RunCommittedDualTargetTeacherLocalContinuationValidatedAsync(
                machine,
                prepared.Build.AllRecords,
                prepared.Options,
                prepared.CustodyCapability,
                trainingBinding,
                prepared.Pipeline,
                committedExternalConsumption,
                ct);
        if (!pipeline.IsSuccess)
            return pipeline.MapError<DesignConversionTrainingRunResult>();

        var pipelineValue = pipeline.Value!;
        var warnings = BuildRunnerWarnings(prepared.Build.Warnings, pipelineValue);
        var publicBuild = ToPublicSafeBuild(prepared.Build);
        var publicPipeline = ToPublicSafePipeline(pipelineValue);

        return OperationResult<DesignConversionTrainingRunResult>.Ok(
            new DesignConversionTrainingRunResult(
                DpoBuild: publicBuild,
                DatasetManifest: pipelineValue.DatasetManifest,
                DatasetCountGate: pipelineValue.DatasetCountGate,
                BoundaryDecision: pipelineValue.BoundaryDecision,
                PipelineResult: publicPipeline,
                Warnings: warnings));
    }

    internal async Task<OperationResult<
        DesignConversionDualTargetTeacherExternalConsumptionRunResult>>
        RunPreparedDualTargetTeacherExternalConsumptionAsync(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                legalTeacherAdmissionAuthority,
            DesignConversionMachine machine,
            DesignConversionPreparedTrainingRecordExecution prepared,
            FileDpoPairStore.ExternalConsumptionTransactionHandle
                externalTransaction,
            CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            return OperationResult<
                DesignConversionDualTargetTeacherExternalConsumptionRunResult>.Fail(
                    "DesignConversion training runner cancelled.",
                    "CANCELLED");
        }
        if (legalTeacherAdmissionAuthority is null || machine is null ||
            prepared is null || prepared.Pipeline.FinalOptimizer is null ||
            externalTransaction is null)
        {
            return OperationResult<
                DesignConversionDualTargetTeacherExternalConsumptionRunResult>.Fail(
                    "Legal teacher admission authority and exact prepared training content are required.",
                    LegalTrainingTeacherAdmissionAuthority
                        .TrainingContentInvalidErrorCode);
        }

        var pipeline = await _pipeline
            .RunDualTargetTeacherExternalConsumptionValidatedAsync(
                legalTeacherAdmissionAuthority,
                machine,
                prepared.Build.AllRecords,
                prepared.Options,
                prepared.CustodyCapability,
                prepared.Pipeline,
                externalTransaction,
                ct);
        if (!pipeline.IsSuccess || pipeline.Value is null)
        {
            return pipeline.MapError<
                DesignConversionDualTargetTeacherExternalConsumptionRunResult>();
        }

        var pipelineValue = pipeline.Value.Pipeline;
        var run = new DesignConversionTrainingRunResult(
            DpoBuild: ToPublicSafeBuild(prepared.Build),
            DatasetManifest: pipelineValue.DatasetManifest,
            DatasetCountGate: pipelineValue.DatasetCountGate,
            BoundaryDecision: pipelineValue.BoundaryDecision,
            PipelineResult: ToPublicSafePipeline(pipelineValue),
            Warnings: BuildRunnerWarnings(
                prepared.Build.Warnings,
                pipelineValue));
        return OperationResult<
            DesignConversionDualTargetTeacherExternalConsumptionRunResult>.Ok(
                new(run, pipeline.Value.ExternalConsumption));
    }

    internal async Task<OperationResult<
        DesignConversionDualTargetTeacherExternalConsumptionRunResult>>
        RunPreparedAdmittedCorpusExternalConsumptionAsync(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                legalTeacherAdmissionAuthority,
            DesignConversionMachine machine,
            DesignConversionDpoAdmittedCorpus corpus,
            DesignConversionPreparedTrainingRecordExecution prepared,
            FileDpoPairStore.ExternalConsumptionTransactionHandle
                externalTransaction,
            CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            return OperationResult<
                DesignConversionDualTargetTeacherExternalConsumptionRunResult>.Fail(
                    "DesignConversion training runner cancelled.",
                    "CANCELLED");
        }
        if (legalTeacherAdmissionAuthority is null || machine is null ||
            corpus is null || prepared is null ||
            prepared.Pipeline.FinalOptimizer is null ||
            externalTransaction is null)
        {
            return OperationResult<
                DesignConversionDualTargetTeacherExternalConsumptionRunResult>.Fail(
                    "Legal teacher admission, corpus admission, and exact external training content are required.",
                    LegalTrainingTeacherAdmissionAuthority
                        .TrainingContentInvalidErrorCode);
        }

        var pipeline = await _pipeline
            .RunAdmittedCorpusExternalConsumptionValidatedAsync(
                legalTeacherAdmissionAuthority,
                machine,
                corpus,
                prepared.Pipeline,
                externalTransaction,
                ct);
        if (!pipeline.IsSuccess || pipeline.Value is null)
        {
            return pipeline.MapError<
                DesignConversionDualTargetTeacherExternalConsumptionRunResult>();
        }

        var build = new DesignConversionDpoBuildResult(
            AcceptedRecords: corpus.Admission.Originals,
            QuarantinedRecords: [],
            Warnings:
            [
                "DESIGN_CONVERSION_SERVER_OWNED_CORPUS_ADMISSION",
                "SOURCE_RECORDS_REMAIN_REVIEW_ONLY_NOT_TRAINING_GRADE"
            ]);
        var pipelineValue = pipeline.Value.Pipeline;
        var run = new DesignConversionTrainingRunResult(
            DpoBuild: ToPublicSafeBuild(build),
            DatasetManifest: pipelineValue.DatasetManifest,
            DatasetCountGate: pipelineValue.DatasetCountGate,
            BoundaryDecision: pipelineValue.BoundaryDecision,
            PipelineResult: ToPublicSafePipeline(pipelineValue),
            Warnings: BuildRunnerWarnings(build.Warnings, pipelineValue));
        return OperationResult<
            DesignConversionDualTargetTeacherExternalConsumptionRunResult>.Ok(
                new(run, pipeline.Value.ExternalConsumption));
    }

    internal static IReadOnlyList<string> BuildResolvedRecordWarnings(
        IReadOnlyList<DesignConversionDpoTrainingRecord> accepted,
        IReadOnlyList<DesignConversionCandidatePayloadCustodyStore.CommittedEvidence>?
            committedCustodyEvidence = null)
    {
        var verifiedCount = (accepted ?? [])
            .Count(record => HasVerifiedPayloadCustody(
                record,
                committedCustodyEvidence ?? []));
        var unverifiedCount = (accepted?.Count ?? 0) - verifiedCount;
        var warnings = new List<string>
        {
            "DESIGN_CONVERSION_DPO_RECORDS_RESOLVED_FROM_APPROVED_TRAINING_INTAKE",
            $"DESIGN_CONVERSION_DPO_RECORDS_WITH_VERIFIED_PAYLOAD_CUSTODY_COUNT={verifiedCount}",
            $"DESIGN_CONVERSION_DPO_RECORDS_WITHOUT_VERIFIED_PAYLOAD_CUSTODY_COUNT={unverifiedCount}",
            "DETERMINISTIC_QA_EVIDENCE_IS_NOT_TREATED_AS_LEARNING"
        };

        if (verifiedCount > 0 && unverifiedCount == 0)
        {
            warnings.Add(
                "DESIGN_CONVERSION_DPO_RECORDS_HAVE_VERIFIED_PAYLOAD_CUSTODY_BOUNDARY");
        }

        if (unverifiedCount > 0)
        {
            warnings.Add(
                "DESIGN_CONVERSION_DPO_RECORDS_ARE_TRAIN_READY_SCAFFOLD_INPUTS_ONLY");
        }

        return warnings.ToArray();
    }

    private static bool HasVerifiedPayloadCustody(
        DesignConversionDpoTrainingRecord record,
        IReadOnlyList<DesignConversionCandidatePayloadCustodyStore.CommittedEvidence> evidence)
        => DesignConversionCandidatePayloadCustodyStore
            .IsExactStoreBackedRecord(record, evidence);

    private static IReadOnlyList<string> BuildRunnerWarnings(
        IReadOnlyList<string> buildWarnings,
        DesignConversionTrainingPipelineResult pipeline)
    {
        var warnings = new List<string>();
        warnings.AddRange(buildWarnings ?? []);
        warnings.AddRange(pipeline.Warnings ?? []);
        warnings.Add(pipeline.LifecycleEvidence.CheckpointContainsRealTrainedWeights
            ? "DESIGN_CONVERSION_TRAINING_RUNNER_LOCAL_LEARNED_WEIGHTS_PRODUCED"
            : "DESIGN_CONVERSION_TRAINING_RUNNER_DID_NOT_PRODUCE_REAL_LEARNED_WEIGHTS");
        warnings.Add(pipeline.LifecycleEvidence.CheckpointCandidateCreated
            ? "DESIGN_CONVERSION_TRAINING_RUNNER_CREATED_LOCAL_CHECKPOINT_CANDIDATE"
            : "DESIGN_CONVERSION_TRAINING_RUNNER_DID_NOT_CREATE_CHECKPOINT");
        warnings.Add("DESIGN_CONVERSION_TRAINING_RUNNER_DID_NOT_PERFORM_EXTERNAL_MODEL_SERVER_TRAINING");
        warnings.Add("DESIGN_CONVERSION_TRAINING_RUNNER_DID_NOT_PROMOTE_MODEL");

        return warnings
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    internal static DesignConversionDpoBuildResult ToPublicSafeBuild(
        DesignConversionDpoBuildResult build)
        => new DesignConversionDpoBuildResult(
            build.AcceptedRecords.Select(ToPublicSafeRecord).ToArray(),
            build.QuarantinedRecords.Select(ToPublicSafeRecord).ToArray(),
            build.Warnings)
        {
            AcceptedSummaryOverride = build.AcceptedRecords
                .Select(DesignConversionSafeTrainingProjection.ToSummary)
                .ToArray(),
            QuarantinedSummaryOverride = build.QuarantinedRecords
                .Select(DesignConversionSafeTrainingProjection.ToSummary)
                .ToArray()
        };

    private static DesignConversionTrainingPipelineResult ToPublicSafePipeline(
        DesignConversionTrainingPipelineResult pipeline)
        => pipeline with
        {
            Batches = pipeline.Batches.Select(batch => batch with
            {
                Records = batch.Records.Select(ToPublicSafeRecord).ToArray(),
                TrainingItems = batch.TrainingItems.Select(ToPublicSafeItem).ToArray(),
                RecordSummaryOverride = batch.Records
                    .Select(DesignConversionSafeTrainingProjection.ToSummary)
                    .ToArray(),
                TrainingItemSummaryOverride = batch.TrainingItems
                    .Select(DesignConversionSafeTrainingProjection.ToSummary)
                    .ToArray()
            }).ToArray(),
            TrainingItems = pipeline.TrainingItems.Select(ToPublicSafeItem).ToArray(),
            TrainingItemSummaryOverride = pipeline.TrainingItems
                .Select(DesignConversionSafeTrainingProjection.ToSummary)
                .ToArray()
        };

    private static DesignConversionDpoTrainingRecord ToPublicSafeRecord(
        DesignConversionDpoTrainingRecord record)
        => record with
        {
            InternalUnitStatePayload = DesignConversionDpoGenerator.Sha256Ref(
                record.InternalUnitStatePayload ?? string.Empty),
            InternalVectorPayload = [],
            SourceStructurePayload = DesignConversionDpoGenerator.Sha256Ref(
                record.SourceStructurePayload ?? string.Empty),
            Chosen = ToPublicSafeOutput(record.Chosen),
            Rejected = ToPublicSafeOutput(record.Rejected),
            TieBreakerPayload = string.IsNullOrEmpty(record.TieBreakerPayload)
                ? string.Empty
                : DesignConversionDpoGenerator.Sha256Ref(record.TieBreakerPayload),
            PayloadCustodySecret = null
        };

    private static DesignConversionImprovementOutput ToPublicSafeOutput(
        DesignConversionImprovementOutput output)
        => output with
        {
            Html = DesignConversionDpoGenerator.Sha256Ref(output.Html ?? string.Empty),
            Css = DesignConversionDpoGenerator.Sha256Ref(output.Css ?? string.Empty)
        };

    private static AgentInfrastructure.Logical.LearningTrainingItem ToPublicSafeItem(
        AgentInfrastructure.Logical.LearningTrainingItem item)
        => item with
        {
            Prompt = DesignConversionDpoGenerator.Sha256Ref(item.Prompt),
            ChosenResponse = DesignConversionDpoGenerator.Sha256Ref(item.ChosenResponse),
            RejectedResponse = DesignConversionDpoGenerator.Sha256Ref(item.RejectedResponse)
        };

    private static OperationResult<DesignConversionStoredDpoExample[]> SnapshotExamples(
        IEnumerable<DesignConversionStoredDpoExample>? examples)
        => DesignConversionDpoTrainingContentAuthority.CaptureExamples(
            examples);

    private static OperationResult<DesignConversionTrainingRunResult>
        LegalTeacherConfigurationRequired()
        => OperationResult<DesignConversionTrainingRunResult>.Fail(
            "Server-owned legal teacher admission configuration is required.",
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode);
}
