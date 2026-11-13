# Layer 0-B — Router Pairs (014)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-014-001 — [] → knowledge_edge: "rough cut" alone

**Predicates received:** `[]`

**Gap context:** gap-vibe-coding-rough-cut-001

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
GapWarnings: [gap-vibe-coding-rough-cut-001]
```

---

## B32-0B-014-002 — [IMPLEMENT] → IMPLEMENT flow: "rough cut build" context

**Predicates received:** `["IMPLEMENT"]`

**Gap context:** gap-vibe-coding-rough-cut-001

**Router decision:**

- IMPLEMENT present → ImplementationFlow selected
- SCHEDULE_CAPABILITY_LEARNING NOT emitted (predicates non-empty)

**Output:**
```
RouteTarget: ImplementationFlow
FlowSelected: ImplementationFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-rough-cut-001]
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0B-014-003 — [ARCHITECTURE] → ARCHITECTURE flow: "rough cut design" context

**Predicates received:** `["ARCHITECTURE"]`

**Gap context:** gap-vibe-coding-rough-cut-001

**Router decision:**

- ARCHITECTURE → ArchitectureFlow31
- Rough-cut is warning only; routing unaffected

**Output:**
```
RouteTarget: ArchitectureFlow31
FlowSelected: ArchitectureFlow31
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-rough-cut-001]
```

---

## B32-0B-014-004 — [IMPLEMENT, UI] → dual flow: "rough cut screen build"

**Predicates received:** `["IMPLEMENT", "UI"]`

**Gap context:** gap-vibe-coding-rough-cut-001

**Router decision:**

- IMPLEMENT → ImplementationFlow
- UI → UIFlow
- Both selected; gap warning does not suppress either

**Output:**
```
RouteTarget: [ImplementationFlow, UIFlow]
FlowSelected: [ImplementationFlow, UIFlow]
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-rough-cut-001]
```

---
