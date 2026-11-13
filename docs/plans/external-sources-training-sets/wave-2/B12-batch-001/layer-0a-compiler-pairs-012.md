# Layer 0-A — Compiler Pairs (012)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: compiler

---

## B12-0A-012-001 — Complete B12 predicate rule table

```
source_id: xiigen-b12-0a-012-001
pair_kind: compiler_reference
coverage_status: reference
```

### B12 API_CONTRACT vocabulary (complete trigger table)

| Term | Fires API_CONTRACT | Condition |
|------|--------------------|-----------|
| api | YES | always |
| contract | YES | always (regardless of modifier) |
| endpoint | YES | always |
| route | YES | in API context |
| interface | YES | in API context (also fires ARCHITECTURE in system design context) |
| integration | YES | in API context (Core Rule 17: not IMPLEMENT) |
| swagger | YES | always |
| openapi | YES | always |
| graphql | YES | always |
| rest | YES | always |
| grpc | YES | always |
| webhook | YES | always |
| schema | YES | ONLY with API co-occurrence (api/rest/graphql/openapi/swagger/grpc/webhook) |
| dependency | YES | ONLY with API co-occurrence |
| response | YES | in API context |
| request | YES | in API context |
| versioning | YES | in API context (Core Rule 21) |
| version | YES | in API context (Core Rule 21) |
| protobuf | YES | ONLY with gRPC co-occurrence |
| spec | YES | fires DOCUMENT (via specification); fires API_CONTRACT via associated terms |

### B12 DOCUMENT vocabulary

| Term | Fires DOCUMENT |
|------|----------------|
| document | YES |
| define | YES |
| specify | YES |
| describe | YES |
| annotate | YES |
| write spec | YES (context: spec = document) |
| specification | YES |
| requirements | YES |
| write specification | YES |

### B12 IMPLEMENT vocabulary

| Term | Fires IMPLEMENT | Note |
|------|-----------------|------|
| write | YES | code artifact creation context |
| implement | YES | always |
| build | YES | code artifact context |
| create | YES | code artifact context |
| write spec | conditionally | depends on context |

### B12 SECURITY vocabulary

| Term | Fires SECURITY | Condition |
|------|----------------|-----------|
| authentication | YES | ALWAYS — any context (Core Rule 6) |
| token | YES | always (Core Rule 8) |
| oauth | YES | also fires AUTHORIZATION |

### B12 AUTHORIZATION vocabulary

| Term | Fires AUTHORIZATION | Routes to |
|------|---------------------|-----------|
| access control | YES | 3.4 |
| authorization | YES | 3.4 |
| oauth | YES | 3.4 (also fires SECURITY) |

### B12 ARCHITECTURE vocabulary

| Term | Fires ARCHITECTURE | Condition |
|------|--------------------|-----------|
| design | YES | always |
| architecture | YES | always |
| interface | YES | system design context (also fires API_CONTRACT in API context) |

---

## B12-0A-012-002 — Complete predicate-to-sub-phase routing map

```
source_id: xiigen-b12-0a-012-002
pair_kind: compiler_reference
coverage_status: reference
```

| Predicate | Routes to | Sub-phase name |
|-----------|-----------|----------------|
| API_CONTRACT | 3.3 | API Contract Design |
| IMPLEMENT | 4.3 | Implementation |
| DOCUMENT | 2.1 | Documentation (primary) |
| DOCUMENT | 2.4 | Documentation (alternate) |
| ARCHITECTURE | 3.1 | Architecture (system) |
| ARCHITECTURE + IMPLEMENT | 3.2 | Architecture + Implementation |
| SECURITY | 3.4 | Security + Authorization |
| AUTHORIZATION | 3.4 | Security + Authorization |

Phase 3 sub-phase ownership:
  3.1 = ARCHITECTURE (system design)
  3.2 = ARCHITECTURE + IMPLEMENT (component design with implementation)
  3.3 = API_CONTRACT (API contract documentation — B12 primary)
  3.4 = SECURITY + AUTHORIZATION (access control, auth flows)
  3.5 = ARCHITECTURE (infrastructure/cloud)

---

## B12-0A-012-003 — TransitionType rules for B12

```
source_id: xiigen-b12-0a-012-003
pair_kind: compiler_reference
coverage_status: reference
```

| Scenario | TransitionType |
|----------|----------------|
| API_CONTRACT alone | documentation |
| API_CONTRACT + DOCUMENT | documentation |
| API_CONTRACT + DOCUMENT + SECURITY | documentation |
| API_CONTRACT + IMPLEMENT | feature_addition |
| IMPLEMENT + API_CONTRACT + DOCUMENT | feature_addition |
| IMPLEMENT + API_CONTRACT + DOCUMENT + SECURITY | feature_addition |
| IMPLEMENT + API_CONTRACT + DOCUMENT + SECURITY + AUTHORIZATION | feature_addition |
| DOCUMENT alone | documentation |
| ARCHITECTURE + API_CONTRACT | documentation |
| ARCHITECTURE + API_CONTRACT + DOCUMENT | documentation |

Rule: feature_addition when IMPLEMENT fires; documentation otherwise.
Special case: new endpoint/contract being created for the first time → feature_addition
even without explicit IMPLEMENT (creating new artifact).

---

## B12-0A-012-004 — B12 No-GapHandler declaration

```
source_id: xiigen-b12-0a-012-004
pair_kind: compiler_reference
coverage_status: reference
```

B12 has no GapHandler (Core Rule 22). This means:
  - No gap vocabulary in this scenario
  - GapHandler does not activate for B12 requests
  - Contrast with scenarios that include "I don't know", "unclear", "not sure" vocabulary
    which would trigger GapHandler in those scenarios

DomainId for all B12 pairs: "software-development" (Core Rule 24)

DecisionCount formula (Core Rule 23):
  DecisionCount = COUNT(predicates fired in CompiledPredicateIds)
  Example: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY", "AUTHORIZATION"] → DecisionCount = 5
  NOT: count of unique sub-phases (SECURITY + AUTHORIZATION both to 3.4 = 2 decisions to 3.4)
