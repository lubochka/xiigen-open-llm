# Held-Out and Corruption Pairs (014)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H014-001 — Held-out: "sidecar pattern" fires ARCHITECTURE

A: "Design the sidecar pattern for the service mesh communication layer."
  Expected: ["ARCHITECTURE"]
  Held-out for evaluation.

---

## H014-002 — Held-out: "enable Istio" is gap

A: "Enable Istio on the Kubernetes cluster."
  Expected: []
  Held-out: "enable" and "Istio" are gap vocabulary.

---

## H014-003 — Held-out: mTLS fires SECURITY independently

A: "Enforce mTLS between the OrderService and PaymentService."
  Expected: ["SECURITY"]
  Held-out for evaluation.

---

## C014-001 — Corruption v6-01: DEVOPS invented for service mesh

A: "Design the Istio service mesh topology."
  Correct: ["ARCHITECTURE"]
  Corrupt: ["ARCHITECTURE", "DEVOPS"] ← v6-01 (DEVOPS is not a valid predicate)
  Teaching: v6-01 invents non-existent predicates. Service mesh = ARCHITECTURE. No DEVOPS predicate exists.

---

## C014-002 — Corruption v6-02: mTLS fires ARCHITECTURE instead of SECURITY

A: "Enforce mTLS between services."
  Correct: ["SECURITY"]
  Corrupt: ["ARCHITECTURE"] ← v6-02 (mTLS incorrectly mapped to ARCHITECTURE)
  Teaching: mTLS → SECURITY ("tls" in SECURITY lexicon). mTLS is not in ARCHITECTURE lexicon. v6-02 confuses security vocabulary with architecture vocabulary.
