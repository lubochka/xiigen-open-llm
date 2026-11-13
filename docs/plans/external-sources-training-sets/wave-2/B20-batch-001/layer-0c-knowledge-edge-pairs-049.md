# Layer 0-C — Knowledge Edge Pairs (049) — StringParser: gap analysis for parse-domain verbs
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E049-001 — "parse" does NOT fire IMPLEMENT

"Parse the input string using the configured delimiter in the StringParser." → CompiledPredicateIds: []
"parse" is a domain operation verb. It is NOT in the IMPLEMENT predicate lexicon.
Confirmed gap: string processing verbs (parse, split, tokenise, trim, normalise) all zero predicates.

---

## E049-002 — "split the token stream" does NOT fire IMPLEMENT

"Split the token stream at semicolon delimiters using the StringParser." → CompiledPredicateIds: []
"split" → domain operation verb; zero predicates.
Token stream processing is a domain operation, not a construction action.

---

## E049-003 — "delimiter" domain term does NOT fire any predicate

"Configure the StringParser to use a pipe | delimiter for CSV (Comma-Separated Values) files." → CompiledPredicateIds: []
"configure" → domain operation verb; zero predicates. "delimiter" → domain term; zero predicates.
CSV (Comma-Separated Values) → domain abbreviation; zero predicates.

---

## E049-004 — "ArgumentNullException" in TEST context vs execution context

"Assert that the StringParser throws ArgumentNullException when input is null." → CompiledPredicateIds: ["TEST"]
  "assert" → TEST fires. Exception type (ArgumentNullException) is domain vocabulary; does not fire predicates.
"The StringParser throws ArgumentNullException when input is null." → CompiledPredicateIds: []
  No action verb present that fires a predicate. Statement-only request produces zero predicates.
Teaching: the exception type does not determine predicate firing; the action verb does.
