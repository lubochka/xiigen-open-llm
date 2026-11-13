# Layer 0-C — Knowledge-Edge Pairs (007)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: knowledge-edge

---

## B17-0C-007-001 — Phase 4 sub-phase vocabulary map

**Phase 4** (Implementation and Development) has five sub-phases. IMPLEMENT routing selects among them based on vocabulary co-occurrence.

| Sub-phase | Name | Vocabulary signals |
|---|---|---|
| 4.1 | Backend Services and Logic | service, logic, class, function, method, repository, domain, entity, handler (internal), business rule |
| 4.2 | Frontend and UI | frontend, UI, component, page, view, screen, layout, style, CSS, HTML, React, Vue, template |
| 4.3 | Integration and External Services | connector, integration, webhook, API, external service, third-party, gateway, SDK, endpoint (external) |
| 4.4 | Data Migration | migration, migrate, schema, database migration, data transform, ETL (Extract, Transform, Load) |
| 4.5 | Configuration and Feature Flags | config, configuration, flag, feature flag, environment variable, settings, toggle |

---

## B17-0C-007-002 — Sub-phase selection precedence for IMPLEMENT

**When vocabulary from multiple sub-phases is present:**

**Precedence (highest to lowest):**
1. 4.3 (integration) — wins over all others
2. 4.4 (migration) — wins over 4.1, 4.2, 4.5
3. 4.2 (frontend) — wins over 4.1, 4.5
4. 4.5 (config) — wins over 4.1
5. 4.1 (backend) — default when no other signals present

**Example:** "Build the API gateway configuration service" → API (4.3 signal) wins → 4.3

---

## B17-0C-007-003 — B17 exclusively activates 4.3 for IMPLEMENT

**B17 scenario:** Development — integration — third-party payment connector

In B17, IMPLEMENT always routes to 4.3 because the scenario inherently involves connector/integration/API vocabulary. B17 does not exercise 4.1, 4.2, 4.4, or 4.5 routing for IMPLEMENT.

**Exception:** A B17 input that mentions a service or logic without any integration vocabulary would route IMPLEMENT to 4.1. This is tested in negative pairs to ensure the distinction is maintained.

---

## B17-0C-007-004 — Sub-phase vocabulary conflict resolution examples

| Input | Dominant vocabulary | IMPLEMENT sub-phase |
|---|---|---|
| "Build the payment API service" | API (4.3) beats service (4.1) | 4.3 |
| "Build the migration connector" | connector (4.3) beats migration (4.4) | 4.3 |
| "Build the connector UI component" | connector (4.3) beats component (4.2) | 4.3 |
| "Build the PaymentService" | service (4.1), no integration | 4.1 |
| "Build the migration schema" | migration (4.4), no integration | 4.4 |
| "Build the checkout UI page" | UI/page (4.2), no integration | 4.2 |
| "Build the feature flag config" | config (4.5), no integration | 4.5 |
