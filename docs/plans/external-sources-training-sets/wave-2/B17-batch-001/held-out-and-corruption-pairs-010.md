# Held-Out and Corruption Pairs (010)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption

---

## B17-HO-010-001 — HELD-OUT: "authentication" → SECURITY (not AUTHORIZATION alone)

**Input:**
"Build the Stripe connector and implement authentication for the payment API requests."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
SubPhase: 4.3, 3.3, 3.4
TransitionType: feature_addition

**Gate B:** SECURITY fires (from "authentication"). AUTHORIZATION does NOT fire (no "oauth" present).

---

## B17-HO-010-002 — HELD-OUT: "API authentication" → SECURITY only (no AUTHORIZATION)

**Input:**
"Implement the Twilio connector with API authentication using the account SID and auth token."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
SubPhase: 4.3, 3.3, 3.4

**Note:** "auth token" contains "token" → SECURITY fires. "authentication" → SECURITY fires. AUTHORIZATION does NOT fire (no "oauth").

---

## B17-HO-010-003 — CORRUPTION: "authentication" wrongly fires AUTHORIZATION only

**Input:**
"Build the Stripe connector with API key authentication."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "AUTHORIZATION"]
(Error: "authentication" was mapped to AUTHORIZATION instead of SECURITY)

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

**Error:** Rule 8 is absolute: "authentication" → SECURITY. AUTHORIZATION requires "oauth". There is no "oauth" in this input. The model conflated authentication (identity verification) with authorization (OAuth protocol).

---

## B17-HO-010-004 — CORRUPTION: "authentication" fires both SECURITY and AUTHORIZATION (over-firing)

**Input:**
"Build the Plaid connector with account authentication."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
(Error: "authentication" triggered BOTH SECURITY and AUTHORIZATION)

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
(AUTHORIZATION requires "oauth" — not present)

**Error:** "authentication" fires SECURITY only. It does not also fire AUTHORIZATION. Only "oauth" fires AUTHORIZATION. The model invented AUTHORIZATION as a secondary effect of "authentication" — a v6-01 variant.
