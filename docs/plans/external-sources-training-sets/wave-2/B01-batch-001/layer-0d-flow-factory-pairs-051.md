# Layer 0-D — Flow Factory Selection Pairs (051)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-051-001 — RichTextEditor: UI+IMPLEMENT factory

```
source_id: xiigen-b01-0d-051-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Implement a rich text editor component with formatting toolbar and markdown export."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-051-002 — RichTextEditor: DOCUMENT+IMPLEMENT factory

```
source_id: xiigen-b01-0d-051-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Write the feature specification for the rich text editor."
CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]

Factory[2.1]: null → "universal-nine-node-development-flow-2.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-051-003 — RichTextEditor: UI only (via "ux" in casual phrasing)

```
source_id: xiigen-b01-0d-051-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "The editor feels clunky — fix the UX."
CompiledPredicateIds: ["UI"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
FallbackUsed: false
TransitionType: documentation

Key: "ux" fires UI even in informal/complaint phrasing. Factory[1.1] invoked normally.
"fix" is gap verb — IMPLEMENT does NOT fire, so Factory[4.1] is NOT invoked.
