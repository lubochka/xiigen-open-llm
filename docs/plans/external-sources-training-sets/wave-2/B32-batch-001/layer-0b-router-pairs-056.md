# Layer 0-B — Router Pairs (056)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-056-001 — complete gap ID routing reference

**All six gap IDs produce the same routing decision:**

| Gap ID present (alone) | Route |
|---|---|
| gap-vibe-coding-style-001 | knowledge_edge + SCL |
| gap-vibe-coding-hack-001 | knowledge_edge + SCL |
| gap-vibe-coding-prototype-001 | knowledge_edge + SCL |
| gap-vibe-coding-spike-001 | knowledge_edge + SCL |
| gap-vibe-coding-rough-cut-001 | knowledge_edge + SCL |
| gap-vibe-coding-dashboard-001 | knowledge_edge + SCL |

All gap IDs produce the same routing outcome when predicates = [].

---

## B32-0B-056-002 — "quick" + IMPLEMENT: routing

**Request:** "Quick implementation — build the retry handler."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
IMPLEMENT → ImplementationFlow
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-056-003 — multi-gap-ID knowledge_edge routing

**Request:** "Lean spike prototype hack-together."

```
Input: Predicates = []
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-spike-001, gap-vibe-coding-prototype-001, gap-vibe-coding-hack-001]
Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (once)
DomainId: "software-development"
```

Four gap IDs; one SCL; one routing decision.

---
