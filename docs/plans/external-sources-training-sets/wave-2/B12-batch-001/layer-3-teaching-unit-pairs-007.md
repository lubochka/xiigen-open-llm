# Layer 3 — Teaching Unit Pairs (007)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B12-TU-007 — Teaching: TransitionType for API work — feature_addition vs documentation

```
source_id: xiigen-b12-tu-007
rule_id: B12-RULE-TRANSITIONTYPE-API
pair_kind: teaching_unit
```

### Concept

TransitionType determines how the orchestrator treats the change:
  feature_addition: creating something NEW (new endpoint, new spec file, new service)
  documentation: describing or updating something EXISTING

The primary determination rule: does IMPLEMENT fire?
  YES → feature_addition
  NO → documentation (default for B12)

### The IMPLEMENT Trigger Vocabulary

These verbs fire IMPLEMENT:
  write — "write the OpenAPI spec" (creates a new spec artifact)
  implement — "implement the token endpoint" (creates new code)
  build — "build the webhook handler" (creates new code)
  create — "create the API endpoint" (creates new code/artifact)
  develop — "develop the payment integration" (creates new code)

These verbs do NOT fire IMPLEMENT:
  document — documentation verb → DOCUMENT, not IMPLEMENT
  define — specification verb → DOCUMENT, not IMPLEMENT
  specify — specification verb → DOCUMENT, not IMPLEMENT
  describe — description verb → DOCUMENT, not IMPLEMENT
  annotate — annotation verb → DOCUMENT, not IMPLEMENT
  design — design/planning verb → ARCHITECTURE, not IMPLEMENT
  integrate — API boundary verb → API_CONTRACT, not IMPLEMENT (Rule 17)

### Training Pairs

documentation (no IMPLEMENT):
  A: "Define the gRPC service contract for the user service."
  B: TransitionType: "documentation"
     Reason: "define" → DOCUMENT only; IMPLEMENT does not fire

  A: "Specify the webhook payload contract for order events."
  B: TransitionType: "documentation"
     Reason: "specify" → DOCUMENT only

  A: "Document the REST API versioning strategy."
  B: TransitionType: "documentation"
     Reason: "document" → DOCUMENT only

feature_addition (IMPLEMENT fires):
  A: "Write the OpenAPI specification for the payment gateway."
  B: TransitionType: "feature_addition"
     Reason: "write" → IMPLEMENT fires; new spec artifact being created

  A: "Implement the gRPC contract for the shipping service."
  B: TransitionType: "feature_addition"
     Reason: "implement" → IMPLEMENT fires

  A: "Build the webhook endpoint for order notifications."
  B: TransitionType: "feature_addition"
     Reason: "build" → IMPLEMENT fires

### Special Case: "integrate" (Core Rule 17)

  A: "Integrate the payment gateway webhook."
  B: TransitionType: "documentation"
     Reason: "integrate" → API_CONTRACT, NOT IMPLEMENT
     "Integrate" describes using an API boundary, not building new code.

  Contrast:
  A: "Implement the payment gateway webhook integration."
  B: TransitionType: "feature_addition"
     Reason: "implement" explicitly fires IMPLEMENT

### Memory Aid

Before labeling feature_addition: confirm "write/implement/build/create/develop" is present.
If only "define/specify/document/describe/annotate/design/integrate" → documentation.
feature_addition requires explicit construction verb. API context alone does not determine TransitionType.
