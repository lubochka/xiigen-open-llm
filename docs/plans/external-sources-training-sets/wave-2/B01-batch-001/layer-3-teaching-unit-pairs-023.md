# Layer 3 — Teaching Unit Pairs (023)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B01-L3-023-001 — v6 corruption detection: type v6-01 (knowledge-edge missed)

```
source_id: xiigen-b01-l3-023-001
pair_kind: teaching_unit
```

A: Identify the corruption in this routing output:
   Input: "optimize the checkout workflow"
   CompiledPredicateIds: ["UI"]
   RoutingDecisions: [{ SubPhaseId: "1.1", FlowId: "ScreenInventoryFlow11" }]
B:
  CorruptionType: "v6-01"
  Explanation: "optimize the checkout workflow" — "optimize", "checkout", "workflow" are not UI predicate terms. CompiledPredicateIds should be [] (empty). Empty predicates → knowledge-edge path; routing to ScreenInventoryFlow11 is incorrect. Correct output: KNOWLEDGE_EDGE verdict with 6 Metrics keys, no RoutingDecisions array.
  CorrectCompiledPredicateIds: []
  CorrectVerdict: "KNOWLEDGE_EDGE"

---

## B01-L3-023-002 — v6 corruption detection: type v6-02 (flow fallback suppressed)

```
source_id: xiigen-b01-l3-023-002
pair_kind: teaching_unit
```

A: Identify the corruption in this factory output:
   Input: "design a responsive product grid"
   TrustState: "probationary_rejected"
   SelectionOutcome: "ScreenInventoryFlow11 selected — FallbackUsed: false"
B:
  CorruptionType: "v6-02"
  Explanation: probationary_rejected trust state must report FallbackUsed: true. The candidate flow was proposed but rejected — the fallback mechanism fired. Reporting FallbackUsed: false suppresses evidence of fallback use. Correct SelectionOutcome must include FallbackUsed: true for any probationary_rejected state.
  CorrectFallbackUsed: "true"
  CorrectTrustState: "probationary_rejected"

---

## B01-L3-023-003 — v6 corruption detection: type v6-03 (multi-branch collapse)

```
source_id: xiigen-b01-l3-023-003
pair_kind: teaching_unit
```

A: Identify the corruption in this routing output:
   Input: "build and document the user profile screen"
   CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT"]
   RoutingDecisions: [{ SubPhaseId: "1.1", FlowId: "ScreenInventoryFlow11", PredicateIds: ["UI", "IMPLEMENT", "DOCUMENT"] }]
B:
  CorruptionType: "v6-03"
  Explanation: Three predicates (UI (user interface), IMPLEMENT, DOCUMENT) must produce three independent RoutingDecisions — never merged into a single decision. The single RoutingDecision with all three PredicateIds is a multi-branch collapse. Correct output: three separate RoutingDecisions, one per predicate, with independent SubPhaseId and FlowId assignments.
  CorrectDecisionCount: 3
  CorrectStructure: "one RoutingDecision per CompiledPredicateId — never merged"
