# Layer 0-B — Router Pairs (016)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-016-001 — [] → knowledge_edge: "hack together" alone

**Predicates received:** `[]`

**Gap context:** gap-vibe-coding-hack-001

**Router decision:**

- Predicates empty → knowledge_edge
- DomainId: "software-development"
- SCHEDULE_CAPABILITY_LEARNING emitted

**Output:**
```
RouteTarget: knowledge_edge
FlowSelected: none
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
GapWarnings: [gap-vibe-coding-hack-001]
```

---

## B32-0B-016-002 — [] → knowledge_edge: "cobble together" synonym for hack

**Predicates received:** `[]`

**Gap context:** gap-vibe-coding-hack-001 ("cobble together")

**Router decision:**

- gap-vibe-coding-hack-001 is the same gap ID for "cobble together" and "hack together"
- Predicates empty → knowledge_edge

**Output:**
```
RouteTarget: knowledge_edge
FlowSelected: none
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
GapWarnings: [gap-vibe-coding-hack-001]
```

**Note:** "cobble together", "throw together", and "hack together" all map to gap-vibe-coding-hack-001. The router receives the same gap ID regardless of which synonym triggered it.

---

## B32-0B-016-003 — [IMPLEMENT] → IMPLEMENT flow: "hack together + build" context

**Predicates received:** `["IMPLEMENT"]`

**Gap context:** gap-vibe-coding-hack-001, gap-vibe-coding-dashboard-001, gap-vibe-coding-nothing-fancy-001

**Router decision:**

- IMPLEMENT present → ImplementationFlow
- Multiple gap warnings; routing unaffected by count of gap IDs

**Output:**
```
RouteTarget: ImplementationFlow
FlowSelected: ImplementationFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-hack-001, gap-vibe-coding-dashboard-001, gap-vibe-coding-nothing-fancy-001]
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0B-016-004 — Multiple hack synonyms; still single gap ID

**Rule:** No matter how many hack-synonym phrases appear in a single request, they all resolve to gap-vibe-coding-hack-001. The gap ID list deduplicates.

**Example:**
```
Request: "Hack together and cobble together and throw together a solution."
Gap IDs detected: gap-vibe-coding-hack-001 (deduplicated from 3 triggers)
SCHEDULE_CAPABILITY_LEARNING: emitted once
```

---
