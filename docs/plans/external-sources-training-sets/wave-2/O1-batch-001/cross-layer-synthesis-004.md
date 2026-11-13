# Cross-Layer Synthesis (004)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05

---

## Scenario: Full five-predicate trace — "Architect the auth system, implement JWT, integrate the tenant API, secure with OAuth, write isolation tests, document the auth flow, and QA the security gates."

---

### Layer 0-A — Compiler

Prompt: "Architect the auth system, implement JWT, integrate the tenant API, secure with OAuth, write isolation tests, document the auth flow, and QA the security gates."

Predicate trace (canonical order): ARCHITECTURE → API_CONTRACT → IMPLEMENT → SECURITY → AUTHORIZATION → TEST ‖ DOCUMENT → QA

Predicates fired: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "AUTHORIZATION", "TEST", "DOCUMENT", "QA"]
TransitionType: feature_addition (IMPLEMENT fires on "implement"; overrides all others)

Lexicon evidence:
  ARCHITECTURE: "architect"
  API_CONTRACT: "api", "integrate" (integration implies API contract context)
  IMPLEMENT: "implement", "integrate"
  SECURITY: "jwt", "oauth", "secure"
  AUTHORIZATION: "tenant"
  TEST: "write isolation tests"
  DOCUMENT: "document"
  QA: "qa"

---

### Layer 0-B — Router

Input predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "AUTHORIZATION", "TEST", "DOCUMENT", "QA"]

Routing decisions: 7 (8 predicates; SECURITY+AUTHORIZATION deduplicated into one 3.4 decision)

  Decision 1: ARCHITECTURE → 3.x (architecture sub-phase)
  Decision 2: API_CONTRACT → 3.3
  Decision 3: IMPLEMENT → 4.1
  Decision 4: SECURITY+AUTHORIZATION → 3.4 (deduplicated: both map to 3.4)
  Decision 5: TEST → 5.1
  Decision 6: DOCUMENT → 2.1
  Decision 7: QA → 7.x (always last)

Sub-phases activated: 3.x, 3.3, 4.1, 3.4, 5.1, 2.1, 7.x (7 sub-phases)

---

### Layer 0-D — Flow Factory

Factory selections (7 factories, canonical order):
  Factory 1: ArchitectureFlow (3.x) | TrustState: deterministic_generic
  Factory 2: ApiContract33FlowAdapter (3.3) | TrustState: deterministic_generic
  Factory 3: ImplementationFlow41 (4.1) | TrustState: deterministic_generic
  Factory 4: SecurityVerification34FlowAdapter (3.4) | TrustState: trusted_learned | EvidencePredicates: ["SECURITY", "AUTHORIZATION"]
  Factory 5: generic nine-node fallback (5.1) | TrustState: probationary_rejected
  Factory 6: RequirementsAuthoringFlow21 (2.1) | TrustState: deterministic_generic
  Factory 7: QAGatingFlow73 (7.x) | TrustState: deterministic_generic

Trust summary: 1 trusted_learned, 1 probationary_rejected (generic fallback), 5 deterministic_generic

---

### Layer 1 — Transition Orchestrator

SelectedLogicalTransition:
  Orchestration: ArchitectureFlow → ApiContract33Flow → ImplementationFlow41 → (SecurityVerification34Flow ‖ generic-5.1 ‖ RequirementsAuthoringFlow21) → QAGatingFlow73
  TransitionType: feature_addition

ExecutionPhases:
  Phase 1 (sequential): ArchitectureFlow (3.x)
  Phase 2 (sequential after Phase 1): ApiContract33Flow (3.3)
  Phase 3 (sequential after Phase 2): ImplementationFlow41 (4.1)
  Phase 4 (parallel after Phase 3): SecurityVerification34Flow (3.4) ‖ generic nine-node (5.1) ‖ RequirementsAuthoringFlow21 (2.1)
  Phase 5 (sequential after Phase 4): QAGatingFlow73 (7.x) — always last

Note: Architecture and API contract precede implementation; security+tests+docs run in parallel after implementation; QA gates everything.

---

### Layer 2 — Nine-Node 3.4 (SecurityVerification34Flow branch in Phase 4)

The 3.4 branch processes the OAuth2 + JWT + tenant authorization model verification.
Node 01: DomainSplitter — splits SECURITY (oauth, jwt) + AUTHORIZATION (tenant) into domain packets
Node 08: Arbiter — verdict: accepted | gateB: passed
Node 09: Formatter — FactoryState: trusted_learned

Other Phase 4 branches use generic nine-node flow.

---

### Cross-layer integrity check

| Layer | Predicates/Decisions | Chain valid? |
|---|---|---|
| 0-A | 8 predicates fired | YES |
| 0-B | 7 routing decisions (SECURITY+AUTH deduplicated) | YES |
| 0-D | 7 factories selected | YES |
| 1 | 5 execution phases, QA last | YES |
| 2 | 3.4 branch: trusted_learned; others: generic/rejected | YES |

Cross-layer integrity: CONSISTENT — maximum O1 scenario (8 predicates, 7 routing decisions, 5 execution phases, all 6 sub-phases activated).

---

### Summary: canonical predicate ordering enforced throughout

| Position | Predicate | Sub-phase | Factory | ExecutionPhase |
|---|---|---|---|---|
| 1 | ARCHITECTURE | 3.x | ArchitectureFlow | Phase 1 |
| 2 | API_CONTRACT | 3.3 | ApiContract33FlowAdapter | Phase 2 |
| 3 | IMPLEMENT | 4.1 | ImplementationFlow41 | Phase 3 |
| 4 | SECURITY+AUTHORIZATION | 3.4 | SecurityVerification34FlowAdapter | Phase 4 |
| 5 | TEST | 5.1 | generic (probationary_rejected fallback) | Phase 4 (parallel) |
| 6 | DOCUMENT | 2.1 | RequirementsAuthoringFlow21 | Phase 4 (parallel) |
| 7 | QA | 7.x | QAGatingFlow73 | Phase 5 (always last) |

The canonical ordering (ARCHITECTURE → API → IMPLEMENT → SECURITY+AUTH → TEST ‖ DOCUMENT → QA)
is preserved from predicate compilation through routing through factory selection through
orchestration execution phases. All seven layers are consistent.
