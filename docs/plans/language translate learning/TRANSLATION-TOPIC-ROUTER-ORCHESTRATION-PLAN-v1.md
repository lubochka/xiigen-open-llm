# Translation Topic Router Orchestration Plan
# Version: 1.0 - 2026-05-11
# Domain: Small Translation Models with Learnable Routing
# Source registry: TRANSLATION-SOURCE-REGISTRY-v3.md
# Parallel to: EXAMINATION-METHOD-ORCHESTRATION-v8.md
# Runtime contract: ILogicalFlow / ILogicalOrchestrator, not a hardcoded switch
# DPO guide: TRANSLATION-ROUTING-FLOW-DPO-GUIDANCE-v1.md
# Executor prompt: EXECUTOR-PROMPT-TRANSLATION-50-PAIR-LEARNING-v1.md
# Rejected prompts: TRANSLATION-DPO-REJECTED-PROMPTS-v1.md
# Look-like report: TRANSLATION-LOOK-LIKE-RUN-REPORT-TEMPLATE-v1.md

---

## Purpose

A small translation model should not be forced to learn every language direction,
topic, register, and risk class in one shared adapter. The translation system
needs a learnable routing flow, similar to the software-orchestration flow, that
selects the correct translation adapter before generation.

This router is not a helper function, regex classifier, enum switch, or static
lookup table. It is a trainable logical orchestration surface:

```csharp
TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>
TranslationRoutingOrchestrator : ILogicalOrchestrator
```

Inside the flow, individual decisions may be implemented as `ILogicalUnit`
nodes, but the public training target is the flow/orchestrator behavior.

The routing flow predicts:

1. translation direction
2. topic domain
3. register
4. risk class
5. translation level
6. adapter id

The translator then uses the selected adapter to produce the final text.

---

## Core Principle

Do not train one universal translator first.

Train small specialized adapters and an orchestration flow that learns when to
use each one.
This keeps the model small, debuggable, reversible, and measurable.

The router must be learned from DPO pairs and measured like any other logical
capability. Static rules are allowed only as bootstrapping evidence and safety
guards; they are not the final routing authority.

---

## Logical Flow Contract

The translation router should be shaped like the existing logical orchestration
contracts in project 21:

```csharp
public sealed record TranslationRoutingRequest(
    string SourceText,
    string? SourceLanguageHint,
    string TargetLanguage,
    string? UserDomainHint,
    string? UserRegisterHint,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record TranslationRoutingDecision(
    string Direction,
    string TopicDomain,
    string Register,
    string RiskClass,
    int TranslationLevel,
    string AdapterId,
    double Confidence,
    IReadOnlyList<string> EvidenceTerms,
    IReadOnlyList<string> RejectedAdapters,
    string DecisionReason);

public sealed class TranslationRoutingFlow
    : ILogicalFlow<TranslationRoutingRequest>
{
    public Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<TranslationRoutingRequest> request,
        CancellationToken cancellation = default);
}
```

The flow should be registered and evaluated as a learned capability. The
training target is not only the final `AdapterId`; it is the whole decision
packet: direction, domain, register, risk, confidence, evidence, and rejected
alternative.

### Internal Flow Nodes

Recommended internal nodes:

1. `TranslationRequestCompilerUnit`
   - `ILogicalUnit<TranslationRoutingRequest, TranslationRouterSourceDocument>`
   - extracts script, language hints, domain terms, entity candidates, and risk
     signals.
2. `TranslationDirectionRouterUnit`
   - predicts `RU_EN`, `EN_RU`, `HE_EN`, or `EN_HE`.
3. `TranslationTopicRouterUnit`
   - predicts `general`, `named_entity`, `legal_diplomatic`, `software_ui`,
     `literary`, or `high_stakes_blocked`.
4. `TranslationRegisterRiskRouterUnit`
   - predicts register and risk class.
5. `TranslationAdapterSelectorUnit`
   - selects the adapter id and rejected adjacent adapters.
6. `TranslationRoutingArbiterUnit`
   - validates confidence, source authority, and high-risk blocking.

This makes the translation router trainable in the same way as orchestration:
unit pairs teach local decisions; flow pairs teach the combined routing
behavior.

---

## Router Source Document Contract

Each translation request is compiled into:

```json
{
  "source_text": "...",
  "source_language": "ru",
  "target_language": "en",
  "direction": "RU_EN",
  "detected_script": "Cyrillic",
  "topic_domain": "general",
  "register": "neutral",
  "risk_class": "normal",
  "level_required": 1,
  "named_entities": [],
  "evidence_terms": ["cyrillic", "short_sentence", "general"],
  "router_confidence": 0.0
}
```

The routing flow must never infer a high-risk domain without evidence. If
evidence is weak, choose `general` for normal-risk text or return
`needs_review` for formal/high-risk text.

---

## Topic Domains

### D01 - General Everyday

Everyday short sentences, simple statements, requests, and common phrases.

Allowed clean sources:
- Tatoeba

Adapters:
- `adapter-ru-en-general`
- `adapter-en-ru-general`
- `adapter-he-en-general`
- `adapter-en-he-general`

### D02 - Named Entities and Transliteration

Person names, organizations, locations, products, and titles.

Allowed clean sources:
- ParaNames

Adapters:
- `adapter-ru-en-named-entity`
- `adapter-en-ru-named-entity`
- `adapter-he-en-named-entity`
- `adapter-en-he-named-entity`

### D03 - Formal Diplomatic / Legal-Administrative

UN-style formal documents, resolutions, parliamentary text, administrative
phrasing, long formal clauses.

Allowed clean sources:
- UN Parallel Corpus for RU_EN and EN_RU only

Support-only sources:
- Knesset Hebrew for formal-spoken Hebrew register support; not parallel chosen
  authority

Adapters:
- `adapter-ru-en-legal-diplomatic`
- `adapter-en-ru-legal-diplomatic`

Hebrew formal-domain adapters remain `planned` until a clean HE-EN parallel
source is registered.

### D04 - Software UI and Technical Product Text

Buttons, menus, validation messages, settings, developer-facing UI text, and
short product instructions.

Status:
- REVIEW_REQUIRED

Candidate sources:
- KDE/GNOME/Ubuntu localization strings only after component-level license
  verification

Adapters:
- planned only, not train-ready

### D05 - Literary / Narrative

Prose, dialogue, figurative language, and literary register.

Status:
- REVIEW_REQUIRED_PER_TITLE

Candidate sources:
- OPUS Books only after title-level public-domain or CC-BY filtering

Adapters:
- planned only, not train-ready

### D06 - Medical / Finance / High-Stakes

High-risk domains where translation errors can cause harm or legal exposure.

Status:
- BLOCKED_FOR_FIRST_CURRICULUM

No train-ready batches until exact source authority and review workflow exist.

---

## Routing Labels

Every training row must include:

```json
{
  "direction": "RU_EN",
  "topic_domain": "general",
  "register": "neutral",
  "risk_class": "normal",
  "translation_level": 1,
  "adapter_id": "adapter-ru-en-general"
}
```

Allowed register labels:

- `neutral`
- `colloquial`
- `formal_spoken`
- `formal_written`
- `legal_diplomatic`
- `ui_short`
- `named_entity`

Allowed risk classes:

- `normal`
- `format_sensitive`
- `named_entity_sensitive`
- `formal_high_precision`
- `high_stakes_blocked`

---

## Four-Level Curriculum With Routing

The 4-level plan remains intact, but every level is now split by topic domain.

One learning stream is:

```
direction x topic_domain x level
```

One batch inside that stream is:

```
50 new train pairs + 50 held-out evaluation rows
```

Examples:

```
RU_EN x general x Level 1 / batch 001 x 50
EN_RU x general x Level 1 / batch 001 x 50
RU_EN x legal_diplomatic x Level 2 / batch 001 x 50
HE_EN x named_entity x Level 3 / batch 001 x 50
```

Do not mix topic domains inside one 50-pair batch. The point of the router is to
make topic-specific adapter behavior measurable over many small batches.

---

## Router Flow Training Batches

The routing flow itself is trained separately from the translator.

Each router batch is 50 DPO pairs and targets one routing responsibility. This
is the batch size, not the full router-level dataset size. The
chosen side must include the full `TranslationRoutingDecision`, not only a label.
The rejected side must select a wrong adjacent route and explain the functional
consequence with `[WRONG: ...]`.

### Router Level R1 - Direction Detection

50 rows per direction:

- source text
- expected direction
- wrong rejected direction

Rejected examples:

- Russian source routed to `EN_RU`
- Hebrew source routed to `RU_EN`
- English source routed to a source-English direction when target is missing

### Router Level R2 - Topic Domain Detection

50 rows per topic domain:

- general
- named_entity
- legal_diplomatic for RU/EN
- software_ui only after source promotion

Rejected examples:

- UN formal sentence routed to `general`
- ParaNames entity routed to `general_sentence`
- Tatoeba everyday phrase routed to `legal_diplomatic`

### Router Level R3 - Register and Risk Detection

50 rows per register/risk group:

- colloquial vs formal_written
- formal_spoken vs formal_written
- named_entity_sensitive
- format_sensitive

Rejected examples:

- Knesset formal-spoken support example labeled as formal-written
- named entity translated semantically instead of preserved/transliterated

### Router Level R4 - Adapter Selection

50 rows per adapter family:

- expected adapter id
- rejected adjacent adapter id
- reason why wrong adapter causes translation failure

Rejected examples:

- `adapter-ru-en-general` chosen for UN legal text
- `adapter-he-en-named-entity` chosen for ordinary sentence translation

---

## Translator Training Batches

Translator batches use the routing-flow labels but do not train the routing flow
at the same time.

For each stream batch:

```
50 train pairs
50 held-out evaluation pairs
same direction
same topic_domain
same level
same adapter_id
```

If the routing flow cannot select the right adapter for at least 95% of held-out
requests in a domain across the accepted stream batches, do not train the
translator adapter for the next level.

---

## Measurement

### Router Flow Metrics

Measure after each 50-row router batch:

- direction accuracy
- topic-domain accuracy
- register accuracy
- adapter-id accuracy
- confusion matrix
- false high-stakes rate
- false low-risk rate
- average confidence

Promotion gates:

- direction accuracy >= 99%
- topic-domain accuracy >= 95%
- adapter-id accuracy >= 95%
- false low-risk rate = 0 for high-stakes or formal-high-precision examples

### Translator Metrics

Measure after each 50-pair translator batch:

- DPO reward margin
- DPO accuracy
- BLEU
- chrF
- target-script accuracy
- copied-source rate
- wrong-direction rate
- named-entity preservation
- completeness score
- latency, CPU, RAM, disk

Promotion gates are defined in `TRANSLATION-50-PAIR-ABLATION-RUNBOOK-v1.md`.

---

## Runtime Orchestration

Runtime flow:

```text
Input text
  -> TranslationRequestCompiler
  -> TranslationRoutingOrchestrator
       -> TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>
          -> TranslationDirectionRouterUnit
          -> TranslationTopicRouterUnit
          -> TranslationRegisterRiskRouterUnit
          -> TranslationAdapterSelectorUnit
          -> TranslationRoutingArbiterUnit
  -> TranslationPromptBuilder
  -> Direction/topic adapter
  -> TranslationQualityArbiter
  -> Output or repair request
```

If routing confidence is low:

1. Do not guess a specialized adapter.
2. Use the general adapter only for normal-risk text.
3. For formal/high-risk text, return `needs_review` or request domain
   confirmation.

---

## First Implementation Order

1. Build router labels into the pair schema.
2. Define `TranslationRoutingRequest` and `TranslationRoutingDecision`.
3. Implement `TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>`
   as the public trainable routing contract.
4. Create 50-row router batch for direction detection.
5. Create 50-row router batch for `general` vs `named_entity`.
6. Train `RU_EN general Level 1` translator only after router R1 passes.
7. Add `legal_diplomatic` only for RU_EN/EN_RU after UN source extraction works.
8. Add Hebrew formal adapters only after a clean HE-EN formal parallel source is
   registered.

---

## What Not To Do

- Do not train all directions in one adapter first.
- Do not mix general and legal/diplomatic pairs in one 50-pair translator batch.
- Do not train the routing flow and translator from the same rows in the same run.
- Do not implement routing as only a hardcoded `switch` over language names.
- Do not skip `ILogicalFlow` and call adapter selection directly from the
  translator.
- Do not use OpenSubtitles, CCMatrix, ParaCrawl, or UI localization strings until
  their registry state changes from `REVIEW_REQUIRED`.
- Do not use Knesset monolingual Hebrew as chosen-side translation authority.
- Do not allow a high-stakes text to fall through to a normal-risk adapter.
