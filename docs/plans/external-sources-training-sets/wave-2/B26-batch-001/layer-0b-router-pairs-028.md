# Layer 0-B — Router Pairs (028) — CISBenchmark Routing
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R028-001 — CIS Benchmark gap → GapHandler

R028-001 test: "Complete the CIS Benchmark assessment for Linux hosts."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING

---

## R028-002 — CIS Benchmark + hardening audit → SECURITY → 6.3

R028-002 test: "Audit server hardening configuration against CIS Benchmark Level 2."
  SECURITY → 6.3 ("audit" + "hardening") | CompiledPredicateIds: ["SECURITY"]

---

## R028-003 — CIS Benchmark + vulnerability scan → SECURITY → 6.3

R028-003 test: "Scan infrastructure for CIS Benchmark violations and open vulnerabilities."
  SECURITY → 6.3 ("scan" + "vulnerabilities") | CompiledPredicateIds: ["SECURITY"]

---

## R028-004 — "harden" verb standalone → SECURITY → 6.3

R028-004 test: "Harden the API gateway and review injection vulnerability exposure."
  SECURITY → 6.3 ("harden" → security action) | CompiledPredicateIds: ["SECURITY"]

---

## R028-005 — CIS + SSH + TLS routing

R028-005 test: "Review SSH key authentication and TLS protocol versions against CIS controls."
  SECURITY → 6.3 (review + SSH key authentication + TLS)
  Both "ssh" (authentication group) and "tls" fire SECURITY independently.
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
