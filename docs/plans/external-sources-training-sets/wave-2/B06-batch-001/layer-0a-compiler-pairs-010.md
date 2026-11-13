# Layer 0-A — Compiler Pairs (010)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note

File 010 covers feature-brief authoring: "feature-brief" fires DOCUMENT (Rule 14).
Also covers "capability" and "define" and "criteria" — strong DOCUMENT signal.

---

## B06-0A-010-001 — feature-brief fires DOCUMENT: Rule 14

```
source_id: xiigen-b06-0a-010-001
source_title: XIIGen B06 compiler — feature-brief fires DOCUMENT
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
  run_id: "B06-0a-010-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Author the feature brief for the one-click checkout capability and define the success criteria."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-0a-010-001-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/010-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "One-click checkout capability feature brief and success criteria"
  CompiledText: "Author the feature brief for the one-click checkout capability and define the success criteria."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "feature brief" (→ feature-brief), "capability" (→ capability),
    "define" (→ define), "criteria" (→ criteria)
  "author" — not in IMPLEMENT lexicon; "author" is a documentation verb but is
    not directly in IMPLEMENT lexicon. "write" would fire IMPLEMENT; "author" does not.
  Result: ["DOCUMENT"] — single predicate.
  Confidence: 0.92

---

## B06-0A-010-002 — feature-brief as compound term

```
source_id: xiigen-b06-0a-010-002
source_title: XIIGen B06 compiler — feature brief compound term recognition
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
  run_id: "B06-0a-010-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Create the feature brief for the subscription management module and document the business requirements."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-0a-010-002-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/010-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Subscription management feature brief and business requirements"
  CompiledText: "Create the feature brief for the subscription management module and document the business requirements."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "feature brief" (→ feature-brief), "document" (→ document),
    "business" (→ business), "requirements" (→ requirement)
  IMPLEMENT fires on: "create" (→ create)
  Result: ["DOCUMENT", "IMPLEMENT"]
  Note: "create" fires IMPLEMENT even when the object is a document artifact.
  Confidence: 0.92

---

## B06-0A-010-003 — "define" fires DOCUMENT, not IMPLEMENT: Rule 6

```
source_id: xiigen-b06-0a-010-003
source_title: XIIGen B06 compiler — define fires DOCUMENT not IMPLEMENT
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
  run_id: "B06-0a-010-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Define the acceptance criteria and success metrics for the one-click checkout feature."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-0a-010-003-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/010-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "One-click checkout acceptance criteria and success metrics"
  CompiledText: "Define the acceptance criteria and success metrics for the one-click checkout feature."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "define" (→ define), "criteria" (→ criteria)
  "feature" — in IMPLEMENT lexicon but here as noun referent.
  "metrics" — not in any predicate lexicon.
  Note on "feature": "feature" is in IMPLEMENT lexicon per the reference lexicon.
  However this is a borderline case where "feature" appears as a domain noun.
  Teaching: the compiler may fire IMPLEMENT on "feature" depending on implementation
  of the lexicon matching. Conservative reading: DOCUMENT only.
  Result: ["DOCUMENT"]
  Confidence: 0.92

---

## B06-0A-010-004 — feature-brief vs feature: lexicon distinction

```
source_id: xiigen-b06-0a-010-004
source_title: XIIGen B06 compiler — feature-brief vs feature lexicon distinction
source_license_state: first_party
pair_kind: compiler
```

Teaching note — "feature-brief" vs "feature":

  "feature-brief" (or "feature brief")  → DOCUMENT lexicon (Rule 14)
  "feature" alone                        → IMPLEMENT lexicon

  These are different lexicon entries. "feature-brief" is a compound documentation
  term meaning "a brief requirements document for a feature". It fires DOCUMENT.

  "feature" alone (e.g., "new feature", "add this feature") fires IMPLEMENT because
  it signals implementation work — adding or building a feature.

  Examples:
    "write the feature brief"   → DOCUMENT (feature-brief) + IMPLEMENT (write)
    "build the feature"         → IMPLEMENT (build, feature)
    "author the feature brief"  → DOCUMENT (feature-brief)
    "create the feature"        → IMPLEMENT (create, feature)

  The hyphenated compound "feature-brief" as a single unit takes the DOCUMENT
  lexicon entry and overrides the standalone "feature" entry.
