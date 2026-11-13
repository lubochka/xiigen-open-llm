# Layer 0-C — Knowledge Edge Pairs (040)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-040-001 — "upskill [team] on" vocabulary edge

"Upskill the backend team on Rust (Rust programming language) ownership patterns" → gap-skills-guidance-train-001
"Upskill and implement" → IMPLEMENT fires; upskill = gap warning
"Upskill" alone (from Round 1 vocabulary) = gap, DomainId: software-development
Upskill = always guidance vocabulary regardless of technology topic mentioned.

---

## KE-040-002 — "shadow [person while they do X]" vs "requester doing X" edge

"Shadow the senior engineer while they configure the Kubernetes (K8s) cluster" → gap (requester shadows; engineer configures = third-party action)
"Shadow and then configure the cluster yourself" → IMPLEMENT fires; shadow = gap warning
"Shadow while she integrates the service" → special: API_CONTRACT fires (integrate exception: even third-party integration fires API_CONTRACT)
"Shadow while she writes the tests" → gap (team's test-writing = third-party action; integrate exception does not extend to TEST)

---

## KE-040-003 — "pair with [person] to [verb]" edge extensions

"Pair with to learn the testing framework" → gap (learning intent → gap)
"Pair with to write the integration tests" → TEST fires; pair-with = gap warning (write tests = direct code action)
"Pair with to integrate the auth service" → API_CONTRACT fires; pair-with = gap warning (integrate = always fires)
"Pair with to refactor the controller" → IMPLEMENT fires; pair-with = gap warning

---

## KE-040-004 — "workshop on [topic]" vocabulary edge

"Workshop on microservices design patterns" → gap (workshop as training event)
"Run a workshop to introduce the team to GraphQL (Graph Query Language)" → gap (workshop + introduce = guidance)
"Workshop the API (Application Programming Interface) design and then publish the API contract" → API_CONTRACT fires; workshop = gap warning
"Workshop the design and implement the service" → IMPLEMENT fires; workshop = gap warning

---

## KE-040-005 — "advise [person] to [verb]" vs "advise and [verb]" edge

"Advise the team to implement the circuit breaker pattern" → gap (advise to = recommend others act = gap)
"Advise on the approach and implement the circuit breaker" → IMPLEMENT fires; advise = gap warning (requester implements)
"Advise [person] to [code verb]" → gap (they perform; requester advises)
"Advise and [code verb]" → code predicate fires; advise = gap warning (requester performs)
