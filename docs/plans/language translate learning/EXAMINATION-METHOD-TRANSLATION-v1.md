# Examination Method — Translation Domain
# Current source registry: TRANSLATION-SOURCE-REGISTRY-v3.md
# Rejected-side prompts: TRANSLATION-DPO-REJECTED-PROMPTS-v1.md
# Version: 1.0 — 2026-05-11
# Domain: Russian/Hebrew ↔ English Translation
# Parallel to: EXAMINATION-METHOD-ORCHESTRATION-v8.md

---

## Purpose

This document describes how to examine a source document or parallel corpus
sentence pair and extract a valid DPO training pair for the translation domain.

An examiner reads a source entry, determines which pair family it belongs to,
constructs the A-side, chosen, and rejected, and applies all gates before
marking the pair train-ready.

---

## Layer 0-A — Source Qualification

Before reading any sentence pair, qualify the source.

Check in order:

1. **Registry decision** — Look up the source id in `TRANSLATION-SOURCE-REGISTRY-v3.md`.
   - BLOCK → stop. Do not read further.
   - REVIEW REQUIRED → stop. File a registry review note.
   - QUARANTINE → continue, but set `batch_isolation: true` on every pair from this source.
   - ACCEPT → continue normally.

2. **Script and encoding** — Verify that the source and target fields decode
   cleanly as UTF-8. Mojibake marker codepoints (`U+00C3`, `U+00C2`,
   `U+00E2 U+20AC`, `U+00C3 U+00A2`) in either field →
   reject the entry. Do not attempt to repair encoding; discard and move to next.

3. **Language verification** — Confirm the declared language pair matches the
   actual script:
   - Russian fields must contain Cyrillic.
   - Hebrew fields must contain Hebrew script (Unicode range U+0590–U+05FF or U+FB00–U+FB4F).
   - English fields must be primarily Latin.
   - Mixed-script tokens (proper names, technical terms) are acceptable; entirely
     wrong-script fields are not.

4. **Length filter** — Skip entries shorter than 4 source tokens. The upper
   limit depends on tokenizer state and model context length:

   - **Before tokenizer extension** (Phase 1–2, byte-level fallback for
     Cyrillic/Hebrew): maximum **15 source tokens** for Hebrew, **20 source
     tokens** for Russian. Without trained merges each character costs 2 base
     tokens; a 15-word Hebrew sentence encodes to ~60 tokens, filling the model's
     `ContextLen = 64` leaving little room for the target side.
   - **After tokenizer extension** (Phase 3+, ≥ 90%/95% coverage verified):
     maximum **32 source tokens** (`ContextLen / 2 = 64 / 2`). This leaves the
     other 32 context positions for the target sequence and any padding.
   - The 256-token figure cited in early drafts refers to future model
     configurations with longer context. Do not use 256 as a length gate until
     `ContextLen` is increased beyond 128.

---

## Layer 0-B — Pair Family Assignment

Assign exactly one family code from `TRANSLATION-DPO-REGISTRY-GUIDE-v1.md`.

Use this decision tree:

```
Does the source sentence contain an idiom or fixed expression?
  Yes → T04 (Idiom and Fixed Expression)

Does the source sentence contain a known false cognate?
  Yes → T05 (False Cognate) — only for RU_EN and HE_EN directions.
  T05 is not defined for EN_RU or EN_HE (no false cognate list exists for those
  directions). Route EN→target sentences with cognate risk to T01 instead.

Is the target language Hebrew and does the pair test script/directionality?
  Yes → T06 (RTL Script and Directionality)

Is the source entry from a round-trip back-translation test?
  Yes → T07 (Back-Translation Consistency)

Does the pair test for a named entity (person, place, organisation)?
  Yes → T09 (Named Entity Preservation)

Does the pair test register (formal vs colloquial) explicitly?
  Yes → T02 (Register Preservation)

Does the correct translation require a specific morphological choice
(Russian case/aspect, Hebrew gender/binyan)?
  Yes → T03 (Morphological Accuracy)

Does the rejected side have an omission or expansion?
  Yes → T08 (Length and Completeness)

Default → T01 (Sentence Translation Core)
```

If more than one family applies, use the most specific one (lowest-numbered
special case, not T01).

---

## Layer 1 — A-Side Construction

The A-side is the source language sentence.

Rules:
1. Use the source sentence verbatim from the corpus. Do not rephrase it.
2. Do not add "Translate the following:" or any metalanguage — the system prompt
   provides the translation direction.
3. For T02 (Register), append `(register: formal)` or `(register: colloquial)`
   after the sentence, in parentheses.
4. For T03 (Morphological), append the relevant morphological category in
   parentheses if ambiguity exists, e.g. `(requires perfective aspect in target)`.
5. For T07 (Back-Translation), the A-side is: `[BACK-TRANSLATE] {target_language_text}`.
6. For T06 (RTL), the A-side is the source sentence in English or Russian, not Hebrew.

---

## Layer 2 — Chosen Side Construction

The chosen side is the best available human translation.

Rules:
1. Use the parallel corpus human translation verbatim if available and quality-verified.
2. If the corpus translation is poor, mark the pair `quality_status: review_required`
   and do not promote to train-ready without human correction.
3. Do not add explanation, commentary, or alternate readings to the chosen side.
   Translation only. Clean target sentence.
4. For back-translation pairs (T07), the chosen side is the source sentence
   reconstructed faithfully by the back-translation.
5. Do not include metalanguage such as "Here is the translation:" in chosen.

---

## Layer 3 — Rejected Side Construction

The rejected side must be a plausibly wrong translation with `[WRONG: ...]`.

### Step 3.1 — Choose an error category

Select the error most relevant to the pair family:

| Family | Primary error categories |
|---|---|
| T01 | mistranslation, omission, literal calque |
| T02 | register shift |
| T03 | morphological (specify sub-type) |
| T04 | literal calque |
| T05 | false cognate |
| T06 | RTL directionality error |
| T07 | back-translation divergence |
| T08 | omission, addition |
| T09 | named entity (transliteration scheme) |

### Step 3.2 — Construct the rejected translation

Write a translation that would result from the chosen error category. Do not
invent absurd nonsense. The rejected side should be the kind of mistake a
competent but careless translator or a naive model would produce.

Good sources for rejected constructions:
- Literal (word-for-word) rendering of the source
- Machine translation output before post-editing (for T01, T04, T05)
- A translation in the wrong register from a different corpus sentence (T02)
- A translation with the wrong morphological form — look up the incorrect form in
  a grammar reference (T03)
- A back-translation from a poor translation (T07)

### Step 3.3 — Append [WRONG: ...] annotation

The `[WRONG: ...]` must be:
- Appended to the end of the rejected translation on a new line.
- Specific: name the error category AND the specific token or element.
- Not generic: `[WRONG: bad translation]` is not acceptable.

Example (T01, RU_EN):
```
Target sentence with mistranslation here.
[WRONG: mistranslation — Russian "жалеть" means "to regret/pity", not "to spare/save"; correct: "I regret" not "I saved"]
```

Example (T03, EN_RU morphological):
```
Неправильная форма глагола.
[WRONG: morphological — English source "I finished" implies completed action; Russian requires perfective aspect "закончил", not imperfective "заканчивал"]
```

Example (T05, RU_EN false cognate):
```
The magazine was full of interesting items.
[WRONG: false cognate — Russian "магазин" means "shop/store", not "magazine"; correct: "The store was full of interesting items"]
```

---

## Layer 4 — Gate Check Before Writing

Apply all gates in order. If any gate fails, do not write the pair.

### Gate 1: Token overlap
Compute overlap_ratio. If > 0.60 and len(chosen_tokens) + len(rejected_tokens) >= 12 → discard.

### Gate 2: Identity check
`chosen.strip() == rejected.strip()` → discard.

### Gate 3: Mojibake check
Any mojibake marker in any field → discard.

### Gate 4: [WRONG:] presence
`rejected` does not contain `[WRONG:` → discard or repair.

### Gate 5: Language verification
Spot-check that chosen is in the declared target language.
Quick test: does the first word use the correct script?

### Gate 6: Length completeness
Does chosen translate all clauses in the source? If a clause was dropped, this
may be correct (T08 omission test) or an error. Decide before writing.

### Gate 7: Registry licence
Confirm `source_license_state` matches the registry for the source id.

---

## Layer 5 — Pair Record

Write the pair JSON record following the shape in `TRANSLATION-DPO-REGISTRY-GUIDE-v1.md`.

Required fields:
```json
{
  "pair_id": "T01-{sha1[:10]}-{seq:04d}",
  "source_family": "T01",
  "direction": "RU_EN",
  "mechanism": "translation_accuracy",
  "source_path": "path/to/corpus/entry",
  "source_id": "opus-tatoeba",
  "source_license_state": "ACCEPT",
  "a_side": "...",
  "chosen": "...",
  "rejected": "... [WRONG: ...]",
  "rejected_type": "mistranslation",
  "quality_status": "train_ready",
  "pair_kind": "sentence_translation",
  "source_canonicality": "canonical",
  "canonical_confirmed_via": "opus-tatoeba-v2023 sentence id 12345",
  "batch_isolation": false,
  "compiled_prompt_in_training": false,
  "eval_only": false
}
```

---

## Failure Modes to Avoid

### FM-TR-01: Chosen from model output without human verification
The chosen side must be from a verified parallel corpus or a human-reviewed
translation. Do not use raw model output as chosen.

### FM-TR-02: Rejected is a garbled string
The rejected side must be a plausible wrong translation, not nonsense. A
garbled string produces low reward margin because the model easily avoids it.

### FM-TR-03: [WRONG:] is too vague
`[WRONG: bad translation]` does not teach the model why the translation is
wrong. Specify the error category and the specific token.

### FM-TR-04: Encoding error in Hebrew text
Hebrew text that passes through systems without proper UTF-8 handling often
becomes mojibake. Always verify Hebrew fields byte-by-byte before writing pairs.

### FM-TR-05: Source sentence is a translation artefact
Some parallel corpora contain machine-translated sentences presented as human
translations. Check the corpus documentation — if a sentence is marked MT-origin,
treat it as rejected material only, not chosen authority.

### FM-TR-06: Register mixing
Formal Russian and colloquial Hebrew in the same batch produce conflicting
reward signals if not separated by direction code and batch. Keep register-labelled
pairs in a dedicated batch until standalone ablation passes.

### FM-TR-07: Back-translation pair uses the same model for both directions
Back-translation pairs are only meaningful when the two translation directions
use different model checkpoints or different decoding strategies. Using the same
model forward and back collapses the signal.

---

## Confidence Labels

| Confidence | Condition | Action |
|---|---|---|
| HIGH | Human-verified parallel corpus, ACCEPT licence, all gates pass | `quality_status: train_ready` |
| MEDIUM | Corpus is ACCEPT but translation quality is uncertain; or QUARANTINE source | `quality_status: review_required` |
| LOW | Encoding issue, MT-origin, or error category is ambiguous | `quality_status: repair_required` |
| BLOCK | BLOCK source, or gate failure that cannot be repaired | Discard; do not write pair |
