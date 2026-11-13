# Cross-Layer Synthesis (005)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: cross-layer-synthesis

---

## B17-CLS-005 — IMPLEMENT + API_CONTRACT: Build Stripe connector (new domain, canonical 4.3 path)

**Input:**
"Build the Stripe payment connector and implement the charge, capture, and dispute webhook endpoints."

---

### Layer 0-A (Compiler)
```
Triggers detected:
- "Build" → IMPLEMENT
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT (Rule 4)
- "webhook" → API_CONTRACT (Rule 5; reinforces)
- "endpoints" → API_CONTRACT (reinforces)
- Integration vocabulary: connector, webhook, endpoints → 4.3 context

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
- "webhook" → API_CONTRACT: ✓ (Rule 5; reinforces; fires once)
- IMPLEMENT + connector → 4.3: ✓ (integration vocabulary override)
- API_CONTRACT alone → 3.3: ✓ (Rule 10)
- No GapHandler: ✓ (Rule 19)
```

---

### Layer 0-D (Flow Factory)
```
Factory selection:
- IMPLEMENT → 4.3 → IntegrationConnectorFlow43 [deterministic_generic]
- API_CONTRACT → 3.3 → APIContractFlow33 [deterministic_generic]

SelectedFlows: [IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3)]
TransitionType: feature_addition
```

---

### Layer 1 (Transition Orchestrator)
```
Orchestration plan:
Step 1: IntegrationConnectorFlow43 (4.3) — build StripePaymentConnector
Step 2: APIContractFlow33 (3.3) — document Stripe payment API contract

No TEST → no sequential dependency after Step 2
No QA → orchestration ends after Step 2
```

---

### Layer 2 (Nine-Node 4.3 — StripePaymentConnector)
```
N01: ["IMPLEMENT", "API_CONTRACT"] — charge, capture, dispute webhook
N02: Stripe REST API v2024-06-20 — 3 handlers: ChargeHandler, CaptureHandler, DisputeWebhookHandler
N03: IStripePaymentConnector: ChargeAsync, CaptureAsync, HandleDisputeWebhook
N04: HttpClient (injected), IStripeWebhookValidator
N05: 5 files (connector, interface, validator, models, config)
N06: POST /v1/payment_intents, capture, HMAC-SHA256 dispute webhook validation
N08: PASS — 3 handlers implemented, PCI guard present, HMAC validation present
N09: StripePaymentConnector complete, 5 files generated
```

---

### Layer 3 (Teaching)
```
Key lessons from this trace:
1. Stripe is a new connector in this batch (013); it uses the same nine-node flow as original Stripe (file .md) but with different handler scope
2. "webhook" and "endpoints" reinforce API_CONTRACT; predicate fires once regardless of multiple triggers
3. Canonical 4.3 path: IMPLEMENT(4.3) + API_CONTRACT(3.3), feature_addition, DecisionCount 2
4. "connector" is the primary integration vocabulary token — it alone is sufficient for 4.3 routing
5. PCI guard is enforced as a domain business rule (N04/N08), not as a predicate
```

---

**Full trace summary:**
Compiler → ["IMPLEMENT", "API_CONTRACT"] → Router → [4.3, 3.3] → Factory → [IntegrationConnectorFlow43, APIContractFlow33] → Orchestrator → Step1: 4.3, Step2: 3.3 → Nine-node: N08 PASS, 5 files → Teaching: canonical 4.3 path applies across all connector domains.
