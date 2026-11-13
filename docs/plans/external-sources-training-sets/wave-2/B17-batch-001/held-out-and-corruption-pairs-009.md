# Held-Out and Corruption Pairs (009)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption

---

## B17-HO-009-001 — HELD-OUT: "integrate API" only → API_CONTRACT, routing 3.3

**Input:**
"Integrate the Shopify payment API for the store checkout."

**Expected output:**
CompiledPredicateIds: ["API_CONTRACT"]
SubPhase: 3.3
TransitionType: documentation
DecisionCount: 1

**Gate B:** Output must NOT contain IMPLEMENT. SubPhase must be 3.3 (not 4.3).

---

## B17-HO-009-002 — HELD-OUT: "integrate" + "API" is a pure API_CONTRACT case

**Input:**
"Integrate the Salesforce CRM API for contact synchronization."

**Expected output:**
CompiledPredicateIds: ["API_CONTRACT"]
SubPhase: 3.3
TransitionType: documentation
DecisionCount: 1

**Same result as above.** "Integrate" + API always → API_CONTRACT only → 3.3 → documentation.

---

## B17-HO-009-003 — CORRUPTION: "integrate API" only → wrongly routes to 4.3

**Input:**
"Integrate the Stripe payment API for the checkout flow."

**CORRUPTED output:**
CompiledPredicateIds: ["API_CONTRACT"]
SubPhase: 4.3  ← WRONG

**CORRECT output:**
SubPhase: 3.3

**Error type:** v6-03 (wrong sub-phase for API_CONTRACT). API_CONTRACT alone always routes to 3.3. 4.3 requires IMPLEMENT. The integration vocabulary in the input misleads the router to select 4.3 — but without IMPLEMENT in the predicate set, 4.3 is never valid.

**Rule 10 is absolute:** API_CONTRACT alone → 3.3. Integration vocabulary in the input is irrelevant to this rule. 4.3 requires IMPLEMENT predicate to be present.

---

## B17-HO-009-004 — CORRUPTION: "integrate API" → IMPLEMENT invented + 4.3

**Input:**
"Integrate the PayPal payment API for the order processing module."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3, 3.3
(Error: IMPLEMENT invented from "integrate")

**CORRECT output:**
CompiledPredicateIds: ["API_CONTRACT"]
SubPhase: 3.3
TransitionType: documentation

**Double error:** v6-01 (IMPLEMENT invented) AND consequently the routing also changes to 4.3 (because the invented IMPLEMENT is routed there). Fixing v6-01 fixes both issues.
