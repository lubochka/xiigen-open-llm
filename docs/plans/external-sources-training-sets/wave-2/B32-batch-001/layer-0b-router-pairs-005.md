# Layer 0-B — Router Pairs (005)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0b-router-pairs

---

## B32-0B-005-001 — [] → knowledge_edge: dashboard not in UI lexicon

**Predicates received:** `[]`

**Source request:** "Throw together a quick dashboard for the admin panel — nothing fancy."

**Gap warnings present:** gap-vibe-coding-hack-001, gap-vibe-coding-dashboard-001, gap-vibe-coding-nothing-fancy-001

**Router decision:**

Step 1 — predicates empty → no flow can be selected
Step 2 — gap warnings include gap-vibe-coding-dashboard-001 → system recognizes attempted UI reference
Step 3 — "dashboard" is NOT in UI lexicon → UI flow not triggered even in router
Step 4 — Route to knowledge_edge; emit SCHEDULE_CAPABILITY_LEARNING

**Output:**
```
RouteTarget: knowledge_edge
FlowSelected: none
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
GapWarnings: [gap-vibe-coding-hack-001, gap-vibe-coding-dashboard-001, gap-vibe-coding-nothing-fancy-001]
```

**Important:** The router does NOT retroactively map "dashboard" to UIFlow. The gap was established at compiler stage; router respects the compiler output.

---

## B32-0B-005-002 — Router gap-awareness: dashboard gap in routing context

**Rule:** When gap-vibe-coding-dashboard-001 is present in warnings but predicates = [], the router:
1. Routes to knowledge_edge (mandatory for empty predicates)
2. Emits SCHEDULE_CAPABILITY_LEARNING with DomainId "software-development"
3. Does NOT attempt to infer "dashboard → UI" at routing stage
4. Passes all gap warnings downstream for learning pipeline consumption

---

## B32-0B-005-003 — Contrast: dashboard + screen → UI fires; routing changes

**Predicates received:** `["UI", "IMPLEMENT"]`

**Source request:** "Build a dashboard with a main screen showing the metrics."

**Gap warnings present:** gap-vibe-coding-dashboard-001

**Router output:**
```
RouteTarget: UIFlow + BackendServiceFlow41
FlowSelected: [UIFlow, BackendServiceFlow41]
TransitionType: feature_addition
GapWarnings: [gap-vibe-coding-dashboard-001]
DomainId: "software-development"
```

**Contrast with 005-001:** Adding "screen" (registered UI lexicon word) changes routing from knowledge_edge to UIFlow + BackendServiceFlow41. The "dashboard" still generates a gap warning.

---

## B32-0B-005-004 — knowledge_edge routing: no contribution flag

**When predicates = [] and routing = knowledge_edge:**

TransitionType: unknown (cannot determine transition type without predicates)
ContributionFlag: false (no code/architecture contribution possible)
LearningFlag: true (SCHEDULE_CAPABILITY_LEARNING emitted)

This is consistent across all B32 zero-predicate cases regardless of which gap IDs are present.

---
