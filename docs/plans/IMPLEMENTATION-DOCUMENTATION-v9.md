# IMPLEMENTATION DOCUMENTATION - v9

Date: 2026-05-08

This document is a correction delta over `IMPLEMENTATION-DOCUMENTATION-v8.md`.

Do not read v8 alone for current state. v8 is historical context. v9 supersedes the architecture and teaching-data sections.

## Current Architecture Correction

The training system is no longer supposed to live in `21.AgentInfrastructure`.

Current intended boundary:

| Project | Responsibility |
|---|---|
| `00.Core` | shared logical contracts, training contracts, common DTOs |
| `16.AutonomousLearning` | autonomous learning support services |
| `18.CapabilityTraining` | capability trainer implementation, DPO mechanics, checkpoint lifecycle, replay fusion, trainer cache |
| `21.AgentInfrastructure` | orchestration, routing, logical registry use, calling trainer interfaces |
| `22.AgentAlgorithms` | algorithmic helpers used by orchestration and training |

Implementation rule:

`21.AgentInfrastructure` may call `ILogicalCapabilityTrainer`, but must not own DPO training mechanics.

## Current Training Implementation Files

The current training implementation is expected under:

`C:\Projects\llm_mvp_core\18.CapabilityTraining`

Important files include:

- `DefaultLogicalCapabilityTrainer.cs`
- `LogicalExperimentOptimizations.cs`
- `ReplayFusionRanker.cs`
- `TeachingRegistryPersistence.cs`
- `TeachingRegistrySnapshot.cs`
- `CapabilityResultTranslator.cs`
- `LlmIngredientLogicalUnits.cs`

Shared contracts are expected under:

`C:\Projects\llm_mvp_core\00.Core`

## Current Teaching Data Correction

The current internal teaching data shape is wrong for a full DPO teaching run.

The inspected run copy:

`C:\Projects\llm_mvp_core\experiments-v2\XIIGEN-DOCS-BATCH-COMPARISON\run-2026-05-08-093325\rows-200-fixed.jsonl`

is not a DPO pair DTO file.

It is a source-row registry copy with fields like:

- `RowId`
- `UserRequest`
- `Domain`
- `Predicates`
- `TargetSubPhases`

It does not include chosen/rejected pair data or DPO pair metadata.

## Required DPO Pair DTO Shape

Before the next full internal teaching run, the runner should consume records that include at least:

- source row id
- source registry id
- original prompt
- compiled prompt representation
- domain
- segment id when applicable
- sub-phase id
- chosen answer
- rejected answer
- rejected type
- pair kind
- contract signature
- overlap ratio
- annotation quality
- examination method metadata

The exact names should follow the internal learning guides, but the concept is non-negotiable: the trainer needs explicit chosen and rejected examples, not a generated generic negative string.

## Current Forbidden Implementation Pattern

The current training code still contains generic rejected text:

`Do not generate a capability for sub-phase ...`

Known files:

- `C:\Projects\llm_mvp_core\18.CapabilityTraining\DefaultLogicalCapabilityTrainer.cs`
- `C:\Projects\llm_mvp_core\18.CapabilityTraining\LogicalExperimentOptimizations.cs`

This pattern must be removed or isolated behind a diagnostic-only path before the next real internal teaching run.

## Multi-Route Prompt Handling

A source row can target multiple sub-phases and multiple domains.

Correct behavior:

- compile the prompt
- segment it when needed
- attach each segment to its own domain/sub-phase evidence
- author pair DTOs at the segment/sub-phase level

Incorrect behavior:

- assign one global domain to the whole prompt
- explode all target sub-phases into independent pairs without segment provenance
- train on one generic branch result for every target sub-phase

## Source Boundary Correction

The path family `external-sources-training-sets` must not be treated as internal XIIGen knowledge by default.

The user explicitly clarified that:

`external-sources-training-sets/wave-2`

is external and unrelated to internal teaching.

Open question:

which path is the canonical internal source registry for XIIGen internal knowledge?

Until this is answered, do not restart full internal teaching except as an explicitly labeled diagnostic run.

## Runner Behavior To Fix Before Next Full Run

The teaching runner must validate input before training starts.

Minimum validation gates:

1. input file is a DPO pair DTO file, not a source-row registry file
2. every row has chosen and rejected text
3. rejected text is not the generic "Do not generate..." template
4. compiled prompt metadata is present for developer natural-language prompts
5. multi-route prompts either have segment IDs or are rejected from the run
6. source provenance is present
7. pair quality metrics are present

## Current Safe Execution Policy

Allowed:

- inspect docs
- write the DTO builder plan
- implement pair validation gates
- run unit tests
- run small diagnostic conversion checks

Blocked until resolved:

- full XIIGen internal knowledge teaching from current source rows
- treating `rows-200-fixed.jsonl` as canonical input
- training on generic rejected strings

---

## 27.TranslationDomain — Active (added 2026-05-29)

This section covers the translation training domain added after v9. It is a
correction delta over the absence in v9 — v9 had zero coverage of this domain.

### Project Location

`C:\Projects\llm_mvp_core\27.TranslationDomain`

### What It Implements

`TranslationMachine : ILearningMachine<TranslationDomain>` — the top-level
training machine for RU↔EN and HE↔EN translation.

### Key Files

| File | Purpose |
|------|---------|
| `TranslationMachine.cs` | `ILearningMachine<TranslationDomain>` implementation |
| `TranslationBatch029Runner.cs` | Batch runner: 40 train + 10 held-out rows; emits `runtimeProductAccuracy`, `runtimeBootstrapRate`; similarity-contamination guard (`TRAIN_HELDOUT_SIMILARITY_OVERLAP`) |
| `TranslationSemanticTokenizer.cs` | Semantic tokenizer; emits rule tags (tense, gender, agreement, case); UTF-8 Cyrillic methods called first, mojibake fallback |
| `TranslationStructuredBatchBuilder.cs` | Builds three-unit DPO examples per source pair; parses `[RULE:]`, `[AGREEMENT:]`, `[PHRASE:]` metadata |
| `TranslationTrainedUnits.cs` | `SemanticTokenizerUnit`, `ExecutorUnit`, `QualityVerifierUnit`; `FindNearestCascade` Full→Prefix→Structure→BootstrapFallback |
| `TranslationDirectionAdapters.cs` | RU_EN, EN_RU, HE_EN, EN_HE adapters |
| `TranslationRoutingFlow.cs` | `ILogicalFlow<TranslationRoutingRequest>` registered as `translation-routing-flow` |
| `TranslationProductHeldOutRunner.cs` | Product held-out runner: exact/product/bootstrap/script/copy-source/wrong-direction/emoji/named-entity/BLEU/chrF metrics |
| `TranslationCorpusSourcing.cs` | Frequency word-list builder: POS filter, license gate, deduplication by lemma |

### Three Trainable Units

| Unit | Teaches |
|------|---------|
| `SemanticTokenizerUnit` | Correct structural signature (POS bigrams, rule tags) for the input pattern |
| `ExecutorUnit` | Correct word choice and word order; composes from learned phrase/word mappings |
| `QualityVerifierUnit` | Which terms are required for a translation to pass quality check |

### Training Loop

100 source/target sentence-pair sets → 3 unit-level DPO examples each → train → test next 100 never-seen sets → repeat until `runtimeProductAccuracy ≥ 0.95`. See `TRANSLATION-CORPUS-SOURCING-PLAN-v1.md` §13 for the full loop definition.

### Test Status

44 tests passing as of phase 24 (2026-05-29), branch `codex/chat-gpt-5.5`.

### Governance Files (docs/plans/)

- `TRANSLATION-DOMAIN-TRAINING-GUIDE-v1.md` — training guide with TR-HO held-out suite
- `TRANSLATION-UNIT-DPO-PAIR-AUTHORING-GUIDE-v1.md` — DPO pair authoring rules, language rule source protocol
- `TRANSLATION-CORPUS-SOURCING-PLAN-v1.md` — base-block architecture (P0→P1→P2→P3→100+100 loop)
- `GRAMMAR-RULE-DPO-REGISTRY-GUIDE-v1.md` — G01–G10 grammar-rule DPO families
- `LANGUAGE-RULE-SOURCE-REGISTRY-v1.md` — approved linguistic rule sources
