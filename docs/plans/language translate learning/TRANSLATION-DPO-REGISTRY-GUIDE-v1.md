# Translation DPO Registry Guide
# Version: 1.0 — 2026-05-11
# Domain: Language Translation — Russian/Hebrew ↔ English
# Parallel to: XIIGEN-DPO-REGISTRY-GUIDE-v1.md (orchestration domain)

## 2026-05-11 Routing Addendum

Translation routing is a trainable orchestration structure. Do not treat routing
as a static classifier or source-file rule. The public routing contract is:

```csharp
TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>
TranslationRoutingOrchestrator : ILogicalOrchestrator
```

Internal routing decisions may use `ILogicalUnit` nodes, but DPO pairs that
teach adapter selection must include the complete `TranslationRoutingDecision`:
direction, topic domain, register, risk class, level, adapter id, confidence,
evidence terms, rejected alternatives, and decision reason.

Use `TRANSLATION-ROUTING-FLOW-DPO-GUIDANCE-v1.md` for router-specific pair
authoring. Translator DPO pairs must not be promoted unless the corresponding
routing-flow gate has passed.

Use `TRANSLATION-DPO-REJECTED-PROMPTS-v1.md` whenever creating, auditing, or
repairing rejected sides. A rejected side is not train-ready merely because it
is wrong; it must be plausible, specific, and annotated with a functional
consequence.

---

## Three Gateway Questions

Before writing any pair, answer all three:

1. **Who is the producer of the chosen side?**
   Must be: a verified human translation, a licensed parallel corpus sentence pair,
   or a model-corrected translation with human review. Never: a raw model output
   presented as authority without verification.

2. **Who is the consumer and what action does the pair teach?**
   Must be: the translation model learns to prefer a more accurate, fluent, or
   register-appropriate rendering. Not: teaching unrelated NLP tasks inside the
   translation pipeline.

3. **Is this a new pair family or a new version of an existing one?**
   Decide by: source language, target language, error category, and register.
   Do not create a new family if the existing family covers the error; add a new
   example to the existing family instead.

---

## Pair Mechanisms

Four DPO mechanisms apply to translation:

| Mechanism | When to use |
|---|---|
| `translation_accuracy` | Chosen is semantically correct; rejected has a mistranslation, omission, or addition. |
| `register_appropriateness` | Chosen maintains the source register (formal/informal/biblical/colloquial); rejected shifts register. |
| `morphological_accuracy` | Chosen uses correct case, gender, number, aspect, or RTL (right-to-left) script handling; rejected has a morphological error. |
| `back_translation_consistency` | Chosen round-trips faithfully (source → target → source with minimal semantic loss); rejected diverges. |

---

## Translation Direction Codes

| Code | Direction |
|---|---|
| RU_EN | Russian → English |
| EN_RU | English → Russian |
| HE_EN | Hebrew → English |
| EN_HE | English → Hebrew |

Every pair must carry one direction code.

---

## Pair Families

### T01 — Sentence Translation (Core)

**What it is:** A single source sentence paired with chosen correct translation
and rejected mistranslation.

**Producer:** Human-verified parallel corpus from an ACCEPT source, such as
Tatoeba. Do not use OpenSubtitles unless a later source registry explicitly
promotes it from `REVIEW_REQUIRED` to `ACCEPT`.

**Consumer action:** Model produces the target sentence.

**A-side shape:** Source language sentence only. No metalanguage. No "Translate
the following" prefix — the system prompt provides the direction.

**Chosen shape:** Fluent, accurate target sentence. No extraneous explanation.

**Rejected shape:** Target sentence with one or more specific errors:
- wrong word choice (`[WRONG: mistranslation — "X" is not the correct equivalent of "Y"]`)
- omission of a key semantic element (`[WRONG: omission — "Z" was dropped]`)
- literal rendering that violates target-language syntax (`[WRONG: literal calque — unnatural word order in target]`)

**Direction codes:** All four (RU_EN, EN_RU, HE_EN, EN_HE).

**Min pairs per direction per batch:** 8

---

### T02 — Register Preservation

**What it is:** Source text in a specific register (formal, colloquial, biblical,
technical). Chosen preserves the register. Rejected shifts it.

**Producer:** Human curator — annotator picks source from a register-labelled
corpus and writes both translations.

**A-side shape:** Source sentence + register label in parentheses, e.g.
`(formal register)` or `(colloquial spoken Russian)`.

**Chosen shape:** Target sentence in the matching register.

**Rejected shape:** Same semantic content, wrong register.
`[WRONG: register shift — formal source translated into colloquial target]`

**Direction codes:** RU_EN, EN_RU, HE_EN, EN_HE.

---

### T03 — Morphological Accuracy

**What it is:** Source sentence where correct translation requires matching the
morphological demands of the target language.

Russian sub-types:
- case assignment after prepositions
- verbal aspect (perfective vs imperfective)
- short-form vs long-form adjectives
- animate vs inanimate accusative

Hebrew sub-types:
- gender agreement (Hebrew nouns are grammatically gendered)
- construct state (smichut — סמיכות)
- definite article propagation (ha- on noun and adjective)
- verbal binyan (pa'al, pi'el, hif'il, hitpa'el, etc.)

**A-side shape:** Source sentence only.

**Chosen shape:** Morphologically correct target.

**Rejected shape:** Target with specific morphological error.
`[WRONG: morphological — wrong verb aspect; should be perfective]`
`[WRONG: morphological — gender disagreement on adjective]`

**Direction codes:** RU_EN and EN_RU for Russian morphology; HE_EN and EN_HE for Hebrew morphology.

---

### T04 — Idiom and Fixed Expression

**What it is:** Source sentence containing an idiomatic expression that does not
translate word-for-word. Chosen conveys the equivalent idiom or paraphrase in
the target. Rejected is a literal word-for-word rendering.

Examples:
- RU: "ни рыба, ни мясо" → EN chosen: "neither fish nor fowl"; rejected: "neither fish nor meat [WRONG: literal calque — Russian idiom has a standard English equivalent]"
- HE: "לאכול את הראש" → EN chosen: "to nag / drive someone crazy"; rejected: "to eat the head [WRONG: literal calque — Hebrew idiom loses meaning in literal translation]"

**Direction codes:** All four.

---

### T05 — False Cognate / False Friend

**What it is:** Source sentence containing a word that looks like a word in the
target language but means something different.

Russian examples:
- "данные" → data (not "donate")
- "магазин" → store/shop (not "magazine")
- "симпатичный" → cute/nice (not "sympathetic")

**A-side shape:** Source sentence.

**Chosen shape:** Correct translation that avoids the false cognate trap.

**Rejected shape:** Translation that uses the false cognate.
`[WRONG: false cognate — "симпатичный" ≠ "sympathetic"; correct: "nice/cute"]`

**Direction codes:** RU_EN, HE_EN.

---

### T06 — RTL Script and Directionality (Hebrew)

**What it is:** Hebrew output validation pairs. The A-side is an English or Russian
source. The chosen Hebrew translation is properly formed. The rejected version has
a script or directionality error.

Sub-types:
- Mixed direction embedding (numbers, English proper nouns inside Hebrew text)
- Punctuation mirroring (Hebrew uses mirrored punctuation in some contexts)
- Vowelisation (nikud) present vs absent in chosen vs rejected when appropriate

**Direction codes:** EN_HE only.

**Rejected types:**
`[WRONG: RTL — digits embedded without Unicode bidi marker]`
`[WRONG: RTL — Hebrew quotation marks replaced with Latin quotes]`

---

### T07 — Back-Translation Consistency

**What it is:** A round-trip pair. The source text is translated to target, then
back to source. Chosen round-trip reconstructs the original meaning faithfully.
Rejected round-trip produces a different meaning.

This family teaches the model to prefer translations that preserve the semantic
core rather than paraphrasing loosely.

**A-side shape:** Source → [TRANSLATION] → back-translation prompt.

**Chosen shape:** Back-translation that matches source meaning.

**Rejected shape:** Back-translation that diverges.
`[WRONG: back-translation divergence — key semantic element "X" was lost in round-trip]`

**Direction codes:** RU↔EN cycle, HE↔EN cycle.

---

### T08 — Length and Completeness

**What it is:** Source sentence translated completely vs. truncated or expanded.

**Chosen shape:** Complete translation — every clause accounted for.

**Rejected types:**
- Omission: source clause missing from target `[WRONG: omission — subordinate clause dropped]`
- Expansion: target adds information not present in source `[WRONG: addition — translator added hedging not present in source]`

---

### T09 — Named Entity Preservation

**What it is:** Source contains a proper name (person, place, organisation).
Chosen transliterates or preserves it correctly per target-language convention.
Rejected uses the wrong transliteration or translates an untranslatable name.

Russian → English: Cyrillic transliteration follows BGN/PCGN (Board on Geographic
Names / Permanent Committee on Geographical Names) convention.

Hebrew → English: Library of Congress romanisation table for Hebrew.

English → Russian: Standard Russian transliteration for well-known names; phonetic
approximation for unknown names.

English → Hebrew: Phonetic approximation using Hebrew script.

**Rejected types:**
`[WRONG: named entity — wrong transliteration scheme for Cyrillic]`
`[WRONG: named entity — person name translated instead of transliterated]`

---

## Pair Shape Requirements

Every pair in this domain must have:

```json
{
  "pair_id": "T01-{sha1[:10]}-{seq:04d}",
  "source_family": "T01",
  "direction": "RU_EN",
  "mechanism": "translation_accuracy",
  "source_path": "...",
  "source_id": "opus-tatoeba",
  "source_license_state": "ACCEPT",
  "a_side": "...",
  "chosen": "...",
  "rejected": "... [WRONG: ...]",
  "rejected_type": "mistranslation",
  "quality_status": "train_ready",
  "pair_kind": "sentence_translation",
  "source_canonicality": "canonical",
  "canonical_confirmed_via": "opus-tatoeba release v2023",
  "batch_isolation": false,
  "compiled_prompt_in_training": false,
  "eval_only": false
}
```

`compiled_prompt_in_training: false` — translation pairs use raw source text as
the A-side, not a compiled orchestration prompt. This is the same exception as
CODE_TO_DOC raw_snippet mode.

`batch_isolation: true` — required for all QUARANTINE sources (WikiMatrix,
HeWiki, FLORES-200).

---

## Hard Gates

A pair fails and goes to `repair_required` if any of these are true:

- `chosen == rejected` after trimming
- mojibake marker codepoints (`U+00C3`, `U+00C2`, `U+00E2 U+20AC`,
  `U+00C3 U+00A2`) in any field
- `rejected` does not contain `[WRONG: ...]`
- `source_license_state` is BLOCK or REVIEW REQUIRED
- `direction` field is missing
- `a_side` is in a language different from the declared source direction
- `chosen` is in a language different from the declared target direction

---

## Token Overlap Gate

```
shared = tokens in both chosen and rejected NOT also in a_side
total  = tokens in chosen + tokens in rejected - tokens in a_side
overlap_ratio = shared / total
```

`overlap_ratio > 0.60` → discard. The pair does not teach distributional
separation — chosen and rejected are too similar for the model to learn from.

For very short sentences (under 6 tokens) token overlap is not meaningful.
Apply the gate only when `len(chosen_tokens) + len(rejected_tokens) >= 12`.

---

## Rejected Side Quality Rules

A rejected side must be:

1. **Plausibly wrong** — a translation mistake a competent translator might make,
   not an obviously garbled string.
2. **Specifically annotated** — `[WRONG: ...]` must name the error category and
   the specific element that is wrong.
3. **Different in structure** from chosen — not just one word swapped.

Good rejected types:
- `mistranslation — "X" rendered as "Y"; correct is "Z"`
- `literal calque — Russian/Hebrew idiom has a standard English equivalent`
- `morphological — wrong verbal aspect; source implies perfective`
- `omission — subordinate clause "..." dropped from target`
- `register shift — formal source translated into colloquial`
- `false cognate — "X" ≠ "Y"; correct: "Z"`
- `named entity — wrong transliteration scheme`

Bad rejected types:
- Same sentence with one word synonymised
- Chosen text with a negation prepended
- Completely unrelated sentence
- Correct translation with an unrelated grammar error added
