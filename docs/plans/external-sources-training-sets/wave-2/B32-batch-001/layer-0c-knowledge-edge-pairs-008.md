# Layer 0-C — Knowledge-Edge Pairs (008)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-008-001 — When zero predicates fire in B32 vs B36: same SCHEDULE_CAPABILITY_LEARNING, different DomainId

**Knowledge edge:** Both B32 and B36 produce SCHEDULE_CAPABILITY_LEARNING when predicates = []. The mechanism is identical; only the DomainId differs.

### B32 zero-predicate example

**Request:** "Run a spike on the caching solution."

**Output:**
```
Predicates: []
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
GapIds: ["gap-vibe-coding-spike-001"]
```

### B36 zero-predicate example (for comparison)

**Request (hypothetical B36):** "Run a roadmapping session for Q3."

**Output:**
```
Predicates: []
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "project-management"
GapIds: ["gap-b36-roadmap-001"]
```

---

## B32-0C-008-002 — SCHEDULE_CAPABILITY_LEARNING: mechanism comparison

| Property | B32 | B36/B37 |
|---|---|---|
| Trigger condition | predicates = [] | predicates = [] |
| DomainId | software-development | project-management |
| Gap ID format | gap-vibe-coding-{term}-001 | gap-b36-{term}-001 or gap-b37-{term}-001 |
| Learning pipeline | software-dev capability expansion | project-mgmt capability expansion |
| Fires once per request | yes | yes |

---

## B32-0C-008-003 — The DomainId determines learning pipeline destination

**When SCHEDULE_CAPABILITY_LEARNING fires:**

```
DomainId: "software-development"
  → software-development learning pipeline
  → extends software-development predicate lexicons
  → may add "spike", "vibe", "hack" to recognized vocabulary over time

DomainId: "project-management"  
  → project-management learning pipeline
  → extends project-management predicate lexicons
  → may add "roadmap", "sprint", "velocity" to recognized vocabulary over time
```

Misassigning DomainId sends learning signals to the wrong pipeline and delays capability acquisition in the correct domain.

---

## B32-0C-008-004 — Mixed scenario: B32 gap + B36 gap in same request (edge case)

**Hypothetical:** "Spike the project roadmap planning."

Analysis:
- "spike" → B32 gap; gap-vibe-coding-spike-001
- "roadmap" → B36 gap (if recognized)
- "planning" → not in any registered predicate lexicon

DomainId assignment: "software-development" (B32 context dominates when vibe vocabulary present and no registered B36 predicates fire). The scenario is categorized by its primary framing.

---
