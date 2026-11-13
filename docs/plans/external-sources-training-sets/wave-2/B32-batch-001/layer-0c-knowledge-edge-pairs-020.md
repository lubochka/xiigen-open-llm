# Layer 0-C — Knowledge-Edge Pairs (020)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-020-001 — Style qualifier + action verb: the core B32 mixed pattern

**Knowledge edge:** When a style qualifier (gap vocabulary) co-occurs with an action verb, the predicate fires and the gap qualifier becomes a warning-only annotation.

**Core rule:**

```
Gap vocabulary alone → predicates = [] → knowledge_edge
Gap vocabulary + action verb → predicate fires → active flow + gap warning
```

**Master example table:**

| Request | Gap vocabulary | Action verb | Predicates | GapWarnings |
|---|---|---|---|---|
| "Just make it work — build the checkout" | just-works-001 | build | [IMPLEMENT] | [just-works-001] |
| "Quick and dirty — implement billing" | quick-dirty-001 | implement | [IMPLEMENT] | [quick-dirty-001] |
| "Vibe code it — create an export" | style-001 | create | [IMPLEMENT] | [style-001] |
| "Hack together + write the adapter" | hack-001 | write | [IMPLEMENT] | [hack-001] |
| "Scrappy build + test" | style-001 | build + test | [IMPLEMENT, TEST] | [style-001] |

---

## B32-0C-020-002 — Corruption pattern v6-02: style suppresses predicate (WRONG)

**What v6-02 corruption looks like:**

```
Request: "Just make it work — build the checkout flow."
CORRUPTED Output: Predicates: [] 
Reason given: "just make it work" framing indicates no formal action needed
```

**Why this is wrong:**
- "build" is in IMPLEMENT lexicon → IMPLEMENT must fire
- "just make it work" is gap vocabulary → warning only, NEVER a suppressant
- Any output with action verb present but predicates empty = v6-02 corrupt

---

## B32-0C-020-003 — CapabilityGapWarning field in non-empty predicate outputs

**When predicates are non-empty but gap vocabulary is present:**

The output includes a `CapabilityGapWarning` field to signal the quality-bar context:

```
CapabilityGapWarning: style_qualifier_noted
```

This field:
- Appears when gap vocabulary is present alongside non-empty predicates
- Does NOT trigger SCHEDULE_CAPABILITY_LEARNING
- Is for analytics and downstream learning pipeline use
- Does not affect flow selection or execution

---
