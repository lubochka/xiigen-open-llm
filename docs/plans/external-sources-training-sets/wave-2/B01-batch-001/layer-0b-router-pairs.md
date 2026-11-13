# Layer 0-B — Router Pairs
# Batch: B01-batch-001
# Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Contract: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: router

---

## Method note

Layer 0-B pairs teach the router contract. The A side is taken verbatim
from the B output of the corresponding Layer 0-A compiler pair. The B side
is the routing decision array the router produces.

Routing rules demonstrated in this batch:
  [UI] alone                → single decision: 1.1 (User Flows and Screen Inventory)
  [UI, DOCUMENT] together   → two decisions: 1.1 (UI flows) + 2.1 (page specification)
  KnowledgeEdge input       → KnowledgeEdge: true result, null sub-phase (covered in 0-C)

EM-9 guard: RoutingDecision must NOT contain fields dependency_order,
parallel_group, domain_label, or is_primary. Only the canonical fields.

---

## Pair B01B1-WDA-router-001

```
source_id: xiigen-b01-router-001
source_title: XIIGen B01 router — UI-only routing to sub-phase 1.1
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — taken verbatim from Layer 0-A pair B01B1-WDA-compiler-001 B output]
  DocumentId: "B01-compiler-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "SaaS onboarding screen flow and UX mapping"
  CompiledText: "We need to map all screen flows and create a complete screen inventory for the SaaS onboarding UX, covering signup, email activation, and the first-run experience."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-compiler-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 1

Routing rationale:
  UI predicate is the sole signal. Sub-phase 1.1 (User Flows and Screen Inventory)
  owns the UI predicate for Phase 1. NearestWrongSubPhaseScore 0.41 reflects that
  sub-phases 1.2 through 1.5 are plausible UI candidates but 1.1 is the primary
  owner for screen inventory and flow mapping specifically.

---

## Pair B01B1-WDA-router-002

```
source_id: xiigen-b01-router-002
source_title: XIIGen B01 router — UI page-inventory routing to sub-phase 1.1
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — taken verbatim from Layer 0-A pair B01B1-WDA-compiler-002 B output]
  DocumentId: "B01-compiler-002-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Admin interface page inventory with navigation and interaction patterns"
  CompiledText: "Create an inventory of all pages in the admin interface, listing each page's purpose, navigation links, and interaction patterns."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-compiler-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 1

Routing rationale:
  UI predicate (page, interface, interaction) routes to 1.1. "Inventory" is a
  strong screen-inventory signal that makes 1.1 the clear owner over 1.2-1.5.
  No competing predicates are present.

---

## Pair B01B1-WDA-router-003

```
source_id: xiigen-b01-router-003
source_title: XIIGen B01 router — UI+DOCUMENT dual-branch routing to 1.1 and 2.1
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — taken verbatim from Layer 0-A pair B01B1-WDA-compiler-003 B output]
  DocumentId: "B01-compiler-003-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Checkout interface flow documentation and page specification"
  CompiledText: "Document the complete user interface flows for the checkout process and produce a page specification with form interaction details at each step."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-compiler-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.44
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-compiler-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.39
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Routing rationale:
  Two predicates → two independent routing decisions. UI evidence predicates route
  to 1.1 (screen flows and inventory). DOCUMENT evidence predicates route to 2.1
  (Business Requirements and Use Cases — the page specification is a business
  requirement artifact). The model must produce two separate RoutingDecision entries,
  each carrying only its own evidence predicates — never merge them into one.
