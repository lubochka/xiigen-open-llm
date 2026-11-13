# Held-Out and Corruption Pairs (021)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H021-001 — Held-out: QA+ARCHITECTURE+IMPLEMENT — DR system

A: "Design the DR failover architecture, build the failover service, and QA the disaster recovery release."
  Expected: ["QA", "ARCHITECTURE", "IMPLEMENT"]
  Held-out: "design" → ARCHITECTURE; "build" → IMPLEMENT; "failover service" → IMPLEMENT via "service" (dedup); "QA" → QA. feature_addition.

---

## H021-002 — Held-out: gap — DR failover execution

A: "Trigger the failover to the secondary region and reroute DNS."
  Expected: []
  Held-out: "trigger failover" → gap; "reroute DNS" → gap. knowledge_edge.

---

## H021-003 — Held-out: QA+IMPLEMENT+TEST — DR with RTO verification

A: "Implement the failover trigger, verify the RTO target is met, and QA sign-off."
  Expected: ["QA", "IMPLEMENT", "TEST"]
  Held-out: "implement" → IMPLEMENT; "verify" → TEST; "RTO" → gap (metric); "QA sign-off" → QA. feature_addition.

---

## C021-001 — Corruption: "failover" fires IMPLEMENT

A: "Trigger the failover to the DR site."
  Correct: [] — gap
  Corrupt: ["IMPLEMENT"] ← wrong ("failover" treated as implementation)
  Teaching: "trigger failover" is gap execution vocabulary. Not IMPLEMENT.

---

## C021-002 — Corruption: DR gate FAIL normalized to PASS

A: DR QA gate — RTO exceeds target (45s actual vs 30s target).
  Correct: FAIL — RTO not met
  Corrupt: PASS ← wrong (FAIL normalized to PASS)
  Teaching: DR gate FAIL when RTO/RPO targets are unmet. FAIL must not be normalized.
