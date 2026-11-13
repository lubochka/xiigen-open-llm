# Layer 1 — Transition Orchestrator Pairs (007)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-1-transition-orchestrator-pairs

---

## B32-L1-007-001 — Seven-predicate vibe session: all flows ordered; QA last

**Predicates:** `["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"]`

**Source request:** "Quick vibe session: architect the auth layer, integrate the OAuth provider, build the login service, secure the tokens, write unit tests, document the flow, and QA the implementation."

**Gap warnings:** gap-vibe-coding-style-001

**Transition orchestrator output:**
```
TransitionType: feature_addition
FlowOrder:
  Phase 1 (parallel): [ArchitectureFlow31, APIContractFlow]
  Phase 2 (sequential): [BackendServiceFlow41]
  Phase 3 (parallel): [SecurityFlow, TestFlow, DocumentFlow]
  Phase 4 (sequential): [QAFlow]
DomainId: "software-development"
GapMetadata: [gap-vibe-coding-style-001]
ContributionFlag: true
SessionState: active
```

---

## B32-L1-007-002 — QA always last: ordering constraint

**Rule:** QAFlow is always placed in the final execution phase. This is an invariant of the orchestrator, not specific to B32. QA requires all other flows (especially IMPLEMENT and TEST) to be complete before it can execute.

**Enforcement:** The orchestrator enforces this ordering even if the user requests "QA first" or vibe framing suggests informal ordering. The ordering constraint is not overridable by framing.

---

## B32-L1-007-003 — Vibe framing and execution ordering: no interaction

**The gap warning "vibe session" does not affect:**
1. Which flows are selected
2. What order flows execute in
3. Whether flows run in parallel or sequential
4. QA's position (always last)

The gap metadata is passed to each flow as context. The orchestration logic is completely unaffected by vibe vocabulary.

---

## B32-L1-007-004 — Seven-flow session: complete orchestration record

```
SessionRecord {
  scenarioId: "B32",
  requestText: "Quick vibe session: ...",
  predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"],
  gapWarnings: ["gap-vibe-coding-style-001"],
  transitionType: "feature_addition",
  domainId: "software-development",
  flowPhases: [
    {phase: 1, flows: ["ArchitectureFlow31", "APIContractFlow"], mode: "parallel"},
    {phase: 2, flows: ["BackendServiceFlow41"], mode: "sequential"},
    {phase: 3, flows: ["SecurityFlow", "TestFlow", "DocumentFlow"], mode: "parallel"},
    {phase: 4, flows: ["QAFlow"], mode: "sequential"}
  ],
  contributionFlag: true
}
```

---
