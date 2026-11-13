# Layer 0-D — Flow Factory Pairs (024)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-024-001 — Complete factory selection reference for B32

**Factory-to-predicate mapping:**

| Predicate | Factory | Flow created |
|---|---|---|
| IMPLEMENT | ImplementationFactory | ImplementationFlow |
| UI | UIFactory | UIFlow |
| ARCHITECTURE | ArchitectureFactory | ArchitectureFlow31 |
| API_CONTRACT | APIContractFactory | APIContractFlow |
| SECURITY | SecurityFactory | SecurityFlow |
| TEST | TestFactory | TestFlow |
| DOCUMENT | DocumentFactory | DocumentFlow |
| QA | QAFactory | QAFlow |
| [] (empty) | none | none → knowledge_edge |

---

## B32-0D-024-002 — Factory bypass conditions in B32

**Factory is bypassed (knowledge_edge) when:**
- Predicates = [] regardless of gap warning count
- All B32 zero-predicate patterns trigger bypass:
  - spike alone
  - prototype alone
  - dashboard alone
  - vibe alone (without "code")
  - hack together alone
  - throwaway alone
  - rough cut alone
  - any combination of gap vocabulary with no action verbs

**CapabilityGapWarning is added to flow metadata (not bypass) when:**
- Predicates are non-empty
- Gap vocabulary was also present in the request
- Field value: "style_qualifier_noted"

DomainId in factory output: always "software-development" for B32.

---
