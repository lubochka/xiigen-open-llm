# Layer 1 — Transition Orchestrator Pairs (044) — InsecureDeserializationAudit TransitionType
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T044-001 — Deserialization audit → feature_addition

T044-001 test: "Audit all deserialization endpoints for insecure deserialization vulnerabilities."
  SECURITY → 6.3 | TransitionType: feature_addition

---

## T044-002 — Deserialization + privilege escalation → feature_addition (dual predicate)

T044-002 test: "Scan for deserialization gadget chains enabling privilege escalation."
  SECURITY → 6.3 | AUTHORIZATION → 6.3 | TransitionType: feature_addition

---

## T044-003 — "migrate to System.Text.Json" → N/A

T044-003 test: "Migrate all serialization from Newtonsoft.Json to System.Text.Json."
  CompiledPredicateIds: [] | TransitionType: N/A

---

## T044-004 — Deserialization + IMPLEMENT → feature_addition

T044-004 test: "Implement type allowlist deserialization and audit gadget chain exposure in all endpoints."
  IMPLEMENT → 4.1 | SECURITY → 6.3 | TransitionType: feature_addition

---

## T044-005 — Deserialization + DOCUMENT → documentation

T044-005 test: "Review TypeNameHandling exploitation vectors and document the deserialization remediation findings."
  SECURITY → 6.3 | DOCUMENT → 6.2 | TransitionType: documentation
