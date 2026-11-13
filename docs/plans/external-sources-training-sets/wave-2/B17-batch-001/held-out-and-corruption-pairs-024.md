# Held-Out and Corruption Pairs (024)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption | focus: Complete held-out and corruption reference for connectors 013-024

---

## B17-HO-024-001 — Complete held-out test cases summary (connectors 013-024)

| File | Input pattern | Expected predicates | Gate B |
|---|---|---|---|
| 013 | "integrate Stripe API" | [API_CONTRACT] | IMPLEMENT absent |
| 013 | "refactor Stripe connector" | [IMPL, API] + refactoring | TransitionType=refactoring |
| 014 | "integrate Mailgun" | [API_CONTRACT] | IMPLEMENT absent |
| 015 | "Implement FedEx rate service" (no connector) | [IMPLEMENT] → 4.1 | 4.1 not 4.3 |
| 015 | "integrate FedEx API" | [API_CONTRACT] | IMPLEMENT absent |
| 016 | "integrate HubSpot CRM" | [API_CONTRACT] | IMPLEMENT absent |
| 017 | "implement analytics data pipeline" | [IMPLEMENT] → 4.1 | 4.1 not 4.3 |
| 018 | "integrate Zuora billing API" | [API_CONTRACT] | IMPLEMENT absent |
| 019 | "build Okta OIDC connector" | [IMPL, API] | AUTHORIZATION absent (OIDC≠OAuth) |
| 020 | "integrate Intercom API" | [API_CONTRACT] | IMPLEMENT absent |
| 021 | "quality assurance" written out | [IMPL, API] | QA absent |
| 022 | "Avalara with Basic auth" (no "token") | [IMPL, API] | SECURITY absent |
| 023 | "integrate multi-connector" | [API_CONTRACT] | IMPLEMENT absent |

---

## B17-HO-024-002 — Complete corruption catalogue for connectors 013-024

| File | v6 code | Corruption | Correct |
|---|---|---|---|
| 013 | v6-01 | "integrate Stripe" → IMPLEMENT invented | API_CONTRACT only |
| 013 | v6-02 | "idempotency token" → SECURITY suppressed | SECURITY fires on "token" |
| 014 | v6-01 | "build email sender" → API_CONTRACT invented | No API_CONTRACT (no trigger) |
| 015 | v6-03 | "FedEx" proper noun → 4.3 routing | FedEx is transparent; connector token → 4.3 |
| 016 | v6-03 | "CRM" → integration vocabulary attribution | "connector" is the source of 4.3 routing |
| 017 | v6-01 | "orchestrate" → IMPLEMENT invented | Not an IMPLEMENT trigger |
| 017 | v6-14 | "refactor" → feature_addition TransitionType | TransitionType must be refactoring |
| 018 | v6-01 | "OAuth client credentials" → SECURITY invented | SECURITY requires "token" or "authenticate" |
| 019 | v6-01 | "OIDC" → AUTHORIZATION invented | Only "OAuth" fires AUTHORIZATION |
| 019 | v6-01 | "authorization" English word → AUTHORIZATION | English word ≠ trigger; only "OAuth" fires |
| 020 | v6-02 | "bearer token" → SECURITY suppressed | Rule 6 is unconditional |
| 021 | v6-01 | "quality assurance" → QA invented | Rule 21 strict: "QA" abbreviation only |
| 022 | v6-02 | "Basic auth" → SECURITY invented | "Basic auth" is not a SECURITY trigger |
| 023 | v6-01 | "integrate five connectors" → IMPLEMENT | Rule 1 unconditional; no scale exception |

---

## B17-HO-024-003 — Top 5 errors for connectors 013-024

1. **"integrate" → IMPLEMENT (v6-01)** — most common; applies to ALL 10 new connectors
2. **"OIDC"/"OpenID Connect" → AUTHORIZATION (v6-01)** — Okta-specific but high risk
3. **"token" suppressed (v6-02)** — all connectors that use bearer tokens or access tokens
4. **Proper noun → integration vocabulary (v6-03 attribution)** — FedEx, CRM, POS, etc.
5. **"quality assurance" → QA (v6-01, Rule 21)** — Square POS QA scenario

---

## B17-HO-024-004 — Correct behaviors reference (013-024 combined)

**Always true:**
- "integrate" alone → ["API_CONTRACT"] → 3.3 → documentation (10/10 connectors)
- "OAuth" present → AUTHORIZATION fires (6/10 connectors where applicable)
- "token" present → SECURITY fires (all connectors using token auth)
- "connector" present + build verb → IMPLEMENT → 4.3 (all 10 connectors)
- "QA" abbreviation → QA last (1 connector demonstrated: Square)
- "OIDC" alone → no AUTHORIZATION (Okta OIDC-only case)
- Multi-connector "integrate" → ["API_CONTRACT"] (1 decision, not multiplied by connector count)
