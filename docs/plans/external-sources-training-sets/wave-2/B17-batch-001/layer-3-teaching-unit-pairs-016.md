# Layer 3 — Teaching Unit Pairs (016)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: teaching-unit | focus: HubSpot CRM connector

---

## B17-TU-016-001 — "CRM" acronym is transparent to the predicate system

**A:** Does "CRM" in "Build the HubSpot CRM connector" fire any predicate?

**B:** No. "CRM" (Customer Relationship Management) is a product category acronym. It fires no predicate. The predicate fires from "connector" (API_CONTRACT, Rule 4) and "Build" (IMPLEMENT, Rule 11). The acronym "CRM" is transparent — it is treated as a label with no predicate assignment.

**Rule:** Product category acronyms (CRM, ERP, POS, SaaS, etc.) are transparent to the predicate system.

---

## B17-TU-016-002 — "endpoint" as API_CONTRACT reinforcer vs independent routing signal

**A:** Does "endpoint" alone route to 4.3 (without IMPLEMENT)?

**B:** No. "endpoint" is an API_CONTRACT reinforcer, not an independent routing signal.

**Without IMPLEMENT:**
"Integrate the HubSpot deal endpoint" → ["API_CONTRACT"] → 3.3 (API_CONTRACT alone → 3.3, not 4.3)

**With IMPLEMENT:**
"Build the HubSpot deal endpoint" → ["IMPLEMENT", "API_CONTRACT"] → IMPLEMENT routes to 4.3 (endpoint is integration vocabulary), API_CONTRACT routes to 3.3

"endpoint" shifts the IMPLEMENT routing from 4.1 to 4.3 when IMPLEMENT is present, but it does not independently activate 4.3 without IMPLEMENT.

---

## B17-TU-016-003 — "HubSpot private app token" fires SECURITY (Rule 6)

**A:** "Build the HubSpot private app connector, implement the OAuth token refresh." Does SECURITY fire?

**B:** Yes — twice, from two rules:
1. "OAuth" → AUTHORIZATION (Rule 7)
2. "token" → SECURITY (Rule 6: "token refresh" contains "token")

Both predicates fire. Result: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

**HubSpot private apps use bearer token authentication.** The access token concept fires SECURITY regardless of whether it is a HubSpot token, OAuth token, or Stripe API key. The word "token" is the trigger.

---

## B17-TU-016-004 — TestingFlow51 dependency on IntegrationConnectorFlow43 (HubSpot context)

**A:** "Develop the HubSpot connector, write unit tests for each endpoint handler." Why does TestingFlow51 run after IntegrationConnectorFlow43 and not in parallel?

**B:** TestingFlow51 (sub-phase 5.1) has a hard dependency on IntegrationConnectorFlow43 (sub-phase 4.3). Unit tests for the HubSpot connector endpoint handlers require the connector code to exist before tests can be written and executed. This is a code dependency: tests test code; the code must be generated first.

DocumentationFlow21 (2.1) does NOT have this dependency — documentation can be written from the interface specification (which is known at design time, before code generation). Therefore, documentation runs in parallel with code generation, but testing runs after code generation.
