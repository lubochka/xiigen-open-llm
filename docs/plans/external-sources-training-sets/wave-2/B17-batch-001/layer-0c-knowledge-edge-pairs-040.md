# Layer 0-C — Knowledge-Edge Pairs (040)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Datadog connector

---

## B17-0C-040-001 — Datadog: non-trigger terms

**Proper nouns — no predicate:** Datadog
**Domain nouns — no predicate:** metric, trace, log, dashboard, monitor, alert, APM (Application Performance Monitoring), infrastructure, host, tag, span, service, pipeline
**Acronyms — no predicate:** APM (Application Performance Monitoring), SLO (Service Level Objective), SLI (Service Level Indicator)

---

## B17-0C-040-002 — Datadog: "API key" vs "application key" vs "token"

- "Datadog API key" → no SECURITY (no "token")
- "Datadog application key" → no SECURITY (no "token")
- "Datadog API key token" → SECURITY fires
- "Datadog agent authentication token" → SECURITY fires
Teaching: Datadog uses API keys (not tokens) by default. The model must not invent SECURITY unless "token" explicitly appears.

---

## B17-0C-040-003 — Datadog: "monitoring" does not imply integration vocabulary

"Monitoring" is a domain noun. It does not supply integration vocabulary to IMPLEMENT. Only "connector", "webhook", "SDK", "API" in integration context route IMPLEMENT to 4.3.

---

## B17-0C-040-004 — Datadog: refactoring TransitionType

"Refactor" fires IMPLEMENT — but TransitionType becomes "refactoring", not "feature_addition". Routing (4.3, 3.3) remains the same. TransitionType and routing are independent properties.
