# DPO Pair Authoring Guidance — Translation Domain
# Version: 1.0 — 2026-05-11
# Domain: Russian/Hebrew ↔ English Translation
# Parallel to: DPO-PAIR-AUTHORING-GUIDANCE-v2.md (orchestration domain)
# Rejected-side prompts: TRANSLATION-DPO-REJECTED-PROMPTS-v1.md

## 2026-05-11 Rejected-Side Addendum

Use `TRANSLATION-DPO-REJECTED-PROMPTS-v1.md` when creating, auditing, or
repairing rejected sides. A rejected translation must be plausible, specifically
wrong, and annotated with a functional consequence. Random bad text, identical
chosen/rejected, and generic `[WRONG: bad]` annotations are not train-ready.

---

## Why This Document Exists

The orchestration DPO guidance covers ARCH, PLAN, SKILL, and CODE sub-phases.
Translation is a separate capability domain with different:

- A-side source (raw sentence, not compiled orchestration prompt)
- Chosen authority (verified parallel corpus, not XIIGen internal contract)
- Rejected error taxonomy (linguistic errors, not routing/governance errors)
- Reward margin mechanics (token-level linguistic divergence, not predicate match)

Read this document instead of DPO-PAIR-AUTHORING-GUIDANCE-v2.md when writing
translation pairs. Do not mix translation pairs into orchestration batches.

---

## Reward Margin Mechanics for Translation

DPO computes for every pair:

```
reward_margin = log P(chosen | a_side) - log P(rejected | a_side)
```

For translation pairs the model needs to learn that the chosen translation is
more likely given the source. This means:

- If chosen and rejected are very similar (high overlap), the model's logits
  for both are nearly identical → reward margin near zero → no learning signal.
- If the rejected side is obviously garbled, the model already scores it low
  before training → margin is already positive → pair teaches nothing new.
- The sweet spot is a rejected side that a naive model or careless translator
  would actually produce, but that a well-trained model should reject.

**Target reward margin per pair: > 0.05 after standalone ablation.**

If a pair class consistently produces negative or near-zero margin, diagnose
using the table in the Diagnosis section.

---

## The Four Translation Error Types (Rejected Side)

### Type A — Semantic Error (mistranslation, false cognate, omission)

The rejected translation conveys a different meaning. This produces strong
margin because model logits for semantically different tokens diverge sharply.

Proportion target: ≥ 40% of batch.

Examples:
- RU→EN: "горячий" → rejected: "bitter" (wrong) vs chosen: "hot" (correct)
- HE→EN: "ספרייה" → rejected: "Spain" (false cognate with "ספרד") vs chosen: "library"

### Type B — Morphological Error

The rejected translation has the right words but the wrong form. This is harder
for the model to learn because logit differences are smaller (same stem, different
ending). Use these sparingly — require at least 8 semantic-error pairs in a batch
before adding morphological pairs.

Proportion target: 20%–30% of batch.

Examples:
- EN→RU: wrong case after preposition ("в магазин" vs "в магазине")
- EN→HE: wrong gender agreement ("הספר הגדולה" → should be "הגדול")

### Type C — Register or Style Error

The rejected translation is semantically correct but in the wrong register.
This produces moderate margin — the model can distinguish formal from
colloquial if the training signal is consistent.

Proportion target: 15%–25% of batch.

Examples:
- Formal HE→EN: "אני מבקש" → rejected: "I wanna" vs chosen: "I request/would like"
- Colloquial RU→EN: "всё нормально" → rejected: "everything is within normal parameters" vs chosen: "it's fine / all good"

### Type D — Structural or Completeness Error

The rejected translation has an omission, addition, or structural reordering
that changes the completeness of the output. Use for T08 pairs.

Proportion target: 10%–15% of batch.

---

## Batch Composition Rules

For a 64-pair first batch, target distribution:

| Direction | T01 core | T03 morphological | T02 register | T04/T05 idiom/cognate | T09 named entity |
|---|---|---|---|---|---|
| RU→EN | 6 | 3 | 2 | 2 | 1 |
| EN→RU | 5 | 3 | 2 | 1 | 1 |
| HE→EN | 6 | 3 | 2 | 2 | 1 |
| EN→HE | 5 | 4 | 2 | 1 | 2 |

Total: 64 pairs across 4 directions and 5 families.

Do not mix directions in a single sub-batch for ablation. Run direction-level
ablation before mixing all four directions.

---

## Script-Specific Rules

### Russian (Cyrillic)

1. Use UTF-8 exclusively. Do not use KOI8-R or Windows-1251 encoded input.
2. Stress marks (Unicode combining acute accent U+0301) are optional in pairs;
   do not treat their presence or absence as an error.
3. "Ё" (U+0401) vs "Е" (U+0415): modern Russian often drops the diaeresis.
   Both forms are acceptable in chosen; do not use one as a rejected variant
   of the other unless the pair explicitly tests this distinction.
4. Verb aspect (perfective / imperfective) is the single highest-value
   morphological teaching target for EN→RU pairs. Prioritise it in T03.

### Hebrew

1. Hebrew text must be in logical order (left-to-right in the byte stream),
   not visual order. Unicode bidi algorithm handles display.
2. Nikud (vowel points) — most modern Hebrew text omits nikud. Chosen sides
   should match the target register:
   - Biblical / liturgical text → nikud expected
   - Modern prose → nikud absent
   - Do not use nikud presence/absence as a rejected variant unless testing T06.
3. Gershayim (״) is the Hebrew double punctuation used for abbreviations and
   acronyms. Do not replace with ASCII quotation marks in Hebrew text.
4. The Hebrew alphabet has five final letters (ך ם ן ף ץ). Using a non-final
   form at word end, or a final form at word start/middle, is a morphological
   error — use as T03 rejected type.
5. Binyan (verb conjugation pattern) is the highest-value morphological teaching
   target for EN→HE pairs. Prioritise it in T03.
6. **Knesset source register label:** Pairs sourced from `knesset-he` must be
   labelled `(register: formal spoken)`, not `(register: formal)`. Knesset
   transcripts are spoken parliamentary Hebrew. Formal written Hebrew (prose,
   legal documents) is a different register — do not conflate them. Use
   `(register: formal)` only when the source is written formal text.

---

## Prompt System Setup (compiled_prompt_in_training: false)

Translation pairs do NOT go through the XIIGen semantic prompt compiler.
The a_side is raw source text.

The system prompt template at inference time is:

```
Translate the following {source_language} text into {target_language}.
Produce only the translation. Do not add explanations.
```

Do NOT include this system prompt in the `a_side` field of the pair.
The `a_side` is the source sentence only. The system prompt is injected
by the training harness.

Set `compiled_prompt_in_training: false` on every translation pair.

---

## [WRONG:] Writing Rules

### Required format

```
{rejected translation text}
[WRONG: {error_category} — {specific description}]
```

### Error category codes

| Code | Meaning |
|---|---|
| `mistranslation` | Wrong word or phrase equivalent |
| `false_cognate` | Word looks like target-language word but means something different |
| `literal_calque` | Word-for-word rendering that violates target idiom |
| `omission` | Source clause or element dropped |
| `addition` | Content added that is not in source |
| `morphological` | Wrong case / gender / number / aspect / binyan |
| `register_shift` | Wrong formality level |
| `rtl_error` | Script direction or bidi marker error |
| `back_translation_divergence` | Round-trip semantic loss |
| `named_entity` | Wrong transliteration or translation of proper name |

### Length and specificity

- Minimum: `[WRONG: {code} — at least one specific reference to the offending token or clause]`
- Maximum: 60 words. If more is needed to explain the error, the pair is too complex — split it.

---

## Token Overlap Test

Compute before submitting each pair to the batch:

```python
def overlap_ratio(a_side_tokens, chosen_tokens, rejected_tokens):
    prompt_set = set(a_side_tokens)
    shared = set(chosen_tokens) & set(rejected_tokens) - prompt_set
    total = len(set(chosen_tokens) | set(rejected_tokens)) - len(prompt_set & (set(chosen_tokens) | set(rejected_tokens)))
    if total == 0:
        return 0.0
    return len(shared) / total
```

If `overlap_ratio > 0.60` and pair length qualifies (>= 12 tokens): discard.

For translation pairs this typically fires when chosen and rejected differ by
only one or two tokens. In that case, select a more divergent rejected variant.

---

## Diagnosis Table

| Symptom | Likely cause | Fix |
|---|---|---|
| All pairs in a batch show reward_margin < 0 | Rejected sides are too good (too similar to chosen) | Increase structural divergence; use Type A errors instead of Type B |
| Reward margin near zero for all pairs | Chosen and rejected are token-similar | Apply overlap test; select more divergent rejected variants |
| High loss but low accuracy | Model is uncertain which is correct | Pair may be ambiguous; add more specific [WRONG:] annotation |
| Direction-specific negative margin | Tokenizer handles that language poorly | Check tokenizer coverage for the script; verify BPE (Byte Pair Encoding) vocabulary |
| Hebrew pairs all fail | Encoding issue (bidi) | Verify UTF-8 byte order in Hebrew fields; check that logical order is correct |
| Russian morphological pairs negative margin | Aspect is too subtle | Add more phonologically distinct chosen/rejected pairs before returning to aspect |
| Named entity pairs near-zero margin | Both transliterations are phonetically close | Use pairs where one form is clearly non-standard (e.g. dated Soviet romanisation vs modern BGN) |

---

## What NOT to Do

1. **Do not use machine translation output as chosen.** MT output is a reference
   for rejected construction, not authority for chosen.

2. **Do not write chosen and rejected in different languages.** Both must be in the
   declared target language.

3. **Do not create pairs from blocked or review-required sources** (CulturaX,
   RNC, WizardLM, ShareGPT equivalents, or any source not in the registry).

4. **Do not mix QUARANTINE source rows with ACCEPT rows in the same batch file.**
   Separate files, separate ablation runs.

5. **Do not use the same sentence pair to produce both a T01 and a T03 pair.**
   Each source sentence contributes one pair to the batch. If it tests both a
   mistranslation and a morphological error, pick the primary error type.

6. **Do not omit [WRONG:] because the error is "obvious."** The annotation is
   required by the hard gate and by the model's ability to learn the error category.

7. **Do not use classical Biblical Hebrew as chosen for modern prose translation.**
   Classical and modern registers produce conflicting reward signals. Label the
   pair T02 register explicitly if classical Hebrew is relevant.

8. **Do not produce pairs shorter than 4 tokens in the A-side.** Short sentences
   produce high overlap ratios regardless of content.
