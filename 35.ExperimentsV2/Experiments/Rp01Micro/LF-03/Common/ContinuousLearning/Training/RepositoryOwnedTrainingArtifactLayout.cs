// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;

/// <summary>
/// Names the only repository-owned root where generated DPO datasets may live.
/// </summary>
public sealed class RepositoryOwnedDpoArtifactRoot
{
    internal RepositoryOwnedDpoArtifactRoot(
        string repositoryRoot,
        string dpoArtifactRoot,
        string dpoArtifactRootRelativePath)
    {
        RepositoryRoot = repositoryRoot;
        DpoArtifactRoot = dpoArtifactRoot;
        DpoArtifactRootRelativePath = dpoArtifactRootRelativePath;
    }

    public string RepositoryRoot { get; }
    public string DpoArtifactRoot { get; }
    public string DpoArtifactRootRelativePath { get; }
}

/// <summary>
/// Names one immutable DPO dataset address under the canonical repository tree.
/// </summary>
public sealed class RepositoryOwnedDpoDatasetAddress
{
    internal RepositoryOwnedDpoDatasetAddress(
        string repositoryRoot,
        string dpoArtifactRoot,
        string domainSlug,
        string designPatternSlug,
        string datasetVersion,
        string datasetRoot,
        string pairStoreRoot,
        string pairsRoot,
        string manifestPath,
        string datasetRootRelativePath,
        string pairStoreRootRelativePath,
        string pairsRootRelativePath,
        string manifestRelativePath)
    {
        RepositoryRoot = repositoryRoot;
        DpoArtifactRoot = dpoArtifactRoot;
        DomainSlug = domainSlug;
        DesignPatternSlug = designPatternSlug;
        DatasetVersion = datasetVersion;
        DatasetRoot = datasetRoot;
        PairStoreRoot = pairStoreRoot;
        PairsRoot = pairsRoot;
        ManifestPath = manifestPath;
        DatasetRootRelativePath = datasetRootRelativePath;
        PairStoreRootRelativePath = pairStoreRootRelativePath;
        PairsRootRelativePath = pairsRootRelativePath;
        ManifestRelativePath = manifestRelativePath;
    }

    public string RepositoryRoot { get; }
    public string DpoArtifactRoot { get; }
    public string DomainSlug { get; }
    public string DesignPatternSlug { get; }
    public string DatasetVersion { get; }
    public string DatasetRoot { get; }
    public string PairStoreRoot { get; }
    public string PairsRoot { get; }
    public string ManifestPath { get; }
    public string DatasetRootRelativePath { get; }
    public string PairStoreRootRelativePath { get; }
    public string PairsRootRelativePath { get; }
    public string ManifestRelativePath { get; }
}

/// <summary>
/// Names the only repository-owned root where common model checkpoints may live.
/// </summary>
public sealed class RepositoryOwnedModelArtifactRoot
{
    internal RepositoryOwnedModelArtifactRoot(
        string repositoryRoot,
        string modelArtifactRoot,
        string modelArtifactRootRelativePath)
    {
        RepositoryRoot = repositoryRoot;
        ModelArtifactRoot = modelArtifactRoot;
        ModelArtifactRootRelativePath = modelArtifactRootRelativePath;
    }

    public string RepositoryRoot { get; }
    public string ModelArtifactRoot { get; }
    public string ModelArtifactRootRelativePath { get; }
}

/// <summary>One original, objective-bound baseline outside training attempts.</summary>
public sealed class RepositoryOwnedPreparedBaselineAddress
{
    internal RepositoryOwnedPreparedBaselineAddress(string repositoryRoot,
        string objectiveSha256, string baselineRoot, string relativeRoot)
    {
        RepositoryRoot = repositoryRoot;
        ObjectiveSha256 = objectiveSha256;
        BaselineRoot = baselineRoot;
        BaselineRootRelativePath = relativeRoot;
    }

    public string RepositoryRoot { get; }
    public string ObjectiveSha256 { get; }
    public string BaselineRoot { get; }
    public string BaselineRootRelativePath { get; }
    public string BaselinePath => Path.Combine(BaselineRoot, "baseline.json");
    internal string ClaimPath => Path.Combine(BaselineRoot, "baseline.claim");
    internal string LockPath => Path.Combine(BaselineRoot, ".owner.lock");
}

/// <summary>
/// Names one immutable common model checkpoint address under the repository.
/// </summary>
public sealed class RepositoryOwnedModelCheckpointAddress
{
    internal RepositoryOwnedModelCheckpointAddress(
        string repositoryRoot,
        string modelArtifactRoot,
        string domainSlug,
        string designPatternSlug,
        string modelIdSlug,
        string modelVersion,
        int step,
        string stepSlug,
        string checkpointRoot,
        string weightsPath,
        string manifestPath,
        string checkpointRootRelativePath,
        string weightsRelativePath,
        string manifestRelativePath)
    {
        RepositoryRoot = repositoryRoot;
        ModelArtifactRoot = modelArtifactRoot;
        DomainSlug = domainSlug;
        DesignPatternSlug = designPatternSlug;
        ModelIdSlug = modelIdSlug;
        ModelVersion = modelVersion;
        Step = step;
        StepSlug = stepSlug;
        CheckpointRoot = checkpointRoot;
        WeightsPath = weightsPath;
        ManifestPath = manifestPath;
        CheckpointRootRelativePath = checkpointRootRelativePath;
        WeightsRelativePath = weightsRelativePath;
        ManifestRelativePath = manifestRelativePath;
    }

    public string RepositoryRoot { get; }
    public string ModelArtifactRoot { get; }
    public string DomainSlug { get; }
    public string DesignPatternSlug { get; }
    public string ModelIdSlug { get; }
    public string ModelVersion { get; }
    public int Step { get; }
    public string StepSlug { get; }
    public string CheckpointRoot { get; }
    public string WeightsPath { get; }
    public string ManifestPath { get; }
    public string CheckpointRootRelativePath { get; }
    public string WeightsRelativePath { get; }
    public string ManifestRelativePath { get; }
}

/// <summary>
/// Адрес одной неизменяемой попытки обучения модели внутри репозитория. Это
/// история черновых файлов нового урока, а не принятая версия модели.
/// </summary>
public sealed class RepositoryOwnedModelTrainingAttemptAddress
{
    internal RepositoryOwnedModelTrainingAttemptAddress(
        string repositoryRoot,
        string modelArtifactRoot,
        string domainSlug,
        string designPatternSlug,
        string modelIdSlug,
        string modelVersion,
        string trainingRunId,
        string trainingRunIdSha256,
        string attemptRoot,
        string attemptClaimPath,
        string attemptRootRelativePath,
        string attemptClaimRelativePath)
    {
        RepositoryRoot = repositoryRoot;
        ModelArtifactRoot = modelArtifactRoot;
        DomainSlug = domainSlug;
        DesignPatternSlug = designPatternSlug;
        ModelIdSlug = modelIdSlug;
        ModelVersion = modelVersion;
        TrainingRunId = trainingRunId;
        TrainingRunIdSha256 = trainingRunIdSha256;
        AttemptRoot = attemptRoot;
        AttemptClaimPath = attemptClaimPath;
        AttemptRootRelativePath = attemptRootRelativePath;
        AttemptClaimRelativePath = attemptClaimRelativePath;
    }

    public string RepositoryRoot { get; }
    public string ModelArtifactRoot { get; }
    public string DomainSlug { get; }
    public string DesignPatternSlug { get; }
    public string ModelIdSlug { get; }
    public string ModelVersion { get; }
    public string TrainingRunId { get; }
    public string TrainingRunIdSha256 { get; }
    public string AttemptRoot { get; }
    public string AttemptClaimPath { get; }
    public string AttemptRootRelativePath { get; }
    public string AttemptClaimRelativePath { get; }
}

/// <summary>
/// Names the only repository-owned root where frozen paid-reference corpora may
/// live.
/// </summary>
public sealed class RepositoryOwnedReferenceCorpusArtifactRoot
{
    internal RepositoryOwnedReferenceCorpusArtifactRoot(
        string repositoryRoot,
        string referenceCorpusArtifactRoot,
        string referenceCorpusArtifactRootRelativePath)
    {
        RepositoryRoot = repositoryRoot;
        ReferenceCorpusArtifactRoot = referenceCorpusArtifactRoot;
        ReferenceCorpusArtifactRootRelativePath =
            referenceCorpusArtifactRootRelativePath;
    }

    public string RepositoryRoot { get; }
    public string ReferenceCorpusArtifactRoot { get; }
    public string ReferenceCorpusArtifactRootRelativePath { get; }
}

/// <summary>
/// Names one immutable frozen paid-reference corpus under the repository tree.
/// </summary>
public sealed class RepositoryOwnedReferenceCorpusAddress
{
    internal RepositoryOwnedReferenceCorpusAddress(
        string repositoryRoot,
        string referenceCorpusArtifactRoot,
        string domainSlug,
        string subjectSlug,
        string corpusVersion,
        string corpusRoot,
        string manifestPath,
        string corpusRootRelativePath,
        string manifestRelativePath)
    {
        RepositoryRoot = repositoryRoot;
        ReferenceCorpusArtifactRoot = referenceCorpusArtifactRoot;
        DomainSlug = domainSlug;
        SubjectSlug = subjectSlug;
        CorpusVersion = corpusVersion;
        CorpusRoot = corpusRoot;
        ManifestPath = manifestPath;
        CorpusRootRelativePath = corpusRootRelativePath;
        ManifestRelativePath = manifestRelativePath;
    }

    public string RepositoryRoot { get; }
    public string ReferenceCorpusArtifactRoot { get; }
    public string DomainSlug { get; }
    public string SubjectSlug { get; }
    public string CorpusVersion { get; }
    public string CorpusRoot { get; }
    public string ManifestPath { get; }
    public string CorpusRootRelativePath { get; }
    public string ManifestRelativePath { get; }
}

/// <summary>
/// Names one immutable rule-calibration manifest under a sealed frozen corpus.
/// </summary>
public sealed class RepositoryOwnedReferenceMatchingCalibrationAddress
{
    internal RepositoryOwnedReferenceMatchingCalibrationAddress(
        string repositoryRoot,
        string referenceCorpusArtifactRoot,
        string domainSlug,
        string subjectSlug,
        string corpusVersion,
        string ruleIdSlug,
        string ruleIdentitySha256,
        string ruleIdentitySha256Segment,
        string calibrationSetSha256,
        string calibrationSetSha256Segment,
        string calibrationRoot,
        string manifestPath,
        string calibrationRootRelativePath,
        string manifestRelativePath)
    {
        RepositoryRoot = repositoryRoot;
        ReferenceCorpusArtifactRoot = referenceCorpusArtifactRoot;
        DomainSlug = domainSlug;
        SubjectSlug = subjectSlug;
        CorpusVersion = corpusVersion;
        RuleIdSlug = ruleIdSlug;
        RuleIdentitySha256 = ruleIdentitySha256;
        RuleIdentitySha256Segment = ruleIdentitySha256Segment;
        CalibrationSetSha256 = calibrationSetSha256;
        CalibrationSetSha256Segment = calibrationSetSha256Segment;
        CalibrationRoot = calibrationRoot;
        ManifestPath = manifestPath;
        CalibrationRootRelativePath = calibrationRootRelativePath;
        ManifestRelativePath = manifestRelativePath;
    }

    public string RepositoryRoot { get; }
    public string ReferenceCorpusArtifactRoot { get; }
    public string DomainSlug { get; }
    public string SubjectSlug { get; }
    public string CorpusVersion { get; }
    public string RuleIdSlug { get; }
    public string RuleIdentitySha256 { get; }
    public string RuleIdentitySha256Segment { get; }
    public string CalibrationSetSha256 { get; }
    public string CalibrationSetSha256Segment { get; }
    public string CalibrationRoot { get; }
    public string ManifestPath { get; }
    public string CalibrationRootRelativePath { get; }
    public string ManifestRelativePath { get; }
}

/// <summary>
/// Builds repo-relative, deterministic training-artifact paths without asking Git.
/// </summary>
public static class RepositoryOwnedTrainingArtifactLayout
{
    public const string InvalidErrorCode =
        "REPOSITORY_OWNED_TRAINING_ARTIFACT_INVALID";
    public const string DpoArtifactRootRelativePath = "training-data/dpo";
    public const string ReferenceCorpusArtifactRootRelativePath =
        "training-data/reference-corpora";
    public const string ModelArtifactRootRelativePath =
        "training-artifacts/models";
    public const string PairStoreDirectoryName = "pairs";
    public const string CalibrationsDirectoryName = "calibrations";
    public const string CheckpointsDirectoryName = "checkpoints";
    public const string AttemptsDirectoryName = "attempts";
    public const string ModelWeightsFileName = "weights.bin";
    public const string ManifestFileName = "manifest.json";
    public const string TrainingAttemptClaimFileName = "attempt.claim";
    private const int MaximumSlugLength = 96;

    public static OperationResult<RepositoryOwnedPreparedBaselineAddress>
        CreatePreparedBaselineAddress(string repositoryRoot, string domainSlug,
            string designPatternSlug, string modelIdSlug, string modelVersion,
            string objectiveSha256)
    {
        var modelRoot = ResolveModelArtifactRoot(repositoryRoot);
        if (!modelRoot.IsSuccess || modelRoot.Value is null)
            return modelRoot.MapError<RepositoryOwnedPreparedBaselineAddress>();
        if (!ValidateSlug(domainSlug) || !ValidateSlug(designPatternSlug) ||
            !ValidateSlug(modelIdSlug) || !ValidateSlug(modelVersion) ||
            objectiveSha256 is null || objectiveSha256.Length != 64 ||
            !objectiveSha256.All(static c => c is >= '0' and <= '9' or >= 'a' and <= 'f'))
            return InvalidRoot<RepositoryOwnedPreparedBaselineAddress>();
        var root = Path.GetFullPath(Path.Combine(modelRoot.Value.ModelArtifactRoot,
            domainSlug, designPatternSlug, modelIdSlug, modelVersion,
            "prepared-baselines", objectiveSha256));
        if (!IsStrictDescendant(root, modelRoot.Value.ModelArtifactRoot) ||
            ExpectedDirectorySegmentIsFile(modelRoot.Value.RepositoryRoot, root) ||
            ExistingPathSegmentsContainReparsePoint(root) ||
            new[] { "baseline.json", "baseline.claim", ".owner.lock" }.Any(name =>
                ExistingPathSegmentsContainReparsePoint(Path.Combine(root, name))))
            return InvalidRoot<RepositoryOwnedPreparedBaselineAddress>();
        return OperationResult<RepositoryOwnedPreparedBaselineAddress>.Ok(new(
            modelRoot.Value.RepositoryRoot, objectiveSha256, root,
            ToRepositoryRelativePath(modelRoot.Value.RepositoryRoot, root)));
    }

    /// <summary>
    /// Parses the only accepted checkpoint step directory name: six decimal
    /// digits for a positive numeric step. Callers must compare steps as
    /// numbers, not as directory-name text or timestamps.
    /// </summary>
    public static bool TryParseModelCheckpointStepSlug(
        string value,
        out int step)
        => TryParseStepSlug(value, out step);

    public static bool IsValidModelTrainingAttemptRunId(string? value)
        => ValidateTrainingRunId(value);

    public static OperationResult<RepositoryOwnedModelArtifactRoot>
        ResolveModelArtifactRoot(string repositoryRoot)
    {
        var canonicalRoot = CanonicalizeRepositoryRoot(repositoryRoot);
        if (!canonicalRoot.IsSuccess || canonicalRoot.Value is null)
            return canonicalRoot.MapError<RepositoryOwnedModelArtifactRoot>();

        var modelRoot = Path.GetFullPath(Path.Combine(
            canonicalRoot.Value,
            "training-artifacts",
            "models"));
        if (!IsStrictDescendant(modelRoot, canonicalRoot.Value)
            || ExpectedDirectorySegmentIsFile(canonicalRoot.Value, modelRoot)
            || ExistingPathSegmentsContainReparsePoint(modelRoot))
        {
            return InvalidRoot<RepositoryOwnedModelArtifactRoot>();
        }

        return OperationResult<RepositoryOwnedModelArtifactRoot>.Ok(
            new RepositoryOwnedModelArtifactRoot(
                canonicalRoot.Value,
                modelRoot,
                ModelArtifactRootRelativePath));
    }

    public static OperationResult<RepositoryOwnedModelCheckpointAddress>
        CreateModelCheckpointAddress(
            string repositoryRoot,
            string domainSlug,
            string designPatternSlug,
            string modelIdSlug,
            string modelVersion,
            int step)
    {
        var modelRoot = ResolveModelArtifactRoot(repositoryRoot);
        if (!modelRoot.IsSuccess || modelRoot.Value is null)
            return modelRoot.MapError<RepositoryOwnedModelCheckpointAddress>();

        return CreateModelCheckpointAddress(
            modelRoot.Value,
            domainSlug,
            designPatternSlug,
            modelIdSlug,
            modelVersion,
            step);
    }

    public static OperationResult<RepositoryOwnedModelCheckpointAddress>
        CreateModelCheckpointAddress(
            RepositoryOwnedModelArtifactRoot modelRoot,
            string domainSlug,
            string designPatternSlug,
            string modelIdSlug,
            string modelVersion,
            int step)
    {
        ArgumentNullException.ThrowIfNull(modelRoot);
        if (!ValidateSlug(domainSlug)
            || !ValidateSlug(designPatternSlug)
            || !ValidateSlug(modelIdSlug)
            || !ValidateSlug(modelVersion)
            || step <= 0)
        {
            return InvalidRoot<RepositoryOwnedModelCheckpointAddress>();
        }

        var resolvedRoot = ResolveModelArtifactRoot(modelRoot.RepositoryRoot);
        if (!resolvedRoot.IsSuccess || resolvedRoot.Value is null)
            return resolvedRoot.MapError<RepositoryOwnedModelCheckpointAddress>();
        if (!PathComparer.Equals(
                resolvedRoot.Value.ModelArtifactRoot,
                modelRoot.ModelArtifactRoot))
        {
            return InvalidRoot<RepositoryOwnedModelCheckpointAddress>();
        }

        var resolvedModelRoot = resolvedRoot.Value;
        var stepSlug = step.ToString("D6", CultureInfo.InvariantCulture);
        var checkpointRoot = Path.GetFullPath(Path.Combine(
            resolvedModelRoot.ModelArtifactRoot,
            domainSlug,
            designPatternSlug,
            modelIdSlug,
            modelVersion,
            CheckpointsDirectoryName,
            stepSlug));
        var weightsPath = Path.GetFullPath(Path.Combine(
            checkpointRoot,
            ModelWeightsFileName));
        var manifestPath = Path.GetFullPath(Path.Combine(
            checkpointRoot,
            ManifestFileName));

        // WHY: model ids can originate in external registries, but the
        // repository address itself is a sealed safe-segment contract. The
        // descendant and reparse checks make later bytes unable to escape repo.
        if (!IsConfiguredModelCheckpointRoot(
                resolvedModelRoot,
                checkpointRoot)
            || !IsStrictDescendant(checkpointRoot, resolvedModelRoot.ModelArtifactRoot)
            || !IsStrictDescendant(weightsPath, checkpointRoot)
            || !IsStrictDescendant(manifestPath, checkpointRoot)
            || ExpectedDirectorySegmentIsFile(
                resolvedModelRoot.RepositoryRoot,
                checkpointRoot)
            || ExistingPathSegmentsContainReparsePoint(checkpointRoot)
            || ExistingPathSegmentsContainReparsePoint(weightsPath)
            || ExistingPathSegmentsContainReparsePoint(manifestPath))
        {
            return InvalidRoot<RepositoryOwnedModelCheckpointAddress>();
        }

        return OperationResult<RepositoryOwnedModelCheckpointAddress>.Ok(
            new RepositoryOwnedModelCheckpointAddress(
                resolvedModelRoot.RepositoryRoot,
                resolvedModelRoot.ModelArtifactRoot,
                domainSlug,
                designPatternSlug,
                modelIdSlug,
                modelVersion,
                step,
                stepSlug,
                checkpointRoot,
                weightsPath,
                manifestPath,
                ToRepositoryRelativePath(
                    resolvedModelRoot.RepositoryRoot,
                    checkpointRoot),
                ToRepositoryRelativePath(
                    resolvedModelRoot.RepositoryRoot,
                    weightsPath),
                ToRepositoryRelativePath(
                    resolvedModelRoot.RepositoryRoot,
                    manifestPath)));
    }

    public static OperationResult<RepositoryOwnedModelCheckpointAddress>
        ResolveModelCheckpointAddressFromCheckpointRoot(
            RepositoryOwnedModelArtifactRoot modelRoot,
            string checkpointRoot)
    {
        ArgumentNullException.ThrowIfNull(modelRoot);
        if (!TryParseModelCheckpointRoot(
                modelRoot,
                checkpointRoot,
                out var resolvedRoot,
                out var domainSlug,
                out var designPatternSlug,
                out var modelIdSlug,
                out var modelVersion,
                out var step))
        {
            return InvalidRoot<RepositoryOwnedModelCheckpointAddress>();
        }

        return CreateModelCheckpointAddress(
            resolvedRoot!,
            domainSlug!,
            designPatternSlug!,
            modelIdSlug!,
            modelVersion!,
            step);
    }

    public static bool IsValidModelCheckpointAddress(
        RepositoryOwnedModelCheckpointAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);
        var resolved = ResolveModelCheckpointAddressFromCheckpointRoot(
            new RepositoryOwnedModelArtifactRoot(
                address.RepositoryRoot,
                address.ModelArtifactRoot,
                ModelArtifactRootRelativePath),
            address.CheckpointRoot);
        if (!resolved.IsSuccess || resolved.Value is null)
            return false;

        var expected = resolved.Value;
        return PathComparer.Equals(address.RepositoryRoot, expected.RepositoryRoot)
            && PathComparer.Equals(address.ModelArtifactRoot, expected.ModelArtifactRoot)
            && StringComparer.Ordinal.Equals(address.DomainSlug, expected.DomainSlug)
            && StringComparer.Ordinal.Equals(address.DesignPatternSlug, expected.DesignPatternSlug)
            && StringComparer.Ordinal.Equals(address.ModelIdSlug, expected.ModelIdSlug)
            && StringComparer.Ordinal.Equals(address.ModelVersion, expected.ModelVersion)
            && address.Step == expected.Step
            && StringComparer.Ordinal.Equals(address.StepSlug, expected.StepSlug)
            && PathComparer.Equals(address.CheckpointRoot, expected.CheckpointRoot)
            && PathComparer.Equals(address.WeightsPath, expected.WeightsPath)
            && PathComparer.Equals(address.ManifestPath, expected.ManifestPath)
            && StringComparer.Ordinal.Equals(address.CheckpointRootRelativePath, expected.CheckpointRootRelativePath)
            && StringComparer.Ordinal.Equals(address.WeightsRelativePath, expected.WeightsRelativePath)
            && StringComparer.Ordinal.Equals(address.ManifestRelativePath, expected.ManifestRelativePath);
    }

    public static OperationResult<RepositoryOwnedModelTrainingAttemptAddress>
        CreateModelTrainingAttemptAddress(
            string repositoryRoot,
            string domainSlug,
            string designPatternSlug,
            string modelIdSlug,
            string modelVersion,
            string trainingRunId)
    {
        var modelRoot = ResolveModelArtifactRoot(repositoryRoot);
        if (!modelRoot.IsSuccess || modelRoot.Value is null)
            return modelRoot.MapError<RepositoryOwnedModelTrainingAttemptAddress>();

        return CreateModelTrainingAttemptAddress(
            modelRoot.Value,
            domainSlug,
            designPatternSlug,
            modelIdSlug,
            modelVersion,
            trainingRunId);
    }

    public static OperationResult<RepositoryOwnedModelTrainingAttemptAddress>
        CreateModelTrainingAttemptAddress(
            RepositoryOwnedModelArtifactRoot modelRoot,
            string domainSlug,
            string designPatternSlug,
            string modelIdSlug,
            string modelVersion,
            string trainingRunId)
    {
        ArgumentNullException.ThrowIfNull(modelRoot);
        if (!ValidateSlug(domainSlug)
            || !ValidateSlug(designPatternSlug)
            || !ValidateSlug(modelIdSlug)
            || !ValidateSlug(modelVersion)
            || !ValidateTrainingRunId(trainingRunId))
        {
            return InvalidRoot<RepositoryOwnedModelTrainingAttemptAddress>();
        }

        var resolvedRoot = ResolveModelArtifactRoot(modelRoot.RepositoryRoot);
        if (!resolvedRoot.IsSuccess || resolvedRoot.Value is null)
            return resolvedRoot.MapError<RepositoryOwnedModelTrainingAttemptAddress>();
        if (!PathComparer.Equals(
                resolvedRoot.Value.ModelArtifactRoot,
                modelRoot.ModelArtifactRoot))
        {
            return InvalidRoot<RepositoryOwnedModelTrainingAttemptAddress>();
        }

        var resolvedModelRoot = resolvedRoot.Value;
        var trainingRunIdSha256 = Sha256HexOfUtf8(trainingRunId);
        if (!IsCanonicalSha256HexSegment(trainingRunIdSha256))
            return InvalidRoot<RepositoryOwnedModelTrainingAttemptAddress>();

        var attemptRoot = Path.GetFullPath(Path.Combine(
            resolvedModelRoot.ModelArtifactRoot,
            domainSlug,
            designPatternSlug,
            modelIdSlug,
            modelVersion,
            AttemptsDirectoryName,
            trainingRunIdSha256));
        var attemptClaimPath = Path.GetFullPath(Path.Combine(
            attemptRoot,
            TrainingAttemptClaimFileName));

        // Имя запуска сохраняется в адресе как точная строка, но не становится
        // именем каталога: в нём разрешён ':', а такой символ нельзя безопасно
        // использовать как сегмент пути на Windows. Полная SHA-256 контрольная
        // сумма даёт стабильное безопасное имя каталога; отделение от принятых
        // версий обеспечивает сам раздел attempts, а не хэширование.
        if (!IsStrictDescendant(attemptRoot, resolvedModelRoot.ModelArtifactRoot)
            || !IsStrictDescendant(attemptRoot, resolvedModelRoot.RepositoryRoot)
            || !IsStrictDescendant(attemptClaimPath, attemptRoot)
            || ExpectedDirectorySegmentIsFile(
                resolvedModelRoot.RepositoryRoot,
                attemptRoot)
            || ExistingPathSegmentsContainReparsePoint(attemptRoot)
            || ExistingPathSegmentsContainReparsePoint(attemptClaimPath))
        {
            return InvalidRoot<RepositoryOwnedModelTrainingAttemptAddress>();
        }

        return OperationResult<RepositoryOwnedModelTrainingAttemptAddress>.Ok(
            new RepositoryOwnedModelTrainingAttemptAddress(
                resolvedModelRoot.RepositoryRoot,
                resolvedModelRoot.ModelArtifactRoot,
                domainSlug,
                designPatternSlug,
                modelIdSlug,
                modelVersion,
                trainingRunId,
                trainingRunIdSha256,
                attemptRoot,
                attemptClaimPath,
                ToRepositoryRelativePath(
                    resolvedModelRoot.RepositoryRoot,
                    attemptRoot),
                ToRepositoryRelativePath(
                    resolvedModelRoot.RepositoryRoot,
                    attemptClaimPath)));
    }

    public static bool IsValidModelTrainingAttemptAddress(
        RepositoryOwnedModelTrainingAttemptAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);
        var rebuilt = CreateModelTrainingAttemptAddress(
            address.RepositoryRoot,
            address.DomainSlug,
            address.DesignPatternSlug,
            address.ModelIdSlug,
            address.ModelVersion,
            address.TrainingRunId);
        if (!rebuilt.IsSuccess || rebuilt.Value is null)
            return false;

        var expected = rebuilt.Value;
        return PathComparer.Equals(address.RepositoryRoot, expected.RepositoryRoot)
            && PathComparer.Equals(address.ModelArtifactRoot, expected.ModelArtifactRoot)
            && StringComparer.Ordinal.Equals(address.DomainSlug, expected.DomainSlug)
            && StringComparer.Ordinal.Equals(address.DesignPatternSlug, expected.DesignPatternSlug)
            && StringComparer.Ordinal.Equals(address.ModelIdSlug, expected.ModelIdSlug)
            && StringComparer.Ordinal.Equals(address.ModelVersion, expected.ModelVersion)
            && StringComparer.Ordinal.Equals(address.TrainingRunId, expected.TrainingRunId)
            && StringComparer.Ordinal.Equals(address.TrainingRunIdSha256, expected.TrainingRunIdSha256)
            && PathComparer.Equals(address.AttemptRoot, expected.AttemptRoot)
            && PathComparer.Equals(address.AttemptClaimPath, expected.AttemptClaimPath)
            && StringComparer.Ordinal.Equals(address.AttemptRootRelativePath, expected.AttemptRootRelativePath)
            && StringComparer.Ordinal.Equals(address.AttemptClaimRelativePath, expected.AttemptClaimRelativePath);
    }

    public static bool IsConfiguredModelCheckpointRoot(
        RepositoryOwnedModelArtifactRoot modelRoot,
        string checkpointRoot)
    {
        ArgumentNullException.ThrowIfNull(modelRoot);
        // WHY: a model checkpoint is not "some child of models"; its domain,
        // pattern, model, version and step are all part of immutable identity.
        return TryParseModelCheckpointRoot(
            modelRoot,
            checkpointRoot,
            out _,
            out _,
            out _,
            out _,
            out _,
            out _);
    }

    public static OperationResult<RepositoryOwnedReferenceCorpusArtifactRoot>
        ResolveReferenceCorpusArtifactRoot(string repositoryRoot)
    {
        var canonicalRoot = CanonicalizeRepositoryRoot(repositoryRoot);
        if (!canonicalRoot.IsSuccess || canonicalRoot.Value is null)
        {
            return canonicalRoot
                .MapError<RepositoryOwnedReferenceCorpusArtifactRoot>();
        }

        var referenceCorpusRoot = Path.GetFullPath(Path.Combine(
            canonicalRoot.Value,
            "training-data",
            "reference-corpora"));
        if (!IsStrictDescendant(referenceCorpusRoot, canonicalRoot.Value)
            || ExpectedDirectorySegmentIsFile(
                canonicalRoot.Value,
                referenceCorpusRoot)
            || ExistingPathSegmentsContainReparsePoint(referenceCorpusRoot))
        {
            return InvalidRoot<RepositoryOwnedReferenceCorpusArtifactRoot>();
        }

        return OperationResult<RepositoryOwnedReferenceCorpusArtifactRoot>.Ok(
            new RepositoryOwnedReferenceCorpusArtifactRoot(
                canonicalRoot.Value,
                referenceCorpusRoot,
                ReferenceCorpusArtifactRootRelativePath));
    }

    public static OperationResult<RepositoryOwnedReferenceCorpusAddress>
        CreateReferenceCorpusAddress(
            string repositoryRoot,
            string domainSlug,
            string subjectSlug,
            string corpusVersion)
    {
        var referenceCorpusRoot =
            ResolveReferenceCorpusArtifactRoot(repositoryRoot);
        if (!referenceCorpusRoot.IsSuccess
            || referenceCorpusRoot.Value is null)
        {
            return referenceCorpusRoot
                .MapError<RepositoryOwnedReferenceCorpusAddress>();
        }

        return CreateReferenceCorpusAddress(
            referenceCorpusRoot.Value,
            domainSlug,
            subjectSlug,
            corpusVersion);
    }

    public static OperationResult<RepositoryOwnedReferenceCorpusAddress>
        CreateReferenceCorpusAddress(
            RepositoryOwnedReferenceCorpusArtifactRoot referenceCorpusRoot,
            string domainSlug,
            string subjectSlug,
            string corpusVersion)
    {
        ArgumentNullException.ThrowIfNull(referenceCorpusRoot);
        if (!ValidateSlug(domainSlug)
            || !ValidateSlug(subjectSlug)
            || !ValidateSlug(corpusVersion))
        {
            return InvalidRoot<RepositoryOwnedReferenceCorpusAddress>();
        }

        var resolvedRoot =
            ResolveReferenceCorpusArtifactRoot(referenceCorpusRoot.RepositoryRoot);
        if (!resolvedRoot.IsSuccess || resolvedRoot.Value is null)
            return resolvedRoot.MapError<RepositoryOwnedReferenceCorpusAddress>();
        if (!PathComparer.Equals(
                resolvedRoot.Value.ReferenceCorpusArtifactRoot,
                referenceCorpusRoot.ReferenceCorpusArtifactRoot))
        {
            return InvalidRoot<RepositoryOwnedReferenceCorpusAddress>();
        }

        var resolvedReferenceCorpusRoot = resolvedRoot.Value;
        var corpusRoot = Path.GetFullPath(Path.Combine(
            resolvedReferenceCorpusRoot.ReferenceCorpusArtifactRoot,
            domainSlug,
            subjectSlug,
            corpusVersion));
        var manifestPath = Path.GetFullPath(Path.Combine(
            corpusRoot,
            ManifestFileName));

        // WHY: paid-reference corpora are evaluation instruments, not training
        // inputs. Their repository address must therefore be just as sealed and
        // escape-resistant as DPO/model artifacts before a future store writes
        // any bytes into it.
        if (!TryParseReferenceCorpusRoot(
                resolvedReferenceCorpusRoot,
                corpusRoot,
                out _,
                out _,
                out _,
                out _)
            || !IsStrictDescendant(
                corpusRoot,
                resolvedReferenceCorpusRoot.ReferenceCorpusArtifactRoot)
            || !IsStrictDescendant(manifestPath, corpusRoot)
            || ExpectedDirectorySegmentIsFile(
                resolvedReferenceCorpusRoot.RepositoryRoot,
                corpusRoot)
            || ExistingPathSegmentsContainReparsePoint(corpusRoot)
            || ExistingPathSegmentsContainReparsePoint(manifestPath))
        {
            return InvalidRoot<RepositoryOwnedReferenceCorpusAddress>();
        }

        return OperationResult<RepositoryOwnedReferenceCorpusAddress>.Ok(
            new RepositoryOwnedReferenceCorpusAddress(
                resolvedReferenceCorpusRoot.RepositoryRoot,
                resolvedReferenceCorpusRoot.ReferenceCorpusArtifactRoot,
                domainSlug,
                subjectSlug,
                corpusVersion,
                corpusRoot,
                manifestPath,
                ToRepositoryRelativePath(
                    resolvedReferenceCorpusRoot.RepositoryRoot,
                    corpusRoot),
                ToRepositoryRelativePath(
                    resolvedReferenceCorpusRoot.RepositoryRoot,
                    manifestPath)));
    }

    public static bool IsValidReferenceCorpusAddress(
        RepositoryOwnedReferenceCorpusAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);
        if (!TryParseReferenceCorpusRoot(
            new RepositoryOwnedReferenceCorpusArtifactRoot(
                address.RepositoryRoot,
                address.ReferenceCorpusArtifactRoot,
                ReferenceCorpusArtifactRootRelativePath),
            address.CorpusRoot,
            out var resolvedRoot,
            out var domainSlug,
            out var subjectSlug,
            out var corpusVersion))
        {
            return false;
        }

        var recreated = CreateReferenceCorpusAddress(
            resolvedRoot!,
            domainSlug!,
            subjectSlug!,
            corpusVersion!);
        if (!recreated.IsSuccess || recreated.Value is null)
            return false;

        var expected = recreated.Value;
        return PathComparer.Equals(address.RepositoryRoot, expected.RepositoryRoot)
            && PathComparer.Equals(
                address.ReferenceCorpusArtifactRoot,
                expected.ReferenceCorpusArtifactRoot)
            && StringComparer.Ordinal.Equals(
                address.DomainSlug,
                expected.DomainSlug)
            && StringComparer.Ordinal.Equals(
                address.SubjectSlug,
                expected.SubjectSlug)
            && StringComparer.Ordinal.Equals(
                address.CorpusVersion,
                expected.CorpusVersion)
            && PathComparer.Equals(address.CorpusRoot, expected.CorpusRoot)
            && PathComparer.Equals(address.ManifestPath, expected.ManifestPath)
            && StringComparer.Ordinal.Equals(
                address.CorpusRootRelativePath,
                expected.CorpusRootRelativePath)
            && StringComparer.Ordinal.Equals(
                address.ManifestRelativePath,
                expected.ManifestRelativePath);
    }

    public static OperationResult<RepositoryOwnedReferenceMatchingCalibrationAddress>
        CreateReferenceMatchingCalibrationAddress(
            string repositoryRoot,
            string domainSlug,
            string subjectSlug,
            string corpusVersion,
            string ruleIdSlug,
            string ruleIdentitySha256,
            string calibrationSetSha256)
    {
        var referenceCorpusRoot =
            ResolveReferenceCorpusArtifactRoot(repositoryRoot);
        if (!referenceCorpusRoot.IsSuccess
            || referenceCorpusRoot.Value is null)
        {
            return referenceCorpusRoot
                .MapError<RepositoryOwnedReferenceMatchingCalibrationAddress>();
        }

        return CreateReferenceMatchingCalibrationAddress(
            referenceCorpusRoot.Value,
            domainSlug,
            subjectSlug,
            corpusVersion,
            ruleIdSlug,
            ruleIdentitySha256,
            calibrationSetSha256);
    }

    public static OperationResult<RepositoryOwnedReferenceMatchingCalibrationAddress>
        CreateReferenceMatchingCalibrationAddress(
            RepositoryOwnedReferenceCorpusArtifactRoot referenceCorpusRoot,
            string domainSlug,
            string subjectSlug,
            string corpusVersion,
            string ruleIdSlug,
            string ruleIdentitySha256,
            string calibrationSetSha256)
    {
        ArgumentNullException.ThrowIfNull(referenceCorpusRoot);
        if (!ValidateSlug(domainSlug)
            || !ValidateSlug(subjectSlug)
            || !ValidateSlug(corpusVersion)
            || !ValidateSlug(ruleIdSlug)
            || !TryNormalizeSha256RefToPathSegment(
                ruleIdentitySha256,
                out var ruleIdentitySegment)
            || !TryNormalizeSha256RefToPathSegment(
                calibrationSetSha256,
                out var calibrationSetSegment))
        {
            return InvalidRoot<
                RepositoryOwnedReferenceMatchingCalibrationAddress>();
        }

        var resolvedRoot =
            ResolveReferenceCorpusArtifactRoot(referenceCorpusRoot.RepositoryRoot);
        if (!resolvedRoot.IsSuccess || resolvedRoot.Value is null)
        {
            return resolvedRoot
                .MapError<RepositoryOwnedReferenceMatchingCalibrationAddress>();
        }

        if (!PathComparer.Equals(
                resolvedRoot.Value.ReferenceCorpusArtifactRoot,
                referenceCorpusRoot.ReferenceCorpusArtifactRoot))
        {
            return InvalidRoot<
                RepositoryOwnedReferenceMatchingCalibrationAddress>();
        }

        var resolvedReferenceCorpusRoot = resolvedRoot.Value;
        var calibrationRoot = Path.GetFullPath(Path.Combine(
            resolvedReferenceCorpusRoot.ReferenceCorpusArtifactRoot,
            domainSlug,
            subjectSlug,
            corpusVersion,
            CalibrationsDirectoryName,
            ruleIdSlug,
            ruleIdentitySegment,
            calibrationSetSegment));
        var manifestPath = Path.GetFullPath(Path.Combine(
            calibrationRoot,
            ManifestFileName));

        // WHY: calibration numbers are only meaningful when tied to exactly one
        // sealed corpus, rule identity and calibration-set hash. The address is
        // therefore parsed back through the configured shape before any write.
        if (!TryParseReferenceMatchingCalibrationRoot(
                resolvedReferenceCorpusRoot,
                calibrationRoot,
                out _,
                out _,
                out _,
                out _,
                out _,
                out _,
                out _)
            || !IsStrictDescendant(
                calibrationRoot,
                resolvedReferenceCorpusRoot.ReferenceCorpusArtifactRoot)
            || !IsStrictDescendant(manifestPath, calibrationRoot)
            || ExpectedDirectorySegmentIsFile(
                resolvedReferenceCorpusRoot.RepositoryRoot,
                calibrationRoot)
            || ExistingPathSegmentsContainReparsePoint(calibrationRoot)
            || ExistingPathSegmentsContainReparsePoint(manifestPath))
        {
            return InvalidRoot<
                RepositoryOwnedReferenceMatchingCalibrationAddress>();
        }

        return OperationResult<RepositoryOwnedReferenceMatchingCalibrationAddress>
            .Ok(new RepositoryOwnedReferenceMatchingCalibrationAddress(
                resolvedReferenceCorpusRoot.RepositoryRoot,
                resolvedReferenceCorpusRoot.ReferenceCorpusArtifactRoot,
                domainSlug,
                subjectSlug,
                corpusVersion,
                ruleIdSlug,
                ruleIdentitySha256,
                ruleIdentitySegment,
                calibrationSetSha256,
                calibrationSetSegment,
                calibrationRoot,
                manifestPath,
                ToRepositoryRelativePath(
                    resolvedReferenceCorpusRoot.RepositoryRoot,
                    calibrationRoot),
                ToRepositoryRelativePath(
                    resolvedReferenceCorpusRoot.RepositoryRoot,
                    manifestPath)));
    }

    public static OperationResult<RepositoryOwnedReferenceMatchingCalibrationAddress>
        ResolveReferenceMatchingCalibrationAddressFromCalibrationRoot(
            RepositoryOwnedReferenceCorpusArtifactRoot referenceCorpusRoot,
            string calibrationRoot)
    {
        ArgumentNullException.ThrowIfNull(referenceCorpusRoot);
        if (!TryParseReferenceMatchingCalibrationRoot(
                referenceCorpusRoot,
                calibrationRoot,
                out var resolvedRoot,
                out var domainSlug,
                out var subjectSlug,
                out var corpusVersion,
                out var ruleIdSlug,
                out var ruleIdentitySegment,
                out var calibrationSetSegment))
        {
            return InvalidRoot<
                RepositoryOwnedReferenceMatchingCalibrationAddress>();
        }

        return CreateReferenceMatchingCalibrationAddress(
            resolvedRoot!,
            domainSlug!,
            subjectSlug!,
            corpusVersion!,
            ruleIdSlug!,
            ToSha256RefFromSegment(ruleIdentitySegment!),
            ToSha256RefFromSegment(calibrationSetSegment!));
    }

    public static bool IsValidReferenceMatchingCalibrationAddress(
        RepositoryOwnedReferenceMatchingCalibrationAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);
        var resolved =
            ResolveReferenceMatchingCalibrationAddressFromCalibrationRoot(
                new RepositoryOwnedReferenceCorpusArtifactRoot(
                    address.RepositoryRoot,
                    address.ReferenceCorpusArtifactRoot,
                    ReferenceCorpusArtifactRootRelativePath),
                address.CalibrationRoot);
        if (!resolved.IsSuccess || resolved.Value is null)
            return false;

        var expected = resolved.Value;
        return PathComparer.Equals(address.RepositoryRoot, expected.RepositoryRoot)
            && PathComparer.Equals(
                address.ReferenceCorpusArtifactRoot,
                expected.ReferenceCorpusArtifactRoot)
            && StringComparer.Ordinal.Equals(
                address.DomainSlug,
                expected.DomainSlug)
            && StringComparer.Ordinal.Equals(
                address.SubjectSlug,
                expected.SubjectSlug)
            && StringComparer.Ordinal.Equals(
                address.CorpusVersion,
                expected.CorpusVersion)
            && StringComparer.Ordinal.Equals(
                address.RuleIdSlug,
                expected.RuleIdSlug)
            && StringComparer.Ordinal.Equals(
                address.RuleIdentitySha256,
                expected.RuleIdentitySha256)
            && StringComparer.Ordinal.Equals(
                address.RuleIdentitySha256Segment,
                expected.RuleIdentitySha256Segment)
            && StringComparer.Ordinal.Equals(
                address.CalibrationSetSha256,
                expected.CalibrationSetSha256)
            && StringComparer.Ordinal.Equals(
                address.CalibrationSetSha256Segment,
                expected.CalibrationSetSha256Segment)
            && PathComparer.Equals(
                address.CalibrationRoot,
                expected.CalibrationRoot)
            && PathComparer.Equals(address.ManifestPath, expected.ManifestPath)
            && StringComparer.Ordinal.Equals(
                address.CalibrationRootRelativePath,
                expected.CalibrationRootRelativePath)
            && StringComparer.Ordinal.Equals(
                address.ManifestRelativePath,
                expected.ManifestRelativePath);
    }

    public static OperationResult<RepositoryOwnedDpoArtifactRoot>
        ResolveDpoArtifactRoot(string repositoryRoot)
    {
        var canonicalRoot = CanonicalizeRepositoryRoot(repositoryRoot);
        if (!canonicalRoot.IsSuccess || canonicalRoot.Value is null)
            return canonicalRoot.MapError<RepositoryOwnedDpoArtifactRoot>();

        var dpoRoot = Path.GetFullPath(Path.Combine(
            canonicalRoot.Value,
            "training-data",
            "dpo"));
        if (!IsStrictDescendant(dpoRoot, canonicalRoot.Value)
            || ExpectedDirectorySegmentIsFile(canonicalRoot.Value, dpoRoot)
            || ExistingPathSegmentsContainReparsePoint(dpoRoot))
        {
            return InvalidRoot<RepositoryOwnedDpoArtifactRoot>();
        }

        return OperationResult<RepositoryOwnedDpoArtifactRoot>.Ok(
            new RepositoryOwnedDpoArtifactRoot(
                canonicalRoot.Value,
                dpoRoot,
                DpoArtifactRootRelativePath));
    }

    public static OperationResult<RepositoryOwnedDpoDatasetAddress>
        CreateDpoDatasetAddress(
            string repositoryRoot,
            string domainSlug,
            string designPatternSlug,
            string datasetVersion)
    {
        var dpoRoot = ResolveDpoArtifactRoot(repositoryRoot);
        if (!dpoRoot.IsSuccess || dpoRoot.Value is null)
            return dpoRoot.MapError<RepositoryOwnedDpoDatasetAddress>();

        return CreateDpoDatasetAddress(
            dpoRoot.Value,
            domainSlug,
            designPatternSlug,
            datasetVersion);
    }

    public static OperationResult<RepositoryOwnedDpoDatasetAddress>
        CreateDpoDatasetAddress(
            RepositoryOwnedDpoArtifactRoot dpoRoot,
            string domainSlug,
            string designPatternSlug,
            string datasetVersion)
    {
        ArgumentNullException.ThrowIfNull(dpoRoot);
        if (!ValidateSlug(domainSlug)
            || !ValidateSlug(designPatternSlug)
            || !ValidateSlug(datasetVersion))
        {
            return InvalidRoot<RepositoryOwnedDpoDatasetAddress>();
        }

        var resolvedRoot = ResolveDpoArtifactRoot(dpoRoot.RepositoryRoot);
        if (!resolvedRoot.IsSuccess || resolvedRoot.Value is null)
            return resolvedRoot.MapError<RepositoryOwnedDpoDatasetAddress>();
        if (!PathComparer.Equals(
                resolvedRoot.Value.DpoArtifactRoot,
                dpoRoot.DpoArtifactRoot))
        {
            return InvalidRoot<RepositoryOwnedDpoDatasetAddress>();
        }

        var resolvedDpoRoot = resolvedRoot.Value;
        var datasetRoot = Path.GetFullPath(Path.Combine(
            resolvedDpoRoot.DpoArtifactRoot,
            domainSlug,
            designPatternSlug,
            datasetVersion));
        var pairsRoot = Path.GetFullPath(Path.Combine(
            datasetRoot,
            PairStoreDirectoryName));
        var manifestPath = Path.GetFullPath(Path.Combine(
            datasetRoot,
            ManifestFileName));

        // WHY: the slugs are already lexical-safe, but the final descendant
        // check catches platform surprises and makes the boundary visible.
        if (!IsConfiguredDpoPairRoot(resolvedDpoRoot, pairsRoot)
            || !IsStrictDescendant(datasetRoot, resolvedDpoRoot.DpoArtifactRoot)
            || !IsStrictDescendant(manifestPath, datasetRoot)
            || ExpectedDirectorySegmentIsFile(
                resolvedDpoRoot.RepositoryRoot,
                pairsRoot)
            || ExistingPathSegmentsContainReparsePoint(datasetRoot)
            || ExistingPathSegmentsContainReparsePoint(pairsRoot)
            || ExistingPathSegmentsContainReparsePoint(manifestPath))
        {
            return InvalidRoot<RepositoryOwnedDpoDatasetAddress>();
        }

        return OperationResult<RepositoryOwnedDpoDatasetAddress>.Ok(
            new RepositoryOwnedDpoDatasetAddress(
                resolvedDpoRoot.RepositoryRoot,
                resolvedDpoRoot.DpoArtifactRoot,
                domainSlug,
                designPatternSlug,
                datasetVersion,
                datasetRoot,
                pairsRoot,
                pairsRoot,
                manifestPath,
                ToRepositoryRelativePath(resolvedDpoRoot.RepositoryRoot, datasetRoot),
                ToRepositoryRelativePath(resolvedDpoRoot.RepositoryRoot, pairsRoot),
                ToRepositoryRelativePath(resolvedDpoRoot.RepositoryRoot, pairsRoot),
                ToRepositoryRelativePath(resolvedDpoRoot.RepositoryRoot, manifestPath)));
    }

    public static OperationResult<RepositoryOwnedDpoDatasetAddress>
        ResolveDpoDatasetAddressFromPairStoreRoot(
            RepositoryOwnedDpoArtifactRoot dpoRoot,
            string pairStoreRoot)
    {
        ArgumentNullException.ThrowIfNull(dpoRoot);
        if (!TryParseDpoPairRoot(
                dpoRoot,
                pairStoreRoot,
                out var resolvedRoot,
                out var domainSlug,
                out var designPatternSlug,
                out var datasetVersion))
        {
            return InvalidRoot<RepositoryOwnedDpoDatasetAddress>();
        }

        return CreateDpoDatasetAddress(
            resolvedRoot!,
            domainSlug!,
            designPatternSlug!,
            datasetVersion!);
    }

    public static bool IsValidDpoDatasetAddress(
        RepositoryOwnedDpoDatasetAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);
        var resolved = ResolveDpoDatasetAddressFromPairStoreRoot(
            new RepositoryOwnedDpoArtifactRoot(
                address.RepositoryRoot,
                address.DpoArtifactRoot,
                DpoArtifactRootRelativePath),
            address.PairStoreRoot);
        if (!resolved.IsSuccess || resolved.Value is null)
            return false;

        var expected = resolved.Value;
        return PathComparer.Equals(address.RepositoryRoot, expected.RepositoryRoot)
            && PathComparer.Equals(address.DpoArtifactRoot, expected.DpoArtifactRoot)
            && StringComparer.Ordinal.Equals(address.DomainSlug, expected.DomainSlug)
            && StringComparer.Ordinal.Equals(address.DesignPatternSlug, expected.DesignPatternSlug)
            && StringComparer.Ordinal.Equals(address.DatasetVersion, expected.DatasetVersion)
            && PathComparer.Equals(address.DatasetRoot, expected.DatasetRoot)
            && PathComparer.Equals(address.PairStoreRoot, expected.PairStoreRoot)
            && PathComparer.Equals(address.PairsRoot, expected.PairsRoot)
            && PathComparer.Equals(address.ManifestPath, expected.ManifestPath)
            && StringComparer.Ordinal.Equals(address.DatasetRootRelativePath, expected.DatasetRootRelativePath)
            && StringComparer.Ordinal.Equals(address.PairStoreRootRelativePath, expected.PairStoreRootRelativePath)
            && StringComparer.Ordinal.Equals(address.PairsRootRelativePath, expected.PairsRootRelativePath)
            && StringComparer.Ordinal.Equals(address.ManifestRelativePath, expected.ManifestRelativePath);
    }

    public static bool IsConfiguredDpoPairRoot(
        RepositoryOwnedDpoArtifactRoot dpoRoot,
        string pairStoreRoot)
    {
        ArgumentNullException.ThrowIfNull(dpoRoot);
        // WHY: a DPO store is not "some child of training-data/dpo"; it is
        // exactly one dataset's pairs folder, so a wrong level cannot become a
        // hidden shared bucket for unrelated domains.
        return TryParseDpoPairRoot(
            dpoRoot,
            pairStoreRoot,
            out _,
            out _,
            out _,
            out _);
    }

    private static OperationResult<string> CanonicalizeRepositoryRoot(
        string repositoryRoot)
    {
        if (string.IsNullOrWhiteSpace(repositoryRoot)
            || !StringComparer.Ordinal.Equals(repositoryRoot, repositoryRoot.Trim())
            || repositoryRoot.Any(char.IsControl)
            || !Path.IsPathFullyQualified(repositoryRoot))
        {
            return InvalidRoot<string>();
        }

        string fullRoot;
        try
        {
            fullRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(repositoryRoot));
        }
        catch (Exception exception)
            when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                .IsNonFatal(exception))
        {
            return InvalidRoot<string>();
        }

        var trimmedInput = Path.TrimEndingDirectorySeparator(repositoryRoot);
        if (!PathComparer.Equals(trimmedInput, fullRoot)
            || PathComparer.Equals(Path.GetPathRoot(fullRoot), fullRoot)
            || File.Exists(fullRoot)
            || !Directory.Exists(fullRoot)
            || ExistingPathSegmentsContainReparsePoint(fullRoot)
            || !HasRequiredRepositorySentinels(fullRoot))
        {
            return InvalidRoot<string>();
        }

        return OperationResult<string>.Ok(fullRoot);
    }

    private static bool HasRequiredRepositorySentinels(string repositoryRoot)
    {
        // WHY: runtime must not shell out to Git, so the product root is
        // identified by stable project sentinels that exist in this repository.
        var sentinels = new[]
        {
            Path.Combine(repositoryRoot, "LlmFromScratch.sln"),
            Path.Combine(repositoryRoot, "14.ContinuousLearning"),
            Path.Combine(repositoryRoot, "11.DPO"),
        };
        return File.Exists(sentinels[0])
               && Directory.Exists(sentinels[1])
               && Directory.Exists(sentinels[2])
               && sentinels.All(path => !ExistingPathSegmentsContainReparsePoint(path));
    }

    internal static bool ValidateSlug(string value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value.Length > MaximumSlugLength
            || !StringComparer.Ordinal.Equals(value, value.Trim())
            || value is "." or ".."
            || Path.IsPathRooted(value)
            || value.Contains('/')
            || value.Contains('\\')
            || value.Contains('.')
            || value.Any(char.IsControl))
        {
            return false;
        }

        return value.All(character =>
            character is >= 'a' and <= 'z'
            || character is >= '0' and <= '9'
            || character == '-');
    }

    private static bool ValidateTrainingRunId(string? value)
        => value is not null
           && value.Length is > 0 and <= 256
           && !string.IsNullOrWhiteSpace(value)
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && !value.Any(char.IsControl)
           && value.All(static character =>
               character is >= 'a' and <= 'z'
                   or >= 'A' and <= 'Z'
                   or >= '0' and <= '9'
                   or '-' or '_' or '.' or ':');

    private static string Sha256HexOfUtf8(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private static bool TryNormalizeSha256RefToPathSegment(
        string value,
        out string segment)
    {
        segment = string.Empty;
        if (!FrozenReferenceCanonicalization.IsCanonicalSha256Ref(value))
            return false;

        var candidate = value["sha256:".Length..];
        if (!IsCanonicalSha256HexSegment(candidate))
            return false;

        segment = candidate;
        return true;
    }

    private static bool IsCanonicalSha256HexSegment(string value)
        => value.Length == 64
           && value.All(static character => character is >= '0' and <= '9'
               or >= 'a' and <= 'f');

    private static string ToSha256RefFromSegment(string segment)
        => "sha256:" + segment;

    private static bool TryParseDpoPairRoot(
        RepositoryOwnedDpoArtifactRoot dpoRoot,
        string pairStoreRoot,
        out RepositoryOwnedDpoArtifactRoot? resolvedRoot,
        out string? domainSlug,
        out string? designPatternSlug,
        out string? datasetVersion)
    {
        resolvedRoot = null;
        domainSlug = null;
        designPatternSlug = null;
        datasetVersion = null;

        if (string.IsNullOrWhiteSpace(pairStoreRoot)
            || !StringComparer.Ordinal.Equals(pairStoreRoot, pairStoreRoot.Trim())
            || pairStoreRoot.Any(char.IsControl)
            || !Path.IsPathFullyQualified(pairStoreRoot))
            return false;

        try
        {
            var canonicalRoot = ResolveDpoArtifactRoot(dpoRoot.RepositoryRoot);
            if (!canonicalRoot.IsSuccess || canonicalRoot.Value is null)
                return false;
            if (!PathComparer.Equals(
                    canonicalRoot.Value.DpoArtifactRoot,
                    dpoRoot.DpoArtifactRoot))
            {
                return false;
            }

            var canonicalPairRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(pairStoreRoot));
            // WHY: accepting a spelling that merely normalizes to the right
            // address hides traversal and ambiguous separators in persisted
            // configuration. Runtime accepts only the canonical spelling.
            if (!PathComparer.Equals(pairStoreRoot, canonicalPairRoot)
                || !IsStrictDescendant(canonicalPairRoot, canonicalRoot.Value.DpoArtifactRoot)
                || !IsStrictDescendant(canonicalPairRoot, canonicalRoot.Value.RepositoryRoot)
                || ExpectedDirectorySegmentIsFile(
                    canonicalRoot.Value.RepositoryRoot,
                    canonicalPairRoot)
                || ExistingPathSegmentsContainReparsePoint(canonicalPairRoot))
            {
                return false;
            }

            var relative = Path.GetRelativePath(
                    canonicalRoot.Value.DpoArtifactRoot,
                    canonicalPairRoot)
                .Replace(Path.DirectorySeparatorChar, '/')
                .Replace(Path.AltDirectorySeparatorChar, '/');
            if (relative.Length == 0
                || relative == "."
                || relative == ".."
                || relative.StartsWith("../", StringComparison.Ordinal))
            {
                return false;
            }

            var segments = relative.Split('/');
            if (segments.Length != 4
                || !StringComparer.Ordinal.Equals(
                    segments[3],
                    PairStoreDirectoryName)
                || !ValidateSlug(segments[0])
                || !ValidateSlug(segments[1])
                || !ValidateSlug(segments[2]))
            {
                return false;
            }

            var expectedPairRoot = Path.GetFullPath(Path.Combine(
                canonicalRoot.Value.DpoArtifactRoot,
                segments[0],
                segments[1],
                segments[2],
                PairStoreDirectoryName));
            if (!PathComparer.Equals(expectedPairRoot, canonicalPairRoot))
                return false;

            resolvedRoot = canonicalRoot.Value;
            domainSlug = segments[0];
            designPatternSlug = segments[1];
            datasetVersion = segments[2];
            return true;
        }
        catch (Exception exception)
            when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                .IsNonFatal(exception))
        {
            return false;
        }
    }

    private static bool TryParseReferenceCorpusRoot(
        RepositoryOwnedReferenceCorpusArtifactRoot referenceCorpusRoot,
        string corpusRoot,
        out RepositoryOwnedReferenceCorpusArtifactRoot? resolvedRoot,
        out string? domainSlug,
        out string? subjectSlug,
        out string? corpusVersion)
    {
        resolvedRoot = null;
        domainSlug = null;
        subjectSlug = null;
        corpusVersion = null;

        if (string.IsNullOrWhiteSpace(corpusRoot)
            || !StringComparer.Ordinal.Equals(corpusRoot, corpusRoot.Trim())
            || corpusRoot.Any(char.IsControl)
            || !Path.IsPathFullyQualified(corpusRoot))
        {
            return false;
        }

        try
        {
            var canonicalRoot = ResolveReferenceCorpusArtifactRoot(
                referenceCorpusRoot.RepositoryRoot);
            if (!canonicalRoot.IsSuccess || canonicalRoot.Value is null)
                return false;
            if (!PathComparer.Equals(
                    canonicalRoot.Value.ReferenceCorpusArtifactRoot,
                    referenceCorpusRoot.ReferenceCorpusArtifactRoot))
            {
                return false;
            }

            var canonicalCorpusRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(corpusRoot));
            // WHY: persisted corpus roots must already be canonical. A path
            // spelling that only becomes safe after normalization would hide a
            // caller-controlled traversal in future manifests.
            if (!PathComparer.Equals(corpusRoot, canonicalCorpusRoot)
                || !IsStrictDescendant(
                    canonicalCorpusRoot,
                    canonicalRoot.Value.ReferenceCorpusArtifactRoot)
                || !IsStrictDescendant(
                    canonicalCorpusRoot,
                    canonicalRoot.Value.RepositoryRoot)
                || ExpectedDirectorySegmentIsFile(
                    canonicalRoot.Value.RepositoryRoot,
                    canonicalCorpusRoot)
                || ExistingPathSegmentsContainReparsePoint(
                    canonicalCorpusRoot))
            {
                return false;
            }

            var relative = Path.GetRelativePath(
                    canonicalRoot.Value.ReferenceCorpusArtifactRoot,
                    canonicalCorpusRoot)
                .Replace(Path.DirectorySeparatorChar, '/')
                .Replace(Path.AltDirectorySeparatorChar, '/');
            if (relative.Length == 0
                || relative == "."
                || relative == ".."
                || relative.StartsWith("../", StringComparison.Ordinal))
            {
                return false;
            }

            var segments = relative.Split('/');
            if (segments.Length != 3
                || !ValidateSlug(segments[0])
                || !ValidateSlug(segments[1])
                || !ValidateSlug(segments[2]))
            {
                return false;
            }

            var expectedCorpusRoot = Path.GetFullPath(Path.Combine(
                canonicalRoot.Value.ReferenceCorpusArtifactRoot,
                segments[0],
                segments[1],
                segments[2]));
            if (!PathComparer.Equals(expectedCorpusRoot, canonicalCorpusRoot))
                return false;

            resolvedRoot = canonicalRoot.Value;
            domainSlug = segments[0];
            subjectSlug = segments[1];
            corpusVersion = segments[2];
            return true;
        }
        catch (Exception exception)
            when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                .IsNonFatal(exception))
        {
            return false;
        }
    }

    private static bool TryParseReferenceMatchingCalibrationRoot(
        RepositoryOwnedReferenceCorpusArtifactRoot referenceCorpusRoot,
        string calibrationRoot,
        out RepositoryOwnedReferenceCorpusArtifactRoot? resolvedRoot,
        out string? domainSlug,
        out string? subjectSlug,
        out string? corpusVersion,
        out string? ruleIdSlug,
        out string? ruleIdentitySha256Segment,
        out string? calibrationSetSha256Segment)
    {
        resolvedRoot = null;
        domainSlug = null;
        subjectSlug = null;
        corpusVersion = null;
        ruleIdSlug = null;
        ruleIdentitySha256Segment = null;
        calibrationSetSha256Segment = null;

        if (string.IsNullOrWhiteSpace(calibrationRoot)
            || !StringComparer.Ordinal.Equals(
                calibrationRoot,
                calibrationRoot.Trim())
            || calibrationRoot.Any(char.IsControl)
            || !Path.IsPathFullyQualified(calibrationRoot))
        {
            return false;
        }

        try
        {
            var canonicalRoot = ResolveReferenceCorpusArtifactRoot(
                referenceCorpusRoot.RepositoryRoot);
            if (!canonicalRoot.IsSuccess || canonicalRoot.Value is null)
                return false;
            if (!PathComparer.Equals(
                    canonicalRoot.Value.ReferenceCorpusArtifactRoot,
                    referenceCorpusRoot.ReferenceCorpusArtifactRoot))
            {
                return false;
            }

            var canonicalCalibrationRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(calibrationRoot));
            // WHY: the hash path segments are authority, not decoration. A path
            // spelling that normalizes into the right directory is still a
            // caller-controlled traversal and cannot be persisted.
            if (!PathComparer.Equals(
                    calibrationRoot,
                    canonicalCalibrationRoot)
                || !IsStrictDescendant(
                    canonicalCalibrationRoot,
                    canonicalRoot.Value.ReferenceCorpusArtifactRoot)
                || !IsStrictDescendant(
                    canonicalCalibrationRoot,
                    canonicalRoot.Value.RepositoryRoot)
                || ExpectedDirectorySegmentIsFile(
                    canonicalRoot.Value.RepositoryRoot,
                    canonicalCalibrationRoot)
                || ExistingPathSegmentsContainReparsePoint(
                    canonicalCalibrationRoot))
            {
                return false;
            }

            var relative = Path.GetRelativePath(
                    canonicalRoot.Value.ReferenceCorpusArtifactRoot,
                    canonicalCalibrationRoot)
                .Replace(Path.DirectorySeparatorChar, '/')
                .Replace(Path.AltDirectorySeparatorChar, '/');
            if (relative.Length == 0
                || relative == "."
                || relative == ".."
                || relative.StartsWith("../", StringComparison.Ordinal))
            {
                return false;
            }

            var segments = relative.Split('/');
            if (segments.Length != 7
                || !StringComparer.Ordinal.Equals(
                    segments[3],
                    CalibrationsDirectoryName)
                || !ValidateSlug(segments[0])
                || !ValidateSlug(segments[1])
                || !ValidateSlug(segments[2])
                || !ValidateSlug(segments[4])
                || !IsCanonicalSha256HexSegment(segments[5])
                || !IsCanonicalSha256HexSegment(segments[6]))
            {
                return false;
            }

            var expectedCalibrationRoot = Path.GetFullPath(Path.Combine(
                canonicalRoot.Value.ReferenceCorpusArtifactRoot,
                segments[0],
                segments[1],
                segments[2],
                CalibrationsDirectoryName,
                segments[4],
                segments[5],
                segments[6]));
            if (!PathComparer.Equals(
                    expectedCalibrationRoot,
                    canonicalCalibrationRoot))
            {
                return false;
            }

            resolvedRoot = canonicalRoot.Value;
            domainSlug = segments[0];
            subjectSlug = segments[1];
            corpusVersion = segments[2];
            ruleIdSlug = segments[4];
            ruleIdentitySha256Segment = segments[5];
            calibrationSetSha256Segment = segments[6];
            return true;
        }
        catch (Exception exception)
            when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                .IsNonFatal(exception))
        {
            return false;
        }
    }

    private static bool TryParseModelCheckpointRoot(
        RepositoryOwnedModelArtifactRoot modelRoot,
        string checkpointRoot,
        out RepositoryOwnedModelArtifactRoot? resolvedRoot,
        out string? domainSlug,
        out string? designPatternSlug,
        out string? modelIdSlug,
        out string? modelVersion,
        out int step)
    {
        resolvedRoot = null;
        domainSlug = null;
        designPatternSlug = null;
        modelIdSlug = null;
        modelVersion = null;
        step = 0;

        if (string.IsNullOrWhiteSpace(checkpointRoot)
            || !StringComparer.Ordinal.Equals(checkpointRoot, checkpointRoot.Trim())
            || checkpointRoot.Any(char.IsControl)
            || !Path.IsPathFullyQualified(checkpointRoot))
            return false;

        try
        {
            var canonicalRoot = ResolveModelArtifactRoot(modelRoot.RepositoryRoot);
            if (!canonicalRoot.IsSuccess || canonicalRoot.Value is null)
                return false;
            if (!PathComparer.Equals(
                    canonicalRoot.Value.ModelArtifactRoot,
                    modelRoot.ModelArtifactRoot))
            {
                return false;
            }

            var canonicalCheckpointRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(checkpointRoot));
            // WHY: raw traversal that normalizes to a good path still hides
            // caller-controlled spelling; persisted artifact addresses are
            // accepted only in their canonical spelling.
            if (!PathComparer.Equals(checkpointRoot, canonicalCheckpointRoot)
                || !IsStrictDescendant(
                    canonicalCheckpointRoot,
                    canonicalRoot.Value.ModelArtifactRoot)
                || !IsStrictDescendant(
                    canonicalCheckpointRoot,
                    canonicalRoot.Value.RepositoryRoot)
                || ExpectedDirectorySegmentIsFile(
                    canonicalRoot.Value.RepositoryRoot,
                    canonicalCheckpointRoot)
                || ExistingPathSegmentsContainReparsePoint(
                    canonicalCheckpointRoot))
            {
                return false;
            }

            var relative = Path.GetRelativePath(
                    canonicalRoot.Value.ModelArtifactRoot,
                    canonicalCheckpointRoot)
                .Replace(Path.DirectorySeparatorChar, '/')
                .Replace(Path.AltDirectorySeparatorChar, '/');
            if (relative.Length == 0
                || relative == "."
                || relative == ".."
                || relative.StartsWith("../", StringComparison.Ordinal))
            {
                return false;
            }

            var segments = relative.Split('/');
            if (segments.Length != 6
                || !StringComparer.Ordinal.Equals(
                    segments[4],
                    CheckpointsDirectoryName)
                || !ValidateSlug(segments[0])
                || !ValidateSlug(segments[1])
                || !ValidateSlug(segments[2])
                || !ValidateSlug(segments[3])
                || !TryParseStepSlug(segments[5], out var parsedStep))
            {
                return false;
            }

            var expectedCheckpointRoot = Path.GetFullPath(Path.Combine(
                canonicalRoot.Value.ModelArtifactRoot,
                segments[0],
                segments[1],
                segments[2],
                segments[3],
                CheckpointsDirectoryName,
                segments[5]));
            if (!PathComparer.Equals(
                    expectedCheckpointRoot,
                    canonicalCheckpointRoot))
            {
                return false;
            }

            resolvedRoot = canonicalRoot.Value;
            domainSlug = segments[0];
            designPatternSlug = segments[1];
            modelIdSlug = segments[2];
            modelVersion = segments[3];
            step = parsedStep;
            return true;
        }
        catch (Exception exception)
            when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                .IsNonFatal(exception))
        {
            return false;
        }
    }

    private static bool TryParseStepSlug(string value, out int step)
    {
        step = 0;
        if (value.Length != 6
            || !value.All(static character => character is >= '0' and <= '9')
            || !int.TryParse(
                value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var parsed)
            || parsed <= 0)
        {
            return false;
        }

        step = parsed;
        return true;
    }

    private static string ToRepositoryRelativePath(string repositoryRoot, string path)
    {
        var relative = Path.GetRelativePath(repositoryRoot, path)
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');
        if (relative.Length == 0
            || relative.StartsWith("../", StringComparison.Ordinal)
            || relative == "..")
        {
            throw new InvalidDataException("Repository artifact path escaped root.");
        }
        return relative;
    }

    private static bool ExistingPathSegmentsContainReparsePoint(string path)
        => SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path);

    private static bool ExpectedDirectorySegmentIsFile(
        string repositoryRoot,
        string expectedDirectory)
    {
        var canonicalRepositoryRoot = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(repositoryRoot));
        var canonicalExpectedDirectory = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(expectedDirectory));
        if (!IsStrictDescendant(canonicalExpectedDirectory, canonicalRepositoryRoot))
            return true;

        var relative = Path.GetRelativePath(
            canonicalRepositoryRoot,
            canonicalExpectedDirectory);
        var current = canonicalRepositoryRoot;
        foreach (var segment in relative.Split(
                     new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                     StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, segment);
            if (File.Exists(current))
                return true;
        }

        return false;
    }

    private static bool IsStrictDescendant(string candidate, string root)
    {
        var normalizedCandidate = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(candidate));
        var normalizedRoot = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(root));
        if (PathComparer.Equals(normalizedCandidate, normalizedRoot))
            return false;
        var prefix = normalizedRoot + Path.DirectorySeparatorChar;
        return normalizedCandidate.StartsWith(
            prefix,
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);
    }

    private static OperationResult<T> InvalidRoot<T>()
        => OperationResult<T>.Fail(
            "Repository-owned training artifact path is invalid.",
            InvalidErrorCode);

    private static StringComparer PathComparer { get; }
        = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
}
