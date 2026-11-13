# Layer 0-D — Flow Factory Pairs (015)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: FedEx/UPS shipping connector factory

---

## B17-0D-015-001 — FedExShippingConnector: deterministic_generic (no candidate registered)

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3
ConnectorName: FedExShippingConnector
CandidateStatus: not_registered
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" }
]
SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
```

---

## B17-0D-015-002 — UpsShippingConnector: identical factory selection to FedEx

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3
ConnectorName: UpsShippingConnector
CandidateStatus: not_registered
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" }
]
```

**Note:** Different connector name, identical factory selection. The factory selects flows based on sub-phases, not connector names.

---

## B17-0D-015-003 — FedExShippingConnector OAuth variant: four flows selected

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
SubPhase set: [4.3, 3.3, 3.4]
ConnectorName: FedExShippingConnector (OAuth variant)
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "3.4", flow: "SecurityAuditFlow34", status: "deterministic_generic" },
  { subPhase: "3.4", flow: "AuthorizationFlow34", status: "deterministic_generic" }
]
```

**Note:** Two flows for sub-phase 3.4 (SecurityAuditFlow34 and AuthorizationFlow34) are both selected — one per predicate (SECURITY and AUTHORIZATION). This is correct (Rule 9).

---

## B17-0D-015-004 — Flow status comparison: shipping connector vs original Stripe

| ConnectorName | 4.3 flow status | 3.3 flow status |
|---|---|---|
| StripeConnector (original, file .md) | probationary_accepted | deterministic_generic |
| PayPalConnector (002) | probationary_accepted | deterministic_generic |
| FedExShippingConnector (015) | deterministic_generic | deterministic_generic |
| UpsShippingConnector (015) | deterministic_generic | deterministic_generic |

**The original and early connectors have probationary_accepted status for IntegrationConnectorFlow43. New connectors (013-024) start at deterministic_generic (not_registered). They will advance to probationary when a candidate is submitted and passes Gate B.**
