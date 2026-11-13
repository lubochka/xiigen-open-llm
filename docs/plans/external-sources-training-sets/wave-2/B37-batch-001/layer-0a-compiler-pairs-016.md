# Layer 0-A — Compiler Pairs (016)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-016-001 — "decision-log" vocabulary: zero predicates fire

**Input request:**
"Update the decision log with all key decisions made during the steering committee meeting."

**Compiler analysis:**

Token scan:
- "Update" → not in any predicate lexicon (update ≠ DOCUMENT trigger)
- "decision log" → not in any predicate lexicon (gap vocabulary)
- "key decisions" → not in any predicate lexicon
- "steering committee" → not in any predicate lexicon

Result: []

DomainId: "project-management" (zero predicates fired — Rule 14)

CapabilityGapWarnings:
- gap-project-managing-manage-delivery-001 (decision-log is project managing vocabulary)
- gap-project-managing-accountability-001 (decisions have owners)

SCHEDULE_CAPABILITY_LEARNING: emitted (zero-predicate gap request — Rule 17)

---

## B37-0A-016-002 — "decision-log" with "document": DOCUMENT fires

**Input request:**
"Document all architectural decisions in the decision log with rationale and alternatives considered."

**Compiler analysis:**

Token scan:
- "Document" → DOCUMENT fires (Rule 8)
- "architectural" → ARCHITECTURE fires (architecture/design/structure — "architectural" triggers ARCHITECTURE)
- "decisions" → gap
- "decision log" → gap
- "rationale" → not in predicate lexicon
- "alternatives" → not in predicate lexicon

Result: ["DOCUMENT", "ARCHITECTURE"]

DomainId: "software-development" (multiple predicates fired)

CapabilityGapWarnings:
- gap-project-managing-manage-delivery-001

---

## B37-0A-016-003 — "decision-log" with "track": zero predicates

**Input request:**
"Track all open decisions in the decision log and follow up with owners on outstanding items."

**Compiler analysis:**

Token scan:
- "Track" → gap (Rule 2: track ≠ TEST)
- "decisions" → gap
- "decision log" → gap
- "follow up" → gap (follow-up vocabulary)
- "owners" → gap (accountability vocabulary)

Result: []

DomainId: "project-management"

CapabilityGapWarnings:
- gap-project-managing-track-001
- gap-project-managing-follow-up-001
- gap-project-managing-accountability-001
- gap-project-managing-manage-delivery-001

SCHEDULE_CAPABILITY_LEARNING: emitted

---

## B37-0A-016-004 — "decision-log" with "review": QA fires

**Input request:**
"Review the decision log before the programme board and confirm all decisions have been actioned."

**Compiler analysis:**

Token scan:
- "Review" → QA fires (Rule 19)
- "decision log" → gap
- "programme board" → not in predicate lexicon
- "confirm" → not in predicate lexicon
- "decisions" → gap
- "actioned" → not in predicate lexicon

Result: ["QA"]

DomainId: "software-development" (QA predicate fired)

CapabilityGapWarnings:
- gap-project-managing-manage-delivery-001
- gap-project-managing-action-items-001 (actioned = action-items vocabulary)

---

## B37-0A-016-005 — "decision-log" with "define": DOCUMENT fires

**Input request:**
"Define the decision-making process and log all decisions in the decision log."

**Compiler analysis:**

Token scan:
- "Define" → DOCUMENT fires (Rule 8)
- "decision-making process" → not in predicate lexicon
- "log" → not in DOCUMENT trigger list (log ≠ document/define/describe/etc.)
- "decisions" → gap
- "decision log" → gap

Result: ["DOCUMENT"]

DomainId: "software-development" (DOCUMENT predicate fired)

CapabilityGapWarnings:
- gap-project-managing-manage-delivery-001
- gap-project-managing-accountability-001
