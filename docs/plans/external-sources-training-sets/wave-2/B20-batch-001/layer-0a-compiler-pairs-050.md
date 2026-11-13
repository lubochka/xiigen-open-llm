# Layer 0-A — Compiler Pairs (050) — RegexValidator: TEST fires on test vocabulary, not regex terms
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P050-001 — TEST+IMPLEMENT: "build RegexValidator and add test cases"

A: "Build the RegexValidator and add test cases for email, phone number, and postal code patterns."
  "build" → IMPLEMENT | "add" → IMPLEMENT (deduplicates) | "test cases" → TEST
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P050-002 — TEST: "verify" fires TEST on regex match correctness

A: "Verify that the RegexValidator rejects invalid email addresses missing the @ symbol."
  "verify" → TEST.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P050-003 — GAP: "match the pattern" fires zero predicates

A: "Match the input string against the email pattern defined in the RegexValidator."
  "match" → domain operation verb; not in any predicate lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P050-004 — TEST: "spec" fires TEST for RegexValidator boundary conditions

A: "Spec the RegexValidator behaviour for strings at the maximum allowed length."
  "spec" → TEST.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P050-005 — GAP: "apply regex rules" fires zero predicates

A: "Apply the regex rules to sanitise user input before storing it."
  "apply" (standalone) → domain operation verb; not in IMPLEMENT lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
