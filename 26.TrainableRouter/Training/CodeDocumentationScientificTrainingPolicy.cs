// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
namespace TrainableRouter.Training; // Scientific admission is separate from native quality and optimizer execution.

using System.Text; // Hash exact source text without normalization.
using System.Text.Json; // Bind configured policy and independent real review bytes.
using System.Text.Json.Serialization; // New contracts reject unknown members.
using AgentInfrastructure; // Return typed refusals for missing or changed evidence.
using ContinuousLearning; // Reuse private retention/custody and actual assessment graphs.

public sealed record CodeDocumentationScientificSource(string RelativePath, string Sha256, bool Private); // Hash-bound protocol/grant/scorer input; no raw body is exposed.
public sealed record CodeDocumentationScientificCase(string CaseId, string FamilySha256, string OwnerInputSha256); // Freeze twelve independent source families before T1.
public sealed record CodeDocumentationScientificPolicyDocument(string Schema, string TrainingRunId, string ParentManifestSha256, string DatasetManifestSha256, string PairId, string ExaminerAuthority, int FamilyCount, int RepeatCount, int MinimumCorrectFamilyGain, int MaximumTrialCritical, int MaximumNewCritical, CodeDocumentationScientificSource Protocol, CodeDocumentationScientificSource Commitment, CodeDocumentationScientificSource EvaluationRetentionGrant, CodeDocumentationScientificSource ScorerBinding, CodeDocumentationScientificCase[] Cases); // Only operator-admitted preregistered source values belong here.
public sealed record CodeDocumentationScientificFamilyVerdict(string CaseId, string FamilySha256, bool BaselineCorrect, bool TrialCorrect, int TrialCriticalErrors, int NewCriticalErrors, string[] BaselineRawAnswerSha256, string[] TrialRawAnswerSha256); // Five repeats support each binary family result; they are not sixty independent families.
public sealed record CodeDocumentationScientificVerdict(string Schema, string GraderRole, string ExaminerAuthority, string PolicySha256, string ProtocolSha256, string TrainingRunId, string HeldBundleSha256, string SourceBindingSha256, string ParentManifestSha256, string TrialWeightsSha256, CodeDocumentationScientificFamilyVerdict[] Families); // This must be a real distinct examiner artifact, hash-pinned by trusted server startup.
public sealed record CodeDocumentationScientificTrainingResponse(string Status, string TrainingRunId, CodeDocumentationPrivateArtifactReference? HeldTrial, string? NativeQualityStatus, CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence? Publication) // Pending never pretends to carry a publication receipt.
{ // Optional accounting is independent of scientific score and frozen policy identity.
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Publish-only performs no new measurements.
    public CodeDocumentationScientificEvaluationAccounting? EvaluationAccounting { get; init; } // Historical costs remain UNKNOWN on recovery.
} // Policy document and source-binding serialization are unchanged.

public sealed class CodeDocumentationScientificTrainingPolicy // Captured server-owned scientific trust anchor, not a client boolean or Native capability.
{ // Operator provenance is explicit; a hash does not attest model identity or scientific independence.
    public const string PolicyRelativePath = "config/code-documentation/first-rp01-scientific-policy-v1.json"; // One fixed configured path, with no caller output path option.
    public const string Invalid = "CODE_DOCUMENTATION_SCIENTIFIC_POLICY_OR_VERDICT_INVALID"; // Missing/negative/drifted evidence cannot promote a model.
    internal static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = false, RespectRequiredConstructorParameters = true, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow }; // Exact camelCase, all required constructor fields, no unknown members or defaulted missing verdict booleans.
    internal CodeDocumentationScientificPolicyDocument Document { get; } // The frozen source/criteria preimage remains internal to lifecycle owners.
    public string Sha256 { get; } // Public digest is safe evidence, not an authorization claim by itself.
    internal string RepositoryRoot { get; } // Bind every source read to one canonical checkout.
    private readonly string _documentSha256; // Detect mutation of arrays retained inside the captured policy object.
    private CodeDocumentationScientificTrainingPolicy(string root, string sha, CodeDocumentationScientificPolicyDocument document) { RepositoryRoot = root; Sha256 = sha; Document = document; _documentSha256 = ValueHash(document); } // Only the hash-verified configuration reader constructs this object.
    public static OperationResult<CodeDocumentationScientificTrainingPolicy> Open(string repositoryRoot, string policySha256, CancellationToken token = default) // Read the fixed preregistered policy before T1.
    { // The server, not request JSON, supplies this digest.
        var bytes = CodeDocumentationPrivateEvaluationStore.ReadTrustedInput(repositoryRoot, PolicyRelativePath, policySha256, true, token); // Verify actual private configuration bytes/custody.
        if (!bytes.IsSuccess || bytes.Value is null) return bytes.MapError<CodeDocumentationScientificTrainingPolicy>(); // Refuse missing/changed policy without model work.
        try // Policy text is untrusted input, not instructions.
        { // Strictly bind finite frozen first-RP01 criteria.
            using var raw = JsonDocument.Parse(bytes.Value); // Duplicate members cannot select a different criterion.
            if (HasDuplicates(raw.RootElement)) return Refuse<CodeDocumentationScientificTrainingPolicy>(); // Reject ambiguous policy values.
            var document = JsonSerializer.Deserialize<CodeDocumentationScientificPolicyDocument>(bytes.Value, Json); // Unknown members are rejected.
            if (document is null || document.Schema != "first-rp01-scientific-policy-v1" || document.FamilyCount != 12 || document.RepeatCount != 5 || document.MinimumCorrectFamilyGain != 2 || document.MaximumTrialCritical != 0 || document.MaximumNewCritical != 0 || // Frozen acceptance is twelve paired binary families, gain at least two, zero trial/new criticals.
                !Hex(policySha256) || !Hex(document.ParentManifestSha256) || !Hex(document.DatasetManifestSha256) || !Hex(document.PairId) || !Visible(document.TrainingRunId) || !Visible(document.ExaminerAuthority) || // Require explicit actual run/model/lesson/grader binding.
                document.Cases is not { Length: 12 } || document.Cases.Any(c => c is null || !Visible(c.CaseId) || !Hex(c.FamilySha256) || !Hex(c.OwnerInputSha256)) || document.Cases.Select(c => c.CaseId).Distinct(StringComparer.Ordinal).Count() != 12 || document.Cases.Select(c => c.FamilySha256).Distinct(StringComparer.Ordinal).Count() != 12 || // Do not inflate five repeats into independent case families.
                document.EvaluationRetentionGrant is not { Private: true } || document.ScorerBinding is not { Private: true }) return Refuse<CodeDocumentationScientificTrainingPolicy>(); // Evaluation retention/scorer binding remain private and cannot become training authority.
            var policy = new CodeDocumentationScientificTrainingPolicy(repositoryRoot, policySha256, document); // Retain this immutable actual decoded preimage.
            var current = policy.ValidateCurrent(token); // Prove protocol, commitment, real retention grant and frozen scorer references now.
            return current.IsSuccess ? OperationResult<CodeDocumentationScientificTrainingPolicy>.Ok(policy) : current.MapError<CodeDocumentationScientificTrainingPolicy>(); // No model/evaluator action has happened.
        } // Finish configuration admission.
        catch (Exception exception) when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { return Refuse<CodeDocumentationScientificTrainingPolicy>(); } // Do not disclose configured private sources.
    } // The actual independent grader executes later, outside this owner.
    internal OperationResult<bool> ValidateCurrent(CancellationToken token) // Recheck the fixed policy and all supporting hashes at effect/publication boundaries.
    { // Criteria and source selection cannot change after C0 or optimizer execution.
        if (ValueHash(Document) != _documentSha256) return Refuse<bool>(); // Frozen in-memory preimage must still match the actually admitted policy.
        var policy = CodeDocumentationPrivateEvaluationStore.ReadTrustedInput(RepositoryRoot, PolicyRelativePath, Sha256, true, token); // Bind actual current policy bytes again.
        if (!policy.IsSuccess) return policy.MapError<bool>(); // A stale server object cannot admit changed files.
        foreach (var source in new[] { Document.Protocol, Document.Commitment, Document.EvaluationRetentionGrant, Document.ScorerBinding }) // Freeze all actual source support, not only a verdict's assertion.
        { // No Gold/rubric contents are parsed or logged here.
            if (source is null || !Hex(source.Sha256)) return Refuse<bool>(); // Missing references never become default grants.
            var read = CodeDocumentationPrivateEvaluationStore.ReadTrustedInput(RepositoryRoot, source.RelativePath, source.Sha256, source.Private, token); // Hash/custody-read each real artifact through the existing private owner.
            if (!read.IsSuccess) return read.MapError<bool>(); // Any source drift prevents T1 or publication.
        } // Evaluation retention and scorer scope are operator-trusted, not claimed cryptographic attestation.
        return OperationResult<bool>.Ok(true); // Retain the exact frozen pre-T1 trust anchor.
    } // This check does not score student ability.
    internal static bool HasDuplicates(JsonElement e) => e.ValueKind == JsonValueKind.Object ? e.EnumerateObject().GroupBy(p => p.Name, StringComparer.Ordinal).Any(g => g.Count() != 1) || e.EnumerateObject().Any(p => HasDuplicates(p.Value)) : e.ValueKind == JsonValueKind.Array && e.EnumerateArray().Any(HasDuplicates); // Reject ambiguous nested source/verdict data.
    internal static string ValueHash<T>(T value) => CodeDocumentationPrivateEvaluationStore.Hash(JsonSerializer.SerializeToUtf8Bytes(value, Json)); // Bind complete actual values with a fixed serializer.
    internal static string TextHash(string text) => CodeDocumentationPrivateEvaluationStore.Hash(new UTF8Encoding(false, true).GetBytes(text)); // Preserve Unicode/control/newline identity.
    internal static bool Hex(string? value) => value is { Length: 64 } && value.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f'); // Use canonical lowercase SHA256 values.
    internal static bool Visible(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 256 && value == value.Trim() && !value.Any(char.IsControl); // Keep public identifiers bounded and path-free.
    internal static OperationResult<T> Refuse<T>() => OperationResult<T>.Fail("Frozen scientific policy, real independent verdict or complete bound private evidence is unavailable or rejected.", Invalid); // Never turn missing evidence into acceptance.
} // Existing native quality acceptance remains a necessary separate condition.
