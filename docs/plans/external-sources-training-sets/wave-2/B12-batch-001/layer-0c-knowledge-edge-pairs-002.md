# Layer 0-C — Knowledge Edge Pairs (002)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B12-0C-002-001 — Rule: "contract" fires API_CONTRACT always — five modifier examples

```
source_id: xiigen-b12-0c-002-001
rule_id: B12-RULE-CONTRACT-ALWAYS
pair_kind: knowledge_edge
```

### The Rule

"contract" fires API_CONTRACT REGARDLESS of its modifier (noun or adjective preceding it).
The trigger is the word "contract" itself, not the full noun phrase.
No modifier can suppress API_CONTRACT when "contract" is present.

### Five examples (all fire API_CONTRACT)

Example 1: "data contracts"
  Request: "Define the data contracts between the order and fulfillment services."
  Trigger term: "contracts" (in "data contracts")
  Fires: API_CONTRACT + DOCUMENT
  Modifier "data" does not suppress

Example 2: "service contracts"
  Request: "Specify the service contracts for the payment microservice."
  Trigger term: "contracts" (in "service contracts")
  Fires: API_CONTRACT + DOCUMENT
  Modifier "service" does not suppress

Example 3: "message contracts"
  Request: "Document the message contracts for events on the notification queue."
  Trigger term: "contracts" (in "message contracts")
  Fires: API_CONTRACT + DOCUMENT
  Modifier "message" does not suppress

Example 4: "event contracts"
  Request: "Define the event contracts for customer lifecycle events."
  Trigger term: "contracts" (in "event contracts")
  Fires: API_CONTRACT + DOCUMENT
  Modifier "event" does not suppress

Example 5: "webhook contract"
  Request: "Specify the webhook contract for order status notifications."
  Trigger term: "contract" (in "webhook contract")
  Fires: API_CONTRACT + DOCUMENT
  Modifier "webhook" does not suppress (and also independently fires API_CONTRACT)

---

## B12-0C-002-002 — Additional contract modifier examples: all fire API_CONTRACT

```
source_id: xiigen-b12-0c-002-002
rule_id: B12-RULE-CONTRACT-ALWAYS
pair_kind: knowledge_edge
```

Extended modifier list — all fire API_CONTRACT:

| Phrase | API_CONTRACT fires? | Note |
|--------|---------------------|------|
| "data contract" | YES | Core Rule 2 |
| "service contract" | YES | Core Rule 2 |
| "message contract" | YES | Core Rule 2 |
| "event contract" | YES | Core Rule 2 |
| "webhook contract" | YES | Core Rule 2 + "webhook" also fires |
| "API contract" | YES | Core Rule 2 + "api" also fires |
| "REST contract" | YES | Core Rule 2 + "rest" also fires |
| "gRPC contract" | YES | Core Rule 2 + "grpc" also fires |
| "GraphQL contract" | YES | Core Rule 2 + "graphql" also fires |
| "integration contract" | YES | Core Rule 2 |
| "interface contract" | YES | Core Rule 2 + "interface" also fires |
| "versioning contract" | YES | Core Rule 2 + "versioning" also fires |
| "schema contract" | YES | Core Rule 2 (schema co-occurrence with "contract" = API context) |
| "protocol contract" | YES | Core Rule 2 |
| "response contract" | YES | Core Rule 2 + "response" also fires |

### What does NOT fire API_CONTRACT (no "contract" present)

| Phrase | API_CONTRACT fires? | Note |
|--------|---------------------|------|
| "agreement" | NO | "agreement" is not in the API_CONTRACT lexicon |
| "obligation" | NO | not in lexicon |
| "commitment" | NO | not in lexicon |
| "database schema" | NO | "schema" without API co-occurrence |
| "SLA" | NO | service-level agreement, not API contract |

---

## B12-0C-002-003 — v6-01 corruption target: the suppression error

```
source_id: xiigen-b12-0c-002-003
rule_id: B12-RULE-CONTRACT-ALWAYS
corruption_type: v6-01
pair_kind: knowledge_edge
```

v6-01 corruption occurs when the model treats the modifier as a domain signal that overrides
the "contract" trigger:

Corrupted reasoning: "data contracts" → "data" signals data modeling domain → API_CONTRACT
should be suppressed because this is about data, not APIs.

Why this reasoning is wrong:
  1. "contract" is a universal API_CONTRACT trigger in B12
  2. Data contracts ARE an API concept — they define the interface between services
  3. The "data" in "data contracts" refers to the payload data exchanged over an API
  4. Core Rule 2 has no modifier exception list

Correct reasoning: "data contracts" → "contract" present → API_CONTRACT fires.
Check: does the phrase contain "contract"? Yes → API_CONTRACT fires. End of check.
