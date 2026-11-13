# Layer 3 — Teaching Unit Pairs (006)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: teaching_unit

## TU-006 — Four predicates never collapse to one

Rule: When four action verbs fire four distinct predicates, all four flow factories are selected. Never collapse to one.

"Build the service [IMPLEMENT] + integrate with API [API_CONTRACT] + write tests [TEST] + document the spec [DOCUMENT]"
→ Four predicates → four factories → four flows

Common v6-03 error: all four → ["IMPLEMENT"] only → ImplementationFlow41 only
Correct: ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"] → four factories

Why it matters: API_CONTRACT ensures contract spec runs; TEST ensures test artifacts are created; DOCUMENT ensures documentation is produced. Collapsing to one loses three critical flows.
