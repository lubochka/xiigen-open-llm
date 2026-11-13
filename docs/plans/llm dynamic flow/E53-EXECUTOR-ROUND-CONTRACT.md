# E53 Executor Round Contract

Version: v1

Date: 2026-05-04

Status: Round A oracle document for the next implementation round

Next round: E53 Round B - Deterministic Experiment

Repository: `C:\Projects\llm_mvp_core`

Documentation folder: `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow`

## Purpose

This contract tells the executor exactly how to implement E53 Round B.

Round B is not a live LLM experiment. It is a deterministic orchestration proof.

The goal is to prove that the dynamic flow can manage a complex, stack-independent problem through routing, splitting, translation, planning, processing, application, convergence, knowledge-edge handling, arbiter review, result translation, and final formatting.

The executor must not replace this with a code-generation test.

The executor must not add new training pairs as a substitute for orchestration evidence.

## Required Reading Before Editing

Read these files in full before writing code:

1. `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\E53-HIGH-LEVEL-ORCHESTRATION-PROOF-PLAN-v1.md`
2. `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\E53-SCENARIO-001-SOURCE-PACKET.md`
3. `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\E53-SCENARIO-001-EXPECTED-TRACE.md`
4. `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\E53-ORCHESTRATION-EVIDENCE-SCHEMA.md`
5. `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\E53-HUMAN-RESULT-CHECKLIST.md`
6. `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\E53-LEARNING-SET-TRANSLATION-MATRIX.md`
7. `C:\Projects\llm_mvp_core\AGENTS.md`

Do not start from memory. The expected trace is the oracle.

## Round B Scope

Round B must add a deterministic experiment to the codebase.

The deterministic experiment must:

- load or embed the E53 scenario input;
- execute the dynamic flow stations in the same order defined by the expected trace;
- use deterministic station fixtures for every station output;
- record every node output;
- record every edge handoff;
- record array fan-out and fan-in counts;
- record dependency checks;
- record parallel-group checks;
- record convergence results;
- record the knowledge-edge context pack;
- record arbiter selections and verdicts;
- record the final human result;
- record learning gap packets;
- write JSON, markdown, and Mermaid evidence files;
- return a machine result that tests can assert against.

Round B must not:

- call any live external AI provider;
- choose a programming language, database, framework, cloud provider, or deployment environment;
- allow the duplicate-account branch;
- invent biometric passport verification behavior;
- hide the saved-state resume boundary;
- treat a metric-only pass as success;
- skip the human-readable evidence.

## Recommended Code Location

Add the experiment here:

`C:\Projects\llm_mvp_core\35.ExperimentsV2\Experiments\E53HighLevelOrchestrationProof.cs`

Add tests here unless the existing suite structure clearly prefers a separate file:

`C:\Projects\llm_mvp_core\Tests\DynamicFlowE53Tests.cs`

If the test project already groups all dynamic-flow experiments in:

`C:\Projects\llm_mvp_core\Tests\DynamicFlowE51Tests.cs`

then adding E53 tests there is acceptable, but a separate E53 file is preferred for readability.

## Required Public Experiment Shape

The experiment should expose one deterministic run method.

Suggested public shape:

```csharp
public static OperationResult<E53OrchestrationProofResult> Run()
```

If the repository's experiment pattern uses a different result type, follow the local pattern.

The public method must return the repository's success/error result wrapper instead of throwing as the normal control path.

The result object must include:

- scenario id;
- evidence file paths;
- node trace;
- edge trace;
- array counts;
- dependency trace;
- parallel trace;
- convergence report;
- knowledge-edge context pack;
- arbiter selections;
- arbiter verdicts;
- human fragments;
- final human result;
- learning gap packets;
- metrics;
- pass gates.

## Deterministic Station Rule

Each station fixture must represent one dynamic flow station.

The fixture must not call an LLM.

The fixture must not compute a different answer from the expected trace.

The fixture exists to prove orchestration, not model intelligence.

The station outputs must match `E53-SCENARIO-001-EXPECTED-TRACE.md`.

If the executor notices an inconsistency in the expected trace, stop and report it. Do not silently correct the oracle inside code.

## Required Station Coverage

The deterministic flow must include these stations or named equivalents:

1. logical structure compiler pre-stage;
2. topology route;
3. domain splitter;
4. domain translator array;
5. scheme planner;
6. domain processor array;
7. applier array;
8. convergence gate;
9. knowledge-edge context pack builder;
10. arbiter selector;
11. arbiter units;
12. result translator array;
13. formatter;
14. learning gap collector.

The evidence may use dynamic-flow node names from the architecture, but all of the above responsibilities must be visible.

## Required Test Names

Add these tests or exact local equivalents:

- `E53_Run_WritesJsonMarkdownAndMermaidEvidence`
- `E53_Run_PreservesAllHardEdges`
- `E53_Run_PreservesArrayFanOutAndFanInCounts`
- `E53_Run_EnforcesDependencyOrder`
- `E53_Run_AllowsOnlyValidParallelGroups`
- `E53_Run_RoutesBiometricPassportToKnowledgeEdge`
- `E53_Run_RejectsDuplicateAccountCreation`
- `E53_Run_KeepsFinalResultStackIndependent`
- `E53_Run_FinalHumanResultPassesChecklist`
- `E53_Run_ProducesLearningGapPacketsForMissingCoverage`

Each test must assert behavior, not only `IsSuccess`.

## Required Metrics

The experiment must produce these exact metric keys:

```text
lost_edge_count
node02_input_count
node02_output_count
node04_input_count
node04_output_count
node05_input_count
node05_output_count
dependency_violation_count
invalid_parallel_run_count
invented_biometric_plan_items
duplicate_creation_accepted
forbidden_stack_term_count
human_result_sections_present
live_external_calls
learning_gap_packet_count
e2e_orchestration_success
```

Expected values for Scenario 001:

```text
lost_edge_count = 0
node02_input_count = 11
node02_output_count = 11
node04_input_count = 11
node04_output_count = 11
node05_input_count = 11
node05_output_count = 11
dependency_violation_count = 0
invalid_parallel_run_count = 0
invented_biometric_plan_items = 0
duplicate_creation_accepted = false
forbidden_stack_term_count = 0
human_result_sections_present = 5/5
live_external_calls = 0
learning_gap_packet_count = 4
e2e_orchestration_success = true
```

## Required Pass Gates

The pass gates must include:

| Gate | Required Result |
|---|---|
| Edge preservation | every source fact, conflict, unknown, and rule is visible in at least one downstream node |
| Array preservation | domain translator, processor, and applier input/output counts match |
| Dependency order | no blocked dependency is executed as satisfied |
| Parallel grouping | only independent plan items run in parallel |
| Knowledge edge | biometric passport verification becomes a context pack, not a local implementation |
| Arbiter authority | duplicate account creation is rejected |
| Stack independence | final result contains no forbidden stack term |
| Human readability | final answer passes the five-section checklist |
| Evidence completeness | JSON, markdown, and Mermaid files are written |
| Learning feedback | four learning gap packets are produced |

## Required Evidence Files

Write files under:

`C:\Projects\llm_mvp_core\experiments-v2\E53-HIGH-LEVEL-ORCHESTRATION-PROOF\results`

Required files:

```text
run-YYYY-MM-DD-###.json
run-YYYY-MM-DD-###.md
run-YYYY-MM-DD-###.mmd
```

The JSON file must follow `E53-ORCHESTRATION-EVIDENCE-SCHEMA.md`.

The markdown file must include the human-readable result and the checklist pass/fail table.

The Mermaid file must show the actual route that ran, including the knowledge-edge branch and arbiter branch separately.

## Human Result Rule

The final result must be readable by a human who does not know the codebase.

It must include:

- what can proceed now;
- what is blocked until source rules exist;
- what is rejected;
- what requires outside or future learning;
- next actions.

The final result must not contain the forbidden stack terms listed in `E53-HUMAN-RESULT-CHECKLIST.md`.

## Learning Gap Rule

Round B does not train any model.

Round B does produce learning gap packets.

The four expected packets are:

1. saved-state resume rule missing;
2. biometric passport verification missing;
3. policy acceptance source rule needs confirmation;
4. duplicate callback rejection example should be retained as an arbiter negative example.

The packets must be evidence records, not hidden comments.

## Verification Commands

The executor must run:

```powershell
dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj
```

If the repository has a faster focused test command for dynamic-flow tests, run it first, then run the full command above.

The final report must include:

- focused test count and result;
- full suite count and result;
- evidence file paths;
- zip file path.

## Zip Requirement

Every round must produce a zip.

Round B zip must include:

- round manifest;
- files changed;
- test output summary;
- evidence JSON;
- evidence markdown;
- evidence Mermaid;
- any new or updated documentation;
- next-round prompt.

Store the zip under the working documentation folder:

`C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\round-zips`

## Round B Acceptance

Round B is accepted only when:

- all required tests pass;
- the full suite passes;
- evidence files exist and are readable;
- every metric matches the expected value;
- the final human result passes the checklist;
- no live external AI call occurs;
- no forbidden stack choice appears;
- the zip is produced.

If any item fails, the round remains open.

## Important Reminder

The hard part being tested is orchestration.

Do not make the experiment about producing code.

Do not make the experiment about one large LLM answer.

Do not skip edges to make the answer cleaner.

Do not use external AI to fill the biometric knowledge edge.

The correct behavior for an unknown competence is to name the missing competence, create a context pack, preserve the blocked branch, and create learning material for a future idle-learning round.
