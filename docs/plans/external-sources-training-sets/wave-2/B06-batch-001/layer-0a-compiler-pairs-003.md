# Layer 0-A — Compiler Pairs (003)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note

File 003 covers the UserStoryWriting scenario: "write stories" fires DOCUMENT+IMPLEMENT.
This is the canonical dual-predicate case for B06. DOCUMENT fires on "story" and
"criteria". IMPLEMENT fires on "write". Neither suppresses the other.

---

## B06-0A-003-001 — UserStoryWriting: DOCUMENT+IMPLEMENT dual predicate

```
source_id: xiigen-b06-0a-003-001
source_title: XIIGen B06 compiler — user story writing dual predicate
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
task_type: requirements-and-business-rules
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B06-0a-003-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Write the user stories for the order management feature with acceptance criteria for each scenario."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-0a-003-001-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/003-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Order management user stories with acceptance criteria"
  CompiledText: "Write the user stories for the order management feature with acceptance criteria for each scenario."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "story" (→ story, from "user stories"),
    "criteria" (→ criteria, from "acceptance criteria")
  IMPLEMENT fires on: "write" (→ write)
  Both predicates fire independently. "write" is not cancelled by context.
  The compiler is lexicon-based: if "write" appears, IMPLEMENT fires — even
  when the sentence is "write user stories" rather than "write code".
  Confidence: 0.92

Teaching note — Rule 3 of B06 core rules:
  "write the user stories" → DOCUMENT fires on "story" + IMPLEMENT fires on "write"
  — DUAL PREDICATE result; neither suppresses the other.
  This is distinguishable from "describe the user stories" where only DOCUMENT fires
  because "describe" is in DOCUMENT lexicon but not in IMPLEMENT lexicon.

---

## B06-0A-003-002 — UserStoryWriting: "write" fires IMPLEMENT even in doc context

```
source_id: xiigen-b06-0a-003-002
source_title: XIIGen B06 compiler — write fires IMPLEMENT in documentation context
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
task_type: requirements-and-business-rules
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B06-0a-003-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Write the specification for the order cancellation use case including the business rules."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-0a-003-002-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/003-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Order cancellation specification with business rules"
  CompiledText: "Write the specification for the order cancellation use case including the business rules."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "specification" (→ specification),
    "use case" (→ use-case), "business" (→ business)
  IMPLEMENT fires on: "write" (→ write)
  Result: dual predicate ["DOCUMENT", "IMPLEMENT"]
  Key teaching: "write the specification" → DOCUMENT fires on "specification"
  + IMPLEMENT fires on "write" — same dual-predicate pattern as Rule 4 of B06 core rules.
  Confidence: 0.92

---

## B06-0A-003-003 — UserStoryWriting: contrast — "describe" does NOT fire IMPLEMENT

```
source_id: xiigen-b06-0a-003-003
source_title: XIIGen B06 compiler — describe does not fire IMPLEMENT
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
  run_id: "B06-0a-003-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Describe the user stories for the order management feature including the acceptance criteria."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-0a-003-003-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/003-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Order management user stories described"
  CompiledText: "Describe the user stories for the order management feature including the acceptance criteria."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "describe" (→ describe), "story" (→ story),
    "criteria" (→ criteria)
  IMPLEMENT does NOT fire: "describe" is in DOCUMENT lexicon only.
    "feature" is in IMPLEMENT lexicon but here appears as a noun referent.
  Result: ["DOCUMENT"] only — single predicate.
  Contrast with pair 003-001: "write" vs "describe" — the verb makes the difference.
  Confidence: 0.92

---

## B06-0A-003-004 — "write" vs "describe" boundary rule table

```
source_id: xiigen-b06-0a-003-004
source_title: XIIGen B06 compiler — write vs describe boundary
source_license_state: first_party
pair_kind: compiler
```

Teaching boundary table — IMPLEMENT firing on action verbs:

  Verb        | Predicate fired | Notes
  ------------|-----------------|-------
  write       | IMPLEMENT       | IMPLEMENT lexicon term; fires regardless of object
  create      | IMPLEMENT       | IMPLEMENT lexicon term
  build       | IMPLEMENT       | IMPLEMENT lexicon term
  develop     | IMPLEMENT       | IMPLEMENT lexicon term
  implement   | IMPLEMENT       | IMPLEMENT lexicon term
  describe    | DOCUMENT        | DOCUMENT lexicon term; NOT IMPLEMENT
  define      | DOCUMENT        | DOCUMENT lexicon term; NOT IMPLEMENT
  document    | DOCUMENT        | DOCUMENT lexicon term; NOT IMPLEMENT
  specify     | DOCUMENT        | maps to "spec/specification" in DOCUMENT lexicon
  capture     | neither         | not in any predicate lexicon

  Rule: if the action verb is in IMPLEMENT lexicon, IMPLEMENT fires.
  Even when the verb precedes a documentation noun ("write the specification"),
  IMPLEMENT fires because the compiler is lexicon-based, not intent-based.
