# Layer 0-B — Router Pairs (013)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-013-001 — [] → knowledge_edge: "scrappy" alone context

**Predicates received:** `[]`

**Gap context:** gap-vibe-coding-style-001 ("scrappy")

**Router decision:**

Step 1 — predicate list empty → no flow can be selected
Step 2 — gap-vibe-coding-style-001 confirmed → B32 gap context
Step 3 — DomainId: "software-development"
Step 4 — SCHEDULE_CAPABILITY_LEARNING emitted once

**Output:**
```
RouteTarget: knowledge_edge
FlowSelected: none
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
GapWarnings: [gap-vibe-coding-style-001]
```

---

## B32-0B-013-002 — [IMPLEMENT] → IMPLEMENT flow: "scrappy build" context

**Predicates received:** `["IMPLEMENT"]`

**Gap context:** gap-vibe-coding-style-001 ("scrappy")

**Router decision:**

Step 1 — IMPLEMENT present → ImplementationFlow selected
Step 2 — gap warnings logged (do not affect routing)
Step 3 — SCHEDULE_CAPABILITY_LEARNING NOT emitted (predicates non-empty)

**Output:**
```
RouteTarget: ImplementationFlow
FlowSelected: ImplementationFlow
DomainId: "software-development"
CapabilityAction: none
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0B-013-003 — [ARCHITECTURE] → ARCHITECTURE flow: "scrappy design" context

**Predicates received:** `["ARCHITECTURE"]`

**Gap context:** gap-vibe-coding-style-001 ("scrappy")

**Router decision:**

- ARCHITECTURE present → ArchitectureFlow31 selected
- Gap warning logged; routing unaffected

**Output:**
```
RouteTarget: ArchitectureFlow31
FlowSelected: ArchitectureFlow31
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0B-013-004 — [IMPLEMENT, TEST] → dual flow: "scrappy build + test" context

**Predicates received:** `["IMPLEMENT", "TEST"]`

**Gap context:** gap-vibe-coding-style-001

**Router decision:**

- IMPLEMENT + TEST → ImplementationFlow + TestFlow
- Both flows selected; gap warning does not suppress either

**Output:**
```
RouteTarget: [ImplementationFlow, TestFlow]
FlowSelected: [ImplementationFlow, TestFlow]
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
```

---
