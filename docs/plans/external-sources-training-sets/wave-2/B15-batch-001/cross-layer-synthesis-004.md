# Cross-Layer Synthesis (004)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

## CLS-004 — Seven-predicate: Full B15 pipeline

Request: "Design the checkout service architecture, define the payment API data contracts, build the checkout service, secure the payment token handling, write E2E tests for the checkout flow, document the checkout API, and QA before release to staging."

### Layer 0-A (Compiler)
design architecture → ARCHITECTURE | data contracts → API_CONTRACT | build → IMPLEMENT | secure → SECURITY | write E2E tests → TEST | document → DOCUMENT | QA → QA
Predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"] — all seven

### Layer 0-B (Router)
All seven flows selected. TEST → 5.3 (E2E + checkout flow). QA → 7.1 ("before release to staging").

### Layer 0-C (Knowledge Edge)
"data contracts" fires API_CONTRACT (contract keyword). "design architecture" fires ARCHITECTURE.
"secure payment token handling" fires SECURITY. All seven verbs fire independently.

### Layer 0-D (Flow Factory)
Seven factories. TransitionType: feature_addition (IMPLEMENT present).

### Layer 1 (Transition Orchestrator)
ArchitectureFlow31 → APIContractFlow33 → ImplementationFlow41 → SecurityAuditFlow → E2ETestingFlow53 ‖ DocumentationFlow → ReleaseGateFlow71
QA absolute last.

### Layer 2 (Flow Execution)
All seven flows approved. Full pipeline executed.
ReleaseGateFlow71: Staging release cleared.

### Layer 3 (Teaching)
"data contracts" catches API_CONTRACT (v6-02 common error). ARCHITECTURE first — before IMPLEMENT. QA = 7.1 because "release to staging" vocabulary present. All seven predicates fire; no collapse.

TransitionType: feature_addition | QA (7.1) last | All seven flows complete.
