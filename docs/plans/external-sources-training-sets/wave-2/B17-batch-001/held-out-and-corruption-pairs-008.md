# Held-Out and Corruption Pairs (008)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption

---

## B17-HO-008-001 — HELD-OUT: Five predicates (IMPLEMENT + API_CONTRACT + SECURITY + AUTHORIZATION + TEST)

**Input:**
"Build the Auth0 OAuth identity connector, secure the client credentials with token-based auth, and write comprehensive unit tests."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST"]
SubPhase: 4.3, 3.3, 3.4, 3.4, 5.1
DecisionCount: 5
TransitionType: feature_addition

**Reasoning:**
- "Build" → IMPLEMENT
- "connector" → API_CONTRACT
- "OAuth" → AUTHORIZATION
- "secure" + "token-based auth" (token) → SECURITY (two SECURITY triggers, fires once)
- "write unit tests" → TEST

**Gate B:** All five predicates present in canonical order.

---

## B17-HO-008-002 — HELD-OUT: Five predicates from slightly different phrasing

**Input:**
"Implement the OAuthIdentityConnector with token refresh, validate OAuth tokens for security, and add integration test coverage."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST"]
SubPhase: 4.3, 3.3, 3.4, 3.4, 5.1
DecisionCount: 5
TransitionType: feature_addition

---

## B17-HO-008-003 — CORRUPTION: AUTHORIZATION suppressed (only SECURITY fires)

**Input:**
"Build the Auth0 OAuth connector and implement token refresh flow."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
(Error: "OAuth" present but AUTHORIZATION not fired; only SECURITY from "token" fired)

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

**Error type:** v6-02 (AUTHORIZATION suppressed). "OAuth" → AUTHORIZATION must fire independently of SECURITY. They are separate predicates.

**DecisionCount impact:** 3 (corrupted) vs 4 (correct). The suppression of AUTHORIZATION reduces DecisionCount by 1 and omits AuthorizationFlow34.

---

## B17-HO-008-004 — CORRUPTION: SECURITY suppressed (only AUTHORIZATION fires)

**Input:**
"Build the Auth0 OAuth connector and store the access token securely."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "AUTHORIZATION"]
(Error: "token" present but SECURITY not fired; only AUTHORIZATION from "OAuth" fired)

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

**Error type:** v6-02 (SECURITY suppressed from "token"). DecisionCount should be 4 (corrupted output gives 3).
