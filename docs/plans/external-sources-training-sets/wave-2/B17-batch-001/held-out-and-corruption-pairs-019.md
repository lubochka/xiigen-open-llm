# Held-Out and Corruption Pairs (019)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption | focus: Okta identity connector

---

## B17-HO-019-001 — HELD-OUT: "build" + "OAuth" + "token" → SECURITY + AUTHORIZATION

**Input:**
"Build the Okta identity connector, implement the OAuth 2.0 PKCE authorization flow, and validate the access token on each request."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
SubPhase: 4.3, 3.3, 3.4
TransitionType: feature_addition

**Gate B:** AUTHORIZATION from "OAuth". SECURITY from "token" + "validate". IMPLEMENT → 4.3.

---

## B17-HO-019-002 — HELD-OUT: "OIDC" connector → no AUTHORIZATION

**Input:**
"Create the Okta OIDC connector and implement the authentication token introspection endpoint."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
SubPhase: 4.3, 3.3, 3.4
TransitionType: feature_addition

**Gate B:** AUTHORIZATION must NOT be present ("OIDC" ≠ "OAuth"). SECURITY from "authentication" + "token".

---

## B17-HO-019-003 — CORRUPTION: "OIDC" → AUTHORIZATION invented (v6-01 variant)

**Input:**
"Build the Okta OIDC connector."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "AUTHORIZATION"]

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]

**Error:** AUTHORIZATION invented for "OIDC". Rule 7 fires on "OAuth" only. "OIDC" is an acronym for OpenID Connect — it is a related protocol but not the word "OAuth". AUTHORIZATION must not fire on "OIDC", "OpenID Connect", "SAML", or "PKCE".

---

## B17-HO-019-004 — CORRUPTION: "authorization" English word → AUTHORIZATION invented (v6-01 variant)

**Input:**
"Build the authorization connector for Okta access control."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "AUTHORIZATION"]

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]

**Error:** AUTHORIZATION invented from the English word "authorization". Rule 7 fires on "OAuth" (the word), not on "authorization" (the English concept). "authorization connector" contains "authorization" as an English descriptor — it does not fire the AUTHORIZATION predicate. AUTHORIZATION fires only from "OAuth" or "oauth" (case-insensitive).
