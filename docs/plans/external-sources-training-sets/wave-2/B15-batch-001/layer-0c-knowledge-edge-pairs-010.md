# Layer 0-C — Knowledge Edge Pairs (010)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: knowledge_edge

## KE-010-001 — QA "always last" in B15: no exception
Even if QA is mentioned first in the request, orchestration places it last.
"QA the order service, then build it" → ImplementationFlow41 → QAGatingFlow73 (QA still last)
Rule: QA validates all preceding flows; it cannot run before them.

## KE-010-002 — Parallel execution opportunities in B15
IMPLEMENT ‖ DOCUMENT (when documentation does not depend on implementation output)
TEST ‖ DOCUMENT (after IMPLEMENT completes)
API_CONTRACT ‖ DOCUMENT (contract spec and documentation may proceed concurrently)
SECURITY ‖ AUTHORIZATION (when both audit the same artifact)

## KE-010-003 — "add" + security control = SECURITY, not IMPLEMENT
"add input validation middleware" → IMPLEMENT (middleware is code artifact)
"add input validation against injection attacks" → SECURITY (injection attack = security context)
Borderline: if "add middleware that validates against injection" → IMPLEMENT + SECURITY (both fire)

## KE-010-004 — SECURITY + AUTHORIZATION common in B15
"secure the data AND add role-based access" → SECURITY + AUTHORIZATION
These two predicates are commonly co-present when both data security and access control are mentioned.
Orchestration: ImplementationFlow41 → SecurityAuditFlow + AuthorizationFlow (parallel audit) → QA (if present)
