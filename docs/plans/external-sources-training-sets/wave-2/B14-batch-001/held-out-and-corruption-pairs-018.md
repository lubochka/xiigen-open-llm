# Held-Out and Corruption Pairs (018)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H018-001 — Held-out: "disaster recovery topology" fires ARCHITECTURE

A: "Design the disaster recovery topology for the payments platform."
  Expected: ["ARCHITECTURE"]
  Held-out for evaluation.

---

## H018-002 — Held-out: "test the DR failover" fires TEST

A: "Test the disaster recovery failover procedure."
  Expected: ["TEST"]
  Held-out: "test" → TEST; "disaster recovery" alone does not fire ARCHITECTURE.

---

## H018-003 — Held-out: "RPO/RTO topology" fires ARCHITECTURE in design context

A: "Design the backup and recovery topology — RPO < 30 seconds and RTO < 3 minutes."
  Expected: ["ARCHITECTURE"]
  Held-out for evaluation.

---

## C018-001 — Corruption v6-01: DISASTER_RECOVERY invented as predicate

A: "Design the disaster recovery topology."
  Correct: ["ARCHITECTURE"]
  Corrupt: ["ARCHITECTURE", "DISASTER_RECOVERY"] ← v6-01 (DISASTER_RECOVERY is not a valid predicate)
  Teaching: Domain-specific vocabulary (disaster recovery, backup, replication) does not create new predicates. Only 9 valid predicates exist.

---

## C018-002 — Corruption v6-02: "test the DR failover" fires ARCHITECTURE

A: "Test the disaster recovery failover by promoting the replica."
  Correct: ["TEST"]
  Corrupt: ["ARCHITECTURE"] ← v6-02 (incorrectly fires ARCHITECTURE on DR domain vocabulary)
  Teaching: "test" fires TEST. "disaster recovery" as domain vocabulary without design framing does not fire ARCHITECTURE. Predicate selection is driven by action verbs + predicate vocabulary, not domain topic.
