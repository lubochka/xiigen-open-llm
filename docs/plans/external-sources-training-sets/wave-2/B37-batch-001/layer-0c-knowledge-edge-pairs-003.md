# Layer 0-C — Knowledge-Edge Pairs (003)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: knowledge-edge-pairs

---

## B37-0C-003-001 — "report" ≠ DOCUMENT: what fires DOCUMENT vs what does not

**Knowledge edge: The "report" boundary**

"report" alone does NOT fire DOCUMENT. "status report" does NOT fire DOCUMENT.

**DOCUMENT fires on (complete trigger list):**
document, spec, define, describe, requirement, story, criteria, stakeholder, functional, behavior, use-case

"report" is NOT on this list.

**Examples where DOCUMENT does NOT fire:**
- "send a status report" → DOCUMENT does NOT fire
- "generate the weekly report" → DOCUMENT does NOT fire
- "report the delivery progress" → DOCUMENT does NOT fire
- "report to the steering committee" → DOCUMENT does NOT fire
- "incident report" → DOCUMENT does NOT fire (unless "stakeholder" or other DOCUMENT token present)

**Examples where DOCUMENT fires despite managing framing:**
- "status report to stakeholders" → DOCUMENT fires (on "stakeholders")
- "report the requirements" → DOCUMENT fires (on "requirements")
- "describe the status" → DOCUMENT fires (on "describe")
- "define the reporting criteria" → DOCUMENT fires (on "define" and "criteria")

---

## B37-0C-003-002 — "status" vocabulary: no predicate fires

**Knowledge edge: "status" tokens**

"status", "status report", "status update", "on-track status", "off-track status", "RAG-status", "delivery status" — NONE of these fire any predicate.

**Status-related gap IDs:**
- gap-project-managing-status-report-001 (status report intent)
- gap-project-managing-track-status-001 (on-track/off-track tracking)
- gap-project-managing-rag-status-001 (RAG-status reporting)

**Five zero-predicate status examples:**
1. "Provide a status update to the team" → []
2. "The project is on-track for delivery" → []
3. "Generate the RAG-status report" → []
4. "What is the delivery status?" → []
5. "Update the status report with current progress" → []

---

## B37-0C-003-003 — "report" adjacent to DOCUMENT triggers: compound firing

**When "report" sentence DOES produce DOCUMENT:**

"Report the requirements to stakeholders" → DOCUMENT fires on "requirements" AND "stakeholders"
- "report" itself: no predicate
- "requirements": DOCUMENT fires
- "stakeholders": DOCUMENT fires
- Result: ["DOCUMENT"]

"Report back on the acceptance criteria" → TEST fires on "acceptance"; DOCUMENT fires on "criteria"
- Result: ["TEST", "DOCUMENT"]

**Rule:** The word "report" in a sentence never prevents or enables DOCUMENT. Only the DOCUMENT trigger tokens (document, spec, define, describe, requirement, story, criteria, stakeholder, functional, behavior, use-case) control DOCUMENT firing.

---

## B37-0C-003-004 — Complete "report" knowledge edge: five cases

1. "Write the status report" → IMPLEMENT fires on "write"; "report" adds nothing
2. "Describe the status report structure" → DOCUMENT (describe), ARCHITECTURE (structure); "report" adds nothing
3. "Review the status report" → QA (review); "report" adds nothing
4. "Status report distributed to stakeholders" → DOCUMENT (stakeholders); "report" adds nothing
5. "Send the status report" → []; no predicate fires on "send" or "report"

---

## B37-0C-003-005 — Why "report" exclusion matters for B37

**B37 intent vs lexical outcome:**

The core B37 activity is status reporting to stakeholders and management. Yet:

- "status report" → zero predicates (both tokens are non-firing)
- "progress report" → zero predicates (both tokens are non-firing)
- "delivery report" → zero predicates
- "project report" → zero predicates

This means the most common B37 request pattern produces zero predicates and routes to knowledge_edge. This is by design — B37 is a capability gap scenario. The absence of a REPORTING predicate is the gap being documented.
