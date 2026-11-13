# Layer 0-B — Router Pairs (020)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-020-001 — [IMPLEMENT] → IMPLEMENT: gap + IMPLEMENT mixed ("just make it work + build")

**Predicates received:** `["IMPLEMENT"]`

**Gap context:** gap-vibe-coding-just-works-001, gap-vibe-coding-nothing-fancy-001

**Router decision:**

- IMPLEMENT present → ImplementationFlow selected
- Two gap warnings; neither suppresses routing
- SCHEDULE_CAPABILITY_LEARNING NOT emitted (predicates non-empty)

**Output:**
```
RouteTarget: ImplementationFlow
FlowSelected: ImplementationFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-just-works-001, gap-vibe-coding-nothing-fancy-001]
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0B-020-002 — [IMPLEMENT] → IMPLEMENT: "quick and dirty + implement"

**Predicates received:** `["IMPLEMENT"]`

**Gap context:** gap-vibe-coding-quick-dirty-001

**Router decision:**

- IMPLEMENT → ImplementationFlow
- Gap warning noted

**Output:**
```
RouteTarget: ImplementationFlow
FlowSelected: ImplementationFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-quick-dirty-001]
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0B-020-003 — [IMPLEMENT] → IMPLEMENT: "vibe + create" context

**Predicates received:** `["IMPLEMENT"]`

**Gap context:** gap-vibe-coding-style-001

**Router decision:**

- IMPLEMENT → ImplementationFlow

**Output:**
```
RouteTarget: ImplementationFlow
FlowSelected: ImplementationFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0B-020-004 — [IMPLEMENT] → IMPLEMENT: "hack together + write" context

**Predicates received:** `["IMPLEMENT"]`

**Gap context:** gap-vibe-coding-hack-001

**Router decision:**

- IMPLEMENT → ImplementationFlow

**Output:**
```
RouteTarget: ImplementationFlow
FlowSelected: ImplementationFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-hack-001]
```

---
