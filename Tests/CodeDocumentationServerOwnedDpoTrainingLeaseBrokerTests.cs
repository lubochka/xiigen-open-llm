// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;

public sealed class CodeDocumentationServerOwnedDpoTrainingLeaseBrokerTests
{
    [Fact]
    public void CodeDocumentationLease_UsesBoundPathDistinctNamespaceAndExactContentIdentity()
    {
        var trainingContent = ProjectContent("primary");
        var changedContent = ProjectContent("changed");
        var unboundValidatorCallCount = 0;
        DpoTrainingContentBatch? observedContent = null;
        TrainingTeacherProvenance?[]? observedProvenance = null;
        var authority = CreateAuthority(
            _ =>
            {
                unboundValidatorCallCount++;
                return OperationResult<LegalTrainingTeacherAdmission>.Fail(
                    "The unbound validator must not admit CodeDocumentation DPO.",
                    "UNBOUND_VALIDATOR_CALLED");
            },
            (content, provenance) =>
            {
                observedContent = content;
                observedProvenance = provenance?.ToArray();
                return OperationResult<LegalTrainingTeacherAdmission>.Ok(
                    CreateAdmission());
            });

        using var lease = AssertSuccessfulCodeDocumentationLease(
            authority,
            trainingContent,
            "code-doc-run:2026.09.06-primary");

        Assert.Equal(0, unboundValidatorCallCount);
        Assert.Same(trainingContent, observedContent);
        var provenance = Assert.Single(observedProvenance!);
        Assert.NotNull(provenance);
        Assert.StartsWith(
            "code-documentation-dpo:",
            provenance!.RequestId,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "translation-dpo:",
            provenance.RequestId,
            StringComparison.Ordinal);
        Assert.Contains(
            trainingContent.Sha256,
            provenance.RequestId,
            StringComparison.Ordinal);
        Assert.Equal(
            trainingContent.Rows[0].Sha256,
            provenance.TrainingContentSha256);
        Assert.True(lease.MatchesExactTrainingContent(trainingContent));
        Assert.False(lease.MatchesExactTrainingContent(changedContent));

        var permit = lease.AcquireMutationPermit();
        Assert.True(permit.IsSuccess, permit.ErrorMessage);
        Assert.NotNull(permit.Value);
        permit.Value!.Dispose();
    }

    [Theory]
    [MemberData(nameof(InvalidTrainingRunIds))]
    public void CodeDocumentationLease_InvalidRunIdFailsClosedBeforeLease(
        string? trainingRunId)
    {
        var boundValidatorCallCount = 0;
        var authority = CreateAuthority(
            _ => OperationResult<LegalTrainingTeacherAdmission>.Fail(
                "The unbound validator must not admit CodeDocumentation DPO.",
                "UNBOUND_VALIDATOR_CALLED"),
            (_, _) =>
            {
                boundValidatorCallCount++;
                return OperationResult<LegalTrainingTeacherAdmission>.Ok(
                    CreateAdmission());
            });
        var broker = (IServerOwnedDpoTrainingLeaseBroker)authority;

        var lease = broker.AcquireCodeDocumentationTrainingLease(
            ProjectContent("invalid-run-id"),
            trainingRunId!);

        Assert.False(lease.IsSuccess);
        Assert.Null(lease.Value);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority
                .TrainingContentInvalidErrorCode,
            lease.ErrorCode);
        Assert.Equal(0, boundValidatorCallCount);
    }

    [Fact]
    public async Task CodeDocumentationLease_DisposeAndAuthorityRevocationBlockMutation()
    {
        var disposedAuthority = CreateAuthority();
        var disposedLease = AssertSuccessfulCodeDocumentationLease(
            disposedAuthority,
            ProjectContent("dispose"),
            "code-doc-run:dispose");
        disposedLease.Dispose();
        Assert.Equal(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .AdmissionRevokedErrorCode,
            disposedLease.AcquireMutationPermit().ErrorCode);

        var revokedAuthority = CreateAuthority();
        using var revokedLease = AssertSuccessfulCodeDocumentationLease(
            revokedAuthority,
            ProjectContent("revocation"),
            "code-doc-run:revocation");
        await revokedAuthority.BeginRevocation().WaitAsync(
            TimeSpan.FromSeconds(5));

        Assert.Equal(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .AdmissionRevokedErrorCode,
            revokedLease.AcquireMutationPermit().ErrorCode);
    }

    public static IEnumerable<object?[]> InvalidTrainingRunIds()
    {
        yield return [null];
        yield return [string.Empty];
        yield return ["   "];
        yield return ["code-doc/run"];
        yield return ["code-doc\nrun"];
        yield return [new string('a', 257)];
    }

    private static ServerOwnedLegalTrainingTeacherAdmissionLease
        AssertSuccessfulCodeDocumentationLease(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority authority,
            DpoTrainingContentBatch trainingContent,
            string trainingRunId)
    {
        var lease = ((IServerOwnedDpoTrainingLeaseBroker)authority)
            .AcquireCodeDocumentationTrainingLease(
                trainingContent,
                trainingRunId);
        Assert.True(lease.IsSuccess, lease.ErrorMessage);
        Assert.NotNull(lease.Value);
        return lease.Value!;
    }

    private static ServerOwnedLegalTrainingTeacherAdmissionAuthority
        CreateAuthority()
        => CreateAuthority(
            _ => OperationResult<LegalTrainingTeacherAdmission>.Fail(
                "CodeDocumentation deterministic leases require exact content.",
                "UNBOUND_VALIDATOR_CALLED"),
            (_, _) => OperationResult<LegalTrainingTeacherAdmission>.Ok(
                CreateAdmission()));

    private static ServerOwnedLegalTrainingTeacherAdmissionAuthority
        CreateAuthority(
            Func<IEnumerable<TrainingTeacherProvenance?>?,
                OperationResult<LegalTrainingTeacherAdmission>> validateBatch,
            Func<DpoTrainingContentBatch?,
                IEnumerable<TrainingTeacherProvenance?>?,
                OperationResult<LegalTrainingTeacherAdmission>>
                validateBoundBatch)
        => ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
            validateBatch,
            static () => true,
            concurrencyObserverForTesting: null,
            sealReplay: null,
            verifyReplay: null,
            sealExecutableManifest: null,
            verifyExecutableManifest: null,
            validateBoundBatch);

    private static LegalTrainingTeacherAdmission CreateAdmission()
        => new(
            [],
            ExternalTeacherInvocationCount: 0,
            PolicyIdentityRef: "policy/code-documentation-dpo-lease",
            EvidenceRefs: []);

    private static DpoTrainingContentBatch ProjectContent(string tag)
    {
        var projection = BoundedDpoTrainingContentProjector.Project(
        [
            new DpoTrainingContentRowInput(
                $"document code path {tag}",
                $"accurate documentation {tag}",
                $"wrong documentation {tag}",
                [0.125, 0.25],
                "code-documentation-dpo-training-lease-test-v1")
        ]);
        Assert.True(projection.IsSuccess, projection.ErrorMessage);
        Assert.NotNull(projection.Value);
        return projection.Value!;
    }
}
