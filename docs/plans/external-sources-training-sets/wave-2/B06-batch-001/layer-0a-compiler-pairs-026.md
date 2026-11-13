# Layer 0-A — Compiler Pairs (026)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-026-001 — ReferralProgram: DOCUMENT alone

```
source_id: xiigen-b06-0a-026-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the business rules and use cases for a referral program including invite tracking, reward accrual, and fraud prevention."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "business" (→ business), "rules" context, "use cases" (→ use-case)
  Note: "business" fires DOCUMENT via Rule 7.
  Non-terms: "referral", "invite tracking", "reward accrual", "fraud prevention" — domain nouns
  Confidence: 0.93

---

## B06-0A-026-002 — ReferralProgram: IMPLEMENT alone

```
source_id: xiigen-b06-0a-026-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Build the referral tracking service with invite link generation and reward credit automation."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Build" (→ build)
  Non-terms: "referral tracking", "invite link", "generation", "reward credit", "automation"
  Confidence: 0.93

---

## B06-0A-026-003 — ReferralProgram: DOCUMENT + IMPLEMENT (dual)

```
source_id: xiigen-b06-0a-026-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the requirements for the referral program and develop the reward crediting module."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "requirements" (→ requirement)
    IMPLEMENT fires on: "Write" (→ write), "develop" (→ develop)
  TransitionType: feature_addition
  Confidence: 0.92

---

## B06-0A-026-004 — ReferralProgram: knowledge-edge

```
source_id: xiigen-b06-0a-026-004
source_license_state: first_party
pair_kind: compiler
```

A: "Track referrals and assign rewards to referring customers."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Track" and "assign" are operational verbs not in any predicate lexicon. No predicate fires.
  Confidence: 0.95

---

## B06-0A-026-005 — ReferralProgram: predicate boundary — "criteria"

```
source_id: xiigen-b06-0a-026-005
source_license_state: first_party
pair_kind: compiler
```

A: "Specify the eligibility criteria for referral program rewards."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Specify" (→ specify), "criteria" (→ criteria)
  Teaching: two DOCUMENT terms in one sentence — both fire; only one predicate emitted.
  Confidence: 0.94
