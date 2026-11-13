# Layer 0-B — Router Pairs (050) — RegexValidator: routing decisions for TEST and zero-predicate cases
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R050-001 — TEST+IMPLEMENT: "build RegexValidator and add test cases" routes to 5.1 + 4.1

Request: "Build the RegexValidator and add test cases for email, phone, and postal code patterns."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  RoutingDecisions: [TEST → 5.1, IMPLEMENT → 4.1] | TransitionType: feature_addition

---

## R050-002 — TEST alone: "verify" on RegexValidator invalid-input routes to 5.1, documentation

Request: "Verify that the RegexValidator rejects invalid email addresses missing the @ symbol."
  CompiledPredicateIds: ["TEST"]
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation | SourceStatus: routable

---

## R050-003 — Zero predicate: "match the pattern" routes to nowhere

Request: "Match the input string against the email pattern defined in the RegexValidator."
  "match" → domain operation verb; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## R050-004 — Routing minimal pair: "spec" (TEST → 5.1) vs "apply regex" (nowhere)

A-spec: "Spec the RegexValidator behaviour for strings at maximum length." → [TEST → 5.1] | documentation
A-apply: "Apply the regex rules to sanitise user input." → [] | unknown | knowledge_edge
Teaching: "spec" is in the TEST lexicon; "apply" (standalone) is not in any predicate lexicon.
