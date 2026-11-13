# Layer 0-A — Compiler Pairs (024)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: compiler | focus: Complete predicate rule reference

---

## B17-0A-024-001 — Complete predicate trigger table for B17 (extended, connectors 013-024)

| Vocabulary trigger | Predicate fired | Rule | Notes |
|---|---|---|---|
| build, implement, write, create, code, develop | IMPLEMENT | 11 | Build verbs |
| refactor, restructure, rewrite | IMPLEMENT | 14 | Also sets TransitionType: refactoring |
| integrate | API_CONTRACT | 1 | NEVER IMPLEMENT — top rule |
| connector | API_CONTRACT | 4 | Any position, any compound |
| webhook | API_CONTRACT | 5 | Any form |
| wire up | API_CONTRACT | 22 | Like "integrate", never IMPLEMENT |
| connect + external service | API_CONTRACT | 22 | |
| API, APIs, endpoint (external) | API_CONTRACT | — | Reinforcers |
| token | SECURITY | 6 | Unconditional |
| authentication, authenticate | SECURITY | 8 | Not AUTHORIZATION |
| validate signature, secure, HMAC validation | SECURITY | 8 | |
| OAuth (any case) | AUTHORIZATION | 7 | Not SECURITY; only the word "OAuth" |
| document, spec | DOCUMENT | 12 | |
| write tests, create tests, add tests | TEST | 13 | |
| QA (abbreviation only) | QA | 21 | "quality assurance" written out does NOT fire QA |

---

## B17-0A-024-002 — IMPLEMENT sub-phase routing table (complete)

| IMPLEMENT context vocabulary | Sub-phase | Rule |
|---|---|---|
| connector, integration, webhook, API, external service, third-party, gateway, SDK, endpoint (external) | 4.3 | 2/3 |
| service, logic, class, function, method (no integration vocabulary) | 4.1 | 2 |
| frontend, UI, component, page, view | 4.2 | 2 |
| migration, migrate, schema (data) | 4.4 | 2 |
| config, flag, feature flag, environment | 4.5 | 2 |

**Rule:** Integration vocabulary always wins over service vocabulary when both are present.
**Proper nouns (Stripe, FedEx, Okta, etc.) do NOT count as integration vocabulary** — only the structural terms above.

---

## B17-0A-024-003 — Predicate non-triggers for connectors 013-024

**These terms do NOT fire any predicate:**
- Stripe, Mailgun, FedEx, UPS, HubSpot, Segment, Zuora, Okta, Intercom, Square, Avalara (proper nouns — no predicate)
- payment, email, shipping, billing, analytics, tax, messaging, POS (domain nouns — no predicate)
- OIDC, SAML, REST, CRM, ERP, POS (acronyms — no predicate unless the expanded term matches a trigger)
- orchestrate, coordinate, bridge, sync (operation verbs — not in IMPLEMENT lexicon for this corpus)
- add (not in standard IMPLEMENT trigger set for this corpus; "implement" and "build" are triggers)

---

## B17-0A-024-004 — Canonical predicate ordering (Rule 20) with sub-phase map

**Fixed canonical sequence:**
1. IMPLEMENT → sub-phase 4.3 (with integration vocabulary) or 4.1 (without)
2. API_CONTRACT → sub-phase 3.3
3. SECURITY → sub-phase 3.4
4. AUTHORIZATION → sub-phase 3.4
5. TEST → sub-phase 5.1
6. DOCUMENT → sub-phase 2.1
7. QA → sub-phase 7.x (always last — Rule 21)

**DecisionCount** = total predicates fired (not unique sub-phases).
**UniqueSubPhases** = count of distinct sub-phase destinations.

**Example:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"] → DecisionCount: 4, UniqueSubPhases: 3 (4.3, 3.3, 3.4 — SECURITY and AUTHORIZATION share 3.4).

---

## B17-0A-024-005 — v6 corruption quick reference for connectors 013-024

| v6 code | Symptom | Example trigger | Correct behavior |
|---|---|---|---|
| v6-01 | "integrate" → IMPLEMENT invented | "Integrate the Stripe API" | API_CONTRACT only; IMPLEMENT absent |
| v6-01 | "OIDC" → AUTHORIZATION invented | "Create the Okta OIDC connector" | No AUTHORIZATION; OIDC is not "OAuth" |
| v6-02 | "token" suppressed; SECURITY missing | "Build connector with token auth" | SECURITY must fire on "token" |
| v6-02 | "OAuth" suppressed; AUTHORIZATION missing | "Build OAuth connector" | AUTHORIZATION must fire on "OAuth" |
| v6-03 | IMPLEMENT → 4.1 despite connector vocabulary | "Build the Stripe connector" | Integration vocabulary → 4.3, not 4.1 |
| v6-03 | "integrate" alone → 4.3 (wrong) | "Integrate the Mailgun API" | API_CONTRACT only → 3.3 (not 4.3) |
