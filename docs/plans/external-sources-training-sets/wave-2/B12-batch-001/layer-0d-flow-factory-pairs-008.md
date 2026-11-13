# Layer 0-D — Flow Factory Pairs (008)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B12-0D-008-001 — API_CONTRACT + DOCUMENT + SECURITY factory: three flows

```
source_id: xiigen-b12-0d-008-001
flow_type: deterministic_generic
transition_type: documentation
phase: 3
sub_phase: 3.3
pair_kind: factory_selection
```

A:
  run_id: "B12-0D-008-001"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  sub_phase_hint: "3.3"
  request: "Document the REST API endpoints including authentication requirements."

B:
  FlowFactoryResult:
    FlowType: "deterministic_generic"
    TransitionType: "documentation"
    TotalFlows: 3
    PrimarySubPhase: "3.3"
    SelectedFlows:
      - { FlowId: "APIContractFlow33", SubPhase: "3.3", Predicate: "API_CONTRACT", Order: 1 }
      - { FlowId: "DocumentationFlow21", SubPhase: "2.1", Predicate: "DOCUMENT", Order: 2 }
      - { FlowId: "SecurityAuditFlow34", SubPhase: "3.4", Predicate: "SECURITY", Order: 3 }
    FlowOrder: ["APIContractFlow33", "DocumentationFlow21", "SecurityAuditFlow34"]
    ParallelEligible: ["DocumentationFlow21", "SecurityAuditFlow34"]
    FactoryReason: "API_CONTRACT primary. DOCUMENT secondary. SECURITY adds 3.4 review.
    No IMPLEMENT → documentation TransitionType. Security review can run parallel with documentation."

Note on parallel eligibility: DocumentationFlow21 and SecurityAuditFlow34 can run in parallel
because they have no dependency between them. Both consume the APIContractFlow33 output.
APIContractFlow33 must complete first before the other two can begin.

---

## B12-0D-008-002 — SecurityAuditFlow34 in B12 API contract context

```
source_id: xiigen-b12-0d-008-002
flow_type: deterministic_generic
pair_kind: factory_selection
```

SecurityAuditFlow34 handles in B12 context:
  - Authentication mechanism review (API key, Bearer token, OAuth)
  - Token security assessment (token format, expiration, rotation)
  - Security header requirements (Authorization header, X-API-Key)
  - HTTPS enforcement requirements
  - Rate limiting and throttling documentation

SecurityAuditFlow34 input (from APIContractFlow33):
  - API endpoint list
  - Authentication requirements per endpoint
  - Token specifications

SecurityAuditFlow34 output:
  - Security assessment of the API contract
  - Authentication documentation additions
  - Security requirements checklist

---

## B12-0D-008-003 — API_CONTRACT + DOCUMENT + SECURITY vs API_CONTRACT + DOCUMENT + AUTHORIZATION

```
source_id: xiigen-b12-0d-008-003
flow_type: deterministic_generic
pair_kind: factory_selection
```

Two similar three-predicate patterns with different flow compositions:

Pattern A: [API_CONTRACT, DOCUMENT, SECURITY]
  Trigger: "authentication" or "token" in request
  Flow: APIContractFlow33 + DocumentationFlow21 + SecurityAuditFlow34
  SubPhases: 3.3 + 2.1 + 3.4
  Focus: who can authenticate (identity verification)

Pattern B: [API_CONTRACT, DOCUMENT, AUTHORIZATION]
  Trigger: "access control" or "authorization" in request (not "authentication")
  Flow: APIContractFlow33 + DocumentationFlow21 + AuthorizationFlow34
  SubPhases: 3.3 + 2.1 + 3.4
  Focus: what actions are permitted (permission verification)

Pattern C: [API_CONTRACT, DOCUMENT, SECURITY, AUTHORIZATION]
  Trigger: both "authentication"/"token" AND "access control"/"authorization"/"oauth"
  Flow: APIContractFlow33 + DocumentationFlow21 + SecurityAuditFlow34 + AuthorizationFlow34
  SubPhases: 3.3 + 2.1 + 3.4 + 3.4
  Note: Two routing decisions to 3.4 (DecisionCount = 4)

---

## B12-0D-008-004 — Factory output for [API_CONTRACT, DOCUMENT, SECURITY] complete

```
source_id: xiigen-b12-0d-008-004
flow_type: deterministic_generic
pair_kind: factory_selection
```

Full factory output structure for [API_CONTRACT, DOCUMENT, SECURITY]:

Input:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  DomainId: "software-development"
  TransitionType: "documentation"

Output:
  FlowFactoryResult:
    FlowType: "deterministic_generic"
    DomainId: "software-development"
    TransitionType: "documentation"
    PrimarySubPhase: "3.3"
    DecisionCount: 3
    FlowCount: 3
    SelectedFlows:
      1. APIContractFlow33 (3.3) — primary flow for B12
      2. DocumentationFlow21 (2.1) — documentation sub-phase
      3. SecurityAuditFlow34 (3.4) — security review
    FlowOrder: ["APIContractFlow33", "DocumentationFlow21", "SecurityAuditFlow34"]
    ParallelGroups:
      Sequential: [APIContractFlow33]
      Parallel: [DocumentationFlow21, SecurityAuditFlow34]
    QAPosition: "after all flows" (QA is always last — Core Rule context)
