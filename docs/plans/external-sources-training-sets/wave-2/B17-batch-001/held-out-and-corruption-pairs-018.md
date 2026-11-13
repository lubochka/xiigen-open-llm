# Held-Out and Corruption Pairs (018)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption | focus: Zuora billing connector

---

## B17-HO-018-001 — HELD-OUT: "build" + "OAuth" + "token" → 4 predicates

**Input:**
"Build the Zuora REST connector, implement the OAuth 2.0 client credentials flow, and secure the bearer token for all billing API calls."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
SubPhase: 4.3, 3.3, 3.4
TransitionType: feature_addition

**Gate B:** AUTHORIZATION from "OAuth 2.0". SECURITY from "bearer token" + "secure". IMPLEMENT → 4.3.

---

## B17-HO-018-002 — HELD-OUT: "integrate" Zuora → API_CONTRACT only

**Input:**
"Integrate the Zuora billing API to manage recurring subscription lifecycle events."

**Expected output:**
CompiledPredicateIds: ["API_CONTRACT"]
SubPhase: 3.3
TransitionType: documentation

**Gate B:** IMPLEMENT absent. "integrate" → API_CONTRACT only. "billing" and "subscription" fire no predicates.

---

## B17-HO-018-003 — CORRUPTION: "subscription" treated as integration vocabulary (v6-03)

**Input:**
"Implement the Zuora subscription billing connector."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3 (attributed to "subscription")

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3 (correctly attributed to "connector")

**Error:** The 4.3 routing is correct but the attribution is wrong. "subscription" is domain vocabulary (no predicate). The 4.3 routing comes from "connector" (integration vocabulary), not from "subscription". This corruption matters for explainability: the model must correctly identify "connector" as the integration vocabulary trigger, not "subscription".

---

## B17-HO-018-004 — CORRUPTION: "OAuth client credentials" → SECURITY invented without "token" (v6-02 reverse)

**Input:**
"Build the Zuora connector, implement the OAuth 2.0 client credentials flow."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "AUTHORIZATION"]

**Error:** SECURITY invented without a SECURITY trigger. "OAuth 2.0 client credentials flow" fires AUTHORIZATION (from "OAuth") but does NOT fire SECURITY unless "token" or "authenticate" or "secure" or "signature" is also present in the input. "Client credentials" is not a SECURITY trigger.
