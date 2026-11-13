# Layer 0-A — Compiler Pairs (019)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-019-001 — "stakeholder" → DOCUMENT fires: simple case

**Input request:**
"Send a status update to all project stakeholders confirming the delivery is on track."

**Compiler analysis:**

Token scan:
- "Send" → not in any predicate lexicon
- "status update" → gap (status vocabulary — Rule 4: status fires nothing)
- "project" → not in predicate lexicon
- "stakeholders" → DOCUMENT fires (Rule 6: stakeholder → DOCUMENT always)
- "delivery" → gap
- "on track" → gap (track-status vocabulary)

Result: ["DOCUMENT"]

DomainId: "software-development" (DOCUMENT predicate fired on "stakeholders" — Rule 14)

CapabilityGapWarnings:
- gap-project-managing-status-report-001
- gap-project-managing-track-status-001
- gap-project-managing-stakeholder-update-001 (intent is notification, not documentation)
- gap-project-managing-manage-delivery-001

---

## B37-0A-019-002 — "stakeholder" in escalation context: DOCUMENT fires despite managing framing

**Input request:**
"Escalate the delivery blocker to senior stakeholders and coordinate an emergency response."

**Compiler analysis:**

Token scan:
- "Escalate" → not in any predicate lexicon (Rule 13)
- "delivery blocker" → gap
- "senior stakeholders" → DOCUMENT fires (Rule 6: stakeholder token fires DOCUMENT regardless of framing)
- "coordinate" → not in any predicate lexicon (Rule 11)
- "emergency response" → not in predicate lexicon

Result: ["DOCUMENT"]

DomainId: "software-development" (DOCUMENT predicate fired)

CapabilityGapWarnings:
- gap-project-managing-escalation-001
- gap-project-managing-blockers-001
- gap-project-managing-stakeholder-update-001
- gap-project-managing-coordinate-001

---

## B37-0A-019-003 — Multiple "stakeholder" occurrences: ONE DOCUMENT predicate

**Input request:**
"Notify all stakeholders of the schedule change and prepare a stakeholder communication plan."

**Compiler analysis:**

Token scan:
- "Notify" → not in predicate lexicon
- "stakeholders" → DOCUMENT fires (Rule 6) — first occurrence
- "schedule change" → not in predicate lexicon
- "stakeholder" → DOCUMENT fires (Rule 6) — second occurrence
- "communication plan" → not in predicate lexicon (plan ≠ spec/define/document)

Result: ["DOCUMENT"] (one predicate despite two firings)

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-stakeholder-update-001

---

## B37-0A-019-004 — No "stakeholder" → zero predicates (boundary contrast)

**Input request:**
"Send a status update to the programme board confirming the delivery is on track."

**Compiler analysis:**

Token scan:
- "Send" → not in predicate lexicon
- "status update" → gap (Rule 4)
- "programme board" → not in predicate lexicon (not "stakeholder")
- "delivery" → gap
- "on track" → gap

Result: []

DomainId: "project-management"

CapabilityGapWarnings:
- gap-project-managing-status-report-001
- gap-project-managing-track-status-001
- gap-project-managing-manage-delivery-001

SCHEDULE_CAPABILITY_LEARNING: emitted

**Key contrast with 019-001:** "programme board" does NOT fire DOCUMENT. "stakeholders" DOES fire DOCUMENT. The token matters, not the intent.

---

## B37-0A-019-005 — "stakeholder" with "requirements": double DOCUMENT fire

**Input request:**
"Gather requirements from stakeholders and track progress against the agreed delivery timeline."

**Compiler analysis:**

Token scan:
- "Gather" → not in predicate lexicon
- "requirements" → DOCUMENT fires (Rule 8: requirement fires DOCUMENT)
- "stakeholders" → DOCUMENT fires (Rule 6)
- "track" → gap (Rule 2: track ≠ TEST)
- "progress" → gap (Rule 12)
- "delivery timeline" → gap

Result: ["DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-stakeholder-update-001
- gap-project-managing-track-progress-001
- gap-project-managing-manage-delivery-001

Note: "requirements" and "stakeholders" both fire DOCUMENT, but only ONE DOCUMENT predicate is produced.
