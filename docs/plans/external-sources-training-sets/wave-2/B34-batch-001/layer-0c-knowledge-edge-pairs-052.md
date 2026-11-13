# Layer 0-C — Knowledge Edge Pairs (052)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-052-001 — "present [X] and implement [X]" same-topic edge

"Present the circuit breaker design and implement it" → IMPLEMENT fires; present = gap warning
"Present the test strategy and write the tests" → TEST fires; present = gap warning
"Present the architecture and architect the system" → ARCHITECTURE fires; present = gap warning
"Present the security model and harden the service" → SECURITY fires; present = gap warning
Rule: present + same-topic code action = code predicate fires; present = gap warning in all cases.

---

## KE-052-002 — "discuss [X] and implement [X]" same-topic edge

"Discuss the implementation approach and implement the solution" → IMPLEMENT fires; discuss = gap warning
"Discuss the test strategy and write the integration tests" → TEST fires; discuss = gap warning
"Discuss the architecture and architect the boundaries" → ARCHITECTURE fires; discuss = gap warning
"Discuss the API design and integrate the endpoints" → API_CONTRACT fires; discuss = gap warning
Rule: discuss + same-topic code action = code predicate fires; discuss = gap warning in all cases.

---

## KE-052-003 — guidance vocabulary in multi-sentence request edge

"Please mentor the team this week. Also, implement the onboarding portal by Friday."
→ Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-mentor-001
Rule: multi-sentence requests are scanned as a unit. Guidance vocabulary in first sentence = gap warning; code action in second sentence = predicate fires.

---

## KE-052-004 — guidance vocabulary in parenthetical edge

"Implement the health check service (and brief the team on how it works when done)."
→ Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-mentor-001
Rule: parenthetical guidance vocabulary = gap warning; primary clause code action = predicate fires. Parenthetical position does not suppress gap warning.

---

## KE-052-005 — guidance vocabulary in footnote/qualifier edge

"Build and deploy the analytics pipeline — the team will need to be briefed on this afterward."
→ Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-mentor-001
Rule: future-intent guidance vocabulary ("will need to be briefed") = gap warning; current code action = predicate fires. Future framing does not suppress gap warning.
