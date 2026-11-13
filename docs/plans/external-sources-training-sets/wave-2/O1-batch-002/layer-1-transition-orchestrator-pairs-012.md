# Layer 1 — Transition Orchestrator Pairs (012)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: orchestration

---

## O1b-L1-012-001 — Complete orchestration rules: execution model

A: O1b orchestration execution models

B: | Flows | Execution Model |
   |-------|----------------|
   | One flow | single |
   | Two independent flows (e.g., 3.4+3.3) | parallel (‖) |
   | 3.4 + 4.1 | sequential (3.4 first, 4.1 uses 3.4 output) |
   | 3.4 + 5.1 | sequential (3.4 first, 5.1 uses 3.4 output) |
   | Any flow + QA | sequential (QA absolute last) |
   | Zero flows | none (knowledge_edge) |

---

## O1b-L1-012-002 — Complete orchestration rules: flow count

A: O1b flow count rules

B: Flow count = distinct sub-phases in routing decisions. SECURITY alone → 1 flow. SECURITY+AUTH → 1 flow (dedup). SECURITY+API_CONTRACT → 2 flows. SECURITY+AUTH+API_CONTRACT → 2 flows (dedup within 3.4). All predicates → maximum 5 flows + QA gate. knowledge_edge → 0 flows.

---

## O1b-L1-012-003 — Complete orchestration rules: QA placement

A: QA orchestration placement rule

B: QA (when present): always appended as the final step after all domain flows complete. QA is the only predicate that maps to an orchestration step (QAGatingFlow73) but not to a routing decision (no sub-phase). QA reviews assembled result from all domain flows.

---

## O1b-L1-012-004 — Complete orchestration rules: knowledge_edge

A: knowledge_edge orchestration rule

B: knowledge_edge → OrchestrationPlan { flows: [], execution: "none" }. No exceptions. No flow runs for knowledge_edge. The prompt is logged to unroutable corpus. This is the correct and expected behavior, not an error.

---

## O1b-L1-012-005 — Complete orchestration rules: TransitionType effect

A: How does TransitionType affect orchestration?

B: TransitionType affects the nine-node pipeline MODE within each flow: feature_addition → pipeline produces new feature design. documentation → pipeline produces spec/doc output. refactoring → pipeline produces refactor plan. knowledge_edge → no pipeline. TransitionType does NOT change which flows are selected (that is determined by routing decisions). It changes how the DomainProcessorUnit and FormatterUnit within each flow interpret their inputs.
