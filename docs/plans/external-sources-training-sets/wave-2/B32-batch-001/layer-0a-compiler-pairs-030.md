# Layer 0-A — Compiler Pairs (030)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-030-001 — "walking skeleton" alone → zero predicates

**Input:** "Walking skeleton for the order processing system."

```
Token scan:
"walking skeleton" → gap-vibe-coding-spike-001 (walking-skeleton (WS) = thin end-to-end slice; investigation/minimal framing)
"order processing system" → domain noun compound; no action verb

Action verb scan: none present

Predicates: []
GapWarnings: [gap-vibe-coding-spike-001]
RouteTarget: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (DomainId: software-development)
```

---

## B32-0A-030-002 — "walking skeleton" + implement → IMPLEMENT fires

**Input:** "Build a walking skeleton of the order processing system."

```
Token scan:
"build" → IMPLEMENT fires ✓
"walking skeleton" → gap-vibe-coding-spike-001

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-spike-001]
RouteTarget: ImplementationFlow
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0A-030-003 — "walking skeleton" + TEST → TEST fires

**Input:** "Create a walking skeleton with unit tests covering the happy path."

```
Token scan:
"create" → IMPLEMENT fires ✓
"walking skeleton" → gap-vibe-coding-spike-001
"unit tests" → TEST fires ✓ (positive mention; "covering" = action toward tests)

Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-spike-001]
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0A-030-004 — "skeleton" alone vs "walking skeleton": contrast

**Input A:** "Walking skeleton."
**Input B:** "Skeleton implementation."

```
Input A: "walking skeleton" (compound) → gap-vibe-coding-spike-001 → Predicates: []
Input B: "skeleton" (adjective modifier) + "implementation" → no registered gap; no action verb → Predicates: []

Input A emits gap warning. Input B emits no gap warning (skeleton as modifier is not gap vocabulary).
Both route knowledge_edge (no action verb).
```

---

## B32-0A-030-005 — "walking skeleton" + ARCHITECTURE + IMPLEMENT

**Input:** "Design a walking-skeleton architecture and then implement it."

```
Token scan:
"design" → ARCHITECTURE fires ✓
"walking-skeleton" → gap-vibe-coding-spike-001
"architecture" → ARCHITECTURE reinforces
"implement" → IMPLEMENT fires ✓

Predicates: ["ARCHITECTURE", "IMPLEMENT"]
GapWarnings: [gap-vibe-coding-spike-001]
TransitionType: feature_addition (IMPLEMENT present → takes precedence)
CapabilityGapWarning: style_qualifier_noted
```

---
