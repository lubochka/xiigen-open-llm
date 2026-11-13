# Layer 0-A — Prompt Routing Input Compiler Pairs
# Batch: O1-batch-001
# Scenario: O1 — multi-tenant JWT authentication
# Contract: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: adaptive_routing_compiler

---

## Coverage map

| Pair ID | Prompt type | Predicates matched | Confidence |
|---|---|---|---|
| O1B1-WDA-compiler-001 | Multi-predicate JWT+tenant+API+test | SECURITY, AUTHORIZATION, API_CONTRACT, TEST | 0.92 |
| O1B1-WDA-compiler-002 | No-signal ambiguous short prompt | (none) | 0.70 |
| O1B1-WDA-compiler-003 | Single-domain API-contract prompt | API_CONTRACT | 0.92 |

---

## Pair O1B1-WDA-compiler-001 — multi-predicate JWT tenant prompt

```
source_id: xiigen-o1-prompt-001
source_title: XIIGen O1 scenario user prompt — JWT authentication
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
phase: whole_development_area
sub_phase: whole_development_area
task_type: adaptive_routing_compilation
node_role: prompt-routing-input-compiler
coverage_status: covered
pair_kind: adaptive_routing_compiler
```

Contract:
  PromptRoutingInputCompilerUnit
      : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>

A:
  run_id: "O1-router-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "We need to add multi-tenant JWT authentication to the API and make sure tenant isolation is tested."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true
    UseContextCarryForward: true
  metadata:
    source: "xiigen-first-party-prompt"
    license_state: "first_party"

B:
  DocumentId: "O1-router-001-prompt"
  SourcePath: "logical-orchestrator://user-prompt"
  SourceTier: "runtime_prompt"
  LicenseOrAuthority: "user_request"
  DocumentType: "user_prompt"
  DocumentTitle: "Runtime user prompt"
  CompiledText: "We need to add multi-tenant JWT authentication to the API and make sure tenant isolation is tested."
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: "runtime_prompt_compiled"
  [unit result envelope]
    UnitId: "unit-prompt-routing-input-compiler-v1"
    UnitRole: "prompt-routing-input-compiler"
    DomainTag: "whole_development_area"
    Confidence: 0.92
    PredicateIds: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "TEST"]
    Warnings: []

Lexicon evidence:
  SECURITY matched on: "jwt", "authentication"
  AUTHORIZATION matched on: "tenant"
  API_CONTRACT matched on: "api"
  TEST matched on: "tested"

---

## Pair O1B1-WDA-compiler-002 — no-predicate ambiguous prompt

```
source_id: xiigen-o1-no-signal-001
source_title: XIIGen short ambiguous prompt — no clear domain signal
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: unknown
phase: whole_development_area
sub_phase: whole_development_area
task_type: adaptive_routing_compilation
node_role: prompt-routing-input-compiler
coverage_status: covered
pair_kind: adaptive_routing_compiler
```

Contract:
  PromptRoutingInputCompilerUnit
      : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>

A:
  run_id: "O1-router-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "The thing on the login page is broken again."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true
  metadata:
    source: "xiigen-first-party-prompt"
    license_state: "first_party"

B:
  DocumentId: "O1-router-002-prompt"
  SourcePath: "logical-orchestrator://user-prompt"
  SourceTier: "runtime_prompt"
  LicenseOrAuthority: "user_request"
  DocumentType: "user_prompt"
  DocumentTitle: "Runtime user prompt"
  CompiledText: "The thing on the login page is broken again."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "runtime_prompt_compiled"
  [unit result envelope]
    UnitId: "unit-prompt-routing-input-compiler-v1"
    UnitRole: "prompt-routing-input-compiler"
    DomainTag: "whole_development_area"
    Confidence: 0.70
    PredicateIds: []
    Warnings: ["no_high_signal_predicate"]

Lexicon evidence:
  SECURITY: "login" is not in the SECURITY lexicon (jwt, authentication, auth, oauth, oidc,
    token, session, asvs, mfa, rbac, security, authorization). "login" is colloquial, not
    a predicate-matched term. No predicate fires.
  All other predicates: no match.

---

## Pair O1B1-WDA-compiler-003 — single-domain API-only prompt

```
source_id: xiigen-o1-api-only-001
source_title: XIIGen API contract prompt — single domain
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
phase: whole_development_area
sub_phase: whole_development_area
task_type: adaptive_routing_compilation
node_role: prompt-routing-input-compiler
coverage_status: covered
pair_kind: adaptive_routing_compiler
```

Contract:
  PromptRoutingInputCompilerUnit
      : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>

A:
  run_id: "O1-router-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "We need to document the API endpoints for our external partners, including request and response schema."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true
  metadata:
    source: "xiigen-first-party-prompt"
    license_state: "first_party"

B:
  DocumentId: "O1-router-003-prompt"
  SourcePath: "logical-orchestrator://user-prompt"
  SourceTier: "runtime_prompt"
  LicenseOrAuthority: "user_request"
  DocumentType: "user_prompt"
  DocumentTitle: "Runtime user prompt"
  CompiledText: "We need to document the API endpoints for our external partners, including request and response schema."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: "runtime_prompt_compiled"
  [unit result envelope]
    UnitId: "unit-prompt-routing-input-compiler-v1"
    UnitRole: "prompt-routing-input-compiler"
    DomainTag: "whole_development_area"
    Confidence: 0.92
    PredicateIds: ["API_CONTRACT"]
    Warnings: []

Lexicon evidence:
  API_CONTRACT matched on: "api", "endpoints", "request", "response", "schema"
  DOCUMENT matched on: "document" — but DOCUMENT is not in the SECURITY/AUTHORIZATION/ARCHITECTURE
    predicate set used for routing. It is registered in the lexicon but the sub-phase
    registry maps DOCUMENT to requirements-phase sub-phases (2.1, 2.2), not to 3.3.
    DOCUMENT does not appear in CompiledPredicateIds because the router lexicon used
    here covers the nine routing predicates only: SECURITY, AUTHORIZATION, API_CONTRACT,
    ARCHITECTURE, IMPLEMENT, TEST, QA, UI, DOCUMENT.
  Correction: "document" matches DOCUMENT predicate.
  Revised CompiledPredicateIds: ["DOCUMENT", "API_CONTRACT"]
  Revised Confidence: 0.92 (at least one predicate matched)

Correction note: The B above shows CompiledPredicateIds: ["DOCUMENT", "API_CONTRACT"].
  The router will match this document to sub-phase 3.3 (API_CONTRACT) and potentially 2.1
  (DOCUMENT). For this batch the 3.3 routing decision is the relevant one.
