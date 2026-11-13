# Layer 0-A — Compiler Pairs (006)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: compiler-pairs

---

## B37-0A-006-001 — "dependencies" WITHOUT API co-occurrence → GAP only

**Input request:**
"Map out the project dependencies and identify the blockers holding up the delivery."

**Compiler analysis:**

Token scan:
- "Map out" → not in any predicate lexicon
- "project" → not in any predicate lexicon
- "dependencies" → no API co-occurrence tokens present → gap (Rule 5: API_CONTRACT ONLY with API co-occurrence)
- "identify" → not in any predicate lexicon
- "blockers" → not in any predicate lexicon (gap vocabulary)
- "delivery" → gap vocabulary

Result: []

DomainId: "project-management" (zero predicates)

CapabilityGapWarnings:
- gap-project-managing-dependencies-team-001
- gap-project-managing-blockers-001
- gap-project-managing-manage-delivery-001

SCHEDULE_CAPABILITY_LEARNING: emitted

knowledge_edge: "dependencies" without API co-occurrence is a gap; "dependencies" with API co-occurrence → API_CONTRACT

---

## B37-0A-006-002 — "team dependencies" explicitly: zero predicates

**Input request:**
"List all team dependencies for Q3 delivery and escalate cross-team blockers to the PMO."

**Compiler analysis:**

Token scan:
- "team dependencies" → no API co-occurrence → gap (Rule 5)
- "Q3 delivery" → gap vocabulary
- "escalate" → not in any predicate lexicon (Rule 13)
- "cross-team" → not in any predicate lexicon
- "blockers" → gap vocabulary
- "PMO" → not in any predicate lexicon

Result: []

DomainId: "project-management"

CapabilityGapWarnings:
- gap-project-managing-dependencies-team-001
- gap-project-managing-escalation-001
- gap-project-managing-blockers-001
- gap-project-managing-manage-delivery-001

SCHEDULE_CAPABILITY_LEARNING: emitted

---

## B37-0A-006-003 — "dependencies" near "service" but NOT "API": context-sensitivity test

**Input request:**
"Resolve the dependencies between the backend services before the release."

**Compiler analysis:**

Token scan:
- "dependencies" → co-occurrence check: "service" is NOT in the API co-occurrence token list when not accompanied by "api", "contract", "endpoint", "interface", "integration", "swagger", "openapi", "graphql", "rest", "grpc", "webhook"
- "backend" → not an API co-occurrence token by itself
- "services" → not an API co-occurrence token by itself
- "release" → not in any predicate lexicon

Result: []

DomainId: "project-management"

CapabilityGapWarnings:
- gap-project-managing-dependencies-team-001

SCHEDULE_CAPABILITY_LEARNING: emitted

Note: Critical boundary — "service" alone does NOT trigger API co-occurrence for "dependencies". Only explicit API vocabulary (api, contract, endpoint, interface, integration, swagger, openapi, graphql, rest, grpc, webhook) counts.

---

## B37-0A-006-004 — "blockers" standalone: zero predicates

**Input request:**
"Identify and resolve all blockers preventing the team from hitting the sprint deadline."

**Compiler analysis:**

Token scan:
- "Identify" → not in any predicate lexicon
- "blockers" → gap vocabulary
- "team" → not in any predicate lexicon
- "sprint" → not in any predicate lexicon (B36 domain: project-planning; not a predicate in either B36 or B37)
- "deadline" → not in any predicate lexicon

Result: []

DomainId: "project-management"

CapabilityGapWarnings:
- gap-project-managing-blockers-001

SCHEDULE_CAPABILITY_LEARNING: emitted

---

## B37-0A-006-005 — "blockers" + "coordinate": full gap request

**Input request:**
"Coordinate with engineering leads to unblock all blockers and report status to the steering group."

**Compiler analysis:**

Token scan:
- "Coordinate" → not in any predicate lexicon (Rule 11)
- "unblock" → not in any predicate lexicon
- "blockers" → gap vocabulary
- "report" → DOCUMENT does NOT fire on "report" alone (Rule 3)
- "status" → not in any predicate lexicon (Rule 4)
- "steering" → not in any predicate lexicon

Result: []

DomainId: "project-management"

CapabilityGapWarnings:
- gap-project-managing-coordinate-blockers-001 (compound gap ID)
- gap-project-managing-status-report-001

SCHEDULE_CAPABILITY_LEARNING: emitted
