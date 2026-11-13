# Layer 0-A — Compiler Pairs (005)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note

File 005 covers the StakeholderRequirements scenario: DOCUMENT routes to both 2.1 and 2.4
when stakeholder signals and requirements signals co-occur. Single predicate, dual routing.

---

## B06-0A-005-001 — DOCUMENT → 2.1 + 2.4 dual routing signals

```
source_id: xiigen-b06-0a-005-001
source_title: XIIGen B06 compiler — DOCUMENT with both 2.1 and 2.4 signals
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
phase: 2
sub_phase: 2.1
task_type: requirements-and-business-rules
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B06-0a-005-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Describe the checkout flow requirements and document the stakeholder expectations for each user type."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-0a-005-001-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/005-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Checkout flow requirements and stakeholder expectations"
  CompiledText: "Describe the checkout flow requirements and document the stakeholder expectations for each user type."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "describe" (→ describe), "requirements" (→ requirement),
    "document" (→ document), "stakeholder" (→ stakeholder)
  Four DOCUMENT terms, one predicate compilation.
  Sub-phase routing signals:
    "requirements" → 2.1 signal (Business Requirements and Use Cases)
    "stakeholder"  → 2.4 signal (Stakeholder and External Requirements)
  The compiler compiles ONE predicate (DOCUMENT). The router reads both sub-phase
  signals within the DOCUMENT predicate and produces TWO routing decisions.
  Confidence: 0.92

---

## B06-0A-005-002 — DOCUMENT alone: "stakeholder" term fires 2.4 sub-phase signal

```
source_id: xiigen-b06-0a-005-002
source_title: XIIGen B06 compiler — stakeholder term alone, DOCUMENT predicate
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
phase: 2
sub_phase: 2.4
task_type: requirements-and-business-rules
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B06-0a-005-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Document the stakeholder expectations and external partner requirements for the checkout integration."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-0a-005-002-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/005-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Stakeholder expectations and external partner requirements"
  CompiledText: "Document the stakeholder expectations and external partner requirements for the checkout integration."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "document" (→ document), "stakeholder" (→ stakeholder),
    "requirements" (→ requirement)
  Sub-phase signal: "stakeholder" and "external" → primary signal for 2.4.
  "requirements" also present → secondary 2.1 signal.
  Single predicate DOCUMENT; router will produce decisions for 2.4 (primary)
  and potentially 2.1 (secondary, lower confidence).
  Confidence: 0.92

---

## B06-0A-005-003 — Phase 2 sub-phase signal mapping

```
source_id: xiigen-b06-0a-005-003
source_title: XIIGen B06 compiler — phase 2 sub-phase signal mapping
source_license_state: first_party
pair_kind: compiler
```

Teaching note — Phase 2 sub-phase signals within DOCUMENT predicate:

  Sub-phase | Primary signals                          | Owner
  ----------|------------------------------------------|------
  2.1       | requirements, use-case, use case,        | Business Requirements and Use Cases
            | acceptance criteria, business rules,     |
            | functional, behavior, story              |
  2.2       | technical requirements, data model,      | Technical Requirements
            | API contract, schema, interface spec     |
  2.3       | performance, scalability, security,      | Non-Functional Requirements
            | non-functional, SLA (Service-Level       |
            | Agreement), load, availability           |
  2.4       | stakeholder, external, partner,          | Stakeholder and External Requirements
            | compliance, regulatory, legal, vendor    |

  When a DOCUMENT predicate is compiled and the user request contains signals
  from multiple sub-phases, the router produces one routing decision per
  sub-phase where sufficient signal density exists.

  One predicate → multiple routing decisions is valid and expected.

---

## B06-0A-005-004 — B06 Rule 11: one predicate routing to multiple sub-phases

```
source_id: xiigen-b06-0a-005-004
source_title: XIIGen B06 compiler — rule 11 one predicate multiple sub-phases
source_license_state: first_party
pair_kind: compiler
```

B06 Core Rule 11 teaching:
  "one predicate can route to multiple sub-phases"
  — DOCUMENT routes to 2.1 AND 2.4 when both sub-phase signals are present
    (business requirements + stakeholder requirements in same request)

  This is a routing rule, not a compiler rule.
  The compiler produces: CompiledPredicateIds: ["DOCUMENT"] (one entry)
  The router produces: two RoutingDecisions — one for 2.1, one for 2.4
  The compiler output does NOT change; only the router output expands.

  Wrong model behavior to avoid:
    - Compiling two DOCUMENT entries (incorrect — one predicate fires once)
    - Routing to only 2.1 and ignoring the 2.4 stakeholder signal (v6-03 corruption)
    - Routing to only 2.4 and ignoring the 2.1 requirements signal
