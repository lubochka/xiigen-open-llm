# Layer 0-A — Compiler Pairs (005)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: compiler

---

## B01-0A-005-001 — DOCUMENT alone: "describe" + "functional" + "behavior"

```
source_id: xiigen-b01-0a-005-001
source_title: XIIGen B01 compiler-005 — functional behavior description, no UI terms
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
phase: 2
sub_phase: 2.1
task_type: business-requirements
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B01-0a-005-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Describe the functional behavior of the onboarding wizard and document the user journey requirements."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-005-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-005-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Onboarding wizard functional behavior and user journey requirements"
  CompiledText: "Describe the functional behavior of the onboarding wizard and document the user journey requirements."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "describe" (DOCUMENT lexicon), "functional" (DOCUMENT lexicon),
    "behavior" (DOCUMENT lexicon), "document" (DOCUMENT lexicon), "requirement"
    (DOCUMENT lexicon — via "requirements")
  UI terms checked: "onboarding" — NOT in UI lexicon; "wizard" — NOT in UI lexicon;
    "journey" — NOT in UI lexicon. No UI term is present.
  Note: "onboarding" is a domain context word, not a predicate term. Rule 10 from
    B01 rules: "onboarding", "signup", "email activation", "first-run" → NOT in any
    predicate lexicon directly. Context signals but does not fire predicates.
  Final predicates: ["DOCUMENT"]
  TransitionType: documentation (DOCUMENT alone, no UI or IMPLEMENT present)
  Confidence: 0.92

---

## B01-0A-005-002 — DOCUMENT alone: "use-case" + "criteria" + "narrative"

```
source_id: xiigen-b01-0a-005-002
source_title: XIIGen B01 compiler-005 — use-case narrative, no UI terms
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
phase: 2
sub_phase: 2.1
task_type: business-requirements
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B01-0a-005-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Write a narrative use-case for the admin user role, including acceptance criteria and capability epics."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-005-002-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-005-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Admin role use-case narrative with acceptance criteria and epics"
  CompiledText: "Write a narrative use-case for the admin user role, including acceptance criteria and capability epics."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "narrative" (DOCUMENT lexicon), "use-case" (DOCUMENT lexicon),
    "criteria" (DOCUMENT lexicon — via "acceptance criteria"), "capability" (DOCUMENT
    lexicon), "epics" (DOCUMENT lexicon)
  IMPLEMENT fires on: "write" (IMPLEMENT lexicon) — "Write a narrative..."
  UI checked: "admin", "user", "role" — none are in UI lexicon.
  Final predicates: ["DOCUMENT", "IMPLEMENT"]
  TransitionType: feature_addition (IMPLEMENT present)
  Confidence: 0.92

---

## B01-0A-005-003 — DOCUMENT alone routing to 2.1 (no UI = no 1.1 routing)

```
source_id: xiigen-b01-0a-005-003
source_title: XIIGen B01 compiler-005 — requirements only, no UI lexicon
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
phase: 2
sub_phase: 2.1
task_type: business-requirements
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B01-0a-005-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Define the business requirements for the password reset feature and describe each step's acceptance criteria."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-005-003-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-005-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Password reset business requirements and acceptance criteria"
  CompiledText: "Define the business requirements for the password reset feature and describe each step's acceptance criteria."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "define" (DOCUMENT lexicon), "requirement" (via "requirements",
    DOCUMENT lexicon), "describe" (DOCUMENT lexicon), "criteria" (via "acceptance
    criteria", DOCUMENT lexicon)
  UI checked: "password reset" — no UI terms; "feature" — not in UI lexicon (it is
    in DOCUMENT lexicon? No — "feature-brief" is in DOCUMENT lexicon but "feature"
    alone is an IMPLEMENT lexicon term). "feature" in IMPLEMENT: yes.
  Revised: DOCUMENT fires + IMPLEMENT fires on "feature" (IMPLEMENT lexicon includes
    "feature"). Final predicates: ["DOCUMENT", "IMPLEMENT"]

  Correction applied:
    CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  TransitionType: feature_addition (IMPLEMENT present)
  Confidence: 0.92
