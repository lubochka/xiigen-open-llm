# Layer 2 — Capability-Gap-Extended Pairs (011)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: capability-gap-extended-pairs

---

## B37-L2-011-001 — "define governance structure": ARCHITECTURE+DOCUMENT fire; gap warnings for managing

**Input:** "Define the project governance structure and document the escalation criteria for critical blockers."

**Full mixed-signal trace:**

Step 1 — Compiler scan:
- "Define" → DOCUMENT fires (Rule 8)
- "project" → not in any predicate lexicon
- "governance" → not in any predicate lexicon → gap context
- "structure" → ARCHITECTURE fires (Rule 7)
- "document" → DOCUMENT fires (Rule 8)
- "escalation" → not in any predicate lexicon (Rule 13) → gap
- "criteria" → DOCUMENT fires (Rule 8)
- "critical" → not in any predicate lexicon
- "blockers" → not in any predicate lexicon → gap

Step 2 — Predicate list: ["ARCHITECTURE", "DOCUMENT"]

Step 3 — DomainId: "software-development"

Step 4 — Routes: ArchitectureFlow31 (3.1) + DocumentationFlow (2.1)

Step 5 — SCHEDULE_CAPABILITY_LEARNING: NOT emitted

Step 6 — CapabilityGapWarnings:
- gap-project-managing-escalation-001 (escalation intent unmet)
- gap-project-managing-blockers-001 (blocker management intent unmet)
- gap-project-managing-accountability-001 (governance = accountability)

**Key: ARCHITECTURE and DOCUMENT fire on "structure", "define", "document", "criteria". Governance/escalation/blockers are gap vocabulary forwarded as warnings.**

---

## B37-L2-011-002 — Governance without structure: only DOCUMENT fires

**Input:** "Document the project governance model and define the decision rights."

**Trace:**
- "Document" → DOCUMENT fires (Rule 8)
- "governance" → not in any predicate lexicon (no "structure" to trigger ARCHITECTURE)
- "define" → DOCUMENT fires (Rule 8)
- "decision rights" → not in any predicate lexicon

Result: ["DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings: gap-project-managing-accountability-001

**Key: Without "structure", ARCHITECTURE does NOT fire for governance.**

---

## B37-L2-011-003 — Governance with "architecture": ARCHITECTURE fires explicitly

**Input:** "Design the project governance architecture and define the escalation framework."

**Trace:**
- "Design" → ARCHITECTURE fires (Rule 20)
- "governance" → not in any predicate lexicon
- "architecture" → ARCHITECTURE fires (Rule 20)
- "define" → DOCUMENT fires (Rule 8)
- "escalation" → gap (Rule 13)
- "framework" → not in any predicate lexicon

Result: ["ARCHITECTURE", "DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings: gap-project-managing-escalation-001, gap-project-managing-accountability-001

---

## B37-L2-011-004 — Governance with all three: ARCHITECTURE + DOCUMENT + QA

**Input:** "Review the governance structure and document the accountability criteria."

**Trace:**
- "Review" → QA fires (Rule 19)
- "governance" → not in any predicate lexicon
- "structure" → ARCHITECTURE fires (Rule 7)
- "document" → DOCUMENT fires (Rule 8)
- "accountability" → gap
- "criteria" → DOCUMENT fires (Rule 8)

Result: ["QA", "ARCHITECTURE", "DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings: gap-project-managing-accountability-001

---

## B37-L2-011-005 — "define governance structure" pattern recognition

**Pattern:** "Define the [managing concept] structure" → ARCHITECTURE + DOCUMENT

This is one of the most common B37 compound patterns. It appears in:
- "Define the accountability structure"
- "Define the governance structure"
- "Define the escalation structure"
- "Define the RACI structure"
- "Define the reporting structure"
- "Define the org structure"

In all cases: ARCHITECTURE (structure) + DOCUMENT (define). Managing vocabulary → gap warnings. Routes: 3.1 + 2.1.

**Key training signal:** The phrase "define the [X] structure" always fires ARCHITECTURE + DOCUMENT regardless of what [X] is, because both "define" → DOCUMENT and "structure" → ARCHITECTURE fire unconditionally.
