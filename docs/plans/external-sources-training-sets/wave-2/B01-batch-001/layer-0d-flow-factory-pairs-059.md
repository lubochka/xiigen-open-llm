# Layer 0-D — Flow Factory Selection Pairs (059)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-059-001 — SocialShare: UI+IMPLEMENT factory

```
source_id: xiigen-b01-0d-059-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build a social share component with Twitter, LinkedIn, and copy-to-clipboard button variants."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-059-002 — SocialShare: ARCHITECTURE+DOCUMENT factory

```
source_id: xiigen-b01-0d-059-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Document the social sharing API integration including authentication tokens and rate limits."
CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]

Factory[3.1]: null → "universal-nine-node-development-flow-3.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[2.1]: null → "universal-nine-node-development-flow-2.1" | DETERMINISTIC_GENERIC_SELECTED

Note: No UI factory (1.1) — API documentation context has no UI terms.
TransitionType: documentation (IMPLEMENT absent).

---

## B01-0D-059-003 — SocialShare: knowledge-edge — no factory

```
source_id: xiigen-b01-0d-059-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "The sharing button went viral and broke our API rate limit — scale the service to handle the spike."
CompiledPredicateIds: []

Factory: not invoked.
SelectionOutcome: "KNOWLEDGE_EDGE_NO_FACTORY"
