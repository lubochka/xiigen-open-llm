# Layer 0-C — Knowledge-Edge Pairs (002)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: knowledge-edge

---

## B17-0C-002-001 — 4.3 vs 4.1 disambiguation: integration vocabulary vs service vocabulary

**The core disambiguation rule:**
When IMPLEMENT fires, the sub-phase destination (4.3 vs 4.1) is determined by vocabulary co-occurrence in the same input.

**4.3 vocabulary (integration context):**
connector, integration, webhook, API, external service, third-party, gateway, SDK, endpoint (external)

**4.1 vocabulary (service/logic context):**
service, logic, class, function, method, handler (internal), repository, domain, entity

---

## B17-0C-002-002 — Disambiguation examples: 4.3 vs 4.1

| Input fragment | Integration vocabulary present | Sub-phase for IMPLEMENT |
|---|---|---|
| "Implement the StripeConnector" | connector | 4.3 |
| "Implement the PaymentService" | none (service alone) | 4.1 |
| "Implement the webhook listener" | webhook | 4.3 |
| "Implement the discount logic" | none (logic) | 4.1 |
| "Implement the external API client" | external, API | 4.3 |
| "Implement the UserRepository" | none (repository) | 4.1 |
| "Implement the Plaid SDK wrapper" | SDK | 4.3 |
| "Implement the EventHandler" | none (internal handler) | 4.1 |

---

## B17-0C-002-003 — Conflict resolution: integration vocabulary wins over service vocabulary

**Input:** "Implement the PaymentConnectorService class and its integration logic."

**Vocabulary present:**
- Integration: "connector", "integration" → 4.3 signal
- Service/logic: "service", "class", "logic" → 4.1 signal

**Resolution:** Integration vocabulary wins. IMPLEMENT routes to 4.3.

**Rule:** When both integration and service vocabulary are present, integration vocabulary takes precedence and routes IMPLEMENT to 4.3.

---

## B17-0C-002-004 — Exact token match required for integration vocabulary

**"connector" in class name counts:** "StripeConnectorService" contains "connector" → 4.3
**"integration" as part of compound counts:** "integration layer", "integration module" → 4.3
**"API" as standalone word counts:** "the payment API" → 4.3
**"api" lowercase also counts:** case-insensitive match

**Does NOT count:**
- "internal" (contains "int" but is not "integration")
- "connector" misspelled as "connecter" (exact token match)
- "web hook" (two words — "webhook" single token is the trigger, though context is strong)
