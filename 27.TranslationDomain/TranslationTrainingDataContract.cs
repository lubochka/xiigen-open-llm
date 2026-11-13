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
    DateTimeOffset CreatedAtUtc);

public static class TranslationTrainingDataContract
{
    public const int SchemaVersion = 1;
    public const string LinkedModelVersion = TranslationModelManifestContract.ModelId;
    public const string ManifestRepoPath = "training-data/translation/translation-v1/training-manifest.json";
    public const string CorpusFilename = "corpus.jsonl.gz";
    public const string Compression = "gzip";
    public const string ContentType = "application/x-jsonlines";
    public const string SourceCategory = "synthetic_self_authored";
    public const string DataClass = "translation_seed_dpo_pairs";
    public const string StorageScope = "CommonGlobal";
    public const string PrivacyScope = "common_allowed";
    public const bool CommonTrainReady = true;
    public const bool DpoTrainReady = true;
    public const string ProvenanceRef = "seed-corpus/translation/language-translate-seed-v1";
    public const string SplitPolicy = "train_only_source_disjoint_from_held_out";
    public const string SourceDisjointGroup = "language-translate-seed-v1-train";
    public const string LicenseOrProjectInternalRef = "project_internal_self_authored";
    public const string LegalVerdict = "ACCEPT";

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
            SourceCategory: SourceCategory,
            DataClass: DataClass,
            StorageScope: StorageScope,
            PrivacyScope: PrivacyScope,
            CommonTrainReady: CommonTrainReady,
            DpoTrainReady: DpoTrainReady,
            ProvenanceRef: ProvenanceRef,
            SplitPolicy: SplitPolicy,
            SourceDisjointGroup: SourceDisjointGroup,
            LicenseOrProjectInternalRef: LicenseOrProjectInternalRef,
            LegalVerdict: LegalVerdict,
            CreatedAtUtc: createdAtUtc);

    public static bool IsValidSharedTrainingDataManifest(TranslationTrainingDataManifest? manifest)
        => manifest is not null &&
           manifest.SchemaVersion == SchemaVersion &&
           string.Equals(manifest.LinkedModelVersion, LinkedModelVersion, StringComparison.Ordinal) &&
           string.Equals(manifest.CorpusFilename, CorpusFilename, StringComparison.Ordinal) &&
           string.Equals(manifest.CorpusRef, CorpusFilename, StringComparison.Ordinal) &&
           IsSha256Hex(manifest.CorpusSha256) &&
           manifest.TrainingPairsCount > 0 &&
           string.Equals(manifest.Compression, Compression, StringComparison.Ordinal) &&
           string.Equals(manifest.ContentType, ContentType, StringComparison.Ordinal) &&
           string.Equals(manifest.SourceCategory, SourceCategory, StringComparison.Ordinal) &&
           string.Equals(manifest.DataClass, DataClass, StringComparison.Ordinal) &&
           string.Equals(manifest.StorageScope, StorageScope, StringComparison.Ordinal) &&
           string.Equals(manifest.PrivacyScope, PrivacyScope, StringComparison.Ordinal) &&
           manifest.CommonTrainReady == CommonTrainReady &&
           manifest.DpoTrainReady == DpoTrainReady &&
           string.Equals(manifest.ProvenanceRef, ProvenanceRef, StringComparison.Ordinal) &&
           string.Equals(manifest.SplitPolicy, SplitPolicy, StringComparison.Ordinal) &&
           string.Equals(manifest.SourceDisjointGroup, SourceDisjointGroup, StringComparison.Ordinal) &&
           string.Equals(manifest.LicenseOrProjectInternalRef, LicenseOrProjectInternalRef, StringComparison.Ordinal) &&
           string.Equals(manifest.LegalVerdict, LegalVerdict, StringComparison.Ordinal) &&
           manifest.CreatedAtUtc != default &&
           TranslationModelManifestContract.IsSafeRelativeRef(manifest.CorpusFilename) &&
           TranslationModelManifestContract.IsSafeRelativeRef(manifest.CorpusRef) &&
           TranslationModelManifestContract.IsSafeRelativeRef(manifest.ProvenanceRef);

    private static bool IsSha256Hex(string value)
        => value is { Length: 64 } &&
           value.All(ch => ch is >= '0' and <= '9' or >= 'a' and <= 'f');
}
