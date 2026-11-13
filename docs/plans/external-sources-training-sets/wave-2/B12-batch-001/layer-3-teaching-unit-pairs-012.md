# Layer 3 — Teaching Unit Pairs (012)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B12-TU-012 — Teaching: Complete B12 teaching summary

```
source_id: xiigen-b12-tu-012
rule_id: B12-TEACHING-SUMMARY
pair_kind: teaching_unit
coverage_status: reference
```

### B12 Scenario Summary

Scenario: B12 — Development — API contract documentation (REST and GraphQL)
Primary sub-phase: 3.3 (API Contract Design)
Primary predicate: API_CONTRACT
Primary flow: APIContractFlow33
DomainId: "software-development"

### 12 Key Teaching Points for B12

1. "contract" → API_CONTRACT regardless of modifier
   Data contracts, service contracts, message contracts, event contracts, webhook contracts
   ALL fire API_CONTRACT via the "contract" trigger. No suppressor exists.

2. "schema" → API_CONTRACT ONLY with API co-occurrence
   Requires: api/rest/graphql/openapi/swagger/grpc/webhook as co-occurrence term.
   "database schema" = DOCUMENT only. "GraphQL schema" = API_CONTRACT + DOCUMENT.

3. "interface" → context-determines which predicate(s)
   API vocabulary only → API_CONTRACT. Design vocabulary only → ARCHITECTURE.
   Both → ARCHITECTURE + API_CONTRACT simultaneously.

4. "authentication" → SECURITY always
   No context suppresses SECURITY. API doc context, gRPC context, webhook context — always SECURITY.
   v6-02 corruption occurs when "authentication" is treated as API vocabulary that suppresses SECURITY.

5. "authorization" / "access control" / "oauth" → AUTHORIZATION
   All route to 3.4. "oauth" fires BOTH SECURITY and AUTHORIZATION simultaneously.

6. TransitionType: feature_addition when IMPLEMENT fires; documentation otherwise
   IMPLEMENT verbs: write, implement, build, create, develop.
   "integrate" → API_CONTRACT (not IMPLEMENT) — Core Rule 17.

7. DecisionCount = predicate count, not unique sub-phases
   SECURITY + AUTHORIZATION both to 3.4 = two decisions, not one.
   Five predicates = DecisionCount 5 even if four unique sub-phases.

8. Phase 3 sub-phases: 3.1 ARCHITECTURE, 3.2 ARCH+IMPL, 3.3 API_CONTRACT, 3.4 SECURITY+AUTH, 3.5 infra
   DOCUMENT routes to Phase 2.1 (not Phase 3). B12 primary = 3.3.

9. "design" alone → ARCHITECTURE. "design the API" → ARCHITECTURE + API_CONTRACT
   Design vocabulary and API vocabulary combine without suppressing each other.

10. v6 corruptions: three types in B12
    v6-01: "contract" modifier suppresses API_CONTRACT (wrong — contract is unconditional)
    v6-02: "authentication" in API context suppresses SECURITY (wrong — always fires)
    v6-03: multiple predicates collapsed to single 3.3 routing (wrong — each routes independently)

11. No GapHandler in B12
    B12 has no gap vocabulary. GapHandler never activates for B12 requests.

12. FlowOrder: ARCHITECTURE first, then IMPLEMENT/API_CONTRACT (parallel), then DOCUMENT/SECURITY/AUTH (parallel), QA last
    QA is always the final step. Never inserted between flows.

### B12 Quick-Reference Card

```
API_CONTRACT triggers (unconditional): api, contract, endpoint, route, webhook, grpc, graphql, rest, openapi, swagger, integration, versioning, version
API_CONTRACT triggers (conditional): interface (API context), schema (API co-occurrence), protobuf (gRPC co-occurrence)
DOCUMENT triggers: document, define, specify, describe, annotate, write spec, specification, requirements
IMPLEMENT triggers: write, implement, build, create, develop
SECURITY triggers: authentication, token
AUTHORIZATION triggers: authorization, access control, oauth
ARCHITECTURE triggers: design, architecture

Sub-phase routing:
  API_CONTRACT → 3.3
  DOCUMENT → 2.1
  ARCHITECTURE → 3.1
  IMPLEMENT → 4.3
  SECURITY → 3.4
  AUTHORIZATION → 3.4

TransitionType:
  IMPLEMENT present → feature_addition
  IMPLEMENT absent → documentation

DecisionCount = COUNT(CompiledPredicateIds)
```
