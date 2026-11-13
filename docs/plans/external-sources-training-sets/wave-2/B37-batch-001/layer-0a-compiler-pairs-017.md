# Layer 0-A — Compiler Pairs (017)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-017-001 — "lessons-learned" vocabulary: zero predicates fire

**Input request:**
"Run the lessons-learned session at the end of the project and collect feedback from all workstream leads."

**Compiler analysis:**

Token scan:
- "Run" → not in any predicate lexicon (run ≠ IMPLEMENT trigger list; build/implement/create/write/service/module fire IMPLEMENT)
- "lessons-learned" → not in any predicate lexicon (gap vocabulary)
- "session" → not in predicate lexicon
- "collect" → not in predicate lexicon
- "feedback" → not in predicate lexicon
- "workstream leads" → gap (workstream vocabulary)

Result: []

DomainId: "project-management" (zero predicates fired — Rule 14)

CapabilityGapWarnings:
- gap-project-managing-work-stream-001
- gap-project-managing-manage-delivery-001 (lessons-learned is project managing vocabulary)

SCHEDULE_CAPABILITY_LEARNING: emitted (zero-predicate gap request — Rule 17)

---

## B37-0A-017-002 — "lessons-learned" with "document": DOCUMENT fires

**Input request:**
"Document the lessons learned from the delivery programme and circulate to all project stakeholders."

**Compiler analysis:**

Token scan:
- "Document" → DOCUMENT fires (Rule 8)
- "lessons learned" → gap
- "delivery" → gap
- "programme" → not in predicate lexicon
- "project" → not in predicate lexicon
- "stakeholders" → DOCUMENT fires (Rule 6)

Result: ["DOCUMENT"]

DomainId: "software-development" (DOCUMENT predicate fired)

CapabilityGapWarnings:
- gap-project-managing-manage-delivery-001
- gap-project-managing-stakeholder-update-001 (stakeholder intent is circulation/notification)

---

## B37-0A-017-003 — "lessons-learned" with "track": zero predicates

**Input request:**
"Track the lessons-learned action items and follow up with owners to ensure completion."

**Compiler analysis:**

Token scan:
- "Track" → gap (Rule 2: track ≠ TEST)
- "lessons-learned" → gap
- "action items" → gap (action-items vocabulary)
- "follow up" → gap (follow-up vocabulary)
- "owners" → gap (accountability vocabulary)
- "completion" → not in predicate lexicon

Result: []

DomainId: "project-management"

CapabilityGapWarnings:
- gap-project-managing-track-001
- gap-project-managing-action-items-001
- gap-project-managing-follow-up-001
- gap-project-managing-accountability-001

SCHEDULE_CAPABILITY_LEARNING: emitted

---

## B37-0A-017-004 — "lessons-learned" with "review": QA fires

**Input request:**
"Review the lessons learned from the previous release and validate the improvement actions."

**Compiler analysis:**

Token scan:
- "Review" → QA fires (Rule 19)
- "lessons learned" → gap
- "release" → not in predicate lexicon
- "validate" → QA fires (validate → QA)
- "improvement actions" → gap (action-items vocabulary)

Result: ["QA"]

DomainId: "software-development" (QA predicate fired)

CapabilityGapWarnings:
- gap-project-managing-action-items-001

---

## B37-0A-017-005 — "lessons-learned" with "define" and "structure": DOCUMENT + ARCHITECTURE fire

**Input request:**
"Define the lessons-learned process structure for all future project deliveries."

**Compiler analysis:**

Token scan:
- "Define" → DOCUMENT fires (Rule 8)
- "lessons-learned" → gap
- "process" → not in predicate lexicon
- "structure" → ARCHITECTURE fires (Rule 7: structure → ARCHITECTURE always, even in governance/process context)
- "future" → not in predicate lexicon
- "project deliveries" → gap

Result: ["DOCUMENT", "ARCHITECTURE"]

DomainId: "software-development" (multiple predicates fired)

CapabilityGapWarnings:
- gap-project-managing-manage-delivery-001

Note: "structure" fires ARCHITECTURE even in "lessons-learned process structure". This is the same false positive as "accountability structure" and "governance structure".
