# Held-Out and Corruption Pairs (010)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B12-HO-010 — Held-out: "data contract" → API_CONTRACT fires

```
source_id: xiigen-b12-ho-010
pair_kind: held_out
hold_out_reason: "Tests generalization of 'contract' rule to 'data contract' (singular)"
```

A:
  user_request: "Define the data contract for the event payload exchanged between the order and shipping services."

B (expected):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  Routing: ["3.3", "2.1"]
  Reason:
    "data contract" → API_CONTRACT fires on "contract" (Rule 2: modifier "data" irrelevant)
    "define" → DOCUMENT fires
    "event payload" → "event" context; "payload" alone not a trigger
    Note: no "webhook" here → "payload" does not additionally fire API_CONTRACT
    Core test: "data contract" → API_CONTRACT fires

Pass condition: CompiledPredicateIds contains "API_CONTRACT"

---

## B12-COR-010 — Corruption: "data" suppresses API_CONTRACT on "data contract" (v6-01)

```
source_id: xiigen-b12-cor-010
corruption_type: v6-01
pair_kind: corruption
```

A: same request as B12-HO-010

B (corrupt):
  CompiledPredicateIds: ["DOCUMENT"]  ← API_CONTRACT missing (v6-01)
  Routing: ["2.1"]  ← 3.3 missing
  CorruptReason: "v6-01: 'data contract' → classifier determined 'data' = data engineering domain
  → 'contract' suppressed as non-API usage → only DOCUMENT fires"

Corruption label: API_CONTRACT SUPPRESSED ON 'DATA CONTRACT' (v6-01)
Expected correct: ["API_CONTRACT", "DOCUMENT"]
Corrupt output: ["DOCUMENT"]

Teaching: This is the canonical v6-01 example. "data contract" = "contract" for data payload at
an API boundary. "Data" describes what is being contracted, not the domain.

The five contract modifiers that ALL trigger v6-01 risk:
  "data contract" → v6-01 suppresses API_CONTRACT ← THIS CASE
  "service contract" → v6-01 suppresses API_CONTRACT
  "message contract" → v6-01 suppresses API_CONTRACT
  "event contract" → v6-01 suppresses API_CONTRACT

All are incorrect. In all cases: "contract" → API_CONTRACT fires unconditionally.
