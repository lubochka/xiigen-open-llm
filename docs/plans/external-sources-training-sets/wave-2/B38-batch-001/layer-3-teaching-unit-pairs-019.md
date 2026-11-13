# Layer 3 — Teaching Unit Pairs (019)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B38-TU-019-001 — Teaching: every "criteria" qualifier fires DOCUMENT

LESSON: Rule 3 is absolute for "criteria". Every qualifier produces DOCUMENT.

Complete list of confirmed qualifiers that fire DOCUMENT via "criteria":
- release criteria → DOCUMENT
- acceptance criteria → DOCUMENT
- exit criteria → DOCUMENT
- entry criteria → DOCUMENT
- readiness criteria → DOCUMENT
- completion criteria → DOCUMENT
- success criteria → DOCUMENT
- performance criteria → DOCUMENT
- quality criteria → DOCUMENT
- security criteria → DOCUMENT
- promotion criteria → DOCUMENT
- handoff criteria → DOCUMENT
- version tagging criteria → DOCUMENT
- post-release criteria → DOCUMENT
- incident resolution criteria → DOCUMENT
- hotfix exit criteria → DOCUMENT

Every single qualifier above fires DOCUMENT. No qualifier suppresses this rule.

---

## B38-TU-019-002 — Teaching: DOCUMENT-only scenarios (no QA terms)

LESSON: It is possible to have DOCUMENT without QA when no QA vocabulary is present.

"Document all exit criteria and readiness criteria for the production release."
- "Document" → DOCUMENT
- "exit criteria" → DOCUMENT
- "readiness criteria" → DOCUMENT
- No "review", "verify", "approve", "sign-off", "audit", etc.
→ ["DOCUMENT"] only

Result: DocumentationFlow only (Phase 2.1). No QA gate. TransitionType: no_contribution.

This is a valid result. Not every documentation task requires QA sign-off.

---

## B38-TU-019-003 — Teaching: security criteria fires both DOCUMENT and SECURITY

LESSON: "Security criteria" fires DOCUMENT (from "criteria") AND SECURITY (from "security").

Plus, "audit" + "security" co-fires both QA and SECURITY (rule 19).

"Audit the security criteria" → ["QA", "DOCUMENT", "SECURITY"]
- "audit" → QA (release context) + SECURITY (rule 19: security+audit co-fire)
- "security" → SECURITY
- "criteria" → DOCUMENT

Result: Three predicates from four words. Three routing decisions.
TransitionType: no_contribution (no IMPLEMENT).
