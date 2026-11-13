# Layer 3 — Teaching Unit Pairs (002)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B12-TU-002 — Teaching: "contract" fires API_CONTRACT regardless of modifier

```
source_id: xiigen-b12-tu-002
rule_id: B12-RULE-CONTRACT-ALWAYS
pair_kind: teaching_unit
```

### Concept

The word "contract" is a universal trigger for API_CONTRACT in B12.
No modifier (word placed before "contract") can suppress this trigger.

### Why This Rule Exists

APIs are defined by their contracts — the agreed-upon shape of requests and responses
between producer and consumer. Whether the contract is called:
  - a "data contract" (about the data payload)
  - a "service contract" (about the service interface)
  - a "message contract" (about the message format)
  - an "event contract" (about an event payload)
  - a "webhook contract" (about a webhook delivery)

...the underlying concept is the same: a formal agreement between two systems about
how they will exchange information via an API boundary.

"Contract" is the invariant trigger because all these phrases describe API interface
agreements, regardless of what the modifying noun emphasizes.

### Training Examples

A:
  Request: "Define the data contracts between the order service and the fulfillment service."
  Expected: API_CONTRACT fires (on "contracts")

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Reason: "data contracts" → "contract" → API_CONTRACT (modifier "data" irrelevant)

---

A:
  Request: "Specify the service contracts for the payment microservice."
  Expected: API_CONTRACT fires (on "contracts")

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Reason: "service contracts" → "contract" → API_CONTRACT

---

A:
  Request: "Document the message contracts for the event bus."
  Expected: API_CONTRACT fires (on "contracts")

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Reason: "message contracts" → "contract" → API_CONTRACT

---

A:
  Request: "Define the event contracts for customer lifecycle events."
  Expected: API_CONTRACT fires (on "contracts")

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Reason: "event contracts" → "contract" → API_CONTRACT

---

A:
  Request: "Specify the webhook contract for order status notifications."
  Expected: API_CONTRACT fires (on "contract") and also on "webhook"

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Reason: "webhook contract" → BOTH "contract" AND "webhook" → API_CONTRACT (double trigger)

### Common Error

v6-01 corruption: classifier sees "data contracts" → classifies as data domain
→ API_CONTRACT suppressed → WRONG

Detection: if request has "contract" but CompiledPredicateIds lacks API_CONTRACT → corruption

### Memory Aid

"contract" = API boundary agreement → ALWAYS API_CONTRACT
The noun preceding "contract" describes the content, not the domain.
Data contracts are still API contracts — they define the data at an API boundary.
