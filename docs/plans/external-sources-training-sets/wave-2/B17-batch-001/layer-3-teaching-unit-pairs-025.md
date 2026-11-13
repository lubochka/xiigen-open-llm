# Layer 3 — Teaching Unit Pairs (025)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: teaching-unit | focus: Twilio SMS connector

---

## B17-TU-025-001 — "integrate Twilio" vs "build Twilio connector"

**A:** What is the difference in predicate output between "Integrate the Twilio Messaging API" and "Build the Twilio SMS connector"?

**B:** "Integrate the Twilio Messaging API" → ["API_CONTRACT"] → 3.3 → documentation. "Build the Twilio SMS connector" → ["IMPLEMENT", "API_CONTRACT"] → 4.3 + 3.3 → feature_addition. Rule 1 is absolute: "integrate" fires only API_CONTRACT. "Build" fires IMPLEMENT; "connector" fires API_CONTRACT.

---

## B17-TU-025-002 — Twilio: when does SECURITY fire?

**A:** For the Twilio connector, what is required for SECURITY to fire?

**B:** The word "token" must appear in the input. "Twilio authentication token" → SECURITY fires (Rule 6). "Twilio webhook" alone → no SECURITY. "Twilio API key" alone → no SECURITY (no "token"). The trigger is lexical: the word "token" in any compound noun.

---

## B17-TU-025-003 — Twilio: HMAC-SHA256 as an alternative SECURITY trigger

**A:** Can SECURITY fire for Twilio without "token" in the input?

**B:** Yes. SECURITY fires from two sources: Rule 6 ("token") and Rule 8 (HMAC validation phrase). "Validate the HMAC-SHA256 X-Twilio-Signature on each request" → SECURITY fires (Rule 8) even without "token". Both rules produce the same SecurityAuditFlow34 selection.

---

## B17-TU-025-004 — Twilio: predicate deduplication

**A:** "Build the Twilio connector with webhook delivery and authentication token; validate the HMAC-SHA256 signature." How many times does SECURITY appear in CompiledPredicateIds?

**B:** Once. SECURITY fires from "token" (Rule 6) and from "validate the HMAC-SHA256 signature" (Rule 8), but CompiledPredicateIds is a deduplicated set. SECURITY appears exactly once regardless of how many triggers fire it. DecisionCount counts predicates, not trigger occurrences.
