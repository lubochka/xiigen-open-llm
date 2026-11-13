# Layer 0-A — Compiler Pairs (007)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: compiler-pairs

---

## B37-0A-007-001 — "review" fires QA even in managing context

**Input request:**
"Review the escalation log and follow up with the engineering team on open action items."

**Compiler analysis:**

Token scan:
- "Review" → QA fires on "review" ALWAYS (Rule 19: even in project managing context)
- "escalation" → not in any predicate lexicon (Rule 13)
- "log" → not in any predicate lexicon
- "follow up" → not in any predicate lexicon (gap vocabulary)
- "action items" → not in any predicate lexicon (gap vocabulary)

Result: ["QA"]

DomainId: "software-development" (QA predicate fired — Rule 14)

CapabilityGapWarnings:
- gap-project-managing-escalation-001
- gap-project-managing-follow-up-001
- gap-project-managing-action-items-001

Note: QA fires on "review" regardless of whether the context is code review, document review, or project managing review. The intent here is project managing review, but the lexical token fires QA.

---

## B37-0A-007-002 — "review" + "follow-up": QA fires, follow-up is gap

**Input request:**
"Review the weekly action items list and follow up on any items marked as overdue."

**Compiler analysis:**

Token scan:
- "Review" → QA fires (Rule 19)
- "action items" → gap vocabulary
- "follow up" → gap vocabulary
- "overdue" → not in any predicate lexicon

Result: ["QA"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-action-items-001
- gap-project-managing-follow-up-001

---

## B37-0A-007-003 — "review" absent: pure action-items/follow-up → zero predicates

**Input request:**
"Send follow-up messages to all assignees with open action items from yesterday's standup."

**Compiler analysis:**

Token scan:
- "follow-up" → gap vocabulary
- "action items" → gap vocabulary
- "standup" → not in any predicate lexicon

Result: []

DomainId: "project-management"

CapabilityGapWarnings:
- gap-project-managing-follow-up-001
- gap-project-managing-action-items-001

SCHEDULE_CAPABILITY_LEARNING: emitted

---

## B37-0A-007-004 — "review" + "escalation" + "accountability": QA fires, rest are gap

**Input request:**
"Review the escalation process and ensure accountability across all delivery workstreams."

**Compiler analysis:**

Token scan:
- "Review" → QA fires (Rule 19)
- "escalation" → gap vocabulary
- "accountability" → gap vocabulary
- "delivery" → gap vocabulary
- "workstreams" → gap vocabulary

Result: ["QA"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-escalation-001
- gap-project-managing-accountability-001
- gap-project-managing-work-stream-001

---

## B37-0A-007-005 — "review" + "structure" + "escalation": QA + ARCHITECTURE fire

**Input request:**
"Review the escalation structure and document the follow-up procedures for critical incidents."

**Compiler analysis:**

Token scan:
- "Review" → QA fires (Rule 19)
- "escalation" → gap vocabulary
- "structure" → ARCHITECTURE fires (Rule 7)
- "document" → DOCUMENT fires (Rule 8)
- "follow-up" → gap vocabulary
- "procedures" → not in any predicate lexicon

Result: ["QA", "ARCHITECTURE", "DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-escalation-001
- gap-project-managing-follow-up-001
