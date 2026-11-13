# Layer 0-B — Router Pairs (018)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-018-001 — [] → knowledge_edge: "throwaway" alone

**Predicates received:** `[]`

**Gap context:** gap-vibe-coding-throwaway-001

**Router decision:**

- Predicates empty → knowledge_edge
- SCHEDULE_CAPABILITY_LEARNING emitted

**Output:**
```
RouteTarget: knowledge_edge
FlowSelected: none
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
GapWarnings: [gap-vibe-coding-throwaway-001]
```

---

## B32-0B-018-002 — [] → knowledge_edge: "throwaway + prototype + demo"

**Predicates received:** `[]`

**Gap context:** gap-vibe-coding-throwaway-001, gap-vibe-coding-prototype-001, gap-vibe-coding-demo-001

**Router decision:**

- Predicates empty → knowledge_edge
- Three gap IDs; SCHEDULE_CAPABILITY_LEARNING emitted once

**Output:**
```
RouteTarget: knowledge_edge
FlowSelected: none
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
GapWarnings: [gap-vibe-coding-throwaway-001, gap-vibe-coding-prototype-001, gap-vibe-coding-demo-001]
```

---

## B32-0B-018-003 — [IMPLEMENT] → IMPLEMENT flow: "throwaway write" context

**Predicates received:** `["IMPLEMENT"]`

**Gap context:** gap-vibe-coding-throwaway-001

**Router decision:**

- IMPLEMENT → ImplementationFlow
- Gap warning logged

**Output:**
```
RouteTarget: ImplementationFlow
FlowSelected: ImplementationFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-throwaway-001]
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0B-018-004 — "throwaway UI" (UI alone abbreviation, no UI lexicon trigger) → [] → knowledge_edge

**Predicates received:** `[]`

**Gap context:** gap-vibe-coding-throwaway-001

**Router decision:**

- "UI" abbreviation and "panel" are not UI lexicon triggers; UI predicate never fired
- Predicates empty → knowledge_edge

**Output:**
```
RouteTarget: knowledge_edge
FlowSelected: none
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
GapWarnings: [gap-vibe-coding-throwaway-001]
```

**Teaching note:** UI fires only on: screen, page, interface, component, form, layout, button, input, widget, modal. Not on "UI" as an abbreviation.

---
