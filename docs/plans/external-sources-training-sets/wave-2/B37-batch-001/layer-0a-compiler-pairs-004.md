# Layer 0-A — Compiler Pairs (004)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: compiler-pairs

---

## B37-0A-004-001 — "structure" fires ARCHITECTURE: governance context

**Input request:**
"Define the accountability structure for the project delivery team."

**Compiler analysis:**

Token scan:
- "Define" → DOCUMENT fires on "define" (Rule 8)
- "accountability" → not in any predicate lexicon (gap vocabulary)
- "structure" → ARCHITECTURE fires on "structure" ALWAYS (Rule 7: even in "accountability structure", "org structure", "governance structure")
- "project" → not in any predicate lexicon
- "delivery" → not in any predicate lexicon

Result: ["ARCHITECTURE", "DOCUMENT"]

DomainId: "software-development" (predicates fired)

CapabilityGapWarnings:
- gap-project-managing-accountability-001 ("accountability" intent not captured)
- gap-project-managing-manage-delivery-001 ("project delivery" not captured)

Note: "structure" fires ARCHITECTURE regardless of whether the sentence is about software architecture or organizational governance. This is a false positive for ARCHITECTURE in project managing context.

---

## B37-0A-004-002 — "structure" fires ARCHITECTURE: org chart context

**Input request:**
"Document the governance structure and accountability framework for the programme."

**Compiler analysis:**

Token scan:
- "Document" → DOCUMENT fires on "document" (Rule 8)
- "governance" → not in any predicate lexicon
- "structure" → ARCHITECTURE fires (Rule 7)
- "accountability" → gap vocabulary
- "framework" → not in any predicate lexicon

Result: ["ARCHITECTURE", "DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-accountability-001

---

## B37-0A-004-003 — "structure" fires ARCHITECTURE: escalation path context

**Input request:**
"Outline the escalation structure and ensure every team member knows their reporting line."

**Compiler analysis:**

Token scan:
- "escalation" → not in any predicate lexicon (Rule 13)
- "structure" → ARCHITECTURE fires (Rule 7)
- "ensure" → not in any predicate lexicon
- "reporting" → not in any predicate lexicon

Result: ["ARCHITECTURE"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-escalation-001
- gap-project-managing-accountability-001 (reporting line = accountability)

---

## B37-0A-004-004 — "define" + "structure" both fire: combined ARCHITECTURE+DOCUMENT

**Input request:**
"Define the RACI structure and document who is accountable for each project deliverable."

**Compiler analysis:**

Token scan:
- "Define" → DOCUMENT fires (Rule 8)
- "RACI" → not in any predicate lexicon
- "structure" → ARCHITECTURE fires (Rule 7)
- "document" → DOCUMENT fires (Rule 8)
- "accountable" → gap vocabulary

Result: ["ARCHITECTURE", "DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-accountability-001

Note: Both "Define" and "document" trigger DOCUMENT — result remains ["DOCUMENT"] as a single predicate. ARCHITECTURE fires on "structure".

---

## B37-0A-004-005 — No "structure": pure accountability vocabulary → zero predicates

**Input request:**
"Establish clear accountability and ownership for each workstream in the delivery programme."

**Compiler analysis:**

Token scan:
- "accountability" → gap vocabulary (not in any predicate lexicon)
- "ownership" → not in any predicate lexicon
- "workstream" → gap vocabulary
- "delivery" → gap vocabulary
- "programme" → not in any predicate lexicon

Result: []

DomainId: "project-management"

CapabilityGapWarnings:
- gap-project-managing-accountability-001
- gap-project-managing-work-stream-001
- gap-project-managing-manage-delivery-001

SCHEDULE_CAPABILITY_LEARNING: emitted
