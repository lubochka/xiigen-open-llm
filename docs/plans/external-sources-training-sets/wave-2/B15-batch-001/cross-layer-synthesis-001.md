# Cross-Layer Synthesis (001)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

## CLS-001 — IMPLEMENT+API_CONTRACT: Build order service + integrate payment API

Request: "Build the order processing service and integrate with the Stripe payment gateway."

### Layer 0-A (Compiler)
build → IMPLEMENT | integrate → API_CONTRACT
Predicates: ["IMPLEMENT", "API_CONTRACT"]

### Layer 0-B (Router)
IMPLEMENT → ImplementationFlow41 | API_CONTRACT → APIContractFlow33
Ordering: APIContractFlow33 before ImplementationFlow41

### Layer 0-C (Knowledge Edge)
"integrate" always fires API_CONTRACT. "build" fires IMPLEMENT. Neither suppresses the other.

### Layer 0-D (Flow Factory)
Two flows selected. TransitionType: feature_addition.

### Layer 1 (Transition Orchestrator)
APIContractFlow33 → ImplementationFlow41 (contract spec informs implementation)

### Layer 2 (Flow Execution)
APIContractFlow33: Stripe API contract spec — endpoints, auth, webhook signature. APPROVED.
ImplementationFlow41: OrderProcessingService + StripePaymentClient (via IPaymentGatewayClient). APPROVED.

### Layer 3 (Teaching)
"integrate" always fires API_CONTRACT even when "build" is also present. Two predicates → two flows.

TransitionType: feature_addition | Gap: None (predicate-firing batch)
