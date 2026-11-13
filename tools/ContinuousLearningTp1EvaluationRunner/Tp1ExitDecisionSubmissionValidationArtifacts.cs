// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearningTp1EvaluationRunner;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

internal sealed record Tp1ExitHumanDecisionSubmissionArtifact(
    string DecisionRequestId,
    string SourceCeilingEvaluationSha256,
    bool SourceLocalTeacherCeilingCandidate,
    string IndependentReviewerAuthorityRef,
    string IndependentReviewedAtUtc,
    bool SourceRunOrderingVerified,
    bool SourceAuthenticityVerified,
    bool NumericEvidenceVerified,
    bool LocalTeacherCeilingAccepted,
    bool IndependentAcceptance,
    string LubaAuthorityRef,
    string LubaDecidedAtUtc,
    string Tp1ExitDecision,
    IReadOnlyList<string> ReasonRefs);

internal sealed record Tp1ExitDecisionSubmissionParseResult(
    Tp1ExitHumanDecisionSubmissionArtifact Submission,
    byte[] CanonicalBytes);

internal sealed record Tp1ExitDecisionRequestPacketHashes(
    string DecisionRequestSha256,
    string DecisionRequestMarkdownSha256,
    string HumanDecisionSubmissionSchemaSha256,
    string DecisionRequestRunSha256);

internal sealed record Tp1ExitUntrustedDeclaredClaims(
    bool SourceRunOrderingVerified,
    bool SourceAuthenticityVerified,
    bool NumericEvidenceVerified,
    bool LocalTeacherCeilingAccepted,
    bool IndependentAcceptance,
    string Tp1ExitDecision);

internal sealed record Tp1ExitDecisionSubmissionValidationArtifact(
    string Status,
    string ValidationId,
    string DecisionRequestId,
    string SourceCeilingEvaluationId,
    string SourcePolicyId,
    bool SourceLocalTeacherCeilingCandidate,
    Tp1ExitDecisionRequestPacketHashes SourceRequestPacketHashes,
    string OriginalSubmissionSha256,
    string CanonicalSubmissionSemanticRecordSha256,
    Tp1ExitUntrustedDeclaredClaims UntrustedDeclaredClaims,
    bool SubmissionPresent,
    bool ShapeValidated,
    bool SchemaTemplateOnly,
    bool SchemaNotAuthority,
    bool SchemaNeverParseAsReceipt,
    bool TrustedHumanDecisionPresent,
    bool AuthorityEvidenceVerified,
    bool IndependentReviewerIdentityVerified,
    bool LubaIdentityVerified,
    bool SubmissionSignatureVerified,
    bool ReplayProtectionVerified,
    bool ReceiptAccepted,
    bool IndependentAcceptance,
    bool LocalTeacherCeilingAccepted,
    bool Passed,
    bool Tp1ExitGateEvaluated,
    bool Tp2Authorized,
    bool EvalOnly,
    bool DpoTrainReady,
    bool TeacherOutputUsedForTraining,
    bool TrainingStarted,
    int DpoStepsExecuted,
    bool CheckpointPromoted,
    bool PromotionStarted,
    bool ActivationStarted,
    bool HumanDecisionRequired);

internal sealed record Tp1ExitDecisionSubmissionValidationRunArtifact(
    string Status,
    string ValidationId,
    string DecisionRequestId,
    string ValidationSha256,
    string MarkdownSha256,
    Tp1ExitDecisionRequestPacketHashes SourceRequestPacketHashes,
    string OriginalSubmissionSha256,
    string CanonicalSubmissionSemanticRecordSha256,
    IReadOnlyList<string> PublishedArtifactLeafNames,
    string PublishedArtifactRosterSha256,
    bool SubmissionPresent,
    bool ShapeValidated,
    bool SchemaTemplateOnly,
    bool SchemaNotAuthority,
    bool SchemaNeverParseAsReceipt,
    bool TrustedHumanDecisionPresent,
    bool AuthorityEvidenceVerified,
    bool IndependentReviewerIdentityVerified,
    bool LubaIdentityVerified,
    bool SubmissionSignatureVerified,
    bool ReplayProtectionVerified,
    bool ReceiptAccepted,
    bool IndependentAcceptance,
    bool LocalTeacherCeilingAccepted,
    bool Passed,
    bool Tp1ExitGateEvaluated,
    bool Tp2Authorized,
    bool EvalOnly,
    bool DpoTrainReady,
    bool TeacherOutputUsedForTraining,
    bool TrainingStarted,
    int DpoStepsExecuted,
    bool CheckpointPromoted,
    bool PromotionStarted,
    bool ActivationStarted,
    bool HumanDecisionRequired);

internal sealed record Tp1ExitDecisionSubmissionValidationSuccess(
    bool Ok,
    string Status,
    string ValidationId,
    string DecisionRequestId,
    bool ShapeValidated,
    bool TrustedHumanDecisionPresent,
    bool Tp1ExitGateEvaluated);

internal static class Tp1ExitDecisionSubmissionContract
{
    private static readonly JsonSerializerOptions SnakeJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = false,
        MaxDepth = 32,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower),
        },
    };

    internal static IReadOnlyList<string> PropertyNames { get; } =
        Array.AsReadOnly(new[]
        {
            "decision_request_id",
            "source_ceiling_evaluation_sha256",
            "source_local_teacher_ceiling_candidate",
            "independent_reviewer_authority_ref",
            "independent_reviewed_at_utc",
            "source_run_ordering_verified",
            "source_authenticity_verified",
            "numeric_evidence_verified",
            "local_teacher_ceiling_accepted",
            "independent_acceptance",
            "luba_authority_ref",
            "luba_decided_at_utc",
            "tp1_exit_decision",
            "reason_refs",
        });

    private static HashSet<string> PropertySet { get; } =
        new(PropertyNames, StringComparer.Ordinal);

    internal static byte[] BuildSchema(
        string decisionRequestId,
        string sourceEvaluationSha256,
        bool candidate)
    {
        JsonObject RefType() => new()
        {
            ["type"] = "string",
            ["pattern"] = "^sha256:[0-9a-f]{64}$",
        };
        JsonObject UtcType() => new()
        {
            ["type"] = "string",
            ["format"] = "date-time",
            ["pattern"] = "Z$",
        };
        JsonObject BooleanType() => new() { ["type"] = "boolean" };

        var decisionEnum = new JsonArray("TP1_EXIT_ACCEPT", "TP1_EXIT_REJECT");
        var properties = new JsonObject
        {
            [PropertyNames[0]] = new JsonObject { ["const"] = decisionRequestId },
            [PropertyNames[1]] = new JsonObject { ["const"] = sourceEvaluationSha256 },
            [PropertyNames[2]] = new JsonObject { ["const"] = candidate },
            [PropertyNames[3]] = RefType(),
            [PropertyNames[4]] = UtcType(),
            [PropertyNames[5]] = BooleanType(),
            [PropertyNames[6]] = BooleanType(),
            [PropertyNames[7]] = BooleanType(),
            [PropertyNames[8]] = BooleanType(),
            [PropertyNames[9]] = BooleanType(),
            [PropertyNames[10]] = RefType(),
            [PropertyNames[11]] = UtcType(),
            [PropertyNames[12]] = new JsonObject
            {
                ["type"] = "string",
                ["enum"] = decisionEnum,
            },
            [PropertyNames[13]] = new JsonObject
            {
                ["type"] = "array",
                ["minItems"] = 1,
                ["uniqueItems"] = true,
                ["items"] = RefType(),
            },
        };
        var required = new JsonArray(PropertyNames
            .Select(static name => (JsonNode?)name)
            .ToArray());
        var allowedTuples = new JsonArray();
        for (var mask = 0; mask < 1 << 4; mask++)
        {
            var ordering = (mask & 1) != 0;
            var authenticity = (mask & 2) != 0;
            var numeric = (mask & 4) != 0;
            var ceilingAccepted = (mask & 8) != 0;
            foreach (var independentAcceptance in new[] { false, true })
            foreach (var decision in new[] { "TP1_EXIT_ACCEPT", "TP1_EXIT_REJECT" })
            {
                if (!IsCombinationAllowed(
                    candidate,
                    ordering,
                    authenticity,
                    numeric,
                    ceilingAccepted,
                    independentAcceptance,
                    decision))
                    continue;
                allowedTuples.Add(new JsonObject
                {
                    ["properties"] = new JsonObject
                    {
                        [PropertyNames[5]] = new JsonObject { ["const"] = ordering },
                        [PropertyNames[6]] = new JsonObject { ["const"] = authenticity },
                        [PropertyNames[7]] = new JsonObject { ["const"] = numeric },
                        [PropertyNames[8]] = new JsonObject { ["const"] = ceilingAccepted },
                        [PropertyNames[9]] = new JsonObject { ["const"] = independentAcceptance },
                        [PropertyNames[12]] = new JsonObject { ["const"] = decision },
                    },
                    ["required"] = new JsonArray(
                        PropertyNames[5],
                        PropertyNames[6],
                        PropertyNames[7],
                        PropertyNames[8],
                        PropertyNames[9],
                        PropertyNames[12]),
                });
            }
        }
        var schema = new JsonObject
        {
            ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
            ["$id"] = $"urn:xiigen:tp1-exit-decision:{decisionRequestId}",
            ["title"] = "TP1 exit human decision submission template",
            ["type"] = "object",
            ["additionalProperties"] = false,
            ["x_template_only"] = true,
            ["x_not_authority"] = true,
            ["x_never_parse_as_receipt"] = true,
            ["properties"] = properties,
            ["required"] = required,
            ["oneOf"] = allowedTuples,
        };
        return JsonSerializer.SerializeToUtf8Bytes(schema, SnakeJson);
    }

    internal static bool IsCombinationAllowed(
        bool sourceCandidate,
        bool sourceRunOrderingVerified,
        bool sourceAuthenticityVerified,
        bool numericEvidenceVerified,
        bool localTeacherCeilingAccepted,
        bool independentAcceptance,
        string tp1ExitDecision)
    {
        if (!StringComparer.Ordinal.Equals(tp1ExitDecision, "TP1_EXIT_ACCEPT")
            && !StringComparer.Ordinal.Equals(tp1ExitDecision, "TP1_EXIT_REJECT"))
            return false;
        if (!sourceCandidate)
        {
            return !localTeacherCeilingAccepted
                && !independentAcceptance
                && StringComparer.Ordinal.Equals(
                    tp1ExitDecision,
                    "TP1_EXIT_REJECT");
        }
        var allChecksPassed = sourceRunOrderingVerified
            && sourceAuthenticityVerified
            && numericEvidenceVerified
            && localTeacherCeilingAccepted;
        if (independentAcceptance != allChecksPassed)
            return false;
        return allChecksPassed
            || StringComparer.Ordinal.Equals(tp1ExitDecision, "TP1_EXIT_REJECT");
    }

    internal static Tp1ExitDecisionSubmissionParseResult ParseAndValidate(
        byte[] bytes,
        string expectedDecisionRequestId,
        string expectedSourceEvaluationSha256,
        bool expectedCandidate)
    {
        Tp1EvaluationEvidencePacketIo.ValidateStrictUtf8WithoutBom(bytes);
        using var document = JsonDocument.Parse(bytes, new JsonDocumentOptions
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow,
            MaxDepth = 32,
        });
        ValidateExactShape(document.RootElement);
        var submission = JsonSerializer.Deserialize<Tp1ExitHumanDecisionSubmissionArtifact>(
                bytes,
                SnakeJson)
            ?? throw new FormatException("The TP1 exit submission is empty.");
        var normalized = submission with
        {
            IndependentReviewedAtUtc = NormalizeUtc(submission.IndependentReviewedAtUtc),
            LubaDecidedAtUtc = NormalizeUtc(submission.LubaDecidedAtUtc),
        };
        ValidateSemantics(
            normalized,
            expectedDecisionRequestId,
            expectedSourceEvaluationSha256,
            expectedCandidate);
        return new Tp1ExitDecisionSubmissionParseResult(
            normalized,
            JsonSerializer.SerializeToUtf8Bytes(normalized, SnakeJson));
    }

    internal static bool IsCanonicalEvidenceRef(string value)
        => Tp1EvaluationEvidencePacketIo.IsCanonicalEvidenceRef(value);

    internal static string NormalizeUtc(string value)
    {
        if (string.IsNullOrEmpty(value)
            || !value.EndsWith('Z')
            || !DateTimeOffset.TryParseExact(
                value,
                new[]
                {
                    "yyyy-MM-dd'T'HH:mm:ss'Z'",
                    "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'",
                },
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsed)
            || parsed.Offset != TimeSpan.Zero)
            throw new FormatException("A normalized UTC-Z timestamp is required.");
        return parsed.UtcDateTime.ToString("O", CultureInfo.InvariantCulture);
    }

    private static void ValidateExactShape(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
            throw new FormatException("The TP1 exit submission must be an object.");
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in root.EnumerateObject())
        {
            if (!PropertySet.Contains(property.Name) || !seen.Add(property.Name))
                throw new FormatException("The TP1 exit submission property roster is invalid.");
        }
        if (!seen.SetEquals(PropertySet))
            throw new FormatException("The TP1 exit submission property roster is incomplete.");

        RequireKind(root, PropertyNames[0], JsonValueKind.String);
        RequireKind(root, PropertyNames[1], JsonValueKind.String);
        RequireKind(root, PropertyNames[2], JsonValueKind.True, JsonValueKind.False);
        RequireKind(root, PropertyNames[3], JsonValueKind.String);
        RequireKind(root, PropertyNames[4], JsonValueKind.String);
        for (var index = 5; index <= 9; index++)
            RequireKind(root, PropertyNames[index], JsonValueKind.True, JsonValueKind.False);
        RequireKind(root, PropertyNames[10], JsonValueKind.String);
        RequireKind(root, PropertyNames[11], JsonValueKind.String);
        RequireKind(root, PropertyNames[12], JsonValueKind.String);
        RequireKind(root, PropertyNames[13], JsonValueKind.Array);
        foreach (var item in root.GetProperty(PropertyNames[13]).EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
                throw new FormatException("Reason references must be strings.");
        }
    }

    private static void ValidateSemantics(
        Tp1ExitHumanDecisionSubmissionArtifact submission,
        string expectedDecisionRequestId,
        string expectedSourceEvaluationSha256,
        bool expectedCandidate)
    {
        if (!StringComparer.Ordinal.Equals(
                submission.DecisionRequestId,
                expectedDecisionRequestId)
            || !StringComparer.Ordinal.Equals(
                submission.SourceCeilingEvaluationSha256,
                expectedSourceEvaluationSha256)
            || submission.SourceLocalTeacherCeilingCandidate != expectedCandidate
            || !IsCanonicalEvidenceRef(submission.IndependentReviewerAuthorityRef)
            || !IsCanonicalEvidenceRef(submission.LubaAuthorityRef)
            || submission.ReasonRefs is null
            || submission.ReasonRefs.Count == 0
            || submission.ReasonRefs.Any(static value =>
                !IsCanonicalEvidenceRef(value))
            || submission.ReasonRefs.Distinct(StringComparer.Ordinal).Count()
                != submission.ReasonRefs.Count
            || !IsCombinationAllowed(
                expectedCandidate,
                submission.SourceRunOrderingVerified,
                submission.SourceAuthenticityVerified,
                submission.NumericEvidenceVerified,
                submission.LocalTeacherCeilingAccepted,
                submission.IndependentAcceptance,
                submission.Tp1ExitDecision))
            throw new FormatException("The TP1 exit submission bindings are invalid.");
    }

    private static void RequireKind(
        JsonElement root,
        string propertyName,
        params JsonValueKind[] allowed)
    {
        var kind = root.GetProperty(propertyName).ValueKind;
        if (!allowed.Contains(kind))
            throw new FormatException("The TP1 exit submission property type is invalid.");
    }
}
