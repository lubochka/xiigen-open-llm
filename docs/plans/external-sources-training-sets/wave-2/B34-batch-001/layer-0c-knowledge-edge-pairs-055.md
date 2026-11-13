# Layer 0-C — Knowledge Edge Pairs (055)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-055-001 — "merge" is not a code predicate in B34

"Review and merge the pull request" → gap (review with = guidance; merge = not a B34 code predicate)
"Code review and merge" → gap (both = mentoring/review activities; merge ≠ implement)
"Merge and then fix the conflict" → gap (fix conflict = not a code predicate; refactoring intent = borderline IMPLEMENT)
"Merge and refactor the conflicting logic" → IMPLEMENT fires; merge = neutral; refactor = IMPLEMENT
Rule: "merge" alone is not a B34 code predicate. Only when paired with clear IMPLEMENT action does IMPLEMENT fire.

---

## KE-055-002 — "approve" is not a code predicate in B34

"Review and approve the PR (Pull Request)" → gap (review with + approve = mentoring/review; approve ≠ code action)
"Approve the architecture design" → gap (approve = decision act, not architecture action)
"Approve and implement the design" → IMPLEMENT fires; approve = neutral/gap warning depending on context
Rule: "approve" alone is not a B34 code predicate. Approval = decision act, not code action.

---

## KE-055-003 — "validate" edge: validation as guidance vs code action

"Validate the approach with the team" → gap (validate with person = advisory → gap)
"Validate the API (Application Programming Interface) contract against the specification" → API_CONTRACT fires (validate = technical contract verification action)
"Validate and publish the contract" → API_CONTRACT fires; validate = code action in this context
"Get the approach validated" → gap (seeking validation from others = advisory)
Rule: "validate with person" = gap; "validate [technical artifact]" = code action context-dependent.

---

## KE-055-004 — "review [X] for [purpose]" disambiguation edge

"Review the codebase for learning opportunities" → gap (review for learning = guidance purpose → gap)
"Review the codebase for security vulnerabilities" → SECURITY fires (review for vulnerabilities = security audit action)
"Review the codebase for test coverage gaps" → TEST fires (review for test gaps = test analysis action)
"Review the codebase with the team" → gap (review with person = mentoring)
Rule: "review for [learning/mentoring purpose]" = gap; "review for [security/testing purpose]" = code predicate.

---

## KE-055-005 — complete B34 knowledge edge reference: infinitive vs participial

Infinitive ([to] + base verb after guidance vocabulary) = always gap:
show how TO build, demonstrate how TO test, guide THROUGH [how] TO integrate

Participial (verb+ing describing ongoing action) in guidance frame = predicate fires:
walk through INTEGRATING, coach through BUILDING, guide through WRITING tests

Noun phrase (code artifact named but not acted upon) in guidance frame = always gap:
explain THE TEST SUITE, brief on THE API CONTRACT, walk through THE ARCHITECTURE

Rule: the grammatical form determines the predicate outcome, not the vocabulary topic.
