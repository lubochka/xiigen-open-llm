// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

using AgentInfrastructure;

public sealed record TranslationModelTrainingDataEvidence(
    string ManifestPath,
    string ManifestRepoPath,
    string CorpusPath,
    string CorpusSha256,
    int CorpusRowsCount,
    int ManifestTrainingPairsCount,
    string SourceCategory,
    string DataClass,
    string StorageScope,
    string PrivacyScope,
    bool CommonTrainReady,
    bool DpoTrainReady,
    string ProvenanceRef,
    string SplitPolicy,
    string SourceDisjointGroup,
    string LicenseOrProjectInternalRef,
    string LegalVerdict,
    bool LegalCommonDpoManifestValidated);

public sealed record TranslationModelManifest(
    string ModelId,
    string Domain,
    string Version,
    string CheckpointFilename,
    string RunnerReportRef,
    string RunnerReportFilename,
    string TrainingDataRef,
    TranslationModelTrainingDataEvidence TrainingDataEvidence,
    double AccuracyAtExport,
    string LoaderType,
    string SourceBaseCommit,
    int SchemaVersion,
    DateTimeOffset CreatedAtUtc);

public static class TranslationModelManifestContract
{
    public const string ModelId = "translation-v1";
    public const string Domain = TranslationMachine.DomainTag;
    public const string Version = "1.0.0";
    public const string CheckpointFilename = "translation-v1-checkpoint.json";
    public const string RunnerReportRef = "runner-report.json";
    public const string TrainingDataRef = "training-data/translation/translation-v1/training-manifest.json";
    public const string TrainingDataCorpusRef = "training-data/translation/translation-v1/corpus.jsonl.gz";
    public const string LoaderType = nameof(TranslationMachine);
    public const int SchemaVersion = 1;

    public static TranslationModelManifest Create(
        double accuracyAtExport,
        string sourceBaseCommit,
        DateTimeOffset createdAtUtc,
        TranslationModelTrainingDataEvidence trainingDataEvidence)
        => new(
            ModelId: ModelId,
            Domain: Domain,
            Version: Version,
            CheckpointFilename: CheckpointFilename,
            RunnerReportRef: RunnerReportRef,
            RunnerReportFilename: RunnerReportRef,
            TrainingDataRef: TrainingDataRef,
            TrainingDataEvidence: trainingDataEvidence,
            AccuracyAtExport: accuracyAtExport,
            LoaderType: LoaderType,
            SourceBaseCommit: sourceBaseCommit,
            SchemaVersion: SchemaVersion,
            CreatedAtUtc: createdAtUtc);

    public static TranslationModelTrainingDataEvidence CreateTrainingDataEvidence(
        TranslationTrainingDataArtifactValidationResult artifact)
    {
        var manifest = artifact.Manifest;
        return new TranslationModelTrainingDataEvidence(
            ManifestPath: TrainingDataRef,
            ManifestRepoPath: TranslationTrainingDataContract.ManifestRepoPath,
            CorpusPath: TrainingDataCorpusRef,
            CorpusSha256: artifact.CorpusSha256,
            CorpusRowsCount: artifact.CorpusRowsCount,
            ManifestTrainingPairsCount: manifest.TrainingPairsCount,
            SourceCategory: manifest.SourceCategory,
            DataClass: manifest.DataClass,
            StorageScope: manifest.StorageScope,
            PrivacyScope: manifest.PrivacyScope,
            CommonTrainReady: manifest.CommonTrainReady,
            DpoTrainReady: manifest.DpoTrainReady,
            ProvenanceRef: manifest.ProvenanceRef,
            SplitPolicy: manifest.SplitPolicy,
            SourceDisjointGroup: manifest.SourceDisjointGroup,
            LicenseOrProjectInternalRef: manifest.LicenseOrProjectInternalRef,
            LegalVerdict: manifest.LegalVerdict,
            LegalCommonDpoManifestValidated: TranslationTrainingDataContract.IsValidSharedTrainingDataManifest(manifest));
    }

    public static bool IsValidSharedAssetManifest(TranslationModelManifest manifest)
        => string.Equals(manifest.ModelId, ModelId, StringComparison.Ordinal) &&
           string.Equals(manifest.Domain, Domain, StringComparison.Ordinal) &&
           string.Equals(manifest.Version, Version, StringComparison.Ordinal) &&
           string.Equals(manifest.CheckpointFilename, CheckpointFilename, StringComparison.Ordinal) &&
           string.Equals(manifest.RunnerReportRef, RunnerReportRef, StringComparison.Ordinal) &&
           string.Equals(manifest.RunnerReportFilename, RunnerReportRef, StringComparison.Ordinal) &&
           string.Equals(manifest.TrainingDataRef, TrainingDataRef, StringComparison.Ordinal) &&
           IsValidTrainingDataEvidence(manifest.TrainingDataEvidence) &&
           manifest.AccuracyAtExport > 0.0 &&
           string.Equals(manifest.LoaderType, LoaderType, StringComparison.Ordinal) &&
           manifest.SchemaVersion == SchemaVersion &&
           IsSafeRelativeRef(manifest.CheckpointFilename) &&
           IsSafeRelativeRef(manifest.RunnerReportRef) &&
           IsSafeRelativeRef(manifest.RunnerReportFilename) &&
           IsSafeRelativeRef(manifest.TrainingDataRef);

    public static OperationResult<bool> ValidateTrainingDataEvidenceAgainstArtifact(
        TranslationModelTrainingDataEvidence? evidence,
        TranslationTrainingDataArtifactValidationResult? artifact)
    {
        if (!IsValidTrainingDataEvidence(evidence))
            return OperationResult<bool>.Fail(
                "Translation model asset is missing validated training-data artifact evidence.",
                "TRANSLATION_MODEL_TRAINING_DATA_EVIDENCE_INVALID");

        if (artifact is null)
            return OperationResult<bool>.Fail(
                "Current translation training-data artifact evidence is required.",
                "TRANSLATION_MODEL_TRAINING_DATA_ARTIFACT_REQUIRED");

        var current = CreateTrainingDataEvidence(artifact);
        if (!string.Equals(evidence!.ManifestPath, current.ManifestPath, StringComparison.Ordinal) ||
            !string.Equals(evidence.ManifestRepoPath, current.ManifestRepoPath, StringComparison.Ordinal) ||
            !string.Equals(evidence.CorpusPath, current.CorpusPath, StringComparison.Ordinal) ||
            !string.Equals(evidence.CorpusSha256, current.CorpusSha256, StringComparison.Ordinal) ||
            evidence.CorpusRowsCount != current.CorpusRowsCount ||
            evidence.ManifestTrainingPairsCount != current.ManifestTrainingPairsCount ||
            !string.Equals(evidence.SourceCategory, current.SourceCategory, StringComparison.Ordinal) ||
            !string.Equals(evidence.DataClass, current.DataClass, StringComparison.Ordinal) ||
            !string.Equals(evidence.StorageScope, current.StorageScope, StringComparison.Ordinal) ||
            !string.Equals(evidence.PrivacyScope, current.PrivacyScope, StringComparison.Ordinal) ||
            evidence.CommonTrainReady != current.CommonTrainReady ||
            evidence.DpoTrainReady != current.DpoTrainReady ||
            !string.Equals(evidence.ProvenanceRef, current.ProvenanceRef, StringComparison.Ordinal) ||
            !string.Equals(evidence.SplitPolicy, current.SplitPolicy, StringComparison.Ordinal) ||
            !string.Equals(evidence.SourceDisjointGroup, current.SourceDisjointGroup, StringComparison.Ordinal) ||
            !string.Equals(evidence.LicenseOrProjectInternalRef, current.LicenseOrProjectInternalRef, StringComparison.Ordinal) ||
            !string.Equals(evidence.LegalVerdict, current.LegalVerdict, StringComparison.Ordinal) ||
            evidence.LegalCommonDpoManifestValidated != current.LegalCommonDpoManifestValidated)
            return OperationResult<bool>.Fail(
                "Translation model asset training-data evidence does not match the current validated artifact.",
                "TRANSLATION_MODEL_TRAINING_DATA_EVIDENCE_STALE");

        return OperationResult<bool>.Ok(true);
    }

    public static bool IsSafeRelativeRef(string value)
        => !string.IsNullOrWhiteSpace(value) &&
           !Path.IsPathRooted(value) &&
           !Uri.TryCreate(value, UriKind.Absolute, out _) &&
           !value.Contains('\\', StringComparison.Ordinal) &&
           !value.Contains("..", StringComparison.Ordinal);

    private static bool IsValidTrainingDataEvidence(TranslationModelTrainingDataEvidence? evidence)
        => evidence is not null &&
           string.Equals(evidence.ManifestPath, TrainingDataRef, StringComparison.Ordinal) &&
           string.Equals(evidence.ManifestRepoPath, TranslationTrainingDataContract.ManifestRepoPath, StringComparison.Ordinal) &&
           string.Equals(evidence.CorpusPath, TrainingDataCorpusRef, StringComparison.Ordinal) &&
           IsSha256Hex(evidence.CorpusSha256) &&
           evidence.CorpusRowsCount > 0 &&
           evidence.ManifestTrainingPairsCount == evidence.CorpusRowsCount &&
           string.Equals(evidence.SourceCategory, TranslationTrainingDataContract.SourceCategory, StringComparison.Ordinal) &&
           string.Equals(evidence.DataClass, TranslationTrainingDataContract.DataClass, StringComparison.Ordinal) &&
           string.Equals(evidence.StorageScope, TranslationTrainingDataContract.StorageScope, StringComparison.Ordinal) &&
           string.Equals(evidence.PrivacyScope, TranslationTrainingDataContract.PrivacyScope, StringComparison.Ordinal) &&
           evidence.CommonTrainReady == TranslationTrainingDataContract.CommonTrainReady &&
           evidence.DpoTrainReady == TranslationTrainingDataContract.DpoTrainReady &&
           string.Equals(evidence.ProvenanceRef, TranslationTrainingDataContract.ProvenanceRef, StringComparison.Ordinal) &&
           string.Equals(evidence.SplitPolicy, TranslationTrainingDataContract.SplitPolicy, StringComparison.Ordinal) &&
           string.Equals(evidence.SourceDisjointGroup, TranslationTrainingDataContract.SourceDisjointGroup, StringComparison.Ordinal) &&
           string.Equals(evidence.LicenseOrProjectInternalRef, TranslationTrainingDataContract.LicenseOrProjectInternalRef, StringComparison.Ordinal) &&
           string.Equals(evidence.LegalVerdict, TranslationTrainingDataContract.LegalVerdict, StringComparison.Ordinal) &&
           evidence.LegalCommonDpoManifestValidated &&
           IsSafeRelativeRef(evidence.ManifestPath) &&
           IsSafeRelativeRef(evidence.ManifestRepoPath) &&
           IsSafeRelativeRef(evidence.CorpusPath) &&
           IsSafeRelativeRef(evidence.ProvenanceRef);

    private static bool IsSha256Hex(string value)
        => value is { Length: 64 } &&
           value.All(ch => ch is >= '0' and <= '9' or >= 'a' and <= 'f');
}
