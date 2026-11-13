# Layer 0-C — Knowledge-Edge Pairs (041)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Splunk connector

---

## B17-0C-041-001 — Splunk: non-trigger terms

**Proper nouns — no predicate:** Splunk
**Domain nouns — no predicate:** index, search, event, log, field, sourcetype, dashboard, alert, saved search, data model
**Acronyms — no predicate:** HEC (HTTP Event Collector), SPL (Splunk Processing Language)

---

## B17-0C-041-002 — Splunk: "HEC" alone does not fire

HEC (HTTP Event Collector) is a Splunk-specific acronym. It fires no predicate. The word "webhook" (if present) fires API_CONTRACT; "connector" fires API_CONTRACT. HEC itself is not a trigger.

---

## B17-0C-041-003 — Splunk: HMAC phrase fires SECURITY; "validate payload" alone does not

- "validate the HMAC-SHA256 payload signature" → SECURITY fires (Rule 8 complete phrase)
- "validate the payload" → no SECURITY (incomplete phrase — no HMAC reference)
- "HEC token" → SECURITY fires (compound contains "token")

---

## B17-0C-041-004 — Splunk: six-predicate input edge

**Input:** "Build the Splunk connector, implement OAuth 2.0 token refresh, write tests, and document the ingestion API."

All six predicates fire: IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION, TEST, DOCUMENT.
DecisionCount: 6, UniqueSubPhases: 5 (4.3 + 3.3 + 3.4 + 5.1 + 2.1).
This is the maximum predicate count without QA and is achievable for any connector in B17.
