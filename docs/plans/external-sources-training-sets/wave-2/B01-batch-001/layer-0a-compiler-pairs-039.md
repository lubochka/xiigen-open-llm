# Layer 0-A — Compiler Pairs (039)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-039-001 — SidebarNav: "create a sidebar navigation component" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-039-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Create a collapsible sidebar navigation component with nested menu items and active state indicators."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.91

Predicate evidence:
  UI fires on: "component" (navigation alone does NOT fire UI — confirmed gap)
  IMPLEMENT fires on: "create"
  "sidebar" NOT in UI lexicon (confirmed gap from round-2).
  "navigation" NOT in UI lexicon (confirmed gap from files 001-024).
  Fires only via "component".
  TransitionType: feature_addition (IMPLEMENT present).

---

## B01-0A-039-002 — SidebarNav: "sidebar navigation design" fires nothing

```
source_id: xiigen-b01-0a-039-002
source_license_state: first_party
pair_kind: compiler
transition_type: knowledge_edge
```

A:
  user_request: "Improve the sidebar navigation design for better usability and user flow clarity."
  CompiledPredicateIds: []
  Confidence: 0.44

Predicate evidence:
  "sidebar" NOT in UI lexicon (gap).
  "navigation" NOT in UI lexicon (gap).
  "usability" NOT in any predicate lexicon (gap).
  "flow" NOT in any predicate lexicon (gap).
  "improve" NOT in IMPLEMENT lexicon (gap verb).
  CompiledPredicateIds: [] → knowledge-edge path.

---

## B01-0A-039-003 — SidebarNav: "implement sidebar with responsive layout" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-039-003
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Implement the sidebar component with responsive layout breakpoints for mobile and tablet screens."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.94

Predicate evidence:
  UI fires on: "component", "responsive", "layout", "screen"
  IMPLEMENT fires on: "implement"
  Four UI lexicon hits in one request; confidence boosted.
  TransitionType: feature_addition.

---

## B01-0A-039-004 — SidebarNav: sidebar gap reinforcement

```
source_id: xiigen-b01-0a-039-004
source_license_state: first_party
pair_kind: compiler
transition_type: knowledge_edge
```

A:
  user_request: "Analyze the sidebar navigation patterns across competitor products and propose improvements."
  CompiledPredicateIds: []
  Confidence: 0.40

Predicate evidence:
  "analyze" NOT in any predicate lexicon.
  "sidebar" NOT in UI lexicon (gap — reinforced).
  "navigation patterns" NOT in any lexicon.
  "propose" NOT in any predicate lexicon.
  All four gap-reinforcement confirmations for "sidebar navigation" term cluster.
