# Layer 0-D — Flow Factory Pairs (028)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-028-001 — gap + SECURITY: SecurityFactory invoked despite gap

**Request:** "Quick-win security — secure the endpoints."

```
Input: Predicates = ["SECURITY"]
GapWarnings: [gap-vibe-coding-style-001]

Factory selected: SecurityFactory
Flow created: SecurityFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

Gap framing does not bypass SecurityFactory. SECURITY fires independently.

---

## B32-0D-028-002 — "iterative" + IMPLEMENT + TEST: two factories invoked

**Request:** "Iterative sprint — implement the module and test the output."

```
Input: Predicates = ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-style-001]

Factory 1: ImplementationFactory → ImplementationFlow
Factory 2: TestFactory → TestFlow
GapMetadata applied to both: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-028-003 — "iterative" alone: no factory

**Request:** "Let's be iterative."

```
Input: Predicates = []
GapWarnings: [gap-vibe-coding-style-001]

Factory selected: none
Flow created: none
Route: knowledge_edge
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

---

## B32-0D-028-004 — "iterative" + dashboard + IMPLEMENT: one factory, two gap IDs

**Request:** "Iterative sprint — implement the dashboard."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001]

Factory selected: ImplementationFactory
Flow created: ImplementationFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

Both gap IDs attached to the single flow's GapMetadata.

---
