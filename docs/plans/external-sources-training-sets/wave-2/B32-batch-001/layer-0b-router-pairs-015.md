# Layer 0-B — Router Pairs (015)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-015-001 — [] → knowledge_edge: "quick win" alone

**Predicates received:** `[]`

**Gap context:** gap-vibe-coding-style-001 ("quick win")

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

## B32-0B-015-002 — [IMPLEMENT] + gap warning: "quick win build" — IMPLEMENT fires

**Predicates received:** `["IMPLEMENT"]`

**Gap context:** gap-vibe-coding-style-001 ("quick win")

**Router decision:**

- IMPLEMENT present → ImplementationFlow
- Gap warning logged as CapabilityGapWarning
- SCHEDULE_CAPABILITY_LEARNING NOT emitted

**Output:**
```
RouteTarget: ImplementationFlow
FlowSelected: ImplementationFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
```

**Routing rule:** "quick win" style qualifier is noted but does not change the flow selected.

---

## B32-0B-015-003 — [DOCUMENT] → DOCUMENT flow: "quick win document" context

**Predicates received:** `["DOCUMENT"]`

**Gap context:** gap-vibe-coding-style-001

**Router decision:**

- DOCUMENT → DocumentFlow
- Gap warning logged

**Output:**
```
RouteTarget: DocumentFlow
FlowSelected: DocumentFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
```

---

## B32-0B-015-004 — [TEST] → TEST flow: "quick win test" context

**Predicates received:** `["TEST"]`

**Gap context:** gap-vibe-coding-style-001

**Router decision:**

- TEST → TestFlow
- "Quick win" framing noted; routing unaffected

**Output:**
```
RouteTarget: TestFlow
FlowSelected: TestFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
```

---
