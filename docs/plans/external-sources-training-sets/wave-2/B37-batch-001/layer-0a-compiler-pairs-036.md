# Layer 0-A — Compiler Pairs (036)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-036-001 — "status report" vs "weekly report" vs "progress report"

**All three variants confirm Rule 3:**

Input A: "Send the weekly status report." → Predicates: [] — "status report" zero predicates
Input B: "Send the weekly progress report." → Predicates: [] — "progress report" zero predicates
Input C: "Send the weekly report." → Predicates: [] — "report" alone = NOT DOCUMENT (Rule 3)
Input D: "Document the weekly status report format for stakeholders." → Predicates: ["DOCUMENT"] — "document" + "stakeholders" fire

Rule 3 is absolute: "report" alone or in "status report" / "progress report" / "weekly report" → zero predicates.

---

## B37-0A-036-002 — "SLA" with and without "tracking" — both zero-predicate

**Input A:** "Monitor SLA compliance for the vendor portfolio."
- "Monitor" → gap; "SLA compliance" → gap; "vendor portfolio" → gap
- Predicates: []

**Input B:** "Track SLA breaches across all contracts."
- "Track" → NOT TEST; "SLA breaches" → gap; "contracts" alone → gap
- Predicates: []

**Input C:** "Implement SLA monitoring for the vendor API contract."
- "Implement" → IMPLEMENT fires; "SLA monitoring" → gap; "vendor API contract" → "API" co-occurrence → API_CONTRACT fires
- Predicates: ["IMPLEMENT", "API_CONTRACT"]

---

## B37-0A-036-003 — "Gantt chart" and "critical path" combined zero-predicate

**Input:** "Maintain the Gantt chart, update the critical path, and track milestones."

Token scan:
- "Maintain" → gap
- "Gantt chart" → gap
- "update" → gap
- "critical path" → gap
- "track" → NOT TEST → gap
- "milestones" → gap

Predicates: []
DomainId: "project-management"
GapIds: manage-delivery-001, track-001, progress-001
SCL: emitted

---

## B37-0A-036-004 — "capacity planning" + "resource allocation" combined zero-predicate

**Input:** "Run capacity planning and manage resource allocation for the next programme increment."

Token scan:
- "Run" → gap
- "capacity planning" → gap
- "manage" → gap
- "resource allocation" → gap
- "programme increment" → gap (no predicate trigger on "increment")

Predicates: []
DomainId: "project-management"
GapIds: manage-delivery-001, coordinate-001
SCL: emitted

---

## B37-0A-036-005 — Complete Round 2 vocabulary DomainId reference (025-036)

**DomainId "project-management" confirmed for ALL Round 2 zero-predicate requests:**

1. status report management → project-management
2. progress update reporting → project-management
3. milestone tracking → project-management
4. Gantt chart maintenance → project-management
5. resource allocation coordination → project-management
6. capacity planning → project-management
7. dependency mapping (no API) → project-management
8. critical path analysis → project-management
9. budget tracking → project-management
10. vendor management → project-management
11. SLA tracking → project-management
12. Combined PM sentences with only the above vocabulary → project-management

**DomainId "software-development" when ANY of these co-occur:**
review, validate, stakeholder, define, document, criteria, structure, design, architecture, build, create, implement, write, service, module, API + dependencies
