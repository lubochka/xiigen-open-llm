# Layer 1 — Transition Orchestrator Pairs (041)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: Splunk connector

---

## B17-L1-041-001 — Splunk: orchestration for [IMPL, API, SEC]

**Flows:** IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3), SecurityAuditFlow34(3.4)

```
Step 1: IntegrationConnectorFlow43 (4.3) — build SplunkConnector
Step 2: APIContractFlow33 (3.3) — HEC API contract [after Step 1]
Step 3: SecurityAuditFlow34 (3.4) — HEC token audit [after Step 1]
```

---

## B17-L1-041-002 — Splunk: six-predicate orchestration

**Flows:** IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3), SecurityAuditFlow34(3.4), AuthorizationFlow34(3.4), TestingFlow51(5.1), DocumentationFlow21(2.1)

```
Step 1: IntegrationConnectorFlow43 (4.3) — build SplunkConnector [parallel with Step 2]
Step 2: DocumentationFlow21 (2.1) — draft ingestion API documentation [parallel with Step 1]
Step 3: APIContractFlow33 (3.3) — HEC API contract [after Step 1]
Step 4: SecurityAuditFlow34 (3.4) — token/HMAC audit [after Step 1; parallel with Step 5]
Step 5: AuthorizationFlow34 (3.4) — OAuth review [after Step 1; parallel with Step 4]
Step 6: TestingFlow51 (5.1) — ingestion integration tests [after Step 1; hard dependency]
```

Steps 4 and 5 can run in parallel. Step 6 has hard sequential dependency on Step 1.

---

## B17-L1-041-003 — Splunk: "integrate" orchestration

**Flows:** APIContractFlow33(3.3)

```
Step 1: APIContractFlow33 (3.3) — document Splunk HEC API contract
```

TransitionType: documentation

---

## B17-L1-041-004 — Splunk orchestration: max without QA = 6 steps

Six-predicate orchestration (IMPL + API + SEC + AUTH + TEST + DOC) produces 6 steps.
Maximum with QA = 7 steps (QAGatingFlow73 last-absolute added at the end).
