# Node 07 — Arbiter Selector — Examination Domain Pairs
# Produced by gap closure pass across five experiment examination domains
# Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
# Version: 1.0 — 2026-05-04

Node 7 receives the full collection of formal examination results from Node 5 plus the flow context.
It produces the exact names of examination review units that must fire, and an explicit confirmation for units that do not fire.
These pairs teach arbiter selection behavior in the five examination domains introduced by the experiment translation work.

A is the full examination result collection plus the flow context as they arrive at runtime.
B is the exact selector output: named examination arbiters with their scope, plus explicit no-fire statements for units that do not apply.
A B that is silent on any active examination domain label is wrong — every label must be explicitly addressed.

The pass-through case must appear in this file. A selector trained only on fire cases dispatches arbiters for every result.

---

## Experiment 1 Domain — scheme-planner-examination

Pair EX1-07-001 — Scheme planner: arbiter fires on parallel cross-reference, applier crossover, and hollow prerequisite
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Examination findings for scheme plan review — CB1 batch:
[R1] Scheme plan CB1-03-001: Step 3 transformational prompt body contains "positioned after the memory_limiter (which will be added in Step 4)." Step header declares Step 3 parallel with Step 4. Reason field asserts Steps 3 and 4 are independent and applied together at deployment time.
[R2] Scheme plan CB1-03-002: Phase 3 transformational prompt contains the exact SQL statement "UPDATE Users SET EmailAddress = Email WHERE EmailAddress IS NULL" with an exact batch size (10,000 rows) and an exact pause duration (100ms between batches).
[R3] Scheme plan CB1-03-004: Step 1 wait state reads "Confirm Redis is reachable from the NestJS application." The A input for this pair states that Redis is deployed and responding to PING from all three instances as a starting condition.

Flow context: Examination domain label active: scheme-planner-examination. Available examination review units: scheme-planner-arbiter, domain-translator-arbiter, domain-processor-arbiter, arbiter-selector-arbiter, cascade-plan-arbiter.
B: scheme-planner-arbiter: FIRE on [R1], [R2], and [R3] together. Review scope:
— [R1] Parallel cross-reference: confirm the finding that a parallel independence claim and a cross-step reference coexist in CB1-03-001 Step 3. The Reason field declares Steps 3 and 4 independent; the transformational prompt body references Step 4's output as a positional constraint. These two statements contradict each other.
— [R2] Applier crossover: confirm the finding that CB1-03-002 Phase 3's transformational prompt contains exact SQL syntax with exact numeric parameters. Scheme plan transformational prompts describe mechanism and constraint at the structural level; exact SQL with exact batch sizes and pause durations belong in the applier's output.
— [R3] Hollow prerequisite: confirm the finding that CB1-03-004 Step 1's wait state restates a condition already satisfied by the A input. A blocking wait state whose condition is already resolved by the input does not function as a gate.

domain-translator-arbiter: NO FIRE. [R1] through [R3] contain no formal domain statements from Node 2 — these are scheme plan outputs, not domain translation outputs. No domain-translator concern is applicable.

domain-processor-arbiter: NO FIRE. [R1] through [R3] contain no domain processor outputs — no success/failure/wait-state enumeration is under review here. No domain-processor concern is applicable.

arbiter-selector-arbiter: NO FIRE. [R1] through [R3] contain no arbiter selector outputs. No arbiter-selector concern is applicable.

cascade-plan-arbiter: NO FIRE. These findings are from a scheme plan, not a cascade plan. No cascade-plan concern is applicable.
Reason: The selector fires on the active examination domain label. scheme-planner-examination is active; scheme-planner-arbiter fires. The other four arbiters are available but do not fire because no active examination domain label in the flow context corresponds to their review scope.
Coverage status: concern
Handoff marker: send_to_scheme_planner_arbiter
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX1-07-001 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Scheme plan examination results — CB1 batch:
[R1] CB1-03-001 Step 3: transformational prompt body forward-references Step 4's output ("which will be added in Step 4") while the step header and Reason field declare Steps 3 and 4 parallel and independent.
[R2] CB1-03-002 Phase 3: transformational prompt specifies exact SQL (UPDATE ... WHERE EmailAddress IS NULL), exact batch size (10,000 rows), and exact pause (100ms) between batches.
[R3] CB1-03-004 Step 1: wait state requires confirming Redis reachability — a condition the A input already establishes as satisfied.

Flow context: Active examination domain: scheme-planner-examination. Available examination arbiters: scheme-planner-arbiter, domain-translator-arbiter, domain-processor-arbiter, arbiter-selector-arbiter, cascade-plan-arbiter.
B: scheme-planner-arbiter: FIRE — review [R1], [R2], and [R3].
— [R1]: parallel independence claim contradicted by cross-step reference in the same transformational prompt body.
— [R2]: exact SQL syntax with numeric parameters present in scheme plan transformational prompt — applier-layer content in scheme-planner output.
— [R3]: wait state restates a condition already resolved by the A input — hollow prerequisite.

domain-translator-arbiter: NO FIRE — no Node 2 formal domain statement content present in [R1]–[R3].

domain-processor-arbiter: NO FIRE — no success/failure/wait-state enumeration present in [R1]–[R3].

arbiter-selector-arbiter: NO FIRE — no selector output present in [R1]–[R3].

cascade-plan-arbiter: NO FIRE — scheme plan outputs, not cascade plan outputs.

---

Pair EX1-07-002 — Scheme planner: no arbiter fires on fully compliant examination findings
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Examination findings for scheme plan review — CB1 corrected batch:
[R1] Scheme plan CB1-03-001 Step 3 corrected: transformational prompt body reads "In the final configured pipeline, the tailsampling processor appears after the memory_limiter — Step 4 configures the memory_limiter concurrently, and both processors are applied together at deployment time. Configure the tailsampling processor independently of Step 4; do not wait for Step 4 before beginning this step." No forward reference to Step 4's output. Parallel independence claim and transformational prompt body are consistent.
[R2] Scheme plan CB1-03-002 Phase 3 corrected: transformational prompt reads "Batch-update existing rows to copy Email values into EmailAddress for rows where EmailAddress is still NULL. Keep batches small enough to avoid acquiring a table lock — a batch that is too large will block concurrent reads and writes during the backfill window. Pause between batches to allow replication lag to recover." No exact SQL syntax. No exact numeric parameters. Structural constraint named; implementation detail deferred to the applier.
[R3] Scheme plan CB1-03-004 Step 1 corrected: wait state reads "Confirm that the admin update handler has an injection point where cacheManager.del('product-catalog') can be called through the NestJS DI (dependency injection) container." The A input does not establish this condition as satisfied — the injection point must be verified before proceeding.

Flow context: Examination domain label active: scheme-planner-examination. Available examination review units: scheme-planner-arbiter, domain-translator-arbiter, domain-processor-arbiter, arbiter-selector-arbiter, cascade-plan-arbiter.
B: scheme-planner-arbiter: NO FIRE — results reviewed inline.
[R1] Parallel consistency check: the transformational prompt body describes the final pipeline state and explicitly states both steps are applied concurrently at deployment time. No forward reference to Step 4's output. Parallel independence claim and prompt body are consistent. Pass.
[R2] Applier crossover check: the transformational prompt names the mechanism (batch update with size constraint), the constraint (small enough to avoid table lock), and the coordination requirement (pause for replication lag). No exact SQL, no exact numeric parameters. Scheme-planner level content. Pass.
[R3] Hollow prerequisite check: the wait state requires confirming an injection point that is not established as satisfied by the A input. The blocking condition is not pre-resolved. Pass.

domain-translator-arbiter: NO FIRE — no Node 2 formal domain statement content under review.

domain-processor-arbiter: NO FIRE — no success/failure/wait-state enumeration under review.

arbiter-selector-arbiter: NO FIRE — no selector output under review.

cascade-plan-arbiter: NO FIRE — scheme plan outputs, not cascade plan outputs.

Results are approved for formatting.
Reason: The selector fires on the active examination domain label. scheme-planner-examination is active. The scheme-planner-arbiter reviews [R1]–[R3] inline and finds no concern — parallel consistency holds, no applier-layer content is present, and the wait state's blocking condition is not pre-resolved by the A input. No arbiter fires.
Coverage status: pass
Handoff marker: send_to_formatter
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX1-07-002 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Scheme plan examination — corrected CB1 batch findings:
[R1] CB1-03-001 Step 3: final-state description used ("the tailsampling processor appears after the memory_limiter ... Step 4 configures the memory_limiter concurrently"). No forward reference to Step 4 output. Parallel claim and prompt body consistent.
[R2] CB1-03-002 Phase 3: structural description only — batch size constrained by lock avoidance, no exact SQL, no numeric pause duration.
[R3] CB1-03-004 Step 1: wait state requires verifying a DI injection point not assumed by the A input.

Flow context: Active examination domain: scheme-planner-examination. Available examination arbiters: scheme-planner-arbiter, domain-translator-arbiter, domain-processor-arbiter, arbiter-selector-arbiter, cascade-plan-arbiter.
B: scheme-planner-arbiter: NO FIRE — inline review: [R1] no parallel cross-reference, [R2] no applier-layer SQL content, [R3] blocking condition not pre-resolved. All three checks pass.

domain-translator-arbiter: NO FIRE — no Node 2 content present.

domain-processor-arbiter: NO FIRE — no Node 4 content present.

arbiter-selector-arbiter: NO FIRE — no Node 7 content present.

cascade-plan-arbiter: NO FIRE — scheme plan content, not cascade plan.

Results are approved for formatting.

---

## Experiment 2 Domain — domain-translator-examination

Pair EX2-07-001 — Domain translator: arbiter fires on SQL syntax, solution name, and untraced target
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Examination findings for domain translation review — CB2-02 batch:
[R1] Domain translation CB2-02-002 B: required outcome field contains "Target state: 1 database query, generating SELECT blog_post.*, auth_user.* FROM blog_post INNER JOIN auth_user ON blog_post.author_id = auth_user.id LIMIT 100." SQL SELECT clause with specific table names, field lists, and JOIN syntax appears in the required outcome of a formal domain statement.
[R2] Domain translation CB2-02-003 B: required topology reads "Backend For Frontend (BFF). One thin API layer per client type (web, mobile, IoT)." The term BFF names a specific solution pattern. A formal domain description names the structural requirement — the topology — not the named solution pattern that satisfies it.
[R3] Domain translation CB2-02-004 B: required timeline states "provisioning must complete within 30–60 seconds." The A fragment for this pair names no numeric provisioning duration. The 30–60 second figure appears in the formal domain statement without traceability to the A input.

Flow context: Examination domain label active: domain-translator-examination. Available examination review units: scheme-planner-arbiter, domain-translator-arbiter, domain-processor-arbiter, arbiter-selector-arbiter, cascade-plan-arbiter.
B: domain-translator-arbiter: FIRE on [R1], [R2], and [R3] together. Review scope:
— [R1] Translation boundary: confirm the finding that SQL SELECT syntax appears in the required outcome of a formal domain statement from Node 2. A formal domain statement describes the structural outcome — join relationship type, cardinality, round-trip count — not the SQL clause that implements it.
— [R2] Solution name in formal description: confirm the finding that "BFF" names a solution pattern rather than a structural topology. The formal domain description must describe the topology (independent layers per client type with separate deployment and projection contracts) without naming the architectural pattern that satisfies it.
— [R3] Target traceability: confirm the finding that the 30–60 second provisioning target appears in the formal domain statement without a source in the A fragment. A synthesized target is acceptable only if the domain statement explicitly names it as domain knowledge derived from the technology's documented characteristics, not as a translation of the A input.

scheme-planner-arbiter: NO FIRE. [R1] through [R3] contain no scheme plan outputs. No scheme-planner concern is applicable.

domain-processor-arbiter: NO FIRE. [R1] through [R3] contain no domain processor outputs. No domain-processor concern is applicable.

arbiter-selector-arbiter: NO FIRE. [R1] through [R3] contain no arbiter selector outputs.

cascade-plan-arbiter: NO FIRE. [R1] through [R3] contain no cascade plan outputs.
Reason: The selector fires on the active examination domain label. domain-translator-examination is active; domain-translator-arbiter fires. The other four arbiters do not fire because no active domain label in the flow context corresponds to their scope.
Coverage status: concern
Handoff marker: send_to_domain_translator_arbiter
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX2-07-001 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Domain translation examination results — CB2-02:
[R1] CB2-02-002 B required outcome: contains SQL SELECT clause with specific table names, JOIN syntax, and field lists.
[R2] CB2-02-003 B: uses "Backend For Frontend (BFF)" — a named solution pattern — in a formal structural description.
[R3] CB2-02-004 B: states 30–60 second provisioning target with no A-input source for this figure.

Flow context: Active examination domain: domain-translator-examination. Available examination arbiters: scheme-planner-arbiter, domain-translator-arbiter, domain-processor-arbiter, arbiter-selector-arbiter, cascade-plan-arbiter.
B: domain-translator-arbiter: FIRE — review [R1], [R2], and [R3].
— [R1]: SQL syntax in required outcome — implementation vocabulary in formal domain statement.
— [R2]: solution pattern name ("BFF") in structural description — formal domain statement must describe topology without naming the solution.
— [R3]: numeric target without A-input source — synthesized target must be explicitly labeled as domain knowledge, not presented as a translation of the input.

scheme-planner-arbiter: NO FIRE — no scheme plan content in [R1]–[R3].

domain-processor-arbiter: NO FIRE — no Node 4 content in [R1]–[R3].

arbiter-selector-arbiter: NO FIRE — no Node 7 content in [R1]–[R3].

cascade-plan-arbiter: NO FIRE — no cascade plan content in [R1]–[R3].

---

Pair EX2-07-002 — Domain translator: no arbiter fires on fully compliant domain translation findings
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Examination findings for domain translation review — CB2-02 corrected batch:
[R1] Domain translation CB2-02-002 B corrected: required outcome reads "a JOIN between the blog_post table and the auth_user table on the author foreign key, returning all requested fields in one round trip, limited to 100 rows." No SQL SELECT syntax. Structural description of the join relationship, cardinality, and round-trip count.
[R2] Domain translation CB2-02-003 B corrected: required topology reads "three independent API layers, one per client type (web, mobile, IoT), each with its own deployment lifecycle and field projection contract covering exactly the fields its client requires." No solution pattern name. Topology described in structural terms.
[R3] Domain translation CB2-02-004 B corrected: required timeline reads "provisioning must complete within 30–60 seconds. Note: this target is derived from Karpenter's documented provisioning characteristics (direct EC2 Fleet API call latency) — it is domain knowledge added by Node 2, not a translation of the A fragment." Synthesized target explicitly labeled with its source.

Flow context: Examination domain label active: domain-translator-examination. Available examination review units: scheme-planner-arbiter, domain-translator-arbiter, domain-processor-arbiter, arbiter-selector-arbiter, cascade-plan-arbiter.
B: domain-translator-arbiter: NO FIRE — results reviewed inline.
[R1] Translation boundary check: required outcome uses structural vocabulary — join type, table relationship, cardinality, round-trip count. No SQL clause syntax. Pass.
[R2] Solution name check: topology described as independent layers with deployment and projection properties. No architectural pattern name. Pass.
[R3] Target traceability check: synthesized target explicitly labeled as domain knowledge with named source (Karpenter's documented provisioning characteristics). Not presented as a translation of the A fragment. Pass.

scheme-planner-arbiter: NO FIRE — no scheme plan output under review.

domain-processor-arbiter: NO FIRE — no Node 4 output under review.

arbiter-selector-arbiter: NO FIRE — no selector output under review.

cascade-plan-arbiter: NO FIRE — no cascade plan output under review.

Results are approved for formatting.
Reason: domain-translator-examination is active. The domain-translator-arbiter reviews [R1]–[R3] inline: translation boundary holds (no SQL syntax in required outcome), topology description avoids solution pattern names, and the synthesized target is explicitly labeled as domain knowledge rather than a translated input value. No concern. No arbiter fires.
Coverage status: pass
Handoff marker: send_to_formatter
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX2-07-002 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Domain translation examination — corrected CB2-02 findings:
[R1] CB2-02-002 required outcome: structural join description — no SQL SELECT syntax.
[R2] CB2-02-003 topology: three independent layers with deployment and projection properties — no BFF name.
[R3] CB2-02-004 target: 30–60 seconds labeled as Karpenter domain knowledge, not a translated A-fragment value.

Flow context: Active examination domain: domain-translator-examination. Available examination arbiters: scheme-planner-arbiter, domain-translator-arbiter, domain-processor-arbiter, arbiter-selector-arbiter, cascade-plan-arbiter.
B: domain-translator-arbiter: NO FIRE — inline review: [R1] no implementation syntax in required outcome, [R2] no solution pattern name, [R3] synthesized target explicitly labeled. All three checks pass.

scheme-planner-arbiter: NO FIRE — no Node 3 content present.

domain-processor-arbiter: NO FIRE — no Node 4 content present.

arbiter-selector-arbiter: NO FIRE — no Node 7 content present.

cascade-plan-arbiter: NO FIRE — no cascade plan content present.

Results are approved for formatting.

---

## Experiment 3 Domain — domain-processor-examination

Pair EX3-07-001 — Domain processor: arbiter fires on silent signal, unself-contained wait state, and undistinguished failure states
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Examination findings for domain processor review — CB2-04 batch:
[R1] Domain processor CB2-04-002 Failure state 2: the failure state names the condition (load balancer cannot parse traceparent header) and the consequence (incorrect routing) but contains no observable signal statement. The failure is silent at the load balancer; no log entry, no metric, no direct check is named at either the origin or downstream.
[R2] Domain processor CB2-04-002 Wait state: the wait state blocks Steps 3 and 4 until the routing change is confirmed on live traffic but does not name a specific resolution check. The wait state reads "must not be applied until the synthetic trace test confirms routing affinity on a live trace" without naming what the synthetic trace test consists of, how many spans, across how many services, or what success looks like.
[R3] Domain processor CB2-04-003: Failure state 1 (NOT NULL with DEFAULT at column creation causing a table lock) and Failure state 3 (NOT NULL constraint added before backfill completes causing a constraint violation) are both present. The Reason field does not distinguish their different phases, observable signals, or remediations — they are listed without explanation of why two separate entries are required.

Flow context: Examination domain label active: domain-processor-examination. Available examination review units: scheme-planner-arbiter, domain-translator-arbiter, domain-processor-arbiter, arbiter-selector-arbiter, cascade-plan-arbiter.
B: domain-processor-arbiter: FIRE on [R1], [R2], and [R3] together. Review scope:
— [R1] Signal coverage: confirm the finding that CB2-04-002 Failure state 2 contains no observable signal. A failure state with silent onset must either name a direct observable signal at the failing component or explicitly acknowledge the silence and name the downstream signal. Neither is present.
— [R2] Wait state self-containedness: confirm the finding that CB2-04-002 Wait state names the blocking event (synthetic trace test) without naming the test's parameters — span count, service count, what metric to check, and what constitutes a passing result. A wait state must be actionable inline without reference to other sections.
— [R3] Failure state distinctness: confirm the finding that CB2-04-003 Failure states 1 and 3 share the same root cause but appear without explanation of why they are separate. A Reason field covering multiple failure states with a shared root cause must explain the distinguishing factors: phase, observable signal, and remediation.

scheme-planner-arbiter: NO FIRE. [R1] through [R3] contain no scheme plan outputs.

domain-translator-arbiter: NO FIRE. [R1] through [R3] contain no Node 2 formal domain statement outputs.

arbiter-selector-arbiter: NO FIRE. [R1] through [R3] contain no arbiter selector outputs.

cascade-plan-arbiter: NO FIRE. [R1] through [R3] contain no cascade plan outputs.
Reason: domain-processor-examination is active; domain-processor-arbiter fires. The other four arbiters do not fire because no active examination domain label corresponds to their review scope.
Coverage status: concern
Handoff marker: send_to_domain_processor_arbiter
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX3-07-001 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Domain processor examination results — CB2-04:
[R1] CB2-04-002 Failure state 2: condition and consequence named; no observable signal — failure is silent at the load balancer layer.
[R2] CB2-04-002 Wait state: blocks on synthetic trace test without naming test parameters (span count, service count, success metric).
[R3] CB2-04-003: two failure states with shared root cause (NOT NULL constraint applied before all rows have values) present without explanation of distinct phases, signals, or remediations.

Flow context: Active examination domain: domain-processor-examination. Available examination arbiters: scheme-planner-arbiter, domain-translator-arbiter, domain-processor-arbiter, arbiter-selector-arbiter, cascade-plan-arbiter.
B: domain-processor-arbiter: FIRE — review [R1], [R2], [R3].
— [R1]: silent failure state — no observable signal named at the failing component or downstream.
— [R2]: wait state blocking condition not self-contained — resolution check parameters absent.
— [R3]: shared-root-cause failure states listed without phase/signal/remediation distinction in the Reason field.

scheme-planner-arbiter: NO FIRE — no scheme plan content.

domain-translator-arbiter: NO FIRE — no Node 2 content.

arbiter-selector-arbiter: NO FIRE — no Node 7 content.

cascade-plan-arbiter: NO FIRE — no cascade plan content.

---

Pair EX3-07-002 — Domain processor: no arbiter fires on fully compliant domain processor findings
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Examination findings for domain processor review — CB2-04 corrected batch:
[R1] Domain processor CB2-04-002 Failure state 2 corrected: "Observable signal: the load balancer does not produce an error log for this failure — the failure is silent at the load balancer layer and becomes visible downstream. After the routing change is deployed, Collector metrics show 'incomplete trace' or 'spans from unknown trace' counters incrementing during the synthetic trace test. If those counters increment after the routing change but the synthetic trace test was expected to pass, traceparent parsing failure is the likely cause." The failure state names the silent onset explicitly and names the downstream observable signal.
[R2] Domain processor CB2-04-002 Wait state corrected: "Steps 3 and 4 (tail sampling processor and memory_limiter configuration) must not be applied until the synthetic trace test confirms routing affinity on a live trace. Specific resolution check: inject a minimum 5-span synthetic trace across at least 3 services, then confirm all 5 spans appear in one Collector instance's span buffer and that the Collector metrics show no 'incomplete trace' counter increment during the test window. If any span lands on a different instance, or if the 'incomplete trace' counter increments, the routing change is incomplete." Wait state is self-contained with named test parameters and pass/fail criteria.
[R3] Domain processor CB2-04-003 Reason field corrected: "Note: Failure state 1 (NOT NULL with DEFAULT at column creation) and Failure state 3 (NOT NULL constraint added before backfill completes) share the same root cause — a NOT NULL constraint present before all rows have values — but are kept as separate entries because they occur at different phases, produce different observable signals (a table lock during the initial migration versus a constraint violation during a subsequent migration), and require different remediations (fix the column definition in the current migration versus defer the NOT NULL constraint to Phase 5)." Shared root cause explained; distinguishing factors named.

Flow context: Examination domain label active: domain-processor-examination. Available examination review units: scheme-planner-arbiter, domain-translator-arbiter, domain-processor-arbiter, arbiter-selector-arbiter, cascade-plan-arbiter.
B: domain-processor-arbiter: NO FIRE — results reviewed inline.
[R1] Signal coverage check: failure state explicitly acknowledges silent onset at the load balancer layer and names the downstream signal (Collector 'incomplete trace' counter). Observable signal present. Pass.
[R2] Wait state self-containedness check: resolution check names the test (5-span synthetic trace, minimum 3 services), the confirmation condition (all 5 spans on one instance, no counter increment), and the failure condition (any span on a different instance or counter incrementing). Self-contained. Pass.
[R3] Failure state distinctness check: Reason field names the distinguishing factors — phase, observable signal, and remediation — for the two shared-root-cause failure states. Distinguishable. Pass.

scheme-planner-arbiter: NO FIRE — no scheme plan output under review.

domain-translator-arbiter: NO FIRE — no Node 2 output under review.

arbiter-selector-arbiter: NO FIRE — no selector output under review.

cascade-plan-arbiter: NO FIRE — no cascade plan output under review.

Results are approved for formatting.
Reason: domain-processor-examination is active. The domain-processor-arbiter reviews [R1]–[R3] inline: observable signal named (downstream, with explicit acknowledgment of silence at origin), wait state resolution check fully specified with span count, service count, and pass/fail criteria, and shared-root-cause failure states distinguished by phase, observable signal, and remediation. No concern. No arbiter fires.
Coverage status: pass
Handoff marker: send_to_formatter
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX3-07-002 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Domain processor examination — corrected CB2-04 findings:
[R1] CB2-04-002 Failure state 2: silence at origin acknowledged; downstream signal named (Collector 'incomplete trace' counter increments after routing change is deployed).
[R2] CB2-04-002 Wait state: 5-span synthetic trace test across at least 3 services with named confirmation condition and failure condition.
[R3] CB2-04-003 Reason: two shared-root-cause failure states distinguished by phase (initial migration vs subsequent migration), observable signal (table lock vs constraint violation), and remediation (fix column definition vs defer constraint to Phase 5).

Flow context: Active examination domain: domain-processor-examination. Available examination arbiters: scheme-planner-arbiter, domain-translator-arbiter, domain-processor-arbiter, arbiter-selector-arbiter, cascade-plan-arbiter.
B: domain-processor-arbiter: NO FIRE — inline review: [R1] observable signal named (downstream, silence acknowledged), [R2] wait state self-contained with test parameters, [R3] failure states distinguished by phase/signal/remediation. All three checks pass.

scheme-planner-arbiter: NO FIRE — no Node 3 content.

domain-translator-arbiter: NO FIRE — no Node 2 content.

arbiter-selector-arbiter: NO FIRE — no Node 7 content.

cascade-plan-arbiter: NO FIRE — no cascade plan content.

Results are approved for formatting.

---

## Experiment 4 Domain — arbiter-selector-examination

Pair EX4-07-001 — Arbiter selector: arbiter fires on implicit domain disposition and result-unreferenced inline review
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Examination findings for arbiter selector review — CB2-07 batch:
[R1] Arbiter selector output CB2-07-001 B: the B-backend-response-latency domain label is active in the flow context. Result [R4] (render time: 0.38 seconds, down from 4.2 seconds) belongs to this label. The selector output names verdicts for four available review units and is silent on the B-backend-response-latency label and on [R4] — no entry explains the label's status or the result's disposition.
[R2] Arbiter selector output CB2-07-001 B fire scope: the database-query-efficiency-arbiter fires on [R1] and [R2] with review scope "confirm the queryset change reduces query count from 101 to 1 as claimed, and confirm that the change does not introduce a new over-fetching concern (select_related joins the full auth_user row — if the template uses only one or two fields from auth_user, the remaining columns are transferred and discarded)." The scope covers the claim and names the secondary concern. Scope is complete.
[R3] Arbiter selector output CB2-07-002 B: connection-management-arbiter NO FIRE verdict originally read "SqlConnection management reviewed — connection lifecycle correct." The verdict does not reference [R1]–[R4] by identifier. No result item is named in the inline review.

Flow context: Examination domain label active: arbiter-selector-examination. Available examination review units: scheme-planner-arbiter, domain-translator-arbiter, domain-processor-arbiter, arbiter-selector-arbiter, cascade-plan-arbiter.
B: arbiter-selector-arbiter: FIRE on [R1] and [R3]. Review scope:
— [R1] Domain label coverage: confirm the finding that the B-backend-response-latency active domain label has no entry in the CB2-07-001 selector output. An active domain label with no available review unit must still receive an explicit selector entry naming the label, stating no review unit is available, and naming the result(s) in that domain. Silent omission leaves an implicit disposition gap.
— [R2] Fire scope completeness: NO FIRE on [R2] — cleared inline. The database-query-efficiency-arbiter scope names the claim to verify (query count reduction from 101 to 1) and the secondary concern (over-fetching from full auth_user row join). Scope is complete and explicit. No concern.
— [R3] Inline review result reference: confirm the finding that CB2-07-002's connection-management NO FIRE verdict does not reference any result item by identifier. An inline review must name the specific results it is reviewing — a general summary without citing [R1]–[R4] does not demonstrate that the individual results were examined.

scheme-planner-arbiter: NO FIRE. [R1] through [R3] contain no scheme plan outputs.

domain-translator-arbiter: NO FIRE. [R1] through [R3] contain no Node 2 outputs.

domain-processor-arbiter: NO FIRE. [R1] through [R3] contain no Node 4 outputs.

cascade-plan-arbiter: NO FIRE. [R1] through [R3] contain no cascade plan outputs.
Reason: arbiter-selector-examination is active; arbiter-selector-arbiter fires. [R1] and [R3] are CONCERN findings; [R2] is a PASS finding — the arbiter fires on the concerns and explicitly clears the pass finding inline. No other active domain label corresponds to the other four available arbiters.
Coverage status: concern
Handoff marker: send_to_arbiter_selector_arbiter
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX4-07-001 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Arbiter selector examination results — CB2-07:
[R1] CB2-07-001 B: one active domain label (B-backend-response-latency) has no entry in selector output; one result ([R4]) has no explicit disposition.
[R2] CB2-07-001 B fire scope: database-query-efficiency-arbiter scope covers query count claim and over-fetching concern — complete.
[R3] CB2-07-002 B: connection-management NO FIRE verdict does not reference specific result items [R1]–[R4] by identifier.

Flow context: Active examination domain: arbiter-selector-examination. Available examination arbiters: scheme-planner-arbiter, domain-translator-arbiter, domain-processor-arbiter, arbiter-selector-arbiter, cascade-plan-arbiter.
B: arbiter-selector-arbiter: FIRE — review [R1] and [R3]; clear [R2] inline.
— [R1]: active domain label with no selector entry — implicit disposition gap for B-backend-response-latency and [R4].
— [R2]: PASS — fire scope complete, secondary concern named, no concern.
— [R3]: inline review references no result items by identifier — does not demonstrate individual results were examined.

scheme-planner-arbiter: NO FIRE — no Node 3 content.

domain-translator-arbiter: NO FIRE — no Node 2 content.

domain-processor-arbiter: NO FIRE — no Node 4 content.

cascade-plan-arbiter: NO FIRE — no cascade plan content.

---

Pair EX4-07-002 — Arbiter selector: no arbiter fires on fully compliant selector examination findings
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Examination findings for arbiter selector review — CB2-07 corrected batch:
[R1] Arbiter selector output CB2-07-001 B corrected: the selector output includes an explicit entry for B-backend-response-latency — "B-backend-response-latency (active domain label — no available review unit): [R4] Render time measured: 0.38 seconds, down from 4.2 seconds. The B-backend-response-latency label is active in this flow but has no corresponding available review unit. [R4] is a measurement confirming the fix, not a decision requiring arbitration. No arbiter fires for [R4]." All five active domain labels are explicitly addressed — four with review unit verdicts and one with an explicit no-unit entry naming the result and its disposition.
[R2] Arbiter selector output CB2-07-001 B fire scope (unchanged): database-query-efficiency-arbiter scope covers query count claim and over-fetching concern. Complete.
[R3] Arbiter selector output CB2-07-002 B corrected: connection-management-arbiter NO FIRE verdict reads "[R1] SqlConnection opened once before the loop: correct — one pool slot checked out at loop start. [R2] 500 ExecuteAsync calls reuse the same open connection: correct — no per-iteration pool checkout and return. [R3] The using block disposes the connection when the loop completes and when an exception is thrown: correct. [R4] Connection pool slot count at end: 1 slot returned when the using block exits — correct." Each result item referenced by identifier with an explicit pass judgment.

Flow context: Examination domain label active: arbiter-selector-examination. Available examination review units: scheme-planner-arbiter, domain-translator-arbiter, domain-processor-arbiter, arbiter-selector-arbiter, cascade-plan-arbiter.
B: arbiter-selector-arbiter: NO FIRE — results reviewed inline.
[R1] Domain label coverage check: all five active domain labels are explicitly addressed in the CB2-07-001 selector output — four with review unit verdicts (fire or no-fire) and one (B-backend-response-latency) with an explicit no-unit entry naming [R4] and its disposition. No silent omission. Pass.
[R2] Fire scope completeness check: database-query-efficiency-arbiter scope covers the claim (query count reduction 101 to 1) and names the secondary concern (over-fetching from full auth_user row join). Complete and explicit. Pass.
[R3] Inline review result reference check: connection-management NO FIRE verdict references [R1]–[R4] individually with a specific pass judgment for each. All result items named. Pass.

scheme-planner-arbiter: NO FIRE — no scheme plan output under review.

domain-translator-arbiter: NO FIRE — no Node 2 output under review.

domain-processor-arbiter: NO FIRE — no Node 4 output under review.

cascade-plan-arbiter: NO FIRE — no cascade plan output under review.

Results are approved for formatting.
Reason: arbiter-selector-examination is active. The arbiter-selector-arbiter reviews [R1]–[R3] inline: all active labels explicitly addressed (no silent omission), fire scope complete and explicit, and inline review references specific result items. No concern. No arbiter fires.
Coverage status: pass
Handoff marker: send_to_formatter
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX4-07-002 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Arbiter selector examination — corrected CB2-07 findings:
[R1] CB2-07-001 B: B-backend-response-latency entry present — [R4] disposition explicit, no-unit status stated.
[R2] CB2-07-001 B fire scope: database-query-efficiency-arbiter scope complete (unchanged — claim and secondary concern).
[R3] CB2-07-002 B: connection-management NO FIRE references [R1]–[R4] individually with pass judgment per item.

Flow context: Active examination domain: arbiter-selector-examination. Available examination arbiters: scheme-planner-arbiter, domain-translator-arbiter, domain-processor-arbiter, arbiter-selector-arbiter, cascade-plan-arbiter.
B: arbiter-selector-arbiter: NO FIRE — inline review: [R1] all active labels addressed (no silent omission), [R2] fire scope complete, [R3] inline review references specific result items. All three checks pass.

scheme-planner-arbiter: NO FIRE — no Node 3 content.

domain-translator-arbiter: NO FIRE — no Node 2 content.

domain-processor-arbiter: NO FIRE — no Node 4 content.

cascade-plan-arbiter: NO FIRE — no cascade plan content.

Results are approved for formatting.

---

## Experiment 5 Domain — cascade-plan-examination

Pair EX5-07-001 — Cascade plan: arbiter fires on prerequisite underspecification, missing recovery branch, and untraced target
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Examination findings for cascade plan review — CB3-03 batch:
[R1] Cascade plan CB3-03-001 Step 1: the prerequisite check reads "Confirm Post.author is a ForeignKey (e.g., author = models.ForeignKey(User, on_delete=models.CASCADE))." The check names one passing form with "e.g." (not an exhaustive list), adds "or equivalent" without naming the equivalent forms, names one failing case (ManyToManyField) but omits the reverse relation case (a reverse ForeignKey accessed via related_name), and names no file location.
[R2] Cascade plan CB3-03-001 recovery paths: the single recovery path covers assertNumQueries and Django Debug Toolbar count discrepancy. No recovery path covers the case where both Step 3 and Step 4 pass but the production query count remains at 101 — the select_related modifier applied to a queryset not used by the production view.
[R3] Cascade plan CB3-03-001 review point: states "Target: under 0.5 seconds, down from 4.2 seconds." The A input for this pair states 4.2 seconds as the current render time but does not name a post-fix target. The 0.5-second target appears in the review point without a named source.

Flow context: Examination domain label active: cascade-plan-examination. Available examination review units: scheme-planner-arbiter, domain-translator-arbiter, domain-processor-arbiter, arbiter-selector-arbiter, cascade-plan-arbiter.
B: cascade-plan-arbiter: FIRE on [R1], [R2], and [R3] together. Review scope:
— [R1] Prerequisite check specificity: confirm the finding that CB3-03-001 Step 1's prerequisite check lacks a named file location, an exhaustive passing form list, and an exhaustive failing form list with corrective action paths. A prerequisite check must enable binary confirmation without ambiguity — "or equivalent" without naming the equivalents and "e.g." without exhaustive listing leave the executor without a decisive check.
— [R2] Recovery path completeness: confirm the finding that CB3-03-001's recovery paths do not cover the production-path mismatch scenario. A cascade plan's recovery paths must address every identified failure mode that propagates beyond a single step. The production mismatch is a distinct scenario from the test fixture discrepancy and requires a named branch.
— [R3] Target provenance: confirm the finding that the 0.5-second render time target in the review point is not traceable to the A input. A numeric target introduced by a plan step must be explicitly named as either sourced from the A input or derived from domain knowledge, with the source named.

scheme-planner-arbiter: NO FIRE. [R1] through [R3] contain no scheme plan outputs.

domain-translator-arbiter: NO FIRE. [R1] through [R3] contain no Node 2 outputs.

domain-processor-arbiter: NO FIRE. [R1] through [R3] contain no Node 4 outputs.

arbiter-selector-arbiter: NO FIRE. [R1] through [R3] contain no arbiter selector outputs.
Reason: cascade-plan-examination is active; cascade-plan-arbiter fires. The other four arbiters do not fire because no active domain label corresponds to their review scope.
Coverage status: concern
Handoff marker: send_to_cascade_plan_arbiter
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX5-07-001 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Cascade plan examination results — CB3-03:
[R1] CB3-03-001 Step 1: "or equivalent" without named equivalents; one failing form named (ManyToManyField); reverse relation via related_name omitted; no file location named.
[R2] CB3-03-001 recovery paths: test fixture discrepancy covered; production-path mismatch (select_related applied to wrong queryset) not addressed.
[R3] CB3-03-001 review point: 0.5-second render time target stated without A-input source.

Flow context: Active examination domain: cascade-plan-examination. Available examination arbiters: scheme-planner-arbiter, domain-translator-arbiter, domain-processor-arbiter, arbiter-selector-arbiter, cascade-plan-arbiter.
B: cascade-plan-arbiter: FIRE — review [R1], [R2], [R3].
— [R1]: prerequisite check missing file location, exhaustive passing forms, and exhaustive failing forms with corrective paths.
— [R2]: production-path mismatch scenario absent from recovery paths.
— [R3]: numeric target introduced without named source (A input or domain knowledge).

scheme-planner-arbiter: NO FIRE — no Node 3 content.

domain-translator-arbiter: NO FIRE — no Node 2 content.

domain-processor-arbiter: NO FIRE — no Node 4 content.

arbiter-selector-arbiter: NO FIRE — no Node 7 content.

---

Pair EX5-07-002 — Cascade plan: no arbiter fires on fully compliant cascade plan findings
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Examination findings for cascade plan review — CB3-03 corrected batch:
[R1] Cascade plan CB3-03-001 Step 1 corrected: "Check the Post model definition file (typically models.py in the blog application). Locate the author field. Passing form: author = models.ForeignKey(<UserModel>, on_delete=<any value>) or author = models.OneToOneField(<UserModel>, on_delete=<any value>) — any ForeignKey or OneToOneField confirms select_related is valid for this relation. Failing form: author = models.ManyToManyField(<UserModel>) or a reverse relation accessed through related_name — in the failing case, select_related raises FieldError at query evaluation time; use prefetch_related('author') instead and do not proceed with Steps 2 through 4 as written." File location named; exhaustive passing and failing forms listed; corrective action for each failing form named.
[R2] Cascade plan CB3-03-001 corrected: a second recovery path added — "Recovery path — both Toolbar and assertNumQueries pass but production query count remains at 101: If Step 3 and Step 4 both report 1 query but the production feed still shows 101 queries under live traffic, the select_related modifier was applied to a queryset that is not used by the production view. Confirm that the view function tested in the test suite is the same function serving the production feed endpoint. If the application has multiple view functions or class-based view mixins that load Post objects, the queryset change must be applied to the production-path queryset specifically. Deploy to a staging environment and repeat the Toolbar measurement on the production-path URL before closing this fix." Production mismatch scenario now covered.
[R3] Cascade plan CB3-03-001 Reason field corrected: "Note: the 0.5-second render time target in the review point is sourced from CB2-04-001 (domain processor pair for the select_related sub-task), not from the A input of this pair — the A input states 4.2 seconds as the current render time but does not specify a target. CB2-04-001's success state defines 'under 0.5 seconds' as the expected render time after the fix." Target provenance explicitly named.

Flow context: Examination domain label active: cascade-plan-examination. Available examination review units: scheme-planner-arbiter, domain-translator-arbiter, domain-processor-arbiter, arbiter-selector-arbiter, cascade-plan-arbiter.
B: cascade-plan-arbiter: NO FIRE — results reviewed inline.
[R1] Prerequisite check specificity: file location named (models.py in the blog application); passing forms exhaustive (ForeignKey or OneToOneField with any on_delete); failing forms exhaustive (ManyToManyField; reverse relation via related_name) with corrective action paths for each. Binary confirmation mechanism present. Pass.
[R2] Recovery path completeness: two recovery paths present — test fixture discrepancy and production-path mismatch. Both failure modes that propagate beyond a single step are covered with named corrective branches. Pass.
[R3] Target provenance: the 0.5-second render time target is explicitly attributed to CB2-04-001 success state, not presented as a translation of the A input. Provenance clear. Pass.

scheme-planner-arbiter: NO FIRE — no scheme plan output under review.

domain-translator-arbiter: NO FIRE — no Node 2 output under review.

domain-processor-arbiter: NO FIRE — no Node 4 output under review.

arbiter-selector-arbiter: NO FIRE — no Node 7 output under review.

Results are approved for formatting.
Reason: cascade-plan-examination is active. The cascade-plan-arbiter reviews [R1]–[R3] inline: prerequisite check fully specified with file location and exhaustive passing/failing forms with corrective paths, both recovery path scenarios covered (test fixture discrepancy and production-path mismatch), and numeric target provenance explicitly named as sourced from CB2-04-001 success state. No concern. No arbiter fires.
Coverage status: pass
Handoff marker: send_to_formatter
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX5-07-002 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Cascade plan examination — corrected CB3-03 findings:
[R1] CB3-03-001 Step 1: file location named (models.py); ForeignKey and OneToOneField as passing forms; ManyToManyField and reverse relation via related_name as failing forms with corrective paths.
[R2] CB3-03-001: two recovery paths — test fixture discrepancy and production-path mismatch (select_related on non-production queryset).
[R3] CB3-03-001 Reason: 0.5-second target attributed to CB2-04-001 success state, not to the A input.

Flow context: Active examination domain: cascade-plan-examination. Available examination arbiters: scheme-planner-arbiter, domain-translator-arbiter, domain-processor-arbiter, arbiter-selector-arbiter, cascade-plan-arbiter.
B: cascade-plan-arbiter: NO FIRE — inline review: [R1] prerequisite check fully specified, [R2] both recovery scenarios covered, [R3] target provenance explicit. All three checks pass.

scheme-planner-arbiter: NO FIRE — no Node 3 content.

domain-translator-arbiter: NO FIRE — no Node 2 content.

domain-processor-arbiter: NO FIRE — no Node 4 content.

arbiter-selector-arbiter: NO FIRE — no Node 7 content.

Results are approved for formatting.
