# WDA-01 Router Replacement Expected Trace v1
# Date: 2026-05-05

---

## Purpose

Define the expected evidence for replacing the WDA-01 deterministic
classification fixture with the real `SubPhaseRouterUnit`.

---

## Trace Contract

```text
experiment_id: WDA-01
round: real-router-replacement
mode: real_source_sub_phase_router_unit
probe_count: 31
selected_source_count: 31
source_file_exists_count: 31
router_unit_id: unit-sub-phase-router-v1
deterministic_fixture_used: false
```

---

## Required Machine Metrics

| Metric | Expected value |
|---|---:|
| classification_count | 31 |
| correct_phase_count | 31 |
| correct_sub_phase_count | 31 |
| ready_source_count | 31 |
| source_knowledge_edge_count | 0 |
| router_knowledge_edge_count | 0 |
| missing_source_without_knowledge_edge_count | 0 |
| min_confidence | >= 0.70 |
| max_wrong_domain_score | < 0.50 |
| wrong_domain_high_confidence_count | 0 |
| deterministic_fixture_used | false |
| broad_wave1_seed_ready | true |

---

## Required Human Result

The human result must include:

```text
runtime_unit: sub-phase-router unit executed
ready_source_count: 31
source_knowledge_edge_count: 0
correct_phase_count: 31
correct_sub_phase_count: 31
next_action: start Wave 1 source-ledger expansion
```

The human result must not include:

```text
runtime_gap: sub-phase-router unit not yet implemented
broad corpus intake blocked for 31 sub-phases
```

---

## Expected Classification Table

| document_id | expected_phase_id | expected_sub_phase_id |
|---|---|---|
| WDA-01-SOURCE-01 | phase-1-ui-ux-definition | 1.1-design-system-and-tokens |
| WDA-01-SOURCE-02 | phase-1-ui-ux-definition | 1.2-screen-and-layout-structure |
| WDA-01-SOURCE-03 | phase-1-ui-ux-definition | 1.3-interaction-model-and-state-transitions |
| WDA-01-SOURCE-04 | phase-1-ui-ux-definition | 1.4-accessibility-and-compliance |
| WDA-01-SOURCE-05 | phase-1-ui-ux-definition | 1.5-motion-and-animation |
| WDA-01-SOURCE-06 | phase-2-functional-specification | 2.1-user-stories-and-acceptance-criteria |
| WDA-01-SOURCE-07 | phase-2-functional-specification | 2.2-business-rules |
| WDA-01-SOURCE-08 | phase-2-functional-specification | 2.3-integration-point-specification |
| WDA-01-SOURCE-09 | phase-2-functional-specification | 2.4-data-model-boundaries |
| WDA-01-SOURCE-10 | phase-3-technical-design | 3.1-architecture-and-component-topology |
| WDA-01-SOURCE-11 | phase-3-technical-design | 3.2-data-schema-and-migration-design |
| WDA-01-SOURCE-12 | phase-3-technical-design | 3.3-api-contract-definition |
| WDA-01-SOURCE-13 | phase-3-technical-design | 3.4-security-and-authorization-model |
| WDA-01-SOURCE-14 | phase-3-technical-design | 3.5-infrastructure-and-deployment-topology |
| WDA-01-SOURCE-15 | phase-4-implementation | 4.1-backend-services |
| WDA-01-SOURCE-16 | phase-4-implementation | 4.2-frontend-and-client-rendering |
| WDA-01-SOURCE-17 | phase-4-implementation | 4.3-integration-and-provider-connectors |
| WDA-01-SOURCE-18 | phase-4-implementation | 4.4-data-migration-execution |
| WDA-01-SOURCE-19 | phase-4-implementation | 4.5-documentation |
| WDA-01-SOURCE-20 | phase-5-testing | 5.1-unit-and-contract-tests |
| WDA-01-SOURCE-21 | phase-5-testing | 5.2-integration-tests |
| WDA-01-SOURCE-22 | phase-5-testing | 5.3-end-to-end-scenario-tests |
| WDA-01-SOURCE-23 | phase-5-testing | 5.4-performance-and-load-tests |
| WDA-01-SOURCE-24 | phase-6-qa | 6.1-functional-review |
| WDA-01-SOURCE-25 | phase-6-qa | 6.2-regression-coverage-verification |
| WDA-01-SOURCE-26 | phase-6-qa | 6.3-security-and-compliance-review |
| WDA-01-SOURCE-27 | phase-6-qa | 6.4-accessibility-audit |
| WDA-01-SOURCE-28 | phase-7-qa-automation | 7.1-test-scenario-automation |
| WDA-01-SOURCE-29 | phase-7-qa-automation | 7.2-regression-suite-maintenance |
| WDA-01-SOURCE-30 | phase-7-qa-automation | 7.3-ci-cd-gate-configuration |
| WDA-01-SOURCE-31 | phase-7-qa-automation | 7.4-coverage-and-reporting |

---

## Failure Classification

If the test fails, classify it as one of:

- `source_ledger_gap`: a selected source path is missing or not ready;
- `router_confidence_gap`: minimum confidence drops below 0.70;
- `wrong_domain_bleed`: nearest wrong-domain score reaches 0.50 or higher;
- `classification_drift`: phase or sub-phase id differs from oracle;
- `router_knowledge_edge`: ready source still becomes a knowledge edge.

The next action must target the failing class rather than broad corpus writing.
