# Layer 0-A — Compiler Pairs (030)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-030-001 — "progress update" sent to stakeholders

**Input A:** "Share a progress update with the team."

Token scan:
- "Share" → not in predicate lexicon → gap
- "progress update" → zero predicates → gap
- "team" → gap

Predicates: []
DomainId: "project-management"

**Input B:** "Share a progress update with stakeholders."

Token scan:
- "Share" → gap
- "progress update" → gap
- "stakeholders" → DOCUMENT fires (Rule 6)

Predicates: ["DOCUMENT"]
DomainId: "software-development"
GapIds forwarded: progress-001
SCL: NOT emitted

---

## B37-0A-030-002 — "capacity planning" + "structure" ARCHITECTURE false positive

**Input:** "Establish the capacity planning structure for the programme governance board."

Token scan:
- "Establish" → not in predicate lexicon → gap
- "capacity planning" → gap
- "structure" → ARCHITECTURE fires (Rule 7)
- "programme governance board" → gap (no DOCUMENT trigger here)

Predicates: ["ARCHITECTURE"]
DomainId: "software-development"
GapIds forwarded: manage-delivery-001

---

## B37-0A-030-003 — "critical path" + pure PM sentence

**Input:** "Update the critical path and notify the delivery workstream leads."

Token scan:
- "Update" → not in predicate lexicon → gap
- "critical path" → gap
- "notify" → not in predicate lexicon → gap (not "stakeholder")
- "delivery workstream leads" → gap

Predicates: []
DomainId: "project-management"
GapIds: manage-delivery-001, work-stream-001, track-001
SCL: emitted

---

## B37-0A-030-004 — "vendor management" + "stakeholder" false positive

**Input:** "Manage vendor contracts and report SLA status to stakeholders."

Token scan:
- "Manage" → gap
- "vendor contracts" → "contract" alone without API → gap
- "report" → NOT DOCUMENT (Rule 3) → gap
- "SLA status" → gap
- "stakeholders" → DOCUMENT fires (Rule 6)

Predicates: ["DOCUMENT"]
DomainId: "software-development"
GapIds forwarded: coordinate-001, track-001, manage-delivery-001, status-report-001

---

## B37-0A-030-005 — Round 2 complete PM vocabulary token scan

**All Round 2 tokens — zero-predicate confirmation:**

| Input phrase | Token scan result | Predicates |
|-------------|------------------|-----------|
| "status report" | Rule 3 + Rule 4: both zero | [] |
| "progress update" | "update" not in DOCUMENT lexicon here | [] |
| "milestone tracking" | "track" ≠ TEST; "milestone" = gap | [] |
| "Gantt chart" | no predicate lexicon entry | [] |
| "resource allocation" | no predicate lexicon entry | [] |
| "capacity planning" | no predicate lexicon entry | [] |
| "dependency mapping" | no API co-occurrence | [] |
| "critical path" | no predicate lexicon entry | [] |
| "budget tracking" | "track" ≠ TEST; "budget" = gap | [] |
| "vendor management" | no predicate lexicon entry | [] |
| "SLA tracking" | "track" ≠ TEST; "SLA" = gap | [] |

Eleven Round 2 PM tokens confirmed zero-predicate. DomainId "project-management" for all pure requests.
