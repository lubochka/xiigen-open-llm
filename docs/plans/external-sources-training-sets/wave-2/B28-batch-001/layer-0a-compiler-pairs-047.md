# Layer 0-A — Compiler Pairs (047) — TaggingStrategy
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-047-001 — "tag release" and "git tag" are gap

A: "Tag the release with v2.5.0 and push the git tag."
  "tag the release" → gap | "v2.5.0" → gap | "push the git tag" → gap
  Teaching: Git tagging vocabulary is gap. No predicates fire.
  CompiledPredicateIds: []

---

## B28-compiler-047-002 — DOCUMENT+QA for tagging strategy criteria

A: "Document the semantic versioning tagging criteria and QA the tag naming convention."
  "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "semantic versioning" → gap | "QA" → QA
  Teaching: "criteria" and "document" fire DOCUMENT. "semantic versioning" is gap filler. QA last.
  CompiledPredicateIds: ["DOCUMENT", "QA"]

---

## B28-compiler-047-003 — IMPLEMENT+DOCUMENT for automated tagging service

A: "Build the automated release tagger service and document the tag schema."
  "build" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "document" → DOCUMENT | "tag schema" → gap
  Teaching: Build+service → IMPLEMENT. DOCUMENT via verb. "tag schema" alone is gap.
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT"]

---

## B28-compiler-047-004 — "semver" (Semantic Versioning) and "versioning" are gap

A: "Apply semver tagging and enforce versioning policy."
  "apply" → gap | "semver tagging" → gap | "versioning policy" → gap | "enforce" → gap
  Teaching: Semver and versioning policy vocabulary is gap.
  CompiledPredicateIds: []

---

## B28-compiler-047-005 — IMPLEMENT+TEST+DOCUMENT+QA for tagging framework

A: "Implement the tag creation service, write tests for version bumping logic, document the tagging criteria, and QA the release tag."
  "implement" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "write tests" → TEST | "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "QA" → QA
  Teaching: Four-predicate set. IMPLEMENT drives feature_addition. QA absolute last.
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "DOCUMENT", "QA"]
