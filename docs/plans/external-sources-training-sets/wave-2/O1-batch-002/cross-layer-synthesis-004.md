# Cross-Layer Synthesis (004)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## Scenario: Full eight-predicate trace
## Prompt: "Architect the auth service, build OAuth2 client credentials, integrate the tenant API, secure with JWT, add scope management, write isolation tests, document the auth flows, and QA the security gates."

---

### Layer 0-A: Compiler — all 8 predicates

A: "Architect the auth service, build OAuth2 client credentials, integrate the tenant API, secure with JWT, add scope management, write isolation tests, document the auth flows, and QA the security gates."

B: Predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "AUTHORIZATION", "TEST", "DOCUMENT", "QA"] | TransitionType: feature_addition
   Reasoning by term:
   - "Architect the auth service" → ARCHITECTURE (architecture) + SECURITY (auth)
   - "build OAuth2 client credentials" → IMPLEMENT (build) + SECURITY (oauth, credential)
   - "integrate the tenant API" → API_CONTRACT (api) + AUTHORIZATION (tenant)
   - "secure with JWT" → SECURITY (jwt, secure/security)
   - "add scope management" → AUTHORIZATION (scope) + IMPLEMENT (add)
   - "write isolation tests" → TEST (test) + IMPLEMENT (write)
   - "document the auth flows" → DOCUMENT (document) + SECURITY (auth)
   - "QA the security gates" → QA (qa) + SECURITY (security)
   IMPLEMENT present, no "refactor" → feature_addition. Canonical order: ARCHITECTURE → API_CONTRACT → IMPLEMENT → SECURITY → AUTHORIZATION → TEST ‖ DOCUMENT → QA

---

### Layer 0-B: Router — 8 predicates → 6 routing decisions

A: Predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "AUTHORIZATION", "TEST", "DOCUMENT", "QA"]

B: RoutingDecisions:
   [{ sub_phase: "architecture", reason: "ARCHITECTURE" },
    { sub_phase: "3.3", reason: "API_CONTRACT", state: "deterministic_generic" },
    { sub_phase: "4.1", reason: "IMPLEMENT", state: "deterministic_generic" },
    { sub_phase: "3.4", reason: "SECURITY+AUTHORIZATION dedup", state: "trusted_learned" },
    { sub_phase: "5.1", reason: "TEST", state: "probationary_rejected" },
    { sub_phase: "2.1", reason: "DOCUMENT", state: "deterministic_generic" }]
   QA: no routing decision (QA does not add sub-phase). SECURITY+AUTHORIZATION dedup → one 3.4.
   Total: 6 routing decisions from 8 predicates.

---

### Layer 0-D: Factory — 6 flows selected + QA gate

A: RoutingDecisions: [architecture, 3.3, 4.1, 3.4, 5.1, 2.1]

B: FlowSelections:
   [{ sub_phase: "3.4", adapter: "SecurityVerification34FlowAdapter", state: "trusted_learned" },
    { sub_phase: "3.3", adapter: "ApiContract33FlowAdapter", state: "deterministic_generic" },
    { sub_phase: "4.1", adapter: "UniversalNineNodeDevelopmentFlow", state: "deterministic_generic" },
    { sub_phase: "5.1", adapter: "UnitTestingFlow51", state: "probationary_rejected", fallback: true },
    { sub_phase: "2.1", adapter: "DocumentationFlow21", state: "deterministic_generic" },
    { sub_phase: "architecture", adapter: "ArchitectureFlow", state: "deterministic_generic" }]
   QA: QAGatingFlow73 appended as absolute last step (not in factory selection — implicit).

---

### Layer 1: Orchestration — full fan-out

A: SelectedLogicalTransition: { all 8 predicates, routing: 6 decisions, transition_type: "feature_addition" }

B: OrchestrationPlan:
   Phase 1 (parallel): SecurityVerification34Flow (3.4) ‖ ApiContractFlow33 (3.3) ‖ ArchitectureFlow ‖ DocumentationFlow21 (2.1)
   Phase 2 (after Phase 1): UniversalNineNodeDevelopmentFlow (4.1) — uses 3.4 security output
   Phase 3 (after Phase 2): GenericFallbackFlow (5.1 fallback) — uses 3.4 test requirements
   Phase 4 (absolute last): QAGatingFlow73 — reviews all assembled results
   Total: 6 domain flows + 1 QA gate

---

### Layer 2: Nine-Node (3.4 primary flow — summarized)

A: SelectedLogicalTransition → SecurityVerification34Flow (trusted_learned)

B: DomainSplitterUnit → 5 domain packets (SECURITY+AUTHORIZATION+IMPLEMENT+TEST+DOCUMENT).
   SchemePlannerUnit → 6 work branches covering all domains.
   ProcessedBranchResults → JWT policy, scope model, token service, test scaffold, documentation spec.
   ArbiterUnit → all verdicts approved.
   FormatterUnit → "Complete OAuth2 tenant auth system: architecture defined, client credentials built, JWT secured, tenant scopes managed, isolation tests scaffolded (fallback), auth flows documented. QA gate passed."

---

### Cross-Layer Summary

8 predicates → canonical ordering applied → 6 routing decisions (SECURITY+AUTHORIZATION dedup, QA no routing) → 6 factory flows + QA gate → parallel + sequential orchestration → full nine-node pipeline in 3.4 (primary) + 5 supporting flows. TransitionType: feature_addition. This is the maximum complexity scenario for O1b — all eight predicates active, all sub-phases routing, maximum fan-out.
