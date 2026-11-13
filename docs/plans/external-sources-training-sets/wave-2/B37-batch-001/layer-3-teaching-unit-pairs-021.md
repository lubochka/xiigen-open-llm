# Layer 3 — Teaching-Unit Pairs (021)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: teaching-unit-pairs

---

## B37-L3-021-001 — Teaching: dependencies API co-occurrence rule

**Concept:** "dependencies" fires API_CONTRACT ONLY with API co-occurrence.

Q: "Map team dependencies." → predicates?
A: []. No API co-occurrence → gap. DomainId "project-management".

Q: "Map API dependencies." → predicates?
A: [API_CONTRACT]. "API" is co-occurrence token. DomainId "software-development".

Q: "Map service dependencies." → predicates?
A: [IMPLEMENT]. "service" fires IMPLEMENT but NOT API co-occurrence. DomainId "software-development".

---

## B37-L3-021-002 — Teaching: API co-occurrence list (exam-level)

**11 API co-occurrence tokens:**
api, contract, endpoint, interface, integration, swagger, openapi, graphql, rest, grpc, webhook

**NOT co-occurrence tokens (common confusions):**
service, module, library, component, database, backend, frontend, class, function

---

## B37-L3-021-003 — Teaching: "service dependencies" boundary

"service dependencies" is the most common test case for the API co-occurrence rule:

Q: "Resolve service dependencies." → predicates?
A: [IMPLEMENT]. "service" fires IMPLEMENT; "service" is NOT an API co-occurrence token.

API_CONTRACT does NOT fire on "service dependencies".
IMPLEMENT fires on "service".
Gap warning: gap-project-managing-dependencies-team-001 (no API co-occurrence).

---

## B37-L3-021-004 — Teaching: dependencies decision matrix

| Input | API co-occurrence? | Predicates |
|-------|------------------|-----------|
| team dependencies | No | [] |
| project dependencies | No | [] |
| cross-workstream dependencies | No | [] |
| API dependencies | Yes (api) | [API_CONTRACT] |
| endpoint dependencies | Yes (endpoint) | [API_CONTRACT] |
| REST dependencies | Yes (rest) | [API_CONTRACT] |
| service dependencies | No (service not in list) | [IMPLEMENT] |
| module dependencies | No (module not in list) | [IMPLEMENT] |

---

## B37-L3-021-005 — Teaching: dependencies exam

1. "Map team dependencies." → []
2. "Resolve project dependencies." → []
3. "Map API dependencies." → [API_CONTRACT]
4. "Identify integration dependencies." → [API_CONTRACT]
5. "Resolve service dependencies." → [IMPLEMENT]
6. "Map module dependencies." → [IMPLEMENT]
7. "Review API dependencies." → [QA, API_CONTRACT]
8. "Document API endpoint dependencies." → [DOCUMENT, API_CONTRACT]
9. DomainId for question 2? → "project-management"
10. DomainId for question 4? → "software-development"
