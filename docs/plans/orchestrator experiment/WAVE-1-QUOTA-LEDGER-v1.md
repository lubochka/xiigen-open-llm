# Wave 1 Quota Ledger v1
# Date: 2026-05-05

---

## Purpose

Track the v4 corpus quota after the WDA-01 real-router seed proof.

This ledger prevents the 31 seed rows from being mistaken for the completed
corpus. A sub-phase is not quota-complete until it has 100 promoted training
documents and 20 promoted held-out documents with the required strata.

---

## Quota Summary

| Scope | Required | Current promoted | Current seed | Status |
|---|---:|---:|---:|---|
| training documents | 3,100 | 0 | 31 | open |
| sub-phase held-out documents | 620 | 0 | 0 | open |
| transition held-out documents | 60 | 0 | 0 | open |
| total held-out documents | 680 | 0 | 0 | open |

---

## Per-Sub-Phase Quota

| sub_phase_id | training_required | training_promoted | seed_rows_ready | held_out_required | held_out_promoted | status |
|---|---:|---:|---:|---:|---:|---|
| 1.1-design-system-and-tokens | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 1.2-screen-and-layout-structure | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 1.3-interaction-model-and-state-transitions | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 1.4-accessibility-and-compliance | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 1.5-motion-and-animation | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 2.1-user-stories-and-acceptance-criteria | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 2.2-business-rules | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 2.3-integration-point-specification | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 2.4-data-model-boundaries | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 3.1-architecture-and-component-topology | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 3.2-data-schema-and-migration-design | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 3.3-api-contract-definition | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 3.4-security-and-authorization-model | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 3.5-infrastructure-and-deployment-topology | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 4.1-backend-services | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 4.2-frontend-and-client-rendering | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 4.3-integration-and-provider-connectors | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 4.4-data-migration-execution | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 4.5-documentation | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 5.1-unit-and-contract-tests | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 5.2-integration-tests | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 5.3-end-to-end-scenario-tests | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 5.4-performance-and-load-tests | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 6.1-functional-review | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 6.2-regression-coverage-verification | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 6.3-security-and-compliance-review | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 6.4-accessibility-audit | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 7.1-test-scenario-automation | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 7.2-regression-suite-maintenance | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 7.3-ci-cd-gate-configuration | 100 | 0 | 1 | 20 | 0 | seed-ready |
| 7.4-coverage-and-reporting | 100 | 0 | 1 | 20 | 0 | seed-ready |

---

## Promotion Rule

The 31 seed rows are allowed to train or validate the first router
replacement. They do not count as promoted quota rows until each row has:

1. source ledger fields populated;
2. pair file generated for the relevant logical unit;
3. conformance suite entry;
4. held-out classification status assigned;
5. manifest and zip for the batch that introduced it.

---

## Next Work

Wave 1 must now expand each sub-phase from one seed row to the full stratum
shape:

```text
25 training rows from Stratum A
25 training rows from Stratum B
25 training rows from Stratum C
25 training rows from Stratum D
5 held-out rows per held-out category
```

The next executable corpus batch should pick one center-phase group first:

```text
3.1 Architecture and component topology
3.3 API contract definition
3.4 Security and authorization model
4.1 Backend services
4.3 Integration and provider connectors
```

Those five sub-phases align with v4 Wave 2 and have the strongest current
source coverage.
