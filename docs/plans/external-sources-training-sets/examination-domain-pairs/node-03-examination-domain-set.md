# Node 03 — Scheme Planner — Examination Domain Pairs
# Produced by gap closure pass across five experiment translations
# Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
# Version: 1.0 — 2026-05-04

Node 3 receives the complete set of formal examination statements from Node 2
and produces an ordered examination plan with actual steps, actual dependencies,
actual wait states, and actual transformational prompts.
A B that says "examine the plan" is wrong — the actual examination steps
with actual sequencing logic must appear.

---

## Experiment 1 — scheme-planner-examination plan

Pair EX1-03-001 — Scheme planner examination plan
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: [scheme-planner-examination] Three formal examination findings for the CB1-03 scheme planner pairs:
[S1] CB1-03-001 Step 3 — parallel independence claim contradicted by content cross-reference. Prompt body references "Step 4's output" while Reason field asserts parallel independence. Coverage status: concern.
[S2] CB1-03-002 Phase 3 — exact SQL and batch parameters in scheme plan transformational prompt body. Implementation syntax belongs in Node 5, not Node 3. Coverage status: concern.
[S3] CB1-03-004 Step 1 — hollow prerequisite: A input resolves Step 1's question before Step 1 executes. Coverage status: concern.
B:
Step 1 — Examine enforcement claims [prerequisite; no dependencies; results inform Step 3 interpretation]
Transformational prompt: Review each pair for claims that state a mechanism to catch a violation. For CB1-03-001: the Reason field asserts Steps 3 and 4 are parallel and independent. Check whether the Step 3 transformational prompt body is consistent with that claim. Identify the cross-reference "positioned after the memory_limiter (which will be added in Step 4)." Record S1: parallel independence claim contradicted by content cross-reference. Coverage status: concern.
Wait state: Record S1 with its exact text location before proceeding. The finding type (enforcement) informs whether S3 (coverage) is a separate issue or related.

Step 2 — Examine binding claims [depends on Step 1; parallel with Step 3]
Transformational prompt: Review each B for content that belongs to an adjacent node's domain. Scheme planner transformational prompts name what each phase accomplishes and the orchestration strategy — they do not contain exact SQL, exact code, or exact parameter values. For CB1-03-002 Phase 3: "UPDATE Users SET EmailAddress = Email WHERE EmailAddress IS NULL in batches of 10,000 rows with a 100ms pause between batches" is implementation-level SQL and applier-level parameters. Record S2: exact SQL and batch parameters in scheme plan body. Location: CB1-03-002 Phase 3 transformational prompt. Coverage status: concern.

Step 3 — Examine coverage claims [depends on Step 1; parallel with Step 2]
Transformational prompt: For each wait state, compare its blocking condition against the A input for the same pair. If A resolves the wait state's blocking question before the step executes, the wait state is hollow. For CB1-03-004 Step 1: the transformational prompt asks "determine whether the cache must be consistent across all three instances." The A input for CB1-03-004 states "shared cache required because cache invalidation must propagate immediately" — A has already resolved Step 1's question. Record S3: hollow prerequisite — A resolves Step 1's blocking condition. Location: CB1-03-004 Step 1 transformational prompt. Coverage status: concern. Confirm S3 is independent of S1 before recording — they are on separate pairs and are independent findings.

Step 4 — Produce examination output [depends on Steps 2 and 3]
Transformational prompt: For each finding (S1, S2, S3), produce a formal examination record naming the pair ID, the specific section within the pair, the finding type (enforcement / binding / coverage), and the specific required change. All three are concern-level — no source pair requires reprocessing. S1 required change: rephrase Step 3 prompt body to describe final pipeline state without forward-referencing Step 4. S2 required change: replace exact SQL and parameters with batching strategy description. S3 required change: reformulate Step 1 to check something not already resolved by A — specifically, confirm that the update handler has an injection point for CacheManager.

Review point: Confirm all three findings cover three different finding types (S1: enforcement, S2: binding, S3: coverage). If two findings share a type, the examination coverage is insufficient for the domain.
Reason: Enforcement claims are examined first because an enforcement gap may partially explain a coverage gap — knowing whether the enforcement mechanism is intact informs how to interpret the coverage finding. Binding and coverage examinations run in parallel because they target independent content sections of independent pairs.
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX1-03-001 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: [scheme-planner-examination]
[S1] CB1-03-001 — Reason field: Steps 3 and 4 are parallel; Step 3 prompt body: "after the memory_limiter (which will be added in Step 4)." Contradiction. Concern.
[S2] CB1-03-002 Phase 3 prompt body — "UPDATE Users SET EmailAddress = Email WHERE EmailAddress IS NULL in batches of 10,000 rows." Applier content in scheme plan. Concern.
[S3] CB1-03-004 Step 1 — asks "determine if shared invalidation is needed" while A states "shared cache required." Hollow prerequisite. Concern.
B:
Step 1 — Enforcement examination [prerequisite]
CB1-03-001: Reason field asserts Steps 3 and 4 are parallel. Step 3 prompt body says "after the memory_limiter (which will be added in Step 4)." A prompt that references a parallel step's output creates an apparent ordering dependency. Record S1: enforcement concern.
Wait state: Record S1 before Step 3 — enforcement finding type informs coverage interpretation.

Step 2 — Binding examination [parallel with Step 3]
CB1-03-002 Phase 3: exact UPDATE SQL and specific batch parameters (10,000 rows, 100ms). Scheme plan content should name the batching intent (avoid locks, pause for replication recovery) — not the exact SQL. Record S2: binding concern (applier crossover).

Step 3 — Coverage examination [parallel with Step 2]
CB1-03-004 Step 1: "determine whether cache must be consistent." A input: "shared cache required because cache invalidation must propagate immediately." A resolves Step 1 before Step 1 runs. Record S3: coverage concern (hollow prerequisite).

Step 4 — Examination output [depends on Steps 2 and 3]
Record S1 (CB1-03-001 Step 3 prompt body), S2 (CB1-03-002 Phase 3 prompt body), S3 (CB1-03-004 Step 1 prompt). All concern-level. Required changes: rephrase S1 prompt; replace S2 SQL with strategy description; reformulate S3 to check injection point availability.

---

## Experiment 2 — domain-translator-examination plan

Pair EX2-03-001 — Domain translator examination plan
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: [domain-translator-examination]
[S1] CB2-02-002 B — required outcome contains SQL SELECT clause with specific field lists. Implementation syntax in formal domain statement. Concern.
[S2] CB2-02-003 B — required outcome names "Backend For Frontend (BFF)" — a solution pattern name rather than a structural property description. Concern.
[S3] CB2-02-004 B — required outcome target "30-60 seconds" not present in A fragment ("2-5 minutes" is the current state; no target specified in A). Concern.
B:
Step 1 — Examine translation boundary enforcement [prerequisite; determines which subsequent claims to examine most closely]
Transformational prompt: For each B, check whether the required outcome section contains implementation syntax — SQL keywords, API call names, specific library method names, or exact parameter values. CB2-02-002 required outcome: "generating SELECT blog_post.*, auth_user.* FROM blog_post INNER JOIN auth_user ON blog_post.author_id = auth_user.id LIMIT 100." This is implementation syntax (SELECT clause with specific column and table names). A formal domain description of the required outcome names the structural relationship and cardinality only. Record S1: enforcement concern (translation boundary violated by implementation syntax in required outcome).
Wait state: Record S1 type and location before proceeding — translation boundary violations may partially explain why other findings in the same pair also cross the boundary.

Step 2 — Examine binding claims [depends on Step 1; parallel with Step 3]
Transformational prompt: For each B, check whether the required outcome names a solution pattern by name rather than a structural property. CB2-02-003 required outcome: "Backend For Frontend (BFF). One thin API layer per client type." "BFF" is a named architectural solution. Node 2 produces structural descriptions of the required state — "three independent deployment units, each with its own field projection contract" — not solution names. Solution names belong in the scheme plan (Node 3). Record S2: binding concern (solution-name embedding in formal domain statement).

Step 3 — Examine honesty claims [depends on Step 1; parallel with Step 2]
Transformational prompt: For each B containing a specific numeric target in the required outcome or Reason field, trace that target to the A fragment. CB2-02-004 B contains "30-60 seconds"; A contains "2-5 minutes to provision" (current state only, no target). The 30-60 second target is domain knowledge about Karpenter's provisioning characteristics — it was not derived from the A fragment. A translator that adds targets from domain knowledge should name the knowledge source in the Reason field. Record S3: honesty concern (synthesized target not traceable to A; source not named in Reason field).

Step 4 — Produce examination output [depends on Steps 2 and 3]
For S1: concern — required outcome should state join relationship and cardinality without SQL syntax. For S2: concern — required outcome should describe topology properties without naming the BFF pattern. For S3: concern — Reason field should state that the 30-60 second target is derived from Karpenter domain knowledge, not from A.
Reason: Enforcement (translation boundary) examined first because it catches the most common failure mode in domain translator pairs. Binding and honesty run in parallel — they target independent content sections and independent pairs.
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX2-03-001 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: [domain-translator-examination]
[S1] CB2-02-002 — SELECT clause in required outcome. Concern.
[S2] CB2-02-003 — "Backend For Frontend (BFF)" in required outcome. Concern.
[S3] CB2-02-004 — "30-60 seconds" target absent from A. Concern.
B:
Step 1 — Translation boundary enforcement [prerequisite]
CB2-02-002 required outcome: SQL syntax present. Translation stops at structural level. Record S1: enforcement concern.

Step 2 — Binding examination [parallel with Step 3]
CB2-02-003 required outcome: "BFF" names a solution. Node 2 should name the structural property (independent deployment units). Record S2: binding concern.

Step 3 — Honesty examination [parallel with Step 2]
CB2-02-004: "30-60 seconds" not in A ("2-5 minutes" is the current state; no target in A). Target is Karpenter domain knowledge. Reason field does not name source. Record S3: honesty concern.

Step 4 — Output: S1 (SQL syntax in required outcome), S2 (solution name in required outcome), S3 (target provenance not acknowledged).

---

## Experiment 3 — domain-processor-examination plan

Pair EX3-03-001 — Domain processor examination plan
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: [domain-processor-examination]
[S1] CB2-04-002 Failure state 2 — load balancer traceparent parse failure: condition named, consequence named, observable signal absent. Concern.
[S2] CB2-04-002 Wait state — "until the synthetic trace test confirms routing affinity" without naming the specific test. Concern.
[S3] CB2-04-003 Failure states 1 and 3 — same root cause (NOT NULL before all rows have values); different phases and observable signals; Reason field does not explain why two entries exist. Concern.
B:
Step 1 — Examine observable signal coverage [prerequisite; sequential with Steps 2 and 3 for the same pair]
Transformational prompt: For each failure state, check whether an observable signal is named — a specific check or metric an executor can use to confirm the failure state has or has not occurred. For CB2-04-002 Failure state 2: condition = "load balancer cannot parse the traceparent header"; consequence = "cannot perform consistent routing." Neither the failure state text nor the surrounding context names an observable signal at the load balancer layer. The failure is silent at the load balancer (no error log) and only visible downstream as Collector metrics. Record S1: coverage concern (silent failure state — no observable signal named). The wait state and failure states in this pair are interdependent — examine the wait state next.
Wait state: Record S1 type and location before examining S2 — if the failure state has no observable signal, the wait state's resolution check must be even more specific.

Step 2 — Examine wait state self-containedness [depends on Step 1; parallel with Step 3 if on different pairs]
Transformational prompt: For each wait state, check whether the resolution check is stated within the wait state itself — not only in the success state. CB2-04-002 wait state: "until the synthetic trace test confirms routing affinity on a live trace." The specific test (inject 5-span trace, confirm all 5 spans on one instance, confirm no incomplete-trace counter increment) is stated only in the success state. The wait state must be self-contained so an executor can determine when to proceed without re-reading the success state. Record S2: coverage concern (non-self-contained wait state — resolution check referenced but not named).

Step 3 — Examine failure state distinctness [parallel with Step 2; on CB2-04-003, a different pair]
Transformational prompt: For each pair of failure states that appear related, check whether they share a root cause and whether the Reason field explains why they are separate entries. CB2-04-003 Failure state 1 (NOT NULL at column creation → table lock) and Failure state 3 (NOT NULL before backfill → constraint violation): same root cause (NOT NULL before all rows have values) at different phases. Check observable signals: Failure 1 produces a table lock during the initial migration; Failure 3 produces a constraint violation during a subsequent migration. Observable signals differ — the entries are justified as distinct. Check Reason field: Reason field does not explain why two entries exist rather than one. Record S3: concern (shared root cause without justification in Reason field for keeping entries separate).

Step 4 — Produce examination output [depends on Steps 2 and 3]
S1: concern — failure state must name observable signal or explicitly acknowledge silence and name downstream signal. S2: concern — wait state must include the specific resolution check inline. S3: concern — Reason field must state why Failure 1 and Failure 3 are separate entries (different phases, different observable signals, different remediations).
Reason: Observable signal coverage examined first because it is the most likely failure mode in domain processor pairs and because it affects the interpretation of the wait state (a failure state without an observable signal increases the importance of a self-contained wait state resolution check).
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX3-03-001 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: [domain-processor-examination]
[S1] CB2-04-002 Failure 2 — traceparent parse failure is silent at load balancer; no observable signal in failure state. Concern.
[S2] CB2-04-002 Wait state — "synthetic trace test confirms" without specifying the test. Concern.
[S3] CB2-04-003 Failures 1 and 3 — same root cause at different phases; Reason field silent on why two entries. Concern.
B:
Step 1 — Signal coverage [prerequisite]
CB2-04-002 Failure 2: condition and consequence named; direct observable signal absent. Failure is silent at origin (load balancer) and visible only downstream (Collector metrics). Record S1 concern.
Wait state: Record S1 before Step 2 — silent failure signal affects priority of wait state specificity.

Step 2 — Wait state self-containedness [depends on Step 1]
CB2-04-002 wait state: "until synthetic trace test confirms routing affinity." Specific test (5-span trace, one instance, no incomplete-trace metric) is only in success state. Wait state alone cannot guide the executor. Record S2 concern.

Step 3 — Failure state distinctness [parallel with Step 2]
CB2-04-003 Failures 1 and 3: both occur because NOT NULL constraint is present before all rows have values. Observable signals differ (table lock vs constraint violation). Phases differ. Remediations differ. Entries are justified distinct — but Reason field does not state this. Record S3 concern.

Step 4 — Output: S1 (add observable signal), S2 (add resolution check to wait state), S3 (add justification to Reason field for separate entries).

---

## Experiment 4 — arbiter-selector-examination plan

Pair EX4-03-001 — Arbiter selector examination plan
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: [arbiter-selector-examination]
[S1] CB2-07-001 — B-backend-response-latency is active (five active labels) but has no available review unit; [R4] belongs to this label; selector B output contains no entry for this label or [R4]. Concern.
[S2] CB2-07-001 — fire scope completeness: database-query-efficiency-arbiter fires on [R1]+[R2]; orm-usage-pattern-arbiter fires on [R1]. Scope appears correct and complete. Pass.
[S3] CB2-07-002 — connection-management-arbiter NO FIRE inline review gives correct conclusion without referencing [R1]–[R4] by name. Concern.
B:
Step 1 — Examine no-arbiter domain coverage [prerequisite; must be resolved before Step 4 interpretation]
Transformational prompt: Enumerate active domain labels in the flow context. Enumerate domain labels addressed in the selector output (fire verdicts, explicit no-fire entries, or explicit no-review statements). For CB2-07-001: five active labels (D-database-query-efficiency, E-orm-usage-pattern, B-backend-response-latency, and the two implicit labels covered by security-arbiter and connection-management-arbiter); four addressed in the selector output. B-backend-response-latency and its result [R4] are addressed only in the Reason field — not in the B output. Record S1: concern — active domain label and its result must be explicitly addressed in the selector B output, not only in the Reason field.
Wait state: Record S1 before Step 2. S1 is about coverage; S2 is about correctness within a covered scope. Both S1 and S2 apply to the same pair — check them in this order.

Step 2 — Examine fire scope completeness [depends on Step 1; examines the same pair]
Transformational prompt: For each fire verdict in CB2-07-001, verify that the scope names the specific result items the arbiter will examine and that the scoped results are the correct ones for that arbiter domain. database-query-efficiency-arbiter fires on [R1] and [R2] together — [R1] is the queryset change; [R2] is the query count confirmation. These two results are the efficiency domain results. [R3] (regression test) and [R4] (render time) are not in scope — correct. orm-usage-pattern-arbiter fires on [R1] only — [R1] is the ORM change; [R2]–[R4] are not ORM pattern concerns — correct. Record S2: pass — fire scope is correct and complete for both verdicts.

Step 3 — Examine inline review completeness [separate pair from Steps 1 and 2; no dependency on Step 1 result]
Transformational prompt: For CB2-07-002 pass-through case, check whether the connection-management-arbiter NO FIRE inline review names specific result items. The inline review explains the correct connection management pattern ("one connection outside the loop, using block guarantees Dispose") but does not reference [R1] (SqlConnection opened once), [R2] (loop reuse), [R3] (using block disposal), [R4] (pool slot count) by name. An inline review that is traceable to specific results is more verifiable than one giving a general conclusion. Record S3: concern — inline review should name result items individually.

Step 4 — Produce examination output [depends on Steps 2 and 3]
S1: concern — selector B must include explicit entry for B-backend-response-latency and [R4]. S2: pass — scope is correct for both fire verdicts. S3: concern — inline review should reference [R1]–[R4] explicitly.
Reason: No-arbiter domain coverage examined first because it is a structural property of the selector output — a missing entry affects all subsequent interpretation. Fire scope completeness depends on knowing which labels are covered. Inline review completeness is examined separately because it applies to a different pair.
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX4-03-001 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: [arbiter-selector-examination]
[S1] CB2-07-001 — five active labels, four addressed in B, one (B-backend-response-latency with [R4]) addressed only in Reason field. Concern.
[S2] CB2-07-001 — fire verdicts scope: efficiency arbiter [R1]+[R2]; ORM arbiter [R1]. Correct. Pass.
[S3] CB2-07-002 — inline review: correct conclusion without [R1]–[R4] result references. Concern.
B:
Step 1 — No-arbiter label coverage [prerequisite]
CB2-07-001: B-backend-response-latency and [R4] appear only in Reason field, not in B output. Selector must produce explicit B output entry for every active domain label. Record S1 concern.
Wait state: Record S1. S2 applies to the same pair after S1.

Step 2 — Fire scope [depends on Step 1, same pair]
Both fire verdicts name their scopes. Efficiency arbiter: [R1]+[R2] — queryset change and query count. ORM arbiter: [R1] only — ORM change. Scopes correct. Record S2 pass.

Step 3 — Inline review [separate pair, no dependency]
CB2-07-002 connection-management NO FIRE: correct reasoning but no result item references. Record S3 concern.

Step 4 — Output: S1 concern (add [R4] entry to B); S2 pass; S3 concern (add result references to inline review).

---

## Experiment 5 — cascade-plan-examination plan

Pair EX5-03-001 — Cascade plan examination plan
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: [cascade-plan-examination]
[S1] CB3-03-001 Step 1 — prerequisite check: "Confirm Post.author is a ForeignKey (or equivalent)" — no file location, no explicit passing form, no explicit failing forms. Concern.
[S2] CB3-03-001 Recovery path — covers assertNumQueries vs Toolbar mismatch (branch 1); does not cover both-pass-but-production-still-101 (branch 2). Concern.
[S3] CB3-03-001 Review point — "under 0.5 seconds" target; A input does not contain this target; Reason field does not name the source. Concern.
B:
Step 1 — Examine prerequisite check specificity [prerequisite; no dependencies]
Transformational prompt: For CB3-03-001 Step 1, check whether the transformational prompt names (a) the file to check, (b) the specific passing forms with no ambiguous language, (c) the specific failing forms with their corrective action paths. Current Step 1: names the condition (ForeignKey); adds "or equivalent" without naming OneToOneField explicitly; names ManyToManyField as one failing form; does not name reverse relations; does not name the file (models.py). "Or equivalent" is ambiguous — the equivalent forms must be named. Record S1: concern (prerequisite check condition underspecified — passing form, failing forms, and file location not fully named).
Wait state: Record S1 before proceeding to recovery path examination — S1 finding type (enforcement) is independent of S2 (coverage) but both target the same pair.

Step 2 — Examine recovery path branch coverage [depends on Step 1; both apply to CB3-03-001]
Transformational prompt: Identify the likely failure branches for the scenario and check whether each branch has a recovery path. CB3-03-001: branch 1 (assertNumQueries differs from Django Debug Toolbar count) — covered: wrap view call only, exclude fixture setup. Branch 2 (Step 3 and Step 4 both report 1 query but production feed still shows 101 queries) — not covered. Branch 2 can occur when select_related is applied to a queryset not used by the production view — passing tests do not rule this out. Record S2: concern (single-branch recovery path — production-mismatch branch absent).

Step 3 — Examine target provenance [parallel with Step 2; targets review point of the same pair]
Transformational prompt: For each numeric target in the review point, trace the target to the A input. CB3-03-001 review point: "under 0.5 seconds." A input: "4.2 seconds to render" (current state only — no target stated). Search adjacent pairs: CB2-04-001 success state states "Expected render time: under 0.5 seconds, down from 4.2 seconds." The scheme plan sourced its target from the domain processor pair (CB2-04-001), not from its own A input. The Reason field does not name CB2-04-001 as the source. Record S3: concern (target provenance — review point target not in A; source not named in Reason field).

Step 4 — Produce examination output [depends on Steps 2 and 3]
S1: concern — prerequisite check must name file (models.py), passing forms (ForeignKey or OneToOneField with any on_delete), failing forms (ManyToManyField → use prefetch_related; reverse relation via related_name → use prefetch_related). S2: concern — recovery path must add branch 2 (production-mismatch after both tests pass). S3: concern — Reason field must state that the 0.5s target is sourced from CB2-04-001, not from A.
Reason: Prerequisite check specificity is examined first because it affects how the executor runs all subsequent steps — an underspecified check propagates ambiguity through the plan. Recovery path and target provenance are examined in parallel because they target independent sections of the same pair.
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX5-03-001 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: [cascade-plan-examination]
[S1] CB3-03-001 Step 1: "or equivalent" without naming equivalents; no file; ManyToManyField named but reverse relation not. Concern.
[S2] CB3-03-001 Recovery path: Toolbar/assertNumQueries mismatch covered; production mismatch not. Concern.
[S3] CB3-03-001 Review point "under 0.5 seconds": absent from A; CB2-04-001 is the source; Reason field does not name it. Concern.
B:
Step 1 — Prerequisite specificity [prerequisite]
Step 1 prompt: "ForeignKey or equivalent" — equivalents unnamed. No file. Failing form "ManyToManyField" named; reverse relation not. Record S1: prerequisite check underspecified.
Wait state: S1 and S2 both target CB3-03-001. Record S1 before moving to recovery path.

Step 2 — Recovery path branches [depends on Step 1]
Branch 1 (assertNumQueries ≠ Toolbar): covered. Branch 2 (both pass, production still at 101): not covered. select_related applied to a test-only queryset can pass all tests without fixing the production view. Record S2 concern.

Step 3 — Target provenance [parallel with Step 2]
"Under 0.5 seconds" in review point. A input: "4.2 seconds" (current state, no target). CB2-04-001 success state: "under 0.5 seconds." Target sourced from adjacent pair. Reason field does not name CB2-04-001. Record S3 concern.

Step 4 — Output: S1 (add location, passing forms, failing forms); S2 (add production-mismatch branch); S3 (name CB2-04-001 as target source in Reason field).
