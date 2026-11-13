# Held-Out and Corruption Pairs (020)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H020-001 — Held-out: "observability infrastructure" fires ARCHITECTURE

A: "Architect the observability infrastructure for the microservices platform."
  Expected: ["ARCHITECTURE"]
  Held-out for evaluation.

---

## H020-002 — Held-out: "add a Prometheus alert" is gap

A: "Add a Prometheus alert for high memory usage."
  Expected: []
  Held-out: "add" is operational gap; "Prometheus alert" is tool configuration gap.

---

## H020-003 — Held-out: "configure Prometheus" fires IMPLEMENT

A: "Configure Prometheus scrape intervals for the payment-service."
  Expected: ["IMPLEMENT"]
  Held-out for evaluation.

---

## C020-001 — Corruption v6-01: OBSERVABILITY invented as predicate

A: "Design the observability infrastructure."
  Correct: ["ARCHITECTURE"]
  Corrupt: ["ARCHITECTURE", "OBSERVABILITY"] ← v6-01 (OBSERVABILITY is not a valid predicate)
  Teaching: "observability" is a domain vocabulary word. "observability infrastructure" → ARCHITECTURE via "infrastructure". No OBSERVABILITY predicate exists.

---

## C020-002 — Corruption v6-03: "add a Prometheus alert" fires IMPLEMENT

A: "Add a Prometheus alert for high CPU."
  Correct: []
  Corrupt: ["IMPLEMENT"] ← v6-03 (treats operational configuration as implementation)
  Teaching: "add" is not in IMPLEMENT lexicon. IMPLEMENT requires write/build/create/scaffold/implement/configure (code artifact context). "add an alert" is UI/operational action = gap.
