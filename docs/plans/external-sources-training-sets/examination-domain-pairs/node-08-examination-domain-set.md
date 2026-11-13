# Node 08 — Arbiter Units — Examination Domain Pairs
# Produced by gap closure pass across five experiment examination domains
# Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
# Version: 1.0 — 2026-05-04

Node 8 receives exactly one examination result and the review scope from Node 7.
It produces one verdict: Concern or pass.
These pairs teach arbiter unit behavior in the five examination domains introduced by the experiment translation work.

A is the single examination result submitted to the arbiter plus the arbiter's review scope as it arrives at runtime.
B is the exact verdict: either a Concern statement or the literal word pass.
A B that says "this finding may need attention" is wrong — B is either a Concern with the rule, location, correct version, and required change, or the word pass.

The pass case must appear in this file. An arbiter trained only on concern cases raises concerns for every result.

---

## Experiment 1 Domain — scheme-planner-examination

Pair EX1-08-001 — Scheme planner: concern verdict on parallel cross-reference in transformational prompt body
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Examination result submitted to scheme-planner-arbiter:
CB1-03-001 Step 3 transformational prompt body reads: "In the pipeline configuration file for the tail sampling processor, add the tailsampling processor entry after the memory_limiter processor entry. The memory_limiter processor is added in Step 4 (which runs in parallel with this step), so the tailsampling entry must appear below it in the pipeline list. Apply this configuration file independently of Step 4."

Arbiter scope: scheme-planner-arbiter reviewing CB1-03-001 Step 3 for parallel cross-reference. Step header declares Step 3 parallel with Step 4. Reason field asserts "processor configuration files are independent" and "applied together at deployment time." Check whether the transformational prompt body references Step 4's output as a dependency or positional constraint.
B: Concern: A step declared parallel with a sibling step must not reference that sibling step's output in its transformational prompt body. CB1-03-001 Step 3 transformational prompt body contains "The memory_limiter processor is added in Step 4 (which runs in parallel with this step), so the tailsampling entry must appear below it" — a forward reference to Step 4's output as a positional constraint while the Reason field declares Steps 3 and 4 independent. Required: the transformational prompt body must describe the final pipeline state without forward-referencing Step 4's output — "In the final configured pipeline, the tailsampling processor appears after the memory_limiter; configure the tailsampling processor entry independently without waiting for Step 4." The parallel claim and the prompt body must not contain contradictory ordering implications.
Coverage status: concern
Handoff marker: send_to_scheme_planner_with_concern_flag
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX1-08-001 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Single result for scheme-planner-arbiter:
CB1-03-001 Step 3 body references "the memory_limiter processor is added in Step 4 (which runs in parallel with this step)" as a positional requirement for the tailsampling processor entry. Step 3 and Step 4 are declared parallel with independent configuration files in the Reason field.

Arbiter scope: scheme-planner-arbiter — parallel cross-reference check. If the transformational prompt body names a parallel step's output as a prerequisite or positional constraint, the parallel claim is contradicted.
B: Concern: A step declared parallel and independent must not name the parallel step's output as a positional constraint in its transformational prompt body. CB1-03-001 Step 3 body uses "added in Step 4 (which runs in parallel)" as a positional anchor — the Reason field declares no ordering dependency. Required: the prompt body names the final pipeline order as an outcome ("the tailsampling processor appears after the memory_limiter in the final configured pipeline") without implying Step 3 must wait for Step 4.
Coverage status: concern
Handoff marker: send_to_scheme_planner_with_concern_flag

---

Pair EX1-08-002 — Scheme planner: pass verdict on corrected parallel step description
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Examination result submitted to scheme-planner-arbiter:
CB1-03-001 Step 3 transformational prompt body reads: "In the final configured pipeline, the tailsampling processor appears after the memory_limiter — Step 4 configures the memory_limiter concurrently, and both processors are applied together at deployment time. Configure the tailsampling processor independently of Step 4; do not wait for Step 4 before beginning this step."

Arbiter scope: scheme-planner-arbiter reviewing CB1-03-001 Step 3 for parallel cross-reference. Step header declares Step 3 parallel with Step 4. Reason field asserts "processor configuration files are independent" and "applied together at deployment time." Check whether the transformational prompt body references Step 4's output as a dependency or positional constraint.
B: pass
Coverage status: pass
Handoff marker: send_to_formatter
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX1-08-002 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Single result for scheme-planner-arbiter:
CB1-03-001 Step 3 body: "In the final configured pipeline, the tailsampling processor appears after the memory_limiter — Step 4 configures the memory_limiter concurrently, and both processors are applied together at deployment time." Step 3 and Step 4 are declared parallel in the Reason field.

Arbiter scope: scheme-planner-arbiter — parallel cross-reference check. Does the transformational prompt body name Step 4's output as a prerequisite or positional constraint?
B: pass
Coverage status: pass
Handoff marker: send_to_formatter

---

## Experiment 2 Domain — domain-translator-examination

Pair EX2-08-001 — Domain translator: concern verdict on implementation syntax in required outcome
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Examination result submitted to domain-translator-arbiter:
CB2-02-002 B required outcome field reads: "Target state: 1 database query, generating SELECT blog_post.*, auth_user.* FROM blog_post INNER JOIN auth_user ON blog_post.author_id = auth_user.id LIMIT 100. No additional queries triggered by template attribute access."

Arbiter scope: domain-translator-arbiter reviewing CB2-02-002 B required outcome for translation boundary violation. Check whether the required outcome contains implementation syntax (SQL clause vocabulary, specific field lists, table names in SQL form) rather than a structural description of the required outcome.
B: Concern: A formal domain statement's required outcome must describe the structural outcome — join relationship type, cardinality, round-trip count — not the SQL clause that implements it. CB2-02-002 B required outcome contains "generating SELECT blog_post.*, auth_user.* FROM blog_post INNER JOIN auth_user ON blog_post.author_id = auth_user.id LIMIT 100" — a complete SQL SELECT clause with specific field lists, table names, JOIN keyword, ON condition, and LIMIT clause. Required: the required outcome names the join relationship (a JOIN between the blog_post table and the auth_user table on the author foreign key), the cardinality (100 rows with corresponding author data), and the round-trip count (1), without SQL clause syntax. SQL vocabulary belongs in Node 3 or Node 4, not in Node 2 output.
Coverage status: concern
Handoff marker: send_to_scheme_planner_with_concern_flag
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX2-08-001 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Single result for domain-translator-arbiter:
CB2-02-002 B required outcome: "Target state: 1 database query, generating SELECT blog_post.*, auth_user.* FROM blog_post INNER JOIN auth_user ON blog_post.author_id = auth_user.id LIMIT 100."

Arbiter scope: domain-translator-arbiter — translation boundary check. Does the required outcome contain SQL syntax or implementation vocabulary that belongs in a lower node?
B: Concern: Implementation syntax present in the required outcome of a formal domain description. A required outcome from Node 2 names the structural result — join type, table relationship, round-trip count — not the SELECT clause. CB2-02-002 B contains SQL SELECT clause syntax with specific table names, field wildcards, JOIN keyword, ON condition, and LIMIT clause. Required: replace with structural description — "one round-trip JOIN query between blog_post and auth_user on the author foreign key, returning 100 rows with related author data."
Coverage status: concern
Handoff marker: send_to_scheme_planner_with_concern_flag

---

Pair EX2-08-002 — Domain translator: pass verdict on structurally correct required outcome
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Examination result submitted to domain-translator-arbiter:
CB2-02-002 B required outcome field reads: "Target state: a JOIN between the blog_post table and the auth_user table on the author foreign key, returning all requested fields in one round trip, limited to 100 rows. No additional queries triggered by template attribute access."

Arbiter scope: domain-translator-arbiter reviewing CB2-02-002 B required outcome for translation boundary violation. Check whether the required outcome contains SQL clause vocabulary, specific field lists, or table names in SQL form rather than a structural description.
B: pass
Coverage status: pass
Handoff marker: send_to_formatter
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX2-08-002 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Single result for domain-translator-arbiter:
CB2-02-002 B required outcome: "a JOIN between the blog_post table and the auth_user table on the author foreign key, returning all requested fields in one round trip, limited to 100 rows."

Arbiter scope: domain-translator-arbiter — translation boundary check. Does the required outcome contain SQL clause syntax or implementation vocabulary?
B: pass
Coverage status: pass
Handoff marker: send_to_formatter

---

## Experiment 3 Domain — domain-processor-examination

Pair EX3-08-001 — Domain processor: concern verdict on silent failure state with no observable signal
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Examination result submitted to domain-processor-arbiter:
CB2-04-002 Failure state 2 reads: "Hash key not available at routing time: TraceId is propagated in the W3C traceparent header (format: 00-<traceId>-<spanId>-<flags>). If the load balancer cannot parse the traceparent header to extract the traceId, it cannot perform consistent routing. Confirm that the load balancer can match and route on a substring of a custom header before attempting this configuration."

Arbiter scope: domain-processor-arbiter reviewing CB2-04-002 Failure state 2 for signal coverage. Check whether the failure state names an observable signal at the point of failure or, if the failure is silent at that point, explicitly acknowledges the silence and names the downstream observable signal.
B: Concern: A failure state with a silent onset must explicitly name either a direct observable signal at the failing component or acknowledge the silence and name the downstream signal where the failure becomes detectable. CB2-04-002 Failure state 2 names the condition (load balancer cannot parse traceparent header) and the consequence (incorrect routing) but contains no observable signal statement — no log entry, no metric, and no check is named at either the load balancer layer or downstream. Required: the failure state must explicitly state "this failure is not visible at the load balancer — it appears downstream as 'incomplete trace' or 'spans from unknown trace' counters incrementing in Collector metrics after the routing change is deployed" so the executor has a named check to run.
Coverage status: concern
Handoff marker: send_to_scheme_planner_with_concern_flag
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX3-08-001 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Single result for domain-processor-arbiter:
CB2-04-002 Failure state 2: "If the load balancer cannot parse the traceparent header to extract the traceId, it cannot perform consistent routing. Confirm that the load balancer can match and route on a substring of a custom header before attempting this configuration." No observable signal named at the load balancer or downstream.

Arbiter scope: domain-processor-arbiter — signal coverage check. Does the failure state name an observable signal, or does it acknowledge silence at origin and name the downstream signal?
B: Concern: Silent failure state — condition and consequence named; observable signal absent at both origin and downstream. A failure state must enable the executor to determine whether the failure has occurred. This failure produces no error log at the load balancer and is only visible downstream in Collector metrics. Required: the failure state names the downstream signal explicitly — "this failure is silent at the load balancer; it appears downstream as 'incomplete trace' counters incrementing in Collector metrics after the routing change is deployed."
Coverage status: concern
Handoff marker: send_to_scheme_planner_with_concern_flag

---

Pair EX3-08-002 — Domain processor: pass verdict on failure state with explicit observable signal
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Examination result submitted to domain-processor-arbiter:
CB2-04-002 Failure state 2 reads: "Hash key not available at routing time: TraceId is propagated in the W3C traceparent header (format: 00-<traceId>-<spanId>-<flags>). If the load balancer cannot parse the traceparent header to extract the traceId, it cannot perform consistent routing. Observable signal: the load balancer does not produce an error log for this failure — the failure is silent at the load balancer layer and becomes visible downstream. After the routing change is deployed, Collector metrics show 'incomplete trace' or 'spans from unknown trace' counters incrementing during the synthetic trace test. If those counters increment after the routing change but the synthetic trace test was expected to pass, traceparent parsing failure is the likely cause. Confirm that the load balancer can match and route on a substring of a custom header before attempting this configuration."

Arbiter scope: domain-processor-arbiter reviewing CB2-04-002 Failure state 2 for signal coverage. Check whether the failure state names an observable signal at the point of failure or, if the failure is silent at that point, explicitly acknowledges the silence and names the downstream observable signal.
B: pass
Coverage status: pass
Handoff marker: send_to_formatter
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX3-08-002 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Single result for domain-processor-arbiter:
CB2-04-002 Failure state 2 corrected: failure state acknowledges silence at the load balancer layer and names the downstream signal — "Collector metrics show 'incomplete trace' or 'spans from unknown trace' counters incrementing during the synthetic trace test."

Arbiter scope: domain-processor-arbiter — signal coverage check. Does the failure state acknowledge silence at the origin and name the downstream observable signal?
B: pass
Coverage status: pass
Handoff marker: send_to_formatter

---

## Experiment 4 Domain — arbiter-selector-examination

Pair EX4-08-001 — Arbiter selector: concern verdict on active domain label with implicit result disposition
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Examination result submitted to arbiter-selector-arbiter:
CB2-07-001 B selector output:
"database-query-efficiency-arbiter: FIRE on [R1] and [R2] together. [scope text]
orm-usage-pattern-arbiter: FIRE on [R1]. [scope text]
security-arbiter: NO FIRE. [reason]
connection-management-arbiter: NO FIRE. [reason]"
Flow context for this pair: Active domain labels: D-database-query-efficiency, E-orm-usage-pattern, B-backend-response-latency. Available review units: database-query-efficiency-arbiter, orm-usage-pattern-arbiter, security-arbiter, connection-management-arbiter. Result [R4] (render time: 0.38 seconds) belongs to the B-backend-response-latency domain label.

Arbiter scope: arbiter-selector-arbiter reviewing CB2-07-001 B for domain label coverage. Count active domain labels in the flow context. Count domain labels addressed in the selector output. Check whether any active label has no entry in the selector output.
B: Concern: An arbiter selector output must include an explicit entry for every active domain label in the flow context, regardless of whether a review unit exists for that label. CB2-07-001 B names verdicts for four available review units and is silent on the B-backend-response-latency active domain label and on result [R4]. Required: the selector output must include an explicit entry — "B-backend-response-latency (active domain label — no available review unit): [R4] Render time measured: 0.38 seconds, down from 4.2 seconds. The B-backend-response-latency label is active in this flow but has no corresponding available review unit. [R4] is a measurement confirming the fix, not a decision requiring arbitration. No arbiter fires for [R4]." A silent omission of an active domain label leaves an implicit disposition gap.
Coverage status: concern
Handoff marker: send_to_scheme_planner_with_concern_flag
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX4-08-001 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Single result for arbiter-selector-arbiter:
CB2-07-001 B selector output covers database-query-efficiency-arbiter, orm-usage-pattern-arbiter, security-arbiter, and connection-management-arbiter. Flow context lists five active domain labels including B-backend-response-latency. Result [R4] (render time: 0.38s) belongs to B-backend-response-latency. No entry in the selector output names the disposition of [R4] or the status of B-backend-response-latency.

Arbiter scope: arbiter-selector-arbiter — domain label coverage check. Are all active domain labels explicitly addressed?
B: Concern: Selector output is silent on one active domain label and its result. Active domain label count in flow context: 5. Domain labels addressed in selector output: 4. Difference: 1 — B-backend-response-latency has no entry. Required: add an explicit entry naming the label's no-unit status and the result's disposition so downstream nodes know [R4] was seen and cleared, not missed.
Coverage status: concern
Handoff marker: send_to_scheme_planner_with_concern_flag

---

Pair EX4-08-002 — Arbiter selector: pass verdict on selector output with complete domain label coverage
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Examination result submitted to arbiter-selector-arbiter:
CB2-07-001 B selector output:
"database-query-efficiency-arbiter: FIRE on [R1] and [R2] together. [scope text]
orm-usage-pattern-arbiter: FIRE on [R1]. [scope text]
security-arbiter: NO FIRE. [reason]
connection-management-arbiter: NO FIRE. [reason]
B-backend-response-latency (active domain label — no available review unit): [R4] Render time measured: 0.38 seconds, down from 4.2 seconds. The B-backend-response-latency label is active in this flow but has no corresponding available review unit. [R4] is a measurement confirming the fix, not a decision requiring arbitration. No arbiter fires for [R4]."
Flow context for this pair: Active domain labels: D-database-query-efficiency, E-orm-usage-pattern, B-backend-response-latency. Available review units: database-query-efficiency-arbiter, orm-usage-pattern-arbiter, security-arbiter, connection-management-arbiter. Result [R4] belongs to B-backend-response-latency.

Arbiter scope: arbiter-selector-arbiter reviewing CB2-07-001 B for domain label coverage. Count active domain labels in the flow context. Count domain labels addressed in the selector output. Check whether any active label has no entry in the selector output.
B: pass
Coverage status: pass
Handoff marker: send_to_formatter
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX4-08-002 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Single result for arbiter-selector-arbiter:
CB2-07-001 B selector output includes an explicit B-backend-response-latency entry: "[R4] is a measurement confirming the fix, not a decision requiring arbitration. No arbiter fires for [R4]." All five active domain labels addressed.

Arbiter scope: arbiter-selector-arbiter — domain label coverage check. Are all active domain labels explicitly addressed?
B: pass
Coverage status: pass
Handoff marker: send_to_formatter

---

## Experiment 5 Domain — cascade-plan-examination

Pair EX5-08-001 — Cascade plan: concern verdict on prerequisite check with underspecified confirmation criteria
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Examination result submitted to cascade-plan-arbiter:
CB3-03-001 Step 1 transformational prompt reads: "In the Post model definition, locate the author field. Confirm it is declared as a ForeignKey (e.g., author = models.ForeignKey(User, on_delete=models.CASCADE)) or equivalent. If it is a ManyToManyField, use prefetch_related('authors') instead."

Arbiter scope: cascade-plan-arbiter reviewing CB3-03-001 Step 1 for prerequisite check specificity. Check whether the prerequisite check names (1) the file to check, (2) the exhaustive list of passing forms with no ambiguity, and (3) the exhaustive list of failing forms with their corrective action paths.
B: Concern: A prerequisite confirmation step must name the file location, an exhaustive list of passing forms, and an exhaustive list of failing forms with corrective action paths for each. CB3-03-001 Step 1 uses "e.g." for the passing form (not an exhaustive list), adds "or equivalent" without naming the equivalent forms, names one failing case (ManyToManyField) but omits the reverse relation case (a reverse ForeignKey accessed via related_name), and names no file location. Required: the transformational prompt names (1) models.py as the file to check, (2) models.ForeignKey and models.OneToOneField with any on_delete value as the exhaustive passing forms, (3) models.ManyToManyField and a reverse relation accessed via related_name as the exhaustive failing forms, each with its corrective action path — "in the failing case, select_related raises FieldError at query evaluation time; use prefetch_related('author') instead and do not proceed with Steps 2 through 4 as written."
Coverage status: concern
Handoff marker: send_to_scheme_planner_with_concern_flag
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX5-08-001 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Single result for cascade-plan-arbiter:
CB3-03-001 Step 1: "Confirm Post.author is a ForeignKey (e.g., author = models.ForeignKey(User, on_delete=models.CASCADE)) or equivalent. If it is a ManyToManyField, use prefetch_related." Uses "e.g." and "or equivalent" — no exhaustive lists. Reverse relation case not named. No file location.

Arbiter scope: cascade-plan-arbiter — prerequisite check specificity. Does the check name the file, the exhaustive passing forms, and the exhaustive failing forms with corrective paths?
B: Concern: Prerequisite check with underspecified confirmation criteria. "e.g." and "or equivalent" leave the executor without a binary confirmation mechanism. Required: name models.py as the file; name ForeignKey and OneToOneField as the exhaustive passing forms; name ManyToManyField and reverse relation via related_name as the exhaustive failing forms, each with its corrective path.
Coverage status: concern
Handoff marker: send_to_scheme_planner_with_concern_flag

---

Pair EX5-08-002 — Cascade plan: pass verdict on fully specified prerequisite check
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Examination result submitted to cascade-plan-arbiter:
CB3-03-001 Step 1 transformational prompt reads: "Check the Post model definition file (typically models.py in the blog application). Locate the author field. Passing form: author = models.ForeignKey(<UserModel>, on_delete=<any value>) or author = models.OneToOneField(<UserModel>, on_delete=<any value>) — any ForeignKey or OneToOneField confirms select_related is valid for this relation. Failing form: author = models.ManyToManyField(<UserModel>) or a reverse relation accessed through related_name — in the failing case, select_related raises FieldError at query evaluation time; use prefetch_related('author') instead and do not proceed with Steps 2 through 4 as written."

Arbiter scope: cascade-plan-arbiter reviewing CB3-03-001 Step 1 for prerequisite check specificity. Check whether the prerequisite check names (1) the file to check, (2) the exhaustive list of passing forms with no ambiguity, and (3) the exhaustive list of failing forms with their corrective action paths.
B: pass
Coverage status: pass
Handoff marker: send_to_formatter
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal

Pair EX5-08-002 — Phrasing B
Source: EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal)
License: Internal
A: Single result for cascade-plan-arbiter:
CB3-03-001 Step 1 corrected: file location named (models.py); ForeignKey and OneToOneField listed as passing forms; ManyToManyField and reverse relation via related_name listed as failing forms with corrective paths (use prefetch_related, do not proceed with Steps 2–4).

Arbiter scope: cascade-plan-arbiter — prerequisite check specificity. Does the check name the file, the exhaustive passing forms, and the exhaustive failing forms with corrective paths?
B: pass
Coverage status: pass
Handoff marker: send_to_formatter
