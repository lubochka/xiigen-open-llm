# Layer 0-A — Compiler Pairs
# Batch: B38-batch-001
# Scenario: B38 — Project delivery — release gates and go-live checklists
# Contract: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: compiler

---

## Method note

B38 covers release gate and go-live vocabulary in the context of sub-phase 7.1
(Release and Deployment Automation). QA predicate fires on: review, approve,
verify, validate, accept, quality, audit, sign-off, compliance, inspect.
Gap vocabulary in this batch: go-live, deploy (standalone), launch, checklist,
cutover, rollout, release-gate (as compound), on-call, runbook.

Key boundary: "deploy" alone does not fire IMPLEMENT (no code-level co-occurrence
required). "approve the release" fires QA on "approve". "review the checklist"
fires QA on "review" even though "checklist" is gap vocabulary.

---

## Pair B38B1-WDA-compiler-001 — release review with QA approval: QA fires

```
source_id: xiigen-b38-compiler-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
phase: 7
sub_phase: 7.1
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B38-compiler-001"
  user_request: "Review the release checklist and approve the go-live for the payment service upgrade."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B38-compiler-001-prompt"
  SourcePath: "wave-2/B38-batch-001/compiler-input/001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Release checklist review and go-live approval for payment service"
  CompiledText: "Review the release checklist and approve the go-live for the payment service upgrade."
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "review" → QA fires on "review"
  "approve" → QA fires on "approve"
  "release" — not in QA lexicon as a standalone term; fires via "review" + "approve"
  "checklist" — not in any predicate lexicon (gap vocabulary)
  "go-live" — not in any predicate lexicon (gap vocabulary)
  "payment service" — "service" is in IMPLEMENT lexicon → IMPLEMENT fires!

  Correction: "service" fires IMPLEMENT.
  CompiledPredicateIds: ["QA", "IMPLEMENT"]

  Note: "payment service upgrade" contains "service" which is in IMPLEMENT. The
  compiler is literal — "service" fires IMPLEMENT even in a release review sentence.
  CapabilityGapWarnings for checklist and go-live.

Corrected B:
  CompiledPredicateIds: ["QA", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  CapabilityGapWarnings:
    - "gap-project-delivery-checklist-001: 'release checklist' not captured"
    - "gap-project-delivery-go-live-001: 'go-live' not captured"

---

## Pair B38B1-WDA-compiler-002 — pure release gate: QA fires on "validate"

```
source_id: xiigen-b38-compiler-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
phase: 7
sub_phase: 7.1
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B38-compiler-002"
  user_request: "Validate all release criteria and confirm the deployment is ready for the production cutover."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B38-compiler-002-prompt"
  SourcePath: "wave-2/B38-batch-001/compiler-input/002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Release criteria validation and production cutover readiness"
  CompiledText: "Validate all release criteria and confirm the deployment is ready for the production cutover."
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "validate" → QA fires on "validate"
  "release criteria" — "criteria" IS in DOCUMENT lexicon → DOCUMENT fires!
  "confirm" — not in any predicate lexicon
  "deployment" — not in IMPLEMENT (deploy/deployment without code-level context)
  "production" — not in any predicate lexicon
  "cutover" — not in any predicate lexicon (gap vocabulary)

  Correction: "criteria" fires DOCUMENT.
  CompiledPredicateIds: ["QA", "DOCUMENT"]

Corrected B:
  CompiledPredicateIds: ["QA", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  CapabilityGapWarnings:
    - "gap-project-delivery-cutover-001: 'production cutover' not captured"
    - "gap-project-delivery-deployment-001: 'deployment' not captured; not in IMPLEMENT without code co-occurrence"

---

## Pair B38B1-WDA-compiler-003 — pure gap: deploy and rollout only

```
source_id: xiigen-b38-compiler-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: unknown
phase: null
sub_phase: null
coverage_status: knowledge_edge
pair_kind: compiler
```

A:
  run_id: "B38-compiler-003"
  user_request: "Execute the production rollout and monitor the go-live deployment for any issues."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B38-compiler-003-prompt"
  SourcePath: "wave-2/B38-batch-001/compiler-input/003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Production rollout execution and go-live monitoring"
  CompiledText: "Execute the production rollout and monitor the go-live deployment for any issues."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate evidence:
  "execute" — not in any predicate lexicon
  "production" — not in any predicate lexicon
  "rollout" — not in any predicate lexicon (gap vocabulary)
  "monitor" — not in any predicate lexicon
  "go-live" — not in any predicate lexicon (gap vocabulary)
  "deployment" — not in IMPLEMENT (no code-level co-occurrence)
  "issues" — not in any predicate lexicon
  Result: CompiledPredicateIds: [], knowledge_edge
  Capability gap ID: gap-project-delivery-rollout-golive-001
