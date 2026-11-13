// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Security.Cryptography;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

/// <summary>
/// Holds a freshly opened checkpoint stream plus the manifest that was verified
/// before that stream was returned.
/// </summary>
public sealed class RepositoryOwnedModelCheckpointLoadResult : IDisposable
{
    internal RepositoryOwnedModelCheckpointLoadResult(
        RepositoryOwnedModelCheckpointManifest manifest,
        Stream weights,
        string weightsSha256,
        long weightsByteLength,
        IReadOnlyList<string> diagnostics)
    {
        Manifest = manifest;
        Weights = weights;
        WeightsSha256 = weightsSha256;
        WeightsByteLength = weightsByteLength;
        Diagnostics = diagnostics;
    }

    public RepositoryOwnedModelCheckpointManifest Manifest { get; }
    public Stream Weights { get; }
    public string WeightsSha256 { get; }
    public long WeightsByteLength { get; }
    public IReadOnlyList<string> Diagnostics { get; }

    public void Dispose()
        => Weights.Dispose();
}

/// <summary>
/// Finds the latest sealed, runtime-compatible common-model checkpoint inside
/// the canonical repository-owned model tree and returns a fresh weight stream.
/// </summary>
public sealed class RepositoryOwnedModelCheckpointLoader
{
    public const string InvalidRequestErrorCode =
        "REPOSITORY_OWNED_MODEL_CHECKPOINT_LOAD_REQUEST_INVALID";
    public const string NoCheckpointTreeErrorCode =
        "REPOSITORY_OWNED_MODEL_CHECKPOINT_TREE_MISSING";
    public const string NoSealedCandidateErrorCode =
        "REPOSITORY_OWNED_MODEL_CHECKPOINT_SEALED_CANDIDATE_MISSING";
    public const string IncompatibleCandidateErrorCode =
        "REPOSITORY_OWNED_MODEL_CHECKPOINT_COMPATIBLE_CANDIDATE_MISSING";
    public const string CorruptCandidateErrorCode =
        "REPOSITORY_OWNED_MODEL_CHECKPOINT_CANDIDATE_INVALID";
    public const string AmbiguousCandidateErrorCode =
        "REPOSITORY_OWNED_MODEL_CHECKPOINT_CANDIDATE_AMBIGUOUS";
    public const string UnavailableErrorCode =
        "REPOSITORY_OWNED_MODEL_CHECKPOINT_LOAD_UNAVAILABLE";
    public const string InitialControlCheckpointPresentErrorCode = "REPOSITORY_OWNED_MODEL_INITIAL_CONTROL_CHECKPOINT_PRESENT"; // Initial control refuses candidate presence without opening accepted payloads.

    private const int FileBufferSize = 81920;
    private const int HashHexLength = 64;
    private const int MaximumTextLength = 256;
    private const long MaximumWeightBytes = 16L * 1024L * 1024L * 1024L;

    internal enum ImmediateStepEntryDecision
    {
        Ignore,
        CanonicalStep,
        Unsafe,
    }

    internal readonly record struct ImmediateStepEntryClassification(
        ImmediateStepEntryDecision Decision,
        int? Step)
    {
        internal static ImmediateStepEntryClassification Ignore()
            => new(ImmediateStepEntryDecision.Ignore, null);

        internal static ImmediateStepEntryClassification CanonicalStep(int step)
            => new(ImmediateStepEntryDecision.CanonicalStep, step);

        internal static ImmediateStepEntryClassification Unsafe(int? step = null)
            => new(ImmediateStepEntryDecision.Unsafe, step);
    }

    /// <summary>
    /// Loads the greatest numeric compatible sealed checkpoint for one exact
    /// domain, design pattern, model id and version. Corrupt or unsafe numeric
    /// candidates fail the whole discovery; incompatible sealed candidates are
    /// reported and skipped so an older compatible checkpoint can still resume.
    /// </summary>
    public OperationResult<RepositoryOwnedModelCheckpointLoadResult>
        LoadLatestCompatible(
            string repositoryRoot,
            string domainSlug,
            string designPatternSlug,
            string modelIdSlug,
            string modelVersion,
            RepositoryOwnedModelRuntimeCompatibility expectedCompatibility,
            bool initialOnly = false) // Ordinary callers retain sealed checkpoint loading; initial control requests discovery-only absence.
    {
        if (!RuntimeCompatibilityIsWellFormed(expectedCompatibility))
            return Invalid<RepositoryOwnedModelCheckpointLoadResult>();

        var checkpointTree = ResolveCheckpointTree(
            repositoryRoot,
            domainSlug,
            designPatternSlug,
            modelIdSlug,
            modelVersion);
        if (!checkpointTree.IsSuccess || checkpointTree.Value is null)
            return checkpointTree.MapError<RepositoryOwnedModelCheckpointLoadResult>();

        if (initialOnly) // The initial-only route never enters custody preparation, manifest reads or weight import, including after discovery races.
        { // Reuse canonical addresses and unsafe-entry classification before allowing clean initialization.
            if (!Directory.Exists(checkpointTree.Value.CheckpointsRoot)) // Only genuine missing tree can become clean cold state.
                return Failure<RepositoryOwnedModelCheckpointLoadResult>("No repository-owned checkpoint tree exists for this exact model identity.", NoCheckpointTreeErrorCode); // No checkpoint payload has been opened.
            var initialDiscovery = DiscoverImmediateStepDirectories(checkpointTree.Value); // Inspect names and filesystem metadata, never accepted manifest/DPO/weights content.
            if (!initialDiscovery.IsSuccess || initialDiscovery.Value is null) return initialDiscovery.MapError<RepositoryOwnedModelCheckpointLoadResult>(); // Unsafe or ambiguous namespaces stay fail-closed.
            return initialDiscovery.Value.Count == 0 // Candidate absence retains the existing clean-missing classification.
                ? Failure<RepositoryOwnedModelCheckpointLoadResult>("The checkpoint tree exists, but it contains no sealed checkpoint step candidates.", NoSealedCandidateErrorCode) // Empty discovery permits the existing authorized cold branch only.
                : Failure<RepositoryOwnedModelCheckpointLoadResult>("Initial control refuses an existing checkpoint candidate before opening any accepted payload.", InitialControlCheckpointPresentErrorCode); // Presence is not a claim that unread bytes are valid or corrupt.
        } // Initial-only discovery cannot fall through to accepted checkpoint loading.

        // WHY: latest-compatible discovery is a disk-first route. After a
        // fresh Git/LFS clone, the tree may be byte-correct but too broad for
        // strict local custody; prepare that canonical subtree once before
        // choosing any checkpoint candidate.
        var custodyContext = RepositoryOwnedTrainingArtifactCloneCustodyPreparer
            .PrepareModelAndDpoContextForStrictReaders(
                checkpointTree.Value.ModelRoot.RepositoryRoot);
        if (!custodyContext.IsSuccess || custodyContext.Value is null)
        {
            return Failure<RepositoryOwnedModelCheckpointLoadResult>(
                "Repository-owned checkpoint clone custody preparation failed before latest-compatible discovery.",
                UnavailableErrorCode);
        }

        if (!Directory.Exists(checkpointTree.Value.CheckpointsRoot))
        {
            return Failure<RepositoryOwnedModelCheckpointLoadResult>(
                "No repository-owned checkpoint tree exists for this exact model identity.",
                NoCheckpointTreeErrorCode);
        }

        var discovered = DiscoverImmediateStepDirectories(
            checkpointTree.Value);
        if (!discovered.IsSuccess || discovered.Value is null)
            return discovered.MapError<RepositoryOwnedModelCheckpointLoadResult>();
        if (discovered.Value.Count == 0)
        {
            return Failure<RepositoryOwnedModelCheckpointLoadResult>(
                "The checkpoint tree exists, but it contains no sealed checkpoint step candidates.",
                NoSealedCandidateErrorCode);
        }

        var sealedCompatible = new List<OpenedCheckpointCandidate>();
        var incompatibleDiagnostics = new List<string>();
        var corruptDiagnostics = new List<string>();
        foreach (var candidate in discovered.Value.OrderBy(static item => item.Step))
        {
            var address = RepositoryOwnedTrainingArtifactLayout
                .CreateModelCheckpointAddress(
                    checkpointTree.Value.ModelRoot.RepositoryRoot,
                    domainSlug,
                    designPatternSlug,
                    modelIdSlug,
                    modelVersion,
                    candidate.Step);
            if (!address.IsSuccess || address.Value is null)
            {
                corruptDiagnostics.Add(
                    $"Rejected corrupt or invalid checkpoint step {candidate.StepSlug}: canonical address rebuild failed.");
                continue;
            }

            var opened = RepositoryOwnedModelCheckpointManifestSealer
                .OpenSealedWithPreparedCloneCustody(
                    address.Value,
                    custodyContext.Value);
            if (!opened.IsSuccess || opened.Value is null)
            {
                corruptDiagnostics.Add(
                    $"Rejected corrupt or invalid checkpoint step {candidate.StepSlug}: sealed manifest, parent lineage, DPO manifest bytes, or weight hash did not verify.");
                continue;
            }

            if (!CompatibilityMatches(
                    opened.Value.Manifest.RuntimeCompatibility,
                    expectedCompatibility))
            {
                incompatibleDiagnostics.Add(
                    $"Rejected sealed but incompatible checkpoint step {candidate.StepSlug}: runtime compatibility fingerprints or compatibility key differ.");
                continue;
            }

            sealedCompatible.Add(new OpenedCheckpointCandidate(
                candidate.Step,
                candidate.StepSlug,
                address.Value,
                opened.Value));
        }

        if (corruptDiagnostics.Count != 0)
        {
            return Failure<RepositoryOwnedModelCheckpointLoadResult>(
                "At least one corrupt, incomplete, tampered, or unsafe checkpoint candidate was found; discovery failed closed without loading older bytes. "
                + string.Join(" ", corruptDiagnostics),
                CorruptCandidateErrorCode);
        }

        if (sealedCompatible.Count == 0 && incompatibleDiagnostics.Count != 0)
        {
            return Failure<RepositoryOwnedModelCheckpointLoadResult>(
                "Sealed checkpoints exist for this model identity, but none match the expected runtime compatibility. "
                + string.Join(" ", incompatibleDiagnostics),
                IncompatibleCandidateErrorCode);
        }

        if (sealedCompatible.Count == 0)
        {
            return Failure<RepositoryOwnedModelCheckpointLoadResult>(
                "No sealed checkpoint candidate could be verified for this model identity.",
                NoSealedCandidateErrorCode);
        }

        var selected = sealedCompatible.MaxBy(static item => item.Step)!;
        var freshWeights = OpenFreshVerifiedWeights(
            selected.Address,
            selected.OpenResult);
        if (!freshWeights.IsSuccess || freshWeights.Value is null)
            return freshWeights.MapError<RepositoryOwnedModelCheckpointLoadResult>();

        var diagnostics = incompatibleDiagnostics
            .Append(
                $"Loaded compatible sealed checkpoint step {selected.StepSlug}: weights were freshly reopened and hashed after manifest verification.")
            .ToArray();

        return OperationResult<RepositoryOwnedModelCheckpointLoadResult>.Ok(
            new RepositoryOwnedModelCheckpointLoadResult(
                selected.OpenResult.Manifest,
                freshWeights.Value,
                selected.OpenResult.WeightsSha256,
                selected.OpenResult.WeightsByteLength,
                diagnostics));
    }

    private static OperationResult<CheckpointTreeAddress> ResolveCheckpointTree(
        string repositoryRoot,
        string domainSlug,
        string designPatternSlug,
        string modelIdSlug,
        string modelVersion)
    {
        var modelRoot = RepositoryOwnedTrainingArtifactLayout
            .ResolveModelArtifactRoot(repositoryRoot);
        if (!modelRoot.IsSuccess || modelRoot.Value is null)
            return modelRoot.MapError<CheckpointTreeAddress>();

        var probe = RepositoryOwnedTrainingArtifactLayout
            .CreateModelCheckpointAddress(
                modelRoot.Value,
                domainSlug,
                designPatternSlug,
                modelIdSlug,
                modelVersion,
                1);
        if (!probe.IsSuccess || probe.Value is null)
            return Invalid<CheckpointTreeAddress>();

        var checkpointsRoot = Path.GetDirectoryName(probe.Value.CheckpointRoot);
        if (string.IsNullOrWhiteSpace(checkpointsRoot)
            || !PathsEqual(
                checkpointsRoot,
                Path.GetFullPath(Path.Combine(
                    modelRoot.Value.ModelArtifactRoot,
                    domainSlug,
                    designPatternSlug,
                    modelIdSlug,
                    modelVersion,
                    RepositoryOwnedTrainingArtifactLayout
                        .CheckpointsDirectoryName))))
        {
            return Invalid<CheckpointTreeAddress>();
        }

        return OperationResult<CheckpointTreeAddress>.Ok(
            new CheckpointTreeAddress(modelRoot.Value, checkpointsRoot));
    }

    private static OperationResult<IReadOnlyList<CheckpointStepCandidate>>
        DiscoverImmediateStepDirectories(CheckpointTreeAddress checkpointTree)
    {
        try
        {
            if (!SecureLocalFileBoundary.IsSupportedPlatform()
                || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    checkpointTree.CheckpointsRoot))
            {
                return Unavailable<IReadOnlyList<CheckpointStepCandidate>>();
            }

            var byStep = new Dictionary<int, CheckpointStepCandidate>();
            var unsafeStepEntries = new List<string>();
            foreach (var entry in new DirectoryInfo(
                         checkpointTree.CheckpointsRoot)
                         .EnumerateFileSystemInfos(
                             "*",
                             SearchOption.TopDirectoryOnly))
            {
                var classification = ClassifyImmediateStepEntry(
                    entry.Name,
                    entry.Attributes);
                if (classification.Decision
                    == ImmediateStepEntryDecision.Unsafe)
                {
                    unsafeStepEntries.Add(entry.Name);
                    continue;
                }

                if (classification.Decision
                    != ImmediateStepEntryDecision.CanonicalStep)
                {
                    continue;
                }

                var step = classification.Step.GetValueOrDefault();
                if (byStep.ContainsKey(step))
                {
                    unsafeStepEntries.Add(entry.Name);
                    continue;
                }

                byStep.Add(
                    step,
                    new CheckpointStepCandidate(step, entry.Name));
            }

            if (unsafeStepEntries.Count != 0)
            {
                return Failure<IReadOnlyList<CheckpointStepCandidate>>(
                    "The checkpoint tree contains an unsafe or duplicate numeric step representation, or an immediate reparse entry, so discovery failed closed: "
                    + string.Join(", ", unsafeStepEntries.OrderBy(static name => name, StringComparer.Ordinal)),
                    AmbiguousCandidateErrorCode);
            }

            return OperationResult<IReadOnlyList<CheckpointStepCandidate>>.Ok(
                byStep.Values
                    .OrderBy(static item => item.Step)
                    .ToArray());
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<IReadOnlyList<CheckpointStepCandidate>>();
        }
    }

    internal static ImmediateStepEntryClassification ClassifyImmediateStepEntry(
        string name,
        FileAttributes attributes)
    {
        var hasCanonicalStepSlug = RepositoryOwnedTrainingArtifactLayout
            .TryParseModelCheckpointStepSlug(name, out var step);
        if ((attributes & FileAttributes.ReparsePoint) != 0)
            return ImmediateStepEntryClassification.Unsafe(
                hasCanonicalStepSlug ? step : null);

        if (hasCanonicalStepSlug)
        {
            if ((attributes & FileAttributes.Directory) == 0
                || HasUnsafeStepAttributes(attributes))
            {
                return ImmediateStepEntryClassification.Unsafe(step);
            }

            return ImmediateStepEntryClassification.CanonicalStep(step);
        }

        if (LooksLikeNumericStepName(name))
            return ImmediateStepEntryClassification.Unsafe();

        if (ShouldIgnoreEntry(name, attributes))
            return ImmediateStepEntryClassification.Ignore();

        return ImmediateStepEntryClassification.Ignore();
    }

    private static OperationResult<Stream> OpenFreshVerifiedWeights(
        RepositoryOwnedModelCheckpointAddress address,
        RepositoryOwnedModelCheckpointOpenResult opened)
    {
        FileStream? stream = null;
        try
        {
            if (!SecureLocalFileBoundary.IsSupportedPlatform()
                || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    address.WeightsPath)
                || !File.Exists(address.WeightsPath))
            {
                return InvalidStream(stream);
            }

            stream = new FileStream(
                address.WeightsPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                FileBufferSize,
                FileOptions.SequentialScan);
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    stream.SafeFileHandle,
                    address.WeightsPath))
            {
                return InvalidStream(stream);
            }

            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            var buffer = new byte[FileBufferSize];
            long total = 0;
            while (true)
            {
                var read = stream.Read(buffer, 0, buffer.Length);
                if (read == 0)
                    break;

                if (read > MaximumWeightBytes - total)
                    return InvalidStream(stream);

                total += read;
                hash.AppendData(buffer.AsSpan(0, read));
            }

            var sha256 = Convert.ToHexString(hash.GetHashAndReset())
                .ToLowerInvariant();
            if (total <= 0
                || total != opened.WeightsByteLength
                || total != opened.Manifest.WeightsByteLength
                || !StringComparer.Ordinal.Equals(sha256, opened.WeightsSha256)
                || !StringComparer.Ordinal.Equals(
                    sha256,
                    opened.Manifest.WeightsSha256)
                || !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    stream.SafeFileHandle,
                    address.WeightsPath)
                || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    address.WeightsPath)
                || !stream.CanSeek)
            {
                return InvalidStream(stream);
            }

            stream.Position = 0;
            var owned = stream;
            stream = null;
            return OperationResult<Stream>.Ok(owned);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return UnavailableStream(stream);
        }
    }

    private static bool CompatibilityMatches(
        RepositoryOwnedModelRuntimeCompatibility actual,
        RepositoryOwnedModelRuntimeCompatibility expected)
        => StringComparer.Ordinal.Equals(actual.ConfigSha256, expected.ConfigSha256)
           && StringComparer.Ordinal.Equals(
               actual.TopologySha256,
               expected.TopologySha256)
           && StringComparer.Ordinal.Equals(
               actual.TokenizerSha256,
               expected.TokenizerSha256)
           && StringComparer.Ordinal.Equals(
               actual.TrainerSha256,
               expected.TrainerSha256)
           && StringComparer.Ordinal.Equals(actual.SchemaSha256, expected.SchemaSha256)
           && StringComparer.Ordinal.Equals(
               actual.CompatibilityKey,
               expected.CompatibilityKey);

    private static bool RuntimeCompatibilityIsWellFormed(
        RepositoryOwnedModelRuntimeCompatibility? compatibility)
        => compatibility is not null
           && IsCanonicalSha256Hex(compatibility.ConfigSha256)
           && IsCanonicalSha256Hex(compatibility.TopologySha256)
           && IsCanonicalSha256Hex(compatibility.TokenizerSha256)
           && IsCanonicalSha256Hex(compatibility.TrainerSha256)
           && IsCanonicalSha256Hex(compatibility.SchemaSha256)
           && IsSafeToken(compatibility.CompatibilityKey);

    private static bool ShouldIgnoreEntry(
        string name,
        FileAttributes attributes)
        => name.StartsWith(".", StringComparison.Ordinal)
           || name.EndsWith(".tmp", StringComparison.Ordinal)
           || (attributes & FileAttributes.Hidden) != 0
           || (attributes & FileAttributes.System) != 0
           || (attributes & FileAttributes.Temporary) != 0;

    private static bool HasUnsafeStepAttributes(FileAttributes attributes)
        => (attributes & FileAttributes.Hidden) != 0
           || (attributes & FileAttributes.System) != 0
           || (attributes & FileAttributes.Temporary) != 0;

    private static bool LooksLikeNumericStepName(string value)
        => value.Length != 0
           && value.All(static character => character is >= '0' and <= '9');

    private static bool IsCanonicalSha256Hex(string? value)
        => value is { Length: HashHexLength }
           && value.All(static character => character is >= '0' and <= '9'
               or >= 'a' and <= 'f');

    private static bool IsSafeToken(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && value.Length <= MaximumTextLength
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && value.All(static character =>
               character is >= 'a' and <= 'z'
               || character is >= '0' and <= '9'
               || character is '-' or '_' or '.');

    private static bool PathsEqual(string first, string second)
        => string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(first)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(second)),
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);

    private static OperationResult<T> Invalid<T>()
        => Failure<T>(
            "Repository-owned model checkpoint load request is invalid.",
            InvalidRequestErrorCode);

    private static OperationResult<T> Failure<T>(string message, string code)
        => OperationResult<T>.Fail(message, code);

    private static OperationResult<T> Unavailable<T>()
        => Failure<T>(
            "Repository-owned model checkpoint load storage is unavailable.",
            UnavailableErrorCode);

    private static OperationResult<Stream> InvalidStream(Stream? stream)
    {
        stream?.Dispose();
        return Failure<Stream>(
            "Selected repository-owned checkpoint weights failed fresh read verification.",
            CorruptCandidateErrorCode);
    }

    private static OperationResult<Stream> UnavailableStream(Stream? stream)
    {
        stream?.Dispose();
        return Unavailable<Stream>();
    }

    private sealed record CheckpointTreeAddress(
        RepositoryOwnedModelArtifactRoot ModelRoot,
        string CheckpointsRoot);

    private sealed record CheckpointStepCandidate(int Step, string StepSlug);

    private sealed record OpenedCheckpointCandidate(
        int Step,
        string StepSlug,
        RepositoryOwnedModelCheckpointAddress Address,
        RepositoryOwnedModelCheckpointOpenResult OpenResult);
}
