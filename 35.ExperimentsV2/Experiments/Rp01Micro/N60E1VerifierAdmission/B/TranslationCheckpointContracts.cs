// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace N60E1.B;

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

// This file defines all durable checkpoint DTOs for TranslationMachine.
// A checkpoint is a complete snapshot of all trained unit state, written as UTF-8 JSON.
// Format contract: schema_version = 1; snake_case JSON; enum values as strings; no git commit.
// Private default location: %USERPROFILE%\Documents\xiigen\private-corpora\translation-checkpoints\
// Governed by: TRANSLATION-MACHINE-CHECKPOINT-REPAIR-PLAN-v1.md, OP-39.

// ── Top-level machine checkpoint ──────────────────────────────────────────────

// TranslationMachineCheckpoint is the root record written to disk by SaveCheckpointAsync.
// It contains all trained unit state needed to restore the machine after a process restart.
// No replay or retraining is needed when a valid checkpoint file is present.
public sealed record TranslationMachineCheckpoint(
    // SchemaVersion identifies the checkpoint format. Must equal 1 to be loaded.
    int SchemaVersion,
    // MachineId must match TranslationMachine.MachineId ("translation-machine") to load.
    string MachineId,
    // DomainTag must match TranslationMachine.DomainTag ("TranslationDomain") to load.
    string DomainTag,
    // Version is the model version string at the time of export.
    string Version,
    // AccuracyAfterLastBatch is the last scalar accuracy measured before export.
    double AccuracyAfterLastBatch,
    // HasStructuredTraining is true when at least one structured TranslationLearningBatch was applied.
    bool HasStructuredTraining,
    // ExportedAtUtc is the wall-clock timestamp when this file was written.
    DateTimeOffset ExportedAtUtc,
    // KnnStore holds every trained (signature → source/target) pair.
    TranslationKnnStoreCheckpoint KnnStore,
    // SemanticTokenizer holds all trained pattern and signature corrections.
    SemanticTokenizerUnitCheckpoint SemanticTokenizer,
    // Executors holds one entry per trained translation direction.
    IReadOnlyList<ExecutorUnitCheckpoint> Executors,
    // QualityVerifier holds all trained concept and term rules.
    QualityVerifierUnitCheckpoint QualityVerifier,
    // TrainingHistory holds every batch result in the order it was applied.
    IReadOnlyList<TranslationTrainingHistoryEntryCheckpoint> TrainingHistory,
    // StructuredTrainingIdentities holds every structured example identity trained into this checkpoint.
    IReadOnlyList<TranslationStructuredTrainingIdentityCheckpoint>? StructuredTrainingIdentities,
    // TatoebaLoop holds the runner cursor and scan history, or null when not produced by the loop runner.
    TatoebaPilotLoopCheckpoint? TatoebaLoop);

// ── KNN (K-Nearest Neighbour) store ──────────────────────────────────────────

// TranslationKnnStoreCheckpoint holds all signature-bucketed translation pairs in a flat list.
public sealed record TranslationKnnStoreCheckpoint(
    // Entries is the deterministically sorted flat list of all stored signature/source/target triples.
    IReadOnlyList<TranslationKnnStoreEntryCheckpoint> Entries);

// TranslationKnnStoreEntryCheckpoint is one (signature, source, target) triple stored in the KNN store.
public sealed record TranslationKnnStoreEntryCheckpoint(
    // Signature is the structural KNN lookup key (e.g. "IMPERATIVE_V|ADD_OP|VALIDATE_CHECK").
    string Signature,
    // Source is the source-language text for this training pair.
    string Source,
    // Target is the trained target-language translation for this pair.
    string Target);

// ── SemanticTokenizerUnit ─────────────────────────────────────────────────────

// SemanticTokenizerUnitCheckpoint holds all trained tokenizer overrides.
public sealed record SemanticTokenizerUnitCheckpoint(
    // PatternCorrections holds trained signature → sentence-pattern overrides.
    IReadOnlyList<SemanticTokenizerPatternCorrectionCheckpoint> PatternCorrections,
    // SignatureCorrections holds trained normalized-raw-text → full TokenizerOutput overrides.
    IReadOnlyList<SemanticTokenizerSignatureCorrectionCheckpoint> SignatureCorrections);

// SemanticTokenizerPatternCorrectionCheckpoint is one trained pattern override entry.
public sealed record SemanticTokenizerPatternCorrectionCheckpoint(
    // Signature is the structural key that this pattern correction applies to.
    string Signature,
    // Pattern is the trained (corrected) sentence pattern.
    [property: JsonConverter(typeof(JsonStringEnumConverter))] SentencePattern Pattern);

// SemanticTokenizerSignatureCorrectionCheckpoint is one trained exact-input override entry.
public sealed record SemanticTokenizerSignatureCorrectionCheckpoint(
    // NormalizedRawText is the normalized input text used as the dictionary key.
    string NormalizedRawText,
    // Pattern is the trained sentence pattern for this input.
    [property: JsonConverter(typeof(JsonStringEnumConverter))] SentencePattern Pattern,
    // TokenizerSignature is the full structural signature from the trained TokenizerOutput.
    string TokenizerSignature);

// ── ExecutorUnit ──────────────────────────────────────────────────────────────

// ExecutorUnitCheckpoint holds all trained state for one translation direction.
public sealed record ExecutorUnitCheckpoint(
    // Direction is the translation direction this executor handles (e.g. RU_EN, EN_RU).
    [property: JsonConverter(typeof(JsonStringEnumConverter))] TranslationDirection Direction,
    // WordMap holds the contextual (lemma + concept → target word) trained mappings.
    IReadOnlyList<ExecutorWordMapEntryCheckpoint> WordMap,
    // SurfaceWordMap holds surface-form (word → target) trained mappings.
    IReadOnlyList<ExecutorStringMapEntryCheckpoint> SurfaceWordMap,
    // PhraseMap holds trained multi-word phrase mappings (source phrase → target phrase).
    IReadOnlyList<ExecutorStringMapEntryCheckpoint> PhraseMap,
    // ExactSourceMap holds trained exact-source → exact-target sentence mappings.
    IReadOnlyList<ExecutorStringMapEntryCheckpoint> ExactSourceMap);

// ExecutorWordMapEntryCheckpoint is one entry from the contextual word map.
public sealed record ExecutorWordMapEntryCheckpoint(
    // Lemma is the normalized base form of the source word used as part of the lookup key.
    string Lemma,
    // Concept is the semantic domain concept that forms the second part of the lookup key.
    [property: JsonConverter(typeof(JsonStringEnumConverter))] SemanticConcept Concept,
    // Target is the trained target-language word for this (lemma, concept) pair.
    string Target);

// ExecutorStringMapEntryCheckpoint is one entry from any string-keyed dictionary.
public sealed record ExecutorStringMapEntryCheckpoint(
    // Key is the normalized source string.
    string Key,
    // Value is the trained target string.
    string Value);

// ── QualityVerifierUnit ───────────────────────────────────────────────────────

// QualityVerifierUnitCheckpoint holds all trained verifier rules.
public sealed record QualityVerifierUnitCheckpoint(
    // ConceptRules holds trained (source concept, context concept) → required word rules.
    IReadOnlyList<QualityVerifierConceptRuleCheckpoint> ConceptRules,
    // RequiredTermsBySignature holds per-structural-signature required-term lists.
    IReadOnlyList<QualityVerifierStringListCheckpoint> RequiredTermsBySignature,
    // RequiredTermsBySourceMarker holds per-source-marker required-term lists.
    IReadOnlyList<QualityVerifierStringListCheckpoint> RequiredTermsBySourceMarker);

// QualityVerifierConceptRuleCheckpoint is one trained concept-pair constraint.
public sealed record QualityVerifierConceptRuleCheckpoint(
    // SourceConcept is the semantic concept found in the source sentence.
    [property: JsonConverter(typeof(JsonStringEnumConverter))] SemanticConcept SourceConcept,
    // ContextConcept is the co-occurring context concept that activates this rule.
    [property: JsonConverter(typeof(JsonStringEnumConverter))] SemanticConcept ContextConcept,
    // Required is the word that must appear in the candidate translation when this rule fires.
    string Required);

// QualityVerifierStringListCheckpoint is one (key → list-of-terms) entry.
public sealed record QualityVerifierStringListCheckpoint(
    // Key is the structural signature or source marker that these required terms apply to.
    string Key,
    // Values is the list of required terms for this key.
    IReadOnlyList<string> Values);

// ── Training history ──────────────────────────────────────────────────────────

// TranslationTrainingHistoryEntryCheckpoint is one batch result stored in training order.
public sealed record TranslationTrainingHistoryEntryCheckpoint(
    // BatchId is the identifier assigned to the training batch.
    string BatchId,
    // RowsTrained is the number of training rows processed in this batch.
    int RowsTrained,
    // AccuracyBefore is the measured accuracy before this batch was applied.
    double AccuracyBefore,
    // AccuracyAfter is the measured accuracy after this batch was applied.
    double AccuracyAfter,
    // RewardMargin is the DPO (Direct Preference Optimisation) reward margin for this batch.
    double RewardMargin,
    // CpuMs is the CPU time consumed by this batch in milliseconds.
    long CpuMs,
    // PeakRamMb is the peak RAM usage during this batch in megabytes.
    long PeakRamMb,
    // ElapsedMs is the wall-clock time for this batch in milliseconds.
    long ElapsedMs,
    // PromotedUnitIds lists the unit IDs whose checkpoints were promoted after this batch.
    IReadOnlyList<string> PromotedUnitIds);

// TranslationStructuredTrainingIdentityCheckpoint is one trained structured example identity.
public sealed record TranslationStructuredTrainingIdentityCheckpoint(
    // ExampleId is the same identity type as TranslationLearningExample.ExampleId and corpus pair_id.
    string ExampleId,
    // SourceId is the source identity carried by TranslationLearningExample.SourceId.
    string SourceId,
    // UnitId identifies the trainable unit that consumed this example.
    string UnitId,
    // BatchId identifies the structured batch that introduced this example.
    string BatchId);

// ── Tatoeba loop runner cursor ────────────────────────────────────────────────

// TatoebaPilotLoopCheckpoint holds the runner cursor and scan history for checkpoint-resume.
// Present only when the checkpoint was produced by TatoebaPilotLoopRunner.
public sealed record TatoebaPilotLoopCheckpoint(
    // PrivatePairsPath is the path to the private pairs JSONL file used for this training run.
    string PrivatePairsPath,
    // TrainSetSize is the number of rows in each train set (typically 100).
    int TrainSetSize,
    // TestSetSize is the number of rows in each test set (typically 100).
    int TestSetSize,
    // LastCompletedCycle is the absolute visible cycle number last evaluated and persisted (0 means none).
    int LastCompletedCycle,
    // Cursor is the total number of source-file rows already scanned; resume skips ahead to this offset.
    int Cursor,
    // TrainRows is the total number of source rows used for training so far.
    int TrainRows,
    // BestRuntimeProductAccuracy is the best runtimeProductAccuracy observed by this loop.
    // It may be absent in older checkpoints; runners must fall back to their safe default.
    double? BestRuntimeProductAccuracy,
    // AcceptedTrainingRows lists lightweight references for every pair promoted to training.
    IReadOnlyList<TatoebaPilotPairRefCheckpoint> AcceptedTrainingRows,
    // SkippedRows lists lightweight references for pairs scanned but not promoted.
    IReadOnlyList<TatoebaPilotPairRefCheckpoint> SkippedRows);

// TatoebaPilotPairRefCheckpoint is a lightweight reference to one pair from the source file.
public sealed record TatoebaPilotPairRefCheckpoint(
    // Direction is the translation direction string (e.g. "RU_EN").
    string Direction,
    // SourceSentenceId is the numeric ID of the source sentence in the pairs file.
    int SourceSentenceId,
    // TargetSentenceId is the numeric ID of the target sentence in the pairs file.
    int TargetSentenceId,
    // SourcePattern is the sentence-pattern label from tokenization of this pair.
    string SourcePattern);

// TranslationMachineLoadResult carries the restored machine and the runner loop cursor.
// TatoebaLoop is null when the checkpoint was not produced by TatoebaPilotLoopRunner.
// This record is returned by TranslationMachine.LoadCheckpointAsync so the runner can
// restore its cursor and lastCompletedCycle without a second disk read.
public sealed record TranslationMachineLoadResult(
    // Machine is the fully restored TranslationMachine ready for inference and continued training.
    TranslationMachine Machine,
    // TatoebaLoop holds the saved runner cursor and history, or null when absent.
    TatoebaPilotLoopCheckpoint? TatoebaLoop);

// ── Shared JSON options ───────────────────────────────────────────────────────

// TranslationCheckpointJsonOptions provides the canonical JsonSerializerOptions for checkpoint I/O.
public static class TranslationCheckpointJsonOptions
{
    // Standard is the shared options instance used by all checkpoint read and write operations.
    // WriteIndented: produces human-readable JSON.
    // SnakeCaseLower: matches the naming convention used in runner reports.
    // UnsafeRelaxedJsonEscaping: Cyrillic and Hebrew characters are written as-is, not as \uXXXX.
    // JsonStringEnumConverter: enum values are stored as readable name strings, not integers.
    public static readonly JsonSerializerOptions Standard = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };
}
