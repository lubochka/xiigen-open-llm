# Layer 0-A — Compiler Pairs (014)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-014-001 — "change-request" vocabulary: zero predicates fire

**Input request:**
"Process the change request and coordinate approval from the change advisory board."

**Compiler analysis:**

Token scan:
- "Process" → not in any predicate lexicon
- "change request" → not in any predicate lexicon (gap vocabulary)
- "coordinate" → not in any predicate lexicon (Rule 11)
- "approval" → not in any predicate lexicon
- "change advisory board" → not in any predicate lexicon

Result: []

DomainId: "project-management" (zero predicates fired — Rule 14)

CapabilityGapWarnings:
- gap-project-managing-coordinate-001
- gap-project-managing-manage-delivery-001 (change management is delivery-managing vocabulary)

SCHEDULE_CAPABILITY_LEARNING: emitted (zero-predicate gap request — Rule 17)

---

## B37-0A-014-002 — "change-request" with "document": DOCUMENT fires

**Input request:**
"Document the change request with full impact analysis and stakeholder sign-off requirements."

**Compiler analysis:**

Token scan:
- "Document" → DOCUMENT fires (Rule 8)
- "change request" → gap
- "impact analysis" → not in predicate lexicon
- "stakeholder" → DOCUMENT fires (Rule 6: stakeholder always fires DOCUMENT)
- "requirements" → DOCUMENT fires (Rule 8: requirement fires DOCUMENT)

Result: ["DOCUMENT"]

DomainId: "software-development" (DOCUMENT predicate fired three times; one DOCUMENT predicate)

CapabilityGapWarnings:
- gap-project-managing-manage-delivery-001
- gap-project-managing-stakeholder-update-001 (stakeholder intent is notification, not documentation)

---

## B37-0A-014-003 — "change-request" with "review": QA fires

**Input request:**
"Review the pending change requests in the change log before the CAB (Change Advisory Board) meeting."

**Compiler analysis:**

Token scan:
- "Review" → QA fires (Rule 19)
- "change requests" → gap
- "change log" → gap (change management vocabulary)
- "CAB" → not in predicate lexicon
- "meeting" → not in predicate lexicon

Result: ["QA"]

DomainId: "software-development" (QA predicate fired)

CapabilityGapWarnings:
- gap-project-managing-manage-delivery-001

---

## B37-0A-014-004 — "change-request" track status: zero predicates

**Input request:**
"Track all open change requests and escalate any that are overdue to the project manager."

**Compiler analysis:**

Token scan:
- "Track" → gap (Rule 2: track ≠ TEST)
- "change requests" → gap
- "escalate" → not in any predicate lexicon (Rule 13)
- "overdue" → not in any predicate lexicon
- "project manager" → not in any predicate lexicon

Result: []

DomainId: "project-management"

CapabilityGapWarnings:
- gap-project-managing-track-001
- gap-project-managing-escalation-001
- gap-project-managing-manage-delivery-001

SCHEDULE_CAPABILITY_LEARNING: emitted

---

## B37-0A-014-005 — "change-request" with "implement": IMPLEMENT fires

**Input request:**
"Implement the approved change request to add authentication to the user service."

**Compiler analysis:**

Token scan:
- "Implement" → IMPLEMENT fires (Rule 9)
- "change request" → gap
- "authentication" → not in predicate lexicon
- "user service" → IMPLEMENT fires on "service" (Rule 9)

Result: ["IMPLEMENT"]

DomainId: "software-development" (IMPLEMENT predicate fired)

CapabilityGapWarnings:
- gap-project-managing-manage-delivery-001

Note: "change request" is gap vocabulary but the sentence contains two IMPLEMENT triggers (implement, service). The compiler fires IMPLEMENT regardless of managing framing.
