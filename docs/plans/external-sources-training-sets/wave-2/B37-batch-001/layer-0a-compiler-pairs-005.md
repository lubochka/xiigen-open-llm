# Layer 0-A — Compiler Pairs (005)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: compiler-pairs

---

## B37-0A-005-001 — "dependencies" WITH API co-occurrence → API_CONTRACT fires

**Input request:**
"Document the service API dependencies and coordinate the release schedule with the platform team."

**Compiler analysis:**

Token scan:
- "Document" → DOCUMENT fires (Rule 8)
- "service" → in B37 gap context; but also API co-occurrence trigger
- "API" → explicit API co-occurrence token
- "dependencies" → API_CONTRACT fires because "API" co-occurrence present (Rule 5)
- "coordinate" → not in any predicate lexicon (Rule 11)
- "release" → not in any predicate lexicon
- "schedule" → not in any predicate lexicon

Result: ["API_CONTRACT", "DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-coordinate-001 ("coordinate" not captured)
- gap-project-managing-manage-delivery-001 ("release schedule" managing intent not captured)

Note: "service" + "API" together constitute API co-occurrence. Rule 5 states: "dependencies" → API_CONTRACT ONLY with API co-occurrence (api, contract, endpoint, interface, integration, swagger, openapi, graphql, rest, grpc, webhook).

---

## B37-0A-005-002 — "dependencies" with "endpoint" co-occurrence → API_CONTRACT fires

**Input request:**
"Map the service dependencies for each API endpoint and track the integration status."

**Compiler analysis:**

Token scan:
- "dependencies" → "API", "endpoint" co-occurrence present → API_CONTRACT fires (Rule 5)
- "endpoint" → API co-occurrence token
- "track" → not in TEST lexicon; not in any predicate lexicon (gap)
- "integration" → API co-occurrence token (also confirms API_CONTRACT)
- "status" → not in any predicate lexicon

Result: ["API_CONTRACT"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-track-status-001 ("track the integration status" managing intent)

---

## B37-0A-005-003 — "dependencies" with "interface" co-occurrence → API_CONTRACT fires

**Input request:**
"Define the interface dependencies between the payment service and the notification module."

**Compiler analysis:**

Token scan:
- "Define" → DOCUMENT fires (Rule 8)
- "interface" → API co-occurrence token
- "dependencies" → API_CONTRACT fires (interface co-occurrence present — Rule 5)
- "service" → API co-occurrence context (reinforces API_CONTRACT)
- "module" → IMPLEMENT fires on "module" (Rule 9)

Result: ["API_CONTRACT", "DOCUMENT", "IMPLEMENT"]

DomainId: "software-development"

CapabilityGapWarnings: none (all vocabulary captured by predicates)

---

## B37-0A-005-004 — "dependencies" WITHOUT API co-occurrence → gap only

**Input request:**
"Review the project dependencies and flag any blockers to the delivery manager."

**Compiler analysis:**

Token scan:
- "Review" → QA fires on "review" (Rule 19)
- "project" → not in any predicate lexicon
- "dependencies" → no API co-occurrence tokens present → gap (Rule 5)
- "blockers" → gap vocabulary
- "delivery" → gap vocabulary
- "manager" → not in any predicate lexicon

Result: ["QA"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-dependencies-team-001
- gap-project-managing-blockers-001
- gap-project-managing-manage-delivery-001

---

## B37-0A-005-005 — "coordinate" with "API" in sentence: API_CONTRACT from dependencies, coordinate → gap

**Input request:**
"Coordinate the API dependencies across services and document the integration contracts."

**Compiler analysis:**

Token scan:
- "coordinate" → not in any predicate lexicon (Rule 11)
- "API" → API co-occurrence token
- "dependencies" → API_CONTRACT fires (API co-occurrence — Rule 5)
- "document" → DOCUMENT fires (Rule 8)
- "integration" → API co-occurrence token (reinforces API_CONTRACT)
- "contracts" → not in predicate lexicon but "contract" is API co-occurrence token → reinforces API_CONTRACT

Result: ["API_CONTRACT", "DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-coordinate-001 ("coordinate" not captured)
