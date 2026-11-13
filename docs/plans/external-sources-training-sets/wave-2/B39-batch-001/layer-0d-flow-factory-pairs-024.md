# Layer 0-D — Flow Factory Pairs (024)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F024-001 — Complete flow reference: which FlowId corresponds to which predicate+sub-phase

A: Which FlowId is selected for each predicate/sub-phase in B39?
B:
  TEST → 5.3 → E2ETestingFlow53
  TEST → 5.1 → UnitIntegrationTestingFlow51
  UI → 1.5 → UIInteractionFlow15
  QA → 7.3 → QAGatingFlow73
  QA → 7.1 → ReleaseGateFlow71
  IMPLEMENT → 4.1 → BackendServiceFlow41
  DOCUMENT → 6.2 → DocumentationFlow62

---

## F024-002 — Complete flow reference: TransitionType rules

A: What TransitionType does each predicate combination produce?
B:
  IMPLEMENT present → feature_addition (IMPLEMENT overrides all)
  TEST+UI (no IMPLEMENT) → documentation
  TEST alone → documentation
  UI alone → documentation
  QA alone → no contribution (QA does not set TransitionType)
  TEST+UI+QA (no IMPLEMENT) → documentation
  IMPLEMENT+TEST+UI+QA → feature_addition

---

## F024-003 — Complete flow reference: ordering rule for multi-flow execution

A: When TEST+UI+QA all fire, in what order do the flows execute?
B: Ordering: IMPLEMENT (4.1) → TEST+UI (parallel: 5.3 and 1.5 execute in parallel) → QA (7.3 always last).
QA gate validates all preceding flow outputs before signaling completion.
Rule: QA is always the final validator; it never precedes TEST or IMPLEMENT flows.

---

## F024-004 — Complete flow reference: gap vocabulary passthrough

A: When gap vocabulary is detected, does the CapabilityGapSignal block flow instantiation?
B: Yes — when zero predicates fire: knowledge_edge_synthetic; no FlowInstances created; CapabilityGapSignal forwarded to AutonomousLearningPipeline (subproject 16); SCHEDULE_CAPABILITY_LEARNING emitted.
When gap vocabulary co-occurs with valid predicates: gap warning is a passthrough annotation; flows instantiate normally for the valid predicates; CapabilityGapWarningPassthrough: true.

---

## F024-005 — Complete flow reference: TrustState values in B39

A: What are the valid TrustState values and when does each apply in B39?
B:
  deterministic_generic — all standard predicate-to-flow mappings; all flows in B39 with at least one predicate.
  knowledge_edge_synthetic — zero predicates; no flow instantiated.
  probationary — flow instantiated but under extra scrutiny (used for novel or ambiguous predicate combinations in B39-013 to 024 edge cases).
