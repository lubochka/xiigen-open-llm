# Layer 3 — Teaching Unit Pairs (012)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: teaching_unit

## TU-012 — Complete B15 knowledge summary

No GapHandler in B15. Every meaningful request fires at least IMPLEMENT.

Key rules:
1. build/implement/scaffold/bootstrap/create/extend/refactor → IMPLEMENT
2. "contract" (any modifier) → API_CONTRACT also fires
3. "domain model" / "bounded context" → ARCHITECTURE also fires
4. integrate/wire up → API_CONTRACT
5. secure/harden → SECURITY
6. add RBAC/role-based access → AUTHORIZATION
7. write/create tests → TEST (5.1 or 5.3)
8. document/spec → DOCUMENT
9. QA abbreviation → QA (7.1 or 7.3)

TransitionType: feature_addition for IMPLEMENT; refactoring when "refactor" verb present.
QA: always last. No exceptions.
OperationResult<T>: always on public methods.
Fabric-first: always inject interfaces.
