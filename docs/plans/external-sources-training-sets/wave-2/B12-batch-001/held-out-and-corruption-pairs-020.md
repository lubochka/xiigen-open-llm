# Held-Out and Corruption Pairs (020)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B12-HO-020 — Held-out: "multipart upload API" fires API_CONTRACT and "authentication" fires SECURITY

```
source_id: xiigen-b12-ho-020
pair_kind: held_out
hold_out_reason: "Tests that file upload domain does not suppress API_CONTRACT, and that authentication fires SECURITY unconditionally even inside API definition context"
```

A:
  user_request: "Define the multipart file upload API contract including authentication and error response codes."

B (expected):
  CompiledPredicateIds: ["API_CONTRACT", "SECURITY", "DOCUMENT"]
  Routing: ["3.3", "3.4", "2.1"]
  TransitionType: "documentation"
  Reason: "api" + "contract" → API_CONTRACT (double trigger); "authentication" → SECURITY (unconditional); "define" + "response codes" → DOCUMENT

Held-out evaluation:
  Pass: CompiledPredicateIds contains "API_CONTRACT" and "SECURITY"
  Fail (v6-02): "authentication" treated as API documentation term → SECURITY suppressed

---

## B12-COR-020 — Corruption: SECURITY suppressed on "authentication" in upload API contract

```
source_id: xiigen-b12-cor-020
corruption_type: v6-02
pair_kind: corruption
```

A: same request as B12-HO-020

B (corrupt):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]  ← SECURITY missing
  CorruptReason: "v6-02: classifier saw 'authentication' inside an API contract context and treated it as an API term (auth method documentation); SECURITY fires on 'authentication' unconditionally regardless of surrounding context"

Correct: ["API_CONTRACT", "SECURITY", "DOCUMENT"]
Corrupt: ["API_CONTRACT", "DOCUMENT"]

---

## B12-COR-020B — Corruption: DecisionCount=2 instead of 3 for three-predicate upload request

```
source_id: xiigen-b12-cor-020b
corruption_type: v6-03
pair_kind: corruption
```

A: same request as B12-HO-020

B (corrupt):
  CompiledPredicateIds: ["API_CONTRACT", "SECURITY", "DOCUMENT"]
  RoutingDecisions: [
    { predicate: "API_CONTRACT", routes_to: "3.3" },
    { predicate: "SECURITY", routes_to: "3.4" }
  ]
  DecisionCount: 2  ← CORRUPT (should be 3)

Correct: DecisionCount: 3 (3.3 + 3.4 + 2.1)
Corrupt: DecisionCount: 2 (DOCUMENT routing to 2.1 dropped)
CorruptReason: "v6-03: DOCUMENT routing omitted when two higher-priority predicates present; each predicate always produces its own routing decision"
