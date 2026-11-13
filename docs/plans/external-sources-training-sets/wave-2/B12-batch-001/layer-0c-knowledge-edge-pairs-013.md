# Layer 0-C — Knowledge Edge Pairs (013)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B12-0C-013-001 — "webhook contract" fires API_CONTRACT twice (webhook + contract)

```
source_id: xiigen-b12-0c-013-001
rule_id: B12-RULE-WEBHOOK-CONTRACT
pair_kind: knowledge_edge
```

A:
  CompiledText: "Specify the order webhook contract for shipment events."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Predicate analysis:
    "specify" → DOCUMENT fires
    "webhook" → API_CONTRACT fires (webhook is a first-class API_CONTRACT trigger)
    "contract" → API_CONTRACT fires AGAIN (contract is also a first-class trigger)
    → Two terms both trigger API_CONTRACT, but result is still ONE predicate instance
    "order" — domain context, no predicate
    "shipment events" — domain context, no predicate
  Result: ["API_CONTRACT", "DOCUMENT"]
  Teaching: double trigger on same predicate does not add it twice to CompiledPredicateIds.
  "webhook contract" → API_CONTRACT appears once regardless of trigger count.

---

## B12-0C-013-002 — "idempotent" fires API_CONTRACT in delivery contract context

```
source_id: xiigen-b12-0c-013-002
rule_id: B12-RULE-IDEMPOTENT
pair_kind: knowledge_edge
```

A:
  CompiledText: "Specify the idempotent delivery contract for the order webhook endpoint."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Predicate analysis:
    "specify" → DOCUMENT fires
    "idempotent" → API_CONTRACT fires (idempotency is an API contract concept)
    "contract" → API_CONTRACT fires
    "webhook" → API_CONTRACT fires
    "endpoint" → API_CONTRACT fires
  Result: ["API_CONTRACT", "DOCUMENT"]
  Teaching: "idempotent" is in the API_CONTRACT lexicon. Idempotency is a fundamental
  API contract guarantee — it specifies how the API behaves on repeated requests.

---

## B12-0C-013-003 — OrderWebhooks boundary: "HMAC authentication" in webhook → SECURITY fires

```
source_id: xiigen-b12-0c-013-003
rule_id: B12-RULE-AUTHENTICATION-ALWAYS
corruption_type: v6-02
pair_kind: knowledge_edge
```

A:
  CompiledText: "Define the order webhook endpoint contract including HMAC authentication for payload verification."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  Predicate analysis:
    "define" → DOCUMENT fires
    "webhook" → API_CONTRACT fires
    "endpoint" → API_CONTRACT fires
    "contract" → API_CONTRACT fires
    "HMAC authentication" → SECURITY fires on "authentication" (ALWAYS, Core Rule 6)
  Result: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  Teaching: "authentication" fires SECURITY even when embedded in a webhook context.
  v6-02 target: "HMAC authentication is just a webhook security detail, not SECURITY" → WRONG.
  The word "authentication" unconditionally fires SECURITY regardless of method (HMAC, JWT, OAuth).
