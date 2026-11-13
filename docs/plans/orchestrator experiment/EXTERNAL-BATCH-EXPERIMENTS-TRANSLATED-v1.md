# External Batch Experiments — Translated
# Five training set pair files translated into experiment designs
# using HOW-TO-TRANSLATE-A-DOCUMENT-INTO-AN-EXPERIMENT-v1.md
# Source documents: clean-batch-001, clean-batch-002, clean-batch-003
# Version: 1.0 — 2026-05-04

---

## What This Document Is

Five training set pair files produced during the orchestration domain training
phase are translated here into experiment designs. Each translation follows
the six-step method from the guidance document exactly. The output of this
translation is five experiment designs, each ready to proceed to Round A
(oracle document production) upon approval.

The five source documents cover: scheme planning for infrastructure
orchestration scenarios (clean-batch-001 Node 3), domain translation of
four orchestration-relevant labels (clean-batch-002 Node 2), domain
processing of sub-task outcome spaces (clean-batch-002 Node 4), arbiter
selector fire and pass-through cases (clean-batch-002 Node 7), and the
single-pair Django N+1 cascade unblock scheme plan (clean-batch-003 Node 3).

All five are "training set pair file" type per the HOW-TO table. The
highest-value claim types for this document type are:
- Binding (A-to-node assignment correct)
- Coverage (B shows output, not describes it)

The most common failure mode: a B that describes what the output would contain
rather than containing it — and the structurally adjacent failure mode for pair
files that do contain actual output: the B crosses into an adjacent node's
domain while appearing correctly assigned.

---

---

## Experiment 1 — Scheme Planner Examination (clean-batch-001 Node 3)

**Source:** `clean-batch-001/node-03-scheme-planner-set.md`
**Pairs:** CB1-03-001 (OTel tail sampling), CB1-03-002 (EF Core expand-contract),
CB1-03-003 (Dapper local transaction vs saga), CB1-03-004 (Redis shared cache)
**Domain:** Multi-step infrastructure orchestration and deployment sequencing

---

### Central Question

Given the four scheme plan pairs in CB1-03 and the infrastructure orchestration
domain they govern, does each B correctly produce an actual ordered plan with
actual steps, actual wait states, actual transformational prompts, and actual
recovery paths — or does any B describe what a plan would contain rather than
containing it, name parallel steps that have cross-references making them
appear dependent, or cross from scheme planning into applier territory by
providing exact implementation code where an orchestration step should appear?

---

### Step 1 — Claims

**Enforcement claim:** The node header states that "A B that says 'create a
plan with phases' is wrong — the actual plan with actual steps must appear as
B." This enforces a standard: B must be actual content, not a description of
content.

**Binding claim 1:** Each B is correctly assigned to Node 3 (Scheme Planner)
and not to Node 4 (Domain Processor, which produces success/failure/wait
outcome spaces) or Node 5 (Applier, which produces exact code changes). The
binding claim is that scheme plan content is distinct from applier content and
that no B crosses the boundary.

**Binding claim 2:** Each pair's A input is a collection of formal domain
descriptions, not a raw user fragment. The binding claim is that A does not
contain user-level text that Node 2 should have translated but did not.

**Coverage claim:** Every B covers all five required plan elements — ordered
steps, parallel steps where allowed, wait states, recovery paths, and review
points. No required element is silently absent.

**Sequencing claim:** Within each plan, step dependencies are correctly stated
— steps that depend on earlier steps name those dependencies explicitly, and
the parallel claim for concurrent steps is justified by a stated independence
argument.

**Honesty claim:** Each Reason field names the specific correctness property
the step ordering enforces. "The step order matters" is not a valid Reason —
the specific dependency that would fail if the order were reversed must be
named.

---

### Step 2 — Real Failure Modes

**Failure mode 1 — Parallel step cross-reference contradiction (CB1-03-001).**
CB1-03-001 Steps 3 and 4 are stated as parallel. The Reason field justifies
this: "processor configuration files are independent; they are applied together
at deployment time." However, the Step 3 transformational prompt says "Add the
tailsampling processor to the Collector pipeline, positioned after the
memory_limiter (which will be added in Step 4)." This is a cross-reference —
Step 3's prompt describes the final configured state (after Step 4 exists) rather
than the independent action of configuring Step 3's processor. A reader who
treats the transformational prompt as an execution instruction rather than a
final-state description will conclude that Step 3 depends on Step 4's output.
The enforcement claim (parallel steps are stated independent) appears satisfied
by the Reason field. The cross-reference in the prompt body creates a surface
reading that contradicts it. Observable evidence: if this failure is present, the
orchestrator will sequence Steps 3 and 4 rather than running them concurrently,
because the transformational prompt references Step 4 as a prerequisite.

**Failure mode 2 — Applier crossover in transformational prompt body
(CB1-03-002).** CB1-03-002 Phase 3's transformational prompt says "Run UPDATE
Users SET EmailAddress = Email WHERE EmailAddress IS NULL in batches of 10,000
rows with a 100ms pause between batches." This is an exact SQL statement with
exact batch parameters — applier-level content. A scheme planning step should
specify what the phase accomplishes and why the batching strategy is required;
the exact SQL and the exact batch parameters belong in Node 5 where the applier
receives them and executes them. The node assignment claim (this B belongs to
Node 3) appears satisfied — the file is correctly in the Node 3 file, and the
pair has the correct pair ID prefix, steps, wait states, and a Reason field. The
boundary violation is inside the transformational prompt body, invisible from the
structural surface. Observable evidence: if this failure is present and the pair
is used to train Node 3, the scheme planner will produce exact SQL in its step
outputs, and Node 5 will receive exact code in the scheme plan rather than in
its own domain processor output.

**Failure mode 3 — Hollow prerequisite resolved by A (CB1-03-004).** CB1-03-004
Step 1's transformational prompt says "Determine whether the cache must be
consistent across all three instances." The A input states "shared cache required
because cache invalidation must propagate immediately." The decision Step 1 asks
the executor to make is already made in A. The coverage claim (Step 1 is a real
prerequisite that blocks subsequent steps) appears satisfied because a wait state
is present. The actual prerequisite is hollow: Step 1 will always pass because A
has already resolved the question. Observable evidence: if this failure is present,
Step 1 produces no new information beyond restating A — the orchestrator passes
through it without any decision being made.

---

### Step 3 — Three Scenarios

**Scenario 1 — Parallel-step cross-reference probe (CB1-03-001, Steps 3 and 4)**

Name: PARALLEL-CROSS-REFERENCE-PROBE

Tests: Failure mode 1 (parallel step dependency contradiction).

Why hard: The Reason field says Steps 3 and 4 are parallel and independent.
The Step 3 prompt body references Step 4 as if it exists. Determining whether
this is a final-state description or a dependency ordering requires reading
both the Reason field and the prompt body, understanding the distinction between
"the configuration describes a final state" and "the configuration depends on
Step 4 existing," and then deciding whether the contradiction warrants a concern
or whether the Reason field's explanation makes the parallel claim correct.
A mechanical check finds a wait state between Steps 2 and 3 and concludes the
plan is correctly sequenced. Judgment is required to find the internal
contradiction.

A input: CB1-03-001 B (full plan) — Steps 1 through 5 with wait states and Reason
field — as written in the node file.

Expected finding: The Step 3 transformational prompt references Step 4's output
("positioned after the memory_limiter (which will be added in Step 4)") while
the Reason field asserts parallel independence. The finding is a claim
contradiction: the structural claim (parallel) and the content claim (reference
to Step 4) cannot both be literally correct. The resolution is that "positioned
after" describes the final pipeline order, not an execution dependency. The
transformational prompt should be reworded to make this explicit.

Expected arbiter verdict: CONCERN. The parallel claim is substantively correct
but the wording creates an apparent dependency that could mislead the scheme
planner in future training. The plan does not need to be reprocessed — it can
be used — but the step wording should be corrected.

---

**Scenario 2 — Applier crossover probe (CB1-03-002, Phase 3)**

Name: APPLIER-CROSSOVER-PHASE3-PROBE

Tests: Failure mode 2 (binding — B crosses into Node 5 territory).

Why hard: The distinction between a scheme planning step and an applier step
is not visible from the structural surface of the plan. Phase 3 has a step
number, a transformational prompt, and a Phase header — all correct Node 3
markers. The exact SQL and exact batch parameters inside the prompt body are
the violation, and they look like helpful specificity rather than a boundary
crossing. An examiner that focuses on whether the B is "an actual plan with
actual steps" will pass the enforcement check. Judgment is required to detect
that "actual steps" in Node 3 means orchestration steps, not implementation
steps.

A input: CB1-03-002 B Phase 3 transformational prompt and surrounding context
as written in the node file.

Expected finding: The Phase 3 transformational prompt provides exact SQL
("UPDATE Users SET EmailAddress = Email WHERE EmailAddress IS NULL") and exact
batch parameters (10,000 rows, 100ms pause). These belong in Node 5 (Applier).
The correct Node 3 content for Phase 3 is: specify the batching requirement
(batch the update to avoid locking), the termination condition (continue until
EmailAddress IS NULL returns zero rows), and the monitoring requirement (track
replication lag and lock wait times). The exact SQL and parameters are applier
content and should be removed from the scheme plan.

Expected arbiter verdict: CONCERN. The pair is usable but the Phase 3 prompt
needs to be corrected before the pair is used to train a production scheme
planner. The boundary crossing is at the prompt body level; the plan structure
is correct.

---

**Scenario 3 — Hollow prerequisite probe (CB1-03-004, Step 1)**

Name: HOLLOW-PREREQUISITE-CACHE-PROBE

Tests: Failure mode 3 (coverage — wait state whose blocking condition is
resolved by the A input).

Why hard: The wait state exists, is syntactically present, and produces an
explicit "Confirm whether a cache invalidation event exists in the system"
check. A coverage check that looks for wait states will find one and pass.
Detecting that the wait state is hollow requires reading A and observing that
A already resolves the question before Step 1 runs. The hollowness is invisible
from the step alone.

A input: CB1-03-004 A ("shared cache required because cache invalidation must
propagate immediately") and B Step 1 (transformational prompt and wait state)
as written in the node file.

Expected finding: Step 1 asks the executor to determine whether shared cache
is required. A already states that shared cache is required. The wait state in
Step 1 is hollow — it produces no new information and will always be satisfied
trivially given A. The step should be reformulated to test something not already
in A — for example: "Confirm that the admin update handler has an injection point
for CacheManager where del() can be called" — which is a prerequisite Step 2
genuinely needs and which A does not resolve.

Expected arbiter verdict: CONCERN. Step 1 can be reformulated rather than
removed. The pair is structurally correct but Step 1 provides no examination
value.

---

### Step 4 — Oracle Documents Required for Round A

1. **Examination Domain Map** — defines the vocabulary for scheme-planner pair
   examination: "actual orchestration step" (specifies what, why, and when —
   not how); "applier-crossover" (transformational prompt specifies exact code
   or exact parameters); "parallel independence claim" (Reason field asserts
   steps are parallel and why); "hollow prerequisite" (wait state resolved by
   A). Required before Round B so the domain translator produces formal statements
   in this vocabulary rather than general language.

2. **Binding Failure Taxonomy** — defines three failure types with their
   observable signals and correct arbiter verdicts: (a) applier crossover —
   signal: exact SQL/code/parameter in transformational prompt body, verdict:
   CONCERN; (b) hollow prerequisite — signal: A resolves Step 1 decision before
   Step 1 runs, verdict: CONCERN; (c) parallel claim contradiction — signal:
   prompt body references sibling step's output, Reason field asserts independence,
   verdict: CONCERN. Distinguishes these from REPROCESS: a REPROCESS verdict
   applies if the step ordering itself is wrong, not if the wording creates a
   misleading appearance of wrong ordering.

3. **Expected Orchestration Trace** — three-scenario trace with expected
   node-by-node output. Most critical section: Node 3 (Scheme Planner) output
   for the examination task — the planned examination sequence must list the
   three failure modes as inspection targets before Node 4 processes them.

4. **Human Result Standards** — defines required sections: (a) findings (each
   named finding with its location in the source document and its reformulation
   recommendation); (b) arbiter verdicts (each verdict with its type and the
   specific text change that would resolve it); (c) overall disposition (the
   document is usable but three specific corrections are required before
   production training). Forbidden: a finding that names only the pair ID
   without naming the specific text location; a verdict without a stated
   correction path.

5. **Executor Contract** — names files to read, test names to add, evidence
   files to produce, pass gates to satisfy, and zip name. Every design decision
   is pre-made; the executor makes no design choices.

---

### Step 5 — Training Gap Map

| Node | Approach | Reason |
|---|---|---|
| 00 — Compiler | Human proxy | Mechanical step; compilation of examination input is deterministic |
| 01 — Domain Splitter | Human proxy | Single domain: "scheme-planner-examination" — no classification judgment needed |
| 02 — Domain Translator | New pair needed | Vocabulary of scheme-planner examination domain (applier crossover, hollow prerequisite) is not in prior training |
| 03 — Scheme Planner | New pair needed | The examination plan has a specific dependency ordering (find binding failures before enforcement failures); this dependency pattern is new |
| 04–06 — Processor, Applier, Translator | Human proxy | First pass; content is deterministic given the scheme plan |
| 07 — Arbiter Selector | New pair needed | Fire case (concern verdict on scheme-planner examination) and pass case both new to this domain |
| 08 — Arbiter Units | New pair needed | CONCERN verdict type for "applier crossover" and "hollow prerequisite" — new verdict context |
| 09 — Formatter | Human proxy | Existing format sufficient for examination report audience |

---

### Step 6 — Pass Gates and Failure Classification

| Gate | Condition | Failure means |
|---|---|---|
| GATE-1A | COUNT("applier crossover" or "Phase 3" in arbiter output for CB1-03-002) ≥ 1 | Node 8 did not recognize applier crossover. Fix: new Node 8 pair with applier-crossover concern verdict example |
| GATE-1B | COUNT("parallel" AND "cross-reference" in scheme planner output for CB1-03-001) ≥ 1 | Node 3 did not identify the parallel contradiction. Fix: new Node 3 pair with parallel-but-referenced step case |
| GATE-1C | COUNT("hollow" OR "already resolved" OR "Step 1 adds no information" in processor output for CB1-03-004) ≥ 1 | Hollow prerequisite not detected. Fix: taxonomy fix — add "hollow prerequisite" as named type in Binding Failure Taxonomy |
| GATE-1D | COUNT(REPROCESS verdicts) = 0 for all three scenarios | A REPROCESS appeared. Fix: content fix in Binding Failure Taxonomy — tighten distinction between concern and reprocess for scheme-planner examination |
| GATE-1E | Human reviewer confirms ≥ 3 actionable statements in final output, each naming a specific text location and a specific correction | Findings in machine trace did not reach human result. Fix: new Node 9 pair for scheme-planner examination audience |

---

---

## Experiment 2 — Domain Translator Examination (clean-batch-002 Node 2)

**Source:** `clean-batch-002/node-02-domain-translator-set.md`
**Pairs:** CB2-02-001 (React, C-frontend-rendering-performance),
CB2-02-002 (Django N+1, D-database-query-efficiency),
CB2-02-003 (BFF, A-api-topology-selection),
CB2-02-004 (Karpenter, A-node-provisioning-strategy)
**Domain:** Formal domain translation — converting domain labels and fragments
into measurable formal statements for downstream processing

---

### Central Question

Given the four domain translator pairs in CB2-02 and the formal translation
domain they govern, does each B correctly produce a formal domain description
that names the pathology, the measurable current state, and the required outcome
shape — staying within translation and not crossing into scheme planning — or
does any B add context that was not in A, name a solution rather than a required
outcome, or provide an outcome shape so specific that it functions as an
implementation recommendation rather than a formal domain statement?

---

### Step 1 — Claims

**Enforcement claim:** The domain translator must not add context that was not
in A. The claim is that B translates what is in A into a formal domain statement;
it does not synthesize, infer, or recommend. A translator that adds a target
metric not present in A is adding information, not translating.

**Binding claim 1:** Each B is correctly assigned to Node 2 (Domain Translator).
Node 2 produces formal domain statements. Node 3 (Scheme Planner) produces
ordered plans. The binding claim is that no B begins planning — that "the
required outcome is one JOIN query" is translation, but "the fix is to add
select_related" is planning.

**Binding claim 2:** Each pair handles exactly one domain label from the A input.
The array node rule requires one pair per element. The binding claim is that no
B attempts to translate two labels simultaneously.

**Coverage claim:** Each B covers three required elements: (a) the specific
pathology named by the domain label, (b) the measurable current state (numbers,
query counts, frame times, provisioning durations), (c) the required outcome
shape (what the correct state looks like when the problem is resolved, expressed
as a measurable condition).

**Sequencing claim:** Phrasing B appears after the original pair and rephrases
at equal depth — not summarizing, not simplifying.

**Honesty claim:** If B names a required outcome target (e.g., 30-60 seconds
for provisioning), that target must be traceable to A — either present in A
explicitly or derivable from A without additional knowledge. A target synthesized
by the translator from domain knowledge not in A is a honesty violation.

---

### Step 2 — Real Failure Modes

**Failure mode 1 — Required outcome crosses into scheme planning (CB2-02-002).
** CB2-02-002 B names the required outcome as "1 JOIN query — SELECT blog_post.*,
auth_user.* FROM blog_post INNER JOIN auth_user ON blog_post.author_id =
auth_user.id LIMIT 100." The SELECT clause is an exact SQL shape. A formal
domain statement of the required outcome would say "one query using a JOIN
between blog_post and auth_user, returning all requested fields in a single
round trip." The exact SELECT clause with the specific JOIN syntax is closer to
a specification of what the scheme planner or applier must produce than to a
formal domain statement of the required state. The claim that B stays in domain
translation territory appears satisfied — the output looks like a formal
description. The SQL shape inside it functions as a scheme plan seed.

**Failure mode 2 — Solution name embedded in formal description (CB2-02-003).**
CB2-02-003 B names "BFF pattern with 3 independent services" as the required
outcome. "BFF pattern" is a named architectural solution. A formal domain
statement of the required outcome would be: "three separate deployment units,
each with a field projection contract covering the exact fields its consumer
requires, deployable independently without coordination." Naming the BFF pattern
in B makes the formal description carry a solution recommendation — a Node 3
responsibility — inside a Node 2 output. The claim that B stays in formal
domain description territory appears satisfied because the statement is about
the topology. The violation is that a solution name is present where a required
state description should be.

**Failure mode 3 — Synthesized target not traceable to A (CB2-02-004).**
CB2-02-004 B states "30-60 second target" for node provisioning time. The A
input states that nodes take "2-5 minutes to provision." No A input states a
30-60 second target. The translator appears to have synthesized this target from
domain knowledge (Karpenter's known performance characteristics when using EC2
Fleet API direct calls) rather than translating it from A. The honesty claim
(B only formalizes what is in A) appears satisfied because the target is present
in the formal statement. The actual source is external knowledge, making B's
coverage claim partially unjustified.

---

### Step 3 — Three Scenarios

**Scenario 1 — SQL shape as translation vs specification (CB2-02-002)**

Name: SQL-SHAPE-TRANSLATION-BOUNDARY-PROBE

Tests: Failure mode 1 (binding — required outcome crosses into scheme planning).

Why hard: The exact SELECT clause is inside a formal domain description. It
looks like precision rather than a boundary crossing. An examiner that checks
whether B is "more formal than the A fragment" will find B more formal and pass.
Judgment is required to determine whether the SQL clause constitutes the domain
vocabulary for the required outcome or whether it already specifies what the
planner should produce.

A input: CB2-02-002 A and B as written in the node file.

Expected finding: The required outcome statement in CB2-02-002 B contains an
exact SELECT clause with specific JOIN syntax. The formal domain statement of
the required outcome should specify the JOIN relationship (posts and authors
retrieved in one round trip), the cardinality (one query for 100 posts and their
authors), and the type of join (inner join on the foreign key). The specific
SELECT keyword list and the full SQL syntax are beyond the domain translation
boundary — they belong in Node 3 or Node 4 where the scheme planner or domain
processor specifies the implementation shape.

Expected arbiter verdict: CONCERN. The pair is usable and the formal content
is otherwise correct. The SELECT clause should be replaced with a description
of the join structure and cardinality. The pair does not need reprocessing —
the core formal statement is correctly in translation territory.

---

**Scenario 2 — Solution name in formal description (CB2-02-003)**

Name: BFF-SOLUTION-NAME-IN-FORMAL-DESCRIPTION-PROBE

Tests: Failure mode 2 (binding — solution name embedded in required outcome).

Why hard: "BFF pattern" is widely known vocabulary in the API topology domain.
Naming it in a formal domain statement looks like domain vocabulary rather than
a solution recommendation. The difference between "the required topology is a
BFF pattern" (Node 2 acceptable if BFF is domain vocabulary for this label) and
"the solution is to implement BFF" (Node 3 content) is the distinction the
examiner must make.

A input: CB2-02-003 A and B as written in the node file.

Expected finding: "BFF pattern with 3 independent services" appears in the
required outcome section of CB2-02-003 B. "BFF" is a named architectural
pattern — naming it in a formal domain statement is appropriate only if the
A-api-topology-selection label's domain vocabulary includes named patterns as
valid formal outcomes. If the label's domain vocabulary covers only structural
properties (deployment unit count, field contract independence, deployment
coordination requirements), then "BFF pattern" is a solution name that belongs
in Node 3's scheme plan. The examiner must determine whether naming a pattern
is domain vocabulary or scheme recommendation.

Expected arbiter verdict: CONCERN. The required outcome should describe the
topology properties (three independent deployment units, independent field
contracts, independent deployment lifecycle) without naming the BFF pattern
by name. The pattern name belongs in Node 3's plan. The B is usable but the
solution-name embedding creates a risk that the scheme planner receives a
pre-named solution rather than a formal domain statement from which it must
derive a plan.

---

**Scenario 3 — Synthesized target traceability (CB2-02-004)**

Name: KARPENTER-TARGET-TRACEABILITY-PROBE

Tests: Failure mode 3 (honesty — required outcome target not traceable to A).

Why hard: The target appears in B and looks like a formal measurement. A reader
who checks whether a target is present will find it and pass the coverage check.
Tracing the target back to A requires reading A carefully and confirming that
"30-60 seconds" appears nowhere in A, then inferring that the translator added
it from external domain knowledge. This requires the examiner to distinguish
between "A contains this" and "the translator knows this about Karpenter."

A input: CB2-02-004 A ("nodes take 2-5 minutes to provision") and B (30-60
second target) as written in the node file.

Expected finding: The A input names the current provisioning time as 2-5
minutes. The B names the required outcome as 30-60 seconds. The 30-60 second
target does not appear in A — it is domain knowledge about Karpenter's
provisioning performance with EC2 Fleet API direct calls. The translator added
this target from external knowledge rather than translating it from A. The
formal domain statement should either omit a specific target (stating only that
the required outcome is "provisioning time reduced to below the scaling event
propagation window") or note that the target is external knowledge rather than
derived from the user's problem statement.

Expected arbiter verdict: CONCERN. The pair should note in the Reason field
that the 30-60 second target is derived from Karpenter domain knowledge rather
than from the A input. Without this note, a future examiner cannot determine
whether the target is from A or synthesized.

---

### Step 4 — Oracle Documents Required for Round A

1. **Examination Domain Map** — defines domain translator examination vocabulary:
   "translation boundary" (B formalizes A, does not add, recommend, or plan);
   "required outcome shape" (measurable state description vs solution name vs
   implementation specification); "target traceability" (target present in A
   vs synthesized from domain knowledge). Without this, Node 2 in the
   examination flow produces general statements rather than formal examination
   findings.

2. **Binding Failure Taxonomy** — three failure types: (a) SQL-shape crossover —
   required outcome contains specific implementation syntax, verdict CONCERN;
   (b) solution-name embedding — required outcome names a pattern rather than
   describing the structural property, verdict CONCERN; (c) synthesized target —
   B states a measurable target not traceable to A, verdict CONCERN. All three
   are CONCERN, not REPROCESS, because the pairs are usable with correction.

3. **Expected Orchestration Trace** — three scenarios. Most critical: Node 2
   output for the examination task — the formal domain statements for this
   examination must correctly name the translator-examination pathologies (not
   re-translate the original CB2-02 content).

4. **Human Result Standards** — required: each finding names its exact location
   (which pair, which section, which phrase), names what should replace it, and
   states whether the pair is usable or needs correction before training.
   Forbidden: a finding that says "the B is partially incorrect" without naming
   the specific phrase that is incorrect.

5. **Executor Contract** — files, tests, evidence, gates, zip.

---

### Step 5 — Training Gap Map

| Node | Approach | Reason |
|---|---|---|
| 00 — Compiler | Human proxy | Mechanical |
| 01 — Domain Splitter | Human proxy | Single domain: "domain-translator-examination" |
| 02 — Domain Translator | New pair needed | Domain translator examination vocabulary is not in prior training |
| 03 — Scheme Planner | New pair needed | Examination plan for a translation-boundary domain requires a new sequencing pattern |
| 04–06 | Human proxy | First pass |
| 07 — Arbiter Selector | New pair needed | Fire case (translation boundary violation) and pass case both new |
| 08 — Arbiter Units | New pair needed | CONCERN verdict for translation-boundary failures new to this domain |
| 09 — Formatter | Human proxy | Existing format sufficient |

---

### Step 6 — Pass Gates and Failure Classification

| Gate | Condition | Failure means |
|---|---|---|
| GATE-2A | COUNT("SQL" OR "SELECT clause" OR "implementation syntax" in arbiter output for CB2-02-002) ≥ 1 | SQL-shape crossover not detected. Fix: new Node 8 pair with SQL-shape-crossover concern example |
| GATE-2B | COUNT("pattern name" OR "solution name" OR "BFF" in arbiter output for CB2-02-003) ≥ 1 | Solution-name embedding not detected. Fix: taxonomy fix — add "solution-name embedding" as named type |
| GATE-2C | COUNT("not in A" OR "synthesized" OR "external knowledge" OR "30-60" in arbiter output for CB2-02-004) ≥ 1 | Synthesized target not detected. Fix: new Node 2 pair teaching target-traceability examination |
| GATE-2D | COUNT(REPROCESS verdicts) = 0 | Incorrect verdict severity. Fix: taxonomy content fix |
| GATE-2E | Human reviewer confirms ≥ 3 findings each naming specific phrase and correction path | Findings absent from human result. Fix: Node 9 pair for translator-examination audience |

---

---

## Experiment 3 — Domain Processor Examination (clean-batch-002 Node 4)

**Source:** `clean-batch-002/node-04-domain-processor-set.md`
**Pairs:** CB2-04-001 (select_related sub-task), CB2-04-002 (consistent-hash
routing), CB2-04-003 (nullable column migration), CB2-04-004 (Redis CacheModule)
**Domain:** Sub-task outcome space specification — success/failure/wait-state
coverage for infrastructure and database orchestration sub-tasks

---

### Central Question

Given the four domain processor pairs in CB2-04 and the sub-task outcome domain
they govern, does each B correctly enumerate every reachable success state, every
named failure state with its specific trigger and observable signal, and every
wait state with a specific resolution check — or does any B name a failure state
without a specific observable signal, name a wait state whose resolution condition
is not checkable, or produce failure states that appear distinct but describe the
same root cause at different stages of the same failure path?

---

### Step 1 — Claims

**Enforcement claim:** The node header states that "a B that says 'the outcomes
include success and failure cases' is wrong — each outcome must be named with
its specific condition and specific result." The enforcement check is that B
contains specific conditions and specific results, not descriptions of what
those would address.

**Coverage claim 1:** Every failure state has a specific observable trigger —
not just a general failure condition, but the specific observable signal that
indicates this failure state has occurred rather than a different one.

**Coverage claim 2:** Every wait state names a specific check that confirms the
blocking condition is resolved. "Confirm it is complete" is not a specific check.
"Confirm the migration ID appears in __EFMigrationsHistory" is a specific check.

**Sequencing claim:** Success state appears before failure states; failure states
appear before wait states. Within failure states, the ordering is by likelihood
or severity — the most likely failure comes first.

**Honesty claim:** Failure states that exist in the domain but are silent (no
observable signal before they manifest as downstream errors) are either named
explicitly with the caveat that they are silent, or the Reason field acknowledges
the coverage gap.

**Binding claim:** B is success/failure/wait outcome content for Node 4 (Domain
Processor). It is not a plan (Node 3), not an action (Node 5). A B that says
"the fix is to add .only() if needed" is crossing into Node 5 (Applier)
territory where the mitigation action is specified.

---

### Step 2 — Real Failure Modes

**Failure mode 1 — Failure state with silent signal (CB2-04-002, Failure state 2).
** CB2-04-002 Failure state 2 says "TraceId is propagated in the W3C traceparent
header. If the load balancer cannot parse the traceparent header to extract the
traceId, it cannot perform consistent routing." The observable signal for this
failure is absent. If the load balancer silently fails to parse traceparent and
falls back to round-robin, spans will scatter across instances — but this will
not produce an error log on the load balancer. The failure will only become
visible downstream when the tail sampling processor fires on incomplete trace
data. The failure state names the condition but not the observable signal. The
coverage claim (every failure state has an observable signal) appears satisfied
because a condition is named. The actual signal is downstream and indirect.

**Failure mode 2 — Wait state resolution check not named (CB2-04-002, Wait state)
.** CB2-04-002 Wait state says "Steps 3 and 4 must not be applied until the
synthetic trace test confirms routing affinity on a live trace." The success
state names "all 5 spans land on one Collector instance" as the check. The wait
state references this implicitly but does not name it as the resolution check.
The coverage claim (wait states name specific resolution checks) appears
satisfied because the success state contains the check. The wait state itself
does not state the check — an applier reading only the wait state would not know
what specific test to run before proceeding.

**Failure mode 3 — Two failure states with the same root cause (CB2-04-003,
Failure states 1 and 3).** CB2-04-003 Failure state 1 says "if the migration
specifies NVARCHAR(256) NOT NULL DEFAULT '' instead of NULL, SQL Server must
update every existing row to set the default value, acquiring a lock." Failure
state 3 says "If a subsequent migration immediately adds a NOT NULL constraint
before the backfill runs in Phase 3, existing rows with NULL EmailAddress violate
the constraint and the migration fails." Both failure states have the same root
cause — a NOT NULL constraint present before all rows have values. They differ
in timing: Failure 1 is NOT NULL at column creation time; Failure 3 is NOT NULL
constraint added in a subsequent migration before backfill. If the arbiter
cannot distinguish these by observable signal (Failure 1 manifests as table lock
during migration; Failure 3 manifests as constraint violation during a later
migration), it may classify them as duplicates. The coverage claim (failure
states are distinct) appears satisfied because they are listed as separate
entries with separate names. Their root cause is the same.

---

### Step 3 — Three Scenarios

**Scenario 1 — Silent failure signal probe (CB2-04-002, Failure state 2)**

Name: TRACEPARENT-SILENT-SIGNAL-PROBE

Tests: Failure mode 1 (coverage — failure state has no observable signal).

Why hard: The failure state names the condition clearly. An examiner checking
for "specific condition" will find one and pass. Detecting the missing signal
requires determining whether the condition description comes with a direct
observable signal (an error on the load balancer) or whether the signal is
indirect (wrong downstream behavior after the condition is present). This
requires understanding that traceparent parsing failure on a load balancer
does not produce an error log — it silently falls back to a default routing
strategy.

A input: CB2-04-002 B Failure state 2 as written in the node file.

Expected finding: Failure state 2 names the condition (load balancer cannot
parse traceparent) and the consequence (spans scatter). It does not name the
observable signal for the condition — specifically, that this failure is silent
on the load balancer and only becomes visible as downstream trace incompleteness.
The failure state should add: "Observable signal: Collector metrics show
'incomplete trace' or 'spans from unknown trace' counters incrementing after
the routing change is deployed. The load balancer does not log a parsing error —
the failure is only visible downstream." Without this, the applier has no
direct check to run that confirms the failure state has not occurred.

Expected arbiter verdict: CONCERN. The pair is usable but Failure state 2
should add an explicit statement of the observable signal (or explicitly
acknowledge that the signal is downstream-only). The applier cannot confirm
routing correctness without a direct check.

---

**Scenario 2 — Wait state resolution check self-containedness (CB2-04-002)**

Name: WAIT-STATE-RESOLUTION-CHECK-PROBE

Tests: Failure mode 2 (coverage — wait state resolution check not named in the
wait state itself).

Why hard: The success state names the resolution check. The wait state refers
to the success state test without naming it explicitly. An examiner that reads
success state and wait state together will find the check. An applier reading
only the wait state to determine whether to proceed will not find a specific
check — they will see "the synthetic trace test confirms routing affinity" but
not the specific test (5 spans from one trace, all landing on one instance,
"incomplete trace" counter not incrementing). The wait state must be
self-contained.

A input: CB2-04-002 B Wait state and Success state as written in the node file.

Expected finding: The wait state says "the synthetic trace test confirms routing
affinity on a live trace" but does not name the specific test. The resolution
check from the success state (5-span synthetic trace, all spans on one instance,
no "incomplete trace" metrics) should appear in the wait state, not just in
the success state. As written, the wait state depends on the success state to
specify its resolution condition — it is not self-contained.

Expected arbiter verdict: CONCERN. The wait state should include the specific
resolution check inline so that the applier does not need to re-read the success
state to determine when the blocking condition is resolved.

---

**Scenario 3 — Indistinct failure state root causes (CB2-04-003, Failure
states 1 and 3)**

Name: NOT-NULL-ROOT-CAUSE-DISTINCTION-PROBE

Tests: Failure mode 3 (coverage — failure states with the same root cause
appear as distinct entries).

Why hard: Failure state 1 (NOT NULL at column creation) and Failure state 3
(NOT NULL constraint added too early in subsequent migration) have different
observable manifestations — Failure 1 causes a table lock during the initial
migration; Failure 3 causes a constraint violation during a later migration.
Their root cause is the same (NOT NULL constraint before all rows have values)
but their timing and observable signals differ. An examiner must determine
whether the observable signal difference is sufficient to justify two distinct
failure entries, or whether both should be collapsed into one entry with
stage-specific notes.

A input: CB2-04-003 B Failure state 1 and Failure state 3 as written in the
node file.

Expected finding: Failure state 1 and Failure state 3 share the same root
cause (NOT NULL constraint before all rows have values). Their observable signals
differ: Failure 1 produces a blocking lock during the initial migration; Failure
3 produces a constraint violation exception during a later migration. The two
entries are justified as distinct because they occur at different phases with
different observable signals and different remediation paths (fix: specify NULL
at column creation vs fix: defer NOT NULL to Phase 5). However, the Reason
field does not state why both entries exist rather than one combined entry. A
note is needed: "These two failure states have the same root cause but different
observable manifestations at different phases — they are kept separate because
the remediation for each is phase-specific."

Expected arbiter verdict: CONCERN. The two entries are correct and should not
be collapsed. The Reason field should be extended to explain why both entries
exist rather than one, so future examiners do not conclude they are duplicates.

---

### Step 4 — Oracle Documents Required for Round A

1. **Examination Domain Map** — defines domain processor examination vocabulary:
   "observable signal" (the direct check that confirms a failure state has or
   has not occurred, distinguished from downstream evidence); "wait state
   self-containedness" (the wait state names its own resolution check, not just
   references the success state); "root-cause distinctness" (two failure states
   are distinct when their observable signals and remediation paths differ,
   regardless of shared root cause).

2. **Binding Failure Taxonomy** — three failure types: (a) silent failure state —
   condition named but observable signal absent or downstream-only, verdict
   CONCERN; (b) non-self-contained wait state — resolution check named only
   in success state, verdict CONCERN; (c) shared-root-cause entries without
   justification — Reason field does not explain why entries are separate,
   verdict CONCERN.

3. **Expected Orchestration Trace** — three scenarios with node-by-node output.
   Critical: Node 4 output for the examination task must name the three failure
   modes as specific outcome types before Node 7 arbitrates.

4. **Human Result Standards** — required: each finding names the pair, the
   specific section (Failure state 2, Wait state), the specific gap, and the
   specific text addition that would resolve it.

5. **Executor Contract** — files, tests, evidence, gates, zip.

---

### Step 5 — Training Gap Map

| Node | Approach | Reason |
|---|---|---|
| 00–01 | Human proxy | Mechanical |
| 02 | New pair needed | Domain processor examination vocabulary (observable signal, wait state self-containedness) not in prior training |
| 03 | New pair needed | Examination plan for outcome-space domain requires new step dependency pattern |
| 04–06 | Human proxy | First pass |
| 07 | New pair needed | Fire case for domain-processor examination domain new |
| 08 | New pair needed | CONCERN verdict for silent-signal failure and non-self-contained wait state new |
| 09 | Human proxy | Existing format sufficient |

---

### Step 6 — Pass Gates and Failure Classification

| Gate | Condition | Failure means |
|---|---|---|
| GATE-3A | COUNT("observable signal" OR "downstream" OR "silent" in arbiter output for CB2-04-002 Failure 2) ≥ 1 | Silent signal not detected. Fix: new Node 8 pair with silent-failure-state concern example |
| GATE-3B | COUNT("self-contained" OR "wait state" AND "resolution check" in arbiter output for CB2-04-002 Wait state) ≥ 1 | Wait state dependency not detected. Fix: new Node 2 pair for wait-state-self-containedness domain vocabulary |
| GATE-3C | COUNT("root cause" OR "distinct" OR "phase-specific" in arbiter output for CB2-04-003 Failure states 1 and 3) ≥ 1 | Root-cause overlap not detected. Fix: taxonomy fix — add "shared-root-cause entries" as named type |
| GATE-3D | COUNT(REPROCESS verdicts) = 0 | Incorrect severity. Fix: Binding Failure Taxonomy content fix |
| GATE-3E | Human reviewer confirms ≥ 3 findings each with specific section reference and specific text addition | Findings not propagated to human result. Fix: Node 9 pair for domain-processor examination audience |

---

---

## Experiment 4 — Arbiter Selector Examination (clean-batch-002 Node 7)

**Source:** `clean-batch-002/node-07-arbiter-selector-set.md`
**Pairs:** CB2-07-001 (Django N+1 fire case), CB2-07-002 (Dapper pass-through)
**Domain:** Arbiter selection — correctly dispatching named review units and
correctly approving results for formatting in the pass-through case

---

### Central Question

Given the two arbiter selector pairs in CB2-07 and the arbiter orchestration
domain they govern, does CB2-07-001 correctly name every available review unit
with its scope and fire/no-fire verdict, does CB2-07-002 correctly produce the
explicit "Results are approved for formatting" terminal state with justified
inline review for each no-fire verdict — or does the fire case produce a verdict
without a scope, dispatch a review unit that is not in the available list,
produce the pass-through case as silent, or produce inline review that names
a general conclusion without referencing specific result items?

---

### Step 1 — Claims

**Enforcement claim 1:** Every available review unit in the flow context
receives an explicit verdict — fire with scope, or no fire with reason. Silence
is not a valid selector output for any available unit.

**Enforcement claim 2:** The pass-through case (no arbiter fires) must conclude
with the explicit statement "Results are approved for formatting." Silence is
not a valid terminal state — the formatter has no dispatch signal without the
explicit approval.

**Binding claim:** The selector maps domain labels to the correct review unit
names from the available list. A selector that fires a review unit not in the
available list is binding to a non-existent resource. A selector that uses an
approximate name (e.g., "database-efficiency-arbiter" instead of
"database-query-efficiency-arbiter") binds to a unit that does not exist.

**Coverage claim:** Every result in [R1] through [R4] is addressed — either
a fired arbiter names it in its scope, or a no-fire verdict names it in its
justification. No result passes through without an explicit disposition.

**Sequencing claim:** Fire verdicts appear before no-fire verdicts in each B.
Within the fire verdicts, the primary domain arbiter appears before secondary
domain arbiters.

**Honesty claim:** The inline review in the no-fire verdicts names specific
result items examined, not just a general conclusion. "No review required" is
not sufficient — "No authentication logic, no authorization changes, no
user-supplied values inserted into queries, and no deserialization of untrusted
input" is the required form.

---

### Step 2 — Real Failure Modes

**Failure mode 1 — Fire verdict without scope (CB2-07-001 candidate).** A fire
verdict that says "database-query-efficiency-arbiter: FIRE" without naming the
result items in its scope produces a structurally correct fire dispatch but
leaves the review unit without material to examine. The enforcement claim (every
fire verdict includes scope) appears satisfied if "FIRE" is present. The scope
("[R1] and [R2] together") is the mechanism that tells the arbiter which results
to examine. Without scope, the arbiter receives a dispatch but no work item. As
written, CB2-07-001 does correctly include scope. This scenario tests whether
the examiner detects that scope is required — and can identify where it is
missing in a modified version.

**Failure mode 2 — Review unit not in available list (CB2-07-001, B-backend-
response-latency).** The flow context for CB2-07-001 lists four available review
units: database-query-efficiency-arbiter, orm-usage-pattern-arbiter,
security-arbiter, connection-management-arbiter. The domain label
B-backend-response-latency is active but has no corresponding available review
unit. [R4] (render time measured: 0.38 seconds) is a result in the B-backend-
response-latency domain. A selector that interprets B-backend-response-latency
as requiring arbiter dispatch would produce a verdict for a unit not in the
available list — either inventing "backend-response-latency-arbiter" or
attempting to fire connection-management-arbiter on [R4] because it is the
closest available unit. The binding claim (selector maps to available units
only) appears satisfied in CB2-07-001. This scenario tests whether the examiner
detects the [R4] disposition gap and evaluates whether its handling is correct.

**Failure mode 3 — Pass-through inline review without specific result reference
(CB2-07-002).** CB2-07-002 connection-management-arbiter NO FIRE verdict
provides inline review: "Opening one connection outside the loop and reusing
it for 500 operations is the correct pattern." This is a substantive inline
review. The question is whether it explicitly references the specific result
items ([R1], [R2], [R3], [R4]) that it examined, or whether it describes a
general conclusion. The current text describes the pattern rather than citing
the specific results. An inline review that names [R1] ("SqlConnection opened
once before the processing loop"), [R2] ("500 INSERT operations reuse the
same connection"), [R3] ("using block disposes on exception"), [R4] ("1 pool
slot checked out") provides traceable justification. The honesty claim (inline
review names specific items examined) appears partially satisfied by the
substantive content. The specific result references are absent from the inline
review body.

---

### Step 3 — Three Scenarios

**Scenario 1 — [R4] disposition without matching arbiter (CB2-07-001)**

Name: R4-NO-ARBITER-DOMAIN-DISPOSITION-PROBE

Tests: Failure mode 2 (coverage — active domain label has no available review
unit; how is the corresponding result handled?).

Why hard: [R4] is a measurement result in the B-backend-response-latency domain.
The Reason field in CB2-07-001 explains that B-backend-response-latency is
"informational" and its arbiter is not available. But the Reason field is not
part of B — it is metadata. B itself does not contain a statement about [R4] in
the no-fire verdicts. An examiner who reads B alone finds that [R4] is not named
in any no-fire verdict. Determining whether [R4] is correctly handled or whether
the selector silently omitted it requires checking the Reason field and the flow
context simultaneously.

A input: CB2-07-001 B (full selector output) and flow context as written in the
node file.

Expected finding: [R4] (render time: 0.38 seconds) belongs to the B-backend-
response-latency domain label, which is active but has no available review unit.
The selector correctly does not dispatch a non-existent review unit. However,
the B does not include an explicit statement for [R4] in the no-fire section
explaining why it is not sent to an arbiter. The no-fire section covers
security-arbiter and connection-management-arbiter for the available units that
do not apply. [R4]'s disposition — "B-backend-response-latency is active but
has no available arbiter; [R4] is a measurement confirming the fix, not a
decision requiring arbitration" — should appear as an explicit entry in the
selector output.

Expected arbiter verdict: CONCERN. The disposition of [R4] is correct but
implicit. Making it explicit prevents future training from concluding that the
selector silently omitted a result.

---

**Scenario 2 — Scope completeness in fire verdicts (CB2-07-001)**

Name: FIRE-SCOPE-COMPLETENESS-PROBE

Tests: Failure mode 1 (enforcement — fire verdict scope names the specific
result items).

Why hard: Both fire verdicts in CB2-07-001 do include scope. This scenario
tests whether the examiner can verify that scope is complete — that [R1] and
[R2] for database-query-efficiency-arbiter is the correct scope, and that [R1]
alone for orm-usage-pattern-arbiter is the correct scope. Scope completeness
requires knowing which results each arbiter is responsible for examining, which
requires understanding the arbiter domain and the content of each result.

A input: CB2-07-001 B fire verdicts and [R1]–[R4] result descriptions as written
in the node file.

Expected finding: database-query-efficiency-arbiter fires on [R1] and [R2]
together — [R1] is the queryset change; [R2] is the query count confirmation.
These two results are correctly scoped together because the efficiency arbiter
evaluates whether the count claim is supported by the change. [R3] (regression
test) and [R4] (render time) are not in scope for database-query-efficiency-
arbiter — [R3] belongs to the regression-testing domain (no available arbiter),
and [R4] belongs to B-backend-response-latency (no available arbiter). Scope
is correct. orm-usage-pattern-arbiter fires on [R1] only — correct, because [R1]
is the ORM change and [R2]–[R4] are not ORM pattern concerns. The scopes are
correct and complete.

Expected arbiter verdict: PASS. Both fire verdicts have correct and complete
scopes. No finding required.

---

**Scenario 3 — Inline review result reference completeness (CB2-07-002)**

Name: PASS-THROUGH-INLINE-REVIEW-REFERENCE-PROBE

Tests: Failure mode 3 (honesty — inline review describes conclusion without
referencing specific result items).

Why hard: The inline review in CB2-07-002 is substantive — it explains the
correct connection management pattern clearly. An examiner checking whether
inline review is "present" will find it and pass. Detecting that the specific
result items ([R1]–[R4]) are not referenced requires checking the inline review
against the result list and noticing that the review describes a general pattern
rather than examining specific results.

A input: CB2-07-002 B (connection-management-arbiter NO FIRE verdict and inline
review) and [R1]–[R4] result descriptions as written in the node file.

Expected finding: The connection-management-arbiter inline review explains the
correct pattern (one connection opened once, reused for 500 operations, using
block guarantees disposal) but does not name [R1], [R2], [R3], [R4] by reference.
[R1] is the open-once call; [R2] is the loop reuse; [R3] is the using block
disposal; [R4] is the pool slot count. The inline review should tie each
conclusion to a specific result: "[R1] — SqlConnection opened before the loop:
correct. [R2] — 500 INSERTs reuse the same connection: correct. [R3] — using
block ensures Dispose() on exception: correct. [R4] — 1 pool slot checked out
and returned: correct." Without result references, the inline review cannot
be verified against the actual results.

Expected arbiter verdict: CONCERN. The inline review content is correct but
should be extended to reference result items explicitly.

---

### Step 4 — Oracle Documents Required for Round A

1. **Examination Domain Map** — defines arbiter selector examination vocabulary:
   "scope" (the named result items the review unit will examine); "dispatch to
   available unit only" (the selector cannot name a review unit not in the
   available list); "explicit approval" ("Results are approved for formatting"
   as required terminal statement); "inline review completeness" (result items
   named individually, not described in aggregate); "disposition of no-arbiter
   domain" (active domain label with no available unit requires explicit
   statement).

2. **Binding Failure Taxonomy** — failure types: (a) scope-missing fire — verdict
   "FIRE" without "[Rx]" reference, verdict CONCERN; (b) non-available unit
   dispatch — selector names a unit not in the flow context available list,
   verdict REPROCESS (this is a hard binding error, not a wording concern);
   (c) implicit no-arbiter-domain disposition — active domain label has no
   available unit and the selector is silent about the result in that domain,
   verdict CONCERN; (d) general inline review — conclusion without result item
   references, verdict CONCERN.

3. **Expected Orchestration Trace** — three scenarios. Most critical: Node 7
   output for the selector examination — the selector examining CB2-07-001
   must identify the [R4] implicit disposition and confirm scope completeness
   before Node 8 arbitrates.

4. **Human Result Standards** — required: each finding named with its specific
   pair and specific verdict entry; PASS verdict for Scenario 2 explicitly
   confirmed; CONCERN verdicts for Scenarios 1 and 3 with specific addition
   recommendations.

5. **Executor Contract** — files, tests, evidence, gates, zip.

---

### Step 5 — Training Gap Map

| Node | Approach | Reason |
|---|---|---|
| 00–01 | Human proxy | Mechanical |
| 02 | New pair needed | Arbiter selector examination vocabulary (scope, inline review completeness, no-arbiter domain) not in prior training |
| 03 | New pair needed | Examination plan with a PASS scenario alongside CONCERN scenarios is a new pattern |
| 04–06 | Human proxy | First pass |
| 07 | Existing pair usable | CB2-07-001 and CB2-07-002 provide the fire/pass cases — the examiner uses these as input, not the selector itself |
| 08 | New pair needed | PASS verdict for Scenario 2 is a new verdict outcome in this examination context |
| 09 | Human proxy | Existing format sufficient |

---

### Step 6 — Pass Gates and Failure Classification

| Gate | Condition | Failure means |
|---|---|---|
| GATE-4A | COUNT("[R4]" AND "no available arbiter" OR "informational" in arbiter output for Scenario 1) ≥ 1 | Implicit [R4] disposition not detected. Fix: taxonomy fix — add "no-arbiter domain disposition" as named type |
| GATE-4B | Scenario 2 arbiter output = PASS (no finding, no concern) | False positive on scope completeness. Fix: new Node 8 pair with scope-correct pass example |
| GATE-4C | COUNT("result item" OR "[R1]" OR "[R2]" in arbiter output for CB2-07-002 inline review) ≥ 1 | Result-item references not detected. Fix: new Node 2 pair for inline-review-completeness vocabulary |
| GATE-4D | Scenario 1 and 3 verdicts = CONCERN; no REPROCESS for Scenarios 1 and 3 | Incorrect severity (REPROCESS on wording-only finding). Fix: Binding Failure Taxonomy content fix |
| GATE-4E | Human reviewer confirms Scenario 2 as PASS and Scenarios 1 and 3 as CONCERN with named additions | Verdict type not propagated to human result. Fix: Node 9 pair for arbiter-selector examination audience |

---

---

## Experiment 5 — Cascade Unblock Scheme Plan Examination (clean-batch-003 Node 3)

**Source:** `clean-batch-003/node-03-scheme-planner-set.md`
**Pairs:** CB3-03-001 (Django N+1 scheme plan, 1 pair + Phrasing B)
**Domain:** Scheme plan for a known-gap scenario — produced from a partially
formal A input (D-database-query-efficiency translated; E-orm-usage-pattern
and B-backend-response-latency not translated, labels with fragments only)

---

### Central Question

Given the single scheme plan pair in CB3-03 and the orchestration domain it
governs, does the B correctly produce an ordered scheme plan with actual steps,
actual transformational prompts, actual wait states, and an actual recovery path
— where the plan is appropriately scoped to the formally translated domain
(D-database-query-efficiency) and explicitly names the two untranslated labels
as known gaps rather than silently incorporating their content — or does the
plan silently incorporate content from the untranslated labels without noting
this, produce a prerequisite step whose confirmation condition is not
specifically checkable, or produce a recovery path that covers one branch but
silently omits a likely second branch?

---

### Step 1 — Claims

**Enforcement claim:** Step 1 is stated as a hard prerequisite that blocks all
subsequent steps. The enforcement check is that Step 1 has a confirmation
condition specific enough that an executor can confirm it has been met before
proceeding. "Confirm Post.author is a ForeignKey" is the stated check. The
enforcement claim is that this check has a specific observable form.

**Binding claim:** The A input contains two untranslated labels (E-orm-usage-
pattern and B-backend-response-latency) in addition to the formally translated
D-database-query-efficiency label. The binding claim is that the plan correctly
works with the formal translation for D and explicitly notes where it is working
from label fragments for E and B. The plan should not silently incorporate E
or B content as if it were formally translated.

**Coverage claim:** The recovery path covers the failure mode where assertNumQueries
(Step 4) returns a count different from Django Debug Toolbar (Step 3). The
coverage claim is that this is the most likely failure branch and that the
recovery path for it is complete.

**Sequencing claim:** Steps 1 through 4 have correctly stated dependencies:
Step 1 blocks Steps 2–4; Step 2 depends on Step 1; Step 3 depends on Step 2
and blocks Step 4; Step 4 depends on Step 3. The sequencing claim is that this
dependency graph is complete and correct.

**Honesty claim 1:** The review point says "measure render time, target under
0.5s." This target must be traceable to the A input. The A input contains the
fragment "renders in 4.2 seconds" but does not name a 0.5s target. The 0.5s
target appears in CB2-04-001 (the domain processor pair for the select_related
sub-task). The honesty claim is that the scheme plan sources its targets from
its inputs or from named upstream pairs, not from domain knowledge added by
the planner.

**Honesty claim 2:** The B notes that E-orm-usage-pattern and B-backend-
response-latency are in the A as untranslated labels with fragments. The honesty
claim is that the B explicitly acknowledges working from fragments for those two
labels rather than silently treating them as translated.

---

### Step 2 — Real Failure Modes

**Failure mode 1 — Prerequisite check without specific observable form (Step 1).
** Step 1 says "Confirm Post.author is a ForeignKey [prerequisite; blocks Steps
2–4]" with the transformational prompt "Verify that Post.author is defined as
author = models.ForeignKey(User, on_delete=models.CASCADE) or equivalent." The
observable form is "check the model definition." But the check does not specify
where to look (the Django model file), what the specific signal of a passing
check is (models.ForeignKey present in the field definition), or what the signal
of a failing check is (models.ManyToManyField or reverse relation instead). An
enforcement check whose passing condition is "look at the model definition" is
specific enough for a human executor but too ambiguous for training a scheme
planner. The enforcement claim (Step 1 is a hard prerequisite with a checkable
condition) appears satisfied because a condition is stated. The condition's form
is underspecified.

**Failure mode 2 — Recovery path covers one branch, silently omits a second.
** The recovery path says "if assertNumQueries differs from Toolbar count,
isolate view call inside context manager." This covers the case where Step 4's
test and Step 3's Toolbar show different counts. It does not cover the case
where Step 4's test and Step 3's Toolbar both show 1 query, but the production
query count is still 101 — possible if the test and Toolbar are testing the view
in a context where select_related is already applied to the queryset at a
different layer (e.g., a query cache or a test fixture that pre-selects). The
coverage claim (the recovery path covers the most likely failure branch)
appears satisfied because a recovery path is present. The second branch —
where the test passes but the problem persists in production — is not named.

**Failure mode 3 — Review point target sourced from adjacent pair, not from A.
** The review point says "measure render time, target under 0.5s." The 0.5s
target is not in the A input for CB3-03-001 — it appears in the A input for
CB2-02-002 (which mentions the 4.2 second render time) but does not state a
0.5s target. The 0.5s target appears in CB2-04-001 (domain processor success
state: "render time: under 0.5 seconds, down from 4.2 seconds"). The scheme
plan's review point is sourcing a target from the domain processor output
(CB2-04-001), not from its own A input. The honesty claim (the B sources
targets from its inputs) is violated: the 0.5s target is not in the A input
for CB3-03-001. The review point target appears well-sourced because it is
a reasonable target for the scenario. Its provenance is from an adjacent pair,
not from A.

---

### Step 3 — Three Scenarios

**Scenario 1 — Step 1 confirmation specificity probe**

Name: FKEY-CONFIRMATION-SPECIFICITY-PROBE

Tests: Failure mode 1 (enforcement — prerequisite check without specific
observable form).

Why hard: Step 1 names a condition (Post.author is a ForeignKey) and names a
check (verify the model definition). This looks specific — it names the model
and the field type. Detecting the underspecification requires distinguishing
between "a condition is named" and "the passing and failing forms of the check
are named." The specific file location, the specific pattern that indicates a
pass, and the specific pattern that indicates a fail are all absent. An examiner
checking for "is a condition named?" will pass. An examiner checking for "can an
executor confirm this condition without additional information?" will find the
gap.

A input: CB3-03-001 B Step 1 (transformational prompt, wait state) as written
in the node file.

Expected finding: The Step 1 confirmation condition is underspecified. The
transformational prompt says "Verify that Post.author is defined as
author = models.ForeignKey(User, on_delete=models.CASCADE) or equivalent." The
specific location to check (the Django model file, not the admin or serializer)
is absent. The specific passing form is partially named (models.ForeignKey) but
"or equivalent" is too vague — the equivalent forms should be named: ForeignKey
with any on_delete setting. The specific failing form (ManyToManyField or
reverse ForeignKey) is absent from the step but present in the recovery
material in CB2-04-001. The step should add: "Check models.py or the equivalent
model definition file. Passing: post.author is models.ForeignKey to any User
model. Failing: post.author is models.ManyToManyField or a reverse relation
— in this case, use prefetch_related instead of select_related."

Expected arbiter verdict: CONCERN. Step 1 is correct but needs additional
specificity to be useful as a training example. The check should name its
passing and failing forms explicitly.

---

**Scenario 2 — Recovery path second branch omission probe**

Name: PRODUCTION-MISMATCH-RECOVERY-PROBE

Tests: Failure mode 2 (coverage — recovery path covers one failure branch
and silently omits a second).

Why hard: The recovery path is substantive — it covers the case where
assertNumQueries and Toolbar counts differ, and it names a specific remedy
(isolate view call in context manager). The question is not whether a recovery
path is present, but whether the recovery path covers all likely failure
branches. Detecting the missing branch requires domain knowledge about Django
caching and test fixture behavior — specifically, that a test can pass while
production still has N+1 queries if the test is executed in a context where
query results are pre-cached.

A input: CB3-03-001 B Recovery path and review point as written in the node
file.

Expected finding: The recovery path covers one branch: assertNumQueries
(Step 4 test) returns a different count than Django Debug Toolbar (Step 3).
It does not cover a second branch: both Step 3 and Step 4 report 1 query,
but production query count remains at 101. This second branch can occur if
the test is executed in a context where another component caches the queryset
or if the select_related is applied to a queryset that is not used in the
production view (e.g., applied to a test fixture queryset rather than the
view's own queryset). The plan should add a production verification step:
"After Step 3 and Step 4 pass, deploy to a staging environment and measure
query count in production conditions. If the production query count remains
at 101, the select_related is not applied to the queryset the view is
actually using — confirm that the view function's queryset definition is
the one modified in Step 2."

Expected arbiter verdict: CONCERN. The recovery path is correct for the
branch it covers. The missing branch should be added. The plan is usable
for the training scenario it currently covers.

---

**Scenario 3 — Review point target provenance probe**

Name: RENDER-TIME-TARGET-PROVENANCE-PROBE

Tests: Failure mode 3 (honesty — review point target sourced from adjacent
pair, not from A input).

Why hard: The 0.5s target is reasonable and directly relevant to the scenario.
An examiner who checks whether a target is present will find it and pass.
Detecting the provenance gap requires checking whether 0.5s appears in the
A input for CB3-03-001 or only in adjacent pairs (CB2-04-001 success state).
The target is consistent with the scenario but its source is external to the
A input.

A input: CB3-03-001 A (including the D-database-query-efficiency formal
translation from CB2-02-002, and the E and B label fragments) and B Review
point ("measure render time, target under 0.5s") as written in the node file.

Expected finding: The 0.5s render time target in the review point does not
appear in the A input for CB3-03-001. The A input contains "renders in 4.2
seconds" (from the original SWE-bench source) but does not state a 0.5s
target. The 0.5s target appears in CB2-04-001 (domain processor success state:
"Expected render time: under 0.5 seconds, down from 4.2 seconds"). The scheme
plan is sourcing this target from the domain processor's success state rather
than from its own A input. This is either a correct cross-pair dependency
(the scheme plan is allowed to reference domain processor outputs) or a honesty
violation (the scheme plan incorporates a target from a source not cited in A).
The Reason field should state that the 0.5s target is sourced from CB2-04-001
rather than from the A input.

Expected arbiter verdict: CONCERN. The target is correct and the cross-pair
sourcing is defensible. The Reason field should name CB2-04-001 as the source
of the 0.5s target so the pair is traceable. Without this note, a future
examiner cannot determine whether the target is from A or synthesized by the
planner.

---

### Step 4 — Oracle Documents Required for Round A

1. **Examination Domain Map** — defines vocabulary for cascade-unblock scheme
   plan examination: "prerequisite check specificity" (passing form, failing
   form, and file location all named); "recovery path branch coverage" (each
   likely failure branch has a named recovery action); "target provenance"
   (target in review point traced to A input or to a named upstream pair);
   "untranslated label handling" (plan correctly scopes to formally translated
   labels and acknowledges working from fragments for others).

2. **Binding Failure Taxonomy** — failure types: (a) underspecified check —
   prerequisite condition named but passing/failing forms absent, verdict
   CONCERN; (b) single-branch recovery — second likely failure branch silently
   absent from recovery path, verdict CONCERN; (c) untraceable target — review
   point target not in A and source not cited in Reason field, verdict CONCERN.
   All three are CONCERN rather than REPROCESS because the pair is usable as-is
   for the training scenario, with additions recommended.

3. **Expected Orchestration Trace** — three scenarios. Most critical: Node 3
   output for the examination — the scheme plan for the examination task must
   correctly identify the provenance probe as requiring cross-pair source
   checking, not just an internal check of CB3-03-001 A.

4. **Human Result Standards** — required: each finding names the step and
   specific element (Step 1 confirmation condition; recovery path second branch;
   review point target); each finding names the specific addition that would
   resolve it; the overall disposition states the pair is usable with three
   additions.

5. **Executor Contract** — files, tests, evidence, gates, zip.

---

### Step 5 — Training Gap Map

| Node | Approach | Reason |
|---|---|---|
| 00–01 | Human proxy | Mechanical |
| 02 | New pair needed | Cascade-unblock scheme plan examination vocabulary (prerequisite check specificity, target provenance) not in prior training |
| 03 | New pair needed | Examination plan with a cross-pair source-check step (Scenario 3 requires checking CB2-04-001 as a reference) is a new sequencing pattern |
| 04–06 | Human proxy | First pass |
| 07 | New pair needed | Fire case for cascade-unblock scheme plan examination domain new |
| 08 | New pair needed | CONCERN verdict for underspecified check and single-branch recovery new in this context |
| 09 | Human proxy | Existing format sufficient |

---

### Step 6 — Pass Gates and Failure Classification

| Gate | Condition | Failure means |
|---|---|---|
| GATE-5A | COUNT("passing form" OR "failing form" OR "models.ForeignKey" in arbiter output for Scenario 1) ≥ 1 | Underspecified check not detected. Fix: new Node 2 pair for prerequisite-check-specificity vocabulary |
| GATE-5B | COUNT("production" OR "staging" OR "second branch" OR "cached" in arbiter output for Scenario 2) ≥ 1 | Missing recovery branch not detected. Fix: new Node 3 pair with single-branch recovery coverage probe |
| GATE-5C | COUNT("CB2-04-001" OR "provenance" OR "not in A" OR "0.5s" in arbiter output for Scenario 3) ≥ 1 | Target provenance not detected. Fix: taxonomy fix — add "untraceable target" as named failure type |
| GATE-5D | COUNT(REPROCESS verdicts) = 0 | Incorrect severity. Fix: Binding Failure Taxonomy content fix |
| GATE-5E | Human reviewer confirms ≥ 3 findings each naming the specific element and the specific addition | Findings not propagated to human result. Fix: Node 9 pair for cascade-unblock examination audience |

---

---

## Summary — Five Experiments Ready for Round A

| # | Source document | Central failure modes | Scenarios | Approval status |
|---|---|---|---|---|
| 1 | clean-batch-001 node-03 scheme planner | Parallel contradiction, applier crossover, hollow prerequisite | 3 | Awaiting approval |
| 2 | clean-batch-002 node-02 domain translator | SQL-shape crossover, solution-name embedding, synthesized target | 3 | Awaiting approval |
| 3 | clean-batch-002 node-04 domain processor | Silent failure signal, non-self-contained wait state, indistinct root causes | 3 | Awaiting approval |
| 4 | clean-batch-002 node-07 arbiter selector | Implicit no-arbiter domain, scope completeness (PASS), inline review references | 3 | Awaiting approval |
| 5 | clean-batch-003 node-03 cascade plan | Underspecified prerequisite, single-branch recovery, untraceable target | 3 | Awaiting approval |

Each experiment is ready to proceed to Round A (five oracle documents) upon
approval. The oracle documents for each experiment are listed in each Step 4
section. They must be produced in the specified order because each depends on
the one before.

No experiment should proceed to Round A without the user confirming the central
question and the three scenarios are correctly scoped. The oracle documents
are the most valuable output of the next phase — they force the gaps in each
source document to become visible before any execution begins.
