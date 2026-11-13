# Layer 0-C — Knowledge-Edge Pairs (019)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-019-001 — "criteria" is the strongest DOCUMENT trigger in B38

"Criteria" fires DOCUMENT with every qualifier:
- "release criteria" → DOCUMENT
- "acceptance criteria" → DOCUMENT
- "exit criteria" → DOCUMENT
- "readiness criteria" → DOCUMENT
- "entry criteria" → DOCUMENT
- "completion criteria" → DOCUMENT
- "success criteria" → DOCUMENT
- "performance criteria" → DOCUMENT
- "quality criteria" → DOCUMENT
- "security criteria" → DOCUMENT
- "promotion criteria" → DOCUMENT
- "version tagging criteria" → DOCUMENT

Rule 3 is absolute: any qualifier before "criteria" still fires DOCUMENT. There is no qualifier that suppresses this rule.

---

## B38-0C-019-002 — "criteria document" vs "criteria checklist"

"criteria document" → "criteria" fires DOCUMENT + "document" fires DOCUMENT (two DOCUMENT triggers, one predicate)
"criteria checklist" → "criteria" fires DOCUMENT + "checklist" is GAP (gap warning for checklist)

Result for "criteria document": ["DOCUMENT"] or more with QA terms
Result for "criteria checklist": ["DOCUMENT"] with CapabilityGapWarning: ["checklist"]

Lesson: "criteria" always fires DOCUMENT. The word that follows it does not suppress DOCUMENT. If it's another DOCUMENT trigger (like "document"), they consolidate. If it's gap vocabulary (like "checklist"), a gap warning is added but DOCUMENT still fires.

---

## B38-0C-019-003 — "readiness" alone vs "readiness criteria"

"readiness" alone → NOT necessarily a predicate trigger in most contexts
"readiness criteria" → DOCUMENT fires from "criteria" (rule 3)
"readiness review" → QA fires from "review"
"readiness gate" → QA may fire if "gate" is used as a verb ("gate the readiness"); context-dependent

Lesson: "readiness" as a standalone adjective or noun does not reliably fire predicates. The predicate trigger comes from the noun or verb it modifies: "criteria" (DOCUMENT) or "review/verify/approve" (QA).
