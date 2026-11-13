# Layer 0-A — Compiler Pairs (020)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-020-001 — QA alone: A/B testing gate

A: "QA review the A/B testing release before the experiment goes live."
  "QA review" → QA | "A/B testing" → gap | "goes live" → gap
  CompiledPredicateIds: ["QA"]

---

## B28-compiler-020-002 — QA+IMPLEMENT: A/B experiment service

A: "Build the A/B (A-slash-B) experiment assignment service and QA the variant selection logic."
  "build" → IMPLEMENT | "experiment assignment service" → IMPLEMENT | "QA" → QA | "variant selection" → gap
  CompiledPredicateIds: ["QA", "IMPLEMENT"]

---

## B28-compiler-020-003 — gap: A/B experiment configuration

A: "Configure the A/B test to split traffic 50/50 between control and treatment groups."
  "configure" → gap | "A/B test" → gap | "split traffic" → gap | "control/treatment" → gap
  CompiledPredicateIds: [] → gap

---

## B28-compiler-020-004 — QA+TEST: A/B testing validation

A: "Verify the A/B variant assignment and QA the statistical significance criteria."
  "verify" → TEST | "QA" → QA | "statistical significance" → gap | "variant assignment" → gap
  CompiledPredicateIds: ["QA", "TEST"]

---

## B28-compiler-020-005 — QA+IMPLEMENT+DOCUMENT: A/B release documentation

A: "Build the experiment framework, document the A/B release criteria, and QA approve the rollout."
  "build" → IMPLEMENT | "document" → DOCUMENT | "release criteria" → DOCUMENT | "QA approve" → QA | "rollout" → gap
  CompiledPredicateIds: ["QA", "IMPLEMENT", "DOCUMENT"]
