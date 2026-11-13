# Layer 2 — Nine-Node 3.3 Pairs (020)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: nine_node

---

## B12-L2-020 — PartnerApiContract: API_CONTRACT+SECURITY (authentication in partner contract)

```
source_id: xiigen-b12-l2-020
flow_name: PartnerApiContract
transition_type: documentation
phase: 3
sub_phase: 3.3
pair_kind: nine_node
gate_b: PASS
```

### Input (N01)

N01 classification:
  user_request: "Document the partner API authentication and API key management contract for external integration."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  TransitionType: "documentation"
  DomainId: "software-development"
  FlowId: "PartnerApiContract"
  SubPhase: "3.3"
  SelectedLogicalTransition: A = SelectedLogicalTransition
  TriggerTerms:
    API_CONTRACT: "API", "contract"
    DOCUMENT: "document"
    SECURITY: "authentication"
  Note: SECURITY routes separately to SecurityAuditFlow34 (3.4). N01 input here is for
  the 3.3 (API_CONTRACT) flow execution only.

### Processing (N02-N08)

N02 — DomainTranslatorUnit:
  Domain: partner/external integration API management
  Auth mechanism: API key (X-API-Key header) + optional OAuth 2.0 client credentials

N03 — SchemePlannerUnit:
  WorkBranches:
    - branch-001: API key authentication contract
    - branch-002: OAuth 2.0 client credentials flow for partners
    - branch-003: rate limiting and quota contract

N04 — DomainProcessorUnit:
  API key contract:
    Header: X-API-Key: {key}
    Key format: 32-character hex string
    Key rotation: 90-day TTL, /api-keys/rotate endpoint
  OAuth contract:
    Token endpoint: POST /oauth/token (client_credentials grant)
    Scope: partner:read, partner:write, partner:events
    Token TTL: 3600 seconds
  Rate limits: 1000 req/hour per API key, 429 response with Retry-After header

N05 — ApplierUnit:
  Apply: partner-api-contract.yaml (auth schemes + rate limit spec)
  Apply: OAuth client credentials flow diagram

N06 — ResultTranslatorUnit:
  auth_methods: 2 (API key, OAuth 2.0)
  rate_limit_documented: YES
  key_rotation_documented: YES

N07 — ArbiterSelectorUnit:
  Request: api-contract-completeness-arbiter

N08 — ArbiterUnit:
  Verdict: PASS
  Findings: "2 auth methods documented; rate limits specified; key rotation contract included"

### Output (N09)

N09 — FormatterUnit:
  FlowStatus: "PASS"
  Artifacts:
    1. partner-api-contract.yaml (API key + OAuth + rate limit contract)
    2. partner-integration-guide.md (from DocumentationFlow21)
    3. partner-security-requirements.md (from SecurityAuditFlow34 — 3.4 flow output)
  GateB: PASS — "partner-api-contract.yaml includes securitySchemes and rate limit definitions"
  TransitionType: "documentation"
