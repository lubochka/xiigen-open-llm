# Layer 3 — Teaching Unit Pairs (021)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU021-001 — QA+ARCHITECTURE+IMPLEMENT: DR system

A: "Design the DR failover architecture, build the failover service, and QA the release."
B: ["ARCHITECTURE", "IMPLEMENT", "QA"] — "design" → ARCHITECTURE; "build" → IMPLEMENT; "failover service" → IMPLEMENT via "service" (dedup); "QA" → QA.
Reason: Three predicates. TransitionType: feature_addition (IMPLEMENT present).
CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "QA"]

---

## TU021-002 — Gap: DR failover execution

A: "Trigger the disaster recovery failover to the secondary AWS (Amazon Web Services) region."
B: [] — "trigger failover" → gap; "secondary region" → gap.
Reason: DR failover execution vocabulary → gap. knowledge_edge.
CompiledPredicateIds: []

---

## TU021-003 — QA+IMPLEMENT+TEST: DR with RTO validation

A: "Implement the failover trigger, verify the RTO is met, and QA sign-off."
B: ["IMPLEMENT", "TEST", "QA"] — "implement" → IMPLEMENT; "verify" → TEST; "QA sign-off" → QA.
Reason: "RTO" is gap vocabulary but does not suppress TEST from "verify." Three predicates.
CompiledPredicateIds: ["IMPLEMENT", "TEST", "QA"]

---

## TU021-004 — "RTO" alone is gap; "verify RTO is met" fires TEST

A: "Monitor the RTO for the DR site." vs "Verify the RTO is met for the failover."
B1: [] — "monitor" → gap; "RTO" → gap.
B2: ["TEST"] — "verify" → TEST; "RTO" stays gap but does not suppress TEST.
Reason: "monitor" alone is gap. "verify" fires TEST regardless of what is verified.
CompiledPredicateIds (B1): [] | CompiledPredicateIds (B2): ["TEST"]

---

## TU021-005 — FAIL is valid for DR gate

A: "QA the disaster recovery release for the PaymentService."
B: ["QA"] — gate can produce FAIL if RTO or RPO (Recovery Point Objective) targets not met.
Reason: DR QA gate FAIL is expected when recovery objectives are unmet. QA alone → no TransitionType.
CompiledPredicateIds: ["QA"]
