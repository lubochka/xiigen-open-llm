// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace ContinuousLearning;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using LoRA;
using TransformerModel.Config;

public sealed record TrainingSourceManifest(
    IReadOnlyList<string> SourceIds,
    string Sha256);

public sealed record TrainingSourceIdentity(
    string SourceId,
    string OriginSha256,
    string ContentSha256);

public sealed record ModelConfigurationIdentity(
    int VocabSize,
    int ContextLen,
    int HiddenDim,
    int NumLayers,
    int NumHeads,
    int NumKvHeads,
    int FfnHiddenDim,
    float RopeTheta,
    float RmsNormEps,
    string Sha256);

public sealed record LoraConfigurationIdentity(
    int Rank,
    float Alpha,
    float Scaling,
    float Dropout,
    IReadOnlyList<string> OrderedTargetModules,
    string Sha256);

/// <summary>
/// Exact durable receipt which binds a published adapter to one selected DPO
/// queue transaction.  It is evidence only; it grants no training authority.
/// </summary>
public sealed record DpoAdapterPublicationReceipt(
    string TransactionId,
    string PairBatchSha256,
    IReadOnlyList<string> OrderedPairIds,
    IReadOnlyList<string> OrderedDomains);

public sealed record ModelAdapterProvenance(
    TrainingSourceManifest TrainingSources,
    string AdapterSha256,
    string BaseCheckpointSha256,
    string TokenizerSha256,
    ModelConfigurationIdentity Model,
    LoraConfigurationIdentity Lora,
    DpoAdapterPublicationReceipt? DpoPublication = null);

public sealed record TrainedAdapterArtifact(
    string AdapterPath,
    ModelAdapterProvenance Provenance);

public static class TrainingSourceProvenance
{
    public const int CanonicalSourceIdLength = 133;

    public static string CreateSourceId(string origin, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(origin);
        ArgumentNullException.ThrowIfNull(content);
        var canonicalOrigin = NormalizeOrigin(origin);
        return string.Concat(
            "src:",
            Sha256Utf8(canonicalOrigin),
            ":",
            Sha256Utf8(content));
    }

    public static bool TryParseSourceId(
        string? sourceId,
        out TrainingSourceIdentity? identity)
    {
        identity = null;
        if (!IsCanonicalSourceId(sourceId))
            return false;
        identity = new TrainingSourceIdentity(
            sourceId!,
            sourceId!.Substring(4, 64),
            sourceId.Substring(69, 64));
        return true;
    }

    public static OperationResult<TrainingSourceManifest> BuildManifest(
        IEnumerable<string?> sourceIds)
    {
        if (sourceIds is null)
        {
            return OperationResult<TrainingSourceManifest>.Fail(
                "Training source provenance is required.",
                "TRAINING_SOURCE_MANIFEST_REQUIRED");
        }

        var ordered = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var sourceId in sourceIds)
        {
            if (!IsCanonicalSourceId(sourceId))
            {
                return OperationResult<TrainingSourceManifest>.Fail(
                    "Every training source id must use the canonical source identity format.",
                    "TRAINING_SOURCE_ID_NONCANONICAL");
            }

            if (!ordered.Add(sourceId!))
            {
                return OperationResult<TrainingSourceManifest>.Fail(
                    "Training source ids must be unique.",
                    "TRAINING_SOURCE_ID_DUPLICATE");
            }
        }

        if (ordered.Count == 0)
        {
            return OperationResult<TrainingSourceManifest>.Fail(
                "Training source provenance is required.",
                "TRAINING_SOURCE_MANIFEST_REQUIRED");
        }

        var snapshot = Array.AsReadOnly(ordered.ToArray());
        return OperationResult<TrainingSourceManifest>.Ok(
            new TrainingSourceManifest(
                snapshot,
                Sha256Utf8(string.Join('\n', snapshot))));
    }

    public static OperationResult<TrainingSourceManifest> Combine(
        TrainingSourceManifest? inherited,
        TrainingSourceManifest current)
    {
        ArgumentNullException.ThrowIfNull(current);
        var currentValidation = ValidateManifest(current);
        if (!currentValidation.IsSuccess)
            return currentValidation;
        if (inherited is null)
            return currentValidation;

        var inheritedValidation = ValidateManifest(inherited);
        if (!inheritedValidation.IsSuccess)
            return inheritedValidation;

        return BuildManifest(
            inherited.SourceIds
                .Concat(current.SourceIds)
                .Distinct(StringComparer.Ordinal));
    }

    private static OperationResult<TrainingSourceManifest> ValidateManifest(
        TrainingSourceManifest manifest)
    {
        if (manifest.SourceIds is null)
        {
            return OperationResult<TrainingSourceManifest>.Fail(
                "Training source provenance is required.",
                "TRAINING_SOURCE_MANIFEST_REQUIRED");
        }

        var rebuilt = BuildManifest(manifest.SourceIds);
        if (!rebuilt.IsSuccess || rebuilt.Value is null)
            return rebuilt;
        if (!StringComparer.Ordinal.Equals(rebuilt.Value.Sha256, manifest.Sha256))
        {
            return OperationResult<TrainingSourceManifest>.Fail(
                "Training source manifest hash does not match its source ids.",
                "TRAINING_SOURCE_MANIFEST_HASH_MISMATCH");
        }

        return rebuilt;
    }

    public static bool IsCanonicalSourceId(string? sourceId)
    {
        if (sourceId is null || sourceId.Length != CanonicalSourceIdLength)
            return false;
        if (!sourceId.StartsWith("src:", StringComparison.Ordinal))
            return false;
        if (sourceId[68] != ':')
            return false;

        for (var index = 4; index < sourceId.Length; index++)
        {
            if (index == 68)
                continue;
            var character = sourceId[index];
            if (character is not (>= '0' and <= '9')
                and not (>= 'a' and <= 'f'))
            {
                return false;
            }
        }

        return true;
    }

    public static ModelConfigurationIdentity Identify(ModelConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        var canonical = string.Join('|',
            config.VocabSize.ToString(CultureInfo.InvariantCulture),
            config.ContextLen.ToString(CultureInfo.InvariantCulture),
            config.HiddenDim.ToString(CultureInfo.InvariantCulture),
            config.NumLayers.ToString(CultureInfo.InvariantCulture),
            config.NumHeads.ToString(CultureInfo.InvariantCulture),
            config.NumKvHeads.ToString(CultureInfo.InvariantCulture),
            config.FfnHiddenDim.ToString(CultureInfo.InvariantCulture),
            config.RopeTheta.ToString("R", CultureInfo.InvariantCulture),
            config.RmsNormEps.ToString("R", CultureInfo.InvariantCulture));
        return new ModelConfigurationIdentity(
            config.VocabSize,
            config.ContextLen,
            config.HiddenDim,
            config.NumLayers,
            config.NumHeads,
            config.NumKvHeads,
            config.FfnHiddenDim,
            config.RopeTheta,
            config.RmsNormEps,
            Sha256Utf8(canonical));
    }

    public static LoraConfigurationIdentity Identify(LoraConfig config)
    {
        var canonicalConfig = LoraApplicator.ValidateAndCanonicalize(config);
        var targets = Array.AsReadOnly(canonicalConfig.TargetModules.ToArray());
        var canonical = string.Join('|',
            canonicalConfig.Rank.ToString(CultureInfo.InvariantCulture),
            canonicalConfig.Alpha.ToString("R", CultureInfo.InvariantCulture),
            canonicalConfig.Scaling.ToString("R", CultureInfo.InvariantCulture),
            canonicalConfig.Dropout.ToString("R", CultureInfo.InvariantCulture),
            string.Join(',', targets));
        return new LoraConfigurationIdentity(
            canonicalConfig.Rank,
            canonicalConfig.Alpha,
            canonicalConfig.Scaling,
            canonicalConfig.Dropout,
            targets,
            Sha256Utf8(canonical));
    }

    public static bool IsCanonicalSha256(string? value)
    {
        if (value is null || value.Length != 64)
            return false;
        foreach (var character in value)
        {
            if (character is not (>= '0' and <= '9')
                and not (>= 'a' and <= 'f'))
            {
                return false;
            }
        }
        return true;
    }

    public static OperationResult<DpoAdapterPublicationReceipt>
        BuildDpoPublicationReceipt(
            IEnumerable<string?> pairIds,
            IEnumerable<string?> domains)
    {
        if (pairIds is null || domains is null)
        {
            return OperationResult<DpoAdapterPublicationReceipt>.Fail(
                "DPO publication identity is required.",
                "DPO_PUBLICATION_IDENTITY_INVALID");
        }

        var pairSnapshot = SnapshotBounded(pairIds, 1024);
        var domainSnapshot = SnapshotBounded(domains, 256);
        if (!pairSnapshot.IsSuccess || pairSnapshot.Value is null)
            return pairSnapshot.MapError<DpoAdapterPublicationReceipt>();
        if (!domainSnapshot.IsSuccess || domainSnapshot.Value is null)
            return domainSnapshot.MapError<DpoAdapterPublicationReceipt>();
        var orderedPairIds = pairSnapshot.Value;
        var orderedDomains = domainSnapshot.Value;
        if (orderedPairIds.Length == 0
            || orderedDomains.Length == 0
            || orderedPairIds.Any(static value => !IsCanonicalSha256(value))
            || orderedDomains.Any(static value =>
                string.IsNullOrWhiteSpace(value)
                || value.Length > 256
                || !StringComparer.Ordinal.Equals(value, value.Trim())
                || value.Any(char.IsControl))
            || !orderedPairIds.SequenceEqual(
                orderedPairIds.OrderBy(static value => value, StringComparer.Ordinal),
                StringComparer.Ordinal)
            || !orderedDomains.SequenceEqual(
                orderedDomains.OrderBy(static value => value, StringComparer.Ordinal),
                StringComparer.Ordinal)
            || orderedPairIds.Distinct(StringComparer.Ordinal).Count()
                != orderedPairIds.Length
            || orderedDomains.Distinct(StringComparer.Ordinal).Count()
                != orderedDomains.Length)
        {
            return OperationResult<DpoAdapterPublicationReceipt>.Fail(
                "DPO publication identity is invalid.",
                "DPO_PUBLICATION_IDENTITY_INVALID");
        }

        var canonicalPairIds = Array.AsReadOnly(
            orderedPairIds.Select(static value => value!).ToArray());
        var canonicalDomains = Array.AsReadOnly(
            orderedDomains.Select(static value => value!).ToArray());
        var pairBatchSha256 = Sha256Framed(
            "xiigen.dpo-pair-batch.v1",
            canonicalPairIds);
        var domainBatchSha256 = Sha256Framed(
            "xiigen.dpo-domain-batch.v1",
            canonicalDomains);
        var transactionId = Sha256Framed(
            "xiigen.dpo-pair-transaction.v1",
            [pairBatchSha256, domainBatchSha256]);
        return OperationResult<DpoAdapterPublicationReceipt>.Ok(
            new DpoAdapterPublicationReceipt(
                transactionId,
                pairBatchSha256,
                canonicalPairIds,
                canonicalDomains));
    }

    public static bool IsCanonicalDpoPublicationReceipt(
        DpoAdapterPublicationReceipt? receipt)
    {
        if (receipt is null
            || receipt.OrderedPairIds is null
            || receipt.OrderedDomains is null)
        {
            return false;
        }

        var rebuilt = BuildDpoPublicationReceipt(
            receipt.OrderedPairIds,
            receipt.OrderedDomains);
        return rebuilt.IsSuccess
               && rebuilt.Value is not null
               && StringComparer.Ordinal.Equals(
                   rebuilt.Value.TransactionId,
                   receipt.TransactionId)
               && StringComparer.Ordinal.Equals(
                   rebuilt.Value.PairBatchSha256,
                   receipt.PairBatchSha256);
    }

    public static string Sha256(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
            throw new ArgumentException("Hash stream must be readable and seekable.", nameof(stream));
        var originalPosition = stream.Position;
        try
        {
            stream.Position = 0;
            return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }

    public static async ValueTask<string> Sha256Async(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
            throw new ArgumentException("Hash stream must be readable and seekable.", nameof(stream));
        var originalPosition = stream.Position;
        try
        {
            stream.Position = 0;
            var hash = await SHA256.HashDataAsync(stream, cancellationToken)
                .ConfigureAwait(false);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }

    internal static string RequireCanonicalRegularFilePath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!Path.IsPathFullyQualified(path))
            throw new InvalidDataException("Artifact path must be fully qualified.");

        var fullPath = Path.GetFullPath(path);
        var pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        if (!string.Equals(path, fullPath, pathComparison))
            throw new InvalidDataException("Artifact path must be canonical.");

        var attributes = File.GetAttributes(fullPath);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
            throw new InvalidDataException("Artifact must be a regular non-reparse file.");

        for (var directory = new FileInfo(fullPath).Directory;
             directory is not null;
             directory = directory.Parent)
        {
            if ((directory.Attributes & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException("Artifact path contains a reparse point.");
        }

        return fullPath;
    }

    private static string NormalizeOrigin(string origin)
    {
        var trimmed = origin.Trim();
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            && uri.Scheme is "http" or "https")
        {
            if (!string.IsNullOrEmpty(uri.UserInfo)
                || !string.IsNullOrEmpty(uri.Fragment))
                throw new ArgumentException("Training source URI must not contain user info or a fragment.", nameof(origin));

            var builder = new UriBuilder(uri)
            {
                Scheme = uri.Scheme.ToLowerInvariant(),
                Host = uri.IdnHost.ToLowerInvariant(),
                Fragment = string.Empty,
                Port = uri.IsDefaultPort ? -1 : uri.Port,
            };
            return builder.Uri.GetComponents(
                UriComponents.SchemeAndServer | UriComponents.PathAndQuery,
                UriFormat.UriEscaped);
        }

        var slashNormalized = trimmed.Replace('\\', '/');
        if (IsWindowsAbsolutePath(slashNormalized))
            return NormalizePathSegments(slashNormalized, caseInsensitive: true);
        if (slashNormalized.StartsWith("/", StringComparison.Ordinal))
            return NormalizePathSegments(slashNormalized, caseInsensitive: false);

        if (slashNormalized.Contains("//", StringComparison.Ordinal)
            || slashNormalized.Split('/').Any(segment => segment is "." or ".." or ""))
            throw new ArgumentException("Logical training source origin is ambiguous.", nameof(origin));
        return slashNormalized;
    }

    private static bool IsWindowsAbsolutePath(string path)
        => (path.Length >= 3
            && char.IsAsciiLetter(path[0])
            && path[1] == ':'
            && path[2] == '/')
           || path.StartsWith("//", StringComparison.Ordinal);

    private static string NormalizePathSegments(string path, bool caseInsensitive)
    {
        var isUnc = path.StartsWith("//", StringComparison.Ordinal);
        var prefix = isUnc
            ? "//"
            : path.Length >= 2 && path[1] == ':'
                ? path[..3]
                : "/";
        var start = prefix.Length;
        var segments = new List<string>();
        foreach (var segment in path[start..].Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment == ".")
                continue;
            if (segment == "..")
            {
                if (segments.Count == 0)
                    throw new ArgumentException("Training source path escapes its root.", nameof(path));
                segments.RemoveAt(segments.Count - 1);
                continue;
            }
            segments.Add(segment);
        }
        var normalized = string.Concat(prefix, string.Join('/', segments));
        return caseInsensitive ? normalized.ToLowerInvariant() : normalized;
    }

    private static string Sha256Utf8(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private static string Sha256Framed(
        string discriminator,
        IEnumerable<string> values)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendFramed(hash, discriminator);
        foreach (var value in values)
            AppendFramed(hash, value);
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static OperationResult<string?[]> SnapshotBounded(
        IEnumerable<string?> source,
        int maximumCount)
    {
        try
        {
            var snapshot = new List<string?>(Math.Min(maximumCount, 256));
            using var enumerator = source.GetEnumerator();
            while (snapshot.Count <= maximumCount && enumerator.MoveNext())
                snapshot.Add(enumerator.Current);
            if (snapshot.Count > maximumCount)
            {
                return OperationResult<string?[]>.Fail(
                    "DPO publication identity exceeds its bounded capacity.",
                    "DPO_PUBLICATION_IDENTITY_INVALID");
            }
            return OperationResult<string?[]>.Ok(snapshot.ToArray());
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<string?[]>.Fail(
                "DPO publication identity could not be snapshotted.",
                "DPO_PUBLICATION_IDENTITY_INVALID");
        }
    }

    private static void AppendFramed(IncrementalHash hash, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        Span<byte> length = stackalloc byte[sizeof(int)];
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(
            length,
            bytes.Length);
        hash.AppendData(length);
        hash.AppendData(bytes);
    }
}
