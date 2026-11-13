# Layer 1 — Transition Orchestrator Pairs (007)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-007-001 — Refactor verb TransitionType override

Request: "Refactor the agent skill dispatcher for better modularity."
Predicates: ["IMPLEMENT"] (refactor triggers IMPLEMENT predicate in code context)
Orchestration: ImplementationFlow41 (single flow)
TransitionType: refactoring ("refactor" verb overrides; highest priority over feature_addition)
Note: TransitionType hierarchy: refactoring > feature_addition > documentation > no contribution.

---

## TO-007-002 — ARCHITECTURE sub-phase 3.5 vs 3.1 in AI context

Request: "Design the infrastructure for the agent's vector database cluster."
Predicates: ["ARCHITECTURE"]
Orchestration: ArchitectureFlow35 (sub-phase 3.5 — infrastructure: database cluster, servers, deployment topology)
TransitionType: documentation
Note: "vector database cluster" is infrastructure vocabulary → 3.5, not 3.1. AI context does not change sub-phase rule.

---

## TO-007-003 — Multiple gap verbs, zero predicates

Request: "Adapt, fine-tune, and orchestrate the agent for healthcare queries."
Predicates: [] (gap — all three verbs: adapt/fine-tune/orchestrate are AI vocabulary gaps)
Orchestration: GapHandler invoked once (not three times — one SCHEDULE_CAPABILITY_LEARNING per request)
Gap ID: gap-ai-agent-adapt-001
DomainId: software-development
Returns: "Capability not available. Request logged for learning pipeline."

---

## TO-007-004 — IMPLEMENT+API_CONTRACT+SECURITY three-flow

Request: "Build the agent API client, integrate with the CRM API, and secure the token exchange."
Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
Orchestration: APIContractFlow33 → ImplementationFlow41 → SecurityAuditFlow
TransitionType: feature_addition
Ordering: API_CONTRACT first (contract spec informs implementation); IMPLEMENT second; SECURITY audits the result.

---

## TO-007-005 — QA sub-phase 7.1 in AI context

Request: "QA the agent release to staging."
Predicates: ["QA"]
Orchestration: ReleaseGateFlow71 (sub-phase 7.1 — staging/deployment vocabulary)
TransitionType: no contribution (QA alone does not drive TransitionType)
Note: "staging" → 7.1 (release gate); feature acceptance without staging context → 7.3 (default).
