# Layer 0-D — Flow Factory Pairs (004)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B12-0D-004-001 — trusted_learned: 3.3 promoted flow for API_CONTRACT + DOCUMENT

```
source_id: xiigen-b12-0d-004-001
flow_type: trusted_learned
transition_type: documentation
phase: 3
sub_phase: 3.3
pair_kind: factory_selection
```

A:
  run_id: "B12-0D-004-001"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  sub_phase_hint: "3.3"
  flow_registry_query: "3.3"
  learned_flow_available: true

B:
  FlowType: "trusted_learned"
  SelectedFlow: "APIContractFlow33-learned-v1"
  SubPhase: "3.3"
  TransitionType: "documentation"
  TrustStatus: "trusted"
  GateBStatus: "PASS (historical)"
  ProductionUses: 15
  LastError: null
  FactoryReason: "trusted_learned flow for [API_CONTRACT, DOCUMENT] pattern. Promoted after
  Gate B pass + 15 production uses without error. Selected as highest priority flow."
  FlowOrder: ["APIContractFlow33-learned-v1", "DocumentationFlow21"]

Factory log:
  Phase 1: deterministic_generic used (no learned flow)
  Phase 2: probationary candidate registered → Gate B evaluated
  Phase 3: Gate B PASS → probationary_accepted status
  Phase 4: 15 production uses without error → promoted to trusted_learned
  Current: trusted_learned selected with highest priority

---

## B12-0D-004-002 — trusted_learned: what "trusted" means for the factory

```
source_id: xiigen-b12-0d-004-002
flow_type: trusted_learned
pair_kind: factory_selection
```

A trusted_learned flow:
  - Has a verified Gate B pass from probationary evaluation
  - Has a track record of correct classifications in production
  - Is selected first before any other flow type for its predicate pattern
  - Has lower uncertainty than probationary_accepted
  - Is periodically re-evaluated against new test cases to ensure no regression

The factory does not "lock in" a trusted_learned flow permanently:
  - If new B12 test cases reveal errors → re-evaluation triggered
  - If error rate rises above threshold → demoted back to probationary_accepted
  - deterministic_generic always retained as fallback

For B12 [API_CONTRACT, DOCUMENT] trusted_learned flow:
  Selected flow: APIContractFlow33-learned-v1
  Trust rationale: consistent correct routing for all five B12 contract modifier patterns
  (data contracts, service contracts, message contracts, event contracts, webhook contracts)

---

## B12-0D-004-003 — trusted_learned for different predicate patterns

```
source_id: xiigen-b12-0d-004-003
flow_type: trusted_learned
pair_kind: factory_selection
```

Different predicate patterns may have different trust statuses:

Pattern [API_CONTRACT, DOCUMENT]:
  Status: trusted_learned (most common pattern, well-trained)
  Flow: APIContractFlow33-learned-v1

Pattern [ARCHITECTURE, API_CONTRACT, DOCUMENT]:
  Status: probationary_accepted (less common, fewer training examples)
  Flow: ArchitectureAPIContractFlow31-33-candidate-v1

Pattern [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY, AUTHORIZATION]:
  Status: deterministic_generic (rare five-predicate pattern, no learned flow yet)
  Flow: ImplementationFlow43 + APIContractFlow33 + DocumentationFlow21 + SecurityAuditFlow34 + AuthorizationFlow34

The trust status is pattern-specific. Having a trusted flow for [API_CONTRACT, DOCUMENT]
does not mean all B12 patterns have trusted flows. New predicate combinations start at
deterministic_generic until enough training examples enable probationary candidacy.

---

## B12-0D-004-004 — trusted_learned output structure

```
source_id: xiigen-b12-0d-004-004
flow_type: trusted_learned
pair_kind: factory_selection
```

Trusted_learned factory output fields:

  FlowType: "trusted_learned"
  SelectedFlow: [flow_id] — the learned flow identifier
  SubPhase: "3.3" — routing destination
  TransitionType: "documentation" or "feature_addition"
  TrustStatus: "trusted" — confirmed trusted status
  GateBStatus: "PASS" — historical gate pass
  ProductionUses: [N] — number of production uses since last gate evaluation
  LastError: null or [error_description] — null when no recent errors
  FactoryReason: [explanation of why this flow was selected]
  FlowOrder: [ordered list of flows for this predicate combination]

Difference from probationary_accepted output:
  + TrustStatus field present (trusted vs provisional)
  + ProductionUses higher (>10 for trusted status)
  + LastError always null for currently trusted flows (errors trigger re-evaluation)
