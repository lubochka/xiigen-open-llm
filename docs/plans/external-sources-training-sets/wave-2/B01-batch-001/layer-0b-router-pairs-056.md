# Layer 0-B — Router Pairs (056)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-056-001 — ColorPicker: UI+IMPLEMENT (triple-hit) routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-056-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Build a color picker component with hue slider, saturation field, and hex/RGB input fields."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

Note: "component", "color", "input" — three strong UI hits.

---

## B01-0B-056-002 — ColorPicker: UI+IMPLEMENT (four-hit) routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-056-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Create a color swatch palette widget for the design-system theme editor."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

Note: "widget", "color", "design-system", "theme" — four UI hits → still one routing decision.

---

## B01-0B-056-003 — ColorPicker: ARCHITECTURE only routes to 3.1

```
source_id: xiigen-b01-0b-056-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["ARCHITECTURE"]
   User request: "Define the color token schema for the design system."
B: DecisionCount: 1 | SubPhaseId: "3.1" | KnowledgeEdge: false

Note: "color" IS UI lexicon but "define ... schema" without UI action verb → UI does NOT fire; ARCHITECTURE fires on "schema".

---

## B01-0B-056-004 — ColorPicker: UI only (accessibility+color) routes to 1.1

```
source_id: xiigen-b01-0b-056-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI"]
   User request: "The color picker is not accessible to colorblind users — review the color contrast ratios."
B: DecisionCount: 1 | SubPhaseId: "1.1" | KnowledgeEdge: false

Note: "color" and "accessibility" (via "accessible") fire UI; "review" is not IMPLEMENT.
