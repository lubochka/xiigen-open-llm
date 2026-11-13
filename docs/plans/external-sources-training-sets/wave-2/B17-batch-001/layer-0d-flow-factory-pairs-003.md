# Layer 0-D — Flow Factory Pairs (003)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: flow-factory

---

## B17-0D-003-001 — trusted_learned: 4.3 (promoted after evidence threshold)

**Scenario:** TwilioSmsConnector — flow has been probationary_accepted across multiple instances and has accumulated sufficient evidence to promote.

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3
CandidateFlow: IntegrationConnectorFlow43
CandidateStatus: trusted_learned
ConnectorName: TwilioSmsConnector
EvidenceCount: 12 (threshold: 10)
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "trusted_learned" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "trusted_learned" }
]
TransitionType: feature_addition
```

---

## B17-0D-003-002 — trusted_learned: what promotion means

**Promotion path:**
probationary_accepted (Gate B pass, single instance)
→ [evidence accumulation across multiple instances]
→ trusted_learned (Gate B pass, evidence count ≥ threshold)

**trusted_learned status:**
- Flow is fully integrated into the factory
- No longer requires Gate B check on each invocation (pre-validated)
- Replaces deterministic_generic permanently for this predicate combination
- Factory confidence: high

**Evidence threshold:** Configurable per scenario. B17 default: 10 instances passing Gate B.

---

## B17-0D-003-003 — trusted_learned factory record

```
{
  "factoryId": "IntegrationConnectorFlow43-Twilio",
  "candidateStatus": "trusted_learned",
  "gateB": "PASS (pre-validated)",
  "evidenceCount": 12,
  "evidenceThreshold": 10,
  "replacesGeneric": true,
  "promotedFrom": "probationary_accepted",
  "connectorName": "TwilioSmsConnector",
  "subPhase": "4.3"
}
```

---

## B17-0D-003-004 — State machine: probationary_accepted → trusted_learned

```
new candidate submitted
  ↓
Gate B evaluation
  ├─ FAIL → probationary_rejected → fallback to deterministic_generic
  └─ PASS → probationary_accepted
              ↓ (per-instance evidence accumulation)
              evidenceCount >= evidenceThreshold?
              ├─ No → remains probationary_accepted
              └─ Yes → trusted_learned (permanent promotion)
```

**Demotion:** trusted_learned can be demoted to probationary_accepted if a new Gate B failure occurs after promotion (rare but possible in production scenarios).
