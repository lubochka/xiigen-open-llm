// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using AgentInfrastructure;
using ContinuousLearning;
using ProjectWikiDomain;

namespace LlmFromScratch.Tests;

public sealed class ProjectWikiAngularCodeDocumentationReferenceMatchingRuleTests
{
    private static readonly DateTimeOffset CapturedAtUtc =
        new(2026, 9, 8, 11, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset FrozenAtUtc =
        new(2026, 9, 8, 11, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Identity_BindsProjectWikiAngularSubjectSchemaAndExactSetPolicy()
    {
        var first = Require(ProjectWikiAngularCodeDocumentationReferenceMatchingRule.CreateIdentity());
        var second = Require(ProjectWikiAngularCodeDocumentationReferenceMatchingRule.CreateIdentity());

        Assert.Equal(first.IdentitySha256, second.IdentitySha256);
        Assert.Equal(ProjectWikiAngularCodeDocumentationReferenceMatchingRule.DomainTag, first.DomainTag);
        Assert.Equal(ProjectWikiAngularCodeDocumentationReferenceMatchingRule.RuleId, first.RuleId);
        Assert.Contains(ProjectWikiReferenceEvidencePayload.Schema, first.RuleVersion, StringComparison.Ordinal);
        Assert.Contains(ProjectWikiAngularCodeDocumentationReferenceMatchingRule.SubjectId, first.RuleVersion, StringComparison.Ordinal);
        Assert.Contains("policy-exact-set-ordinal", first.RuleVersion, StringComparison.Ordinal);
        Assert.Contains("not DPO", first.HumanDescription, StringComparison.Ordinal);
    }

    [Fact]
    public void IsMatch_SameEvidenceIdsInDifferentInputOrderMatchEvenWhenAnswerTextDiffers()
    {
        var candidate = Payload(
            "Candidate explains the Angular component route in its own words.",
            "component:OrderListComponent",
            "template:order-list.html",
            "service:OrderApiClient");
        var reference = Payload(
            "Paid reference used different prose for the same Angular proof set.",
            "service:OrderApiClient",
            "component:OrderListComponent",
            "template:order-list.html");

        Assert.True(ProjectWikiAngularCodeDocumentationReferenceMatchingRule.IsMatch(
            candidate,
            reference));
    }

    [Fact]
    public void IsMatch_MutationSensitiveAddingOrRemovingOneEvidenceIdChangesResult()
    {
        var reference = Payload(
            "Reference binds component, template, and service evidence.",
            "component:OrderListComponent",
            "template:order-list.html",
            "service:OrderApiClient");
        var sameSet = Payload(
            "Candidate uses another wording for the same typed ids.",
            "service:OrderApiClient",
            "component:OrderListComponent",
            "template:order-list.html");
        var removedOne = Payload(
            "Candidate misses the template evidence.",
            "component:OrderListComponent",
            "service:OrderApiClient");
        var addedOne = Payload(
            "Candidate adds an unrelated directive.",
            "component:OrderListComponent",
            "template:order-list.html",
            "service:OrderApiClient",
            "directive:DebugOnlyDirective");

        Assert.True(ProjectWikiAngularCodeDocumentationReferenceMatchingRule.IsMatch(sameSet, reference));
        Assert.False(ProjectWikiAngularCodeDocumentationReferenceMatchingRule.IsMatch(removedOne, reference));
        Assert.False(ProjectWikiAngularCodeDocumentationReferenceMatchingRule.IsMatch(addedOne, reference));
    }

    [Fact]
    public void IsMatch_RejectsCSharpAndSqlSubjectsEvenWithExactlyEqualEvidenceIds()
    {
        var angularReference = Payload(
            "Angular reference cites a shared-looking evidence id.",
            "member:Shared.Documentation.Anchor");
        var csharpCandidate = PayloadForSubject(
            ProjectWikiCodeDocumentationReferenceMatchingRule.SubjectId,
            "C# candidate reuses the exact same evidence id string.",
            "member:Shared.Documentation.Anchor");
        var sqlCandidate = PayloadForSubject(
            "code-documentation-sql",
            "SQL candidate reuses the exact same evidence id string.",
            "member:Shared.Documentation.Anchor");

        Assert.False(ProjectWikiAngularCodeDocumentationReferenceMatchingRule.IsMatch(
            csharpCandidate,
            angularReference));
        Assert.False(ProjectWikiAngularCodeDocumentationReferenceMatchingRule.IsMatch(
            sqlCandidate,
            angularReference));
        Assert.False(ProjectWikiAngularCodeDocumentationReferenceMatchingRule.IsMatch(
            angularReference,
            csharpCandidate));
    }

    [Fact]
    public void FromAnswer_UsesTypedEvidenceIdsAndDoesNotParseIdsFromProse()
    {
        var answer = new ProjectWikiAnswer(
            "shop-ui",
            "docs-v1",
            "The prose mentions component:ProseOnly.Decoy, but that is not typed evidence.",
            ["component:TypedEvidence.Real"],
            ["src/app/orders/order-list.component.ts"],
            0.92,
            "",
            ProjectWikiRetrievalMode.Mode3LexicalEvidence,
            []);

        var payload = Require(ProjectWikiReferenceEvidencePayload.FromAnswer(
            ProjectWikiAngularCodeDocumentationReferenceMatchingRule.SubjectId,
            answer));

        Assert.Equal(ProjectWikiAngularCodeDocumentationReferenceMatchingRule.SubjectId, payload.SubjectId);
        Assert.Contains("component:TypedEvidence.Real", payload.ToCanonicalString(), StringComparison.Ordinal);
        Assert.DoesNotContain("component:ProseOnly.Decoy", payload.EvidenceIds);
        Assert.True(ProjectWikiAngularCodeDocumentationReferenceMatchingRule.IsMatch(
            payload.ToCanonicalString(),
            Payload("Reference uses the same typed Angular evidence.", "component:TypedEvidence.Real")));
        Assert.False(ProjectWikiAngularCodeDocumentationReferenceMatchingRule.IsMatch(
            payload.ToCanonicalString(),
            Payload("Reference uses the prose decoy only.", "component:ProseOnly.Decoy")));
    }

    [Fact]
    public void EvaluateCalibration_WrongRuleIdentityFailsBeforeCalibrationNumbers()
    {
        var entry = CreateEntry(
            "wrong-rule",
            "Which Angular service loads the order list?",
            Payload("Paid answer cites the Angular service.", "service:OrderApiClient"));
        var wrongIdentity = Require(ReferenceMatchingRuleIdentity.Create(
            ProjectWikiAngularCodeDocumentationReferenceMatchingRule.DomainTag,
            "project-wiki-foreign-rule",
            "v1",
            "Same domain but deliberately wrong rule identity for guard testing."));
        var input = Require(ReferenceMatchingCalibrationInput.Create(
            CreateCorpus(entry),
            wrongIdentity,
            knownPositivePairs:
            [
                Pair(
                    "positive-wrong-identity",
                    entry.EntryId,
                    Payload("Candidate cites service.", "service:OrderApiClient"),
                    entry.PaidModelReferenceAnswer)
            ],
            knownNegativePairs:
            [
                Pair(
                    "negative-wrong-identity",
                    entry.EntryId,
                    Payload("Candidate cites component.", "component:OrderListComponent"),
                    entry.PaidModelReferenceAnswer)
            ]));

        var result = ProjectWikiAngularCodeDocumentationReferenceMatchingRule.EvaluateCalibration(input);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            "PROJECT_WIKI_ANGULAR_REFERENCE_MATCHING_RULE_IDENTITY_MISMATCH",
            result.ErrorCode);
    }

    [Fact]
    public void EvaluateCalibration_CorpusSubjectMismatchFailsBeforeCalibrationNumbers()
    {
        var entry = CreateEntry(
            "wrong-subject",
            "Which C# method saves the order?",
            PayloadForSubject(
                ProjectWikiCodeDocumentationReferenceMatchingRule.SubjectId,
                "Paid C# answer cites the save method.",
                "member:OrderRepository.Save"),
            ProjectWikiCodeDocumentationReferenceMatchingRule.SubjectId);
        var input = Require(ReferenceMatchingCalibrationInput.Create(
            CreateCorpus(entry),
            Require(ProjectWikiAngularCodeDocumentationReferenceMatchingRule.CreateIdentity()),
            knownPositivePairs:
            [
                Pair(
                    "positive-wrong-subject",
                    entry.EntryId,
                    PayloadForSubject(
                        ProjectWikiCodeDocumentationReferenceMatchingRule.SubjectId,
                        "Candidate cites save.",
                        "member:OrderRepository.Save"),
                    entry.PaidModelReferenceAnswer)
            ],
            knownNegativePairs:
            [
                Pair(
                    "negative-wrong-subject",
                    entry.EntryId,
                    PayloadForSubject(
                        ProjectWikiCodeDocumentationReferenceMatchingRule.SubjectId,
                        "Candidate cites load.",
                        "member:OrderRepository.Load"),
                    entry.PaidModelReferenceAnswer)
            ]));

        var result = ProjectWikiAngularCodeDocumentationReferenceMatchingRule.EvaluateCalibration(input);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            "PROJECT_WIKI_ANGULAR_REFERENCE_MATCHING_CORPUS_SUBJECT_MISMATCH",
            result.ErrorCode);
    }

    [Fact]
    public void IsMatch_MalformedDuplicateWrongSchemaUnsafeSubjectAndUnsafeIdsFailClosed()
    {
        var reference = Payload("Reference.", "component:OrderListComponent");

        Assert.False(ProjectWikiAngularCodeDocumentationReferenceMatchingRule.IsMatch(
            "{\"schema\":\"xiigen.project-wiki-code-documentation-reference-evidence.v2\",\"subject_id\":\"code-documentation-angular\",\"answer\":\"dup\",\"evidence_ids\":[\"component:OrderListComponent\",\"component:OrderListComponent\"]}",
            reference));
        Assert.False(ProjectWikiAngularCodeDocumentationReferenceMatchingRule.IsMatch(
            "not json",
            reference));
        Assert.False(ProjectWikiAngularCodeDocumentationReferenceMatchingRule.IsMatch(
            "{\"schema\":\"xiigen.other-schema.v1\",\"subject_id\":\"code-documentation-angular\",\"answer\":\"wrong\",\"evidence_ids\":[\"component:OrderListComponent\"]}",
            reference));
        Assert.False(ProjectWikiAngularCodeDocumentationReferenceMatchingRule.IsMatch(
            "{\"schema\":\"xiigen.project-wiki-code-documentation-reference-evidence.v2\",\"subject_id\":\"bad subject\",\"answer\":\"unsafe subject\",\"evidence_ids\":[\"component:OrderListComponent\"]}",
            reference));
        Assert.False(ProjectWikiAngularCodeDocumentationReferenceMatchingRule.IsMatch(
            "{\"schema\":\"xiigen.project-wiki-code-documentation-reference-evidence.v2\",\"subject_id\":\"code-documentation-angular\",\"answer\":\"unsafe id\",\"evidence_ids\":[\"bad id with spaces\"]}",
            reference));
        Assert.False(ProjectWikiAngularCodeDocumentationReferenceMatchingRule.IsMatch(
            "{\"schema\":\"xiigen.project-wiki-code-documentation-reference-evidence.v2\",\"answer\":\"missing subject\",\"evidence_ids\":[]}",
            reference));
        Assert.False(ProjectWikiAngularCodeDocumentationReferenceMatchingRule.IsMatch(
            "{\"answer\":\"wrong order\",\"schema\":\"xiigen.project-wiki-code-documentation-reference-evidence.v2\",\"subject_id\":\"code-documentation-angular\",\"evidence_ids\":[]}",
            reference));
    }

    private static FrozenReferenceCorpus CreateCorpus(
        params FrozenReferenceEntry[] entries)
        => Require(FrozenReferenceCorpus.Build(
            "project-wiki-angular-code-documentation-reference-fixture",
            "v2026-09-08-unit",
            FrozenAtUtc,
            entries));

    private static FrozenReferenceEntry CreateEntry(
        string seed,
        string ownerInput,
        string referenceAnswer,
        string subjectId = ProjectWikiAngularCodeDocumentationReferenceMatchingRule.SubjectId)
    {
        // Test-only fake paid provenance: this proves the adapter contract
        // without claiming a real paid corpus, model call, DPO row, or training.
        var referenceHash =
            FrozenReferenceEntry.ComputeNormalizedTextSha256(referenceAnswer);
        var provenance = Require(PaidReferenceProvenance.Create(
            "unit-fake-paid-provider",
            "unit-fake-paid-model",
            "2026-09-08",
            "unit-request-" + seed,
            CapturedAtUtc,
            referenceHash));

        return Require(FrozenReferenceEntry.Create(
            ownerInput,
            referenceAnswer,
            provenance,
            ProjectWikiAngularCodeDocumentationReferenceMatchingRule.DomainTag,
            subjectId,
            FrozenReferencePartition.HeldOut,
            FrozenReferenceEntry.ComputeNormalizedTextSha256(
                "unit-source:" + seed)));
    }

    private static ReferenceMatchingCalibrationPair Pair(
        string pairId,
        string referenceEntryId,
        string candidate,
        string reference)
        => Require(ReferenceMatchingCalibrationPair.Create(
            pairId,
            referenceEntryId,
            candidate,
            reference));

    private static string Payload(
        string answer,
        params string[] evidenceIds)
        => PayloadForSubject(
            ProjectWikiAngularCodeDocumentationReferenceMatchingRule.SubjectId,
            answer,
            evidenceIds);

    private static string PayloadForSubject(
        string subjectId,
        string answer,
        params string[] evidenceIds)
        => Require(ProjectWikiReferenceEvidencePayload.Create(
            subjectId,
            answer,
            evidenceIds)).ToCanonicalString();

    private static T Require<T>(OperationResult<T> result)
    {
        Assert.True(
            result.IsSuccess,
            $"Expected success but got {result.ErrorCode}: {result.ErrorMessage}");
        Assert.NotNull(result.Value);
        return result.Value!;
    }
}
