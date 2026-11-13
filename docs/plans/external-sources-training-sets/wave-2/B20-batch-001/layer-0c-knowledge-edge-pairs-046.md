# Layer 0-C — Knowledge Edge Pairs (046) — HashMapImpl: gap analysis for hash-domain verbs and boundary "verify"
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E046-001 — "compute the hash" does NOT fire IMPLEMENT

"Compute the hash of the key to locate the correct bucket in the HashMap." → CompiledPredicateIds: []
"compute" is a domain computation verb. It is NOT in the IMPLEMENT predicate lexicon.
"locate" → domain operation verb; also zero predicates.

---

## E046-002 — "insert and retrieve" are domain verbs; "verify" in same sentence DOES fire TEST

"Insert and retrieve key-value pairs from the HashMapImpl to verify throughput." → CompiledPredicateIds: ["TEST"]
"insert" → domain operation verb; zero predicate. "retrieve" → domain operation verb; zero predicate.
"verify" → IS in the TEST predicate lexicon; fires TEST independently.
Critical boundary: "verify" fires TEST even when the primary action verbs are domain-only.
The action verb governs when it is the PRIMARY verb; "verify" here is a purpose clause and still fires.

---

## E046-003 — "collision" domain term does NOT fire any predicate

"Resolve hash collisions in the HashMapImpl using open addressing." → CompiledPredicateIds: []
"resolve" → domain operation verb; zero predicates. "collision" → domain term; zero predicates.
"open addressing" → domain algorithm term; zero predicates.

---

## E046-004 — "chaining" domain term does NOT fire any predicate

"Use chaining to handle collisions in the HashMapImpl." → CompiledPredicateIds: []
"use" → context verb; does not fire predicates. "chaining" → domain term; zero predicates.
Gap type: implementation technique instruction without construction vocabulary.
