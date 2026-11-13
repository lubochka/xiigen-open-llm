# Layer 0-B — Router Pairs (002)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0b-router-pairs

---

## B32-0B-002-001 — [] → knowledge_edge routing (spike, zero predicates)

**Predicates received:** `[]`

**Router decision:**

Step 1 — check predicate list: empty → no flow can be selected
Step 2 — check gap warnings: gap-vibe-coding-spike-001 present → confirms B32 gap context
Step 3 — DomainId check: "software-development" → confirms B32 routing path
Step 4 — emit SCHEDULE_CAPABILITY_LEARNING for the gap domain

**Output:**
```
RouteTarget: knowledge_edge
FlowSelected: none
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
GapWarnings: [gap-vibe-coding-spike-001, gap-vibe-coding-exploratory-001]
```

**Routing rule:** When predicates = [], the router always routes to knowledge_edge regardless of the semantic content of the request.

---

## B32-0B-002-002 — [] → knowledge_edge: DomainId "software-development" confirmed

**Predicates received:** `[]`

**Gap context:** gap-vibe-coding-prototype-001 ("prototype" alone, no action verb)

**Router decision:**

- Predicates empty → knowledge_edge
- DomainId: "software-development" (B32, not B36/B37)
- SCHEDULE_CAPABILITY_LEARNING emitted once per request

**Output:**
```
RouteTarget: knowledge_edge
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
GapWarnings: [gap-vibe-coding-prototype-001]
```

---

## B32-0B-002-003 — [] → knowledge_edge: dashboard gap context

**Predicates received:** `[]`

**Gap context:** gap-vibe-coding-dashboard-001, gap-vibe-coding-hack-001, gap-vibe-coding-nothing-fancy-001

**Router decision:**

- Predicates empty → knowledge_edge
- DomainId: "software-development"
- SCHEDULE_CAPABILITY_LEARNING emitted

**Output:**
```
RouteTarget: knowledge_edge
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
GapWarnings: [gap-vibe-coding-dashboard-001, gap-vibe-coding-hack-001, gap-vibe-coding-nothing-fancy-001]
```

---

## B32-0B-002-004 — Router: SCHEDULE_CAPABILITY_LEARNING emitted exactly once per request

**Rule:** No matter how many gap IDs are present in a single request, SCHEDULE_CAPABILITY_LEARNING is emitted exactly once. Multiple gap warnings do not produce multiple SCHEDULE_CAPABILITY_LEARNING actions.

**Example:**
```
Request: "Spike, vibe code, hack together, nothing fancy, throwaway prototype."
GapWarnings: [spike, vibe, hack, nothing-fancy, throwaway, prototype] — 6 warnings
SCHEDULE_CAPABILITY_LEARNING: emitted once (not 6 times)
```

---
