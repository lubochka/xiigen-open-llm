# Layer 2 — Capability-Gap-Extended Pairs (007)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: capability-gap-extended-pairs

---

## B37-L2-007-001 — "dependencies" context rule: API dependency vs team dependency

**Side-by-side comparison:**

**Request A (API dependency):** "Document the API dependencies between the payment service and the notification endpoint."
- "API" → API co-occurrence token
- "dependencies" → API_CONTRACT fires (Rule 5)
- "endpoint" → API co-occurrence token (reinforces)
- "Document" → DOCUMENT fires
- "service" → IMPLEMENT fires (Rule 9)
- Result: ["API_CONTRACT", "DOCUMENT", "IMPLEMENT"]
- DomainId: "software-development"

**Request B (team dependency):** "Map the project team dependencies and identify cross-team blockers."
- "project" → not in any predicate lexicon
- "team dependencies" → no API co-occurrence → gap (Rule 5)
- "dependencies" → gap-project-managing-dependencies-team-001
- "blockers" → gap
- Result: []
- DomainId: "project-management"
- Gap IDs: gap-project-managing-dependencies-team-001, gap-project-managing-blockers-001
- SCL emitted

---

## B37-L2-007-002 — API dependency trace: API_CONTRACT fires

**Input:** "Review all API dependencies for the integration between the auth service and the REST endpoint."

**Trace:**
- "Review" → QA fires (Rule 19)
- "API" → API co-occurrence token
- "dependencies" → API_CONTRACT fires (API, integration, REST, endpoint all present as co-occurrence)
- "integration" → API co-occurrence token
- "service" → IMPLEMENT fires (Rule 9)
- "REST" → API co-occurrence token
- "endpoint" → API co-occurrence token

Result: ["QA", "API_CONTRACT", "IMPLEMENT"]

DomainId: "software-development"

CapabilityGapWarnings: none (all vocabulary captured)

---

## B37-L2-007-003 — Team dependency trace: knowledge_edge

**Input:** "Identify all team dependencies blocking the Q3 delivery milestone."

**Trace:**
- "Identify" → not in any predicate lexicon
- "team dependencies" → no API co-occurrence → gap
- "blocking" → not in any predicate lexicon
- "Q3 delivery" → gap
- "milestone" → not in any predicate lexicon (B36 domain: project-planning, but not a predicate)

Result: []

DomainId: "project-management"

Gap IDs: gap-project-managing-dependencies-team-001, gap-project-managing-manage-delivery-001

SCL emitted

---

## B37-L2-007-004 — "dependencies" ambiguity: "service dependencies" without API token

**Input:** "Resolve the service dependencies before the deployment."

**Trace:**
- "Resolve" → not in any predicate lexicon
- "service dependencies" → "service" is NOT in API co-occurrence list; no API token present
- "dependencies" → no API co-occurrence → gap (Rule 5)
- "deployment" → not in any predicate lexicon

Result: []

DomainId: "project-management"

Gap IDs: gap-project-managing-dependencies-team-001

SCL emitted

**Key distinction:** "service dependencies" alone → gap. "service API dependencies" → API_CONTRACT (because "API" is in co-occurrence list). The word "service" by itself is not an API co-occurrence trigger.

---

## B37-L2-007-005 — Complete "dependencies" context rule summary

**API_CONTRACT fires when dependencies + any of:**
api, contract, endpoint, interface, integration, swagger, openapi, graphql, rest, grpc, webhook

**Gap fires when dependencies + any of:**
team, project, delivery, cross-team, workstream, programme, sprint, milestone, service (alone), backend, frontend, module (alone)

**Borderline cases resolved by co-occurrence list:**
"service API dependencies" → API_CONTRACT (api present)
"API service dependencies" → API_CONTRACT (api present)
"REST service dependencies" → API_CONTRACT (rest present)
"service module dependencies" → gap (neither service nor module is in co-occurrence list)
