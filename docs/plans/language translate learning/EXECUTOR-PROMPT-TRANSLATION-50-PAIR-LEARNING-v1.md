# Executor Prompt - Translation 50-Pair Learning Cycle
# Version: 1.0 - 2026-05-11
# Domain: Russian/Hebrew <-> English Translation
# Runtime contract: TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>
# Purpose: Teach exactly one new 50-pair batch inside an ongoing learning stream, measure translation ability, and decide the next batch.

---

## Copy-Paste Prompt For Executor

You are executing one translation learning cycle.

First read:

```text
language translate learning/TRANSLATION-EXECUTOR-HARD-RESET-v1.md
```

Do not train a broad mixed translation set.
Do not train all directions at once.
Do not bypass the routing flow.
Do not implement product/runtime code unless the user explicitly asks for code
implementation.
Do not report static validation, file creation, or unit-test pass rate as
translation or DPO accuracy.

Your job is to build, train, evaluate, and record exactly one new 50-pair
translation learning batch inside an ongoing stream, then decide the next batch.

Important: 50 pairs is the batch size, not the total flow size. A direction,
topic, router level, or translation level may require many accepted 50-pair
batches before it is considered learned. This matches the orchestration training
style: small measured increments, cumulative learning, no broad unmeasured
merge.

---

## Required Document Use Protocol

Do not skim the documents as a flat folder. Use them in this exact order and
write `DOCUMENT-USE-LOG.md` in the run directory showing which rule you took
from each document.

### Step 0 - Restore Prior State

Open the current translation run directory:

```text
language translate learning/translation-runs/
```

Use it to answer:

- What stream is currently active?
- What was the latest accepted batch?
- Did the latest `NEXT-BATCH-DECISION.md` say `continue_same_level`,
  `promote_to_next_level`, `repair_same_batch`, or `stop_stream`?
- What were the last accepted CPU, RAM, DPO, BLEU, chrF, copied-source, and
  wrong-direction metrics?

If no prior run exists, start with `router-r1-direction/batch-001`.

### Step 1 - Read The Rationale, But Do Not Use It As A Gate

Read:

```text
language translate learning/TRANSLATION-SMALL-MODEL-RATIONALE-v1.md
```

Use it to understand why the work is split into routing flow, adapters, streams,
and 50-pair batches.

Do not use it to override source gates, DPO gates, or evaluation gates. It is
motivation, not authority for training acceptance.

### Step 2 - Read The Source Registry Before Any Pair Work

Read:

```text
language translate learning/TRANSLATION-SOURCE-REGISTRY-v3.md
```

Use it to decide:

- whether the source is `ACCEPT`, `ACCEPT_SUPPORT_ONLY`, `REVIEW_REQUIRED`,
  `QUARANTINE`, or `BLOCK`
- whether the source can produce chosen-side translation authority
- whether attribution is required
- whether the source is allowed only for support, routing, or evaluation

If the exact source/purpose is not `ACCEPT`, do not use that source as
chosen-side authority. Write a source-registration-required note, continue with
registered ACCEPT rows if enough remain, and otherwise write a partial source
ledger plus the exact missing registration fields. Do not fall back to v1 or v2
registries.

### Step 3 - Read The Routing Architecture

Read:

```text
language translate learning/TRANSLATION-TOPIC-ROUTER-ORCHESTRATION-PLAN-v1.md
```

Use it to decide:

- whether this batch is router or translator work
- the stream id
- the direction/topic/register/risk/adapter labels
- whether `TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>` gate
  must run first
- which topic domains are train-ready and which are planned/review-only

Do not implement routing as a static switch, filename classifier, or direct
adapter selection.

### Step 4 - Read Router DPO Guidance If This Is A Router Batch

Read:

```text
language translate learning/TRANSLATION-ROUTING-FLOW-DPO-GUIDANCE-v1.md
```

Use it only for router batches. Extract:

- router level: R1, R2, R3, or R4
- full `TranslationRoutingDecision` chosen shape
- allowed adjacent rejected route types
- router promotion gates

If this is a translator batch, use this document only to verify that the
routing-flow gate has already passed for the stream.

### Step 5 - Read The Four-Level Training Plan

Read:

```text
language translate learning/TRANSLATION-FOUR-LEVEL-LLM-TRAINING-PLAN-v1.md
```

Use it to decide:

- current translation level
- level-specific pair mix
- level-specific metrics
- whether the stream should continue at the same level or is eligible for
  promotion

Remember: one level is not 50 pairs. One level is a stream trained through many
50-pair batches until stream gates pass.

### Step 6 - Read The 50-Pair Batch Runbook

Read:

```text
language translate learning/TRANSLATION-50-PAIR-ABLATION-RUNBOOK-v1.md
```

Use it to execute the batch mechanics:

- output directory format
- source inventory
- 50 train / 50 held-out split
- pair JSON shape
- hard gates
- batch acceptance
- stream-state update
- next-batch decision

This is the operational batch procedure. Do not replace it with older
`TRANSLATION-RUNBOOK-v1.md` instructions.

### Step 7 - Read The DPO Registry Guide

Read:

```text
language translate learning/TRANSLATION-DPO-REGISTRY-GUIDE-v1.md
```

Use it to select the correct translation pair family:

- T01 core sentence translation
- T02 register preservation
- T03 morphology
- T04 idiom/fixed expression
- T05 false cognate
- T06 Hebrew RTL/script
- T07 back-translation
- T08 completeness
- T09 named entity

Do not invent new pair families during a batch. If the row does not fit, route it
to repair/review and continue.

### Step 8 - Read The Examination Method

Read:

```text
language translate learning/EXAMINATION-METHOD-TRANSLATION-v1.md
```

Use it row-by-row to qualify each source entry:

- source registry decision
- script/encoding
- language verification
- length/context suitability
- pair-family assignment
- train-ready vs repair-required decision

Do not use rows that fail examination to fill the 50-pair quota.

### Step 9 - Read The Rejected-Side Prompt Library

Read:

```text
language translate learning/TRANSLATION-DPO-REJECTED-PROMPTS-v1.md
```

Use it to create, audit, and repair rejected sides. A rejected side must be:

- plausible
- specifically wrong
- one primary error type
- annotated with `[WRONG: ...]`
- clear about functional consequence

Reject random bad text, identical chosen/rejected, and generic `[WRONG: bad]`
annotations.

### Step 10 - Read Tokenizer Guidance Before Training

Read:

```text
language translate learning/TRANSLATION-TOKENIZER-GUIDE-v1.md
```

Use it to verify:

- Cyrillic coverage
- Hebrew coverage
- Latin coverage
- context length viability
- script-specific failure modes

If tokenizer coverage blocks the stream, stop with `TOKENIZER_BLOCKED`.

### Step 11 - Read The Look-Like Report Template Before Evaluation

Read:

```text
language translate learning/TRANSLATION-LOOK-LIKE-RUN-REPORT-TEMPLATE-v1.md
```

Use it to write `LOOK-LIKE-RUN-REPORT.md` after evaluation. The report must show:

- CPU
- RAM
- elapsed time
- DPO reward margin and accuracy
- BLEU
- chrF
- target-script accuracy
- copied-source rate
- wrong-direction rate
- named-entity preservation
- completeness
- human-readable verdict

### Step 12 - Historical Documents Are Evidence Only

These files may be read for context but must not override current execution:

```text
language translate learning/TRANSLATION-SOURCE-REGISTRY-v1.md
language translate learning/TRANSLATION-SOURCE-REGISTRY-v2.md
language translate learning/TRANSLATION-TRAINING-PLAN-v1.md
language translate learning/TRANSLATION-RUNBOOK-v1.md
```

If they conflict with v3 registry, four-level plan, or executor prompt, the
current documents win.

---

## Non-Negotiable Architecture Rule

The router is a learnable orchestration structure.

It must be represented as:

```csharp
TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>
```

and optionally coordinated by:

```csharp
TranslationRoutingOrchestrator : ILogicalOrchestrator
```

Do not implement routing as only:

- a hardcoded switch
- a filename classifier
- a language-code lookup
- a static source-registry shortcut
- direct adapter selection from the translator

Internal routing responsibilities may be `ILogicalUnit` nodes, but the measured
training target is the full routing-flow decision.

---

## Current Cycle Input

At the start of the run, declare exactly one target stream and one batch index:

```json
{
  "cycle_kind": "router_or_translator",
  "stream_id": "ru-en-general-l1",
  "batch_index": 1,
  "router_level": "R1|R2|R3|R4|null",
  "direction": "RU_EN|EN_RU|HE_EN|EN_HE",
  "topic_domain": "general|named_entity|legal_diplomatic|software_ui|literary|high_stakes_blocked",
  "translation_level": 1,
  "adapter_id": "adapter-...",
  "pair_count": 50,
  "heldout_count": 50,
  "source_id": "...",
  "source_license_state": "ACCEPT"
}
```

If this is the first run, use:

```json
{
  "cycle_kind": "router",
  "stream_id": "router-r1-direction",
  "batch_index": 1,
  "router_level": "R1",
  "direction": "mixed_by_design_for_direction_detection",
  "topic_domain": "general",
  "translation_level": 0,
  "adapter_id": "routing-flow",
  "pair_count": 50,
  "heldout_count": 50,
  "source_id": "tatoeba + paranames where needed",
  "source_license_state": "ACCEPT"
}
```

---

## Learning Stream vs Batch

A learning stream is the long-running target:

```text
router R1 direction detection
RU_EN x general x Level 1
EN_HE x named_entity x Level 3
RU_EN x legal_diplomatic x Level 2
```

A batch is one incremental training step inside that stream:

```text
50 new train pairs + 50 held-out evaluation rows
```

Do not close the stream after the first accepted batch. Continue with
`batch_index + 1` until the stream-level gate is satisfied or the runbook says to
move to the next difficulty level.

---

## Source Gate

Use only sources whose registry state is `ACCEPT` for the exact purpose.

Allowed first-curriculum sources:

- Tatoeba for general sentence translation
- ParaNames for named-entity and transliteration routing/pairs
- UN Parallel Corpus for RU_EN and EN_RU legal_diplomatic only
- Knesset Hebrew support-only for register/morphology evidence, never as
  chosen-side parallel translation authority

Do not use:

- OpenSubtitles
- CCMatrix
- ParaCrawl
- Stack Exchange
- MIT OCW
- Wikipedia/WikiMatrix except quarantine/eval-only paths
- any source marked `REVIEW_REQUIRED`, `QUARANTINE`, or `BLOCK`

If a source is not registered, do not use it as chosen-side authority. Write a
source-registration-required note, continue with registered ACCEPT rows if
enough remain, and otherwise write a partial source ledger plus the exact
missing registration fields.

---

## Difficulty Ladder

Move from simple to complex only after gates pass.

### Router Ladder

1. R1 - direction detection
2. R2 - topic domain detection
3. R3 - register and risk detection
4. R4 - adapter selection

### Translator Ladder

1. Level 1 - direction obedience and core sentence translation
2. Level 2 - meaning, syntax, and completeness
3. Level 3 - morphology, register, named entities, Hebrew RTL
4. Level 4 - robustness, round-trip, longer real-use translation

Do not run translator Level 1 until router R1 passes.
Do not run topic-specific translator streams until router R2 passes for that topic.
Do not run formal/high-risk streams until router R3 risk handling passes.
Do not run adapter-specific translator streams until router R4 passes.

---

## Required Output Directory

Write every run under:

```text
language translate learning/translation-runs/YYYY-MM-DD/<stream-id>/batch-<NNN>/
```

Use this cycle id format:

```text
router-r1-direction/batch-001
router-r2-topic/batch-001
ru-en-general-l1/batch-001
en-ru-general-l1/batch-001
he-en-general-l1/batch-001
en-he-general-l1/batch-001
```

Required files:

```text
source-inventory.jsonl
translation-router-train-50.jsonl        # router cycles only
translation-router-heldout-50.jsonl      # router cycles only
translation-train-50.jsonl               # translator cycles only
translation-heldout-50.jsonl             # translator cycles only
training-pairs-repair-required.jsonl
translation-batch-summary.json
translation-heldout-evaluation.json
translation-error-ledger.jsonl
translation-sample-review.md
DOCUMENT-USE-LOG.md
STREAM-STATE.json
NEXT-BATCH-DECISION.md
LOOK-LIKE-RUN-REPORT.md
```

---

## Pair Construction Rules

Before constructing pairs, write `DOCUMENT-USE-LOG.md` with this structure:

```markdown
# Document Use Log

## Step 0 - Prior State
- run directory inspected:
- latest stream:
- latest accepted batch:
- decision source file:

## Step 1 - Rationale
- document:
- used for:
- not used for:

## Step 2 - Source Registry
- document:
- source_id:
- source decision:
- attribution required:
- chosen-side authority allowed: yes | no

## Step 3 - Routing Architecture
- document:
- stream_id:
- cycle_kind:
- required routing gate:

## Step 4 - Router DPO Guidance
- document:
- used: yes | no
- router level:

## Step 5 - Four-Level Training Plan
- document:
- translation level:
- level pair mix:
- level promotion rule:

## Step 6 - 50-Pair Runbook
- document:
- output directory:
- train count target:
- held-out count target:

## Step 7 - DPO Registry
- document:
- pair families used:

## Step 8 - Examination Method
- document:
- row qualification rules applied:

## Step 9 - Rejected-Side Prompts
- document:
- rejected audit method:

## Step 10 - Tokenizer Guide
- document:
- tokenizer status:

## Step 11 - Look-Like Report Template
- document:
- report file:

## Historical Documents
- documents read:
- conflicts found:
- current document that wins:
```

Do not proceed to training if `DOCUMENT-USE-LOG.md` is missing.

### Router Cycle Pair

For router cycles, every chosen side must be a full
`TranslationRoutingDecision`, not only a label:

```json
{
  "direction": "RU_EN",
  "topic_domain": "general",
  "register": "neutral",
  "risk_class": "normal",
  "translation_level": 1,
  "adapter_id": "adapter-ru-en-general",
  "confidence": 0.94,
  "evidence_terms": ["cyrillic", "target_language=en"],
  "rejected_adapters": ["adapter-en-ru-general"],
  "decision_reason": "Russian source with English target routes to RU_EN."
}
```

The rejected side must be a plausible wrong route and must include:

```text
[WRONG: specific reason - functional consequence]
```

### Translator Cycle Pair

For translator cycles, every pair must include:

```json
{
  "direction": "RU_EN",
  "topic_domain": "general",
  "register": "neutral",
  "risk_class": "normal",
  "level": 1,
  "family": "T01",
  "adapter_id": "adapter-ru-en-general",
  "prompt": "Translate from Russian to English. Produce only the translation.\n\n...",
  "a_side": "...",
  "chosen": "...",
  "rejected": "...\n[WRONG: ...]",
  "compiled_prompt_in_training": false
}
```

Hard gates:

- chosen != rejected
- rejected contains `[WRONG: ...]`
- rejected passes `TRANSLATION-DPO-REJECTED-PROMPTS-v1.md` audit
- no mojibake in prompt, a_side, chosen, rejected
- exact source authority recorded
- exact direction recorded
- exact topic_domain recorded
- exact adapter_id recorded
- no mixed topic domains in one translator batch
- no mixed translation levels in one translator batch

---

## Training And Evaluation

Train exactly 50 new pairs in this batch.

Evaluate on exactly 50 held-out pairs that are disjoint from training pairs.
Held-out rows may be a stable stream-level held-out slice or a fresh held-out
slice for this batch, but they must never overlap with this batch's train rows.

For router cycles, measure:

- direction accuracy
- topic-domain accuracy
- register accuracy
- risk-class accuracy
- adapter-id accuracy
- false low-risk rate
- average confidence
- DPO reward margin
- DPO accuracy
- CPU, RAM, disk, elapsed time

For translator cycles, measure:

- DPO reward margin
- DPO accuracy
- BLEU
- chrF
- target-script accuracy
- copied-source rate
- wrong-direction rate
- named-entity preservation
- completeness score
- latency per request
- CPU, RAM, disk, elapsed time

Record metrics in `translation-heldout-evaluation.json`.

Also write `LOOK-LIKE-RUN-REPORT.md` using
`TRANSLATION-LOOK-LIKE-RUN-REPORT-TEMPLATE-v1.md`. This report must be readable
without opening raw JSON and must include CPU, RAM, elapsed time, DPO metrics,
and translation-quality metrics.

---

## Promotion Gates

### Router Batch Acceptance

Accept this router batch only if:

- direction accuracy >= 99% for R1
- topic-domain accuracy >= 95% for R2
- register/risk accuracy >= 95% for R3
- adapter-id accuracy >= 95% for R4
- false low-risk rate = 0 for formal/high-risk examples
- DPO reward margin > 0
- no hard-gate pair failures

### Translator Batch Acceptance

Accept this translator batch only if:

- DPO reward margin > 0
- DPO accuracy improves or does not regress against previous accepted batch in
  the same stream
- target-script accuracy >= 98%
- copied-source rate <= 2%
- wrong-direction rate <= 2%
- BLEU and chrF improve or remain non-regressive
- manual sample review accepts at least 8 of 10 samples
- no systematic dropped-clause pattern appears

If any batch gate fails, do not move to the next difficulty level.

Accepted batch does not automatically mean level completion. A stream-level
promotion requires cumulative evidence across accepted batches:

- current batch passes
- cumulative stream metrics remain non-regressive
- held-out errors do not show the same systematic failure across batches
- the stream has enough accepted examples for the current level target

If the stream is still improving or still under-covered, the next allowed action
is another 50-pair batch in the same stream.

---

## STREAM-STATE.json Format

At the end of every batch, update:

```json
{
  "stream_id": "ru-en-general-l1",
  "cycle_kind": "translator",
  "direction": "RU_EN",
  "topic_domain": "general",
  "translation_level": 1,
  "accepted_batches": 1,
  "accepted_train_pairs_total": 50,
  "heldout_rows_total": 50,
  "latest_batch_index": 1,
  "latest_verdict": "ACCEPT_BATCH",
  "stream_status": "continue_same_level"
}
```

---

## NEXT-BATCH-DECISION.md Format

At the end of every batch, write:

```markdown
# Next Batch Decision

## Completed Batch

- cycle_kind:
- stream_id:
- batch_index:
- router_level:
- direction:
- topic_domain:
- translation_level:
- adapter_id:
- train_pairs:
- heldout_pairs:
- source_id:

## Metrics

- dpo_reward_margin:
- dpo_accuracy:
- direction_accuracy:
- topic_domain_accuracy:
- adapter_id_accuracy:
- bleu:
- chrf:
- target_script_accuracy:
- copied_source_rate:
- wrong_direction_rate:
- named_entity_preservation:
- completeness_score:
- elapsed_time:
- peak_ram_mb:
- avg_cpu_percent:

## Verdict

ACCEPT_BATCH | REPAIR_BATCH | REJECT_BATCH | SOURCE_BLOCKED | TOKENIZER_BLOCKED

## Reason

Short explanation using metrics and observed errors.

## Stream Status

continue_same_level | promote_to_next_level | repair_same_batch | stop_stream

## Next Allowed Batch

Declare exactly one next batch.
Do not propose a broader batch.
```

---

## Default First Sequence

Run this sequence unless a prior accepted run says otherwise:

1. `router-r1-direction/batch-001`
2. `router-r1-direction/batch-002` if R1 is not stream-complete
3. `router-r2-topic-general-vs-named-entity/batch-001`
4. `router-r2-topic-general-vs-named-entity/batch-002` if R2 is not stream-complete
5. `ru-en-general-l1/batch-001`
6. `ru-en-general-l1/batch-002` if Level 1 is not stream-complete
7. `en-ru-general-l1/batch-001`
8. `he-en-general-l1/batch-001`
9. `en-he-general-l1/batch-001`
10. Continue same stream with new 50-pair batches before promoting to Level 2.

Do not start legal_diplomatic until both RU_EN and EN_RU general Level 1 pass
and router R2/R3 can distinguish formal-high-precision text from general text.

---

## Status Update Format

While running, report status in this compact format:

```text
Cycle: <stream-id>/batch-<NNN>
Stage: source | build_pairs | train | evaluate | decision
Pairs: <built>/50 train, <heldout>/50 held-out
Repair-required: <n>
Elapsed: <hh:mm:ss>
CPU: <avg%>
RAM: <peak MB>
Current metric: <name=value>
Current verdict: pending | accept | repair | reject
```

Do not write a narrative report unless asked. The authoritative record is the
run directory.

---

## Stop Conditions

Stop training/acceptance for the current batch and write
`NEXT-BATCH-DECISION.md` with `REPAIR_BATCH`, `REJECT_BATCH`,
`PAIRSET_READY_TRAINING_PENDING`, or
`PAIRSET_READY_TRAINING_RUNNER_MISSING` if:

- source authority is not ACCEPT
- fewer than 50 clean train pairs are available
- fewer than 50 clean held-out pairs are available
- chosen == rejected
- rejected lacks `[WRONG: ...]`
- mojibake appears in training fields
- DPO reward margin becomes negative
- wrong-direction rate exceeds 2%
- copied-source rate exceeds 2%
- Hebrew output script is corrupted
- router false low-risk rate is non-zero on formal/high-risk probes

Never patch over a failed gate by adding more pairs in the same run. Repair or
reject the batch, then start a new 50-pair batch attempt.

Do not end with "awaiting your direction" after discovering a missing runner,
missing evaluator, missing runtime flow, or unregistered source. Write the
prepared pairset or repair ledger, metrics with `metric_mode: "not_run"`, and
the exact next action contract.
