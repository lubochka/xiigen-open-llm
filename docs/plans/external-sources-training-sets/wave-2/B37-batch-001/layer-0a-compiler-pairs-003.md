# Layer 0-A — Compiler Pairs (003)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: compiler-pairs

---

## B37-0A-003-001 — "stakeholder" fires DOCUMENT: mixed signal in managing context

**Input request:**
"Send a progress update to stakeholders and report the on-track status for each workstream."

**Compiler analysis:**

Token scan:
- "progress" → not in any predicate lexicon (Rule 12: progress → NOT in any predicate lexicon)
- "update" → not in any predicate lexicon
- "stakeholders" → DOCUMENT fires on "stakeholder" ALWAYS (Rule 6) — even in status report sentence
- "report" → DOCUMENT does NOT fire on "report" alone (Rule 3: "report" ≠ DOCUMENT)
- "on-track" → not in any predicate lexicon (gap vocabulary)
- "status" → not in any predicate lexicon (Rule 4: status → zero predicates)
- "workstream" → not in any predicate lexicon

Result: ["DOCUMENT"]

DomainId: "software-development" (DOCUMENT predicate fired — Rule 14)

CapabilityGapWarnings:
- gap-project-managing-progress-001 ("progress update" intent not captured)
- gap-project-managing-track-status-001 ("on-track status" not captured)
- gap-project-managing-work-stream-001 ("workstream" not captured)
- gap-project-managing-stakeholder-update-001 (intent was stakeholder update, not documentation)

Note: This is a false positive for DOCUMENT. The intent is stakeholder update / status reporting, but "stakeholder" fires DOCUMENT regardless of context per the lexicon rules.

---

## B37-0A-003-002 — "stakeholder" fires DOCUMENT: confirmation with report framing

**Input request:**
"Prepare the weekly status report for stakeholders covering all open blockers and delivery progress."

**Compiler analysis:**

Token scan:
- "status" → not in any predicate lexicon
- "report" → DOCUMENT does NOT fire on "report"
- "stakeholders" → DOCUMENT fires (Rule 6: stakeholder → DOCUMENT fires ALWAYS)
- "blockers" → not in any predicate lexicon (gap vocabulary)
- "delivery" → not in any predicate lexicon
- "progress" → not in any predicate lexicon

Result: ["DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-status-report-001
- gap-project-managing-blockers-001
- gap-project-managing-progress-001

---

## B37-0A-003-003 — "stakeholder" + "criteria" fires DOCUMENT twice (one predicate)

**Input request:**
"Share the acceptance criteria with stakeholders and confirm the delivery is on track."

**Compiler analysis:**

Token scan:
- "acceptance" → TEST fires on "acceptance" (Rule 2: acceptance is in TEST lexicon)
- "criteria" → DOCUMENT fires on "criteria" (Rule 8)
- "stakeholders" → DOCUMENT fires (Rule 6)
- "on track" → gap vocabulary

Result: ["TEST", "DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-track-status-001 ("on track" delivery intent not captured)

Note: "acceptance" fires TEST even though the phrase "acceptance criteria" in context refers to documentation, not testing. Lexical firing is context-independent.

---

## B37-0A-003-004 — Pure managing vocabulary: zero predicates (no stakeholder)

**Input request:**
"Provide a status update on current blockers and indicate whether the project is on track."

**Compiler analysis:**

Token scan:
- "status" → not in any predicate lexicon
- "update" → not in any predicate lexicon
- "blockers" → not in any predicate lexicon
- "on track" → not in any predicate lexicon
- "project" → not in any predicate lexicon

Result: []

DomainId: "project-management"

CapabilityGapWarnings:
- gap-project-managing-status-report-001
- gap-project-managing-blockers-001
- gap-project-managing-track-status-001

SCHEDULE_CAPABILITY_LEARNING: emitted

---

## B37-0A-003-005 — "stakeholders" + "define" fires both DOCUMENT tokens

**Input request:**
"Define the escalation thresholds and communicate them to stakeholders before go-live."

**Compiler analysis:**

Token scan:
- "Define" → DOCUMENT fires on "define" (Rule 8)
- "escalation" → not in any predicate lexicon (Rule 13)
- "stakeholders" → DOCUMENT fires (Rule 6)
- "communicate" → not in any predicate lexicon

Result: ["DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-escalation-001

Note: Both "define" and "stakeholders" fire DOCUMENT; result is still ["DOCUMENT"] (single predicate, not duplicated).
