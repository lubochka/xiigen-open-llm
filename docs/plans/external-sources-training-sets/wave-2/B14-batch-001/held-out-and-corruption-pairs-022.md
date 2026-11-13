# Held-Out and Corruption Pairs (022)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H022-001 — Held-out: "zero-trust" fires SECURITY+ARCHITECTURE

A: "Design the zero-trust network architecture."
  Expected: ["ARCHITECTURE", "SECURITY"]
  Held-out for evaluation.

---

## H022-002 — Held-out: "open port 443" is gap

A: "Open port 443 on the web-tier security group."
  Expected: []
  Held-out: "open port" is operational firewall action gap.

---

## H022-003 — Held-out: "PrivateLink" alone is gap

A: "Set up AWS PrivateLink for the payment service."
  Expected: []
  Held-out: "set up" + "PrivateLink" are both gap.

---

## C022-001 — Corruption v6-01: NETWORKING invented as predicate

A: "Design the VPC network topology."
  Correct: ["ARCHITECTURE"]
  Corrupt: ["ARCHITECTURE", "NETWORKING"] ← v6-01 (NETWORKING is not a valid predicate)
  Teaching: "VPC network topology" fires ARCHITECTURE via "topology". NETWORKING is not a valid predicate.

---

## C022-002 — Corruption v6-02: "zero-trust" fires ARCHITECTURE only (misses SECURITY)

A: "Design the zero-trust network topology."
  Correct: ["ARCHITECTURE", "SECURITY"]
  Corrupt: ["ARCHITECTURE"] ← v6-02 (misses SECURITY for "zero-trust")
  Teaching: "zero-trust" is in SECURITY predicate lexicon independently. It fires SECURITY even when embedded in an ARCHITECTURE phrase. Both predicates must fire.
