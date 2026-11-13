# Layer 0-C — Knowledge-Edge Pairs (003)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B38-0C-003-001 — Rule: "criteria" fires DOCUMENT always

Rule statement:
"criteria" → DOCUMENT ALWAYS
This fires regardless of the qualifying word before "criteria".

B38 Core Rule 3: "criteria" → DOCUMENT ALWAYS — even as "release criteria", "exit criteria", "acceptance criteria"

---

## B38-0C-003-002 — Release criteria fires DOCUMENT

Input: "Define the release criteria for the Q3 deployment."
- "release criteria" → DOCUMENT
- "deployment" → GAP
- "Define" → DOCUMENT (documentation action)
Result: ["DOCUMENT"]
Routing: Phase 2.1

Note: No QA terms present in this example. DOCUMENT fires but QA does not. Result is a single DOCUMENT predicate routing to 2.1.

---

## B38-0C-003-003 — Acceptance criteria fires DOCUMENT

Input: "Document the acceptance criteria for the user authentication feature."
- "Document" → DOCUMENT (explicit documentation action)
- "acceptance criteria" → DOCUMENT (criteria fires)
- "feature" → not IMPLEMENT here (no code authoring action; this is a documentation request)
Result: ["DOCUMENT"]
Routing: Phase 2.1

Contrast with: "Build the user authentication feature and document the acceptance criteria" → ["IMPLEMENT", "DOCUMENT"]

---

## B38-0C-003-004 — Exit criteria fires DOCUMENT

Input: "Validate the exit criteria before closing the release milestone."
- "Validate" → QA
- "exit criteria" → DOCUMENT (criteria fires DOCUMENT; "exit" qualifier irrelevant)
Result: ["QA", "DOCUMENT"]
Routing: 2.1 + 7.1

---

## B38-0C-003-005 — All criteria qualifiers: DOCUMENT fires for all

| Phrase | Predicate | Notes |
|---|---|---|
| release criteria | DOCUMENT | Rule 3 canonical example |
| acceptance criteria | DOCUMENT | Most common in sprint/gate context |
| exit criteria | DOCUMENT | Sprint/milestone completion |
| entry criteria | DOCUMENT | Gate entry requirements |
| completion criteria | DOCUMENT | Phase completion definition |
| readiness criteria | DOCUMENT | Deployment readiness check |
| performance criteria | DOCUMENT | NFR (Non-Functional Requirement) specification |
| quality criteria | DOCUMENT | QA definition |
| success criteria | DOCUMENT | Project success definition |

Every row fires DOCUMENT regardless of qualifier. The word "criteria" is the trigger.
