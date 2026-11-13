# Layer 0-A — Compiler Pairs (023)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-023-001 — Mixed: managing vocab + ARCHITECTURE (accountability structure)

**Input request:**
"Define the accountability structure for project governance and coordinate workstream ownership."

**Compiler analysis:**

Token scan:
- "Define" → DOCUMENT fires (Rule 8)
- "accountability structure" → "structure" → ARCHITECTURE fires (Rule 7: structure always fires ARCHITECTURE)
- "accountability" → gap
- "project governance" → not in predicate lexicon
- "coordinate" → not in predicate lexicon (Rule 11)
- "workstream ownership" → gap (workstream + accountability)

Result: ["DOCUMENT", "ARCHITECTURE"]

DomainId: "software-development" (multiple predicates fired)

CapabilityGapWarnings:
- gap-project-managing-accountability-001
- gap-project-managing-coordinate-001
- gap-project-managing-work-stream-001

---

## B37-0A-023-002 — Managing vocabulary only, no ARCHITECTURE: zero predicates

**Input request:**
"Coordinate workstream ownership and clarify accountability for delivery outcomes."

**Compiler analysis:**

Token scan:
- "Coordinate" → not in predicate lexicon (Rule 11)
- "workstream ownership" → gap
- "clarify" → not in predicate lexicon
- "accountability" → gap
- "delivery outcomes" → gap

Result: []

DomainId: "project-management"

CapabilityGapWarnings:
- gap-project-managing-coordinate-001
- gap-project-managing-work-stream-001
- gap-project-managing-accountability-001
- gap-project-managing-manage-delivery-001

SCHEDULE_CAPABILITY_LEARNING: emitted

**Contrast with 023-001:** No "structure", no "define" → zero predicates. Adding "accountability structure" and "define" triggers two predicates.

---

## B37-0A-023-003 — ARCHITECTURE only (structure, no define)

**Input request:**
"Map the accountability structure and escalation hierarchy for the programme."

**Compiler analysis:**

Token scan:
- "Map" → not in predicate lexicon
- "accountability structure" → "structure" → ARCHITECTURE fires (Rule 7)
- "accountability" → gap
- "escalation hierarchy" → gap (escalation vocabulary)
- "programme" → not in predicate lexicon

Result: ["ARCHITECTURE"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-accountability-001
- gap-project-managing-escalation-001

---

## B37-0A-023-004 — Three predicates: DOCUMENT + ARCHITECTURE + IMPLEMENT

**Input request:**
"Define the accountability structure, build the escalation tracking service, and document the process."

**Compiler analysis:**

Token scan:
- "Define" → DOCUMENT fires (Rule 8)
- "accountability structure" → "structure" → ARCHITECTURE fires
- "build" → IMPLEMENT fires (Rule 9)
- "escalation tracking service" → "service" → IMPLEMENT fires; "escalation"/"tracking" → gap
- "document" → DOCUMENT fires
- "process" → not in predicate lexicon

Result: ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-accountability-001
- gap-project-managing-escalation-001
- gap-project-managing-track-001

---

## B37-0A-023-005 — "design" in place of "structure": ARCHITECTURE fires

**Input request:**
"Design the accountability framework and coordinate delivery ownership across teams."

**Compiler analysis:**

Token scan:
- "Design" → ARCHITECTURE fires (Rule 7: design → ARCHITECTURE, same as structure and architecture)
- "accountability framework" → gap (no predicate on "accountability" or "framework")
- "coordinate" → not in predicate lexicon
- "delivery ownership" → gap
- "teams" → not in predicate lexicon

Result: ["ARCHITECTURE"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-accountability-001
- gap-project-managing-coordinate-001
- gap-project-managing-manage-delivery-001

Note: "design" fires ARCHITECTURE the same way "structure" does. "Design the accountability framework" → ARCHITECTURE, despite the intent being governance design, not software design.
