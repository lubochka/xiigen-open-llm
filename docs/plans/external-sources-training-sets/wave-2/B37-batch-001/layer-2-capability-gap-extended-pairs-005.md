# Layer 2 — Capability-Gap-Extended Pairs (005)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: capability-gap-extended-pairs

---

## B37-L2-005-001 — RAID log: full knowledge_edge trace

**Input:** "Set up and maintain the RAID log for all project risks, assumptions, issues, and dependencies."

**Full knowledge_edge trace:**

Step 1 — Compiler scan:
- "Set up" → not in IMPLEMENT trigger list (set up ≠ build/create/implement)
- "maintain" → not in any predicate lexicon
- "RAID" → not in any predicate lexicon → gap
- "log" → not in any predicate lexicon
- "risks" → not in any predicate lexicon → gap (RAID-R)
- "assumptions" → not in any predicate lexicon → gap (RAID-A)
- "issues" → not in any predicate lexicon → gap (RAID-I)
- "dependencies" → no API co-occurrence present → gap (RAID-D, Rule 5)

Step 2 — Predicate list: []

Step 3 — Gap mapping:
- "RAID log" → gap-project-managing-raid-log-001
- "dependencies" (no API) → gap-project-managing-dependencies-team-001
- Note: risks, assumptions, issues are subsumed under gap-project-managing-raid-log-001

Step 4 — DomainId: "project-management"

Step 5 — Route: knowledge_edge

Step 6 — SCHEDULE_CAPABILITY_LEARNING emitted

**Gap: gap-project-managing-raid-log-001, gap-project-managing-dependencies-team-001**

---

## B37-L2-005-002 — RAID log: "create" fires IMPLEMENT

**Input:** "Create the RAID log template for the project kick-off."

**Trace:**
- "Create" → IMPLEMENT fires (Rule 9: create is an IMPLEMENT trigger)
- "RAID" → gap
- "log" → not in predicate lexicon
- "template" → not in predicate lexicon

Result: ["IMPLEMENT"]

DomainId: "software-development"

CapabilityGapWarnings: gap-project-managing-raid-log-001

**Key:** "Create" fires IMPLEMENT even when creating a RAID log template (a managing artifact). The IMPLEMENT predicate fires on the lexical token "create" regardless of what is being created.

---

## B37-L2-005-003 — RAID log: "document" fires DOCUMENT

**Input:** "Document the RAID log entries and share with the project board."

**Trace:**
- "Document" → DOCUMENT fires (Rule 8)
- "RAID" → gap
- "share" → not in any predicate lexicon

Result: ["DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings: gap-project-managing-raid-log-001

---

## B37-L2-005-004 — RAID log: "review" fires QA

**Input:** "Review the RAID log and escalate any critical risks to the programme director."

**Trace:**
- "Review" → QA fires (Rule 19)
- "RAID" → gap
- "escalate" → gap (Rule 13)
- "risks" → gap (RAID-R)

Result: ["QA"]

DomainId: "software-development"

CapabilityGapWarnings: gap-project-managing-raid-log-001, gap-project-managing-escalation-001

---

## B37-L2-005-005 — RAID log + "test": TEST fires on "test"

**Input:** "Test the RAID log assumptions before the project baseline review."

**Trace:**
- "Test" → TEST fires (Rule 21: even "test the assumptions")
- "RAID" → gap
- "assumptions" → gap (RAID-A)
- "baseline" → not in any predicate lexicon
- "review" → QA fires (Rule 19)

Result: ["TEST", "QA"]

DomainId: "software-development"

CapabilityGapWarnings: gap-project-managing-raid-log-001

**Key:** "test the assumptions" in RAID context → TEST fires. "review" in managing context → QA fires. Both are false positives from RAID management perspective.
