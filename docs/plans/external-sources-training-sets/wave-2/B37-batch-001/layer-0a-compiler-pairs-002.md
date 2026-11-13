# Layer 0-A — Compiler Pairs (002)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: compiler-pairs

---

## B37-0A-002-001 — RAID log request: zero predicates fire

**Input request:**
"Set up the RAID log for the project and track all risks, assumptions, issues, and dependencies."

**Compiler analysis:**

Token scan:
- "RAID" → not in any predicate lexicon (gap vocabulary)
- "track" → not in any predicate lexicon (gap vocabulary — B37 Rule 2: track ≠ TEST)
- "risks" → not in any predicate lexicon
- "assumptions" → not in any predicate lexicon
- "issues" → not in any predicate lexicon
- "dependencies" → API_CONTRACT requires API co-occurrence; no API co-occurrence present → gap

Result: []

DomainId: "project-management" (zero predicates fired — Rule 14)

CapabilityGapWarnings:
- gap-project-managing-raid-log-001
- gap-project-managing-track-001
- gap-project-managing-dependencies-team-001

SCHEDULE_CAPABILITY_LEARNING: emitted (zero-predicate gap request — Rule 17)

---

## B37-0A-002-002 — "track" boundary confirmation: zero-predicate result

**Input request:**
"Track the delivery milestones and report on the current RAG-status for each workstream."

**Compiler analysis:**

Token scan:
- "track" → NOT in TEST lexicon; TEST fires on: test, verify, assert, validate, coverage, spec, case, suite, scenario, acceptance, unit, regression, e2e — "track" has NO entry
- "delivery" → not in any predicate lexicon (gap vocabulary)
- "report" → DOCUMENT does NOT fire on "report"; DOCUMENT fires on: document, spec, define, describe, requirement, story, criteria, stakeholder, functional, behavior, use-case
- "RAG-status" → not in any predicate lexicon (gap vocabulary)
- "workstream" → not in any predicate lexicon (gap vocabulary)

Result: []

DomainId: "project-management"

CapabilityGapWarnings:
- gap-project-managing-track-001
- gap-project-managing-rag-status-001
- gap-project-managing-work-stream-001

SCHEDULE_CAPABILITY_LEARNING: emitted

---

## B37-0A-002-003 — RAID log with team dependencies: confirming gap boundary

**Input request:**
"Populate the RAID log with all open risks and team dependencies before the sprint review."

**Compiler analysis:**

Token scan:
- "RAID" → gap
- "risks" → gap
- "dependencies" → no API co-occurrence ("team dependencies" context) → gap
- "sprint" → not in B37 gap lexicon (B36 domain: project-planning); not in predicate lexicon
- "review" → QA fires on "review" (Rule 19)

Result: ["QA"]

DomainId: "software-development" (QA predicate fired — Rule 14)

CapabilityGapWarnings:
- gap-project-managing-raid-log-001
- gap-project-managing-dependencies-team-001
Note: "sprint review" in B36 domain — B37 gap warnings apply to RAID/dependencies only

---

## B37-0A-002-004 — Pure RAID log: no review, no predicates

**Input request:**
"Create the RAID log template and assign owners to each risk, assumption, and issue entry."

**Compiler analysis:**

Token scan:
- "Create" → IMPLEMENT fires on create (Rule 9: build/implement/create/write/service/module)

Result: ["IMPLEMENT"]

DomainId: "software-development" (IMPLEMENT fired)

CapabilityGapWarnings:
- gap-project-managing-raid-log-001
Note: "Create" in this context refers to creating a template document, but the compiler fires IMPLEMENT on the lexical token "create" regardless of managing intent.

---

## B37-0A-002-005 — Confirming SCHEDULE_CAPABILITY_LEARNING for zero-predicate RAID request

**Input request:**
"Monitor the RAID log weekly and escalate any critical risks to the steering committee."

**Compiler analysis:**

Token scan:
- "Monitor" → not in any predicate lexicon
- "RAID" → gap
- "escalate" → not in any predicate lexicon (Rule 13: escalate/escalation → NOT in any predicate lexicon)
- "risks" → gap
- "steering" → not in any predicate lexicon

Result: []

DomainId: "project-management"

CapabilityGapWarnings:
- gap-project-managing-raid-log-001
- gap-project-managing-escalation-001
- gap-project-managing-track-status-001

SCHEDULE_CAPABILITY_LEARNING: emitted (zero predicates, gap request)
