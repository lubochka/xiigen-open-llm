# Layer 0-B — Router Pairs (053)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-053-001 — IMPLEMENT + TEST + DOCUMENT routing

**Request:** "Lean — implement, write tests, document the API."

```
Input: Predicates = ["IMPLEMENT", "TEST", "DOCUMENT"]
GapWarnings: [gap-vibe-coding-style-001]
IMPLEMENT → ImplementationFlow
TEST → TestFlow
DOCUMENT → DocumentFlow
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-053-002 — ARCHITECTURE + IMPLEMENT + API_CONTRACT routing

**Request:** "Pragmatic — architect, implement, define contract."

```
Input: Predicates = ["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT"]
GapWarnings: [gap-vibe-coding-style-001]
ARCHITECTURE → ArchitectureFlow31
IMPLEMENT → ImplementationFlow
API_CONTRACT → APIContractFlow
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-053-003 — all eight predicates routing

**Request:** "Expedient sprint — all predicates fired."

```
Input: Predicates = ["IMPLEMENT", "UI", "ARCHITECTURE", "API_CONTRACT", "SECURITY", "TEST", "DOCUMENT", "QA"]
GapWarnings: [gap-vibe-coding-style-001]
All eight factories invoked → eight flows created
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-053-004 — IMPLEMENT + TEST (with negations) routing

**Request:** "Lean — implement, test, skip docs, skip architecture."

```
Input: Predicates = ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-style-001]
IMPLEMENT → ImplementationFlow
TEST → TestFlow
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---
