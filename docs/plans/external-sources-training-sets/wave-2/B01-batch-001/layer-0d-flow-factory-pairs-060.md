# Layer 0-D — Flow Factory Selection Pairs (060)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-060-001 — Complete factory reference: four-predicate

```
source_id: xiigen-b01-0d-060-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build complete round-3 component library, document design system specification, define component API schemas, implement accessibility."
CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT", "ARCHITECTURE"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[2.1]: null → "universal-nine-node-development-flow-2.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[3.1]: null → "universal-nine-node-development-flow-3.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-060-002 — Round-3 factory state matrix summary

```
source_id: xiigen-b01-0d-060-r3-matrix
source_license_state: first_party
pair_kind: flow_factory
```

Round-3 (files 037–060) factory patterns confirmed:
  Sub-phase 1.1 (UI): screen-inventory-flow-1.1-v3 when trusted_learned
  Sub-phase 2.1 (DOCUMENT): universal-nine-node-development-flow-2.1 (no registered learned flow)
  Sub-phase 3.1 (ARCHITECTURE): universal-nine-node-development-flow-3.1 (no registered learned flow)
  Sub-phase 4.1 (IMPLEMENT): universal-nine-node-development-flow-4.1 (no registered learned flow)
  Sub-phase SEC.1 (SECURITY): universal-nine-node-development-flow-sec.1 (no registered learned flow)
  Sub-phase TEST.1 (TEST): universal-nine-node-development-flow-test.1 (no registered learned flow)
  Knowledge-edge: no factory invocation (CompiledPredicateIds: [])
  FallbackUsed summary: never true in round-3 (all deterministic_generic or trusted_learned; no probationary_rejected)

Key rule for round-3:
  Only sub-phase 1.1 has a registered trusted_learned flow.
  All other sub-phases use deterministic_generic fallback — FallbackUsed: false (not rejected; generic is default).
  FallbackUsed: true ONLY when a registered candidate was proposed and then rejected.
