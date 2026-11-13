# Layer 3 — Teaching Unit Pairs (024)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B01-L3-024-001 — Complete B01 UI lexicon summary

```
source_id: xiigen-b01-l3-024-001
pair_kind: teaching_unit
```

A: What is the complete UI predicate lexicon for B01-batch-001?
B:
  UILexicon:
    - ui, interface, ux
    - screen, page, modal
    - form, layout, component
    - responsive, accessibility, visual
    - wireframe, prototype, interaction
    - button, input, typography
    - color, spacing, design-system, theme
  NotInUILexicon: "navigation, sidebar, flow, workflow, process, steps, optimize, checkout (alone)"
  KeyRule: "Only exact lexicon terms fire UI — domain context does not override the lexicon"

---

## B01-L3-024-002 — Complete B01 predicate rule summary

```
source_id: xiigen-b01-l3-024-002
pair_kind: teaching_unit
```

A: What are the complete predicate routing rules for B01-batch-001?
B:
  PredicateRules:
    UI: "fires on UI lexicon terms (see -001); TransitionType: documentation unless IMPLEMENT co-fires"
    DOCUMENT: "fires on: document, spec, specification, requirement, story, criteria, describe, define, functional, behavior, use-case, narrative, feature-brief, capability, epics; TransitionType: documentation unless IMPLEMENT co-fires"
    IMPLEMENT: "fires on: build, implement, create, scaffold, write (code), add (code), develop; TransitionType: feature_addition always"
    ARCHITECTURE: "fires on: architecture, system-design, service, API, database, schema, integration, infrastructure, pipeline, module, dependency"
  SpecialRules:
    - "'write user stories' → DOCUMENT+IMPLEMENT (both fire)"
    - "'navigation' does NOT fire UI"
    - "Empty CompiledPredicateIds → knowledge-edge path always"
    - "Three predicates → three independent RoutingDecisions (never merged)"
  TransitionTypes:
    - "feature_addition: when IMPLEMENT fires (alone or combined)"
    - "documentation: when UI and/or DOCUMENT fire without IMPLEMENT"

---

## B01-L3-024-003 — Complete B01 teaching summary

```
source_id: xiigen-b01-l3-024-003
pair_kind: teaching_unit
```

B01-batch-001 complete teaching summary (files 001-024):
  CoreLexiconLessons:
    - "navigation" NOT in UI lexicon → gap (files 001-003, reinforced in 021)
    - "sidebar" NOT in UI lexicon → gap (file 015)
    - "user experience" longform NOT UI — only "ux" fires UI (file 015)
    - "form" IS UI even in non-form contexts (file 016)
    - "logic" IS IMPLEMENT (files 016, 019)
    - "color" IS UI (file 017)
    - "input" IS UI (file 017)
    - "visual" IS UI (file 018)
    - "accessibility" IS UI (file 018)
    - "screen" IS UI (file 019)
    - "layout" IS UI (file 020)
    - "spacing" IS UI (file 015)
  RoutingLessons:
    - Empty predicates → KNOWLEDGE_EDGE always (files 013, 021, 022)
    - Multi-predicate → independent RoutingDecisions per predicate (files 016, 021-023)
    - v6-01: false UI detection corrected (files 013, 023)
    - v6-02: FallbackUsed suppressed corrected (files 014, 023)
    - v6-03: multi-branch collapse corrected (files 015, 023)
  FlowLessons:
    - ScreenInventoryFlow11: nine-node chain, UI-only scope
    - DomainSplitter: produces one packet per predicate — never merges
    - TransitionType: feature_addition when IMPLEMENT; documentation otherwise
  TotalFilesInBatch: "024" | TotalPairsInBatch: "744" | AllGatesPassed: "true"
