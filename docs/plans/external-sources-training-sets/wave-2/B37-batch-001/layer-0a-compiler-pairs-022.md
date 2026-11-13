# Layer 0-A — Compiler Pairs (022)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-022-001 — Mixed: managing vocab + DOCUMENT (stakeholder report)

**Input request:**
"Produce the weekly progress report for stakeholders and coordinate action items."

**Compiler analysis:**

Token scan:
- "Produce" → not in predicate lexicon
- "weekly progress report" → "progress" gap (Rule 12), "report" gap (Rule 3: report ≠ DOCUMENT)
- "stakeholders" → DOCUMENT fires (Rule 6)
- "coordinate" → not in predicate lexicon (Rule 11)
- "action items" → gap

Result: ["DOCUMENT"]

DomainId: "software-development" (DOCUMENT predicate fired on "stakeholders")

CapabilityGapWarnings:
- gap-project-managing-progress-001
- gap-project-managing-status-report-001
- gap-project-managing-stakeholder-update-001
- gap-project-managing-coordinate-001
- gap-project-managing-action-items-001

---

## B37-0A-022-002 — Mixed: managing vocab + DOCUMENT (stakeholder sign-off)

**Input request:**
"Track delivery blockers and escalate critical items to stakeholders for sign-off."

**Compiler analysis:**

Token scan:
- "Track" → gap (Rule 2)
- "delivery blockers" → gap
- "escalate" → gap (Rule 13)
- "critical items" → not in predicate lexicon
- "stakeholders" → DOCUMENT fires (Rule 6)
- "sign-off" → not in predicate lexicon

Result: ["DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-track-001
- gap-project-managing-blockers-001
- gap-project-managing-escalation-001
- gap-project-managing-stakeholder-update-001

---

## B37-0A-022-003 — Managing vocab only, no DOCUMENT: zero predicates

**Input request:**
"Track delivery blockers and escalate critical items to the programme board."

**Compiler analysis:**

Token scan:
- "Track" → gap (Rule 2)
- "delivery blockers" → gap
- "escalate" → gap (Rule 13)
- "critical items" → not in predicate lexicon
- "programme board" → not in predicate lexicon (not "stakeholder")

Result: []

DomainId: "project-management"

CapabilityGapWarnings:
- gap-project-managing-track-001
- gap-project-managing-blockers-001
- gap-project-managing-escalation-001
- gap-project-managing-manage-delivery-001

SCHEDULE_CAPABILITY_LEARNING: emitted

**Contrast with 022-002:** Replacing "stakeholders" with "programme board" removes the DOCUMENT predicate.

---

## B37-0A-022-004 — Mixed: manage + coordinate + DOCUMENT (stakeholder in title)

**Input request:**
"Coordinate the delivery of stakeholder communications for the programme."

**Compiler analysis:**

Token scan:
- "Coordinate" → not in predicate lexicon (Rule 11)
- "delivery" → gap
- "stakeholder" → DOCUMENT fires (Rule 6)
- "communications" → not in predicate lexicon
- "programme" → not in predicate lexicon

Result: ["DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-coordinate-001
- gap-project-managing-manage-delivery-001
- gap-project-managing-stakeholder-update-001

---

## B37-0A-022-005 — Full managing vocabulary pile-up + DOCUMENT

**Input request:**
"Track blockers, coordinate escalations, follow up on action items, and report status to stakeholders."

**Compiler analysis:**

Token scan:
- "Track" → gap
- "blockers" → gap
- "coordinate" → not in predicate lexicon
- "escalations" → gap
- "follow up" → gap
- "action items" → gap
- "report" → DOCUMENT does NOT fire (Rule 3)
- "status" → gap (Rule 4)
- "stakeholders" → DOCUMENT fires (Rule 6)

Result: ["DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-track-001
- gap-project-managing-blockers-001
- gap-project-managing-coordinate-001
- gap-project-managing-escalation-001
- gap-project-managing-follow-up-001
- gap-project-managing-action-items-001
- gap-project-managing-status-report-001
- gap-project-managing-stakeholder-update-001
