# Held-Out and Corruption Pairs (017)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption | focus: Segment analytics connector

---

## B17-HO-017-001 — HELD-OUT: "create" + "connector" + "write tests"

**Input:**
"Create the Segment analytics connector, wire up the event sink API endpoints, and write integration tests for the track and identify calls."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "TEST"]
SubPhase: 4.3, 3.3, 5.1
TransitionType: feature_addition

**Gate B:** TEST present. IMPLEMENT → 4.3. API_CONTRACT from connector + wire up + API + endpoints.

---

## B17-HO-017-002 — HELD-OUT: "data pipeline" without integration vocabulary → 4.1

**Input:**
"Implement the analytics data pipeline with event batching and retry logic."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT"]
SubPhase: 4.1
TransitionType: feature_addition

**Gate B:** IMPLEMENT → 4.1 (no integration vocabulary). "data pipeline" is not integration vocabulary.

---

## B17-HO-017-003 — CORRUPTION: "orchestrate" treated as IMPLEMENT trigger (v6-01 variant)

**Input:**
"Orchestrate the Segment and HubSpot connectors for the CRM data pipeline."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3, 3.3

**CORRECT output:**
CompiledPredicateIds: ["API_CONTRACT"]
SubPhase: 3.3
TransitionType: documentation

**Error:** "Orchestrate" treated as an IMPLEMENT trigger. "Orchestrate" is not in the IMPLEMENT trigger list. Only "connector" fires API_CONTRACT. No build verb → no IMPLEMENT.

---

## B17-HO-017-004 — CORRUPTION: "refactor" sets feature_addition TransitionType (v6-01 variant)

**Input:**
"Refactor the Segment connector to use the Segment HTTP API v1.3 batch endpoint."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: refactoring

**Error:** "refactor" → IMPLEMENT + TransitionType: refactoring (Rule 14). TransitionType must be refactoring, not feature_addition. The predicates and routing are correct but TransitionType is wrong.
