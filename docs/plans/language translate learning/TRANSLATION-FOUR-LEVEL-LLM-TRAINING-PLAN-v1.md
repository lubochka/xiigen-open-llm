# Translation Four-Level LLM Training Plan
# Version: 1.0 - 2026-05-11
# Domain: Russian/Hebrew <-> English Translation
# Source registry: TRANSLATION-SOURCE-REGISTRY-v3.md
# Topic router plan: TRANSLATION-TOPIC-ROUTER-ORCHESTRATION-PLAN-v1.md
# Routing-flow DPO guide: TRANSLATION-ROUTING-FLOW-DPO-GUIDANCE-v1.md
# Executor prompt: EXECUTOR-PROMPT-TRANSLATION-50-PAIR-LEARNING-v1.md
# Rejected prompts: TRANSLATION-DPO-REJECTED-PROMPTS-v1.md
# Look-like report: TRANSLATION-LOOK-LIKE-RUN-REPORT-TEMPLATE-v1.md
# Small-model rationale: TRANSLATION-SMALL-MODEL-RATIONALE-v1.md
# Parallel to: ORCHESTRATOR-TEACHING-PLAN-v5.md

---

## Purpose

This plan trains four parallel translation adapters:

- `RU_EN` - Russian to English
- `EN_RU` - English to Russian
- `HE_EN` - Hebrew to English
- `EN_HE` - English to Hebrew

The training shape follows the same discipline as the orchestration teaching
work: one small batch at a time, explicit source authority, explicit rejected
error type, immediate measurement, and no promotion when a batch regresses.

This plan is not a flat translation curriculum. A small model needs a learnable
translation routing flow that selects the right direction/topic/register adapter
before generation. The routing curriculum is defined in
`TRANSLATION-TOPIC-ROUTER-ORCHESTRATION-PLAN-v1.md`.

The routing layer must be implemented as a logical orchestration contract, not a
hardcoded classifier:

```csharp
TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>
TranslationRoutingOrchestrator : ILogicalOrchestrator
```

Individual routing decisions may be internal `ILogicalUnit` nodes, but the
trainable public surface is the routing flow/orchestrator behavior.

The stream shape is:

```
one direction x one topic domain x one translation level
```

The batch size inside each stream is always:

```
50 new training pairs + 50 held-out evaluation rows
```

Fifty pairs is not the total size of a flow or level. It is the incremental
batch size. A stream may require many accepted 50-pair batches before it is
ready to promote to the next translation level.

Use `EXECUTOR-PROMPT-TRANSLATION-50-PAIR-LEARNING-v1.md` as the operational
prompt for each cycle. It intentionally permits only one new 50-pair batch per
run so that every increment is measured before the stream continues or promotes.

---

## The Four Translation Levels

### Level 1 - Direction and Core Sentence Translation

The model learns to obey the direction prompt and produce a clean translation in
the target language.

Primary sources:
- Tatoeba RU-EN / HE-EN

Pair mix per 50:
- 35 T01 core sentence translation
- 5 T08 completeness / no dropped clause
- 5 T09 named entity preservation
- 5 corruption pairs: wrong target language, copied source, or explanation text

Promotion gate:
- Target-language script check >= 98%
- DPO reward margin > 0
- chrF and BLEU improve over the pre-level baseline or remain non-regressive
- No more than 1 copied-source output in 50 held-out translations

### Level 2 - Sentence Meaning, Syntax, and Completeness

The model learns clause order, negation, prepositions, and full sentence
coverage.

Primary sources:
- Tatoeba sentence pairs

Pair mix per 50:
- 20 T01 semantic equivalence
- 10 T08 omission/addition
- 10 T03 syntax-sensitive morphology
- 5 T04 fixed expression / idiom when available
- 5 Type C rejected outputs that look plausible but select the wrong direction,
  register, or completeness rule

Promotion gate:
- BLEU does not drop by more than 1 point from Level 1
- chrF improves or remains stable
- Human/manual audit of 10 samples finds no systematic dropped-clause pattern
- DPO margin > 0 for both semantic and completeness families

### Level 3 - Morphology, Register, and Named Entities

The model learns the hard language-specific errors:

- Russian case, aspect, agreement, and word order
- Hebrew gender, number, construct state, binyan, final-letter forms, and RTL
  output discipline
- Register preservation
- Proper-name preservation and transliteration

Primary sources:
- Tatoeba for sentence pairs
- ParaNames for T09 named entities
- Knesset Hebrew only as support evidence for formal-spoken register and Hebrew
  morphology; not as chosen-side translation authority

Pair mix per 50:
- 15 T03 morphology
- 10 T02 register
- 10 T09 named entity
- 5 T06 Hebrew RTL/script discipline for `EN_HE` and `HE_EN`; replace with T03
  for Russian directions
- 10 T01/T08 stabilizers so the batch does not become too narrow

Promotion gate:
- Named entity preservation >= 95% on held-out entity probes
- Hebrew output contains Hebrew script and no visual-order corruption
- Russian output passes a basic Cyrillic script check and has no Latin-only
  target output except allowed named entities
- DPO margin > 0 for morphology and named-entity families separately

### Level 4 - Robustness, Round-Trip, and Real-Use Translation

The model learns robustness under realistic prompts while preserving direction,
meaning, and style.

Primary sources:
- Tatoeba held-out sentences
- ParaNames held-out entities
- Optional review-required corpora only after registry promotion; otherwise do
  not use them for train-ready pairs

Pair mix per 50:
- 15 T07 round-trip consistency
- 10 T08 completeness under longer sentences
- 10 T04 idiom/fixed expression
- 10 T02 register/style
- 5 hard negatives: explanation text, wrong direction, source copying, or
  named-entity corruption

Promotion gate:
- Round-trip semantic score >= prior level baseline
- BLEU and chrF do not regress on any direction
- No wrong-direction output in 50 held-out samples
- Manual audit accepts at least 8 of 10 sampled outputs

---

## Stream And Batch Schedule

Each table cell below is a learning stream for the `general` topic domain. Each
stream is trained through repeated 50-pair batches. Additional topic domains add
their own streams only when the source registry has an ACCEPT source for that
topic.

| Level | RU_EN | EN_RU | HE_EN | EN_HE |
|---|---:|---:|---:|---:|
| Level 1 | 50 | 50 | 50 | 50 |
| Level 2 | 50 | 50 | 50 | 50 |
| Level 3 | 50 | 50 | 50 | 50 |
| Level 4 | 50 | 50 | 50 | 50 |

Minimum first pass:

```
4 directions x 1 general topic x 4 levels x 1 batch x 50 pairs = 800 training pairs
```

This is only the first pass, not the total curriculum size. Do not train all 800
at once. Train and evaluate one 50-pair batch at a time. Continue the same stream
with batch 002, 003, etc. until stream-level gates justify promotion.

Topic-specific expansion examples:

```
RU_EN x legal_diplomatic x Level 2 x 50   # UN corpus, ACCEPT
EN_RU x legal_diplomatic x Level 2 x 50   # UN corpus, ACCEPT
HE_EN x named_entity x Level 3 x 50       # ParaNames, ACCEPT
EN_HE x named_entity x Level 3 x 50       # ParaNames, ACCEPT
```

Do not add a topic stream until the translation routing flow has passed the
corresponding router gate.

Recommended order:

1. `RU_EN` Level 1
2. `EN_RU` Level 1
3. `HE_EN` Level 1
4. `EN_HE` Level 1
5. Continue to Level 2 only for directions whose Level 1 gate passed.

---

## Measurement After Every 50-Pair Batch

Every batch writes:

- `translation-batch-summary.json`
- `translation-heldout-evaluation.json`
- `translation-error-ledger.jsonl`
- `translation-sample-review.md`

Required metrics:

| Metric | Meaning |
|---|---|
| DPO reward margin | Chosen translation scored higher than rejected. Must be positive. |
| DPO accuracy | Fraction of pairs with positive margin. |
| BLEU | N-gram overlap against held-out reference. Use as trend, not sole judge. |
| chrF | Character-level score; important for Russian/Hebrew morphology. |
| Target script accuracy | Output uses the expected target script. |
| Copy-source rate | Output copies the source instead of translating. |
| Wrong-direction rate | Output translates into the wrong language. |
| Named-entity preservation | Entities preserved/transliterated correctly. |
| Completeness score | No dropped clause or added unsupported content. |
| Latency/RAM/CPU | Same resource profile style as orchestration runs. |

Minimum held-out shape per batch:

```
50 training pairs
50 held-out evaluation pairs from the same direction and level
10 manual-review samples from the held-out outputs
```

If only 50 clean pairs exist for a direction/level, use them as held-out first
and do not train that stream yet. Translation without held-out measurement is not
a valid run.

---

## Translation Level Score

After each batch, assign a level score:

| Score | Meaning |
|---|---|
| 0 | Failed. Wrong direction, negative margin, or script corruption. |
| 1 | Direction works, but meaning quality is poor. |
| 2 | Basic sentence translation works; frequent morphology/completeness errors. |
| 3 | Usable translation; rare systematic errors remain. |
| 4 | Production-candidate for that direction/level; held-out and manual audit pass. |

The direction's current translation level is the highest level whose gate passed.

Example:

```
RU_EN: Level 2 passed, Level 3 failed => current translation level = 2
EN_RU: Level 1 passed only => current translation level = 1
```

---

## Pair Shape

Each row is JSONL:

```json
{
  "id": "TR-RU_EN-L1-0001",
  "domain": "TRANSLATION",
  "direction": "RU_EN",
  "topic_domain": "general",
  "register": "neutral",
  "risk_class": "normal",
  "level": 1,
  "family": "T01",
  "adapter_id": "adapter-ru-en-general",
  "source_id": "tatoeba-ru-en",
  "source_license_state": "ACCEPT",
  "prompt": "Translate from Russian to English. Produce only the translation.\n\n<source sentence>",
  "a_side": "<source sentence>",
  "chosen": "<verified human translation>",
  "rejected": "<plausible wrong translation>\n[WRONG: mistranslation - specific token or clause error]",
  "compiled_prompt_in_training": false,
  "heldout_group": "train"
}
```

For reverse directions, swap source/target and set `direction` accordingly.

---

## Why Four Separate Adapters

Do not train one mixed adapter first.

Russian and Hebrew have different scripts, morphology, and directionality. The
four directions also have asymmetric difficulty: translating into Russian or
Hebrew requires generating rich morphology, while translating into English often
requires disambiguating source morphology. Separate adapters make regressions
observable and reversible.

The optional mixed adapter is a later Level 4 experiment only after all four
direction adapters pass Level 2.

## Routing Flow Gate Before Translator Training

Before a translator batch is trained, `TranslationRoutingFlow` must prove that
it can select the correct adapter for that stream.

Minimum gates:

- direction accuracy >= 99%
- topic-domain accuracy >= 95%
- adapter-id accuracy >= 95%
- false low-risk rate = 0 on formal/high-risk examples

If the routing flow fails, do not train the translator stream. Add or repair
routing-flow DPO pairs first.

---

## Failure Rules

Stop a batch immediately if any of these happens:

- DPO reward margin is negative after the first checkpoint.
- Wrong-direction rate is above 2%.
- Copy-source rate is above 2%.
- Hebrew output is visual-order corrupted.
- The pair source has no ACCEPT registry entry.
- Chosen and rejected are identical.
- Rejected lacks `[WRONG: ...]`.
- Held-out BLEU/chrF crashes while DPO margin improves; this means the pair
  shape is teaching preference artifacts, not translation.

Failed batches are not deleted. Move them to:

```
language translate learning/translation-runs/rejected/
```

and write a one-page failure note.

---

## First Implementation Tasks

1. Build `TranslationPromptBuilder` with four direction prompts.
2. Build `TranslationRoutingRequest` and `TranslationRoutingDecision`.
3. Build `TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>`.
4. Build `TranslationRoutingOrchestrator : ILogicalOrchestrator` or register the
   flow inside the existing logical orchestrator.
5. Build adapter-id labels.
6. Build a routing-flow DPO builder that emits 50 rows per router level.
7. Build a translation row builder that emits 50 train and 50 held-out rows per
   direction/level.
8. Build a translation DPO runner that reads `prompt/chosen/rejected`, not
   orchestration `SessionLearningRow`.
9. Add BLEU/chrF evaluation.
10. Run router Level R1 direction detection first.
11. Run `RU_EN general Level 1` first with Tatoeba only.
12. Run `EN_RU general Level 1` only after `RU_EN Level 1` has a positive margin and no
   source-copy failure.
