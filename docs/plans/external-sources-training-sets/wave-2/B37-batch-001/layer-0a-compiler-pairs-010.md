# Layer 0-A — Compiler Pairs (010)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: compiler-pairs

---

## B37-0A-010-001 — DOCUMENT+ARCHITECTURE mixed with escalation vocabulary

**Input request:**
"Define the project governance structure and document the escalation criteria for critical blockers."

**Compiler analysis:**

Token scan:
- "Define" → DOCUMENT fires (Rule 8)
- "project" → not in any predicate lexicon
- "governance" → not in any predicate lexicon
- "structure" → ARCHITECTURE fires (Rule 7)
- "document" → DOCUMENT fires (Rule 8)
- "escalation" → not in any predicate lexicon (Rule 13)
- "criteria" → DOCUMENT fires on "criteria" (Rule 8)
- "blockers" → gap vocabulary

Result: ["ARCHITECTURE", "DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-escalation-001
- gap-project-managing-blockers-001
- gap-project-managing-accountability-001 (governance = accountability framework)

Note: "define", "document", and "criteria" all fire DOCUMENT — result is still ["DOCUMENT"] (single predicate). "structure" fires ARCHITECTURE. Managing vocabulary (escalation, blockers, governance) generates gap warnings.

---

## B37-0A-010-002 — "governance" without "structure": only DOCUMENT fires

**Input request:**
"Document the project governance process and define the roles for each workstream lead."

**Compiler analysis:**

Token scan:
- "Document" → DOCUMENT fires (Rule 8)
- "governance" → not in any predicate lexicon
- "define" → DOCUMENT fires (Rule 8)
- "roles" → not in any predicate lexicon
- "workstream" → gap vocabulary

Result: ["DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-work-stream-001
- gap-project-managing-accountability-001 (roles/responsibilities)

---

## B37-0A-010-003 — "criteria" fires DOCUMENT: escalation criteria example

**Input request:**
"Specify the escalation criteria and describe when issues should be raised to the steering board."

**Compiler analysis:**

Token scan:
- "Specify" → not directly in DOCUMENT trigger list, but "criteria" is
- "escalation" → gap (Rule 13)
- "criteria" → DOCUMENT fires (Rule 8)
- "describe" → DOCUMENT fires on "describe" (Rule 8)
- "issues" → not in any predicate lexicon (gap: RAID-I)
- "raised" → not in any predicate lexicon
- "steering" → not in any predicate lexicon

Result: ["DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-escalation-001
- gap-project-managing-raid-log-001 (issues = RAID-I)

---

## B37-0A-010-004 — "requirements" fires DOCUMENT: in project managing framing

**Input request:**
"Document the requirements for the escalation process and define the approval thresholds."

**Compiler analysis:**

Token scan:
- "Document" → DOCUMENT fires (Rule 8)
- "requirements" → DOCUMENT fires (Rule 8)
- "escalation" → gap (Rule 13)
- "define" → DOCUMENT fires (Rule 8)
- "approval" → not in any predicate lexicon
- "thresholds" → not in any predicate lexicon

Result: ["DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-escalation-001

---

## B37-0A-010-005 — Zero predicates: governance without triggering tokens

**Input request:**
"Establish and maintain project governance to ensure all workstreams remain on track."

**Compiler analysis:**

Token scan:
- "Establish" → not in any predicate lexicon
- "project governance" → not in any predicate lexicon
- "workstreams" → gap vocabulary
- "on track" → gap vocabulary ("on-track" = gap)

Result: []

DomainId: "project-management"

CapabilityGapWarnings:
- gap-project-managing-work-stream-001
- gap-project-managing-track-status-001

SCHEDULE_CAPABILITY_LEARNING: emitted
