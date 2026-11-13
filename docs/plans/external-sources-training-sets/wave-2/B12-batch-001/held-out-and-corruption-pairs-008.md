# Held-Out and Corruption Pairs (008)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B12-HO-008 — Held-out: webhook + document → API_CONTRACT + DOCUMENT (not IMPLEMENT)

```
source_id: xiigen-b12-ho-008
pair_kind: held_out
hold_out_reason: "Tests that webhook documentation fires API_CONTRACT not IMPLEMENT"
```

A:
  user_request: "Document the webhook delivery mechanism for payment events and specify the signature verification requirements."

B (expected):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  TransitionType: "documentation"
  Routing: ["3.3", "2.1", "3.4"]
  Reason:
    "webhook" → API_CONTRACT fires
    "document" → DOCUMENT fires
    "specify" → DOCUMENT fires
    "signature verification" → "verification" has security context → SECURITY fires
    (alternatively: if "signature" alone does not fire SECURITY, then only API_CONTRACT + DOCUMENT)
    IMPLEMENT does NOT fire

Note: "signature verification" is borderline. If the classifier does not classify signature
as a SECURITY trigger (it's not in the core list), then:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Either result is acceptable for this held-out test; the key is IMPLEMENT does NOT fire.

Pass condition: CompiledPredicateIds does NOT contain "IMPLEMENT"
Core test: "document the webhook" → API_CONTRACT not IMPLEMENT

---

## B12-COR-008 — Corruption: webhook fires IMPLEMENT instead of API_CONTRACT (wrong)

```
source_id: xiigen-b12-cor-008
corruption_type: webhook_misclassified_as_implement
pair_kind: corruption
```

A: same request as B12-HO-008

B (corrupt):
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT"]  ← API_CONTRACT missing, IMPLEMENT wrongly added
  TransitionType: "feature_addition"  ← WRONG
  CorruptReason: "Webhook delivery mechanism classified as implementation task: classifier
  treated 'webhook delivery mechanism' as building a delivery system → IMPLEMENT fires"

Corruption label: WEBHOOK → IMPLEMENT (wrong — webhook fires API_CONTRACT)
Expected correct: ["API_CONTRACT", "DOCUMENT"] (or ["API_CONTRACT", "DOCUMENT", "SECURITY"])
Corrupt output: ["IMPLEMENT", "DOCUMENT"]

Teaching: webhook is an API delivery mechanism (API contract term = API_CONTRACT).
Documenting a webhook contract = API contract documentation = Phase 3.3.
"Document the webhook" does NOT fire IMPLEMENT.
IMPLEMENT only fires for "implement the webhook handler", "build the webhook receiver", etc.
The webhook URL and payload are API contract definitions, not code implementations.

Also note Core Rule 17: "integrate" → API_CONTRACT (not IMPLEMENT).
Similarly: "document the webhook" → API_CONTRACT (not IMPLEMENT).
