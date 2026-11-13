# Layer 1 — Transition Orchestrator Pairs (058) — "security" in test context: SECURITY fires for algorithm domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T058-001 — "security tests" fires TEST + SECURITY: algorithm domain example

Request: "Write security tests for the RandomGenerator to verify entropy quality."
  "write" → IMPLEMENT | "security tests" → TEST (via "tests") + SECURITY (via "security")
  "entropy" → domain term; does NOT fire SECURITY independently.
  CompiledPredicateIds: ["TEST", "IMPLEMENT", "SECURITY"] | TransitionType: feature_addition

---

## T058-002 — "security" in domain context does NOT fire SECURITY

Request: "Ensure the RandomGenerator uses a cryptographically secure seed source."
  "ensure" → informal QA; zero predicates. "cryptographically secure" → domain term.
  "secure" (adjective) does NOT fire SECURITY independently.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## T058-003 — SECURITY lexicon confirmed: only explicit security terms fire SECURITY

SECURITY fires on: security, authentication, JWT (JSON Web Token), token, auth, encryption, authorization, signing, hashing, tampered, vulnerability, revocation
SECURITY does NOT fire on: entropy, randomness, seed, distribution, validation (in non-security context)
"secure" (adjective alone) → does NOT fire SECURITY.
"security" (noun/modifier) → DOES fire SECURITY.
