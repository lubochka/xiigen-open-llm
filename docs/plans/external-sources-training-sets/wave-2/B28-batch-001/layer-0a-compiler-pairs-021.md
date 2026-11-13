# Layer 0-A — Compiler Pairs (021)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-021-001 — QA+ARCHITECTURE+IMPLEMENT: disaster recovery system

A: "Design the DR (Disaster Recovery) failover architecture, build the failover service, and QA the recovery procedure."
  "design" → ARCHITECTURE | "DR failover architecture" → ARCHITECTURE | "build" → IMPLEMENT | "QA" → QA | "recovery procedure" → gap
  CompiledPredicateIds: ["QA", "ARCHITECTURE", "IMPLEMENT"]

---

## B28-compiler-021-002 — QA+TEST+IMPLEMENT: DR validation

A: "Implement the failover trigger, verify that RTO (Recovery Time Objective) is met, and QA sign-off."
  "implement" → IMPLEMENT | "verify" → TEST | "RTO" → gap | "QA sign-off" → QA
  CompiledPredicateIds: ["QA", "IMPLEMENT", "TEST"]

---

## B28-compiler-021-003 — gap: DR failover execution

A: "Trigger the disaster recovery failover to the secondary region and reroute traffic."
  "trigger failover" → gap | "secondary region" → gap | "reroute traffic" → gap
  CompiledPredicateIds: [] → gap

---

## B28-compiler-021-004 — QA+DOCUMENT: DR release criteria

A: "Document the disaster recovery release criteria and QA review the runbook."
  "document" → DOCUMENT | "release criteria" → DOCUMENT | "QA review" → QA | "runbook" → gap
  CompiledPredicateIds: ["QA", "DOCUMENT"]

---

## B28-compiler-021-005 — QA+ARCHITECTURE+IMPLEMENT+TEST+DOCUMENT: full DR lifecycle

A: "Design the DR architecture, build the failover service, write DR scenario tests, document the runbook, and QA gate the release."
  "design" → ARCHITECTURE | "build" → IMPLEMENT | "write tests" → TEST | "document" → DOCUMENT | "QA gate" → QA
  CompiledPredicateIds: ["QA", "ARCHITECTURE", "IMPLEMENT", "TEST", "DOCUMENT"]
