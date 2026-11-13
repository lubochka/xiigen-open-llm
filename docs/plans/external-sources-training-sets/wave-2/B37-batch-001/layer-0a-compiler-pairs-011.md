# Layer 0-A — Compiler Pairs (011)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: compiler-pairs

---

## B37-0A-011-001 — v6-01 corruption: MANAGEMENT predicate invented for "coordinate"

**Corrupted output (v6 model):**
Request: "Coordinate the workstreams and track delivery progress across all teams."
v6 result: ["MANAGEMENT"] — invented predicate not in the predicate registry

**Correct output:**
Token scan:
- "Coordinate" → not in any predicate lexicon (Rule 11: coordinate → NOT in any predicate lexicon)
- "workstreams" → gap vocabulary
- "track" → not in any predicate lexicon (Rule 2: track ≠ TEST)
- "delivery" → gap vocabulary
- "progress" → not in any predicate lexicon (Rule 12)

Correct result: []

DomainId: "project-management"
CapabilityGapWarnings: gap-project-managing-coordinate-001, gap-project-managing-track-progress-001

v6 error type: v6-01 — inventing MANAGEMENT or COORDINATION predicate for project managing vocabulary

---

## B37-0A-011-002 — v6-02 corruption: "stakeholder" DOCUMENT suppressed

**Corrupted output (v6 model):**
Request: "Send the status report to stakeholders and confirm the delivery is on track."
v6 result: [] — DOCUMENT suppressed because "the context is status reporting, not documentation"

**Correct output:**
Token scan:
- "status" → not in any predicate lexicon
- "report" → DOCUMENT does NOT fire on "report" alone
- "stakeholders" → DOCUMENT fires ALWAYS (Rule 6: stakeholder → DOCUMENT fires even in status report sentence)
- "delivery" → gap vocabulary
- "on track" → gap vocabulary

Correct result: ["DOCUMENT"]

DomainId: "software-development"
CapabilityGapWarnings: gap-project-managing-status-report-001, gap-project-managing-track-status-001

v6 error type: v6-02 — DOCUMENT suppressed for "stakeholder" because context is status reporting

---

## B37-0A-011-003 — v6-03 corruption: DomainId "project-management" when IMPLEMENT fired

**Corrupted output (v6 model):**
Request: "Manage delivery of the authentication service and build the access control module."
v6 result: ["IMPLEMENT"] with DomainId: "project-management"

**Correct output:**
Token scan:
- "Manage" → gap (Rule 10)
- "delivery" → gap vocabulary
- "authentication" → not directly triggering
- "service" → IMPLEMENT fires (Rule 9)
- "build" → IMPLEMENT fires (Rule 9)
- "module" → IMPLEMENT fires (Rule 9)

Correct result: ["IMPLEMENT"]

DomainId: "software-development" (Rule 14: DomainId = "project-management" ONLY when zero predicates fire; ANY predicate → "software-development")

v6 error type: v6-03 — DomainId set to "project-management" when IMPLEMENT fired alongside managing vocabulary

---

## B37-0A-011-004 — Three v6 corruptions in sequence: diagnostic summary

**v6 corruption taxonomy for B37:**

| v6 Code | Error Pattern | Trigger | Correct Behaviour |
|---------|--------------|---------|------------------|
| v6-01 | Invented predicate MANAGEMENT or COORDINATION | "coordinate", "manage", "track" vocabulary | Zero predicates; gap warnings only |
| v6-02 | DOCUMENT suppressed for "stakeholder" | Status report or managing context with "stakeholder" | DOCUMENT fires ALWAYS on "stakeholder" |
| v6-03 | DomainId "project-management" when predicate fired | Managing vocabulary adjacent to IMPLEMENT/DOCUMENT/etc | DomainId = "software-development" when ANY predicate fires |

All three corruptions stem from the same root: v6 incorrectly applies semantic intent to override lexical firing rules.

---

## B37-0A-011-005 — v6-01 + v6-03 compound corruption example

**Corrupted output (v6 model):**
Request: "Coordinate the escalation process and implement the tracking dashboard module."
v6 result: ["MANAGEMENT", "IMPLEMENT"] with DomainId: "project-management"

**Correct output:**
Token scan:
- "Coordinate" → not in any predicate lexicon (Rule 11)
- "escalation" → gap (Rule 13)
- "implement" → IMPLEMENT fires (Rule 9)
- "tracking" → gap vocabulary (track family)
- "dashboard" → not in any predicate lexicon
- "module" → IMPLEMENT fires (Rule 9)

Correct result: ["IMPLEMENT"]

DomainId: "software-development" (IMPLEMENT fired; ANY predicate → software-development)

CapabilityGapWarnings:
- gap-project-managing-coordinate-001
- gap-project-managing-escalation-001

v6 errors: v6-01 (invented MANAGEMENT) AND v6-03 (wrong DomainId despite IMPLEMENT firing)
