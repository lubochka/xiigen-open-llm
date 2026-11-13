# Layer 0-D — Flow Factory Pairs (014)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Mailgun email connector factory

---

## B17-0D-014-001 — MailgunEmailConnector: deterministic_generic for 4.3 (new domain)

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3
ConnectorName: MailgunEmailConnector
CandidateStatus: not_registered (no candidate submitted yet)
FallbackFlow: IntegrationConnectorFlow43
```

**Factory selection logic:**
1. IMPLEMENT → 4.3 → No candidate registered → USE deterministic_generic: IntegrationConnectorFlow43
2. API_CONTRACT → 3.3 → No candidate → USE deterministic_generic: APIContractFlow33

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" }
]
FallbackUsed: true (no probationary candidate exists)
SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
```

---

## B17-0D-014-002 — Multi-flow selection when SECURITY is present (email connector)

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST"]
SubPhase set: [4.3, 3.3, 3.4, 5.1]
ConnectorName: MailgunEmailConnector
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "3.4", flow: "SecurityAuditFlow34", status: "deterministic_generic" },
  { subPhase: "5.1", flow: "TestingFlow51", status: "deterministic_generic" }
]
```

---

## B17-0D-014-003 — API_CONTRACT alone: only APIContractFlow33 selected (no IMPLEMENT)

**Factory input:**
```
CompiledPredicateIds: ["API_CONTRACT"]
SubPhase: 3.3
Input: "Integrate Mailgun into the notification service."
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" }
]
IntegrationConnectorFlow43 NOT selected (requires IMPLEMENT)
```

**Factory rule:** IntegrationConnectorFlow43 (4.3) is only selected when IMPLEMENT is in the predicate set. API_CONTRACT alone selects only APIContractFlow33 (3.3).

---

## B17-0D-014-004 — Factory selection decision tree for email connectors

```
Input predicates arrive at factory
  ↓
IMPLEMENT present?
  ├─ Yes → 4.3 flow check:
  │   ├─ probationary_accepted candidate? → USE candidate (probationary_accepted)
  │   ├─ trusted_learned candidate? → USE candidate (trusted_learned)
  │   └─ No candidate (Mailgun case) → USE deterministic_generic IntegrationConnectorFlow43
  └─ No → Skip 4.3 entirely (Rule 10)

API_CONTRACT present?
  └─ Yes → USE deterministic_generic APIContractFlow33 (3.3)
```
