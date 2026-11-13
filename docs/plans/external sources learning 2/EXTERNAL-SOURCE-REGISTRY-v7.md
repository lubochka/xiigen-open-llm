# External Source Registry
# Version: 7.2 - 2026-05-11
# Supersedes: EXTERNAL-SOURCE-REGISTRY-v6.md

---

## 2026-05-11 Routing-Flow Authority Rule

This registry may list sources that ground `PromptRoutingInputCompilerUnit` or
`MultiSubPhaseRouterUnit` pairs. That does not make those sources sufficient for
accepted routing training by themselves.

Routing promotion requires a flow-level target:

```csharp
OrchestrationRoutingFlow : ILogicalFlow<LogicalOrchestratorRequest>
```

or equivalent `AdaptiveLogicalOrchestrator` behavior.

Source families that only ground compiler/router station rows are classified as
diagnostic coverage until a flow-level pair set connects the same source to:
compiled document, routing decisions, knowledge-edge state, flow/factory
resolution, and final routing verdict.

---

## What changed on 2026-05-11 (v7.2)

**Family 15 added - Instruction-Tuning Prompt Surfaces.**
OASST1/OASST2 are accepted with filtering as A-side prompt-surface sources for
orchestration training. FLAN is method-only until per-task licensing is audited.
Dolly 15k is quarantined because it is CC-BY-SA-3.0. Alpaca, ShareGPT variants,
WizardLM/Evol-Instruct, and UltraChat are blocked or review-required unless exact
license and provenance checks pass.

Primary guide: `external sources learning 2/INSTRUCTION-TUNING-SOURCE-ASSESSMENT-v1.md`.

---

## What changed on 2026-05-10 (v7.1)

**Six new source families added — Family 9 through Family 14.**
Coverage extends to config-driven multi-vertical platform architecture
(the Genie DNA pattern in `Multimodel Orchestrator/module-architecture.zip`):
content-type builders, schema-driven form/render engines, modular commerce,
headless UI primitives, design systems, and plugin/visual-editor architectures.

**2026-05-11 routing hold:** Families 9-14 still describe UI/design work with
sub-phase `3.6`, but current production topology has only `3.1` through `3.5`.
Rows targeting `3.6` must be held or remapped by model-led review before
training. Do not mark a `3.6` row train-ready until the topology registers that
sub-phase or the registry maps the row to an existing code-backed sub-phase.

**Strict license filter applied.** Only MIT and Apache 2.0 sources accepted
in this update. CC BY 4.0, BSD-3, and public-domain candidates that were
considered (Saleor BSD-3, JSON Forms BSD-3, Material Design 3 CC BY 4.0,
Microsoft Learn CC BY 4.0, Refactoring Guru CC BY 4.0, Volere template,
NIST SP 800-160v1 already in Family 8) are excluded from this batch.
The acceptance rule in the License States section is unchanged: MIT and
Apache 2.0 only for these families.

---

## What changed from v6

**Pair quality rules added.** Data from calibration run 2026-05-06 showed that
75% of batches had negative reward margin. The root cause was not missing sources
— it was the structure of rejected examples. Every source entry now states the
DPO pair quality rules that apply to pairs produced from that source.

**ARCH and PLAN prioritised over CODE.** Code sub-phases (4.x) are
performing well in training. The primary accuracy deficit is in PLAN and ARCH
domains. Source priorities and wave assignments reflect this.

**New source families added.** Six new external source families covering
architecture, orchestration patterns, API contracts, and testing edge cases
— all MIT or Apache 2.0 licensed.

**DPO pair authoring rules added as a top-level section.** These rules apply
universally across all source families. The current "Do not generate a capability
for sub-phase X" rejected template is forbidden. Every rejected response must be
a plausible wrong answer with a [WRONG: ...] explanation.

---

## 2026-05-11 Run-Experience Fix

Operational runbook:
`C:\Users\lubao\Documents\xiigen\llm mvp\external sources learning 2\documentation\TRAINING-SET-BUILD-AND-TESTING-RUNBOOK-v1.md`

Registry acceptance is necessary but not sufficient for training acceptance.
The current run found that large generated external libraries can be structurally
valid and still not be train-ready.

Additional source acceptance gates:

1. Generated rows from an external family must still pass pair-field hygiene:
   `chosen != rejected`, no mojibake in training fields, and specific
   `[WRONG: ...]` contrast.
2. A source family that is not listed here is not train-ready, even if the rows
   have `source_boundary: external`.
3. Large external sets must be tested in 16-64 row standalone ablations before
   they can join the current orchestration/plans/skills baseline.
4. External code-documentation rows must not be mixed directly into
   orchestration/plans/skills training. They need standalone code-doc validation
   first.
5. Ledger, audit, progress, and status files are evidence surfaces. They can
   justify rows but must not be used as DPO rows themselves.

Known pending external code-documentation domains from the 2026-05-11 audit:

```text
mongodb-csharp
elasticsearch-csharp
```

These domains require explicit registry entries before train-ready use.

---

## Canonical Pattern Dependency

This registry uses `DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v2.md` as the
authority. Sources are mapped to runtime contracts, not to informal component
names.

---

## Registry Rule

Every source entry must state:

```
which transition types it can teach
which phases, sub-phases, and task types it can teach
which runtime contracts it can produce rows for
which uses are excluded
which DPO pair type applies (A, B, or C — defined below)
```

If a source cannot be mapped to at least one runtime contract, it remains outside
the learning set.

---

## License States

```
first_party
external_license_checked
attribution_required
source_gap
corruption_case
verify_before_use
```

`verify_before_use` cannot enter training rows.

MIT and Apache 2.0 are the only accepted external licenses for training data.
CC BY 4.0 is accepted with attribution. CC BY-SA is not accepted — the share-alike
clause is incompatible with a closed training corpus. AGPL is not accepted.

---

## DPO Pair Quality Rules — Universal

These rules apply to every pair produced from any source in this registry.
They override any source-specific guidance where there is a conflict.

### The forbidden rejected template

The following rejected template is forbidden in all batches:

```
"Do not generate a capability for sub-phase X."
```

This template shares tokens with the chosen response and gives the model no
discriminating signal. It was the primary cause of negative reward margin in
calibration run 2026-05-06 (29 of 40 batches had chosen_reward < rejected_reward).

### Three valid rejected types

Every rejected response must belong to one of these three types.

**Type A — Plausible adjacent sub-phase.**
The rejected response addresses a different sub-phase that is semantically close
to the chosen one. It contains real technical content and a coherent argument,
but it addresses the wrong responsibility.

Use Type A when two sub-phases share vocabulary but own different decisions.
Example: a prompt about testing the auth guard — chosen is 5.1 Unit Test Design,
rejected is 3.4 Security and Authorization Model (plausible because "auth" appears
in the prompt, but the task is writing tests, not designing the security model).

**Type B — Correct sub-phases, wrong dependency order.**
The rejected response names the same sub-phases as the chosen response but
sequences them incorrectly or omits a dependency constraint.

Use Type B for all PLAN pairs where the prompt contains multiple tasks. The
rejected must show a plausible but wrong ordering with an explanation of why
the dependency makes the reversed order fail.

**Type C — Wrong domain for the prompt's verb.**
The rejected response addresses a different domain (BUG, CODE_Q, PLAN, ARCH)
based on a misreading of the prompt's intent signal.

Use Type C to teach domain boundaries. The intent verb determines the domain:
"explain / how does X work" → CODE_Q, "X throws / X fails" → BUG,
"implement / add / build" → PLAN or IMPLEMENT, "design / define boundaries" → ARCH,
"review / audit" → QA or ARCH review.

### Required [WRONG: ...] annotation

Every rejected response must end with a [WRONG: ...] block that names the
specific confusion — not a generic statement like "this is incorrect".

```
[WRONG: "define boundaries" asks who owns what and where dependencies flow,
not what the class body contains. Writing the class body is implementation.
Implementation decisions follow boundary decisions, they do not replace them.]
```

### One prompt, one pair per batch

A prompt must not appear as chosen in one pair and rejected in another within
the same batch. This produces a contradictory training signal that the model
cannot resolve.

### Batch composition minimums

| Pair type | Minimum fraction |
|---|---|
| Type A (adjacent sub-phase) | 40% of pairs in the batch |
| Type B (wrong dependency order) | 20% — required when prompt is multi-task |
| Type C (wrong domain) | 30% — required for every domain in the batch |
| Stage 9 corruption rows | At least 1 of each corruption kind per batch |

---

## Runtime Contract Registry

### Adaptive Routing Contracts

| Contract | Source use |
|---|---|
| `PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>` | Raw developer prompts compiled into structured router input. |
| `MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>` | Multi-sub-phase routing decisions, dependency ordering, ambiguous prompts. |
| `AdaptiveLogicalOrchestrator [knowledge_edge_detection pairs]` | Prompts routing to unregistered sub-phases, always with a registered-flow comparison. |
| `ILogicalFlowFactory<TInput> [flow_factory_selection pairs]` | All four trust states: trusted learned, deterministic, probationary, rejected with fallback. |

### Orchestrator Contracts

| Contract | Source use |
|---|---|
| `LogicalTransitionOrchestrator : ILogicalOrchestrator` | Real prompts, transition candidate evidence, selected and rejected candidates. |
| `TeachingOrchestrator : ILogicalOrchestrator` | Measured learning-cycle prompts and retraining workflows. |

### Flow Node Contracts

| Contract | Source use |
|---|---|
| `DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>` | Phase, sub-phase, task split rows. |
| `DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>` | Formal domain vocabulary rows. |
| `SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>` | Dependency, parallelism, wait-state, recovery, and knowledge-edge branch rows. |
| `DomainProcessorUnit : ILogicalUnit<WorkBranch, ProcessedBranchResult>` | Branch reasoning rows. |
| `ApplierUnit : ILogicalUnit<ProcessedBranchResult, AppliedBranchResult>` | Logical application rows. |
| `ResultTranslatorUnit : ILogicalUnit<AppliedBranchResult[], TranslatedResultFragment[]>` | Formal-to-human fragment rows. |
| `ArbiterSelectorUnit : ILogicalUnit<TranslatedResultFragment[], ArbiterReviewRequest[]>` | Arbiter routing rows. |
| `ArbiterUnit : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>` | Pass, concern, reprocess, reject rows. |
| `FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>` | Human result rows. |

### Teaching Contracts

| Contract | Source use |
|---|---|
| `PairParserUnit` | Pair file ingestion rows. |
| `FeatureExtractionUnit` | Feature extraction rows. |
| `ModelTrainingUnit` | Measured training rows with timing, CPU, RAM, disk, accuracy. |
| `HeldOutEvaluationUnit` | Held-out evaluation rows. |
| `CorrectionLearningUnit` | One-row correction and retraining rows. |

---

## Source Families

---

### Family 1 — XIIGen Conversation and Session Material

License state: `first_party`

Primary value:
```
Real developer prompt language — the only source of authentic A-side wording.
Real corrections from Luba — the highest-quality rejected signal available.
Real development-cycle intent for PLAN, ARCH, and multi-phase prompts.
```

Priority: **highest**. Batches 16 and 17 of calibration run 2026-05-06 showed
accuracy of 0.618 and 0.771 — the two best results in the entire run. Both
contained technical XIIGen session content with named phases, component names,
and explicit dependency language. This is the signal pattern that works.

What makes a good XIIGen prompt for PLAN:
- Contains a named artifact ("Flow 10", "Phase 3", "Factory Interface")
- Contains dependency language ("before", "after", "once X is complete")
- Names specific sub-phases or components, not abstract categories
- A prompt like "continue from where you stopped — Phase 1, Factory Interfaces"
  gives more routing signal than "let's work on the next steps"

What makes a poor XIIGen prompt (batch 30 pattern, accuracy 0.306):
- Meta-commentary about planning: "Option A is clearly the right path"
- Abstract correctness statements: "it's grounded in the docs that already exist"
- No named components, no dependency language, no phase references

Supported contracts:
```
PromptRoutingInputCompilerUnit
MultiSubPhaseRouterUnit
AdaptiveLogicalOrchestrator [knowledge_edge_detection]
LogicalTransitionOrchestrator
DomainSplitterUnit
SchemePlannerUnit
FormatterUnit
CorrectionLearningUnit
```

Supported DPO pair types: A, B, C. Type B is especially valuable for PLAN
prompts from XIIGen that span multiple phases — these are the best source of
real dependency ordering examples.

Excluded use:
```
Do not use session document language as a prompt A side.
Documents are authority evidence for B, not A-side developer wording.
Do not turn user corrections into generic rules.
Preserve the exact failed input and the exact corrected output as a pair.
```

---

### Family 2 — XIIGen Architecture and Experiment Documents

License state: `first_party`

Primary value:
```
Ground truth for orchestration decisions.
Phase and sub-phase topology.
Dependency rules between sub-phases.
Arbiter expectations.
```

Supported contracts:
```
MultiSubPhaseRouterUnit
ILogicalFlowFactory<TInput>
DomainTranslatorUnit
SchemePlannerUnit
ArbiterSelectorUnit
ArbiterUnit
FormatterUnit
ModelTrainingUnit
HeldOutEvaluationUnit
```

Supported DPO pair types: A for arbiter and router pairs. B for SchemePlannerUnit
pairs where the dependency order between sub-phases is the decision being taught.

Excluded use:
```
Do not use oracle document text as a user prompt A side.
Do not derive sub-phase dependency ordering without confirming the topology
registry contains the corresponding transition entries.
```

---

### Family 3 — Wave 1 Security Corpus

Location: `C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\wave-1\`

License state: `external_license_checked`

Primary value:
```
Security authority evidence for OAuth, OIDC, JWT, mTLS, ASVS, NIST SSDF, CWE.
The only source currently saturated for sub-phase 3.4.
Primary source of registered-flow comparison branches in knowledge-edge pairs.
```

Supported contracts:
```
MultiSubPhaseRouterUnit — 3.4 routing confidence grounding
AdaptiveLogicalOrchestrator [knowledge_edge_detection] — 3.4 as comparison branch
DomainTranslatorUnit
SchemePlannerUnit
ArbiterSelectorUnit
ArbiterUnit
FormatterUnit
HeldOutEvaluationUnit
```

DPO pair role: Wave 1 is saturated for chosen pairs in 3.4. Its primary value
going forward is as the **registered-flow comparison branch** in knowledge-edge
pairs for unregistered sub-phases. When a prompt routes to an unregistered
sub-phase (e.g. 3.1), the pair must show both the knowledge-edge branch result
and a comparison branch where 3.4 produces a real flow result. Wave 1 provides
the 3.4 comparison branch content.

Type A rejected use: security prompts that are plausibly confused with 4.1
Backend Services or 6.3 Security and Compliance Review. Use Type A to teach
the boundary: 3.4 owns the security model design; 6.3 owns the review that the
implementation was built correctly; 4.1 owns the code that implements what 3.4
designed.

Excluded use:
```
Wave 1 must not force every security-related prompt into repair or
external-source integration transition types.
Do not use Wave 1 as the sole grounding for sub-phases outside 3.4.
```

---

### Family 4 — Architecture Decision Records

Sources: `joelparkerhenderson/architecture-decision-record` (MIT),
`arc42/arc42-template` (MIT), `adr-tools` (MIT)

License state: `external_license_checked`

Primary value:
```
Real architectural decision language — the vocabulary developers use when
describing component boundaries, dependency direction, and trade-offs.
Each ADR has context (the problem), decision (the choice), and consequences
(what follows) — this maps directly to ARCH sub-phase pair structure.
```

Primary sub-phases: 3.1, 3.2, 6.1

Supported contracts:
```
PromptRoutingInputCompilerUnit — ARCHITECTURE predicate grounding
MultiSubPhaseRouterUnit — 3.1 routing decisions and disambiguation from 3.5
AdaptiveLogicalOrchestrator [knowledge_edge_detection] — 3.1 gap pairs
DomainSplitterUnit
DomainTranslatorUnit
SchemePlannerUnit
ArbiterUnit
```

DPO pair types for this source:

**Type A** — ARCH design confused with implementation. Use ADR decision sections
as chosen for 3.1 (the boundary decision). Rejected is a 4.1 Backend Services
response that implements instead of deciding. Example: ADR says "the notification
service owns delivery state and no other service may read its storage directly"
→ chosen is the boundary rule, rejected is "create a NotificationRepository
with GetDeliveryState() method".

**Type C** — ARCH design confused with PLAN. An ADR framed as "what should we
decide" is a PLAN prompt. An ADR framed as "how do these components relate" is
an ARCH prompt. Type C rejected teaches this boundary.

Excluded use:
```
Do not use ADR text verbatim as a user prompt A side.
ADR context sections describe problems, not developer prompt language.
Rephrase into natural developer language before using as A side.
```

---

### Family 5 — Orchestration Pattern Documentation

Sources: `temporalio/documentation` (MIT), `Netflix/conductor` docs (Apache 2.0),
`dapr/dapr` docs (Apache 2.0), `apache/airflow` tutorial docs (Apache 2.0)

License state: `external_license_checked`

Primary value:
```
Real orchestration vocabulary: workflow, activity, compensation, saga, retry,
dependency graph, parallel group, wait state, rollback.
These are the exact terms that appear in correct SchemePlannerUnit B sides.
Contains anti-patterns with explanations — ready-made Type B rejected material.
```

Primary sub-phases: 3.1, 3.5, 4.1, 4.5 for implementation context;
SchemePlannerUnit for orchestration structure.

Supported contracts:
```
SchemePlannerUnit — dependency ordering, parallel group assignment, wait states
DomainSplitterUnit — task decomposition patterns
DomainProcessorUnit — branch reasoning with compensation and retry
ArbiterUnit — workflow correctness checks
```

DPO pair types for this source:

**Type B** — correct branches in wrong order. Temporal.io and Airflow DAG
documentation contain anti-patterns explicitly: "do not call activity B before
activity A completes if B depends on A's output". These are ready-made Type B
rejected examples. The anti-pattern is the rejected; the correct DAG order is
the chosen.

**Type A** — orchestration confused with implementation. Workflow orchestration
(PLAN, SchemePlannerUnit) confused with the code that runs inside a workflow
activity (4.1 Backend Services). Chosen: define the workflow structure and
dependency order. Rejected: implement the activity class body.

Excluded use:
```
Do not use Temporal or Airflow syntax as user prompt A sides.
These are authority evidence for B sides and for validating PLAN chosen content.
```

---

### Family 6 — API Contract Specifications

Sources: `OAI/OpenAPI-Specification` (Apache 2.0),
`asyncapi/spec` (Apache 2.0),
`aip.dev` Google API Design Guide (CC BY 4.0 — attribution required),
`json-schema-org/json-schema-spec` (MIT)

License state: `external_license_checked`

Primary value:
```
Canonical API_CONTRACT predicate grounding.
Real schema vocabulary: endpoint, request, response, schema, versioning, error codes.
Edge cases: 400 vs 409, idempotency keys, breaking vs non-breaking changes.
These edge cases are the right material for corruption rows in Stage 9.
```

Primary sub-phases: 3.3, 4.5

Supported contracts:
```
PromptRoutingInputCompilerUnit — API_CONTRACT predicate grounding
MultiSubPhaseRouterUnit — 3.3 routing and disambiguation from 3.4
AdaptiveLogicalOrchestrator [knowledge_edge_detection] — 3.3 gap pairs
DomainSplitterUnit
DomainTranslatorUnit
SchemePlannerUnit
ArbiterSelectorUnit
FormatterUnit
```

DPO pair types for this source:

**Type A** — API contract design (3.3) confused with backend implementation (4.1).
Chosen: define the endpoint schema, request body shape, response codes, and error
contract. Rejected: implement the controller method that handles the request.
OpenAPI examples provide the chosen content; rejected is written to show someone
skipping the contract and going straight to implementation.

**Type A** — 3.3 confused with 3.4. A prompt about securing an API endpoint
might route to 3.4 (security model) or 3.3 (contract definition including auth
requirements). The boundary: 3.3 owns the contract shape including which auth
header is required; 3.4 owns the auth mechanism design.

**Stage 9 corruption rows** — wrong-predicate-assignment. A prompt containing
"API endpoint" and "authentication" correctly produces [API_CONTRACT, SECURITY].
Corrupted version assigns only [SECURITY], ignoring API_CONTRACT. OpenAPI specs
are the authority for what triggers API_CONTRACT.

Excluded use:
```
Do not use OpenAPI spec syntax as user prompt A sides.
Real developer prompts do not read like OpenAPI YAML.
Use spec content to validate B sides and to construct rejected content.
```

---

### Family 7 — Testing and QA Documentation

Sources: `microsoft/testfx` docs (MIT),
`microsoft/playwright` docs (Apache 2.0),
`testcontainers/testcontainers-dotnet` (MIT),
`TNG/ArchUnitNET` (Apache 2.0),
IETF RFC 6749 OAuth 2.0 (public domain),
IETF RFC 7519 JWT (public domain),
IETF RFC 9110 HTTP Semantics (public domain),
MITRE CWE database (public domain)

License state: `external_license_checked`

Primary value:
```
Testing sub-phases 5.1, 5.2, 5.3 are currently empty in training data.
RFC and CWE entries contain built-in correct/incorrect pairs:
  — RFC: MUST vs SHOULD vs MAY — each a different boundary condition
  — CWE: vulnerable code (rejected) alongside corrected code (chosen)
These are the best available source of natural hard negatives for 3.4 and 6.3.
```

Primary sub-phases: 5.1, 5.2, 5.3, 6.1, 6.3

Supported contracts:
```
MultiSubPhaseRouterUnit — testing sub-phases at dependency-order 2 after design
AdaptiveLogicalOrchestrator [knowledge_edge_detection] — 5.1, 5.3, 5.4 gap pairs
DomainSplitterUnit
SchemePlannerUnit
DomainProcessorUnit
ApplierUnit
ArbiterUnit
HeldOutEvaluationUnit
```

DPO pair types for this source:

**Type A** — unit test scope (5.1) confused with integration test scope (5.2).
This is the most common testing confusion. Testcontainers documentation shows
exactly when a test requires a real container (5.2) vs when isolation with mocks
is sufficient (5.1). Use Testcontainers examples as chosen for 5.2; write rejected
as a mock-based unit test for the same scenario with explanation of why mocks
fail to test the isolation property.

**Type A** — 6.1 Manual QA confused with 5.x automated testing. Playwright
automation is 5.3 End-to-End. A human acceptance checklist before release is 6.1.
Chosen: human review criteria with judgment items automation cannot assess.
Rejected: a Playwright test that automates the same scenario.

**Type A** — 6.3 Security and Compliance Review confused with 3.4 Security Model.
CWE entries are the authority: CWE-284 (Improper Access Control) is a 6.3 review
finding, not a 3.4 design decision. Chosen: review finding that the implementation
does not enforce the model. Rejected: redesign the auth model (wrong — the model
exists; the implementation is defective).

**RFC as Stage 9 corruption rows** — RFC 6749 section 5.2 defines error codes:
`invalid_grant`, `insufficient_scope`, `invalid_client`. A prompt reporting
a 401 with `insufficient_scope` should route to 3.4. A corrupted pair routes it
to 4.1 Backend Services as if it is an implementation bug. Use RFC as authority
to construct the correct routing and the wrong routing.

**CWE as Type A rejected** — every CWE entry has a "demonstrative example" with
vulnerable code. This is the naturally wrong answer. The corrected example is
the chosen. CWE-287 (authentication bypass), CWE-89 (SQL injection), CWE-22
(path traversal) are the highest-value entries for 3.4 and 6.3.

Excluded use:
```
OWASP Testing Guide is CC BY-SA — share-alike is not compatible with a closed
training corpus. Do not use OWASP content as raw training text.
Stack Overflow is CC BY-SA for the same reason.
xUnit Patterns book does not have an open license.
```

---

### Family 8 — NIST and Government Security Publications

Sources: NIST SP 800-53 (public domain), NIST SP 800-160 v2 (public domain),
NIST SSDF (public domain)

License state: `external_license_checked`

Primary value:
```
Deepens Wave 1 coverage for sub-phase 3.4 with control-level authority.
NIST SP 800-160 provides systems engineering vocabulary for 3.1 and 3.5.
MUST-level requirements from NIST are natural chosen content for arbiter pairs.
```

Primary sub-phases: 3.4, 3.1, 3.5, 6.3

Supported contracts:
```
DomainTranslatorUnit — authority language for security statements
SchemePlannerUnit — control dependencies and sequencing
ArbiterUnit — NIST MUST requirements as arbiter pass/fail criteria
HeldOutEvaluationUnit
```

DPO pair types: Type A for arbiter pairs. A pair where the implementation
satisfies NIST control X (chosen: pass verdict) vs a pair where the implementation
misses the control (rejected: concern or reject verdict with the specific control
citation).

---

### Family 9 — Config-Driven Admin Platforms (CMS / Headless / Content-Type Builders)

Sources: `strapi/strapi` (MIT),
`SoftwareBrothers/adminjs` (MIT),
`refinedev/refine` (MIT),
`keystonejs/keystone` (MIT),
`payloadcms/payload` (MIT),
`tinacms/tinacms` (Apache 2.0),
`appsmith-org/appsmith` (Apache 2.0)

License state: `external_license_checked`

Primary value:
```
Real config-driven admin platform vocabulary: content type, field schema,
collection, single type, dynamic zone, view definition, list/detail/edit
panel, role/permission matrix, plugin lifecycle, custom field type,
populate strategy, populate depth.
These are the exact terms that appear in correct ARCH 3.1, 3.2, and 3.6
sub-phase B sides for any "config-driven UI rendered from JSON" platform.
Strapi's content-type builder, AdminJS's resource-based admin auto-generation,
and Refine's data-provider abstraction are direct analogues of the Genie DNA
"Editor → Storage → Machine" 3-layer architecture.
```

Primary sub-phases: 3.1, 3.2, 3.6, 4.1, 4.4

Supported contracts:
```
DomainTranslatorUnit — config-driven platform vocabulary
SchemePlannerUnit — module-builder dependency ordering (schema → view → admin → API)
DomainProcessorUnit — Editor / Storage / Machine layer separation
ArbiterUnit — config-vs-implementation correctness checks
PromptRoutingInputCompilerUnit — MULTI_VERTICAL, CONFIG_DRIVEN, ADMIN_BUILDER predicate grounding
```

DPO pair types for this source:

**Type A** — config schema design (3.2) confused with rendering implementation
(4.1). Chosen: define the field types, validation rules, and JSON schema for
the content type. Rejected: implement the React form component or the
controller that handles the submit. Strapi schema definitions and Payload
collection configs provide chosen content; rejected is implementation that
should have been driven from the schema.

**Type A** — admin builder UI (3.6) confused with the rendering engine (4.1).
Chosen: design the editor panel layout, field grouping, conditional
visibility rules. Rejected: implement the DOM/React tree directly without a
configuration layer.

**Type B** — module-build order. Correct: schema definition → list/detail
view config → API exposure → admin permissions. Rejected: API exposure
first, schema reverse-engineered from API; or admin UI first, schema
backfilled.

Excluded use:
```
Do not use Strapi/Refine/Appsmith CLI command syntax as user prompt A sides.
Real product prompts do not read like CLI invocations.
Use platform docs to validate B sides and to construct rejected content
showing skipped layers.
```

---

### Family 10 — Schema-Driven Form Engines and Validators

Sources: `rjsf-team/react-jsonschema-form` (Apache 2.0),
`jaredpalmer/formik` (MIT),
`react-hook-form/react-hook-form` (MIT),
`colinhacks/zod` (MIT),
`jquense/yup` (MIT)

License state: `external_license_checked`

Primary value:
```
Render-from-JSON-Schema engine vocabulary and edge cases.
RJSF is the canonical example of "JSON config in, rendered form out" —
the exact pattern of the Genie DNA form_definitions module.
Zod and Yup provide validator-as-data vocabulary: refinement, transform,
narrowing, branded type, discriminated union, parse vs safeParse.
Formik and React Hook Form supply the controlled-vs-uncontrolled and
controlled-validation tradeoffs that drive 3.6 vs 4.1 confusion pairs.
```

Primary sub-phases: 3.2, 3.6, 4.1

Supported contracts:
```
DomainTranslatorUnit — schema-driven form vocabulary
SchemePlannerUnit — schema → render → validate → submit dependency ordering
DomainProcessorUnit — validator composition
ArbiterUnit — schema-validation pass/fail criteria
```

DPO pair types for this source:

**Type A** — JSON Schema design (3.2) confused with form render code (4.1).
Chosen: define the JSON Schema with field types, required, enum, format,
and conditional dependencies. Rejected: write the React component tree with
hard-coded inputs.

**Type A** — validator design (3.2) confused with controller validation
(4.1). Chosen: define the Zod or Yup schema as data. Rejected: write
per-field if-else validation in the controller method body.

**Type B** — render order. Correct: schema → ui:schema → validator → submit
handler. Rejected: submit handler first, validator inferred from server
response shape.

Excluded use:
```
Do not paste RJSF or Zod schema literals as user prompt A sides.
Real prompts describe what the form should do, not the schema syntax.
Use library docs to validate B sides and to construct rejected content.
```

---

### Family 11 — Modular Commerce / Multi-Vertical Domain Modules

Sources: `medusajs/medusa` (MIT)

License state: `external_license_checked`

Primary value:
```
Real modular-commerce vocabulary for Transaction-layer modules:
cart, line item, fulfillment, inventory location, sales channel,
payment session, shipping option, tax line, gift card, promotion,
discount rule, order edit, claim, swap, return reason, draft order.
Medusa's plugin architecture (admin extensions, payment providers,
fulfillment providers) is the direct open-source analogue of the
Genie DNA cart_rules / availability_rules / promotion_definitions
modules. Note: Saleor (BSD-3) and Spree (BSD-3) are deliberately
excluded under the MIT-or-Apache-2.0 filter applied 2026-05-10.
```

Primary sub-phases: 3.1, 3.2, 4.1

Supported contracts:
```
DomainTranslatorUnit — Transaction-layer module vocabulary
SchemePlannerUnit — order lifecycle dependency ordering
DomainProcessorUnit — branch reasoning across cart / promotion / fulfillment
ArbiterUnit — order invariants pass/fail criteria
```

DPO pair types for this source:

**Type A** — Transaction module design (3.1 / 3.2) confused with controller
implementation (4.1). Chosen: define the cart line-item shape, the promotion
rule schema, the fulfillment hand-off contract. Rejected: implement the
controller method that processes the cart submit.

**Type B** — order lifecycle dependency. Correct: cart line items →
tax/shipping calculation → promotion application → payment authorisation →
fulfillment claim. Rejected: fulfillment claim before promotion application
(wrong totals captured) or payment before tax (under-charge).

Excluded use:
```
Do not use Medusa CLI or schema-literal text as user prompt A sides.
Use Medusa docs to validate B sides and to construct rejected content.
```

---

### Family 12 — Headless UI Primitives and Generic Engines

Sources: `radix-ui/primitives` (MIT),
`shadcn-ui/ui` (MIT),
`TanStack/table` (MIT),
`TanStack/router` (MIT),
`TanStack/form` (MIT),
`TanStack/query` (MIT),
`clauderic/dnd-kit` (MIT),
`udecode/plate` (MIT),
`microsoft/fluentui` (MIT),
`primer/react` (MIT),
`Shopify/polaris` (MIT)

License state: `external_license_checked`

Primary value:
```
Generic engine vocabulary for the Layer 3 "Machine" of any config-driven
platform: composable primitive, headless component, render-prop, slot,
controlled state, uncontrolled state, accessor pattern, column definition,
sort state, filter state, pagination state, route tree, nested route,
loader/action, query cache, optimistic update.
TanStack Table is the canonical "config-in, generic-table-out" engine —
the direct analogue of the Genie DNA DynamicTable.
Radix and shadcn/ui supply ARIA-correct primitives that the generic
"Machine" layer composes.
```

Primary sub-phases: 3.6, 4.1, 4.4

Supported contracts:
```
DomainTranslatorUnit — generic-engine vocabulary
DomainProcessorUnit — config-driven rendering branch reasoning
SchemePlannerUnit — primitive → headless component → app-shell composition order
ArbiterUnit — accessibility and config-correctness checks
```

DPO pair types for this source:

**Type A** — generic engine composition (3.6) confused with bespoke
implementation (4.1). Chosen: compose TanStack Table column defs and Radix
Dialog primitives into a configurable shell. Rejected: write the table HTML
and modal markup directly with hard-coded columns and content.

**Type C** — UX design (3.6) wrongly routed to security (3.4) when the
prompt mentions "permission-aware UI affordance". Boundary: 3.6 owns the
visible affordance and ARIA wiring; 3.4 owns the policy decision the
affordance reflects.

Excluded use:
```
Do not paste Radix component code or TanStack column-def literals as user
prompt A sides. Use library docs to validate B sides.
```

---

### Family 13 — Apache 2.0 Design Systems

Sources: `carbon-design-system/carbon` (Apache 2.0),
`adobe/spectrum-css` (Apache 2.0),
`adobe/react-spectrum` (Apache 2.0)

License state: `external_license_checked`

Primary value:
```
Enterprise admin-UI design language: information density tier, spacing
scale, type ramp, motion duration, elevation level, focus indicator,
data-table density mode, settings hub layout, configurator drawer,
breadcrumb hierarchy.
Carbon and Spectrum are designed for the same density and configurability
the Genie DNA Editor (Layer 1) admin UI requires.
Note: Material Design 3, GitHub Primer (CSS), Atlassian Design System and
Shopify Polaris docs are deliberately excluded — Material 3 is CC BY 4.0
and the others mix CSS / brand-guideline licenses outside the strict
MIT-or-Apache-2.0 filter applied 2026-05-10.
```

Primary sub-phases: 3.6

Supported contracts:
```
DomainTranslatorUnit — admin design-system vocabulary
DomainProcessorUnit — UX-pattern branch reasoning
ArbiterUnit — accessibility (WCAG) and density-mode pass/fail criteria
```

DPO pair types for this source:

**Type C** — design pattern (3.6) confused with implementation (4.1).
Chosen: select the Carbon density tier and Spectrum motion duration token
for the admin panel. Rejected: write the CSS rules with literal pixel
values and ms durations.

**Type A** — admin layout design (3.6) confused with backend pagination
(4.1). Chosen: define how the data-table density mode and pagination
controls behave. Rejected: implement the SQL OFFSET/LIMIT query.

Excluded use:
```
Do not paste Carbon CSS classnames or Spectrum token JSON as user prompt A
sides. Use design-system docs to validate B sides and to construct
rejected content showing literal styling instead of token reference.
```

---

### Family 14 — Service Catalogue, Plugin Architecture, and Visual Editor Engines

Sources: `backstage/backstage` (Apache 2.0),
`excalidraw/excalidraw` (MIT),
`tldraw/tldraw` (Apache 2.0)

License state: `external_license_checked`

Primary value:
```
Plugin architecture and service-catalogue vocabulary: extension point,
contribution, plugin manifest, plugin metadata, scaffolder template,
catalogue entity, kind, spec, owner, lifecycle, processor, provider,
extension API, theme provider, persistence layer.
Backstage's plugin system is the direct open-source analogue of the
Genie DNA Dynamic Router + module index. tldraw and Excalidraw provide
canvas / editor patterns directly relevant to the Phase 5 "Visual Editor"
layer of the Genie DNA architecture.
```

Primary sub-phases: 3.1, 3.6, 4.1

Supported contracts:
```
DomainTranslatorUnit — plugin architecture vocabulary
SchemePlannerUnit — plugin lifecycle and contribution-order dependencies
DomainProcessorUnit — extension-point composition reasoning
ArbiterUnit — plugin-isolation and version-compat checks
```

DPO pair types for this source:

**Type A** — plugin architecture design (3.1) confused with plugin
implementation (4.1). Chosen: define the extension points, the contribution
manifest, and the lifecycle hooks. Rejected: implement one specific
plugin's entry point.

**Type A** — visual-editor canvas design (3.6) confused with rendering
runtime (4.1). Chosen: define the shape model, the tool palette, and the
selection state. Rejected: implement the SVG draw routine.

**Type B** — plugin contribution order. Correct: plugin manifest →
extension-point registration → contribution ingestion → render at consumer.
Rejected: render first, contribution discovery added later.

Excluded use:
```
Do not paste Backstage catalog-info.yaml or tldraw shape-spec literals as
user prompt A sides. Use docs to validate B sides.
```

---

## Current Gap Map

### Covered — no new pairs needed

```
3.4 Security and Authorization Model
```

Wave 1 is saturated. Additional pairs from this sub-phase should only be written
as comparison branches in knowledge-edge detection pairs for other sub-phases.

### Highest priority — accuracy deficit confirmed

```
PLAN domain
ARCH domain (3.1, 3.2, 3.5)
```

Calibration run 2026-05-06 showed PLAN at mean accuracy 0.507 with only 2 of 6
batches having positive reward margin. The best PLAN batches contained XIIGen
session content with named components and explicit dependency language. The worst
contained meta-commentary without technical specifics.

ARCH was not represented in the run at all — zero ARCH batches. All ARCH
sub-phases remain at zero training signal.

Immediate action: produce PLAN pairs using Type B (dependency ordering) as the
primary pair type. Produce ARCH pairs using Type A (design vs implementation
confusion) across all four ARCH sub-phases.

### High priority — empty sub-phases

```
5.1 Unit Test Design
5.2 Integration Tests
5.3 End-to-End Tests
6.3 Security and Compliance Review
```

Families 7 and 8 provide the authority content. Type A pairs (unit vs integration
confusion, 6.3 review vs 3.4 design confusion) should be the primary pair type.

### Layer 0 adaptive routing — no pairs exist

```
PromptRoutingInputCompilerUnit — no pairs
MultiSubPhaseRouterUnit — no pairs
knowledge_edge_detection — no pairs
flow_factory_selection — no pairs
```

All four required before the adaptive orchestrator can train. First O1 batch
must include all four alongside Layer 1 and Layer 2 rows.

### Wave assignments

| Wave | Primary sub-phases | Primary source families | Primary pair types |
|---|---|---|---|
| Wave 2 | 6.3 Security and Compliance Review | Family 3 (comparison), Family 7 (CWE), Family 8 (NIST) | Type A: 6.3 vs 3.4 boundary |
| Wave 3 | 3.1 Architecture and Component Topology | Family 4 (ADR, arc42), Family 1 (XIIGen ARCH sessions) | Type A: 3.1 vs 4.1 boundary |
| Wave 3 | 3.2 Data Model and Persistence Design | Family 4, Family 1 | Type A: 3.2 vs 4.1 boundary |
| Wave 3 | 3.5 Observability and Operational Design | Family 5 (Temporal, Dapr), Family 1 | Type A: 3.5 vs 7.3 boundary |
| Wave 4 | 5.2 Integration Tests | Family 7 (Testcontainers, Playwright) | Type A: 5.2 vs 5.1 boundary |
| Wave 4 | 5.1, 5.3 | Family 7 | Type A: test level boundaries |
| Wave 5 | Cross-phase PLAN prompts | Family 1 (XIIGen), Family 5 (orchestration) | Type B: dependency ordering |

---

## ARCH sub-phase confusion table

Every ARCH pair must use one of these rejection patterns. Choose the row that
matches the prompt intent.

| Chosen sub-phase | What it owns | Most plausible rejected | Why it is wrong |
|---|---|---|---|
| 3.1 Architecture and Component Topology | Boundary decisions, dependency direction | 4.1 Backend Services | Writes the class instead of the boundary rule |
| 3.1 Architecture and Component Topology | Boundary decisions, dependency direction | 4.4 Refactoring | Moves code instead of deciding ownership |
| 3.2 Data Model and Persistence Design | Entity shape, storage rules, consistency | 4.1 Backend Services | Writes the repository instead of the model |
| 3.2 Data Model and Persistence Design | Entity shape, storage rules, consistency | 4.5 Integration | Writes the adapter instead of the boundary |
| 3.5 Observability and Operational Design | What to measure, alert thresholds, what not to log | 4.1 Backend Services | Wires the logger instead of defining the strategy |
| 3.5 Observability and Operational Design | What to measure, alert thresholds, what not to log | 7.3 Monitoring and Feedback Loop | Configures the dashboard before the signals are designed |
| 6.1 Manual QA and Acceptance Review | Human review criteria, release judgment | 5.1 Unit Test Design | Automates instead of defining human acceptance |
| 6.1 Manual QA and Acceptance Review | Human review criteria, release judgment | 5.3 End-to-End Scenario Testing | Automates the user journey instead of a human checklist |

---

## PLAN dependency ordering table

Every PLAN pair with multiple branches must use a Type B rejected that reverses
or removes a real dependency from this table.

| Correct sequence | Natural wrong order in rejected |
|---|---|
| 3.1 Architecture → 4.1 Backend Services | 4.1 first, then 3.1 as documentation |
| 3.2 Data Model → 3.3 API Contract | 3.3 first, then model derived from contract |
| 3.4 Security → 5.2 Integration Tests | 5.2 first with mocks, 3.4 added after |
| 3.1 + 3.4 in parallel → 4.1 after both | All three in parallel with no dependency |
| 2.2 Acceptance Criteria → 5.1 Unit Tests | 5.1 first, criteria inferred from tests |
| 3.5 Observability Design → 7.3 Monitoring | 7.3 configuration first, design retroactively |
| 3.3 API Contract → 4.5 Integration Connectors | 4.5 first, contract written to match implementation |

---

## Stage 9 corruption case requirements

Every batch must include at least one of each of the following corruption kinds.

**wrong-predicate-assignment:** The compiler B assigns predicate IDs that do not
match the lexicon terms present in the prompt. Authority for correct assignment
comes from Family 6 (API contracts) and Family 7 (RFC, CWE).

```
Prompt: "test coverage shows 42% on the authentication module"
Correct:  CompiledPredicateIds: ["TEST", "SECURITY"]
Corrupted: CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"]
[WRONG: "test coverage" triggers TEST first. "authentication module" is the
object being measured, not the action. TEST predicate takes priority.]
```

**ambiguous-predicate-case:** The router output emits one routing decision when
two predicates support two different sub-phases. The correct output emits both
routing decisions.

```
Prompt: "add authorization to the API endpoint"
Correct:  Routes to 3.4 (SECURITY) AND 3.3 (API_CONTRACT) — two decisions.
Corrupted: Routes only to 3.4, ignores API_CONTRACT.
[WRONG: API_CONTRACT predicate from "API endpoint" is present.
When multiple predicates match, all sub-phases must be routed.]
```

**knowledge-edge-suppressed:** The result returns PASS for a prompt that routed
to an unregistered sub-phase. The correct result is KNOWLEDGE_EDGE with all six
required metrics.

```
Prompt routes to 5.3 End-to-End Tests — no flow registered.
Correct:  OverallVerdict: "KNOWLEDGE_EDGE"
          Metrics: knowledge_edge=True, missing_sub_phase=5.3, missing_flow=True,
                   routed_phase_id=5, routing_confidence=0.84,
                   knowledge_edge_reason="no logical flow registered for routed sub-phase"
Corrupted: OverallVerdict: "PASS" — suppressed knowledge edge.
```

---

## Registry Acceptance Checklist

Before a source registry update is accepted:

- [ ] Every source has a license state — MIT, Apache 2.0, CC BY 4.0, or first_party.
- [ ] Every source maps to at least one runtime contract.
- [ ] Every source states excluded uses.
- [ ] Every source states which DPO pair types (A, B, C) apply.
- [ ] Every source separates prompt language from authority evidence.
- [ ] Every source that supports MultiSubPhaseRouterUnit states which sub-phases it grounds dependency ordering for.
- [ ] Every source that supports knowledge_edge_detection states whether it provides gap branches, comparison branches, or both.
- [ ] No rejected response in any pair uses the forbidden "Do not generate a capability" template.
- [ ] Every rejected response includes a [WRONG: ...] annotation.
- [ ] Batch composition meets minimum pair type fractions.
- [ ] At least one of each Stage 9 corruption kind is present in every batch.

---

## Family 15 - Instruction-Tuning Prompt Surfaces

Status: gated candidate family added on 2026-05-11.

Authority role: A-side prompt surface only. These datasets do not become B-side
architecture authority for XIIGen. Chosen sides must cite internal contracts,
current code validation, or an approved technical standard.

Primary document:
`external sources learning 2/INSTRUCTION-TUNING-SOURCE-ASSESSMENT-v1.md`

Accepted sources:

- OpenAssistant OASST1 - Apache-2.0. Use for human multi-turn prompt shapes,
  clarification patterns, and ambiguous routing requests.
- OpenAssistant OASST2 - Apache-2.0. Same use as OASST1, with separate source
  ids and separate ablation.

Method-only / held sources:

- FLAN Collection - use templates and task-form methodology only until a
  per-task license ledger is created. Do not ingest raw rows by default.

Quarantine / blocked sources:

- Databricks Dolly 15k - CC-BY-SA-3.0. Quarantine from clean Apache/MIT batches
  unless ShareAlike handling is explicitly accepted.
- Stanford Alpaca - CC-BY-NC-4.0 / research-only. Block from production
  training sets.
- ShareGPT variants - fragmented license and model-output provenance risk.
  Block by default.
- WizardLM / Evol-Instruct - often derived from ShareGPT or model outputs.
  Use method inspiration only unless exact source license/provenance is
  reviewed.
- UltraChat - review required until the exact release has license and provenance
  accepted.

Allowed pair families:

- IT-ORCH-00 root intent to `OrchestrationRoutingFlow` or equivalent adaptive orchestrator; this is the only Family 15 routing family that can support routing promotion.
- IT-ORCH-01 root intent to `PromptRoutingInputCompilerUnit`
- IT-ORCH-02 ambiguous request to multi-sub-phase routing
- IT-ORCH-03 clarification before execution / knowledge-edge
- IT-ORCH-04 complex task decomposition
- IT-ORCH-05 skill activation and load order

Required row fields:

- `external_role: "a_side_style_only"`
- `internal_contract_reference`
- `source_license_state`
- `source_id`
- `pair_kind`
- specific `[WRONG: ...]` consequence on rejected side

First-run gate:

Use OASST1/OASST2 only, 64 rows, standalone ablation first, then combo gate
against V246. Do not mix this family into production rows until the ablation
passes.

---

## Last Updated

2026-05-11 (v7.2 - added Family 15 instruction-tuning prompt surfaces; v7.1 family 9-14 constraints remain active).
