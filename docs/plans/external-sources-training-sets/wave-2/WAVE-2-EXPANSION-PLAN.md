# Wave 2 — 1000-Per-Layer Expansion Plan (v3 — full scope)
# Date: 2026-05-05
# Authority documents:
#   Sub-phase definitions: EXECUTOR-PROMPT-REGISTER-ALL-31-SUB-PHASES.md
#   Source registry:       EXTERNAL-SOURCE-REGISTRY-v6.md
#   Pipeline stages:       EXTERNAL-SOURCES-ORCHESTRATION-PIPELINE-PLAN-v6.md
#   Canonical contracts:   DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v2.md

---

## Goal

1000 high-quality A/B pairs on each of Layers 0-A, 0-B, 0-C, 0-D, 1, 2, 3.

Coverage must span all 17 task areas defined for Wave 2 and all 31
sub-phases from the EXECUTOR-PROMPT.

Wave 2 goal: full orchestrator coverage for ALL development areas and task
types in scope — UI/UX, functional specs, project planning, project managing,
project delivery, development (documentation, from scratch, integration,
refactoring, new features, vibe coding, AI agent adaptation, skills/guidance),
QA, QA automation, testing, E2E, QA UI automation, DevOps.

Production strategy: breadth-first. Establish representative pairs for every
area before deepening any single area. The orchestrator must be able to route
the full breadth of development work after Wave 2 — no single sub-phase is
prioritised above achieving that breadth.

---

## Scope — 17 task areas

| # | Task area | Nature |
|---|---|---|
| 1 | UI/UX | Sub-phases 1.1-1.5 |
| 2 | Functional specs | Sub-phases 2.1-2.4 |
| 3 | Project planning | Predicate gap — knowledge-edge pairs |
| 4 | Project managing | Predicate gap — knowledge-edge pairs |
| 5 | Project delivery | Sub-phase 7.1 |
| 6 | Development — documentation | Sub-phases 2.1, 3.3 |
| 7 | Development — from scratch | Sub-phases 4.1-4.2 |
| 8 | Development — integration | Sub-phase 4.3 |
| 9 | Development — refactoring | Sub-phases 4.1, 4.4 |
| 10 | Development — new features | Sub-phases 4.1-4.2 |
| 11 | Development — vibe coding | Partial gap — routes to 4.2 via IMPLEMENT context; "vibe" itself is a capability gap |
| 12 | Development — AI agent adaptation | Partial gap — routes to 3.1/4.1 via ARCHITECTURE/IMPLEMENT; "agent"/"skill" are capability gaps |
| 13 | Development — skills and guidance | Gap — routes by topic vocabulary only |
| 14 | QA | Sub-phases 6.1-6.4 |
| 15 | QA automation | Sub-phases 7.1-7.4 |
| 16 | Testing (unit, integration, E2E, performance) | Sub-phases 5.1-5.4 |
| 17 | QA UI automation and DevOps | Sub-phases 1.5, 3.5, 5.3, 7.1-7.3 |

---

## The 31 sub-phases (EXECUTOR-PROMPT authority)

| ID | Name | Phase name | Predicate IDs |
|---|---|---|---|
| 1.1 | User Flows and Screen Inventory | UI and UX Definition | UI |
| 1.2 | Wireframes and Layout Design | UI and UX Definition | UI |
| 1.3 | Interaction Design and Prototypes | UI and UX Definition | UI |
| 1.4 | Design System and Component Library | UI and UX Definition | UI |
| 1.5 | Accessibility and Responsive Design | UI and UX Definition | UI |
| 2.1 | Business Requirements and Use Cases | Requirements and Business Rules | DOCUMENT |
| 2.2 | Data Model and Domain Objects | Requirements and Business Rules | DOCUMENT, ARCHITECTURE |
| 2.3 | Workflow and State Machine Design | Requirements and Business Rules | DOCUMENT, ARCHITECTURE |
| 2.4 | External Stakeholder Requirements | Requirements and Business Rules | DOCUMENT |
| 3.1 | Architecture and Component Topology | Technical Design | ARCHITECTURE |
| 3.2 | Database and Storage Design | Technical Design | ARCHITECTURE |
| 3.3 | API Contract Definition | Technical Design | API_CONTRACT |
| 3.4 | Security and Authorization Model | Technical Design | SECURITY, AUTHORIZATION |
| 3.5 | Infrastructure and Deployment Topology | Technical Design | ARCHITECTURE |
| 4.1 | Backend Services and Business Logic | Implementation | IMPLEMENT |
| 4.2 | Frontend Implementation | Implementation | IMPLEMENT, UI |
| 4.3 | Integration and Provider Connectors | Implementation | IMPLEMENT, API_CONTRACT |
| 4.4 | Data Migration and Seeding | Implementation | IMPLEMENT |
| 4.5 | Configuration and Feature Flags | Implementation | IMPLEMENT |
| 5.1 | Unit Tests | Testing | TEST |
| 5.2 | Integration Tests | Testing | TEST |
| 5.3 | End-to-End Tests | Testing | TEST |
| 5.4 | Performance and Load Tests | Testing | TEST, QA |
| 6.1 | Architecture Review | Review | ARCHITECTURE, QA |
| 6.2 | Regression Coverage Review | Review | TEST, QA |
| 6.3 | Security and Compliance Review | Review | SECURITY, QA |
| 6.4 | Code Review and Quality Gate | Review | QA |
| 7.1 | Release and Deployment Automation | Automation and Maintenance | QA |
| 7.2 | Monitoring and Observability | Automation and Maintenance | QA |
| 7.3 | CI/CD Gate Configuration | Automation and Maintenance | QA, TEST |
| 7.4 | Coverage and Reporting | Automation and Maintenance | QA, TEST |

---

## Predicate lexicons (routing authority)

| Predicate | Matching terms |
|---|---|
| SECURITY | jwt, jws, jwe, authentication, auth, oauth, oidc, openid, token, session, asvs, mfa, 2fa, rbac, security, credential, password, hash, encrypt, tls, ssl, certificate, signing, signature, secret, key, cipher |
| AUTHORIZATION | authorize, authorization, tenant, permission, role, access-control, scope, privilege, claim, policy, acl, entitlement, rbac, principal, grant, revoke |
| API_CONTRACT | api, endpoint, request, response, schema, contract, openapi, swagger, rest, restful, graphql, grpc, route, payload, spec, interface, operation, webhook, idempotent |
| ARCHITECTURE | architecture, component, topology, dependency, service, module, layer, pattern, system, infrastructure, microservice, monolith, distributed, event, queue, cache, boundary, coupling, cohesion, scalability |
| IMPLEMENT | implement, code, build, develop, write, create, function, class, logic, library, handler, middleware, controller, backend, feature, method, refactor |
| TEST | test, unit, integration, end-to-end, e2e, spec, assertion, coverage, mock, stub, fixture, verify, scenario, regression, suite, harness, case |
| QA | qa, quality, acceptance, release, validate, validation, compliance, audit, review, sign-off, readiness, approval, gate, checklist, inspection |
| UI | ui, interface, ux, component, layout, responsive, accessibility, visual, theme, typography, button, form, screen, page, modal, input, font, color, spacing, design-system, wireframe, prototype, interaction |
| DOCUMENT | document, spec, specification, requirement, story, criteria, describe, define, functional, business, capability, behavior, use-case, narrative, feature-brief, epics, stakeholder |

Note: "wireframe" and "prototype" and "interaction" added to UI — they appear
in sub-phase purposes 1.2 and 1.3 and must be routable.

Capability gaps — terms with NO current predicate:
  sprint, roadmap, milestone, backlog, velocity, standup, retrospective (→ project planning)
  progress, track, coordinate, status-report, manage (→ project managing)
  vibe, exploratory, rapid-prototype (→ vibe coding)
  agent, skill, orchestrator-adaptation (→ AI agent adaptation)
  teach, learn, guide, explain, mentor (→ skills/guidance)

These terms produce knowledge-edge results with FailureClassification:
  "no_predicate_match_capability_gap" and named gap IDs in EvidenceIds.

---

## Canonical nine-node flow node names

All Layer 2 pairs MUST use these exact names. Nodes 02-08 in the existing
layer-2-nine-node-3.4-pairs.md file are incorrect and require correction.

| Node | Contract |
|---|---|
| Node 01 | DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]> |
| Node 02 | DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement> |
| Node 03 | SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]> |
| Node 04 | DomainProcessorUnit : ILogicalUnit<WorkBranch, ProcessedBranchResult> |
| Node 05 | ApplierUnit : ILogicalUnit<ProcessedBranchResult, AppliedBranchResult> |
| Node 06 | ResultTranslatorUnit : ILogicalUnit<AppliedBranchResult[], TranslatedResultFragment[]> |
| Node 07 | ArbiterSelectorUnit : ILogicalUnit<TranslatedResultFragment[], ArbiterReviewRequest[]> |
| Node 08 | ArbiterUnit : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict> |
| Node 09 | FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult> |

Node 01 A side is always SelectedLogicalTransition — never a raw user prompt string.
Node 02 processes one DomainPacket at a time (array-node contract).
Node 03 receives ALL TranslatedDomainStatements together and produces WorkBranch[].
Node 04 and Node 05 process one branch at a time.
Node 06 receives AppliedBranchResult[] and may fold results.
Node 08 output fields: pass, concern, reprocess, reject verdict.

---

## Batch map — 42 batches

Each batch produces:
  Layer 0-A: ~24 compiler pairs (A = LogicalOrchestratorRequest, B = SubPhaseRouterSourceDocument)
  Layer 0-B: ~24 router pairs (mirroring 0-A outputs)
  Layer 0-C: ~5 knowledge-edge detection pairs
  Layer 0-D: ~5 factory selection pairs
  Layer 1:   ~5 transition orchestrator pairs
  Layer 2:   9 pairs (one nine-node pass for the primary sub-phase)
  Layer 3:   ~5 teaching unit pairs

For gap-area batches (B32-B37), Layer 2 is replaced by additional Layer 0-C
knowledge-edge pairs, since gap areas have no registered sub-phase flow.

42 batches × 24 Layer 0-A pairs = 1008 ≈ 1000 ✓

### GROUP 1 — UI/UX (Batches B01–B05)

| Batch | Scenario title | Primary sub-phase | Predicates | Task area |
|---|---|---|---|---|
| B01 | SaaS onboarding user flow and screen mapping | 1.1 | UI | UI/UX |
| B02 | E-commerce dashboard wireframes and layout | 1.2 | UI | UI/UX |
| B03 | Mobile checkout interaction design and prototype | 1.3 | UI | UI/UX |
| B04 | Enterprise component library and design tokens | 1.4 | UI | UI/UX |
| B05 | Web platform accessibility and responsive design | 1.5 | UI | UI/UX |

### GROUP 2 — Functional Specs / Requirements (Batches B06–B09)

| Batch | Scenario title | Primary sub-phase | Predicates | Task area |
|---|---|---|---|---|
| B06 | Checkout flow business requirements and use cases | 2.1 | DOCUMENT | Functional specs |
| B07 | User domain data model and entity relationships | 2.2 | DOCUMENT, ARCHITECTURE | Functional specs |
| B08 | Order approval workflow and state machine design | 2.3 | DOCUMENT, ARCHITECTURE | Functional specs |
| B09 | Partner API and regulatory stakeholder requirements | 2.4 | DOCUMENT | Functional specs |

### GROUP 3 — Technical Design (Batches B10–B14)

| Batch | Scenario title | Primary sub-phase | Predicates | Task area |
|---|---|---|---|---|
| B10 | Microservices component topology for payments | 3.1 | ARCHITECTURE | Dev — architecture |
| B11 | Database schema and storage design for inventory | 3.2 | ARCHITECTURE | Dev — architecture |
| B12 | REST and GraphQL API contract definition | 3.3 | API_CONTRACT | Dev — documentation |
| B13 | OAuth 2.0 and multi-tenant JWT security model | 3.4 | SECURITY, AUTHORIZATION | Dev — security |
| B14 | Cloud infrastructure and deployment topology | 3.5 | ARCHITECTURE | DevOps |

### GROUP 4 — Development / Implementation (Batches B15–B19)

| Batch | Scenario title | Primary sub-phase | Predicates | Task area |
|---|---|---|---|---|
| B15 | Backend order processing service from scratch | 4.1 | IMPLEMENT | Dev — from scratch |
| B16 | Frontend checkout UI implementation | 4.2 | IMPLEMENT, UI | Dev — new features |
| B17 | Third-party payment provider connector integration | 4.3 | IMPLEMENT, API_CONTRACT | Dev — integration |
| B18 | Customer data migration and seeding scripts | 4.4 | IMPLEMENT | Dev — refactoring |
| B19 | Feature flag management and config system | 4.5 | IMPLEMENT | Dev — new features |

### GROUP 5 — Testing (Batches B20–B23)

| Batch | Scenario title | Primary sub-phase | Predicates | Task area |
|---|---|---|---|---|
| B20 | Unit test design for billing calculation | 5.1 | TEST | Testing |
| B21 | Integration tests for API and database layers | 5.2 | TEST | Testing |
| B22 | End-to-end scenario testing for checkout flow | 5.3 | TEST | E2E / Testing |
| B23 | Performance and load tests for payment endpoints | 5.4 | TEST, QA | Testing |

### GROUP 6 — QA and Review (Batches B24–B27)

| Batch | Scenario title | Primary sub-phase | Predicates | Task area |
|---|---|---|---|---|
| B24 | Architecture review before v2.0 release | 6.1 | ARCHITECTURE, QA | QA |
| B25 | Regression coverage review after library upgrade | 6.2 | TEST, QA | QA |
| B26 | Security and GDPR compliance review | 6.3 | SECURITY, QA | QA |
| B27 | Code review and quality gate enforcement | 6.4 | QA | QA |

### GROUP 7 — QA Automation and DevOps (Batches B28–B31)

| Batch | Scenario title | Primary sub-phase | Predicates | Task area |
|---|---|---|---|---|
| B28 | Release pipeline and deployment automation | 7.1 | QA | QA automation / Project delivery |
| B29 | Monitoring setup and observability configuration | 7.2 | QA | QA automation / DevOps |
| B30 | CI/CD gate and build pipeline configuration | 7.3 | QA, TEST | QA automation / DevOps |
| B31 | Test coverage reporting and audit dashboard | 7.4 | QA, TEST | QA automation |

### GROUP 8 — Extended Development Task Types (Batches B32–B35)

These batches cover development task types from the user's scope that
do not have dedicated sub-phases or use sub-phases in non-obvious ways.

| Batch | Scenario title | Primary sub-phase | Predicates | Task area |
|---|---|---|---|---|
| B32 | Vibe coding — exploratory AI-assisted prototyping | 4.2 (via IMPLEMENT+UI) | IMPLEMENT, UI — plus gap pairs for "vibe" | Dev — vibe coding |
| B33 | AI agent adaptation — orchestrator and skill evolution | 3.1 (via ARCHITECTURE) | ARCHITECTURE, IMPLEMENT — plus gap pairs for "agent"/"skill" | Dev — AI agent adaptation |
| B34 | Developer skills and technical guidance requests | varies by topic | varies — plus gap pairs for "teach"/"guide"/"explain" | Dev — skills / guidance |
| B35 | Development documentation — inline docs, API docs, READMEs | 2.1, 3.3 | DOCUMENT, API_CONTRACT | Dev — documentation |

### GROUP 9 — Project Planning, Managing, and Delivery (Batches B36–B38)

| Batch | Scenario title | Primary sub-phase | Predicates | Task area |
|---|---|---|---|---|
| B36 | Project planning — sprints, roadmaps, and milestones | knowledge-edge (gap) | none — gap pairs for "sprint"/"roadmap"/"milestone" | Project planning |
| B37 | Project managing — progress, coordination, status reporting | knowledge-edge (gap) | none — gap pairs for "track"/"manage"/"status" | Project managing |
| B38 | Project delivery — release gates, go-live checklists, handoff | 7.1 | QA — plus gap pairs for "delivery"/"handoff" | Project delivery |

### GROUP 10 — QA UI Automation (Batch B39)

| Batch | Scenario title | Primary sub-phases | Predicates | Task area |
|---|---|---|---|---|
| B39 | QA UI automation — browser tests, accessibility checks, UI regression | 5.3, 7.3, 1.5 | TEST, UI, QA | QA UI automation |

### GROUP 11 — Cross-Cutting (Batches B40–B42)

| Batch | Purpose | Layer 2 | Task area |
|---|---|---|---|
| B40 | Multi-predicate sweep (all 9 predicates × 5 phrasings + 5 boundary cases) | none | All |
| B41 | Capability-gap knowledge-edge sweep (all 5 gap vocabularies, varied prompts) | none | All gap areas |
| B42 | Corruption and held-out cases (including 3 v6 corruption types) | none | All |

---

## Per-layer pair target

### Layer 0-A — compiler pairs (1000 target)

  Batches B01-B39: 39 batches × 24 pairs = 936
  Batches B40-B42: 3 batches × 21 pairs avg = 63
  Total: 999 ≈ 1000 ✓

  Predicate distribution across all batches:
  UI:            B01-B05, B16, B32, B39, B40 = 9 batch appearances ✓
  DOCUMENT:      B06-B09, B35, B40           = 6 batch appearances ✓
  ARCHITECTURE:  B10-B11, B14, B24, B33, B40 = 6 batch appearances ✓
  API_CONTRACT:  B12, B17, B35, B40          = 4 batch appearances ✓
  SECURITY:      B13, B26, B40               = 3 + O1 existing = 4 ✓
  AUTHORIZATION: B13, B40                    = 2 + O1 existing = 3 (natural ceiling: only 3.4 owns this) ✓
  IMPLEMENT:     B15-B19, B32, B33, B34, B40 = 9 batch appearances ✓
  TEST:          B20-B23, B25, B30, B31, B39, B40 = 10 batch appearances ✓
  QA:            B23-B31, B38-B40            = 12 batch appearances (natural: Phases 6-7 are QA-owned) ✓

  Gap vocabulary batches (no predicate fires, knowledge-edge output):
  B36 (sprint/roadmap/milestone): 24 gap pairs
  B37 (track/manage/status):      24 gap pairs
  B41 (all gap sweeps):           21 gap pairs
  Total gap pairs: 69 — these become Layer 0-C knowledge-edge training material

### Layer 0-B — router pairs (1000 target)

  One router pair per compiler pair. Exactly mirrors Layer 0-A coverage.
  Total: 1000 ✓

### Layer 0-C — knowledge-edge detection pairs (1000 target)

  Normal-flow pairs (compiler matched predicates → registered sub-phase): ~700
  Knowledge-edge pairs (no predicate or unregistered sub-phase):           ~200
  Partial-registration (sub-phase registered, no flow yet):                ~100
  Total: 1000 ✓

  Knowledge-edge pairs come from:
  — Gap batches B32-B34, B36-B37, B41 (capability-gap vocabulary)
  — Every non-3.4 sub-phase that has no registered flow yet (all 30)

### Layer 0-D — flow factory selection pairs (1000 target)

  Four trust states, 250 pairs each:
  trusted_learned:        250 (3.4 is the only trusted learned sub-phase currently)
  deterministic_generic:  250 (all 30 remaining sub-phases use UniversalNineNodeDevelopmentFlow)
  probationary_rejected:  250 (candidate flows that fail Gate B)
  probationary_accepted:  250 (candidate flows that pass Gate B but are not yet promoted)
  Total: 1000 ✓

### Layer 1 — transition orchestrator pairs (1000 target)

  feature_addition:  ~420 (adding new capability, new feature, from-scratch work)
  repair:            ~200 (fixing defects, "broken", "failing", "wrong behavior")
  refactor:          ~200 (restructuring, improving, cleaning up existing code)
  documentation:     ~80  (writing docs, specs, requirements — no implementation)
  unknown/ambiguous: ~100 (insufficient signal for classification)
  Total: 1000 ✓

  Transition types map to task areas:
  feature_addition → dev from-scratch, dev new-features, dev integration
  repair           → dev refactoring (when defect-driven), vibe coding (exploratory fix)
  refactor         → dev refactoring (improvement-driven), dev from-scratch (restructure)
  documentation    → functional specs, dev documentation task area
  unknown          → project planning, project managing, skills, guidance

### Layer 2 — nine-node flow pairs (1000 target)

  Node contracts (all correct canonical names):
  01 DomainSplitterUnit, 02 DomainTranslatorUnit, 03 SchemePlannerUnit,
  04 DomainProcessorUnit, 05 ApplierUnit, 06 ResultTranslatorUnit,
  07 ArbiterSelectorUnit, 08 ArbiterUnit, 09 FormatterUnit

  Sub-phase coverage per round (9 pairs per sub-phase per scenario pass):

  Round 1 (batches B01-B31 + B32, B33, B35, B38, B39):
    35 sub-phase passes × 9 nodes = 315 Layer 2 pairs

  Round 2 (second scenario context for each sub-phase):
    31 sub-phase passes × 9 nodes = 279 Layer 2 pairs

  Round 3 (third scenario context for priority sub-phases):
    ~45 passes × 9 nodes = 405 Layer 2 pairs (prioritizing 3.4, 6.3, 3.1, 5.2)

  Total: 315 + 279 + 405 = 999 ≈ 1000 ✓

  Every sub-phase receives at least 2 scenario passes (≥18 Layer 2 pairs).
  Priority sub-phases (3.4, 6.3, 3.1, 5.2) receive 3+ passes (≥27 pairs).

### Layer 3 — teaching unit pairs (1000 target)

  PairParserUnit variations:         300
  FeatureExtractionUnit variations:  150
  ModelTrainingUnit variations:      250
  HeldOutEvaluationUnit variations:  200
  CorrectionLearningUnit variations: 100
  Total: 1000 ✓

---

## Corrections applied to existing pairs

  File: layer-2-nine-node-3.4-pairs.md (O1-batch-001) — CORRECTED 2026-05-05
  All nine nodes now use canonical registry names and correct contract type chain:
    DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>
    SchemePlannerUnit    : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>
    DomainProcessorUnit  : ILogicalUnit<WorkBranch, ProcessedBranchResult>
    ApplierUnit          : ILogicalUnit<ProcessedBranchResult, AppliedBranchResult>
    ResultTranslatorUnit : ILogicalUnit<AppliedBranchResult[], TranslatedResultFragment[]>
    ArbiterSelectorUnit  : ILogicalUnit<TranslatedResultFragment[], ArbiterReviewRequest[]>
    ArbiterUnit          : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>
    FormatterUnit        : ILogicalUnit<ReviewedResultSet, HumanResult>
  MANIFEST.md also updated to reflect corrected node name (DomainTranslatorUnit).

---

## Production order — round-by-round

Strategy: BREADTH-FIRST. Each round covers one representative batch from each
major task-area group before any group gets a second pass. Quality gate: zero
rejected pairs before advancing to the next round.

  Round 1 — BREADTH SWEEP (one batch per task area, all layers):
    B01  UI/UX                    — user flows and screen mapping          (Group 1)
    B06  Functional specs         — checkout business requirements         (Group 2)
    B36  Project planning (gap)   — sprint / roadmap / milestone pairs     (Group 9)
    B37  Project managing (gap)   — track / coordinate / status pairs      (Group 9)
    B38  Project delivery         — release gates and go-live checklists   (Group 9)
    B15  Dev — from scratch       — backend order processing service       (Group 4)
    B12  Dev — documentation      — REST and GraphQL API contract docs      (Group 3)
    B17  Dev — integration        — third-party payment connector          (Group 4)
    B18  Dev — refactoring        — customer data migration and seeding    (Group 4)
    B32  Dev — vibe coding (gap)  — exploratory AI-assisted prototyping    (Group 8)
    B33  Dev — AI agent (gap)     — orchestrator and skill evolution        (Group 8)
    B34  Dev — skills/guidance    — technical guidance requests            (Group 8)
    B20  Testing                  — unit test design for billing           (Group 5)
    B26  QA / security review     — security and GDPR compliance review    (Group 6)
    B39  QA UI automation         — browser tests and UI regression        (Group 10)
    B28  QA automation / DevOps   — release pipeline automation            (Group 7)
    B14  DevOps                   — cloud infrastructure topology          (Group 3)

  Round 2 — SECOND PASS (second batch from each group, all layers):
    B02, B07, B10, B16, B19, B21, B24, B29, B35, B41 — covering remaining
    sub-phases in each group and extending knowledge-edge coverage.

  Round 3 — THIRD PASS (complete remaining batches):
    B03-B05, B08-B09, B11, B13, B22-B23, B25, B27, B30-B31, B40, B42.

  Round 4 — Layer 2 second scenario passes for all 31 sub-phases.
  Round 5 — Layer 2 third scenario passes for all 31 sub-phases.

  All 42 batches complete after Round 3. Layer 2 reaches 1000 pairs after Round 5.

---

## Quality gates per pair

  [G1] A side contains all required fields for its contract type.
  [G2] B side contains all required fields for its contract type.
  [G3] Lexicon evidence cites only terms from the predicate lexicon table above.
  [G4] Confidence 0.92 for ≥1 matched predicate; 0.70 for no match.
  [G5] CompiledPredicateIds contains only predicates whose terms appear verbatim.
  [G6] Router pairs: no dependency_order, parallel_group, domain_label, is_primary fields.
  [G7] Chain integrity: every node A comes from the accepted B of the previous node.
  [G8] Layer 2 node names must match the canonical nine-node table exactly.
  [G9] Corruption pairs include B-correct, B-corrupt, type name, harm statement.
  [G10] Held-out pairs include generalization test rationale.
  [G11] No pair reuses an exact user_request string from another pair.
  [G12] Gap-area pairs include a named capability gap ID in EvidenceIds.
  [G13] Sub-phase IDs and names must match the EXECUTOR-PROMPT table exactly.
