# Layer 2 — Nine-Node 3.4 Pairs (012)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Sub-phase: 3.4 — Security and Authorization Model
# Flow: SecurityVerification34Flow
# Production date: 2026-05-05 | pair_kind: nine_node_flow

---

## Nine-Node Rule Summary (3.4)

Reference pair — rules governing the nine-node chain for sub-phase 3.4 in O1.

### Node 01 — DomainSplitter rules

- Input: SelectedLogicalTransition (one per flow branch)
- Output: DomainPacket[] (one packet per domain area within the predicates)
- SECURITY and AUTHORIZATION share the 3.4 flow; each gets its own DomainPacket
- Packets have explicit priority order (1 = first)
- Source: the compiler+router+factory+orchestrator chain output

### Node 02 — DomainTranslator rules

- Input: one DomainPacket
- Output: one WorkBranch
- WorkBranch specifies: branchId, workType, securityDomain, expectedOutput
- Never invent workType names not derivable from the predicate and domain

### Node 03 — SchemePlanner rules

- Input: WorkBranch[] (all branches for this flow)
- Output: ExecutionSchema with steps and parallelGroups
- Steps are atomic units of work; parallelGroups indicate which can run concurrently
- At least one step per WorkBranch

### Node 04 — DomainProcessor rules

- Input: one ExecutionSchema step
- Output: ProcessedDomain with result, evidence, and status
- Each call processes exactly one step
- Status values: "defined", "validated", "documented", "passed", "failed"

### Node 05 — Applier rules

- Input: ProcessedDomain
- Output: AppliedResult with applied (bool), artifact, and scope notes
- Applied: true means the domain result was applied to the system model
- Artifact names the concrete registration (middleware, service, store, etc.)

### Node 06 — ResultTranslator rules

- Input: AppliedResult[] (all applied results from this flow)
- Output: TranslatedResult with summary, findings by category
- Summary is human-readable; findings are structured
- Cross-layer notes (other branches, factory states) go in this node

### Node 07 — ArbiterSelector rules

- Input: TranslatedResult[]
- Output: ArbiterReviewRequest with reviewType, criteria, confidence
- Confidence reflects the overall quality signal from prior nodes
- ReviewCriteria must be specific (not "works correctly")

### Node 08 — Arbiter rules

- Input: ArbiterReviewRequest
- Output: ArbiterDecision with verdict (accepted/rejected), rationale, gateB, warnings
- Verdict accepted if all criteria met; rejected if any critical criterion fails
- GateB assertion must be a specific comparison (not vague)
- Warnings are non-blocking observations

### Node 09 — Formatter rules

- Input: ArbiterDecision[]
- Output: FormattedResponse with response text, subPhase, flowId, FactoryState
- Response is the final user-facing output
- FactoryState reflects the flow's trust level
- For SecurityVerification34Flow: FactoryState is always trusted_learned when passing
- When other branches have different states, note them (e.g., FactoryState_5.1: probationary_rejected)
