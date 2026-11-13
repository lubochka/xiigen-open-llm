# Layer 0-A — Compiler Pairs (023)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: compiler | focus: Multi-connector orchestration

---

## B17-0A-023-001 — Multi-connector build: two connectors, one request → IMPLEMENT + API_CONTRACT

**Input:**
"Build the Stripe and Avalara connector pair — implement the checkout payment connector and the tax calculation connector as a coordinated integration."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT (fires once — predicate does not multiply per occurrence)
- Integration vocabulary: connector (appears twice), integration → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**SubPhase:** 4.3
**TransitionType:** feature_addition
**DomainId:** software-development

**Teaching point:** Even when "connector" appears multiple times, the predicate API_CONTRACT fires once. Predicates are sets, not counts.

---

## B17-0A-023-002 — Multi-connector with auth: IMPLEMENT + API_CONTRACT + SECURITY + AUTHORIZATION

**Input:**
"Build the HubSpot and Segment connector bridge, implement the OAuth 2.0 token exchange for both providers, and secure the shared webhook endpoint with signature validation."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT (fires once)
- "webhook" → API_CONTRACT (reinforces)
- "endpoint" → API_CONTRACT (reinforces)
- "OAuth" → AUTHORIZATION (Rule 7)
- "token" → SECURITY (Rule 6: "token exchange" fires SECURITY)
- "secure" → SECURITY (reinforces; fires once)
- "signature validation" → SECURITY (reinforces; fires once)
- Integration vocabulary: connector, OAuth, token, webhook, endpoint → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
**SubPhase:** 4.3, 3.3, 3.4
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-023-003 — "integrate" multiple connectors → API_CONTRACT only (no IMPLEMENT)

**Input:**
"Integrate the Intercom, Mailgun, and Square connectors with the central CRM orchestration layer."

**Compiler analysis:**
- "Integrate" → API_CONTRACT (Rule 1: integrate fires API_CONTRACT ONLY — applies even in multi-connector context)
- "connector" → API_CONTRACT (reinforces; fires once)
- No build verb → IMPLEMENT does NOT fire
- Result: API_CONTRACT only

**CompiledPredicateIds:** ["API_CONTRACT"]
**SubPhase:** 3.3
**TransitionType:** documentation
**DomainId:** software-development

**Teaching point:** The multi-connector context does NOT change the "integrate" rule. Even integrating three named connectors, "integrate" alone fires API_CONTRACT only.

---

## B17-0A-023-004 — "build" + "orchestrate" multiple connectors → IMPLEMENT + API_CONTRACT

**Input:**
"Build the multi-provider payment orchestrator that wraps the Stripe and Square connectors and implements failover routing between payment providers."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "implements" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT
- "orchestrate" — not a standard predicate trigger; no predicate
- Integration vocabulary: connector → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**SubPhase:** 4.3
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-023-005 — Full multi-connector pipeline: all seven predicates

**Input:**
"Build the FedEx and Zuora connector pair, implement the OAuth token exchange, secure the credentials, document the integration API contracts, write end-to-end tests for the shipping-billing pipeline, and run a QA verification on the complete multi-connector flow."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT (fires once)
- "OAuth" → AUTHORIZATION (Rule 7)
- "token" → SECURITY (Rule 6)
- "secure" → SECURITY (reinforces; fires once)
- "document" → DOCUMENT (Rule 12)
- "integration API" → API_CONTRACT (reinforces)
- "write end-to-end tests" → TEST (Rule 13)
- "QA" → QA (Rule 21: abbreviation fires QA; always last)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST", "DOCUMENT", "QA"]
**SubPhase:** 4.3, 3.3, 3.4, 5.1, 2.1, 7.x
**TransitionType:** feature_addition
**DomainId:** software-development

**Teaching point:** Maximum predicate set (7) in a multi-connector context. Rules are identical to single-connector context. No new predicates are introduced by the multi-connector pattern.
