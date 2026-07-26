// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

public sealed record TranslationTrainingDataManifest(
    int SchemaVersion,
    string LinkedModelVersion,
    string CorpusFilename,
    string CorpusRef,
    string CorpusSha256,
    int TrainingPairsCount,
    string Compression,
    string ContentType,
    DateTimeOffset CreatedAtUtc);

public static class TranslationTrainingDataContract
{
    public const int SchemaVersion = 1;
    public const string LinkedModelVersion = TranslationModelManifestContract.ModelId;
    public const string ManifestRepoPath = "training-data/translation/translation-v1/training-manifest.json";
    public const string CorpusFilename = "corpus.jsonl.gz";
    public const string Compression = "gzip";
    public const string ContentType = "application/x-jsonlines";

    public static TranslationTrainingDataManifest CreateManifest(
        string corpusSha256,
        int trainingPairsCount,
        DateTimeOffset createdAtUtc)
        => new(
            SchemaVersion: SchemaVersion,
            LinkedModelVersion: LinkedModelVersion,
            CorpusFilename: CorpusFilename,
            CorpusRef: CorpusFilename,
            CorpusSha256: corpusSha256,
            TrainingPairsCount: trainingPairsCount,
            Compression: Compression,
            ContentType: ContentType,
            CreatedAtUtc: createdAtUtc);

    public static bool IsValidSharedTrainingDataManifest(TranslationTrainingDataManifest manifest)
        => manifest.SchemaVersion == SchemaVersion &&
           string.Equals(manifest.LinkedModelVersion, LinkedModelVersion, StringComparison.Ordinal) &&
           string.Equals(manifest.CorpusFilename, CorpusFilename, StringComparison.Ordinal) &&
           string.Equals(manifest.CorpusRef, CorpusFilename, StringComparison.Ordinal) &&
           IsSha256Hex(manifest.CorpusSha256) &&
           manifest.TrainingPairsCount > 0 &&
           string.Equals(manifest.Compression, Compression, StringComparison.Ordinal) &&
           string.Equals(manifest.ContentType, ContentType, StringComparison.Ordinal) &&
           TranslationModelManifestContract.IsSafeRelativeRef(manifest.CorpusFilename) &&
           TranslationModelManifestContract.IsSafeRelativeRef(manifest.CorpusRef);

    private static bool IsSha256Hex(string value)
        => value.Length == 64 &&
           value.All(ch => ch is >= '0' and <= '9' or >= 'a' and <= 'f');
}
