# Cross-Layer Synthesis (001)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: cross-layer-synthesis

---

## B17-CLS-001 — IMPLEMENT + API_CONTRACT: Build Stripe connector (canonical 4.3 path)

**Input:**
"Build the Stripe payment connector and wire up the charge, refund, and webhook endpoints."

---

### Layer 0-A (Compiler)
```
Triggers detected:
- "Build" → IMPLEMENT
- "connector" → API_CONTRACT
- "wire up" → API_CONTRACT (reinforces; fires once)
- "webhook" → API_CONTRACT (reinforces)
- Integration vocabulary: connector, wire up, webhook → 4.3 context

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition
DomainId: software-development
```

---

### Layer 0-B (Router)
```
Routing decisions:
1. IMPLEMENT → integration vocabulary present → 4.3 (IntegrationConnectorFlow43)
2. API_CONTRACT → 3.3 (APIContractFlow33)

DecisionCount: 2
UniqueSubPhases: 2 (4.3, 3.3)
```

---

### Layer 0-C (Knowledge Edge)
```
Edge assertions verified:
- "connector" → API_CONTRACT: ✓ (Rule 4)
- "wire up" → API_CONTRACT: ✓ (Rule 22)
- IMPLEMENT + connector → 4.3: ✓ (Rule 2/3)
- API_CONTRACT alone → 3.3: ✓ (Rule 10)
- No GapHandler: ✓ (Rule 19)
```

---

### Layer 0-D (Flow Factory)
```
Factory selection:
- IMPLEMENT → 4.3 → IntegrationConnectorFlow43 [probationary_accepted]
- API_CONTRACT → 3.3 → APIContractFlow33 [deterministic_generic]

SelectedFlows: [IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3)]
TransitionType: feature_addition
```

---

### Layer 1 (Transition Orchestrator)
```
Orchestration plan:
Step 1: IntegrationConnectorFlow43 (4.3) — build StripeConnector
Step 2: APIContractFlow33 (3.3) — document payment API contract

No TEST → no sequential dependency
No QA → no final gate
```

---

### Layer 2 (Nine-Node 4.3 — StripeConnector)
```
N01: ["IMPLEMENT", "API_CONTRACT"] — charge, refund, webhook
N02: Stripe REST API v3 — 3 handlers: ChargePayment, RefundCharge, StripeWebhookHandler
N03: IStripeConnector: Charge, Refund, HandleWebhook
N06: POST /v1/payment_intents, POST /v1/refunds, HMAC signature validation
N08: PASS — all handlers implemented
N09: StripeConnector complete, 7 files generated
```

---

### Layer 3 (Teaching)
```
Key lessons from this trace:
1. "wire up" and "webhook" reinforce API_CONTRACT; predicate fires once regardless
2. "connector" alone is sufficient for both API_CONTRACT and 4.3 routing
3. Canonical 4.3 path: IMPLEMENT(4.3) + API_CONTRACT(3.3), feature_addition, DecisionCount 2
4. This is the minimal B17 pipeline — two predicates, two flows, two steps
```

---

**Full trace summary:**
Compiler → ["IMPLEMENT", "API_CONTRACT"] → Router → [4.3, 3.3] → Factory → [IntegrationConnectorFlow43, APIContractFlow33] → Orchestrator → Step1: 4.3, Step2: 3.3 → Nine-node: N08 PASS → Teaching: canonical minimal B17 path.
