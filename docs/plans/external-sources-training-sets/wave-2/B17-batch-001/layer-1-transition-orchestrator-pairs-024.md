# Layer 1 — Transition Orchestrator Pairs (024)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: Complete orchestration reference for B17 connectors 013-024

---

## B17-L1-024-001 — Complete orchestration pattern matrix for connectors 013-024

| Predicate set | Total flows | Steps | Parallel in Step 2 | QA gate |
|---|---|---|---|---|
| [IMPL, API] | 2 | 2 (seq or parallel) | No | No |
| [IMPL, API, SEC] | 3 | 2 | SEC parallel with API | No |
| [IMPL, API, SEC, AUTH] | 4 | 2 | SEC+AUTH+API parallel | No |
| [IMPL, API, TEST] | 3 | 2 | TEST after Step 1 | No |
| [IMPL, API, DOC] | 3 | 2 | DOC parallel Step 1 | No |
| [IMPL, API, TEST, DOC] | 4 | 2 | DOC parallel Step 1; TEST after Step 1 | No |
| [IMPL, API, TEST, QA] | 4 | 4 | None | Yes (Step 4) |
| [IMPL, API, SEC, AUTH, TEST] | 5 | 3 | SEC+AUTH+API parallel; TEST after Step 1 | No |
| [IMPL, API, SEC, AUTH, TEST, DOC, QA] | 7 | 4 | Mixed | Yes (Step 4) |

**Legend:** IMPL=IMPLEMENT, API=API_CONTRACT, SEC=SECURITY, AUTH=AUTHORIZATION, DOC=DOCUMENT

---

## B17-L1-024-002 — TransitionType summary for orchestrator 013-024

| Input pattern | TransitionType | Orchestrator note |
|---|---|---|
| Build/implement/create + connector | feature_addition | Standard new connector |
| Refactor + connector | refactoring | N08 checks backward compat |
| Integrate alone | documentation | APIContractFlow33 only |
| Document alone | documentation | DocumentationFlow21 only |
| No triggering predicates | none | No flows selected |

---

## B17-L1-024-003 — Critical orchestration constraint: TestingFlow51 always after IntegrationConnectorFlow43

**Rule:** TestingFlow51 (5.1) has a hard dependency on IntegrationConnectorFlow43 (4.3). Tests cannot run before the connector code exists.

**Correct (tests after connector build):**
```
Step 1: IntegrationConnectorFlow43 (4.3)
Step 2: TestingFlow51 (5.1)  ← runs after Step 1
```

**Incorrect (tests before connector build):**
```
Step 1 (parallel): IntegrationConnectorFlow43, TestingFlow51  ← WRONG
```

**This is the TestingFlow51 dependency exception to the parallelism rule.** All other flows (APIContractFlow33, SecurityAuditFlow34, AuthorizationFlow34, DocumentationFlow21) can run in parallel after the connector design is known. Only TestingFlow51 requires the completed connector code.

---

## B17-L1-024-004 — QA gate absolute position rule for all B17 connectors

**QAGatingFlow73 position enforcement:**
```
For any predicate set containing QA:
  QAGatingFlow73 MUST be the last step.
  QAGatingFlow73 MUST NOT run in parallel with any other flow.
  QAGatingFlow73 starts only when ALL other selected flows have completed.

This applies to:
  SquarePosConnector + QA
  Any connector + QA
  Multi-connector + QA
  Maximum seven-predicate pipeline
```

**v6 error to avoid:** Placing QAGatingFlow73 in Step 2 (parallel) with other flows. This is incorrect. QA always gates the complete pipeline.
