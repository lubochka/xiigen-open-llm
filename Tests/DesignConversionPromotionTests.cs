// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure;
using ContinuousLearning;
using DesignConversionDomain;

[Collection(DesignConversionLocalHostTestCollectionNames.CollectionName)]
public sealed class DesignConversionPromotionTests : IDisposable
{
    private readonly List<string> _loadedMachineTempRoots = [];

    public void Dispose()
    {
        foreach (var root in _loadedMachineTempRoots)
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void GateReport_DefaultContinueTrainingMargin_UsesAuthoritativeConstant()
    {
        var report = BuildGate();

        Assert.Equal(
            DesignConversionPromotionGateDefaults.ContinueTrainingMargin,
            report.ContinueTrainingMargin);
    }

    [Fact]
    public async Task PublishAsync_RejectsWhenPromotionGatesFailAndDoesNotUpload()
    {
        var uploader = new FakePromotionUploader();
        var result = await new DesignConversionPromotionPublisher(uploader).PublishAsync(
            await CreateLoadedPromotionMachineAsync(),
            BuildPipelineResult(),
            "promotion-rejected",
            BuildArtifacts(),
            BuildGate(heldOutScore: 0.70, requiredHeldOutScore: 0.90));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionPromotionDecisionKind.Reject, result.Value!.Decision.Kind);
        Assert.Equal(DesignConversionPromotionPublicationKind.Rejected, result.Value.PublicationKind);
        Assert.Equal(DesignConversionPromotionPublicationStatus.Rejected, result.Value.PublicationStatus);
        Assert.Null(result.Value.UploadReceipt);
        Assert.Contains(result.Value.Decision.Reasons, reason => reason.StartsWith("HELD_OUT_SCORE_BELOW_THRESHOLD", StringComparison.Ordinal));
        Assert.Equal(0, uploader.UploadCallCount);
        Assert.False(result.Value.Manifest.ModelServerUploadAttempted);
        Assert.False(result.Value.Manifest.ModelServerPromotionClaimed);
    }

    [Fact]
    public async Task PublishAsync_RejectDecisionDoesNotRequireBoundCheckpointOrExport()
    {
        var uploader = new FakePromotionUploader();
        var result = await new DesignConversionPromotionPublisher(uploader).PublishAsync(
            new DesignConversionMachine(),
            BuildPipelineResult(),
            "promotion-rejected-no-export",
            BuildArtifacts(),
            BuildGate(heldOutScore: 0.70, requiredHeldOutScore: 0.90));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionPromotionDecisionKind.Reject, result.Value!.Decision.Kind);
        Assert.Equal(DesignConversionPromotionPublicationKind.Rejected, result.Value.PublicationKind);
        Assert.Equal(DesignConversionPromotionPublicationStatus.Rejected, result.Value.PublicationStatus);
        Assert.Null(result.Value.UploadReceipt);
        Assert.Equal(0, uploader.UploadCallCount);
        Assert.False(result.Value.Manifest.ModelServerUploadAttempted);
        Assert.False(result.Value.Manifest.ModelServerPromotionClaimed);
        Assert.False(result.Value.Manifest.LocalPromotionReceiptWritten);
        Assert.StartsWith("not-exported://design-conversion-machine/", result.Value.Manifest.CheckpointRef, StringComparison.Ordinal);
        Assert.Equal("0.0.0-non-upload", result.Value.Manifest.Version);
        Assert.Contains(
            "DESIGN_CONVERSION_MACHINE_EXPORT_NOT_ATTEMPTED_NON_UPLOAD_DECISION",
            result.Value.Manifest.EvidenceRefs);
        Assert.Contains(
            "DESIGN_CONVERSION_NON_UPLOAD_PUBLICATION_MANIFEST",
            result.Value.Manifest.EvidenceRefs);
    }

    [Fact]
    public async Task PublishAsync_RejectsHiddenDeterminismEvidenceAndDoesNotUpload()
    {
        var uploader = new FakePromotionUploader();
        var result = await new DesignConversionPromotionPublisher(uploader).PublishAsync(
            await CreateLoadedPromotionMachineAsync(),
            BuildPipelineResult(),
            "promotion-hidden-determinism-rejected",
            BuildArtifacts(),
            BuildGate(hiddenShortcutEvidenceDetected: true));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionPromotionDecisionKind.Reject, result.Value!.Decision.Kind);
        Assert.Equal(DesignConversionPromotionPublicationKind.Rejected, result.Value.PublicationKind);
        Assert.Equal(DesignConversionPromotionPublicationStatus.Rejected, result.Value.PublicationStatus);
        Assert.Null(result.Value.UploadReceipt);
        Assert.Contains("HIDDEN_SHORTCUT_EVIDENCE_DETECTED", result.Value.Decision.Reasons);
        Assert.Contains("DESIGN_CONVERSION_PROMOTION_REJECTED", result.Value.Decision.EvidenceRefs);
        Assert.Equal(0, uploader.UploadCallCount);
    }

    [Fact]
    public async Task PublishAsync_RejectsDatasetCountGateFailureAndDoesNotUploadOrClaimPromotion()
    {
        var uploader = new FakePromotionUploader();
        var pipelineResult = BuildCountGateFailedPipelineResult();

        var result = await new DesignConversionPromotionPublisher(uploader).PublishAsync(
            await CreateLoadedPromotionMachineAsync(),
            pipelineResult,
            "promotion-count-gate-rejected",
            BuildArtifacts(),
            BuildGate());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionPromotionDecisionKind.Reject, result.Value!.Decision.Kind);
        Assert.Equal(DesignConversionPromotionPublicationKind.Rejected, result.Value.PublicationKind);
        Assert.Equal(DesignConversionPromotionPublicationStatus.Rejected, result.Value.PublicationStatus);
        Assert.Contains("DESIGN_CONVERSION_DPO_DATASET_COUNT_GATE_NOT_PASSED", result.Value.Decision.Reasons);
        Assert.Contains("DESIGN_CONVERSION_PROMOTION_REJECTED_BY_DATASET_COUNT_GATE", result.Value.Decision.EvidenceRefs);
        Assert.Equal(0, uploader.UploadCallCount);
        Assert.Null(result.Value.UploadReceipt);
        Assert.False(result.Value.Manifest.ModelServerUploadAttempted);
        Assert.False(result.Value.Manifest.ModelServerPromotionClaimed);
        Assert.DoesNotContain("MODEL_SERVER_UPLOAD_ATTEMPTED_BY_INJECTED_UPLOADER", result.Value.Manifest.EvidenceRefs);
        Assert.DoesNotContain("MODEL_SERVER_PROMOTION_CLAIMED_AFTER_INJECTED_UPLOAD", result.Value.Manifest.EvidenceRefs);
    }

    [Fact]
    public async Task PublishAsync_RejectsNonHttpsRemoteReceiptWithoutClaimingModelServerUploadOrLeakingEvidence()
    {
        const string privateMarker = "PRIVATE_REMOTE_PROMOTION_RECEIPT_SECRET";
        var uploader = new FakePromotionUploader();
        var result = await new DesignConversionPromotionPublisher(uploader).PublishAsync(
            await CreateLoadedPromotionMachineAsync(),
            BuildMeasuredPipelineResult(realWeightsProven: true),
            "promotion-fake-receipt-rejected",
            BuildArtifacts(),
            BuildGate());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionPromotionDecisionKind.Reject, result.Value!.Decision.Kind);
        Assert.Collection(
            result.Value.Decision.Reasons,
            reason => Assert.Equal(
                "DESIGN_CONVERSION_PROMOTION_REMOTE_RECEIPT_HTTPS_REF_UNSAFE",
                reason));
        Assert.Equal(DesignConversionPromotionPublicationStatus.Rejected, result.Value.PublicationStatus);
        Assert.Equal(DesignConversionPromotionPublicationKind.Rejected, result.Value.PublicationKind);
        Assert.Equal(1, uploader.UploadCallCount);
        Assert.Null(result.Value.UploadReceipt);
        Assert.False(result.Value.Manifest.ModelServerUploadAttempted);
        Assert.False(result.Value.Manifest.ModelServerPromotionClaimed);
        Assert.False(result.Value.Manifest.LocalPromotionReceiptWritten);
        Assert.Equal("invalid_remote_receipt", result.Value.Manifest.PromotionReceiptKind);
        Assert.Null(result.Value.Manifest.PromotionUploadRef);
        Assert.Null(result.Value.Manifest.PromotionUploadedAtUtc);
        Assert.Contains("DESIGN_CONVERSION_PROMOTION_REMOTE_RECEIPT_REJECTED", result.Value.Manifest.EvidenceRefs);
        Assert.Contains("MODEL_SERVER_PROMOTION_NOT_CLAIMED_REJECTED_RECEIPT", result.Value.Manifest.EvidenceRefs);
        Assert.DoesNotContain("MODEL_SERVER_UPLOAD_VERIFIED_REMOTE_RECEIPT", result.Value.Manifest.EvidenceRefs);
        Assert.DoesNotContain("PROMOTION_RECEIPT_KIND:model_server_upload_verified", result.Value.Manifest.EvidenceRefs);
        var receiptEvidenceHashes = AssertStrictUploadReceiptEvidenceHashes(
            result.Value.Manifest.EvidenceRefs,
            expectedCount: 3);
        Assert.Equal(3, receiptEvidenceHashes.Length);
        var publicEvidence = string.Join(
            '|',
            result.Value.Decision.EvidenceRefs.Concat(result.Value.Manifest.EvidenceRefs));
        Assert.DoesNotContain(privateMarker, publicEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain(@"C:\Users\lubao\private", publicEvidence, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token=credential", publicEvidence, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PublishAsync_LocalReceiptOnlyKeepsModelServerFlagsFalseAndWritesTruthMarkers()
    {
        var uploader = new LocalReceiptPromotionUploader();
        var result = await new DesignConversionPromotionPublisher(uploader).PublishAsync(
            await CreateLoadedPromotionMachineAsync(),
            BuildMeasuredPipelineResult(realWeightsProven: true),
            "promotion-local-receipt",
            BuildArtifacts(),
            BuildGate());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionPromotionDecisionKind.Promote, result.Value!.Decision.Kind);
        Assert.Equal(1, uploader.UploadCallCount);
        Assert.NotNull(result.Value.UploadReceipt);
        Assert.Equal(
            DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt,
            result.Value.PublicationKind);
        Assert.NotEqual(DesignConversionPromotionPublicationKind.Rejected, result.Value.PublicationKind);
        Assert.Equal(DesignConversionPromotionPublicationStatus.NotUploaded, result.Value.PublicationStatus);
        Assert.Equal(
            DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt,
            result.Value.UploadReceipt!.PublicationKind);
        Assert.False(result.Value.UploadReceipt.UploadAttempted);
        Assert.False(result.Value.UploadReceipt.UploadSucceeded);
        Assert.False(result.Value.UploadReceipt.VerificationSucceeded);
        Assert.Equal(
            DesignConversionPromotionPublicationStatus.NotUploaded,
            result.Value.UploadReceipt.PublicationStatus);
        Assert.False(result.Value.Manifest.ModelServerUploadAttempted);
        Assert.False(result.Value.Manifest.ModelServerPromotionClaimed);
        Assert.True(result.Value.Manifest.LocalPromotionReceiptWritten);
        Assert.Equal("local_receipt_only", result.Value.Manifest.PromotionReceiptKind);
        var expectedLocalRef = DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix +
                               Uri.EscapeDataString("promotion-local-receipt");
        Assert.Equal(expectedLocalRef, result.Value.UploadReceipt.RemoteRef);
        Assert.Equal(expectedLocalRef, result.Value.Manifest.PromotionUploadRef);
        Assert.StartsWith(
            DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix,
            result.Value.Manifest.PromotionUploadRef,
            StringComparison.Ordinal);
        Assert.Contains("PROMOTION_RECEIPT_KIND:local_receipt_only", result.Value.Manifest.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_LOCAL_PROMOTION_RECEIPT_WRITTEN", result.Value.Manifest.EvidenceRefs);
        Assert.Contains("EXTERNAL_CLOUD_UPLOAD_NOT_ATTEMPTED", result.Value.Manifest.EvidenceRefs);
        Assert.Contains("MODEL_SERVER_UPLOAD_NOT_ATTEMPTED_LOCAL_RECEIPT", result.Value.Manifest.EvidenceRefs);
        Assert.Contains("MODEL_SERVER_PROMOTION_NOT_CLAIMED_LOCAL_RECEIPT", result.Value.Manifest.EvidenceRefs);
        Assert.Contains(
            $"PROMOTION_PUBLICATION_STATUS:{DesignConversionPromotionPublicationStatus.NotUploaded}",
            result.Value.Manifest.EvidenceRefs);
        Assert.DoesNotContain("MODEL_SERVER_UPLOAD_VERIFIED_REMOTE_RECEIPT", result.Value.Manifest.EvidenceRefs);
        Assert.DoesNotContain("model_server_upload_verified_remote_receipt", result.Value.Manifest.EvidenceRefs);
        Assert.DoesNotContain(
            $"PROMOTION_PUBLICATION_STATUS:{DesignConversionPromotionPublicationStatus.UploadedVerified}",
            result.Value.Manifest.EvidenceRefs);
        var localReceiptEvidenceHashes = AssertStrictUploadReceiptEvidenceHashes(
            result.Value.UploadReceipt.EvidenceRefs,
            expectedCount: 3);
        Assert.All(localReceiptEvidenceHashes, hash => Assert.Contains(hash, result.Value.Manifest.EvidenceRefs));
        Assert.DoesNotContain("MODEL_SERVER_UPLOAD_ATTEMPTED_BY_INJECTED_UPLOADER", result.Value.Manifest.EvidenceRefs);
        Assert.DoesNotContain("MODEL_SERVER_PROMOTION_CLAIMED_AFTER_INJECTED_UPLOAD", result.Value.Manifest.EvidenceRefs);
    }

    [Fact]
    public async Task PublishAsync_AcceptsCoherentVerifiedHttpsModelServerReceiptAndIgnoresInjectedLocalMarkers()
    {
        const string remoteRef = "https://models.example.test/design-conversion/promotions/verified-upload-1";
        const string provenanceRef = "model-server-upload-provenance:verified-upload-1";
        const string uploadTrainingEvidence = "MODEL_SERVER_UPLOAD_NOT_ATTEMPTED:TRAINING_OBSERVATION";
        const string promotionTrainingEvidence = "MODEL_SERVER_PROMOTION_NOT_CLAIMED:TRAINING_OBSERVATION";
        var gate = BuildGate();
        gate = gate with
        {
            EvidenceRefs = gate.EvidenceRefs
                .Concat([uploadTrainingEvidence, promotionTrainingEvidence])
                .ToArray()
        };
        var uploader = new ReceiptPromotionUploader(new DesignConversionPromotionUploadReceipt(
            UploadId: "verified-upload-1",
            RemoteRef: remoteRef,
            UploadedAtUtc: DateTimeOffset.UnixEpoch,
            EvidenceRefs:
            [
                "DESIGN_CONVERSION_LOCAL_PROMOTION_RECEIPT_WRITTEN",
                "EXTERNAL_CLOUD_UPLOAD_NOT_ATTEMPTED",
                "PROMOTION_RECEIPT_KIND:local_receipt_only",
                "promotion_publication_status:not_uploaded"
            ])
        {
            PublicationKind = DesignConversionPromotionPublicationKind.VerifiedModelServerUpload,
            UploadAttempted = true,
            UploadSucceeded = true,
            VerificationSucceeded = true,
            PublicationStatus = DesignConversionPromotionPublicationStatus.UploadedVerified,
            ProvenanceRef = provenanceRef
        });

        var result = await new DesignConversionPromotionPublisher(uploader).PublishAsync(
            await CreateLoadedPromotionMachineAsync(),
            BuildMeasuredPipelineResult(realWeightsProven: true),
            "promotion-verified-remote-receipt",
            BuildArtifacts(),
            gate);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, uploader.UploadCallCount);
        Assert.Equal(DesignConversionPromotionDecisionKind.Promote, result.Value!.Decision.Kind);
        Assert.Equal(DesignConversionPromotionPublicationKind.VerifiedModelServerUpload, result.Value.PublicationKind);
        Assert.NotEqual(DesignConversionPromotionPublicationKind.Rejected, result.Value.PublicationKind);
        Assert.Equal(DesignConversionPromotionPublicationStatus.UploadedVerified, result.Value.PublicationStatus);
        Assert.NotNull(result.Value.UploadReceipt);
        Assert.True(result.Value.UploadReceipt!.UploadAttempted);
        Assert.True(result.Value.UploadReceipt.UploadSucceeded);
        Assert.True(result.Value.UploadReceipt.VerificationSucceeded);
        Assert.Equal(provenanceRef, result.Value.UploadReceipt.ProvenanceRef);
        Assert.Equal(remoteRef, result.Value.UploadReceipt.RemoteRef);
        Assert.True(result.Value.Manifest.ModelServerUploadAttempted);
        Assert.True(result.Value.Manifest.ModelServerPromotionClaimed);
        Assert.False(result.Value.Manifest.LocalPromotionReceiptWritten);
        Assert.Equal("model_server_upload_verified", result.Value.Manifest.PromotionReceiptKind);
        Assert.Equal(remoteRef, result.Value.Manifest.PromotionUploadRef);
        Assert.Contains("MODEL_SERVER_UPLOAD_VERIFIED_REMOTE_RECEIPT", result.Value.Manifest.EvidenceRefs);
        Assert.Contains("MODEL_SERVER_PROMOTION_CLAIMED_VERIFIED_REMOTE_RECEIPT", result.Value.Manifest.EvidenceRefs);
        Assert.DoesNotContain("MODEL_SERVER_UPLOAD_NOT_ATTEMPTED", result.Value.Manifest.EvidenceRefs);
        Assert.DoesNotContain("MODEL_SERVER_PROMOTION_NOT_CLAIMED", result.Value.Manifest.EvidenceRefs);
        Assert.Contains(uploadTrainingEvidence, result.Value.Manifest.EvidenceRefs);
        Assert.Contains(promotionTrainingEvidence, result.Value.Manifest.EvidenceRefs);
        Assert.Contains(
            $"PROMOTION_PUBLICATION_STATUS:{DesignConversionPromotionPublicationStatus.UploadedVerified}",
            result.Value.Manifest.EvidenceRefs);
        Assert.DoesNotContain("DESIGN_CONVERSION_LOCAL_PROMOTION_RECEIPT_WRITTEN", result.Value.Manifest.EvidenceRefs);
        Assert.DoesNotContain("EXTERNAL_CLOUD_UPLOAD_NOT_ATTEMPTED", result.Value.Manifest.EvidenceRefs);
        Assert.DoesNotContain("PROMOTION_RECEIPT_KIND:local_receipt_only", result.Value.Manifest.EvidenceRefs);
        Assert.DoesNotContain("promotion_publication_status:not_uploaded", result.Value.Manifest.EvidenceRefs);
        var remoteReceiptEvidenceHashes = AssertStrictUploadReceiptEvidenceHashes(
            result.Value.UploadReceipt.EvidenceRefs,
            expectedCount: 4);
        Assert.All(remoteReceiptEvidenceHashes, hash => Assert.Contains(hash, result.Value.Manifest.EvidenceRefs));
        const string provenanceHashPrefix = "MODEL_SERVER_UPLOAD_PROVENANCE_SHA256:";
        var provenanceHashMarker = Assert.Single(
            result.Value.Manifest.EvidenceRefs,
            reference => reference.StartsWith(provenanceHashPrefix, StringComparison.Ordinal));
        var expectedProvenanceDigest = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(provenanceRef)))
            .ToLowerInvariant();
        Assert.Equal($"{provenanceHashPrefix}{expectedProvenanceDigest}", provenanceHashMarker);
        Assert.DoesNotContain(result.Value.Manifest.EvidenceRefs, reference =>
            reference.Contains(provenanceRef, StringComparison.Ordinal));
    }

    [Fact]
    public async Task PublishAsync_CanonicalizesAcceptedVerifiedHttpsRemoteReceiptRef()
    {
        const string rawRemoteRef =
            "HTTPS://MODELS.EXAMPLE.TEST/design-conversion/promotions/canonical-upload";
        var canonicalRemoteRef = new Uri(rawRemoteRef, UriKind.Absolute).AbsoluteUri;
        Assert.NotEqual(rawRemoteRef, canonicalRemoteRef);
        var uploader = new ReceiptPromotionUploader(new DesignConversionPromotionUploadReceipt(
            UploadId: "canonical-remote-upload",
            RemoteRef: rawRemoteRef,
            UploadedAtUtc: DateTimeOffset.UnixEpoch,
            EvidenceRefs: ["CANONICAL_REMOTE_RECEIPT_EVIDENCE"])
        {
            PublicationKind = DesignConversionPromotionPublicationKind.VerifiedModelServerUpload,
            UploadAttempted = true,
            UploadSucceeded = true,
            VerificationSucceeded = true,
            PublicationStatus = DesignConversionPromotionPublicationStatus.UploadedVerified,
            ProvenanceRef = "model-server-upload-provenance:canonical-upload"
        });

        var result = await new DesignConversionPromotionPublisher(uploader).PublishAsync(
            await CreateLoadedPromotionMachineAsync(),
            BuildMeasuredPipelineResult(realWeightsProven: true),
            "promotion-canonical-remote-receipt",
            BuildArtifacts(),
            BuildGate());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionPromotionDecisionKind.Promote, result.Value!.Decision.Kind);
        Assert.Equal(DesignConversionPromotionPublicationKind.VerifiedModelServerUpload, result.Value.PublicationKind);
        Assert.NotEqual(DesignConversionPromotionPublicationKind.Rejected, result.Value.PublicationKind);
        Assert.Equal(DesignConversionPromotionPublicationStatus.UploadedVerified, result.Value.PublicationStatus);
        Assert.Equal(canonicalRemoteRef, result.Value.UploadReceipt!.RemoteRef);
        Assert.Equal(canonicalRemoteRef, result.Value.Manifest.PromotionUploadRef);
        Assert.True(result.Value.Manifest.ModelServerUploadAttempted);
        Assert.True(result.Value.Manifest.ModelServerPromotionClaimed);
        Assert.Contains("MODEL_SERVER_UPLOAD_VERIFIED_REMOTE_RECEIPT", result.Value.Manifest.EvidenceRefs);
        Assert.Contains("MODEL_SERVER_PROMOTION_CLAIMED_VERIFIED_REMOTE_RECEIPT", result.Value.Manifest.EvidenceRefs);
        Assert.DoesNotContain("MODEL_SERVER_UPLOAD_NOT_ATTEMPTED", result.Value.Manifest.EvidenceRefs);
        Assert.DoesNotContain("MODEL_SERVER_PROMOTION_NOT_CLAIMED", result.Value.Manifest.EvidenceRefs);
    }

    [Theory]
    [InlineData("raw-crlf")]
    [InlineData("encoded-crlf")]
    [InlineData("userinfo")]
    [InlineData("query")]
    [InlineData("fragment")]
    [InlineData("backslash")]
    [InlineData("whitespace")]
    [InlineData("hidden-crlf-dot-segment")]
    [InlineData("double-encoded-backslash")]
    [InlineData("empty-userinfo")]
    [InlineData("empty-query")]
    [InlineData("empty-fragment")]
    [InlineData("multi-encoded-dot-segment")]
    [InlineData("multi-encoded-slash")]
    [InlineData("encoded-userinfo")]
    [InlineData("malformed-percent")]
    [InlineData("double-encoded-control")]
    [InlineData("over-max-encoding-layers")]
    public async Task PublishAsync_RejectsUnsafeVerifiedHttpsRemoteReceiptRefWithoutLeakage(
        string unsafeKind)
    {
        const string expectedReason = "DESIGN_CONVERSION_PROMOTION_REMOTE_RECEIPT_HTTPS_REF_UNSAFE";
        var (remoteRef, leakMarker) = unsafeKind switch
        {
            "raw-crlf" => (
                "https://models.example.test/promotions/safe\r\nREMOTE_RAW_CRLF_REF_SECRET",
                "REMOTE_RAW_CRLF_REF_SECRET"),
            "encoded-crlf" => (
                "https://models.example.test/promotions/safe%0d%0aREMOTE_ENCODED_CRLF_REF_SECRET",
                "REMOTE_ENCODED_CRLF_REF_SECRET"),
            "userinfo" => (
                "https://REMOTE_USERINFO_REF_SECRET:credential@models.example.test/promotions/safe",
                "REMOTE_USERINFO_REF_SECRET"),
            "query" => (
                "https://models.example.test/promotions/safe?credential=REMOTE_QUERY_REF_SECRET",
                "REMOTE_QUERY_REF_SECRET"),
            "fragment" => (
                "https://models.example.test/promotions/safe#REMOTE_FRAGMENT_REF_SECRET",
                "REMOTE_FRAGMENT_REF_SECRET"),
            "backslash" => (
                @"https://models.example.test/promotions\REMOTE_BACKSLASH_REF_SECRET",
                "REMOTE_BACKSLASH_REF_SECRET"),
            "whitespace" => (
                "https://models.example.test/promotions/REMOTE_WHITESPACE_REF_SECRET value",
                "REMOTE_WHITESPACE_REF_SECRET"),
            "hidden-crlf-dot-segment" => (
                "https://models.example.test/promotions/unsafe%0d%0aREMOTE_HIDDEN_CRLF_REF_SECRET/../safe",
                "REMOTE_HIDDEN_CRLF_REF_SECRET"),
            "double-encoded-backslash" => (
                "https://models.example.test/promotions/safe%255CREMOTE_DOUBLE_ENCODED_BACKSLASH_SECRET",
                "REMOTE_DOUBLE_ENCODED_BACKSLASH_SECRET"),
            "empty-userinfo" => (
                "https://@models.example.test/promotions/REMOTE_EMPTY_USERINFO_REF_SECRET",
                "REMOTE_EMPTY_USERINFO_REF_SECRET"),
            "empty-query" => (
                "https://models.example.test/promotions/REMOTE_EMPTY_QUERY_REF_SECRET?",
                "REMOTE_EMPTY_QUERY_REF_SECRET"),
            "empty-fragment" => (
                "https://models.example.test/promotions/REMOTE_EMPTY_FRAGMENT_REF_SECRET#",
                "REMOTE_EMPTY_FRAGMENT_REF_SECRET"),
            "multi-encoded-dot-segment" => (
                "https://models.example.test/promotions/safe/%252e%252e/REMOTE_MULTI_DOT_REF_SECRET",
                "REMOTE_MULTI_DOT_REF_SECRET"),
            "multi-encoded-slash" => (
                "https://models.example.test/promotions/safe%252FREMOTE_MULTI_SLASH_REF_SECRET",
                "REMOTE_MULTI_SLASH_REF_SECRET"),
            "encoded-userinfo" => (
                "https://REMOTE_ENCODED_USERINFO_SECRET%40models.example.test/promotions/safe",
                "REMOTE_ENCODED_USERINFO_SECRET"),
            "malformed-percent" => (
                "https://models.example.test/promotions/REMOTE_MALFORMED_PERCENT_SECRET%2",
                "REMOTE_MALFORMED_PERCENT_SECRET"),
            "double-encoded-control" => (
                "https://models.example.test/promotions/" +
                EncodeReceiptPathSegment("REMOTE_DEEP_CONTROL_REF_SECRET\r\nsegment", layers: 2),
                "REMOTE_DEEP_CONTROL_REF_SECRET"),
            "over-max-encoding-layers" => (
                "https://models.example.test/promotions/" +
                EncodeReceiptPathSegment(@"REMOTE_MAX_LAYER_REF_SECRET\checkpoint", layers: 10),
                "REMOTE_MAX_LAYER_REF_SECRET"),
            _ => throw new InvalidOperationException($"Unknown unsafe remote-ref case: {unsafeKind}")
        };
        var uploader = new ReceiptPromotionUploader(new DesignConversionPromotionUploadReceipt(
            UploadId: "safe-remote-ref-receipt",
            RemoteRef: remoteRef,
            UploadedAtUtc: DateTimeOffset.UnixEpoch,
            EvidenceRefs: ["REMOTE_REF_BOUNDARY_EVIDENCE"])
        {
            PublicationKind = DesignConversionPromotionPublicationKind.VerifiedModelServerUpload,
            UploadAttempted = true,
            UploadSucceeded = true,
            VerificationSucceeded = true,
            PublicationStatus = DesignConversionPromotionPublicationStatus.UploadedVerified,
            ProvenanceRef = "model-server-upload-provenance:safe-ref-boundary"
        });

        var result = await new DesignConversionPromotionPublisher(uploader).PublishAsync(
            await CreateLoadedPromotionMachineAsync(),
            BuildMeasuredPipelineResult(realWeightsProven: true),
            $"promotion-unsafe-remote-ref-{unsafeKind}",
            BuildArtifacts(),
            BuildGate());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, uploader.UploadCallCount);
        Assert.Equal(DesignConversionPromotionDecisionKind.Reject, result.Value!.Decision.Kind);
        Assert.Collection(result.Value.Decision.Reasons, reason => Assert.Equal(expectedReason, reason));
        Assert.Equal(DesignConversionPromotionPublicationStatus.Rejected, result.Value.PublicationStatus);
        Assert.Equal(DesignConversionPromotionPublicationKind.Rejected, result.Value.PublicationKind);
        Assert.Null(result.Value.UploadReceipt);
        Assert.False(result.Value.Manifest.ModelServerUploadAttempted);
        Assert.False(result.Value.Manifest.ModelServerPromotionClaimed);
        Assert.False(result.Value.Manifest.LocalPromotionReceiptWritten);
        Assert.Equal("invalid_remote_receipt", result.Value.Manifest.PromotionReceiptKind);
        Assert.Null(result.Value.Manifest.PromotionUploadRef);
        Assert.Null(result.Value.Manifest.PromotionUploadedAtUtc);
        var publicSurface = BuildPublicReceiptEvidence(result.Value);
        Assert.DoesNotContain(remoteRef, publicSurface, StringComparison.Ordinal);
        Assert.DoesNotContain(DecodeReceiptRefOnceForAssertion(remoteRef), publicSurface, StringComparison.Ordinal);
        Assert.DoesNotContain(leakMarker, publicSurface, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("raw-control")]
    [InlineData("encoded-control")]
    [InlineData("extra-slash")]
    [InlineData("query")]
    [InlineData("credential")]
    [InlineData("path")]
    [InlineData("dot")]
    [InlineData("dot-dot")]
    public async Task PublishAsync_RejectsUnsafeLocalReceiptRefWithoutLeakage(
        string unsafeKind)
    {
        const string expectedReason = "DESIGN_CONVERSION_PROMOTION_LOCAL_RECEIPT_REF_UNSAFE";
        var prefix = DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix;
        var (remoteRef, leakMarker) = unsafeKind switch
        {
            "raw-control" => (prefix + "safe\r\nLOCAL_RAW_CONTROL_REF_SECRET", "LOCAL_RAW_CONTROL_REF_SECRET"),
            "encoded-control" => (prefix + "safe%0d%0aLOCAL_ENCODED_CONTROL_REF_SECRET", "LOCAL_ENCODED_CONTROL_REF_SECRET"),
            "extra-slash" => (prefix + "safe/LOCAL_EXTRA_SLASH_REF_SECRET", "LOCAL_EXTRA_SLASH_REF_SECRET"),
            "query" => (prefix + "safe?credential=LOCAL_QUERY_REF_SECRET", "LOCAL_QUERY_REF_SECRET"),
            "credential" => (
                prefix + Uri.EscapeDataString("token=credential-LOCAL_CREDENTIAL_REF_SECRET"),
                "LOCAL_CREDENTIAL_REF_SECRET"),
            "path" => (
                prefix + Uri.EscapeDataString(@"C:\private\LOCAL_PATH_REF_SECRET"),
                "LOCAL_PATH_REF_SECRET"),
            "dot" => (prefix + ".", prefix + "."),
            "dot-dot" => (prefix + "..", prefix + ".."),
            _ => throw new InvalidOperationException($"Unknown unsafe local-ref case: {unsafeKind}")
        };
        var uploader = new ReceiptPromotionUploader(new DesignConversionPromotionUploadReceipt(
            UploadId: "safe-local-ref-receipt",
            RemoteRef: remoteRef,
            UploadedAtUtc: DateTimeOffset.UnixEpoch,
            EvidenceRefs: ["LOCAL_REF_BOUNDARY_EVIDENCE"])
        {
            PublicationKind = DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt,
            UploadAttempted = false,
            UploadSucceeded = false,
            VerificationSucceeded = false,
            PublicationStatus = DesignConversionPromotionPublicationStatus.NotUploaded
        });

        var result = await new DesignConversionPromotionPublisher(uploader).PublishAsync(
            await CreateLoadedPromotionMachineAsync(),
            BuildMeasuredPipelineResult(realWeightsProven: true),
            $"promotion-unsafe-local-ref-{unsafeKind}",
            BuildArtifacts(),
            BuildGate());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, uploader.UploadCallCount);
        Assert.Equal(DesignConversionPromotionDecisionKind.Reject, result.Value!.Decision.Kind);
        Assert.Collection(result.Value.Decision.Reasons, reason => Assert.Equal(expectedReason, reason));
        Assert.Equal(DesignConversionPromotionPublicationStatus.Rejected, result.Value.PublicationStatus);
        Assert.Equal(DesignConversionPromotionPublicationKind.Rejected, result.Value.PublicationKind);
        Assert.Null(result.Value.UploadReceipt);
        Assert.False(result.Value.Manifest.ModelServerUploadAttempted);
        Assert.False(result.Value.Manifest.ModelServerPromotionClaimed);
        Assert.False(result.Value.Manifest.LocalPromotionReceiptWritten);
        Assert.Equal("invalid_local_receipt", result.Value.Manifest.PromotionReceiptKind);
        Assert.Null(result.Value.Manifest.PromotionUploadRef);
        Assert.Null(result.Value.Manifest.PromotionUploadedAtUtc);
        var publicSurface = BuildPublicReceiptEvidence(result.Value);
        Assert.DoesNotContain(remoteRef, publicSurface, StringComparison.Ordinal);
        Assert.DoesNotContain(Uri.UnescapeDataString(remoteRef), publicSurface, StringComparison.Ordinal);
        Assert.DoesNotContain(leakMarker, publicSurface, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("token=credential-top-secret")]
    [InlineData("safe\nMODEL_SERVER_UPLOAD_VERIFIED_REMOTE_RECEIPT")]
    [InlineData("DESIGN_CONVERSION_REMOTE_UPLOAD")]
    [InlineData("model_server_upload_verified_remote_receipt")]
    public async Task PublishAsync_RejectsVerifiedRemoteReceiptWithUnsafeProvenanceWithoutLeakage(
        string unsafeProvenance)
    {
        var uploader = new ReceiptPromotionUploader(new DesignConversionPromotionUploadReceipt(
            UploadId: "verified-upload-unsafe-provenance",
            RemoteRef: "https://models.example.test/design-conversion/promotions/unsafe-provenance",
            UploadedAtUtc: DateTimeOffset.UnixEpoch,
            EvidenceRefs: ["REMOTE_RECEIPT_UNSAFE_PROVENANCE_TEST"])
        {
            PublicationKind = DesignConversionPromotionPublicationKind.VerifiedModelServerUpload,
            UploadAttempted = true,
            UploadSucceeded = true,
            VerificationSucceeded = true,
            PublicationStatus = DesignConversionPromotionPublicationStatus.UploadedVerified,
            ProvenanceRef = unsafeProvenance
        });

        var result = await new DesignConversionPromotionPublisher(uploader).PublishAsync(
            await CreateLoadedPromotionMachineAsync(),
            BuildMeasuredPipelineResult(realWeightsProven: true),
            "promotion-unsafe-remote-provenance",
            BuildArtifacts(),
            BuildGate());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, uploader.UploadCallCount);
        Assert.Equal(DesignConversionPromotionDecisionKind.Reject, result.Value!.Decision.Kind);
        Assert.Collection(
            result.Value.Decision.Reasons,
            reason => Assert.Equal(
                "DESIGN_CONVERSION_PROMOTION_REMOTE_RECEIPT_PROVENANCE_UNSAFE",
                reason));
        Assert.Equal(DesignConversionPromotionPublicationStatus.Rejected, result.Value.PublicationStatus);
        Assert.Equal(DesignConversionPromotionPublicationKind.Rejected, result.Value.PublicationKind);
        Assert.Null(result.Value.UploadReceipt);
        Assert.False(result.Value.Manifest.ModelServerUploadAttempted);
        Assert.False(result.Value.Manifest.ModelServerPromotionClaimed);
        Assert.Null(result.Value.Manifest.PromotionUploadRef);
        Assert.Null(result.Value.Manifest.PromotionUploadedAtUtc);
        var publicEvidence = BuildPublicReceiptEvidence(result.Value);
        Assert.DoesNotContain(unsafeProvenance, publicEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("token=credential", publicEvidence, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("top-secret", publicEvidence, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("marker")]
    [InlineData("path")]
    [InlineData("credential")]
    [InlineData("excessive-length")]
    public async Task PublishAsync_RejectsVerifiedRemoteReceiptWithUnsafeUploadIdWithoutLeakage(
        string unsafeKind)
    {
        var unsafeUploadId = unsafeKind switch
        {
            "marker" => "MODEL_SERVER_UPLOAD_VERIFIED_REMOTE_RECEIPT",
            "path" => @"C:\Users\lubao\private\checkpoint.json",
            "credential" => "bearer-secret-credential",
            "excessive-length" => new string('a', 257),
            _ => throw new InvalidOperationException($"Unknown unsafe upload-id case: {unsafeKind}")
        };
        var uploader = new ReceiptPromotionUploader(new DesignConversionPromotionUploadReceipt(
            UploadId: unsafeUploadId,
            RemoteRef: "https://models.example.test/design-conversion/promotions/unsafe-upload-id",
            UploadedAtUtc: DateTimeOffset.UnixEpoch,
            EvidenceRefs: ["REMOTE_RECEIPT_UNSAFE_UPLOAD_ID_TEST"])
        {
            PublicationKind = DesignConversionPromotionPublicationKind.VerifiedModelServerUpload,
            UploadAttempted = true,
            UploadSucceeded = true,
            VerificationSucceeded = true,
            PublicationStatus = DesignConversionPromotionPublicationStatus.UploadedVerified,
            ProvenanceRef = "model-server-upload-provenance:unsafe-upload-id-test"
        });

        var result = await new DesignConversionPromotionPublisher(uploader).PublishAsync(
            await CreateLoadedPromotionMachineAsync(),
            BuildMeasuredPipelineResult(realWeightsProven: true),
            "promotion-unsafe-remote-upload-id",
            BuildArtifacts(),
            BuildGate());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, uploader.UploadCallCount);
        Assert.Equal(DesignConversionPromotionDecisionKind.Reject, result.Value!.Decision.Kind);
        Assert.Collection(
            result.Value.Decision.Reasons,
            reason => Assert.Equal(
                "DESIGN_CONVERSION_PROMOTION_REMOTE_RECEIPT_UPLOAD_ID_UNSAFE",
                reason));
        Assert.Equal(DesignConversionPromotionPublicationStatus.Rejected, result.Value.PublicationStatus);
        Assert.Equal(DesignConversionPromotionPublicationKind.Rejected, result.Value.PublicationKind);
        Assert.Null(result.Value.UploadReceipt);
        Assert.False(result.Value.Manifest.ModelServerUploadAttempted);
        Assert.False(result.Value.Manifest.ModelServerPromotionClaimed);
        Assert.Null(result.Value.Manifest.PromotionUploadRef);
        Assert.Null(result.Value.Manifest.PromotionUploadedAtUtc);
        Assert.DoesNotContain(
            unsafeUploadId,
            BuildPublicReceiptEvidence(result.Value),
            StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("upload-id-marker")]
    [InlineData("upload-id-path")]
    [InlineData("provenance")]
    public async Task PublishAsync_RejectsLocalReceiptWithUnsafeTypedScalarWithoutLeakage(
        string unsafeKind)
    {
        var unsafeValue = unsafeKind switch
        {
            "upload-id-marker" => "UPLOAD_RECEIPT_FORGED_LOCAL_MARKER",
            "upload-id-path" => @"C:\Users\lubao\private\local-checkpoint.json",
            "provenance" => "password=top-secret",
            _ => throw new InvalidOperationException($"Unknown unsafe local scalar case: {unsafeKind}")
        };
        var uploadId = unsafeKind == "provenance" ? "safe-local-receipt-1" : unsafeValue;
        var provenanceRef = unsafeKind == "provenance" ? unsafeValue : string.Empty;
        var expectedReason = unsafeKind == "provenance"
            ? "DESIGN_CONVERSION_PROMOTION_LOCAL_RECEIPT_PROVENANCE_UNSAFE"
            : "DESIGN_CONVERSION_PROMOTION_LOCAL_RECEIPT_UPLOAD_ID_UNSAFE";
        var uploader = new ReceiptPromotionUploader(new DesignConversionPromotionUploadReceipt(
            UploadId: uploadId,
            RemoteRef: DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix + "unsafe-local-scalar",
            UploadedAtUtc: DateTimeOffset.UnixEpoch,
            EvidenceRefs: ["LOCAL_RECEIPT_UNSAFE_TYPED_SCALAR_TEST"])
        {
            PublicationKind = DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt,
            UploadAttempted = false,
            UploadSucceeded = false,
            VerificationSucceeded = false,
            PublicationStatus = DesignConversionPromotionPublicationStatus.NotUploaded,
            ProvenanceRef = provenanceRef
        });

        var result = await new DesignConversionPromotionPublisher(uploader).PublishAsync(
            await CreateLoadedPromotionMachineAsync(),
            BuildMeasuredPipelineResult(realWeightsProven: true),
            "promotion-unsafe-local-receipt-scalar",
            BuildArtifacts(),
            BuildGate());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, uploader.UploadCallCount);
        Assert.Equal(DesignConversionPromotionDecisionKind.Reject, result.Value!.Decision.Kind);
        Assert.Collection(result.Value.Decision.Reasons, reason => Assert.Equal(expectedReason, reason));
        Assert.Equal(DesignConversionPromotionPublicationStatus.Rejected, result.Value.PublicationStatus);
        Assert.Equal(DesignConversionPromotionPublicationKind.Rejected, result.Value.PublicationKind);
        Assert.Null(result.Value.UploadReceipt);
        Assert.False(result.Value.Manifest.ModelServerUploadAttempted);
        Assert.False(result.Value.Manifest.ModelServerPromotionClaimed);
        Assert.False(result.Value.Manifest.LocalPromotionReceiptWritten);
        Assert.Equal("invalid_local_receipt", result.Value.Manifest.PromotionReceiptKind);
        Assert.Null(result.Value.Manifest.PromotionUploadRef);
        Assert.Null(result.Value.Manifest.PromotionUploadedAtUtc);
        Assert.DoesNotContain(
            unsafeValue,
            BuildPublicReceiptEvidence(result.Value),
            StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(true, true, false, "https://models.example.test/design-conversion/promotions/unverified", "DESIGN_CONVERSION_PROMOTION_REMOTE_RECEIPT_UPLOAD_VERIFICATION_REQUIRED")]
    [InlineData(false, true, true, "https://models.example.test/design-conversion/promotions/incoherent", "DESIGN_CONVERSION_PROMOTION_REMOTE_RECEIPT_UPLOAD_VERIFICATION_REQUIRED")]
    [InlineData(true, true, true, "not-an-absolute-uri", "DESIGN_CONVERSION_PROMOTION_REMOTE_RECEIPT_HTTPS_REF_UNSAFE")]
    [InlineData(true, true, true, "http://models.example.test/design-conversion/promotions/non-https", "DESIGN_CONVERSION_PROMOTION_REMOTE_RECEIPT_HTTPS_REF_UNSAFE")]
    public async Task PublishAsync_RejectsMalformedOrIncoherentRemoteReceipt(
        bool uploadAttempted,
        bool uploadSucceeded,
        bool verificationSucceeded,
        string remoteRef,
        string expectedReason)
    {
        const string privateMarker = "PRIVATE_REMOTE_RECEIPT_PAYLOAD_SECRET";
        var uploader = new ReceiptPromotionUploader(new DesignConversionPromotionUploadReceipt(
            UploadId: "invalid-remote-upload",
            RemoteRef: remoteRef,
            UploadedAtUtc: DateTimeOffset.UnixEpoch,
            EvidenceRefs:
            [
                "MODEL_SERVER_UPLOAD_VERIFIED_REMOTE_RECEIPT",
                "PROMOTION_RECEIPT_KIND:model_server_upload_verified",
                $@"REMOTE_RECEIPT:C:\Users\lubao\private\{privateMarker}\checkpoint.json?token=credential"
            ])
        {
            PublicationKind = DesignConversionPromotionPublicationKind.VerifiedModelServerUpload,
            UploadAttempted = uploadAttempted,
            UploadSucceeded = uploadSucceeded,
            VerificationSucceeded = verificationSucceeded,
            PublicationStatus = DesignConversionPromotionPublicationStatus.UploadedVerified,
            ProvenanceRef = "model-server-upload-provenance:invalid-upload"
        });

        var result = await new DesignConversionPromotionPublisher(uploader).PublishAsync(
            await CreateLoadedPromotionMachineAsync(),
            BuildMeasuredPipelineResult(realWeightsProven: true),
            "promotion-invalid-remote-receipt",
            BuildArtifacts(),
            BuildGate());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, uploader.UploadCallCount);
        Assert.Equal(DesignConversionPromotionDecisionKind.Reject, result.Value!.Decision.Kind);
        Assert.Collection(result.Value.Decision.Reasons, reason => Assert.Equal(expectedReason, reason));
        Assert.Equal(DesignConversionPromotionPublicationKind.Rejected, result.Value.PublicationKind);
        Assert.Equal(DesignConversionPromotionPublicationStatus.Rejected, result.Value.PublicationStatus);
        Assert.Null(result.Value.UploadReceipt);
        Assert.False(result.Value.Manifest.ModelServerUploadAttempted);
        Assert.False(result.Value.Manifest.ModelServerPromotionClaimed);
        Assert.False(result.Value.Manifest.LocalPromotionReceiptWritten);
        Assert.Equal("invalid_remote_receipt", result.Value.Manifest.PromotionReceiptKind);
        Assert.Null(result.Value.Manifest.PromotionUploadRef);
        Assert.Null(result.Value.Manifest.PromotionUploadedAtUtc);
        Assert.DoesNotContain("MODEL_SERVER_UPLOAD_VERIFIED_REMOTE_RECEIPT", result.Value.Manifest.EvidenceRefs);
        Assert.DoesNotContain("PROMOTION_RECEIPT_KIND:model_server_upload_verified", result.Value.Manifest.EvidenceRefs);
        AssertStrictUploadReceiptEvidenceHashes(result.Value.Manifest.EvidenceRefs, expectedCount: 3);
        var publicErrorAndEvidence = string.Join(
            '|',
            result.Value.Decision.Reasons.Concat(result.Value.Manifest.EvidenceRefs));
        Assert.DoesNotContain(privateMarker, publicErrorAndEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain(@"C:\Users\lubao\private", publicErrorAndEvidence, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token=credential", publicErrorAndEvidence, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PublishAsync_UploaderEvidenceUsesDeterministicStrictSha256RefsWithoutRawLeakage()
    {
        const string rawEvidence = @"Bearer credential=top-secret C:\Users\lubao\private\PRIVATE_UPLOAD_RECEIPT_HASH_SECRET\checkpoint.json?token=credential";
        var receipt = new DesignConversionPromotionUploadReceipt(
            UploadId: "deterministic-local-receipt",
            RemoteRef: DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix + "deterministic-evidence",
            UploadedAtUtc: DateTimeOffset.UnixEpoch,
            EvidenceRefs: [rawEvidence])
        {
            PublicationKind = DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt,
            UploadAttempted = false,
            UploadSucceeded = false,
            VerificationSucceeded = false,
            PublicationStatus = DesignConversionPromotionPublicationStatus.NotUploaded
        };

        var first = await new DesignConversionPromotionPublisher(new ReceiptPromotionUploader(receipt)).PublishAsync(
            await CreateLoadedPromotionMachineAsync(),
            BuildMeasuredPipelineResult(realWeightsProven: true),
            "promotion-evidence-hash-first",
            BuildArtifacts(),
            BuildGate());
        var second = await new DesignConversionPromotionPublisher(new ReceiptPromotionUploader(receipt)).PublishAsync(
            await CreateLoadedPromotionMachineAsync(),
            BuildMeasuredPipelineResult(realWeightsProven: true),
            "promotion-evidence-hash-second",
            BuildArtifacts(),
            BuildGate());

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(DesignConversionPromotionDecisionKind.Promote, first.Value!.Decision.Kind);
        Assert.Equal(DesignConversionPromotionDecisionKind.Promote, second.Value!.Decision.Kind);
        var firstHash = Assert.Single(AssertStrictUploadReceiptEvidenceHashes(
            first.Value.UploadReceipt!.EvidenceRefs,
            expectedCount: 1));
        var secondHash = Assert.Single(AssertStrictUploadReceiptEvidenceHashes(
            second.Value.UploadReceipt!.EvidenceRefs,
            expectedCount: 1));
        var expectedDigest = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(rawEvidence)))
            .ToLowerInvariant();
        Assert.Equal($"UPLOAD_RECEIPT_EVIDENCE_SHA256:{expectedDigest}", firstHash);
        Assert.Equal(firstHash, secondHash);
        Assert.Contains(firstHash, first.Value.Manifest.EvidenceRefs);
        var publicEvidence = string.Join(
            '|',
            first.Value.Decision.EvidenceRefs
                .Concat(first.Value.Manifest.EvidenceRefs)
                .Concat(first.Value.UploadReceipt.EvidenceRefs));
        Assert.DoesNotContain(rawEvidence, publicEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("top-secret", publicEvidence, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(@"C:\Users\lubao\private", publicEvidence, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token=credential", publicEvidence, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PublishAsync_ManifestIncludesCheckpointTokenizerAndPolicyPaths()
    {
        var artifacts = BuildArtifacts();
        var result = await new DesignConversionPromotionPublisher(new FakePromotionUploader()).PublishAsync(
            await CreateLoadedPromotionMachineAsync(),
            BuildMeasuredPipelineResult(realWeightsProven: true),
            "promotion-artifacts",
            artifacts,
            BuildGate());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(artifacts.CheckpointPath, result.Value!.Manifest.PromotionArtifacts!.CheckpointPath);
        Assert.Equal(artifacts.TokenizerPath, result.Value.Manifest.PromotionArtifacts.TokenizerPath);
        Assert.Equal(artifacts.PolicyPath, result.Value.Manifest.PromotionArtifacts.PolicyPath);
        Assert.Contains(
            $"CHECKPOINT_PATH_REF:{DesignConversionDpoLeakageGuard.HashRef("design-conversion-promotion-checkpoint-path", artifacts.CheckpointPath)}",
            result.Value.Manifest.EvidenceRefs);
        Assert.Contains(
            $"TOKENIZER_PATH_REF:{DesignConversionDpoLeakageGuard.HashRef("design-conversion-promotion-tokenizer-path", artifacts.TokenizerPath)}",
            result.Value.Manifest.EvidenceRefs);
        Assert.Contains(
            $"POLICY_PATH_REF:{DesignConversionDpoLeakageGuard.HashRef("design-conversion-promotion-policy-path", artifacts.PolicyPath)}",
            result.Value.Manifest.EvidenceRefs);
        Assert.False(result.Value.Manifest.DeterministicLogicClaimedAsLearning);
    }

    [Fact]
    public async Task PublishAsync_ContinuesTrainingWhenQualityIsNearThresholdButInsufficient()
    {
        var uploader = new FakePromotionUploader();
        var result = await new DesignConversionPromotionPublisher(uploader).PublishAsync(
            await CreateLoadedPromotionMachineAsync(),
            BuildPipelineResult(),
            "promotion-continue-training",
            BuildArtifacts(),
            BuildGate(
                heldOutScore: 0.875,
                requiredHeldOutScore: 0.90,
                continueTrainingMargin:
                    DesignConversionPromotionGateDefaults.ContinueTrainingMargin));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionPromotionDecisionKind.ContinueTraining, result.Value!.Decision.Kind);
        Assert.Contains(result.Value.Decision.Reasons, reason => reason.StartsWith("HELD_OUT_SCORE_NEAR_THRESHOLD", StringComparison.Ordinal));
        Assert.Equal(0, uploader.UploadCallCount);
        Assert.False(result.Value.Manifest.ModelServerUploadAttempted);
    }

    [Fact]
    public async Task PublishAsync_BlocksFreshLoadGateFailureEvenWhenLocalWeightsExist()
    {
        var uploader = new FakePromotionUploader();
        var pipelineResult = BuildPipelineResult(
            callMachineLifecycleHook: true,
            BuildPassedMeasurementGateRequest());

        Assert.True(pipelineResult.LifecycleEvidence.CheckpointCandidateCreated);
        Assert.True(pipelineResult.LifecycleEvidence.CheckpointContainsRealTrainedWeights);
        AssertCallerAdmissionMeasurementIsAdvisoryOnly(pipelineResult);

        var result = await new DesignConversionPromotionPublisher(uploader).PublishAsync(
            await CreateLoadedPromotionMachineAsync(),
            pipelineResult,
            "promotion-checkpoint-candidate-blocked",
            BuildArtifacts(),
            BuildGate(freshLoadPassed: false));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionPromotionDecisionKind.Rollback, result.Value!.Decision.Kind);
        Assert.Contains("FRESH_LOAD_CHECK_FAILED", result.Value.Decision.Reasons);
        Assert.Equal(0, uploader.UploadCallCount);
        Assert.Null(result.Value.UploadReceipt);
        Assert.False(result.Value.Manifest.ModelServerUploadAttempted);
        Assert.False(result.Value.Manifest.ModelServerPromotionClaimed);
        Assert.Contains("CALLER_ADMISSION_MEASUREMENT_ADVISORY_ONLY",
            result.Value.Manifest.EvidenceRefs);
    }

    [Fact]
    public async Task PublishAsync_BlocksUploadWhenLocalFreshLoadPassedButMeasurementGateNotRun()
    {
        var uploader = new FakePromotionUploader();
        var pipelineResult = BuildPipelineResult(
            callMachineLifecycleHook: true,
            BuildPassedMeasurementGateRequest());

        Assert.Equal(DesignConversionLocalFreshLoadProbe.VerifiedStatus, pipelineResult.LifecycleEvidence.FreshLoadStatus);
        AssertCallerAdmissionMeasurementIsAdvisoryOnly(pipelineResult);

        var result = await new DesignConversionPromotionPublisher(uploader).PublishAsync(
            await CreateLoadedPromotionMachineAsync(),
            pipelineResult,
            "promotion-local-fresh-load-measurement-blocked",
            BuildArtifacts(),
            BuildGate(freshLoadPassed: true));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionPromotionDecisionKind.Reject, result.Value!.Decision.Kind);
        Assert.Contains("DESIGN_CONVERSION_MEASUREMENT_GATE_NOT_PASSED:failed", result.Value.Decision.Reasons);
        Assert.Contains("DESIGN_CONVERSION_PROMOTION_UPLOAD_BLOCKED_WITHOUT_MEASUREMENT_GATE",
            result.Value.Decision.EvidenceRefs);
        Assert.Equal(0, uploader.UploadCallCount);
        Assert.Null(result.Value.UploadReceipt);
        Assert.False(result.Value.Manifest.ModelServerUploadAttempted);
        Assert.False(result.Value.Manifest.ModelServerPromotionClaimed);
    }

    [Fact]
    public async Task PublishAsync_BlocksWhenMeasurementGatePassesButRealWeightsAreMissing()
    {
        var uploader = new FakePromotionUploader();
        var pipelineResult = BuildMeasuredPipelineResult(realWeightsProven: false);

        Assert.Equal(DesignConversionMeasurementGateStatus.Passed, pipelineResult.LifecycleEvidence.MeasurementGateStatus);
        Assert.NotNull(pipelineResult.LifecycleEvidence.MeasurementGate);
        Assert.False(pipelineResult.LifecycleEvidence.CheckpointContainsRealTrainedWeights);
        Assert.False(pipelineResult.LifecycleEvidence.PromotionUploadAllowed);

        var result = await new DesignConversionPromotionPublisher(uploader).PublishAsync(
            await CreateLoadedPromotionMachineAsync(),
            pipelineResult,
            "promotion-measurement-passed-real-weights-missing",
            BuildArtifacts(),
            BuildGate());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionPromotionDecisionKind.Reject, result.Value!.Decision.Kind);
        Assert.Contains("DESIGN_CONVERSION_REAL_TRAINED_WEIGHTS_NOT_PROVEN", result.Value.Decision.Reasons);
        Assert.Contains("DESIGN_CONVERSION_PROMOTION_UPLOAD_BLOCKED_REAL_TRAINED_WEIGHTS_NOT_PROVEN",
            result.Value.Decision.EvidenceRefs);
        Assert.Equal(0, uploader.UploadCallCount);
        Assert.Null(result.Value.UploadReceipt);
        Assert.False(result.Value.Manifest.ModelServerUploadAttempted);
        Assert.False(result.Value.Manifest.ModelServerPromotionClaimed);
    }

    [Fact]
    public async Task PublishAsync_RejectsFakePassedMeasurementGateStatusWithoutConcreteGateEvidence()
    {
        var uploader = new FakePromotionUploader();
        var pipelineResult = BuildMeasuredPipelineResult(
            realWeightsProven: true,
            includeConcreteMeasurementGate: false);

        Assert.Equal(DesignConversionMeasurementGateStatus.Passed, pipelineResult.LifecycleEvidence.MeasurementGateStatus);
        Assert.Null(pipelineResult.LifecycleEvidence.MeasurementGate);
        Assert.True(pipelineResult.LifecycleEvidence.PromotionUploadAllowed);

        var result = await new DesignConversionPromotionPublisher(uploader).PublishAsync(
            await CreateLoadedPromotionMachineAsync(),
            pipelineResult,
            "promotion-fake-measurement-status-blocked",
            BuildArtifacts(),
            BuildGate());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionPromotionDecisionKind.Reject, result.Value!.Decision.Kind);
        Assert.Contains("DESIGN_CONVERSION_MEASUREMENT_GATE_NOT_PASSED:passed", result.Value.Decision.Reasons);
        Assert.Contains(
            "DESIGN_CONVERSION_PROMOTION_UPLOAD_BLOCKED_WITHOUT_MEASUREMENT_GATE",
            result.Value.Decision.EvidenceRefs);
        Assert.Contains("MODEL_SERVER_UPLOAD_NOT_ATTEMPTED", result.Value.Decision.EvidenceRefs);
        Assert.Equal(0, uploader.UploadCallCount);
        Assert.Null(result.Value.UploadReceipt);
        Assert.False(result.Value.Manifest.ModelServerUploadAttempted);
        Assert.False(result.Value.Manifest.ModelServerPromotionClaimed);
    }

    [Fact]
    public async Task RollbackTrainingIntakeLocalPromotionAsync_RestoresPriorActiveLocalPromotionState()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "promotion-rollback-domain");
        var promotionOutputRoot = Path.Combine(root, "promotions");
        var db = new JsonlDesignConversionLocalArtifactDb(
            root,
            DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
            new DesignConversionDpoPolicyBaseTestProvider());
        var api = new DesignConversionDpoUsefulnessApi(db);

        try
        {
            var trainingRecords = BuildApprovedRollbackTrainingRecords("promotion-rollback-first");
            var rollbackArtifactRecord =
                await DesignConversionPostEvaluationEligibilityTestFixture
                    .AppendEligibleRecordAsync(
                        db,
                        BuildRollbackArtifactRecord(
                            "promotion-rollback-record",
                            "promotion-rollback-job",
                            "promotion-rollback-request",
                            trainingRecords));
            var rollbackArtifactEligibility =
                await db.EvaluatePostEvaluationTrainingEligibilityAsync(
                    rollbackArtifactRecord);
            Assert.True(
                rollbackArtifactEligibility.IsEligible,
                rollbackArtifactEligibility.Reason);

            var handoff = await api.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(JobId: "promotion-rollback-job"),
                DateTimeOffset.UnixEpoch.AddMinutes(501));
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            Assert.Equal(100, handoff.Value!.ApprovedCandidateCount);

            var intake = await api.CreateTrainingIntakeAsync(
                handoff.Value,
                "architect:promotion-rollback-domain-test",
                "focused domain rollback test; no product-ready claim",
                DateTimeOffset.UnixEpoch.AddMinutes(502));
            Assert.True(intake.IsSuccess, intake.ErrorMessage);
            Assert.True(intake.Value!.ProductNotReady);

            var firstPromotion = await RunPersistedLocalPromotionAsync(
                api,
                intake.Value.IntakeId,
                Path.Combine(promotionOutputRoot, "first"),
                "promotion-rollback-first-run",
                "promotion-rollback-first-dataset",
                "tenant:promotion-rollback-domain",
                "idempotency:promotion-rollback-first");

            var secondTrainingRecords = BuildApprovedRollbackTrainingRecords("promotion-rollback-second");
            var firstTeacherRequestIds = trainingRecords
                .Select(record => record.Provenance.SourceRequestId)
                .ToHashSet(StringComparer.Ordinal);
            var secondTeacherRequestIds = secondTrainingRecords
                .Select(record => record.Provenance.SourceRequestId)
                .ToHashSet(StringComparer.Ordinal);
            Assert.Equal(100, firstTeacherRequestIds.Count);
            Assert.Equal(100, secondTeacherRequestIds.Count);
            Assert.Empty(firstTeacherRequestIds.Intersect(secondTeacherRequestIds, StringComparer.Ordinal));

            var secondRollbackArtifactRecord =
                await DesignConversionPostEvaluationEligibilityTestFixture
                    .AppendEligibleRecordAsync(
                        db,
                        BuildRollbackArtifactRecord(
                            "promotion-rollback-second-record",
                            "promotion-rollback-second-job",
                            "promotion-rollback-second-request",
                            secondTrainingRecords));
            var secondRollbackArtifactEligibility =
                await db.EvaluatePostEvaluationTrainingEligibilityAsync(
                    secondRollbackArtifactRecord);
            Assert.True(
                secondRollbackArtifactEligibility.IsEligible,
                secondRollbackArtifactEligibility.Reason);

            var secondHandoff = await api.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(JobId: "promotion-rollback-second-job"),
                DateTimeOffset.UnixEpoch.AddMinutes(503));
            Assert.True(secondHandoff.IsSuccess, secondHandoff.ErrorMessage);
            Assert.Equal(100, secondHandoff.Value!.ApprovedCandidateCount);

            var secondIntake = await api.CreateTrainingIntakeAsync(
                secondHandoff.Value,
                "architect:promotion-rollback-domain-test",
                "second source-disjoint intake for focused domain rollback test; no product-ready claim",
                DateTimeOffset.UnixEpoch.AddMinutes(504));
            Assert.True(secondIntake.IsSuccess, secondIntake.ErrorMessage);
            Assert.True(secondIntake.Value!.ProductNotReady);

            var secondPromotion = await RunPersistedLocalPromotionAsync(
                api,
                secondIntake.Value.IntakeId,
                Path.Combine(promotionOutputRoot, "second"),
                "promotion-rollback-second-run",
                "promotion-rollback-second-dataset",
                "tenant:promotion-rollback-domain",
                "idempotency:promotion-rollback-second");

            Assert.True(firstPromotion.IsSuccess, firstPromotion.ErrorMessage);
            Assert.True(secondPromotion.IsSuccess, secondPromotion.ErrorMessage);
            Assert.True(firstPromotion.Value!.LocalPromotionReady);
            Assert.True(secondPromotion.Value!.LocalPromotionReady);
            Assert.Equal(bool.FalseString, firstPromotion.Value.BoundaryMetadata["product_ready"]);
            Assert.Equal(bool.FalseString, secondPromotion.Value.BoundaryMetadata["product_ready"]);

            var firstRef = firstPromotion.Value.BoundaryMetadata["local_promotion_active_local_model_ref"];
            var secondRef = secondPromotion.Value.BoundaryMetadata["local_promotion_active_local_model_ref"];
            var firstCheckpoint = firstPromotion.Value.BoundaryMetadata["local_promotion_active_checkpoint_sha256"];
            Assert.False(string.IsNullOrWhiteSpace(firstRef));
            Assert.False(string.IsNullOrWhiteSpace(secondRef));
            Assert.NotEqual(firstRef, secondRef);
            Assert.Equal("none", firstPromotion.Value.BoundaryMetadata["local_promotion_prior_state_kind"]);
            Assert.Equal(bool.FalseString, firstPromotion.Value.BoundaryMetadata["local_promotion_rollback_supported"]);
            Assert.Equal(firstRef, secondPromotion.Value.BoundaryMetadata["local_promotion_prior_active_local_model_ref"]);
            Assert.Equal(firstCheckpoint, secondPromotion.Value.BoundaryMetadata["local_promotion_prior_active_checkpoint_sha256"]);
            Assert.Equal(bool.TrueString, secondPromotion.Value.BoundaryMetadata["local_promotion_rollback_supported"]);

            var rollback = await api.RollbackTrainingIntakeLocalPromotionAsync(
                secondRef,
                "architect:promotion-rollback-domain-test",
                "rollback the current second local promotion and restore prior active local promotion",
                DateTimeOffset.UnixEpoch.AddMinutes(505));

            Assert.True(rollback.IsSuccess, rollback.ErrorMessage);
            Assert.True(rollback.Value!.RollbackApplied);
            Assert.True(rollback.Value.PriorStateRestored);
            Assert.False(rollback.Value.NoPriorStateRecorded);
            Assert.Equal(secondRef, rollback.Value.RolledBackLocalModelRef);
            Assert.Equal(secondRef, rollback.Value.PreviousActiveLocalModelRef);
            Assert.Equal(firstRef, rollback.Value.RestoredActiveLocalModelRef);
            Assert.Equal(firstCheckpoint, rollback.Value.RestoredActiveCheckpointSha256);
            Assert.Contains("DESIGN_CONVERSION_LOCAL_PROMOTION_ROLLBACK_RECORD_WRITTEN", rollback.Value.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_LOCAL_PROMOTION_ROLLBACK_PRIOR_ACTIVE_STATE_RESTORED", rollback.Value.EvidenceRefs);
            Assert.Equal("rollback", rollback.Value.BoundaryMetadata["local_promotion_record_kind"]);
            Assert.Equal(bool.TrueString, rollback.Value.BoundaryMetadata["local_promotion_rollback_applied"]);
            Assert.Equal(firstRef, rollback.Value.BoundaryMetadata["local_promotion_active_local_model_ref"]);
            Assert.Equal(bool.FalseString, rollback.Value.BoundaryMetadata["deployment_verified"]);
            Assert.Equal(bool.FalseString, rollback.Value.BoundaryMetadata["product_ready"]);

            var staleSecondRollback = await api.RollbackTrainingIntakeLocalPromotionAsync(
                secondRef,
                "architect:promotion-rollback-domain-test",
                "prove the rolled-back second promotion is no longer active",
                DateTimeOffset.UnixEpoch.AddMinutes(506));
            Assert.False(staleSecondRollback.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_LOCAL_PROMOTION_ROLLBACK_TARGET_NOT_ACTIVE",
                staleSecondRollback.ErrorCode);

            var promotionLog = Path.Combine(root, "training-intake-local-promotions.jsonl");
            var lines = await File.ReadAllLinesAsync(promotionLog);
            Assert.Equal(3, lines.Length);
            Assert.Contains("\"local_promotion_record_kind\":\"rollback\"", lines[2], StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static async Task Save(
        IDesignConversionLocalArtifactDb db,
        DesignConversionLocalArtifactDbRecord record)
    {
        var saved = await db.AppendRecordAsync(record);
        Assert.True(saved.IsSuccess, saved.ErrorMessage);
    }

    private static IReadOnlyList<DesignConversionDpoTrainingRecord> BuildApprovedRollbackTrainingRecords(
        string identityScope)
    {
        var build = new DesignConversionDpoGenerator().BuildTrainReadyRecords(
            BuildSourceDisjointExamples(100, identityScope));
        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.QuarantinedRecords);
        Assert.Equal(100, build.Value.AcceptedRecords.Count);

        return build.Value.AcceptedRecords
            .Select((record, index) => ToRuntimeApprovedCandidate(
                WithPersistedRuntimeArtifactRefs(
                    record,
                    $"{identityScope}-artifact-{index:000}")))
            .ToArray();
    }

    private static IReadOnlyList<DesignConversionDpoTrainingRecord>
        BuildApprovedRollbackTrainingRecords()
        => BuildApprovedRollbackTrainingRecords("promotion-rollback");

    private static Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>> RunPersistedLocalPromotionAsync(
        DesignConversionDpoUsefulnessApi api,
        string intakeId,
        string promotionOutputDirectory,
        string runId,
        string datasetId,
        string tenantId,
        string idempotencyKey)
        => DesignConversionTrainingIntakeRouteAdmissionContext.RunAsync(
            new Dictionary<string, string>
            {
                ["training_intake_tenant_id_ref"] = tenantId,
                ["training_intake_idempotency_key_ref"] = idempotencyKey
            },
            () => api.RunTrainingIntakeAndPromoteLocalAsync(
                intakeId,
                promotionOutputDirectory,
                new DesignConversionTrainingPipelineOptions(
                    RunId: runId,
                    BatchSize: 25,
                    DatasetOptions: LocalDatasetOptions(DatasetId: datasetId),
                    MeasurementGateRequest: BuildPassedMeasurementGateRequest())));

    private static DesignConversionDpoTrainingRecord ToRuntimeApprovedCandidate(
        DesignConversionDpoTrainingRecord record)
    {
        var metadata = new Dictionary<string, string>(record.Provenance.Metadata, StringComparer.Ordinal)
        {
            ["runtime_created_dpo_candidate"] = bool.TrueString,
            ["approval_required"] = bool.TrueString,
            ["review_required"] = bool.TrueString,
            ["approval_status"] = DesignConversionDpoUsefulnessApi.ApprovalApprovedStatus,
            ["review_status"] = DesignConversionDpoUsefulnessApi.RuntimeCandidateReviewApprovedStatus,
            ["approved_for_training"] = bool.TrueString,
            ["approved_by_ref"] = "reviewer:promotion-rollback-domain-test",
            ["approval_reason"] = "focused local promotion rollback domain test approval",
            ["approved_at_utc"] = DateTimeOffset.UnixEpoch.AddMinutes(500).ToString("O"),
            ["training_candidate_only"] = bool.TrueString,
            ["production_dpo"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString,
            ["split"] = "runtime_training_candidate_review_approved"
        };
        var licenseMetadata = new Dictionary<string, string>(record.License.Metadata, StringComparer.Ordinal)
        {
            ["approval_required"] = bool.TrueString,
            ["review_required"] = bool.TrueString,
            ["approval_status"] = DesignConversionDpoUsefulnessApi.ApprovalApprovedStatus,
            ["approved_for_training"] = bool.TrueString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString
        };

        return record with
        {
            Provenance = record.Provenance with
            {
                ReviewerRef = "reviewer:promotion-rollback-domain-test",
                Metadata = metadata
            },
            License = record.License with
            {
                Metadata = licenseMetadata
            }
        };
    }

    private static DesignConversionDpoTrainingRecord WithPersistedRuntimeArtifactRefs(
        DesignConversionDpoTrainingRecord record,
        string artifactId)
        => record with
        {
            Chosen = record.Chosen with
            {
                Html = $"artifact://design-conversion/{artifactId}/chosen.html",
                Css = $"artifact://design-conversion/{artifactId}/chosen.css",
                QaResults = [CreateRuntimeArtifactQaResult($"{artifactId}:chosen", score: 0.92, passed: true)]
            },
            Rejected = record.Rejected with
            {
                Html = $"artifact://design-conversion/{artifactId}/rejected.html",
                Css = $"artifact://design-conversion/{artifactId}/rejected.css",
                QaResults = [CreateRuntimeArtifactQaResult($"{artifactId}:rejected", score: 0.42, passed: false)]
            }
        };

    private static RenderQaResultRecord CreateRuntimeArtifactQaResult(
        string artifactId,
        double score,
        bool passed)
    {
        var resolution = new RenderQaResolution("desktop", 1440, 900, 1);
        var expected = new RenderQaImageRecord(
            ImageId: $"{artifactId}:expected",
            Label: "expected",
            Resolution: resolution,
            Uri: $"artifact://design-conversion/{artifactId}/expected.png",
            Sha256: $"sha256:{artifactId}:expected",
            Metadata: new Dictionary<string, string>());
        var actual = new RenderQaImageRecord(
            ImageId: $"{artifactId}:actual",
            Label: "actual",
            Resolution: resolution,
            Uri: $"artifact://design-conversion/{artifactId}/actual.png",
            Sha256: $"sha256:{artifactId}:actual",
            Metadata: new Dictionary<string, string>());
        var verdict = new VisualVerdict(
            VerdictId: $"{artifactId}:verdict",
            Score: score,
            Defects: [],
            EvidenceRefs: [$"artifact://design-conversion/{artifactId}/comparison-evidence"],
            Passed: passed);

        return new RenderQaResultRecord(
            ResultId: $"{artifactId}:qa-result",
            RequestId: $"{artifactId}:qa-request",
            Expected: expected,
            Actual: actual,
            Verdict: verdict,
            Metrics: new Dictionary<string, double> { ["visual_distance"] = 1.0 - score },
            Warnings: []);
    }

    private static DesignConversionLocalArtifactDbRecord BuildRollbackArtifactRecord(
        string recordId,
        string jobId,
        string requestId,
        IReadOnlyList<DesignConversionDpoTrainingRecord> records)
        => new(
            RecordId: recordId,
            DatabaseKind: JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
            JobId: jobId,
            RequestId: requestId,
            SourceKind: DesignConversionSourceKind.Figma.ToString(),
            Tier: "pro",
            SanitizedSourceRefs: new Dictionary<string, string>
            {
                ["source_manifest_ref"] = $"artifact://design-conversion/{recordId}/source-manifest.json",
                ["source_kind"] = "figma",
                ["design_tree_payload_ref"] = $"artifact://design-conversion/{recordId}/design-tree.json",
                ["original_tree_payload_ref"] = $"artifact://design-conversion/{recordId}/original-tree.json",
                ["source_artifact:manifest"] = $"artifact://design-conversion/{recordId}/source-manifest.json"
            },
            ConversionResult: new DesignConversionConversionResultArtifact(
                ResultId: $"{recordId}:conversion-result",
                HtmlRef: $"artifact://design-conversion/{recordId}/improved.html",
                HtmlSha256: string.Empty,
                CssRef: $"artifact://design-conversion/{recordId}/improved.css",
                CssSha256: string.Empty,
                Metadata: new Dictionary<string, string>
                {
                    ["conversion_result"] = "artifact_refs_only"
                }),
            RenderArtifactRefs:
            [
                new DesignConversionRenderArtifactDbRef(
                    ViewportId: "desktop",
                    ArtifactId: $"{recordId}:render:desktop",
                    Kind: "browser_screenshot",
                    UriRef: $"artifact://design-conversion/{recordId}/render-desktop.png",
                    Sha256: "sha256:render-desktop",
                    ByteLength: 1234,
                    Width: 1440,
                    Height: 900,
                    Metadata: new Dictionary<string, string>
                    {
                        ["render_artifact_refs"] = "browser_png"
                    })
            ],
            VisualJudgeStatus: new DesignConversionVisualJudgeArtifactDbStatus(
                Status: DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                ProviderBoundary: DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                ExternalCallExecuted: false,
                EstimatedCostUsd: null,
                TierDecision: DesignConversionVisualQaBoundaryPolicy.CostCapMarker,
                EvidenceRefs: [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker, "visual_judge_status", "product_not_ready"],
                Metadata: new Dictionary<string, string>
                {
                    ["visual_judge_status"] = DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker
                }),
            DpoCandidates: records
                .Select((record, index) => DesignConversionLocalArtifactDbDpoCandidate.FromTrainingRecord(
                    record,
                    $"promotion-rollback-candidate-{index:000}"))
                .ToArray(),
            CreatedAtUtc: DateTimeOffset.UnixEpoch,
            EvidenceRefs:
            [
                "storage_training_artifact_db",
                "local_artifact_db",
                "conversion_result",
                "render_artifact_refs",
                "visual_judge_status",
                "dpo_candidate",
                "usefulness_score",
                "training_candidate_only",
                "product_not_ready"
            ],
            Metadata: new Dictionary<string, string>
            {
                ["product_not_ready"] = bool.TrueString
            });

    private static DesignConversionTrainingPipelineResult BuildPipelineResult(
        bool callMachineLifecycleHook = false,
        DesignConversionMeasurementGateRequest? measurementGateRequest = null)
    {
        var build = new DesignConversionDpoGenerator().BuildTrainReadyRecords(BuildSourceDisjointExamples(100));
        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.QuarantinedRecords);

        var pipeline = new DesignConversionTrainingPipeline();
        var result = DesignConversionLegalTeacherCapabilityTestFactory.RunStrictPipelineAsync(
                pipeline,
                new DesignConversionMachine(),
                build.Value!.AcceptedRecords,
                new DesignConversionTrainingPipelineOptions(
                    "promotion-run",
                    BatchSize: 25,
                    CallMachineLifecycleHook: callMachineLifecycleHook,
                    DatasetOptions: LocalDatasetOptions(),
                    MeasurementGateRequest: measurementGateRequest))
            .GetAwaiter()
            .GetResult();

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(result.Value!.DatasetCountGate.Passed);
        return result.Value!;
    }

    private static DesignConversionTrainingPipelineResult BuildMeasuredPipelineResult(
        bool realWeightsProven = false,
        bool includeConcreteMeasurementGate = true)
    {
        var result = BuildPipelineResult(
            callMachineLifecycleHook: true,
            BuildPassedMeasurementGateRequest());
        AssertCallerAdmissionMeasurementIsAdvisoryOnly(result);

        var measurementGate = includeConcreteMeasurementGate
            ? BuildPassedMeasurementGate()
            : null;
        var syntheticMeasurementEvidenceRefs = measurementGate?.EvidenceRefs ??
        [
            "DESIGN_CONVERSION_MEASUREMENT_GATE_PASSED",
            DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
            "DESIGN_CONVERSION_TEST_MEASUREMENT_GATE_STATUS_ONLY"
        ];

        var syntheticResult = result with
        {
            LifecycleEvidence = result.LifecycleEvidence with
            {
                MeasurementGate = measurementGate,
                MeasurementGateStatus = DesignConversionMeasurementGateStatus.Passed,
                CheckpointContainsRealTrainedWeights = realWeightsProven,
                PromotionStatus = realWeightsProven
                    ? "PROMOTION_ALLOWED_BY_MEASUREMENT_GATE_AND_REAL_WEIGHTS"
                    : "PROMOTION_BLOCKED_REAL_TRAINED_WEIGHTS_NOT_PROVEN",
                PromotionUploadAllowed = realWeightsProven,
                NotProvenFields = BuildSyntheticMeasuredNotProvenFields(
                    result.LifecycleEvidence.NotProvenFields,
                    realWeightsProven),
                EvidenceRefs = BuildSyntheticMeasuredLifecycleEvidenceRefs(
                    result.LifecycleEvidence.EvidenceRefs,
                    syntheticMeasurementEvidenceRefs,
                    realWeightsProven),
                FailureReasons = []
            }
        };
        return AssertAndReturnSyntheticChildDpoMeasuredPromotionFixture(
            syntheticResult,
            realWeightsProven,
            includeConcreteMeasurementGate);
    }

    private static void AssertCallerAdmissionMeasurementIsAdvisoryOnly(
        DesignConversionTrainingPipelineResult result)
    {
        Assert.Equal(DesignConversionMeasurementGateStatus.Failed, result.LifecycleEvidence.MeasurementGateStatus);
        Assert.NotNull(result.LifecycleEvidence.MeasurementGate);
        Assert.Equal(
            DesignConversionMeasurementGateStatus.Failed,
            result.LifecycleEvidence.MeasurementGate!.Status);
        Assert.False(result.LifecycleEvidence.MeasurementGate.Passed);
        Assert.False(result.LifecycleEvidence.PromotionUploadAllowed);
        Assert.Contains("CALLER_ADMISSION_MEASUREMENT_ADVISORY_ONLY", result.LifecycleEvidence.EvidenceRefs);
        Assert.Contains("CALLER_ADMISSION_MEASUREMENT_IS_NOT_CHILD_DPO_ACCURACY", result.LifecycleEvidence.EvidenceRefs);
        Assert.Contains(
            "DESIGN_CONVERSION_CHILD_DPO_PROMOTION_AUTHORITY_NOT_AVAILABLE_IN_PIPELINE",
            result.LifecycleEvidence.EvidenceRefs);
        Assert.Contains(
            "DESIGN_CONVERSION_CHILD_DPO_MEASUREMENT_NOT_AVAILABLE_IN_TRAINING_PIPELINE",
            result.LifecycleEvidence.MeasurementGate.Reasons);
        Assert.Contains(
            "DESIGN_CONVERSION_CHILD_DPO_MEASUREMENT_NOT_AVAILABLE_IN_TRAINING_PIPELINE",
            result.LifecycleEvidence.FailureReasons);
    }

    private static IReadOnlyList<string> BuildSyntheticMeasuredLifecycleEvidenceRefs(
        IReadOnlyList<string> pipelineEvidenceRefs,
        IReadOnlyList<string> syntheticMeasurementEvidenceRefs,
        bool realWeightsProven)
        => pipelineEvidenceRefs
            .Where(IsSyntheticMeasuredLifecycleCarryForwardEvidence)
            .Concat(syntheticMeasurementEvidenceRefs)
            .Append(realWeightsProven
                ? "DESIGN_CONVERSION_TEST_ONLY_REAL_TRAINED_WEIGHTS_FLAG"
                : "DESIGN_CONVERSION_LEARNED_WEIGHTS_PRESENT_FALSE")
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static bool IsSyntheticMeasuredLifecycleCarryForwardEvidence(string reference)
    {
        if (reference.StartsWith("CALLER_ADMISSION_MEASUREMENT_", StringComparison.Ordinal))
            return false;
        if (reference.StartsWith("DESIGN_CONVERSION_CHILD_DPO_", StringComparison.Ordinal))
            return false;
        if (string.Equals(reference, "DESIGN_CONVERSION_MEASUREMENT_GATE_FAILED", StringComparison.Ordinal))
            return false;
        if (string.Equals(reference, "DESIGN_CONVERSION_MEASUREMENT_GATE_NOT_RUN", StringComparison.Ordinal))
            return false;
        if (string.Equals(
                reference,
                "DESIGN_CONVERSION_PROMOTION_UPLOAD_BLOCKED_WITHOUT_MEASUREMENT_GATE",
                StringComparison.Ordinal))
            return false;
        if (string.Equals(
                reference,
                DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGateBlockedEvidence,
                StringComparison.Ordinal))
            return false;

        return true;
    }

    private static IReadOnlyList<string> BuildSyntheticMeasuredNotProvenFields(
        IReadOnlyList<string> notProvenFields,
        bool realWeightsProven)
    {
        var syntheticNotProvenFields = notProvenFields
            .Where(field => !string.Equals(field, "held_out_measurement_gate", StringComparison.Ordinal))
            .Where(field => !string.Equals(field, "promotion_upload", StringComparison.Ordinal))
            .Where(field => realWeightsProven ||
                            !string.Equals(field, "real_trained_weights", StringComparison.Ordinal))
            .ToList();

        if (!realWeightsProven)
        {
            syntheticNotProvenFields.Add("promotion_upload");
            syntheticNotProvenFields.Add("real_trained_weights");
        }

        return syntheticNotProvenFields
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static void AssertSyntheticChildDpoMeasuredPromotionFixture(
        DesignConversionTrainingPipelineResult result,
        bool realWeightsProven,
        bool includeConcreteMeasurementGate)
    {
        Assert.Equal(DesignConversionMeasurementGateStatus.Passed, result.LifecycleEvidence.MeasurementGateStatus);
        Assert.Equal(realWeightsProven, result.LifecycleEvidence.PromotionUploadAllowed);
        Assert.Equal(realWeightsProven, result.LifecycleEvidence.CheckpointContainsRealTrainedWeights);
        Assert.DoesNotContain("CALLER_ADMISSION_MEASUREMENT_ADVISORY_ONLY", result.LifecycleEvidence.EvidenceRefs);
        Assert.DoesNotContain("CALLER_ADMISSION_MEASUREMENT_IS_NOT_CHILD_DPO_ACCURACY", result.LifecycleEvidence.EvidenceRefs);
        Assert.DoesNotContain("DESIGN_CONVERSION_MEASUREMENT_GATE_FAILED", result.LifecycleEvidence.EvidenceRefs);
        Assert.Empty(result.LifecycleEvidence.FailureReasons);
        Assert.Contains("DESIGN_CONVERSION_MEASUREMENT_GATE_PASSED", result.LifecycleEvidence.EvidenceRefs);
        Assert.Contains(
            DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
            result.LifecycleEvidence.EvidenceRefs);

        if (includeConcreteMeasurementGate)
        {
            Assert.NotNull(result.LifecycleEvidence.MeasurementGate);
            Assert.True(result.LifecycleEvidence.MeasurementGate!.Passed);
            Assert.Equal(
                DesignConversionMeasurementGateStatus.Passed,
                result.LifecycleEvidence.MeasurementGate.Status);
            Assert.Contains(
                "DESIGN_CONVERSION_MEASUREMENT_GATE_PASSED",
                result.LifecycleEvidence.MeasurementGate.EvidenceRefs);
            Assert.Contains(
                DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
                result.LifecycleEvidence.MeasurementGate.EvidenceRefs);
        }
        else
        {
            Assert.Null(result.LifecycleEvidence.MeasurementGate);
        }
    }

    private static DesignConversionTrainingPipelineResult AssertAndReturnSyntheticChildDpoMeasuredPromotionFixture(
        DesignConversionTrainingPipelineResult result,
        bool realWeightsProven,
        bool includeConcreteMeasurementGate)
    {
        AssertSyntheticChildDpoMeasuredPromotionFixture(
            result,
            realWeightsProven,
            includeConcreteMeasurementGate);
        return result;
    }

    private static DesignConversionMeasurementGateRequest BuildPassedMeasurementGateRequest()
        => new(
            GateId: "promotion-test-measurement-gate",
            UnitId: DesignConversionDpoUnits.CriticUnit,
            HeldOutMetrics: new DesignConversionHeldOutMeasurementMetrics(
                HeldOutRecordCount: 50,
                HeldOutQualityScore: 0.94,
                RuntimeMs: 320,
                CostUsd: 0m,
                PeakRamMb: 256,
                EvidenceRefs: ["ref:promotion-test-held-out-metrics:pass"]),
            IntegrityProof: new DesignConversionMeasurementIntegrityProof(
                SourceDisjoint: true,
                LeakageDetected: false,
                HiddenDeterminismDetected: false,
                AblationPassed: true,
                EvidenceRefs: ["ref:promotion-test-anti-leakage-ablation:pass"]),
            Thresholds: new DesignConversionMeasurementGateThresholds(
                RequiredHeldOutQualityScore: 0.90,
                MaxRuntimeMs: 800,
                MaxCostUsd: 1.00m,
                MaxPeakRamMb: 512));

    private static DesignConversionTrainingPipelineResult BuildCountGateFailedPipelineResult()
    {
        var build = new DesignConversionDpoGenerator().BuildTrainReadyRecords(BuildSourceDisjointExamples(3));
        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.QuarantinedRecords);

        var pipeline = new DesignConversionTrainingPipeline();
        var result = DesignConversionLegalTeacherCapabilityTestFactory.RunStrictPipelineAsync(
                pipeline,
                new DesignConversionMachine(),
                build.Value!.AcceptedRecords,
                new DesignConversionTrainingPipelineOptions(
                    "promotion-count-gate-failed",
                    BatchSize: 2,
                    DatasetOptions: LocalDatasetOptions()))
            .GetAwaiter()
            .GetResult();

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.DatasetCountGate.Passed);
        Assert.Equal(DesignConversionTrainingBoundaryDecisionKind.CountGateFailed, result.Value.BoundaryDecision.Kind);
        return result.Value!;
    }

    private static IReadOnlyList<DesignConversionStoredDpoExample> BuildSourceDisjointExamples(
        int count,
        string? identityScope = null)
        => Enumerable.Range(0, count)
            .Select(index =>
            {
                var unitId = index % 2 == 0
                    ? DesignConversionDpoUnits.CriticUnit
                    : DesignConversionDpoUnits.ImproverUnit;
                var identityPrefix = string.IsNullOrWhiteSpace(identityScope)
                    ? "promotion"
                    : identityScope;
                var sourceId = $"{identityPrefix}-source-{index:000}";
                var example = DesignConversionDpoGeneratorTests.CreateExample(
                    $"{identityPrefix}-record-{index:000}",
                    unitId,
                    usefulness: 0.8 + (index % 10 * 0.01));

                return example with
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
            })
            .ToArray();

    private static DesignConversionDpoDatasetOptions LocalDatasetOptions(
        string DatasetId = "dcm-dpo-promotion-dataset",
        int MinimumTrainRecords = 50,
        int MinimumHeldOutRecords = 50)
        => new(
            DatasetId: DatasetId,
            MinimumTrainRecords: MinimumTrainRecords,
            MinimumHeldOutRecords: MinimumHeldOutRecords,
            Preflight: DesignConversionDpoDatasetPreflight.LocalFixtureTrainingCandidateOnly());

    private static DesignConversionTrainingManifestArtifacts BuildArtifacts()
        => new(
            CheckpointPath: "artifacts/design-conversion/checkpoint.json",
            TokenizerPath: "artifacts/design-conversion/tokenizer.json",
            PolicyPath: "artifacts/design-conversion/policy.bin");

    private async Task<DesignConversionMachine> CreateLoadedPromotionMachineAsync()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-promotion-machine-{Guid.NewGuid():N}");
        _loadedMachineTempRoots.Add(root);
        var checkpointPath = Path.Combine(root, "checkpoint.json");
        var source = new DesignConversionMachine();
        var save = await source.SaveCheckpointAsync(checkpointPath);
        Assert.True(save.IsSuccess, save.ErrorMessage);
        var load = await DesignConversionMachine.LoadCheckpointAsync(checkpointPath);
        Assert.True(load.IsSuccess, load.ErrorMessage);
        var export = await load.Value!.Machine.ExportAsync();
        Assert.True(export.IsSuccess, export.ErrorMessage);
        Assert.Equal(Path.GetFullPath(checkpointPath), export.Value!.CheckpointRef);
        return load.Value.Machine;
    }

    private static DesignConversionPromotionGateReport BuildGate(
        double heldOutScore = 0.94,
        double requiredHeldOutScore = 0.90,
        bool heldOutSourceDisjoint = true,
        bool deterministicShortcutDetected = false,
        bool hiddenShortcutEvidenceDetected = false,
        bool freshLoadPassed = true,
        bool budgetTierPolicyPassed = true,
        int dpoQuarantineCount = 0,
        int maxDpoQuarantineCount = 1,
        double continueTrainingMargin =
            DesignConversionPromotionGateDefaults.ContinueTrainingMargin)
    {
        var artifacts = BuildArtifacts();
        var artifactPaths = new[]
        {
            artifacts.CheckpointPath,
            artifacts.TokenizerPath,
            artifacts.PolicyPath
        };

        return new DesignConversionPromotionGateReport(
            HeldOutScore: heldOutScore,
            RequiredHeldOutScore: requiredHeldOutScore,
            HeldOutSourceDisjoint: heldOutSourceDisjoint,
            DeterministicShortcutDetected: deterministicShortcutDetected,
            HiddenShortcutEvidenceDetected: hiddenShortcutEvidenceDetected,
            FreshLoadPassed: freshLoadPassed,
            BudgetTierPolicyPassed: budgetTierPolicyPassed,
            BudgetTier: "bounded-test-tier",
            DpoQuarantineCount: dpoQuarantineCount,
            MaxDpoQuarantineCount: maxDpoQuarantineCount,
            RequiredArtifactPaths: artifactPaths,
            PresentArtifactPaths: artifactPaths,
            EvidenceRefs: ["HELD_OUT_SCORE_FAKE_TEST_EVIDENCE"],
            ContinueTrainingMargin: continueTrainingMargin);
    }

    private sealed class FakePromotionUploader : IDesignConversionPromotionUploader
    {
        public int UploadCallCount { get; private set; }

        public Task<OperationResult<DesignConversionPromotionUploadReceipt>> UploadAsync(
            DesignConversionTrainingManifest manifest,
            DesignConversionPromotionGateReport gateReport,
            CancellationToken ct = default)
        {
            UploadCallCount++;
            return Task.FromResult(OperationResult<DesignConversionPromotionUploadReceipt>.Ok(
                new DesignConversionPromotionUploadReceipt(
                    UploadId: "fake-upload-1",
                    RemoteRef: "http://models.example.test/design-conversion/promoted",
                    UploadedAtUtc: DateTimeOffset.UnixEpoch,
                    EvidenceRefs:
                    [
                        "MODEL_SERVER_UPLOAD_VERIFIED_REMOTE_RECEIPT",
                        "PROMOTION_RECEIPT_KIND:model_server_upload_verified",
                        $@"FAKE_PROMOTION_UPLOADER_CALLED:C:\Users\lubao\private\PRIVATE_REMOTE_PROMOTION_RECEIPT_SECRET\checkpoint.json?token=credential"
                    ])
                {
                    PublicationKind = DesignConversionPromotionPublicationKind.VerifiedModelServerUpload,
                    UploadAttempted = true,
                    UploadSucceeded = true,
                    VerificationSucceeded = true,
                    PublicationStatus = DesignConversionPromotionPublicationStatus.UploadedVerified,
                    ProvenanceRef = "model-server-upload-provenance:fake-upload-1"
                }));
        }
    }

    private sealed class LocalReceiptPromotionUploader : IDesignConversionPromotionUploader
    {
        public int UploadCallCount { get; private set; }

        public Task<OperationResult<DesignConversionPromotionUploadReceipt>> UploadAsync(
            DesignConversionTrainingManifest manifest,
            DesignConversionPromotionGateReport gateReport,
            CancellationToken ct = default)
        {
            UploadCallCount++;
            return Task.FromResult(OperationResult<DesignConversionPromotionUploadReceipt>.Ok(
                new DesignConversionPromotionUploadReceipt(
                    UploadId: "local-receipt-1",
                    RemoteRef: DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix +
                               Uri.EscapeDataString(manifest.ManifestId),
                    UploadedAtUtc: DateTimeOffset.UnixEpoch,
                    EvidenceRefs:
                    [
                        "MODEL_SERVER_UPLOAD_VERIFIED_REMOTE_RECEIPT",
                        "model_server_upload_verified_remote_receipt",
                        $"PROMOTION_PUBLICATION_STATUS:{DesignConversionPromotionPublicationStatus.UploadedVerified}"
                    ])
                {
                    PublicationKind = DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt,
                    UploadAttempted = false,
                    UploadSucceeded = false,
                    VerificationSucceeded = false,
                    PublicationStatus = DesignConversionPromotionPublicationStatus.NotUploaded
                }));
        }
    }

    private static string[] AssertStrictUploadReceiptEvidenceHashes(
        IReadOnlyList<string> evidenceRefs,
        int expectedCount)
    {
        const string prefix = "UPLOAD_RECEIPT_EVIDENCE_SHA256:";
        var hashes = evidenceRefs
            .Where(reference => reference.StartsWith(prefix, StringComparison.Ordinal))
            .ToArray();

        Assert.Equal(expectedCount, hashes.Length);
        Assert.All(hashes, reference =>
        {
            var digest = reference[prefix.Length..];
            Assert.Equal(64, digest.Length);
            Assert.All(digest, character => Assert.True(
                character is >= '0' and <= '9' or >= 'a' and <= 'f',
                $"Receipt evidence digest contains non-lowercase-hex character: {character}"));
        });
        return hashes;
    }

    private static string EncodeReceiptPathSegment(string value, int layers)
    {
        var encoded = value;
        for (var layer = 0; layer < layers; layer++)
            encoded = Uri.EscapeDataString(encoded);
        return encoded;
    }

    private static string DecodeReceiptRefOnceForAssertion(string value)
    {
        try
        {
            return Uri.UnescapeDataString(value);
        }
        catch (UriFormatException)
        {
            return value;
        }
    }

    private static string BuildPublicReceiptEvidence(
        DesignConversionPromotionPublicationResult publication)
    {
        IEnumerable<string> publicSurface = publication.Decision.Reasons
            .Concat(publication.Decision.EvidenceRefs)
            .Concat(publication.Manifest.EvidenceRefs)
            .Append(publication.Manifest.PromotionUploadRef ?? string.Empty)
            .Append(publication.Manifest.PromotionReceiptKind)
            .Append(publication.PublicationStatus);

        if (publication.UploadReceipt is { } receipt)
        {
            publicSurface = publicSurface
                .Append(receipt.UploadId)
                .Append(receipt.RemoteRef)
                .Append(receipt.ProvenanceRef)
                .Append(receipt.PublicationStatus)
                .Concat(receipt.EvidenceRefs ?? []);
        }

        return string.Join('|', publicSurface);
    }

    private sealed class ReceiptPromotionUploader(DesignConversionPromotionUploadReceipt receipt)
        : IDesignConversionPromotionUploader
    {
        public int UploadCallCount { get; private set; }

        public Task<OperationResult<DesignConversionPromotionUploadReceipt>> UploadAsync(
            DesignConversionTrainingManifest manifest,
            DesignConversionPromotionGateReport gateReport,
            CancellationToken ct = default)
        {
            UploadCallCount++;
            return Task.FromResult(OperationResult<DesignConversionPromotionUploadReceipt>.Ok(receipt));
        }
    }


    [Fact]
    public async Task RunTrainingIntakeAndPromoteLocalAsync_CallerAdmissionDoesNotCreateRollbackState()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "promotion-rollback-domain");
        var promotionOutputRoot = Path.Combine(root, "promotions");
        var uploader = new UntrustedBoundedPromotionUploader();
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var db = new JsonlDesignConversionLocalArtifactDb(
            root,
            DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
            provider,
            promotionUploader: uploader);
        var api = new DesignConversionDpoUsefulnessApi(db);

        try
        {
            var trainingRecords = BuildApprovedRollbackTrainingRecords();
            var rollbackArtifactRecord =
                await DesignConversionPostEvaluationEligibilityTestFixture
                    .AppendEligibleRecordAsync(
                        db,
                        BuildRollbackArtifactRecord(
                            "promotion-rollback-record",
                            "promotion-rollback-job",
                            "promotion-rollback-request",
                            trainingRecords));
            var rollbackArtifactEligibility =
                await db.EvaluatePostEvaluationTrainingEligibilityAsync(
                    rollbackArtifactRecord);
            Assert.True(
                rollbackArtifactEligibility.IsEligible,
                rollbackArtifactEligibility.Reason);

            var handoff = await api.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(JobId: "promotion-rollback-job"),
                DateTimeOffset.UnixEpoch.AddMinutes(501));
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            Assert.Equal(100, handoff.Value!.ApprovedCandidateCount);

            var intake = await api.CreateTrainingIntakeAsync(
                handoff.Value,
                "architect:promotion-rollback-domain-test",
                "focused domain rollback test; no product-ready claim",
                DateTimeOffset.UnixEpoch.AddMinutes(502));
            Assert.True(intake.IsSuccess, intake.ErrorMessage);
            Assert.True(intake.Value!.ProductNotReady);

            var firstPromotion = await RunPersistedLocalPromotionAsync(
                api,
                intake.Value.IntakeId,
                Path.Combine(promotionOutputRoot, "first"),
                "promotion-rollback-first-run",
                "promotion-rollback-first-dataset",
                "tenant:promotion-rollback-domain",
                "idempotency:promotion-rollback-first");

            Assert.True(firstPromotion.IsSuccess, firstPromotion.ErrorMessage);
            AssertRejectedBudgetPromotionWithoutRollbackState(firstPromotion.Value!);
            Assert.Equal(0, uploader.UploadCallCount);

            var promotionLog = Path.Combine(root, "training-intake-local-promotions.jsonl");
            var operationLog = Path.Combine(
                root,
                JsonlDesignConversionLocalArtifactDb
                    .TrainingIntakeLocalPromotionOperationFileName);
            var pairReservationPath = Path.Combine(
                root,
                JsonlDesignConversionLocalArtifactDb
                    .TrainingIntakeDpoPairStoreDirectoryName,
                ".dpo-pair-batch-reservation.json");
            var promotionLines = await ReadNonEmptyLinesAsync(promotionLog);
            Assert.Single(promotionLines);
            var firstPromotionRow = System.Text.Json.JsonSerializer
                .Deserialize<DesignConversionTrainingIntakeLocalPromotionResult>(
                    promotionLines[0],
                    DesignConversionCheckpointJsonOptions.Standard);
            Assert.NotNull(firstPromotionRow);
            AssertRejectedBudgetPromotionWithoutRollbackState(firstPromotionRow!);
            var operationPhaseNumbers =
                await ReadLocalPromotionOperationPhaseNumbersAsync(operationLog);
            Assert.Equal(new[] { 0, 1, 2, 3 }, operationPhaseNumbers);
            Assert.False(File.Exists(pairReservationPath), pairReservationPath);
            var operationLines = await ReadNonEmptyLinesAsync(operationLog);
            var providerInvocationCountAfterFirst = provider.InvocationCount;

            var replayPromotion = await RunPersistedLocalPromotionAsync(
                api,
                intake.Value.IntakeId,
                Path.Combine(promotionOutputRoot, "first"),
                "promotion-rollback-first-run",
                "promotion-rollback-first-dataset",
                "tenant:promotion-rollback-domain",
                "idempotency:promotion-rollback-first");
            Assert.True(replayPromotion.IsSuccess, replayPromotion.ErrorMessage);
            AssertRejectedBudgetPromotionWithoutRollbackState(replayPromotion.Value!);
            Assert.Equal(
                bool.TrueString,
                replayPromotion.Value!.BoundaryMetadata[
                    "training_intake_idempotency_replay_hit"]);
            Assert.Equal(providerInvocationCountAfterFirst, provider.InvocationCount);
            Assert.Equal(0, uploader.UploadCallCount);
            Assert.Equal(operationLines, await ReadNonEmptyLinesAsync(operationLog));
            Assert.Equal(promotionLines, await ReadNonEmptyLinesAsync(promotionLog));

            const string secondTenant = "tenant:promotion-rollback-domain";
            const string secondIdempotencyKey =
                "idempotency:promotion-rollback-second";
            var secondOperationId = BuildPromotionOperationId(
                secondTenant,
                secondIdempotencyKey);
            var secondOutputReservationPath =
                BuildPromotionOutputReservationPath(root, secondOperationId);
            var secondPromotion = await RunPersistedLocalPromotionAsync(
                api,
                intake.Value.IntakeId,
                Path.Combine(promotionOutputRoot, "second"),
                "promotion-rollback-second-run",
                "promotion-rollback-second-dataset",
                secondTenant,
                secondIdempotencyKey);

            Assert.False(secondPromotion.IsSuccess);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.DuplicateRequestIdErrorCode,
                secondPromotion.ErrorCode);
            Assert.Equal(
                operationLines,
                await ReadNonEmptyLinesAsync(operationLog));
            Assert.Equal(
                promotionLines,
                await ReadNonEmptyLinesAsync(promotionLog));
            Assert.DoesNotContain(
                await ReadNonEmptyLinesAsync(operationLog),
                line => LineHasJsonString(line, "operationId", secondOperationId) ||
                        LineHasJsonString(
                            line,
                            "idempotencyKeyRef",
                            secondIdempotencyKey));
            Assert.False(File.Exists(secondOutputReservationPath), secondOutputReservationPath);
            Assert.False(File.Exists(pairReservationPath), pairReservationPath);
            Assert.False(Directory.Exists(Path.Combine(promotionOutputRoot, "second")));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static void AssertRejectedBudgetPromotionWithoutRollbackState(
        DesignConversionTrainingIntakeLocalPromotionResult promotion)
    {
        Assert.False(promotion.LocalPromotionReady);
        Assert.False(promotion.TrainingRun.ModelUploadReady);
        Assert.Equal(
            DesignConversionPromotionPublicationKind.Rejected,
            promotion.Publication!.PublicationKind);
        Assert.Contains("BUDGET_TIER_POLICY_FAILED", promotion.BlockReasons);
        Assert.Equal(bool.FalseString, promotion.BoundaryMetadata["product_ready"]);
        Assert.False(promotion.BoundaryMetadata.ContainsKey(
            "local_promotion_active_local_model_ref"));
        Assert.Equal(
            bool.FalseString,
            promotion.BoundaryMetadata["local_promotion_rollback_supported"]);
    }

    private static async Task<string[]> ReadNonEmptyLinesAsync(string path)
    {
        if (!File.Exists(path))
            return [];
        return (await File.ReadAllLinesAsync(path))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();
    }

    private static async Task<int[]> ReadLocalPromotionOperationPhaseNumbersAsync(
        string path)
    {
        var lines = await ReadNonEmptyLinesAsync(path);
        return lines.Select(line =>
        {
            using var document = System.Text.Json.JsonDocument.Parse(line);
            return document.RootElement.GetProperty("phase").GetInt32();
        }).ToArray();
    }

    private static string BuildPromotionOutputReservationPath(
        string root,
        string operationId)
        => Path.Combine(
            root,
            ".training-intake-local-promotion-output-reservations",
            $"{Sha256Text(operationId)}.json");

    private static string BuildPromotionOperationId(
        string tenantId,
        string idempotencyKey)
        => $"local-promotion-operation-{Sha256Text(string.Join("|",
        [
            JsonlDesignConversionLocalArtifactDb
                .TrainingIntakeLocalPromotionOperationSchemaVersion,
            tenantId,
            idempotencyKey
        ]))}";

    private static bool LineHasJsonString(
        string line,
        string propertyName,
        string expectedValue)
    {
        using var document = System.Text.Json.JsonDocument.Parse(line);
        return document.RootElement.TryGetProperty(
                   propertyName,
                   out var property) &&
               string.Equals(
                   property.GetString(),
                   expectedValue,
                   StringComparison.Ordinal);
    }

    private static string Sha256Text(string value)
        => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private sealed class UntrustedBoundedPromotionUploader :
        IDesignConversionBoundedPromotionUploader,
        IDesignConversionPromotionUploaderPreflight
    {
        public int UploadCallCount { get; private set; }

        public int MaximumSerializedReceiptUtf8Bytes => 64 * 1024;

        public OperationResult<string> PreflightConfigurationFingerprintSha256()
            => OperationResult<string>.Ok("sha256:" + new string('c', 64));

        public Task<OperationResult<DesignConversionPromotionUploadReceipt>> UploadAsync(
            DesignConversionTrainingManifest manifest,
            DesignConversionPromotionGateReport gateReport,
            CancellationToken ct = default)
        {
            UploadCallCount++;
            throw new InvalidOperationException(
                "Budget-blocked untrusted test uploader must not be called.");
        }
    }


    private static DesignConversionMeasurementGateResult BuildPassedMeasurementGate()
        => new(
            GateId: "promotion-test-measurement-gate",
            UnitId: DesignConversionDpoUnits.CriticUnit,
            Status: DesignConversionMeasurementGateStatus.Passed,
            Passed: true,
            RequiredHeldOutQualityScore: 0.90,
            HeldOutQualityScore: 0.94,
            SourceDisjoint: true,
            AblationPassed: true,
            BudgetPassed: true,
            Reasons: ["ALL_MEASUREMENT_GATES_PASSED"],
            EvidenceRefs:
            [
                "DESIGN_CONVERSION_MEASUREMENT_GATE_PASSED",
                DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
                "promotion-test-held-out-metrics:pass",
                "promotion-test-anti-leakage-ablation:pass"
            ]);

}
