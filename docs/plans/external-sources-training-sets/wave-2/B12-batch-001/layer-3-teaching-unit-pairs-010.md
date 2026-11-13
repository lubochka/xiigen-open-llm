# Layer 3 — Teaching Unit Pairs (010)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B12-TU-010 — Teaching: v6 corruption patterns for B12 with examples

```
source_id: xiigen-b12-tu-010
rule_id: B12-RULE-V6-CORRUPTIONS
pair_kind: teaching_unit
```

### Three v6 Corruptions in B12

B12 has three documented v6 corruption patterns. Each corrupts a specific rule.

---

### v6-01: API_CONTRACT suppressed on "contract" with modifier

Rule violated: "contract" → API_CONTRACT regardless of modifier (Core Rule 2)

Trigger scenario: request contains "data contracts", "service contracts", etc.

Before (correct):
  A: "Define the data contracts between the order service and the fulfillment service."
  B: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
     Routing: 3.3 + 2.1

After v6-01 corruption:
  A: same request
  B: CompiledPredicateIds: ["DOCUMENT"]  ← API_CONTRACT missing
     Routing: 2.1 only  ← 3.3 missing

Impact: API contract design phase (3.3) is skipped entirely.
Fix: treat "contract" as unconditional API_CONTRACT trigger regardless of preceding noun.

Detection: request has "contract" + CompiledPredicateIds lacks "API_CONTRACT"

---

### v6-02: SECURITY suppressed for "authentication" in API doc context

Rule violated: "authentication" → SECURITY ALWAYS in any context (Core Rule 6)

Trigger scenario: request asks for API documentation with authentication requirements.

Before (correct):
  A: "Write the OpenAPI spec for the cart service and include authentication requirements."
  B: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY"]
     Routing: 4.3 + 3.3 + 2.1 + 3.4

After v6-02 corruption:
  A: same request
  B: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]  ← SECURITY missing
     Routing: 4.3 + 3.3 + 2.1  ← 3.4 missing

Impact: Security review phase (3.4) is skipped. Authentication requirements not reviewed.
Fix: "authentication" always fires SECURITY regardless of surrounding API vocabulary.

Detection: request has "authentication" + CompiledPredicateIds lacks "SECURITY"

---

### v6-03: Multiple routing decisions collapsed to single 3.3 decision

Rule violated: DecisionCount = predicate count (Core Rule 23); each predicate routes independently

Trigger scenario: request with multiple predicates (N>1), especially complex requests with
IMPLEMENT + API_CONTRACT + DOCUMENT + SECURITY + AUTHORIZATION.

Before (correct):
  A: "Write the OpenAPI specification for the authentication service, implement OAuth 2.0 token endpoint, document security."
  B: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY", "AUTHORIZATION"]
     RoutingDecisions: [4.3, 3.3, 2.1, 3.4, 3.4]  (five decisions)
     DecisionCount: 5

After v6-03 corruption:
  A: same request
  B: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY", "AUTHORIZATION"]  (correct!)
     RoutingDecisions: [3.3]  ← collapsed to single decision  ← WRONG
     DecisionCount: 1  ← WRONG (should be 5)

Impact: Only API contract flow runs. Implementation, documentation, security, authorization all skipped.
Fix: each predicate in CompiledPredicateIds generates its own routing decision independently.

Detection: CompiledPredicateIds has N>1 entries but RoutingDecisions has fewer than N entries.

---

### Summary Table

| v6 Corruption | Corrupted Rule | Symptom | Detection |
|---------------|---------------|---------|-----------|
| v6-01 | "contract" always fires API_CONTRACT | API_CONTRACT missing when modifier present | "contract" in request, no API_CONTRACT in predicates |
| v6-02 | "authentication" always fires SECURITY | SECURITY missing in API doc context | "authentication" in request, no SECURITY in predicates |
| v6-03 | Each predicate routes independently | RoutingDecisions collapsed to 3.3 only | N predicates, <N routing decisions |
