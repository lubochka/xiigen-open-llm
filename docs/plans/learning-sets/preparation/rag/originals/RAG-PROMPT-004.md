# Node 02 — Domain Translator Pairs

```
Contract:
  DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>
```

## Common coverage labels (apply to every pair in this file)

```
source_license_state: external_license_checked
contract_signature:   DomainTranslatorUnit
node_role:            node_02
pair_kind:            flow_node
coverage_status:      covered
attribution_required: false
```

Per-pair labels carry: `source_id`, `source_title`, `transition_type`,
`phase`, `sub_phase`, `task_type`, `rejected_type`, `overlap_ratio`.

Token-overlap gate: every rejected satisfies overlap_ratio < 0.60.

Node 02 is an array-node — A is **exactly one** DomainPacket, never the
whole array. B is the formal translated domain statement for that packet,
including:

```
control_type, authority_source, required_evidence,
conflict_status, knowledge_edge_status, handoff_marker
```

Five pairs in this file — one per chosen packet from Node 01 that
exercises a different formal-statement vocabulary.

---

## Pair[ID] ES-2026-05-08-3.1-N02-01 — ES-S1 ARCH/3.1 packet → boundary statement

A:
```
DomainPacket:
  packet_id:       "ES-S1-pkt-001"
  domain_label:    "ARCH"
  phase:           "3"
  sub_phase:       "3.1"
  task_type:       "architecture-topology"
  prompt_fragment: "define the component boundaries between the order service and the inventory service; neither may read the other's storage directly"
  handoff_marker:  "ARCH/3.1/architecture-topology"
  source_evidence: ["arc42-template/05_building_block_view", "ADR-template/decision-section"]
```

B:
```
TranslatedDomainStatement:
  packet_id:             "ES-S1-pkt-001"
  control_type:          "ownership_boundary_rule"
  authority_source:      "arc42 §5 Building Block View; ADR Decision section"
  formal_statement:      "Order service owns OrderLifecycleState. Inventory service owns StockState. Neither service reads the other's storage directly. Cross-service state changes go through the published service contracts only."
  required_evidence:     ["arc42-component-list", "ADR record naming the boundary"]
  conflict_status:       "no_conflict"
  knowledge_edge_status: "edge_present — sub-phase 3.1 has no registered flow"
  handoff_marker:        "ARCH/3.1/architecture-topology"
```

Reason:
```
[WRONG: 3.1 owns the boundary rule — who owns what state, what dependency
direction is allowed, what is forbidden. The rejected translates the
packet into an implementation statement with class names and method
signatures (OrderService.GetInventory()). That belongs to 4.1 Backend
Services, not 3.1. The packet's authority sources (arc42 building block,
ADR decision) constrain the statement to ownership-and-direction
vocabulary, not class shape.]
```

Phrasing-B (Type A — 3.1 confused with 4.1):
```
TranslatedDomainStatement:
  packet_id:             "ES-S1-pkt-001"
  control_type:          "implementation_class"
  authority_source:      "C# coding standards"
  formal_statement:      "OrderService class exposes GetInventory(orderId) which calls InventoryRepository.FindByOrderId(orderId)."
  required_evidence:     []
  conflict_status:       "no_conflict"
  knowledge_edge_status: "no_edge"
  handoff_marker:        "ARCH/3.1/architecture-topology"
```

Coverage labels:
```
source_id:       arc42-template, ADR-template, nist-sp-800-160-v2
source_title:    arc42 + ADR + NIST SP 800-160 v2
transition_type: feature_addition
phase:           3
sub_phase:       3.1
task_type:       architecture-topology
rejected_type:   A
overlap_ratio:   0.32   (Type A: chosen ownership_boundary_rule statement vs rejected implementation_class statement; control_type tokens disjoint)
```

---

## Pair[ID] ES-2026-05-08-3.5-N02-02 — ES-S2 ARCH/3.5 → signal catalogue statement

A:
```
DomainPacket:
  packet_id:       "ES-S2-pkt-001"
  domain_label:    "ARCH"
  phase:           "3"
  sub_phase:       "3.5"
  task_type:       "observability-design"
  prompt_fragment: "design what to measure and alert on for the payment processor: workflows running, retries accumulating, compensation activities firing"
  handoff_marker:  "ARCH/3.5/observability-design"
  source_evidence: ["temporal-observability-docs", "dapr-observability-docs", "nist-sp-800-53-au-2"]
```

B:
```
TranslatedDomainStatement:
  packet_id:             "ES-S2-pkt-001"
  control_type:          "observability_signal_catalogue"
  authority_source:      "Temporal observability docs; Dapr observability docs; NIST SP 800-53 AU-2"
  formal_statement:      "Required signals: workflow.completed (count), workflow.failed (count, with reason), activity.retry (count, per activity), saga.compensation_fired (count, per workflow). Required alerts: workflow.failed > 5/min; activity.retry > 10/min sustained for 5 min. Audit events per AU-2: every workflow.failed and saga.compensation_fired must be logged with workflow_id and timestamp."
  required_evidence:     ["temporal-metric-list", "dapr-metric-list", "AU-2-event-list"]
  conflict_status:       "no_conflict"
  knowledge_edge_status: "edge_present — sub-phase 3.5 has no registered flow"
  handoff_marker:        "ARCH/3.5/observability-design"
```

Reason:
```
[WRONG: 3.5 owns "what to measure" and "alert thresholds". The chosen
formal statement enumerates signals and thresholds — the design
artifact. The rejected describes how to wire a logger into the codebase
(call _logger.LogInformation in the workflow.completed handler). That is
4.1 implementation work, not 3.5 design work. The 3.5 vs 4.1 confusion
documented in the ARCH table appears specifically when the translator
slips from "what to capture" to "how to wire the capture call".]
```

Phrasing-B (Type A — 3.5 confused with 4.1):
```
TranslatedDomainStatement:
  packet_id:             "ES-S2-pkt-001"
  control_type:          "logger_wiring"
  authority_source:      ".NET logging conventions"
  formal_statement:      "Inject ILogger<PaymentWorkflow> into the workflow class. Call _logger.LogInformation in the workflow.completed handler. Use Microsoft.Extensions.Logging.LoggerExtensions for structured properties."
  required_evidence:     []
  conflict_status:       "no_conflict"
  knowledge_edge_status: "no_edge"
  handoff_marker:        "ARCH/3.5/observability-design"
```

Coverage labels:
```
source_id:       temporal-observability-docs, dapr-observability-docs, nist-sp-800-53-au-2
source_title:    Temporal + Dapr observability + NIST AU-2
transition_type: feature_addition
phase:           3
sub_phase:       3.5
task_type:       observability-design
rejected_type:   A
overlap_ratio:   0.34   (Type A: chosen observability_signal_catalogue vs rejected logger_wiring; control_type token sets disjoint)
```

---

## Pair[ID] ES-2026-05-08-3.4-N02-03 — ES-S5 ARCH/3.4 → JWT model statement

A:
```
DomainPacket:
  packet_id:       "ES-S5-pkt-001"
  domain_label:    "ARCH"
  phase:           "3"
  sub_phase:       "3.4"
  task_type:       "jwt-authentication"
  prompt_fragment: "design the JWT bearer authentication model: iss/aud/exp claims required, tenant claim placement, bypass paths"
  handoff_marker:  "ARCH/3.4/jwt-authentication"
  source_evidence: ["rfc-7519", "rfc-6749"]
```

B:
```
TranslatedDomainStatement:
  packet_id:             "ES-S5-pkt-001"
  control_type:          "authentication_model"
  authority_source:      "IETF RFC 7519 §4.1; IETF RFC 6749 §1.4"
  formal_statement:      "All API requests must carry an Authorization: Bearer <jwt> header. The JWT must be signed (alg != none) and contain claims iss, aud, exp, and tenant. Validation rejects expired tokens (exp <= now). The tenant claim is the authorization key; the API enforces tenant isolation against this claim. Bypass paths: /health, /metrics, /version are unauthenticated."
  required_evidence:     ["RFC-7519-§4.1.1", "RFC-7519-§4.1.3", "RFC-7519-§4.1.4", "RFC-6749-§1.4"]
  conflict_status:       "no_conflict"
  knowledge_edge_status: "no_edge"
  handoff_marker:        "ARCH/3.4/jwt-authentication"
```

Reason:
```
[WRONG: 3.4 owns the authentication model — claim shape, signature
algorithm allow-list, expiry semantics, bypass paths. The rejected
translates the packet into an API contract statement (request/response
status codes, header names). That belongs to packet ES-S5-pkt-002 / 3.3,
not to ES-S5-pkt-001 / 3.4. Each packet must produce a statement scoped
to its own sub-phase responsibility — the splitter already separated 3.4
and 3.3, and the translator must respect that separation.]
```

Phrasing-B (Type A — 3.4 confused with 3.3):
```
TranslatedDomainStatement:
  packet_id:             "ES-S5-pkt-001"
  control_type:          "api_contract"
  authority_source:      "OpenAPI 3.1"
  formal_statement:      "POST /payments responds 200 on success, 401 when Authorization header missing, 403 when tenant mismatch."
  required_evidence:     []
  conflict_status:       "no_conflict"
  knowledge_edge_status: "no_edge"
  handoff_marker:        "ARCH/3.4/jwt-authentication"
```

Coverage labels:
```
source_id:       rfc-7519, rfc-6749, rfc-8725
source_title:    RFC 7519 + RFC 6749 + RFC 8725
transition_type: feature_addition
phase:           3
sub_phase:       3.4
task_type:       jwt-authentication
rejected_type:   A
overlap_ratio:   0.36   (Type A: chosen authentication_model vs rejected api_contract; statement scope tokens differ)
```

---

## Pair[ID] ES-2026-05-08-5.2-N02-04 — ES-S5 TEST/5.2 → integration test plan statement

A:
```
DomainPacket:
  packet_id:       "ES-S5-pkt-003"
  domain_label:    "TEST"
  phase:           "5"
  sub_phase:       "5.2"
  task_type:       "integration-tests"
  prompt_fragment: "write integration tests for tenant isolation across the authentication and API layers — real fixture, two tenants, assert cross-tenant access is denied"
  handoff_marker:  "TEST/5.2/integration-tests"
  source_evidence: ["testcontainers-dotnet"]
```

B:
```
TranslatedDomainStatement:
  packet_id:             "ES-S5-pkt-003"
  control_type:          "integration_test_plan"
  authority_source:      "Testcontainers PostgreSQL module documentation; Microsoft testfx fixture pattern"
  formal_statement:      "Integration tests must spin up a real PostgreSQL container per test class. Two seeded tenants (tenant_a, tenant_b) each have one order row. Authenticated tenant_a JWT requesting GET /orders/{tenant_b_order_id} must receive 403. Authenticated tenant_a JWT requesting GET /orders/{tenant_a_order_id} must receive 200. Mocked database is forbidden — the tenant filter must be exercised by real SQL."
  required_evidence:     ["testcontainers-postgres-docs", "testfx-fixture-pattern"]
  conflict_status:       "no_conflict"
  knowledge_edge_status: "edge_present — sub-phase 5.2 has no registered flow"
  handoff_marker:        "TEST/5.2/integration-tests"
```

Reason:
```
[WRONG: 5.2 integration tests for tenant isolation require a real
container — the Testcontainers documentation is explicit that mocking
defeats the test's purpose for cross-row access verification. The
rejected translates the packet into a 5.1 unit-test plan with mocked
repository calls. Mocked rows cannot prove that tenant B's data is
inaccessible because the mock is the entire data source. This is the
canonical 5.2 vs 5.1 confusion documented in the testing confusion
table.]
```

Phrasing-B (Type A — 5.2 confused with 5.1):
```
TranslatedDomainStatement:
  packet_id:             "ES-S5-pkt-003"
  control_type:          "unit_test_plan"
  authority_source:      "xUnit pattern"
  formal_statement:      "Unit tests for OrdersController with mocked IOrderRepository. Verify that controller filters by tenant claim correctly when repository returns 0 or 1 rows."
  required_evidence:     []
  conflict_status:       "no_conflict"
  knowledge_edge_status: "no_edge"
  handoff_marker:        "TEST/5.2/integration-tests"
```

Coverage labels:
```
source_id:       testcontainers-dotnet, microsoft-testfx
source_title:    Testcontainers + testfx
transition_type: feature_addition
phase:           5
sub_phase:       5.2
task_type:       integration-tests
rejected_type:   A
overlap_ratio:   0.35   (Type A: chosen integration_test_plan vs rejected unit_test_plan; mock vocabulary disjoint)
```

---

## Pair[ID] ES-2026-05-08-6.3-N02-05 — ES-S6 QA/6.3 → review verdict statement

A:
```
DomainPacket:
  packet_id:       "ES-S6-pkt-001"
  domain_label:    "QA"
  phase:           "6"
  sub_phase:       "6.3"
  task_type:       "security-compliance-review"
  prompt_fragment: "review the admin endpoint against AC-3: confirm the missing role gate is the single root cause; verify the fix once applied; produce a finding citation"
  handoff_marker:  "QA/6.3/security-compliance-review"
  source_evidence: ["nist-sp-800-53-ac-3", "cwe-285"]
```

B:
```
TranslatedDomainStatement:
  packet_id:             "ES-S6-pkt-001"
  control_type:          "compliance_review_finding"
  authority_source:      "NIST SP 800-53 AC-3; MITRE CWE-285"
  formal_statement:      "Finding: GET /admin/users does not enforce role=admin claim. CWE-285 (Improper Authorization) applies — request is authenticated but unauthorized for the resource. NIST AC-3 requires the system to enforce approved authorizations; the current implementation does not. Verdict: concern with named gap. Required remediation evidence: a role gate that returns 403 when role != admin AND a regression test that proves the gate is exercised."
  required_evidence:     ["AC-3-control-citation", "CWE-285-mapping", "regression-test-coverage"]
  conflict_status:       "no_conflict"
  knowledge_edge_status: "edge_present — sub-phase 6.3 has no registered flow"
  handoff_marker:        "QA/6.3/security-compliance-review"
```

Reason:
```
[WRONG: 6.3 owns the review verdict — finding, citation, remediation
evidence requirements. The rejected translates the packet into a security
model redesign statement. But the model already exists (RBAC was already
required by AC-3). Producing a redesign confuses 6.3 with 3.4. The
correct 6.3 statement is a finding plus the evidence required to close
it, not a new model.]
```

Phrasing-B (Type A — 6.3 confused with 3.4):
```
TranslatedDomainStatement:
  packet_id:             "ES-S6-pkt-001"
  control_type:          "authentication_model"
  authority_source:      "RFC 7519"
  formal_statement:      "Add a role claim to the JWT and a role-based access control middleware that maps roles to endpoints."
  required_evidence:     []
  conflict_status:       "no_conflict"
  knowledge_edge_status: "no_edge"
  handoff_marker:        "QA/6.3/security-compliance-review"
```

Coverage labels:
```
source_id:       cwe-285, nist-sp-800-53-ac-3
source_title:    CWE-285 + NIST AC-3
transition_type: repair
phase:           6
sub_phase:       6.3
task_type:       security-compliance-review
rejected_type:   A
overlap_ratio:   0.38   (Type A: chosen compliance_review_finding vs rejected authentication_model; control_type tokens disjoint)
```

---

## Node 02 summary

```
Pairs:                  5 (one packet at a time, never grouped)
Type A rejected:        5
Sub-phases covered:     3.1, 3.5, 3.4, 5.2, 6.3
Knowledge-edge_status:  4 of 5 packets carry edge_present (only 3.4 is registered)
```

Every chosen B is a single statement bound to a single packet. Authority
sources are real external documents in every B.
