# Layer 0-A — Compiler Pairs (018)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-018-001 — "accountability" vocabulary: zero predicates fire

**Input request:**
"Establish clear accountability across all workstream leads for delivery milestones."

**Compiler analysis:**

Token scan:
- "Establish" → not in any predicate lexicon
- "accountability" → not in any predicate lexicon (gap vocabulary)
- "workstream leads" → gap (workstream vocabulary)
- "delivery milestones" → gap (delivery vocabulary)

Result: []

DomainId: "project-management" (zero predicates fired — Rule 14)

CapabilityGapWarnings:
- gap-project-managing-accountability-001
- gap-project-managing-work-stream-001
- gap-project-managing-manage-delivery-001

SCHEDULE_CAPABILITY_LEARNING: emitted (zero-predicate gap request — Rule 17)

---

## B37-0A-018-002 — "accountability" with "structure": ARCHITECTURE fires (boundary case)

**Input request:**
"Define the accountability structure for the project governance model."

**Compiler analysis:**

Token scan:
- "Define" → DOCUMENT fires (Rule 8)
- "accountability" → gap
- "structure" → ARCHITECTURE fires (Rule 7: structure → ARCHITECTURE always — even in "accountability structure")
- "project governance" → not in predicate lexicon
- "model" → not in predicate lexicon

Result: ["DOCUMENT", "ARCHITECTURE"]

DomainId: "software-development" (multiple predicates fired)

CapabilityGapWarnings:
- gap-project-managing-accountability-001

**Key boundary:** "accountability structure" → ARCHITECTURE fires on "structure". This is the critical B37 false positive. The intent is organisational governance, not software architecture. The lexicon fires regardless of intent.

---

## B37-0A-018-003 — "accountability" alone with escalation: zero predicates

**Input request:**
"Coordinate accountability and escalation protocols across all programme workstreams."

**Compiler analysis:**

Token scan:
- "Coordinate" → not in any predicate lexicon (Rule 11)
- "accountability" → gap
- "escalation" → gap (Rule 13)
- "protocols" → not in predicate lexicon
- "programme workstreams" → gap

Result: []

DomainId: "project-management"

CapabilityGapWarnings:
- gap-project-managing-coordinate-001
- gap-project-managing-accountability-001
- gap-project-managing-escalation-001
- gap-project-managing-escalation-accountability-001 (compound: escalation + accountability together)
- gap-project-managing-work-stream-001

SCHEDULE_CAPABILITY_LEARNING: emitted

---

## B37-0A-018-004 — "accountability" with "document": DOCUMENT fires

**Input request:**
"Document the accountability framework and assign ownership to each delivery outcome."

**Compiler analysis:**

Token scan:
- "Document" → DOCUMENT fires (Rule 8)
- "accountability framework" → gap
- "assign" → not in predicate lexicon
- "ownership" → gap (accountability vocabulary: ownership/RACI)
- "delivery outcome" → gap

Result: ["DOCUMENT"]

DomainId: "software-development" (DOCUMENT predicate fired)

CapabilityGapWarnings:
- gap-project-managing-accountability-001

---

## B37-0A-018-005 — "accountability" RACI matrix: zero predicates

**Input request:**
"Build the RACI (Responsible, Accountable, Consulted, Informed) matrix to clarify accountability for all project activities."

**Compiler analysis:**

Token scan:
- "Build" → IMPLEMENT fires (Rule 9: build → IMPLEMENT)
- "RACI" → not in predicate lexicon (RACI is accountability vocabulary but not a predicate)
- "accountability" → gap
- "project activities" → not in predicate lexicon

Result: ["IMPLEMENT"]

DomainId: "software-development" (IMPLEMENT predicate fired)

CapabilityGapWarnings:
- gap-project-managing-accountability-001

Note: "Build the RACI matrix" fires IMPLEMENT on "build" even though a RACI matrix is a project management artefact, not a software component. The compiler fires on the lexical token.
