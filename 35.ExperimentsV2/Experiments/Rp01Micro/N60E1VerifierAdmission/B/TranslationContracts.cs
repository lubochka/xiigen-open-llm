// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace N60E1.B;

// This file defines the domain marker and input records for the Translation domain.
// Governed by: PARALLEL-DOMAIN-TRAINING-ROADMAP-v1.md (Direction 1)
//              and locked decision A-LM-04.
// The four direction adapters are: RU_EN, EN_RU, HE_EN, EN_HE.
// Batch discipline: 50 new training pairs + 50 held-out evaluation rows per batch.

// TranslationDomain is the sealed domain marker for the Translation learning machine.
// Used as TDomain in ILearningMachine<TranslationDomain> and LearningBatch<TranslationDomain>.
public sealed class TranslationDomain { }

// TranslationDirection names the four supported translation direction adapters.
// Each direction has its own DPO (Direct Preference Optimization) training adapter.
public enum TranslationDirection
{
    // RU_EN translates Russian source text to English.
    RU_EN,

    // EN_RU translates English source text to Russian.
    EN_RU,

    // HE_EN translates Hebrew source text to English.
    HE_EN,

    // EN_HE translates English source text to Hebrew.
    EN_HE,

    // ES_EN translates Spanish source text to English.
    ES_EN,

    // EN_ES translates English source text to Spanish.
    EN_ES
}

// TranslationRoutingRequest is the input record for one translation request.
// SourceLanguage and TargetLanguage are ISO-639-1 (International Organization
// for Standardization language code) two-letter codes (e.g. "ru", "en", "he").
// TopicDomain describes the subject matter (e.g. "general", "marketing", "technical").
// InputText is the text to be translated.
public sealed record TranslationRoutingRequest(
    // SourceLanguage is the ISO-639-1 code of the language being translated from.
    string SourceLanguage,
    // TargetLanguage is the ISO-639-1 code of the language being translated to.
    string TargetLanguage,
    // TopicDomain is the subject area of the text — used for adapter routing.
    string TopicDomain,
    // InputText is the text that needs to be translated.
    string InputText);

// TranslationRoutingResult is the output record returned by TranslationRoutingFlow.
// It carries the translated text and the direction adapter that was selected.
public sealed record TranslationRoutingResult(
    // Direction is the adapter that processed this request (RU_EN, EN_RU, HE_EN, or EN_HE).
    TranslationDirection Direction,
    // TranslatedText is the model-produced translation of InputText.
    string TranslatedText,
    // Confidence is the model's estimated confidence for this translation (0.0–1.0).
    double Confidence,
    // AdapterId is the string identifier of the direction adapter that was used.
    string AdapterId);
