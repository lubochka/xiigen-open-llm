# Layer 2 — Nine-Node Flow Pairs (023)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: nine_node_flow (probationary_rejected — QA service quality)

## B15-nine-023 — ProbationaryRejected (IMPLEMENT+QA service quality flow rejected)

Request: "Implement the order quality assurance service and QA the service before deploying to staging."

### Flow: ImplementationFlow41 (primary) + QAGatingFlow71

**Node 1 — DomainSplitterUnit**: Target: OrderQualityService (feature_addition) | A = SelectedLogicalTransition: TransitionType: feature_addition, SubPhaseIds: ["4.1", "7.1"], PredicateIds: ["IMPLEMENT", "QA"] | ImplementationFlow41 runs first; QAGatingFlow71 runs last (absolute ordering).

**Node 2 — DomainTranslatorUnit**: Gate B: service must implement order quality checks; QA gate must verify service quality before staging deploy.

**Node 3 — SchemePlannerUnit**: Candidate flow: "order-qa-service-flow-4.1-v1" (probationary) | Gate B check: gate_b_passed: false | Reason: "N04 DomainProcessorUnit not covered in candidate flow; only 5 of 9 nodes have evidence pairs" | Fallback: universal-nine-node-development-flow-4.1 selected.

**Node 4 — DomainProcessorUnit**: Runs on universal fallback flow. OrderQualityService: IQualityService.RunChecks(OrderId) → OperationResult<QualityReport>; BR-QA-001: quality checks must run before order is released for fulfillment.

**Node 5 — ApplierUnit**: Files generated: IQualityService.cs, OrderQualityService.cs, QualityReport.cs, QualityCheckRegistry.cs.

**Node 6 — ResultTranslatorUnit**: 4 files, 2 interfaces, 1 business rule. Implementation complete. QA gate pending.

**Node 7 — ArbiterSelectorUnit**: QAGatingFlow71 triggered: QAGateArbiter. ReviewFocus: service passes minimum quality bar before staging deploy.

**Node 8 — ArbiterUnit**: QA verdict: PASS (quality checks implemented; staging deploy approved).
Implementation verdict: PASS.
Probationary rejection note: "order-qa-service-flow-4.1-v1" was rejected (Gate B failed — N04 not covered); fallback used — this is the REJECTED_PROBATIONARY_FALLBACK_USED outcome.

**Node 9 — FormatterUnit**: Verdict: IMPLEMENT_APPROVED (via fallback) | QA gate: STAGING_DEPLOY_APPROVED | TransitionType: feature_addition | ProbationaryRejected: order-qa-service-flow-4.1-v1 (N04 not covered)
