# Layer 0-A — Compiler Pairs (021)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-021-001 — "dependencies" with API co-occurrence: API_CONTRACT fires

**Input request:**
"Map all API dependencies between the payment service and the user authentication endpoint."

**Compiler analysis:**

Token scan:
- "Map" → not in predicate lexicon
- "API" → API co-occurrence token (api is on the API co-occurrence list)
- "dependencies" → API_CONTRACT fires (API co-occurrence present — Rule 5)
- "payment service" → IMPLEMENT fires on "service" (Rule 9)
- "user authentication" → not directly in predicate lexicon
- "endpoint" → API co-occurrence token (endpoint is on the API co-occurrence list)

Result: ["API_CONTRACT", "IMPLEMENT"]

DomainId: "software-development" (multiple predicates fired)

CapabilityGapWarnings: none (all vocabulary is predicate-covered)

---

## B37-0A-021-002 — "dependencies" without API co-occurrence: zero predicates

**Input request:**
"Map all team dependencies for the delivery programme and track blockers."

**Compiler analysis:**

Token scan:
- "Map" → not in predicate lexicon
- "team dependencies" → no API co-occurrence present → gap (Rule 5)
- "delivery programme" → gap
- "track" → gap (Rule 2: track ≠ TEST)
- "blockers" → gap

Result: []

DomainId: "project-management" (zero predicates fired)

CapabilityGapWarnings:
- gap-project-managing-dependencies-team-001
- gap-project-managing-manage-delivery-001
- gap-project-managing-track-001
- gap-project-managing-blockers-001

SCHEDULE_CAPABILITY_LEARNING: emitted

**Critical boundary:** "team dependencies" vs "API dependencies". API co-occurrence determines the predicate.

---

## B37-0A-021-003 — "dependencies" with "integration": API_CONTRACT fires

**Input request:**
"Identify all integration dependencies between the notification service and the messaging module."

**Compiler analysis:**

Token scan:
- "Identify" → not in predicate lexicon
- "integration" → API co-occurrence token (integration is on the API co-occurrence list)
- "dependencies" → API_CONTRACT fires (integration is API co-occurrence)
- "notification service" → IMPLEMENT fires on "service" (Rule 9)
- "messaging module" → IMPLEMENT fires on "module" (Rule 9)

Result: ["API_CONTRACT", "IMPLEMENT"]

DomainId: "software-development"

CapabilityGapWarnings: none

---

## B37-0A-021-004 — "dependencies" with "project/workstream": zero predicates

**Input request:**
"Resolve cross-workstream dependencies before the upcoming delivery milestone."

**Compiler analysis:**

Token scan:
- "Resolve" → not in predicate lexicon
- "cross-workstream" → gap (workstream vocabulary)
- "dependencies" → no API co-occurrence → gap (Rule 5)
- "delivery milestone" → gap

Result: []

DomainId: "project-management"

CapabilityGapWarnings:
- gap-project-managing-dependencies-team-001
- gap-project-managing-work-stream-001
- gap-project-managing-manage-delivery-001

SCHEDULE_CAPABILITY_LEARNING: emitted

---

## B37-0A-021-005 — "dependencies" with "swagger": API_CONTRACT fires

**Input request:**
"Review all swagger dependencies and update the API contract documentation."

**Compiler analysis:**

Token scan:
- "Review" → QA fires (Rule 19)
- "swagger" → API co-occurrence token
- "dependencies" → API_CONTRACT fires (swagger is API co-occurrence)
- "API contract" → API_CONTRACT fires (both "API" and "contract" are co-occurrence tokens)
- "documentation" → not directly in DOCUMENT trigger list (but "document" would fire; "documentation" is debatable — treat as gap here)

Result: ["QA", "API_CONTRACT"]

DomainId: "software-development"

CapabilityGapWarnings: none (all vocabulary covered by predicates)
