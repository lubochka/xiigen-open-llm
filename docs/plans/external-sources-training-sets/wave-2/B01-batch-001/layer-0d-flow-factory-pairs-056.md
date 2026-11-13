# Layer 0-D — Flow Factory Selection Pairs (056)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-056-001 — ColorPicker: UI+IMPLEMENT (triple-hit) factory

```
source_id: xiigen-b01-0d-056-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build a color picker component with hue slider, saturation field, and hex/RGB input fields."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

Note: "component" + "color" + "input" — three UI hits → one Factory[1.1].

---

## B01-0D-056-002 — ColorPicker: ARCHITECTURE only factory

```
source_id: xiigen-b01-0d-056-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Define the color token schema for the design system."
CompiledPredicateIds: ["ARCHITECTURE"]

Factory[3.1]: null → "universal-nine-node-development-flow-3.1" | DETERMINISTIC_GENERIC_SELECTED
FallbackUsed: false
TransitionType: documentation

Note: "color" is UI lexicon term but ARCHITECTURE fires via "schema" in schema-definition context.
UI factory (1.1) NOT invoked — no UI action verb present.

---

## B01-0D-056-003 — ColorPicker: UI only (accessibility+color) factory

```
source_id: xiigen-b01-0d-056-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "The color picker is not accessible to colorblind users — review the color contrast ratios."
CompiledPredicateIds: ["UI"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
FallbackUsed: false
TransitionType: documentation

Note: "color" + "accessibility" fire UI. "review" is gap verb — IMPLEMENT does NOT fire.
