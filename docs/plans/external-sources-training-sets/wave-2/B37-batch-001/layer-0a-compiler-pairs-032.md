# Layer 0-A — Compiler Pairs (032)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-032-001 — "SLA tracking" + "define" + "criteria" false positives

**Input:** "Define SLA tracking criteria and document vendor performance thresholds."

Token scan:
- "Define" → DOCUMENT fires (Rule 8)
- "SLA tracking" → gap
- "criteria" → DOCUMENT fires (Rule 8)
- "document" → DOCUMENT fires (Rule 8)
- "vendor performance thresholds" → gap

Multiple DOCUMENT triggers → ONE DOCUMENT predicate
Predicates: ["DOCUMENT"]
DomainId: "software-development"
GapIds forwarded: track-001, manage-delivery-001

---

## B37-0A-032-002 — "Gantt chart" + "review" false positive

**Input:** "Review the Gantt chart and validate milestone dates against the critical path."

Token scan:
- "Review" → QA fires
- "Gantt chart" → gap
- "validate" → QA fires (validate = QA trigger)
- "milestone dates" → gap
- "critical path" → gap

Multiple QA triggers → ONE QA predicate
Predicates: ["QA"]
DomainId: "software-development"
GapIds forwarded: manage-delivery-001, track-001

---

## B37-0A-032-003 — "capacity planning" + "define" false positive multi-predicate

**Input:** "Define the capacity planning approach, design the team structure, and document the outcomes."

Token scan:
- "Define" → DOCUMENT fires
- "capacity planning" → gap
- "design" → ARCHITECTURE fires (Rule 7)
- "team structure" → "structure" → ARCHITECTURE fires (Rule 7)
- "document" → DOCUMENT fires
- "outcomes" → gap

Predicates: ["DOCUMENT", "ARCHITECTURE"]
DomainId: "software-development"
GapIds forwarded: manage-delivery-001

---

## B37-0A-032-004 — "progress update" + "build" false positive

**Input:** "Build a progress update dashboard for the executive reporting platform."

Token scan:
- "Build" → IMPLEMENT fires (Rule 9)
- "progress update" → gap (managing vocabulary)
- "dashboard" → gap (no standalone UI predicate)
- "executive reporting" → "reporting" NOT DOCUMENT → gap
- "platform" → gap

Predicates: ["IMPLEMENT"]
DomainId: "software-development"
GapIds forwarded: progress-001, status-report-001

---

## B37-0A-032-005 — Round 2 mixed vocabulary certification scan

**Input:** "Track milestones, allocate resources, map team dependencies, and report capacity status to the programme board."

Token scan:
- "Track" → NOT TEST → gap
- "milestones" → gap
- "allocate resources" → gap
- "map" → gap
- "team dependencies" → no API co-occurrence → gap
- "report" → NOT DOCUMENT (Rule 3) → gap
- "capacity status" → gap
- "programme board" → zero predicates → gap

Predicates: []
DomainId: "project-management"
GapIds: track-001, manage-delivery-001, dependencies-team-001, status-report-001, work-stream-001
SCL: emitted

This confirms all eleven Round 2 PM terms retain zero-predicate status in combined sentences.
