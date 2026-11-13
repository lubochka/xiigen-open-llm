# Layer 2 — Nine-Node 3.4 Pairs (010)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Sub-phase: 3.4 — Security and Authorization Model
# Flow: SecurityVerification34Flow
# Production date: 2026-05-05 | pair_kind: nine_node_flow

---

## SecurityDocumentation — Predicates: ["SECURITY", "DOCUMENT"] | TransitionType: documentation

Scenario: Document the JWT authentication design — 3.4 branch (trusted) + 2.1 branch (generic, parallel).
Source: "Document the JWT authentication security design including token lifecycle, tenant scoping, and key rotation policy."

### Node 01 — DomainSplitter
A: SelectedLogicalTransition: SecurityDocumentation (3.4 branch) | Predicates: ["SECURITY"]
B: DomainPacket[]: [
     DomainPacket(domain: "jwt-security-documentation", predicate: "SECURITY", priority: 1)
   ]
Note: DOCUMENT routes to 2.1 (RequirementsAuthoringFlow21, deterministic_generic, parallel branch).
  This nine-node trace covers the 3.4 SECURITY branch.
  TransitionType: documentation (DOCUMENT fires; no IMPLEMENT)

### Node 02 — DomainTranslator
A: DomainPacket(domain: "jwt-security-documentation", predicate: "SECURITY")
B: WorkBranch(
     branchId: "jwt-security-design-doc",
     workType: "security-design-documentation",
     securityDomain: "3.4",
     expectedOutput: "structured JWT security design document: token lifecycle, tenant scoping, key rotation"
   )

### Node 03 — SchemePlanner
A: WorkBranch[]: [jwt-security-design-doc]
B: ExecutionSchema(
     steps: [
       Step(id: 1, type: "document-token-lifecycle", branch: "jwt-security-design-doc"),
       Step(id: 2, type: "document-tenant-scoping-model", branch: "jwt-security-design-doc"),
       Step(id: 3, type: "document-key-rotation-policy", branch: "jwt-security-design-doc"),
       Step(id: 4, type: "document-threat-model", branch: "jwt-security-design-doc")
     ],
     parallelGroups: [[1, 2], [3, 4]]
   )

### Node 04 — DomainProcessor
A: ExecutionSchema step 1: document-token-lifecycle
B: ProcessedDomain(
     step: "document-token-lifecycle",
     result: "JWT lifecycle: access token 15min TTL; refresh token 30 days; rotation on refresh; revocation via blacklist",
     documentFormat: "Markdown section: Token Lifecycle",
     crossReference: "see ApiKeySecuritySchema for API key lifecycle; see JwtRefreshTokenFlow for refresh details"
   )

### Node 05 — Applier
A: ProcessedDomain(step: "document-token-lifecycle", status: "documented")
B: AppliedResult(
     applied: true,
     artifact: "JWT-Security-Design.md section 1: Token Lifecycle",
     parallelNote: "2.1 branch (RequirementsAuthoringFlow21) running in parallel producing requirements-layer documentation"
   )

### Node 06 — ResultTranslator
A: AppliedResult[]: [token-lifecycle-doc, tenant-scoping-doc, key-rotation-doc, threat-model-doc]
B: TranslatedResult(
     summary: "JWT security design documentation complete: token lifecycle, tenant scoping model, key rotation policy, threat model",
     documentedSections: ["Token Lifecycle", "Tenant Scoping Model", "Key Rotation Policy", "Threat Model"],
     crossLayerNote: "2.1 parallel branch producing higher-level requirements documentation"
   )

### Node 07 — ArbiterSelector
A: TranslatedResult[]: [security-documentation-result]
B: ArbiterReviewRequest(
     reviewType: "security-documentation-review",
     flowId: "SecurityDocumentation",
     reviewCriteria: ["lifecycle-completeness", "tenant-scoping-accuracy", "key-rotation-coverage", "threat-model-completeness"],
     confidence: 0.90
   )

### Node 08 — Arbiter
A: ArbiterReviewRequest(reviewType: "security-documentation-review", confidence: 0.90)
B: ArbiterDecision(
     verdict: "accepted",
     rationale: "All four sections documented with sufficient detail; cross-references provided for related flows",
     gateB: "passed — documentation completeness score: 4/4 sections",
     warnings: []
   )

### Node 09 — Formatter
A: ArbiterDecision[]: [SecurityDocumentation-decision]
B: FormattedResponse(
     response: "JWT security design documentation complete: Token Lifecycle, Tenant Scoping Model, Key Rotation Policy, Threat Model. Parallel 2.1 branch (generic) produces requirements-level documentation.",
     subPhase: "3.4",
     flowId: "SecurityVerification34Flow"
   ) | FactoryState: trusted_learned (3.4) | FactoryState_2.1: deterministic_generic (parallel)
