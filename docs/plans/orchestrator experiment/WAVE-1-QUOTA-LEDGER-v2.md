# Wave 1 Quota Ledger v2
# Date: 2026-05-05

---

## Purpose

Update the Wave 1 quota ledger after accepting CB4 as license-cleared external
source material for sub-phase 3.4.

This ledger still does not declare corpus v4 complete.

---

## Change From v1

CB4 material is now accepted as usable Tier 2 pair corpus for:

```text
3.4-security-and-authorization-model
```

Current CB4 material:

```text
892 pair instances
446 base pairs
100 original training instances
20 original held-out/test instances
784 additional training-material candidate instances
```

---

## Quota Summary

| Scope | Required by v4 | Current promoted source docs | Current accepted pair instances | Status |
|---|---:|---:|---:|---|
| training documents | 3,100 | 0 | 892 pair instances for 3.4 | open |
| sub-phase held-out documents | 620 | 0 | 20 CB4 test instances | open |
| transition held-out documents | 60 | 0 | 0 | open |
| total held-out documents | 680 | 0 | 20 CB4 test instances | open |

Why the promoted source-doc count remains zero:

CB4 is already translated into pair instances. The original source documents
are four standards families, not 446 separate source documents. The pair corpus
is valuable and usable, but it must be tracked separately from the v4
document-count quota.

---

## Per-Sub-Phase Status

| sub_phase_id | seed_rows_ready | accepted_pair_instances | accepted_base_pairs | held_out_or_test_instances | training_doc_quota_status | pair_corpus_status |
|---|---:|---:|---:|---:|---|---|
| 1.1-design-system-and-tokens | 1 | 0 | 0 | 0 | open | not started |
| 1.2-screen-and-layout-structure | 1 | 0 | 0 | 0 | open | not started |
| 1.3-interaction-model-and-state-transitions | 1 | 0 | 0 | 0 | open | not started |
| 1.4-accessibility-and-compliance | 1 | 0 | 0 | 0 | open | not started |
| 1.5-motion-and-animation | 1 | 0 | 0 | 0 | open | not started |
| 2.1-user-stories-and-acceptance-criteria | 1 | 0 | 0 | 0 | open | not started |
| 2.2-business-rules | 1 | 0 | 0 | 0 | open | not started |
| 2.3-integration-point-specification | 1 | 0 | 0 | 0 | open | not started |
| 2.4-data-model-boundaries | 1 | 0 | 0 | 0 | open | not started |
| 3.1-architecture-and-component-topology | 1 | 0 | 0 | 0 | open | planned center-phase batch |
| 3.2-data-schema-and-migration-design | 1 | 0 | 0 | 0 | open | not started |
| 3.3-api-contract-definition | 1 | 0 | 0 | 0 | open | planned center-phase batch |
| 3.4-security-and-authorization-model | 1 | 892 | 446 | 20 | open | accepted CB4 pair corpus; Stage 5 pending |
| 3.5-infrastructure-and-deployment-topology | 1 | 0 | 0 | 0 | open | not started |
| 4.1-backend-services | 1 | 0 | 0 | 0 | open | planned center-phase batch |
| 4.2-frontend-and-client-rendering | 1 | 0 | 0 | 0 | open | not started |
| 4.3-integration-and-provider-connectors | 1 | 0 | 0 | 0 | open | planned center-phase batch |
| 4.4-data-migration-execution | 1 | 0 | 0 | 0 | open | not started |
| 4.5-documentation | 1 | 0 | 0 | 0 | open | not started |
| 5.1-unit-and-contract-tests | 1 | 0 | 0 | 0 | open | not started |
| 5.2-integration-tests | 1 | 0 | 0 | 0 | open | not started |
| 5.3-end-to-end-scenario-tests | 1 | 0 | 0 | 0 | open | not started |
| 5.4-performance-and-load-tests | 1 | 0 | 0 | 0 | open | not started |
| 6.1-functional-review | 1 | 0 | 0 | 0 | open | not started |
| 6.2-regression-coverage-verification | 1 | 0 | 0 | 0 | open | not started |
| 6.3-security-and-compliance-review | 1 | 0 | 0 | 0 | open | not started |
| 6.4-accessibility-audit | 1 | 0 | 0 | 0 | open | not started |
| 7.1-test-scenario-automation | 1 | 0 | 0 | 0 | open | not started |
| 7.2-regression-suite-maintenance | 1 | 0 | 0 | 0 | open | not started |
| 7.3-ci-cd-gate-configuration | 1 | 0 | 0 | 0 | open | not started |
| 7.4-coverage-and-reporting | 1 | 0 | 0 | 0 | open | not started |

---

## CB4 Stage 5 Requirement

CB4 cannot be marked as an orchestrator-learning success until:

1. `security-verification-3.4` logical flow is registered;
2. the nine-node security pipeline is connected;
3. S1 PASS, S2 REPROCESS, and S3 CONCERN oracle scenarios run;
4. mandatory checks from the OWASP ASVS human-result standard pass;
5. WDA and full suite stay green.

---

## Next Action

Start Phase 84:

```text
CB4 Stage 5 security-verification-3.4 flow registration plan
```

The flow should start as a small deterministic logical flow before attempting
to consume all 892 instances.
