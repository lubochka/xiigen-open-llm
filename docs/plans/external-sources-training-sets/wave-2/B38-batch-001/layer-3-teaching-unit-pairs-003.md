# Layer 3 — Teaching Unit Pairs (003)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B38-TU-003-001 — Teaching: "criteria" fires DOCUMENT always

LESSON: The word "criteria" fires the DOCUMENT predicate regardless of what qualifier precedes it.

COMPLETE LIST (all fire DOCUMENT):
- release criteria → DOCUMENT
- acceptance criteria → DOCUMENT
- exit criteria → DOCUMENT
- entry criteria → DOCUMENT
- completion criteria → DOCUMENT
- readiness criteria → DOCUMENT
- performance criteria → DOCUMENT
- quality criteria → DOCUMENT
- success criteria → DOCUMENT
- deployment readiness criteria → DOCUMENT (even though "deployment" is gap, "criteria" still fires)

MEMORABLE RULE:
Criteria → DOCUMENT. The qualifier never changes this.

---

## B38-TU-003-002 — Teaching: "criteria" does NOT fire QA

TRAP: Some learners expect "quality criteria" or "acceptance criteria" to fire QA because of the QA-adjacent qualifiers.

"quality criteria" → DOCUMENT (from "criteria") — NOT QA from "quality"
Wait — actually "quality" is also in the QA lexicon.
So "quality criteria" → DOCUMENT (from criteria) AND QA (from quality)? Yes:

"Validate the quality criteria" → QA (validate + quality) + DOCUMENT (criteria) = ["QA", "DOCUMENT"]
"Review the acceptance criteria" → QA (review) + DOCUMENT (criteria) = ["QA", "DOCUMENT"]
"Document the exit criteria" → DOCUMENT (document + criteria) = ["DOCUMENT"]

The criteria qualifier ("quality", "acceptance", "exit") contributes to its own predicate only if it is itself a predicate word. "quality" IS a QA word; "exit" is not. "acceptance" in quality context IS a QA word.

BUT: "criteria" always fires DOCUMENT regardless. No qualifier suppresses criteria → DOCUMENT.

---

## B38-TU-003-003 — Teaching: three criteria examples with full predicate analysis

Example 1: "Define the release criteria."
- "Define" → DOCUMENT (documentation action)
- "release criteria" → DOCUMENT (criteria fires)
- Result: ["DOCUMENT"]

Example 2: "Review the acceptance criteria and sign off."
- "Review" → QA
- "acceptance criteria" → DOCUMENT (criteria fires)
- "sign off" → QA
- Result: ["QA", "DOCUMENT"]

Example 3: "The service must meet the security criteria before approval."
- "service" → IMPLEMENT
- "security criteria" → DOCUMENT (criteria fires)
- "security" → SECURITY
- "approval" → QA
- Result: ["QA", "IMPLEMENT", "DOCUMENT", "SECURITY"]
(criteria qualifier "security" happens to also be a SECURITY word — double-predicate effect)
