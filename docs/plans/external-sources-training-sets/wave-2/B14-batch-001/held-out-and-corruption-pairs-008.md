# Held-Out and Corruption Pairs (008)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H008-001 — Held-out: ARCHITECTURE — event sourcing

A: "Architect the event sourcing topology for the OrderService."
  Expected: ["ARCHITECTURE"]
  Held-out: "event sourcing topology" fires ARCHITECTURE.

---

## H008-002 — Held-out: ARCHITECTURE+SECURITY — ingress with WAF

A: "Design the ingress layer with WAF (Web Application Firewall) placement and TLS termination."
  Expected: ["ARCHITECTURE", "SECURITY"]
  Held-out: ARCHITECTURE fires on "ingress layer"; SECURITY fires on "WAF", "TLS".

---

## H008-003 — Held-out: gap — operational monitoring request

A: "Check the CPU usage of the pods in production."
  Expected: []
  Held-out: "check" is gap; "CPU usage", "pods", "production" are operational vocabulary → gap.

---

## C008-001 — Corruption v6-02: gap → ARCHITECTURE (infra sounding)

A: "Update the Kubernetes configuration for the staging environment."
  Correct: []
  Corrupt: ["ARCHITECTURE"] ← v6-02 (Kubernetes + configuration + environment sounds architectural)
  Teaching: "update the configuration" is operational gap even with infrastructure vocabulary. No design verb → gap.

---

## C008-002 — Corruption v6-01: KUBERNETES invented as predicate

A: "Design the Kubernetes cluster topology."
  Correct: ["ARCHITECTURE"]
  Corrupt: ["ARCHITECTURE", "KUBERNETES"] ← v6-01 (KUBERNETES is not a valid predicate)
  Teaching: Platform names (Kubernetes, AWS, Terraform) never become predicates. Only 9 valid predicates.

