// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace LlmFromScratch.Tests;

using AgentInfrastructure.Logical;
using ContinuousLearning;

public sealed class GenericDpoTrainingContentAuthorityTests
{
    [Fact]
    public void Bind_NullPairReturnsTypedNullPairCode()
    {
        var result = DpoPairTrainingContentAuthority.Bind(null);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("NULL_DPO_PAIR", result.ErrorCode);
        Assert.Equal("DPO pair is required.", result.ErrorMessage);
    }

    [Fact]
    public void Bind_MalformedNonNullPairKeepsInvalidPairCompatibility()
    {
        var capturedAt = DateTimeOffset.Parse("2026-08-16T12:00:00Z");
        var malformedPairs = new[]
        {
            new DpoPair(null!, "chosen", "rejected", "BUG", capturedAt),
            new DpoPair("prompt", "chosen", "rejected", null!, capturedAt),
            new DpoPair(
                "prompt",
                new string('x', BoundedDpoTrainingContentProjector.MaximumTextUtf8Bytes + 1),
                "rejected",
                "BUG",
                capturedAt)
        };

        foreach (var pair in malformedPairs)
        {
            var result = DpoPairTrainingContentAuthority.Bind(pair);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.Equal("INVALID_DPO_PAIR", result.ErrorCode);
            Assert.Equal("DPO pair fields are invalid.", result.ErrorMessage);
        }
    }

    [Fact]
    public void Bind_ValidPairWritesExactProjectedContentSha()
    {
        var pair = new DpoPair(
            "prompt",
            "chosen",
            "rejected",
            "BUG",
            DateTimeOffset.Parse("2026-08-16T12:00:00Z"))
        {
            TeacherProvenance = Provenance("exact-bind")
        };
        var projected = DpoPairTrainingContentAuthority.Project([pair]);
        Assert.True(projected.IsSuccess, projected.ErrorMessage);

        var result = DpoPairTrainingContentAuthority.Bind(pair);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value.TeacherProvenance);
        Assert.Equal(
            projected.Value!.Rows[0].Sha256,
            result.Value.TeacherProvenance.TrainingContentSha256);
        Assert.Equal(pair.Prompt, result.Value.Prompt);
        Assert.Equal(pair.Chosen, result.Value.Chosen);
        Assert.Equal(pair.Rejected, result.Value.Rejected);
        Assert.Equal(pair.Domain, result.Value.Domain);
        Assert.Equal(pair.CapturedAt, result.Value.CapturedAt);
    }

    [Fact]
    public void ExactBoundContent_IsAdmitted_AndReceiptCarriesBinding()
    {
        var content = Project("prompt", "chosen", "rejected", [0.25, -0.5]);
        var provenance = Provenance("exact-positive") with
        {
            TrainingContentSha256 = content.Rows[0].Sha256
        };
        var lease = CreateAuthority().AcquireTrainingLease(
            content,
            [provenance]);

        Assert.True(lease.IsSuccess, lease.ErrorMessage);
        using var ownedLease = lease.Value!;
        Assert.NotEmpty(ownedLease.Admission.TrainingContentBatchRef);
        Assert.NotEmpty(ownedLease.Admission.Receipts[0].TrainingContentRef);
    }

    [Fact]
    public void AlteredRowOrVector_CannotReuseBoundProvenance()
    {
        var original = Project(
            "prompt",
            "chosen",
            "rejected",
            [0.25, -0.5]);
        var provenance = Provenance("transplant") with
        {
            TrainingContentSha256 = original.Rows[0].Sha256
        };
        var alteredText = Project(
            "prompt",
            "altered-chosen",
            "rejected",
            [0.25, -0.5]);
        var alteredVector = Project(
            "prompt",
            "chosen",
            "rejected",
            [0.25, -0.25]);
        var authority = CreateAuthority();

        var textResult = authority.AcquireTrainingLease(
            alteredText,
            [provenance]);
        var vectorResult = authority.AcquireTrainingLease(
            alteredVector,
            [provenance]);

        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority
                .TrainingContentInvalidErrorCode,
            textResult.ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority
                .TrainingContentInvalidErrorCode,
            vectorResult.ErrorCode);
    }

    [Fact]
    public void RepeatedRequestId_IsRejectedForDifferentAndExactDuplicateRows()
    {
        var first = Project("p1", "c1", "r1", [1.0]);
        var second = Project("p2", "c2", "r2", [2.0]);
        var differentRows = BoundedDpoTrainingContentProjector.Project(
        [
            Input("p1", "c1", "r1", [1.0]),
            Input("p2", "c2", "r2", [2.0])
        ]).Value!;
        var exactRows = BoundedDpoTrainingContentProjector.Project(
        [
            Input("p1", "c1", "r1", [1.0]),
            Input("p1", "c1", "r1", [1.0])
        ]).Value!;
        var different = CreateAuthority().AcquireTrainingLease(
            differentRows,
            [
                Provenance("duplicate") with
                {
                    TrainingContentSha256 = first.Rows[0].Sha256
                },
                Provenance("duplicate") with
                {
                    TrainingContentSha256 = second.Rows[0].Sha256
                }
            ]);
        var exact = CreateAuthority().AcquireTrainingLease(
            exactRows,
            [
                Provenance("exact-duplicate") with
                {
                    TrainingContentSha256 = exactRows.Rows[0].Sha256
                },
                Provenance("exact-duplicate") with
                {
                    TrainingContentSha256 = exactRows.Rows[1].Sha256
                }
            ]);

        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority
                .DuplicateRequestIdErrorCode,
            different.ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority
                .DuplicateRequestIdErrorCode,
            exact.ErrorCode);
    }

    [Fact]
    public void Projector_RejectsOverboundTextAndVectorBeforeSnapshotCopy()
    {
        var oversizedText = new string(
            'x',
            BoundedDpoTrainingContentProjector.MaximumTextUtf8Bytes + 1);
        var textResult = BoundedDpoTrainingContentProjector.Project(
            [Input(oversizedText, "chosen", "rejected", [])]);
        var vectorResult = BoundedDpoTrainingContentProjector.Project(
            [new DpoTrainingContentRowInput(
                "prompt",
                "chosen",
                "rejected",
                new OverboundVector(),
                "test-context-v1")]);

        Assert.False(textResult.IsSuccess);
        Assert.Equal(
            BoundedDpoTrainingContentProjector.LimitExceededErrorCode,
            vectorResult.ErrorCode);
    }

    private static DpoTrainingContentBatch Project(
        string prompt,
        string chosen,
        string rejected,
        IReadOnlyList<double> vector)
        => BoundedDpoTrainingContentProjector.Project(
            [Input(prompt, chosen, rejected, vector)]).Value!;

    private static DpoTrainingContentRowInput Input(
        string prompt,
        string chosen,
        string rejected,
        IReadOnlyList<double> vector)
        => new(
            prompt,
            chosen,
            rejected,
            vector,
            "test-context-v1");

    private static ServerOwnedLegalTrainingTeacherAdmissionAuthority
        CreateAuthority()
    {
        var configuration = ContinuousLearningTp1DpoConfiguration.Create(
            "qwen3.5:9b",
            new string('d', 64),
            new string('c', 64),
            16,
            $"sha256:{new string('a', 64)}",
            $"sha256:{new string('b', 64)}",
            Enumerable.Range(1, 32)
                .Select(static value => (byte)value)
                .ToArray()).Value!;
        return ServerOwnedLegalTrainingTeacherAdmissionAuthority
            .IssueFromAdmittedConfiguration(
                configuration,
                static () => true);
    }

    private static TrainingTeacherProvenance Provenance(string requestId)
        => new(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            "qwen3.5:9b",
            new string('d', 64),
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            requestId,
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                LegalTrainingTeacherAdmissionAuthority
                    .OllamaModelLayerSha256DigestKind,
            ModelArtifactDigest = new string('c', 64)
        };

    private sealed class OverboundVector : IReadOnlyList<double>
    {
        public int Count =>
            BoundedDpoTrainingContentProjector.MaximumVectorLength + 1;

        public double this[int index] =>
            throw new InvalidOperationException(
                "The projector must reject Count before indexing.");

        public IEnumerator<double> GetEnumerator()
            => throw new InvalidOperationException(
                "The projector must reject Count before enumeration.");

        System.Collections.IEnumerator
            System.Collections.IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
