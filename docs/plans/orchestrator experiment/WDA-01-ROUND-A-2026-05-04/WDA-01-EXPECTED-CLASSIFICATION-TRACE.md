# WDA-01 Expected Classification Trace
# Document to sub-phase classification
# Round A - Oracle trace
# Date: 2026-05-04

---

## Trace Purpose

This trace defines what WDA-01 Round B must prove. The orchestrator must take
one source document or source packet per canonical sub-phase and produce the
correct phase and sub-phase classification with confidence and wrong-domain
guard evidence.

---

## Input Summary

Input count: 31 classification probes.

Each probe contains:

- source document or hand-authored source packet
- source authority tier
- expected phase id
- expected sub-phase id
- expected top-three predicate profile
- known failure mode

---

## Required Output Shape Per Probe

```json
{
  "probe_id": "WDA-01-10",
  "source_status": "ready",
  "predicted_phase_id": "phase-3-technical-design",
  "predicted_sub_phase_id": "3.1-architecture-and-component-topology",
  "confidence": 0.70,
  "top_predicates": ["DECLARE", "CREATE", "RETURN"],
  "nearest_wrong_sub_phase_score": 0.49,
  "knowledge_edge": false,
  "failure_classification": "none"
}
```

If the source is missing:

```json
{
  "probe_id": "WDA-01-05",
  "source_status": "knowledge_edge",
  "predicted_phase_id": "phase-1-ui-ux-definition",
  "predicted_sub_phase_id": "1.5-motion-and-animation",
  "confidence": 0.70,
  "top_predicates": ["PROCESS_OP", "BRANCH", "RETURN"],
  "nearest_wrong_sub_phase_score": 0.49,
  "knowledge_edge": true,
  "failure_classification": "source_missing"
}
```

The hand-authored packet may prove routing shape, but it does not count as a
corpus source document.

---

## Expected Probe Results

| Probe | Expected phase id | Expected sub-phase id | Expected predicates |
|---|---|---|---|
| WDA-01-01 | phase-1-ui-ux-definition | 1.1-design-system-and-tokens | VALIDATE_OP, GET_OP, RETURN |
| WDA-01-02 | phase-1-ui-ux-definition | 1.2-screen-and-layout-structure | CREATE, ITERATE, RETURN |
| WDA-01-03 | phase-1-ui-ux-definition | 1.3-interaction-model-and-state-transitions | BRANCH, UPDATE_OP, RETURN |
| WDA-01-04 | phase-1-ui-ux-definition | 1.4-accessibility-and-compliance | VALIDATE_OP, FAIL, RETURN |
| WDA-01-05 | phase-1-ui-ux-definition | 1.5-motion-and-animation | PROCESS_OP, BRANCH, RETURN |
| WDA-01-06 | phase-2-functional-specification | 2.1-user-stories-and-acceptance-criteria | DECLARE, VALIDATE_OP, RETURN |
| WDA-01-07 | phase-2-functional-specification | 2.2-business-rules | BRANCH, FAIL, VALIDATE_OP |
| WDA-01-08 | phase-2-functional-specification | 2.3-integration-point-specification | SEND_OP, AUTH_OP, HANDLE_ERROR |
| WDA-01-09 | phase-2-functional-specification | 2.4-data-model-boundaries | CREATE, GET_OP, VALIDATE_OP |
| WDA-01-10 | phase-3-technical-design | 3.1-architecture-and-component-topology | DECLARE, CREATE, RETURN |
| WDA-01-11 | phase-3-technical-design | 3.2-data-schema-and-migration-design | CREATE_OP, UPDATE_OP, VALIDATE_OP |
| WDA-01-12 | phase-3-technical-design | 3.3-api-contract-definition | DECLARE, SEND_OP, AUTH_OP |
| WDA-01-13 | phase-3-technical-design | 3.4-security-and-authorization-model | AUTH_OP, VALIDATE_OP, FAIL |
| WDA-01-14 | phase-3-technical-design | 3.5-infrastructure-and-deployment-topology | CREATE, PROCESS_OP, HANDLE_ERROR |
| WDA-01-15 | phase-4-implementation | 4.1-backend-services | CALL, CREATE_OP, HANDLE_ERROR |
| WDA-01-16 | phase-4-implementation | 4.2-frontend-and-client-rendering | GET_OP, UPDATE_OP, RETURN |
| WDA-01-17 | phase-4-implementation | 4.3-integration-and-provider-connectors | SEND_OP, AUTH_OP, HANDLE_ERROR |
| WDA-01-18 | phase-4-implementation | 4.4-data-migration-execution | ITERATE, UPDATE_OP, LOG_OP |
| WDA-01-19 | phase-4-implementation | 4.5-documentation | CONVERT_OP, RETURN, VALIDATE_OP |
| WDA-01-20 | phase-5-testing | 5.1-unit-and-contract-tests | VALIDATE_OP, FAIL, RETURN |
| WDA-01-21 | phase-5-testing | 5.2-integration-tests | CALL, SEND_OP, HANDLE_ERROR |
| WDA-01-22 | phase-5-testing | 5.3-end-to-end-scenario-tests | ITERATE, VALIDATE_OP, RETURN |
| WDA-01-23 | phase-5-testing | 5.4-performance-and-load-tests | PROCESS_OP, LOG_OP, FAIL |
| WDA-01-24 | phase-6-qa | 6.1-functional-review | VALIDATE_OP, RETURN, FAIL |
| WDA-01-25 | phase-6-qa | 6.2-regression-coverage-verification | ITERATE, VALIDATE_OP, LOG_OP |
| WDA-01-26 | phase-6-qa | 6.3-security-and-compliance-review | AUTH_OP, FAIL, VALIDATE_OP |
| WDA-01-27 | phase-6-qa | 6.4-accessibility-audit | VALIDATE_OP, FAIL, RETURN |
| WDA-01-28 | phase-7-qa-automation | 7.1-test-scenario-automation | CREATE, ITERATE, CALL |
| WDA-01-29 | phase-7-qa-automation | 7.2-regression-suite-maintenance | UPDATE_OP, ITERATE, LOG_OP |
| WDA-01-30 | phase-7-qa-automation | 7.3-ci-cd-gate-configuration | BRANCH, FAIL, SEND_OP |
| WDA-01-31 | phase-7-qa-automation | 7.4-coverage-and-reporting | GET_OP, CONVERT_OP, RETURN |

---

## Required Aggregate Metrics

```json
{
  "classification_count": 31,
  "correct_phase_count": 31,
  "correct_sub_phase_count": 31,
  "min_confidence": 0.70,
  "wrong_domain_high_confidence_count": 0,
  "max_wrong_domain_score": 0.49,
  "knowledge_edge_when_document_missing": true,
  "source_knowledge_edge_count": 0,
  "e2e_classification_success": true
}
```

`source_knowledge_edge_count` may be greater than zero if the executor cannot
find ready source documents for all sub-phases. That does not fail WDA-01 if
the source gaps are named. It does block broad corpus intake for those
sub-phases.

---

## Failure Classification

| Failure | Classification | Next action |
|---|---|---|
| wrong phase | phase-router failure | add or correct sub-phase-router pair for phase-level distinction |
| right phase, wrong sub-phase | sub-phase-router failure | add or correct pair for neighboring sub-phase distinction |
| confidence below 0.70 | thin evidence | add one original plus one phrasing B for that sub-phase |
| wrong-domain score >= 0.50 | boundary bleed | add wrong-domain guard pair and domain-scoped retrieval check |
| missing source not marked knowledge edge | source authority failure | fix source ledger and formatter evidence |
| source packet lacks three claims | source readiness failure | replace source packet before classification proof |

---

## Promotion Rule

WDA-01 promotes only if:

1. all 31 probes produce expected phase and sub-phase ids, or missing real
   sources are named as knowledge edges with hand-authored packets used only
   for routing-shape proof
2. every ready-source probe has confidence >= 0.70
3. no wrong-domain neighbor reaches 0.50
4. the source ledger identifies which sub-phases can enter corpus Wave 1

Passing WDA-01 authorizes WDA-02 Phase 3 Round A. It does not authorize broad
corpus expansion by itself.
