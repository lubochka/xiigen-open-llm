# Layer 0-A — Compiler Pairs (028)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-028-001 — "spike" vocabulary: core cases

**Input A:** "Run a spike on the payment gateway integration."
**Input B:** "Technical spike for caching layer."
**Input C:** "Spike to check if GraphQL works for our use case."

```
Input A:
"run a spike" → gap-vibe-coding-spike-001
"run" → in "run a spike" context → not an action predicate verb (run + investigation noun)
Predicates: []  → knowledge_edge

Input B:
"technical spike" → gap-vibe-coding-spike-001
"caching layer" → domain noun; no action verb
Predicates: []  → knowledge_edge

Input C:
"spike" → gap-vibe-coding-spike-001
"check" → investigative verb; not in predicate lexicon
Predicates: []  → knowledge_edge

All three: SCHEDULE_CAPABILITY_LEARNING emitted once per request
```

---

## B32-0A-028-002 — "spike" + action verb → predicate fires

**Input:** "Run a spike and then implement the chosen approach."

```
Token scan:
"run a spike" → gap-vibe-coding-spike-001
"implement" → IMPLEMENT fires ✓

Note: "spike" part does not fire predicates; "implement" fires IMPLEMENT independently

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-spike-001]
RouteTarget: ImplementationFlow
CapabilityGapWarning: style_qualifier_noted
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
```

---

## B32-0A-028-003 — "spike" vs "pin": disambiguation

**Input A:** "Run a spike on caching."
**Input B:** "Pin down the caching strategy."

```
Input A: "spike" → gap-vibe-coding-spike-001 → Predicates: []
Input B: "pin down" → not a registered action predicate verb → Predicates: []

Input A emits gap-vibe-coding-spike-001.
Input B: no gap ID, no predicates — also routes knowledge_edge but with no gap warning.
```

---

## B32-0A-028-004 — "spike" + TEST → TEST fires, spike is gap

**Input:** "Spike the auth service and write unit tests."

```
Token scan:
"spike" → gap-vibe-coding-spike-001
"write" → IMPLEMENT fires ✓
"unit tests" → TEST fires ✓ (positive mention as object of "write")

Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-spike-001]
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0A-028-005 — multi-spike: all three forms, one request

**Input:** "Technical spike, run a spike, just a spike — implement the result."

```
Token scan:
"technical spike" → gap-vibe-coding-spike-001
"run a spike" → gap-vibe-coding-spike-001
"just a spike" → gap-vibe-coding-spike-001
"implement" → IMPLEMENT fires ✓

Note: three gap occurrences all map to same ID → one gap-vibe-coding-spike-001 warning
Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-spike-001]
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
```

---
