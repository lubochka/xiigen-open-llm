# Layer 0-C — Knowledge-Edge Pairs (004)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: knowledge-edge-pairs

---

## B37-0C-004-001 — "dependencies" context rule: API co-occurrence required for API_CONTRACT

**Knowledge edge: The "dependencies" context rule**

"dependencies" alone does NOT fire API_CONTRACT.
"dependencies" WITH API co-occurrence fires API_CONTRACT.

**API co-occurrence tokens (complete list):**
api, contract, endpoint, interface, integration, swagger, openapi, graphql, rest, grpc, webhook

**Rule statement (Rule 5):** "dependencies" → API_CONTRACT ONLY with API co-occurrence; "team dependencies" / "project dependencies" → GAP

---

## B37-0C-004-002 — API co-occurrence present: API_CONTRACT fires

**Examples where API_CONTRACT fires:**

1. "API dependencies" → API_CONTRACT (explicit "api" token)
2. "service API dependencies" → API_CONTRACT ("api" present)
3. "endpoint dependencies" → API_CONTRACT ("endpoint" is co-occurrence token)
4. "interface dependencies" → API_CONTRACT ("interface" is co-occurrence token)
5. "integration dependencies" → API_CONTRACT ("integration" is co-occurrence token)
6. "swagger dependencies" → API_CONTRACT
7. "REST dependencies" → API_CONTRACT ("rest" is co-occurrence token)
8. "grpc dependencies" → API_CONTRACT

---

## B37-0C-004-003 — API co-occurrence absent: gap only

**Examples where "dependencies" produces zero predicates:**

1. "project dependencies" → gap-project-managing-dependencies-team-001
2. "team dependencies" → gap-project-managing-dependencies-team-001
3. "delivery dependencies" → gap-project-managing-dependencies-team-001
4. "workstream dependencies" → gap-project-managing-dependencies-team-001
5. "cross-team dependencies" → gap-project-managing-dependencies-team-001
6. "service dependencies" (without "api") → AMBIGUOUS — see below
7. "backend dependencies" → gap (no API co-occurrence token)
8. "release dependencies" → gap

**Ambiguity: "service" alone as co-occurrence token**
"service dependencies" is borderline. The rule specifies the API co-occurrence list as: api, contract, endpoint, interface, integration, swagger, openapi, graphql, rest, grpc, webhook. "service" is NOT on this list explicitly. However, "service API dependencies" IS on the list (because "api" is present).

Correct interpretation: "service dependencies" without any API co-occurrence token → gap. "service API dependencies" → API_CONTRACT fires.

---

## B37-0C-004-004 — "dependencies" with partial co-occurrence: edge cases

**Edge case 1:** "Document the service dependencies and API contracts."
- "service" → not API co-occurrence token
- "API" → API co-occurrence token
- "contracts" → "contract" is API co-occurrence token
- "dependencies" → API co-occurrence present ("api", "contracts") → API_CONTRACT fires
- "document" → DOCUMENT fires
- Result: ["API_CONTRACT", "DOCUMENT"]

**Edge case 2:** "Map the backend service dependencies."
- "backend" → not API co-occurrence token
- "service" → not API co-occurrence token (alone)
- No API co-occurrence tokens present
- "dependencies" → gap
- Result: [] → knowledge_edge

**Edge case 3:** "Identify all REST service dependencies and integration points."
- "REST" → API co-occurrence token
- "integration" → API co-occurrence token
- "dependencies" → API_CONTRACT fires
- Result: ["API_CONTRACT"]

---

## B37-0C-004-005 — "dependencies" context rule summary

| Context | API co-occurrence present? | Result |
|---------|--------------------------|--------|
| "API dependencies" | Yes (api) | API_CONTRACT |
| "endpoint dependencies" | Yes (endpoint) | API_CONTRACT |
| "integration dependencies" | Yes (integration) | API_CONTRACT |
| "project dependencies" | No | gap (dependencies-team-001) |
| "team dependencies" | No | gap (dependencies-team-001) |
| "delivery dependencies" | No | gap (dependencies-team-001) |
| "service dependencies" | No (service not listed) | gap (dependencies-team-001) |
| "service API dependencies" | Yes (api) | API_CONTRACT |
| "backend dependencies" | No | gap (dependencies-team-001) |
| "REST service dependencies" | Yes (rest) | API_CONTRACT |
