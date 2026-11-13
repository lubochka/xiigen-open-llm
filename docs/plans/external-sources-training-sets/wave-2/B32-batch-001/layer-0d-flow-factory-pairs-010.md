# Layer 0-D — Flow Factory Pairs (010)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-010-001 — ARCHITECTURE+API_CONTRACT factory: two factories in vibe context

**Predicates:** `["ARCHITECTURE", "API_CONTRACT"]`

**Source request:** "Rough-cut integration design — architect the API gateway and define the contract for the payment service."

**Gap warnings:** gap-vibe-coding-rough-cut-001

**Factory selection:**

```
Factories selected: [ArchitectureFlow31, APIContractFlow]
Reason for ArchitectureFlow31: "architect" fires ARCHITECTURE
Reason for APIContractFlow: "integrate" / "contract" fires API_CONTRACT
GapMetadata: {rough-cut: true}
ParallelEligible: true (architecture and API contract work can proceed concurrently)
TransitionType: documentation (no IMPLEMENT present)
DomainId: "software-development"
```

---

## B32-0D-010-002 — ARCHITECTURE+API_CONTRACT: execution pattern

```
Phase 1 (parallel): ArchitectureFlow31, APIContractFlow
  - ArchitectureFlow31 produces: system architecture document
  - APIContractFlow produces: API contract specification
Phase 2: Neither depends on the other → no Phase 2 required
Output: [architecture doc, API contract spec] + gap metadata
```

**Note:** When IMPLEMENT is not present, there is no code production. The output is documentation-type artifacts (architecture diagrams, API contracts). TransitionType: documentation.

---

## B32-0D-010-003 — ARCHITECTURE+API_CONTRACT+IMPLEMENT: three factories

**Predicates:** `["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT"]`

**Source request:** "Design, define, and build the payment gateway integration."

**Factory selection:**

```
Factories selected: [ArchitectureFlow31, APIContractFlow, BackendServiceFlow41]
Phase 1 (parallel): ArchitectureFlow31, APIContractFlow
Phase 2 (after Phase 1 produces output): BackendServiceFlow41
TransitionType: feature_addition
```

---

## B32-0D-010-004 — Factory output types by predicate

| Factory | Output type |
|---|---|
| ArchitectureFlow31 | Architecture document/diagram |
| APIContractFlow | API specification/contract |
| BackendServiceFlow41 | Service implementation code |
| UIFlow | UI component/screen code |
| SecurityFlow | Security implementation/audit |
| TestFlow | Test suite |
| DocumentFlow | Documentation artifact |
| QAFlow | QA report/checklist |

Gap framing does not change output type — it may reduce output scope or add prototype notes.

---
