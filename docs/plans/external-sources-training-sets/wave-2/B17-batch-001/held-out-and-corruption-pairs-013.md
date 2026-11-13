# Held-Out and Corruption Pairs (013)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption | focus: Stripe connector

---

## B17-HO-013-001 — HELD-OUT: "build" + "connector" → IMPLEMENT + API_CONTRACT

**Input:**
"Build the Stripe Connect OAuth connector, implement the token exchange flow, and secure the connected account credentials."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
SubPhase: 4.3, 3.3, 3.4
TransitionType: feature_addition

**Reasoning:**
- "Build" → IMPLEMENT; "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT; "OAuth" → AUTHORIZATION; "token" → SECURITY; "secure" → SECURITY

**Gate B:** Output must be exactly ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"] with IMPLEMENT → 4.3.

---

## B17-HO-013-002 — HELD-OUT: "refactor" Stripe connector → refactoring TransitionType

**Input:**
"Refactor the Stripe payment connector to use the Stripe API v2024-06-20 webhook schema."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: refactoring (not feature_addition)
SubPhase: 4.3, 3.3

**Gate B:** TransitionType must be refactoring (not feature_addition). SubPhase routing must be 4.3 + 3.3 (unchanged from feature_addition routing).

---

## B17-HO-013-003 — CORRUPTION: "integrate Stripe" → IMPLEMENT invented (v6-01)

**Input:**
"Integrate the Stripe payment API into the checkout service."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3, 3.3

**CORRECT output:**
CompiledPredicateIds: ["API_CONTRACT"]
SubPhase: 3.3
TransitionType: documentation

**Error:** IMPLEMENT invented from "integrate". "integrate" → API_CONTRACT only (Rule 1). No IMPLEMENT verb present.

---

## B17-HO-013-004 — CORRUPTION: SECURITY suppressed when "idempotency token" present (v6-02)

**Input:**
"Build the Stripe connector with idempotency token handling."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
(SECURITY fires from "token" — Rule 6)

**Error:** SECURITY suppressed. "idempotency token" contains "token" → SECURITY fires unconditionally. v6-02: predicate suppression based on context reasoning ("idempotency token is not a security token").
