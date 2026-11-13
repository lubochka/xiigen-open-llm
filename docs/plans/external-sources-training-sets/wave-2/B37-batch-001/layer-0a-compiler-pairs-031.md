# Layer 0-A — Compiler Pairs (031)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-031-001 — "status report" + "define" false positive

**Input:** "Define the status report format and document the reporting cadence."

Token scan:
- "Define" → DOCUMENT fires (Rule 8)
- "status report" → zero predicates from "status report" itself
- "format" → gap
- "document" → DOCUMENT fires (Rule 8)
- "reporting cadence" → "reporting" NOT DOCUMENT; "cadence" gap

Predicates: ["DOCUMENT"]
DomainId: "software-development"
GapIds forwarded: status-report-001

---

## B37-0A-031-002 — "milestone tracking" in pure management context

**Input:** "Update milestone tracking for the delivery roadmap."

Token scan:
- "Update" → gap (not DOCUMENT trigger in this context)
- "milestone tracking" → gap
- "delivery roadmap" → gap

Predicates: []
DomainId: "project-management"
GapIds: manage-delivery-001, track-001, progress-001
SCL: emitted

---

## B37-0A-031-003 — "resource allocation" + "implement" false positive

**Input:** "Implement the resource allocation module for the project management system."

Token scan:
- "Implement" → IMPLEMENT fires (Rule 9)
- "resource allocation" → gap
- "module" → IMPLEMENT fires (Rule 9)
- "project management system" → gap (no standalone predicate from "project management")

Predicates: ["IMPLEMENT"]
DomainId: "software-development"
GapIds forwarded: manage-delivery-001

---

## B37-0A-031-004 — "dependency mapping" combined with team and API variants

**Input A:** "Map cross-workstream dependencies for the programme."

Token scan:
- "Map" → gap
- "cross-workstream dependencies" → no API co-occurrence → gap (Rule 5)
- "programme" → gap

Predicates: []
DomainId: "project-management"

**Input B:** "Map REST API dependencies between the auth module and the user service."

Token scan:
- "Map" → gap
- "REST API dependencies" → "REST" + "API" = API co-occurrence → API_CONTRACT fires (Rule 5)
- "auth module" → "module" → IMPLEMENT fires
- "user service" → "service" → IMPLEMENT fires

Predicates: ["API_CONTRACT", "IMPLEMENT"]
DomainId: "software-development"

---

## B37-0A-031-005 — "budget tracking" + "vendor management" combined pure PM

**Input:** "Track the budget, manage vendor contracts, and escalate spending variances to the programme board."

Token scan:
- "Track" → NOT TEST → gap
- "budget" → gap
- "manage" → gap
- "vendor contracts" → "contract" without API co-occurrence → gap
- "escalate" → gap
- "spending variances" → gap
- "programme board" → zero predicates → gap

Predicates: []
DomainId: "project-management"
GapIds: track-001, manage-delivery-001, escalation-001
SCL: emitted
