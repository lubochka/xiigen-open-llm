# Layer 0-A — Compiler Pairs (035)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-035-001 — "progress update" + "define" false positive

**Input:** "Define the progress update format for the weekly status reporting cycle."

Token scan:
- "Define" → DOCUMENT fires (Rule 8)
- "progress update" → gap
- "format" → gap
- "weekly status reporting cycle" → "reporting" NOT DOCUMENT; "status" zero predicates → all gap

Predicates: ["DOCUMENT"]
DomainId: "software-development"
GapIds forwarded: progress-001, status-report-001

---

## B37-0A-035-002 — "vendor management" pure PM multi-term

**Input:** "Coordinate vendor management, track delivery SLAs, manage budget utilisation, and escalate issues to the programme board."

Token scan:
- "Coordinate" → gap
- "vendor management" → gap
- "track" → NOT TEST → gap
- "delivery SLAs" → gap
- "manage" → gap
- "budget utilisation" → gap
- "escalate" → gap
- "issues" → gap
- "programme board" → zero predicates → gap

Predicates: []
DomainId: "project-management"
GapIds: coordinate-001, manage-delivery-001, track-001, escalation-001
SCL: emitted

---

## B37-0A-035-003 — "milestone tracking" + "structure" false positive

**Input:** "Establish a milestone tracking structure for the delivery governance board."

Token scan:
- "Establish" → gap
- "milestone tracking" → gap
- "structure" → ARCHITECTURE fires (Rule 7)
- "delivery governance board" → gap (no DOCUMENT trigger on "board")

Predicates: ["ARCHITECTURE"]
DomainId: "software-development"
GapIds forwarded: track-001, manage-delivery-001

---

## B37-0A-035-004 — "budget tracking" with "build" false positive

**Input:** "Build a budget tracking tool for the programme management office."

Token scan:
- "Build" → IMPLEMENT fires (Rule 9)
- "budget tracking tool" → "tool" not in predicate lexicon; "budget tracking" → gap
- "programme management office" → gap

Predicates: ["IMPLEMENT"]
DomainId: "software-development"
GapIds forwarded: track-001, manage-delivery-001

---

## B37-0A-035-005 — Round 2 false positive reference table

**All false positive combinations confirmed for Round 2 vocabulary:**

| PM Token | False Positive Trigger | Predicate Fires |
|----------|----------------------|-----------------|
| status report + document | document | DOCUMENT |
| progress update + stakeholders | stakeholders | DOCUMENT |
| milestone tracking + review | review | QA |
| Gantt chart + create | create | IMPLEMENT |
| resource allocation + design | design | ARCHITECTURE |
| capacity planning + structure | structure | ARCHITECTURE |
| dependency mapping + API | API co-occurrence | API_CONTRACT |
| critical path + define/criteria | define, criteria | DOCUMENT |
| budget tracking + define | define | DOCUMENT |
| vendor management + stakeholders | stakeholders | DOCUMENT |
| SLA tracking + implement/service | implement, service | IMPLEMENT |

All false positives follow existing lexical rules. No Round 2 vocabulary introduces new predicate lexicon entries.
