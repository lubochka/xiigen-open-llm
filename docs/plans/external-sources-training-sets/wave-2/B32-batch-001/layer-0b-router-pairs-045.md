# Layer 0-B — Router Pairs (045)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-045-001 — negation pattern "no tests needed" alone: routing

**Request:** "No tests needed — just ship it."

```
Input: Predicates = []
GapWarnings: []
Route: (no gap vocabulary; no predicates; normal empty output)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
DomainId: "software-development"
```

No gap vocabulary → no SCL. No predicates → no factory.

---

## B32-0B-045-002 — "good enough + no tests needed": routing

**Request:** "Good enough — no tests needed."

```
Input: Predicates = []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (once)
DomainId: "software-development"
```

---

## B32-0B-045-003 — "lean + implement + don't bother with tests": routing

**Request:** "Lean sprint — implement the feature, don't bother with tests."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
IMPLEMENT → ImplementationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-045-004 — "pragmatic + skip tests + implement": routing

**Request:** "Pragmatic — skip the tests, just implement the upload."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
IMPLEMENT → ImplementationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---
