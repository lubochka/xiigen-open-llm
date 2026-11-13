# Layer 3 — Teaching Unit Pairs (030)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: teaching-unit | focus: Algolia connector

---

## B17-TU-030-001 — Algolia: admin API key vs admin API token

**A:** "Build the Algolia connector using the admin API key for index management." Does SECURITY fire?

**B:** No. "Admin API key" does not contain "token" → SECURITY does NOT fire. CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]. DecisionCount: 2.

---

## B17-TU-030-002 — Algolia: admin API token fires SECURITY

**A:** "Build the Algolia connector with admin API token for secured index operations." Does SECURITY fire?

**B:** Yes. "Admin API token" contains "token" → SECURITY fires (Rule 6). CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]. DecisionCount: 3.

---

## B17-TU-030-003 — Algolia: "search" is not IMPLEMENT

**A:** "Search the Algolia index for matching products." Does IMPLEMENT fire?

**B:** No. "Search" is not an IMPLEMENT trigger. CompiledPredicateIds: [].

---

## B17-TU-030-004 — Algolia: "faceted search" domain term

**A:** Does "faceted search" supply integration vocabulary to route IMPLEMENT to 4.3?

**B:** No. "Faceted search" is a domain noun. Integration vocabulary is supplied by: connector, webhook, API (in context), external service, SDK. "Faceted search" is a search feature concept and fires no predicate. IMPLEMENT routes to 4.3 only when integration vocabulary is present.
