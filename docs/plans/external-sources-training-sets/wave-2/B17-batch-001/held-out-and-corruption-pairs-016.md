# Held-Out and Corruption Pairs (016)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption | focus: HubSpot CRM connector

---

## B17-HO-016-001 — HELD-OUT: "build" + "webhook" + "signature" → IMPLEMENT + API_CONTRACT + SECURITY

**Input:**
"Create the HubSpot webhook connector for deal stage change events and implement request signature validation to secure the inbound payloads."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
SubPhase: 4.3, 3.3, 3.4
TransitionType: feature_addition

**Gate B:** SECURITY present (from "signature validation" + "secure"). IMPLEMENT → 4.3.

---

## B17-HO-016-002 — HELD-OUT: "integrate" HubSpot → API_CONTRACT only

**Input:**
"Integrate HubSpot CRM with the lead management workflow for automatic contact creation."

**Expected output:**
CompiledPredicateIds: ["API_CONTRACT"]
SubPhase: 3.3
TransitionType: documentation

**Gate B:** IMPLEMENT absent. "CRM" fires no predicate. "integrate" → API_CONTRACT only.

---

## B17-HO-016-003 — CORRUPTION: "CRM" treated as integration vocabulary (v6-03 variant)

**Input:**
"Build the HubSpot CRM connector."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "API_CONTRACT"]  (predicate duplicated)
OR
SubPhase: 4.3 attributed to "CRM" rather than "connector"

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3 (from "connector"), 3.3

**Error:** "CRM" is a transparent acronym. The 4.3 routing comes from "connector" (Rule 4), not from "CRM". Predicates cannot be duplicated (set rule).

---

## B17-HO-016-004 — CORRUPTION: "endpoint" routes IMPLEMENT to 4.3 without build verb

**Input:**
"Integrate the HubSpot deal endpoint for pipeline updates."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3, 3.3

**CORRECT output:**
CompiledPredicateIds: ["API_CONTRACT"]
SubPhase: 3.3
TransitionType: documentation

**Error:** Two errors:
1. IMPLEMENT invented (no build verb — only "integrate")
2. "endpoint" treated as if it can activate 4.3 alone (it cannot — 4.3 requires IMPLEMENT)

"endpoint" is an API_CONTRACT reinforcer. It reinforces API_CONTRACT but cannot activate 4.3 without IMPLEMENT.
