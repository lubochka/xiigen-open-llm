# Translation Training Plan — One Batch at a Time
# Version: 1.1 — 2026-05-11
# Supersedes: TRANSLATION-TRAINING-PLAN-v1.0
# Domain: Russian/Hebrew ↔ English Translation
# Source registry: TRANSLATION-SOURCE-REGISTRY-v3.md
# Current operational plan: TRANSLATION-FOUR-LEVEL-LLM-TRAINING-PLAN-v1.md
# Current executor prompt: EXECUTOR-PROMPT-TRANSLATION-50-PAIR-LEARNING-v1.md
# Parallel to: ORCHESTRATOR-TEACHING-PLAN-v5.md
#
# v1.1 changes from v1.0:
# - Source registry reference updated to v2.
# - Phase 1 first-run sources corrected (tatoeba + opensubtitles, both OPUS-filtered).
# - Phase 3 adds ParaNames (MIT) for T09 Named Entity pairs.
# - Phase 6 QUARANTINE sources corrected to CCMatrix + opus-books PD/CC-BY titles.
# - Europarl removed — not available for RU–EN.
#
# v1.1 post-review fixes (2026-05-11 review):
# Critical:
#   - Length filter corrected: 256 → 32 (ContextLen/2) after tokenizer extension;
#     15 tokens for HE, 20 for RU before extension (byte-level fallback).
#   - TranslationPromptBuilder class specified; PC-TR-07 added to Phase 0.
# Important:
#   - PC-T07-01 (BLEU ≥ 25 gate) added to Phase 4 pre-conditions.
#   - PC-TR-06 (OpusFilter installed) added to Phase 0 pre-conditions.
# Minor:
#   - LoRA Dropout=0.05 specified explicitly; BuildTranslationLoraConfig() stub added.
#   - Knesset register label: "formal spoken" throughout.
#
# 2026-05-11 current-run correction:
#   - This file is retained as historical design context.
#   - The active training protocol is now stream-based:
#       stream = direction x topic_domain x translation_level
#       batch = 50 new train pairs + 50 held-out rows
#   - 50 pairs is the batch size, not the total flow/level size.
#   - Earlier 16/24/32/64-pair ablation wording is superseded for executor runs.
#   - OpenSubtitles is no longer train-ready; use only sources allowed by
#     TRANSLATION-SOURCE-REGISTRY-v3.md.

---

## What This Plan Is

This plan teaches the translation model direction by direction, family by family,
one standalone ablation at a time. Each ablation batch is small (16–64 pairs),
measured before the next batch is added, and promoted to combo only when reward
margin is positive and held-out BLEU (Bilingual Evaluation Understudy) does not
drop below the prior checkpoint.

This is not a one-shot bulk training plan. Bulk training hides which direction or
error type caused a regression. One-batch-at-a-time teaching makes every change
observable and reversible.

---

## Infrastructure Requirements

The translation domain uses the same core infrastructure as the orchestration
domain (TensorCore, LoRA, DPO trainer, BPE tokenizer) with these additions:

### Multilingual Tokenizer

The BPE tokenizer must be trained (or extended) to cover three scripts:
- Cyrillic (Russian)
- Hebrew script (Unicode U+0590–U+05FF)
- Latin (English)

Minimum vocabulary coverage targets (share of test corpus tokens covered by
the vocabulary without splitting into individual bytes):
- Russian: ≥ 95%
- Hebrew: ≥ 90% (Hebrew has smaller web-corpus size)
- English: ≥ 97%

**Pre-training check:** Before any LoRA translation training, verify tokenizer
coverage. If Hebrew coverage is below 90%, extend the vocabulary — do not
proceed with training on inadequate tokenization.

### LoRA Configuration for Translation

Translation LoRA adapts the same base as the orchestration model but with
separate adapter weights. Do not share LoRA adapters between orchestration
and translation domains.

Recommended starting configuration (adjust based on parameter budget):
- Rank: 4 (double the orchestration rank of 2, because translation requires more
  representational capacity for morphological variation)
- Alpha: 8 (scaling = Alpha/Rank = 2.0, same as orchestration)
- **Dropout: 0.05** (use the `LoraConfig` default — do NOT use 0.0. With only
  32 training pairs per ablation, dropout prevents overfitting to the small
  training set. The capability trainer uses Dropout=0 because it trains on
  larger batches; translation ablations are smaller and need regularisation.)
- Target modules: q_proj, v_proj (attention only for first run; add k_proj if
  morphological pairs do not converge)

Code stub — add to `18.CapabilityTraining` or a new `18.TranslationTraining`
project, following Rule 1 (OperationResult<T>) and Rule 3 (every line commented):

```csharp
private static LoraConfig BuildTranslationLoraConfig() =>
    // Translation requires higher rank than capability training because
    // Russian case endings and Hebrew binyan patterns need more adapter capacity.
    new LoraConfig
    {
        Rank = 4,           // double the capability-training rank of 2
        Alpha = 8,          // scaling factor = Alpha/Rank = 2.0
        Dropout = 0.05f,    // prevent overfitting on small ablation batches
        TargetModules = ["q_proj", "v_proj"],  // attention projections only
    };
```

### System Prompt Injection — TranslationPromptBuilder

`TextGenerator.Generate(string prompt)` takes a raw string. At training time,
the `DpoBatch` token tensors must include the system prompt prepended to the
A-side source sentence, or the model never sees the translation direction during
training. The pair JSON `a_side` stores the raw source sentence only; the builder
wraps it at tokenization time.

Required class — add to `18.CapabilityTraining/Translation/TranslationPromptBuilder.cs`
(or a new `18.TranslationTraining` project):

```csharp
public static class TranslationPromptBuilder
{
    // Direction codes and their corresponding system prompt strings.
    private static readonly Dictionary<string, string> DirectionPrompts = new()
    {
        ["RU_EN"] = "Translate the following Russian text into English. Produce only the translation.",
        ["EN_RU"] = "Translate the following English text into Russian. Produce only the translation.",
        ["HE_EN"] = "Translate the following Hebrew text into English. Produce only the translation.",
        ["EN_HE"] = "Translate the following English text into Hebrew. Produce only the translation.",
    };

    // Builds the full prompt string from direction code and raw source sentence.
    // Returns Fail if the direction code is not registered.
    public static OperationResult<string> Build(string directionCode, string sourceSentence)
    {
        // Validate that the direction code is known before concatenating.
        if (!DirectionPrompts.TryGetValue(directionCode, out var systemPrompt))
            return OperationResult<string>.Fail(
                $"Unknown translation direction: {directionCode}", "UNKNOWN_DIRECTION");
        // Concatenate system prompt and source sentence with a newline separator.
        return OperationResult<string>.Ok($"{systemPrompt}\n\n{sourceSentence}");
    }
}
```

The DPO training loop calls `TranslationPromptBuilder.Build(pair.Direction, pair.ASide)`
and tokenizes the result into `ChosenIds` and `RejectedIds` context prefix.
The `compiled_prompt_in_training: false` flag signals the training harness to use
this path instead of `SemanticTeachingPromptCompiler.Compile()`.

---

### Evaluation Metrics

Measure after every ablation batch:
- **BLEU-4** on held-out FLORES-200 sentences for each direction
- **chrF** (Character F-score) — particularly important for morphologically rich
  languages (Russian) and non-Latin script (Hebrew)
- **DPO reward margin** — same metric as orchestration domain
- **DPO accuracy** — fraction of pairs where margin > 0

Regression gate:
- BLEU on any direction must not drop more than 2 points from the prior checkpoint.
- chrF on any direction must not drop more than 3 points.
- DPO reward margin must remain positive for the ablation batch.

---

## Training Phases

### Phase 0 — Pre-Conditions (must pass before Phase 1)

PC-TR-01: Tokenizer covers ≥ 90% of Hebrew held-out tokens.
PC-TR-02: Tokenizer covers ≥ 95% of Russian held-out tokens.
PC-TR-03: Baseline BLEU established on FLORES-200 for all four directions using
the base model before any translation LoRA is applied.
PC-TR-04: `training-pairs-repair-required.jsonl` is empty for the first batch
(all 64 pairs pass hard gates).
PC-TR-05: All pairs in the first batch have `source_license_state: ACCEPT`.
PC-TR-06: OpusFilter installed and bicleaner-ai score files generated for ru-en
and he-en language pairs before any OpenSubtitles pairs enter the batch. If
OpusFilter is not available before Phase 1 starts, Phase 1 must be documented
as `tatoeba-only` and the batch size drops to 24 pairs (tatoeba coverage only).
Do not use unfiltered OpenSubtitles pairs.
PC-TR-07: `TranslationPromptBuilder.Build()` is implemented and unit-tested for
all four direction codes before any DPO training loop is run. The output string
for each direction must be verified by reading the tokenized prompt back to
confirm the system prompt is present in the token sequence.

Record baseline BLEU and chrF numbers here:

```
Baseline (before any translation LoRA):
  RU→EN BLEU: [TBD]    chrF: [TBD]
  EN→RU BLEU: [TBD]    chrF: [TBD]
  HE→EN BLEU: [TBD]    chrF: [TBD]
  EN→HE BLEU: [TBD]    chrF: [TBD]
```

---

### Phase 1 — Russian↔English Core (Standalone Ablation)

**Source:** `opus-tatoeba-ru-en` (CC-BY 2.0, ACCEPT) + `opus-opensubtitles-ru-en`
(CC-BY 4.0, ACCEPT — OpusFilter bicleaner-ai ≥ 0.5 applied before extraction)

**Batch size:** 32 pairs
- 16 RU→EN (T01 core: 10, T03 morphological: 4, T04 idiom: 2)
- 16 EN→RU (T01 core: 10, T03 morphological: 4, T04 idiom: 2)

**Gate B (must pass before Phase 2):**
- DPO reward margin ≥ 0.05 on this batch
- BLEU regression ≤ 2 points on all directions (including untrained HE directions)
- All 32 pairs scored — no pair has negative margin that cannot be explained by
  a specific error category

**If Gate B fails:**
- Diagnose using the table in DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1.md
- Fix or replace the failing pairs
- Re-run; do not proceed to Phase 2 until Gate B passes

---

### Phase 2 — Hebrew↔English Core (Standalone Ablation)

**Source:** `opus-tatoeba-he-en` (CC-BY 2.0, ACCEPT) + `opus-opensubtitles-he-en`
(CC-BY 4.0, ACCEPT — OpusFilter bicleaner-ai ≥ 0.5 applied).
`heddc4` (Apache 2.0) as monolingual HE base for back-translation quality check.

**Batch size:** 32 pairs
- 16 HE→EN (T01 core: 8, T03 morphological: 5, T06 RTL: 3)
- 16 EN→HE (T01 core: 6, T03 morphological: 6, T06 RTL: 4)

**Pre-condition:** Phase 1 Gate B passed.

**Gate B:**
- BLEU regression ≤ 2 points on RU↔EN (Phase 1 must not regress)
- DPO reward margin ≥ 0.05 on this batch
- Hebrew encoding: zero mojibake in any pair in this batch (encoding verified
  before training, not after)

---

### Phase 3 — Register, Idiom, and Named Entity (Combo Ablation)

**Source:** `opus-opensubtitles-ru-en` / `opus-opensubtitles-he-en` (colloquial),
`opus-tatoeba-ru-en` / `opus-tatoeba-he-en` (mixed),
`knesset-he` (formal parliamentary HE — T02),
`paranames-ru-en` + `paranames-he-en` (MIT — T09 Named Entity)

**Batch size:** 32 pairs
- 8 T02 (Register) across all four directions
- 8 T04 (Idiom) across all four directions
- 8 T05 (False Cognate) RU→EN + HE→EN
- 8 T09 (Named Entity) across all four directions

**Pre-condition:** Phases 1 and 2 Gate B passed.

**Gate B:**
- BLEU regression ≤ 2 points on all directions vs Phase 2 checkpoint
- Named entity pairs: transliteration standard used must match the declared
  scheme in the pair metadata (BGN for RU, LOC for HE)

---

### Phase 4 — Back-Translation Consistency (T07)

**Pre-condition (PC-T07-01):** Phase 2 checkpoint must achieve **BLEU ≥ 25** on
FLORES-200 HE→EN before T07 pair construction begins. If BLEU < 25 after Phase 2,
run additional Phase 2 pairs (up to 64 total, still 16 per ablation step) until
BLEU ≥ 25. Do not attempt back-translation pairs on a checkpoint below this
threshold — the forward translations will be wrong, making chosen back-translations
wrong, and the entire T07 batch will fail Gate 6. BLEU 25 corresponds to roughly
correct sentence structure with some wrong word choices, which is sufficient for
round-trip pairs to carry a meaningful learning signal.

**Source:** Build back-translation pairs from Phase 1 and 2 checkpoints.
Use forward model at Phase 2 checkpoint for one direction, different checkpoint
or decoding temperature for the reverse direction. Do not use the same model
checkpoint for both directions — a single model forward and back collapses the signal.

**Batch size:** 16 pairs
- 8 RU↔EN round-trip
- 8 HE↔EN round-trip

**Gate B:**
- Round-trip pairs must have chosen with BLEU ≥ 60 vs source
- Rejected pairs must have BLEU ≤ 40 vs source (to ensure distributional separation)

---

### Phase 5 — Combo Promotion

After all four phases pass their Gate B:

1. Combine all pairs from Phases 1–4 into a single combo batch (112 pairs total).
2. Run full combo training.
3. Measure BLEU and chrF on FLORES-200 for all four directions.
4. Compare to baseline (Phase 0).

**Combo gate:**
- BLEU improvement ≥ 3 points on at least 3 of 4 directions vs baseline
- No direction regresses > 2 points vs baseline
- DPO reward margin ≥ 0.02 on the combo batch

**If combo fails:** Do not merge. Diagnose which phase's pairs caused regression.
Remove or repair those pairs. Re-run phases individually before attempting combo again.

---

### Phase 6 — Large-Volume and QUARANTINE Source Integration (Optional)

After Phase 5 combo passes, introduce larger and/or QUARANTINE sources:

**Large-volume ACCEPT sources (run first):**
- `ccmatrix-ru-en` (CC-BY) — automatically aligned; spot-check 100 pairs before
  any batch is marked train-ready. Record `filter_score` in every pair.
- `opus-books-ru-en` (PD / CC-BY titles only) — literary register; important
  for T03 Russian morphological complexity. Check per-title licence tag in OPUS
  metadata before ingesting.

**QUARANTINE sources (run second, separately):**
- `wikimatrix` (CC-BY-SA) — encyclopaedic register; `batch_isolation: true`.

Rules for all Phase 6 sources:
- QUARANTINE pairs must have `batch_isolation: true`.
- ACCEPT large-volume pairs must have `filter_score` recorded.
- Run in separate batch files — never mix ACCEPT and QUARANTINE in the same file.
- Gate B: same BLEU/chrF gates as Phase 5, measured on a separate held-out split
  that excludes CCMatrix test sentences.
- QUARANTINE checkpoint metadata: `contains_sharealikes: true`.

---

## Measurement Record Template

After each phase, record:

```
Phase: [1/2/3/4/5/6]
Checkpoint: [filename]
Pairs trained this phase: [N]
Total pairs to date: [N]

DPO reward margin (this batch): [value]
DPO accuracy (this batch): [value]

BLEU after:
  RU→EN: [value]  (delta vs prior: [+/-])
  EN→RU: [value]  (delta vs prior: [+/-])
  HE→EN: [value]  (delta vs prior: [+/-])
  EN→HE: [value]  (delta vs prior: [+/-])

chrF after:
  RU→EN: [value]  (delta vs prior: [+/-])
  EN→RU: [value]  (delta vs prior: [+/-])
  HE→EN: [value]  (delta vs prior: [+/-])
  EN→HE: [value]  (delta vs prior: [+/-])

Gate B result: [PASS / FAIL]
Gate B failure reason (if any): [...]
Next action: [...]
```

---

## Isolation from Orchestration Baseline

**CRITICAL:** Translation pairs must never be mixed into the orchestration baseline
(RUN-416/V246, orchestration/plans/skills). They are a separate capability domain
with a separate adapter, separate batch files, and separate measurement ledger.

The translation LoRA adapter must not share weights with the orchestration adapter.

If the base model is retrained, the orchestration baseline must be re-validated
before translation training begins again.
