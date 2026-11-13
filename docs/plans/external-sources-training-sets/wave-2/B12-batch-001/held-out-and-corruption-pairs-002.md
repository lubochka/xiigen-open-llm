# Held-Out and Corruption Pairs (002)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B12-HO-002 — Held-out: "service contract" fires API_CONTRACT

```
source_id: xiigen-b12-ho-002
pair_kind: held_out
hold_out_reason: "Tests generalization of 'contract' rule to 'service contract' phrase"
```

A:
  run_id: "B12-HO-002"
  user_request: "Specify the service contract for the notification microservice."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B (expected):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Routing: ["3.3", "2.1"]
  TransitionType: "documentation"
  Reason: "service contract" → API_CONTRACT fires on "contract" (Rule 2); "specify" → DOCUMENT

Held-out evaluation:
  Pass condition: CompiledPredicateIds contains "API_CONTRACT"
  Fail condition: CompiledPredicateIds = ["DOCUMENT"] only (v6-01 corruption)
  
---

## B12-COR-002 — Corruption: API_CONTRACT suppressed on "service contract"

```
source_id: xiigen-b12-cor-002
corruption_type: v6-01
pair_kind: corruption
```

A: same request as B12-HO-002
  user_request: "Specify the service contract for the notification microservice."

B (corrupt):
  CompiledPredicateIds: ["DOCUMENT"]  ← API_CONTRACT missing
  Routing: ["2.1"]  ← 3.3 missing
  TransitionType: "documentation"
  CorruptReason: "v6-01: classifier treated 'service' modifier as non-API domain signal → suppressed API_CONTRACT"

Corruption label: API_CONTRACT SUPPRESSED ON CONTRACT MODIFIER
Expected correct: ["API_CONTRACT", "DOCUMENT"]
Corrupt output: ["DOCUMENT"]

Teaching: "service contract" → "contract" is the key term → API_CONTRACT fires.
"Service" is the modifier describing WHAT is contracted, not the domain of the contract.
