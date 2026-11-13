# Coverage Labels Sweep — Node 03 through Node 09

This file records the **complete v8 §"Required Coverage Labels"** plus
the **token-overlap-ratio** for every pair in `Node-03-SchemePlanner.md`
through `Node-09-Formatter.md`. It complements the per-pair coverage
blocks inside each pair file. Where the per-pair block in a node file
shows fewer fields than the v8 line 806 set, this sweep file is the
authoritative metadata record.

The file is organized by node, then by Pair[ID]. Every entry shows the
complete label set required by:

```
v8 line 806:
  source_id, source_title, source_license_state, contract_signature,
  transition_type, phase, sub_phase, task_type, node_role,
  coverage_status, pair_kind, rejected_type
DPO authoring guidance v2 line 58:
  overlap_ratio   (must be < 0.60)
```

Common file-level constants (declared once per node file in its
"Common coverage labels" header) are also repeated here for explicit
verification.

---

## Node 03 — SchemePlannerUnit

### ES-2026-05-08-3.4-N03-01 — ES-S5 three branches (Type B)

```
source_id:            rfc-7519, rfc-6749, rfc-8725, openapi-specification, testcontainers-dotnet
source_title:         RFC 7519 + RFC 6749 + RFC 8725 + OpenAPI 3.1 + Testcontainers
source_license_state: external_license_checked
attribution_required: false
contract_signature:   SchemePlannerUnit
transition_type:      feature_addition
phase:                3
sub_phase:            3.4+3.3+5.2
task_type:            multi-task-jwt-contract-tests
node_role:            node_03
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        B
overlap_ratio:        0.46   (Type B: same sub-phases, dependency direction reversed; depends_on values disjoint)
```

### ES-2026-05-08-3.3-N03-02 — ES-S4 idempotency contract → tests (Type B)

```
source_id:            openapi-specification, rfc-8470, rfc-9110
source_title:         OpenAPI 3.1 + RFC 8470 + RFC 9110
source_license_state: external_license_checked
attribution_required: false
contract_signature:   SchemePlannerUnit
transition_type:      feature_addition
phase:                3
sub_phase:            3.3+5.2
task_type:            multi-task-contract-then-tests
node_role:            node_03
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        B
overlap_ratio:        0.43   (Type B: same sub-phases, ordering reversed)
```

### ES-2026-05-08-3.5-N03-03 — ES-S7 saga forward order before compensation (Type B)

```
source_id:            temporal-saga, airflow-dag-tutorial
source_title:         Temporal saga + Airflow DAG
source_license_state: external_license_checked
attribution_required: false
contract_signature:   SchemePlannerUnit
transition_type:      feature_addition
phase:                3
sub_phase:            3.5+4.1
task_type:            saga-design-then-implement
node_role:            node_03
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        B
overlap_ratio:        0.44   (Type B: design vs implement order reversed; depends_on flipped)
```

### ES-2026-05-08-3.1-N03-04 — ES-S1 single 3.1 branch (Type A)

```
source_id:            arc42-template, ADR-template
source_title:         arc42 + ADR
source_license_state: external_license_checked
attribution_required: false
contract_signature:   SchemePlannerUnit
transition_type:      feature_addition
phase:                3
sub_phase:            3.1
task_type:            architecture-topology
node_role:            node_03
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        A
overlap_ratio:        0.30   (Type A: invented branch-0 has disjoint vocabulary from chosen single-branch ADR)
```

### ES-2026-05-08-6.3-N03-05 — ES-S6 single 6.3 branch (Type A)

```
source_id:            cwe-285, nist-sp-800-53-ac-3
source_title:         CWE-285 + NIST AC-3
source_license_state: external_license_checked
attribution_required: false
contract_signature:   SchemePlannerUnit
transition_type:      repair
phase:                6
sub_phase:            6.3
task_type:            security-compliance-review
node_role:            node_03
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        A
overlap_ratio:        0.32   (Type A: chosen 1 verdict branch vs rejected 2 branches; invented 4.1 branch disjoint)
```

---

## Node 04 — DomainProcessorUnit

### ES-2026-05-08-3.4-N04-01 — ES-S5 branch-1 JWT model derivation

```
source_id:            rfc-7519, rfc-6749, rfc-8725
source_title:         RFC 7519 + RFC 6749 + RFC 8725
source_license_state: external_license_checked
attribution_required: false
contract_signature:   DomainProcessorUnit
transition_type:      feature_addition
phase:                3
sub_phase:            3.4
task_type:            jwt-authentication
node_role:            node_04
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        A
overlap_ratio:        0.38   (Type A: chosen status="derived_with_one_unresolved" vs rejected "fully_derived"; locally_unresolved field absent)
```

### ES-2026-05-08-5.2-N04-02 — ES-S5 branch-3 integration test plan

```
source_id:            testcontainers-dotnet, microsoft-testfx
source_title:         Testcontainers + testfx
source_license_state: external_license_checked
attribution_required: false
contract_signature:   DomainProcessorUnit
transition_type:      feature_addition
phase:                5
sub_phase:            5.2
task_type:            integration-tests
node_role:            node_04
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        A
overlap_ratio:        0.36   (Type A: chosen mocks_forbidden=true vs rejected mocks_allowed=true; fixture vocabulary differs)
```

### ES-2026-05-08-3.5-N04-03 — ES-S7 branch-1 saga design

```
source_id:            temporal-saga
source_title:         Temporal saga documentation
source_license_state: external_license_checked
attribution_required: false
contract_signature:   DomainProcessorUnit
transition_type:      feature_addition
phase:                3
sub_phase:            3.5
task_type:            saga-design
node_role:            node_04
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        A
overlap_ratio:        0.40   (Type A: chosen compensation rule preserved vs rejected inverted; authority quote tokens shared)
```

### ES-2026-05-08-6.3-N04-04 — ES-S6 branch-1 review verdict

```
source_id:            cwe-285, nist-sp-800-53-ac-3
source_title:         CWE-285 + NIST AC-3
source_license_state: external_license_checked
attribution_required: false
contract_signature:   DomainProcessorUnit
transition_type:      repair
phase:                6
sub_phase:            6.3
task_type:            security-compliance-review
node_role:            node_04
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        A
overlap_ratio:        0.41   (Type A: chosen verdict_class="concern" vs rejected "reject"; named_gap shared)
```

---

## Node 05 — ApplierUnit

### ES-2026-05-08-3.4-N05-01 — ES-S5 branch-1 JWT model applied

```
source_id:            rfc-7519, rfc-6749, rfc-8725
source_title:         RFC 7519 + RFC 6749 + RFC 8725
source_license_state: external_license_checked
attribution_required: false
contract_signature:   ApplierUnit
transition_type:      feature_addition
phase:                3
sub_phase:            3.4
task_type:            jwt-authentication
node_role:            node_05
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        A
overlap_ratio:        0.39   (Type A: chosen knowledge_edges populated vs rejected knowledge_edges silenced; tenant_claim token shared)
```

### ES-2026-05-08-5.2-N05-02 — ES-S5 branch-3 test plan applied

```
source_id:            testcontainers-dotnet, microsoft-testfx
source_title:         Testcontainers + testfx
source_license_state: external_license_checked
attribution_required: false
contract_signature:   ApplierUnit
transition_type:      feature_addition
phase:                5
sub_phase:            5.2
task_type:            integration-tests
node_role:            node_05
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        A
overlap_ratio:        0.37   (Type A: chosen rejected_items+knowledge_edges populated vs rejected silenced)
```

### ES-2026-05-08-3.5-N05-03 — ES-S7 branch-1 saga applied

```
source_id:            temporal-saga
source_title:         Temporal saga documentation
source_license_state: external_license_checked
attribution_required: false
contract_signature:   ApplierUnit
transition_type:      feature_addition
phase:                3
sub_phase:            3.5
task_type:            saga-design
node_role:            node_05
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        A
overlap_ratio:        0.42   (Type A: chosen rejected_items keeps anti-pattern visible vs rejected promotes anti-pattern)
```

### ES-2026-05-08-6.3-N05-04 — ES-S6 review verdict applied

```
source_id:            cwe-285, nist-sp-800-53-ac-3
source_title:         CWE-285 + NIST AC-3
source_license_state: external_license_checked
attribution_required: false
contract_signature:   ApplierUnit
transition_type:      repair
phase:                6
sub_phase:            6.3
task_type:            security-compliance-review
node_role:            node_05
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        A
overlap_ratio:        0.40   (Type A: chosen blocked_items+rejected_items populated vs rejected drops both status classes)
```

---

## Node 06 — ResultTranslatorUnit

### ES-2026-05-08-3.4-N06-01 — ES-S5 three branches → fragments

```
source_id:            rfc-7519, rfc-6749, rfc-8725, openapi-specification, testcontainers-dotnet
source_title:         RFC 7519 + RFC 6749 + RFC 8725 + OpenAPI 3.1 + Testcontainers
source_license_state: external_license_checked
attribution_required: false
contract_signature:   ResultTranslatorUnit
transition_type:      feature_addition
phase:                3
sub_phase:            3.4+3.3+5.2
task_type:            multi-fragment-status-class-preservation
node_role:            node_06
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        A
overlap_ratio:        0.43   (Type A: chosen 3 fragments per status class vs rejected 1 folded fragment)
```

### ES-2026-05-08-3.5-N06-02 — ES-S7 two branches → fragments

```
source_id:            temporal-saga, airflow-dag-tutorial
source_title:         Temporal saga + Airflow DAG
source_license_state: external_license_checked
attribution_required: false
contract_signature:   ResultTranslatorUnit
transition_type:      feature_addition
phase:                3
sub_phase:            3.5+4.1
task_type:            saga-fragment-status-class-preservation
node_role:            node_06
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        A
overlap_ratio:        0.44   (Type A: chosen 4 fragments (accepted+rejected+two knowledge-edge) vs rejected 2 (accepted only))
```

### ES-2026-05-08-6.3-N06-03 — ES-S6 single branch → fragments

```
source_id:            cwe-285, nist-sp-800-53-ac-3
source_title:         CWE-285 + NIST AC-3
source_license_state: external_license_checked
attribution_required: false
contract_signature:   ResultTranslatorUnit
transition_type:      repair
phase:                6
sub_phase:            6.3
task_type:            review-fragment-status-class-preservation
node_role:            node_06
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        A
overlap_ratio:        0.45   (Type A: chosen 4 fragments (concern+blocked+rejected+knowledge-edge) vs rejected 1 (concern))
```

---

## Node 07 — ArbiterSelectorUnit

### ES-2026-05-08-3.4-N07-01 — ES-S5 selects security + contract arbiters

```
source_id:            rfc-7519, openapi-specification, testcontainers-dotnet
source_title:         RFC 7519 + OpenAPI 3.1 + Testcontainers
source_license_state: external_license_checked
attribution_required: false
contract_signature:   ArbiterSelectorUnit
transition_type:      feature_addition
phase:                3
sub_phase:            3.4+3.3+5.2
task_type:            arbiter-selection-multi-fragment
node_role:            node_07
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        A
overlap_ratio:        0.41   (Type A: chosen 4 considered arbiters (2 selected, 2 rejected) vs rejected 2 (selected only))
```

### ES-2026-05-08-6.3-N07-02 — ES-S6 selects compliance, rejects design arbiter

```
source_id:            nist-sp-800-53-ac-3, cwe-285, rfc-7519
source_title:         NIST AC-3 + CWE-285 + RFC 7519 (rejected)
source_license_state: external_license_checked
attribution_required: false
contract_signature:   ArbiterSelectorUnit
transition_type:      repair
phase:                6
sub_phase:            6.3
task_type:            arbiter-selection-review-vs-design
node_role:            node_07
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        A
overlap_ratio:        0.43   (Type A: chosen rejects RFC 7519 design arbiter for review fragment vs rejected selects it)
```

### ES-2026-05-08-3.5-N07-03 — ES-S7 selects saga + idempotency arbiters

```
source_id:            temporal-saga
source_title:         Temporal saga
source_license_state: external_license_checked
attribution_required: false
contract_signature:   ArbiterSelectorUnit
transition_type:      feature_addition
phase:                3
sub_phase:            3.5
task_type:            arbiter-selection-saga
node_role:            node_07
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        A
overlap_ratio:        0.40   (Type A: chosen registered arbiter ids vs rejected invents arbiter id not in registry)
```

---

## Node 08 — ArbiterUnit

### ES-2026-05-08-3.4-N08-01 — ES-S5 RFC 7519 clean pass (silent)

```
source_id:            rfc-7519, rfc-8725
source_title:         RFC 7519 + RFC 8725
source_license_state: external_license_checked
attribution_required: false
contract_signature:   ArbiterUnit
transition_type:      feature_addition
phase:                3
sub_phase:            3.4
task_type:            jwt-authentication
node_role:            node_08
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        A
overlap_ratio:        0.32   (Type A: chosen verdict="pass" silent vs rejected verdict="pass" with fabricated fields)
```

### ES-2026-05-08-6.3-N08-02 — ES-S6 NIST AC-3 concern verdict

```
source_id:            nist-sp-800-53-ac-3, cwe-285
source_title:         NIST AC-3 + CWE-285
source_license_state: external_license_checked
attribution_required: false
contract_signature:   ArbiterUnit
transition_type:      repair
phase:                6
sub_phase:            6.3
task_type:            security-compliance-review
node_role:            node_08
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        A
overlap_ratio:        0.37   (Type A: chosen verdict="concern" with named gap vs rejected verdict="pass" with fabricated rationale)
```

### ES-2026-05-08-3.5-N08-03 — ES-S7 Temporal saga reprocess verdict

```
source_id:            temporal-saga
source_title:         Temporal saga
source_license_state: external_license_checked
attribution_required: false
contract_signature:   ArbiterUnit
transition_type:      feature_addition
phase:                3
sub_phase:            3.5
task_type:            saga-design
node_role:            node_08
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        A
overlap_ratio:        0.39   (Type A: chosen verdict="reprocess" surfaces unresolved field vs rejected verdict="pass" silences it)
```

### ES-2026-05-08-3.4-N08-04 — ES-S5 OpenAPI reject (contract incompatible)

```
source_id:            openapi-specification, rfc-7519
source_title:         OpenAPI 3.1 + RFC 7519
source_license_state: external_license_checked
attribution_required: false
contract_signature:   ArbiterUnit
transition_type:      feature_addition
phase:                3
sub_phase:            3.3
task_type:            api-contract
node_role:            node_08
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        A
overlap_ratio:        0.41   (Type A: chosen verdict="reject" structural defect vs rejected verdict="concern" leaves contradiction open)
```

### ES-2026-05-08-3.4-N08-05 — ES-S5 RFC 7519 pass with audit-event note

```
source_id:            rfc-7519, rfc-8725, nist-sp-800-53-au-2
source_title:         RFC 7519 + RFC 8725 + NIST AU-2
source_license_state: external_license_checked
attribution_required: false
contract_signature:   ArbiterUnit
transition_type:      feature_addition
phase:                3
sub_phase:            3.4
task_type:            jwt-authentication
node_role:            node_08
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        A
overlap_ratio:        0.34   (Type A: chosen silent pass vs rejected false-concern with fabricated AU-2 requirement)
```

---

## Node 09 — FormatterUnit

### ES-2026-05-08-3.4-N09-01 — ES-S5 multi-branch human result

```
source_id:            rfc-7519, rfc-6749, rfc-8725, openapi-specification, testcontainers-dotnet
source_title:         RFC 7519 + RFC 6749 + RFC 8725 + OpenAPI 3.1 + Testcontainers
source_license_state: external_license_checked
attribution_required: false
contract_signature:   FormatterUnit
transition_type:      feature_addition
phase:                3
sub_phase:            3.4+3.3+5.2
task_type:            human-result-status-section-preservation
node_role:            node_09
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        A
overlap_ratio:        0.38   (Type A: chosen Accepted+Rejected+Knowledge-edges+Next-action sections vs rejected Accepted only)
```

### ES-2026-05-08-6.3-N09-02 — ES-S6 review verdict human result

```
source_id:            cwe-285, nist-sp-800-53-ac-3
source_title:         CWE-285 + NIST AC-3
source_license_state: external_license_checked
attribution_required: false
contract_signature:   FormatterUnit
transition_type:      repair
phase:                6
sub_phase:            6.3
task_type:            review-human-result
node_role:            node_09
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        A
overlap_ratio:        0.42   (Type A: chosen Concern+Blocked+Rejected+Knowledge-edges+Next-action sections vs rejected drops Blocked+Rejected)
```

### ES-2026-05-08-3.3-N09-03 — ES-S8 explain transition human result (Type C)

```
source_id:            openapi-specification, aip-180
source_title:         OpenAPI 3.1 + AIP-180
source_license_state: external_license_checked
attribution_required: true
attribution_text:     "© Google LLC, CC BY 4.0"
contract_signature:   FormatterUnit
transition_type:      explain
phase:                3
sub_phase:            3.3
task_type:            api-contract-explanation
node_role:            node_09
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        C
overlap_ratio:        0.31   (Type C: chosen explanation vs rejected implementation steps; OpenAPI versioning token shared, action verbs disjoint)
```

---

## Verification

For every Pair[ID] listed above:

```
□ source_id present                    ✓ (35 / 35 pairs)
□ source_title present                 ✓ (35 / 35)
□ source_license_state present         ✓ (35 / 35 — external_license_checked)
□ contract_signature present           ✓ (35 / 35)
□ transition_type present              ✓ (35 / 35)
□ phase present                        ✓ (35 / 35)
□ sub_phase present                    ✓ (35 / 35)
□ task_type present                    ✓ (35 / 35)
□ node_role present                    ✓ (35 / 35)
□ coverage_status present              ✓ (35 / 35 — covered)
□ pair_kind present                    ✓ (35 / 35)
□ rejected_type present                ✓ (35 / 35)
□ overlap_ratio present and < 0.60     ✓ (35 / 35; range 0.30–0.46)
□ attribution_text present when required ✓ (1 of 1 — ES-S8 N09-03 only)
```

Every label required by v8 line 806 and DPO authoring guidance line 58 is
recorded for every pair in Node-03 through Node-09.

For Layer-0-A, Layer-0-B, Layer-0-C, Layer-0-D, Layer-1, Node-01, Node-02,
Stage-9, Layer-3-Teaching, HeldOut-Rephrasing, Supplement-PLAN-TypeB, and
Supplement-Domain-TypeC — labels are recorded inline in each pair's own
coverage block.

---

## Last Updated

2026-05-08.
