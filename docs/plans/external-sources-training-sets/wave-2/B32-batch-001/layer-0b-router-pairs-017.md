# Layer 0-B — Router Pairs (017)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-017-001 — [] → knowledge_edge: "low-fi" alone

**Predicates received:** `[]`

**Gap context:** gap-vibe-coding-style-001 ("low-fi")

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
GapWarnings: [gap-vibe-coding-style-001]
```

---

## B32-0B-017-002 — [IMPLEMENT, UI] → dual flow: "low-fi build + page" context

**Predicates received:** `["IMPLEMENT", "UI"]`

**Gap context:** gap-vibe-coding-style-001 ("low-fi")

**Router decision:**

- IMPLEMENT → ImplementationFlow
- UI → UIFlow
- Gap warning logged; does not suppress either flow

**Output:**
```
RouteTarget: [ImplementationFlow, UIFlow]
FlowSelected: [ImplementationFlow, UIFlow]
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0B-017-003 — [] → knowledge_edge: "low-fi prototype" — both are gap

**Predicates received:** `[]`

**Gap context:** gap-vibe-coding-style-001, gap-vibe-coding-prototype-001

**Router decision:**

- Predicates empty → knowledge_edge
- Two gap IDs; SCHEDULE_CAPABILITY_LEARNING emitted once

**Output:**
```
RouteTarget: knowledge_edge
FlowSelected: none
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-prototype-001]
```

---

## B32-0B-017-004 — [ARCHITECTURE, UI] → dual flow: "low-fi wireframe design + interface"

**Predicates received:** `["ARCHITECTURE", "UI"]`

**Gap context:** gap-vibe-coding-style-001

**Router decision:**

- ARCHITECTURE → ArchitectureFlow31
- UI → UIFlow
- Both selected; gap warning noted only

**Output:**
```
RouteTarget: [ArchitectureFlow31, UIFlow]
FlowSelected: [ArchitectureFlow31, UIFlow]
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
```

---
