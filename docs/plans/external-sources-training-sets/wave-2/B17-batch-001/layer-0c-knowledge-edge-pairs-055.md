# Layer 0-C — Knowledge-Edge Pairs (055)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Cloudflare connector

---

## B17-0C-055-001 — Cloudflare: non-trigger terms

**Proper nouns — no predicate:** Cloudflare
**Domain nouns — no predicate:** zone, record, worker, page, rule, firewall, WAF (Web Application Firewall), DNS (Domain Name System), cache, tunnel, edge
**Acronyms — no predicate:** WAF (Web Application Firewall), DNS (Domain Name System), CDN (Content Delivery Network)

---

## B17-0C-055-002 — Cloudflare: "DNS" and "WAF" are not integration vocabulary

"Cloudflare DNS zone management" — DNS (Domain Name System) is a domain acronym. It does not supply integration vocabulary.
"Cloudflare WAF rules connector" — WAF (Web Application Firewall) is a domain acronym; "connector" fires API_CONTRACT.

---

## B17-0C-055-003 — Cloudflare: API token scoped to DNS fires SECURITY

"Implement the Cloudflare connector with an API token scoped to DNS zone management" → SECURITY fires (compound "API token" contains "token").
"Implement the Cloudflare connector with an API key" → SECURITY does NOT fire (no "token").
Teaching: Same "API key vs API token" distinction as all other connectors.

---

## B17-0C-055-004 — Cloudflare: Workers KV is not additional API_CONTRACT

"Build the Cloudflare Workers connector with KV (Key-Value) storage access" — KV (Key-Value) is a domain acronym. "connector" fires API_CONTRACT once. No additional API_CONTRACT from "storage access" or "KV". Predicate set: ["IMPLEMENT", "API_CONTRACT"].
