# Layer 1 — Transition Orchestrator Pairs (056) — APTHunting TransitionType
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T056-001 — APT hunting audit → feature_addition

T056-001 test: "Audit logs for APT lateral movement and credential harvesting indicators."
  SECURITY → 6.3 | TransitionType: feature_addition

---

## T056-002 — "monitor network traffic" → N/A

T056-002 test: "Monitor all network traffic to baseline normal behavior."
  CompiledPredicateIds: [] | TransitionType: N/A

---

## T056-003 — APT + AUTHORIZATION (RBAC role injection) → feature_addition (dual predicate)

T056-003 test: "Audit APT privilege escalation via RBAC role injection in service accounts."
  SECURITY → 6.3 | AUTHORIZATION → 6.3 | TransitionType: feature_addition

---

## T056-004 — APT + DOCUMENT → documentation

T056-004 test: "Review APT exfiltration paths and document affected data stores and credential scope."
  SECURITY → 6.3 | DOCUMENT → 6.2 | TransitionType: documentation

---

## T056-005 — APT + IMPLEMENT → feature_addition

T056-005 test: "Implement APT detection rules and audit existing log coverage for lateral movement indicators."
  IMPLEMENT → 4.1 | SECURITY → 6.3 | TransitionType: feature_addition
