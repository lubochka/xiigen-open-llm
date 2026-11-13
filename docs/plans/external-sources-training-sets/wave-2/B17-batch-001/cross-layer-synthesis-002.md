# Cross-Layer Synthesis (002)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: cross-layer-synthesis

---

## B17-CLS-002 — API_CONTRACT alone: "Integrate the Slack API for notifications" (3.3 path, not 4.3)

**Input:**
"Integrate the Slack messaging API for sending channel notifications."

**The critical distinction trace: integrate alone → 3.3, NOT 4.3.**

---

### Layer 0-A (Compiler)
```
Triggers detected:
- "Integrate" → API_CONTRACT (Rule 1: integrate fires API_CONTRACT ONLY)
- "API" → API_CONTRACT (reinforces)
- No build verb present → IMPLEMENT does NOT fire

CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation (Rule 17: API_CONTRACT alone → documentation)
DomainId: software-development
```

---

### Layer 0-B (Router)
```
Routing decisions:
1. API_CONTRACT → 3.3 (APIContractFlow33)
   [NOT 4.3 — Rule 10: API_CONTRACT alone → 3.3]

DecisionCount: 1
UniqueSubPhases: 1 (3.3 only)

Nearest wrong: routing API_CONTRACT to 4.3 (v6-03 variant)
```

---

### Layer 0-C (Knowledge Edge)
```
Edge assertions verified:
- "integrate" → API_CONTRACT: ✓ (Rule 1)
- "integrate" → NOT IMPLEMENT: ✓ (Rule 1 — absolute)
- API_CONTRACT alone → 3.3: ✓ (Rule 10)
- API_CONTRACT alone → NOT 4.3: ✓
- TransitionType: documentation for API_CONTRACT alone: ✓ (Rule 17)
```

---

### Layer 0-D (Flow Factory)
```
Factory selection:
- API_CONTRACT → 3.3 → APIContractFlow33 [deterministic_generic]
- IntegrationConnectorFlow43 NOT selected (requires IMPLEMENT)

SelectedFlows: [APIContractFlow33(3.3)]
DecisionCount: 1
TransitionType: documentation
```

---

### Layer 1 (Transition Orchestrator)
```
Orchestration plan:
Step 1: APIContractFlow33 (3.3) — generate Slack API contract specification

Single flow, no dependencies, no QA gate.
Work product: API specification document (not code)
```

---

### Layer 2 (Nine-Node — not 4.3 this time)
```
Note: IntegrationConnectorFlow43 (nine-node 4.3) does NOT run in this scenario.
Only APIContractFlow33 (3.3) runs.

APIContractFlow33 output:
- Slack API endpoint list
- Message posting schema
- Channel notification contract
- (No connector code generated)
```

---

### Layer 3 (Teaching)
```
Key lessons from this trace:
1. "integrate" alone → API_CONTRACT only → 3.3 → documentation
2. 4.3 is NOT activated without IMPLEMENT
3. DecisionCount is 1 (minimum possible for a single predicate)
4. No code is produced (TransitionType: documentation)
5. The most common v6-01 error is routing this case to 4.3 — this trace shows the correct 3.3 outcome

Contrast with synthesis-001:
- synthesis-001: ["IMPLEMENT", "API_CONTRACT"] → 4.3 + 3.3 → code produced
- synthesis-002: ["API_CONTRACT"] → 3.3 only → specification produced
Single word difference ("integrate" vs "build + integrate") changes the entire pipeline.
```

---

**Full trace summary:**
Compiler → ["API_CONTRACT"] (integrate=API_CONTRACT, no build verb=no IMPLEMENT) → Router → [3.3] → Factory → [APIContractFlow33] → Orchestrator → Step1: 3.3 only → No nine-node 4.3 → Teaching: integrate alone is documentation, not implementation.
