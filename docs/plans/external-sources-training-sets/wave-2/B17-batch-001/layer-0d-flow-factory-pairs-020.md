# Layer 0-D — Flow Factory Pairs (020)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Intercom messaging connector factory

---

## B17-0D-020-001 — IntercomMessagingConnector: two-flow selection

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase set: [4.3, 3.3]
ConnectorName: IntercomMessagingConnector
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" }
]
TransitionType: feature_addition
```

---

## B17-0D-020-002 — IntercomMessagingConnector with SECURITY: three flows

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
Input: "Build the Intercom webhook connector and implement signature validation."
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "3.4", flow: "SecurityAuditFlow34", status: "deterministic_generic" }
]
```

---

## B17-0D-020-003 — IntercomMessagingConnector full delivery: four flows

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"]
SubPhase set: [4.3, 3.3, 5.1, 2.1]
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "5.1", flow: "TestingFlow51", status: "deterministic_generic" },
  { subPhase: "2.1", flow: "DocumentationFlow21", status: "deterministic_generic" }
]
```

---

## B17-0D-020-004 — Summary: all flow selections for connectors 013-024 (minimal case)

**Minimal case (["IMPLEMENT", "API_CONTRACT"]) for all 10 new connectors:**

| ConnectorName | 4.3 flow | 3.3 flow | Status |
|---|---|---|---|
| StripePaymentConnector | IntegrationConnectorFlow43 | APIContractFlow33 | deterministic_generic |
| MailgunEmailConnector | IntegrationConnectorFlow43 | APIContractFlow33 | deterministic_generic |
| FedExShippingConnector | IntegrationConnectorFlow43 | APIContractFlow33 | deterministic_generic |
| HubSpotCrmConnector | IntegrationConnectorFlow43 | APIContractFlow33 | deterministic_generic |
| SegmentAnalyticsConnector | IntegrationConnectorFlow43 | APIContractFlow33 | deterministic_generic |
| ZuoraBillingConnector | IntegrationConnectorFlow43 | APIContractFlow33 | deterministic_generic |
| OktaIdentityConnector | IntegrationConnectorFlow43 | APIContractFlow33 | deterministic_generic |
| IntercomMessagingConnector | IntegrationConnectorFlow43 | APIContractFlow33 | deterministic_generic |
| SquarePosConnector | IntegrationConnectorFlow43 | APIContractFlow33 | deterministic_generic |
| AvalaraTaxConnector | IntegrationConnectorFlow43 | APIContractFlow33 | deterministic_generic |

**All 10 new connectors use deterministic_generic status (no candidate registered yet).**
