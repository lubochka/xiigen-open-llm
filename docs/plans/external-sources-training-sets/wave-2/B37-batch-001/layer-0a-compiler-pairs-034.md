# Layer 0-A — Compiler Pairs (034)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-034-001 — "Gantt chart" in pure PM contexts

**Input A:** "Update the Gantt chart to reflect new milestone dates."

Token scan:
- "Update" → gap
- "Gantt chart" → gap
- "milestone dates" → gap

Predicates: []
DomainId: "project-management"

**Input B:** "Write a script to generate the Gantt chart from the project data."

Token scan:
- "Write" → IMPLEMENT fires (Rule 9: write = implement trigger)
- "script" → not in predicate lexicon → gap
- "generate" → not in predicate lexicon → gap
- "Gantt chart" → gap
- "project data" → gap

Predicates: ["IMPLEMENT"]
DomainId: "software-development"

---

## B37-0A-034-002 — "capacity planning" + "milestone tracking" combined zero-predicate

**Input:** "Coordinate capacity planning and milestone tracking for Q2 delivery."

Token scan:
- "Coordinate" → gap (Rule 11)
- "capacity planning" → gap
- "milestone tracking" → gap
- "Q2 delivery" → gap

Predicates: []
DomainId: "project-management"
GapIds: coordinate-001, manage-delivery-001, track-001, progress-001
SCL: emitted

---

## B37-0A-034-003 — "SLA tracking" + "service" false positive

**Input A:** "Track SLA compliance across all vendor contracts."

Predicates: [] — "track" ≠ TEST, "vendor contracts" without API co-occurrence

**Input B:** "Build the SLA tracking service and define compliance thresholds."

Token scan:
- "Build" → IMPLEMENT fires
- "SLA tracking service" → "service" → IMPLEMENT fires
- "define" → DOCUMENT fires
- "compliance thresholds" → gap

Predicates: ["IMPLEMENT", "DOCUMENT"]
DomainId: "software-development"

---

## B37-0A-034-004 — "critical path" vs "critical path analysis" — both zero-predicate

**Input A:** "Identify the critical path." → Predicates: []
**Input B:** "Run critical path analysis for the delivery programme." → Predicates: []
**Input C:** "Define the critical path and document dependencies for the project charter."

Token scan:
- "Define" → DOCUMENT fires
- "critical path" → gap
- "document" → DOCUMENT fires
- "dependencies" → no API co-occurrence → gap
- "project charter" → gap

Predicates: ["DOCUMENT"]
DomainId: "software-development"

---

## B37-0A-034-005 — "resource allocation" + "report" confirms "report" ≠ DOCUMENT

**Input:** "Report on resource allocation status to the delivery board."

Token scan:
- "Report" → NOT DOCUMENT (Rule 3) → gap
- "resource allocation status" → gap
- "delivery board" → gap

Predicates: []
DomainId: "project-management"
GapIds: status-report-001, manage-delivery-001
SCL: emitted

**Teaching note:** "Report" does NOT fire DOCUMENT even when used as a verb. Rule 3 is absolute.
