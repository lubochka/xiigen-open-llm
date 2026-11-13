# Layer 0-C — Knowledge Edge Pairs (012)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: knowledge_edge

## KE-012-001 — Complete predicate verb priority hierarchy for B15

| Priority | Verb(s) | Predicate | B15 Notes |
|----------|---------|-----------|-----------|
| 1 | design/architect (+ structural noun) | ARCHITECTURE | domain model / bounded context also fires |
| 2 | integrate/wire up/connect (systems) | API_CONTRACT | "contract" noun also fires |
| 3 | build/implement/create/scaffold/bootstrap/write(code)/extend/refactor/add(code) | IMPLEMENT | most common B15 predicate |
| 4 | secure/harden/protect | SECURITY | — |
| 5 | authorize/add RBAC | AUTHORIZATION | — |
| 6 | write/create tests | TEST | 5.1 (unit) or 5.3 (E2E/integration) |
| 7 | document/spec out | DOCUMENT | — |
| 8 | QA (abbreviation) | QA | 7.1 (release) or 7.3 (feature gate) |

## KE-012-002 — "refactor" is always IMPLEMENT with refactoring TransitionType
Never a new predicate. Never a new flow. Always: IMPLEMENT predicate + refactoring TransitionType.

## KE-012-003 — Probationary acceptance in B15 nine-node flows
Node 4 in nine-node flow = Gate B check.
probationary_accepted: implementation scope valid, constraints met → flow continues.
probationary_rejected: out of scope or constraint violation → fallback / rework required.
This is specific to B15's ImplementationFlow41 nine-node format.

## KE-012-004 — No GapHandler in B15
Unlike B33 (AI gaps) and B34 (guidance gaps), B15 has no gap vocabulary.
All B15 requests fire at least IMPLEMENT. GapHandler is never invoked.
If a B15-style request produces [] predicates → error in predicate detection, not a valid gap.
