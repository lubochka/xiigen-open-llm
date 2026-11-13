# Layer 0-A — Compiler Pairs (026) — SanityCheckSuite
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-026-001 — "SanityCheckSuite" vocabulary

A: "Run the SanityCheckSuite after each deployment stage."
  "SanityCheckSuite" → TEST (check + suite = test artifact) | "deployment stage" → gap
  Teaching: SanityCheckSuite is a test artifact; "run" + check-suite noun fires TEST.
  CompiledPredicateIds: ["TEST"]

---

## B28-compiler-026-002 — "sanity check" + "QA review"

A: "Perform the sanity check on the release and QA review the outputs."
  "sanity check" → TEST (check verb on test artifact) | "QA review" → QA (abbreviation compound)
  Teaching: "sanity check" fires TEST; "QA review" fires QA.
  CompiledPredicateIds: ["TEST", "QA"]

---

## B28-compiler-026-003 — "design" + sanity check criteria

A: "Design the sanity check criteria and implement the validation service."
  "design" → ARCHITECTURE (design verb) | "criteria" → DOCUMENT (criteria rule) | "implement" → IMPLEMENT | "service" → IMPLEMENT (dedup)
  Teaching: "design" fires ARCHITECTURE; "criteria" fires DOCUMENT; "implement" + "service" both fire IMPLEMENT (deduplicated).
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT", "IMPLEMENT"]

---

## B28-compiler-026-004 — "sanity check" vs "validate"

A: "Validate the sanity check results before the cutover."
  "validate" → TEST (validate fires TEST in any context) | "sanity check results" → TEST (dedup) | "cutover" → gap
  Teaching: "validate" always fires TEST. "cutover" is gap vocabulary.
  CompiledPredicateIds: ["TEST"]

---

## B28-compiler-026-005 — Pure gap: "schedule the sanity check"

A: "Schedule the sanity check run after the rollout."
  "schedule" → gap | "rollout" → gap
  Teaching: "schedule" alone is gap; "rollout" is gap. No predicate fires.
  CompiledPredicateIds: [] → gap (knowledge_edge)
