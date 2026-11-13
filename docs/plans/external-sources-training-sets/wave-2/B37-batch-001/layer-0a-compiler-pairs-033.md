# Layer 0-A — Compiler Pairs (033)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-033-001 — "critical path" + "stakeholder" false positive

**Input:** "Communicate the critical path to stakeholders and track delivery against milestones."

Token scan:
- "Communicate" → not in predicate lexicon → gap
- "critical path" → gap
- "stakeholders" → DOCUMENT fires (Rule 6)
- "track" → NOT TEST → gap
- "delivery against milestones" → gap

Predicates: ["DOCUMENT"]
DomainId: "software-development"
GapIds forwarded: manage-delivery-001, track-001, progress-001

---

## B37-0A-033-002 — "resource allocation" with "architecture" false positive

**Input:** "Define the resource allocation architecture and build the workforce planning service."

Token scan:
- "Define" → DOCUMENT fires
- "resource allocation" → gap
- "architecture" → ARCHITECTURE fires (Rule 7)
- "build" → IMPLEMENT fires
- "workforce planning service" → "service" → IMPLEMENT fires

Predicates: ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"]
DomainId: "software-development"
GapIds forwarded: manage-delivery-001

---

## B37-0A-033-003 — "vendor management" in pure zero-predicate context

**Input A:** "Coordinate vendor onboarding and track delivery against SLA terms."

Token scan:
- "Coordinate" → gap (Rule 11)
- "vendor onboarding" → gap
- "track" → NOT TEST → gap
- "delivery against SLA terms" → gap

Predicates: []
DomainId: "project-management"

**Input B:** "Review vendor performance against SLA terms and escalate breaches."

Token scan:
- "Review" → QA fires
- "vendor performance" → gap
- "SLA terms" → gap
- "escalate" → gap
- "breaches" → gap

Predicates: ["QA"]
DomainId: "software-development"

---

## B37-0A-033-004 — "budget tracking" + "status" confirms both are zero-predicate

**Input:** "Monitor budget tracking and provide monthly status updates to the programme board."

Token scan:
- "Monitor" → not in predicate lexicon → gap
- "budget tracking" → gap
- "provide" → gap
- "monthly status updates" → "status" zero predicates (Rule 4); "update" not DOCUMENT here → gap
- "programme board" → gap

Predicates: []
DomainId: "project-management"
GapIds: track-001, manage-delivery-001, status-report-001, progress-001
SCL: emitted

---

## B37-0A-033-005 — "dependency mapping" + "document" false positive

**Input:** "Document the dependency map and define the critical path for the release."

Token scan:
- "Document" → DOCUMENT fires
- "dependency map" → gap (no API co-occurrence)
- "define" → DOCUMENT fires
- "critical path" → gap
- "release" → not in predicate lexicon → gap

Predicates: ["DOCUMENT"]
DomainId: "software-development"
GapIds forwarded: dependencies-team-001, manage-delivery-001, track-001
