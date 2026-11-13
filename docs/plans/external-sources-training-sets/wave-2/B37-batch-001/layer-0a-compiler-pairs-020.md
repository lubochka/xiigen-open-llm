# Layer 0-A — Compiler Pairs (020)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-020-001 — "structure" → ARCHITECTURE fires: governance context

**Input request:**
"Define the governance structure for the programme delivery board."

**Compiler analysis:**

Token scan:
- "Define" → DOCUMENT fires (Rule 8)
- "governance structure" → "structure" → ARCHITECTURE fires (Rule 7: structure → ARCHITECTURE always)
- "programme delivery board" → not in predicate lexicon

Result: ["DOCUMENT", "ARCHITECTURE"]

DomainId: "software-development" (multiple predicates fired)

CapabilityGapWarnings:
- gap-project-managing-manage-delivery-001

**Key boundary:** "governance structure" fires ARCHITECTURE on "structure". The intent is organisational governance, not software architecture. The lexical rule fires regardless.

---

## B37-0A-020-002 — "structure" in "org structure": ARCHITECTURE fires

**Input request:**
"Map out the org structure for the delivery team and assign workstream accountability."

**Compiler analysis:**

Token scan:
- "Map" → not in predicate lexicon
- "org structure" → "structure" → ARCHITECTURE fires (Rule 7)
- "delivery team" → gap (delivery vocabulary)
- "workstream" → gap
- "accountability" → gap

Result: ["ARCHITECTURE"]

DomainId: "software-development" (ARCHITECTURE predicate fired)

CapabilityGapWarnings:
- gap-project-managing-manage-delivery-001
- gap-project-managing-work-stream-001
- gap-project-managing-accountability-001

---

## B37-0A-020-003 — "structure" in "reporting structure": ARCHITECTURE fires

**Input request:**
"Clarify the reporting structure and escalation paths for all project workstreams."

**Compiler analysis:**

Token scan:
- "Clarify" → not in predicate lexicon
- "reporting structure" → "structure" → ARCHITECTURE fires (Rule 7)
- "escalation paths" → gap (escalation vocabulary)
- "project workstreams" → gap

Result: ["ARCHITECTURE"]

DomainId: "software-development" (ARCHITECTURE predicate fired)

CapabilityGapWarnings:
- gap-project-managing-escalation-001
- gap-project-managing-work-stream-001

---

## B37-0A-020-004 — No "structure" → zero predicates (boundary contrast)

**Input request:**
"Define the escalation paths and accountability levels for all delivery workstreams."

**Compiler analysis:**

Token scan:
- "Define" → DOCUMENT fires (Rule 8)
- "escalation paths" → gap (no predicate)
- "accountability levels" → gap (no predicate — "accountability" ≠ predicate; "structure" would be needed to fire ARCHITECTURE)
- "delivery workstreams" → gap

Result: ["DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-escalation-001
- gap-project-managing-accountability-001
- gap-project-managing-work-stream-001

**Key contrast with 020-003:** Without "structure", only DOCUMENT fires on "define". ARCHITECTURE fires only when "structure" (or "architecture"/"design") is present.

---

## B37-0A-020-005 — "structure" in "team structure": ARCHITECTURE fires (boundary case)

**Input request:**
"Establish the team structure for the agile delivery pod and confirm workstream ownership."

**Compiler analysis:**

Token scan:
- "Establish" → not in predicate lexicon
- "team structure" → "structure" → ARCHITECTURE fires (Rule 7)
- "agile delivery pod" → not in predicate lexicon
- "workstream ownership" → gap (workstream + accountability)

Result: ["ARCHITECTURE"]

DomainId: "software-development" (ARCHITECTURE predicate fired)

CapabilityGapWarnings:
- gap-project-managing-work-stream-001
- gap-project-managing-accountability-001

Note: "team structure" fires ARCHITECTURE even though the intent is team organisation, not software architecture. Every occurrence of "structure" fires ARCHITECTURE in B37.
