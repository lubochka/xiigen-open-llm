# Layer 0-C — Knowledge Edge Pairs (039)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-039-001 — "rubber duck [architecture]" = gap; "rubber duck and architect" edge

"Rubber duck the architecture decisions with a colleague" → gap (thinking-aloud = advisory)
"Rubber duck the decisions and then architect the final topology" → ARCHITECTURE fires; rubber duck = gap warning
"Think through the architecture choices out loud" → gap (reflective = advisory)
"Think through and architect" → ARCHITECTURE fires; think through = gap warning

---

## KE-039-002 — "sounding board for [test strategy]" vs "write tests" edge

"Use the team as a sounding board for the test strategy" → gap
"Use the team as a sounding board and write the tests based on feedback" → TEST fires; sounding board = gap warning
"Sounding board the approach and QA the release" → QA fires; sounding board = gap warning
All: sounding board alone = gap; direct code action alongside sounding board = code predicate fires.

---

## KE-039-003 — "discuss how to [code verb]" = gap; "discuss and [code verb]" edge

"Discuss how to implement the retry logic" → gap (discuss how to = learning/decision frame)
"Discuss and implement the retry logic" → IMPLEMENT fires; discuss = gap warning
"Discuss how to test the module" → gap (discuss how to = learning target)
"Discuss and write the tests" → TEST fires; discuss = gap warning
Boundary: "discuss how to" = deliberation = gap; "discuss and [verb]" = discussion precedes action = code fires.

---

## KE-039-004 — purpose clause guidance + multiple predicates edge

"For onboarding purposes, implement the developer CLI (Command-Line Interface) tool, write tests, and document it."
→ Predicates: ["IMPLEMENT", "TEST", "DOCUMENT"] | GapWarning: gap-skills-guidance-onboard-001
Purpose clause = gap warning only; three direct code verbs all fire their predicates.

---

## KE-039-005 — "coaching session" vs "coached to build" edge

"Run a coaching session for the backend team" → gap (coaching session = guidance event → gap)
"Coached [someone] to build the service" → gap (coached = past tense guidance vocabulary → gap)
"During the coaching session, implement the feedback" → IMPLEMENT fires; coaching session = gap warning context
"Coach [someone] who then builds the service" → gap (they build = third-party action)
Boundary: coaching/coach = guidance vocabulary → gap; requester's direct build action = IMPLEMENT.
