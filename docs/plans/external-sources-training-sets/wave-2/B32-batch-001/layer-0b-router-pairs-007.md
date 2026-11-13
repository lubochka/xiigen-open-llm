# Layer 0-B — Router Pairs (007)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0b-router-pairs

---

## B32-0B-007-001 — [IMPLEMENT] → 4.1: exploratory coding + module

**Predicates received:** `["IMPLEMENT"]`

**Source request:** "Exploratory coding session — hack together the notification service module."

**Gap warnings present:** gap-vibe-coding-exploratory-001, gap-vibe-coding-hack-001

**Router output:**
```
RouteTarget: BackendServiceFlow41
FlowSelected: [BackendServiceFlow41]
TransitionType: feature_addition
GapWarnings: [gap-vibe-coding-exploratory-001, gap-vibe-coding-hack-001]
DomainId: "software-development"
```

**Note on "coding" → IMPLEMENT:** The predicate IMPLEMENT fired via "coding" (gerund) matching "code" root. This is not a direct "hack together" trigger — "hack together" is gap. The routing proceeds on the "coding" predicate, not the "hack" vocabulary.

---

## B32-0B-007-002 — Exploratory IMPLEMENT routing: DomainId consistency

**Rule:** Even for exploratory or experimental IMPLEMENT requests, DomainId remains "software-development". The exploratory nature is captured in gap warnings, not in DomainId.

**Correct:**
```
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-exploratory-001]
RouteTarget: BackendServiceFlow41
```

**Wrong (v6-03 pattern):**
```
DomainId: "exploratory-development"  ← invented DomainId; not registered
```

---

## B32-0B-007-003 — "hack together" alone: knowledge_edge (no action verb fires)

**Predicates received:** `[]`

**Source request:** "Hack the notification service together quickly."

**Gap warnings present:** gap-vibe-coding-hack-001

**Router output:**
```
RouteTarget: knowledge_edge
FlowSelected: none
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
GapWarnings: [gap-vibe-coding-hack-001]
```

**Contrast with 007-001:** Adding "coding" to the request changes the result from knowledge_edge to BackendServiceFlow41 because "coding" fires the IMPLEMENT predicate.

---

## B32-0B-007-004 — Gap warning source tracking in router

**When routing includes gap warnings, the router records the source of each gap:**

```
GapWarnings: [
  {id: "gap-vibe-coding-exploratory-001", trigger: "exploratory", position: 0},
  {id: "gap-vibe-coding-hack-001", trigger: "hack together", position: 3}
]
```

This allows the learning pipeline to identify which specific terms are unrecognized in which positions.

---
