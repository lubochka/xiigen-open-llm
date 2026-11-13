# Layer 3 — Teaching Unit Pairs (013)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B12-TU-013 — Teaching: OrderWebhooks domain — "webhook contract" double trigger

```
source_id: xiigen-b12-tu-013
rule_id: B12-RULE-WEBHOOK-CONTRACT
pair_kind: teaching_unit
```

### Concept

"Webhook contract" is one of the highest-evidence API_CONTRACT phrases in B12.
Both "webhook" AND "contract" independently fire API_CONTRACT. This creates a double
trigger on the same predicate — but the result is still ONE API_CONTRACT instance.

### Training Examples

A: "Specify the order webhook contract for shipment events."
B: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
   Evidence: webhook → API_CONTRACT; contract → API_CONTRACT; specify → DOCUMENT

---

A: "Define the notification webhook contract for push delivery events."
B: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
   Evidence: webhook (trigger 1) + contract (trigger 2) → API_CONTRACT (once)

---

A: "Specify the carrier webhook contract and authentication requirements."
B: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
   Evidence: webhook → API_CONTRACT; contract → API_CONTRACT; authentication → SECURITY (ALWAYS)

### Memory aid

"webhook contract" = highest confidence API_CONTRACT case (two independent triggers).
"webhook" alone → API_CONTRACT.
"contract" alone → API_CONTRACT.
Together → same one predicate, very high confidence.
