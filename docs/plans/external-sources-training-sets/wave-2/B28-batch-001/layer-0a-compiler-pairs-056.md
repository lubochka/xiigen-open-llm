# Layer 0-A — Compiler Pairs (056) — FeatureFlagFlip
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-056-001 — "feature flag flip" is gap

A: "Flip the feature flag to enable the new checkout flow."
  "flip the feature flag" → gap | "enable" → gap | "new checkout flow" → gap
  Teaching: Feature flag flip vocabulary is gap. No predicates fire.
  CompiledPredicateIds: []

---

## B28-compiler-056-002 — IMPLEMENT+QA for feature flag service

A: "Build the feature flag flip service and QA the flag evaluation logic."
  "build" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "QA" → QA
  Teaching: Build+service → IMPLEMENT. QA fires on abbreviation.
  CompiledPredicateIds: ["IMPLEMENT", "QA"]

---

## B28-compiler-056-003 — "toggle" and "flag state" are gap

A: "Toggle the flag state for the new recommendation engine."
  "toggle" → gap | "flag state" → gap | "recommendation engine" → gap
  Teaching: All flag state toggle vocabulary is gap.
  CompiledPredicateIds: []

---

## B28-compiler-056-004 — TEST+QA for feature flag gate verification

A: "Verify the feature flag evaluation for each user segment and QA the rollout percentage."
  "verify" → TEST | "feature flag evaluation" → gap | "QA" → QA
  Teaching: "verify" fires TEST. Feature flag context is gap filler. QA last.
  CompiledPredicateIds: ["TEST", "QA"]

---

## B28-compiler-056-005 — DOCUMENT+QA for feature flag criteria

A: "Document the feature flag eligibility criteria and QA the targeting rules."
  "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "QA" → QA
  Teaching: DOCUMENT via verb and noun. QA last. "targeting rules" is gap filler.
  CompiledPredicateIds: ["DOCUMENT", "QA"]
