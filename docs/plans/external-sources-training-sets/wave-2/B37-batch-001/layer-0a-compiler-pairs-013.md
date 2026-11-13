# Layer 0-A — Compiler Pairs (013)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-013-001 — "risk-log" vocabulary: zero predicates fire

**Input request:**
"Update the risk log with all new risks identified during the sprint retrospective."

**Compiler analysis:**

Token scan:
- "Update" → not in any predicate lexicon (update ≠ DOCUMENT trigger; document/define/describe/requirement fire DOCUMENT)
- "risk log" → not in any predicate lexicon (gap vocabulary)
- "risks" → not in any predicate lexicon
- "identified" → not in any predicate lexicon
- "sprint" → not in predicate lexicon (B36 planning domain; not in B37 predicate lexicons)
- "retrospective" → not in any predicate lexicon

Result: []

DomainId: "project-management" (zero predicates fired — Rule 14)

CapabilityGapWarnings:
- gap-project-managing-raid-log-001 (risk log is RAID-R vocabulary)

SCHEDULE_CAPABILITY_LEARNING: emitted (zero-predicate gap request — Rule 17)

---

## B37-0A-013-002 — "risk-log" with "review": QA fires

**Input request:**
"Review all entries in the risk log and escalate high-severity items."

**Compiler analysis:**

Token scan:
- "Review" → QA fires (Rule 19: review fires QA always, even in managing context)
- "risk log" → gap
- "escalate" → not in any predicate lexicon (Rule 13)
- "high-severity" → not in any predicate lexicon

Result: ["QA"]

DomainId: "software-development" (QA predicate fired — Rule 14)

CapabilityGapWarnings:
- gap-project-managing-raid-log-001
- gap-project-managing-escalation-001

---

## B37-0A-013-003 — "risk-log" with "document": DOCUMENT fires

**Input request:**
"Document all project risks in the risk log, including probability and impact ratings."

**Compiler analysis:**

Token scan:
- "Document" → DOCUMENT fires (Rule 8)
- "project risks" → gap vocabulary
- "risk log" → gap vocabulary
- "probability" → not in any predicate lexicon
- "impact" → not in any predicate lexicon

Result: ["DOCUMENT"]

DomainId: "software-development" (DOCUMENT predicate fired — Rule 14)

CapabilityGapWarnings:
- gap-project-managing-raid-log-001

---

## B37-0A-013-004 — "risk-log" pure maintenance: zero predicates

**Input request:**
"Maintain the risk log by tracking new risks and monitoring existing risk mitigations."

**Compiler analysis:**

Token scan:
- "Maintain" → not in any predicate lexicon
- "risk log" → gap
- "tracking" → gap (track family — Rule 2: track ≠ TEST)
- "risks" → gap
- "monitoring" → not in any predicate lexicon
- "mitigations" → not in any predicate lexicon

Result: []

DomainId: "project-management"

CapabilityGapWarnings:
- gap-project-managing-raid-log-001
- gap-project-managing-track-001

SCHEDULE_CAPABILITY_LEARNING: emitted

---

## B37-0A-013-005 — "risk-log" with "build": IMPLEMENT fires

**Input request:**
"Build a risk log dashboard to track all open risks and their owners."

**Compiler analysis:**

Token scan:
- "Build" → IMPLEMENT fires (Rule 9)
- "risk log" → gap
- "dashboard" → not in predicate lexicon
- "track" → gap (Rule 2: track ≠ TEST)
- "risks" → gap
- "owners" → not in predicate lexicon

Result: ["IMPLEMENT"]

DomainId: "software-development" (IMPLEMENT predicate fired)

CapabilityGapWarnings:
- gap-project-managing-raid-log-001
- gap-project-managing-track-001
- gap-project-managing-accountability-001 (owners = accountability vocabulary)
