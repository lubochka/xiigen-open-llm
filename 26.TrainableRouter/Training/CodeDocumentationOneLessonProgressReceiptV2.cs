// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;

/// <summary>
/// A complete, immutable description of structurally valid receipt bytes.
/// It is untrusted data: no slot identity, reference resolution, owner receipt,
/// predecessor, branch, terminal state, or effect permission is established.
/// </summary>
internal sealed class CodeDocumentationOneLessonProgressReceiptV2
{
    private static readonly UTF8Encoding Utf8 = new(false, true);
    private readonly byte[] _canonicalBytes;

    private CodeDocumentationOneLessonProgressReceiptV2(JsonElement value, byte[] bytes)
    {
        // Clone the whole graph; neither document disposal nor caller byte edits
        // can alter this descriptive snapshot or its complete-object digest.
        Value = value.Clone();
        _canonicalBytes = bytes;
        ReceiptSha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    internal JsonElement Value { get; }
    internal JsonElement Payload => Value.GetProperty("Payload");
    internal string ReceiptType => Value.GetProperty("ReceiptType").GetString()!;
    internal string ReceiptId => Value.GetProperty("ReceiptId").GetString()!;
    internal string ReceiptSha256 { get; }
    internal byte[] CanonicalBytes => (byte[])_canonicalBytes.Clone();

    internal static OperationResult<CodeDocumentationOneLessonProgressReceiptV2> Decode(
        JsonElement value, CodeDocumentationOneLessonSessionBinding? expectedBinding)
    {
        try
        {
            if (value.ValueKind != JsonValueKind.Object ||
                !value.TryGetProperty("ReceiptType", out var typeValue) ||
                typeValue.ValueKind != JsonValueKind.String)
                return Recovery();
            var type = typeValue.GetString();
            var cap = ObjectCeiling(type);
            if (cap == 0) return Recovery();

            // Recognized type first, then the actual COMPLETE raw UTF-8 object
            // ceiling, before payload primitive validation. The session already
            // enforces its independent LF-inclusive frame ceiling before here.
            var rawBytes = Utf8.GetBytes(value.GetRawText());
            if (rawBytes.Length > cap)
                return OperationResult<CodeDocumentationOneLessonProgressReceiptV2>.Fail(
                    "The complete V2 progress receipt exceeds its type's object ceiling.",
                    CodeDocumentationOneLessonSession.CapacityExceededErrorCode);

            if (!CodeDocumentationOneLessonV2Wire.ExactFields(value,
                    "SchemaVersion", "ReceiptType", "ReceiptId", "ObjectiveId", "SessionId",
                    "TrainingRunId", "BindingSha256", "Payload") ||
                value.GetProperty("SchemaVersion").GetRawText() != "2" ||
                !HashField(value, "ReceiptId", 32) ||
                !CodeDocumentationOneLessonSession.ValidProgressReceiptBindingV2(expectedBinding) ||
                !SameString(value, "ObjectiveId", expectedBinding!.ObjectiveId) ||
                !SameString(value, "TrainingRunId", expectedBinding.TrainingRunId) ||
                !SameString(value, "SessionId", Convert.ToHexString(SHA256.HashData(
                    Utf8.GetBytes(expectedBinding.ObjectiveId))).ToLowerInvariant()) ||
                !SameString(value, "BindingSha256", CodeDocumentationOneLessonSession.ComputeBindingSha256(expectedBinding)) ||
                !ValidPayload(type!, value.GetProperty("Payload")) ||
                !rawBytes.AsSpan().SequenceEqual(CodeDocumentationOneLessonV2Wire.CanonicalBytes(value)))
                return Recovery();

            return OperationResult<CodeDocumentationOneLessonProgressReceiptV2>.Ok(new(value, rawBytes));
        }
        catch (Exception exception) when (exception is JsonException or EncoderFallbackException or
            InvalidOperationException or ArgumentException)
        {
            // Malformed JSON/Unicode and disposed/undefined caller values never
            // leak a partially decoded graph or a usable receipt capability.
            return Recovery();
        }
    }

    private static int ObjectCeiling(string? type) => type switch
    {
        "SealedPair" => 1169, "RunnerCompletion" => 1360, "Baseline" => 1239,
        "VerifiedAppliedUpdate" => 1235, "TrialEvaluation" => 1372, "QualityDecision" => 1388,
        "Disposition" => 1092, "PublicationStarted" => 1214, "PublicationReceipt" => 1360,
        "FinalReopen" => 1298, _ => 0,
    };

    private static bool ValidPayload(string type, JsonElement p) => type switch
    {
        "SealedPair" => Fields(p, "SourceSelectionSha256", "TrainingPairId", "SealedRowSha256", "AuthoringAttemptIdSha256") &&
            Hashes(p, "SourceSelectionSha256", "TrainingPairId", "SealedRowSha256", "AuthoringAttemptIdSha256"),
        "RunnerCompletion" => Fields(p, "SealedPairRef", "RunnerOutcomeSha256") &&
            Ref(p, "SealedPairRef") && Hashes(p, "RunnerOutcomeSha256"),
        "Baseline" => Fields(p, "ExternalBaselineRef", "BaselineObjectIdSha256", "TrainingPairId", "AttemptStateSha256") &&
            Ref(p, "ExternalBaselineRef") && Hashes(p, "BaselineObjectIdSha256", "TrainingPairId", "AttemptStateSha256"),
        "VerifiedAppliedUpdate" => Fields(p, "AttemptObjectIdSha256", "AttemptStateSha256", "ParentCheckpointSha256", "AppliedWeightsSha256", "OptimizerEntriesDecimal") &&
            Hashes(p, "AttemptObjectIdSha256", "AttemptStateSha256", "ParentCheckpointSha256", "AppliedWeightsSha256") &&
            Decimal(p.GetProperty("OptimizerEntriesDecimal")),
        "TrialEvaluation" => Fields(p, "EvaluationPlanSha256", "SourceSelectionSha256", "TrainingPairId", "CompletedSlotCount", "SlotCompletionSetSha256", "CaseSetSha256", "ConditionSha256") &&
            Hashes(p, "EvaluationPlanSha256", "SourceSelectionSha256", "TrainingPairId", "SlotCompletionSetSha256", "CaseSetSha256", "ConditionSha256") &&
            Integer(p.GetProperty("CompletedSlotCount"), 1, 5000),
        "QualityDecision" => Quality(p),
        "Disposition" => Fields(p, "Disposition", "QualityDecisionRef", "SelectedCheckpointSha256") &&
            Choice(p, "Disposition", "RetainParent", "PromoteChild") && Ref(p, "QualityDecisionRef") && Hashes(p, "SelectedCheckpointSha256"),
        "PublicationStarted" => Fields(p, "DispositionRef", "PublicationId", "ParentCheckpointSha256", "ProposedChildCheckpointSha256") &&
            Ref(p, "DispositionRef") && HashField(p, "PublicationId", 32) && Hashes(p, "ParentCheckpointSha256", "ProposedChildCheckpointSha256"),
        "PublicationReceipt" => Fields(p, "PublicationStartedRef", "PublicationId", "ChildCheckpointSha256", "ManifestSha256", "WeightsSha256", "WeightsByteLengthDecimal", "ChildStep") &&
            Ref(p, "PublicationStartedRef") && HashField(p, "PublicationId", 32) && Hashes(p, "ChildCheckpointSha256", "ManifestSha256", "WeightsSha256") &&
            Decimal(p.GetProperty("WeightsByteLengthDecimal")) && Integer(p.GetProperty("ChildStep"), 1, int.MaxValue),
        "FinalReopen" => Fields(p, "SelectedRole", "CheckpointSha256", "ManifestSha256", "WeightsSha256", "WeightsByteLengthDecimal", "PredecessorReceiptRef") &&
            Choice(p, "SelectedRole", "Parent", "Child") && Hashes(p, "CheckpointSha256", "ManifestSha256", "WeightsSha256") &&
            Decimal(p.GetProperty("WeightsByteLengthDecimal")) && Ref(p, "PredecessorReceiptRef"),
        _ => false,
    };

    private static bool Quality(JsonElement p)
    {
        if (p.ValueKind != JsonValueKind.Object || !p.TryGetProperty("Decision", out var decision) ||
            decision.ValueKind != JsonValueKind.String) return false;
        return decision.GetString() switch
        {
            "BaselineSufficient" => Fields(p, "Decision", "ExternalBaselineRef", "DecisionInputsSha256") &&
                Ref(p, "ExternalBaselineRef") && Hashes(p, "DecisionInputsSha256"),
            "Improved" or "NotImproved" => Fields(p, "Decision", "ExternalBaselineRef", "DecisionInputsSha256", "BaselineRef", "TrialEvaluationRef") &&
                Ref(p, "ExternalBaselineRef") && Ref(p, "BaselineRef") && Ref(p, "TrialEvaluationRef") && Hashes(p, "DecisionInputsSha256"),
            _ => false,
        };
    }

    private static bool Fields(JsonElement p, params string[] fields)
        => CodeDocumentationOneLessonV2Wire.ExactFields(p, fields);

    private static bool SameString(JsonElement p, string name, string expected)
        => p.GetProperty(name).ValueKind == JsonValueKind.String && p.GetProperty(name).GetString() == expected;

    private static bool HashField(JsonElement p, string name, int width = 64)
        => p.GetProperty(name).ValueKind == JsonValueKind.String &&
           CodeDocumentationOneLessonV2Wire.Hash(p.GetProperty(name).GetString(), width);

    private static bool Hashes(JsonElement p, params string[] fields)
        => fields.All(name => HashField(p, name));

    private static bool Ref(JsonElement p, string name)
    {
        var r = p.GetProperty(name);
        return Fields(r, "ReceiptId", "ReceiptSha256") && HashField(r, "ReceiptId", 32) && Hashes(r, "ReceiptSha256");
    }

    private static bool Choice(JsonElement p, string name, params string[] choices)
        => p.GetProperty(name).ValueKind == JsonValueKind.String &&
           choices.Contains(p.GetProperty(name).GetString(), StringComparer.Ordinal);

    private static bool Decimal(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.String) return false;
        var s = value.GetString();
        return s is { Length: >= 1 and <= 19 } && (s.Length == 1 || s[0] != '0') &&
            s.All(c => c is >= '0' and <= '9') &&
            long.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out _);
    }

    private static bool Integer(JsonElement value, int minimum, int maximum)
        => value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var n) && n >= minimum && n <= maximum &&
           value.GetRawText() == n.ToString(CultureInfo.InvariantCulture);

    private static OperationResult<CodeDocumentationOneLessonProgressReceiptV2> Recovery()
        => OperationResult<CodeDocumentationOneLessonProgressReceiptV2>.Fail(
            "The V2 progress receipt is malformed, noncanonical, or differs from the expected binding.",
            CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode);
}
