# Layer 0-A — Compiler Pairs (028)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-028-001 — "status report" in multi-sentence request

**Input:** "Circulate the status report, track milestone progress, and escalate blockers to the programme board."

Token scan:
- "Circulate" → not in predicate lexicon → gap
- "status report" → zero predicates (Rule 3, Rule 4) → gap
- "track" → NOT TEST → gap
- "milestone progress" → gap
- "escalate" → not in predicate lexicon → gap
- "blockers" → gap
- "programme board" → zero predicates (confirmed non-DOCUMENT) → gap

Predicates: []
DomainId: "project-management"
GapIds: status-report-001, track-001, progress-001, escalation-001, blockers-001, manage-delivery-001
SCL: emitted (once)

---

## B37-0A-028-002 — "Gantt chart" false positive boundary: "create" vs "maintain"

**Input A:** "Maintain the Gantt chart and update milestone dates."

Token scan:
- "Maintain" → gap
- "Gantt chart" → gap
- "update" → not in predicate lexicon in this context → gap
- "milestone dates" → gap

Predicates: []
DomainId: "project-management"

**Input B:** "Create a Gantt chart for the delivery schedule."

Token scan:
- "Create" → IMPLEMENT fires (Rule 9) — fires even for artefact creation
- "Gantt chart" → gap
- "delivery schedule" → gap

Predicates: ["IMPLEMENT"]
DomainId: "software-development"

**Key rule:** "create" IMPLEMENT false positive applies even when creating a document artefact.

---

## B37-0A-028-003 — "capacity planning" in zero-predicate vs one-predicate context

**Input A:** "Facilitate the capacity planning workshop."

Token scan:
- "Facilitate" → not in predicate lexicon → gap
- "capacity planning" → gap
- "workshop" → not in predicate lexicon → gap

Predicates: []
DomainId: "project-management"

**Input B:** "Define the capacity planning framework and document the outcomes for stakeholders."

Token scan:
- "Define" → DOCUMENT fires
- "capacity planning" → gap
- "framework" → zero predicates (confirmed: "framework" ≠ ARCHITECTURE)
- "document" → DOCUMENT fires
- "outcomes" → gap
- "stakeholders" → DOCUMENT fires

Multiple DOCUMENT triggers → ONE DOCUMENT
Predicates: ["DOCUMENT"]
DomainId: "software-development"

---

## B37-0A-028-004 — "critical path" false positive: "define criteria" co-occurrence

**Input:** "Define the critical path criteria and acceptance conditions for the programme."

Token scan:
- "Define" → DOCUMENT fires
- "critical path" → gap
- "criteria" → DOCUMENT fires
- "acceptance conditions" → "acceptance" → TEST fires; "conditions" → gap

Multiple triggers: DOCUMENT (×2) and TEST (×1)
Predicates: ["DOCUMENT", "TEST"]
DomainId: "software-development"
GapIds forwarded: manage-delivery-001, track-001

---

## B37-0A-028-005 — "vendor management" + "SLA tracking" combined request

**Input:** "Manage vendor relationships, track SLA compliance, and escalate breaches to the steering committee."

Token scan:
- "Manage" → gap
- "vendor relationships" → gap
- "track" → NOT TEST → gap
- "SLA compliance" → gap
- "escalate" → gap
- "breaches" → gap
- "steering committee" → zero predicates (confirmed non-DOCUMENT) → gap

Predicates: []
DomainId: "project-management"
GapIds: coordinate-001, track-001, manage-delivery-001, escalation-001
SCL: emitted
