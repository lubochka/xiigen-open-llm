# Held-Out and Corruption Pairs (015)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption | focus: FedEx/UPS shipping connector

---

## B17-HO-015-001 — HELD-OUT: "build" + "connector" + "OAuth" + "token"

**Input:**
"Create the FedEx OAuth connector, implement the API access token refresh logic, and secure the credential store."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
SubPhase: 4.3, 3.3, 3.4
TransitionType: feature_addition

**Gate B:** All four predicates present. IMPLEMENT → 4.3. SECURITY from "token" + "secure". AUTHORIZATION from "OAuth".

---

## B17-HO-015-002 — HELD-OUT: "FedEx" without integration vocabulary → 4.1

**Input:**
"Implement the FedEx rate calculation service and build the shipping cost estimation logic."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT"]
SubPhase: 4.1
TransitionType: feature_addition

**Gate B:** IMPLEMENT → 4.1 (no integration vocabulary: no connector, no webhook, no API, no endpoint). "FedEx" is a proper noun — no predicate.

---

## B17-HO-015-003 — CORRUPTION: "FedEx" proper noun treated as integration vocabulary (v6-03)

**Input:**
"Implement FedEx shipping."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3, 3.3

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT"]
SubPhase: 4.1

**Error:** "FedEx" (proper noun) treated as integration vocabulary, causing IMPLEMENT → 4.3 (wrong) and API_CONTRACT invented. "FedEx" is a proper noun with no predicate. IMPLEMENT routes to 4.1 without explicit integration vocabulary.

---

## B17-HO-015-004 — CORRUPTION: "integrate" → IMPLEMENT for shipping (v6-01)

**Input:**
"Integrate the FedEx Ship Manager API to calculate shipping rates at checkout."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3, 3.3

**CORRECT output:**
CompiledPredicateIds: ["API_CONTRACT"]
SubPhase: 3.3
TransitionType: documentation

**Error:** IMPLEMENT invented from "integrate". Rule 1 applies to all connector domains including shipping. "integrate" fires API_CONTRACT only.
