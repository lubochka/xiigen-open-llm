# Layer 0-C — Knowledge-Edge Pairs (034)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Salesforce CRM connector edge cases

---

## B17-0C-034-001 — Salesforce: "CRM" acronym — no predicate

"CRM" (Customer Relationship Management) is a product category acronym.
- "CRM" does NOT fire any predicate
- "Build Salesforce CRM connector" → IMPLEMENT → 4.3 because of "connector", not "CRM"
- "Build Salesforce CRM service" → IMPLEMENT → 4.1 because "CRM" provides no integration vocabulary

Identical pattern applies to: ERP (Enterprise Resource Planning), POS (Point of Sale), SaaS (Software as a Service).

---

## B17-0C-034-002 — Salesforce: "lead", "opportunity", "contact" are domain nouns

Salesforce-specific domain nouns: lead, opportunity, contact, account, deal, campaign, pipeline.
None fire any predicate. The compiler is transparent to Salesforce-specific data model vocabulary.

---

## B17-0C-034-003 — Salesforce: "connected app" — not integration vocabulary

"OAuth 2.0 connected app" — "connected app" is a Salesforce product concept.
- "connected" does NOT fire any predicate
- "app" does NOT fire any predicate
- "OAuth" fires AUTHORIZATION
- "token" (if present) fires SECURITY

"Connected app" is domain vocabulary. "OAuth" is the predicate trigger.

---

## B17-0C-034-004 — Salesforce: "lead scoring algorithm" — IMPLEMENT to 4.1

Input: "Implement Salesforce lead scoring algorithm."
- "Implement" → IMPLEMENT
- No integration vocabulary (connector, webhook, API, endpoint, SDK, gateway, external service)
- "Salesforce" proper noun: no integration vocabulary
- "lead scoring algorithm": no integration vocabulary

IMPLEMENT → 4.1 (BackendServiceFlow41)
This is v6-03 prevention: never route to 4.3 because a proper noun is present.
