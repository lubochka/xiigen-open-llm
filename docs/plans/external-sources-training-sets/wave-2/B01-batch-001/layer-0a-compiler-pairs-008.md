# Layer 0-A — Compiler Pairs (008)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: compiler

---

## B01-0A-008-001 — Accessibility: "accessibility" fires UI; combined with DOCUMENT

```
source_id: xiigen-b01-0a-008-001
source_title: XIIGen B01 compiler-008 — accessibility audit with WCAG documentation
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B01-0a-008-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Audit the accessibility of all onboarding screens and document the WCAG compliance requirements."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-008-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-008-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Onboarding screen accessibility audit and WCAG compliance requirements"
  CompiledText: "Audit the accessibility of all onboarding screens and document the WCAG compliance requirements."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "accessibility" (UI lexicon — B01 rule 11: "accessibility" → UI fires),
    "screen" (UI lexicon — "screens")
  DOCUMENT fires on: "document" (DOCUMENT lexicon), "requirement" (DOCUMENT lexicon
    — via "requirements")
  "WCAG" (Web Content Accessibility Guidelines) — acronym for Web Content
    Accessibility Guidelines; not a predicate term itself; does not fire any predicate.
    UI fires on "accessibility" in the preceding phrase.
  "Audit" — not in any predicate lexicon.
  Final predicates: ["UI", "DOCUMENT"]
  Confidence: 0.92

---

## B01-0A-008-002 — "accessibility" alone is sufficient for UI predicate

```
source_id: xiigen-b01-0a-008-002
source_title: XIIGen B01 compiler-008 — accessibility alone fires UI
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B01-0a-008-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Review the accessibility compliance of the new checkout flow and flag any issues."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-008-002-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-008-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Checkout flow accessibility compliance review"
  CompiledText: "Review the accessibility compliance of the new checkout flow and flag any issues."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "accessibility" (UI lexicon — fires UI alone, no additional UI
    term required)
  "checkout flow" — "flow" is not in UI or any predicate lexicon.
  "compliance", "review", "flag", "issues" — none in any predicate lexicon.
  Final predicates: ["UI"]
  Confidence: 0.92

---

## B01-0A-008-003 — Full accessibility + screen + document triple

```
source_id: xiigen-b01-0a-008-003
source_title: XIIGen B01 compiler-008 — full accessibility and screen documentation
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B01-0a-008-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Map all onboarding screens for accessibility issues, define remediation criteria, and describe the expected behavior after fixes."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-008-003-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-008-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Onboarding screen accessibility mapping with remediation criteria"
  CompiledText: "Map all onboarding screens for accessibility issues, define remediation criteria, and describe the expected behavior after fixes."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "screen" (UI lexicon — "screens"), "accessibility" (UI lexicon)
  DOCUMENT fires on: "define" (DOCUMENT lexicon), "criteria" (DOCUMENT lexicon),
    "describe" (DOCUMENT lexicon), "behavior" (DOCUMENT lexicon)
  Final predicates: ["UI", "DOCUMENT"]
  Confidence: 0.92
