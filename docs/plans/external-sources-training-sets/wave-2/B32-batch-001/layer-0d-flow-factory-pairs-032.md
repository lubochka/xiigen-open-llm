# Layer 0-D — Flow Factory Pairs (032)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-032-001 — gap + IMPLEMENT + SECURITY: two factories

**Request:** "Lean sprint — implement auth flow and secure tokens."

```
Input: Predicates = ["IMPLEMENT", "SECURITY"]
GapWarnings: [gap-vibe-coding-style-001]

Factory 1: ImplementationFactory → ImplementationFlow
Factory 2: SecurityFactory → SecurityFlow
GapMetadata applied to both: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-032-002 — "hack together" gap + SECURITY: SecurityFactory with hack gap ID

**Request:** "Hack together a security fix — secure the login form."

```
Input: Predicates = ["SECURITY"]
GapWarnings: [gap-vibe-coding-hack-001]

Factory selected: SecurityFactory
Flow created: SecurityFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-hack-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

Gap ID is hack-001, not style-001. Factory selection is identical; gap metadata differs.

---

## B32-0D-032-003 — gap + ARCHITECTURE + SECURITY: two factories

**Request:** "Expedient design — architect the auth service and secure the API."

```
Input: Predicates = ["ARCHITECTURE", "SECURITY"]
GapWarnings: [gap-vibe-coding-style-001]

Factory 1: ArchitectureFactory → ArchitectureFlow31
Factory 2: SecurityFactory → SecurityFlow
GapMetadata applied to both: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-032-004 — "think about security" alone: no factory

**Request:** "Lean approach — think about security."

```
Input: Predicates = []
Factory selected: none
Flow created: none
Route: knowledge_edge
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

---
