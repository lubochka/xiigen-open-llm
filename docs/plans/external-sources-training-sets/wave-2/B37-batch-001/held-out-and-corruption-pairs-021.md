# Held-Out and Corruption Pairs (021)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B37-HO-021-001 — HO: "API dependencies" → API_CONTRACT fires

**Request:** "Map all API dependencies between the order service and the payment endpoint."

**Expected output:**
- "API" → API co-occurrence token
- "dependencies" → API_CONTRACT fires (API co-occurrence present)
- "order service" → IMPLEMENT fires on "service"
- "payment endpoint" → "endpoint" is API co-occurrence
- Predicates: ["API_CONTRACT", "IMPLEMENT"]
- DomainId: "software-development"
- Routes: APIContractFlow33 (3.3) + BackendServiceFlow41 (4.1)
- Gap IDs: none

---

## B37-HO-021-002 — HO: "team dependencies" → zero predicates

**Request:** "Map all team dependencies and track blockers for the delivery programme."

**Expected output:**
- "team dependencies" → no API co-occurrence → gap
- "track" → gap (Rule 2)
- "blockers" → gap
- "delivery programme" → gap
- Predicates: []
- DomainId: "project-management"
- Route: knowledge_edge
- SCL: emitted
- Gap IDs: gap-project-managing-dependencies-team-001, gap-project-managing-track-001, gap-project-managing-blockers-001, gap-project-managing-manage-delivery-001

---

## B37-HO-021-003 — COR: "service dependencies" fires API_CONTRACT (wrong)

**Request:** "Resolve service dependencies between the auth module and the payment module."

**v6 corrupt output:** Predicates: ["API_CONTRACT", "IMPLEMENT"] — "service" treated as API co-occurrence

**Why this is wrong:** "service" is NOT in the API co-occurrence list. API_CONTRACT requires: api/contract/endpoint/interface/integration/swagger/openapi/graphql/rest/grpc/webhook. "service" fires IMPLEMENT only.

**Correct output:** Predicates: ["IMPLEMENT"], DomainId: "software-development", Gap ID: gap-project-managing-dependencies-team-001

---

## B37-HO-021-004 — COR: "project dependencies" fires API_CONTRACT (wrong)

**Request:** "Resolve all project dependencies before the delivery milestone."

**v6 corrupt output:** Predicates: ["API_CONTRACT"] — "dependencies" fires API_CONTRACT without API co-occurrence

**Why this is wrong:** "dependencies" requires API co-occurrence. No API co-occurrence token present → zero predicates.

**Correct output:** Predicates: [], DomainId: "project-management", Gap ID: gap-project-managing-dependencies-team-001

---

## B37-HO-021-005 — HO: dependencies held-out certification summary

| Case | Request | Expected predicates | Expected DomainId |
|------|---------|-------------------|-----------------|
| HO-021-001 | "API dependencies between services" | [API_CONTRACT, IMPLEMENT] | software-development |
| HO-021-002 | "team dependencies" | [] | project-management |
| HO-021-003 | "integration dependencies" | [API_CONTRACT, IMPLEMENT] | software-development |
| HO-021-004 | "service dependencies" | [IMPLEMENT] | software-development |
| HO-021-005 | "project dependencies" | [] | project-management |

All five must pass. Zero v6 corruptions permitted.
