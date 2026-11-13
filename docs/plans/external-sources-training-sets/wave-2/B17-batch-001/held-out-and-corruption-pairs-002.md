# Held-Out and Corruption Pairs (002)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption

---

## B17-HO-002-001 — HELD-OUT: "build connector to" + "integrate" → IMPLEMENT + API_CONTRACT

**Input:**
"Build a connector to integrate the SendGrid email API into the notification pipeline."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3 (IMPLEMENT), 3.3 (API_CONTRACT)
TransitionType: feature_addition

**Reasoning:**
- "Build" → IMPLEMENT
- "connector" → API_CONTRACT
- "integrate" → API_CONTRACT (reinforces; still fires once)
- Integration vocabulary: connector, integrate, API → 4.3

**Gate B:** Output must be exactly ["IMPLEMENT", "API_CONTRACT"] with IMPLEMENT → 4.3.

---

## B17-HO-002-002 — HELD-OUT: "build connector to" with integration vocabulary reinforcement

**Input:**
"Build a connector to the Stripe payment API for the e-commerce checkout."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3, 3.3
TransitionType: feature_addition

**Subtle test:** "connector to" phrasing (not "connector" standalone). The word "connector" still fires API_CONTRACT regardless of preposition.

---

## B17-HO-002-003 — CORRUPTION: "connect" misclassified as IMPLEMENT

**Input:**
"Connect the PayPal API to the payment processing service."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3, 3.3

**CORRECT output:**
CompiledPredicateIds: ["API_CONTRACT"]
SubPhase: 3.3
TransitionType: documentation

**Error:** "connect" was treated as a build verb → IMPLEMENT invented (v6-01 variant).
"connect" is API_CONTRACT vocabulary. No build verb present. IMPLEMENT must not fire.

---

## B17-HO-002-004 — CORRUPTION: "build connector" routes IMPLEMENT to 4.1

**Input:**
"Build the Stripe connector for the payment module."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.1 (IMPLEMENT), 3.3 (API_CONTRACT)

**CORRECT output:**
SubPhase: 4.3 (IMPLEMENT), 3.3 (API_CONTRACT)

**Error:** IMPLEMENT routed to 4.1. "connector" is integration vocabulary → IMPLEMENT must route to 4.3. This is the classic v6-03 error.
