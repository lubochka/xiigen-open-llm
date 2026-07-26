// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

public sealed record TranslationModelManifest(
    string ModelId,
    string Domain,
    string Version,
    string CheckpointFilename,
    string RunnerReportRef,
    string RunnerReportFilename,
    string TrainingDataRef,
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
    public const string LoaderType = nameof(TranslationMachine);
    public const int SchemaVersion = 1;

    public static TranslationModelManifest Create(
        double accuracyAtExport,
        string sourceBaseCommit,
        DateTimeOffset createdAtUtc)
        => new(
            ModelId: ModelId,
            Domain: Domain,
            Version: Version,
            CheckpointFilename: CheckpointFilename,
            RunnerReportRef: RunnerReportRef,
            RunnerReportFilename: RunnerReportRef,
            TrainingDataRef: TrainingDataRef,
            AccuracyAtExport: accuracyAtExport,
            LoaderType: LoaderType,
            SourceBaseCommit: sourceBaseCommit,
            SchemaVersion: SchemaVersion,
            CreatedAtUtc: createdAtUtc);

    public static bool IsValidSharedAssetManifest(TranslationModelManifest manifest)
        => string.Equals(manifest.ModelId, ModelId, StringComparison.Ordinal) &&
           string.Equals(manifest.Domain, Domain, StringComparison.Ordinal) &&
           string.Equals(manifest.Version, Version, StringComparison.Ordinal) &&
           string.Equals(manifest.CheckpointFilename, CheckpointFilename, StringComparison.Ordinal) &&
           string.Equals(manifest.RunnerReportRef, RunnerReportRef, StringComparison.Ordinal) &&
           string.Equals(manifest.RunnerReportFilename, RunnerReportRef, StringComparison.Ordinal) &&
           string.Equals(manifest.TrainingDataRef, TrainingDataRef, StringComparison.Ordinal) &&
           manifest.AccuracyAtExport > 0.0 &&
           string.Equals(manifest.LoaderType, LoaderType, StringComparison.Ordinal) &&
           manifest.SchemaVersion == SchemaVersion &&
           IsSafeRelativeRef(manifest.CheckpointFilename) &&
           IsSafeRelativeRef(manifest.RunnerReportRef) &&
           IsSafeRelativeRef(manifest.RunnerReportFilename) &&
           IsSafeRelativeRef(manifest.TrainingDataRef);

    public static bool IsSafeRelativeRef(string value)
        => !string.IsNullOrWhiteSpace(value) &&
           !Path.IsPathRooted(value) &&
           !Uri.TryCreate(value, UriKind.Absolute, out _) &&
           !value.Contains('\\', StringComparison.Ordinal) &&
           !value.Contains("..", StringComparison.Ordinal);
}
