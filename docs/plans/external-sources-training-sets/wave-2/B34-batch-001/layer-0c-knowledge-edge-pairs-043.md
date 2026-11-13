# Layer 0-C — Knowledge Edge Pairs (043)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-043-001 — "guidance vocabulary + passive construction" edge

"The team should be walked through the new deployment process" (passive) → gap (walk through = guidance; passive voice = requester arranges, not performs)
"The codebase should be refactored" (passive, no guidance) → IMPLEMENT fires (refactor = code action regardless of voice)
"The junior devs should be mentored on testing patterns" → gap (mentor = guidance vocabulary; passive does not suppress gap)
Rule: passive construction with guidance vocabulary = gap. Passive construction with code verb = code predicate fires.

---

## KE-043-002 — "I need to [guidance verb]" vs "I need to [code verb]" edge

"I need to brief the team on the release plan" → gap (brief = guidance vocabulary)
"I need to implement the release automation" → IMPLEMENT fires
"I need to brief and implement" → IMPLEMENT fires; brief = gap warning
"I need to seek advice before implementing" → IMPLEMENT fires; seek advice = gap warning (before = temporal adjacency still triggers)
Rule: first-person intent framing ("I need to") does not change guidance vocabulary recognition.

---

## KE-043-003 — "we should [guidance verb]" vs "we should [code verb]" edge

"We should consult the security team on this" → gap (consult = advisory)
"We should implement this pattern" → IMPLEMENT fires
"We should consult and implement" → IMPLEMENT fires; consult = gap warning
"We need to get feedback and document the findings" → DOCUMENT fires; get feedback = gap warning
Rule: collective framing ("we should/we need to") does not change vocabulary recognition.

---

## KE-043-004 — "help [person] [code verb]" vs "help [person] learn" edge

"Help the team implement the circuit breaker" → IMPLEMENT fires (help + direct code action = requester performs alongside team)
"Help the team learn how to implement the circuit breaker" → gap (help + learn how to = teaching intent → gap)
"Help [person] integrate [X]" → API_CONTRACT fires (help + integrate = direct action)
"Help [person] understand how to integrate [X]" → gap (help understand how to = learning target)
Rule: "help [person] [code verb]" = code predicate fires; "help [person] learn/understand how to [code verb]" = gap.

---

## KE-043-005 — "knowledge transfer on [topic]" as guidance vocabulary

"Do a knowledge transfer on the CI/CD (Continuous Integration/Continuous Deployment) pipeline" → gap (knowledge transfer = guidance event → mentor-001 or train-001)
"Knowledge transfer and document the process" → DOCUMENT fires; knowledge transfer = gap warning
"Conduct a knowledge transfer session on the architecture" → gap (knowledge transfer = guidance vocabulary)
Rule: knowledge transfer = guidance vocabulary → gap when standalone. Paired with code action = gap warning.
