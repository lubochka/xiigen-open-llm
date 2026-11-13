# Layer 3 — Teaching Unit Pairs (008)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: teaching-unit

---

## B17-TU-008-001 — Phase 4 sub-phase routing vocabulary map (complete 4.1/4.2/4.3/4.4/4.5)

**Phase 4:** Implementation and Development
**Sub-phases:**

| Sub-phase | Name | Primary vocabulary signals |
|---|---|---|
| 4.1 | Backend Services and Logic | service, class, logic, function, method, repository, domain, entity |
| 4.2 | Frontend and UI | frontend, UI, component, page, view, screen, CSS, template, React, Vue |
| 4.3 | Integration and External Services | connector, integration, webhook, API, external, third-party, gateway, SDK |
| 4.4 | Data Migration | migration, migrate, schema (in migration context), ETL, transform |
| 4.5 | Configuration and Feature Flags | config, flag, feature flag, environment variable, toggle, settings |

---

## B17-TU-008-002 — B17 uses 4.3 exclusively (for IMPLEMENT)

**B17 scenario** (Development — integration — third-party payment connector) inherently involves integration vocabulary. Nearly all B17 work items that include IMPLEMENT will route to 4.3.

**Conditions where B17 would route IMPLEMENT to 4.1 (rare but possible):**
- "Implement the PaymentCalculatorService that computes totals." → no integration vocabulary → 4.1
- "Implement the discount logic class." → no integration vocabulary → 4.1

**These are B17-adjacent but not B17-core scenarios.** Core B17 scenarios involve connector/API vocabulary.

---

## B17-TU-008-003 — Phase 4 sub-phase vocabulary conflict table

| Input contains | IMPLEMENT routes to |
|---|---|
| connector | 4.3 (beats all) |
| webhook | 4.3 (beats all) |
| API | 4.3 (beats 4.1, 4.2) |
| migration + connector | 4.3 (connector beats migration) |
| frontend + connector | 4.3 (connector beats frontend) |
| config + connector | 4.3 (connector beats config) |
| migration (no connector/API/webhook) | 4.4 |
| frontend/UI (no connector/API/webhook) | 4.2 |
| config/flag (no connector/API/webhook) | 4.5 |
| service/logic (no other signals) | 4.1 (default) |

---

## B17-TU-008-004 — Why 4.3 has highest precedence among Phase 4 sub-phases

**Rationale:** Integration work crosses organizational and system boundaries. It is the most complex and highest-risk work in Phase 4. Third-party dependencies introduce unpredictable failure modes, rate limits, and contract changes. By routing integration work to a dedicated sub-phase (4.3) with its own specialized flow (IntegrationConnectorFlow43), the orchestrator can apply integration-specific tooling and oversight.

**The precedence rule** (4.3 > all other Phase 4 sub-phases when integration vocabulary is present) reflects this organizational priority.
