# Layer 1 — Transition Orchestrator Pairs (009)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B12-L1-009-001 — QA last rule in API context: detailed example

```
source_id: xiigen-b12-l1-009-001
pair_kind: transition_orchestrator
```

QA position rule: QA runs AFTER ALL flows complete. Never between flows. Never before flows.

Example for [API_CONTRACT, DOCUMENT, SECURITY]:

WRONG orchestration (QA inserted between flows):
  1. APIContractFlow33
  2. QA ← WRONG (QA inserted before all flows complete)
  3. DocumentationFlow21
  4. SecurityAuditFlow34
  5. QA ← WRONG (second QA — only one QA checkpoint per orchestration)

CORRECT orchestration:
  1. APIContractFlow33 (sequential)
  2. DocumentationFlow21 (parallel) + SecurityAuditFlow34 (parallel)
  3. QA ← CORRECT (after all flows complete)

Rationale for QA-last rule:
  - QA validates the COMPLETE set of outputs
  - QA cannot validate documentation until the contract that it documents is complete
  - QA cannot validate security requirements until both contract AND security review complete
  - Early QA would have incomplete information → false positives or false negatives

---

## B12-L1-009-002 — QA checkpoint: what it checks in B12 API context

```
source_id: xiigen-b12-l1-009-002
pair_kind: transition_orchestrator
```

QA checkpoint verification for B12 API contract documentation:

Mandatory QA checks:
  1. Contract completeness
     - All referenced endpoints have complete specifications
     - All request/response schemas are defined
     - No undefined references in the contract

  2. Documentation coverage
     - All endpoints in contract have corresponding documentation
     - No documentation orphans (docs for non-existent endpoints)

  3. Predicate fulfillment
     - API_CONTRACT satisfied: contract file exists and is valid
     - DOCUMENT satisfied: documentation file exists
     - If SECURITY: authentication section present in contract
     - If AUTHORIZATION: authorization scope definitions present

  4. Format validation
     - OpenAPI spec: valid YAML/JSON structure
     - GraphQL schema: valid .graphql syntax
     - gRPC contract: valid .proto syntax

  5. Consistency
     - No conflicts between contract and documentation
     - Version numbers consistent across all files

---

## B12-L1-009-003 — QA last: why security flows cannot precede contract flows

```
source_id: xiigen-b12-l1-009-003
pair_kind: transition_orchestrator
```

Dependency justification for SecurityAuditFlow34 position:

SecurityAuditFlow34 needs APIContractFlow33 output because:
  - It cannot review "authentication requirements" until those requirements are documented
  - The API contract defines WHAT authentication is being used
  - SecurityAuditFlow34 reviews the specific authentication scheme defined in the contract

Wrong ordering:
  1. SecurityAuditFlow34 ← starts before contract is defined
     Problem: what does it review? Authentication requirements don't exist yet.
  2. APIContractFlow33 ← defines authentication after security review

Correct ordering:
  1. APIContractFlow33 ← defines authentication requirements
  2. SecurityAuditFlow34 ← reviews the defined authentication requirements

This is why SecurityAuditFlow34 is in Phase 2 (after APIContractFlow33 completes in Phase 1).
The dependency is structural: no contract → nothing to review.

---

## B12-L1-009-004 — Orchestrator plan notation reference for B12

```
source_id: xiigen-b12-l1-009-004
pair_kind: transition_orchestrator
```

B12 orchestrator plan notation reference:

TransitionType field:
  "documentation" — updating/specifying existing system
  "feature_addition" — creating new artifact

ExecutionPlan structure:
  Phase1_Sequential: flows that must run before any others
  Phase1_Parallel: flows that can run simultaneously (no dependencies)
  Phase2_Sequential: flows that depend on Phase1 (run sequentially after Phase1)
  Phase2_Parallel: flows that depend on Phase1 but not each other (run simultaneously)
  QACheckpoint: always after all phases complete

ParallelWith field: identifies which flows can run alongside this flow
DependsOn field: identifies which flow outputs this flow requires

EstimatedOutputs: minimum number of files expected across all flows

QACheckpoint position: always LAST — after all flows in ExecutionPlan complete

DecisionCount: total predicates in CompiledPredicateIds (not unique sub-phases)
  Five predicates → DecisionCount: 5 (even if two route to same sub-phase)
