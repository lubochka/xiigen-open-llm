# Layer 0-D — Flow Factory Pairs (007)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: flow-factory-pairs

---

## B37-0D-007-001 — v6 factory corruption: MANAGEMENT/COORDINATION flow invented

**Corrupted output (v6 model):**
Request: "Coordinate the workstreams and track delivery progress."
v6 factory output: ManagementFlow or CoordinationFlow selected

**Correct factory output:**
- Zero predicates → no factory selected
- Factory registry not consulted
- knowledge_edge activated
- SCHEDULE_CAPABILITY_LEARNING emitted

v6 error type: v6-01 — invented factory does not exist in registry.

**Registry verification:** The factory registry contains: DocumentationFlow, ArchitectureFlow31, APIContractFlow33, BackendServiceFlow41, TestFlow5x, QAGatingFlow, ReviewFlow. No managing-specific flows exist.

---

## B37-0D-007-002 — v6 factory corruption: DOCUMENT factory suppressed

**Corrupted output (v6 model):**
Request: "Update stakeholders on the current RAG-status."
v6 factory output: knowledge_edge (DocumentationFlow suppressed)

**Correct factory output:**
- "stakeholders" → DOCUMENT fires → DocumentationFlow selected
- knowledge_edge NOT activated (predicate fired)
- DomainId: "software-development"
- Factory: DocumentationFlow

v6 error type: v6-02 — factory selection suppressed because context is managing.

---

## B37-0D-007-003 — v6 factory corruption: wrong DomainId with factory active

**Corrupted output (v6 model):**
Request: "Build the project management dashboard service."
v6 factory output: BackendServiceFlow41, DomainId: "project-management"

**Correct factory output:**
- IMPLEMENT (build, service) → BackendServiceFlow41
- DomainId: "software-development" (predicate fired)

v6 error type: v6-03 — DomainId set to "project-management" while factory is active. A factory can only be active when a predicate fires. A predicate firing mandates DomainId "software-development".

---

## B37-0D-007-004 — Factory corruption detection pattern

**Three-step detection for v6 factory corruptions:**

Step 1 (v6-01 detection): Is the selected factory in the registry?
- Registry: DocumentationFlow, ArchitectureFlow31, APIContractFlow33, BackendServiceFlow41, TestFlow5x, QAGatingFlow, ReviewFlow
- If factory name is not in this list → v6-01 corruption

Step 2 (v6-02 detection): Is DocumentationFlow absent when "stakeholder" appears?
- If sentence contains "stakeholder" AND DocumentationFlow not selected → v6-02 corruption

Step 3 (v6-03 detection): Is DomainId "project-management" while a factory is active?
- If any factory is active AND DomainId = "project-management" → v6-03 corruption

---

## B37-0D-007-005 — Factory selection rules for all B37 predicates: complete table

| Predicate | Factory | Sub-phase |
|----------|---------|---------|
| DOCUMENT | DocumentationFlow | 2.1 |
| ARCHITECTURE | ArchitectureFlow31 | 3.1 |
| API_CONTRACT | APIContractFlow33 | 3.3 |
| IMPLEMENT | BackendServiceFlow41 | 4.1 |
| TEST | TestFlow5x | 5.x |
| QA (review) | ReviewFlow | QA sub-phase |
| QA (validate) | QAGatingFlow | QA sub-phase |
| [] (zero predicates) | No factory | knowledge_edge |

**Combined predicates:** All matching factories are selected in parallel (except Test+QA which is ordered).
