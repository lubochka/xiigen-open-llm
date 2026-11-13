# DYNAMIC-FLOW-MINI-E2E-EXPERIMENT-PLAN-v1.0

## Round contract

Baseline: branch `main`, current observed test baseline `877/877` passing from `dotnet test LlmFromScratch.sln --no-restore`.

Current: the repository already has the pieces needed for a small proof: `29.SemanticCompiler`, `35.ExperimentsV2`, `06.VectorSearch`, the experiment result writer, quality checks, confidence gate, idle detector, replay buffer, and xUnit tests.

Produced: this plan defines one small end-to-end experiment named `E51-DYNAMIC-FLOW-MINI-E2E`.

Phase gate: this is an architecture and experiment-design round. Do not implement production flow code in this round.

Gap: the current code proves many individual parts, but it does not yet prove that the same small learning set can teach a flow and then be tested through the full chain from human request to logical structure, local station choice, quality gate, knowledge-edge detection, evidence, and later learning packet.

## Purpose

The purpose is to test the dynamic-flow theory in the smallest serious form.

The experiment must answer one question:

Can a tiny teaching set and a tiny testing set move through the same logical-structure path and produce traceable, correct station decisions, while arbiters remain a hard quality gate and unknown cases become learning packets?

This is not a full product implementation. It is a controlled proof that the designed flow can work when each part is small.

## Codebase facts found

The repository already has a semantic compiler in `29.SemanticCompiler`.

The compiler pipeline in `Pipeline/SemanticCompilerPipeline.cs` already performs:

1. tokenization,
2. stack detection before normalization,
3. route classification,
4. semantic normalization,
5. family collapsing,
6. logical predicate extraction,
7. optional decomposition,
8. optional beautification.

The logical extractor has a fixed 22-predicate vocabulary. That fits the design goal of a small internal language.

The current canonical codebase domains are locked as `CODE_Q`, `BUG`, `PLAN`, `ARCH`, and `REFACTOR`. The experiment must not change those locked domains.

The repository already has experiment infrastructure in `35.ExperimentsV2`.

The repository already has K-NN-related work:

1. `Core/StructuralKnnPredictor.cs`,
2. `06.VectorSearch/BruteForceIndex.cs`,
3. `E22DatasetRouter.cs`,
4. `E34V2` hybrid matching in the results map.

The repository already has quality and arbiter-like work:

1. `E39QualityOrchestrator.cs`,
2. `Infrastructure/QualityCheckLibrary.cs`.

The repository already has learning-loop support experiments:

1. `E41IdleDetector.cs`,
2. `E43ReplayBuffer.cs`,
3. `E46ConfidenceGate.cs`,
4. `E48DomainBootstrapper.cs`,
5. `E50DomainGraphClosure.cs`.

The current gap is not that these pieces are missing. The gap is that no experiment ties them together on one tiny data family and produces an auditable e2e trace.

## Experiment name

Use:

`E51-DYNAMIC-FLOW-MINI-E2E`

The number follows the existing experiment sequence after `E50-DOMAIN-GRAPH-CLOSURE`.

## Source family

Use one source family only:

`Flow-01 user registration`

This family is small enough to understand and rich enough to test the important flow behaviors:

1. provider sign-in,
2. email verification,
3. duplicate identity prevention,
4. token and session safety,
5. a knowledge edge,
6. an arbiter rejection.

Do not use the full registration corpus in the first experiment. Use only the micro set below.

## Dataset size

Use 18 records total.

1. 12 teaching records,
2. 4 held-out testing records,
3. 1 knowledge-edge challenge,
4. 1 arbiter-hard-gate challenge.

This is intentionally small. If it cannot work here, it will not become clearer by adding hundreds of files.

## Local subject labels

The experiment uses four local subject labels. These are not changes to the locked compiler domain enum.

1. `AUTH_PROVIDER_ENTRY`
2. `EMAIL_VERIFICATION`
3. `IDENTITY_DUPLICATE_PREVENTION`
4. `TOKEN_SESSION_SAFETY`

These labels represent the local topology below the broad registration subject.

The locked compiler domain may still say `PLAN`, `ARCH`, `BUG`, or `CODE_Q`. The local subject label is an experiment-level routing result, not a new compiler domain.

## Teaching records

Each teaching record has:

1. a human request,
2. expected local subject,
3. expected dynamic-flow station outputs,
4. expected arbiter needs,
5. expected learning role.

### Teaching set

| Id | Human request | Expected local subject |
|---|---|---|
| TR-01 | I want to sign up using my Google account. | AUTH_PROVIDER_ENTRY |
| TR-02 | Let users create an account with GitHub login. | AUTH_PROVIDER_ENTRY |
| TR-03 | A new user clicks continue with Google from the signup page. | AUTH_PROVIDER_ENTRY |
| TR-04 | I registered with email; send me a verification link. | EMAIL_VERIFICATION |
| TR-05 | People should confirm their email before using the product. | EMAIL_VERIFICATION |
| TR-06 | The verification email was sent but the account is not active yet. | EMAIL_VERIFICATION |
| TR-07 | The same email already exists when I try to register. | IDENTITY_DUPLICATE_PREVENTION |
| TR-08 | Do not create two users if the callback arrives twice. | IDENTITY_DUPLICATE_PREVENTION |
| TR-09 | I clicked signup twice and got two accounts. | IDENTITY_DUPLICATE_PREVENTION |
| TR-10 | My sign-in token expired while I was registering. | TOKEN_SESSION_SAFETY |
| TR-11 | Keep login cookies safe after registration. | TOKEN_SESSION_SAFETY |
| TR-12 | When the provider token is invalid, stop the registration. | TOKEN_SESSION_SAFETY |

## Held-out test records

These records must not be used for teaching.

| Id | Human request | Expected local subject |
|---|---|---|
| TE-01 | Can I join with my Google profile instead of a password? | AUTH_PROVIDER_ENTRY |
| TE-02 | Make people verify their mailbox after creating the account. | EMAIL_VERIFICATION |
| TE-03 | Prevent duplicate accounts when the same callback is repeated. | IDENTITY_DUPLICATE_PREVENTION |
| TE-04 | What happens if the login token expires before signup finishes? | TOKEN_SESSION_SAFETY |

## Challenge records

| Id | Human request | Expected result |
|---|---|---|
| CH-01 | I want biometric passport verification during signup. | Knowledge edge packet. The local system can see that this belongs near registration, but it does not have a trusted local subject pack for biometric identity proof. |
| CH-02 | If the same provider callback arrives twice, create a second account anyway. | Arbiter rejection. Similar examples may be found, but the quality gate must reject the instruction because it contradicts duplicate prevention. |

## What is being taught

The teaching set does not teach a big model.

It teaches small local mappings:

1. human request -> logical structure,
2. logical structure -> local subject,
3. local subject -> station work statement,
4. station work statement -> tiny scheme plan,
5. scheme plan -> applied result,
6. applied result -> human-readable result,
7. applied result -> required arbiter checks,
8. failed or unknown result -> learning packet.

The experiment should use the same compiler path for teaching records and runtime test records. That is the main theory being tested.

## Feature representation

For every teaching and testing request, build one feature packet.

The feature packet contains:

1. compiler route domain,
2. compiler predicate counts for the 22 predicate vocabulary,
3. normalized roots from `SemanticNormalizer`,
4. decomposition count from `Decomposer`,
5. source role, always `human_request` for this experiment,
6. local subject label for teaching records only.

Do not build vectors from raw text directly.

The normalized roots are allowed because they come from the same compiler path as the predicates. They are not a separate shortcut.

The initial vector should be:

1. 22 slots for predicate counts,
2. 5 slots for locked compiler domain one-hot,
3. up to 32 slots for normalized roots learned from the teaching set,
4. 1 slot for decomposition count.

Total maximum vector width: 60.

This keeps the experiment small and deterministic.

## Runtime flow under test

The experiment should run this full path for each held-out test and challenge record.

```text
human request
-> logical structure compiler
-> local topology K-NN
-> local subject decision
-> station work statement
-> tiny scheme plan
-> applied result
-> convergence check
-> arbiter selector
-> arbiter verdict
-> result translator
-> formatter
-> evidence record
```

For the knowledge-edge challenge, the path is:

```text
human request
-> logical structure compiler
-> local topology K-NN
-> low confidence or missing competence detected
-> external-help context pack created
-> simulated external answer adapted back into local structure
-> learning gap packet created
-> evidence record
```

The first experiment should not call a live external supplier. It should create the external-help context pack and use a fixed simulated answer. The live supplier test comes later, after this local path is proven.

## Station outputs

Each station output should be concrete, not a description of what output should be.

The expected outputs below are the ground truth for the first executor session.

### TE-01 expected trace

`Can I join with my Google profile instead of a password?`

Expected local subject:

`AUTH_PROVIDER_ENTRY`

Expected station work statement:

`The person wants to enter registration through an external identity provider instead of creating a password first. The flow must verify the provider result before creating or linking the account.`

Expected tiny scheme plan:

1. receive provider sign-in result,
2. verify the provider identity result,
3. find whether a matching account already exists,
4. create or link the account only after identity is trusted,
5. start the registered session.

Expected applied result:

`Provider sign-in is accepted only after the provider identity result is trusted. The account is created or linked after that decision, and the registered session starts only after account identity is resolved.`

Expected arbiter selector output:

`Run identity-consistency and token-safety review.`

Expected final user-facing result:

`Yes. The flow should let the person join with Google, but it must first confirm the provider result, then decide whether to create a new account or link an existing one, and only then start the session.`

Expected arbiter verdict:

`accepted`

### TE-02 expected trace

`Make people verify their mailbox after creating the account.`

Expected local subject:

`EMAIL_VERIFICATION`

Expected station work statement:

`The person wants account creation to be followed by mailbox verification. The flow may create a pending account, but normal product access must wait until the verification action proves that the mailbox belongs to the person registering.`

Expected tiny scheme plan:

1. receive the registration request,
2. create or locate the account in a pending-verification state,
3. send a verification message to the mailbox,
4. receive the verification action,
5. confirm that the verification action belongs to the pending account and is still valid,
6. mark the mailbox as verified,
7. allow normal account access only after verification is accepted.

Expected applied result:

`The account can exist before verification, but it remains pending. The mailbox verification action is checked before the account receives normal access, and the flow does not treat an unverified mailbox as a completed registration.`

Expected arbiter selector output:

`Run verification-order and token-safety review.`

Expected final user-facing result:

`Yes. The flow should create the account in a pending state, send the verification message, and only allow normal access after the mailbox verification is accepted.`

Expected arbiter verdict:

`accepted`

### TE-03 expected trace

`Prevent duplicate accounts when the same callback is repeated.`

Expected local subject:

`IDENTITY_DUPLICATE_PREVENTION`

Expected station work statement:

`The person wants repeated provider callbacks to be handled as the same registration event, not as permission to create another account. The flow must look for an existing provider identity or matching account before creating anything new.`

Expected tiny scheme plan:

1. receive the provider callback,
2. read the provider identity from the callback result,
3. search for an existing linked account or pending registration for that identity,
4. reuse the existing account or pending registration when one is found,
5. create a new account only when no matching identity exists,
6. return one account result for repeated callbacks.

Expected applied result:

`A repeated callback is idempotent. It resolves to the already known account or pending registration and does not create a second account for the same provider identity.`

Expected arbiter selector output:

`Run identity-consistency review.`

Expected final user-facing result:

`The flow should treat the repeated callback as the same registration attempt. It should reuse the existing account or pending registration and must not create a duplicate account.`

Expected arbiter verdict:

`accepted`

### TE-04 expected trace

`What happens if the login token expires before signup finishes?`

Expected local subject:

`TOKEN_SESSION_SAFETY`

Expected station work statement:

`The person is asking about a registration path where the token that proves the login attempt is no longer valid. The flow must stop the unsafe continuation, avoid starting a session from the expired token, and ask for a fresh trusted action.`

Expected tiny scheme plan:

1. receive the token-dependent registration step,
2. validate whether the token is still acceptable,
3. stop the registration step when the token is expired,
4. do not create a completed account from the expired token,
5. do not start a registered session from the expired token,
6. ask the person to restart the trusted sign-in or verification action.

Expected applied result:

`An expired token causes a controlled stop. The flow does not trust it for account completion or session start, and the person must repeat the trusted sign-in or verification action.`

Expected arbiter selector output:

`Run token-safety review.`

Expected final user-facing result:

`If the token expires before signup finishes, the flow should stop that registration step and ask the person to restart the trusted action. It should not create the completed account or start a session from an expired token.`

Expected arbiter verdict:

`accepted`

This is the level of concrete output required for every held-out test trace.

## Arbiters in the experiment

Use three tiny arbiters.

1. `identity_consistency`
2. `token_safety`
3. `verification_order`

They are simple deterministic judges in this experiment. They do not need to be LLMs yet.

Expected arbiter selections for the first executor session:

| Record | Expected selected arbiters | Expected verdict |
|---|---|---|
| TE-01 | `identity_consistency`, `token_safety` | `accepted` |
| TE-02 | `verification_order`, `token_safety` | `accepted` |
| TE-03 | `identity_consistency` | `accepted` |
| TE-04 | `token_safety` | `accepted` |
| CH-01 | no accepting arbiter path; create knowledge-edge context pack first | `learning_gap_packet_created` |
| CH-02 | `identity_consistency` | `rejected` |

The important rule being tested is authority:

K-NN can choose memory. Arbiters decide acceptance.

For challenge `CH-02`, the K-NN layer should find duplicate-callback examples and may have high similarity. That must not allow the result to pass.

Expected arbiter result for `CH-02`:

`Rejected. The request asks the system to create a second account when the same provider callback arrives twice. The duplicate-prevention rule says repeated callbacks must not create duplicate accounts. Send this fragment back for reprocessing or ask the user to confirm that they are intentionally changing the rule.`

## Knowledge edge in the experiment

Use one knowledge-edge case:

`I want biometric passport verification during signup.`

Expected detector behavior:

1. registration relation is recognized,
2. no local subject has enough trusted coverage for biometric identity proof,
3. the system does not invent a station answer,
4. a context pack is created,
5. a learning gap packet is created.

The external-help context pack should include:

1. original human request,
2. compiler route domain,
3. predicates,
4. normalized roots,
5. nearest local subjects and their similarity scores,
6. exact missing competence statement,
7. required answer shape,
8. arbiter constraints that still apply.

The missing competence statement should be human-readable:

`The local registration learning set knows provider sign-in, email verification, duplicate prevention, and token safety. It does not yet know how biometric passport verification changes the registration flow.`

## Evidence artifact

The experiment must produce evidence, not only pass tests.

Use the existing experiment output convention:

`experiments-v2/E51-DYNAMIC-FLOW-MINI-E2E/results/run-YYYY-MM-DD-###.json`

Also produce the markdown companion:

`experiments-v2/E51-DYNAMIC-FLOW-MINI-E2E/results/run-YYYY-MM-DD-###.md`

The evidence JSON must include:

1. experiment id,
2. run timestamp,
3. code commit if available,
4. teaching record ids,
5. testing record ids,
6. challenge record ids,
7. compiler coverage,
8. K-NN predictions,
9. selected local subject per test,
10. station work statement per test,
11. scheme plan per test,
12. applied result per test,
13. selected arbiters per test,
14. arbiter verdict per test,
15. final result per accepted test,
16. knowledge-edge context pack for `CH-01`,
17. learning gap packet for `CH-01`,
18. rejected packet for `CH-02`,
19. metric summary.

This evidence must be readable without re-running the experiment.

## Metrics and pass gates

The experiment passes only if all gates pass.

| Gate | Required result |
|---|---|
| Compiler coverage | 18/18 records produce at least one predicate and at least one normalized root. |
| Teaching/runtime symmetry | Every teaching and testing record uses the same feature-building method. |
| Held-out local subject accuracy | 4/4 held-out test records route to the expected local subject. |
| Station output completeness | 4/4 held-out records contain work statement, scheme plan, applied result, arbiter selector output, and final user-facing result. |
| Arbiter authority | `CH-02` is rejected even if nearest-neighbor similarity is high. |
| Knowledge-edge detection | `CH-01` creates a context pack and learning gap packet instead of producing a confident local answer. |
| Evidence completeness | Evidence JSON and markdown are written with per-record traces. |
| Existing test safety | Full solution test count remains at least `877/877` passing. |

The primary metric should be:

`e2e_trace_success_rate`

Definition:

`accepted correct held-out traces + correct challenge traces / total held-out and challenge traces`

For this first experiment:

`6/6` is required.

Do not accept `5/6` as good enough. The data is intentionally tiny.

## Baseline comparison

The experiment should compare two approaches.

Baseline:

Use only the locked compiler route domain, such as `PLAN`, `ARCH`, `BUG`, or `CODE_Q`.

Enhanced:

Use the full feature packet:

1. compiler route domain,
2. predicate counts,
3. normalized roots,
4. decomposition count,
5. K-NN over teaching records.

Expected finding:

The baseline is too broad because many registration requests can fall into the same broad compiler domain. The enhanced path should distinguish provider entry, email verification, duplicate prevention, and token safety.

This comparison matters because it tests why the architecture needs local topology narrowing after logical compilation.

## Implementation location

Keep implementation scoped to experiment and test files.

Add:

1. `35.ExperimentsV2/Experiments/E51DynamicFlowMiniE2E.cs`
2. `Tests/DynamicFlowMiniE2ETests.cs`

Update:

1. `35.ExperimentsV2/ExperimentsV2Dispatcher.cs`
2. `35.ExperimentsV2/ExperimentRunner.cs`
3. `35.ExperimentsV2/Infrastructure/ResultWriter.cs`
4. `docs/EXPERIMENT-RESULTS-MAP.md` after evidence exists

Do not change:

1. `29.SemanticCompiler/Core/Core.cs` domain enum,
2. `29.SemanticCompiler/Compression/LogicExtractor.cs` predicate vocabulary,
3. production projects outside the experiment path,
4. existing result semantics for E01 through E50.

## Test plan

Add focused xUnit tests:

1. `E51_BuildDataset_HasExpectedCounts`
2. `E51_CompileAllRecords_ProducesLogicalFeatures`
3. `E51_HeldOutTests_RouteToExpectedLocalSubject`
4. `E51_StationOutputs_AreConcreteForHeldOutRecords`
5. `E51_ArbiterSelections_MatchGroundTruthForHeldOutRecords`
6. `E51_ArbiterGate_BlocksDuplicateAccountInstruction`
7. `E51_KnowledgeEdge_CreatesContextPackAndLearningPacket`
8. `E51_Run_WritesEvidenceShape`

Then run:

```powershell
dotnet test LlmFromScratch.sln --no-restore --filter "DynamicFlowMini"
dotnet test LlmFromScratch.sln --no-restore
```

The first command proves the experiment. The second command proves it did not damage the existing system.

## Expected evidence shape

The evidence should show one row like this for each held-out test:

```json
{
  "record_id": "TE-01",
  "human_request": "Can I join with my Google profile instead of a password?",
  "compiler_domain": "PLAN",
  "predicates": ["AUTH_OP", "CREATE_OP"],
  "normalized_roots": ["join", "google", "profile", "password"],
  "nearest_neighbors": [
    {"id": "TR-01", "score": 0.82},
    {"id": "TR-03", "score": 0.76}
  ],
  "predicted_local_subject": "AUTH_PROVIDER_ENTRY",
  "expected_local_subject": "AUTH_PROVIDER_ENTRY",
  "station_work_statement": "The person wants to enter registration through an external identity provider instead of creating a password first. The flow must verify the provider result before creating or linking the account.",
  "scheme_plan": [
    "receive provider sign-in result",
    "verify provider identity result",
    "find whether a matching account already exists",
    "create or link the account only after identity is trusted",
    "start the registered session"
  ],
  "selected_arbiters": ["identity_consistency", "token_safety"],
  "arbiter_verdict": "accepted",
  "final_user_result": "Yes. The flow should let the person join with Google, but it must first confirm the provider result, then decide whether to create a new account or link an existing one, and only then start the session."
}
```

The challenge evidence should show:

1. `CH-01` has `knowledge_edge_detected: true`,
2. `CH-01` has an external-help context pack,
3. `CH-01` has a learning gap packet,
4. `CH-02` has `arbiter_verdict: rejected`,
5. `CH-02` records that K-NN similarity did not override the arbiter.

## Why this is the right first chunk

It is small enough to inspect by hand.

It tests the same compiler for teaching and runtime.

It tests topology narrowing without changing locked compiler domains.

It tests K-NN as memory, not authority.

It tests arbiters as authority, not advice.

It tests knowledge-edge handling without relying on a live external supplier.

It produces evidence that can be examined record by record.

It keeps all risk inside the experiment layer.

## What success means

Success does not mean the dynamic-flow architecture is complete.

Success means the architecture has passed its first closed-loop proof:

1. a small learning set can teach a local flow,
2. held-out human prompts can travel through the same logical path,
3. the system can choose the correct local station,
4. generated station outputs can be inspected,
5. arbiters can block wrong output,
6. knowledge edges can become learning packets,
7. evidence can prove every step.

After that, the next round can safely scale one axis at a time:

1. add more registration examples,
2. add real document-derived learning records,
3. replace deterministic station outputs with learned small units,
4. replace simulated external help with supplier adapters,
5. add idle-time replay and installation gates.
