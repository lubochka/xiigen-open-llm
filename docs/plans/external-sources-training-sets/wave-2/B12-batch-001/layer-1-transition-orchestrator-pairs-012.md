# Layer 1 — Transition Orchestrator Pairs (012)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B12-L1-012-001 — v6 ordering violations for B12: three examples

```
source_id: xiigen-b12-l1-012-001
corruption_type: v6-ordering
pair_kind: orchestrator_corruption
```

### v6 Ordering Violation 1: Security before contract

Request: "Document the webhook contract and authentication requirements."
CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]

CORRECT FlowOrder:
  [APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34]
  (contract first → documentation and security after)

v6-ORDERING CORRUPT FlowOrder:
  [SecurityAuditFlow34, APIContractFlow33, DocumentationFlow21]
  (security runs before contract — WRONG)

Why wrong: SecurityAuditFlow34 reviews authentication requirements defined in the contract.
If contract doesn't exist yet, SecurityAuditFlow34 has nothing to review.
This is a dependency violation: security must wait for contract.

### v6 Ordering Violation 2: Documentation before contract

Request: "Document the REST API endpoints and define the request/response schemas."
CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]

CORRECT FlowOrder:
  [APIContractFlow33, DocumentationFlow21]

v6-ORDERING CORRUPT FlowOrder:
  [DocumentationFlow21, APIContractFlow33]
  (documentation runs before contract — WRONG)

Why wrong: documentation describes the API contract. The contract must exist before it can
be documented. DocumentationFlow21 consumes APIContractFlow33 output — reverse ordering
violates this dependency.

### v6 Ordering Violation 3: Architecture after API contract

Request: "Design the API architecture and document the REST endpoints."
CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]

CORRECT FlowOrder:
  [ArchitectureFlow31, APIContractFlow33, DocumentationFlow21]

v6-ORDERING CORRUPT FlowOrder:
  [APIContractFlow33, ArchitectureFlow31, DocumentationFlow21]
  (API contract before architecture — WRONG)

Why wrong: APIContractFlow33 uses architectural decisions from ArchitectureFlow31.
Defining endpoints before the architecture is designed violates the design-then-specify order.

---

## B12-L1-012-002 — v6 ordering violations: detection patterns

```
source_id: xiigen-b12-l1-012-002
pair_kind: orchestrator_corruption
```

Detection patterns for v6 ordering violations:

Violation 1 detection:
  SecurityAuditFlow34 appears BEFORE APIContractFlow33 in FlowOrder
  Signal: any "SaF34" before "ACF33" in flow sequence

Violation 2 detection:
  DocumentationFlow21 appears BEFORE APIContractFlow33 in FlowOrder
  Signal: any "DF21" before "ACF33" in flow sequence

Violation 3 detection:
  APIContractFlow33 appears BEFORE ArchitectureFlow31 when both present
  Signal: "ACF33" before "AF31" when ARCHITECTURE predicate in CompiledPredicateIds

General rule for B12 FlowOrder:
  ArchitectureFlow31 → [always first if present]
  ImplementationFlow43 → [Phase1 parallel with APIContractFlow33]
  APIContractFlow33 → [after architecture, parallel with implementation]
  DocumentationFlow21 → [after contract]
  SecurityAuditFlow34 → [after contract, parallel with documentation]
  AuthorizationFlow34 → [after contract, parallel with documentation and security]
  QA → [always last]

---

## B12-L1-012-003 — Complete B12 orchestration ordering rules reference

```
source_id: xiigen-b12-l1-012-003
pair_kind: transition_orchestrator_reference
coverage_status: reference
```

B12 FlowOrder constraints:

1. ARCHITECTURE first rule:
   If ArchitectureFlow31 in flows → it runs FIRST before all other flows

2. IMPLEMENTATION parallel with API contract rule:
   If ImplementationFlow43 and APIContractFlow33 both present → run in PARALLEL

3. CONTRACT before documentation rule:
   DocumentationFlow21 always runs AFTER APIContractFlow33

4. CONTRACT before security rule:
   SecurityAuditFlow34 always runs AFTER APIContractFlow33

5. CONTRACT before authorization rule:
   AuthorizationFlow34 always runs AFTER APIContractFlow33

6. Documentation/security/authorization parallel rule:
   DocumentationFlow21, SecurityAuditFlow34, AuthorizationFlow34 can run in PARALLEL with each other
   (they share only a read dependency on APIContractFlow33, not on each other)

7. QA last rule:
   QA checkpoint always runs AFTER all flows complete (no exceptions)

---

## B12-L1-012-004 — B12 orchestration: complete decision path

```
source_id: xiigen-b12-l1-012-004
pair_kind: transition_orchestrator_reference
coverage_status: reference
```

Complete decision path from request to orchestration:

Input: user_request
  ↓
Layer 0-A: compile predicates
  → CompiledPredicateIds
  ↓
Layer 0-B: route predicates to sub-phases
  → RoutingDecisions, DecisionCount
  ↓
Layer 0-C: apply knowledge edge rules
  → validate predicates against disambiguation rules
  ↓
Layer 0-D: select flows from factory
  → FlowOrder, FlowType (deterministic_generic / probationary / trusted)
  ↓
Layer 1: orchestrate flow execution
  → ExecutionPlan (sequential / parallel phases)
  → TransitionType
  → QA position (always last)
  ↓
Layer 2: execute each nine-node flow
  → produce artifacts per flow
  ↓
QA checkpoint
  → validate all outputs
  ↓
Complete
