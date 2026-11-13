# Layer 0-C — Knowledge Edge Pairs (034)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-034-001 — "run through" knowledge edge: explanation vs execution

"Run through the checklist with the team" (explanation with person) → gap
"Run through the deployment steps with the ops team" (walk-through with person) → gap
"Run through the test suite" (execute without person target) → borderline: if review intent = gap; if execute intent = TEST
"Run the tests" (direct execution) → TEST fires
Key: presence of "with [person]" target = guidance vocabulary → gap. Absence = context-dependent.

---

## KE-034-002 — "review [X] with [Y]" knowledge edge: teaching vs audit

"Review the code with the junior developer" → gap (mentoring review)
"Review the code for security vulnerabilities" → SECURITY (audit purpose)
"Review the code with the security team and patch bugs" → SECURITY fires; review with = gap warning
"Review and update the documentation" → DOCUMENT fires; review = prep action
"Peer review / code review with [person]" → gap (collaborative review = mentoring)
Edge: "review with team AND fix vulnerability" → SECURITY fires; review with = gap warning.

---

## KE-034-003 — "present" knowledge edge: when does it become code action?

"Present the findings to stakeholders" → gap (always)
"Present the API (Application Programming Interface) contract to clients" → gap (presenting = guidance)
"Present the test results" → gap (presenting = guidance)
"Present and then implement the changes" → IMPLEMENT fires; present = gap warning
"Build a presentation" → DOCUMENT or IMPLEMENT depending on artifact type (slide deck = DOCUMENT; interactive tool = IMPLEMENT)
Edge: "present" never fires a predicate alone. Always gap when standalone.

---

## KE-034-004 — "discuss" knowledge edge: decision boundary

"Discuss the approach with the team" → gap
"Discuss and agree on the architecture" → gap (agree = not a code action)
"Discuss and then architect the system" → ARCHITECTURE fires; discuss = gap warning
"Discuss, document the decision, and implement" → DOCUMENT + IMPLEMENT fire; discuss = gap warning
"Discuss API design in the meeting" → gap (discuss = guidance vocabulary; meeting context irrelevant)

---

## KE-034-005 — advisory vocabulary knowledge edge: when topic fires vs doesn't fire

Advisory vocabulary (consult, seek advice, get feedback, sanity check, second opinion, rubber duck, sounding board):
Topic NEVER fires its associated predicate when only advisory vocabulary is present.
Topic DOES fire when requester's direct code action on that topic also appears:
"Consult on architecture, then architect it" → ARCHITECTURE fires
"Get feedback on tests, then write them" → TEST fires
"Rubber duck security design, then harden it" → SECURITY fires
Predicate fires from the requester's code action, NOT from the advisory vocabulary + topic.
