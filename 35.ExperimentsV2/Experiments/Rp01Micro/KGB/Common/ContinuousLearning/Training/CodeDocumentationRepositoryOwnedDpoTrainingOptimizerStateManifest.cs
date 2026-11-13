// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DPO;
using TransformerModel;
using TransformerModel.Config;
using TrainingLoop.Optimizers;

public sealed record CodeDocumentationRepositoryOwnedDpoTrainingModelShapeIdentity(
    int VocabSize,
    int ContextLen,
    int HiddenDim,
    int NumLayers,
    int NumHeads,
    int NumKvHeads,
    int FfnHiddenDim,
    float RopeTheta,
    float RmsNormEps)
{
    public static CodeDocumentationRepositoryOwnedDpoTrainingModelShapeIdentity
        From(ModelConfig config)
        => new(
            config.VocabSize,
            config.ContextLen,
            config.HiddenDim,
            config.NumLayers,
            config.NumHeads,
            config.NumKvHeads,
            config.FfnHiddenDim,
            config.RopeTheta,
            config.RmsNormEps);

    public bool Matches(ModelConfig config)
        => Equals(From(config));
}

public sealed record CodeDocumentationRepositoryOwnedDpoTrainingOptimizerParameterIdentity(
    string Key,
    int ElementCount);

/// <summary>
/// Final owner commit for the optimizer state belonging to one
/// CodeDocumentation DPO attempt. The file is deliberately separate from
/// baseline.json: baseline.json is immutable pre-training evidence, while this
/// record is prepared and read back before the final policy move. The final
/// metrics name remains the commit marker, and the owning executor deletes this
/// record together with the other attempt artifacts on any failed publication.
/// </summary>
public sealed record CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest(
    int FormatVersion,
    string TrainingRunId,
    string TrainingRunIdSha256,
    string AttemptRootRelativePath,
    string AttemptClaimRelativePath,
    string DomainSlug,
    string DesignPatternSlug,
    string ModelIdSlug,
    string ModelVersion,
    int ParentCheckpointStep,
    string ParentCheckpointManifestRelativePath,
    string ParentCheckpointManifestSha256,
    string ParentCheckpointWeightsRelativePath,
    string ParentCheckpointWeightsSha256,
    long ParentCheckpointWeightsByteLength,
    string DpoManifestRelativePath,
    string DpoManifestSha256,
    string TrainingPairId,
    string TrainingPairRelativePath,
    string TrainingPairSha256,
    int DpoStepCount,
    int OptimizerStateStep,
    string OptimizerStateRelativePath,
    string OptimizerStateSha256,
    long OptimizerStateByteLength,
    int OptimizerStateFormatVersion,
    CodeDocumentationRepositoryOwnedDpoTrainingModelShapeIdentity? ModelShape,
    IReadOnlyList<CodeDocumentationRepositoryOwnedDpoTrainingOptimizerParameterIdentity>?
        OptimizerParameterMap,
    DpoTrainingOptimizerSettings? OptimizerSettings,
    bool RandomStateRequired = false,
    int RandomStateStep = 0,
    string RandomStateRelativePath = "",
    string RandomStateSha256 = "",
    long RandomStateByteLength = 0,
    int RandomStateFormatVersion = 0,
    string RandomStateTopologySha256 = "",
    string LoraConfigurationSha256 = "",
    string TrialWeightsRelativePath = "",
    string TrialWeightsSha256 = "",
    long TrialWeightsByteLength = 0,
    string MaterialVersion = "",
    string FramingVersion = "",
    string ObjectiveVersion = "",
    IReadOnlyList<DpoAllowedParameterIdentity>? AllowedParameterMap = null);

public sealed record CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback(
    string ManifestPath,
    string ManifestRelativePath,
    string ManifestSha256,
    long ManifestByteLength,
    CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest Manifest);

/// <summary>
/// Writes and reopens the final optimizer-state manifest using the same
/// owner-only, bounded, create-new discipline as the other repository-owned
/// attempt journals. Missing or incomplete manifests are not recoverable.
/// </summary>
public static class CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
{
    public const int CurrentFormatVersion = 5;
    public const int LegacyFormatVersion = 2;
    public const string ManifestFileName = "dpo_optimizer_state_manifest.json";
    public const string InvalidErrorCode =
        "CODE_DOCUMENTATION_DPO_OPTIMIZER_STATE_MANIFEST_INVALID";
    public const string UnavailableErrorCode =
        "CODE_DOCUMENTATION_DPO_OPTIMIZER_STATE_MANIFEST_UNAVAILABLE";

    private const long MaximumManifestBytes = 2L * 1024L * 1024L;
    private const int HashHexLength = 64;
    private const int MaximumIdentityLength = 256;
    private const int MaximumRelativePathLength = 1024;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    internal static OperationResult<
            CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback>
        WriteAndReadBack(
            RepositoryOwnedModelTrainingAttemptAddress? address,
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState? baseline,
            DpoTrainingExecutionArtifact? artifact,
            ModelConfig? modelConfig,
            DpoTrainingOptimizerSettings? optimizerSettings,
            CancellationToken cancellationToken = default)
    {
        var manifest = BuildManifest(
            address,
            baseline,
            artifact,
            modelConfig,
            optimizerSettings);
        if (!manifest.IsSuccess || manifest.Value is null)
        {
            return manifest.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback>();
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = BuildManifestPath(address!);
            if (!path.IsSuccess || path.Value is null)
            {
                return path.MapError<
                    CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback>();
            }

            var bytes = JsonSerializer.SerializeToUtf8Bytes(
                manifest.Value,
                JsonOptions);
            if (bytes.Length <= 0 || bytes.LongLength > MaximumManifestBytes)
                return Invalid<
                    CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback>();

            if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    path.Value)
                || Directory.Exists(path.Value)
                || File.Exists(path.Value))
            {
                return Unavailable<
                    CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback>();
            }

            using (var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(
                       path.Value,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       FileOptions.WriteThrough))
            {
                if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                        stream.SafeFileHandle,
                        path.Value)
                    || !SecureLocalFileBoundary.HasOwnerOnlyFileMode(path.Value))
                {
                    return Unavailable<
                        CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback>();
                }

                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(flushToDisk: true);
            }

            cancellationToken.ThrowIfCancellationRequested();
            var readback = ReadExisting(path.Value);
            if (!readback.IsSuccess || readback.Value is null)
            {
                return readback.MapError<
                    CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback>();
            }

            if (!CryptographicOperations.FixedTimeEquals(
                    bytes,
                    readback.Value.Bytes))
            {
                return Invalid<
                    CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback>();
            }

            var restored = JsonSerializer.Deserialize<
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest>(
                readback.Value.Bytes,
                JsonOptions);
            var validation = ValidateManifest(address, restored);
            if (!validation.IsSuccess || restored is null)
            {
                return validation.MapError<
                    CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback>();
            }

            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback>.Ok(
                new CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback(
                    path.Value,
                    address!.AttemptRootRelativePath + "/" + ManifestFileName,
                    readback.Value.Sha256,
                    readback.Value.Bytes.LongLength,
                    restored));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (JsonException)
        {
            return Invalid<
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback>();
        }
        catch (IOException)
        {
            return Unavailable<
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback>();
        }
    }

    public static OperationResult<
            CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback>
        OpenExisting(
            RepositoryOwnedModelTrainingAttemptAddress? address,
            CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (address is null ||
                !RepositoryOwnedTrainingArtifactLayout
                    .IsValidModelTrainingAttemptAddress(address))
            {
                return Invalid<
                    CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback>();
            }

            var path = BuildManifestPath(address);
            if (!path.IsSuccess || path.Value is null)
            {
                return path.MapError<
                    CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback>();
            }

            var readback = ReadExisting(path.Value);
            if (!readback.IsSuccess || readback.Value is null)
            {
                return readback.MapError<
                    CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback>();
            }

            var restored = JsonSerializer.Deserialize<
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest>(
                readback.Value.Bytes,
                JsonOptions);
            var validation = ValidateManifest(address, restored);
            if (!validation.IsSuccess || restored is null)
            {
                return validation.MapError<
                    CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback>();
            }

            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback>.Ok(
                new CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback(
                    path.Value,
                    address.AttemptRootRelativePath + "/" + ManifestFileName,
                    readback.Value.Sha256,
                    readback.Value.Bytes.LongLength,
                    restored));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (JsonException)
        {
            return Invalid<
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback>();
        }
    }

    /// <summary>Only canonical disk readback can issue an exact continuation carrier.</summary>
    public static OperationResult<DpoContinuationProof> IssueContinuationProof(
        RepositoryOwnedModelTrainingAttemptAddress address, AdamWPortableState state,
        LlmModelRandomStateSnapshot? randomState, CancellationToken cancellationToken = default)
    {
        var source = OpenExisting(address, cancellationToken);
        if (!source.IsSuccess || source.Value is null) return source.MapError<DpoContinuationProof>();
        var manifest = source.Value.Manifest;
        var baseline = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.OpenExisting(address, cancellationToken);
        if (!baseline.IsSuccess || baseline.Value is null || !MatchesBaseline(manifest, address, baseline.Value.State).IsSuccess)
            return ProfileMismatch<DpoContinuationProof>();
        try
        {
            var profile = DpoExecutionProfile.FromVerifiedManifest(manifest.MaterialVersion, manifest.FramingVersion,
                manifest.ObjectiveVersion, manifest.AllowedParameterMap!);
            if (state.Step != manifest.OptimizerStateStep || state.FormatVersion != manifest.OptimizerStateFormatVersion ||
                !state.Entries.OrderBy(x => x.Key, StringComparer.Ordinal)
                    .Select(x => new CodeDocumentationRepositoryOwnedDpoTrainingOptimizerParameterIdentity(x.Key, x.FirstMoment.Count))
                    .SequenceEqual(manifest.OptimizerParameterMap.OrderBy(x => x.Key, StringComparer.Ordinal)))
                return ProfileMismatch<DpoContinuationProof>();
            var expectedManifestHash = source.Value.ManifestSha256;
            var expectedBaselineBytes = JsonSerializer.SerializeToUtf8Bytes(baseline.Value.State);
            bool SourceStillMatches()
            {
                var current = OpenExisting(address);
                if (!current.IsSuccess || current.Value is null || current.Value.ManifestSha256 != expectedManifestHash) return false;
                var currentBaseline = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.OpenExisting(address);
                if (!currentBaseline.IsSuccess || currentBaseline.Value is null ||
                    !MatchesBaseline(current.Value.Manifest, address, currentBaseline.Value.State).IsSuccess ||
                    !CryptographicOperations.FixedTimeEquals(expectedBaselineBytes, JsonSerializer.SerializeToUtf8Bytes(currentBaseline.Value.State))) return false;
                var optimizer = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(address.AttemptRoot,
                    manifest.OptimizerStateStep, manifest.OptimizerStateSha256);
                if (!optimizer.IsSuccess || optimizer.Value is null || optimizer.Value.OptimizerStateByteLength != manifest.OptimizerStateByteLength ||
                    !CryptographicOperations.FixedTimeEquals(optimizer.Value.State.ToBytes(), state.ToBytes())) return false;
                if (!manifest.RandomStateRequired) return randomState is null;
                var random = RepositoryOwnedDpoRandomStateFile.OpenAndValidate(Path.Combine(address.AttemptRoot,
                    RepositoryOwnedDpoRandomStateFile.BuildFileName(manifest.RandomStateStep)), manifest.RandomStateSha256,
                    manifest.RandomStateByteLength, manifest.RandomStateStep, manifest.RandomStateTopologySha256, manifest.LoraConfigurationSha256);
                return random.IsSuccess && random.Value is not null && randomState is not null &&
                    CryptographicOperations.FixedTimeEquals(JsonSerializer.SerializeToUtf8Bytes(random.Value), JsonSerializer.SerializeToUtf8Bytes(randomState));
            }
            return OperationResult<DpoContinuationProof>.Ok(DpoContinuationProof.IssueVerified(profile, state, randomState,
                address.RepositoryRoot + "|" + source.Value.ManifestRelativePath + "|" + expectedManifestHash, SourceStillMatches));
        }
        catch (DpoMutationAdmissionException) { return ProfileMismatch<DpoContinuationProof>(); }
    }

    internal static OperationResult<bool> DeleteIfOwned(
        RepositoryOwnedModelTrainingAttemptAddress? address)
    {
        var path = BuildManifestPath(address!);
        if (!path.IsSuccess || path.Value is null)
            return path.MapError<bool>();

        try
        {
            if (!File.Exists(path.Value))
                return OperationResult<bool>.Ok(false);
            if (Directory.Exists(path.Value) ||
                SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    path.Value) ||
                !SecureLocalFileBoundary.HasOwnerOnlyFileMode(path.Value))
            {
                return Unavailable<bool>();
            }

            File.Delete(path.Value);
            return OperationResult<bool>.Ok(!File.Exists(path.Value));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<bool>();
        }
    }

    public static OperationResult<bool> MatchesBaseline(
        CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest? manifest,
        RepositoryOwnedModelTrainingAttemptAddress? address,
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState? baseline)
    {
        var validation = ValidateManifest(address, manifest);
        if (!validation.IsSuccess || manifest is null || baseline is null)
            return validation.MapError<bool>();

        var passport = baseline.AcceptedModelPassport;
        var matches =
            StringComparer.Ordinal.Equals(
                manifest.TrainingRunId,
                baseline.TrainingRunId) &&
            StringComparer.Ordinal.Equals(
                manifest.TrainingRunIdSha256,
                baseline.TrainingRunIdSha256) &&
            StringComparer.Ordinal.Equals(
                manifest.AttemptRootRelativePath,
                baseline.AttemptRootRelativePath) &&
            StringComparer.Ordinal.Equals(
                manifest.AttemptClaimRelativePath,
                baseline.AttemptClaimRelativePath) &&
            StringComparer.Ordinal.Equals(
                manifest.DomainSlug,
                baseline.DomainSlug) &&
            StringComparer.Ordinal.Equals(
                manifest.DesignPatternSlug,
                baseline.DesignPatternSlug) &&
            StringComparer.Ordinal.Equals(
                manifest.ModelIdSlug,
                baseline.ModelIdSlug) &&
            StringComparer.Ordinal.Equals(
                manifest.ModelVersion,
                baseline.ModelVersion) &&
            passport is not null &&
            manifest.ParentCheckpointStep == passport.CheckpointStep &&
            StringComparer.Ordinal.Equals(
                manifest.ParentCheckpointManifestRelativePath,
                passport.RepositoryRelativeManifestPath) &&
            StringComparer.Ordinal.Equals(
                manifest.ParentCheckpointManifestSha256,
                passport.ManifestSha256) &&
            StringComparer.Ordinal.Equals(
                manifest.ParentCheckpointWeightsRelativePath,
                passport.RepositoryRelativeWeightsPath) &&
            StringComparer.Ordinal.Equals(
                manifest.ParentCheckpointWeightsSha256,
                passport.WeightsSha256) &&
            manifest.ParentCheckpointWeightsByteLength == passport.WeightsByteLength &&
            StringComparer.Ordinal.Equals(
                manifest.DpoManifestRelativePath,
                baseline.DpoManifestRelativePath) &&
            StringComparer.Ordinal.Equals(
                manifest.DpoManifestSha256,
                baseline.DpoManifestSha256) &&
            StringComparer.Ordinal.Equals(
                manifest.TrainingPairId,
                baseline.TrainingPairId) &&
            StringComparer.Ordinal.Equals(
                manifest.TrainingPairRelativePath,
                baseline.TrainingPairRelativePath) &&
            StringComparer.Ordinal.Equals(
                manifest.TrainingPairSha256,
                baseline.TrainingPairSha256);

        return matches ? OperationResult<bool>.Ok(true) : Invalid<bool>();
    }

    private static OperationResult<
            CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest>
        BuildManifest(
            RepositoryOwnedModelTrainingAttemptAddress? address,
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState? baseline,
            DpoTrainingExecutionArtifact? artifact,
            ModelConfig? modelConfig,
            DpoTrainingOptimizerSettings? optimizerSettings)
    {
        if (address is null || baseline is null || artifact is null ||
            !RepositoryOwnedTrainingArtifactLayout
                .IsValidModelTrainingAttemptAddress(address) ||
            !StringComparer.Ordinal.Equals(
                baseline.TrainingRunId,
                address.TrainingRunId) ||
            !StringComparer.Ordinal.Equals(
                baseline.TrainingRunIdSha256,
                address.TrainingRunIdSha256) ||
            !StringComparer.Ordinal.Equals(
                baseline.AttemptRootRelativePath,
                address.AttemptRootRelativePath) ||
            !StringComparer.Ordinal.Equals(
                baseline.AttemptClaimRelativePath,
                address.AttemptClaimRelativePath))
        {
            return Invalid<
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest>();
        }

        var passport = baseline.AcceptedModelPassport;
        var optimizerState = artifact.PostStepOptimizerState;
        var optimizerIdentity = artifact.PostStepOptimizerStateIdentity;
        if (passport is null || optimizerState is null || optimizerIdentity is null ||
            modelConfig is null || optimizerSettings is null ||
            !optimizerSettings.IsWellFormed ||
            artifact.DpoStepCount <= 0 ||
            artifact.OptimizerStateStep <= 0 ||
            artifact.OptimizerStateStep != optimizerState.Step ||
            artifact.OptimizerStateFormatVersion != optimizerState.FormatVersion ||
            artifact.OptimizerStateByteLength <= 0 ||
            string.IsNullOrWhiteSpace(artifact.OptimizerStateSha256) ||
            !StringComparer.Ordinal.Equals(
                artifact.OptimizerStateSha256,
                optimizerIdentity.Sha256) ||
            artifact.OptimizerStateByteLength != optimizerIdentity.ByteLength ||
            artifact.OptimizerStateStep != optimizerIdentity.Step ||
                artifact.OptimizerStateFormatVersion != optimizerIdentity.FormatVersion ||
            !IsSha256(artifact.OptimizerStateSha256) ||
            artifact.TrialWeightsByteLength <= 0 ||
            !IsSha256(artifact.TrialWeightsSha256) ||
            artifact.OptimizerSettings is null ||
            artifact.OptimizerSettings != optimizerSettings)
        {
            return Invalid<
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest>();
        }

        var expectedOptimizerPath = Path.GetFullPath(Path.Combine(
            address.AttemptRoot,
            RepositoryOwnedDpoOptimizerStateFile.BuildFileName(
                artifact.OptimizerStateStep)));
        var optimizerPath = Path.GetFullPath(artifact.OptimizerStatePath);
        if (!StringComparerForPaths.Equals(expectedOptimizerPath, optimizerPath))
        {
            return Invalid<
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest>();
        }

        var expectedTrialWeightsPath = Path.GetFullPath(Path.Combine(
            address.AttemptRoot,
            DpoTrainer.BuildPolicyCheckpointFileName(artifact.DpoStepCount)));
        if (!StringComparerForPaths.Equals(
                expectedTrialWeightsPath,
                Path.GetFullPath(artifact.TrialWeightsPath)))
        {
            return Invalid<
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest>();
        }

        var parameterMap = optimizerState.Entries
            .OrderBy(static entry => entry.Key, StringComparer.Ordinal)
            .Select(static entry =>
                new CodeDocumentationRepositoryOwnedDpoTrainingOptimizerParameterIdentity(
                    entry.Key,
                    entry.FirstMoment.Count))
            .ToArray();
        if (artifact.ExecutionProfile is null ||
            parameterMap.Any(static entry =>
                string.IsNullOrWhiteSpace(entry.Key) || entry.ElementCount <= 0))
        {
            return Invalid<
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest>();
        }

        return OperationResult<
            CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest>.Ok(
            new CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest(
                CurrentFormatVersion,
                address.TrainingRunId,
                address.TrainingRunIdSha256,
                address.AttemptRootRelativePath,
                address.AttemptClaimRelativePath,
                address.DomainSlug,
                address.DesignPatternSlug,
                address.ModelIdSlug,
                address.ModelVersion,
                passport.CheckpointStep,
                passport.RepositoryRelativeManifestPath,
                passport.ManifestSha256,
                passport.RepositoryRelativeWeightsPath,
                passport.WeightsSha256,
                passport.WeightsByteLength,
                baseline.DpoManifestRelativePath,
                baseline.DpoManifestSha256,
                baseline.TrainingPairId,
                baseline.TrainingPairRelativePath,
                baseline.TrainingPairSha256,
                artifact.DpoStepCount,
                artifact.OptimizerStateStep,
                address.AttemptRootRelativePath + "/" +
                RepositoryOwnedDpoOptimizerStateFile.BuildFileName(
                    artifact.OptimizerStateStep),
                artifact.OptimizerStateSha256,
                artifact.OptimizerStateByteLength,
                artifact.OptimizerStateFormatVersion,
                CodeDocumentationRepositoryOwnedDpoTrainingModelShapeIdentity
                    .From(modelConfig),
                parameterMap,
                optimizerSettings,
                artifact.RandomStateRequired,
                artifact.RandomStateStep,
                string.IsNullOrWhiteSpace(artifact.RandomStatePath)
                    ? ""
                    : address.AttemptRootRelativePath + "/" +
                      Path.GetFileName(artifact.RandomStatePath),
                artifact.RandomStateSha256,
                artifact.RandomStateByteLength,
                artifact.RandomStateFormatVersion,
                artifact.RandomStateTopologySha256,
                artifact.RandomStateLoraConfigurationSha256,
                address.AttemptRootRelativePath + "/" +
                    DpoTrainer.BuildPolicyCheckpointFileName(
                        artifact.DpoStepCount),
                artifact.TrialWeightsSha256,
                artifact.TrialWeightsByteLength,
                artifact.ExecutionProfile.MaterialVersion,
                artifact.ExecutionProfile.FramingVersion,
                artifact.ExecutionProfile.ObjectiveVersion,
                artifact.ExecutionProfile.AllowedParameterMap.ToArray()));
    }

    private static OperationResult<bool> ValidateManifest(
        RepositoryOwnedModelTrainingAttemptAddress? address,
        CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest? manifest)
    {
        if (manifest is not null && (manifest.FormatVersion != CurrentFormatVersion ||
            manifest.MaterialVersion != DPO.Models.DpoExactPairMaterial.Version ||
            manifest.FramingVersion != DPO.Models.DpoExactPairMaterial.FramingVersion ||
            manifest.ObjectiveVersion != DPO.Models.DpoExactPairMaterial.ObjectiveVersion ||
            manifest.AllowedParameterMap is null || manifest.AllowedParameterMap.Count == 0 ||
            manifest.AllowedParameterMap.Any(x => x is null || string.IsNullOrWhiteSpace(x.Key) || x.ElementCount <= 0) ||
            manifest.AllowedParameterMap.Select(x => x.Key).Distinct(StringComparer.Ordinal).Count() != manifest.AllowedParameterMap.Count))
            return ProfileMismatch<bool>();
        if (address is null || manifest is null ||
            !RepositoryOwnedTrainingArtifactLayout
                .IsValidModelTrainingAttemptAddress(address) ||
            manifest.FormatVersion != CurrentFormatVersion ||
            !StringComparer.Ordinal.Equals(
                manifest.TrainingRunId,
                address.TrainingRunId) ||
            !StringComparer.Ordinal.Equals(
                manifest.TrainingRunIdSha256,
                address.TrainingRunIdSha256) ||
            !StringComparer.Ordinal.Equals(
                manifest.AttemptRootRelativePath,
                address.AttemptRootRelativePath) ||
            !StringComparer.Ordinal.Equals(
                manifest.AttemptClaimRelativePath,
                address.AttemptClaimRelativePath) ||
            !StringComparer.Ordinal.Equals(
                manifest.DomainSlug,
                address.DomainSlug) ||
            !StringComparer.Ordinal.Equals(
                manifest.DesignPatternSlug,
                address.DesignPatternSlug) ||
            !StringComparer.Ordinal.Equals(
                manifest.ModelIdSlug,
                address.ModelIdSlug) ||
            !StringComparer.Ordinal.Equals(
                manifest.ModelVersion,
                address.ModelVersion) ||
            !IsValidModelShape(manifest.ModelShape) ||
            manifest.OptimizerSettings is null ||
            !manifest.OptimizerSettings.IsWellFormed ||
            manifest.OptimizerParameterMap is null ||
            manifest.OptimizerParameterMap.Any(static entry =>
                entry is null ||
                string.IsNullOrWhiteSpace(entry.Key) ||
                entry.Key.Length > MaximumIdentityLength ||
                entry.Key.Any(char.IsControl) ||
                entry.ElementCount <= 0) ||
            manifest.OptimizerParameterMap
                .Select(static entry => entry.Key)
                .Distinct(StringComparer.Ordinal)
                .Count() != manifest.OptimizerParameterMap.Count ||
            manifest.ParentCheckpointStep <= 0 ||
            manifest.DpoStepCount <= 0 ||
            manifest.OptimizerStateStep <= 0 ||
            manifest.OptimizerStateFormatVersion !=
                TrainingLoop.Optimizers.AdamWPortableState.CurrentFormatVersion ||
            manifest.OptimizerStateByteLength <= 0 ||
            !IsSha256(manifest.TrainingRunIdSha256) ||
            !IsSha256(manifest.ParentCheckpointManifestSha256) ||
            !IsSha256(manifest.ParentCheckpointWeightsSha256) ||
            !IsSha256(manifest.DpoManifestSha256) ||
            !IsSha256(manifest.TrainingPairId) ||
            !IsSha256(manifest.TrainingPairSha256) ||
            !IsSha256(manifest.OptimizerStateSha256) ||
            !IsSafeRepositoryRelativePath(
                manifest.ParentCheckpointManifestRelativePath) ||
            !IsSafeRepositoryRelativePath(
                manifest.ParentCheckpointWeightsRelativePath) ||
            !IsSafeRepositoryRelativePath(manifest.DpoManifestRelativePath) ||
            !IsSafeRepositoryRelativePath(manifest.TrainingPairRelativePath) ||
            !IsSafeRepositoryRelativePath(manifest.OptimizerStateRelativePath) ||
            manifest.OptimizerStateRelativePath !=
                manifest.AttemptRootRelativePath + "/" +
                RepositoryOwnedDpoOptimizerStateFile.BuildFileName(
                    manifest.OptimizerStateStep))
        {
            return Invalid<bool>();
        }

        if (!ValidateRandomManifestFields(manifest))
            return Invalid<bool>();

        if (manifest.OptimizerParameterMap!.Any(entry => !manifest.AllowedParameterMap!.Any(allowed =>
            allowed.Key == entry.Key && allowed.ElementCount == entry.ElementCount))) return ProfileMismatch<bool>();

        if (manifest.FormatVersion == CurrentFormatVersion &&
            (manifest.TrialWeightsByteLength <= 0 ||
             !IsSha256(manifest.TrialWeightsSha256) ||
             !IsSafeRepositoryRelativePath(manifest.TrialWeightsRelativePath) ||
             manifest.TrialWeightsRelativePath !=
                 manifest.AttemptRootRelativePath + "/" +
                 DpoTrainer.BuildPolicyCheckpointFileName(
                     manifest.DpoStepCount)))
        {
            return Invalid<bool>();
        }

        return OperationResult<bool>.Ok(true);
    }

    public static OperationResult<bool> MatchesRuntime(
        CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest? manifest,
        RepositoryOwnedModelTrainingAttemptAddress? address,
        ModelConfig? modelConfig,
        IReadOnlyList<AdamWParameterBinding>? parameterBindings,
        DpoTrainingOptimizerSettings? optimizerSettings,
        AdamWPortableState? state)
    {
        var validation = ValidateManifest(address, manifest);
        if (!validation.IsSuccess || manifest is null || modelConfig is null ||
            parameterBindings is null || optimizerSettings is null || state is null ||
            !optimizerSettings.IsWellFormed ||
            !manifest.ModelShape!.Matches(modelConfig) ||
            manifest.OptimizerSettings != optimizerSettings ||
            manifest.OptimizerStateFormatVersion != state.FormatVersion ||
            manifest.OptimizerStateStep != state.Step)
        {
            return Invalid<bool>();
        }

        var stateMap = state.Entries
            .OrderBy(static entry => entry.Key, StringComparer.Ordinal)
            .Select(static entry =>
                new CodeDocumentationRepositoryOwnedDpoTrainingOptimizerParameterIdentity(
                    entry.Key,
                    entry.FirstMoment.Count))
            .ToArray();
        if (!manifest.OptimizerParameterMap.SequenceEqual(stateMap))
            return Invalid<bool>();

        var bindings = new Dictionary<string, TensorCore.Layers.Parameter>(StringComparer.Ordinal);
        var parameters = new HashSet<TensorCore.Layers.Parameter>(ReferenceEqualityComparer.Instance);
        foreach (var binding in parameterBindings)
        {
            if (binding is null || binding.Parameter is null ||
                string.IsNullOrEmpty(binding.Key) ||
                !bindings.TryAdd(binding.Key, binding.Parameter) ||
                !parameters.Add(binding.Parameter))
                return Invalid<bool>();
        }
        var activeMap = parameterBindings.Where(x => x.Parameter.Value.RequiresGrad)
            .Select(x => new DpoAllowedParameterIdentity(x.Key, x.Parameter.Value.Data.Length))
            .OrderBy(x => x.Key, StringComparer.Ordinal).ToArray();
        if (!manifest.AllowedParameterMap!.OrderBy(x => x.Key, StringComparer.Ordinal).SequenceEqual(activeMap)) return ProfileMismatch<bool>();
        // Sparse moments are intentional: unused/frozen bindings need no entry.
        // Each persisted entry must nevertheless match the actual ordered shape.
        foreach (var entry in state.Entries)
        {
            if (!bindings.TryGetValue(entry.Key, out var parameter) ||
                parameter.Value.Data.Length != entry.FirstMoment.Count ||
                parameter.Value.Data.Length != entry.SecondMoment.Count ||
                parameter.Value.Shape.TotalElements != parameter.Value.Data.Length ||
                !entry.Dimensions.SequenceEqual(parameter.Value.Shape.Dimensions))
            {
                return Invalid<bool>();
            }
            var product = 1;
            foreach (var dimension in parameter.Value.Shape.Dimensions)
            {
                if (dimension <= 0 || product > entry.FirstMoment.Count / dimension)
                    return Invalid<bool>();
                product *= dimension;
            }
            if (product != entry.FirstMoment.Count)
                return Invalid<bool>();
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<string> BuildManifestPath(
        RepositoryOwnedModelTrainingAttemptAddress address)
    {
        if (address is null ||
            !RepositoryOwnedTrainingArtifactLayout
                .IsValidModelTrainingAttemptAddress(address))
        {
            return Invalid<string>();
        }

        var attemptRoot = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(address.AttemptRoot));
        var path = Path.GetFullPath(Path.Combine(
            attemptRoot,
            ManifestFileName));
        if (!StringComparerForPaths.Equals(attemptRoot, address.AttemptRoot) ||
            !path.StartsWith(
                attemptRoot + Path.DirectorySeparatorChar,
                PathComparison))
        {
            return Invalid<string>();
        }

        return OperationResult<string>.Ok(path);
    }

    private static bool ValidateRandomManifestFields(
        CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest manifest)
    {
        var noRandomState = !manifest.RandomStateRequired &&
            manifest.RandomStateStep == 0 &&
            string.IsNullOrEmpty(manifest.RandomStateRelativePath) &&
            string.IsNullOrEmpty(manifest.RandomStateSha256) &&
            manifest.RandomStateByteLength == 0 &&
            manifest.RandomStateFormatVersion == 0 &&
            string.IsNullOrEmpty(manifest.RandomStateTopologySha256) &&
            string.IsNullOrEmpty(manifest.LoraConfigurationSha256);
        if (manifest.FormatVersion == LegacyFormatVersion)
            return noRandomState;
        if (!manifest.RandomStateRequired)
            return noRandomState;
        return manifest.RandomStateStep == manifest.OptimizerStateStep &&
               manifest.RandomStateFormatVersion ==
                   RepositoryOwnedDpoRandomStateFile.CurrentFormatVersion &&
               manifest.RandomStateByteLength > 0 &&
               IsSha256(manifest.RandomStateSha256) &&
               IsSha256(manifest.RandomStateTopologySha256) &&
               IsSha256(manifest.LoraConfigurationSha256) &&
               IsSafeRepositoryRelativePath(manifest.RandomStateRelativePath) &&
               manifest.RandomStateRelativePath == manifest.AttemptRootRelativePath + "/" +
                   RepositoryOwnedDpoRandomStateFile.BuildFileName(
                       manifest.RandomStateStep);
    }

    private static OperationResult<StableFileBytes> ReadExisting(string path)
    {
        try
        {
            if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path)
                || !File.Exists(path)
                || Directory.Exists(path))
            {
                return Unavailable<StableFileBytes>();
            }

            using var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                FileOptions.SequentialScan);
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    stream.SafeFileHandle,
                    path)
                || !SecureLocalFileBoundary.HasOwnerOnlyFileMode(path))
            {
                return Unavailable<StableFileBytes>();
            }

            var bytes = SecureLocalFileBoundary.ReadBoundedStableBytes(
                stream,
                MaximumManifestBytes);
            if (bytes is null || bytes.Length <= 0)
                return Invalid<StableFileBytes>();

            return OperationResult<StableFileBytes>.Ok(
                new StableFileBytes(
                    bytes,
                    Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<StableFileBytes>();
        }
    }

    private static bool IsSha256(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length == HashHexLength &&
           value.All(static character =>
               (character >= '0' && character <= '9') ||
               (character >= 'a' && character <= 'f'));

    private static bool IsValidModelShape(
        CodeDocumentationRepositoryOwnedDpoTrainingModelShapeIdentity? shape)
        => shape is not null
           && shape.VocabSize > 0
           && shape.ContextLen > 0
           && shape.HiddenDim > 0
           && shape.NumLayers > 0
           && shape.NumHeads > 0
           && shape.NumKvHeads > 0
           && shape.FfnHiddenDim > 0
           && shape.HiddenDim % shape.NumHeads == 0
           && shape.NumHeads % shape.NumKvHeads == 0
           && float.IsFinite(shape.RopeTheta)
           && shape.RopeTheta > 0f
           && float.IsFinite(shape.RmsNormEps)
           && shape.RmsNormEps > 0f;

    private static bool IsSafeRepositoryRelativePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) ||
            path.Length > MaximumRelativePathLength ||
            !StringComparer.Ordinal.Equals(path, path.Trim()) ||
            path.Any(char.IsControl) ||
            path.StartsWith("/", StringComparison.Ordinal) ||
            path.StartsWith("\\", StringComparison.Ordinal) ||
            path.Contains('\\', StringComparison.Ordinal) ||
            path.Contains(':', StringComparison.Ordinal) ||
            path.Contains("//", StringComparison.Ordinal) ||
            Path.IsPathFullyQualified(path))
        {
            return false;
        }

        var segments = path.Split('/');
        return segments.Length > 0 &&
               segments.All(static segment =>
                   segment.Length > 0 &&
                   segment is not "." and not "..");
    }

    private static OperationResult<T> Invalid<T>()
        => OperationResult<T>.Fail(
            "The CodeDocumentation DPO optimizer-state manifest is missing, malformed, or does not match the sealed run, parent, pair, and optimizer state identity.",
            InvalidErrorCode);

    private static OperationResult<T> ProfileMismatch<T>() => OperationResult<T>.Fail(
        "The canonical CodeDocumentation exact continuation profile or allowed parameter map is missing or incompatible.", DpoExecutionProfile.MismatchErrorCode);

    private static OperationResult<T> Unavailable<T>()
        => OperationResult<T>.Fail(
            "The CodeDocumentation DPO optimizer-state manifest could not be safely persisted or reopened.",
            UnavailableErrorCode);

    private static StringComparison PathComparison
        => OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    private static StringComparer StringComparerForPaths { get; }
        = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

    private sealed record StableFileBytes(byte[] Bytes, string Sha256);
}
