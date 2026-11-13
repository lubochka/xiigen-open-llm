# Layer 0-A — Compiler Pairs (025)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-025-001 — "status report" vs "document the status report"

**Input A:** "Send the weekly status report to the team."

Token scan:
- "Send" → not in any predicate lexicon → gap
- "weekly" → not in any predicate lexicon → gap
- "status report" → zero predicates (Rule 4, Rule 3) → gap

Predicates: []
DomainId: "project-management"

**Input B:** "Document the status report for stakeholders."

Token scan:
- "Document" → DOCUMENT fires (Rule 8)
- "status report" → zero predicates from "status report"
- "stakeholders" → DOCUMENT fires (Rule 6)

Multiple DOCUMENT triggers → ONE DOCUMENT predicate
Predicates: ["DOCUMENT"]
DomainId: "software-development"

**Contrast:** "status report" alone → gap. "document" or "stakeholders" added → DOCUMENT fires.

---

## B37-0A-025-002 — "progress update" is zero-predicate vocabulary

**Input:** "Provide a progress update on the delivery workstream."

Token scan:
- "Provide" → not in predicate lexicon → gap
- "progress update" → zero predicates (Rule 3: "update" ≠ DOCUMENT in this context) → gap
- "delivery" → standalone delivery vocabulary → gap
- "workstream" → zero predicates → gap

Predicates: []
DomainId: "project-management"
GapIds: gap-project-managing-progress-001, gap-project-managing-manage-delivery-001, gap-project-managing-work-stream-001
SCL: emitted

---

## B37-0A-025-003 — "milestone tracking" vocabulary scan

**Input A:** "Track all project milestones for Q3."

Token scan:
- "Track" → NOT in TEST lexicon (Rule 2) → gap
- "project milestones" → zero predicates → gap
- "Q3" → not in predicate lexicon

Predicates: []
DomainId: "project-management"

**Input B:** "Define milestone criteria and document progress."

Token scan:
- "Define" → DOCUMENT fires (Rule 8)
- "milestone criteria" → "criteria" → DOCUMENT fires (Rule 8)
- "document" → DOCUMENT fires (Rule 8)
- "progress" → zero predicates → gap

Multiple DOCUMENT triggers → ONE DOCUMENT predicate
Predicates: ["DOCUMENT"]
DomainId: "software-development"

---

## B37-0A-025-004 — "gantt chart" vocabulary: gap vs DOCUMENT when creating

**Input A:** "Review the Gantt chart to confirm milestone sequencing."

Token scan:
- "Review" → QA fires (Rule: review → QA)
- "Gantt chart" → zero predicates → gap
- "milestone" → gap

Predicates: ["QA"]
DomainId: "software-development"

**Input B:** "Maintain the Gantt chart for the programme."

Token scan:
- "Maintain" → not in predicate lexicon → gap
- "Gantt chart" → zero predicates → gap
- "programme" → zero predicates → gap

Predicates: []
DomainId: "project-management"

**Input C:** "Create a Gantt chart document for the steering committee."

Token scan:
- "Create" → IMPLEMENT fires (Rule 9)
- "Gantt chart" → gap
- "document" → DOCUMENT fires
- "steering committee" → zero predicates → gap

Predicates: ["IMPLEMENT", "DOCUMENT"]
DomainId: "software-development"

---

## B37-0A-025-005 — "resource allocation" is gap vocabulary

**Input:** "Review resource allocation across workstreams and flag blockers."

Token scan:
- "Review" → QA fires
- "resource allocation" → zero predicates → gap
- "workstreams" → zero predicates → gap
- "flag" → not in predicate lexicon → gap
- "blockers" → zero predicates → gap

Predicates: ["QA"]
DomainId: "software-development"
GapIds forwarded: gap-project-managing-work-stream-001, gap-project-managing-blockers-001

**Contrast — pure allocation request:**
"Manage resource allocation across the programme." → Predicates: [] → DomainId: "project-management"
