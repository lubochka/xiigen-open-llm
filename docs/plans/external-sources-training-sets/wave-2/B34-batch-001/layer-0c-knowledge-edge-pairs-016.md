# Layer 0-C — Knowledge Edge Pairs (016)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-016-001 — "workshop" facilitation vs structural design

"Run a workshop on X" → gap (event facilitation)
"Facilitate a workshop on X" → gap (event facilitation)
"Design a workshop [curriculum/agenda/content]" → gap (content noun)
"Design the workshop [platform/tool/system]" → IMPLEMENT (software system noun)
"Build a workshop management system" → IMPLEMENT (build + software system)
"Write a workshop agenda" → DOCUMENT (write + documentation artifact)

Rule: Workshop as an event/learning format → gap. Workshop software system → IMPLEMENT/DOCUMENT.

---

## KE-016-002 — "run a workshop" vs "run tests" — both gap

"Run a workshop" → gap (run = facilitation verb for events → gap)
"Run unit tests" → gap (run ≠ write; running tests does not fire TEST)
"Run the CI/CD pipeline" → gap (run = execution verb → gap)

Rule: "run" is never a predicate-firing verb in any context.

---

## KE-016-003 — Workshop topic does not determine predicate

"Workshop on unit testing" → gap (facilitation of testing topic ≠ TEST)
"Workshop on architecture" → gap (facilitation of architecture topic ≠ ARCHITECTURE)
"Workshop on API design" → gap (facilitation of API topic ≠ API_CONTRACT)
"Workshop on security" → gap (facilitation of security topic ≠ SECURITY)

The topic inside a workshop frame never fires the associated predicate. Only direct action verbs by the requester fire predicates.
