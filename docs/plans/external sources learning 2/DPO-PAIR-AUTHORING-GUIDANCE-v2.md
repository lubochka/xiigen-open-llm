# DPO Pair Authoring Guidance — All Domains
# Version: 2.0
# Date: 2026-05-08
# Supersedes: DPO-PAIR-AUTHORING-GUIDANCE-v1.0

---

## What changed from v1

**Translated to English** to match the rest of the document set.

**Reward margin mechanics added throughout.** Every rule now explains the
token-level reason, not just the content prescription. Authors need to understand
why a pair will or will not produce positive reward margin before writing it.

**Per-domain accuracy data added.** Numbers from calibration run 2026-05-06
explain which domains need which pair types most urgently.

**Token overlap test added as the primary pre-submission gate.** Replaces the
informal "different content" rule with a concrete measurable threshold.

**Diagnosis section added.** When reward margin is negative after a run,
this section maps symptoms to causes to fixes.

**Common errors section expanded** with token-level explanations of why each
error causes negative margin.

---

## 2026-05-11 Run-Experience Fix

Operational runbook:
`C:\Users\lubao\Documents\xiigen\llm mvp\external sources learning 2\documentation\TRAINING-SET-BUILD-AND-TESTING-RUNBOOK-v1.md`

The current accepted baseline is `RUN-416/V246`: 672 rows, weighted accuracy
0.7068, weighted reward margin 0.016892, with domain focus
orchestration/plans/skills. Protect that baseline.

New hard pair gates from the live run:

1. `chosen` and `rejected` must not be identical after trimming.
2. `prompt`, `a_side`, `chosen`, and `rejected` must not contain mojibake
   markers such as `Ã`, `Â`, `â€`, or `Ã¢`.
3. `rejected` must contain a specific `[WRONG: ...]` reason.
4. The source domain must be registered or explicitly first-party/internal.
5. Ledger, status, and audit rows are evidence, not DPO rows.
6. `compiled_prompt_in_training: false` is valid only for pair kinds whose
   prompt mode is explicitly `raw_snippet`, such as CODE_TO_DOC.

Model-led rule:

Do not let deterministic code decide that a user request, genesis prompt, flow
initiation document, or basic prompt is low-authority. In XIIGen those files are
often part of the actual prompt surface that built the system. If the type is
unclear, use the registry guide and model-led adjudication before writing or
discarding pairs.

Promotion rule:

New generated batches enter training only through 16-64 row standalone ablation.
Promote to a combo run only when reward margin is positive and accuracy does not
damage the protected orchestration/plans/skills baseline.

---

## Why this matters

DPO training computes for every pair:

```
reward_margin = log P(chosen | prompt) - log P(rejected | prompt)
```

When `reward_margin > 0` the optimizer reinforces the model's preference for
chosen. When `reward_margin < 0` the optimizer runs but pushes in the wrong
direction — repeated negative-margin pairs degrade the model.

**Calibration run 2026-05-06 results by domain:**

| Domain | Mean accuracy | Positive margin batches | Worst batch |
|---|---|---|---|
| PLAN | 0.507 | 2 of 6 | acc=0.306, margin=-0.0016 |
| BUG | 0.417 | 7 of 27 | acc=0.259, margin=-0.003108 |
| CODE_Q | 0.328 | 0 of 4 | acc=0.280, margin=-0.002247 |
| ARCH | — | 0 of 0 | no batches at all |

Loss across all batches stayed at 0.693 = ln(2) — random-guess level.
The model was not learning. The cause was that rejected was always
`"Do not generate a capability for sub-phase X"`, sharing nearly every
meaningful token with chosen. The optimizer had nothing to distinguish.

---

## The token overlap test

Apply this test to every pair before adding it to a batch:

```
shared = tokens in both chosen and rejected that are NOT also in the prompt
total  = (tokens in chosen) + (tokens in rejected) - (tokens in prompt)
overlap_ratio = shared / total
```

**Rule: overlap_ratio > 0.60 → do not submit. Revise the rejected.**

The forbidden template always fails this test:

```
chosen:   "Generated capability for sub-phase 3.4:
           Security and Authorization Model.
           Execute the requested work through task type
           security-and-authorization-model."

rejected: "Do not generate a capability for sub-phase 3.4."

Shared (not in prompt): "capability", "sub-phase", "3.4", "generate"
Overlap ratio: ~0.70 → FAILS
```

Typical overlap ratios for valid rejected types:

| Rejected type | Typical overlap ratio | Expected margin direction |
|---|---|---|
| Type A — different sub-phase vocabulary | 0.10 – 0.30 | Reliably positive |
| Type B — same sub-phases, wrong order | 0.35 – 0.50 | Positive with strong annotation |
| Type C — wrong domain (BUG vs CODE_Q) | 0.15 – 0.35 | Reliably positive |
| Negation template | 0.60 – 0.80 | Reliably negative |
| Abstract label swap ("Phase 1/Phase 2") | 0.50 – 0.65 | Borderline to negative |

---

## Three laws of a good pair

### Law 1 — Rejected must be plausibly wrong, not a negation

Rejected is not "the wrong answer." It is "what a reasonable person would say
if they misread the prompt or confused two adjacent sub-phases." It contains
real content, real technical terms, and a coherent argument — but it addresses
the wrong responsibility.

If rejected looks immediately absurd the model learns nothing. If rejected looks
completely correct the model cannot distinguish. The target is: coherent but
wrong, with an identifiable reason it is wrong.

A plausible rejected:
- Uses vocabulary from the prompt
- Addresses the same general area
- But answers a different question than the prompt asked

### Law 2 — Chosen and rejected must differ in content, not in form

Forbidden as rejected: `"Do not do X"`, `"Skip X"`, `"X is not applicable"`.
These differ from chosen only grammatically, not semantically. Token
distributions overlap completely.

Required as rejected: an alternative response that addresses a different
sub-phase, a wrong dependency order, or a wrong domain — with a [WRONG: ...]
annotation explaining why the wrong answer looks plausible.

### Law 3 — One prompt, one pair per batch

A prompt must not appear as chosen in one pair and as rejected in another
within the same batch. This produces a contradictory training signal:
the model sees the same token sequence as both the preferred and dispreferred
response, and the gradients cancel.

When one prompt spans multiple sub-phases: write one pair per sub-phase.
Each pair has a different chosen (the correct response for that sub-phase)
and a different rejected (a plausible adjacent sub-phase confusion for that
sub-phase).

---

## Three valid rejected types

### Type A — Plausible adjacent sub-phase

Use when two sub-phases share vocabulary but own different responsibilities.
The rejected addresses the neighbouring sub-phase using that sub-phase's
vocabulary — producing a clearly different token distribution from chosen.

**Why it works:** Chosen uses the vocabulary of the correct sub-phase
(e.g., boundary rules for 3.1). Rejected uses the vocabulary of the
wrong sub-phase (e.g., class names and method signatures for 4.1).
These are largely non-overlapping token sets. Overlap ratio typically 0.15–0.30.

**Example — testing confused with security design:**

Prompt: `"write unit tests for the authorization guard"`

```
CHOSEN — 5.1 Unit Test Design:
Write unit tests for the AuthGuard class in isolation.
Mock the JwtStrategy dependency. Assert that guard.canActivate()
returns true when token is valid and throws UnauthorizedException
when token is missing.

REJECTED — 3.4 Security and Authorization Model:
Design the authorization guard to verify JWT tokens.
Define @UseGuards(AuthGuard) on protected routes and specify
the RBAC roles required for each endpoint.
[WRONG: the prompt asks to write tests, not design the security model.
"authorization" modifies "tests" — it names what is being tested,
not the scope of the work.]
```

**Plausible Type A pairs by prompt signal:**

| Prompt signal | Chosen | Rejected |
|---|---|---|
| "write tests for auth guard" | 5.1 Unit Tests | 3.4 Security Model |
| "define the API schema for tenant" | 3.3 API Contract | 4.1 Backend Services |
| "fix the null ref in the service" | 4.1 Backend Services | 5.1 Unit Tests |
| "add authorization to the endpoint" | 3.4 Security Model | 3.3 API Contract |
| "design the component boundaries" | 3.1 Architecture | 4.1 Backend Services |
| "we have no visibility into failures" | 3.5 Observability Design | 7.3 Monitoring Setup |
| "define acceptance criteria for release" | 6.1 Manual QA | 5.3 End-to-End Tests |
| "camera is not allowed in this document" | 3.4 Security Model | 4.2 Frontend |

### Type B — Correct sub-phases, wrong dependency order

Use for PLAN pairs where the prompt contains multiple tasks. The rejected names
the same sub-phases as chosen but sequences them incorrectly or omits the
`depends_on` constraint.

**Why it works:** Chosen and rejected share the same sub-phase names, so the
distributional difference is smaller than Type A. The signal comes primarily
from the `depends_on` field content and from the [WRONG: ...] annotation.
Overlap ratio typically 0.35–0.50. **The annotation carries most of the signal
for Type B.** A Type B pair without an annotation will often produce near-zero
or negative margin.

**Example — integration tests before implementation:**

Prompt: `"add JWT authentication and then write integration tests for tenant isolation"`

```
CHOSEN — PLAN, correct order:
Branch 1 — 3.4 Security (dependency order 1):
  Implement AuthGuard with JWT validation. Define bypass paths.
  This branch must complete before testing can begin.
Branch 2 — 5.2 Integration Tests (dependency order 2):
  Write integration tests that verify cross-tenant data isolation.
  Dependency: branch 2 depends on branch 1.

REJECTED — PLAN, reversed order:
Branch 1 — 5.2 Integration Tests (dependency order 1):
  Write tenant isolation tests first, using mock JWT tokens.
Branch 2 — 3.4 Security (dependency order 2):
  Implement the real AuthGuard after the tests define what it must satisfy.
[WRONG: integration tests for tenant isolation require a functioning AuthGuard
that issues real tokens. Mock tokens bypass actual validation logic and cannot
test real isolation. The implementation must exist before integration tests
can run meaningfully.]
```

**Why the annotation is critical here:** Both chosen and rejected name `3.4`
and `5.2`. The distinguishing tokens are `depends on`, `must complete before`,
and the annotation's `mock tokens bypass actual validation logic`. Without
these tokens, overlap ratio approaches 0.55 — borderline. The annotation
drops it to ~0.40.

### Type C — Wrong domain for the prompt's intent verb

Use to teach domain boundaries. The intent verb determines the correct domain.
The rejected misreads the verb and routes to the wrong domain.

**Why it works:** Different domains use different vocabulary. Explanation
vocabulary (CODE_Q) does not overlap with planning vocabulary (PLAN) or bug
diagnostic vocabulary (BUG). Overlap ratio typically 0.15–0.35.

**Intent verb → domain mapping:**

| Prompt formulation | Correct domain | Plausible rejected domain |
|---|---|---|
| "explain / how does X work / what is" | CODE_Q | BUG or PLAN |
| "X throws / X fails / error in / bug" | BUG | CODE_Q or PLAN |
| "implement / add / build / create" | PLAN or IMPLEMENT | CODE_Q |
| "design / define boundaries / architect" | ARCH | IMPLEMENT |
| "review / audit / verify compliance" | QA or ARCH review | IMPLEMENT |
| "plan / sequence / order of" | PLAN (SchemePlannerUnit) | CODE_Q |

**Example — explanation confused with bug fix:**

Prompt: `"how does the rxjs pipe work in Angular interceptors?"`

```
CHOSEN — CODE_Q:
The pipe() operator chains rxjs operators. In an Angular HttpInterceptor,
pipe() is called on the next.handle(request) Observable to transform the
request or response stream. switchMap cancels the previous request;
mergeMap runs all requests concurrently.

REJECTED — BUG, 4.2 Frontend Implementation:
Fix the rxjs pipe in the Angular interceptor. Replace mergeMap with switchMap
to cancel stale requests. Update the pipe to:
pipe(switchMap(term => this.http.get(url + term))).
[WRONG: the prompt asks "how does X work" — CODE_Q scope.
No error is reported. No fix is requested. Providing a fix when an
explanation was asked treats the prompt as a BUG report it is not.]
```

---

## Per-domain guidance

### BUG domain — most common confusion

BUG prompts describe something that is broken. The most reliable Type C
rejected is CODE_Q (the person was asking how it works, not that it's broken)
or IMPLEMENT (adding a feature, not fixing one).

The most common token-level confusion: prompts containing security error
messages (403, CSP violation, JWT expired) are about broken behavior but
are often confused with security design (3.4). The distinction:

- `"camera is not allowed"` → BUG pointing at 3.4 (CSP configuration error)
- `"design the CSP policy"` → 3.4 directly (no broken behavior)

Rejected for a 3.4 CSP pair: 4.2 Frontend (adding getUserMedia() component call
instead of fixing the header). The token distributions are fully different.

### CODE_Q domain — zero positive margin batches

CODE_Q had 0 of 4 batches with positive reward margin. Every batch was negative.
This is the most urgent domain to fix.

The problem: CODE_Q prompts ("how does X work", "explain X") produce short
chosen responses (explanations) and the rejected was the negation template.
Overlap ratio was ~0.70 for every pair.

Fix: for every CODE_Q prompt, rejected must be a BUG or PLAN response that
addresses the same vocabulary but takes action instead of explaining.

```
Prompt: "how does the tenantId claim work in JWT validation?"

CHOSEN — CODE_Q:
The tenantId claim is extracted from the JWT payload after signature
verification. The middleware reads jwt.payload.tenantId and sets it on
the request context. Downstream handlers read it from context, not from
the token directly.

REJECTED — PLAN, 3.4 Security:
Add tenantId claim extraction to the AuthGuard: in the handleRequest method,
call this.jwtService.verify(token) and extract payload.tenantId.
Store it on the request object for downstream use.
[WRONG: the prompt asks how the mechanism works (CODE_Q), not to implement it.
Providing an implementation plan when an explanation was requested misreads
the prompt's scope.]
```

**CODE_Q annotation pattern:**

```
[WRONG: the prompt asks "how does X work" — this is CODE_Q scope.
{Rejected action} is what you do when building the feature.
The prompt is asking to understand the mechanism, not to add it.]
```

### PLAN domain — 4 of 6 batches with negative margin

The two positive-margin batches (16 and 17, acc=0.618 and 0.771) had prompts
with named artifacts and dependency language. The four negative-margin batches
had abstract meta-commentary prompts.

**PLAN prompt requirements** (all pairs must satisfy at least one):

```
Named artifact:
  "Flow 10", "Phase 3", "AuthGuard", "TenantResolver",
  "Factory Interface", specific component name or file

Dependency language:
  "before", "after", "then write", "once X is complete",
  "depends on", "when X is in place", "following X"

Technical specificity:
  Predicate vocabulary (jwt, api, architecture, test),
  sub-phase references, technology names
```

**Prompts that must not enter PLAN training:**

```
"Option A is clearly the right path"
"this is grounded in existing documents"
"let's continue with the plan"
"we're making good progress on this"
```

These produce near-zero `CompiledPredicateIds`. The `TrainingText` carries
no routing signal. Reward margin is near zero regardless of rejected quality.

For PLAN, Type B rejected (wrong dependency order) is required for every
multi-task prompt. Type B is the primary training signal for PLAN accuracy.

### ARCH domain — zero training batches

ARCH had no batches at all in calibration run 2026-05-06. Zero training signal.

Type A is the primary rejected type for ARCH. Every ARCH sub-phase pairs
with an IMPLEMENT sub-phase as the natural wrong answer — the rejected
implements what the prompt asked to design.

ARCH chosen vocabulary: `owns`, `boundary`, `forbidden`, `depends on`,
`direction`, `rule`, `decision`.

ARCH Type A rejected vocabulary: `Implement`, `class`, `method`, `Inject`,
`Register`, `DI container`, `framework call`.

These are non-overlapping sets. Overlap ratio typically 0.10–0.20.
ARCH Type A pairs should produce the strongest positive margin in the run.

---

## Stage 9 corruption cases — three required per batch

These are not regular DPO pairs. They teach the model to detect specific
failure modes in the pipeline.

### wrong-predicate-assignment

The compiler assigns predicate IDs that do not match the dominant signal in
the prompt. The correct pair shows the right predicates. The corrupted pair
shows a plausible but wrong assignment.

**Construction rule:** find a prompt where one lexicon term appears as a noun
modifier (secondary) and another appears as the primary verb or subject.
The corrupted version promotes the modifier to primary.

```
Prompt: "test coverage shows 42% on the authentication module"

CORRECT:
  CompiledPredicateIds: ["TEST", "SECURITY"]
  → routes to 6.2 Regression Coverage Review

CORRUPTED:
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"]
  → routes to 3.4 Security Model
[WRONG: "test coverage" is the primary subject — TEST predicate fires first.
"authentication module" is the object being measured. TEST takes priority
when the primary verb is coverage measurement, not when a secondary noun
mentions authentication.]
```

### ambiguous-predicate-case

The router returns one routing decision when the predicates support two
distinct sub-phases. The correct pair shows both decisions.

**Construction rule:** find a prompt where two different predicates each
independently justify a different sub-phase. The corrupted version routes
only to the higher-confidence sub-phase, dropping the second.

```
Prompt: "add authorization to the API endpoint"

CORRECT:
  Routes to 3.4 (SECURITY, AUTHORIZATION) AND 3.3 (API_CONTRACT) — two decisions.

CORRUPTED:
  Routes only to 3.4 — 3.3 omitted.
[WRONG: API_CONTRACT predicate is present from "API endpoint". When multiple
predicates match different sub-phases, all must be routed. Dropping one
sub-phase suppresses valid routing signal and creates a false knowledge edge
for the omitted sub-phase.]
```

### knowledge-edge-suppressed

The orchestrator returns PASS for a prompt that routed to an unregistered
sub-phase. The correct pair shows KNOWLEDGE_EDGE with all six required metrics.

**Construction rule:** use any prompt that routes to a sub-phase with no
registered flow (all sub-phases except 3.4 currently). The corrupted version
returns PASS without knowledge-edge markers.

```
Prompt: "set up the end-to-end test suite for the checkout flow"
Routes to: 5.3 End-to-End Tests — no flow registered.

CORRECT:
  OverallVerdict: "KNOWLEDGE_EDGE"
  FlowId: "knowledge-edge-missing-flow"
  Metrics: knowledge_edge=True, missing_sub_phase=5.3, missing_flow=True,
           routed_phase_id=5, routing_confidence="0.83",
           knowledge_edge_reason="no logical flow registered for routed sub-phase"

CORRUPTED:
  OverallVerdict: "PASS"
  (no knowledge-edge markers)
[WRONG: PASS fabricates a result the system cannot produce for an unregistered
sub-phase. The required output is KNOWLEDGE_EDGE with all six metrics.
The model must learn to produce the knowledge-edge structure, not suppress it.]
```

---

## Batch composition minimums

| Pair type | Minimum fraction | Priority |
|---|---|---|
| Type A (adjacent sub-phase) | 40% of all pairs | Highest — most reliable positive margin |
| Type B (wrong dependency order) | 20% — required for multi-task prompts | Required for PLAN accuracy |
| Type C (wrong domain) | 30% — required for every domain in the batch | Required for CODE_Q recovery |
| Stage 9 corruption rows | 1 of each kind | Required every batch |

---

## Common errors — with token-level diagnosis

**Error 1 — Rejected is the negation template**

```
"Do not generate a capability for sub-phase X."
"Skip sub-phase X."
"This is not applicable here."
```

Token analysis: shares `capability`, `sub-phase`, sub-phase number with chosen.
Overlap ratio: 0.65–0.80. Result: reliably negative margin.

Fix: write a complete alternative response addressing a different sub-phase.

**Error 2 — One prompt → multiple chosen sub-phases in one batch**

From calibration run: one prompt ("подробнее с примерами про GRPC, Consul, Polly")
was assigned as chosen for 3.2, 3.4, 3.5, 4.3, 6.4, 7.2 simultaneously.
This created pairs where the same token sequence is simultaneously preferred
and dispreferred. Gradients cancelled. Batch accuracy: 0.275.

Fix: one prompt → one chosen sub-phase per batch. If the prompt covers multiple
sub-phases, write separate batches or pick the most relevant single sub-phase.

**Error 3 — Rejected addresses a different domain but has no annotation**

```
Rejected: "Sub-phase 7.3: Monitoring and Feedback Loop.
           Set up the observability dashboard."
```

Without a [WRONG: ...] annotation the overlap ratio is whatever it is based
on content alone. For Type B pairs where chosen and rejected share sub-phase
names, the annotation is the primary distinguishing signal. Missing annotation
on a Type B pair often causes negative margin.

Fix: add annotation naming which sub-phase the rejected incorrectly addresses
and why that sub-phase does not own the responsibility in the prompt.

**Error 4 — Abstract PLAN prompt used**

From calibration run batch 30 (acc=0.306): prompts like "Option A is clearly
the right path — the research is too..." contain no predicate vocabulary.
`CompiledPredicateIds` is empty or near-empty. `TrainingText` carries no
routing signal. Reward margin stays near zero regardless of rejected quality.

Fix: use only prompts with at least one named artifact, one dependency verb,
or one technical specificity element.

**Error 5 — Type B annotation only states the wrong order**

```
[WRONG: this is the wrong order. 3.4 must come before 5.2.]
```

This annotation names the correct order but does not explain why the wrong
order fails. Without the "why", the annotation adds fewer distinguishing tokens
and Type B pairs trend toward near-zero margin.

Fix: add the functional reason why the wrong order fails:

```
[WRONG: 5.2 integration tests for tenant isolation require a functioning 3.4
AuthGuard that issues real tokens. Mock tokens bypass the actual validation
logic and cannot test real isolation. The 3.4 implementation must exist before
5.2 tests can run meaningfully.]
```

---

## Pre-submission checklist

```
□ Token overlap test: overlap_ratio < 0.60
  (shared tokens excluding prompt / total tokens)

□ Rejected is a complete response with real technical content
  — not a negation, not a skip directive, not an abstract label

□ Rejected addresses a plausible sub-phase adjacent to chosen
  — not a random or distant sub-phase

□ [WRONG: ...] annotation is present and specific:
  — names which sub-phase rejected incorrectly addresses
  — explains why that sub-phase does not own this responsibility
  — states what the prompt actually asked for that rejected missed

□ For Type B: annotation explains why the wrong order fails
  — not just "the order is wrong"

□ For Type C: intent verb checked against the domain mapping table

□ For CODE_Q: rejected is an action response (fix or plan),
  not another explanation

□ For PLAN: prompt contains named artifact OR dependency language
  OR technical specificity — no meta-commentary, no abstract phases

□ Prompt appears exactly once as chosen in this batch

□ Stage 9: all three corruption kinds present in the batch

□ TrainingText will use compiledText — not the raw prompt string
```

---

## Reward margin diagnosis when a batch returns negative

Run this diagnosis before resubmitting any rejected batch:

| Symptom | Cause | Fix |
|---|---|---|
| All pairs negative margin | Forbidden template used | Rewrite all rejected as Type A, B, or C |
| Specific sub-phase pairs negative | Chosen and rejected share sub-phase label text | Ensure rejected vocabulary comes from a different sub-phase |
| Type B pairs negative | Annotation only states the order, not why it fails | Add functional failure reason to annotation |
| CODE_Q pairs all negative | Rejected is negation or another explanation | Rejected must be an action response (BUG fix or PLAN step) |
| ARCH pairs never in batch | No ARCH prompts in session documents used | Extract prompts with "design", "define boundaries", "architecture" intent verbs |
| Near-zero margin across batch | Prompts contain no predicate vocabulary | Replace abstract prompts with prompts containing named artifacts |
| Corruption rows positive but real pairs negative | Real pairs weaker than corruption pairs | Apply token overlap test to all real pairs; rebuild weakest ones |

---

## Last Updated

2026-05-08.
