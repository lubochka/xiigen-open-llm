# Layer 2 — Nine-Node 3.4 Pairs (012)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: nine_node

---

## O1b-2-012-001 — Nine-node rule: Node 01 A-side is always SelectedLogicalTransition

A: What is the A-side input type for Node 01 (DomainSplitterUnit)?

B: Node 01 A-side MUST be SelectedLogicalTransition — never a raw user prompt string. SelectedLogicalTransition is the output of the predicate compiler and router. It contains: predicates[], transition_type, routing decisions, and the semantic intent. The raw user prompt is consumed by the compiler to produce SelectedLogicalTransition. By the time Node 01 runs, the raw string has been replaced by the structured SelectedLogicalTransition.

---

## O1b-2-012-002 — Nine-node rule: node contract types

A: List all nine canonical node types and their contracts

B: Node 01: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
   Node 02: DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>
   Node 03: SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>
   Node 04: DomainProcessorUnit : ILogicalUnit<WorkBranch, ProcessedBranchResult>
   Node 05: ApplierUnit : ILogicalUnit<ProcessedBranchResult, AppliedBranchResult>
   Node 06: ResultTranslatorUnit : ILogicalUnit<AppliedBranchResult[], TranslatedResultFragment[]>
   Node 07: ArbiterSelectorUnit : ILogicalUnit<TranslatedResultFragment[], ArbiterReviewRequest[]>
   Node 08: ArbiterUnit : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>
   Node 09: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>

---

## O1b-2-012-003 — Nine-node rule: Node 09 A-side is ReviewedResultSet (not ArbiterVerdict[])

A: What is the A-side input to Node 09 (FormatterUnit)?

B: Node 09 A-side: ReviewedResultSet — a structured set containing all ArbiterVerdict results plus the assembled result fragments. NOT a raw ArbiterVerdict[]. ReviewedResultSet is produced by the orchestrator after collecting all ArbiterUnit outputs. FormatterUnit converts ReviewedResultSet into the final HumanResult.

---

## O1b-2-012-004 — Nine-node rule: sub-phase 3.4 scenarios covered in O1b

A: What nine-node scenarios are covered in O1b Layer 2?

B: 11 scenarios: (base) Full client credentials flow. (002) ClientCredentialsFlow. (003) TenantScopeEnforcement. (004) ServiceAccountSetup. (005) TokenIntrospectionEndpoint. (006) ScopeManagementTests. (007) OAuthClientRegistration. (008) TenantGrantRevoke. (009) ProbationaryRejected (5.1 fallback). (010) OAuthDocumentation. (011) FullOAuthSuite (6 predicates).

---

## O1b-2-012-005 — Nine-node rule: ArbiterUnit reviews one request, returns one verdict

A: Node 08 processing model

B: Node 08 (ArbiterUnit) processes ONE ArbiterReviewRequest and returns ONE ArbiterVerdict. It does not batch. Node 07 (ArbiterSelectorUnit) distributes review requests — it may produce multiple ArbiterReviewRequest objects. The orchestrator calls ArbiterUnit once per request. All verdicts are collected into ReviewedResultSet before Node 09 runs.
