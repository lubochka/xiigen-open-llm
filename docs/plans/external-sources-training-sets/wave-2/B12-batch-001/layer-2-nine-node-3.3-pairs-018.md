# Layer 2 — Nine-Node 3.3 Pairs (018)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: nine_node

---

## B12-L2-018 — ReportingApiSpec: API_CONTRACT+DOCUMENT

```
source_id: xiigen-b12-l2-018
flow_name: ReportingApiSpec
transition_type: documentation
phase: 3
sub_phase: 3.3
pair_kind: nine_node
gate_b: PASS
```

### Input (N01)

N01 classification:
  user_request: "Define the OpenAPI spec for the reporting API including query parameters and CSV response schema."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  DomainId: "software-development"
  FlowId: "ReportingApiSpec"
  SubPhase: "3.3"
  SelectedLogicalTransition: A = SelectedLogicalTransition
  TriggerTerms:
    API_CONTRACT: "OpenAPI", "api", "response", "schema"
    DOCUMENT: "define", "spec"

### Processing (N02-N08)

N02 — DomainTranslatorUnit:
  Domain: business intelligence, reporting
  API style: REST (OpenAPI)
  Response formats: JSON, CSV

N03 — SchemePlannerUnit:
  WorkBranches:
    - branch-001: GET /reports/{reportType} — generate report
    - branch-002: GET /reports/scheduled — list scheduled reports
    - branch-003: POST /reports/export — async CSV export

N04 — DomainProcessorUnit:
  GET /reports/{reportType}:
    Query params: startDate (ISO8601), endDate (ISO8601), granularity (daily|weekly|monthly), format (json|csv)
    Response JSON: { reportType, period, rows: [ReportRow], generatedAt }
    Response CSV: text/csv with header row
  POST /reports/export:
    Request: { reportType, startDate, endDate, email }
    Response 202: { jobId, estimatedReadyAt }
    (async delivery via email)

N05 — ApplierUnit:
  Apply: reporting-openapi.yaml (multiple response content types: application/json + text/csv)
  Apply: ReportRow schema definition

N06 — ResultTranslatorUnit:
  endpoints_documented: 3
  response_formats: 2 (JSON, CSV)
  async_export: documented with 202 + jobId pattern

N07 — ArbiterSelectorUnit:
  Request: api-contract-completeness-arbiter

N08 — ArbiterUnit:
  Verdict: PASS
  Findings: "3 endpoints; multiple response formats documented; async export pattern complete"

### Output (N09)

N09 — FormatterUnit:
  FlowStatus: "PASS"
  Artifacts:
    1. reporting-openapi.yaml (OpenAPI 3.0 with multi-format responses)
    2. reporting-api-developer-guide.md (from DocumentationFlow21)
  GateB: PASS — "reporting-openapi.yaml includes CSV response content-type definition"
  TransitionType: "documentation"
