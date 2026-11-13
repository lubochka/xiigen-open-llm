# Layer 0-B — Router Pairs (059)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-059-001 — MonitoringService: IMPLEMENT routing

A: "Build a MonitoringService to collect and expose health metrics."
B: Flow: ImplementationFlow41 | DecisionCount: 1

---

## B15-router-059-002 — MonitoringService: IMPLEMENT+API_CONTRACT routing

A: "Build MonitoringService and integrate with the Prometheus API."
B: Flow: APIContractFlow33 → ImplementationFlow41 | DecisionCount: 2

---

## B15-router-059-003 — MonitoringService: IMPLEMENT+SECURITY routing

A: "Build MonitoringService and secure the metrics endpoint."
B: Flow: SecurityAuditFlow → ImplementationFlow41 | DecisionCount: 2

---

## B15-router-059-004 — MonitoringService: IMPLEMENT+DOCUMENT routing

A: "Build MonitoringService and document all exposed health check endpoints."
B: Flow: ImplementationFlow41 ‖ DocumentationFlow21 | DecisionCount: 2

---

## B15-router-059-005 — MonitoringService: IMPLEMENT+QA routing (QA last)

A: "Build MonitoringService and QA the alert thresholds before release."
B: Flow: ImplementationFlow41 → QAGatingFlow71 | DecisionCount: 2 | QA absolutely last
Anti-pattern: routing QAGatingFlow71 before ImplementationFlow41.
