# Layer 0-A — Compiler Pairs (002)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: compiler

---

## B01-0A-002-001 — UI alone: "page" + "interface" fire; "navigation" does not

```
source_id: xiigen-b01-0a-002-001
source_title: XIIGen B01 compiler-002 — admin interface page inventory
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
  run_id: "B01-0a-002-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Create an inventory of all pages in the admin interface and list each page's purpose and navigation."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-002-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-002-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Admin interface page inventory"
  CompiledText: "Create an inventory of all pages in the admin interface and list each page's purpose and navigation."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "page" (UI lexicon), "interface" (UI lexicon)
  "navigation" — NOT in the UI predicate lexicon; it is a common English word
    but the UI lexicon contains: ui, interface, ux, screen, page, modal, form,
    layout, component, responsive, accessibility, visual, wireframe, prototype,
    interaction, button, input, typography, color, spacing, design-system, theme.
    "navigation" is absent from this list; its presence does not fire UI.
  "purpose" — not in any predicate lexicon.
  "Create" fires IMPLEMENT in some contexts, but "create an inventory" is an
    information-gathering UX task; no IMPLEMENT lexicon term (implement, code,
    build, develop, write, function, class, logic, library, handler) is present.
  Final predicates: ["UI"]
  Confidence: 0.92

---

## B01-0A-002-002 — AdminPanelScreenInventory: "page" + "interface" + "interaction" fire

```
source_id: xiigen-b01-0a-002-002
source_title: XIIGen B01 compiler-002 — admin panel full screen inventory
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
  run_id: "B01-0a-002-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Map every page in the admin panel and document each page's interaction model, entry points, and screen layout."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-002-002-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-002-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Admin panel page interaction and screen layout inventory"
  CompiledText: "Map every page in the admin panel and document each page's interaction model, entry points, and screen layout."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "page" (×2), "interaction" (UI lexicon), "screen" (UI lexicon), "layout" (UI lexicon)
  DOCUMENT fires on: "document" (DOCUMENT lexicon)
  "admin panel" — no predicate fires on domain-context words.
  "entry points" — not in any predicate lexicon.
  Final predicates: ["UI", "DOCUMENT"]
  Confidence: 0.92

---

## B01-0A-002-003 — UI alone boundary: "navigation" present but does not fire

```
source_id: xiigen-b01-0a-002-003
source_title: XIIGen B01 compiler-002 — navigation boundary test
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
  run_id: "B01-0a-002-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "List the navigation structure of the admin interface so we can verify every page is reachable."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-002-003-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-002-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Admin interface navigation structure and page reachability"
  CompiledText: "List the navigation structure of the admin interface so we can verify every page is reachable."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "interface" (UI lexicon), "page" (UI lexicon)
  "navigation structure" — "navigation" is NOT in the UI lexicon; only "interface"
    and "page" trigger UI. The word "navigation" appears prominently but the rule
    is strict: only listed lexicon terms fire predicates.
  "verify" — not in any predicate lexicon.
  Final predicates: ["UI"]
  Confidence: 0.92

---

## B01-0A-002-004 — Key rule reminder: "navigation" gap annotation

```
source_id: xiigen-b01-0a-002-004
source_title: XIIGen B01 compiler-002 — navigation gap rule annotation
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: compiler
```

Rule annotation (no new A/B pair — supplements pairs 001-003):

  "navigation" appears in UX requests frequently. The B01 rule is:
    - "navigation" alone → NO predicate fires (gap-like term, not in any lexicon)
    - "navigation links" → no predicate fires
    - "navigation menu" → no predicate fires
    - "navigation bar" → no predicate fires
    - "page navigation" → UI fires on "page" (not on "navigation")
    - "screen navigation" → UI fires on "screen" (not on "navigation")
    - "interface navigation" → UI fires on "interface" (not on "navigation")

  The correct approach: scan each token independently. "navigation" is always
  inert; fire UI only on the co-present UI lexicon terms.

  This rule does NOT require a gap ID because the request still has a routable
  predicate when other UI terms are present. Gap IDs are only assigned when the
  entire compiled predicate list is empty.
