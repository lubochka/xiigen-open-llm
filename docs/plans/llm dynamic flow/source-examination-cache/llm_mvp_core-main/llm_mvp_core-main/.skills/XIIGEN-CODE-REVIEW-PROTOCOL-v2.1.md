# XIIGEN Code Review Protocol
## MVP plans, REMEDIATION plans, gap-closure plans, architectural fix plans

---


- **the definition-of-done protocol check added — Definition of done includes protocol status + the authentication deployment prerequisite check.**
 Any implementation plan submitted for review must declare that its definition of done
 includes all three protocol gate fields (portability status, authentication status, tenant certification tier)
 and must satisfy the authentication deployment prerequisite check before claiming Tier C eligibility.

 **the definition-of-done protocol check has three sub-checks:**

 the portability gate check (portability): plan includes Phase G or an explicit "Phase G N/A — external repository adapter
 adapter" declaration. Plan's "done" state cannot leave portability status = not yet determined. A plan
 that ends at Phase F without declaring Phase G has an incomplete definition of done for
 any distributable flow.

 the authentication declaration check (auth): plan includes Phase H or an explicit "Phase H N/A — no HTTP controllers"
 declaration. authentication deferred (not yet deployed) is acceptable (auth.module.ts absent) — the plan must name
 it explicitly as the deferred path. A plan with HTTP controllers and no Phase H ships
 unguarded endpoints by design.

 the certification plus the authentication deployment prerequisite check check (certification + the authentication deployment prerequisite check): plan includes Phase I or an explicit "Phase I N/A —
 not distributable" declaration. If the plan targets Tier C or Tier D, the authentication deployment prerequisite check must be
 verified in the plan: auth infrastructure score 4/4 (auth.module.ts + scope-enrichment
 + APP_GUARD + role-strings.ts). A plan claiming Tier C eligibility without the authentication deployment prerequisite check
 evidence will be blocked at Tier B at Phase I execution time.

 Corpus origin: IMPL-the state file across 49 flows shows portability status = not yet determined,
 authentication status = not yet determined, tenant certification tier = NONE on all 49 flows. Root cause: no prior
 implementation plan version had Phase G, H, or I in its scope. the definition-of-done protocol check installs the
 gate that catches this omission at plan-review time before Phase A runs.

- **Tier 2 verdict aggregation** updated to include the definition-of-done protocol check (the portability gate check/b/c).
- **Verdict format** updated: the definition-of-done protocol check appears as a single row with three sub-check columns.
- **Versioning** updated.

---


- **the DNA compliance check (detection before scope claim) added — Detection before scope claim.** Any plan claiming "N flows affected"
 must show detection command output before fix steps. Detection output must match the
 claimed scope. Claims that exceed detected scope are BLOCK. Universal claims ("all flows,"
 "full fleet") require detection against 100% of flows, not a sample.
 Corpus origin: gap 05 from gap mapping round 3 — plans claimed full-fleet scope with detection
 run on 3-5 flows. Remediation sessions that ran on 49 flows had evidence from 6.

- **the DNA compliance check (new-row arbiter re-run) added — New rows trigger arbiter re-run.** When a plan version adds new
 per-flow rows (FLOW-40..48 added to a plan originally written for FLOW-01..39),
 the full arbiter battery runs on all new rows before the plan version is approved.
 A plan version that adds rows without re-review is unapproved for those rows.
 Adding rows to an existing plan is a new plan for those rows.
 Corpus origin: gap 07 and gap 11 from gap mapping round 3 — FLOW-40..48 went unreviewed
 for 10 plan versions. The full arbiter battery was never run on the 9 most important
 new flows in the fleet.

- **Tier 2 verdict aggregation** updated to include the DNA compliance check (detection before scope claim) and the DNA compliance check (new-row arbiter re-run).
- **Verdict format** updated: the DNA compliance check (detection before scope claim) and the DNA compliance check (new-row arbiter re-run) appear as separate rows.

---


- **the code quality check split into the DNA compliance check for generated code and the DNA compliance check for remediation plans.** the DNA compliance check for generated code preserves existing behavior (GENERATION
 plans: generated code specifications honor coding rule 1..coding rule 9). the DNA compliance check for remediation plans is new: REMEDIATION
 plans must verify every proposed solution pattern against the four critical DNA rules
 (coding rule 5 tenant scoping, coding rule 7 fabric-first, coding rule 8 outbox-before-queue, coding rule 3
 DataProcessResult) with IMPLEMENTATION or TEST evidence. DESIGN_DOC evidence
 (RECONCILIATION-STATE, FIX-PLAN documents) does not satisfy the DNA compliance check for remediation plans.
 Corpus origin: module-separation fix plan v2.0–v4.9 passed all custom arbiter passes
 (F1-F4) while proposing "remove ClsService, add explicit tenantId" for 15 flows with
 no IMPLEMENTATION evidence that the proposed pattern was coding rule 5 compliant. DNA violations
 were found at implementation review — after the fact, at scale.
- **Scope line extended** to include REMEDIATION plans, gap-closure plans, and
 architectural fix plans. the code quality check scope was previously implicit (code-generation only).
 Remediation plans that propose changes to existing code are now in scope.
- **Tier 2 verdict aggregation** updated to include the DNA compliance check for remediation plans.
- **Verdict format** updated: the DNA compliance check for generated code and the DNA compliance check for remediation plans appear as separate rows.
- **Versioning** updated.

---


- **Gate 0g business-intent sub-check added:** for any plan step creating or modifying a React page, the plan must cite all four of the following before the first implementation task: (1) examination record read and primary finding named, (2) user intent quoted from FLOW-XX-step 1-INVARIANTS.md, (3) primary role and source file named, (4) grammar type declared (G1–G7). A plan that passes the the interface compliance skill Phase 7 sub-check (Phase 7 planned) but fails the business-intent sub-check has planned the compliance audit without planning what the page is for.
- **Tier 1 Tier 2 verdict aggregation** updated to reflect Gate 0g now having two sub-checks.
- **Cross-references** updated for new document versions.
- **Versioning** updated.

---

## 1. What this protocol reviews

This protocol reviews **plans** — documents that instruct Claude Code (or equivalent executor) to perform work. The review produces a verdict: APPROVED, APPROVED_WITH_CONCERNS, or REJECTED. A REJECTED plan does not execute.

The protocol does not review:
- Code itself — that's execution-layer review at Claude Code's PR-review or Gate F stage
- Fleet-wide design state — that's `XIIGEN-DESIGN-REVIEW-PROTOCOL`
- Live flow runs — that's QA session type

The protocol reviews the plan as an artifact: its coverage, its evidence grounding, its correctness relative to governance, its construction shape. A plan that looks complete but was produced against a misread brief, or that drops a stated goal, or that inherits unverified claims, must be caught here — before the executor runs it and produces wrong output at scale.

---

## 2. When this protocol runs

Triggered by:
- A plan document is produced in a PLANNING session and presented at Stop here and wait for approval
- A plan document is uploaded by the user for review
- A plan version is bumped and requires re-review

Reviewer runs all gates in order. Early-gate failures block later gates.

---

## 3. Gate ordering and precedence

Gates run in two tiers.

**Tier 1 — Structural pre-checks.** Gates 0a through 0m. These catch failures in how the plan was constructed and whether it addresses the user's actual ask. Any BLOCK at Tier 1 rejects the plan before Tier 2 runs.

**Tier 2 — Correctness checks.** the count accuracy check through the interface quality check. These catch failures in the plan's content: goal coverage, architectural coherence, codebase evidence, principles compliance, pipeline contract, and related dimensions.

Tier 1 runs first because a plan that passes correctness review for the wrong plan is worse than no review at all. If the plan's construction is wrong, correcting its content is work against a moving target.

---

## 4. Tier 1 — Structural pre-check gates

### Gate 0a — Plan is self-contained

Every claim, file reference, and decision referenced in the plan must be present in the plan document, a referenced session file, or an explicitly-linked source. A plan that says "per the prior decision" without naming which decision fails Gate 0a.

Verdict: PASS | FAIL. On FAIL, specify which references are unresolved.

### Gate 0b — Plan does not embed skill citations as production dependencies

SK-XXX references in the plan appear only inside gate-check command strings (telling Claude Code to run the grep), not as production code dependencies. A plan instructing Claude Code to `import { something } from "the session file authoring skill"` fails Gate 0b — skills are governance, not code.

Verdict: PASS | FAIL.

### Gate 0c — Plan respects file-path semantic slugs (Rule 16)

Every file path in the plan uses the semantic slug from the naming conventions skill's domain name table. A plan referencing `server/src/engine/flows/flow-15/` fails if the canonical slug is `saas-multi-tenancy`.

Verdict: PASS | FAIL.

### Gate 0d — Plan's scope matches the user's ask

Plan scope is equal to or explicitly narrower than the user's stated ask. Narrowing requires declared justification in the plan document. Widening without declared justification fails.

Verdict: PASS | FAIL.

### Gate 0e — Plan's contract (the output contract question output contract) is specified

The plan declares what "done" looks like with enough specificity that Claude Code can verify completion without asking the user. "All tests pass" is too vague. "Server tsc: 0 errors, 0 warnings. Server jest: 10,617 passed, 0 failures. Client build: success. PNG count in docs/e2e-snapshots/{slug}/ ≥ 4 per FLOW, populated state as primary PNG" is specific.

Verdict: PASS | FAIL.

### Gate 0f — Minimal-wiring preference

If the work touches artifacts with existing design (fixtures, contracts, documented decisions), the plan uses the minimal wiring path — not rebuild, not redesign. Plans exceeding 5 tasks for wiring work are rejected unless the task expansion is justified by verified complexity beyond wiring.

Verdict: PASS | FAIL.

### Gate 0g — Visual proof present for tenant-facing claims *(updated v1.8)*

Any plan step that claims to deliver a tenant-visible feature includes Playwright PNG evidence — either citing existing PNG paths in `docs/e2e-snapshots/{slug}/` or specifying Playwright spec changes that will produce PNGs. Server-only steps are explicitly flagged as server-only with justification.

**the interface compliance skill Phase 7 sub-check:** For any plan step that produces a React page, the plan must also declare Phase 7 (UI/UX Compliance) as a step following the React pages phase. A plan that adds `*.tsx` files without a corresponding Phase 7 step fails this sub-check. Gate 0g only verifies that Phase 7 is *planned*; Gate 0m verifies the role questions; the business-intent sub-check below verifies what the page is actually for.

**Business-intent sub-check (NEW v1.8):** For any plan step creating or modifying a React page in `client/src/pages/`, the plan must cite all four of the following before the first implementation task:

1. **Examination record** — plan states that `docs/screen-examination/{slug}-examination.md` was read (if it exists) and names its primary finding and classification. A plan that ignores an existing examination record fails this sub-check. If no examination record exists, state "no examination record — proceeding from spec files."

2. **User intent source** — plan quotes or closely paraphrases the `user_intent` sentence from `docs/sessions/FLOW-XX/FLOW-XX-STEP-1-INVARIANTS.md`. A plan that describes page functionality without citing what the user is trying to accomplish fails.

3. **Role-visibility source** — plan names the primary role and its source file (`docs/design-reviews/ROLE-ANALYSIS-BATCH-NN.md`). A plan that says "admin view" without citation fails.

4. **Grammar declaration** — plan declares which grammar type the page implements (G1 PROGRESS_STRIP · G2 VERDICT_GRID · G3 CARD_LIST · G4 TOPOLOGY_CANVAS · G5 KIOSK · G6 DASHBOARD · G7 SETTINGS_TABS). A plan that describes a page without declaring a grammar type fails.

**Relationship between sub-checks:** a plan that passes the Phase 7 sub-check (Phase 7 planned) but fails the business-intent sub-check has planned the compliance audit without planning what the page is for. Both sub-checks must pass. The business-intent sub-check installs Vocabulary B (who uses this page and what do they want?) before Vocabulary A (can they access it and is the route correct?).

**this open issue halt (FLOW-04, FLOW-09, FLOW-34):** if the plan includes React page work for these flows, Gate 0g fails automatically until a Luba resolution on the F1 spec gap is on record. These flows have stale F1 documents where the stated user_intent contradicts the slug, pages, and PNGs. A plan cannot declare a valid grammar type for a page whose user intent is disputed.

Verdict: PASS | FAIL. On FAIL, specify which tenant-facing step lacks visual proof, which React pages phase lacks a Phase 7 step, or which of the four business-intent elements is missing.

### Gate 0h — Iron rule and BFA governance honored

The plan does not propose overriding locked iron rules or BFA governance without explicit Luba approval stated in the plan document with timestamp.

Verdict: PASS | FAIL.

### Gate 0i — Architect Habits Discipline (the architect habits guide v1.2.0)

Reviewer runs the three-step doc-first loop on the plan document:

**Step 1 — Scan.** Read the plan against the 30-habit catalog (7 positive, 4 neutral-positive, 19 negative). Note every habit hit with name and bracketed ID.

**Step 2 — Dig in docs FIRST.** For every habit hit suggesting a correctness-propagating concern, search `docs/`, `historyRag/`, `DECISIONS-LOCKED.md`, prior FLOW session files, `SESSION-LOAD-PLAN`, and codebase grep — with conceptual vocabulary, not only literal strings.

**Step 3 — Classify.**
- Class-a (correctness-propagating, BLOCK after documented Step 2 returns nothing)
- Class-b (architectural, CONCERN — plan may ship after author responds in writing)
- Class-c (style, not raised)

Three sub-checks that must clear before Gate 0i passes:
- **this failure pattern sub-guard:** no inherited-verdict citations used without re-running the originating command
- **this failure pattern sub-guard:** plan cites its inputs — what the plan read to produce itself (session brief, attached files, prior plan versions)
- **this failure pattern sub-guard:** plan's own "done" contract re-quoted from session the output contract question, deliverable shape matches element-by-element

**Cluster-aware scan sub-check:** if a single plan exhibits three or more habit hits from the same cluster (e.g., this failure pattern, this failure pattern, this failure pattern all appearing together — the unverified-claims cluster), the plan is flagged regardless of individual-habit classification. Clustered hits indicate systemic construction problems.

Verdict: PASS | PASS_WITH_CONCERNS | BLOCK.

### Gate 0j — Plan addresses user's items in user's order

This gate enforces Response Construction Protocol Step 1 (instruction decomposition) at plan-authoring time. The reviewer checks whether the plan document was constructed per the user's stated order.

**Procedure:**

1. Locate the user's originating request in the plan's provenance (session brief, prior turn, or linked source document). Quote it in full.
2. Parse the request into enumerated items integration round 1, integration round 2, integration round 3... as Protocol Step 1 would.
3. For each item, locate where in the plan it is addressed. Mark: ADDRESSED (with plan section reference), DEFERRED (with declared reason), or ABSENT.
4. Any ABSENT item without a declared reason fails the gate.
5. Items addressed out of user's stated order pass if the plan document declares the reordering explicitly; fail if the reordering is silent.

**This gate catches the this failure pattern (cutting user order) failure mode as it appears in plan documents.**

Worked example (chata_data_2):
- User request: *"Ok, Prepare a claude code plan what needs to be done with every document (check the flows docs session flow files ON EACH FLOW) to achieve these for EACH FLOW"*
- Decomposition: integration round 1 = plan what needs to be done per flow; integration round 2 = check session files on each flow; integration round 3 = achieve Tracks A-E for each flow
- Plan produced: `Execution order: Track A → Track B and Track C pre-phase (parallel) → Track C per-flow → Track D → Track E`
- Gate 0j verdict: BLOCK. Per-flow-first (user's order) silently inverted to per-track-first (plan's order) with no declaration.

Verdict: PASS | BLOCK.

### Gate 0k — Corrections from prior rounds threaded explicitly into plan text

This gate enforces Response Construction Protocol Step 3 (prior-correction thread) at plan-authoring time. The reviewer checks whether user corrections that shaped the plan appear as explicit references in the plan document.

**Procedure:**

1. Identify the user's prior corrections in the session that preceded this plan version.
2. For each correction, locate in the plan document a specific reference or structural element that addresses it.
3. Corrections with no plan-text reference fail the gate.
4. Corrections referenced generically ("we addressed feedback") without specific plan-element linkage fail the gate.

**This gate catches the this failure pattern (prior context not threaded) failure mode as it appears in plan documents.**

Verdict: PASS | PASS_WITH_CONCERNS | BLOCK.

### Gate 0l — Plan's citations have source-layer tags

This gate enforces Response Construction Protocol Step 5 (source-layer check) at plan-authoring time. The reviewer checks whether every citation in the plan document is tagged by source provenance.

**Procedure:**

1. Identify every quoted phrase, file reference, and external citation in the plan.
2. For each, verify the source layer is declared: `[codebase:file:line]`, `[docs:path]`, `[user-earlier-turn:timestamp]`, `[user-attached:filename]`, `[state:field]`, `[prior-plan:version]`.
3. Citations without source-layer tags fail the gate.
4. Citations whose weight depends on source layer (user direction vs prior plan vs codebase) must have the tag adjacent to the claim.

**This gate catches the this failure pattern (source-layer confusion) failure mode as it appears in plan documents.**

Verdict: PASS | FAIL.

---

### Gate 0m — UI/UX compliance for React page plans

This gate enforces the interface compliance skill (UI/UX Compliance) and the interface quality check at plan-authoring time. It applies to any plan that includes steps producing React pages (`*.tsx` files in `client/src/pages/`).

Gate 0m is the last Tier 1 structural pre-check. It verifies the plan's *intent* is UX-correct — not that the implementation is done (that is Phase 7 at execution time), but that the plan as written does not structurally commit to a UX violation before execution begins.

**Procedure:**

**Step 1 — Identify all pages planned.** Scan the plan for steps that create or modify `*.tsx` files, add `<Route>` entries to `App.tsx`, or produce new user-visible surfaces. List as P1, P2, P3...

**Step 2 — Role question check.** For each planned page, verify the plan answers all four role questions from the interface compliance skill §1:

```
Q1 ROLE_TIER: declared? [PLATFORM_ENG | PLATFORM_OPS | TENANT_OPS | TENANT_CONSUMER | PUBLIC]
Q2 ROLE_GATE: primary role named from the interface compliance skill §3 taxonomy?
Q3 ROUTE_GUARD: declared route matches declared role tier?
 /admin/... → PLATFORM_ENG or PLATFORM_OPS only
 /settings/ → TENANT_CONSUMER or PUBLIC (user self-service)
 /<path> → TENANT_CONSUMER or PUBLIC
Q4 VISIBILITY: visibility scope declared from the interface compliance skill §4 registry?
```

Missing answers on any of Q1–the output contract question → BLOCK.

**Step 3 — Run the structural BLOCK check matrix.** For each planned page, the reviewer checks these conditions against the plan text:

| Check | BLOCK condition |
|---|---|
| UX check 06b | Plan specifies PNG capture without `?mock=` param on TENANT_CONSUMER or PUBLIC page |
| UX check 16 | Plan's declared route for the page does not match the declared role tier |
| UX check 17 | Plan describes showing raw HTTP error codes to tenant or public users |
| UX check 18 | Plan renders same UI to all roles where role-conditional rendering is required |
| UX check 19 | Plan routes a user-rights page (consent, privacy, language preference) under `/admin/` |
| UX check 20 | Plan publishes a form with a `publicUrl` field but adds no `/forms/:schemaId` route |
| UX check 22 | Plan adds a page for a no-bypass flow (FLOW-24, 37, 44) without a non-dismissable no-bypass badge |
| UX check 23 | Plan adds a manual action button to a fully automated gate (5-arbiter, safety, design deployment) |
| UX check 24 | Plan's human-in-the-loop page spec omits any of the 4 mandatory fields (item, requester, SLA, decision form) |
| UX check 25 | Plan marks a page `TENANT_FACING` but backs it with generic admin CRUD (`/api/dynamic/xiigen-*`) |
| UX check 26 | Plan adds OAuth integration without specifying both pending-state and return-state screens |
| UX check 27 | Plan's human override form has no mandatory written justification field |
| UX check 29 | Plan's consent-pending state has no approve/deny affordance |
| UX check 30 | Plan adds a TENANT_CONSUMER or PUBLIC page without declaring a grammar type (G1–G7); or declares a grammar type but the page specification describes a generic CRUD table backed by `/api/dynamic/xiigen-*` |

**Missing page registry check:** if the plan implements or extends FLOW-20, FLOW-21, FLOW-28, or FLOW-48, verify the plan includes the corresponding missing page from the interface compliance skill §6:

| Flow | Required page | BLOCK if absent |
|---|---|---|
| FLOW-20 | `/settings/privacy` | Plan adds `ConsentGateEnforcer` work but no `/settings/privacy` route |
| FLOW-21 | `/forms/:schemaId` | Plan publishes a form with `publicUrl` but no submitter route |
| FLOW-28 | `/blog` and `/blog/:slug` | Plan adds `PublicPageRequestPipeline` work but no public blog routes |
| FLOW-48 | `/settings/language` | Plan adds `UserPreferencesManager` work but no language preferences route |

**Step 4 — Template check.** If any planned page matches a screen template pattern (screen template 1..screen template 7 in the interface compliance skill §5), verify the plan references the template. Template match without reference → CONCERN, not BLOCK.

**Step 5 — Phase 7 declared.** Verify that for every React pages phase in the plan, a corresponding Phase 7 (UI/UX Compliance) step is declared after it. Phase 7 absent → BLOCK.

**This gate catches the six structural the interface quality check failure modes (failure mode 1..failure mode 6 from `fc-18-ui-ux-compliance-gate.md`) before execution, not after.**

Verdict: PASS | CONCERN | BLOCK. On BLOCK, specify the check (UX-XX), the page (P-N), and the plan step that produced the violation.

---

## 5. Tier 1 verdict aggregation

Tier 1 produces an aggregate verdict:

- **REJECTED** — any BLOCK in 0a, 0b, 0c, 0d, 0e, 0f, 0g, 0h, 0j, 0k, 0l, or 0m
- **REJECTED** — BLOCK in 0i after documented Step 2 returned nothing
- **APPROVED_WITH_CONCERNS** — any PASS_WITH_CONCERNS in 0i or 0k, or CONCERN in 0m
- **APPROVED_TIER_1** — all gates PASS

Only APPROVED_TIER_1 or APPROVED_WITH_CONCERNS proceeds to Tier 2.

---

## 6. Tier 2 — Correctness checks (the count accuracy check through the interface quality check)

Tier 2 runs the failure-class battery. Order within Tier 2: the goal delivery check, the design artifact content check, the architect behavior check, the response construction check, the interface quality check run FIRST (structural correctness), then the count accuracy check through the documentation completeness check (content correctness).

### the goal delivery check — Goal Delivery Completeness (the goal delivery check skill)

Goal elements decomposed from the goal context record.statement. Each goal mapped to ≥1 plan turn with a verification step. Per-goal verdict: APPROVED | rejected — goal element not covered | rejected — no way to verify | CHALLENGE.

Any BLOCK → plan rejected before the count accuracy check..the documentation completeness check execute.

### the design artifact content check — Design Artifact Populated (the design artifact completeness check)

For every artifact referenced by the plan:
- the design artifact completeness check Checks 1-2: files exist + fields populated
- Checks 3-4: surface mismatches (don't block, produce CONCERN)
- Check 5: informational

Any artifact failing Checks 1-2 → plan rejected or must add enrichment task.

### the architect behavior check — Architect Habits Discipline (the architect habits guide v1.2.0)

Authoring-time cross-check of Gate 0i from Tier 1. the architect behavior check ensures the same three-step doc-first loop that Gate 0i ran at review time was also run at plan-authoring time — i.e., the plan author documented their doc-search evidence in the plan.

Verdict: PASS if authoring-time evidence present; CONCERN if Tier 1 Gate 0i passed but authoring evidence missing; BLOCK if both absent.

### the response construction check — Response Construction Protocol Compliance

For every substantive response in the plan-authoring session that led to this plan:
- Step 1 decomposition artifact exists (in session internal record or plan provenance)
- Step 2 absorption artifact exists if full mode
- Step 3 prior-correction thread artifact exists if full mode
- Step 5 source-layer tags on all citations in response
- Step 6 feedback recheck verdict ADDRESSED for every correction declared addressed

Cross-reference: the response construction check at plan review time overlaps with Gates 0j (decomposition enforcement), 0k (correction thread enforcement), and 0l (source-layer enforcement) from Tier 1. Tier 1 catches the failure in the plan document; the response construction check at Tier 2 catches the failure in the session record that produced the document.

Verdict: PASS | CONCERN | BLOCK.

### the interface quality check — UI/UX Compliance (the interface compliance skill v1.1.0) *(updated v1.8)*

Tier 2 cross-check of Gate 0m from Tier 1. While Gate 0m verifies the plan's *structural intent* is UX-correct, the interface quality check at Tier 2 verifies the plan author ran the the interface compliance skill role question check, the business-intent sub-check, and the BLOCK matrix at authoring time — i.e., the plan document contains evidence that all four business-intent elements were declared and the BLOCK check matrix was consulted.

**the interface quality check Tier 2 check procedure:**

1. Verify the plan document contains role declarations for each React page (Q1–the output contract question per the interface compliance skill §1). If Gate 0m passed but no role declaration appears in the plan text, the interface quality check flags CONCERN.

2. Verify the plan contains all four business-intent elements for each React page (from Gate 0g business-intent sub-check): examination record finding, user intent source, role-visibility source, grammar declaration. If any element is missing, the interface quality check flags BLOCK.

3. Verify the plan references the interface quality check or the interface compliance skill in its Phase 7 specification, or includes an explicit statement that Phase 7 was considered. A plan that passed Gate 0m structurally but contains no mention of the interface quality check compliance work → CONCERN.

4. If the plan implements any of the four missing-page flows (FLOW-20/21/28/48), verify the missing page creation step appears explicitly in the plan with its correct route path.

5. If the plan uses a screen template (screen template 1..screen template 7), verify the template name is stated in the plan alongside the page specification.

6. Verify the plan declares a grammar type (G1–G7) for each TENANT_CONSUMER or PUBLIC page, and that the grammar type matches the examination record or `.impeccable.md` where those exist. A plan that declares grammar G3 CARD_LIST but specifies a CRUD table as the page content fails.

Cross-reference: the interface quality check at Tier 2 overlaps with Gate 0m at Tier 1. Gate 0m catches structural violations; the interface quality check at Tier 2 catches the absence of business-intent and UX compliance authoring-time evidence in the session record.

Any BLOCK finding in the interface quality check → plan rejected before the count accuracy check..the documentation completeness check execute.

Verdict: PASS | CONCERN | BLOCK.

### the count accuracy check — Goal coverage (content-level)

Every user-stated goal has a plan element addressing it with sufficient specificity to execute.

### the path verification check — Evidence grounding

Plan claims about codebase state are grounded in grep/view results cited in the plan. Evidence index references the reconnaissance tracking record lines.

### the skill order check — Architectural coherence

The plan's architectural decisions are internally consistent. No turn contradicts another turn's premise.

### the pipeline contract check — Pipeline contract

Plan respects pipeline position question/b/c/d from the pipeline position check skill. What the plan's output stage produces matches what the next stage consumes.

### the naming convention check — Naming convention compliance (the naming conventions skill)

File paths, slug references, and domain names match the canonical table.

### the locked rules check — Iron rules not silently overridden

Every iron rule referenced in the plan is either honored or explicitly flagged as overridden with Luba-approval timestamp.

### the DNA compliance check for generated code — DNA compliance for GENERATION plans

Generated code specifications in the plan honor coding rule 1 through coding rule 9 patterns.

Applies to: plans that instruct Claude Code to generate new code (GENERATION session type,
new task-type contracts, new service files, new topology nodes).

Verdict: PASS | CONCERN | BLOCK.

---

### the DNA compliance check for remediation plans — DNA compliance for REMEDIATION plans *(NEW v1.9)*

For plans that describe changes to existing code (remediation, gap-closure, refactoring,
module-separation, import-path updates, pattern migration): every proposed solution
pattern must be verified against the four critical DNA rules before the plan can execute.

**What "proposed solution pattern" means:**
Any sentence of the form "remove X," "replace X with Y," "add explicit Z," "migrate from A
to B," or "apply GAP-NN to FLOW-XX" is a proposed solution pattern. It implicitly commits
downstream code to a specific implementation shape. the DNA compliance check for remediation plans requires that shape to be
verified against the four DNA rules before the plan author asserts it is correct.

**The four critical DNA rules for remediation plans:**

1. **coding rule 5 — Tenant scoping via ALS**
 Any proposed change to tenantId handling must preserve ALS-based scoping.
 Common failure: "add explicit tenantId parameter" removes ALS-based scoping and
 replaces it with parameter passing — a coding rule 5 violation that compiles and passes
 unit tests but creates tenant isolation holes under concurrent requests.
 Evidence required: read the actual service file (`[filepath:grep-result]`); confirm
 current scoping mechanism (ALS lookup or parameter); state why the proposed change
 preserves rather than replaces the current mechanism.

2. **coding rule 7 — Fabric-first external access**
 Any proposed change involving external service access must route through fabric
 interfaces. IScopedMemoryService not IRedisService. ISchedulerService not Bull
 directly. ICodeRepositoryService not GitHub MCP.
 Common failure: proposed fix accesses Redis directly to "simplify the implementation"
 — a coding rule 7 violation that makes the service non-portable and breaks the swap protocol.
 Evidence required: confirm no direct provider import in the proposed change; confirm
 the fabric interface exists for the service type (`grep -n "IFabricInterface" server/src/`).

3. **coding rule 8 — Outbox-before-queue**
 Any proposed change that adds or modifies a queue operation must store the document
 before enqueuing it. The outbox write must be ORDER 1 in any handler that enqueues.
 Common failure: proposed handler adds enqueue as a new step without specifying that
 the existing storeDocument call precedes it — or adds a new enqueue without adding a
 preceding storeDocument.
 Evidence required: if change adds or modifies an enqueue, state explicitly:
 "storeDocument at line N precedes enqueue at line M in the proposed code."

4. **coding rule 3 — DataProcessResult return type**
 Any proposed handler change must return DataProcessResult, not a raw object or void.
 Common failure: proposed handler method returns `Promise<boolean>` — compiles, passes
 tests, breaks DPO pipeline.
 Evidence required: proposed handler return type stated explicitly in the plan.

**Evidence standard for the DNA compliance check for remediation plans:**
- IMPLEMENTATION evidence: actual file read with grep result, or proposed code block
 showing the specific implementation. File path + line range required.
- TEST evidence: existing test that confirms the behavior the proposed change preserves.
- DESIGN_DOC evidence — RECONCILIATION-STATE, FIX-PLAN versions, session notes, arbiter
 verdicts — does NOT satisfy the DNA compliance check for remediation plans. Design documents describe intent, not implementation.
 A remediation plan backed only by DESIGN_DOC evidence is a plan that has not been
 verified against the code it proposes to change.

**Scope:**
the DNA compliance check for remediation plans applies to every flow-level proposed change in a remediation plan.
"Apply tenant context service decoupling to FLOW-05" is one application of the DNA compliance check for remediation plans.
"Apply tenant context service decoupling to FLOW-05 through FLOW-20" is sixteen applications of the DNA compliance check for remediation plans.
Each flow requires its own evidence citation — shared evidence ("same pattern applies")
is acceptable only when the referenced service file for each flow has been individually
read and confirmed to match.

**Failure mode this gate catches (corpus evidence):**
module-separation fix plan v2.0–v4.9: proposed "remove ClsService, add explicit tenantId"
for 15 flows across six arbiter passes (A through F, 20 corrections per pass). No pass
applied the DNA compliance check for generated code or the DNA compliance check for remediation plans. No service file for any of the 15 flows was cited as IMPLEMENTATION
evidence. DNA violations were found at implementation review — after the plan had accumulated
18 versions and the executor began running it. One the DNA compliance check for remediation plans review pass at v2.0 would have
caught the coding rule 5 gap before the plan grew to v4.9.

Verdict: PASS (all four DNA rules verified with IMPLEMENTATION evidence for each
 proposed change) | CONCERN (some proposed changes lack evidence — reviewer lists
 which by flow and DNA rule) | BLOCK (proposed pattern demonstrably violates a DNA
 rule — specific violation cited with file:line reference).

### the DNA compliance check (detection before scope claim) — Detection before scope claim *(NEW v2.0)*

Any plan that claims "N flows affected" or "all flows require this change" must show
the detection command and its output before listing fix steps.

**What "scope claim" means:**
- "tenant context service decoupling affects FLOW-05 through FLOW-20" — scope claim for 15 flows
- "All plain TypeScript flows have local interface clones" — scope claim for a flow cluster
- "The full fleet has unscoped FREEDOM keys" — universal scope claim

**Required for the DNA compliance check (detection before scope claim) to PASS:**

```bash
# Example: plan claims "tenant context service decoupling affects 23 flows"
# Required detection output in plan (before any fix steps):

grep -rl "import.*ClsService\|from 'nestjs-cls'" \
 server/src/engine/flows/ --include="*.service.ts" | wc -l
# Output: 23

# OR per-flow detection:
for FLOW in flow-01-user-registration flow-08-sharable-flows ...; do
 COUNT=$(grep -rc "import.*ClsService" server/src/engine/flows/$FLOW/ \
 --include="*.service.ts" 2>/dev/null | awk -F: '{sum+=$2} END {print sum+0}')
 [ $COUNT -gt 0 ] && echo "$FLOW: $COUNT hits"
done
```

**Universal claim rule:** A plan claiming "all flows" or "full fleet" must have run
detection against 100% of flows — not a representative sample. If detection was run
on 6 of 49 flows, the scope is 6 flows — not 49.

**Failure mode this gate catches:**
gap 05 from gap mapping round 3: plans claimed full-fleet scope for portability gaps based on
manual inspection of 3-5 representative flows. Remediation sessions launched for 49
flows had IMPLEMENTATION evidence from 6. The remaining 43 had design-doc evidence only.

Verdict: PASS (detection output present and matches claimed scope) |
CONCERN (detection missing for some claimed flows — list which) |
BLOCK (claimed scope exceeds detected scope — reduce scope to match detection output).

---

### the DNA compliance check (new-row arbiter re-run) — New rows trigger arbiter re-run *(NEW v2.0)*

When a plan version adds new per-flow rows that were not present in the prior reviewed
version, the full arbiter battery (the count accuracy check through the documentation completeness check, the DNA compliance check for generated code/b/c) must run on those
new rows before the plan version is approved.

**What counts as "new rows":**
- FLOW-40..48 added to a plan originally written for FLOW-01..39
- A new gap added to an existing gap-by-gap plan
- A new phase added to an existing implementation plan

**The rule:**
Adding rows to a reviewed plan creates an unapproved plan for those rows.
The prior plan version remains approved for its original scope.
The new rows require their own review pass.

**Detection:**

```bash
# Reviewer: identify new rows by diffing plan versions
diff PLAN-v3.md PLAN-v4.md | grep "^+" | grep "FLOW-\|GAP-\|Phase "
# Each line added is a new row requiring review
```

**Why this matters — corpus evidence:**
gap 11 from gap mapping round 3: FLOW-40..48 were added to the module-separation fix plan
across versions v2.0 through v4.9. The B-arbiter (the plan's own custom reviewer) was
never re-run after these 9 flows were added. For 10 plan versions, the most important
new flows in the fleet had no arbiter review. the DNA compliance check (new-row arbiter re-run) would have caught this at v2.1
when FLOW-40 was first added.

**Exemption:** Minor editorial changes (typo fixes, clarification of an existing row)
do not require a full re-run. The test: does the editorial change alter what Claude
Code will execute for any flow? If yes → re-run required.

Verdict: PASS (new rows reviewed in this pass or no new rows added) |
BLOCK (new rows present but not reviewed — list which by flow/gap/phase).

---

### the definition-of-done protocol check — Definition of done includes protocol status + the authentication deployment prerequisite check *(NEW v2.1)*

For any implementation plan (FLOW-XX-IMPLEMENTATION-PLAN or equivalent), the plan's
definition of done must include all three protocol gate phases, or declare explicit N/A
for each. A plan with no Phase G, H, or I and no N/A declarations has an incomplete
definition of done for the platform's certification and security requirements.

**the portability gate check — Portability gate declared:**

The plan must contain one of:
- Phase G (Portability Mobility Gate) as a declared phase with portability check gate steps (portability check 1..portability check 5 + no SDK imports outside provider files)
- An explicit statement: "Phase G N/A — external repository adapter adapter" (only for adapter flows with no monorepo services)

Detection: scan plan for "Phase G" or "portability status" or "Phase G N/A".

Failure example: plan has Phases A-F with no Phase G and targets distribution.
Failure consequence: flow ships ACTIVE with portability status = not yet determined; cannot reach Tier A.

Verdict: PASS | BLOCK.

**the authentication declaration check — Auth decoration gate declared:**

The plan must contain one of:
- Phase H (Auth Decoration) as a declared phase with authentication check 1/authentication check 2/authentication check 3 checks and Rule 7 tests
- An explicit statement: "Phase H N/A — no HTTP controllers"
- An explicit authentication deferred (not yet deployed) declaration: "Phase H N/A — authentication deferred (not yet deployed): auth.module.ts absent; decorate in Phase H after AUTH-PLAN v3 Phases 1-4"

Detection: scan plan for "Phase H" or "authentication status" or "Phase H N/A" or "authentication deferred (not yet deployed)".

Failure example: plan has HTTP controllers, no Phase H, no authentication deferred (not yet deployed) declaration.
Failure consequence: controllers ship unguarded; authentication status = not yet determined blocks goal is met.

Verdict: PASS | BLOCK.

**the certification plus the authentication deployment prerequisite check check — Certification tier and the authentication deployment prerequisite check declared:**

The plan must contain one of:
- Phase I (Tenant Certification Status) as a declared phase with certification tier check gate steps and tenantCertTarget
- An explicit statement: "Phase I N/A — not distributable"

Additionally, if the plan targets Tier C or Tier D, it must include the authentication deployment prerequisite check evidence:
```
the authentication deployment prerequisite check check:
 auth.module.ts present: [YES | NO]
 scope-enrichment.interceptor.ts: [YES | NO]
 APP_GUARD or JwtAuthGuard in app.module.ts: [YES | NO]
 role-strings.ts present: [YES | NO]
 the authentication deployment prerequisite check score: N/4
```
Score < 4/4 → plan must cap tenantCertTarget at Tier B until the authentication deployment prerequisite check is satisfied.
Claiming Tier C without the authentication deployment prerequisite check evidence is a BLOCK.

Detection: scan plan for "Phase I" or "tenantCertTarget" or "Phase I N/A" or "not distributable".
For Tier C claims: look for the authentication deployment prerequisite check score block.

Failure example: plan declares `tenantCertTarget: Tier C` with no the authentication deployment prerequisite check evidence.
Failure consequence: Phase I Step 6c (flow-lifecycle v1.1.0) exits 1 at execution time.

Verdict: PASS | CONCERN (missing Phase I but also not distributable not declared — ambiguous)
 | BLOCK (Tier C claim without the authentication deployment prerequisite check evidence, or no Phase I and distributable flow).

**the definition-of-done protocol check combined verdict:**
PASS if the portability gate check + the authentication declaration check + the certification plus the authentication deployment prerequisite check check all PASS.
BLOCK if any of the portability gate check, the authentication declaration check, the certification plus the authentication deployment prerequisite check check is BLOCK.
CONCERN if any is CONCERN and none is BLOCK.

---

Plan specifies test gates with concrete pass criteria (numbers, not qualitative descriptions).

### the issue inventory check — Issue inventory present

Plan includes ISSUE-INVENTORY section: FIXED / DEFERRED+CARRY-FORWARD / EXCEPTION.

### the cross-document propagation check — Cross-document propagation

Single plan document; no stale copies. If the plan supersedes a prior version, the prior version is explicitly marked as superseded.

### the overview-detail consistency check — Overview-detail match

Each turn's one-line goal and its code specification are consistent.

### the principles compliance check — Principles compliance

coding rule 1, coding rule 5, coding rule 8, the cross-flow scope rule (missing knowledgeScope defaults to PRIVATE), and other platform principles honored in the plan's code blocks.

### the documentation completeness check — Documentation artifacts per Rule 17

For every phase the plan produces, the corresponding Rule 17 documentation (business topology JSON + MD companion, design-reasoning JSON, contract JSON, etc.) is specified as an output.

---

## 7. Tier 2 verdict aggregation

Tier 2 produces the final verdict:

- **REJECTED** — any BLOCK in the goal delivery check, the design artifact content check, the architect behavior check, the response construction check, the interface quality check, the definition-of-done protocol check, or the count accuracy check through the documentation completeness check
 (including the DNA compliance check for generated code, the DNA compliance check for remediation plans, the DNA compliance check (detection before scope claim), the DNA compliance check (new-row arbiter re-run), and the portability gate check/b/c)
- **APPROVED_WITH_CONCERNS** — any CONCERN in any FC gate
- **APPROVED** — all gates PASS

---

## 8. Verdict output format

Reviewer produces a single document: `PLAN-REVIEW-VERDICT-v{N}.md`

Structure:

```
# Plan Review Verdict — [plan name] v[version]
## Reviewer: [session identifier]

## Tier 1 — Structural pre-checks

| Gate | Verdict | Notes |
|------|---------|-------|
| 0a | PASS | |
| 0b | PASS | |
| 0c | PASS | |
| 0d | PASS | |
| 0e | PASS | |
| 0f | PASS | |
| 0g | PASS | Phase 7 sub-check: Phase 7 declared ✓; Business-intent sub-check: all 4 elements present ✓ |
| 0h | PASS | |
| 0i | PASS | 0 habit hits above Class-c |
| 0j | PASS | User items addressed in stated order |
| 0k | PASS | Corrections threaded explicitly |
| 0l | PASS | All citations source-tagged |
| 0m | PASS | Role tier declared for all 2 pages; UX-30 grammar declared; Phase 7 step present |

Tier 1 aggregate: APPROVED_TIER_1

## Tier 2 — Correctness checks

| FC | Verdict | Notes |
|----|---------|-------|
| the interface quality check | PASS | All 4 business-intent elements present; grammar declared; examination record cited |
| the goal delivery check | PASS | All goal elements mapped |
| the design artifact content check | PASS | All referenced artifacts populated |
| FC-7a | PASS | [if GENERATION plan] Generated code specs honor coding rule 1..coding rule 9 |
| the DNA compliance check for remediation plans | PASS | [if REMEDIATION plan] coding rule 5/7/8/3 verified with IMPLEMENTATION evidence for each proposed change |
| the DNA compliance check (detection before scope claim) | PASS | Detection output present and matches claimed scope (N flows detected = N flows in plan) |
| the DNA compliance check (new-row arbiter re-run) | PASS | New rows reviewed in this pass OR no new rows added since prior reviewed version |
| the definition-of-done protocol check | PASS | the portability gate check: Phase G or N/A declared ✓; the authentication declaration check: Phase H or N/A/authentication deferred (not yet deployed) declared ✓; the certification plus the authentication deployment prerequisite check check: Phase I or not distributable declared ✓; the authentication deployment prerequisite check score adequate if Tier C target ✓ |
| [the count accuracy check..the documentation completeness check excluding the code quality check] | PASS | |

Tier 2 aggregate: APPROVED

## Final verdict: APPROVED
```

---

## 9. "How the reviewer writes" — voice and structure

The reviewer's writing follows four principles observed across productive review sessions:

### 9.1 Findings, not prose

The verdict is a table of gates and a list of findings. Not narrative prose. A reviewer who writes five paragraphs of analysis instead of a verdict table has drifted into architect mode — a different role.

### 9.2 Evidence attached to every claim

Every finding cites the specific line in the plan that triggered it. Vague findings ("the plan seems to overreach") fail Gate 0l if the reviewer emits them — same standard applies to the reviewer's own output as to the plan being reviewed.

### 9.3 Stop here and wait for approval after verdict

The reviewer's document ends with a verdict and a STOP. No recommendations disguised as verdicts. No "I think this could be improved by..." tacked on. The reviewer's job is verdict + required corrections, not design suggestions.

### 9.4 Grade format, not conversational format

The verdict is the output. The reader (plan author, Luba) reads the verdict and acts. The reviewer does not open a dialog — "would you like me to elaborate on Gate 0j?" — unless explicitly asked.

---

## 10. When the reviewer themselves trips a habit

Reviewer output is subject to the same the architect habits guide catalog. Common reviewer failures:

- **this failure pattern (claims without evidence):** the verdict cites a gate failure without pointing to the plan line. Fix: every gate-level BLOCK names the plan section that triggered it.
- **this failure pattern (altitude-as-excuse):** "at review level, I'm flagging this as a concern" without explaining what the concern is. Fix: concerns are specific.
- **this failure pattern (catalog-vocabulary):** reviewer labels a finding "this is this failure pattern" without quoting the user correction that didn't thread. Fix: quote the correction, then name the habit.
- **this failure pattern (no recheck):** reviewer acknowledges plan revision but doesn't verify the revision addresses the prior BLOCK. Fix: before changing a verdict from BLOCK to PASS, verify the specific gate condition now holds.

The reviewer runs the three-step doc-first loop on their own verdict before emission, same as the plan author does on their plan.

---

## 11. Versioning

- v1.0 — initial protocol, Gates 0a-0f plus the count accuracy check..the documentation completeness check
- v1.1 — added Gate 0g visual proof
- v1.2 — added Gate 0h iron rules
- v1.3 — added Gate 0i architect habits (the architect habits guide v1.0.0)
- v1.4 — extended Gate 0i for the architect habits guide v1.1.0, added scope-in/out boundary
- v1.5 — added three Gate 0i sub-checks (plan-cites-inputs, shape-match-at-close, cluster-aware scan), added Gate 0g visual-proof sub-check, added "How the reviewer writes" section
- v1.6 — added Gates 0j (user-order preservation), 0k (correction-thread), 0l (source-layer tags); added the response construction check Response Construction Protocol Compliance; added reviewer self-habits section; cluster-aware scan subcheck kept in Gate 0i
- v1.7 — added Gate 0g the interface compliance skill sub-check (Phase 7 must be planned for React pages phases); added Gate 0m (UI/UX compliance structural pre-check — role questions Q1–the output contract question, 12-item BLOCK matrix, missing-page registry, template check, Phase 7 declared); added the interface quality check UI/UX Compliance (Tier 2 cross-check of Gate 0m); updated Tier 1 and Tier 2 verdict aggregation; updated verdict output format table
- v1.8 — Gate 0g business-intent sub-check added (4 elements required before any React page implementation task: examination record finding, user intent source, role-visibility source, grammar declaration G1–G7); this open issue halt condition added for FLOW-04/09/34; Gate 0m BLOCK matrix updated with UX check 06b and UX check 30; the interface quality check Tier 2 updated to verify business-intent elements and grammar declaration; FM count updated (failure mode 1..failure mode 6); verdict format table updated to show Gate 0g two-sub-check format; cross-references updated
- v1.9 — the code quality check split into the DNA compliance check for generated code (GENERATION plans, unchanged behavior) and the DNA compliance check for remediation plans (REMEDIATION plans — four critical DNA rules, IMPLEMENTATION evidence required per proposed change, DESIGN_DOC evidence explicitly rejected); scope line extended to include REMEDIATION/gap-closure/architectural-fix plans; Tier 2 verdict aggregation and verdict format table updated; corpus origin: module-separation fix plan v2.0–v4.9, coding rule 5 violation in 15 flows caught at implementation not at plan review
- v2.0 — the DNA compliance check (detection before scope claim) added (detection before scope claim; universal claims require 100% coverage); the DNA compliance check (new-row arbiter re-run) added (new rows trigger arbiter re-run); Tier 2 aggregation and verdict format updated
- v2.1 — the definition-of-done protocol check added: three sub-checks the portability gate check (portability/Phase G), the authentication declaration check (auth/Phase H), the certification plus the authentication deployment prerequisite check check (cert tier/Phase I + the authentication deployment prerequisite check for Tier C); Tier 2 aggregation and verdict format updated; corpus origin: all 49 IMPL-the state file files show portability status=not yet determined/authentication status=not yet determined/tenant certification tier=NONE — no prior plan version included Phase G/H/I

---

## 12. Observability

For any plan review using this protocol:

- Verdict document exists with Tier 1 and Tier 2 tables
- Every BLOCK has a plan-section citation
- Every required correction has a specific fix description
- Reviewer's own output passes the same source-layer check it applied to the plan (Gate 0l)
- No verdict shipped with reviewer's own draft unchecked against the architect habits guide catalog

Verdicts that violate these properties are out of protocol compliance and should be re-run.

---

## 13. Cross-references

- **the architect habits guide** — architect habits catalog, used by Gate 0i
- **the goal delivery check skill** — goal delivery completeness, enforced by the goal delivery check
- **the design artifact completeness check** — design artifact completeness, enforced by the design artifact content check
- **the interface compliance skill v1.1.0** — UI/UX compliance (31 checks UX check 01..UX check 30, 52-role taxonomy, 7 screen templates, 4 missing pages, 7 grammar types G1-G7, Section 0 pre-design gate), enforced by Gate 0m and the interface quality check
- **the interface quality check v1.1.0** — UI/UX compliance gate (`fc-18-ui-ux-compliance-gate.md`) — canonical source for Gate 0m BLOCK matrix (including UX check 06b + UX check 30), Audit Trail format, and exemption rules; failure mode 1..failure mode 6 reference for Gate 0m
- **RESPONSE-CONSTRUCTION-PROTOCOL v1.0** — enforced by the response construction check at Tier 2 and Gates 0j/0k/0l at Tier 1
- **HOW-TO-USE-SKILLS v4.5.0** — Rule 33 mandates protocol; Rule 35 mandates UI/UX compliance sequence; Mandatory Check 15 enforces the interface quality check at Stop here and wait for approval
- **DESIGN-REVIEW-PROTOCOL v1.5** — fleet-level aggregation of per-plan findings; Signal 12 corresponds to the interface quality check; Signal 13 corresponds to Gate 0g business-intent sub-check + the interface quality check grammar verification
- **FLOW-DOCUMENT-AUTHORING-GUIDE v1.16** — Q8 (examination record check + WHO/VERB/GRAMMAR before JSX) mirrors Gate 0g business-intent sub-check; Rule 35 (Screen Intent Anchor) mirrors Gate 0g; Phase 7 at authoring time mirrors Gate 0m and the interface quality check
- **XIIGEN-DESIGN-ARCHITECT-SESSION-GUIDE v1.8** — Q10 (job-to-be-done, examination record as priority 1 source) mirrors Gate 0g business-intent sub-check; Mistake 23 (building screen without reading business spec) is the canonical Gate 0g failure example
- **planning--business-flows-registry.md** — pre-declared grammar types and this open issue flags; Gate 0g consults this for FLOW-04/09/34 halt condition
- **docs/screen-examination/{slug}-examination.md** — examination records; Gate 0g sub-check 1 requires plan cites primary finding and classification from this file where it exists
- **SESSION-LOAD-PLAN v31** — this open issue (Page rewrite for FLOW-36/37/38/39/40), this open issue (CLOSED), this open issue (OPEN), this open issue (OPEN — FLOW-04/09/34 halt)
- **the implementation plan guide-IMPLEMENTATION-PLAN v6.3** — Phase G (portability portability check gate), Phase H (auth decoration authentication check 1..authentication check 3 + Rule 7), Phase I (tenant cert tier + the authentication deployment prerequisite check + Tier C checklist); the definition-of-done protocol check verifies these phases are declared
- **pipeline--flow-lifecycle-SKILL v1.1.0** — Step 6a (portability status write), Step 6b (authentication status write), Step 6c (tenant certification tier write + the authentication deployment prerequisite check enforcement); the certification plus the authentication deployment prerequisite check check references the authentication deployment prerequisite check score
- **code-execution--dna-compliance-guard-SKILL v1.2.0** — authentication check 1..authentication check 3 auth checks verified by the authentication declaration check; plans that propose controller-creating steps without Phase H fail the authentication declaration check
- **planning--qa-session-type-SKILL v2.0.0** — Step 5b protocol completeness; the definition-of-done protocol check at plan-review time is the pre-execution gate; Step 5b at QA time is the post-execution gate
