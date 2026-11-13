# Layer 0-D — Flow Factory Pairs (013)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Stripe payment connector factory

---

## B17-0D-013-001 — StripePaymentConnector: probationary_accepted for 4.3

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3
CandidateFlow: IntegrationConnectorFlow43
CandidateStatus: probationary_accepted
ConnectorName: StripePaymentConnector
```

**Factory selection logic:**
1. IMPLEMENT → 4.3 → CandidateStatus: probationary_accepted → USE IntegrationConnectorFlow43
2. API_CONTRACT → 3.3 → No candidate → USE deterministic_generic: APIContractFlow33

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "probationary_accepted" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" }
]
TransitionType: feature_addition
```

---

## B17-0D-013-002 — StripePaymentConnector: probationary_rejected fallback

**Factory input:**
```
SubPhase: 4.3
CandidateFlow: StripePaymentConnectorFlow43-candidate
CandidateStatus: probationary_rejected
GateBReason: "N08 verdict absent; incomplete handler coverage"
FallbackFlow: IntegrationConnectorFlow43
```

**Factory output:**
```
SelectedFlowId: "IntegrationConnectorFlow43"
TrustState: "deterministic_generic"
FallbackUsed: true
RejectedCandidateFlowId: "StripePaymentConnectorFlow43-candidate"
SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"
```

---

## B17-0D-013-003 — Factory state transitions for Stripe connector

```
State: probationary (new candidate submitted)
  ↓ Gate B passes → probationary_accepted
  ↓ Sufficient evidence across instances → trusted_learned
  ↓ Gate B fails → probationary_rejected → fallback to deterministic_generic

Current state for StripePaymentConnector: probationary_accepted
Next threshold: trusted_learned (requires passing Gate B across ≥ 9 evidence instances)
```

---

## B17-0D-013-004 — Full factory metadata for connectors 013-024 (all probationary_accepted)

```
{
  "factoryId": "IntegrationConnectorFlow43",
  "candidateStatus": "probationary_accepted",
  "gateB": "PASS",
  "evidenceThreshold": "pending (not yet trusted_learned)",
  "replacesGeneric": true,
  "applicableConnectors": [
    "StripePaymentConnector", "MailgunEmailConnector", "FedExShippingConnector",
    "HubSpotCrmConnector", "SegmentAnalyticsConnector", "ZuoraBillingConnector",
    "OktaIdentityConnector", "IntercomMessagingConnector", "SquarePosConnector",
    "AvalaraTaxConnector"
  ],
  "subPhase": "4.3"
}
```

**All 10 new connectors use IntegrationConnectorFlow43 at probationary_accepted status.**
