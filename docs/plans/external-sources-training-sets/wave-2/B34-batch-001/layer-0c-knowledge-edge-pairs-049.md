# Layer 0-C — Knowledge Edge Pairs (049)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-049-001 — "shadow" third-party action + integrate exception

"Shadow while the lead engineer integrates the payment provider" → API_CONTRACT fires (integrate exception applies even to third-party action in shadow context)
"Shadow while the lead engineer writes the tests" → gap (test-writing = third-party action; integrate exception does not extend to TEST)
"Shadow while the lead engineer builds the service" → gap (build = third-party action; integrate exception does not extend to IMPLEMENT)
Rule: integrate exception extends to third-party actions; no other predicate extends to third-party actions.

---

## KE-049-002 — "pair with" direct code action vs learning intent

"Pair with to implement the retry logic" → IMPLEMENT fires (pair + direct IMPLEMENT action = code predicate)
"Pair with to learn how to implement the retry logic" → gap (pair + learn how to = learning intent → gap)
"Pair with to integrate the OAuth2 (OAuth 2.0) provider" → API_CONTRACT fires (pair + direct integrate = API_CONTRACT)
"Pair with to understand how to integrate" → gap (pair + understand how to = learning intent → gap)
Rule: "pair with to [direct code verb]" = code predicate fires; "pair with to learn/understand how to" = gap.

---

## KE-049-003 — "workshop [X]" as verb vs noun edge

"Workshop the service design with the team" (verb: collaborative session) → gap (workshop as activity = guidance)
"Run a workshop on service design" (noun: training event) → gap (workshop event = guidance)
"Workshop the API design and publish the contract" → API_CONTRACT fires; workshop = gap warning
"Workshop the implementation and build the service" → IMPLEMENT fires; workshop = gap warning
Both verb and noun forms of workshop = guidance vocabulary → gap when standalone.

---

## KE-049-004 — "review [X] with [Y] and fix/update" edge extensions

"Review the authentication code with the team and fix the injection vulnerability" → SECURITY fires; review with = gap warning
"Review the test coverage with the team and add the missing tests" → TEST fires; review with = gap warning
"Review the API documentation with the team and update the endpoint reference" → DOCUMENT fires; review with = gap warning
"Review the architecture with the team and finalize the topology" → ARCHITECTURE fires; review with = gap warning
Rule: "review [X] with [Y]" = gap warning when code action follows.

---

## KE-049-005 — "explain" edge: all forms, all topics → gap

"Explain the architecture" → gap (explain + architecture topic = gap, not ARCHITECTURE)
"Explain the security model" → gap (explain + security topic = gap, not SECURITY)
"Explain the test strategy" → gap (explain + test topic = gap, not TEST)
"Explain the integration approach" → gap (explain + integration topic = gap, not API_CONTRACT)
"Explain the codebase structure" → gap (explain + code topic = gap, not IMPLEMENT)
"Explain why" / "Explain how" / "Explain what" → all gap
Rule: explain in ANY form, with ANY topic = always gap-skills-guidance-train-001. Zero exceptions.
