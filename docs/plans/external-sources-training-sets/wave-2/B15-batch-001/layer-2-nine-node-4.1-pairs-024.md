# Layer 2 — Nine-Node Flow Pairs (024)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: nine_node_flow (rule summary)

## B15-nine-024 — Complete nine-node rule summary for sub-phase 4.1

### Node reference

| Node | Unit | Contract | Role |
|------|------|----------|------|
| N01 | DomainSplitterUnit | SelectedLogicalTransition → DomainPacket[] | Splits transition into domain packets by predicate |
| N02 | DomainTranslatorUnit | DomainPacket → TranslatedDomainStatement | Translates packet into actionable technical statement |
| N03 | SchemePlannerUnit | TranslatedDomainStatement[] → WorkBranch[] | Plans work branches; Gate B feasibility check |
| N04 | DomainProcessorUnit | WorkBranch → ProcessedBranchResult | Processes each branch; derives business rules |
| N05 | ApplierUnit | ProcessedBranchResult → AppliedBranchResult | Applies changes (generates files, registers rules) |
| N06 | ResultTranslatorUnit | AppliedBranchResult[] → TranslatedResultFragment[] | Assembles implementation summary and test surface |
| N07 | ArbiterSelectorUnit | TranslatedResultFragment[] → ArbiterReviewRequest[] | Selects appropriate arbiters for review |
| N08 | ArbiterUnit | ArbiterReviewRequest → ArbiterVerdict | Performs completeness check; emits PASS/FAIL |
| N09 | FormatterUnit | ReviewedResultSet → HumanResult | Formats output; sets final verdict |

### Key 4.1 rules

1. Node 1 A field = SelectedLogicalTransition (not raw user request).
2. Node 3 probationary_accepted → candidate flow executes; probationary_rejected → fallback executes.
3. TransitionType: refactoring → nodes 1-4 operate in refactoring mode (no business logic deletion check, backward-compatibility required).
4. IMPLEMENT+ARCHITECTURE → ArchitectureFlow31 (3.1) nodes execute before ImplementationFlow41 (4.1) nodes; architecture output is N01 input for implementation phase.
5. OperationResult<T> confirmed in N08 for every public method — arbiter fails if any public method lacks OperationResult<T>.
6. Fabric-first confirmed in N08 — arbiter fails if concrete type is constructed inside service (new X() for injectable dependencies).
7. QA is always at N07/N08 of QAGatingFlow71 — always the last flow in any multi-predicate pipeline.
