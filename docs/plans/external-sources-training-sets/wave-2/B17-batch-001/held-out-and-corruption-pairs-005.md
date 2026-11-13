# Held-Out and Corruption Pairs (005)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption

---

## B17-HO-005-001 — HELD-OUT: "build and document" → IMPLEMENT + API_CONTRACT + DOCUMENT

**Input:**
"Build the DocuSign connector and document the signing workflow interface."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
SubPhase: 4.3, 3.3, 2.1
TransitionType: feature_addition

**Gate B:** All three predicates present. IMPLEMENT → 4.3 (not 4.1). DOCUMENT → 2.1.

---

## B17-HO-005-002 — HELD-OUT: "build and spec out" → IMPLEMENT + API_CONTRACT + DOCUMENT

**Input:**
"Build the Plaid banking connector and spec out the account linking and balance APIs."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
SubPhase: 4.3, 3.3, 2.1
TransitionType: feature_addition

**Key test:** "spec out" → DOCUMENT (Rule 12). Same predicate as "document".

**Gate B:** DOCUMENT fires from "spec out". Output identical to "build and document" scenario.

---

## B17-HO-005-003 — CORRUPTION: "document" verb ignored

**Input:**
"Build the Salesforce connector and document the contact management API interface."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
(Error: "document" verb was present but DOCUMENT predicate not fired)

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
SubPhase: 4.3, 3.3, 2.1

**Error type:** v6-02 (gap suppression) — "document" was detected but DOCUMENT predicate suppressed. This is less common than SECURITY suppression but follows the same error pattern.

**Rule 12 is absolute:** "document" always fires DOCUMENT. No context dependency.

---

## B17-HO-005-004 — CORRUPTION: DOCUMENT routes to 4.3 instead of 2.1

**Input:**
"Build the Stripe connector and document the payment API."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
SubPhase: 4.3, 3.3, 4.3 (DOCUMENT wrongly routes to 4.3)

**CORRECT output:**
SubPhase: 4.3 (IMPLEMENT), 3.3 (API_CONTRACT), 2.1 (DOCUMENT)

**Error type:** v6-03 (wrong sub-phase for DOCUMENT). DOCUMENT always routes to 2.1. It never routes to 4.3, even in integration context. The integration context affects IMPLEMENT routing only.
