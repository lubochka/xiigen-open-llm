# Layer 3 — Teaching Unit Pairs (023)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU-023 — Mixed guidance + code verbs teaching unit

**Core rule summary**: When multiple guidance verbs and code verbs appear in one request:
1. All guidance verbs → gap warnings (never fire predicates)
2. All embedded code verbs → fire their predicates if requester performs the action
3. Third-party actions → never fire predicates
4. GapHandler invoked only when predicate set = []
5. SCHEDULE_CAPABILITY_LEARNING emitted only with GapHandler

**Mixed request analysis examples**:

Three guidance + three embedded code actions:
"Coach through building, walk through integrating, guide through writing tests"
→ IMPLEMENT + API_CONTRACT + TEST | three gap warnings | GapHandler suppressed

Multiple guidance verbs, no embedded code actions:
"Mentor, advise, coach the team on leadership"
→ [] | GapHandler | SCHEDULE_CAPABILITY_LEARNING emitted once

Guidance + requester code action vs guidance + third-party code action:
"Mentor the team and implement the solution" → IMPLEMENT + gap warning
"Mentor the team and let them implement the solution" → [] + GapHandler (let them = third-party directive)

Purpose-clause guidance with direct requester code actions:
"To onboard engineers, implement the starter project and document the guide"
→ IMPLEMENT + DOCUMENT | "to onboard" = purpose gap warning

**Key teaching point**: The actor of the embedded code verb is decisive. Requester acts → predicate fires. Others act → no predicate. Guidance vocabulary framing never changes this rule.
