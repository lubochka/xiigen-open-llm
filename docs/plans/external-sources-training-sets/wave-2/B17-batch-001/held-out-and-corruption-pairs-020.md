# Held-Out and Corruption Pairs (020)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption | focus: Intercom messaging connector

---

## B17-HO-020-001 — HELD-OUT: "build" + "webhook" + "signature validation"

**Input:**
"Build the Intercom webhook connector and implement signature validation using the Intercom shared secret."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
SubPhase: 4.3, 3.3, 3.4
TransitionType: feature_addition

**Gate B:** SECURITY from "signature validation". IMPLEMENT → 4.3. "shared secret" does NOT fire SECURITY independently.

---

## B17-HO-020-002 — HELD-OUT: "integrate" Intercom → API_CONTRACT only

**Input:**
"Integrate the Intercom messaging API to push support ticket updates to the customer chat widget."

**Expected output:**
CompiledPredicateIds: ["API_CONTRACT"]
SubPhase: 3.3
TransitionType: documentation

**Gate B:** IMPLEMENT absent. "integrate" + "API" → API_CONTRACT only.

---

## B17-HO-020-003 — CORRUPTION: "conversation routing" treated as connector vocabulary (v6-03)

**Input:**
"Implement the conversation routing logic and message dispatch service for the Intercom support inbox."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3, 3.3

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT"]
SubPhase: 4.1

**Error:** "conversation routing logic" and "message dispatch service" are internal terms — no integration vocabulary. "Intercom" is a proper noun (no predicate). IMPLEMENT routes to 4.1. API_CONTRACT must not be invented without a trigger word.

---

## B17-HO-020-004 — CORRUPTION: "bearer token" SECURITY suppressed (v6-02)

**Input:**
"Write the Intercom REST connector and include bearer token authentication for all outbound API calls."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

**Error:** SECURITY suppressed for "bearer token". Rule 6 is unconditional: "bearer token" contains "token" → SECURITY fires. v6-02: predicate suppression ("bearer token is just authentication, not a SECURITY concern").
