# Layer 0-D — Flow Factory Pairs (041)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-041-001 — IMPLEMENT + TEST + DOCUMENT: three factories

**Request:** "Lean sprint — implement, write tests, document the API."

```
Input: Predicates = ["IMPLEMENT", "TEST", "DOCUMENT"]
GapWarnings: [gap-vibe-coding-style-001]

Factory 1: ImplementationFactory → ImplementationFlow
Factory 2: TestFactory → TestFlow
Factory 3: DocumentFactory → DocumentFlow
GapMetadata applied to all three: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-041-002 — ARCHITECTURE + IMPLEMENT + API_CONTRACT: three factories

**Request:** "Pragmatic — architect, implement, define contract."

```
Input: Predicates = ["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT"]
GapWarnings: [gap-vibe-coding-style-001]

Factory 1: ArchitectureFactory → ArchitectureFlow31
Factory 2: ImplementationFactory → ImplementationFlow
Factory 3: APIContractFactory → APIContractFlow
GapMetadata applied to all three: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-041-003 — all eight predicates: eight factories

**Request:** "Expedient sprint — all eight predicates."

```
Input: Predicates = ["IMPLEMENT", "UI", "ARCHITECTURE", "API_CONTRACT", "SECURITY", "TEST", "DOCUMENT", "QA"]
GapWarnings: [gap-vibe-coding-style-001]

Eight factories invoked → eight flows created
GapMetadata applied to all eight: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-041-004 — TEST + DOCUMENT (no IMPLEMENT): TestFactory + DocumentFactory

**Request:** "Lean — write tests and document the API."

```
Input: Predicates = ["TEST", "DOCUMENT"]
GapWarnings: [gap-vibe-coding-style-001]

Factory 1: TestFactory → TestFlow
Factory 2: DocumentFactory → DocumentFlow
GapMetadata applied to both: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---
