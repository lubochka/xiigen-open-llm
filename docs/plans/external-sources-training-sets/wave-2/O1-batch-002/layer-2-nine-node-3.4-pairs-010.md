# Layer 2 — Nine-Node 3.4 Pairs (010)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: nine_node
# Scenario: OAuthDocumentation — SECURITY+DOCUMENT — document the OAuth2 flow and scopes
# Flows: SecurityVerification34Flow (trusted_learned) + DocumentationFlow21 (generic 2.1)

---

## O1b-2-010-N01 — Node 01: DomainSplitterUnit

A: SelectedLogicalTransition: { predicates: ["SECURITY", "DOCUMENT"], transition_type: "documentation", user_intent: "document the OAuth 2.0 authorization flow and available tenant scopes for the developer portal", flows: ["SecurityVerification34Flow", "DocumentationFlow21"] }

B: DomainPacket[]: [{ domain: "SECURITY", fragment: "OAuth 2.0 client credentials flow documentation, JWT structure" }, { domain: "DOCUMENT", fragment: "developer portal documentation, scope catalog, integration guide" }]
   Unit: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>

---

## O1b-2-010-N02 — Node 02: DomainTranslatorUnit

A: DomainPacket: { domain: "DOCUMENT", fragment: "developer portal documentation" }

B: TranslatedDomainStatement: { domain: "DOCUMENT", statement: "Produce: (1) OAuth 2.0 client credentials flow diagram (request → token → API call). (2) Scope catalog listing all available scopes with description and required tenant permission. (3) Integration guide: curl (command-line URL tool) examples for token request and authenticated API call. (4) Error response reference." }

---

## O1b-2-010-N03 — Node 03: SchemePlannerUnit

A: TranslatedDomainStatement[]: [SECURITY: JWT structure doc, DOCUMENT: developer portal content]

B: WorkBranch[]: [{ branch_id: "WB-01", focus: "FlowDiagram" }, { branch_id: "WB-02", focus: "ScopeCatalog" }, { branch_id: "WB-03", focus: "IntegrationGuide" }]

---

## O1b-2-010-N04 — Node 04: DomainProcessorUnit

A: WorkBranch: { branch_id: "WB-02", focus: "ScopeCatalog" }

B: ProcessedBranchResult: { branch_id: "WB-02", decisions: [{ decision: "List all scopes: read:data, write:data, read:reports, admin:tenant. For each: name, description, required tenant tier, example use case." }, { decision: "Mark deprecated scopes as such." }] }

---

## O1b-2-010-N05 — Node 05: ApplierUnit

A: ProcessedBranchResult: { branch_id: "WB-02", decisions: ["Scope catalog"] }

B: AppliedBranchResult: { branch_id: "WB-02", applied: { component: "ScopeCatalogDocument", spec: "Markdown table: | Scope | Description | Required Tenant Tier | Deprecated? |. Covers: read:data, write:data, read:reports, admin:tenant." } }

---

## O1b-2-010-N06 — Node 06: ResultTranslatorUnit

A: AppliedBranchResult[]: [FlowDiagram, ScopeCatalog, IntegrationGuide]

B: TranslatedResultFragment[]: [{ summary: "FlowDiagram: client_credentials grant sequence diagram (POST /token → JWT → API)" }, { summary: "ScopeCatalog: 4 scopes with descriptions and tenant tier requirements" }, { summary: "IntegrationGuide: curl examples for token request and API call with Bearer header" }]

---

## O1b-2-010-N07 — Node 07: ArbiterSelectorUnit

A: TranslatedResultFragment[]: [FlowDiagram, ScopeCatalog, IntegrationGuide]

B: ArbiterReviewRequest[]: [{ arbiter: "SecurityArbiter", focus: "Documentation must not expose secrets or internal signing keys" }, { arbiter: "DocumentationArbiter", focus: "Completeness of integration guide" }]

---

## O1b-2-010-N08 — Node 08: ArbiterUnit

A: ArbiterReviewRequest: { arbiter: "SecurityArbiter", focus: "No secrets in docs" }

B: ArbiterVerdict: { verdict: "approved", findings: ["curl examples must use placeholder values for client_id and client_secret", "JWT examples must use sample tokens (not real production tokens)", "Never document actual signing keys"], gate_b: "PASSED — documentation is security-safe" }

---

## O1b-2-010-N09 — Node 09: FormatterUnit

A: ReviewedResultSet: { all verdicts: approved, flows: ["SecurityVerification34Flow", "DocumentationFlow21"] }

B: HumanResult: "OAuthDocumentation: (1) Client credentials flow sequence diagram. (2) Scope catalog: read:data, write:data, read:reports, admin:tenant. (3) Integration guide with curl examples (placeholder credentials). Security review passed — no real secrets in documentation."
   Unit: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
