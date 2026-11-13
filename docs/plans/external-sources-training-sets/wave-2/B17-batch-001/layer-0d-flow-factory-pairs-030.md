# Layer 0-D — Flow Factory Pairs (030)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Algolia search connector factory

---

## B17-0D-030-001 — AlgoliaConnector factory: base case

Input predicates: [IMPLEMENT, API_CONTRACT]

Factory output:
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" }
]
TransitionType: feature_addition
FallbackUsed: false
```

---

## B17-0D-030-002 — Algolia factory: IMPLEMENT to 4.1 (no integration vocabulary)

Input predicates: [IMPLEMENT] (from "Implement Algolia faceted search")

Factory output:
```
SelectedFlows: [
  { subPhase: "4.1", flow: "BackendServiceFlow41", status: "deterministic_generic" }
]
TransitionType: feature_addition
FallbackUsed: false
```

BackendServiceFlow41 selected. No API_CONTRACT selected because no trigger for it was present.

---

## B17-0D-030-003 — Algolia factory: with token

Input predicates: [IMPLEMENT, API_CONTRACT, SECURITY]

Factory output:
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "3.4", flow: "SecurityAuditFlow34", status: "deterministic_generic" }
]
TransitionType: feature_addition
FallbackUsed: false
```

---

## B17-0D-030-004 — Algolia anti-patterns

| Anti-pattern | Correction |
|---|---|
| Selecting IntegrationConnectorFlow43 for [API_CONTRACT] only | 4.3 requires IMPLEMENT |
| Selecting IntegrationConnectorFlow43 when proper noun present but no integration vocab | Proper noun ≠ integration vocab; no 4.3 without structural integration vocabulary |
| Inventing GapHandler when no predicates fire | No GapHandler exists in B17 |
