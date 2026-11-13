# Layer 1 — Transition Orchestrator Pairs (010)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B12-L1-010-001 — TransitionType summary for B12: complete pattern table

```
source_id: xiigen-b12-l1-010-001
pair_kind: transition_orchestrator_reference
coverage_status: reference
```

### Complete TransitionType Summary for B12

| CompiledPredicateIds | IMPLEMENT present? | TransitionType | Orchestration model |
|---------------------|-------------------|----------------|---------------------|
| [API_CONTRACT] | NO | documentation | Single flow, sequential |
| [DOCUMENT] | NO | documentation | Single flow |
| [API_CONTRACT, DOCUMENT] | NO | documentation | 2-flow sequential |
| [ARCHITECTURE] | NO | documentation | Single flow |
| [ARCHITECTURE, API_CONTRACT, DOCUMENT] | NO | documentation | 3-flow sequential |
| [API_CONTRACT, DOCUMENT, SECURITY] | NO | documentation | 1+2 parallel |
| [API_CONTRACT, DOCUMENT, AUTHORIZATION] | NO | documentation | 1+2 parallel |
| [API_CONTRACT, DOCUMENT, SECURITY, AUTHORIZATION] | NO | documentation | 1+3 parallel |
| [IMPLEMENT, API_CONTRACT] | YES | feature_addition | 2-flow parallel |
| [IMPLEMENT, API_CONTRACT, DOCUMENT] | YES | feature_addition | 2+1 parallel |
| [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY] | YES | feature_addition | 2+2 parallel |
| [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY, AUTHORIZATION] | YES | feature_addition | 2+3 parallel |
| [ARCHITECTURE, IMPLEMENT, API_CONTRACT, DOCUMENT] | YES | feature_addition | 1+2+1 sequential+parallel |

### TransitionType rule
feature_addition: IMPLEMENT in CompiledPredicateIds → YES
documentation: IMPLEMENT in CompiledPredicateIds → NO

---

## B12-L1-010-002 — Orchestration model notation explanation

```
source_id: xiigen-b12-l1-010-002
pair_kind: transition_orchestrator_reference
```

Orchestration model notation:
  "2-flow sequential": 2 flows, each depends on previous → runs serially
  "2-flow parallel": 2 flows, no dependency → runs simultaneously
  "1+2 parallel": 1 sequential flow first, then 2 parallel flows
  "2+2 parallel": 2 parallel flows, then 2 more parallel flows
  "2+3 parallel": 2 parallel flows (Phase1), then 3 parallel flows (Phase2)
  "3-flow sequential": 3 flows, each depends on previous → runs in order

Pattern: [number before +] = Phase1 flows, [number after +] = Phase2 flows

---

## B12-L1-010-003 — Orchestrator: handling ARCHITECTURE in B12

```
source_id: xiigen-b12-l1-010-003
pair_kind: transition_orchestrator_reference
```

When ARCHITECTURE fires in B12, orchestrator behavior:

ARCHITECTURE changes the FlowOrder to put ArchitectureFlow31 FIRST.
All other flows depend on ArchitectureFlow31 output.
This forces a sequential dependency chain:
  ArchitectureFlow31 → APIContractFlow33 → [Documentation, Security in parallel]

Pattern [ARCHITECTURE, API_CONTRACT, DOCUMENT]:
  3-flow sequential (not parallel between architecture and API contract)
  ArchitectureFlow31 must complete before APIContractFlow33 can start
  APIContractFlow33 must complete before DocumentationFlow21 can start

If SECURITY also fires [ARCHITECTURE, API_CONTRACT, DOCUMENT, SECURITY]:
  ArchitectureFlow31 (sequential)
  → APIContractFlow33 (sequential after architecture)
  → DocumentationFlow21 (parallel) + SecurityAuditFlow34 (parallel)
  = 1+1+2 model

---

## B12-L1-010-004 — B12 orchestration anti-patterns

```
source_id: xiigen-b12-l1-010-004
pair_kind: transition_orchestrator_reference
```

Anti-patterns that must not appear in B12 orchestration:

Anti-pattern 1: Security before contract
  WRONG: [SecurityAuditFlow34, APIContractFlow33, DocumentationFlow21]
  CORRECT: [APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34] (security after contract)
  Reason: SecurityAuditFlow34 needs contract output to review

Anti-pattern 2: QA in the middle
  WRONG: [APIContractFlow33, QA, DocumentationFlow21, QA]
  CORRECT: [APIContractFlow33, DocumentationFlow21, QA]
  Reason: QA only runs once, after all flows complete

Anti-pattern 3: Documentation before contract
  WRONG: [DocumentationFlow21, APIContractFlow33]
  CORRECT: [APIContractFlow33, DocumentationFlow21]
  Reason: documentation describes the contract — contract must exist first

Anti-pattern 4: Single flow for five predicates
  WRONG: [APIContractFlow33] for [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY, AUTHORIZATION]
  CORRECT: [ImplementationFlow43, APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34, AuthorizationFlow34]
  Reason: each predicate requires its own flow (v6-03 collapse is an anti-pattern)

Anti-pattern 5: Wrong TransitionType for documentation-only
  WRONG: feature_addition for [API_CONTRACT, DOCUMENT] (no IMPLEMENT)
  CORRECT: documentation for [API_CONTRACT, DOCUMENT]
  Reason: TransitionType = feature_addition requires IMPLEMENT predicate
