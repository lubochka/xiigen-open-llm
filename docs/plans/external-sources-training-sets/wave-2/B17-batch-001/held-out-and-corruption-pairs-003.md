# Held-Out and Corruption Pairs (003)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption

---

## B17-HO-003-001 — HELD-OUT: "implement webhook" → IMPLEMENT + API_CONTRACT

**Input:**
"Implement the GitHub webhook handler for push and pull request events."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3 (IMPLEMENT — webhook is integration vocabulary), 3.3 (API_CONTRACT)
TransitionType: feature_addition

**Reasoning:**
- "Implement" → IMPLEMENT
- "webhook" → API_CONTRACT (Rule 5)
- "webhook" also shifts IMPLEMENT to 4.3 (integration vocabulary)

**Gate B:** Output must be ["IMPLEMENT", "API_CONTRACT"] with IMPLEMENT → 4.3 (not 4.1).

---

## B17-HO-003-002 — HELD-OUT: "implement webhook" with security

**Input:**
"Implement the Stripe webhook handler with HMAC signature validation."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
SubPhase: 4.3, 3.3, 3.4
TransitionType: feature_addition

**Gate B:** SECURITY must be present (signature validation detected).

---

## B17-HO-003-003 — CORRUPTION: webhook fires IMPLEMENT only (missing API_CONTRACT)

**Input:**
"Implement the GitHub webhook listener for push events."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT"]
SubPhase: 4.1
(Error: "webhook" was not recognized as API_CONTRACT trigger; treated as generic handler)

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3, 3.3

**Error type:** v6-02 (gap suppression) — "webhook" was present but API_CONTRACT was not fired.
Also v6-03 — IMPLEMENT routed to 4.1 instead of 4.3.

**Two errors in one corrupted output.** Both must be corrected.

---

## B17-HO-003-004 — CORRUPTION: "webhook" fires IMPLEMENT but not API_CONTRACT AND routes to 4.1

**This is the compound error form of v6-02 + v6-03:**
1. "webhook" is present
2. API_CONTRACT should fire → doesn't (v6-02)
3. IMPLEMENT fires but routes to 4.1 (v6-03, because API_CONTRACT's integration context was suppressed)

**Correction process:**
Step 1: Fix v6-02 → "webhook" → API_CONTRACT fires
Step 2: Fix v6-03 → API_CONTRACT's integration context shifts IMPLEMENT to 4.3
Both fixes are interdependent here: suppressing API_CONTRACT caused the sub-phase error.
