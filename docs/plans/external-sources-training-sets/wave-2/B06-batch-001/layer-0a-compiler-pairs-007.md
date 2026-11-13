# Layer 0-A — Compiler Pairs (007)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note

File 007 covers DOCUMENT + UI: requirements with UI context. "document", "requirements",
"describe", "behavior" fire DOCUMENT. "form", "interface" fire UI. Dual predicate.

---

## B06-0A-007-001 — DOCUMENT+UI: checkout form requirements

```
source_id: xiigen-b06-0a-007-001
source_title: XIIGen B06 compiler — checkout form requirements DOCUMENT+UI
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
  run_id: "B06-0a-007-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Document the checkout form requirements and describe the user interface behavior for each step."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-0a-007-001-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/007-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Checkout form requirements and UI behavior specification"
  CompiledText: "Document the checkout form requirements and describe the user interface behavior for each step."
  CompiledPredicateIds: ["DOCUMENT", "UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "document" (→ document), "requirements" (→ requirement),
    "describe" (→ describe), "behavior" (→ behavior)
  UI fires on: "form" (→ form), "interface" (→ interface)
  No IMPLEMENT terms: "document" and "describe" are DOCUMENT-only verbs.
  Result: ["DOCUMENT", "UI"]
  Confidence: 0.92

---

## B06-0A-007-002 — DOCUMENT+UI: screen requirements

```
source_id: xiigen-b06-0a-007-002
source_title: XIIGen B06 compiler — screen requirements DOCUMENT+UI
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
  run_id: "B06-0a-007-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Specify the requirements for the payment screen and describe the UX flow for each payment method selection."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-0a-007-002-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/007-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Payment screen requirements and UX flow specification"
  CompiledText: "Specify the requirements for the payment screen and describe the UX flow for each payment method selection."
  CompiledPredicateIds: ["DOCUMENT", "UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "specify" (→ specification), "requirements" (→ requirement),
    "describe" (→ describe)
  UI fires on: "screen" (→ screen), "ux" (→ ux)
  No IMPLEMENT terms present.
  Result: ["DOCUMENT", "UI"]
  Confidence: 0.92

---

## B06-0A-007-003 — UI lexicon reminder: form/interface/screen/page/component

```
source_id: xiigen-b06-0a-007-003
source_title: XIIGen B06 compiler — UI lexicon terms
source_license_state: first_party
pair_kind: compiler
```

Teaching note — UI predicate lexicon:

  Terms: ui, interface, ux, screen, page, form, component, layout

  "form" → UI fires (checkout form, address form, payment form)
  "interface" → UI fires (user interface, API interface — context helps router)
  "screen" → UI fires (payment screen, confirmation screen)
  "page" → UI fires (checkout page, cart page)
  "component" → UI fires (form component, button component)
  "layout" → UI fires (page layout, card layout)
  "ux" → UI fires (UX (User Experience) flow, UX review)
  "ui" → UI fires (UI (User Interface) spec, UI behavior)

  Note: "interface" fires UI. However when paired with purely technical signals
  (API (Application Programming Interface) interface, service interface) and no other
  UI terms, the router may score this lower for sub-phase 1.1 (UI) and higher for 2.2
  or 3.1. The compiler fires UI regardless.

  DOCUMENT+UI routing consequence:
    DOCUMENT → 2.1 (or 2.4 if stakeholder signals present)
    UI       → 1.1 (Screen Inventory and UX Requirements)

---

## B06-0A-007-004 — DOCUMENT+UI vs DOCUMENT+IMPLEMENT boundary

```
source_id: xiigen-b06-0a-007-004
source_title: XIIGen B06 compiler — DOCUMENT+UI vs DOCUMENT+IMPLEMENT comparison
source_license_state: first_party
pair_kind: compiler
```

Boundary comparison:

  Request: "Document the checkout form requirements"
    DOCUMENT fires: "document", "requirements"
    UI fires: "form"
    IMPLEMENT does NOT fire: no IMPLEMENT verb present
    Result: ["DOCUMENT", "UI"]

  Request: "Build the checkout form and document the requirements"
    DOCUMENT fires: "document", "requirements"
    UI fires: "form"
    IMPLEMENT fires: "build"
    Result: ["DOCUMENT", "IMPLEMENT", "UI"]

  Request: "Write the requirements for the checkout page"
    DOCUMENT fires: "requirements"
    UI fires: "page"
    IMPLEMENT fires: "write"
    Result: ["DOCUMENT", "IMPLEMENT", "UI"]

  Note: Adding "write" or "build" to any UI/DOCUMENT request
  promotes the result to include IMPLEMENT.
  The presence of UI terms does NOT suppress IMPLEMENT.
  The presence of IMPLEMENT does NOT suppress UI.
