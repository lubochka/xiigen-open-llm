# Held-Out and Corruption Pairs (006)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption

---

## B17-HO-006-001 — HELD-OUT: "refactor connector" → refactoring TransitionType

**Input:**
"Refactor the legacy Shopify connector to use the Admin REST API 2024-01."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3, 3.3
TransitionType: refactoring

**Gate B:** TransitionType must be "refactoring" (not "feature_addition"). Routing destinations 4.3 and 3.3 are the same as feature_addition.

---

## B17-HO-006-002 — HELD-OUT: "restructure" fires IMPLEMENT + refactoring

**Input:**
"Restructure the PayPal connector to use the v2 REST API."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3, 3.3
TransitionType: refactoring

**Key test:** "restructure" is in the refactoring verb set. Same output as "refactor".

---

## B17-HO-006-003 — CORRUPTION: "refactor" → TransitionType: feature_addition (wrong)

**Input:**
"Refactor the StripeConnector to use the new payment API."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3, 3.3
TransitionType: feature_addition  ← WRONG

**CORRECT output:**
TransitionType: refactoring

**Error:** "refactor" present → TransitionType must be "refactoring". feature_addition was used as the default without checking for refactoring verb. Rule 14 and Rule 15 override the default.

---

## B17-HO-006-004 — CORRUPTION: "refactor" routes IMPLEMENT to 4.1

**Input:**
"Refactor the payment gateway connector to use the new SDK."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.1 (IMPLEMENT), 3.3 (API_CONTRACT)
TransitionType: refactoring

**CORRECT output:**
SubPhase: 4.3 (IMPLEMENT), 3.3 (API_CONTRACT)
TransitionType: refactoring

**Two errors:** TransitionType is correct (refactoring), but sub-phase is wrong (4.1 instead of 4.3). "connector" and "gateway" are integration vocabulary → IMPLEMENT routes to 4.3 even in refactoring scenarios. v6-03 applies to refactoring scenarios as much as to feature_addition scenarios.
