# Layer 0-C — Knowledge Edge Pairs (005)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: knowledge_edge

## KE-005-001 — Ordering rules for B15 multi-predicate requests
1. ARCHITECTURE first (if present) — design before build
2. API_CONTRACT before IMPLEMENT — contract spec informs implementation
3. IMPLEMENT
4. SECURITY after IMPLEMENT — audits the built artifact
5. TEST after IMPLEMENT — tests run on implemented code
6. DOCUMENT parallel with TEST (after IMPLEMENT)
7. QA absolute last

## KE-005-002 — "wire up" = API_CONTRACT in B15
"wire up the payment gateway" → API_CONTRACT
"wire up the carrier API" → API_CONTRACT
Rule: "wire up" = external system integration verb → API_CONTRACT.

## KE-005-003 — "add" disambiguation: IMPLEMENT vs SECURITY vs AUTHORIZATION
"add order processing capability" → IMPLEMENT (code artifact)
"add input validation against injection" → SECURITY (security control)
"add role-based access control" → AUTHORIZATION (access control)
Rule: "add" fires the predicate of what follows — capability/module → IMPLEMENT; security control → SECURITY; access control → AUTHORIZATION.

## KE-005-004 — "four predicates never collapse" teaching point
All four of these fire independently: build→IMPLEMENT; integrate→API_CONTRACT; write tests→TEST; document→DOCUMENT
v6-03 error: collapsing to one or two predicates loses routing fidelity.
