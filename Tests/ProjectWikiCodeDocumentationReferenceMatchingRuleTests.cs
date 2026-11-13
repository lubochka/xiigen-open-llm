// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using AgentInfrastructure;
using ContinuousLearning;
using ProjectWikiDomain;

namespace LlmFromScratch.Tests;

public sealed class ProjectWikiCodeDocumentationReferenceMatchingRuleTests
{
    private static readonly DateTimeOffset CapturedAtUtc =
        new(2026, 9, 8, 10, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset FrozenAtUtc =
        new(2026, 9, 8, 10, 30, 0, TimeSpan.Zero);

    [Fact]
    public void PayloadCanonicalString_UsesFixedSchemaAnswerEvidenceOrderAndSortedIds()
    {
        // Canonical JSON is the boundary format stored in frozen references.
        // The ids are sorted so two equivalent answers do not hash differently.
        var payload = Require(ProjectWikiReferenceEvidencePayload.Create(
            ProjectWikiCodeDocumentationReferenceMatchingRule.SubjectId,
            "uses file evidence",
            ["method:beta", "method:alpha"]));

        Assert.Equal(
            "{\"schema\":\"xiigen.project-wiki-code-documentation-reference-evidence.v2\",\"subject_id\":\"code-documentation-csharp\",\"answer\":\"uses file evidence\",\"evidence_ids\":[\"method:alpha\",\"method:beta\"]}",
            payload.ToCanonicalString());
        Assert.Equal(ProjectWikiCodeDocumentationReferenceMatchingRule.SubjectId, payload.SubjectId);
    }

    [Fact]
    public void Identity_BindsProjectWikiCSharpSubjectSchemaAndExactSetPolicy()
    {
        var first = Require(ProjectWikiCodeDocumentationReferenceMatchingRule.CreateIdentity());
        var second = Require(ProjectWikiCodeDocumentationReferenceMatchingRule.CreateIdentity());

        Assert.Equal(first.IdentitySha256, second.IdentitySha256);
        Assert.Equal(ProjectWikiCodeDocumentationReferenceMatchingRule.DomainTag, first.DomainTag);
        Assert.Equal(ProjectWikiCodeDocumentationReferenceMatchingRule.RuleId, first.RuleId);
        Assert.Contains(ProjectWikiReferenceEvidencePayload.Schema, first.RuleVersion, StringComparison.Ordinal);
        Assert.Contains(ProjectWikiCodeDocumentationReferenceMatchingRule.SubjectId, first.RuleVersion, StringComparison.Ordinal);
        Assert.Contains("policy-exact-set-ordinal", first.RuleVersion, StringComparison.Ordinal);
        Assert.Contains("not DPO", first.HumanDescription, StringComparison.Ordinal);
    }

    [Fact]
    public void IsMatch_SameEvidenceIdsInDifferentInputOrderMatchEvenWhenAnswerTextDiffers()
    {
        // The instrument measures evidence grounding, not whether the prose was
        // copied from the paid reference.
        var candidate = Payload(
            "Candidate explains the C# method with its own wording.",
            "file:OrderService.cs",
            "member:OrderService.Submit");
        var reference = Payload(
            "Paid reference used different prose for the same grounded facts.",
            "member:OrderService.Submit",
            "file:OrderService.cs");

        Assert.True(ProjectWikiCodeDocumentationReferenceMatchingRule.IsMatch(
            candidate,
            reference));
    }

    [Fact]
    public void IsMatch_MissingExtraAndEmptyEvidenceSetsRespectExactSetSemantics()
    {
        var reference = Payload(
            "Reference cites two exact C# evidence records.",
            "file:OrderService.cs",
            "member:OrderService.Submit");

        // Missing evidence is a false miss risk: the answer is under-grounded.
        Assert.False(ProjectWikiCodeDocumentationReferenceMatchingRule.IsMatch(
            Payload("Candidate cited only the source file.", "file:OrderService.cs"),
            reference));

        // Extra evidence is also wrong for this adapter; subset coverage would
        // pass it, but exact-set frozen-reference matching must not.
        Assert.False(ProjectWikiCodeDocumentationReferenceMatchingRule.IsMatch(
            Payload(
                "Candidate cites the right facts plus an unrelated extra.",
                "file:OrderService.cs",
                "member:OrderService.Submit",
                "member:AuditTrail.Append"),
            reference));

        Assert.True(ProjectWikiCodeDocumentationReferenceMatchingRule.IsMatch(
            Payload("No evidence was needed."),
            Payload("The paid answer also needed no evidence.")));
        Assert.False(ProjectWikiCodeDocumentationReferenceMatchingRule.IsMatch(
            Payload("Empty candidate cannot match grounded reference."),
            reference));
    }

    [Fact]
    public void IsMatch_RejectsAngularAndSqlSubjectsEvenWithExactlyEqualEvidenceIds()
    {
        var csharpReference = Payload(
            "C# reference cites a member id.",
            "member:Shared.Documentation.Anchor");
        var angularCandidate = PayloadForSubject(
            ProjectWikiAngularCodeDocumentationReferenceMatchingRule.SubjectId,
            "Angular candidate reuses the exact same id string.",
            "member:Shared.Documentation.Anchor");
        var sqlCandidate = PayloadForSubject(
            "code-documentation-sql",
            "SQL candidate reuses the exact same id string.",
            "member:Shared.Documentation.Anchor");

        Assert.False(ProjectWikiCodeDocumentationReferenceMatchingRule.IsMatch(
            angularCandidate,
            csharpReference));
        Assert.False(ProjectWikiCodeDocumentationReferenceMatchingRule.IsMatch(
            sqlCandidate,
            csharpReference));
        Assert.False(ProjectWikiCodeDocumentationReferenceMatchingRule.IsMatch(
            csharpReference,
            angularCandidate));
    }

    [Fact]
    public void IsMatch_DuplicateMalformedWrongSchemaAndUnsafeIdsFailClosed()
    {
        var reference = Payload("Reference.", "file:OrderService.cs");

        // Duplicate ids are invalid input, not something the matcher silently
        // deduplicates into a successful exact set.
        Assert.False(ProjectWikiCodeDocumentationReferenceMatchingRule.IsMatch(
            "{\"schema\":\"xiigen.project-wiki-code-documentation-reference-evidence.v2\",\"subject_id\":\"code-documentation-csharp\",\"answer\":\"dup\",\"evidence_ids\":[\"file:OrderService.cs\",\"file:OrderService.cs\"]}",
            reference));

        Assert.False(ProjectWikiCodeDocumentationReferenceMatchingRule.IsMatch(
            "not json",
            reference));
        Assert.False(ProjectWikiCodeDocumentationReferenceMatchingRule.IsMatch(
            "{\"schema\":\"xiigen.other-schema.v1\",\"subject_id\":\"code-documentation-csharp\",\"answer\":\"wrong\",\"evidence_ids\":[\"file:OrderService.cs\"]}",
            reference));
        Assert.False(ProjectWikiCodeDocumentationReferenceMatchingRule.IsMatch(
            "{\"schema\":\"xiigen.project-wiki-code-documentation-reference-evidence.v2\",\"subject_id\":\"bad subject\",\"answer\":\"wrong\",\"evidence_ids\":[\"file:OrderService.cs\"]}",
            reference));
        Assert.False(ProjectWikiCodeDocumentationReferenceMatchingRule.IsMatch(
            "{\"schema\":\"xiigen.project-wiki-code-documentation-reference-evidence.v2\",\"subject_id\":\"code-documentation-csharp\",\"answer\":\"unsafe\",\"evidence_ids\":[\"bad id with spaces\"]}",
            reference));
        Assert.False(ProjectWikiCodeDocumentationReferenceMatchingRule.IsMatch(
            "{\"schema\":\"xiigen.project-wiki-code-documentation-reference-evidence.v2\",\"subject_id\":\"code-documentation-csharp\",\"answer\":\"oversized\",\"evidence_ids\":[\"" + new string('a', 257) + "\"]}",
            reference));
    }

    [Fact]
    public void PayloadParse_MissingUnknownOutOfOrderControlAndWholeOversizedContentFailClosed()
    {
        var reference = Payload("Reference with no required evidence.");

        // The persisted JSON is intentionally narrow: accepting missing, extra,
        // or reordered properties would create a second non-canonical wire form.
        Assert.False(ProjectWikiCodeDocumentationReferenceMatchingRule.IsMatch(
            "{\"schema\":\"xiigen.project-wiki-code-documentation-reference-evidence.v2\",\"subject_id\":\"code-documentation-csharp\",\"answer\":\"missing ids\"}",
            reference));
        Assert.False(ProjectWikiCodeDocumentationReferenceMatchingRule.IsMatch(
            "{\"schema\":\"xiigen.project-wiki-code-documentation-reference-evidence.v2\",\"subject_id\":\"code-documentation-csharp\",\"answer\":\"extra\",\"evidence_ids\":[],\"extra\":true}",
            reference));
        Assert.False(ProjectWikiCodeDocumentationReferenceMatchingRule.IsMatch(
            "{\"schema\":\"xiigen.project-wiki-code-documentation-reference-evidence.v2\",\"answer\":\"missing subject\",\"evidence_ids\":[]}",
            reference));
        Assert.False(ProjectWikiCodeDocumentationReferenceMatchingRule.IsMatch(
            "{\"answer\":\"wrong order\",\"schema\":\"xiigen.project-wiki-code-documentation-reference-evidence.v2\",\"subject_id\":\"code-documentation-csharp\",\"evidence_ids\":[]}",
            reference));

        // Escaped JSON control characters are checked after parse as real
        // string content, so unsafe bytes cannot hide behind JSON escaping.
        Assert.False(ProjectWikiCodeDocumentationReferenceMatchingRule.IsMatch(
            "{\"schema\":\"xiigen.project-wiki-code-documentation-reference-evidence.v2\",\"subject_id\":\"code-documentation-csharp\",\"answer\":\"unsafe\\u0001content\",\"evidence_ids\":[]}",
            reference));

        // The final canonical JSON envelope, not only the answer text, stays
        // under the frozen-reference limit.
        var oversizedCanonical = ProjectWikiReferenceEvidencePayload.Create(
            ProjectWikiCodeDocumentationReferenceMatchingRule.SubjectId,
            new string('a', 65_500),
            ["id"]);
        Assert.False(oversizedCanonical.IsSuccess);
    }

    [Fact]
    public void IsMatch_MutationSensitiveAddingOrRemovingOneEvidenceIdChangesResult()
    {
        var reference = Payload(
            "Reference binds a source file and the exact method.",
            "file:OrderService.cs",
            "member:OrderService.Submit");
        var sameSet = Payload(
            "Candidate cites the same proof set.",
            "member:OrderService.Submit",
            "file:OrderService.cs");
        var removedOne = Payload(
            "Candidate drops the method-level evidence.",
            "file:OrderService.cs");
        var addedOne = Payload(
            "Candidate adds an unrelated method.",
            "file:OrderService.cs",
            "member:OrderService.Submit",
            "member:OrderService.Cancel");

        Assert.True(ProjectWikiCodeDocumentationReferenceMatchingRule.IsMatch(sameSet, reference));
        Assert.False(ProjectWikiCodeDocumentationReferenceMatchingRule.IsMatch(removedOne, reference));
        Assert.False(ProjectWikiCodeDocumentationReferenceMatchingRule.IsMatch(addedOne, reference));
    }

    [Fact]
    public void FromAnswer_UsesTypedEvidenceIdsAndDoesNotParseIdsFromProse()
    {
        var answer = new ProjectWikiAnswer(
            "shop",
            "docs-v1",
            "The prose mentions member:ProseOnly.Decoy, but that is not typed evidence.",
            ["member:TypedEvidence.Real"],
            ["OrderService.cs"],
            0.91,
            "",
            ProjectWikiRetrievalMode.Mode3LexicalEvidence,
            []);

        var payload = Require(ProjectWikiReferenceEvidencePayload.FromAnswer(
            ProjectWikiCodeDocumentationReferenceMatchingRule.SubjectId,
            answer));

        Assert.Equal(ProjectWikiCodeDocumentationReferenceMatchingRule.SubjectId, payload.SubjectId);
        Assert.Contains("member:TypedEvidence.Real", payload.ToCanonicalString(), StringComparison.Ordinal);
        Assert.DoesNotContain("member:ProseOnly.Decoy", payload.EvidenceIds);
        Assert.True(ProjectWikiCodeDocumentationReferenceMatchingRule.IsMatch(
            payload.ToCanonicalString(),
            Payload("Reference uses the same typed evidence.", "member:TypedEvidence.Real")));
        Assert.False(ProjectWikiCodeDocumentationReferenceMatchingRule.IsMatch(
            payload.ToCanonicalString(),
            Payload("Reference uses the prose decoy only.", "member:ProseOnly.Decoy")));
    }

    [Fact]
    public void EvaluateCalibration_WrongRuleIdentityFailsBeforeCalibrationNumbers()
    {
        var entry = CreateEntry(
            "wrong-rule",
            "Which method saves the order?",
            Payload("Paid answer cites the save method.", "member:OrderRepository.Save"));
        var wrongIdentity = Require(ReferenceMatchingRuleIdentity.Create(
            ProjectWikiCodeDocumentationReferenceMatchingRule.DomainTag,
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
                    Payload("Candidate cites save.", "member:OrderRepository.Save"),
                    entry.PaidModelReferenceAnswer)
            ],
            knownNegativePairs:
            [
                Pair(
                    "negative-wrong-identity",
                    entry.EntryId,
                    Payload("Candidate cites load.", "member:OrderRepository.Load"),
                    entry.PaidModelReferenceAnswer)
            ]));

        var result = ProjectWikiCodeDocumentationReferenceMatchingRule.EvaluateCalibration(input);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "PROJECT_WIKI_REFERENCE_MATCHING_RULE_IDENTITY_MISMATCH",
            result.ErrorCode);
    }

    [Fact]
    public void EvaluateCalibration_CorpusSubjectMismatchFailsBeforeCalibrationNumbers()
    {
        var entry = CreateEntry(
            "wrong-subject",
            "Which Angular service loads the order list?",
            PayloadForSubject(
                ProjectWikiAngularCodeDocumentationReferenceMatchingRule.SubjectId,
                "Paid Angular answer cites the loading service.",
                "service:OrderApiClient"),
            ProjectWikiAngularCodeDocumentationReferenceMatchingRule.SubjectId);
        var input = Require(ReferenceMatchingCalibrationInput.Create(
            CreateCorpus(entry),
            Require(ProjectWikiCodeDocumentationReferenceMatchingRule.CreateIdentity()),
            knownPositivePairs:
            [
                Pair(
                    "positive-wrong-subject",
                    entry.EntryId,
                    PayloadForSubject(
                        ProjectWikiAngularCodeDocumentationReferenceMatchingRule.SubjectId,
                        "Candidate cites the same Angular service.",
                        "service:OrderApiClient"),
                    entry.PaidModelReferenceAnswer)
            ],
            knownNegativePairs:
            [
                Pair(
                    "negative-wrong-subject",
                    entry.EntryId,
                    PayloadForSubject(
                        ProjectWikiAngularCodeDocumentationReferenceMatchingRule.SubjectId,
                        "Candidate cites a different Angular component.",
                        "component:OrderListComponent"),
                    entry.PaidModelReferenceAnswer)
            ]));

        var result = ProjectWikiCodeDocumentationReferenceMatchingRule.EvaluateCalibration(input);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            "PROJECT_WIKI_REFERENCE_MATCHING_CORPUS_SUBJECT_MISMATCH",
            result.ErrorCode);
    }

    [Fact]
    public void EvaluateCalibration_SameDomainForeignEvidenceIdsKeepFalseHitAtZero()
    {
        var entry = CreateEntry(
            "foreign-negative",
            "Which C# method writes the audit row?",
            Payload("Paid reference cites the audit writer.", "member:AuditTrail.Append"));
        var input = CreateCalibrationInput(
            CreateCorpus(entry),
            positives:
            [
                Pair(
                    "positive-exact-evidence",
                    entry.EntryId,
                    Payload("Candidate cites the audit writer with different prose.", "member:AuditTrail.Append"),
                    entry.PaidModelReferenceAnswer)
            ],
            negatives:
            [
                Pair(
                    "negative-same-domain-foreign-evidence",
                    entry.EntryId,
                    Payload("Candidate cites another C# evidence id.", "member:AuditTrail.Flush"),
                    entry.PaidModelReferenceAnswer)
            ]);

        var result = Require(ProjectWikiCodeDocumentationReferenceMatchingRule.EvaluateCalibration(input));

        Assert.Equal(1, result.KnownNegativeCount);
        Assert.Equal(0, result.FalseHitCount);
        Assert.Equal(0D, result.FalseHitRate);
    }

    private static ReferenceMatchingCalibrationInput CreateCalibrationInput(
        FrozenReferenceCorpus corpus,
        IReadOnlyList<ReferenceMatchingCalibrationPair> positives,
        IReadOnlyList<ReferenceMatchingCalibrationPair> negatives)
        => Require(ReferenceMatchingCalibrationInput.Create(
            corpus,
            Require(ProjectWikiCodeDocumentationReferenceMatchingRule.CreateIdentity()),
            positives,
            negatives));

    private static FrozenReferenceCorpus CreateCorpus(
        params FrozenReferenceEntry[] entries)
        => Require(FrozenReferenceCorpus.Build(
            "project-wiki-code-documentation-reference-fixture",
            "v2026-09-08-unit",
            FrozenAtUtc,
            entries));

    private static FrozenReferenceEntry CreateEntry(
        string seed,
        string ownerInput,
        string referenceAnswer,
        string subjectId = ProjectWikiCodeDocumentationReferenceMatchingRule.SubjectId)
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
            ProjectWikiCodeDocumentationReferenceMatchingRule.DomainTag,
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
            ProjectWikiCodeDocumentationReferenceMatchingRule.SubjectId,
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
