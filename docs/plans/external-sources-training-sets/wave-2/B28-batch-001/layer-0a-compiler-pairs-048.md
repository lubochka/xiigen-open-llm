# Layer 0-A — Compiler Pairs (048) — QA+IMPLEMENT combo (Round 3 reference)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-048-001 — IMPLEMENT+QA: feature_addition TransitionType

A: "Implement the release gate service and QA the implementation."
  "implement" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "QA" → QA
  TransitionType: feature_addition (IMPLEMENT drives it)
  Teaching: IMPLEMENT+QA always → feature_addition. QA does not override IMPLEMENT's TransitionType contribution.
  CompiledPredicateIds: ["IMPLEMENT", "QA"]

---

## B28-compiler-048-002 — QA+IMPLEMENT ordering in sentence: output order is fixed

A: "QA the current build and then implement the new gate feature."
  Sentence order: QA first, IMPLEMENT second → but predicate output order: ["IMPLEMENT", "QA"]
  Teaching: Predicate output order is always canonical: ARCHITECTURE→IMPLEMENT→TEST→SECURITY→DOCUMENT→QA. Sentence order is ignored.
  CompiledPredicateIds: ["IMPLEMENT", "QA"]

---

## B28-compiler-048-003 — "QA" without IMPLEMENT: no TransitionType

A: "QA the current pipeline state before the release window."
  "QA" → QA | no IMPLEMENT present
  TransitionType: none
  Teaching: QA alone → gate only. TransitionType requires IMPLEMENT (feature_addition) or TEST/DOCUMENT without IMPLEMENT (documentation).
  CompiledPredicateIds: ["QA"]

---

## B28-compiler-048-004 — Multiple IMPLEMENT triggers + QA

A: "Build the orchestrator, implement the stage sequencer service, create the factory class, and QA sign-off."
  "build" → IMPLEMENT | "implement" → IMPLEMENT (dedup) | "service" → IMPLEMENT (dedup) | "create the factory class" → IMPLEMENT (dedup) | "QA sign-off" → QA
  Teaching: All IMPLEMENT triggers deduplicate to one IMPLEMENT predicate. QA last.
  CompiledPredicateIds: ["IMPLEMENT", "QA"]

---

## B28-compiler-048-005 — IMPLEMENT+QA complete combo reference

A: IMPLEMENT triggers: build, implement, create service, develop service, add service, write service, construct service, make service
  QA triggers: "QA" abbreviation in any position (verb, noun modifier, sign-off, approve, gate, review)
  Combined output: ["IMPLEMENT", "QA"] → feature_addition
  Teaching: Complete reference for IMPLEMENT+QA combo predicate behavior.
  CompiledPredicateIds: ["IMPLEMENT", "QA"]
