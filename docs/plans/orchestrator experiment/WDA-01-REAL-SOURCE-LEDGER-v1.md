# WDA-01 Real Source Ledger v1
# Date: 2026-05-05

---

## Purpose

This ledger replaces the WDA-01 hand-authored source-knowledge-edge packets
with local Tier 1 source entries for all 31 canonical development sub-phases.

The ledger does not claim the broad v4 corpus is complete. It satisfies the
Wave 1 seed requirement: one ready source entry per sub-phase so the real
`sub-phase-router` logical unit can replace the deterministic fixture.

---

## Common Entry Fields

These fields apply to every row below unless the row says otherwise.

```text
source_tier: Tier 1 - Local project document
license_or_authority: Local project authority
train_or_held_out: train
promotion_status: WDA-01 router replacement seed
expected_knowledge_edges: none
expected_conflicts: none
```

---

## Ledger Rows

| document_id | source_path_or_url | primary_phase_id | primary_sub_phase_id | stratum | document_type | three_verifiable_claims | known_failure_mode | expected_top_predicates |
|---|---|---|---|---|---|---|---|---|
| WDA-01-SOURCE-01 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\DEVELOPMENT-PHASES-AND-DOCUMENT-IDENTIFICATION-GUIDE-v1.md` | phase-1-ui-ux-definition | 1.1-design-system-and-tokens | A-domain-breadth | phase-sub-phase-source | tokens define visual contract; typography belongs to design system; spacing and component rules carry forward | wrong-domain routing to screen layout | VALIDATE_OP, GET_OP, RETURN |
| WDA-01-SOURCE-02 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\DEVELOPMENT-PHASES-AND-DOCUMENT-IDENTIFICATION-GUIDE-v1.md` | phase-1-ui-ux-definition | 1.2-screen-and-layout-structure | A-domain-breadth | phase-sub-phase-source | screen inventory is a design concern; layout hierarchy precedes implementation; responsive structure is a source constraint | wrong-domain routing to frontend implementation | CREATE, ITERATE, RETURN |
| WDA-01-SOURCE-03 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\DEVELOPMENT-PHASES-AND-DOCUMENT-IDENTIFICATION-GUIDE-v1.md` | phase-1-ui-ux-definition | 1.3-interaction-model-and-state-transitions | A-domain-breadth | phase-sub-phase-source | interaction states define behavior; loading states belong to UX design; error states carry to implementation | wrong-domain routing to testing scenarios | BRANCH, UPDATE_OP, RETURN |
| WDA-01-SOURCE-04 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\DEVELOPMENT-PHASES-AND-DOCUMENT-IDENTIFICATION-GUIDE-v1.md` | phase-1-ui-ux-definition | 1.4-accessibility-and-compliance | D-failure-edge-case | phase-sub-phase-source | accessibility belongs to design; compliance is a design constraint; failures must remain visible | wrong-domain routing to QA accessibility audit | VALIDATE_OP, FAIL, RETURN |
| WDA-01-SOURCE-05 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\DEVELOPMENT-PHASES-AND-DOCUMENT-IDENTIFICATION-GUIDE-v1.md` | phase-1-ui-ux-definition | 1.5-motion-and-animation | C-depth-level | phase-sub-phase-source | motion is a design behavior; animation has boundaries; reduced-motion rules constrain later phases | wrong-domain routing to client rendering | PROCESS_OP, BRANCH, RETURN |
| WDA-01-SOURCE-06 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\DEVELOPMENT-PHASES-AND-DOCUMENT-IDENTIFICATION-GUIDE-v1.md` | phase-2-functional-specification | 2.1-user-stories-and-acceptance-criteria | A-domain-breadth | phase-sub-phase-source | user stories define actors; acceptance criteria define pass conditions; criteria feed testing | wrong-domain routing to QA functional review | DECLARE, VALIDATE_OP, RETURN |
| WDA-01-SOURCE-07 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\DEVELOPMENT-PHASES-AND-DOCUMENT-IDENTIFICATION-GUIDE-v1.md` | phase-2-functional-specification | 2.2-business-rules | D-failure-edge-case | phase-sub-phase-source | business rules name allowed outcomes; rejected outcomes are constraints; conflicts block downstream work | wrong-domain routing to security authorization | BRANCH, FAIL, VALIDATE_OP |
| WDA-01-SOURCE-08 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\DEVELOPMENT-PHASES-AND-DOCUMENT-IDENTIFICATION-GUIDE-v1.md` | phase-2-functional-specification | 2.3-integration-point-specification | B-team-context | phase-sub-phase-source | integration points define handoffs; provider boundaries must be explicit; external dependencies become later constraints | wrong-domain routing to provider connector implementation | SEND_OP, AUTH_OP, HANDLE_ERROR |
| WDA-01-SOURCE-09 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\DEVELOPMENT-PHASES-AND-DOCUMENT-IDENTIFICATION-GUIDE-v1.md` | phase-2-functional-specification | 2.4-data-model-boundaries | B-team-context | phase-sub-phase-source | data boundaries define ownership; privacy constraints carry forward; lifecycle rules affect schema design | wrong-domain routing to migration design | CREATE, GET_OP, VALIDATE_OP |
| WDA-01-SOURCE-10 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\MULTI-PHASE-ORCHESTRATION-DESIGN-AND-EXPERIMENT-PLAN-v1.md` | phase-3-technical-design | 3.1-architecture-and-component-topology | A-domain-breadth | phase-sub-phase-source | architecture defines component topology; service boundaries constrain implementation; runtime flow decisions carry forward | wrong-domain routing to implementation backend services | DECLARE, CREATE, RETURN |
| WDA-01-SOURCE-11 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\MULTI-PHASE-ORCHESTRATION-DESIGN-AND-EXPERIMENT-PLAN-v1.md` | phase-3-technical-design | 3.2-data-schema-and-migration-design | C-depth-level | phase-sub-phase-source | schema decisions precede migration; rollback belongs to design; compatibility must be preserved | wrong-domain routing to migration execution | CREATE_OP, UPDATE_OP, VALIDATE_OP |
| WDA-01-SOURCE-12 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\MULTI-PHASE-ORCHESTRATION-DESIGN-AND-EXPERIMENT-PLAN-v1.md` | phase-3-technical-design | 3.3-api-contract-definition | B-team-context | phase-sub-phase-source | request contracts define inputs; response contracts define outputs; auth and error contracts constrain implementation | wrong-domain routing to integration point specification | DECLARE, SEND_OP, AUTH_OP |
| WDA-01-SOURCE-13 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\MULTI-PHASE-ORCHESTRATION-DESIGN-AND-EXPERIMENT-PLAN-v1.md` | phase-3-technical-design | 3.4-security-and-authorization-model | D-failure-edge-case | phase-sub-phase-source | authorization is a design model; identity constraints must carry forward; rejection rules outrank style preferences | wrong-domain routing to QA security review | AUTH_OP, VALIDATE_OP, FAIL |
| WDA-01-SOURCE-14 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\MULTI-PHASE-ORCHESTRATION-DESIGN-AND-EXPERIMENT-PLAN-v1.md` | phase-3-technical-design | 3.5-infrastructure-and-deployment-topology | B-team-context | phase-sub-phase-source | deployment topology belongs to design; observability is a design constraint; rollback and scaling affect implementation | wrong-domain routing to CI/CD gate configuration | CREATE, PROCESS_OP, HANDLE_ERROR |
| WDA-01-SOURCE-15 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\LOGICAL-UNIT-FLOW-ORCHESTRATOR-IMPLEMENTATION-PHASE-PLAN-v1.md` | phase-4-implementation | 4.1-backend-services | A-domain-breadth | phase-sub-phase-source | backend services implement contracts; interfaces preserve design boundaries; errors must be handled locally | wrong-domain routing to API contract definition | CALL, CREATE_OP, HANDLE_ERROR |
| WDA-01-SOURCE-16 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\LOGICAL-UNIT-FLOW-ORCHESTRATOR-IMPLEMENTATION-PHASE-PLAN-v1.md` | phase-4-implementation | 4.2-frontend-and-client-rendering | B-team-context | phase-sub-phase-source | client rendering implements UI states; data binding belongs to frontend implementation; rendering states depend on design | wrong-domain routing to screen layout design | GET_OP, UPDATE_OP, RETURN |
| WDA-01-SOURCE-17 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\LOGICAL-UNIT-FLOW-ORCHESTRATOR-IMPLEMENTATION-PHASE-PLAN-v1.md` | phase-4-implementation | 4.3-integration-and-provider-connectors | B-team-context | phase-sub-phase-source | connectors implement provider handoffs; provider auth must be handled; failure paths remain explicit | wrong-domain routing to integration point specification | SEND_OP, AUTH_OP, HANDLE_ERROR |
| WDA-01-SOURCE-18 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\LOGICAL-UNIT-FLOW-ORCHESTRATOR-IMPLEMENTATION-PHASE-PLAN-v1.md` | phase-4-implementation | 4.4-data-migration-execution | C-depth-level | phase-sub-phase-source | migrations execute in batches; rollback evidence must exist; migration logs become test evidence | wrong-domain routing to schema design | ITERATE, UPDATE_OP, LOG_OP |
| WDA-01-SOURCE-19 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\LOGICAL-UNIT-FLOW-ORCHESTRATOR-IMPLEMENTATION-PHASE-PLAN-v1.md` | phase-4-implementation | 4.5-documentation | B-team-context | phase-sub-phase-source | documentation records implementation decisions; audience mapping changes wording; trace publication supports review | wrong-domain routing to formatter output | CONVERT_OP, RETURN, VALIDATE_OP |
| WDA-01-SOURCE-20 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\ARTIFACT-EXAMINATION-PROTOCOL-EXPERIMENT-PLAN-v1.0.md` | phase-5-testing | 5.1-unit-and-contract-tests | A-domain-breadth | phase-sub-phase-source | unit tests check local behavior; contract tests check boundaries; failing assertions block promotion | wrong-domain routing to QA regression review | VALIDATE_OP, FAIL, RETURN |
| WDA-01-SOURCE-21 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\ARTIFACT-EXAMINATION-PROTOCOL-EXPERIMENT-PLAN-v1.0.md` | phase-5-testing | 5.2-integration-tests | B-team-context | phase-sub-phase-source | integration tests cross services; service failures must be simulated; connector evidence belongs here | wrong-domain routing to provider connector implementation | CALL, SEND_OP, HANDLE_ERROR |
| WDA-01-SOURCE-22 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\ARTIFACT-EXAMINATION-PROTOCOL-EXPERIMENT-PLAN-v1.0.md` | phase-5-testing | 5.3-end-to-end-scenario-tests | C-depth-level | phase-sub-phase-source | end-to-end tests follow user journeys; state verification is required; full scenario traces preserve context | wrong-domain routing to user stories | ITERATE, VALIDATE_OP, RETURN |
| WDA-01-SOURCE-23 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\ARTIFACT-EXAMINATION-PROTOCOL-EXPERIMENT-PLAN-v1.0.md` | phase-5-testing | 5.4-performance-and-load-tests | D-failure-edge-case | phase-sub-phase-source | load tests measure capacity; latency budgets can fail; degradation evidence must remain visible | wrong-domain routing to infrastructure topology | PROCESS_OP, LOG_OP, FAIL |
| WDA-01-SOURCE-24 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\ARTIFACT-EXAMINATION-PROTOCOL-EXPERIMENT-PLAN-v1.0.md` | phase-6-qa | 6.1-functional-review | A-domain-breadth | phase-sub-phase-source | functional review checks acceptance criteria; failed behavior is not accepted; review produces human decision evidence | wrong-domain routing to acceptance criteria writing | VALIDATE_OP, RETURN, FAIL |
| WDA-01-SOURCE-25 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\ARTIFACT-EXAMINATION-PROTOCOL-EXPERIMENT-PLAN-v1.0.md` | phase-6-qa | 6.2-regression-coverage-verification | C-depth-level | phase-sub-phase-source | regression review checks changed areas; preserved behaviors must remain visible; coverage gaps create next actions | wrong-domain routing to regression suite maintenance | ITERATE, VALIDATE_OP, LOG_OP |
| WDA-01-SOURCE-26 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\ARTIFACT-EXAMINATION-PROTOCOL-EXPERIMENT-PLAN-v1.0.md` | phase-6-qa | 6.3-security-and-compliance-review | D-failure-edge-case | phase-sub-phase-source | security review checks authority rules; compliance evidence can block release; audit obligations carry forward | wrong-domain routing to security design | AUTH_OP, FAIL, VALIDATE_OP |
| WDA-01-SOURCE-27 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\ARTIFACT-EXAMINATION-PROTOCOL-EXPERIMENT-PLAN-v1.0.md` | phase-6-qa | 6.4-accessibility-audit | D-failure-edge-case | phase-sub-phase-source | accessibility audit checks keyboard behavior; screen reader evidence matters; contrast failures cannot be hidden | wrong-domain routing to design accessibility | VALIDATE_OP, FAIL, RETURN |
| WDA-01-SOURCE-28 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\CORPUS-EXPANSION-PLAN-v4.md` | phase-7-qa-automation | 7.1-test-scenario-automation | A-domain-breadth | phase-sub-phase-source | automation scenarios turn review evidence into steps; fixtures support repeatability; automation should not invent pass states | wrong-domain routing to end-to-end scenario tests | CREATE, ITERATE, CALL |
| WDA-01-SOURCE-29 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\CORPUS-EXPANSION-PLAN-v4.md` | phase-7-qa-automation | 7.2-regression-suite-maintenance | C-depth-level | phase-sub-phase-source | regression suites evolve after changes; grouping supports ownership; deletion must preserve evidence | wrong-domain routing to QA regression verification | UPDATE_OP, ITERATE, LOG_OP |
| WDA-01-SOURCE-30 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\CORPUS-EXPANSION-PLAN-v4.md` | phase-7-qa-automation | 7.3-ci-cd-gate-configuration | B-team-context | phase-sub-phase-source | CI gates enforce thresholds; failed gates block deployment; deployment rules belong to automation | wrong-domain routing to infrastructure deployment design | BRANCH, FAIL, SEND_OP |
| WDA-01-SOURCE-31 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\CORPUS-EXPANSION-PLAN-v4.md` | phase-7-qa-automation | 7.4-coverage-and-reporting | B-team-context | phase-sub-phase-source | coverage reports publish evidence; dashboards summarize status; metrics guide next learning actions | wrong-domain routing to QA coverage review | GET_OP, CONVERT_OP, RETURN |

---

## Gate Result

All 31 ledger rows are ready for the WDA-01 router replacement proof.

This ledger is the Wave 1 seed. It is not the final 3,100-document corpus.
