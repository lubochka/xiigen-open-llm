# Layer 0-A — Compiler Pairs (009)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: compiler-pairs

---

## B37-0A-009-001 — IMPLEMENT fires even in project managing sentence

**Input request:**
"Manage the delivery of the authentication service and build the access control module."

**Compiler analysis:**

Token scan:
- "Manage" → not in any predicate lexicon (Rule 10: manage → NOT in any predicate lexicon)
- "delivery" → gap vocabulary
- "authentication" → not in any predicate lexicon (may relate to IMPLEMENT context but not a trigger token)
- "service" → IMPLEMENT fires on "service" (Rule 9: build/implement/create/write/service/module)
- "build" → IMPLEMENT fires on "build" (Rule 9)
- "access control" → not in any predicate lexicon
- "module" → IMPLEMENT fires on "module" (Rule 9)

Result: ["IMPLEMENT"]

DomainId: "software-development" (IMPLEMENT fired)

CapabilityGapWarnings:
- gap-project-managing-manage-delivery-001 ("manage the delivery" intent not captured)

Note: "manage" and "delivery" are gap vocabulary. IMPLEMENT fires on "service", "build", and "module" — three separate trigger tokens. The managing framing ("manage the delivery of") does not prevent IMPLEMENT from firing.

---

## B37-0A-009-002 — "implement" + "coordinate": IMPLEMENT fires, coordinate is gap

**Input request:**
"Implement the new reporting dashboard and coordinate the rollout with the operations team."

**Compiler analysis:**

Token scan:
- "Implement" → IMPLEMENT fires (Rule 9)
- "reporting" → not in any predicate lexicon (not a DOCUMENT trigger in this form)
- "dashboard" → not in any predicate lexicon
- "coordinate" → not in any predicate lexicon (Rule 11)
- "rollout" → not in any predicate lexicon
- "operations" → not in any predicate lexicon

Result: ["IMPLEMENT"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-coordinate-001 ("coordinate the rollout" managing intent)

---

## B37-0A-009-003 — "write" fires IMPLEMENT: even in documentation-adjacent sentence

**Input request:**
"Write the tracking script and manage its deployment across all project environments."

**Compiler analysis:**

Token scan:
- "Write" → IMPLEMENT fires on "write" (Rule 9: write (code) is an IMPLEMENT trigger)
- "tracking" → not in any predicate lexicon (gap: "track" family)
- "script" → not in any predicate lexicon directly, but IMPLEMENT fired via "write"
- "manage" → not in any predicate lexicon (Rule 10)
- "deployment" → not in any predicate lexicon
- "environments" → not in any predicate lexicon

Result: ["IMPLEMENT"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-manage-delivery-001 ("manage its deployment" managing intent)

---

## B37-0A-009-004 — "create" fires IMPLEMENT: even for non-code artifacts

**Input request:**
"Create the status tracking module and integrate it into the project management portal."

**Compiler analysis:**

Token scan:
- "Create" → IMPLEMENT fires on "create" (Rule 9)
- "status" → not in any predicate lexicon (Rule 4)
- "tracking" → gap vocabulary (track family)
- "module" → IMPLEMENT fires on "module" (Rule 9)
- "integrate" → not in any predicate lexicon (note: "integration" is API co-occurrence token but "integrate" alone does not fire IMPLEMENT or API_CONTRACT)
- "project management" → gap vocabulary ("manage" → Rule 10)

Result: ["IMPLEMENT"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-track-status-001 ("status tracking" managing intent)

---

## B37-0A-009-005 — Pure managing: no IMPLEMENT tokens → zero predicates

**Input request:**
"Manage all project workstreams, coordinate deliverables, and escalate blockers to the PMO."

**Compiler analysis:**

Token scan:
- "Manage" → gap (Rule 10)
- "workstreams" → gap vocabulary
- "coordinate" → gap (Rule 11)
- "deliverables" → not in any predicate lexicon
- "escalate" → gap (Rule 13)
- "blockers" → gap vocabulary
- "PMO" → not in any predicate lexicon

Result: []

DomainId: "project-management"

CapabilityGapWarnings:
- gap-project-managing-work-stream-001
- gap-project-managing-coordinate-001
- gap-project-managing-escalation-001
- gap-project-managing-blockers-001

SCHEDULE_CAPABILITY_LEARNING: emitted
