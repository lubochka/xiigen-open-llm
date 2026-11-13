# Supplement — PLAN Type B (Wrong Dependency Order)

Six additional Type B pairs at `SchemePlannerUnit` to lift the Type B
fraction toward the v7 §"Batch composition minimums" target of 20 %.
Each pair pairs the same chosen branches as a base scenario but flips
or omits a real dependency, with a `[WRONG: ...]` annotation that
explains *why the wrong order fails* (per DPO authoring guidance Error 5).

All scenarios are grounded in external sources only.

---

## Common coverage labels

```
source_license_state: external_license_checked
contract_signature:   SchemePlannerUnit
node_role:            node_03_supplement
pair_kind:            flow_node
coverage_status:      covered
rejected_type:        B
```

Per-pair labels carry `source_id`, `source_title`, `transition_type`,
`phase`, `sub_phase`, `task_type`, `overlap_ratio`.

---

## Pair[ID] ES-2026-05-08-3.2-N03B-01 — Data model before API contract (RFC 9110 grounding)

A:
```
TranslatedDomainStatement[]:
  - control_type:     "data_model"
    formal_statement: "Order entity owns OrderId, CustomerId, CreatedAt, Items[]. Storage is single-tenant per row with tenant_id partition key."
    handoff_marker:   "ARCH/3.2/data-persistence"
  - control_type:     "api_contract"
    formal_statement: "GET /orders/{id} returns Order projection per RFC 9110 Content-Type=application/json."
    handoff_marker:   "ARCH/3.3/api-contract"
```

B (chosen — 3.2 before 3.3):
```
WorkBranch[]:
  - branch_id: "branch-1"  sub_phase: "3.2"  action: "Define Order entity shape and storage rules"  depends_on: []
  - branch_id: "branch-2"  sub_phase: "3.3"  action: "Define API projection from Order entity"      depends_on: ["branch-1"]
```

Reason:
```
[WRONG: per the v8 PLAN dependency table, 3.2 Data Model → 3.3 API
Contract is the correct sequence. The contract is a projection of the
underlying entity; the entity decides which fields exist before the
contract decides which to expose. The rejected reverses: define the
contract first, derive the entity to match. Functional failure: the
contract becomes the source of truth and the entity ends up shaped by
wire-format concerns rather than by domain semantics — internal
operations (audit, compaction, retention) get fields they cannot
populate because the contract did not expose them.]
```

Phrasing-B (Type B — 3.3 before 3.2):
```
WorkBranch[]:
  - branch_id: "branch-1"  sub_phase: "3.3"  action: "Define the API contract for Order"                       depends_on: []
  - branch_id: "branch-2"  sub_phase: "3.2"  action: "Define entity shape to match the contract from branch-1" depends_on: ["branch-1"]
```

Coverage labels:
```
source_id:            rfc-9110, json-schema-org
source_title:         RFC 9110 + JSON Schema 2020-12
transition_type:      feature_addition
phase:                3
sub_phase:            3.2+3.3
task_type:            data-and-contract-sequencing
overlap_ratio:        0.44
```

---

## Pair[ID] ES-2026-05-08-2.2-N03B-02 — Acceptance criteria before unit tests

A:
```
TranslatedDomainStatement[]:
  - control_type:     "acceptance_criteria"
    formal_statement: "User story: customer can cancel an order before it ships. Acceptance: cancel allowed when status in {pending, paid}; refund issued when paid; cancellation event recorded in audit log."
    handoff_marker:   "DOCUMENT/2.2/acceptance-criteria"
  - control_type:     "unit_test_plan"
    formal_statement: "Unit tests covering CancellationService.Cancel(orderId) for each acceptance branch."
    handoff_marker:   "TEST/5.1/unit-tests"
```

B (chosen — 2.2 before 5.1):
```
WorkBranch[]:
  - branch_id: "branch-1"  sub_phase: "2.2"  action: "Define acceptance criteria for cancel-order flow" depends_on: []
  - branch_id: "branch-2"  sub_phase: "5.1"  action: "Write unit tests covering each acceptance branch" depends_on: ["branch-1"]
```

Reason:
```
[WRONG: per the v8 PLAN dependency table, 2.2 Acceptance Criteria → 5.1
Unit Tests is the correct sequence. Acceptance criteria are the
specification the unit tests verify; the tests are not allowed to
*define* the criteria. The rejected reverses: write tests first, infer
acceptance from passing tests. Functional failure: tests verify only
what the test author chose to assert, and the acceptance set drifts
from product intent. Edge cases the author did not write tests for
become silently accepted as "outside the spec" because the spec is
defined as "what the tests cover".]
```

Phrasing-B (Type B — 5.1 before 2.2):
```
WorkBranch[]:
  - branch_id: "branch-1"  sub_phase: "5.1"  action: "Write unit tests against an assumed cancel flow"      depends_on: []
  - branch_id: "branch-2"  sub_phase: "2.2"  action: "Document acceptance criteria matching passing tests" depends_on: ["branch-1"]
```

Coverage labels:
```
source_id:            microsoft-testfx, openapi-specification
source_title:         testfx + AsyncAPI acceptance criteria patterns
transition_type:      feature_addition
phase:                2
sub_phase:            2.2+5.1
task_type:            acceptance-then-tests
overlap_ratio:        0.41
```

---

## Pair[ID] ES-2026-05-08-3.5-N03B-03 — Observability design before monitoring config

A:
```
TranslatedDomainStatement[]:
  - control_type:     "observability_design"
    formal_statement: "Required signals: workflow.started, workflow.failed, activity.retry_total per Temporal observability docs. Alert rule: workflow.failed > 5/min."
    handoff_marker:   "ARCH/3.5/observability-design"
  - control_type:     "monitoring_config"
    formal_statement: "Wire signals into Grafana dashboard 'workflows' and Alertmanager rule group 'workflow_failures'."
    handoff_marker:   "AUTOMATION/7.3/monitoring-feedback"
```

B (chosen — 3.5 before 7.3):
```
WorkBranch[]:
  - branch_id: "branch-1"  sub_phase: "3.5"  action: "Pick the signal catalogue and alert thresholds"     depends_on: []
  - branch_id: "branch-2"  sub_phase: "7.3"  action: "Configure Grafana + Alertmanager from the catalogue" depends_on: ["branch-1"]
```

Reason:
```
[WRONG: per the v8 PLAN dependency table, 3.5 Observability Design → 7.3
Monitoring is the correct sequence. The design picks what to measure;
monitoring operationalises the picks. The rejected reverses: configure
the dashboard first, retroactively design what the dashboard shows.
Functional failure: the dashboard ends up showing whatever the
configuration tool defaults to (request count, p99 latency) instead of
the workflow signals the prompt names. Compensation activities and
saga retries — both explicitly named — would not be visible because
the configuration step did not know to capture them.]
```

Phrasing-B (Type B — 7.3 before 3.5):
```
WorkBranch[]:
  - branch_id: "branch-1"  sub_phase: "7.3"  action: "Configure default Grafana dashboard"                  depends_on: []
  - branch_id: "branch-2"  sub_phase: "3.5"  action: "Document which of the visible metrics are the design" depends_on: ["branch-1"]
```

Coverage labels:
```
source_id:            temporal-observability-docs, dapr-observability-docs
source_title:         Temporal + Dapr observability sequencing
transition_type:      feature_addition
phase:                3
sub_phase:            3.5+7.3
task_type:            design-before-config
overlap_ratio:        0.39
```

---

## Pair[ID] ES-2026-05-08-3.3-N03B-04 — API contract before integration connectors

A:
```
TranslatedDomainStatement[]:
  - control_type:     "api_contract"
    formal_statement: "POST /webhooks/stripe accepts Stripe Event payloads per Stripe's published webhook schema; signature header required; idempotency by event.id."
    handoff_marker:   "ARCH/3.3/api-contract"
  - control_type:     "integration_connector"
    formal_statement: "StripeWebhookConnector deserialises payload, verifies signature, dispatches to handlers."
    handoff_marker:   "IMPLEMENT/4.5/provider-connectors"
```

B (chosen — 3.3 before 4.5):
```
WorkBranch[]:
  - branch_id: "branch-1"  sub_phase: "3.3"  action: "Define the webhook endpoint contract"          depends_on: []
  - branch_id: "branch-2"  sub_phase: "4.5"  action: "Implement the connector against the contract" depends_on: ["branch-1"]
```

Reason:
```
[WRONG: per the v8 PLAN dependency table, 3.3 API Contract → 4.5
Integration Connectors is the correct sequence. The contract is the
agreed shape the provider expects; the connector is the implementation
that satisfies it. The rejected reverses: implement the connector first,
then write the contract to match. Functional failure: the contract
becomes a description of whatever fields the implementation chose to
parse — not what Stripe actually sends. When Stripe adds a new field
to its event schema, the connector misses it because the contract
silently ratified the implementation as complete.]
```

Phrasing-B (Type B — 4.5 before 3.3):
```
WorkBranch[]:
  - branch_id: "branch-1"  sub_phase: "4.5"  action: "Implement the connector parsing the fields we know" depends_on: []
  - branch_id: "branch-2"  sub_phase: "3.3"  action: "Document the contract from the connector"           depends_on: ["branch-1"]
```

Coverage labels:
```
source_id:            openapi-specification, asyncapi-spec
source_title:         OpenAPI 3.1 + AsyncAPI (provider-webhook contract)
transition_type:      feature_addition
phase:                3
sub_phase:            3.3+4.5
task_type:            contract-before-connector
overlap_ratio:        0.42
```

---

## Pair[ID] ES-2026-05-08-3.1-N03B-05 — Architecture topology before backend services

A:
```
TranslatedDomainStatement[]:
  - control_type:     "ownership_boundary_rule"
    formal_statement: "Order service owns OrderLifecycleState. Inventory service owns StockState. Cross-service writes via published API only."
    handoff_marker:   "ARCH/3.1/architecture-topology"
  - control_type:     "backend_implementation"
    formal_statement: "Implement OrderService.PlaceOrder using InventoryClient.Reserve."
    handoff_marker:   "IMPLEMENT/4.1/backend-services"
```

B (chosen — 3.1 before 4.1):
```
WorkBranch[]:
  - branch_id: "branch-1"  sub_phase: "3.1"  action: "Document ownership boundary as ADR (arc42 building-block view)" depends_on: []
  - branch_id: "branch-2"  sub_phase: "4.1"  action: "Implement OrderService against the boundary"                  depends_on: ["branch-1"]
```

Reason:
```
[WRONG: per the v8 PLAN dependency table, 3.1 Architecture → 4.1
Backend Services is the correct sequence. The boundary decides what
each service owns and what calls cross the boundary; implementation
follows. The rejected reverses: implement first, then document the
boundary as a description of whatever the implementation did.
Functional failure: the implementation may directly read the inventory
service's storage (a forbidden cross-storage read per the boundary
rule) and the post-hoc ADR will quietly ratify that violation. The
boundary loses its purpose as a constraint when written after the
fact.]
```

Phrasing-B (Type B — 4.1 before 3.1):
```
WorkBranch[]:
  - branch_id: "branch-1"  sub_phase: "4.1"  action: "Implement OrderService.PlaceOrder however works" depends_on: []
  - branch_id: "branch-2"  sub_phase: "3.1"  action: "Document boundary to match the implementation" depends_on: ["branch-1"]
```

Coverage labels:
```
source_id:            arc42-template, ADR-template
source_title:         arc42 building-block view + ADR Decision section
transition_type:      feature_addition
phase:                3
sub_phase:            3.1+4.1
task_type:            architecture-then-implementation
overlap_ratio:        0.43
```

---

## Pair[ID] ES-2026-05-08-3.4-N03B-06 — Security model before integration tests (RFC 7519 grounding)

A:
```
TranslatedDomainStatement[]:
  - control_type:     "authentication_model"
    formal_statement: "JWT bearer required; iss, aud, exp, tenant claims; alg=none rejected per RFC 7519 + RFC 8725."
    handoff_marker:   "ARCH/3.4/jwt-authentication"
  - control_type:     "integration_test_plan"
    formal_statement: "Real-fixture tests asserting tenant_a JWT cannot read tenant_b rows."
    handoff_marker:   "TEST/5.2/integration-tests"
```

B (chosen — 3.4 before 5.2):
```
WorkBranch[]:
  - branch_id: "branch-1"  sub_phase: "3.4"  action: "Define the JWT model (RFC 7519)"                       depends_on: []
  - branch_id: "branch-2"  sub_phase: "5.2"  action: "Write integration tests using real tokens issued by branch-1's model" depends_on: ["branch-1"]
```

Reason:
```
[WRONG: this is the canonical Type B target from v8 §"Layer 2 Node 03"
example: 3.4 Security → 5.2 Integration Tests. The rejected reverses:
write integration tests first using mock tokens, then add the real
auth model after. Functional failure: mock tokens bypass real
validation logic. A test that "passes" with mock tokens proves nothing
about RFC 7519 conformance — the iss/aud/exp/tenant claim validation
is not exercised. Tenant isolation in particular cannot be tested with
mock tokens because the mock is the entire token — the tenant claim
is whatever the test author put there.]
```

Phrasing-B (Type B — 5.2 before 3.4 with mock tokens):
```
WorkBranch[]:
  - branch_id: "branch-1"  sub_phase: "5.2"  action: "Write tenant-isolation integration tests with mock JWT tokens" depends_on: []
  - branch_id: "branch-2"  sub_phase: "3.4"  action: "Define the auth model after tests define what it must satisfy" depends_on: ["branch-1"]
```

Coverage labels:
```
source_id:            rfc-7519, rfc-8725, testcontainers-dotnet
source_title:         RFC 7519 + RFC 8725 + Testcontainers (3.4 → 5.2 sequence)
transition_type:      feature_addition
phase:                3
sub_phase:            3.4+5.2
task_type:            security-then-tests
overlap_ratio:        0.45
```

---

## Supplement summary

```
Pairs:                  6   (all Type B)
v8 PLAN dependency rows covered:
  3.2 → 3.3                           ✓ (pair 01)
  2.2 → 5.1                           ✓ (pair 02)
  3.5 → 7.3                           ✓ (pair 03)
  3.3 → 4.5                           ✓ (pair 04)
  3.1 → 4.1                           ✓ (pair 05)
  3.4 → 5.2                           ✓ (pair 06)

Annotation requirement (every Type B carries the why-it-fails clause): met
```

Lifts the global Type B count from 3 to 9 across the batch. Type B
remains below the 20 % global target (9 / 96 = 9.4 %); see README §"Honest
disclosure" for the per-layer interpretation and the recommended
follow-up.

---

## Last Updated

2026-05-08.
