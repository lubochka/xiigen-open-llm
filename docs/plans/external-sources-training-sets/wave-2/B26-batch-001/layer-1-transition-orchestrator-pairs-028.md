# Layer 1 — Transition Orchestrator Pairs (028) — CISBenchmark TransitionType
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T028-001 — CIS Benchmark label alone → N/A

T028-001 test: "Ensure the servers meet CIS Benchmark Level 2 requirements."
  CompiledPredicateIds: [] | TransitionType: N/A | GapHandler activated

---

## T028-002 — CIS Benchmark + vulnerability scan → feature_addition

T028-002 test: "Scan the server configuration for CIS Level 2 Benchmark vulnerabilities in SSH and TLS configuration."
  SECURITY → 6.3 | TransitionType: feature_addition

---

## T028-003 — CIS Benchmark + audit + DOCUMENT → documentation

T028-003 test: "Audit SSH hardening, password policies, and audit logging against CIS Benchmark controls, then document findings."
  SECURITY → 6.3 | DOCUMENT → 6.2 | TransitionType: documentation

---

## T028-004 — CIS Benchmark + harden → feature_addition

T028-004 test: "Harden the Linux server configuration against the CIS Benchmark for kernel parameters and filesystem permissions."
  SECURITY → 6.3 (harden = security-action verb) | TransitionType: feature_addition

---

## T028-005 — CIS Benchmark + IMPLEMENT → feature_addition (IMPLEMENT overrides)

T028-005 test: "Implement CIS Benchmark-compliant TLS cipher suites and audit current cipher configuration."
  IMPLEMENT → 4.1 | SECURITY → 6.3 | TransitionType: feature_addition
