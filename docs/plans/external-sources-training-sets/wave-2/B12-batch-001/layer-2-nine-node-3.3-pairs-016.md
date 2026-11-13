# Layer 2 — Nine-Node 3.3 Pairs (016)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: nine_node

---

## B12-L2-016 — ShippingRatesRestApi: API_CONTRACT+IMPLEMENT (feature_addition)

```
source_id: xiigen-b12-l2-016
flow_name: ShippingRatesRestApi
transition_type: feature_addition
phase: 3
sub_phase: 3.3
pair_kind: nine_node
gate_b: PASS
```

### Input (N01)

N01 classification:
  user_request: "Write the Swagger documentation for the shipping rates API with request and response examples."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  TransitionType: "feature_addition"
  DomainId: "software-development"
  FlowId: "ShippingRatesRestApi"
  SubPhase: "3.3"
  SelectedLogicalTransition: A = SelectedLogicalTransition
  TriggerTerms:
    IMPLEMENT: "write"
    API_CONTRACT: "swagger", "api", "request", "response"
    DOCUMENT: "documentation"

### Processing (N02-N08)

N02 — DomainTranslatorUnit:
  Domain: shipping rate calculation, carrier integration
  API style: REST (Swagger/OpenAPI)
  Operations: rate quote, carrier comparison, rate confirmation

N03 — SchemePlannerUnit:
  WorkBranches:
    - branch-001: POST /rates/quote — calculate shipping rates
    - branch-002: GET /rates/carriers — list available carriers
    - branch-003: POST /rates/confirm — confirm selected rate

N04 — DomainProcessorUnit:
  POST /rates/quote:
    Request: { originZip, destZip, weight, dimensions, serviceLevel }
    Response: { quotes: [{ carrierId, serviceName, price, currency, estimatedDays }] }
    Auth: API key (X-API-Key header)
  GET /rates/carriers:
    Response: { carriers: [{ carrierId, name, services: [string] }] }
  POST /rates/confirm:
    Request: { quoteId, carrierId }
    Response: { confirmationId, carrierId, price, validUntil }

N05 — ApplierUnit:
  Apply: shipping-rates-swagger.yaml (Swagger 2.0 spec with examples)
  Apply: example request/response objects for each endpoint

N06 — ResultTranslatorUnit:
  endpoints_documented: 3
  examples_included: YES
  auth_type: API_KEY

N07 — ArbiterSelectorUnit:
  Request: api-contract-completeness-arbiter + api-contract-quality-arbiter

N08 — ArbiterUnit:
  Verdict: PASS
  Findings: "3 endpoints with examples; auth documented; rate quote → confirm flow complete"

### Output (N09)

N09 — FormatterUnit:
  FlowStatus: "PASS"
  Artifacts:
    1. shipping-rates-swagger.yaml (Swagger 2.0 specification with examples)
    2. shipping-rates-api-guide.md (from DocumentationFlow21)
    3. shipping-rates-integration-examples.md
  GateB: PASS — "shipping-rates-swagger.yaml contains paths section and example objects"
  TransitionType: "feature_addition" (new Swagger file created via IMPLEMENT/"write")
