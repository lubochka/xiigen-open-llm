// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;
using DesignConversionDomain;
using DPO.Models;
using TransformerModel.Config;

public sealed class DesignConversionDpoPairAdmissionBridgeTests
{
    private const string TeacherModelId = "qwen3.5:9b";
    private static readonly string TeacherManifestDigest = new('d', 64);
    private static readonly string TeacherLayerDigest = new('c', 64);

    [Fact]
    public void Generator_GeneratedPreferenceMapsTeacherChosenAndLocalRejectedAuthors()
    {
        var pair = CreateTeacherPair("generated-preference");
        var artifactRootBinding = DesignConversionDpoGenerator.Sha256Ref(
            "design-conversion-generated-preference-artifact-root");
        var observedAtUtc = DateTimeOffset.UnixEpoch.AddDays(320);
        var receipt = DesignConversionIndependentPreferenceEvidenceTestIssuer.Issue(
            pair,
            artifactRootBinding,
            observedAtUtc,
            DesignConversionIndependentPreferenceEvidenceContract.GeneratedSide);

        var build = new DesignConversionDpoGenerator()
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                [pair],
                [receipt],
                artifactRootBinding,
                observedAtUtc);

        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Equal(2, build.Value!.AcceptedRecords.Count);
        Assert.Empty(build.Value.QuarantinedRecords);
        Assert.All(build.Value.AcceptedRecords, record =>
        {
            Assert.Equal(DpoPairSideAuthorKind.OpenTeacher, record.ChosenAuthorKind);
            Assert.Equal(DpoPairSideAuthorKind.LocalModel, record.RejectedAuthorKind);
            Assert.Equal(
                DesignConversionDpoPolicyCheckpointBridge.PairSourceSideGenerated,
                record.Provenance.Metadata[
                    DesignConversionDpoPolicyCheckpointBridge
                        .PairChosenSourceSideMetadataKey]);
            Assert.Equal(
                DesignConversionDpoPolicyCheckpointBridge.PairSourceSideIncumbent,
                record.Provenance.Metadata[
                    DesignConversionDpoPolicyCheckpointBridge
                        .PairRejectedSourceSideMetadataKey]);
        });
    }

    [Fact]
    public void Generator_IncumbentPreferenceReversesChosenAndRejectedAuthors()
    {
        var pair = CreateTeacherPair(
            "incumbent-preference",
            incumbentScore: 0.91,
            incumbentCanPersist: true,
            incumbentPassed: true,
            generatedScore: 0.31,
            generatedCanPersist: false,
            generatedPassed: false);
        var artifactRootBinding = DesignConversionDpoGenerator.Sha256Ref(
            "design-conversion-incumbent-preference-artifact-root");
        var observedAtUtc = DateTimeOffset.UnixEpoch.AddDays(321);
        var receipt = DesignConversionIndependentPreferenceEvidenceTestIssuer.Issue(
            pair,
            artifactRootBinding,
            observedAtUtc,
            DesignConversionIndependentPreferenceEvidenceContract.IncumbentSide);

        var build = new DesignConversionDpoGenerator()
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                [pair],
                [receipt],
                artifactRootBinding,
                observedAtUtc);

        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Equal(2, build.Value!.AcceptedRecords.Count);
        Assert.Empty(build.Value.QuarantinedRecords);
        Assert.All(build.Value.AcceptedRecords, record =>
        {
            Assert.Equal(DpoPairSideAuthorKind.LocalModel, record.ChosenAuthorKind);
            Assert.Equal(DpoPairSideAuthorKind.OpenTeacher, record.RejectedAuthorKind);
            Assert.Equal(
                DesignConversionDpoPolicyCheckpointBridge.PairSourceSideIncumbent,
                record.Provenance.Metadata[
                    DesignConversionDpoPolicyCheckpointBridge
                        .PairChosenSourceSideMetadataKey]);
            Assert.Equal(
                DesignConversionDpoPolicyCheckpointBridge.PairSourceSideGenerated,
                record.Provenance.Metadata[
                    DesignConversionDpoPolicyCheckpointBridge
                        .PairRejectedSourceSideMetadataKey]);
        });
    }

    [Fact]
    public void Generator_CandidateOnlyRowsStayQuarantinedAndCannotEnterTraining()
    {
        var pair = CreateTeacherPair("candidate-only", routeBound: true);
        var build = new DesignConversionDpoGenerator()
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                [pair],
                DateTimeOffset.UnixEpoch.AddDays(322));

        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.AcceptedRecords);
        var record = Assert.Single(build.Value.QuarantinedRecords);
        Assert.Contains(
            "teacher_candidate_only",
            record.Quarantine.Reason,
            StringComparison.Ordinal);
        Assert.Equal(DpoPairSideAuthorKind.OpenTeacher, record.ChosenAuthorKind);
        Assert.Equal(DpoPairSideAuthorKind.LocalModel, record.RejectedAuthorKind);

        var batch = DesignConversionDpoPolicyCheckpointBridge
            .BuildStandaloneBatchPreviewForTests(record, BuildConfig());

        Assert.False(batch.IsSuccess);
        Assert.Equal(
            DesignConversionDpoPolicyCheckpointBridge
                .PairAuthorshipMissingOrInvalidErrorCode,
            batch.ErrorCode);
    }

    [Fact]
    public void StandaloneTrainingBatch_CarriesExactRawAdmission()
    {
        var row = BuildOpenTeacherRecord("standalone-raw");
        var prompt = DesignConversionDpoPolicyCheckpointBridge
            .BuildPolicyPromptPreviewForTests(row);
        var preference = DesignConversionDpoPolicyCheckpointBridge
            .BuildPolicyPreferenceTextPreviewForTests(row);

        var batch = DesignConversionDpoPolicyCheckpointBridge
            .BuildStandaloneBatchPreviewForTests(row, BuildConfig());

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.NotNull(batch.Value!.Admission);
        AssertAdmission(
            batch.Value.Admission!,
            prompt,
            preference.Chosen,
            preference.Rejected,
            DpoPairSideAuthorKind.OpenTeacher,
            DpoPairSideAuthorKind.LocalModel);
    }

    [Fact]
    public void StandaloneTrainingBatch_DeterministicNoExternalTeacherUsesDeterministicAuthors()
    {
        var row = BuildDeterministicRecord("standalone-deterministic");
        var batch = DesignConversionDpoPolicyCheckpointBridge
            .BuildStandaloneBatchPreviewForTests(row, BuildConfig());

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.NotNull(batch.Value!.Admission);
        Assert.Equal(
            DpoPairSideAuthorKind.DeterministicGenerator,
            batch.Value.Admission!.ChosenAuthor);
        Assert.Equal(
            DpoPairSideAuthorKind.DeterministicGenerator,
            batch.Value.Admission.RejectedAuthor);
        Assert.False(batch.Value.Admission.HasPaidExternalModelSide);
    }

    [Fact]
    public void EvaluatedTrainingBatch_CarriesAdmissionButHeldOutDoesNot()
    {
        var row = BuildOpenTeacherRecord("evaluated-raw");
        var train = DesignConversionDpoPolicyCheckpointBridge
            .BuildEvaluatedPackedBatchPreviewForTests(
                row,
                BuildConfig(),
                includeTrainingVectorAndWeight: true);
        var heldOut = DesignConversionDpoPolicyCheckpointBridge
            .BuildEvaluatedPackedBatchPreviewForTests(
                row,
                BuildConfig(),
                includeTrainingVectorAndWeight: false);

        Assert.True(train.IsSuccess, train.ErrorMessage);
        Assert.True(heldOut.IsSuccess, heldOut.ErrorMessage);
        Assert.NotNull(train.Value!.VectorBatch.Batch.Admission);
        Assert.Null(heldOut.Value!.VectorBatch.Batch.Admission);
        AssertAdmission(
            train.Value.VectorBatch.Batch.Admission!,
            train.Value.Prompt,
            train.Value.Chosen,
            train.Value.Rejected,
            DpoPairSideAuthorKind.OpenTeacher,
            DpoPairSideAuthorKind.LocalModel);

        var validation = DesignConversionDpoPolicyCheckpointBridge
            .ValidateEvaluatedTrainPackedAdmissionsPreviewForTests([train.Value]);
        Assert.True(validation.IsSuccess, validation.ErrorMessage);
    }

    [Theory]
    [InlineData("train", "allows-training-false")]
    [InlineData("train", "contains-user-private-data")]
    [InlineData("train", "quarantined")]
    [InlineData("heldout", "allows-training-false")]
    [InlineData("heldout", "contains-user-private-data")]
    [InlineData("heldout", "quarantined")]
    public async Task EvaluatedComposite_TrainabilityFailuresStopBeforeOptimizerLeaseAndFileAuthority(
        string failingSplit,
        string trainabilityFailure)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"evaluated-trainability-{failingSplit}-{trainabilityFailure}-{Guid.NewGuid():N}");
        var output = Path.Combine(root, "output");
        var partition = BuildEvaluatedTrainabilityPartition(
            $"early-{failingSplit}-{trainabilityFailure}");
        partition = WithTrainabilityFailure(
            partition,
            failingSplit,
            trainabilityFailure);
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var legalTeacher = CreateEvaluatedLegalTeacherCapability();
        var options = new DesignConversionDpoPolicyCheckpointBridgeOptions(
            MaxBridgePreferenceCount: 1,
            DpoStepCount: 1);
        var enumeratedTeacherRows = new List<TrainingTeacherProvenance?>();
        var leaseStages = new List<string>();
        var previousLeaseObserver =
            DesignConversionDpoPolicyCheckpointBridge
                .LeaseAcquisitionObserverForTesting;

        try
        {
            DesignConversionDpoPolicyCheckpointBridge
                .LeaseAcquisitionObserverForTesting = leaseStages.Add;

            var prepared = await DesignConversionDpoPolicyCheckpointBridge
                .PrepareEvaluatedCompositeExecutionAsync(
                    legalTeacher,
                    partition,
                    provider,
                    options,
                    continuationRequest: null,
                    useContinuation: false,
                    CancellationToken.None,
                    enumeratedTeacherRows.Add);
            var executed = await DesignConversionDpoPolicyCheckpointBridge
                .ExecuteEvaluatedAndConvertAsync(
                    legalTeacher,
                    partition,
                    output,
                    $"early-{failingSplit}-{trainabilityFailure}",
                    Path.Combine(output, "runtime.json"),
                    provider,
                    options,
                    ct: CancellationToken.None,
                    legalTeacherEnumerationObserverForTesting:
                        enumeratedTeacherRows.Add);

            AssertEvaluatedTrainabilityEarlyFailure(prepared);
            AssertEvaluatedTrainabilityEarlyFailure(executed);
            Assert.Empty(enumeratedTeacherRows);
            Assert.Empty(leaseStages);
            Assert.Equal(0, provider.InvocationCount);
            Assert.Null(provider.LastModelSet);
            Assert.False(Directory.Exists(output));
        }
        finally
        {
            DesignConversionDpoPolicyCheckpointBridge
                .LeaseAcquisitionObserverForTesting = previousLeaseObserver;
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void RuntimeInferenceBatch_DoesNotCarryTrainerAdmission()
    {
        var config = BuildConfig();
        var binding = DesignConversionDpoPolicyCheckpointBridge
            .BuildRuntimeInferenceEncodingBindingPreviewForTests(
                config,
                evaluated: true);
        Assert.True(binding.IsSuccess, binding.ErrorMessage);

        var batch = DesignConversionDpoPolicyCheckpointBridge
            .BuildRuntimePolicyPairBatchForInference(
                "runtime prompt",
                "runtime chosen",
                "runtime rejected",
                binding.Value!,
                config);

        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        Assert.Null(batch.Value!.Admission);
    }

    [Fact]
    public void BridgeAdmission_MissingContradictoryOrUnknownAuthorshipFailsBeforeBatchCreation()
    {
        var missingRejected = BuildOpenTeacherRecord("missing-rejected") with
        {
            RejectedAuthorKind = null
        };
        var contradictoryDirection = BuildOpenTeacherRecord(
            "contradictory-direction",
            chosenSourceSide:
                DesignConversionDpoPolicyCheckpointBridge.PairSourceSideIncumbent,
            rejectedSourceSide:
                DesignConversionDpoPolicyCheckpointBridge.PairSourceSideGenerated);
        var unknownAuthor = BuildOpenTeacherRecord("unknown-author") with
        {
            ChosenAuthorKind = (DpoPairSideAuthorKind)999
        };

        AssertPairAuthorshipFailure(missingRejected);
        AssertPairAuthorshipFailure(contradictoryDirection);
        AssertPairAuthorshipFailure(unknownAuthor);
    }

    [Fact]
    public void BridgeAdmission_RemovedAdmissionFailsForStandaloneEvaluatedAndCapturedClone()
    {
        var row = BuildOpenTeacherRecord("removed-admission");
        var standalone = DesignConversionDpoPolicyCheckpointBridge
            .BuildStandaloneVectorBatchPreviewForTests(row, BuildConfig());
        var evaluated = DesignConversionDpoPolicyCheckpointBridge
            .BuildEvaluatedPackedBatchPreviewForTests(row, BuildConfig());
        var captured = DesignConversionDpoTrainingContentAuthority.CaptureRecord(row);

        Assert.True(standalone.IsSuccess, standalone.ErrorMessage);
        Assert.True(evaluated.IsSuccess, evaluated.ErrorMessage);
        Assert.True(captured.IsSuccess, captured.ErrorMessage);

        var standaloneWithoutAdmission = standalone.Value! with
        {
            Batch = standalone.Value.Batch with { Admission = null }
        };
        var evaluatedWithoutAdmission = evaluated.Value! with
        {
            VectorBatch = evaluated.Value.VectorBatch with
            {
                Batch = evaluated.Value.VectorBatch.Batch with { Admission = null }
            }
        };
        var cloneBatch = DesignConversionDpoPolicyCheckpointBridge
            .BuildStandaloneVectorBatchPreviewForTests(captured.Value!, BuildConfig());
        Assert.True(cloneBatch.IsSuccess, cloneBatch.ErrorMessage);
        var cloneWithoutAdmission = cloneBatch.Value! with
        {
            Batch = cloneBatch.Value.Batch with { Admission = null }
        };

        AssertAdmissionValidationFailure(
            DesignConversionDpoPolicyCheckpointBridge
                .ValidateStandaloneVectorBatchAdmissionPreviewForTests(
                    row,
                    standaloneWithoutAdmission));
        AssertAdmissionValidationFailure(
            DesignConversionDpoPolicyCheckpointBridge
                .ValidateEvaluatedTrainPackedAdmissionsPreviewForTests(
                    [evaluatedWithoutAdmission]));
        AssertAdmissionValidationFailure(
            DesignConversionDpoPolicyCheckpointBridge
                .ValidateStandaloneVectorBatchAdmissionPreviewForTests(
                    captured.Value!,
                    cloneWithoutAdmission));
    }

    [Fact]
    public void BridgeAdmission_RawTextMutationFailsBeforeFileAuthority()
    {
        var row = BuildOpenTeacherRecord("raw-mutation");
        var standalone = DesignConversionDpoPolicyCheckpointBridge
            .BuildStandaloneVectorBatchPreviewForTests(row, BuildConfig());
        var evaluated = DesignConversionDpoPolicyCheckpointBridge
            .BuildEvaluatedPackedBatchPreviewForTests(row, BuildConfig());

        Assert.True(standalone.IsSuccess, standalone.ErrorMessage);
        Assert.True(evaluated.IsSuccess, evaluated.ErrorMessage);

        var mutatedRow = row with
        {
            Chosen = row.Chosen with
            {
                Html = row.Chosen.Html + "<span data-mutation=\"raw\">x</span>"
            }
        };
        var mutatedEvaluated = evaluated.Value! with
        {
            // Raw-текст меняется после выдачи паспорта: валидатор обязан
            // сверять fingerprint с текущим Prompt/Chosen/Rejected, не с tensor ids.
            Chosen = evaluated.Value.Chosen + "\nmutation"
        };

        AssertAdmissionValidationFailure(
            DesignConversionDpoPolicyCheckpointBridge
                .ValidateStandaloneVectorBatchAdmissionPreviewForTests(
                    mutatedRow,
                    standalone.Value!));
        AssertAdmissionValidationFailure(
            DesignConversionDpoPolicyCheckpointBridge
                .ValidateEvaluatedTrainPackedAdmissionsPreviewForTests(
                    [mutatedEvaluated]));
    }

    [Fact]
    public void TrainingContentCapture_PreservesPairAuthorsAndRejectsUnknownEnum()
    {
        var row = BuildOpenTeacherRecord("capture");
        var captured = DesignConversionDpoTrainingContentAuthority.CaptureRecord(row);
        var unknown = row with
        {
            RejectedAuthorKind = (DpoPairSideAuthorKind)999
        };

        Assert.True(captured.IsSuccess, captured.ErrorMessage);
        Assert.Equal(row.ChosenAuthorKind, captured.Value!.ChosenAuthorKind);
        Assert.Equal(row.RejectedAuthorKind, captured.Value.RejectedAuthorKind);
        Assert.True(
            DesignConversionDpoTrainingContentAuthority.ValidateBeforeSnapshot(row));
        Assert.False(
            DesignConversionDpoTrainingContentAuthority.ValidateBeforeSnapshot(unknown));
        Assert.False(
            DesignConversionDpoTrainingContentAuthority.CaptureRecord(unknown)
                .IsSuccess);
    }

    [Fact]
    public void TrainingRecordJson_RoundTripsPairAuthors()
    {
        var row = BuildOpenTeacherRecord("json-roundtrip");

        var json = JsonSerializer.Serialize(
            row,
            DesignConversionCheckpointJsonOptions.Standard);
        using var document = JsonDocument.Parse(json);
        var clone = JsonSerializer.Deserialize<DesignConversionDpoTrainingRecord>(
            json,
            DesignConversionCheckpointJsonOptions.Standard);

        Assert.Equal(
            (int)DpoPairSideAuthorKind.OpenTeacher,
            document.RootElement.GetProperty("chosenAuthorKind").GetInt32());
        Assert.Equal(
            (int)DpoPairSideAuthorKind.LocalModel,
            document.RootElement.GetProperty("rejectedAuthorKind").GetInt32());
        Assert.NotNull(clone);
        Assert.Equal(DpoPairSideAuthorKind.OpenTeacher, clone!.ChosenAuthorKind);
        Assert.Equal(DpoPairSideAuthorKind.LocalModel, clone.RejectedAuthorKind);
    }

    private static void AssertPairAuthorshipFailure(
        DesignConversionDpoTrainingRecord row)
    {
        var batch = DesignConversionDpoPolicyCheckpointBridge
            .BuildStandaloneBatchPreviewForTests(row, BuildConfig());
        var evaluated = DesignConversionDpoPolicyCheckpointBridge
            .BuildEvaluatedPackedBatchPreviewForTests(row, BuildConfig());

        Assert.False(batch.IsSuccess);
        Assert.Equal(
            DesignConversionDpoPolicyCheckpointBridge
                .PairAuthorshipMissingOrInvalidErrorCode,
            batch.ErrorCode);
        Assert.False(evaluated.IsSuccess);
        Assert.Equal(
            DesignConversionDpoPolicyCheckpointBridge
                .PairAuthorshipMissingOrInvalidErrorCode,
            evaluated.ErrorCode);
    }

    private static void AssertAdmissionValidationFailure(
        OperationResult<bool> validation)
    {
        Assert.False(validation.IsSuccess);
        Assert.Equal(
            DesignConversionDpoPolicyCheckpointBridge
                .PairAuthorshipMissingOrInvalidErrorCode,
            validation.ErrorCode);
    }

    private static void AssertEvaluatedTrainabilityEarlyFailure<T>(
        OperationResult<T> result)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            DesignConversionDpoPolicyCheckpointBridge
                .EvaluatedPackingInvalidErrorCode,
            result.ErrorCode);
    }

    private static void AssertAdmission(
        DpoTrainerPairAdmission admission,
        string prompt,
        string chosen,
        string rejected,
        DpoPairSideAuthorKind chosenAuthor,
        DpoPairSideAuthorKind rejectedAuthor)
    {
        var expected = DpoTrainerPairCustoms.CreateAdmitted(
                prompt,
                chosen,
                rejected,
                chosenAuthor,
                rejectedAuthor,
                nameof(DesignConversionDpoPolicyCheckpointBridge))
            .Admission;

        Assert.Equal(chosenAuthor, admission.ChosenAuthor);
        Assert.Equal(rejectedAuthor, admission.RejectedAuthor);
        Assert.Equal(
            nameof(DesignConversionDpoPolicyCheckpointBridge),
            admission.IssuerName);
        Assert.Equal(
            expected.ContentFingerprintSha256,
            admission.ContentFingerprintSha256);
    }

    private static DesignConversionDpoTrainingRecord BuildOpenTeacherRecord(
        string suffix,
        string chosenSourceSide =
            DesignConversionDpoPolicyCheckpointBridge.PairSourceSideGenerated,
        string rejectedSourceSide =
            DesignConversionDpoPolicyCheckpointBridge.PairSourceSideIncumbent)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DesignConversionDpoPolicyCheckpointBridge
                .PairAuthorshipSourceMetadataKey] =
                "independent_preference_receipt",
            [DesignConversionDpoPolicyCheckpointBridge
                .PairChosenSourceSideMetadataKey] =
                chosenSourceSide,
            [DesignConversionDpoPolicyCheckpointBridge
                .PairRejectedSourceSideMetadataKey] =
                rejectedSourceSide,
            [DesignConversionDpoPolicyCheckpointBridge
                .PairChosenAuthorKindMetadataKey] =
                DpoPairSideAuthorKind.OpenTeacher.ToString(),
            [DesignConversionDpoPolicyCheckpointBridge
                .PairRejectedAuthorKindMetadataKey] =
                DpoPairSideAuthorKind.LocalModel.ToString()
        };
        return BuildRecord(
            suffix,
            ValidAuthoringProvenance("open-teacher-record-" + suffix),
            metadata) with
        {
            ChosenAuthorKind = DpoPairSideAuthorKind.OpenTeacher,
            RejectedAuthorKind = DpoPairSideAuthorKind.LocalModel
        };
    }

    private static DesignConversionDpoResolvedPartition
        BuildEvaluatedTrainabilityPartition(string id)
        => RebindEvaluatedTrainabilityPartition(new DesignConversionDpoResolvedPartition(
            id,
            [BuildOpenTeacherRecord($"{id}-train")],
            [BuildOpenTeacherRecord($"{id}-heldout")],
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty));

    private static DesignConversionDpoResolvedPartition WithTrainabilityFailure(
        DesignConversionDpoResolvedPartition partition,
        string failingSplit,
        string trainabilityFailure)
    {
        var train = partition.TrainRecords.ToArray();
        var heldOut = partition.HeldOutRecords.ToArray();
        if (string.Equals(failingSplit, "train", StringComparison.Ordinal))
        {
            train[0] = MutateTrainability(train[0], trainabilityFailure);
        }
        else if (string.Equals(failingSplit, "heldout", StringComparison.Ordinal))
        {
            heldOut[0] = MutateTrainability(heldOut[0], trainabilityFailure);
        }
        else
        {
            throw new InvalidOperationException("Unknown evaluated split.");
        }

        return RebindEvaluatedTrainabilityPartition(partition with
        {
            TrainRecords = train,
            HeldOutRecords = heldOut
        });
    }

    private static DesignConversionDpoTrainingRecord MutateTrainability(
        DesignConversionDpoTrainingRecord row,
        string trainabilityFailure)
        => trainabilityFailure switch
        {
            "allows-training-false" => row with
            {
                License = row.License with { AllowsTraining = false }
            },
            "contains-user-private-data" => row with
            {
                License = row.License with { ContainsUserPrivateData = true }
            },
            "quarantined" => row with
            {
                Quarantine = row.Quarantine with
                {
                    IsQuarantined = true,
                    Reason = "evaluated trainability regression fixture"
                }
            },
            _ => throw new InvalidOperationException(
                "Unknown trainability regression fixture.")
        };

    private static DesignConversionDpoResolvedPartition
        RebindEvaluatedTrainabilityPartition(
            DesignConversionDpoResolvedPartition partition)
    {
        Assert.True(
            DesignConversionDpoConsumedPartitionProjectionContract.TryCreate(
                partition,
                maximumTrainRecords: 1,
                out var projection));
        Assert.NotNull(projection);
        return partition with
        {
            TrainRecordSetSha256 = projection!.TrainRecordSetSha256,
            HeldOutRecordSetSha256 = projection.HeldOutRecordSetSha256,
            TrainSourceSetSha256 = projection.TrainSourceSetSha256,
            HeldOutSourceSetSha256 = projection.HeldOutSourceSetSha256,
            PartitionSnapshotSha256 = projection.PartitionSnapshotSha256
        };
    }

    private static DesignConversionDpoTrainingRecord BuildDeterministicRecord(
        string suffix)
        => BuildRecord(
            suffix,
            TrainingTeacherProvenance.Deterministic(
                "deterministic-record-" + suffix),
            new Dictionary<string, string>(StringComparer.Ordinal));

    private static DesignConversionDpoTrainingRecord BuildRecord(
        string suffix,
        TrainingTeacherProvenance teacherProvenance,
        IReadOnlyDictionary<string, string> metadata)
    {
        var chosen = BuildOutput(
            $"{suffix}-chosen-output",
            $"{suffix}-chosen-candidate",
            $"<main data-side=\"chosen-{suffix}\">Chosen</main>",
            $"[data-side=\"chosen-{suffix}\"]{{display:grid;gap:18px;}}",
            score: 0.96,
            passed: true,
            accepted: true);
        var rejected = BuildOutput(
            $"{suffix}-rejected-output",
            $"{suffix}-rejected-candidate",
            $"<main data-side=\"rejected-{suffix}\">Rejected</main>",
            $"[data-side=\"rejected-{suffix}\"]{{display:block;gap:1px;}}",
            score: 0.21,
            passed: false,
            accepted: false);

        return new DesignConversionDpoTrainingRecord(
            RecordId: $"design-conversion-pair-admission-{suffix}",
            UnitId: DesignConversionDpoUnits.CriticUnit,
            InternalUnitStatePayload: JsonSerializer.Serialize(new
            {
                unit = DesignConversionDpoUnits.CriticUnit,
                suffix
            }),
            InternalVectorPayload: [0.1, -0.2, 0.3],
            SourceStructurePayload: JsonSerializer.Serialize(new
            {
                kind = "frame",
                suffix
            }),
            Chosen: chosen,
            Rejected: rejected,
            TieBreakerPayload: null,
            UsefulnessScore: 0.87,
            NegativeQualityReason: "measured spacing regression",
            NegativeReasonTaxonomy: "layout",
            Provenance: new DesignConversionDpoProvenance(
                SourceRequestId:
                    $"design-conversion-pair-admission-{suffix}-request",
                SourceKind: nameof(DesignConversionSourceKind.Figma),
                SourceManifestRef:
                    $"artifact://design-conversion/{suffix}/manifest.json",
                ReviewerRef: "reviewer:redacted",
                CreatedAtUtc: DateTimeOffset.UnixEpoch.AddDays(300),
                Metadata: new Dictionary<string, string>(
                    metadata,
                    StringComparer.Ordinal)),
            License: new DesignConversionLicenseScope(
                LicenseId: $"design-conversion-pair-admission-{suffix}-license",
                RightsScope: "project-local",
                AllowsTraining: true,
                ContainsUserPrivateData: false,
                Metadata: new Dictionary<string, string>()),
            Quarantine: new DesignConversionQuarantineState(
                IsQuarantined: false,
                Reason: string.Empty,
                EvidenceRefs: []),
            TeacherProvenance: teacherProvenance);
    }

    private static ServerOwnedLegalTrainingTeacherAdmissionAuthority
        CreateEvaluatedLegalTeacherCapability()
        => DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted();

    private static DesignConversionImprovementOutput BuildOutput(
        string outputId,
        string candidateId,
        string html,
        string css,
        double score,
        bool passed,
        bool accepted)
        => new(
            outputId,
            candidateId,
            html,
            css,
            new VisualVerdict(
                $"{outputId}-verdict",
                score,
                [
                    new VisualDefect(
                        $"{outputId}-defect",
                        DesignConversionNegativeReasonTaxonomy.Spacing,
                        Severity: "medium",
                        Description: "Spacing mismatch",
                        X: 1,
                        Y: 2,
                        Width: 3,
                        Height: 4,
                        EvidenceImageIds: [$"{outputId}-image"])
                ],
                [$"{outputId}-evidence"],
                passed),
            QaResults: [],
            EvidenceRefs: [$"{outputId}-evidence"],
            Accepted: accepted);

    private static DesignConversionTeacherAuthoredEvaluationPair CreateTeacherPair(
        string suffix,
        bool routeBound = false,
        double incumbentScore = 0.42,
        bool incumbentCanPersist = false,
        bool incumbentPassed = false,
        double generatedScore = 0.91,
        bool generatedCanPersist = true,
        bool generatedPassed = true)
    {
        var incumbent = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: incumbentCanPersist,
            requestProvenanceOverride: routeBound
                ? BuildRouteAdmissionProvenance(suffix)
                : null,
            jobId: $"pair-admission-{suffix}-incumbent-job",
            requestId: $"pair-admission-{suffix}-incumbent-request",
            outputId: $"pair-admission-{suffix}-incumbent-output",
            candidateId: $"pair-admission-{suffix}-incumbent-candidate",
            verdictId: $"pair-admission-{suffix}-incumbent-verdict",
            score: incumbentScore,
            passed: incumbentPassed,
            htmlSuffix: $"incumbent-{suffix}");
        var request = CreateUpdateRequest(incumbent);
        var candidateHtml =
            $"<main data-teacher=\"{suffix}\">Teacher candidate</main>";
        var candidateCss =
            $"[data-teacher=\"{suffix}\"]{{display:grid;gap:18px;}}";
        var output = JsonSerializer.Serialize(new
        {
            schema = DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
            incumbent_ref = DesignConversionDpoLeakageGuard.HashRef(
                "dcm-dpo-teacher-incumbent",
                DesignConversionDpoTeacherAttestationRuntime
                    .BuildIncumbentBinding(request.CurrentRequest)),
            candidate_html = candidateHtml,
            candidate_css = candidateCss
        });
        var authored = DesignConversionDpoTeacherAttestationRuntime
            .ParseAuthoringOutputForTesting(
                output,
                request,
                ValidAuthoringProvenance("authoring-request-" + suffix));
        Assert.True(authored.IsSuccess, authored.ErrorMessage);

        var generated = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: generatedCanPersist,
            requestProvenanceOverride: routeBound
                ? BuildRouteAdmissionProvenance(suffix)
                : null,
            jobId: $"pair-admission-{suffix}-generated-job",
            requestId: authored.Value!.Candidate.RequestId,
            outputId: $"pair-admission-{suffix}-generated-output",
            candidateId: authored.Value.Candidate.CandidateId,
            verdictId: $"pair-admission-{suffix}-generated-verdict",
            score: generatedScore,
            passed: generatedPassed,
            htmlSuffix: $"generated-{suffix}");
        var submitted = generated.SubmittedRequest with
        {
            RequestId = authored.Value.Candidate.RequestId,
            Html = authored.Value.Candidate.Html,
            Css = authored.Value.Candidate.Css
        };
        generated = generated with
        {
            SubmittedRequest = submitted,
            RuntimeFlowResult = generated.RuntimeFlowResult with
            {
                Request = submitted,
                Candidate = authored.Value.Candidate,
                Output = generated.RuntimeFlowResult.Output with
                {
                    CandidateId = authored.Value.Candidate.CandidateId
                }
            }
        };

        return new DesignConversionTeacherAuthoredEvaluationPair(
            AttemptNumber: 2,
            GeneratedEvaluation: generated,
            IncumbentEvaluation: incumbent,
            AuthoringEvidence: authored.Value.AuthoringEvidence!,
            GeneratedCandidateId: authored.Value.Candidate.CandidateId);
    }

    private static DesignConversionCandidateUpdateRequest CreateUpdateRequest(
        DesignConversionInternalEvaluationResult incumbent)
    {
        var submitted = incumbent.SubmittedRequest;
        return new DesignConversionCandidateUpdateRequest(
            LoopId: "design-conversion-pair-admission-loop",
            CompletedAttemptNumber: 1,
            CurrentRequest: new DesignConversionInternalEvaluateRequest(
                incumbent.JobId,
                submitted.RequestId,
                submitted.SourceKind,
                submitted.Html,
                submitted.Css,
                submitted.DesignTreePayload,
                submitted.OriginalTreePayload,
                submitted.Screenshots,
                submitted.DesignRenders,
                submitted.Budget,
                RenderResolutions: [],
                AiTeacherEvidenceRefs: [],
                DeterministicEvidenceRefs: new Dictionary<string, string>(),
                submitted.Provenance),
            CurrentEvaluation: incumbent,
            TierPolicy: incumbent.VisualQaBoundaryTrace.TierPolicy,
            AttemptHistory: []);
    }

    private static IReadOnlyDictionary<string, string> BuildRouteAdmissionProvenance(
        string suffix)
    {
        var tenantIdRef = DesignConversionModelActivationRegistry.BuildTenantIdRef(
            "design-conversion-pair-admission-" + suffix);
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["source_manifest_ref"] = "manifest://runtime-source",
            [DesignConversionThinClientJobRuntime
                .ServerOwnedRouteAdmissionTenantBoundMetadataKey] =
                bool.TrueString,
            [DesignConversionThinClientJobRuntime
                .ServerOwnedRouteAdmissionTenantIdRefMetadataKey] =
                tenantIdRef,
            [DesignConversionThinClientJobRuntime
                .ServerOwnedRouteAdmissionScopeRefMetadataKey] =
                BuildRouteAdmissionScopeRef(
                    "design-conversion-live-route\n" + tenantIdRef)
        };
    }

    private static string BuildRouteAdmissionScopeRef(string source)
        => "sha256:design-conversion-route-admission-scope:" +
           Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source)))
               .ToLowerInvariant();

    private static TrainingTeacherProvenance ValidAuthoringProvenance(
        string requestId)
        => new(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            TeacherModelId,
            TeacherManifestDigest,
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            RequestId: requestId,
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                LegalTrainingTeacherAdmissionAuthority
                    .OllamaModelLayerSha256DigestKind,
            ModelArtifactDigest = TeacherLayerDigest
        };

    private static ModelConfig BuildConfig()
        => new()
        {
            VocabSize = 256,
            ContextLen = 128,
            HiddenDim = 16,
            NumLayers = 1,
            NumHeads = 4,
            NumKvHeads = 2,
            FfnHiddenDim = 32
        };
}
