# Held-Out and Corruption Pairs (023)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B12-HO-023 — Held-out: IMPLEMENT + API_CONTRACT dual-fire on "build the REST endpoint"

```
source_id: xiigen-b12-ho-023
pair_kind: held_out
hold_out_reason: "Tests that IMPLEMENT and API_CONTRACT fire simultaneously; TransitionType becomes feature_addition"
```

A:
  user_request: "Build the REST API endpoints for the order processing service and document the request/response contract."

B (expected):
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "DOCUMENT"]
  Routing: ["3.3", "3.2", "2.1"]
  TransitionType: "feature_addition"
  Reason: "REST" + "api" + "endpoints" + "contract" → API_CONTRACT; "build" → IMPLEMENT; "document" → DOCUMENT; "request"/"response" reinforce API_CONTRACT

Held-out evaluation:
  Pass: CompiledPredicateIds contains both "API_CONTRACT" and "IMPLEMENT"; TransitionType = "feature_addition"
  Fail (v6-01): IMPLEMENT suppressed → TransitionType stays "documentation"

---

## B12-COR-023 — Corruption: IMPLEMENT suppressed on "build REST API endpoints"

```
source_id: xiigen-b12-cor-023
corruption_type: v6-01
pair_kind: corruption
```

A: same request as B12-HO-023

B (corrupt):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]  ← IMPLEMENT missing
  TransitionType: "documentation"  ← wrong
  CorruptReason: "v6-01: classifier saw 'document the contract' and treated the request as documentation-only; 'build' fires IMPLEMENT unconditionally regardless of the documentation clause"

Correct: ["API_CONTRACT", "IMPLEMENT", "DOCUMENT"], TransitionType: "feature_addition"
Corrupt: ["API_CONTRACT", "DOCUMENT"], TransitionType: "documentation"

---

## B12-COR-023B — Corruption: FlowOrder wrong when IMPLEMENT and API_CONTRACT both fire

```
source_id: xiigen-b12-cor-023b
corruption_type: v6-03
pair_kind: corruption
```

A: same request as B12-HO-023

B (corrupt):
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "DOCUMENT"]
  FlowOrder: ["APIContractFlow33", "ImplementFlow32", "DocumentationFlow21"]  ← CORRUPT order
  CorruptReason: "v6-03: IMPLEMENT and API_CONTRACT should execute in parallel (same tier), not sequentially; DocumentationFlow21 is last; QA always follows Documentation"

Correct FlowOrder: IMPLEMENT ∥ API_CONTRACT (parallel, same tier) → DOCUMENT → QA
Corrupt FlowOrder: APIContractFlow33 → ImplementFlow32 → DocumentationFlow21 (sequential, wrong)
Rule: IMPLEMENT and API_CONTRACT are peer sub-phases 3.2 and 3.3; they execute in parallel unless dependency requires sequencing.
