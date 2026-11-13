# Layer 0-C — Knowledge-Edge Pairs (016)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: HubSpot CRM connector edges

---

## B17-0C-016-001 — Edge: "CRM" acronym does not fire any predicate

**Assertion:** "CRM" (Customer Relationship Management) is an acronym for a product category. It fires no predicate.

**Evidence:**
| Input | CRM fires? | Result |
|---|---|---|
| "Build the HubSpot CRM connector" | No (CRM is transparent) | IMPLEMENT + API_CONTRACT (from connector) |
| "Integrate the CRM" | No | API_CONTRACT only (from integrate) |
| "Build the CRM service" | No | IMPLEMENT (no integration vocabulary; routes to 4.1) |

**Rule:** Product category acronyms (CRM, ERP, POS, etc.) are transparent to the predicate system. They do not fire predicates or contribute to integration vocabulary.

---

## B17-0C-016-002 — Edge: HubSpot private app "token" → SECURITY in CRM context

**Input:** "Build the HubSpot private app connector, implement the OAuth token refresh."

**Edge assertions:**
- "connector" → API_CONTRACT (Rule 4)
- "OAuth" → AUTHORIZATION (Rule 7)
- "token" → SECURITY (Rule 6)
- "Build", "implement" → IMPLEMENT

**Result:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

**CRM-specific note:** HubSpot private apps use bearer token authentication. The token concept fires SECURITY regardless of whether the token is a HubSpot private app access token or a generic OAuth bearer token. The word "token" is the trigger, not the token type.

---

## B17-0C-016-003 — Edge: "authenticate" fires SECURITY in CRM webhook context

**Input:** "Build the HubSpot webhook connector and authenticate all inbound deal stage change events with request signature verification."

**Edge assertions:**
- "Build" → IMPLEMENT
- "connector" → API_CONTRACT
- "webhook" → API_CONTRACT (reinforces)
- "authenticate" → SECURITY (Rule 8: authenticate fires SECURITY)
- "signature verification" → SECURITY (Rule 8: validate signature fires SECURITY; reinforces once)

**Result:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

---

## B17-0C-016-004 — Edge: "endpoint" in CRM context fires API_CONTRACT as reinforcer

**The "endpoint" reinforcer rule:**
"endpoint" is an API_CONTRACT reinforcer. It does not independently route to 4.3 — IMPLEMENT must also be present.

**Contrast:**
| Input | API_CONTRACT fires? | IMPLEMENT fires? | SubPhase for IMPLEMENT |
|---|---|---|---|
| "Build the HubSpot deal endpoint" | Yes (endpoint reinforcer) | Yes (build) | 4.3 (endpoint = integration vocabulary) |
| "Integrate the HubSpot deal endpoint" | Yes (integrate + endpoint reinforces) | No | Routes to 3.3 only |

**Rule 10 confirmed:** The presence of "endpoint" (an API_CONTRACT reinforcer) does not activate 4.3. Only IMPLEMENT activates 4.3. "endpoint" shifts an IMPLEMENT routing from 4.1 to 4.3 by contributing integration vocabulary — but only when IMPLEMENT is already present.
