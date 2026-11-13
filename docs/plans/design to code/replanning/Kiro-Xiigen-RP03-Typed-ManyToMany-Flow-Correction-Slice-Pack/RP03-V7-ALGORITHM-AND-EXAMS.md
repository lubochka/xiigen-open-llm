# RP03 V7 — гибридный алгоритм, первый trial и предметные экзамены

Обновлено 16.10.2026. Это selected design и методика, не executed implementation. Свежий inspected K=`58d12ecfe7af05e3221f984bf6c4c67d82fe3b96`; исполнитель перепроверяет свой HEAD. [Findings](../RP01-RP05-REPAIR-ATLAS-2026-10-15/17-RP03-FIRST-LESSON-READINESS-REPAIR-2026-10-16.md) содержат exact source/bounds. Старые N22–N31 сохраняются, ниже устранена неоднозначность trial/adoption.

## A. Данные, законы, представления и четыре роли

```text
request/input IDs + raw content + rights + domain uncertainty
→ domain/law/schema/representation passport
→ Unit: source-associated many-to-many candidate outputs
→ lawful OutputRef resolution into actual TOutput
→ F0: type/tenant/law/version/effect/cycle constraints
→ F1: learned score over lawful semantic candidate features
→ F0 recheck immediately before any effect
→ actual next Unit → requested output → independent outcome/feedback
→ authorized training owner → new isolated checkpoint
```

q1→{o1,o2}, q2→{o2,o3} means FOUR association edges. Corpus SourceIds, request IDs, CandidateIds and OutputRefs are different identities. A queryable scalar projection, list of hashes, OrderedTargets or `TargetsExecuted=false` handoff is not proof of typed next-Unit execution. Preserve K scalar `ILogicalUnit<TInput,TOutput>` and use the smallest compatible composition; do not create duplicate public APIs only to match conceptual names.

ILearningUnit learns/maps a domain's typed many→many transformation. ILearningFlow learns lawful correction/relations between Units. RP04 owns asynchronous ILearningOrchestrator and higher-order ILearningDynamicGraph. RP03 does not silently redefine the full graph or replace it with a lookup table. Training owner and pure L2 inference are separate; domain-specific A-LM-02 is not a command to move every trainer.

### Law-conditioned kNN is part of the algorithm

RP02 maps known/mixed/unknown domains, creates domain-appropriate roles/AST/grammar/referent/schema features, scopes lawful neighbor sets and adapts metric/reranking/k under a resource budget. C# nullability/type roles, EN→RU POS/coreference/negation, SQL dialect/schema/NULL/tenant semantics must not disappear behind token frequency or CandidateId. Record role-conditioned errors and domain confusion, not only distance.

Primary [arbiter/KNN amendment](../../../llm%20dynamic%20flow/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md): pre-generation positive/negative/warning signals guide retrieval; post-generation arbiter retains acceptance authority. A failed neighbor can warn without being executed. Historical 0.50 threshold/E51 claims are not newly validated universal constants.

RP03 first experiment freezes one actual RP02 representation/index/kNN version to isolate F1. This is experimental control, not lower priority for kNN; RP02 can develop in parallel. If available phi is only IDs/order or lacks required semantics, report FEATURE_GAP and implement the smallest actual feature bridge, not a fake domain-aware learner. Later compare mapper/representation/kNN effects separately and a small predeclared kNN×F1 interaction when both work.

RP18 owns LoRA→semantic coordinate projection/alignment. Student LoRA weights, token IDs, embeddings and vector index are not interchangeable. Every join needs feature order, schema/coordinate/index identities, dimensionality and an actual consuming reader. Same dimension is insufficient compatibility.

## B. First real Flow learner: selected minimal mechanism

Use a small regularized pairwise scorer, not a new giant trainer. Two F0-lawful comparable actions for the SAME x/domain/law/port have actual pre-outcome features phi. Independent authorized observation chooses a+ over a−.

```text
g = phi(x,a+) - phi(x,a-)
z = dot(W0,g)
p = stable_sigmoid(z)
W_trial = project_bounds(W0 + eta*((1-p)*g - lambda*W0))
```

Freeze feature semantics/order, eta/lambda/bounds and the objective before the lesson; log clipping/regularization and finite values. Candidate ID, serialization position, future answer, grade or outcome MUST NOT be features. IDs only resolve evidence; guard renaming must not erase or change semantics. Availability of law/version cannot be presumed from `request.Metadata`: inspected pipeline builds an empty dictionary at that call site. Use an existing validated immutable context or explicitly design the smallest shared snapshot; trace its reader first.

One authorized unique feedback event is applied once. Tie, one legal alternative, missing consent/independence, incompatible version, g=0 or non-finite input means NO_UPDATE. Bounds saturation with zero delta is NO_EFFECT, not invented W1. Diagnostic main/order/identity calls are inference-only, use one W/feature snapshot and do not consume training events.

### Numeric update does NOT require a rank flip

Illustration, not data: phi(A)=(1,0), phi(B)=(0,1), W0=(1,0), B>A, eta=.2, lambda=0, inactive bounds. W1=(.8537882843,.1462117157); B−A margin changes -1→-.7075765685 and pairwise loss 1.3132616875→1.1082549776, while A stays top1. These coordinates illustrate arithmetic, not candidate-identity one-hot encoding.

Record separately PARAMETER_APPLIED, SCORE_EFFECT, RANK_CHANGED, TRIAL_COLD_CONSUMED, DOMAIN_GAIN and USER_ADOPTED. Correct numerical application and real reader are first-step evidence; generalization remains NOT_MEASURED. Never increase eta, cherry-pick a pair or reuse feedback until argmax flips. With regularization/clipping do not promise every partial margin/loss improves monotonically; inspect the declared objective and report the actual effect.

## C. Trial lifecycle without premature serving

**BUILD:** reuse/find scorer core, feature reader, training owner and serializer. Only if absent, implement a minimal internal equivalent, callable from an authorized isolated experiment. Production and trial must share the algorithm implementation, not a test-only toy.

**UPDATE:** W0→W_trial in isolated state; distinguish volatile numerical step from durable commit. Preserve preimage + feedback identity. No live exact-ID publication, false IsLearned/Trusted or ambient mutable global.

**COLD TRIAL (02D):** coherent checkpoint contains W, feature/law/coordinate schema, event-ledger, parent reference and checksum. New PID loads this artifact through the shared real reader in experiment mode. Parent restore and once-only replay are observed there. Live registry tuple remains unchanged. If a crash leaves commit unknown, inspect existing intent/checkpoint before any retry; do not generate a new ID for the same old observation.

**TRIAL JOIN / EXAM:** typed real consumers may be exercised in an isolated composition. Grade actual outputs when available; scorer-only evidence stays scorer-only. Run 03B before ordinary adoption; BUILD/JOIN does not itself require quality acceptance.

**ADOPT (02C, publisher machinery 02D):** only the existing authorized scoped admission can publish accepted weights plus compatible registry tuple. Exact resolver currently requires IsLearned and >=Probationary and selects latest exact-ID before validation. Never publish experimental Candidate under the live ID to unblock a trial. A failed/rejected/uncertain trial leaves previous accepted serving intact. Cold-load is not the admission decision. If no accepted parent exists, report NOT_ADOPTED and use lawful existing fallback/abstain; do not invent a trusted W0 baseline.

03C verifies actual ordinary Host after adoption. Laboratory C0 and ordinary T are not comparable latency arms. When only T is accepted, keep C0/T science in the same isolated composition and report ordinary T separately; a previously accepted W0 can be compared on the ordinary path only when legitimately available.

## D. DPO is a separate learning objective and role contract

For the RP03 handoff, CODEX with **Astra 6 UltraCode** selects the case in the delegated human role. The authorized **external open Chinese model authors BOTH final chosen/rejected**. A **separate CODEX with Astra 6 UltraCode** grades the exact final bytes/hashes. Actual model/revision/call IDs and rights are required; no fabricated call and no same-author self-vote. Flow feedback may instead be an independently observed authorized metric; do not force every Flow preference through Student DPO.

Standard Student DPO uses response-conditioned policy/reference log-ratios and its own optimizer/LoRA recipient, not CODEX score, cosine distance or Flow W. Read the [current RP01 entry](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-EXECUTE-NOW.md) for that owner's execution; do not dispatch historical RP01 V6 or globally transplant its development exception.

**Negative examples have different roles.** An F0-forbidden action cannot be a lawful executable F1 alternative. A domain-wrong answer may still be an informative DPO rejected response or warning neighbor under the owning data policy; it is not thereby allowed to execute. Example: losing nullable `?` is refused as a Flow action but can identify a semantic error in an authorized teacher-authored rejected answer. Do not delete all negative learning material by overgeneralizing the F0 rule. Both-bad/tie/unknown is quarantined rather than inventing preference.

## E. Scientific domain examination — no software coverage quota

Use independent **source-family** partitions TRAIN / OP_DEV / EXAM_SEALED. Seal rights/provenance, grader rubric, preprocessing and candidate policy before seeing exam outputs. Byte seal and inaccessible exam split are different properties. Original source, paraphrases, translations and generated descendants belong to one family for leakage control.

**C0 and T see the SAME held-out cases.** These cases are different from training/development families, not different across arms. C0=corrected code with pre-update W0; T=identical code with W_trial. Freeze Unit/Student, domain mapping, tokens/vector/index/kNN, graph, candidate pool, decoder, grader, budget and input. T−C0 estimates the selected learning factor; P legacy is optional additional wiring contrast. If P cannot use the same interface, label descriptive/noncausal; never drop C0.

Compare paired case outcomes: both correct, gain, loss, both wrong; report k/N and (gains−losses)/N, law breaches, abstention and coverage. Missing/timeout/denied cases remain in the predeclared accounting, not silently removed. Report family-level uncertainty; a tiny pilot is INCONCLUSIVE for broad quality, not a 95% certificate. Repeated checkpoints are examined on OP_DEV for selection; EXAM_SEALED cannot become a tuning loop. If held-out results influence changes, retire that panel to DEV and obtain a fresh independent panel for the next confirmatory claim.

### Concrete first three domain passports

| Domain | GOOD / meaningful lawful alternatives | BAD / why | Edge and examination |
|---|---|---|---|
| C# documentation | Two descriptions preserve `Task<int?>`, async/exception contract; independently compare usefulness | `Task<int>` silently loses nullable; string match or compile alone called documentation quality | Overloads, nullable context/version, cancellation, multiple valid descriptions. Verify facts against source/type model; blind grade clarity/completeness. |
| EN→RU | `The doctor said she would return` → consistent female referent; both lawful renderings preserve meaning | Output switches the same referent to masculine without evidence, loses negation, or treats all doctor mentions as male | Unknown antecedent, grammatical vs semantic gender, multiple valid translations: conditional/unknown, not invented hard prohibition. Measure role/coreference/negation errors and calibrated abstain. |
| SQL | Pinned dialect/schema; two authorized semantically equivalent queries preserve tenant scope and NULL semantics | Missing tenant filter; `= NULL` where intended dialect requires IS NULL; latency compared on different data | Unknown engine, duplicate rows, NULL joins, empty set, rights change: abstain/deny or domain-specific oracle. Evaluate sandboxed query semantics and real measured cost, not learner coverage. |

Separate children CSHARP/ENRU/SQL remain first; ORCH/DESIGN/ANGULAR/DPO extend to all seven WorkLeft tracks without blocking first W1. No domain-specific update → label zero-shot transfer/retention/OOD; even measured transfer is not evidence of a separate domain's training event. Do not fabricate seven weight recipients from one C# lesson.

## F. Bulk-to-bulk learning and PC economics

First record one true update as soon as a lawful signal exists. Then predeclare a bounded batch of NEW eligible feedback events per domain; a suggested pilot ceiling is 16, not a prerequisite, target quality threshold or mandated test count. Existing owner budget/authorization overrides this proposed ceiling. Every event reports received/eligible/applied/tie/rejected/replayed/no-effect; several losers from one input are not independent lessons.

For B0,B1,... record checkpoint parent/current, new unique data, domain/representation versions, update wall time, OP_DEV curve, periodic untouched exam panel, accuracy/gain/loss, lawful route coverage, abstention, retained prior-family performance and worst-domain regression. A valid pair or finite update can yield no benefit or regression. Keep the accepted parent on INCONCLUSIVE/REGRESS; do not weaken thresholds after results. Learning continues in a new isolated trial, not by discarding feedback history.

Record hardware/OS/runtime, cold-start versus warm latency, complete request p50/p95 with sample count, inference calls including actual guard probes, CPU/GPU, peak RAM/VRAM, model/LoRA/tokenizer/vector index/Flow state/optimizer/journal/rollback/cache disk, teacher/judge/failed-attempt costs and per-bulk amortization. Local compactness is measured, not inferred from a tiny W matrix while excluding the rest of the process. Same code/inputs/hardware/cache policy/budget in compared arms; interleave order to reduce drift. No 9× latency claim from static call count.

Optional later one-factor arms: domain map; semantic representation; lawful learned metric/rerank; adaptive k; Flow F1; RP18 projection; RP01 Student DPO. Preserve the owner and real state for each. A small predeclared kNN×F1 factorial can examine interaction after mechanisms exist; it is not mandatory work before the first lesson.

## G. Minimal engineering evidence and honesty

Source/planning/grading = **0 new product tests**. Changed deterministic boundary = minimum reused or narrow check for the actual risk, e.g. finite formula, once-only event, unauthorized effect denial, typed association, atomic trial checkpoint. No artificial minimum/maximum test count, no line-coverage target and no blanket Windows CI to certify a learning system. A compiler/SQL engine/domain oracle used to judge a generated answer is scientific task evidence, not a quota of learner unit tests.

Preserve security/native release obligations when that path is reached. Do not disable App Control. Publish raw failures and NOT_RUN/NOT_MEASURED/NOT_PERFORMED. Plan publication, arithmetic illustration and author audit are not actual Student/Flow training, measured domain gain, PC benchmark or independent Astra invocation.
