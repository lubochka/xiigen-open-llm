# Layer 0-B — Router Pairs (054)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-054-001 — DomainId invariant in all router outputs

**Rule: router always sets DomainId = "software-development"**

This is confirmed across all routing scenarios:

| Scenario | DomainId |
|---|---|
| Active flow (IMPLEMENT) | software-development |
| Active flow (multiple predicates) | software-development |
| knowledge_edge (empty predicates) | software-development |
| Multi-gap, multi-predicate | software-development |
| No gap vocabulary | software-development |

DomainId is never "project-management", "vibe-coding", null, or omitted.

---

## B32-0B-054-002 — DomainId verification: three scenarios

**Scenario A: gap + active flow**
```
Request: "Lean sprint — implement the feature."
DomainId: "software-development" ✓
```

**Scenario B: gap + knowledge_edge**
```
Request: "Be pragmatic."
DomainId: "software-development" ✓
```

**Scenario C: no gap + active flow**
```
Request: "Build the service."
DomainId: "software-development" ✓
```

---

## B32-0B-054-003 — DomainId with multi-gap: still "software-development"

**Request:** "Expedient spike — implement the dashboard."

```
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-spike-001, gap-vibe-coding-dashboard-001]
DomainId: "software-development"
```

Multiple gap IDs do not affect DomainId.

---
