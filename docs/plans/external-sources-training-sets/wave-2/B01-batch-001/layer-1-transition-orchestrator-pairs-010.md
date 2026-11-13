# Layer 1 — Transition Orchestrator Pairs (010)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B01-L1-010-001 — Canonical B01 request patterns (UI-primary)

```
source_id: xiigen-b01-l1-010-001
source_title: XIIGen B01 layer1-010 — canonical B01 request patterns and transitions
source_license_state: first_party
pair_kind: transition_orchestrator
coverage_status: covered
```

Canonical B01 request patterns — UI-primary:

  Pattern 1: Screen inventory (original)
    Request: "Map all screen flows and create a complete screen inventory for the SaaS onboarding UX."
    Predicates: ["UI"] (screen, ux)
    Transition: feature_addition, SubPhaseId: "1.1"
    Flow: ScreenInventoryFlow11

  Pattern 2: Admin panel inventory
    Request: "Create an inventory of all pages in the admin interface."
    Predicates: ["UI"] (page, interface)
    Transition: feature_addition, SubPhaseId: "1.1"
    Flow: ScreenInventoryFlow11

  Pattern 3: Checkout form interactions
    Request: "Map the checkout form interactions and identify each input field."
    Predicates: ["UI"] (form, interaction, input)
    Transition: feature_addition, SubPhaseId: "1.1"
    Flow: ScreenInventoryFlow11

  Pattern 4: Dashboard with requirements
    Request: "Map the user dashboard interface screens and define the requirements."
    Predicates: ["UI", "DOCUMENT"] (interface, screen / define, requirement)
    Transition: feature_addition, SubPhaseId: "1.1"
    Flows: ScreenInventoryFlow11 + BusinessRequirementsFlow21

  Pattern 5: Accessibility + requirements
    Request: "Audit the accessibility of all screens and document the requirements."
    Predicates: ["UI", "DOCUMENT"] (accessibility, screen / document, requirement)
    Transition: feature_addition, SubPhaseId: "1.1"
    Flows: ScreenInventoryFlow11 + BusinessRequirementsFlow21

---

## B01-L1-010-002 — Canonical B01 request patterns (DOCUMENT-primary)

```
source_id: xiigen-b01-l1-010-002
source_title: XIIGen B01 layer1-010 — canonical B01 request patterns DOCUMENT-primary
source_license_state: first_party
pair_kind: transition_orchestrator
coverage_status: covered
```

Canonical B01 request patterns — DOCUMENT-primary:

  Pattern 6: Requirements only
    Request: "Describe the functional behavior of the onboarding wizard."
    Predicates: ["DOCUMENT"] (describe, functional, behavior)
    Transition: documentation, SubPhaseId: "2.1"
    Flow: BusinessRequirementsFlow21

  Pattern 7: Use-case narrative
    Request: "Document the use-cases for the admin role."
    Predicates: ["DOCUMENT"] (document, use-case)
    Transition: documentation, SubPhaseId: "2.1"
    Flow: BusinessRequirementsFlow21

  Pattern 8: Design system documentation
    Request: "Create the design system documentation — define typography, color, spacing."
    Predicates: ["UI", "DOCUMENT", "IMPLEMENT"] (design-system, typography, color, spacing / documentation, define / create)
    Transition: feature_addition, SubPhaseId: "1.1"
    Flows: ScreenInventoryFlow11 + BusinessRequirementsFlow21 + ImplementationFlow41

  Pattern 9: Accessibility specification
    Request: "Write the accessibility specification for all screens."
    Predicates: ["UI", "DOCUMENT", "IMPLEMENT"] (accessibility, screen / specification / write)
    Transition: feature_addition, SubPhaseId: "1.1"
    Flows: ScreenInventoryFlow11 + BusinessRequirementsFlow21 + ImplementationFlow41

---

## B01-L1-010-003 — Transition classification basis vocabulary

```
source_id: xiigen-b01-l1-010-003
source_title: XIIGen B01 layer1-010 — transition_classification_basis vocabulary
source_license_state: first_party
pair_kind: transition_orchestrator
coverage_status: covered
```

Vocabulary for transition_classification_basis in Metadata:

  Signal types:
    implement_signal: [term list] — terms from IMPLEMENT lexicon that fired
    ui_signal: [term list] — terms from UI lexicon that fired
    document_signal: [term list] — terms from DOCUMENT lexicon that fired
    architecture_signal: [term list] — ARCHITECTURE trigger terms

  Intent signals:
    add_new_capability_signal: [capability list] — what new capability is being added
    documentation_signal: [term list] — what documentation is being produced
    architecture_ui_signal: [term list] — combined design + UI terms

  Primary intent values:
    "create" — creating a new artifact
    "build" — implementing code
    "document" — writing documentation
    "design" — designing an architecture or UX

  Format: "signal_type: term1, term2; signal_type2: term3"
  Example: "implement_signal: write, implement; ui_signal: screen, responsive, layout"
