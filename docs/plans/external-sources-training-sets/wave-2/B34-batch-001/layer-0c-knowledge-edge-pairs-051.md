# Layer 0-C — Knowledge Edge Pairs (051)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-051-001 — "brief and QA" edge

"Brief the team on the QA (Quality Assurance) process and QA the release candidate" → QA fires; brief = gap warning
"Brief on QA best practices" → gap (brief on = guidance; QA practices = topic, not direct QA action)
"Brief and ensure quality" → gap (ensure quality ≠ QA abbreviation direct action)
Rule: "QA" abbreviation as verb = QA fires; guidance vocabulary + quality topic (without abbreviation verb) = gap.

---

## KE-051-002 — "help [person] understand" edge

"Help the team understand the deployment pipeline" → gap (help understand = teaching intent → gap-skills-guidance-train-001)
"Help the team understand and implement the deployment pipeline" → IMPLEMENT fires; help understand = gap warning
"Help the team understand how to integrate the service" → gap (help understand how to = learning frame)
"Help the team integrate the service" → API_CONTRACT fires (help + direct action; no learning frame)
Rule: "help [person] understand [topic]" = gap; "help [person] [direct code verb]" = code predicate fires.

---

## KE-051-003 — "coach through [X]" participial embedded verb extensions

"Coach through building the service" → IMPLEMENT fires + gap warning (build = participial in guidance frame)
"Coach through writing the tests" → TEST fires + gap warning (write tests = participial)
"Coach through integrating the provider" → API_CONTRACT fires + gap warning (integrate = embedded verb rule)
"Coach through architecting the service boundaries" → ARCHITECTURE fires + gap warning
"Coach through documenting the API" → DOCUMENT fires + gap warning
Rule: all participial code verbs in coach-through frame = predicate fires + gap warning. Mirror of walk-through rule.

---

## KE-051-004 — "guide through [X]" participial embedded verb extensions

"Guide through implementing the circuit breaker" → IMPLEMENT fires + gap warning
"Guide through testing the payment flow" → TEST fires + gap warning
"Guide through integrating the payment gateway" → API_CONTRACT fires + gap warning
"Guide through hardening the authentication module" → SECURITY fires + gap warning
"Guide through documenting the service contracts" → DOCUMENT fires + gap warning
Rule: guide through + participial code verb = predicate fires + gap warning. Same as coach-through.

---

## KE-051-005 — "walk [person] through" vs "walk through" (no person) edge

"Walk the team through the deployment process" → gap (walk [person] through = explanation with explicit person target)
"Walk through the deployment process with the team" → gap (walk through [X] with [person] = explanation)
"Walk through the deployment process" (no person) → gap (walk through alone = explanation vocabulary; person implied)
"Walk through building the service" → IMPLEMENT fires + gap warning (participial = embedded verb rule; person target irrelevant)
Rule: "walk through [noun phrase]" = gap; "walk through [participial verb phrase]" = embedded verb fires.
