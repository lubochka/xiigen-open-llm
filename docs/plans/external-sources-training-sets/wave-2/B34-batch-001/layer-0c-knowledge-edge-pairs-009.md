# Layer 0-C — Knowledge Edge Pairs (009)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-009-001 — "refactor" in training context

"Teach refactoring techniques" → gap (teach = guidance; refactoring = topic)
"Refactor the training module code" → IMPLEMENT + TransitionType: refactoring
"Guide the team through refactoring" → IMPLEMENT fires (refactoring = embedded code action via "through refactoring")
"Write a guide on refactoring" → DOCUMENT ("write a guide" → DOCUMENT)

Rule: "refactor" as direct code action → IMPLEMENT (refactoring TransitionType). "teach/explain refactoring" → gap. "write about refactoring" → DOCUMENT.

---

## KE-009-002 — "adopt" methodology → gap

"Adopt agile practices for the team" → gap (adopt methodology = organizational change vocabulary → gap)
"Implement the agile sprint board feature" → IMPLEMENT (code verb + software artifact)
"Document the agile process" → DOCUMENT

Rule: "adopt [methodology]" = organizational practice vocabulary → gap. No predicate fires.

---

## KE-009-003 — "introduce" vs "implement"

"Introduce clean architecture to the team" → gap (introduce = teaching vocabulary)
"Implement a clean architecture separation in the service" → IMPLEMENT (implement = code verb)
"Introduce new developers to the codebase" → gap (introduce people = onboarding vocabulary)

Rule: "introduce" always → gap. "implement" always → IMPLEMENT (code context).

---

## KE-009-004 — Predicate deduplication in B34 mixed requests

"Build the skill tracker, implement the badge system, and create the progress dashboard" → ["IMPLEMENT"] (three IMPLEMENT triggers → one predicate)
"Write the onboarding guide and document the process flow" → ["DOCUMENT"] (two DOCUMENT triggers → one predicate)
Rule: Deduplication applies — same predicate triggered multiple times → appears once in predicate set.
