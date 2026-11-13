# Layer 0-B — Router Pairs (012)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: router

---

## B12-0B-012-001 — Complete B12 routing decision table

```
source_id: xiigen-b12-0b-012-001
pair_kind: router_reference
coverage_status: reference
```

### Predicate → Sub-phase routing table

| Predicate | Routes to | Sub-phase name | Flow name |
|-----------|-----------|----------------|-----------|
| API_CONTRACT | 3.3 | API Contract Design | APIContractFlow33 |
| DOCUMENT | 2.1 | Documentation | DocumentationFlow21 |
| ARCHITECTURE | 3.1 | Architecture (system) | ArchitectureFlow31 |
| IMPLEMENT | 4.3 | Implementation | ImplementationFlow43 |
| SECURITY | 3.4 | Security + Authorization | SecurityAuditFlow34 |
| AUTHORIZATION | 3.4 | Security + Authorization | AuthorizationFlow34 |

### Common B12 routing patterns (complete)

| CompiledPredicateIds | Routing | TransitionType | DecisionCount |
|---------------------|---------|----------------|---------------|
| [API_CONTRACT] | 3.3 | documentation | 1 |
| [DOCUMENT] | 2.1 | documentation | 1 |
| [API_CONTRACT, DOCUMENT] | 3.3 + 2.1 | documentation | 2 |
| [ARCHITECTURE] | 3.1 | documentation | 1 |
| [ARCHITECTURE, DOCUMENT] | 3.1 + 2.1 | documentation | 2 |
| [ARCHITECTURE, API_CONTRACT, DOCUMENT] | 3.1 + 3.3 + 2.1 | documentation | 3 |
| [API_CONTRACT, DOCUMENT, SECURITY] | 3.3 + 2.1 + 3.4 | documentation | 3 |
| [API_CONTRACT, DOCUMENT, AUTHORIZATION] | 3.3 + 2.1 + 3.4 | documentation | 3 |
| [IMPLEMENT, API_CONTRACT] | 4.3 + 3.3 | feature_addition | 2 |
| [IMPLEMENT, API_CONTRACT, DOCUMENT] | 4.3 + 3.3 + 2.1 | feature_addition | 3 |
| [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY] | 4.3 + 3.3 + 2.1 + 3.4 | feature_addition | 4 |
| [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY, AUTHORIZATION] | 4.3 + 3.3 + 2.1 + 3.4 + 3.4 | feature_addition | 5 |
| [ARCHITECTURE, API_CONTRACT, DOCUMENT, SECURITY] | 3.1 + 3.3 + 2.1 + 3.4 | documentation | 4 |
| [ARCHITECTURE, API_CONTRACT, DOCUMENT, AUTHORIZATION] | 3.1 + 3.3 + 2.1 + 3.4 | documentation | 4 |

---

## B12-0B-012-002 — TransitionType rules for routing

```
source_id: xiigen-b12-0b-012-002
pair_kind: router_reference
coverage_status: reference
```

TransitionType determination:
  feature_addition: IMPLEMENT predicate present in CompiledPredicateIds
  documentation: IMPLEMENT predicate NOT present

Special case: new endpoint/contract created for the first time without explicit "implement/build"
  → feature_addition even without IMPLEMENT if creation is unambiguous

ARCHITECTURE does NOT change TransitionType (always documentation unless IMPLEMENT also fires).
SECURITY does NOT change TransitionType.
AUTHORIZATION does NOT change TransitionType.

---

## B12-0B-012-003 — FlowOrder rules for routing

```
source_id: xiigen-b12-0b-012-003
pair_kind: router_reference
coverage_status: reference
```

FlowOrder priority:
  1. ARCHITECTURE flows run first (before API contract and implementation)
  2. IMPLEMENT flows run early (can parallel with API_CONTRACT)
  3. API_CONTRACT flows (3.3) run in middle
  4. DOCUMENT flows run after contract is defined
  5. SECURITY and AUTHORIZATION flows run last (review after definition)

FlowOrder for common patterns:
  [ARCHITECTURE, API_CONTRACT, DOCUMENT]:
    → [ArchitectureFlow31, APIContractFlow33, DocumentationFlow21]

  [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY]:
    → [ImplementationFlow43, APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34]

  [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY, AUTHORIZATION]:
    → [ImplementationFlow43, APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34, AuthorizationFlow34]

---

## B12-0B-012-004 — B12 DomainId and GapHandler routing rules

```
source_id: xiigen-b12-0b-012-004
pair_kind: router_reference
coverage_status: reference
```

DomainId: "software-development" (fixed for all B12 routing decisions, Core Rule 24)

GapHandler: NOT present in B12 (Core Rule 22 — no gap vocabulary in B12 scenario).
  No routing decisions trigger GapHandler in B12.
  Contrast: scenarios with "unclear", "not sure", "I don't know" vocabulary would trigger
  GapHandler in those scenarios, but B12 has no such vocabulary class.

Phase 3 sub-phase summary for B12:
  3.1 = ARCHITECTURE predicate (system design)
  3.2 = ARCHITECTURE + IMPLEMENT predicate combination (component with implementation)
  3.3 = API_CONTRACT predicate (B12 primary sub-phase)
  3.4 = SECURITY predicate + AUTHORIZATION predicate (access control, auth flows)
  3.5 = ARCHITECTURE predicate (infrastructure/cloud — context-dependent)
