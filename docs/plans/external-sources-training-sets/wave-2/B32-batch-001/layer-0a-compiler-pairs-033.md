# Layer 0-A — Compiler Pairs (033)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-033-001 — gap + TEST embedded: TEST fires alongside gap

**Input:** "Quick and dirty but write unit tests for every branch."

```
Token scan:
"quick and dirty" → gap-vibe-coding-quick-dirty-001
"write" → IMPLEMENT fires ✓ (write = action verb)
"unit tests" → TEST fires ✓ (positive mention as object of write)

Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-quick-dirty-001]
RouteTarget: [ImplementationFlow, TestFlow]
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0A-033-002 — "scrappy" + TEST: gap warning does not suppress TEST

**Input:** "Scrappy implementation but still test the critical paths."

```
Token scan:
"scrappy" → gap-vibe-coding-style-001
"implementation" → IMPLEMENT fires ✓ (implementation noun from implement root)
"test" → TEST fires ✓ (positive imperative form)
"critical paths" → scope

Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
Note: "scrappy" does NOT suppress TEST — Rule 3 confirmed
```

---

## B32-0A-033-003 — "proof-of-concept" + TEST

**Input:** "PoC only, but write tests for the integration layer."

```
Token scan:
"PoC" → gap-vibe-coding-prototype-001
"but" → conjunction; no effect on predicates
"write" → IMPLEMENT fires ✓
"tests" → TEST fires ✓

Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-prototype-001]
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0A-033-004 — "throwaway" + TEST: gap warning + TEST fires

**Input:** "Throwaway code — but add unit tests so we can verify behaviour."

```
Token scan:
"throwaway" → gap-vibe-coding-throwaway-001
"add" → IMPLEMENT fires ✓ (add = action verb in IMPLEMENT lexicon)
"unit tests" → TEST fires ✓

Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-throwaway-001]
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0A-033-005 — "walking skeleton" + TEST + DOCUMENT

**Input:** "Walking skeleton with tests and documentation."

```
Token scan:
"walking skeleton" → gap-vibe-coding-spike-001
"tests" → TEST fires ✓ (positive mention with "with" instrumental)
"documentation" → DOCUMENT fires ✓

Note: "with tests and documentation" = positive instrumental → both fire

Predicates: ["TEST", "DOCUMENT"]
GapWarnings: [gap-vibe-coding-spike-001]
CapabilityGapWarning: style_qualifier_noted
```

---
