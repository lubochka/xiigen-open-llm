// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure.Logical;
using DesignConversionDomain;

[Collection(RepoArtifactSnapshotCollectionNames.CollectionName)]
public sealed class DesignConversionTrainingPipelineTests
{
    [Fact]
    public async Task RunAsync_GroupsAcceptedPairsIntoUnitBatchesWithoutLifecycleWhenCountGateBypassed()
    {
        var records = BuildRecords([
            DesignConversionDpoGeneratorTests.CreateExample("critic-1", DesignConversionDpoUnits.CriticUnit, usefulness: 0.95),
            DesignConversionDpoGeneratorTests.CreateExample("critic-2", DesignConversionDpoUnits.CriticUnit, usefulness: 0.80),
            DesignConversionDpoGeneratorTests.CreateExample("critic-3", DesignConversionDpoUnits.CriticUnit, usefulness: 0.70),
            DesignConversionDpoGeneratorTests.CreateExample("improver-1", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.90),
            DesignConversionDpoGeneratorTests.CreateExample("improver-2", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.60)
        ]);
        var pipeline = new DesignConversionTrainingPipeline();

        var result = await pipeline.RunAsync(
            new DesignConversionMachine(),
            records,
            new DesignConversionTrainingPipelineOptions(
                "slice-4",
                BatchSize: 2,
                RequireDatasetCountGate: false,
                DatasetOptions: LocalDatasetOptions()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(5, result.Value!.AcceptedRecordCount);
        Assert.Equal(0, result.Value.QuarantinedRecordCount);
        Assert.Equal(DesignConversionTrainingBoundaryDecisionKind.CountGateBypassedForScaffold,
            result.Value.BoundaryDecision.Kind);
        Assert.False(result.Value.BoundaryDecision.AllowsMachineLifecycleHook);
        Assert.Equal(3, result.Value.Batches.Count);
        Assert.Equal([2, 1, 2], result.Value.Batches.Select(batch => batch.Records.Count).ToArray());
        Assert.Empty(result.Value.BatchResults);
        Assert.Equal(5, result.Value.TrainingItems.Count);
        Assert.False(result.Value.MachineLifecycleHookAttempted);
        Assert.False(result.Value.LifecycleEvidence.MachineLifecycleHookCalled);
        Assert.Equal("not_proven", result.Value.LifecycleEvidence.CheckpointCandidateStatus);
        Assert.Contains("DESIGN_CONVERSION_COUNT_GATE_BYPASS_DOES_NOT_ALLOW_MACHINE_LIFECYCLE_HOOK",
            result.Value.BoundaryDecision.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_TRAINING_PIPELINE_SCAFFOLD_NO_REAL_MODEL_MATH", result.Value.Warnings);
    }

    [Fact]
    public async Task RunAsync_ExcludesQuarantinedRecordsFromTrainingItems()
    {
        var valid = BuildRecords([DesignConversionDpoGeneratorTests.CreateExample("valid", DesignConversionDpoUnits.CriticUnit)]).Single();
        var quarantined = valid with
        {
            RecordId = "quarantined",
            Quarantine = new DesignConversionQuarantineState(
                IsQuarantined: true,
                Reason: "test_quarantine",
                EvidenceRefs: ["test"])
        };
        var pipeline = new DesignConversionTrainingPipeline();

        var result = await pipeline.RunAsync(
            new DesignConversionMachine(),
            [valid, quarantined],
            new DesignConversionTrainingPipelineOptions(
                "slice-4-quarantine",
                BatchSize: 4,
                RequireDatasetCountGate: false,
                DatasetOptions: LocalDatasetOptions()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, result.Value!.AcceptedRecordCount);
        Assert.Equal(1, result.Value.QuarantinedRecordCount);
        Assert.Single(result.Value.TrainingItems);
        Assert.Equal("valid", result.Value.TrainingItems[0].ItemId);
        Assert.Empty(result.Value.BatchResults);
        Assert.False(result.Value.MachineLifecycleHookAttempted);
        Assert.Contains("checkpoint_candidate", result.Value.LifecycleEvidence.NotProvenFields);
    }

    [Fact]
    public async Task RunTrainingRecordsAsync_ExcludesPendingStatusTrainingGradeAndNegativeMetadata()
    {
        var records = BuildRecords(
        [
            DesignConversionDpoGeneratorTests.CreateExample("metadata-allowed", DesignConversionDpoUnits.CriticUnit),
            DesignConversionDpoGeneratorTests.CreateExample("metadata-status-pending", DesignConversionDpoUnits.CriticUnit),
            DesignConversionDpoGeneratorTests.CreateExample("metadata-status-pending-not-training-grade", DesignConversionDpoUnits.CriticUnit),
            DesignConversionDpoGeneratorTests.CreateExample("metadata-status-pending-negative-pair-not-proven", DesignConversionDpoUnits.CriticUnit),
            DesignConversionDpoGeneratorTests.CreateExample("metadata-pending", DesignConversionDpoUnits.CriticUnit),
            DesignConversionDpoGeneratorTests.CreateExample("metadata-approved-false", DesignConversionDpoUnits.CriticUnit),
            DesignConversionDpoGeneratorTests.CreateExample("metadata-not-training-grade", DesignConversionDpoUnits.CriticUnit),
            DesignConversionDpoGeneratorTests.CreateExample("metadata-training-grade-false", DesignConversionDpoUnits.CriticUnit),
            DesignConversionDpoGeneratorTests.CreateExample("metadata-synthetic-negative", DesignConversionDpoUnits.CriticUnit),
            DesignConversionDpoGeneratorTests.CreateExample("metadata-scaffold-negative", DesignConversionDpoUnits.CriticUnit),
            DesignConversionDpoGeneratorTests.CreateExample("metadata-negative-pair-not-proven", DesignConversionDpoUnits.CriticUnit)
        ]).ToArray();
        var allowed = records.Single(record => record.RecordId == "metadata-allowed");
        var blocked = new[]
        {
            WithProvenanceMetadata(
                records.Single(record => record.RecordId == "metadata-status-pending"),
                ("status", "pending")),
            WithProvenanceMetadata(
                records.Single(record => record.RecordId == "metadata-status-pending-not-training-grade"),
                ("status", "pending"),
                ("not_training_grade", bool.TrueString)),
            WithProvenanceMetadata(
                records.Single(record => record.RecordId == "metadata-status-pending-negative-pair-not-proven"),
                ("status", "pending"),
                ("high_quality_negative_pair_not_proven", bool.TrueString)),
            WithProvenanceMetadata(
                records.Single(record => record.RecordId == "metadata-pending"),
                ("approval_status", "pending"),
                ("review_status", "unapproved")),
            WithProvenanceMetadata(
                records.Single(record => record.RecordId == "metadata-approved-false"),
                ("approved_for_training", bool.FalseString)),
            WithProvenanceMetadata(
                records.Single(record => record.RecordId == "metadata-not-training-grade"),
                ("not_training_grade", bool.TrueString)),
            WithProvenanceMetadata(
                records.Single(record => record.RecordId == "metadata-training-grade-false"),
                ("training_grade", bool.FalseString)),
            WithProvenanceMetadata(
                records.Single(record => record.RecordId == "metadata-synthetic-negative"),
                ("synthetic_negative", bool.TrueString)),
            WithProvenanceMetadata(
                records.Single(record => record.RecordId == "metadata-scaffold-negative"),
                ("scaffold_negative", bool.TrueString)),
            WithProvenanceMetadata(
                records.Single(record => record.RecordId == "metadata-negative-pair-not-proven"),
                ("high_quality_negative_pair_not_proven", bool.TrueString))
        };

        var result = await new DesignConversionTrainingRunner().RunTrainingRecordsAsync(
            new DesignConversionMachine(),
            blocked.Prepend(allowed).ToArray(),
            new DesignConversionTrainingPipelineOptions(
                "hd3-explicit-metadata-exclusion",
                BatchSize: 4,
                RequireDatasetCountGate: false,
                DatasetOptions: LocalDatasetOptions()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal([allowed.RecordId], result.Value!.DpoBuild.AcceptedRecords.Select(record => record.RecordId).ToArray());
        Assert.Equal(blocked.Select(record => record.RecordId).Order(StringComparer.Ordinal),
            result.Value.DpoBuild.QuarantinedRecords.Select(record => record.RecordId).Order(StringComparer.Ordinal));
        Assert.Equal(1, result.Value.PipelineResult.AcceptedRecordCount);
        var item = Assert.Single(result.Value.PipelineResult.TrainingItems);
        Assert.Equal("metadata-allowed", item.ItemId);
        Assert.Empty(result.Value.PipelineResult.BatchResults);
        Assert.False(result.Value.PipelineResult.MachineLifecycleHookAttempted);
        Assert.Contains("approval_missing",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked[0]));
        Assert.Contains("approval_missing",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked[1]));
        Assert.Contains("not_training_grade",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked[1]));
        Assert.Contains("approval_missing",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked[2]));
        Assert.Contains("high_quality_negative_pair_not_proven",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked[2]));
        Assert.Contains("approval_missing",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked[3]));
        Assert.Contains("approval_missing",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked[4]));
        Assert.Contains("not_training_grade",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked[5]));
        Assert.Contains("not_training_grade",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked[6]));
        Assert.Contains("synthetic_negative",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked[7]));
        Assert.Contains("scaffold_negative",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked[8]));
        Assert.Contains("high_quality_negative_pair_not_proven",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked[9]));
    }

    [Fact]
    public async Task RunAsync_DoesNotLeakProvenanceLicenseQuarantineOrDeterministicEvidenceIntoTrainingItems()
    {
        var example = DesignConversionDpoGeneratorTests.CreateExample("no-leakage", DesignConversionDpoUnits.CriticUnit) with
        {
            Provenance = new DesignConversionDpoProvenance(
                SourceRequestId: "source-request-should-not-be-feature",
                SourceKind: DesignConversionSourceKind.Figma.ToString(),
                SourceManifestRef: "manifest://source-redacted",
                ReviewerRef: "reviewer-redacted",
                CreatedAtUtc: DateTimeOffset.UnixEpoch,
                Metadata: new Dictionary<string, string>
                {
                    ["deterministicEvidenceRefs"] = "deterministic-input-evidence:fixture=qa",
                    ["license"] = "project-local"
                }),
            TeacherEvidenceRefs = ["deterministic-input-evidence:fixture=qa"]
        };
        var records = BuildRecords([example]);
        var pipeline = new DesignConversionTrainingPipeline();

        var result = await pipeline.RunAsync(
            new DesignConversionMachine(),
            records,
            new DesignConversionTrainingPipelineOptions(
                "slice-4-no-leakage",
                BatchSize: 2,
                RequireDatasetCountGate: false,
                DatasetOptions: LocalDatasetOptions()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var item = Assert.Single(result.Value!.TrainingItems);
        var payload = string.Join("\n", item.Prompt, item.ChosenResponse, item.RejectedResponse);
        Assert.DoesNotContain("source-request-should-not-be-feature", payload, StringComparison.Ordinal);
        Assert.DoesNotContain("deterministic-input-evidence", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("license", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("quarantine", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("reviewer-redacted", payload, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsync_CarriesSourceEncoderFeatureMapAndVectorIntoTrainingItems()
    {
        var example = DesignConversionDpoGeneratorTests.CreateExample("source-encoder-training", DesignConversionDpoUnits.ImproverUnit) with
        {
            InternalVectorPayload = [],
            SourceStructurePayload = """
                {
                  "root":{"type":"FRAME","layoutMode":"VERTICAL","width":1280,"height":720},
                  "children":[{"type":"TEXT"},{"type":"COMPONENT","name":"cta_button"}],
                  "breakpoints":["desktop"]
                }
                """,
            Chosen = DesignConversionDpoGeneratorTests.CreateOutput(
                "source-encoder-training-chosen",
                accepted: true,
                score: 0.91,
                htmlSuffix: "source-encoder-chosen") with
            {
                Html = "<main class=\"screen grid\"><button class=\"cta\">Save</button></main>",
                Css = ".screen.grid{display:grid;gap:24px}.cta{padding:12px;border-radius:8px}"
            },
            Rejected = DesignConversionDpoGeneratorTests.CreateOutput(
                "source-encoder-training-rejected",
                accepted: false,
                score: 0.38,
                htmlSuffix: "source-encoder-rejected") with
            {
                Html = "<main class=\"screen\"><button>Save</button></main>",
                Css = ".screen{display:block}.cta{padding:0}"
            }
        };
        var records = BuildRecords([example]);
        var pipeline = new DesignConversionTrainingPipeline();

        var result = await pipeline.RunAsync(
            new DesignConversionMachine(),
            records,
            new DesignConversionTrainingPipelineOptions(
                "source-encoder-training-intake",
                BatchSize: 1,
                RequireDatasetCountGate: false,
                DatasetOptions: LocalDatasetOptions()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var item = Assert.Single(result.Value!.TrainingItems);
        using var promptDocument = JsonDocument.Parse(item.Prompt);
        var promptRoot = promptDocument.RootElement;
        Assert.True(promptRoot.TryGetProperty("internal_vector", out var internalVector));
        Assert.True(internalVector.GetArrayLength() > 0);
        Assert.StartsWith("dcm-source-encoder-vector-", promptRoot.GetProperty("source_encoder_vector_hash").GetString());
        Assert.False(string.IsNullOrWhiteSpace(promptRoot.GetProperty("source_encoder_feature_names").GetString()));
        Assert.Equal(
            "source_kind_source_id_not_vector_features",
            promptRoot.GetProperty("source_encoder_shortcut_guard").GetString());
        Assert.Equal(bool.FalseString, promptRoot.GetProperty("source_kind_in_vector").GetString());
        Assert.Equal(bool.FalseString, promptRoot.GetProperty("source_id_in_vector").GetString());
        Assert.Contains("source_json_node_count", item.Prompt, StringComparison.Ordinal);
        Assert.Contains("chosen_css_rule_count", item.Prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("accepted_flag_delta", item.Prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("visual_score_margin", item.Prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("chosen_visual_score", item.Prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("rejected_visual_score", item.Prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("usefulness_score", item.Prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("layoutMode\":\"VERTICAL", item.ChosenResponse, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ManifestPublisher_ExportsPromotionMetadataWithoutModelServerUpload()
    {
        var records = BuildRecords([DesignConversionDpoGeneratorTests.CreateExample("manifest-record", DesignConversionDpoUnits.ImproverUnit)]);
        var pipeline = new DesignConversionTrainingPipeline();
        var machine = new DesignConversionMachine();
        var pipelineResult = await pipeline.RunAsync(
            machine,
            records,
            new DesignConversionTrainingPipelineOptions(
                "slice-4-manifest",
                BatchSize: 2,
                RequireDatasetCountGate: false,
                DatasetOptions: LocalDatasetOptions()));
        Assert.True(pipelineResult.IsSuccess, pipelineResult.ErrorMessage);

        var manifest = await new DesignConversionManifestPublisher().PublishManifestMetadataAsync(
            machine,
            pipelineResult.Value!,
            "slice-4-manifest-metadata");

        Assert.True(manifest.IsSuccess, manifest.ErrorMessage);
        Assert.Equal("slice-4-manifest-metadata", manifest.Value!.ManifestId);
        Assert.Equal(DesignConversionMachine.MachineId, manifest.Value.MachineId);
        Assert.Equal(DesignConversionMachine.DomainTag, manifest.Value.DomainTag);
        Assert.False(manifest.Value.ModelServerUploadAttempted);
        Assert.False(manifest.Value.ModelServerPromotionClaimed);
        Assert.False(manifest.Value.DeterministicLogicClaimedAsLearning);
        Assert.Contains("MODEL_SERVER_UPLOAD_NOT_ATTEMPTED", manifest.Value.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_MANIFEST_DATASET_COUNT_GATE_NOT_PASSED", manifest.Value.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_LIFECYCLE_EVIDENCE_NOT_ATTEMPTED", manifest.Value.EvidenceRefs);
    }

    [Fact]
    public async Task RunAsync_RefusesLifecycleHookWhenDatasetCountGateLacksRequiredTrainAndHeldOutRecords()
    {
        var records = BuildSyntheticSourceDisjointRecords(3);
        var pipeline = new DesignConversionTrainingPipeline();

        var result = await pipeline.RunAsync(
            new DesignConversionMachine(),
            records,
            new DesignConversionTrainingPipelineOptions(
                "s3-count-gate-fail",
                BatchSize: 2,
                DatasetOptions: LocalDatasetOptions()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionTrainingBoundaryDecisionKind.CountGateFailed, result.Value!.BoundaryDecision.Kind);
        Assert.True(result.Value.BoundaryDecision.CountGateRequired);
        Assert.False(result.Value.BoundaryDecision.CountGatePassed);
        Assert.False(result.Value.BoundaryDecision.AllowsMachineLifecycleHook);
        Assert.False(result.Value.BoundaryDecision.RealTrainingRunClaimed);
        Assert.False(result.Value.BoundaryDecision.CheckpointClaimed);
        Assert.False(result.Value.BoundaryDecision.PromotionClaimed);
        Assert.False(result.Value.MachineLifecycleHookAttempted);
        Assert.False(result.Value.LifecycleEvidence.MachineLifecycleHookCalled);
        Assert.False(result.Value.LifecycleEvidence.DatasetCountGatePassed);
        Assert.Equal("not_proven", result.Value.LifecycleEvidence.CheckpointCandidateStatus);
        Assert.Equal("not_proven", result.Value.LifecycleEvidence.FreshLoadStatus);
        Assert.Equal("PROMOTION_BLOCKED_MEASUREMENT_GATE_NOT_PASSED", result.Value.LifecycleEvidence.PromotionStatus);
        Assert.False(result.Value.LifecycleEvidence.PromotionUploadAllowed);
        Assert.Contains("DATASET_COUNT_GATE_NOT_PASSED", result.Value.LifecycleEvidence.FailureReasons);
        Assert.False(result.Value.DatasetCountGate.Passed);
        Assert.Equal(50, result.Value.DatasetCountGate.RequiredTrainRecords);
        Assert.Equal(50, result.Value.DatasetCountGate.RequiredHeldOutRecords);
        Assert.True(result.Value.DatasetCountGate.ActualTrainRecords < 50);
        Assert.True(result.Value.DatasetCountGate.ActualHeldOutRecords < 50);
        Assert.Empty(result.Value.Batches);
        Assert.Empty(result.Value.BatchResults);
        Assert.Empty(result.Value.TrainingItems);
        Assert.Contains("DESIGN_CONVERSION_TRAINING_BOUNDARY_REFUSED_BY_DATASET_COUNT_GATE",
            result.Value.BoundaryDecision.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_MACHINE_TRAINBATCH_LIFECYCLE_HOOK_NOT_CALLED", result.Value.Warnings);
    }

    [Fact]
    public async Task RunAsync_SourceDisjointHundredAcceptedRecordsPassCountGateWithoutCallingTraining()
    {
        var records = BuildSyntheticSourceDisjointRecords(100);
        var pipeline = new DesignConversionTrainingPipeline();

        var result = await pipeline.RunAsync(
            new DesignConversionMachine(),
            records,
            new DesignConversionTrainingPipelineOptions(
                "s3-count-gate-pass-no-training",
                BatchSize: 25,
                CallMachineLifecycleHook: false,
                DatasetOptions: LocalDatasetOptions()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionTrainingBoundaryDecisionKind.CountGatePassed, result.Value!.BoundaryDecision.Kind);
        Assert.True(result.Value.BoundaryDecision.CountGateRequired);
        Assert.True(result.Value.BoundaryDecision.CountGatePassed);
        Assert.True(result.Value.BoundaryDecision.AllowsMachineLifecycleHook);
        Assert.False(result.Value.BoundaryDecision.RealTrainingRunClaimed);
        Assert.False(result.Value.BoundaryDecision.CheckpointClaimed);
        Assert.False(result.Value.BoundaryDecision.PromotionClaimed);
        Assert.True(result.Value.DatasetCountGate.Passed);
        Assert.Equal(50, result.Value.DatasetCountGate.RequiredTrainRecords);
        Assert.Equal(50, result.Value.DatasetCountGate.RequiredHeldOutRecords);
        Assert.True(result.Value.DatasetCountGate.ActualTrainRecords >= 50);
        Assert.True(result.Value.DatasetCountGate.ActualHeldOutRecords >= 50);
        Assert.True(result.Value.DatasetCountGate.SourceDisjoint);
        Assert.Equal(100, result.Value.DatasetManifest.AcceptedRecordCount);
        Assert.False(result.Value.MachineLifecycleHookAttempted);
        Assert.Empty(result.Value.BatchResults);
        Assert.Equal(50, result.Value.TrainingItems.Count);
        Assert.True(result.Value.LifecycleEvidence.DatasetCountGatePassed);
        Assert.False(result.Value.LifecycleEvidence.MachineLifecycleHookCalled);
        Assert.Equal("not_proven", result.Value.LifecycleEvidence.CheckpointCandidateStatus);
        Assert.Equal("not_proven", result.Value.LifecycleEvidence.ExportStatus);
        Assert.Equal("not_proven", result.Value.LifecycleEvidence.ImportStatus);
        Assert.Equal("not_proven", result.Value.LifecycleEvidence.FreshLoadStatus);
        Assert.Equal("not_proven", result.Value.LifecycleEvidence.ContinueTrainingStatus);
        Assert.Equal("not_run", result.Value.LifecycleEvidence.MeasurementGateStatus);
        Assert.False(result.Value.LifecycleEvidence.PromotionUploadAllowed);
        Assert.Contains("LIFECYCLE_HOOK_DISABLED_BY_OPTIONS", result.Value.LifecycleEvidence.FailureReasons);
        Assert.Contains("COUNT_GATE_PASSED", result.Value.DatasetCountGate.Reasons);
        Assert.Contains("DESIGN_CONVERSION_DPO_DATASET_MANIFEST_ATTACHED_TO_TRAINING_BOUNDARY",
            result.Value.BoundaryDecision.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_MACHINE_TRAINBATCH_LIFECYCLE_HOOK_NOT_CALLED", result.Value.Warnings);
    }

    [Fact]
    public async Task RunAsync_SourceDisjointHundredAcceptedRecordsPassCountGateAndCallsLifecycleHookWithHonestEvidence()
    {
        var records = BuildSyntheticSourceDisjointRecords(100);
        var dataset = new DesignConversionDpoDatasetBuilder().BuildDataset(records, LocalDatasetOptions());
        Assert.True(dataset.IsSuccess, dataset.ErrorMessage);
        Assert.Equal(50, dataset.Value!.TrainRows.Count);
        Assert.Equal(50, dataset.Value.HeldOutRows.Count);

        var trainRowHashes = dataset.Value.TrainRows
            .Select(row => row.RecordIdentityHash)
            .ToHashSet(StringComparer.Ordinal);
        var heldOutRowHashes = dataset.Value.HeldOutRows
            .Select(row => row.RecordIdentityHash)
            .ToHashSet(StringComparer.Ordinal);
        var trainRecordIds = records
            .Where(record => trainRowHashes.Contains(ToRecordIdentityHash(record)))
            .Select(record => record.RecordId)
            .ToHashSet(StringComparer.Ordinal);
        var heldOutRecordIds = records
            .Where(record => heldOutRowHashes.Contains(ToRecordIdentityHash(record)))
            .Select(record => record.RecordId)
            .ToHashSet(StringComparer.Ordinal);
        var observations = new List<DesignConversionTrainBatchObservation>();
        var observerAttemptedReplacementRowsTrained = -999;
        var pipeline = new DesignConversionTrainingPipeline(
            trainBatchObserver: observation =>
            {
                observations.Add(observation);
                observerAttemptedReplacementRowsTrained = int.MaxValue;
            });
        var constructorParameters = typeof(DesignConversionTrainingPipeline).GetConstructors()
            .SelectMany(ctor => ctor.GetParameters())
            .ToArray();
        Assert.DoesNotContain(constructorParameters, parameter => parameter.Name == "trainBatchAsync");
        Assert.Contains(constructorParameters, parameter =>
            parameter.Name == "trainBatchObserver" &&
            parameter.ParameterType == typeof(Action<DesignConversionTrainBatchObservation>));

        var result = await pipeline.RunAsync(
            new DesignConversionMachine(),
            records,
            new DesignConversionTrainingPipelineOptions(
                "s3-local-lifecycle-pass",
                BatchSize: 25,
                DatasetOptions: LocalDatasetOptions()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionTrainingBoundaryDecisionKind.CountGatePassed, result.Value!.BoundaryDecision.Kind);
        Assert.True(result.Value.DatasetCountGate.Passed);
        Assert.True(result.Value.BoundaryDecision.AllowsMachineLifecycleHook);
        Assert.True(result.Value.MachineLifecycleHookAttempted);
        Assert.Equal(result.Value.Batches.Count, result.Value.BatchResults.Count);
        Assert.Equal(50, result.Value.BatchResults.Sum(batch => batch.RowsTrained));
        Assert.All(result.Value.BatchResults, batch => Assert.Empty(batch.PromotedUnitIds));
        Assert.Equal(result.Value.Batches.Count, observations.Count);
        Assert.Equal(50, observations.Sum(observation => observation.TrainingItemCount));
        Assert.Equal(
            result.Value.BatchResults.Select(batch => batch.BatchId).Order(StringComparer.Ordinal),
            observations.Select(observation => observation.BatchId).Order(StringComparer.Ordinal));
        Assert.All(observations, observation => Assert.Equal(DesignConversionMachine.DomainTag, observation.DomainTag));
        Assert.Equal(50, result.Value.BatchResults.Sum(batch => batch.RowsTrained));
        Assert.NotEqual(observerAttemptedReplacementRowsTrained, result.Value.BatchResults.Sum(batch => batch.RowsTrained));

        var observedTrainingItemIds = observations
            .SelectMany(observation => observation.TrainingItemIds)
            .ToArray();
        Assert.Equal(trainRecordIds.Order(StringComparer.Ordinal), observedTrainingItemIds.Order(StringComparer.Ordinal));
        Assert.All(observedTrainingItemIds, id => Assert.DoesNotContain(id, heldOutRecordIds));

        var trainedRecordHashes = result.Value.Batches
            .SelectMany(batch => batch.Records)
            .Select(ToRecordIdentityHash)
            .ToArray();
        Assert.Equal(50, trainedRecordHashes.Length);
        Assert.Equal(trainRowHashes.Order(StringComparer.Ordinal), trainedRecordHashes.Order(StringComparer.Ordinal));
        Assert.All(trainedRecordHashes, hash => Assert.DoesNotContain(hash, heldOutRowHashes));
        Assert.Equal(50, result.Value.TrainingItems.Count);

        var lifecycle = result.Value.LifecycleEvidence;
        Assert.True(lifecycle.DatasetCountGatePassed);
        Assert.True(lifecycle.MachineLifecycleHookCalled);
        Assert.Equal(result.Value.Batches.Count, lifecycle.LocalLifecycleBatchResultCount);
        Assert.Equal("local_checkpoint_candidate_with_real_learned_weights", lifecycle.CheckpointCandidateStatus);
        Assert.True(lifecycle.CheckpointCandidateCreated);
        Assert.True(lifecycle.CheckpointContainsRealTrainedWeights);
        Assert.Equal("in_process_checkpoint_import_verified", lifecycle.CheckpointImportStatus);
        Assert.Equal("metadata_only_export_verified", lifecycle.ExportStatus);
        Assert.Equal("metadata_only_import_verified", lifecycle.ImportStatus);
        Assert.Equal(DesignConversionLocalFreshLoadProbe.VerifiedStatus, lifecycle.FreshLoadStatus);
        Assert.NotNull(lifecycle.LocalFreshLoadProbe);
        Assert.True(lifecycle.LocalFreshLoadProbe!.MetadataContinuityVerified);
        Assert.Equal(DesignConversionMachine.MachineId, lifecycle.LocalFreshLoadProbe.MachineId);
        Assert.Equal(DesignConversionMachine.DomainTag, lifecycle.LocalFreshLoadProbe.DomainTag);
        Assert.True(lifecycle.LocalFreshLoadProbe.CheckpointContainsRealTrainedWeights);
        Assert.False(lifecycle.LocalFreshLoadProbe.ContinueTrainingProven);
        Assert.False(lifecycle.LocalFreshLoadProbe.MeasurementGateRun);
        Assert.False(lifecycle.LocalFreshLoadProbe.PromotionUploadAllowed);
        Assert.False(File.Exists(lifecycle.LocalFreshLoadProbe.CheckpointRef));
        Assert.Equal(DesignConversionLocalContinueTrainingProbe.VerifiedStatus, lifecycle.ContinueTrainingStatus);
        Assert.NotNull(lifecycle.LocalContinueTrainingProbe);
        Assert.Equal(DesignConversionLocalContinueTrainingProbe.VerifiedStatus, lifecycle.LocalContinueTrainingProbe!.Status);
        Assert.Equal(lifecycle.LocalFreshLoadProbe.CheckpointRef, lifecycle.LocalContinueTrainingProbe.SourceCheckpointRef);
        Assert.StartsWith(Path.GetFullPath(Path.GetTempPath()), lifecycle.LocalContinueTrainingProbe.ContinuedCheckpointRef, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(lifecycle.LocalContinueTrainingProbe.ContinuedCheckpointRef));
        Assert.True(lifecycle.LocalContinueTrainingProbe.FreshLoadMetadataContinuityVerified);
        Assert.True(lifecycle.LocalContinueTrainingProbe.DatasetCountGatePassed);
        Assert.True(lifecycle.LocalContinueTrainingProbe.TrainSplitOnly);
        Assert.True(lifecycle.LocalContinueTrainingProbe.SourceCheckpointUnchanged);
        Assert.Equal(
            lifecycle.LocalContinueTrainingProbe.SourceCheckpointSha256Before,
            lifecycle.LocalContinueTrainingProbe.SourceCheckpointSha256After);
        Assert.True(lifecycle.LocalContinueTrainingProbe.ContinuationBatchAppended);
        Assert.True(lifecycle.LocalContinueTrainingProbe.NoReplayBatchIdVerified);
        Assert.False(lifecycle.LocalContinueTrainingProbe.ContinueTrainingMetadataLocalProbeOnly);
        Assert.True(lifecycle.LocalContinueTrainingProbe.RealTrainedWeightsContinued);
        Assert.False(lifecycle.LocalContinueTrainingProbe.MeasurementGateRun);
        Assert.False(lifecycle.LocalContinueTrainingProbe.PromotionUploadAllowed);
        Assert.Equal(0, lifecycle.LocalContinueTrainingProbe.HeldOutRecordUsageCount);
        Assert.Equal(10, lifecycle.LocalContinueTrainingProbe.ContinueTrainRecordCount);
        Assert.DoesNotContain(
            lifecycle.LocalContinueTrainingProbe.ContinueTrainingRecordIds,
            id => heldOutRecordIds.Contains(id));
        Assert.True(lifecycle.LocalContinueTrainingProbe.ContinuedTrainingHistoryCount >
            lifecycle.LocalContinueTrainingProbe.StartingTrainingHistoryCount);
        Assert.Equal("not_run", lifecycle.MeasurementGateStatus);
        Assert.Equal("PROMOTION_BLOCKED_MEASUREMENT_GATE_NOT_PASSED", lifecycle.PromotionStatus);
        Assert.False(lifecycle.PromotionUploadAllowed);
        Assert.DoesNotContain("fresh_load", lifecycle.NotProvenFields);
        Assert.Contains("trained_feature_fresh_load_metrics", lifecycle.NotProvenFields);
        Assert.DoesNotContain("continue_training", lifecycle.NotProvenFields);
        Assert.DoesNotContain("continue_training_real_trained_weights", lifecycle.NotProvenFields);
        Assert.Contains("held_out_measurement_gate", lifecycle.NotProvenFields);
        Assert.DoesNotContain("real_trained_weights", lifecycle.NotProvenFields);
        Assert.Contains("DESIGN_CONVERSION_LOCAL_LIFECYCLE_HOOK_CALLED_AFTER_COUNT_GATE_PASS",
            lifecycle.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_LOCAL_FRESH_LOAD_METADATA_CONTINUITY_VERIFIED",
            lifecycle.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_AFTER_FRESH_LOAD_METADATA_PASS",
            lifecycle.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_HELD_OUT_ROWS_USED_0",
            lifecycle.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_SOURCE_CHECKPOINT_UNCHANGED",
            lifecycle.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_APPENDED_ONE_BATCH",
            lifecycle.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_NO_REPLAY_BATCH_ID_VERIFIED",
            lifecycle.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_REAL_TRAINED_WEIGHTS_CONTINUED",
            lifecycle.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_LEARNED_WEIGHTS_PRESENT_TRUE_AFTER_FRESH_LOAD",
            lifecycle.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_PROMOTION_UPLOAD_BLOCKED_WITHOUT_MEASUREMENT_GATE",
            lifecycle.EvidenceRefs);
        Assert.Empty(lifecycle.FailureReasons);
    }

    [Fact]
    public async Task RunAsync_SourceDisjointHundredAcceptedRecordsPersistsRealLearnedWeightsWithoutRawPayloads()
    {
        var records = BuildSyntheticSourceDisjointRecords(100);
        var machine = new DesignConversionMachine();
        var pipeline = new DesignConversionTrainingPipeline();

        var result = await pipeline.RunAsync(
            machine,
            records,
            new DesignConversionTrainingPipelineOptions(
                "pass114-real-weight-checkpoint",
                BatchSize: 25,
                DatasetOptions: LocalDatasetOptions()));
        var checkpoint = machine.ExportCheckpoint();
        var serializedCheckpoint = System.Text.Json.JsonSerializer.Serialize(
            checkpoint.Value,
            DesignConversionCheckpointJsonOptions.Standard);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(checkpoint.IsSuccess, checkpoint.ErrorMessage);
        Assert.True(DesignConversionMachine.CheckpointHasRealLearnedWeights(checkpoint.Value));
        Assert.True(DesignConversionMachine.CountRealLearnedWeightArtifacts(checkpoint.Value) > 0);
        Assert.True(DesignConversionMachine.CountRealLearnedWeightObservations(checkpoint.Value) > 0);
        Assert.True(result.Value!.LifecycleEvidence.CheckpointContainsRealTrainedWeights);
        Assert.False(result.Value.LifecycleEvidence.PromotionUploadAllowed);

        var learnedWeights = new[]
            {
                checkpoint.Value!.CriticUnit,
                checkpoint.Value.ImproverUnit
            }
            .SelectMany(unit => unit.LearnedWeights ?? [])
            .ToArray();
        Assert.NotEmpty(learnedWeights);
        Assert.All(learnedWeights, weight =>
        {
            Assert.True(weight.ObservationCount > 0);
            Assert.True(double.IsFinite(weight.Weight));
            Assert.Contains(DesignConversionMachine.LearnedWeightsSanitizedFieldsEvidence, weight.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_LEARNED_WEIGHT_NO_RAW_TREE_HTML_SCREENSHOT", weight.EvidenceRefs);
            Assert.All(weight.SourceIdentityHashes, hash =>
            {
                Assert.Equal(64, hash.Length);
                Assert.All(hash, ch => Assert.True(Uri.IsHexDigit(ch)));
            });
        });

        foreach (var record in result.Value.Batches.SelectMany(batch => batch.Records))
        {
            Assert.DoesNotContain(record.RecordId, serializedCheckpoint, StringComparison.Ordinal);
            Assert.DoesNotContain(record.Provenance.SourceManifestRef, serializedCheckpoint, StringComparison.Ordinal);
            if (!string.IsNullOrWhiteSpace(record.SourceStructurePayload))
                Assert.DoesNotContain(record.SourceStructurePayload, serializedCheckpoint, StringComparison.Ordinal);
            if (!string.IsNullOrWhiteSpace(record.Chosen.Html))
                Assert.DoesNotContain(record.Chosen.Html, serializedCheckpoint, StringComparison.Ordinal);
            if (!string.IsNullOrWhiteSpace(record.Rejected.Html))
                Assert.DoesNotContain(record.Rejected.Html, serializedCheckpoint, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task LocalContinueTrainingProbe_AppendsRealWeightsWithoutMutatingSourceCheckpoint()
    {
        var records = BuildSyntheticSourceDisjointRecords(4);
        var batches = new DesignConversionTrainingPipeline()
            .BuildBatches(
                records,
                new DesignConversionTrainingPipelineOptions(
                    "pass114-direct-continue",
                    BatchSize: 1,
                    DatasetOptions: LocalDatasetOptions(
                        "pass114-direct-continue",
                        MinimumTrainRecords: 1,
                        MinimumHeldOutRecords: 1)));
        Assert.True(batches.IsSuccess, batches.ErrorMessage);
        Assert.True(batches.Value!.Count >= 2);
        var sourceBatch = batches.Value[0];
        var continuationBatch = batches.Value[1];
        var machine = new DesignConversionMachine();
        var train = await machine.TrainBatchAsync(new LearningBatch<DesignConversionDomain.DesignConversionDomain>(
            BatchId: sourceBatch.BatchId,
            DomainTag: DesignConversionMachine.DomainTag,
            TrainingItems: sourceBatch.TrainingItems,
            SourceIds: sourceBatch.SourceIds));
        Assert.True(train.IsSuccess, train.ErrorMessage);
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"pass114-real-weights-{Guid.NewGuid():N}");
        var sourcePath = Path.Combine(root, "source-checkpoint.json");
        var continuedPath = Path.Combine(root, "continued-checkpoint.json");

        try
        {
            var fresh = await DesignConversionLocalFreshLoadProbe.VerifySavedCheckpointAsync(machine, sourcePath);
            Assert.True(fresh.IsSuccess, fresh.ErrorMessage);
            Assert.True(fresh.Value!.CheckpointContainsRealTrainedWeights);
            var sourceShaBefore = await ReadFileSha256Async(sourcePath);
            var sourceLoadBefore = await DesignConversionMachine.LoadCheckpointAsync(sourcePath);
            Assert.True(sourceLoadBefore.IsSuccess, sourceLoadBefore.ErrorMessage);
            var sourceObservationCount = DesignConversionMachine.CountRealLearnedWeightObservations(
                sourceLoadBefore.Value!.Checkpoint);

            var continued = await DesignConversionLocalContinueTrainingProbe.VerifyFreshLoadedCheckpointAsync(
                fresh.Value,
                continuationBatch,
                heldOutRecordIdentityHashes: [],
                continuedPath,
                datasetCountGatePassed: true);

            Assert.True(continued.IsSuccess, continued.ErrorMessage);
            Assert.True(continued.Value!.SourceCheckpointUnchanged);
            Assert.Equal(sourceShaBefore, continued.Value.SourceCheckpointSha256Before);
            Assert.Equal(sourceShaBefore, continued.Value.SourceCheckpointSha256After);
            Assert.True(continued.Value.ContinuationBatchAppended);
            Assert.False(continued.Value.ContinueTrainingMetadataLocalProbeOnly);
            Assert.True(continued.Value.RealTrainedWeightsContinued);
            Assert.Equal(sourceShaBefore, await ReadFileSha256Async(sourcePath));

            var sourceLoadAfter = await DesignConversionMachine.LoadCheckpointAsync(sourcePath);
            var continuedLoad = await DesignConversionMachine.LoadCheckpointAsync(continuedPath);
            Assert.True(sourceLoadAfter.IsSuccess, sourceLoadAfter.ErrorMessage);
            Assert.True(continuedLoad.IsSuccess, continuedLoad.ErrorMessage);
            Assert.Equal(
                sourceObservationCount,
                DesignConversionMachine.CountRealLearnedWeightObservations(sourceLoadAfter.Value!.Checkpoint));
            Assert.True(
                DesignConversionMachine.CountRealLearnedWeightObservations(continuedLoad.Value!.Checkpoint) >
                sourceObservationCount);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RunAsync_LocalFreshLoadProbeUsesTempPathAndDoesNotWriteRepoArtifacts()
    {
        var beforeArtifacts = SnapshotRepoArtifacts();
        var records = BuildSyntheticSourceDisjointRecords(100);
        var pipeline = new DesignConversionTrainingPipeline();

        var result = await pipeline.RunAsync(
            new DesignConversionMachine(),
            records,
            new DesignConversionTrainingPipelineOptions(
                "s3-local-fresh-load-no-artifacts",
                BatchSize: 25,
                DatasetOptions: LocalDatasetOptions()));

        var afterArtifacts = SnapshotRepoArtifacts();
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(beforeArtifacts, afterArtifacts);
        var probe = Assert.IsType<DesignConversionLocalFreshLoadProbeReport>(
            result.Value!.LifecycleEvidence.LocalFreshLoadProbe);
        Assert.StartsWith(Path.GetFullPath(Path.GetTempPath()), probe.CheckpointRef, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Path.Combine("llm_mvp_core", "artifacts"), probe.CheckpointRef, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(probe.CheckpointRef));
        var continueProbe = Assert.IsType<DesignConversionLocalContinueTrainingProbeReport>(
            result.Value.LifecycleEvidence.LocalContinueTrainingProbe);
        Assert.StartsWith(Path.GetFullPath(Path.GetTempPath()), continueProbe.ContinuedCheckpointRef, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Path.Combine("llm_mvp_core", "artifacts"), continueProbe.ContinuedCheckpointRef, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(continueProbe.ContinuedCheckpointRef));
        Assert.Equal(DesignConversionLocalFreshLoadProbe.VerifiedStatus, result.Value.LifecycleEvidence.FreshLoadStatus);
        Assert.Equal(DesignConversionLocalContinueTrainingProbe.VerifiedStatus, result.Value.LifecycleEvidence.ContinueTrainingStatus);
        Assert.Equal("not_run", result.Value.LifecycleEvidence.MeasurementGateStatus);
        Assert.False(result.Value.LifecycleEvidence.PromotionUploadAllowed);
    }

    [Fact]
    public async Task RunAsync_RefusesBeforeTrainingWhenTrainAndHeldOutIdentitiesOverlap()
    {
        var records = BuildRecords(
        [
            WithSource(DesignConversionDpoGeneratorTests.CreateExample(
                "overlap-record",
                DesignConversionDpoUnits.CriticUnit,
                usefulness: 0.95), "overlap-source-a"),
            WithSource(DesignConversionDpoGeneratorTests.CreateExample(
                "overlap-record",
                DesignConversionDpoUnits.CriticUnit,
                usefulness: 0.85), "overlap-source-b")
        ]);
        var machine = new DesignConversionMachine();
        var versionBefore = await ExportMachineVersionAsync(machine);
        var trainBatchCallCount = 0;
        var pipeline = new DesignConversionTrainingPipeline(
            trainBatchObserver: _ =>
            {
                trainBatchCallCount++;
            });

        var result = await pipeline.RunAsync(
            machine,
            records,
            new DesignConversionTrainingPipelineOptions(
                "s3-overlap-refuses-before-training",
                BatchSize: 1,
                DatasetOptions: new DesignConversionDpoDatasetOptions(
                    DatasetId: "s3-overlap-refuses-before-training",
                    MinimumTrainRecords: 1,
                    MinimumHeldOutRecords: 1,
                    Preflight: LocalPreflight())));

        Assert.False(result.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_DPO_TRAIN_HELDOUT_IDENTITY_OVERLAP", result.ErrorCode);
        Assert.Equal(0, trainBatchCallCount);
        Assert.Equal(versionBefore, await ExportMachineVersionAsync(machine));
    }

    [Fact]
    public async Task RunAsync_RefusesBeforeTrainingWhenTrainSplitCannotMapBackToRecords()
    {
        var originalRecords = BuildSyntheticSourceDisjointRecords(4);
        var mutatedRecords = originalRecords
            .Select(record => record with { RecordId = $"mapping-mismatch-{record.RecordId}" })
            .ToArray();
        var records = new MutatingAfterDatasetBuildRecords(originalRecords, mutatedRecords);
        var machine = new DesignConversionMachine();
        var versionBefore = await ExportMachineVersionAsync(machine);
        var trainBatchCallCount = 0;
        var pipeline = new DesignConversionTrainingPipeline(
            trainBatchObserver: _ =>
            {
                trainBatchCallCount++;
            });

        var result = await pipeline.RunAsync(
            machine,
            records,
            new DesignConversionTrainingPipelineOptions(
                "s3-train-split-mapping-failed",
                BatchSize: 2,
                DatasetOptions: new DesignConversionDpoDatasetOptions(
                    DatasetId: "s3-train-split-mapping-failed",
                    MinimumTrainRecords: 1,
                    MinimumHeldOutRecords: 1,
                    Preflight: LocalPreflight())));

        Assert.False(result.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_TRAIN_SPLIT_MAPPING_FAILED", result.ErrorCode);
        Assert.Equal(0, trainBatchCallCount);
        Assert.Equal(versionBefore, await ExportMachineVersionAsync(machine));
    }

    [Fact]
    public async Task Runner_RunAsync_ReturnsDatasetManifestAndCountGateDecisionAtLocalBoundary()
    {
        var examples = BuildSyntheticSourceDisjointExamples(3);
        var runner = new DesignConversionTrainingRunner();

        var result = await runner.RunAsync(
            new DesignConversionMachine(),
            examples,
            new DesignConversionTrainingPipelineOptions(
                "s3-runner-count-gate-fail",
                BatchSize: 2,
                DatasetOptions: LocalDatasetOptions()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionTrainingBoundaryDecisionKind.CountGateFailed, result.Value!.BoundaryDecision.Kind);
        Assert.False(result.Value.DatasetCountGate.Passed);
        Assert.Equal(result.Value.DatasetManifest, result.Value.PipelineResult.DatasetManifest);
        Assert.Equal(result.Value.DatasetCountGate, result.Value.PipelineResult.DatasetCountGate);
        Assert.False(result.Value.BoundaryDecision.RealTrainingRunClaimed);
        Assert.False(result.Value.BoundaryDecision.CheckpointClaimed);
        Assert.False(result.Value.BoundaryDecision.PromotionClaimed);
        Assert.False(result.Value.PipelineResult.MachineLifecycleHookAttempted);
        Assert.False(result.Value.PipelineResult.LifecycleEvidence.MachineLifecycleHookCalled);
        Assert.Contains("DESIGN_CONVERSION_TRAINING_RUNNER_DID_NOT_PRODUCE_REAL_LEARNED_WEIGHTS",
            result.Value.Warnings);
        Assert.Contains("DESIGN_CONVERSION_TRAINING_RUNNER_DID_NOT_PERFORM_EXTERNAL_MODEL_SERVER_TRAINING",
            result.Value.Warnings);
        Assert.Contains("DESIGN_CONVERSION_TRAINING_RUNNER_DID_NOT_CREATE_CHECKPOINT",
            result.Value.Warnings);
        Assert.Contains("DESIGN_CONVERSION_TRAINING_RUNNER_DID_NOT_PROMOTE_MODEL",
            result.Value.Warnings);
    }

    private static IReadOnlyList<DesignConversionDpoTrainingRecord> BuildRecords(
        IReadOnlyList<DesignConversionStoredDpoExample> examples)
    {
        var result = new DesignConversionDpoGenerator().BuildTrainReadyRecords(examples);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.QuarantinedRecords);
        return result.Value.AcceptedRecords;
    }

    private static DesignConversionDpoTrainingRecord WithProvenanceMetadata(
        DesignConversionDpoTrainingRecord record,
        params (string Key, string Value)[] values)
    {
        var metadata = new Dictionary<string, string>(record.Provenance.Metadata, StringComparer.Ordinal);
        foreach (var (key, value) in values)
            metadata[key] = value;

        return record with
        {
            Provenance = record.Provenance with
            {
                Metadata = metadata
            }
        };
    }

    private static IReadOnlyList<DesignConversionDpoTrainingRecord> BuildSyntheticSourceDisjointRecords(int count)
    {
        var result = new DesignConversionDpoGenerator().BuildTrainReadyRecords(BuildSyntheticSourceDisjointExamples(count));
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.QuarantinedRecords);
        Assert.Equal(count, result.Value.AcceptedRecords.Count);
        return result.Value.AcceptedRecords;
    }

    private static IReadOnlyList<DesignConversionStoredDpoExample> BuildSyntheticSourceDisjointExamples(int count)
        => Enumerable.Range(0, count)
            .Select(index =>
            {
                var unitId = index % 2 == 0
                    ? DesignConversionDpoUnits.CriticUnit
                    : DesignConversionDpoUnits.ImproverUnit;
                var example = DesignConversionDpoGeneratorTests.CreateExample(
                    $"s3-source-disjoint-{index:000}",
                    unitId,
                    usefulness: 0.8 + (index % 10 * 0.01));
                return WithSource(example, $"s3-source-{index:000}");
            })
            .ToArray();

    private static DesignConversionStoredDpoExample WithSource(
        DesignConversionStoredDpoExample example,
        string sourceId)
        => example with
        {
            Provenance = example.Provenance with
            {
                SourceRequestId = $"request:{sourceId}",
                SourceManifestRef = $"manifest://dcm-source/{sourceId}",
                Metadata = new Dictionary<string, string>
                {
                    ["source_design_id"] = sourceId
                }
            }
        };

    private static DesignConversionDpoDatasetOptions LocalDatasetOptions(
        string DatasetId = "dcm-dpo-dataset",
        int MinimumTrainRecords = 50,
        int MinimumHeldOutRecords = 50)
        => new(
            DatasetId: DatasetId,
            MinimumTrainRecords: MinimumTrainRecords,
            MinimumHeldOutRecords: MinimumHeldOutRecords,
            Preflight: LocalPreflight());

    private static DesignConversionDpoDatasetPreflight LocalPreflight()
        => DesignConversionDpoDatasetPreflight.LocalFixtureTrainingCandidateOnly();

    private static async Task<string> ExportMachineVersionAsync(DesignConversionMachine machine)
    {
        var export = await machine.ExportAsync();
        Assert.True(export.IsSuccess, export.ErrorMessage);
        return export.Value!.Version;
    }

    private static string ToRecordIdentityHash(DesignConversionDpoTrainingRecord record)
        => HashIdentity($"record|{record.RecordId}|{record.UnitId}");

    private static string HashIdentity(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static async Task<string> ReadFileSha256Async(string path)
        => Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(path))).ToLowerInvariant();

    private static IReadOnlyList<string> SnapshotRepoArtifacts()
    {
        var root = FindRepoRoot();
        var artifactsRoot = Path.Combine(root, "artifacts");
        if (!Directory.Exists(artifactsRoot))
            return [];

        return Directory.EnumerateFiles(artifactsRoot, "*", SearchOption.AllDirectories)
            .Select(path =>
            {
                var info = new FileInfo(path);
                return $"{Path.GetRelativePath(root, path)}|{info.Length}|{info.LastWriteTimeUtc.Ticks}";
            })
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
            directory = directory.Parent;

        Assert.NotNull(directory);
        return directory!.FullName;
    }

    private sealed class MutatingAfterDatasetBuildRecords : IReadOnlyList<DesignConversionDpoTrainingRecord>
    {
        private readonly IReadOnlyList<DesignConversionDpoTrainingRecord> _datasetBuildRecords;
        private readonly IReadOnlyList<DesignConversionDpoTrainingRecord> _mappingRecords;
        private int _enumerationCount;

        public MutatingAfterDatasetBuildRecords(
            IReadOnlyList<DesignConversionDpoTrainingRecord> datasetBuildRecords,
            IReadOnlyList<DesignConversionDpoTrainingRecord> mappingRecords)
        {
            _datasetBuildRecords = datasetBuildRecords;
            _mappingRecords = mappingRecords;
        }

        public int Count => _datasetBuildRecords.Count;

        public DesignConversionDpoTrainingRecord this[int index] => _datasetBuildRecords[index];

        public IEnumerator<DesignConversionDpoTrainingRecord> GetEnumerator()
        {
            _enumerationCount++;
            return (_enumerationCount <= 3 ? _datasetBuildRecords : _mappingRecords).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
