# Layer 2 — Nine-Node Flow Pairs (sub-phase 3.3)
# Batch: B12-batch-001
# Scenario: B12 — Development — API contract documentation
# Flow: ApiContractDesignFlow33
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: nine_node

---

## N01 — DomainSplitterUnit

```
source_id: xiigen-b12-n01
contract_signature: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
node: 01
```

A:
  TransitionId: "transition-B12-t1-001-3.3-feature_addition"
  TransitionType: "feature_addition"
  PhaseId: "3"
  SubPhaseId: "3.3"
  UserRequest: "Document the REST API endpoints for the order service including request/response schemas and authentication requirements."
  PredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]

B:
  DomainPackets:
    - PacketId: "pkt-B12-001-api"
      Domain: "API_CONTRACT"
      Focus: "REST API endpoint contract specification"
      InputSummary: "Order service REST API: endpoints, request/response schemas, auth requirements"
      PredicateOwner: "API_CONTRACT"
  PacketCount: 1

Note: API_CONTRACT is the primary predicate for this 3.3 flow execution. DOCUMENT and
SECURITY route to their own sub-phase executions (2.4 and 3.4 respectively).

---

## N02 — DomainTranslatorUnit

```
source_id: xiigen-b12-n02
contract_signature: DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>
node: 02
```

A: [API_CONTRACT DomainPacket from N01]

B:
  StatementId: "stmt-B12-001-api"
  DomainStatement: "Define REST API contract for OrderService v2. Endpoints to document:
    POST /orders (create), GET /orders/{id} (retrieve), PUT /orders/{id}/status (update
    status), DELETE /orders/{id} (cancel). Each endpoint: HTTP method, URL pattern, path
    parameters, query parameters, request body schema (JSON), response schemas (200/400/401/404/500),
    authentication: Bearer JWT required on all endpoints, rate limits per endpoint.
    Format: OpenAPI 3.0 YAML."
  SourcePacketId: "pkt-B12-001-api"
  TranslationConfidence: 0.92

---

## N03 — SchemePlannerUnit

```
source_id: xiigen-b12-n03
contract_signature: SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>
node: 03
```

A: [TranslatedDomainStatement from N02]

B:
  WorkBranches:
    - BranchId: "branch-B12-create-order-endpoint"
      BranchName: "POST /orders — Create Order"
      Priority: 1

    - BranchId: "branch-B12-get-order-endpoint"
      BranchName: "GET /orders/{id} — Retrieve Order"
      Priority: 2

    - BranchId: "branch-B12-update-status-endpoint"
      BranchName: "PUT /orders/{id}/status — Update Status"
      Priority: 3

    - BranchId: "branch-B12-cancel-endpoint"
      BranchName: "DELETE /orders/{id} — Cancel Order"
      Priority: 4

  BranchCount: 4

---

## N04 — DomainProcessorUnit

```
source_id: xiigen-b12-n04
contract_signature: DomainProcessorUnit : ILogicalUnit<WorkBranch, ProcessedBranchResult>
node: 04
```

A: [POST /orders WorkBranch]
  BranchId: "branch-B12-create-order-endpoint"

B:
  BranchId: "branch-B12-create-order-endpoint"
  ProcessedItems:
    - ItemId: "api-B12-E01"
      Endpoint: "POST /api/v2/orders"
      RequestBody:
        ContentType: "application/json"
        Schema: "CreateOrderRequest { customerId: string(uuid), lineItems: LineItem[], deliveryAddress: Address }"
        Required: ["customerId", "lineItems"]
      Responses:
        "201": "OrderCreatedResponse { orderId: string(uuid), status: 'Pending', createdAt: datetime }"
        "400": "ErrorResponse { code: 'ORDER_INVALID', details: string[] }"
        "401": "ErrorResponse { code: 'UNAUTHORIZED' }"
        "409": "OrderCreatedResponse (existing order, idempotent)"
      Auth: "Bearer JWT — scope: orders:write"
      RateLimit: "100 req/min per client"

    - ItemId: "api-B12-E01-schema"
      SchemaDefinitions:
        LineItem: "{ productId: string(uuid), quantity: integer(min:1), unitPrice: decimal }"
        Address: "{ street: string, city: string, postalCode: string, countryCode: string(ISO-3166-1-alpha-2) }"

  OverallBranchStatus: "PROCESSED"

---

## N05 — ApplierUnit

```
source_id: xiigen-b12-n05
contract_signature: ApplierUnit : ILogicalUnit<ProcessedBranchResult, AppliedBranchResult>
node: 05
```

A: [ProcessedBranchResult from N04]
  BranchId: "branch-B12-create-order-endpoint"

B:
  BranchId: "branch-B12-create-order-endpoint"
  AppliedItems:
    - ActionId: "apply-B12-A01"
      Action: "Write OpenAPI YAML for POST /orders endpoint with full request/response schemas"
      Result: "Written to openapi/paths/orders.yaml — POST block"

    - ActionId: "apply-B12-A02"
      Action: "Register LineItem and Address component schemas in openapi/components/schemas.yaml"
      Result: "Schemas registered"

    - ActionId: "apply-B12-A03"
      Action: "Add security scheme reference (BearerAuth) to POST /orders"
      Result: "Security reference added"

  AppliedCount: 3
  BranchStatus: "APPLIED"

---

## N06 — ResultTranslatorUnit

```
source_id: xiigen-b12-n06
contract_signature: ResultTranslatorUnit : ILogicalUnit<AppliedBranchResult[], TranslatedResultFragment[]>
node: 06
```

A: [All 4 AppliedBranchResults]

B:
  ResultFragments:
    - FragmentId: "frag-B12-api-contract-summary"
      FragmentType: "api_contract_summary"
      Content:
        spec_format: "OpenAPI 3.0 YAML"
        endpoints_documented: 4
        schemas_defined: 6
        security_scheme: "BearerAuth (JWT)"
        response_codes_covered: ["200", "201", "400", "401", "404", "409", "500"]
        total_operations: 4

    - FragmentId: "frag-B12-contract-quality"
      FragmentType: "contract_quality_assessment"
      Content:
        all_endpoints_have_auth: true
        all_endpoints_have_error_responses: true
        schemas_have_types_and_constraints: true
        idempotency_documented: true

  FragmentCount: 2

---

## N07 — ArbiterSelectorUnit

```
source_id: xiigen-b12-n07
contract_signature: ArbiterSelectorUnit : ILogicalUnit<TranslatedResultFragment[], ArbiterReviewRequest[]>
node: 07
```

A: [2 ResultFragments]

B:
  ArbiterRequests:
    - RequestId: "arb-B12-001"
      ArbiterType: "api-contract-completeness-arbiter"
      FragmentId: "frag-B12-api-contract-summary"
      ReviewFocus: "Verify all endpoints documented, all HTTP methods covered, all response codes present"

    - RequestId: "arb-B12-002"
      ArbiterType: "api-contract-quality-arbiter"
      FragmentId: "frag-B12-contract-quality"
      ReviewFocus: "Verify auth, error responses, type constraints, and idempotency are all documented"

  RequestCount: 2

---

## N08 — ArbiterUnit

```
source_id: xiigen-b12-n08
contract_signature: ArbiterUnit : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>
node: 08
```

A: [api-contract-completeness-arbiter request]
  RequestId: "arb-B12-001"

B:
  RequestId: "arb-B12-001"
  Verdict: "PASS"
  Confidence: 0.96
  Findings:
    - "endpoints_documented: 4 of 4 required — PASS"
    - "all HTTP methods present (POST, GET, PUT, DELETE) — PASS"
    - "response codes: 200, 201, 400, 401, 404, 409, 500 — PASS"
    - "schemas: 6 defined with types and constraints — PASS"
  VerdictReason: "API contract is complete: all 4 endpoints documented with full schema, auth, and error coverage."

---

## N09 — FormatterUnit

```
source_id: xiigen-b12-n09
contract_signature: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
node: 09
```

A:
  ReviewedResultSet:
    OverallVerdict: "PASS"

B:
  HumanResult:
    Title: "OrderService REST API Contract — OpenAPI 3.0 Specification"
    OverallVerdict: "PASS"
    Summary: "4 REST endpoints fully documented in OpenAPI 3.0 YAML format. All endpoints have request/response schemas, Bearer JWT authentication, rate limits, and error responses."
    EndpointsDocumented:
      - "POST /api/v2/orders — Create order (201, 400, 401, 409)"
      - "GET /api/v2/orders/{id} — Retrieve order (200, 401, 404)"
      - "PUT /api/v2/orders/{id}/status — Update status (200, 400, 401, 404)"
      - "DELETE /api/v2/orders/{id} — Cancel order (200, 401, 404, 409)"
    SchemasGenerated: ["CreateOrderRequest", "OrderCreatedResponse", "UpdateStatusRequest", "OrderResponse", "LineItem", "Address"]
  Metrics:
    node_01_unit_id: "DomainSplitterUnit"
    node_02_unit_id: "DomainTranslatorUnit"
    node_03_unit_id: "SchemePlannerUnit"
    node_04_unit_id: "DomainProcessorUnit"
    node_05_unit_id: "ApplierUnit"
    node_06_unit_id: "ResultTranslatorUnit"
    node_07_unit_id: "ArbiterSelectorUnit"
    node_08_unit_id: "ArbiterUnit"
    node_09_unit_id: "FormatterUnit"
    endpoints_documented: 4
    schemas_defined: 6
    sub_phase: "3.3"
    flow_id: "ApiContractDesignFlow33"
