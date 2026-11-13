// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
namespace ContinuousLearning;

using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;

/// <summary>Complete source/selection preimage for one raw TRAIN task; no paid answer is asserted.</summary>
public sealed record RepositoryOwnedDpoRawSourceIdentity(
    string Schema, string SourceKind, string Partition, int SourceCount,
    string SourceId, string FamilyId, string Language, string LanguageVersion,
    string OwnerInputSha256, string SpecimenSha256, string QuestionSha256,
    string SelectionOrigin, string SelectorAgentId, string SelectionReceiptSha256,
    string OperatorAuthorizationSha256, string IntakeSha256);

public static partial class RepositoryOwnedDpoDatasetManifestSealer
{
    public const string RawRequestIdentitySchema = "repository-owned-dpo-dataset-request-identity-raw-v2";
    public const string RawSourceSchema = "raw-code-documentation-train-source-v1";
    public const string OriginalSyntheticTrainSource = "ORIGINAL_SYNTHETIC_AGENT_AUTHORED_TRAIN_SPECIMEN";
    public const string PlatformSelectionOrigin = "ACTUAL_PLATFORM_SUBAGENT_SELECTION";
    private static readonly JsonSerializerOptions RawSourceJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    /// <summary>Seals identity only. Actual source-use authority belongs to the operator intake owner.</summary>
    public static OperationResult<RepositoryOwnedDpoDatasetRequestIdentity> CreateRawRequestIdentity(
        RepositoryOwnedDpoRawSourceIdentity source, RepositoryOwnedDpoOpenTeacherIdentity teacher,
        RepositoryOwnedDpoGenerationPolicyIdentity policy, RepositoryOwnedDpoCreatorIdentity creator)
    {
        if (!ValidateRawSource(source) || !ValidateOpenTeacherIdentity(teacher) ||
            !ValidateGenerationPolicy(policy) || !ValidateCreatorIdentity(creator))
            return Invalid<RepositoryOwnedDpoDatasetRequestIdentity>();
        var unsigned = new RepositoryOwnedDpoDatasetRequestIdentity(RawRequestIdentitySchema, null,
            teacher, policy, creator, string.Empty, source with { });
        var identity = unsigned with { RequestIdentitySha256 = ComputeSha256(WriteCanonicalRequestIdentityBytes(unsigned)) };
        var valid = ValidateRequestIdentity(identity);
        return valid.IsSuccess ? OperationResult<RepositoryOwnedDpoDatasetRequestIdentity>.Ok(identity)
            : valid.MapError<RepositoryOwnedDpoDatasetRequestIdentity>();
    }

    /// <summary>Returns the complete canonical preimage, including all raw-source fields, for historical bindings.</summary>
    public static byte[] CopyCanonicalRequestIdentityBytes(RepositoryOwnedDpoDatasetRequestIdentity identity)
    {
        if (!IsValidRequestIdentity(identity)) throw new ArgumentException("Invalid request identity.", nameof(identity));
        return WriteCanonicalRequestIdentityBytes(identity);
    }

    private static bool ValidateRequestSource(RepositoryOwnedDpoDatasetRequestIdentity identity)
        => identity.Schema == CurrentRequestIdentitySchema
            ? identity.RawSource is null && ValidatePaidReferenceAnswerSet(identity.PaidReferenceAnswers)
            : identity.Schema == RawRequestIdentitySchema && identity.PaidReferenceAnswers is null && ValidateRawSource(identity.RawSource);

    private static bool ValidateRawSource(RepositoryOwnedDpoRawSourceIdentity? source)
        => source is not null && source.Schema == RawSourceSchema && source.SourceKind == OriginalSyntheticTrainSource &&
           source.Partition == "TRAIN" && source.SourceCount == 1 && source.Language == "C#" && source.LanguageVersion == "12.0" &&
           source.SelectionOrigin == PlatformSelectionOrigin &&
           new[] { source.SourceId, source.FamilyId }.All(IsSafeIdentityText) &&
           !string.IsNullOrWhiteSpace(source.SelectorAgentId) && source.SelectorAgentId.Length <= 256 &&
           source.SelectorAgentId == source.SelectorAgentId.Trim() && source.SelectorAgentId.All(c => c is >= '!' and <= '~') &&
           new[] { source.OwnerInputSha256, source.SpecimenSha256, source.QuestionSha256, source.SelectionReceiptSha256,
               source.OperatorAuthorizationSha256, source.IntakeSha256 }.All(IsCanonicalSha256Hex);

    private static void WriteRawSource(Utf8JsonWriter writer, RepositoryOwnedDpoRawSourceIdentity source)
        => JsonSerializer.Serialize(writer, source, RawSourceJson);

    private static bool TryReadRawSource(JsonElement element, out RepositoryOwnedDpoRawSourceIdentity? source)
    {
        source = null;
        // Duplicate, extra and missing properties cannot hide a second source preimage.
        if (element.ValueKind != JsonValueKind.Object) return false;
        var fields = element.EnumerateObject().ToArray();
        if (fields.Length != 16 || fields.Select(p => p.Name).Distinct(StringComparer.Ordinal).Count() != 16) return false;
        try { source = element.Deserialize<RepositoryOwnedDpoRawSourceIdentity>(RawSourceJson); }
        catch (JsonException) { return false; }
        return ValidateRawSource(source);
    }
}
