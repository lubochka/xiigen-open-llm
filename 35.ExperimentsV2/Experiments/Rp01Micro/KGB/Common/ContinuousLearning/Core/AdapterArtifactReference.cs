// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Stable storage authority represented by an adapter artifact reference.
/// Explicit numeric values keep the serialized contract stable while allowing
/// unknown future values to deserialize and fail closed during conversion.
/// </summary>
internal enum AdapterArtifactStorageKind
{
    RetainedAdapterRoot = 1,
}

/// <summary>
/// Path-free immutable evidence that identifies one retained adapter artifact.
/// Conversion validates all caller-controlled data before it can reach the
/// retained-root capability.
/// </summary>
[JsonConverter(typeof(AdapterArtifactReferenceJsonConverter))]
internal sealed record AdapterArtifactReference
{
    internal const int CurrentSchemaVersion = 1;

    public AdapterArtifactReference(
        int schemaVersion,
        AdapterArtifactStorageKind storageKind,
        string canonicalLeafName,
        ServerOwnedContinuousLearningAdapterRootIdentity retainedRootIdentity,
        ServerOwnedContinuousLearningAdapterArtifactIdentity finalArtifactIdentity,
        long length,
        string sha256Hex)
    {
        SchemaVersion = schemaVersion;
        StorageKind = storageKind;
        CanonicalLeafName = canonicalLeafName;
        RetainedRootIdentity = retainedRootIdentity;
        FinalArtifactIdentity = finalArtifactIdentity;
        Length = length;
        Sha256Hex = sha256Hex;
    }

    [JsonPropertyName("schema_version")]
    public int SchemaVersion { get; init; }

    [JsonPropertyName("storage_kind")]
    public AdapterArtifactStorageKind StorageKind { get; init; }

    [JsonPropertyName("canonical_leaf_name")]
    public string CanonicalLeafName { get; init; }

    [JsonPropertyName("retained_root_identity")]
    public ServerOwnedContinuousLearningAdapterRootIdentity RetainedRootIdentity { get; init; }

    [JsonPropertyName("final_artifact_identity")]
    public ServerOwnedContinuousLearningAdapterArtifactIdentity FinalArtifactIdentity { get; init; }

    [JsonPropertyName("length")]
    public long Length { get; init; }

    [JsonPropertyName("sha256_hex")]
    public string Sha256Hex { get; init; }

    internal static AdapterArtifactReference FromPublication(
        ServerOwnedContinuousLearningAdapterPublication publication)
    {
        ValidateArtifactEvidence(
            publication.CanonicalLeafName,
            publication.RetainedRootIdentity,
            publication.FinalArtifactIdentity,
            publication.Length,
            publication.Sha256Hex);

        return new AdapterArtifactReference(
            CurrentSchemaVersion,
            AdapterArtifactStorageKind.RetainedAdapterRoot,
            publication.CanonicalLeafName,
            publication.RetainedRootIdentity,
            publication.FinalArtifactIdentity,
            publication.Length,
            publication.Sha256Hex);
    }

    internal ServerOwnedContinuousLearningAdapterPublication ToPublication()
        => ToPublication(this);

    internal static ServerOwnedContinuousLearningAdapterPublication ToPublication(
        AdapterArtifactReference? reference)
    {
        if (reference is null)
        {
            throw new AdapterArtifactReferenceValidationException(
                AdapterArtifactReferenceValidationException.ReferenceRequired);
        }
        if (reference.SchemaVersion != CurrentSchemaVersion)
        {
            throw new AdapterArtifactReferenceValidationException(
                AdapterArtifactReferenceValidationException.VersionUnsupported);
        }
        if (reference.StorageKind != AdapterArtifactStorageKind.RetainedAdapterRoot)
        {
            throw new AdapterArtifactReferenceValidationException(
                AdapterArtifactReferenceValidationException.KindUnsupported);
        }

        ValidateArtifactEvidence(
            reference.CanonicalLeafName,
            reference.RetainedRootIdentity,
            reference.FinalArtifactIdentity,
            reference.Length,
            reference.Sha256Hex);

        return new ServerOwnedContinuousLearningAdapterPublication(
            reference.CanonicalLeafName,
            reference.RetainedRootIdentity,
            reference.FinalArtifactIdentity,
            reference.Length,
            reference.Sha256Hex);
    }

    private static void ValidateArtifactEvidence(
        string? canonicalLeafName,
        ServerOwnedContinuousLearningAdapterRootIdentity retainedRootIdentity,
        ServerOwnedContinuousLearningAdapterArtifactIdentity finalArtifactIdentity,
        long length,
        string? sha256Hex)
    {
        if (!ServerOwnedContinuousLearningAdapterPublicationRequest
                .IsCanonicalFinalLeafName(canonicalLeafName) ||
            !IsValidIdentity(
                retainedRootIdentity.VolumeSerialNumber,
                retainedRootIdentity.FileIdLow,
                retainedRootIdentity.FileIdHigh) ||
            !IsValidIdentity(
                finalArtifactIdentity.VolumeSerialNumber,
                finalArtifactIdentity.FileIdLow,
                finalArtifactIdentity.FileIdHigh) ||
            length <= 0 ||
            length > ServerOwnedContinuousLearningCacheRootNative.MaximumOperationBytes ||
            !IsCanonicalSha256(sha256Hex))
        {
            throw new AdapterArtifactReferenceValidationException(
                AdapterArtifactReferenceValidationException.ReferenceInvalid);
        }
    }

    private static bool IsValidIdentity(
        ulong volumeSerialNumber,
        ulong fileIdLow,
        ulong fileIdHigh)
        => volumeSerialNumber != 0 && (fileIdLow != 0 || fileIdHigh != 0);

    private static bool IsCanonicalSha256(string? value)
        => value is { Length: 64 } &&
           value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');
}

/// <summary>
/// Owns the complete v1 wire boundary. No serializer naming policy or enum
/// converter participates in reading or writing this reference contract.
/// </summary>
internal sealed class AdapterArtifactReferenceJsonConverter
    : JsonConverter<AdapterArtifactReference>
{
    public override bool HandleNull => true;

    public override AdapterArtifactReference Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            throw new AdapterArtifactReferenceValidationException(
                AdapterArtifactReferenceValidationException.ReferenceRequired);
        }

        try
        {
            return ReadCanonicalReference(ref reader);
        }
        catch (AdapterArtifactReferenceValidationException)
        {
            throw;
        }
        catch (JsonException)
        {
            throw InvalidReference();
        }
        catch (InvalidOperationException)
        {
            throw InvalidReference();
        }
        catch (FormatException)
        {
            throw InvalidReference();
        }
        catch (OverflowException)
        {
            throw InvalidReference();
        }
    }

    public override void Write(
        Utf8JsonWriter writer,
        AdapterArtifactReference value,
        JsonSerializerOptions options)
    {
        if (value is null)
        {
            throw new AdapterArtifactReferenceValidationException(
                AdapterArtifactReferenceValidationException.ReferenceRequired);
        }

        _ = value.ToPublication();

        writer.WriteStartObject();
        writer.WriteNumber("schema_version", value.SchemaVersion);
        writer.WriteNumber("storage_kind", (int)value.StorageKind);
        writer.WriteString("canonical_leaf_name", value.CanonicalLeafName);
        WriteIdentity(writer, "retained_root_identity", value.RetainedRootIdentity);
        WriteIdentity(writer, "final_artifact_identity", value.FinalArtifactIdentity);
        writer.WriteNumber("length", value.Length);
        writer.WriteString("sha256_hex", value.Sha256Hex);
        writer.WriteEndObject();
    }

    private static AdapterArtifactReference ReadCanonicalReference(
        ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw InvalidReference();

        var rootDepth = reader.CurrentDepth;
        var schemaVersion = default(int);
        var storageKind = default(AdapterArtifactStorageKind);
        string? canonicalLeafName = null;
        var retainedRootIdentity = default(ServerOwnedContinuousLearningAdapterRootIdentity);
        var finalArtifactIdentity = default(ServerOwnedContinuousLearningAdapterArtifactIdentity);
        var length = default(long);
        string? sha256Hex = null;
        var hasSchemaVersion = false;
        var hasStorageKind = false;
        var hasCanonicalLeafName = false;
        var hasRetainedRootIdentity = false;
        var hasFinalArtifactIdentity = false;
        var hasLength = false;
        var hasSha256Hex = false;

        while (ReadNext(ref reader))
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw InvalidReference();

            var propertyName = reader.GetString();
            switch (propertyName)
            {
                case "schema_version":
                    RejectDuplicate(hasSchemaVersion);
                    hasSchemaVersion = true;
                    ReadNext(ref reader);
                    schemaVersion = ReadInt32(ref reader);
                    break;
                case "storage_kind":
                    RejectDuplicate(hasStorageKind);
                    hasStorageKind = true;
                    ReadNext(ref reader);
                    storageKind = (AdapterArtifactStorageKind)ReadInt32(ref reader);
                    break;
                case "canonical_leaf_name":
                    RejectDuplicate(hasCanonicalLeafName);
                    hasCanonicalLeafName = true;
                    ReadNext(ref reader);
                    canonicalLeafName = ReadString(ref reader);
                    break;
                case "retained_root_identity":
                    RejectDuplicate(hasRetainedRootIdentity);
                    hasRetainedRootIdentity = true;
                    ReadNext(ref reader);
                    var retained = ReadIdentity(ref reader);
                    retainedRootIdentity = new ServerOwnedContinuousLearningAdapterRootIdentity(
                        retained.VolumeSerialNumber,
                        retained.FileIdLow,
                        retained.FileIdHigh);
                    break;
                case "final_artifact_identity":
                    RejectDuplicate(hasFinalArtifactIdentity);
                    hasFinalArtifactIdentity = true;
                    ReadNext(ref reader);
                    var artifact = ReadIdentity(ref reader);
                    finalArtifactIdentity = new ServerOwnedContinuousLearningAdapterArtifactIdentity(
                        artifact.VolumeSerialNumber,
                        artifact.FileIdLow,
                        artifact.FileIdHigh);
                    break;
                case "length":
                    RejectDuplicate(hasLength);
                    hasLength = true;
                    ReadNext(ref reader);
                    length = ReadInt64(ref reader);
                    break;
                case "sha256_hex":
                    RejectDuplicate(hasSha256Hex);
                    hasSha256Hex = true;
                    ReadNext(ref reader);
                    sha256Hex = ReadString(ref reader);
                    break;
                default:
                    throw InvalidReference();
            }
        }

        if (reader.TokenType != JsonTokenType.EndObject ||
            !hasSchemaVersion ||
            !hasStorageKind ||
            !hasCanonicalLeafName ||
            !hasRetainedRootIdentity ||
            !hasFinalArtifactIdentity ||
            !hasLength ||
            !hasSha256Hex)
        {
            throw InvalidReference();
        }

        if (rootDepth == 0)
        {
            var trailingReader = reader;
            if (trailingReader.Read())
                throw InvalidReference();
        }

        var reference = new AdapterArtifactReference(
            schemaVersion,
            storageKind,
            canonicalLeafName!,
            retainedRootIdentity,
            finalArtifactIdentity,
            length,
            sha256Hex!);
        _ = reference.ToPublication();
        return reference;
    }

    private static (ulong VolumeSerialNumber, ulong FileIdLow, ulong FileIdHigh)
        ReadIdentity(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw InvalidReference();

        var volumeSerialNumber = default(ulong);
        var fileIdLow = default(ulong);
        var fileIdHigh = default(ulong);
        var hasVolumeSerialNumber = false;
        var hasFileIdLow = false;
        var hasFileIdHigh = false;

        while (ReadNext(ref reader))
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw InvalidReference();

            var propertyName = reader.GetString();
            switch (propertyName)
            {
                case "volume_serial_number":
                    RejectDuplicate(hasVolumeSerialNumber);
                    hasVolumeSerialNumber = true;
                    ReadNext(ref reader);
                    volumeSerialNumber = ReadUInt64(ref reader);
                    break;
                case "file_id_low":
                    RejectDuplicate(hasFileIdLow);
                    hasFileIdLow = true;
                    ReadNext(ref reader);
                    fileIdLow = ReadUInt64(ref reader);
                    break;
                case "file_id_high":
                    RejectDuplicate(hasFileIdHigh);
                    hasFileIdHigh = true;
                    ReadNext(ref reader);
                    fileIdHigh = ReadUInt64(ref reader);
                    break;
                default:
                    throw InvalidReference();
            }
        }

        if (reader.TokenType != JsonTokenType.EndObject ||
            !hasVolumeSerialNumber ||
            !hasFileIdLow ||
            !hasFileIdHigh)
        {
            throw InvalidReference();
        }

        return (volumeSerialNumber, fileIdLow, fileIdHigh);
    }

    private static void WriteIdentity(
        Utf8JsonWriter writer,
        string propertyName,
        ServerOwnedContinuousLearningAdapterRootIdentity identity)
    {
        writer.WriteStartObject(propertyName);
        writer.WriteNumber("volume_serial_number", identity.VolumeSerialNumber);
        writer.WriteNumber("file_id_low", identity.FileIdLow);
        writer.WriteNumber("file_id_high", identity.FileIdHigh);
        writer.WriteEndObject();
    }

    private static void WriteIdentity(
        Utf8JsonWriter writer,
        string propertyName,
        ServerOwnedContinuousLearningAdapterArtifactIdentity identity)
    {
        writer.WriteStartObject(propertyName);
        writer.WriteNumber("volume_serial_number", identity.VolumeSerialNumber);
        writer.WriteNumber("file_id_low", identity.FileIdLow);
        writer.WriteNumber("file_id_high", identity.FileIdHigh);
        writer.WriteEndObject();
    }

    private static bool ReadNext(ref Utf8JsonReader reader)
    {
        if (!reader.Read())
            throw InvalidReference();
        return true;
    }

    private static void RejectDuplicate(bool alreadyRead)
    {
        if (alreadyRead)
            throw InvalidReference();
    }

    private static string ReadString(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw InvalidReference();
        return reader.GetString() ?? throw InvalidReference();
    }

    private static int ReadInt32(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.Number || !reader.TryGetInt32(out var value))
            throw InvalidReference();
        return value;
    }

    private static long ReadInt64(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.Number || !reader.TryGetInt64(out var value))
            throw InvalidReference();
        return value;
    }

    private static ulong ReadUInt64(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.Number || !reader.TryGetUInt64(out var value))
            throw InvalidReference();
        return value;
    }

    private static AdapterArtifactReferenceValidationException InvalidReference()
        => new(AdapterArtifactReferenceValidationException.ReferenceInvalid);
}

/// <summary>
/// Narrow caller-data failure raised before retained filesystem trust is used.
/// </summary>
internal sealed class AdapterArtifactReferenceValidationException : ArgumentException
{
    internal const string ReferenceRequired = "ADAPTER_REFERENCE_REQUIRED";
    internal const string VersionUnsupported = "ADAPTER_REFERENCE_VERSION_UNSUPPORTED";
    internal const string KindUnsupported = "ADAPTER_REFERENCE_KIND_UNSUPPORTED";
    internal const string ReferenceInvalid = "ADAPTER_REFERENCE_INVALID";

    internal AdapterArtifactReferenceValidationException(string code)
        : base(code)
    {
        Code = code;
    }

    internal string Code { get; }
}
