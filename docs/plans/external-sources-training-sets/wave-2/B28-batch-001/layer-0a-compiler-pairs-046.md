# Layer 0-A — Compiler Pairs (046) — ChangelogGeneration
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-046-001 — "generate changelog" fires DOCUMENT

A: "Generate the changelog for the v2.5.0 release."
  "generate changelog" → DOCUMENT ("changelog" is a documentation artifact)
  Teaching: "changelog" is a documentation artifact. "generate" in changelog context → DOCUMENT.
  CompiledPredicateIds: ["DOCUMENT"]

---

## B28-compiler-046-002 — IMPLEMENT+DOCUMENT+QA for changelog generator service

A: "Build the changelog generator service, document the generation criteria, and QA the output format."
  "build" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "QA" → QA
  Teaching: Three-predicate set. "changelog generator service" → IMPLEMENT via "service" rule.
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT", "QA"]

---

## B28-compiler-046-003 — "release notes" fires DOCUMENT

A: "Write the release notes for the production deployment."
  "write" → gap (not test-writing context) | "release notes" → DOCUMENT (documentation artifact)
  Teaching: "release notes" is a documentation artifact → DOCUMENT. "write" alone is not TEST without test context.
  CompiledPredicateIds: ["DOCUMENT"]

---

## B28-compiler-046-004 — "commit log" alone is gap

A: "Pull the commit log and format it for distribution."
  "pull" → gap | "commit log" → gap | "format" → gap | "distribution" → gap
  Teaching: "commit log" raw vocabulary is gap. Only "changelog" or "release notes" fire DOCUMENT.
  CompiledPredicateIds: []

---

## B28-compiler-046-005 — TEST+DOCUMENT+QA for changelog with validation

A: "Write tests for the changelog parser, document the entry format criteria, and QA the generated output."
  "write tests" → TEST | "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "QA" → QA
  Teaching: TEST via test-writing verb. DOCUMENT via verb and noun. QA last.
  CompiledPredicateIds: ["TEST", "DOCUMENT", "QA"]
