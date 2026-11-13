// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using AgentInfrastructure;
using ContinuousLearning;
using ProjectWikiDomain;

namespace LlmFromScratch.Tests;

public sealed class ProjectWikiSqlCodeDocumentationReferenceMatchingRuleTests
{
    private static readonly DateTimeOffset CapturedAtUtc =
        new(2026, 9, 8, 12, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset FrozenAtUtc =
        new(2026, 9, 8, 12, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Identity_BindsProjectWikiSqlSubjectSchemaAndExactSetPolicy()
    {
        var first = Require(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.CreateIdentity());
        var second = Require(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.CreateIdentity());

        Assert.Equal(first.IdentitySha256, second.IdentitySha256);
        Assert.Equal(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.DomainTag, first.DomainTag);
        Assert.Equal(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.RuleId, first.RuleId);
        Assert.Contains(ProjectWikiReferenceEvidencePayload.Schema, first.RuleVersion, StringComparison.Ordinal);
        Assert.Contains(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.SubjectId, first.RuleVersion, StringComparison.Ordinal);
        Assert.Contains("policy-exact-set-ordinal", first.RuleVersion, StringComparison.Ordinal);
        Assert.Contains("File extensions, procedure labels, and prose are not SQL authority", first.HumanDescription, StringComparison.Ordinal);
        Assert.Contains("not DPO", first.HumanDescription, StringComparison.Ordinal);
    }

    [Fact]
    public void IsMatch_SameEvidenceIdsInDifferentInputOrderMatchEvenWhenAnswerTextDiffers()
    {
        var candidate = Payload(
            "Candidate explains the SQL procedure with its own wording.",
            "file:database.orders.sql",
            "procedure:dbo.SubmitOrder",
            "table:dbo.Orders");
        var reference = Payload(
            "Paid reference used different prose for the same SQL proof set.",
            "table:dbo.Orders",
            "procedure:dbo.SubmitOrder",
            "file:database.orders.sql");

        Assert.True(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.IsMatch(
            candidate,
            reference));
    }

    [Fact]
    public void IsMatch_MutationSensitiveAddingOrRemovingOneEvidenceIdChangesResult()
    {
        var reference = Payload(
            "Reference binds procedure, table, and migration evidence.",
            "procedure:dbo.SubmitOrder",
            "table:dbo.Orders",
            "migration:20260908-add-orders");
        var sameSet = Payload(
            "Candidate uses another wording for the same typed SQL ids.",
            "migration:20260908-add-orders",
            "procedure:dbo.SubmitOrder",
            "table:dbo.Orders");
        var removedOne = Payload(
            "Candidate misses the migration evidence.",
            "procedure:dbo.SubmitOrder",
            "table:dbo.Orders");
        var addedOne = Payload(
            "Candidate adds an unrelated stored procedure.",
            "procedure:dbo.SubmitOrder",
            "table:dbo.Orders",
            "migration:20260908-add-orders",
            "procedure:dbo.DebugOnly");

        Assert.True(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.IsMatch(sameSet, reference));
        Assert.False(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.IsMatch(removedOne, reference));
        Assert.False(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.IsMatch(addedOne, reference));
    }

    [Fact]
    public void IsMatch_RejectsCSharpAndAngularSubjectsEvenWithExactlyEqualEvidenceIds()
    {
        var sqlReference = Payload(
            "SQL reference cites shared-looking evidence ids.",
            "file:database.shared.sql",
            "procedure:Shared.Documentation.Anchor");
        var csharpCandidate = PayloadForSubject(
            ProjectWikiCodeDocumentationReferenceMatchingRule.SubjectId,
            "C# candidate reuses the exact same evidence id strings.",
            "file:database.shared.sql",
            "procedure:Shared.Documentation.Anchor");
        var angularCandidate = PayloadForSubject(
            ProjectWikiAngularCodeDocumentationReferenceMatchingRule.SubjectId,
            "Angular candidate reuses the exact same evidence id strings.",
            "file:database.shared.sql",
            "procedure:Shared.Documentation.Anchor");

        Assert.False(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.IsMatch(
            csharpCandidate,
            sqlReference));
        Assert.False(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.IsMatch(
            angularCandidate,
            sqlReference));
        Assert.False(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.IsMatch(
            sqlReference,
            csharpCandidate));
        Assert.False(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.IsMatch(
            sqlReference,
            angularCandidate));
    }

    [Fact]
    public void IsMatch_DoesNotInferSqlFromFileExtensionProcedurePrefixOrProse()
    {
        var reference = Payload(
            "Reference uses the typed SQL subject.",
            "file:database.orders.sql",
            "procedure:dbo.SubmitOrder");
        var csharpSubjectWithSqlLookingIds = PayloadForSubject(
            ProjectWikiCodeDocumentationReferenceMatchingRule.SubjectId,
            "This prose says SQL and points at orders.sql and procedure dbo.SubmitOrder.",
            "file:database.orders.sql",
            "procedure:dbo.SubmitOrder");
        var proseOnlyCandidate =
            "SQL answer for orders.sql with procedure:dbo.SubmitOrder and file:database/orders.sql.";

        Assert.False(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.IsMatch(
            csharpSubjectWithSqlLookingIds,
            reference));
        Assert.False(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.IsMatch(
            proseOnlyCandidate,
            reference));
    }

    [Fact]
    public void FromAnswer_UsesTypedEvidenceIdsAndDoesNotParseSqlLookingIdsFromProse()
    {
        var answer = new ProjectWikiAnswer(
            "shop-db",
            "docs-v1",
            "The prose mentions procedure:ProseOnly.Decoy and file:decoy.sql, but those are not typed evidence.",
            ["procedure:TypedEvidence.Real"],
            ["database/orders.sql"],
            0.93,
            "",
            ProjectWikiRetrievalMode.Mode3LexicalEvidence,
            []);

        var payload = Require(ProjectWikiReferenceEvidencePayload.FromAnswer(
            ProjectWikiSqlCodeDocumentationReferenceMatchingRule.SubjectId,
            answer));

        Assert.Equal(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.SubjectId, payload.SubjectId);
        Assert.Contains("procedure:TypedEvidence.Real", payload.ToCanonicalString(), StringComparison.Ordinal);
        Assert.DoesNotContain("procedure:ProseOnly.Decoy", payload.EvidenceIds);
        Assert.DoesNotContain("file:decoy.sql", payload.EvidenceIds);
        Assert.True(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.IsMatch(
            payload.ToCanonicalString(),
            Payload("Reference uses the same typed SQL evidence.", "procedure:TypedEvidence.Real")));
        Assert.False(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.IsMatch(
            payload.ToCanonicalString(),
            Payload("Reference uses the prose decoy only.", "procedure:ProseOnly.Decoy")));
    }

    [Fact]
    public void EvaluateCalibration_WrongRuleIdentityFailsBeforeCalibrationNumbers()
    {
        var entry = CreateEntry(
            "wrong-rule",
            "Which SQL procedure saves the order?",
            Payload("Paid SQL answer cites the save procedure.", "procedure:dbo.SaveOrder"));
        var wrongIdentity = Require(ReferenceMatchingRuleIdentity.Create(
            ProjectWikiSqlCodeDocumentationReferenceMatchingRule.DomainTag,
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
                    Payload("Candidate cites save.", "procedure:dbo.SaveOrder"),
                    entry.PaidModelReferenceAnswer)
            ],
            knownNegativePairs:
            [
                Pair(
                    "negative-wrong-identity",
                    entry.EntryId,
                    Payload("Candidate cites load.", "procedure:dbo.LoadOrder"),
                    entry.PaidModelReferenceAnswer)
            ]));

        var result = ProjectWikiSqlCodeDocumentationReferenceMatchingRule.EvaluateCalibration(input);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            ProjectWikiSqlCodeDocumentationReferenceMatchingRule.RuleIdentityMismatchErrorCode,
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
            Require(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.CreateIdentity()),
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

        var result = ProjectWikiSqlCodeDocumentationReferenceMatchingRule.EvaluateCalibration(input);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            ProjectWikiSqlCodeDocumentationReferenceMatchingRule.CorpusSubjectMismatchErrorCode,
            result.ErrorCode);
    }

    [Fact]
    public void IsMatch_MalformedDuplicateWrongSchemaUnsafeSubjectAndUnsafeIdsFailClosed()
    {
        var reference = Payload("Reference.", "procedure:dbo.SubmitOrder");

        Assert.False(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.IsMatch(
            "{\"schema\":\"xiigen.project-wiki-code-documentation-reference-evidence.v2\",\"subject_id\":\"code-documentation-sql\",\"answer\":\"dup\",\"evidence_ids\":[\"procedure:dbo.SubmitOrder\",\"procedure:dbo.SubmitOrder\"]}",
            reference));
        Assert.False(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.IsMatch(
            "not json",
            reference));
        Assert.False(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.IsMatch(
            "{\"schema\":\"xiigen.other-schema.v1\",\"subject_id\":\"code-documentation-sql\",\"answer\":\"wrong\",\"evidence_ids\":[\"procedure:dbo.SubmitOrder\"]}",
            reference));
        Assert.False(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.IsMatch(
            "{\"schema\":\"xiigen.project-wiki-code-documentation-reference-evidence.v2\",\"subject_id\":\"bad subject\",\"answer\":\"unsafe subject\",\"evidence_ids\":[\"procedure:dbo.SubmitOrder\"]}",
            reference));
        Assert.False(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.IsMatch(
            "{\"schema\":\"xiigen.project-wiki-code-documentation-reference-evidence.v2\",\"subject_id\":\"code-documentation-sql\",\"answer\":\"unsafe id\",\"evidence_ids\":[\"bad id with spaces\"]}",
            reference));
        Assert.False(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.IsMatch(
            "{\"schema\":\"xiigen.project-wiki-code-documentation-reference-evidence.v2\",\"answer\":\"missing subject\",\"evidence_ids\":[]}",
            reference));
        Assert.False(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.IsMatch(
            "{\"answer\":\"wrong order\",\"schema\":\"xiigen.project-wiki-code-documentation-reference-evidence.v2\",\"subject_id\":\"code-documentation-sql\",\"evidence_ids\":[]}",
            reference));
    }

    private static ReferenceMatchingCalibrationInput CreateCalibrationInput(
        FrozenReferenceCorpus corpus,
        IReadOnlyList<ReferenceMatchingCalibrationPair> positives,
        IReadOnlyList<ReferenceMatchingCalibrationPair> negatives)
        => Require(ReferenceMatchingCalibrationInput.Create(
            corpus,
            Require(ProjectWikiSqlCodeDocumentationReferenceMatchingRule.CreateIdentity()),
            positives,
            negatives));

    private static FrozenReferenceCorpus CreateCorpus(
        params FrozenReferenceEntry[] entries)
        => Require(FrozenReferenceCorpus.Build(
            "project-wiki-sql-code-documentation-reference-fixture",
            "v2026-09-08-unit",
            FrozenAtUtc,
            entries));

    private static FrozenReferenceEntry CreateEntry(
        string seed,
        string ownerInput,
        string referenceAnswer,
        string subjectId = ProjectWikiSqlCodeDocumentationReferenceMatchingRule.SubjectId)
    {
        // Test-only fake paid provenance: this proves the SQL adapter contract
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
            ProjectWikiSqlCodeDocumentationReferenceMatchingRule.DomainTag,
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
            ProjectWikiSqlCodeDocumentationReferenceMatchingRule.SubjectId,
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
