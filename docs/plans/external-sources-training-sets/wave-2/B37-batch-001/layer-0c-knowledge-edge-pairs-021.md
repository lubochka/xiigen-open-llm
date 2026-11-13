# Layer 0-C — Knowledge-Edge Pairs (021)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-021-001 — "dependencies" API co-occurrence rule: complete reference

**Knowledge edge: "dependencies" fires API_CONTRACT ONLY with API co-occurrence**

Rule 5 (context-sensitive): "dependencies" fires API_CONTRACT ONLY when one of the following API co-occurrence tokens is present in the same request:
api, contract, endpoint, interface, integration, swagger, openapi, graphql, rest, grpc, webhook

Without any API co-occurrence token: "dependencies" fires NOTHING → gap.

**Gap ID (no API co-occurrence):** gap-project-managing-dependencies-team-001

---

## B37-0C-021-002 — API co-occurrence token list (exam-level)

**Complete API co-occurrence list (11 tokens):**
1. api
2. contract
3. endpoint
4. interface
5. integration
6. swagger
7. openapi
8. graphql
9. rest
10. grpc
11. webhook

**Mnemonic:** API, Contract, Endpoint, Interface, Integration + Swagger, OpenAPI, GraphQL, REST, gRPC, Webhook

**What is NOT on the list:**
- service (not API co-occurrence — fires IMPLEMENT but not API_CONTRACT)
- module (not API co-occurrence — fires IMPLEMENT)
- library (not API co-occurrence)
- team, project, cross-workstream, delivery, programme (all gap only)

---

## B37-0C-021-003 — "dependencies" decision tree (exam-level)

```
"dependencies" present in sentence?
├── Yes
│   ├── API co-occurrence present? (api/contract/endpoint/interface/integration/swagger/openapi/graphql/rest/grpc/webhook)
│   │   ├── Yes → API_CONTRACT fires; DomainId "software-development"
│   │   └── No → gap: gap-project-managing-dependencies-team-001; DomainId "project-management" (if no other predicates)
│   └── Other predicate tokens present?
│       ├── Yes (+ API co-occurrence) → API_CONTRACT + other predicates
│       └── Yes (no API co-occurrence) → other predicates only + gap warning
└── No → not relevant
```

---

## B37-0C-021-004 — "dependencies" context table: six cases

| Input | API co-occurrence? | Predicates | DomainId |
|-------|------------------|-----------|---------|
| "project dependencies" | No | [] | project-management |
| "team dependencies" | No | [] | project-management |
| "cross-workstream dependencies" | No | [] | project-management |
| "API dependencies" | Yes (api) | [API_CONTRACT] | software-development |
| "integration dependencies" | Yes (integration) | [API_CONTRACT] | software-development |
| "endpoint dependencies" | Yes (endpoint) | [API_CONTRACT] | software-development |

---

## B37-0C-021-005 — "service dependencies" boundary: IMPLEMENT fires but not API_CONTRACT

**Input:** "Identify service dependencies between the auth module and the payment module."

Knowledge edge:
- "service" → IMPLEMENT fires (Rule 9)
- "dependencies" → no API co-occurrence ("service" is NOT on the API co-occurrence list) → gap
- "module" → IMPLEMENT fires (Rule 9)

Result: ["IMPLEMENT"]
Gap warning: gap-project-managing-dependencies-team-001

**Teaching point:** "service dependencies" contains "service" (IMPLEMENT trigger) but "service" is NOT an API co-occurrence token for the "dependencies" rule. API_CONTRACT does NOT fire. IMPLEMENT fires on "service" and "module".

DomainId "software-development" (IMPLEMENT fired, not from API_CONTRACT).
