# Layer 0-C — Knowledge-Edge Pairs (030)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Algolia search connector edge cases

---

## B17-0C-030-001 — Algolia: proper noun transparency

"Algolia" is a proper noun — transparent to routing. It does not supply integration vocabulary.
- "Implement Algolia" → no integration vocabulary → IMPLEMENT → 4.1
- "Build Algolia connector" → "connector" is integration vocabulary → IMPLEMENT → 4.3
- "Integrate Algolia" → API_CONTRACT only (Rule 1) → 3.3

This is identical to how "Stripe", "FedEx", "Okta" etc. behave. Proper nouns are always transparent.

---

## B17-0C-030-002 — Algolia: "search" and "index" are domain nouns

"search", "index", "faceted", "catalogue", "product lookup" — domain nouns for Algolia.
None fire any predicate.
Integration vocabulary that would route Algolia IMPLEMENT to 4.3: connector, webhook, API, endpoint, SDK, gateway, external service.

---

## B17-0C-030-003 — Algolia: "admin API token" — all three triggers

Input: "Build Algolia connector using the admin API token for index writes."
- "Build" → IMPLEMENT
- "connector" → API_CONTRACT (integration vocabulary → 4.3)
- "API" → API_CONTRACT (reinforces; fires once)
- "token" → SECURITY (Rule 6; unconditional)

Three distinct trigger types (build verb, structural vocabulary, security vocabulary) → 3 predicates.
DecisionCount: 3 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)

---

## B17-0C-030-004 — Algolia: "catalogue" vs "connector" routing disambiguation

| Input | Route | Reason |
|---|---|---|
| "Build Algolia catalogue connector" | IMPLEMENT → 4.3 | "connector" present |
| "Build Algolia catalogue service" | IMPLEMENT → 4.1 | no integration vocabulary |
| "Build Algolia catalogue API client" | IMPLEMENT → 4.3 | "API" in external context |
