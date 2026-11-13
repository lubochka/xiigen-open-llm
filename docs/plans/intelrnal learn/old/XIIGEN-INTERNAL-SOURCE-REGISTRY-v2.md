# XIIGen Internal Source Registry
# Version: 2.0 — 2026-05-08
# Supersedes: XIIGEN-INTERNAL-SOURCE-REGISTRY-v1.md

---

## What changed from v1

**Document type taxonomy added.** Six types of XIIGen internal documents are now
classified by their role in DPO pair construction: A-side authority, B-side authority,
correction source, measurement evidence, topology authority, and excluded material.
v1 had no taxonomy — documents were described individually without a shared classification
system, making it unclear how to handle a newly discovered document.

**Rejected construction rules expanded.** The four rejected types (A, B, C, D) now cover
all internal document types, not only ARCH and PLAN. Each type maps to the specific
internal document that provides its vocabulary.

**Hard negative construction rules added.** Session documents contain two classes of
hard negative that were not in v1: intra-domain confusion (same domain, wrong sub-phase)
and inter-domain confusion (wrong domain classification from overlapping signal words).
Both require different rejected construction strategies.

**Token overlap test linked to document type.** Each document type has a known typical
overlap ratio range. Using the wrong document type as rejected source inflates overlap
and produces negative margin.

**Session row classification rules added.** v1 described what rows to include and exclude
but not how to classify an ambiguous row. The classification rules in this version resolve
the most common ambiguous cases from the calibration run.

---

## Document type taxonomy

Every XIIGen internal document belongs to one of six types. The type determines how
it can be used in DPO pair construction.

---

### Type 1 — A-side authority: real developer prompts

**What it is:** Raw human messages from Claude export conversations or local session
files. Unedited. May contain typos, Hebrew, Russian, code fragments, partial sentences.

**Why it is A-side authority:** The model must learn to receive real developer input,
not idealised prompts. Only this type provides authentic input-language distribution.

**Examples in the XIIGen corpus:**
```
"fix the rxjs pipe in the Angular interceptor — I keep getting
 'object unsubscribed' after the first request"

"עזור לי עם המשך לחלק הבא — JOB שמחשב התאמות לזוגות"

"I don't want to use legacy peer I want to fix the problem
 npm install keeps failing with peer dependency conflict"

"Please generate flow 10 files that will include only these
 specific to flow 10 sections: P1Family 27 — Tena..."
```

**Eligibility test for PLAN and ARCH pairs** — at least one of:
- Named artifact: component name, phase number, flow ID, file name, technology name
- Dependency language: "before", "after", "depends on", "once X is complete", "first"
- Technical specificity: predicate vocabulary, sub-phase reference, stack name

**Messages that must never enter training as A sides:**
```
"Option A is clearly the right path"
"this is grounded in existing documents"
"let's continue with the plan"
"Continue."
"Read this."
"Please load the session."
"Based on the review and the response you've shared..."
"The plan is structured in five phases, each with a clear gate..."
"This is the right plan. It's grounded in the docs..."
```

These are meta-commentary. `CompiledPredicateIds` will be near-empty for them.
TrainingText built from these carries no routing signal. The calibration run
confirmed this: batches 30, 37 used meta-commentary rows from session 1235–1753
and produced mean accuracy 0.306–0.415.

**Overlap ratio when used as A side:** not applicable — A side is prompt input,
not paired content. The DPO signal comes from chosen/rejected token distributions
relative to this A side.

---

### Type 2 — B-side authority: formal XIIGen documents

**What it is:** Architecture decisions, session state documents, implementation
documentation, orchestrator teaching plans, dynamic flow design patterns,
and the canonical design pattern. Written in formal architecture vocabulary.

**Why it is B-side authority:** These documents define what the correct output looks
like. The sub-phase Purpose fields, dependency ordering rules, and arbiter criteria
in these documents are the ground truth for chosen responses.

**Examples in the XIIGen corpus:**
```
DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v3-5.md
SESSION-STATE-2026-05-07-v18.md
IMPLEMENTATION-DOCUMENTATION-v8.md
ORCHESTRATOR-TEACHING-PLAN-v3.md
DevelopmentLifecycleTopologySeeder.cs (sub-phase Purpose fields)
DECISIONS-LOCKED.md (A-series decisions)
```

**How to use for chosen response construction:**
- Sub-phase Purpose field → chosen content for 3.1, 3.2, 3.3 ARCH pairs
- Dependency ordering from teaching plan → chosen for PLAN Type B pairs
- Decision entries (A43–A66) → chosen for architectural trade-off pairs
- Locked decisions with reasoning → chosen for arbiter pass/fail criteria

**How NOT to use:** Never use as A side. Architecture document language produces
register mismatch: the model learns to expect formal vocabulary as input, which no
real developer ever provides.

**Overlap ratio when used as B side (chosen):** 0.10–0.20 against a Type 1 A side,
because the formal vocabulary of the document is largely non-overlapping with the
developer's surface words.

---

### Type 3 — Correction source: Claude export conversations

**What it is:** H0/A0/H1/A1 chains in export conversations where Claude produced a
wrong answer (A0) that the user corrected (H1) or where Claude revised its output
(A1). The correction pair is pre-made: A0 becomes rejected, A1 (or H1-derived) becomes
chosen, H0 (the original prompt) is the A side.

**Why this is the highest-value pair type:** The wrong answer (A0) came from a real
reasoning path, not from a constructed counter-example. The correct answer (A1) is
ground truth, not an approximation. The token distributions are naturally non-overlapping
because they came from different reasoning steps. Expected overlap ratio: below 0.25.

**The extraction pattern:**
```
Export conversation contains:
  H0  "design the data model for multi-tenant user preferences"
  A0  "Create a UserPreferencesRepository with GetByUserId(string userId)..."
  H1  "this is wrong — I need the entity shape, not the repository"
  A1  "Entity: UserPreference { tenantId (partition key), userId, key, value,
       updatedAt }. Partition key is tenantId to ensure tenant isolation at
       the storage layer."

Extracted pair:
  A:        H0 — "design the data model for multi-tenant user preferences"
  Chosen:   A1 — "Entity: UserPreference { tenantId (partition key)..."
  Rejected: A0 — "Create a UserPreferencesRepository with GetByUserId..."
  [WRONG: "design the data model" asks for entity shape and partitioning rules,
  not the repository class. The repository is 4.1 Backend Services work —
  it is built after the model is defined, not instead of it.]

  conversation_uuid: {uuid}
  human_message_index: 0
  quality_rating: High
```

**What makes an annotation strong (names three things):**
- The verb in H0 that signals the scope: "design" signals shape decisions, not code
- What the correct sub-phase actually owns: entity shape, partitioning rules
- Why rejected is in the wrong sub-phase: "repository is 4.1 work — built after, not instead"

**What makes an annotation weak (fails the margin test):**
```
[WRONG: this is the wrong sub-phase.]
[WRONG: rejected provides implementation instead of design.]
```

These add tokens but not distinguishing ones. The annotation must name the confusion
specifically enough that the tokens in the annotation do not also appear in rejected.

**Quality rating:**
```
High:   Chosen and rejected have clearly different vocabulary (overlap < 0.25)
        Include unconditionally.

Medium: Chosen and rejected address different responsibilities but share some
        vocabulary (overlap 0.25–0.50)
        Include with strong annotation.

Low:    Correction is only phrasing, not substance
        (chosen and rejected say the same thing differently, overlap > 0.50)
        Exclude.
```

---

### Type 4 — Measurement evidence: calibration run data

**What it is:** `dpo_step_*.json` files, `batch-summary.jsonl`, `progress.txt`,
checkpoint manifests from calibration runs.

**How to use:** Populate B sides for ModelTrainingUnit, HeldOutEvaluationUnit pairs.
The exact numbers (elapsed_ms, cpu_percent, working_set_delta_mb, checkpoint_bytes,
accuracy, loss) are the B-side content for teaching measurement pairs.

**How NOT to use:** Never derive A sides from run logs. Run log language is diagnostic
output, not developer input.

---

### Type 5 — Topology authority: seeder and registry code

**What it is:** `DevelopmentLifecycleTopologySeeder.cs`, `LogicalOrchestratorAssistant.cs`,
the 31 sub-phase registration blocks, and predicate lexicon definitions.

**How to use:** The `Name`, `Purpose`, and `PredicateIds` fields of each `LogicalSubPhase`
record are the canonical vocabulary for chosen responses in domain routing pairs and
DomainTranslatorUnit pairs. When writing a Type A rejected for sub-phase 3.1, the chosen
must use vocabulary from the 3.1 Purpose field. When writing a rejected, it must use
vocabulary from a different sub-phase's Purpose field.

**Sub-phase Purpose fields as vocabulary source for rejected construction:**
```
3.1 "component boundaries, dependency direction, lifecycle topology, responsibilities"
    → chosen vocabulary for ARCH boundary pairs

4.1 "service implementation, controller logic, dependency injection, framework calls"
    → rejected vocabulary when confused with 3.1

3.4 "authentication, authorisation, tenant isolation, JWT, RBAC, token validation"
    → rejected vocabulary when 3.1 boundary work is confused with security work

3.5 "metrics, traces, logs, alert thresholds, observability contracts"
    → rejected vocabulary when confused with 3.1 or 4.1
```

The rule: chosen uses Purpose vocabulary from the correct sub-phase. Rejected uses
Purpose vocabulary from the most plausible wrong sub-phase. Overlap ratio between
these two vocabulary sets is typically 0.10–0.25 for adjacent sub-phases.

---

### Type 6 — Excluded: document-loading and session-management messages

**What it is:** All prompt messages that load files, name documents, or manage
the session state without expressing a development intent.

**Examples:**
```
"Please load the session state."
"Read this document and confirm you understood it."
"Continue from where we stopped."
"What are the open threads?"
"Please update the document register."
```

**Why excluded:** These produce near-zero `CompiledPredicateIds`. The compiled text
carries no development signal. Any pair built from these A sides will have random
accuracy regardless of how well the rejected is constructed.

---

## Rejected construction rules by domain

The calibration run (2026-05-06) confirmed that 75% of batches had negative reward
margin because rejected was either a negation template or another correct sub-phase
response for the same prompt. Neither produces positive margin.

This section defines how to construct rejected for each domain using the document type
taxonomy above.

---

### BUG domain — rejected construction

BUG domain mean accuracy in the calibration run: 0.417. Root cause: multiple sub-phases
received the same prompt as chosen. The model saw "fix the rxjs pipe" routed to both
2.1 and 4.3 within the same training set and could not learn the distinction.

**Rule:** For any BUG A side, rejected must address a different responsibility class,
not a different sub-phase that the same prompt also legitimately owns.

**Type A rejected for BUG — wrong responsibility class:**

A BUG prompt that asks to fix a runtime error has a correct sub-phase (e.g., 4.1 Backend
Services). The rejected must address a completely different responsibility:

```
Prompt:   "wildcard fallback makes it skip azure ai search — please fix"
Domain:   BUG
Sub-phase chosen: 4.1 Backend Services
  "The wildcard route handler intercepts requests before the Azure AI Search
   route is matched. Fix: register the Azure AI Search route before the
   wildcard. In app.module.ts, move AzureAiSearchController registration
   above the fallback route. The router matches in registration order."

Sub-phase rejected: 3.3 API Contract Definition
  "Define the Azure AI Search endpoint contract: POST /api/search with
   request body { query: string, filters: object }. Document the response
   shape including result array, total count, and pagination tokens."
[WRONG: defining the API contract describes what the endpoint should accept
and return — it does not fix the routing registration order that causes
requests to be intercepted by the wrong handler. Contract definition is
3.3 work; fixing the wrong handler is 4.1 work.]
```

**Why this produces positive margin:** The chosen uses implementation vocabulary
(route handler, registration order, module, controller). The rejected uses contract
vocabulary (endpoint, request body, response shape, pagination). Near-zero overlap.

**Type A rejected for BUG — wrong diagnostic scope:**

```
Prompt:   "I don't want to use legacy peer — fix the npm install conflict"
Chosen:   "Add --legacy-peer-deps to .npmrc or switch the conflicting
           dependency to a compatible version. Check npm ls for the full
           dependency chain: the conflict is between..."
Rejected: "Design the dependency management policy: define which package
           versions the project accepts, document the rationale for pinning
           versus ranges, and establish a review gate for major version
           upgrades."
[WRONG: npm install is failing now — the immediate fix is a command or
config change, not a policy document. Policy design (3.1 / 2.2) follows
working dependencies; it does not unblock the broken install.]
```

**Hard negative for BUG — intra-domain confusion between BUG and REFACTOR:**

These are the hardest cases. The signal words overlap ("fix", "clean", "wrong"):

```
Prompt:   "the auth middleware runs twice on protected routes — fix it"
Chosen (BUG — 4.1):
  "The middleware is registered globally in app.module.ts AND individually
   on the route decorator. Remove the route-level @UseGuards() — the global
   registration handles all routes already."

Rejected (REFACTOR — 4.4 — plausible but wrong domain):
  "Extract the auth middleware into a dedicated AuthInterceptor class.
   Move the token validation logic out of the route handler and into
   the interceptor. Rename the existing middleware to reflect its actual
   scope."
[WRONG: the symptom is double execution from duplicate registration — not
disorganised code. Refactoring the interceptor class does not fix the
duplicate run. The fix is removing one registration. Code organisation
is a separate concern that can be addressed after the bug is resolved.]
```

The annotation here is critical: it names why REFACTOR vocabulary (extract, move,
rename, reorganise) does not fix the BUG (duplicate registration). Without the
annotation, overlap ratio is ~0.35 — borderline. With the annotation, the additional
tokens in chosen (duplicate, registration, removes, global) push the distributional
separation to produce reliable positive margin.

---

### CODE_Q domain — rejected construction

CODE_Q calibration result: 0% positive margin batches. All four batches had
overlap_ratio ~0.70 from negation template. The rejected must be a wrong domain
classification, not a non-answer.

**The core rule for CODE_Q:** A CODE_Q prompt asks "what does X do" or "how does Y
work". The rejected must be an action response — either a BUG fix or a PLAN step —
not another explanation.

**Type C rejected for CODE_Q — wrong domain (explanation vs action):**

```
Prompt:   "how does the replay buffer work in the capability trainer"
Chosen (CODE_Q — explain):
  "The replay buffer stores manifests from previous training steps for this
   domain adapter. On each training call, the trainer draws up to 3 manifests
   from the buffer (FindReplayManifests) and includes them alongside the new
   example. This prevents catastrophic forgetting: the model sees recent past
   examples on every step, not only the current one. Buffer growth is bounded
   at 4096 entries per DefaultMaxCapacity."

Rejected (BUG — wrong domain):
  "The replay buffer is growing without bound because MaxCapacity is not
   being enforced. Add a cap: after each Append(), check buffer.Count >
   _maxCapacity and call RemoveRange(0, buffer.Count - _maxCapacity).
   This prevents RAM growth across long calibration runs."
[WRONG: the prompt asks how the replay buffer works — what its mechanism
is and what problem it solves. The fix for unbounded growth is A54-level
work, not an answer to "how does this work". Answering a how-does-it-work
question with a bug fix addresses a different intent entirely.]
```

**Why this works:** Chosen uses explanation vocabulary (stores, draws, prevents,
catastrophic forgetting, bounded). Rejected uses fix vocabulary (growing without bound,
add a cap, prevents RAM growth). Overlap ratio: typically 0.15–0.25.

**Intra-domain confusion for CODE_Q — explanation vs demonstration:**

```
Prompt:   "show me how the DomainClassifierUnit classifies predicates"
Chosen (CODE_Q — explain mechanism):
  "DomainClassifierUnit.ClassifyBaseDomain checks two paths. If _trained
   is true and Predicates is non-null, it calls the KNN classifier on the
   predicate integer array. If _trained is false, it falls to
   ClassifyByKeywords — a static keyword scan of the raw prompt string.
   In the current code, _trained is always false because Train() is never
   called from PromptDomainClassifier — this is the A61 gap."

Rejected (CODE_Q — demonstrate usage, not mechanism):
  "Here is how to call DomainClassifierUnit:
   var unit = new DomainClassifierUnit();
   var request = new DomainClassifierRequest(prompt, Predicates: [0, 1, 2]);
   var result = unit.ClassifyBaseDomain(request);
   // result.Domain contains the predicted D-04 domain"
[WRONG: "how does it classify" asks for the decision logic — what conditions
trigger which path. A usage example shows the API surface but not the
internal decision. The prompt asks about the mechanism, not the call syntax.]
```

---

### PLAN domain — rejected construction

PLAN calibration result: 4 of 6 batches negative margin. Root cause: batches 30, 26,
37, 12 used meta-commentary prompts. Batches 16, 17 (positive) used named-artifact prompts.

**Type B rejected for PLAN — wrong dependency order (primary pair type):**

Use the dependency confusion table from `DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v2.md`.
Every annotation must explain WHY the wrong order fails, not just that it is wrong.

```
Prompt:   "plan the implementation of JWT authentication for the multi-tenant API"
Chosen (PLAN — correct order):
  Branch 1 — 3.4 Security and Authorization Model (dependency order 1):
    Define the JWT validation rules: signature algorithm (RS256), required
    claims (sub, tenantId, roles), expiry window (15min access, 7d refresh).
    This branch owns the contract that all other branches must satisfy.

  Branch 2 — 3.3 API Contract Definition (dependency order 2):
    Define the protected endpoints and their required roles.
    Dependency: 3.4 must be complete — the contract cannot specify role
    requirements until the role model is decided.

  Branch 3 — 4.1 Backend Services (dependency order 3):
    Implement the JwtValidationMiddleware, TokenService, and
    AuthorizationPolicyProvider.
    Dependency: 3.4 and 3.3 must be complete — implementation follows spec.

Rejected (PLAN — wrong order, all parallel):
  Branch 1 — 3.4 Security and Authorization Model
  Branch 2 — 3.3 API Contract Definition
  Branch 3 — 4.1 Backend Services
  All branches can run in parallel — start everything at once.
[WRONG: branch 3 implements the middleware against a JWT spec. If 3.4 is
not complete, the implementation uses assumptions that may conflict with the
final validation rules. Branch 2 cannot define protected endpoint contracts
until the role model from 3.4 is decided — the contract references roles
that do not yet exist. Parallel execution of these branches always produces
rework when 3.4 decisions change what 3.3 and 4.1 already built.]
```

**Type C rejected for PLAN — abstract phase labels instead of sub-phases:**

```
Prompt:   "plan the migration from REST to GraphQL for the reporting service"
Chosen (PLAN — specific sub-phases):
  Branch 1 — 3.3 API Contract Definition (dependency order 1):
    Define the GraphQL schema: query types, field names, pagination
    pattern (cursor-based), and error type. The schema is the contract
    that all downstream work must match.

  Branch 2 — 4.3 Infrastructure and Deployment Code (dependency order 2):
    Add the GraphQL server package, configure the schema registration,
    and add the playground endpoint behind a feature flag.

  Branch 3 — 5.2 Integration Tests (dependency order 3):
    Write integration tests against the new GraphQL endpoint using
    real query strings. Dependency: schema must be finalised.

Rejected (PLAN — abstract labels):
  Phase 1: design the GraphQL migration.
  Phase 2: implement the migration.
  Phase 3: test the migration.
  Phase 4: deploy the migration.
[WRONG: a plan must name the specific sub-phases, their dependency order,
and what decision or artifact each branch produces. "Design, implement,
test, deploy" are labels, not a plan. They carry no routing signal to
the orchestrator — it cannot dispatch work to a sub-phase called "design".]
```

---

### ARCH domain — rejected construction

ARCH calibration result: zero batches. ARCH vocabulary appeared in BUG/PLAN sessions
(rows 1–520) but was never classified as ARCH. The core pair type is Type A: boundary
decision vs implementation.

**Type A rejected for ARCH — implementation confused with boundary decision:**

```
Prompt:   "define the component boundaries for the notification service"
Chosen (ARCH — 3.1 boundary decision):
  NotificationDispatcher — owns sending. Stateless. No persistence.
  TemplateStore — owns template registry. Read-only. Versioned.
  DeliveryTracker — owns delivery state and retry scheduling. Owns its own storage.
  Dependency direction: Dispatcher depends on TemplateStore and DeliveryTracker.
  TemplateStore and DeliveryTracker must not depend on Dispatcher.
  Forbidden: Dispatcher must not own state between requests.

Rejected (4.1 Backend Services — wrong scope):
  public class NotificationDispatcher : INotificationDispatcher
  {
      private readonly ITemplateStore _templates;
      private readonly IDeliveryTracker _tracker;

      public NotificationDispatcher(ITemplateStore templates,
          IDeliveryTracker tracker) { ... }

      public async Task SendAsync(Notification n)
      {
          var template = await _templates.GetAsync(n.TemplateId);
          await _tracker.RecordAsync(n.Id);
          await _emailClient.SendAsync(template.Render(n));
      }
  }
[WRONG: "define component boundaries" asks who owns what and where
dependencies are forbidden — it does not ask for the class body.
The class body is 4.1 implementation work that follows after the
boundary decision. Writing the implementation before the boundaries
are decided embeds coupling assumptions that later cause rework.]
```

**Overlap ratio:** chosen uses ownership vocabulary (owns, stateless, depends on,
forbidden, must not). Rejected uses class vocabulary (public class, private, async,
inject). Near-zero overlap. Reliably positive margin.

**Type A rejected for ARCH — operational configuration confused with signal design:**

```
Prompt:   "design the observability contract for the payment processing service"
Chosen (ARCH — 3.5 Observability Design):
  Metrics to emit:
    payment.processed — counter with dimensions: status (success/fail/timeout),
      provider (stripe/paypal), amount_tier (low/medium/high)
    payment.latency_ms — histogram, p50/p95/p99 breakpoints
  Alert threshold: payment.processed{status=fail} > 5% of total in 5min window
  Trace requirement: every payment request carries a trace ID from entry through
    all provider calls. Minimum span depth: 3 (gateway → service → provider)

Rejected (7.3 Monitoring and Feedback Loop — wrong scope):
  Set up a Grafana dashboard for payment processing:
  - Panel 1: payment success rate (line chart, 5min intervals)
  - Panel 2: latency p95 (gauge, alert at 800ms)
  - Panel 3: provider breakdown (pie chart)
  Configure PagerDuty integration: escalate when success rate < 95%.
[WRONG: "design the observability contract" defines what signals must be
emitted and what thresholds are meaningful — it does not configure the
tools that consume those signals. Grafana and PagerDuty are consumers of
the contract. They cannot be configured until the contract says what to
measure. Tool configuration is 7.3 Monitoring work; signal contract design
is 3.5 work.]
```

---

## Hard negative construction rules

A hard negative is a rejected response that a casual reader might accept as correct.
These are the highest-value pairs: they produce the strongest positive margin when
constructed correctly, and cause the worst negative margin when constructed carelessly.

---

### Hard negative class 1 — Same domain, wrong sub-phase

Two sub-phases in the same domain share vocabulary. The model must learn the boundary
between them. The rejected must use the vocabulary of the plausible wrong sub-phase
without crossing into clearly wrong territory.

**Rule:** The rejected must be defensible as an answer to a subtly different phrasing
of the same prompt. If a reasonable person would mark the rejected as clearly wrong,
it is not a hard negative — it is a Type A pair. Use Type A for the easy cases; use
hard negatives only for genuinely ambiguous boundaries.

**Example — BUG vs REFACTOR (most common intra-domain confusion):**

The distinction: BUG = something is broken and wrong output is produced. REFACTOR =
something works but is structured poorly. A prompt with "fix" can belong to either.
The signal is whether the output is currently wrong or merely messy.

```
BUG signal words:  error, exception, null, wrong, fails, breaks, unexpected,
                   returns incorrect, doesn't work, crash, TypeError

REFACTOR signal words: messy, hard to read, duplicate, too long, extract,
                       rename, clean up, simplify, restructure, dead code
```

A "fix" prompt that describes wrong output → BUG. Same vocabulary but describing poor
structure with correct output → REFACTOR. The hard negative for a BUG pair uses
REFACTOR vocabulary on the same code, and vice versa.

**Example — PLAN vs ARCH (adjacent domains):**

PLAN = sequencing existing decisions. ARCH = making a new structural decision.
A prompt asking "what should I do about X" can route to either.

```
PLAN signal: "plan the", "in what order", "sequence the", "which comes first",
             "how should I phase", "steps for implementing"

ARCH signal: "design the", "what pattern", "should I use X or Y",
             "define the boundary", "is this the right structure",
             "what are the components"
```

A "plan" prompt that implicitly requires an ARCH decision before sequencing is a
hard case. The hard negative uses sequencing vocabulary for work that cannot yet be
sequenced because the architectural decision is missing.

---

### Hard negative class 2 — Wrong domain classification from overlapping signal words

Some prompts contain signal words from multiple domains. The model must learn which
signal dominates.

**Priority rule from calibration run evidence (EXAMINATION-METHOD-ORCHESTRATION-v8.md):**
```
BUG before REFACTOR before PLAN before ARCH before CODE_Q
```

When a prompt contains both BUG and CODE_Q signals, BUG dominates. A hard negative
for this case classifies the prompt as CODE_Q (explanation) when it should be BUG (fix):

```
Prompt:   "the tenant isolation check in the JWT middleware returns true for
           every request — explain why this happens and how to fix it"

Chosen (BUG — BUG dominates because output is wrong):
  "The check `if (token.TenantId == request.TenantId)` uses string equality on
   a field that DevelopmentPromptCompiler normalises to lowercase. The token
   carries the original case from the auth provider. Fix: normalise both sides
   before comparison — token.TenantId.ToLower() == request.TenantId.ToLower()."

Rejected (CODE_Q — wrong because wrong output exists, not explanation requested):
  "The tenant isolation check compares token.TenantId against the request's
   tenantId claim. When these do not match, the middleware rejects the request.
   The field comes from the JWT payload — the auth provider sets it during
   token issuance."
[WRONG: the prompt says the check returns true for EVERY request — meaning
the output is currently wrong. A prompt that describes wrong output is BUG,
not CODE_Q, regardless of the "explain why" phrasing. The explanation should
name the defect; the chosen response names the defect AND provides the fix.
A pure explanation without a fix leaves the broken behaviour in place.]
```

---

### Hard negative class 3 — Structurally correct but wrong scope

The rejected has the right format and technical content but covers a different scope
than the prompt requested. This class is most common in PLAN pairs where a valid partial
plan is submitted as the full plan.

```
Prompt:   "plan the full implementation of the tenant management feature —
           includes data model, API, auth, and UI"

Chosen (PLAN — full scope, four branches):
  Branch 1 — 3.2 Data Model and Persistence Design (order 1)
  Branch 2 — 3.4 Security and Authorization Model (order 2)
  Branch 3 — 3.3 API Contract Definition (order 3)
  Branch 4 — 4.2 Frontend and Client Integration (order 4, depends on 3.3)

Rejected (PLAN — partial scope, only one branch):
  Branch 1 — 4.1 Backend Services:
    Implement the TenantController with CRUD endpoints, TenantRepository
    with entity framework, and TenantService with validation logic.
[WRONG: the prompt asks for the full plan across data model, API, auth,
and UI. A single implementation branch does not address 3.2 entity design,
3.4 auth model, 3.3 API contract, or 4.2 frontend. Implementing before
the data model and auth model are defined creates dependencies on assumptions
that the omitted branches are supposed to resolve.]
```

---

## Session row classification rules

These rules resolve ambiguous rows from the internal session export.

**Rule 1 — Prompt with "fix" and wrong output described → BUG, not REFACTOR**

If the session row describes a symptom (wrong value, crash, unexpected result), classify
as BUG even if "clean up" or "extract" language appears later in the same prompt.
BUG dominates REFACTOR.

**Rule 2 — Prompt with phase/flow/step reference → PLAN**

If the session row names a flow ID, phase number, or sub-phase, classify as PLAN
even if the prompt also asks how something works. PLAN dominates CODE_Q when
sequencing language is present.

**Rule 3 — Prompt asking "should I use X or Y" → ARCH**

Trade-off prompts belong to ARCH regardless of whether the answer is a short explanation.
ARCH signals: "best approach", "what pattern", "should I use", "is this the right structure".

**Rule 4 — Prompt with "explain", "what does", "how does" and no defect → CODE_Q**

If there is no described wrong output and no sequencing request and no trade-off
question, classify as CODE_Q even if the content is technically complex.

**Rule 5 — Prompt containing ARCHITECTURE predicate vocabulary BUT described as BUG
in session context → reclassify as ARCH for pair construction**

Rows 1–520 of XIIGEN-SESSION-1000 were labelled BUG/PLAN in the calibration run, but
many contain "architecture", "component", "boundary", "topology" vocabulary. These rows
contain ARCH content that was never trained. Extract as ARCH pairs by reclassifying the
domain label and constructing Type A rejected (implementation vs boundary decision).

**Rule 6 — Row where same prompt appears as chosen for multiple sub-phases → exclude from
single-sub-phase pairs; use only for multi-route PLAN pairs**

If the same prompt legitimately routes to five sub-phases, it cannot be the A side for
a pair that teaches sub-phase 3.2 specifically — because rejected would have to be one
of the other four correct sub-phases. Use these prompts only for PLAN pairs where the
full multi-branch output is the chosen.

This rule explains 20/27 negative-margin BUG batches in the calibration run. The runner
assigned the same prompt to each sub-phase as its chosen, creating intra-batch conflict.

---

## Sources

### Source 1 — XIIGEN-SESSION-1000

Document type: Type 1 (A-side authority) for rows 521–727; mixed for rows 1–520;
excluded as A side for rows 1235–1753.

**Pair extraction priority:**
- Rows 521–727: PLAN Type B. Proven positive margin. Extract more pairs from this range.
- Rows 1–520: reclassify ARCHITECTURE-vocabulary rows as ARCH Type A.
- Rows 1235–1753: correction events only. Do not use raw prompts as A sides.

**Row classification test before extraction:**
- Check `CompiledPredicateIds` for the row. If near-empty: exclude as A side.
- Check for meta-commentary phrases (see Type 6). If present: exclude.
- Check for named artifacts or dependency language. If absent and PLAN: exclude.

---

### Source 2 — Claude Export Batch-0000

Document type: Type 3 (correction source) — primary.

**~393 correction events not yet extracted.** These are the highest-priority pairs.
Extraction order:

```
1. legacy-docs-skills-from-old-code (35 corrections) — ARCH, CODE_Q, DOCUMENT
2. value-proposition-code-gaps (104 corrections) — ARCH, BUG, PLAN
3. flow-01-station-testing (79 corrections) — BUG, IMPLEMENT, TEST
4. figma-to-code-skills-to-xiigen (69 corrections) — PLAN, ARCH, IMPLEMENT
5. session-prompt-and-learning-protocol (71 corrections) — PLAN, CODE_Q
```

For each correction event:
- H0 → A side (only if Type 1 eligibility passes)
- A0 → rejected + [WRONG: annotation derived from H1 correction text]
- A1 → chosen

Quality gate before including:
- Run token overlap test: overlap < 0.50 required, < 0.25 preferred
- Rate High/Medium/Low (see Type 3 quality rating)
- Low = exclude regardless of how clean the export looks

---

### Source 3 — Local session files (xiigen_history)

Document type: Type 2 (B-side authority) for architecture documents;
Type 1 (A-side authority) for local session H turns.

Documents in `xiigen_history/old/` are Type 2 — formal architecture vocabulary.
Use for chosen B sides in ARCH and PLAN pairs. Do NOT use as A sides.

---

### Source 4 — Orchestrator Teaching Plan Documents

Document type: Type 2 (B-side authority). Never A side.

Use `ORCHESTRATOR-TEACHING-PLAN-v3.md` sub-cycle steps for PLAN Type B chosen content.
Use stopping rules and MUST conditions for ArbiterUnit chosen/rejected criteria.

---

### Source 5 — Session Teaching Ledger

Document type: Type 4 (measurement evidence).

Use for ModelTrainingUnit B sides. Do not derive A sides from teaching ledger rows.

---

## Gap map by domain

| Domain | Calibration result | Root cause | Primary gap |
|--------|-------------------|------------|-------------|
| ARCH | 0 batches | Never extracted — vocabulary present in rows 1–520 | Need Type A pairs (boundary vs implementation) |
| CODE_Q | 0% positive margin | Negation template — overlap ~0.70 | Need Type C rejected (action response, not negation) |
| BUG | 74% negative margin | Same prompt as chosen for 8+ sub-phases | Need Rule 6: exclude multi-route prompts from single sub-phase pairs |
| PLAN | 66% negative margin | Meta-commentary A sides | Need Rule: exclude rows 1235–1753 as A sides |

---

## Pre-submission checklist

Before any pair from any internal source enters a batch:

```
□ Source type identified: Type 1, 2, 3, 4, 5, or 6
□ Type 6 prompt detected and excluded
□ A side passes Type 1 eligibility: named artifact OR dependency language OR
  technical specificity (for PLAN/ARCH); not meta-commentary
□ CompiledPredicateIds not near-empty for the A side
□ Row classification rule applied (rules 1–6 above)
□ Token overlap test: overlap_ratio < 0.60 (target: < 0.35)
□ Rejected is a complete, coherent response from the wrong sub-phase vocabulary
□ Rejected is NOT a negation, NOT a non-answer, NOT another correct response
  for a different sub-phase of the same prompt
□ [WRONG: ...] annotation names:
    (1) the sub-phase the rejected incorrectly addresses
    (2) why that sub-phase does not own this responsibility
    (3) what the prompt actually asked for that rejected missed
□ Annotation is specific: "this is wrong" fails; named confusion required
□ Hard negative class identified (1, 2, or 3) if applicable
□ For correction events: quality rated High or Medium; Low excluded
□ For correction events: conversation_uuid recorded
□ TrainingText will use compiledText from SemanticTeachingPromptCompiler
  — not raw prompt string (hard gate)
□ The same prompt does not appear as chosen for more than one sub-phase
  in this batch (Rule 6 violation check)
□ reward_margin_mean >= 0 confirmed after batch runs before committing
```

---

## Last Updated

2026-05-08.
