# S17 source routes, current code seams, RAG and owner boundaries

## Known branch baselines for **authoring only** (refresh at execution)
Planning P=`lubochka/xiigen-planning:llm_plan3`, observed HEAD `f093ca11aef8c5eec9663a72c0ac904bda012ed5`. Core K=`lubochka/llm_mvp_core:codex/dpo-implementation`, observed HEAD `3c7f5f8c7809936e25579603c96ea6df8598fc40`. Stale SHAs are history, never automatic reset targets. D=`P/llm mvp/design to code session`; G=`P/llm mvp/dpo legal build`; F=`P/llm mvp/llm dynamic flow`; V=`P/llm mvp/visual studio & visual studio code continue`.

**First current-reality check:** read P/D `AGENTS.md`, P/D/replanning `AGENTS.md`, K `AGENTS.md` / applicable `CLAUDE.md`, branch HEAD and affected file blobs, full method+callers and fresh task owners. Record `READ_FULL / READ_SCOPED / INDEX_ONLY / NOT_READ` for each relevant route. No agent may promote a directory listing or someone else's Rxx claim into a fresh complete source review.

## Minimum first-party provenance; scoped by task, not another 25-pass audit
| Root | Required question | Starting material and mandatory descent |
|---|---|---|
| D/original task.md; task prompt.md; goal of the round.md; Алгоритм создания ДПО.md | Original four roles, DPO legality, PC, G/F/V/DTC/IDE | Map each applicable requirement→current owner→actual downstream consumer; no S04-only shortcut |
| D/09.09.26WorkLeft-02-teaching-chinese-model.md, -MECH-01-mechanism-validation.md, -RESEARCH-03-dpo-algorithm-conformance.md | Who teaches and what weights/state actually change? | Examine relevant method/source citations rather than trusting old conclusions |
| D/09.09.26WorkLeft-RAG-MASTER-INDEX.md and -RAG-00-usage-guide.md | Route to G/F/V originals | Open applicable RAG-01/02/03/04 MD and NDJSON entries, follow referenced primary documents; include unresolved paths and visited-set |
| G/active-codex-execution-plan-07-dpo-builder.md, -09-auto-dpo-loop.md, -10-training-lifecycle.md | Exact pair, legal learner, machine training | Inspect source paragraphs and K current DPO/ModelServer recipient |
| F/DECISIONS-LOCKED.md; ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md; DYNAMIC-FLOW-LOCKED-DECISION-D13-AUTHORITY-v1.md | Domain authority, k/radius, learned vs deterministic arbiter | Inspect source law and underlying K consumer; D-07 stack detection precedes normalization |
| V/current RAG and host plans | Ordinary IDE user surface | Preserve authority and report missing host binding rather than inventing transport |
| replanning/previous rounds thinking results.md, PARALLEL-R01-R03-JOIN.md, RECOVERED-DESIGN-CONTRACT.md, HYBRID-ALGORITHMS-AND-RUNS.md, R01-R25-TO-CODE-SLICES-STRATEGY.md | Preserve prior decisions and concurrency | R01–R25 run independently; evidence is not proof of completed model calls or gains |
| replanning/results/R04/R05/R06/R07/R08/R09/R13/R17/R18/R21/R22/R23/R24/R25 | Apply a particular research claim | Read its FINDINGS/SOURCES/DECISIONS and checkpoint/status as needed. Independent R15/R25 Astra not assumed performed |

## Verified K source observations (13 October; revalidate at executor time)
- `K/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs`: `Train` creates BPE-token + predicate vectors/HNSW; `ExecuteAsync` calls `_index.Search(query,_k)` but uses `hits[0]`'s `Output`, attaching all hit IDs as evidence. Thus `k>1` evidence is **not** multi-neighbor output fusion. **RP02.02A** owns general fusion; S17 owns version-sensitive admissibility for whichever actual consumer is verified.
- `K/21.AgentInfrastructure/Logical/PromptDomainClassifier.cs`: current five D-04 classifier labels `BUG/PLAN/ARCH/CODE_Q/REFACTOR` are **developer task domains, not** C# language-version, SQL dialect, EN/RU register, DTC device targets or law version. Never use the D-04 label as the only compatibility key.
- `K/23.SemanticCompilerAdapter/TrainingVectorBuilder.cs`: E51B `VectorWidth=60` development vector; it is **not identical** to `KnnLogicalUnit`'s BPE/predicate space. Treat version/schema/metric compatibility as explicit.
- `K/00.Core/LogicalContracts.cs`, `K/00.Core/LearningMachineContracts.cs`: `ILogicalUnit<TInput,TOutput>` and `ILogicalFlow` are current boundaries; top-level `ILearningMachine<TDomain>.TrainBatchAsync` owns training, NOT newly invented `ILearningUnit.Train`. Map original four conceptual roles to existing code instead of renaming interfaces.
- `K/21.AgentInfrastructure/Logical/DynamicGraphEdgeFlow.cs`, `17.DynamicDomains/DynamicDomainGraph.cs`: graph flow and topology provide actual candidate seams, not proof that learned drift-aware policy exists. Follow real caller before creating one.
- Existing other potential owners: `06.VectorSearch`, `11.DPO`, `14.ContinuousLearning`, `25.ModelServer/ContinuousLearning`, `27.TranslationDomain`, `44.DesignConversionDomain` — **discover actual files/callers**; listing is not implementation evidence.

## RP and historical S-series ownership
- RP01/E1: exact Chinese-authored DPO pair, CODEX case selection/judgment, Student trainer/objective/first lesson.
- RP02/E2: domain mapper, semantic codebook, kNN multi-neighbor, learned k/radius and index; S17 supplies *law-version change guard* and consumes accepted ports.
- RP03: typed Unit many→many and Flow correction; S17 passes domain law version in actual typed association, not a second type system.
- RP04: bounded async Orchestrator and higher-order Graph route policy; S17 protects changed-law transitions, not another orchestrator.
- RP05/RP10: feedback/journal/full PC/cold promotion; S17 supplies compatibility checks without second checkpoint manager.
- RP06 + S08/S09: DTC/IDE host; RP07/S07: Builder; RP08: cross-domain transfer; RP09: independent exam; RP11: credit; RP12: causal repair.
- Historical `D/Kiro-Xiigen-Luna-S00-to-S02-Pack` and `S03–S10` remain relevant by original requirement/entrypoint, **not mandatory ten serial gates**. No assumption S11–S16 exist.
- Research **R17** owns evidence policy; this S17 does **not** disable CI or edit workflows. Review trigger is causality/native property, not test volume.

Every implemented change needs an exact delta receipt: source/ref/blob/span→original requirement→real caller/consumer→owned code/state→measure→status and one next owner/trigger. If actual caller or permission missing: `INTEGRATION_OPEN`, not a fabricated implementation.
