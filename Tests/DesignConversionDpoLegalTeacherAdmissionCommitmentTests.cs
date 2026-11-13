// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Text.Json;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;
using Xunit;

public sealed class DesignConversionDpoLegalTeacherAdmissionCommitmentTests
{
    private const string ManifestA =
        "1111111111111111111111111111111111111111111111111111111111111111";
    private const string ManifestB =
        "2222222222222222222222222222222222222222222222222222222222222222";
    private const string LayerA =
        "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string LayerB =
        "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    private const string EvidenceA =
        "sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string EvidenceB =
        "sha256:bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

    [Fact]
    public void SameAdmission_InDifferentRowOrder_ProducesSameCommitment()
    {
        var authority = CreateAuthority(ManifestA, LayerA);
        var first = External("request-b", ManifestA, LayerA);
        var second = External("request-a", ManifestA, LayerA);

        var forward = AdmitExact(authority, [first, second]);
        var reverse = forward with
        {
            Receipts = forward.Receipts.Reverse().ToArray(),
            EvidenceRefs = forward.EvidenceRefs.Reverse().ToArray()
        };

        var forwardCommitment = Finalize(forward, ["checkpoint:b", "checkpoint:a"]);
        var reverseCommitment = Finalize(reverse, ["checkpoint:a", "checkpoint:b"]);
        Assert.Equal(forwardCommitment, reverseCommitment);
        Assert.True(DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.Validate(forwardCommitment));
        Assert.Equal(
            JsonSerializer.Serialize(
                forwardCommitment,
                DesignConversionCheckpointJsonOptions.Standard),
            JsonSerializer.Serialize(
                reverseCommitment,
                DesignConversionCheckpointJsonOptions.Standard));
    }

    [Fact]
    public void ManifestOrModelLayerCommitmentChange_ChangesReceiptAndCommitmentRoots()
    {
        var firstAdmission = AdmitExact(
            CreateAuthority(ManifestA, LayerA),
            [External("request-a", ManifestA, LayerA)]);
        var manifestChanged = AdmitExact(
            CreateAuthority(ManifestB, LayerA),
            [External("request-a", ManifestB, LayerA)]);
        var layerChanged = AdmitExact(
            CreateAuthority(ManifestA, LayerB),
            [External("request-a", ManifestA, LayerB)]);

        var first = Finalize(firstAdmission, ["checkpoint:a"]);
        var second = Finalize(manifestChanged, ["checkpoint:a"]);
        var third = Finalize(layerChanged, ["checkpoint:a"]);

        Assert.NotEqual(first.ReceiptSetSha256, second.ReceiptSetSha256);
        Assert.NotEqual(first.ReceiptSetSha256, third.ReceiptSetSha256);
        Assert.NotEqual(first.CommitmentSha256, second.CommitmentSha256);
        Assert.NotEqual(first.CommitmentSha256, third.CommitmentSha256);
    }

    [Fact]
    public void DeterministicAdmission_PreservesExplicitZeroInvocationCount()
    {
        var authority = LegalTrainingTeacherAdmissionAuthority.Create(
            [],
            maxTeacherCalls: 0,
            Enumerable.Repeat((byte)7, 32).ToArray()).Value!;
        var admission = AdmitExact(
            authority,
            [TrainingTeacherProvenance.Deterministic("deterministic-request")]);

        var commitment = Finalize(admission, ["checkpoint:deterministic"]);
        Assert.Equal(0, commitment.ExternalTeacherInvocationCount);
        Assert.Equal(1, commitment.ReceiptCount);
        Assert.True(DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.Validate(commitment));
    }

    [Fact]
    public void AdmissionWithoutExactTrainingContentBatchRef_IsRejected()
    {
        var authority = CreateAuthority(ManifestA, LayerA);
        var unbound = authority.ValidateBatch([
            External("unbound-request", ManifestA, LayerA)
        ]);

        Assert.True(unbound.IsSuccess, unbound.ErrorMessage);
        Assert.Equal(string.Empty, unbound.Value!.TrainingContentBatchRef);
        var result = DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
            .CaptureAdmission(unbound.Value);
        Assert.False(result.IsSuccess);
        Assert.Equal(
            DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.InvalidAdmissionErrorCode,
            result.ErrorCode);
    }

    [Fact]
    public void ExactAdmission_RejectsBlankOrWrongTrainingContentBinding()
    {
        var authority = CreateAuthority(ManifestA, LayerA);
        var content = ProjectExactContent(rowCount: 1, role: "binding-negative");
        var provenance = External("binding-request", ManifestA, LayerA);

        var blank = authority.ValidateBatch(content, [provenance]);
        var wrong = authority.ValidateBatch(content, [provenance with
        {
            TrainingContentSha256 = new string('f', 64)
        }]);

        Assert.False(blank.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.TrainingContentInvalidErrorCode,
            blank.ErrorCode);
        Assert.False(wrong.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.TrainingContentInvalidErrorCode,
            wrong.ErrorCode);
    }

    [Fact]
    public void CompositeAdmission_RequiresDistinctExactContentForTrainAndHeldout()
    {
        var authority = CreateAuthority(ManifestA, LayerA);
        var train = AdmitExact(
            authority,
            [External("shared-request", ManifestA, LayerA)],
            contentRole: "train");
        var heldout = AdmitExact(
            authority,
            [External("shared-request", ManifestA, LayerA)],
            contentRole: "heldout-evaluation");

        Assert.Equal(train.PolicyIdentityRef, heldout.PolicyIdentityRef);
        Assert.NotEqual(train.TrainingContentBatchRef, heldout.TrainingContentBatchRef);
        Assert.NotEqual(
            Assert.Single(train.Receipts).TrainingContentRef,
            Assert.Single(heldout.Receipts).TrainingContentRef);
        var accepted = DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
            .CaptureAdmissions(train, heldout);
        var sameContent = DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
            .CaptureAdmissions(train, train);

        Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
        Assert.False(sameContent.IsSuccess);
        Assert.Equal(
            DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.InvalidAdmissionErrorCode,
            sameContent.ErrorCode);
    }

    [Fact]
    public void CompositeAdmission_SwappedRolesCannotValidateAsOriginalCommitment()
    {
        var authority = CreateAuthority(ManifestA, LayerA);
        var train = AdmitExact(
            authority,
            [External("shared-request", ManifestA, LayerA)],
            contentRole: "train");
        var heldout = AdmitExact(
            authority,
            [External("shared-request", ManifestA, LayerA)],
            contentRole: "heldout-evaluation");
        var forward = DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
            .CaptureAdmissions(train, heldout);
        var swapped = DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
            .CaptureAdmissions(heldout, train);

        Assert.True(forward.IsSuccess, forward.ErrorMessage);
        Assert.True(swapped.IsSuccess, swapped.ErrorMessage);
        var forwardCommitment = Finalize(forward.Value!, ["checkpoint:composite"]);
        var swappedCommitment = Finalize(swapped.Value!, ["checkpoint:composite"]);
        Assert.NotEqual(
            forwardCommitment.ReceiptSetSha256,
            swappedCommitment.ReceiptSetSha256);
        Assert.NotEqual(
            forwardCommitment.AdmissionEvidenceSetSha256,
            swappedCommitment.AdmissionEvidenceSetSha256);
        Assert.NotEqual(
            forwardCommitment.CommitmentSha256,
            swappedCommitment.CommitmentSha256);
        Assert.False(DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.Validate(
            forwardCommitment with
            {
                ReceiptSetSha256 = swappedCommitment.ReceiptSetSha256,
                AdmissionEvidenceSetSha256 =
                    swappedCommitment.AdmissionEvidenceSetSha256
            }));
    }

    [Fact]
    public void CommitmentJson_DoesNotPersistRawTeacherOrAbsolutePathSentinels()
    {
        const string rawModel = "qwen-raw-model-sentinel";
        const string rawRequest = "C:\\private\\raw-request-sentinel";
        var authority = CreateAuthority(ManifestA, LayerA, rawModel);
        var admission = AdmitExact(
            authority,
            [External(rawRequest, ManifestA, LayerA, rawModel)]);

        var json = JsonSerializer.Serialize(
            Finalize(admission, ["checkpoint:safe-marker"]),
            DesignConversionCheckpointJsonOptions.Standard);

        Assert.DoesNotContain(rawModel, json, StringComparison.Ordinal);
        Assert.DoesNotContain(rawRequest, json, StringComparison.Ordinal);
        Assert.DoesNotContain(ManifestA, json, StringComparison.Ordinal);
        Assert.DoesNotContain(LayerA, json, StringComparison.Ordinal);
        Assert.DoesNotContain("C:\\private", json, StringComparison.Ordinal);
        Assert.Contains("\"rawIdentityPersisted\": false", json, StringComparison.Ordinal);
        Assert.Contains("\"absolutePathPersisted\": false", json, StringComparison.Ordinal);
    }

    [Fact]
    public void DuplicateReceiptOrEvidence_IsRejected()
    {
        var admission = AdmitExact(
            CreateAuthority(ManifestA, LayerA),
            [External("request-a", ManifestA, LayerA)]);
        var duplicateReceipt = admission with
        {
            Receipts = new[] { admission.Receipts[0], admission.Receipts[0] }
        };
        var duplicateAdmissionEvidence = admission with
        {
            EvidenceRefs = new[] { admission.EvidenceRefs[0], admission.EvidenceRefs[0] }
        };

        var receiptResult =
            DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.CaptureAdmission(
                duplicateReceipt);
        var evidenceResult =
            DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.CaptureAdmission(
                duplicateAdmissionEvidence);
        var core = DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
            .CaptureAdmission(admission).Value!;
        var finalEvidenceResult =
            DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.FinalizeCommitment(
                core,
                ["checkpoint:a", "checkpoint:a"]);

        Assert.False(receiptResult.IsSuccess);
        Assert.Equal(
            DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.DuplicateReceiptErrorCode,
            receiptResult.ErrorCode);
        Assert.False(evidenceResult.IsSuccess);
        Assert.Equal(
            DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.DuplicateEvidenceErrorCode,
            evidenceResult.ErrorCode);
        Assert.False(finalEvidenceResult.IsSuccess);
        Assert.Equal(
            DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.DuplicateEvidenceErrorCode,
            finalEvidenceResult.ErrorCode);
    }

    [Fact]
    public void CommitmentOrFinalEvidenceMutation_IsRejected()
    {
        var admission = AdmitExact(
            CreateAuthority(ManifestA, LayerA),
            [External("request-a", ManifestA, LayerA)]);
        var commitment = Finalize(admission, ["checkpoint:a"]);

        Assert.False(DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.Validate(
            commitment with { ReceiptCount = commitment.ReceiptCount + 1 }));
        Assert.False(
            DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.MatchesCheckpointEvidence(
                commitment,
                ["checkpoint:b"]));
        Assert.False(
            DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.MatchesCheckpointEvidence(
                commitment,
                []));
        Assert.False(
            DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.MatchesCheckpointEvidence(
                commitment,
                ["checkpoint:a", "checkpoint:added"]));
        Assert.False(
            DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
                .MatchesAdmissionAndCheckpoint(
                    commitment,
                    admission with { ExternalTeacherInvocationCount = 0 },
                    ["checkpoint:a"]));
        Assert.False(DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.CaptureAdmission(
            admission with { ExternalTeacherInvocationCount = 0 }).IsSuccess);
    }

    [Fact]
    public void CraftedDeterministicReceiptWithExternalMetadata_IsRejected()
    {
        var authority = LegalTrainingTeacherAdmissionAuthority.Create(
            [], 0, Enumerable.Repeat((byte)4, 32).ToArray()).Value!;
        var admission = AdmitExact(
            authority,
            [TrainingTeacherProvenance.Deterministic("deterministic")]);
        var crafted = admission with
        {
            Receipts = [admission.Receipts[0] with
            {
                Family = OpenWeightTeacherFamily.Qwen
            }]
        };

        Assert.False(DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
            .CaptureAdmission(crafted).IsSuccess);
    }

    [Fact]
    public void CraftedLocalReceiptWithHostedTermsOrWrongEvidenceMode_IsRejected()
    {
        var admission = AdmitExact(
            CreateAuthority(ManifestA, LayerA),
            [External("request", ManifestA, LayerA)]);
        var receipt = admission.Receipts[0];

        Assert.False(DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
            .CaptureAdmission(admission with
            {
                Receipts = [receipt with { ProviderTermsChecked = true }]
            }).IsSuccess);
        Assert.False(DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
            .CaptureAdmission(admission with
            {
                Receipts = [receipt with
                {
                    LegalEvidenceMode = LegalTrainingTeacherAdmissionAuthority
                        .HostedProviderTermsEvidenceMode
                }]
            }).IsSuccess);
    }

    [Fact]
    public void CraftedHostedReceiptWithoutApprovedTermsOrWithArtifactRef_IsRejected()
    {
        var authority = CreateHostedAuthority();
        var admission = AdmitExact(authority, [Hosted("hosted-request")]);
        var receipt = admission.Receipts[0];

        Assert.False(DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
            .CaptureAdmission(admission with
            {
                Receipts = [receipt with { ProviderTermsAllowTraining = false }]
            }).IsSuccess);
        Assert.False(DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
            .CaptureAdmission(admission with
            {
                Receipts = [receipt with { ModelArtifactRef = EvidenceA }]
            }).IsSuccess);
    }

    [Fact]
    public void ExternalReceiptWithoutLegalRefs_IsRejected()
    {
        var admission = AdmitExact(
            CreateAuthority(ManifestA, LayerA),
            [External("request", ManifestA, LayerA)]);
        var receipt = admission.Receipts[0];
        Assert.False(DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
            .CaptureAdmission(admission with
            {
                Receipts = [receipt with { LocalRetentionPolicyRef = null }]
            }).IsSuccess);
        Assert.False(DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
            .CaptureAdmission(admission with
            {
                Receipts = [receipt with { OutputLicenseRef = null }]
            }).IsSuccess);
    }

    [Fact]
    public void DistinctReceiptsSharingRequestRef_AreRejected()
    {
        var admission = AdmitExact(
            CreateAuthority(ManifestA, LayerA),
            [
                External("request-a", ManifestA, LayerA),
                External("request-b", ManifestA, LayerA)
            ]);
        var receipts = admission.Receipts.ToArray();
        receipts[1] = receipts[1] with { RequestRef = receipts[0].RequestRef };
        var result = DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
            .CaptureAdmission(admission with { Receipts = receipts });
        Assert.False(result.IsSuccess);
        Assert.Equal(
            DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.DuplicateReceiptErrorCode,
            result.ErrorCode);
    }

    [Fact]
    public void AggregateAdmissionEvidenceBeyondBound_IsRejected()
    {
        var admission = AdmitExact(
            CreateAuthority(ManifestA, LayerA),
            [External("request", ManifestA, LayerA)]);
        var oversized = admission with
        {
            EvidenceRefs = Enumerable.Range(0, 3_000)
                .Select(index => $"evidence:{index:D4}:{new string('x', 3_000)}")
                .ToArray()
        };

        var result = DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
            .CaptureAdmission(oversized);
        Assert.False(result.IsSuccess);
        Assert.Equal(
            DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.LimitExceededErrorCode,
            result.ErrorCode);
    }

    [Fact]
    public void LegacyTrainingContextJsonAndDeconstructRemainUnchanged()
    {
        var context = new DesignConversionDpoPolicyCheckpointTrainingContext(
            "legacy", 2, 3, 4, 1, 1, 1, 8, false, false, 2);
        var json = JsonSerializer.Serialize(
            context,
            DesignConversionCheckpointJsonOptions.Standard);
        Assert.DoesNotContain("legalTeacherAdmissionCommitment", json, StringComparison.Ordinal);

        var (configKind, vocabSize, contextLength, hiddenDim, numLayers,
            numHeads, numKvHeads, ffnHiddenDim, rawPayloadPersisted,
            absolutePathPersisted, schemaVersion, _, _, _, _, _, _, _, _, _) = context;
        Assert.Equal("legacy", configKind);
        Assert.Equal(2, vocabSize);
        Assert.Equal(3, contextLength);
        Assert.Equal(4, hiddenDim);
        Assert.Equal(1, numLayers);
        Assert.Equal(1, numHeads);
        Assert.Equal(1, numKvHeads);
        Assert.Equal(8, ffnHiddenDim);
        Assert.False(rawPayloadPersisted);
        Assert.False(absolutePathPersisted);
        Assert.Equal(2, schemaVersion);
    }

    private static DesignConversionDpoLegalTeacherAdmissionCommitment Finalize(
        LegalTrainingTeacherAdmission admission,
        IEnumerable<string> checkpointEvidence)
    {
        var core = DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
            .CaptureAdmission(admission);
        Assert.True(core.IsSuccess, core.ErrorMessage);
        var commitment = DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
            .FinalizeCommitment(core.Value!, checkpointEvidence);
        Assert.True(commitment.IsSuccess, commitment.ErrorMessage);
        return commitment.Value!;
    }

    private static DesignConversionDpoLegalTeacherAdmissionCommitment Finalize(
        DesignConversionDpoLegalTeacherAdmissionCore admissionCore,
        IEnumerable<string> checkpointEvidence)
    {
        var commitment = DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
            .FinalizeCommitment(admissionCore, checkpointEvidence);
        Assert.True(commitment.IsSuccess, commitment.ErrorMessage);
        return commitment.Value!;
    }

    private static LegalTrainingTeacherAdmission AdmitExact(
        LegalTrainingTeacherAdmissionAuthority authority,
        IReadOnlyList<TrainingTeacherProvenance> provenance,
        string contentRole = "train")
    {
        var content = ProjectExactContent(provenance.Count, contentRole);
        var boundProvenance = provenance
            .Select((row, index) => row with
            {
                TrainingContentSha256 = content.Rows[index].Sha256
            })
            .ToArray();
        var admission = authority.ValidateBatch(content, boundProvenance);
        Assert.True(admission.IsSuccess, admission.ErrorMessage);
        return admission.Value!;
    }

    private static DpoTrainingContentBatch ProjectExactContent(
        int rowCount,
        string role)
    {
        var projection = BoundedDpoTrainingContentProjector.Project(
            Enumerable.Range(0, rowCount)
                .Select(index => new DpoTrainingContentRowInput(
                    $"legal teacher commitment {role} prompt {index}",
                    $"accepted bounded {role} row {index}",
                    $"rejected bounded {role} row {index}",
                    [index + 1.0, 0.0],
                    $"design-conversion-legal-teacher-{role}-v1"))
                .ToArray());
        Assert.True(projection.IsSuccess, projection.ErrorMessage);
        return projection.Value!;
    }

    private static LegalTrainingTeacherAdmissionAuthority CreateAuthority(
        string manifestDigest,
        string layerDigest,
        string modelId = "qwen3.5:9b")
    {
        var entry = new LegalTrainingTeacherPolicyEntry(
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            modelId,
            manifestDigest,
            TrainingTeacherModelLicense.Apache20,
            EvidenceA,
            EvidenceB)
        {
            ModelArtifactDigestKind =
                LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
            ModelArtifactDigest = layerDigest
        };
        var result = LegalTrainingTeacherAdmissionAuthority.Create(
            [entry],
            maxTeacherCalls: 8,
            Enumerable.Repeat((byte)9, 32).ToArray());
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!;
    }

    private static TrainingTeacherProvenance External(
        string requestId,
        string manifestDigest,
        string layerDigest,
        string modelId = "qwen3.5:9b")
        => new(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            modelId,
            manifestDigest,
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            requestId,
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
            ModelArtifactDigest = layerDigest
        };

    private static LegalTrainingTeacherAdmissionAuthority CreateHostedAuthority()
    {
        var entry = new LegalTrainingTeacherPolicyEntry(
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.OnlineHostedOpenWeight,
            TrainingTeacherHostingProvider.Together,
            "qwen3.5:9b",
            ManifestA,
            TrainingTeacherModelLicense.Apache20,
            EvidenceA,
            EvidenceB);
        var result = LegalTrainingTeacherAdmissionAuthority.Create(
            [entry], 8, Enumerable.Repeat((byte)8, 32).ToArray());
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!;
    }

    private static TrainingTeacherProvenance Hosted(string requestId)
        => new(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.OnlineHostedOpenWeight,
            TrainingTeacherHostingProvider.Together,
            "qwen3.5:9b",
            ManifestA,
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: true,
            ProviderTermsAllowTraining: true,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            requestId,
            InvocationCount: 1);
}

internal static class DesignConversionDpoLegalTeacherAdmissionCommitmentTestFactory
{
    public static DesignConversionDpoLegalTeacherAdmissionCommitment Create(
        IEnumerable<string> checkpointEvidenceRefs)
    {
        var authority = LegalTrainingTeacherAdmissionAuthority.Create(
            [],
            maxTeacherCalls: 0,
            Enumerable.Repeat((byte)0x5a, 32).ToArray());
        Assert.True(authority.IsSuccess, authority.ErrorMessage);
        var trainingContent = BoundedDpoTrainingContentProjector.Project(
        [
            new DpoTrainingContentRowInput(
                "design-conversion legal teacher commitment test",
                "accepted bounded DPO row",
                "rejected bounded DPO row",
                [1.0, 0.0],
                "design-conversion-legal-teacher-commitment-test-v1")
        ]);
        Assert.True(trainingContent.IsSuccess, trainingContent.ErrorMessage);
        var provenance = TrainingTeacherProvenance
            .Deterministic("test-custody-commitment") with
        {
            TrainingContentSha256 = trainingContent.Value!.Rows[0].Sha256
        };
        var admission = authority.Value!.ValidateBatch(
            trainingContent.Value,
            [provenance]);
        Assert.True(admission.IsSuccess, admission.ErrorMessage);
        var core = DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.CaptureAdmission(
            admission.Value!);
        Assert.True(core.IsSuccess, core.ErrorMessage);
        var commitment = DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.FinalizeCommitment(
            core.Value!,
            checkpointEvidenceRefs);
        Assert.True(commitment.IsSuccess, commitment.ErrorMessage);
        return commitment.Value!;
    }
}
