# Layer 0-C — Knowledge Edge Pairs (050) — RegexValidator: gap analysis for regex-domain verbs
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E050-001 — "match" does NOT fire TEST or IMPLEMENT

"Match the input string against the email pattern defined in the RegexValidator." → CompiledPredicateIds: []
"match" is a domain operation verb. It is NOT in any predicate lexicon.
Regex operation verbs: match, pattern-match, test-against (note: "test-against" is a compound; "test" alone fires TEST).

---

## E050-002 — "apply regex rules" does NOT fire IMPLEMENT

"Apply the regex rules to sanitise user input before storing it." → CompiledPredicateIds: []
"apply" (standalone) → NOT in IMPLEMENT lexicon; zero predicates.
"sanitise" → domain operation verb; zero predicates.
Double gap: two domain verbs, zero predicates.

---

## E050-003 — "regex pattern" does NOT fire TEST or SECURITY

"Update the regex pattern in the RegexValidator to support Unicode characters." → CompiledPredicateIds: []
"update" → NOT in IMPLEMENT lexicon (IMPLEMENT is: write, create, build, develop, implement, add, extend, generate).
"regex pattern" → domain term; zero predicates. "Unicode" → technology term; does not fire SECURITY.
Confirmed: "update" is NOT in the IMPLEMENT lexicon; this is a recurring gap trap.

---

## E050-004 — "sanitise" is a domain verb, NOT a SECURITY trigger

"Sanitise user input using the RegexValidator before database insertion." → CompiledPredicateIds: []
"sanitise" → domain operation verb; zero predicates.
SECURITY fires on: security, authentication, JWT (JSON Web Token), token, encryption, authorization, vulnerability, hashing.
"sanitise" and "sanitisation" do NOT fire SECURITY even in security-adjacent contexts.
