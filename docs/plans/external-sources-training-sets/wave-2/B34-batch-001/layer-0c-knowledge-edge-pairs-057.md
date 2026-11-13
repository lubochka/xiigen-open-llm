# Layer 0-C — Knowledge Edge Pairs (057)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-057-001 — "knowledge transfer" extended edge cases

"Do a knowledge transfer on the payment processing pipeline" → gap (knowledge transfer = guidance event)
"Knowledge transfer the deployment process to the new team" → gap (knowledge transfer as verb = guidance)
"Knowledge transfer and document the process" → DOCUMENT fires; knowledge transfer = gap warning
"Facilitate a knowledge transfer session" → gap (facilitate + knowledge transfer = both guidance vocabulary)
"Organize a knowledge transfer and implement the automation" → IMPLEMENT fires; organize knowledge transfer = gap warning
Rule: knowledge transfer in any form = guidance vocabulary → gap when standalone.

---

## KE-057-002 — "lunch and learn" / "brown bag" as guidance vocabulary

"Run a lunch and learn on the new authentication framework" → gap (lunch and learn = training event = guidance vocabulary → train-001)
"Brown bag session on microservices" → gap (brown bag = informal training = guidance vocabulary → train-001)
"Organize a lunch and learn and implement the framework" → IMPLEMENT fires; lunch and learn = gap warning
"Schedule a brown bag and document the architecture" → DOCUMENT fires; brown bag = gap warning
Rule: informal training event vocabulary (lunch and learn, brown bag) = guidance → gap.

---

## KE-057-003 — "walkthrough" (one word) vs "walk through" (two words) edge

"Do a walkthrough of the deployment process with the team" → gap (walkthrough/walk through = same vocabulary; both = guidance)
"Walkthrough and implement the process" → IMPLEMENT fires; walkthrough = gap warning
"Code walkthrough with the junior developer" → gap (code walkthrough = guidance/mentoring vocabulary)
Rule: "walkthrough" (one word) = same as "walk through" (two words) = guidance vocabulary.

---

## KE-057-004 — "show" in multiple senses: display vs guidance

"Show [person] how to [verb]" → gap (guidance: teaching frame)
"Show [person] [noun/result]" → neutral (display: not guidance by itself)
"Show [feature] to [stakeholder] in demo" → gap (demo = demonstration act = guidance vocabulary)
"Show the test results in the CI (Continuous Integration) report" → neutral (display in automated report)
"Show the architecture on the whiteboard to the team" → gap (show to team = presentation = guidance)
Rule: "show how to" = gap; "show [output] to [person]" = borderline/gap if presentation intent; "show in report" = neutral.

---

## KE-057-005 — "get input on" / "ask for input on" as advisory vocabulary

"Get input on the database schema from the team" → gap (get input = advisory vocabulary → mentor-001)
"Ask for input on the API design" → gap (ask for input = seeking advisory → mentor-001)
"Get input and implement the schema changes" → IMPLEMENT fires; get input = gap warning
"Ask for input on how to implement" → gap (advisory + how to = double exception)
Rule: "get input on" / "ask for input on" = advisory vocabulary → gap. Parallel to seek advice on / get feedback on.
