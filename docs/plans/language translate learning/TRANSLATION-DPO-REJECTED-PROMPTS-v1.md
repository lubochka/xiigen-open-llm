# Translation DPO Rejected-Side Prompt Library
# Version: 1.0 - 2026-05-11
# Domain: Russian/Hebrew <-> English Translation
# Applies to: routing-flow DPO and translator DPO

---

## Purpose

This document gives copy-paste prompts for creating and auditing rejected sides
in translation DPO pairs.

A good rejected side is not random bad text. It is a plausible wrong answer that
teaches one specific boundary:

- wrong direction
- wrong topic route
- wrong adapter
- mistranslation
- omission
- addition
- morphology error
- register shift
- named-entity corruption
- Hebrew RTL/script error

Every rejected side must include:

```text
[WRONG: specific_error - functional consequence]
```

---

## Prompt 1 - Create A Good Translator Rejected Side

Use this prompt when you already have a source sentence and a verified chosen
translation.

```text
You are creating the rejected side for a translation DPO pair.

Goal:
Create one plausible but wrong translation. The rejected side must be close
enough to the chosen translation that the model learns a real translation
boundary, but wrong enough that the chosen side is clearly preferred.

Input:
- direction: <RU_EN | EN_RU | HE_EN | EN_HE>
- topic_domain: <general | named_entity | legal_diplomatic | ...>
- level: <1 | 2 | 3 | 4>
- family: <T01 | T02 | T03 | T04 | T05 | T06 | T07 | T08 | T09>
- source: <source sentence>
- chosen: <verified translation>

Allowed rejected error types:
- wrong_direction
- copied_source
- mistranslation
- omission
- addition
- negation_error
- morphology_error
- register_shift
- idiom_calque
- false_cognate
- named_entity_corruption
- rtl_or_script_error

Instructions:
1. Produce exactly one rejected translation.
2. Make it fluent enough to be plausible.
3. Introduce exactly one primary error type.
4. Do not make it random, broken, or obviously garbage.
5. Do not copy the chosen with only punctuation changes.
6. Do not include explanation outside the rejected text.
7. Append one [WRONG: ...] annotation that names the error and the functional consequence.

Output format:
rejected: "<wrong translation>\n[WRONG: <error_type> - <what the model cannot learn from this rejected side and what failure it would cause>]"
```

---

## Prompt 2 - Create A Good Routing-Flow Rejected Side

Use this prompt for `TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>`.

```text
You are creating the rejected side for a translation routing-flow DPO pair.

Goal:
Create one plausible but wrong TranslationRoutingDecision. The rejected route
must be adjacent to the chosen route, not random. It should teach the model why
the selected direction/topic/register/risk/adapter is wrong.

Input:
- source_text: <text>
- target_language: <target language>
- chosen_direction: <RU_EN | EN_RU | HE_EN | EN_HE>
- chosen_topic_domain: <general | named_entity | legal_diplomatic | ...>
- chosen_register: <neutral | colloquial | formal_written | ...>
- chosen_risk_class: <normal | named_entity_sensitive | formal_high_precision | ...>
- chosen_adapter_id: <adapter id>
- chosen_evidence_terms: <terms>

Allowed rejected routing errors:
- wrong_direction
- wrong_topic_domain
- wrong_register
- wrong_risk_class
- wrong_adapter
- unsafe_low_risk_route
- over_specialized_route
- evidence_free_route

Instructions:
1. Produce exactly one rejected TranslationRoutingDecision.
2. Keep most fields plausible; change the field that teaches the intended boundary.
3. Include a confidence value that looks plausible but is wrong.
4. Include evidence_terms that reveal the mistake or missing evidence.
5. Include [WRONG: ...] inside decision_reason.
6. Explain the functional consequence: wrong decoder, wrong style, unsafe route,
   entity corruption, or blocked high-risk handling.

Output format:
{
  "direction": "...",
  "topic_domain": "...",
  "register": "...",
  "risk_class": "...",
  "translation_level": 1,
  "adapter_id": "...",
  "confidence": 0.0,
  "evidence_terms": ["..."],
  "decision_reason": "...\n[WRONG: <routing_error> - <functional consequence>]"
}
```

---

## Prompt 3 - Audit Whether A Rejected Side Is Good

Use this prompt before a pair is allowed into train-ready.

```text
You are auditing a rejected side for a translation DPO pair.

Input:
- direction:
- topic_domain:
- level:
- family:
- source:
- chosen:
- rejected:

Judge the rejected side against these gates:

Hard reject:
1. chosen == rejected
2. rejected has no [WRONG: ...]
3. rejected is random garbage or not in the target language
4. rejected is only a punctuation/capitalization variant
5. rejected contains mojibake
6. rejected teaches more than one primary error in a confusing way
7. rejected is accidentally better than chosen
8. rejected is too easy and only teaches "bad output is bad"

Accept only if:
1. rejected is plausible
2. exactly one primary error is clear
3. [WRONG: ...] names the error type
4. [WRONG: ...] states the functional consequence
5. the model can learn a reusable boundary from the contrast

Return:
- verdict: ACCEPT_REJECTED | REPAIR_REJECTED | HARD_REJECT_PAIR
- primary_error_type:
- reason:
- repaired_rejected: <only if repair is possible without changing chosen>
```

---

## Prompt 4 - Repair A Weak Rejected Side

Use this when a pair is useful but the rejected side is weak.

```text
You are repairing the rejected side of a translation DPO pair.

Input:
- direction:
- topic_domain:
- level:
- family:
- source:
- chosen:
- current_rejected:
- intended_error_type:

Repair rules:
1. Preserve the source and chosen exactly.
2. Replace only the rejected side.
3. Make the rejected side plausible but specifically wrong.
4. Use one primary error type only.
5. Add a strong [WRONG: ...] annotation with functional consequence.
6. Do not make the rejected random, silly, or obviously invalid.

Output:
repaired_rejected: "<text>\n[WRONG: <error_type> - <functional consequence>]"
```

---

## Examples Of Good Rejected Sides

### Good: Omission

```text
chosen:
"The committee approved the proposal after reviewing the budget."

rejected:
"The committee approved the proposal.\n[WRONG: omission - the rejected translation drops the budget-review condition, so the model would learn to remove a subordinate clause that changes the decision context.]"
```

### Good: Register Shift

```text
chosen:
"The ministry shall submit the report within thirty days."

rejected:
"The ministry should send in the report pretty soon.\n[WRONG: register_shift - the rejected translation converts legal-administrative wording into casual speech, so the model would lose formal register control.]"
```

### Good: Routing Wrong Adapter

```json
{
  "adapter_id": "adapter-ru-en-general",
  "decision_reason": "Routes UN resolution text as ordinary general translation.\n[WRONG: wrong_adapter - legal-diplomatic text needs the formal adapter; using the general adapter loses mandatory formal register and clause completeness.]"
}
```

---

## Examples Of Bad Rejected Sides

### Bad: Random Garbage

```text
rejected:
"asdf table moon blue"
```

Why bad:
The model learns only that nonsense is bad. It does not learn translation.

### Bad: Same As Chosen

```text
chosen:
"I am going home."

rejected:
"I am going home."
```

Why bad:
No preference signal exists.

### Bad: Too Many Errors

```text
rejected:
"Home going mine yesterday tomorrow minister."
```

Why bad:
Direction, syntax, tense, and semantics all fail at once. The model cannot learn
which boundary matters.

### Bad: Annotation Without Consequence

```text
rejected:
"The committee approved the proposal.\n[WRONG: omission]"
```

Why bad:
The error type is named, but the model is not taught what information was lost or
why the rejected output is harmful.

---

## Minimal Quality Rule

A rejected side is good when a reviewer can complete this sentence:

```text
The chosen side is better because the rejected side makes exactly this mistake:
<specific mistake>, and if the model learned the rejected side it would fail by:
<functional consequence>.
```

If that sentence cannot be completed, repair or reject the pair.
