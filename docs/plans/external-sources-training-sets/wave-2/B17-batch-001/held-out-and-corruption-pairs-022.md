# Held-Out and Corruption Pairs (022)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption | focus: Avalara tax connector

---

## B17-HO-022-001 — HELD-OUT: "implement" + "bearer token" → IMPLEMENT + API_CONTRACT + SECURITY

**Input:**
"Implement the Avalara REST connector and include bearer token authentication with automatic credential token renewal."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
SubPhase: 4.3, 3.3, 3.4
TransitionType: feature_addition

**Gate B:** SECURITY from "bearer token" (Rule 6) + "authentication" (Rule 8). IMPLEMENT → 4.3. AUTHORIZATION absent (no "OAuth" present).

---

## B17-HO-022-002 — HELD-OUT: "integrate" Avalara → API_CONTRACT only

**Input:**
"Integrate the Avalara AvaTax API for real-time sales tax calculation at checkout."

**Expected output:**
CompiledPredicateIds: ["API_CONTRACT"]
SubPhase: 3.3
TransitionType: documentation

**Gate B:** IMPLEMENT absent. "integrate" → API_CONTRACT only. Tax vocabulary fires no predicates.

---

## B17-HO-022-003 — CORRUPTION: "Basic auth" treated as SECURITY trigger (v6-02 variant)

**Input:**
"Implement the Avalara connector with Basic auth."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]

**Error:** SECURITY invented for "Basic auth". "Basic auth" contains neither "token" nor "authentication" — it is an authentication method label but not a SECURITY trigger. The SECURITY triggers are: token, authentication, authenticate, validate signature, secure, HMAC validation. "Basic auth" does not match any of these.

---

## B17-HO-022-004 — CORRUPTION: "void" treated as IMPLEMENT trigger (v6-01 variant)

**Input:**
"Build the Avalara connector and void the transaction."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "IMPLEMENT"] (double IMPLEMENT from "build" and "void")

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]

**Error:** "void" treated as an IMPLEMENT trigger. "Void transaction" is an accounting operation. "void" is not in the IMPLEMENT trigger list (build, implement, write, create, code, develop, refactor, restructure, rewrite). Even if "void" were a trigger, predicates are sets — IMPLEMENT would appear only once.
