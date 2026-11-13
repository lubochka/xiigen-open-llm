# Held-Out and Corruption Pairs (014)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption | focus: Mailgun email connector

---

## B17-HO-014-001 — HELD-OUT: "build" + "webhook" → IMPLEMENT + API_CONTRACT

**Input:**
"Build the Mailgun email connector and implement the HMAC-SHA256 signature validation for delivery and bounce webhook events."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
SubPhase: 4.3, 3.3, 3.4
TransitionType: feature_addition

**Gate B:** Output must contain SECURITY (from "HMAC-SHA256 signature validation" — Rule 8). SubPhase 3.4 must be present.

---

## B17-HO-014-002 — HELD-OUT: "integrate" alone → API_CONTRACT only

**Input:**
"Integrate Mailgun into the notification pipeline for transactional email delivery."

**Expected output:**
CompiledPredicateIds: ["API_CONTRACT"]
SubPhase: 3.3
TransitionType: documentation

**Gate B:** IMPLEMENT must NOT be present. SubPhase must be 3.3 only. TransitionType must be documentation.

---

## B17-HO-014-003 — CORRUPTION: "build email sender" without integration vocabulary → 4.3 routing error (v6-03)

**Input:**
"Build the EmailNotificationService and implement the send and retry logic."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3, 3.3

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT"]
SubPhase: 4.1
TransitionType: feature_addition

**Error:** Two errors combined:
1. API_CONTRACT invented without connector/webhook/API trigger (v6-01 variant)
2. IMPLEMENT routed to 4.3 without integration vocabulary (v6-03)

"EmailNotificationService" contains "service" — internal vocabulary → 4.1. No connector, webhook, API, or integration vocabulary → no API_CONTRACT, no 4.3.

---

## B17-HO-014-004 — CORRUPTION: SECURITY suppressed for "validate the HMAC signature" (v6-02)

**Input:**
"Create the Mailgun webhook connector and validate the HMAC signature."

**CORRUPTED output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]

**CORRECT output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

**Error:** SECURITY suppressed despite "validate the HMAC signature" being an explicit Rule 8 trigger. v6-02: predicate suppression.
