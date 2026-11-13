# Translation 50-Pair Ablation Runbook
# Version: 1.0 - 2026-05-11
# Domain: Russian/Hebrew <-> English Translation
# Training plan: TRANSLATION-FOUR-LEVEL-LLM-TRAINING-PLAN-v1.md
# Topic router plan: TRANSLATION-TOPIC-ROUTER-ORCHESTRATION-PLAN-v1.md
# Routing contract: TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>
# Routing-flow DPO guide: TRANSLATION-ROUTING-FLOW-DPO-GUIDANCE-v1.md
# Executor prompt: EXECUTOR-PROMPT-TRANSLATION-50-PAIR-LEARNING-v1.md
# Executor hard reset: TRANSLATION-EXECUTOR-HARD-RESET-v1.md
# Rejected prompts: TRANSLATION-DPO-REJECTED-PROMPTS-v1.md
# Look-like report: TRANSLATION-LOOK-LIKE-RUN-REPORT-TEMPLATE-v1.md

---

## Purpose

This runbook explains exactly how to build and test one 50-pair translation
batch inside a longer learning stream:

```
one direction x one translation level x one batch index
one topic domain
50 train pairs
50 held-out evaluation pairs
```

The 50-pair count is the batch size. It is not the total number of pairs for the
whole flow, direction, topic, or translation level. A stream usually needs many
accepted 50-pair batches before promotion.

It exists because translation cannot be tested like orchestration routing. A
translation batch must prove that the model learned a better target-language
output, not merely that it routed to the right sub-phase.

For actual execution, use
`EXECUTOR-PROMPT-TRANSLATION-50-PAIR-LEARNING-v1.md`. This runbook is the
reference; the executor prompt is the operational instruction.

---

## Inputs

Required:

- `TRANSLATION-SOURCE-REGISTRY-v3.md`
- `TRANSLATION-FOUR-LEVEL-LLM-TRAINING-PLAN-v1.md`
- a clean source extract from an ACCEPT source
- direction code: `RU_EN`, `EN_RU`, `HE_EN`, or `EN_HE`
- topic domain: `general`, `named_entity`, `legal_diplomatic`, or another
  registry-approved topic
- level: `1`, `2`, `3`, or `4`

Do not use a `REVIEW_REQUIRED`, `QUARANTINE`, or `BLOCK` source as
chosen-side authority. Continue with registered ACCEPT rows if enough remain.
If there are not enough rows, write the partial source ledger and
`SOURCE-REGISTRATION-REQUIRED.md` instead of ending with a question.

---

## Output Directory

Each run writes to:

```
language translate learning/translation-runs/<YYYY-MM-DD>/<direction>/level-<N>/batch-<NNN>/
```

When topic routing is enabled, use:

```
language translate learning/translation-runs/<YYYY-MM-DD>/<direction>/<topic-domain>/level-<N>/batch-<NNN>/
```

Required files:

| File | Purpose |
|---|---|
| `source-inventory.jsonl` | Every candidate sentence/entity before filtering. |
| `translation-train-50.jsonl` | Exactly 50 train-ready DPO pairs. |
| `translation-heldout-50.jsonl` | Exactly 50 held-out examples, disjoint from train. |
| `training-pairs-repair-required.jsonl` | Rows blocked by gates. |
| `translation-batch-summary.json` | DPO and resource metrics. |
| `translation-heldout-evaluation.json` | BLEU/chrF/script/entity/completeness metrics. |
| `translation-error-ledger.jsonl` | Per-output error classification. |
| `translation-sample-review.md` | 10 manually inspected held-out translations. |
| `LOOK-LIKE-RUN-REPORT.md` | Human-readable report with CPU, RAM, timing, and translation quality. |
| `STREAM-STATE.json` | Cumulative state across accepted 50-pair batches in this stream. |
| `NEXT-BATCH-DECISION.md` | One next batch decision, not a whole broad plan. |
| `RUNNER-CONTRACT.md` | Required when training or evaluation cannot run because a runtime component is missing. |

---

## Step 1 - Source Inventory

Read candidate rows and write `source-inventory.jsonl`.

Each inventory row must include:

```json
{
  "source_id": "tatoeba-ru-en",
  "source_license_state": "ACCEPT",
  "source_locator": "sentence-id-or-release-row",
  "source_language": "ru",
  "target_language": "en",
  "source_text": "...",
  "target_text": "...",
  "attribution": "author/sentence id when available",
  "quality_status": "candidate"
}
```

Hard reject before pair construction:

- wrong license state
- missing attribution for CC-BY source
- mojibake
- wrong script
- empty source or target
- source and target identical except named-entity-only rows
- too long for current context length

---

## Step 2 - Split 50 Train / 50 Held-Out

Never select train and held-out from adjacent duplicate rows.

For Tatoeba:

1. Deduplicate by normalized source text.
2. Deduplicate by normalized target text.
3. Sort deterministically by source id.
4. Take alternating rows:
   - even index -> train candidates
   - odd index -> held-out candidates
5. Stop when there are 50 train and 50 held-out rows.

For ParaNames:

1. Deduplicate by entity id.
2. Keep one row per entity per direction.
3. Stratify by entity type: `PER`, `LOC`, `ORG`.
4. Select train/held-out with no shared entity id.

If fewer than 50 held-out rows remain, do not train that batch.

---

## Step 3 - Build DPO Pairs

Every train row becomes:

```json
{
  "id": "TR-RU_EN-L1-0001",
  "prompt": "Translate from Russian to English. Produce only the translation.\n\n...",
  "a_side": "...",
  "chosen": "...",
  "rejected": "...\n[WRONG: specific_error - specific token/clause consequence]",
  "direction": "RU_EN",
  "topic_domain": "general",
  "register": "neutral",
  "risk_class": "normal",
  "level": 1,
  "family": "T01",
  "adapter_id": "adapter-ru-en-general",
  "rejected_type": "mistranslation",
  "compiled_prompt_in_training": false,
  "source_id": "tatoeba-ru-en",
  "source_license_state": "ACCEPT",
  "heldout_group": "train"
}
```

Rejected side rule:

The rejected translation must be plausible and specifically wrong. It must not
be random garbage and must not be nearly identical to chosen.

Preferred rejected types by level:

| Level | Preferred rejected types |
|---|---|
| 1 | wrong direction, copied source, mistranslation |
| 2 | omission, addition, negation error, wrong clause relation |
| 3 | morphology, register shift, named-entity corruption, RTL error |
| 4 | round-trip divergence, idiom calque, completeness under longer sentence |

---

## Step 4 - Train One 50-Pair Batch

Run exactly one 50-pair batch inside one stream:

```
direction=<DIRECTION>
topic_domain=<TOPIC_DOMAIN>
level=<LEVEL>
pairs=50
batch_index=<NNN>
```

Before this step, run the routing-flow gate:

- direction accuracy >= 99%
- topic-domain accuracy >= 95%
- adapter-id accuracy >= 95%
- false low-risk rate = 0 for formal/high-risk examples

If the routing-flow gate fails, stop. Train or repair
`TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>` before
training the translator adapter.

The gate is not satisfied by a hardcoded switch, deterministic filename
classifier, or static language-code lookup. It must be backed by 50-row DPO
router batches and held-out routing evaluation.

The runner must:

- inject the direction prompt before tokenization
- use `compiled_prompt_in_training=false`
- train only the adapter for this direction
- train only the adapter for this topic domain
- save a checkpoint after the 50-pair batch
- write CPU/RAM/disk/timing metrics
- write `LOOK-LIKE-RUN-REPORT.md`

Do not mix directions inside one first-run batch.
Do not mix topic domains inside one translation batch.

---

## Step 5 - Held-Out Translation Evaluation

After training, translate all 50 held-out source texts.

For each held-out item write:

```json
{
  "id": "TR-RU_EN-L1-HO-0001",
  "source_text": "...",
  "reference": "...",
  "model_output": "...",
  "bleu_contribution": 0.0,
  "chrf_contribution": 0.0,
  "script_ok": true,
  "copied_source": false,
  "wrong_direction": false,
  "named_entity_ok": true,
  "completeness_ok": true,
  "error_category": "none"
}
```

Batch-level metrics:

- BLEU
- chrF
- target script accuracy
- copied-source rate
- wrong-direction rate
- named-entity preservation
- completeness pass rate
- average latency per translation
- peak RAM
- CPU time

---

## Step 6 - Batch Decision And Stream Progress

Assign one decision:

| Decision | Meaning |
|---|---|
| `ACCEPT_BATCH` | Accept this 50-pair batch into the stream. |
| `REPAIR_BATCH` | Pair construction is weak; repair rejected sides and retry this batch. |
| `REJECT_BATCH` | Source slice is noisy or unsuitable. |
| `TOKENIZER_BLOCKED` | Tokenizer/script coverage blocks training. |
| `MODEL_CAPACITY_BLOCKED` | Model cannot learn this level with current adapter config. |

Promotion requires:

- DPO reward margin > 0
- DPO accuracy >= 0.60 for the 50-pair batch
- BLEU does not regress from prior checkpoint
- chrF does not regress from prior checkpoint
- target script accuracy >= 98%
- copied-source rate <= 2%
- wrong-direction rate = 0 for Level 1 and <= 2% for Levels 2-4
- 10-sample manual review accepts at least 8 outputs

Do not treat `ACCEPT_BATCH` as level completion. It only means this 50-pair
increment may be added to the cumulative stream. Continue with the next 50-pair
batch until stream-level metrics justify promotion to the next level.

---

## Step 7 - What To Do When It Fails

If DPO margin is negative:

1. Inspect 10 chosen/rejected pairs.
2. If rejected is too plausible or accidentally better, repair the pair.
3. If chosen is weak corpus text, move the row to repair-required.
4. If the whole family fails, reduce the batch to only T01 semantic errors and retry.

If BLEU/chrF drops while DPO improves:

The batch is teaching preference artifacts rather than translation quality.
Reject the batch and rewrite rejected sides.

If wrong-direction outputs appear:

Do not move to Level 2. Add Level 1 hard negatives: copied source, wrong target
language, explanation text, and transliteration-only output.

If Hebrew output is visually corrupted:

Stop `EN_HE` and `HE_EN`. Fix tokenizer and RTL handling before adding more
pairs.

---

## First Run Recommendation

Run in this order:

1. Router R1 direction detection, 50 rows per direction, measured through
   `TranslationRoutingFlow`.
   Continue R1 with additional 50-pair batches until R1 stream gates pass.
2. Router R2 `general` vs `named_entity`, 50 rows per topic, measured through
   `TranslationRoutingFlow`.
   Continue R2 with additional 50-pair batches until R2 stream gates pass.
3. `RU_EN general` Level 1, batch 001, 50 train + 50 held-out, Tatoeba only.
4. Continue `RU_EN general` Level 1 with batch 002/003/etc. until stream gates pass.
5. `EN_RU general` Level 1, batch 001, 50 train + 50 held-out, Tatoeba only.
6. `HE_EN general` Level 1, batch 001, 50 train + 50 held-out, Tatoeba only.
7. `EN_HE general` Level 1, batch 001, 50 train + 50 held-out, Tatoeba only.

Add ParaNames only after Level 1 direction obedience passes, because named
entity rows are useful for T09 but not sufficient to teach sentence translation.
