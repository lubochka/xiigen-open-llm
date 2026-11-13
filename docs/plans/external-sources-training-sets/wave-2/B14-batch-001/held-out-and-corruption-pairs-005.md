# Held-Out and Corruption Pairs (005)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H005-001 — Held-out: ARCHITECTURE — circuit breaker pattern

A: "Define the circuit breaker pattern and fallback strategy for the PaymentService outbound calls."
  Expected: ["ARCHITECTURE"]
  Held-out: "circuit breaker pattern" fires ARCHITECTURE; "fallback strategy" fires ARCHITECTURE.

---

## H005-002 — Held-out: gap — interrogative

A: "Is the infrastructure ready for production traffic?"
  Expected: []
  Held-out: Interrogative without imperative design verb → gap.

---

## H005-003 — Held-out: ARCHITECTURE+QA — sign-off

A: "Finalize the infrastructure blueprint and get QA sign-off before the production rollout."
  Expected: ["ARCHITECTURE", "QA"]
  Held-out: "finalize the infrastructure blueprint" fires ARCHITECTURE; "QA sign-off" fires QA.

---

## C005-001 — Corruption v6-02: gap → IMPLEMENT (not ARCHITECTURE)

A: "Configure the firewall rules to allow traffic on port 443."
  Correct: []
  Corrupt: ["IMPLEMENT"] ← v6-02 (model defaults to IMPLEMENT for configuration commands)
  Teaching: "configure" is universally gap. Not IMPLEMENT, not ARCHITECTURE.

---

## C005-002 — Corruption v6-01: NETWORK invented

A: "Design the network segmentation architecture."
  Correct: ["ARCHITECTURE"]
  Corrupt: ["ARCHITECTURE", "NETWORK"] ← v6-01 (NETWORK is not a valid predicate)
  Teaching: Network-related requests fire ARCHITECTURE (or SECURITY for security aspects). NETWORK is not a valid predicate.

