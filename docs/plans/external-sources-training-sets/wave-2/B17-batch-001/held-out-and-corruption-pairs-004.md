# Held-Out and Corruption Pairs (004)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption

---

## B17-HO-004-001 — HELD-OUT: "wire up OAuth" + build → IMPLEMENT + API_CONTRACT + AUTHORIZATION

**Input:**
"Build the identity connector and wire up the OAuth authorization flow."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "AUTHORIZATION"]
SubPhase: 4.3, 3.3, 3.4
TransitionType: feature_addition

**Reasoning:**
- "Build" → IMPLEMENT
- "connector" → API_CONTRACT
- "wire up" → API_CONTRACT (reinforces; fires once)
- "OAuth" → AUTHORIZATION

**Gate B:** AUTHORIZATION must fire. SECURITY must NOT fire (no token, authentication, or validate signature vocabulary present).

---

## B17-HO-004-002 — HELD-OUT: "OAuth" alone (no other security vocabulary) → AUTHORIZATION only

**Input:**
"Build the Auth0 connector using the OAuth 2.0 protocol."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "AUTHORIZATION"]
SubPhase: 4.3, 3.3, 3.4
TransitionType: feature_addition

**Key test:** "OAuth" present, no "token" or "authentication" → AUTHORIZATION fires, SECURITY does NOT.

---

## B17-HO-004-003 — CORRUPTION: "OAuth" wrongly fires SECURITY only (no AUTHORIZATION)

**Input:**
"Build the Auth0 connector and implement the OAuth authorization code flow."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
(Error: "OAuth" was mapped to SECURITY instead of AUTHORIZATION)

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "AUTHORIZATION"]
SubPhase: 4.3, 3.3, 3.4

**Error:** "oauth" → AUTHORIZATION (Rule 7). Not SECURITY. The model confused authorization protocol with security predicate.

**Correction:** Rule 7 is unambiguous: "oauth" (any case) → AUTHORIZATION. Not SECURITY.

---

## B17-HO-004-004 — CORRUPTION: "OAuth" fires SECURITY AND invents AUTHORIZATION without "token"

**Input:**
"Build the Auth0 OAuth connector."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
(Error: Both SECURITY and AUTHORIZATION fired from "OAuth" alone)

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "AUTHORIZATION"]
(SECURITY should NOT fire — no token, authentication, or validate/secure vocabulary)

**Error:** "OAuth" → AUTHORIZATION (correct) but also → SECURITY (wrong; SECURITY requires its own trigger). SECURITY cannot piggyback on OAuth. They are independent predicates.
