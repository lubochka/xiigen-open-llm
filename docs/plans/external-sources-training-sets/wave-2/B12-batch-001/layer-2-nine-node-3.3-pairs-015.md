# Layer 2 — Nine-Node 3.3 Pairs (015)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: nine_node

---

## B12-L2-015 — UserManagementApiContract: API_CONTRACT alone (documentation)

```
source_id: xiigen-b12-l2-015
flow_name: UserManagementApiContract
transition_type: documentation
phase: 3
sub_phase: 3.3
pair_kind: nine_node
gate_b: PASS
```

### Input (N01)

N01 classification:
  user_request: "Specify the user management API contract including create, update, and deactivate operations."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  DomainId: "software-development"
  FlowId: "UserManagementApiContract"
  SubPhase: "3.3"
  SelectedLogicalTransition: A = SelectedLogicalTransition
  TriggerTerms:
    API_CONTRACT: "API", "contract"
    DOCUMENT: "specify"

### Processing (N02-N08)

N02 — DomainTranslatorUnit:
  Domain: user account management
  API style: REST
  Operations: user creation, profile update, account deactivation

N03 — SchemePlannerUnit:
  WorkBranches:
    - branch-001: POST /users — create user
    - branch-002: PUT /users/{id} — update user profile
    - branch-003: DELETE /users/{id} — deactivate user

N04 — DomainProcessorUnit:
  POST /users:
    Request: { email, displayName, role, organizationId }
    Response 201: { userId, email, displayName, role, createdAt }
    Response 409: { code: "USER_ALREADY_EXISTS", email }
  PUT /users/{id}:
    Request: { displayName, role, preferences }
    Response 200: { userId, displayName, role, updatedAt }
  DELETE /users/{id}:
    Response 200: { userId, status: "deactivated", deactivatedAt }
    Note: Soft delete — user data retained, access revoked

N05 — ApplierUnit:
  Apply: user-management-openapi.yaml (three endpoints)
  Apply: schemas: CreateUserRequest, UserResponse, UpdateUserRequest

N06 — ResultTranslatorUnit:
  endpoints_documented: 3
  schemas_defined: 3
  delete_behavior: soft_delete (documented)

N07 — ArbiterSelectorUnit:
  Request: api-contract-completeness-arbiter

N08 — ArbiterUnit:
  Verdict: PASS
  Findings: "3 CRUD endpoints documented; soft-delete behavior specified; schemas complete"

### Output (N09)

N09 — FormatterUnit:
  FlowStatus: "PASS"
  Artifacts:
    1. user-management-openapi.yaml (OpenAPI 3.0 with CRUD operations)
    2. user-management-api-guide.md (from DocumentationFlow21)
  GateB: PASS — "user-management-openapi.yaml contains POST, PUT, DELETE paths with schemas"
  TransitionType: "documentation"
