# Layer 0-B — Router Pairs (039)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-039-001 — SidebarNav: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-039-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Create a collapsible sidebar navigation component with nested menu items."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

Note: fires via "component" only — "sidebar" and "navigation" are both gap terms.

---

## B01-0B-039-002 — SidebarNav: double-gap knowledge-edge

```
source_id: xiigen-b01-0b-039-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: []
   User request: "Improve the sidebar navigation design for better usability."
B: DecisionCount: 1 | SubPhaseId: null | KnowledgeEdge: true | FailureClassification: "no_predicate_match"

Note: "sidebar" + "navigation" + "usability" are all gap terms — triple gap reinforcement.

---

## B01-0B-039-003 — SidebarNav: four-UI-hit request: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-039-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Implement the sidebar component with responsive layout breakpoints for mobile screens."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

Note: "component", "responsive", "layout", "screen" all fire UI — four hits, one predicate.

---

## B01-0B-039-004 — SidebarNav: routing rule — multiple UI hits still produce one routing decision

```
source_id: xiigen-b01-0b-039-r3-rule
source_license_state: first_party
pair_kind: router
```

Rule confirmed in file 039:
  Multiple UI lexicon hits in one request → still one RoutingDecision for UI.
  Confidence may be higher (more evidence) but SubPhaseId remains "1.1" — never duplicated.
  "sidebar" alone never routes; "navigation" alone never routes.
  Only co-occurring UI lexicon terms enable UI routing for sidebar/navigation requests.
