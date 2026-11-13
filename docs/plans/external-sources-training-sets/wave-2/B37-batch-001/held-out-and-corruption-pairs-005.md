# Held-Out and Corruption Pairs (005)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption-pairs

---

## B37-HO-005-001 — HO: "API dependencies" → API_CONTRACT fires

**Held-out request:** "Map all API dependencies between the notification service and the webhook endpoints."

**Expected output:**
- "Map" → not in any predicate lexicon
- "API" → API co-occurrence token
- "dependencies" → API_CONTRACT fires (API + webhook co-occurrence)
- "notification" → not directly triggering
- "service" → IMPLEMENT fires (Rule 9)
- "webhook" → API co-occurrence token
- "endpoints" → "endpoint" is API co-occurrence token
- Predicates: ["API_CONTRACT", "IMPLEMENT"]
- DomainId: "software-development"
- Gap IDs: none (all vocabulary captured)

---

## B37-HO-005-002 — HO: "project dependencies" → gap

**Held-out request:** "Identify all project dependencies that could delay the Q4 delivery."

**Expected output:**
- "Identify" → not in any predicate lexicon
- "project dependencies" → no API co-occurrence → gap (Rule 5)
- "delay" → not in any predicate lexicon
- "Q4 delivery" → gap
- Predicates: []
- DomainId: "project-management"
- Route: knowledge_edge
- SCL: emitted
- Gap IDs: gap-project-managing-dependencies-team-001, gap-project-managing-manage-delivery-001

---

## B37-HO-005-003 — Side-by-side: API vs project dependencies

**Case A:** "Document the API dependencies for the payment gateway endpoint."
- "API" + "endpoint" → API co-occurrence
- "dependencies" → API_CONTRACT fires
- "Document" → DOCUMENT fires
- Result: ["API_CONTRACT", "DOCUMENT"], DomainId "software-development"

**Case B:** "Document the project dependencies for the payment gateway implementation."
- No API co-occurrence tokens (no api, endpoint, interface, etc.)
- "dependencies" → gap
- "Document" → DOCUMENT fires
- "implementation" → IMPLEMENT fires ("implement" family)
- Result: ["DOCUMENT", "IMPLEMENT"], DomainId "software-development", gap-project-managing-dependencies-team-001

---

## B37-HO-005-004 — COR: "project dependencies" fires API_CONTRACT (wrong)

**Corrupted output:** "Identify project dependencies for Q4 delivery." → ["API_CONTRACT"]

**Why this is wrong:** No API co-occurrence tokens present. "project" and "delivery" are NOT in the API co-occurrence list (api, contract, endpoint, interface, integration, swagger, openapi, graphql, rest, grpc, webhook).

**Correct output:** [] (zero predicates; knowledge_edge)

**Corruption label:** API_CONTRACT fired for "dependencies" without API co-occurrence

---

## B37-HO-005-005 — HO: "integration dependencies" → API_CONTRACT fires

**Request:** "Map the integration dependencies between all third-party services."

**Expected output:**
- "Map" → not in any predicate lexicon
- "integration" → API co-occurrence token
- "dependencies" → API_CONTRACT fires (integration co-occurrence)
- "third-party" → not in any predicate lexicon
- "services" → IMPLEMENT fires (Rule 9: "service" plural)
- Predicates: ["API_CONTRACT", "IMPLEMENT"]
- DomainId: "software-development"
- Gap IDs: none
