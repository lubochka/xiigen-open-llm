# Layer 0-C — Knowledge Edge Pairs (023)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-023-001 — Mixed guidance + code verbs: priority analysis

When multiple guidance verbs and code verbs appear in one request:
1. Scan all action verbs for code predicates first.
2. For each guidance verb, check if it has a participial embedded code action.
3. Code predicates fire if any are present (regardless of how many guidance verbs appear).
4. Each guidance verb becomes a gap warning (not a gap log) when any predicate fires.
5. GapHandler invoked only if final predicate set = [].

---

## KE-023-002 — Mixed request examples

"Coach through building the API, walk through integrating the payment gateway, guide through writing smoke tests."
→ IMPLEMENT (build), API_CONTRACT (integrate), TEST (write tests)
→ Three guidance frames = three gap warnings
→ GapHandler NOT invoked

"Mentor, advise, and coach the team on scaling."
→ [] (no code verbs)
→ GapHandler + SCHEDULE_CAPABILITY_LEARNING

"To teach and onboard engineers, implement the sample project and document the guide."
→ IMPLEMENT (implement), DOCUMENT (document)
→ "teach" and "onboard" = gap warnings
→ "to onboard" = purpose clause; gap warning only

---

## KE-023-003 — Guidance framing vs guidance as primary action

"Mentor the team and implement the solution" → IMPLEMENT fires; mentor = gap warning
"Implement the solution while mentoring the team" → IMPLEMENT fires; mentoring = gap warning
"Mentoring the team, implement the solution" → IMPLEMENT fires; mentoring = gap warning

All three are equivalent. Position of guidance verb (before, after, participial) does not change the rule. Code verb fires its predicate; guidance verb becomes gap warning.
