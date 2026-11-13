# Layer 0-A — Compiler Pairs (060) — Complete Factory Reference (Round 3)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-060-001 — Complete predicate trigger list for Round 3 topics

Round 3 topics (049–060) predicate triggers:
  PostDeploymentVerification: "verification" + gate context → TEST
  SyntheticMonitoring: monitor + service → IMPLEMENT; "synthetic test suite" → TEST
  ChaosEngineeringGate: "chaos engineering" → TEST
  CapacityVerification: "capacity verification" → TEST; "verify" → TEST
  DNSCutover: all DNS vocabulary → gap; "verify" after cutover → TEST
  LoadBalancerSwitch: all LB routing → gap; "verify" → TEST; "secure" → SECURITY
  DatabaseMigrationGate: migration gate → TEST; "validate"/"verify" → TEST
  FeatureFlagFlip: all flag toggle → gap; "verify" → TEST
  ObservabilityCheck: "check gate" → TEST; metrics/traces/logs config → gap
  SLAVerification: "SLA verification" → TEST; SLA/SLO alone → gap
  ExecutiveDashboard: "executive dashboard" → DOCUMENT; "reporting service" → IMPLEMENT
  CompiledPredicateIds: reference

---

## B28-compiler-060-002 — Gap terms specific to Round 3

Round 3 gap terms: "promote", "cutover", "flip", "toggle", "rollout", "hotfix" (alone), "DNS record", "TTL", "weighted routing", "round-robin", "telemetry", "dashboards", "uptime", "error budget", "tag", "semver", "commit log", "alert", "alerting", "steady state hypothesis", "fault injection" (alone), "KPI" (alone)
  Teaching: None of these fire any predicate; all produce knowledge_edge output.
  CompiledPredicateIds: []

---

## B28-compiler-060-003 — "verify" fires TEST universally in Round 3 contexts

A: "Verify DNS resolution" | "Verify SLA compliance" | "Verify capacity" | "Verify feature flag evaluation" | "Verify observability coverage"
  All → TEST
  Teaching: "verify" fires TEST universally regardless of what is being verified. Consistent with all rounds.
  CompiledPredicateIds: ["TEST"]

---

## B28-compiler-060-004 — "service" rule in all Round 3 contexts

A: "DNS cutover automation service" → IMPLEMENT | "load balancer controller service" → IMPLEMENT | "database migration runner service" → IMPLEMENT | "feature flag flip service" → IMPLEMENT | "SLA verification service" → IMPLEMENT | "observability check service" → IMPLEMENT
  Teaching: "service" fires IMPLEMENT in all Round 3 compound nouns without exception.
  CompiledPredicateIds: ["IMPLEMENT"]

---

## B28-compiler-060-005 — QA absolute last in all Round 3 multi-predicate sets

A: "Design the pipeline orchestration architecture, build the stage sequencer service, run post-deployment verification, validate the SLA thresholds, document the release criteria, and QA approve."
  Predicate set: ["ARCHITECTURE", "IMPLEMENT", "TEST", "DOCUMENT", "QA"]
  Sequential order: ARCHITECTURE → parallel(IMPLEMENT ‖ TEST) → DOCUMENT → QA
  Teaching: QA is always the last predicate in any Round 3 multi-predicate sequence. No predicate follows QA.
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "TEST", "DOCUMENT", "QA"]
