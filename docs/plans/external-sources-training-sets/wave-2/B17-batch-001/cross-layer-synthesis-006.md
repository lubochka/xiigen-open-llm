# Cross-Layer Synthesis (006)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: cross-layer-synthesis

---

## B17-CLS-006 — API_CONTRACT ONLY: "Integrate the Avalara AvaTax API" (documentation trace, no connector code)

**Input:**
"Integrate the Avalara AvaTax API for sales tax calculation."

---

### Layer 0-A (Compiler)
```
Triggers detected:
- "Integrate" → API_CONTRACT ONLY (Rule 1; does NOT fire IMPLEMENT)
- "API" → API_CONTRACT (Rule 4; reinforces; fires once)
- "Integrate" is NOT a build verb; it is an integration verb

CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
DomainId: software-development
```

---

### Layer 0-B (Router)
```
Routing decisions:
1. API_CONTRACT → 3.3 (APIContractFlow33)

DecisionCount: 1
UniqueSubPhases: 1 (3.3 only)
```

---

### Layer 0-C (Knowledge Edge)
```
Edge assertions verified:
- "integrate" → API_CONTRACT only: ✓ (Rule 1)
- "integrate" → IMPLEMENT: ✗ (Rule 1 explicitly prohibits this)
- "API" → API_CONTRACT: ✓ (reinforces; fires once)
- API_CONTRACT alone → 3.3: ✓ (Rule 10)
- No GapHandler: ✓ (Rule 19)
- "Avalara" proper noun: no predicate fired ✓
- "AvaTax" product name: no predicate fired ✓
```

---

### Layer 0-D (Flow Factory)
```
Factory selection:
- API_CONTRACT → 3.3 → APIContractFlow33 [deterministic_generic]

SelectedFlows: [APIContractFlow33(3.3)]
TransitionType: documentation
```

---

### Layer 1 (Transition Orchestrator)
```
Orchestration plan:
Step 1: APIContractFlow33 (3.3) — document Avalara AvaTax API contract

No IMPLEMENT → no IntegrationConnectorFlow43 → no connector code generated
No TEST → orchestration ends after Step 1
No QA → no QAGatingFlow73
```

---

### Layer 2 (N/A — no 4.3 flow selected)
```
Sub-phase 4.3 NOT activated.
No nine-node flow runs.
APIContractFlow33 (3.3) produces documentation artifacts only:
- AvaTax API endpoint reference
- Request/response schema documentation
- Authentication notes (API key or OAuth — documented, not implemented)
- Integration guide outline

No connector files generated. No N08 gate.
```

---

### Layer 3 (Teaching)
```
Key lessons from this trace:
1. "integrate" is the most important single-word rule in the predicate system (Rule 1)
   — it fires API_CONTRACT ONLY, never IMPLEMENT, regardless of context
2. Adding "API" reinforces API_CONTRACT but does not change the outcome
3. The result is a documentation trace (TransitionType: documentation), not a build trace
4. Contrast with "Build the Avalara connector" → IMPLEMENT + API_CONTRACT → 4.3 + 3.3 (connector code)
5. Proper nouns ("Avalara", "AvaTax") and product category names never fire predicates
```

---

**Full trace summary:**
Compiler → ["API_CONTRACT"] → Router → [3.3] → Factory → [APIContractFlow33] → Orchestrator → Step1: 3.3 (documentation only) → No nine-node (4.3 not activated) → Teaching: "integrate" = API_CONTRACT ONLY; "build" = IMPLEMENT + API_CONTRACT.
