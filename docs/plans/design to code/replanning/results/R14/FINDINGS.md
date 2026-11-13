# R14 — capability-first reconstruction of the whole program

## CURRENT REPAIR V2 — 2026-10-13

This section supersedes the earlier linear reading and stale R13 status in this file.

### R14-FIX-01 — R13 is present and changes R14 materially
Current `results/R13` is observed. It defines four falsifiable experiment families: E1 DPO/update/adoption, E2 domain/representation/dynamic-kNN, E3 Flow/Orchestrator relation learning with Units frozen, and E4 DynamicGraph compositional transfer. Therefore the earlier R14 statement that R13 was not observed is obsolete.

### R14-FIX-02 — M0→M7 is not an execution sequence
The previous fixed priority `M0 first DPO lesson → M1 second domain → M2 domain/vector/kNN …` was too restrictive and could recreate the same planning failure under a new naming scheme. The current plan is a **capability DAG**. The first experiment is the smallest causally valid E1–E4 candidate with the best expected information gain / implementation cost.

### R14-FIX-03 — current R04–R06 evidence changes candidate selection
- R04 shows a real existing kNN/BPE/HNSW baseline and requires oracle-domain vs predicted-domain vs controlled-perturbation contrasts; representation changes must preserve law-sensitive distinctions and keep solver/candidate pool/k fixed when measuring the representation factor.
- R05/R04 handoff identifies the exact-pair → token/mask → recipient/objective → parameters → ordinary-consumer seam as the bounded DPO uncertainty. Pair validity alone is not enough.
- R06 shows existing exact-search/BM25/HNSW/domain-classifier/reusable graph seams, fixed-at-train k in `KnnLogicalUnit`, first-hit output behavior, and unactivated DynamicGraphMachine training lifecycle. Dynamic-k is therefore not “change k and test”; it needs a learned state/signal and a consumer where multiple neighbours can causally affect the decision.

### R14-FIX-04 — candidate queue
| Candidate | Main question | Reuse | Smallest missing seam | Main invalid-evidence risk | Current ranking rule |
|---|---|---|---|---|---|
| E1 DPO/update/adoption | Does one exact assessed pair improve the intended Student capability? | Existing DPO coordinator/trainer/checkpoint path | exact semantic recipient + response-conditioned token/objective + ordinary consumer | judged bytes differ from optimized bytes; wrong recipient; consumer on old state | select when R12-F binds the recipient and this is the highest-value unresolved question |
| E2 domain/vector/dynamic-kNN | Does domain-aware representation/retrieval improve lawful neighbour/path choice? | existing BPE/HNSW, exact search, BM25, DomainClassifier | one domain-law-preserving representation or learned rerank/dynamic-budget factor with real neighbour consumer | changing representation+k+metric+solver together; oracle leakage | may run before E1 when representation/retrieval uncertainty would confound DPO |
| E3 Flow/Orchestrator | Can learned relation/path state improve valid composition with Units frozen? | current flow/orchestrator contracts and runtime seams | exact learned relation/path owner and ordinary execution consumer | solver quality changes simultaneously | select when relation/path authority is the highest-impact uncertainty |
| E4 DynamicGraph | Can learned graph state transfer to unseen lawful higher-order composition? | graph topology/runtime seams | activate/bind one trainable graph state without inventing a second trainer | memorized path presented as transfer | select after a bounded compositional panel and update owner are available; need not wait for full curriculum |

R14 does **not** declare a universal winner before R12-F and concrete lawful panels are bound. It declares the ranking method and removes the false E1-first dependency.

### R14-FIX-05 — cross-domain replication is a claim gate
A structurally different domain/source family is required before a broad transfer claim, but it is not a universal prerequisite for investigating E2/E3/E4. Domain passports must expose laws, near misses, ambiguous/multi-correct cases, unknown/mixed-domain cases, token/vector distinctions and expected retrieval behavior.

### R14-FIX-06 — model-ready smallest packet requirement
The first selected packet must include exact prompt, context, skills, RAG route, primary references, current code seam, good/bad examples, edge cases, frozen factors, one changed factor, lawful panel, functional examination, ordinary-consumer adoption, time/accuracy/retention/full-PC metrics, branch rules and allowed/forbidden scope. No coverage target may substitute for a learning result.

### R14-FIX-07 — DPO role contract
CODEX with GPT-6 Astra UltraCode performs the human lesson/case selection. The external open Chinese model authors both final chosen and rejected. CODEX with GPT-6 Astra UltraCode separately judges the exact final projected pair. Student learning and post-update evaluation remain distinct roles.


**Date:** 2026-10-13  
**Status:** R14 ANALYSIS PUBLISHED / BOUNDED BY UPSTREAM EXACT CHOICES / LEARNING NOT MEASURED

## 1. Human conclusion

The current program should no longer use “finish the next slice and its test/Windows gates” as the primary unit of progress. The new unit is:

> **open one valuable learning question, run the smallest causally valid intervention, observe ordinary behavior, and use the result to choose the next algorithmic seam.**

This does **not** delete product correctness, deterministic safeguards, DTC, IDE, native/host delivery, or the full Unit→Flow→Orchestrator→DynamicGraph architecture. It changes when they enter the critical path.

The shortest whole-program order is:

1. **M0 — first valid useful lesson and ordinary adoption**
2. **M1 — second structurally different domain**
3. **M2 — domain-aware mapping + representation + retrieval**
4. **M3 — learned Flow correction**
5. **M4 — async Orchestrator path selection**
6. **M5 — DynamicGraph higher-order transfer**
7. **M6 — compact PC continual learning + user feedback**
8. **M7 — DTC/IDE/native/host delivery completion**

The order is causal, not slice-number-based. S00–S10 remain traceable to these capabilities.

## 2. Upstream status boundary

R14 did not invent missing inputs.

- R11 provides a usable first-lesson vs full-delivery causal split.
- R12 is currently **REEXECUTION_REQUIRED**. Its historical CP1 is bounded candidate evidence, not final hybrid selection.
- No published `results/R13` was observed during this R14 execution.

Therefore R14 distinguishes:
- **stable queue decisions** that do not depend on the exact R12 winner;
- **provisional seam fields** that wait for the exact R12/R13 recipe.

This missing upstream precision does **not** block whole-program capability migration.

## 3. Non-negotiable DPO role contract

For every future DPO milestone:

1. **CODEX using GPT-6 Astra UltraCode performs the human lesson/case selection.**
2. **The external open Chinese model writes BOTH final DPO sides: chosen and rejected.**
3. **CODEX using GPT-6 Astra UltraCode separately evaluates the exact final projected pair.**
4. The Student receives only admitted material.
5. Post-update Student examination is separate from pair judgment.

The Chinese model does not choose the preferred side on behalf of the human role and does not certify its own pair.

## 4. Capability milestones

### M0 — first valid useful lesson

**Purpose:** prove that one lawful learning intervention changes the state actually used by an ordinary consumer and produces interpretable functional evidence.

**Reuse first:** current DPO/learner/update/checkpoint infrastructure traced by R05/R11, unless final R12 disproves its suitability.

**Minimum prerequisites:**
- exact assessed training material reaches optimizer without semantic mutation;
- explicit objective/recipient/state owner;
- one bounded mutation with provenance/no-repeat/rollback;
- ordinary consumer uses accepted state;
- baseline/challenger/no-update comparison on the same panel;
- functional learning verdict: IMPROVE / REGRESS / INCONCLUSIVE / INVALID_EVIDENCE.

**Not prerequisites by default:** full Windows qualification, full graph, all domains, every ANN variant, complete IDE or full curriculum.

**Current exact seam:** PROVISIONAL_PENDING_R12_R13. R05 exact-pair/objective/adoption remains strong E1 candidate but not automatic winner.

### M1 — second structurally different domain

**Purpose:** prevent the first gain from being mistaken for general learning.

The second domain must differ structurally, not only by paraphrase/source-family. Examples of meaningful structural contrast include:
- natural-language translation vs SQL semantics;
- code/documentation vs orchestration path selection;
- visual/layout relation vs textual explanation.

**Observation:** same discipline of held-out families, hard-law metrics, no-update control, retention and resource accounting.

### M2 — domain-aware mapping + representation + retrieval

This milestone makes explicit what earlier plans tended to defer.

Required axes:
- predicted domain vs oracle domain;
- unknown/mixed/multilabel handling;
- raw → canonical → token → feature → vector → index identities;
- domain-law distinctions that must survive representation;
- lawful candidate filtering;
- positive/negative/warning memory;
- exact-search reference;
- current fixed/first-hit/BM25+kNN baseline;
- learned reranking/aggregation;
- bounded domain-sensitive dynamic-k or radius hypothesis;
- ANN compatibility separately from exact semantics.

**Key rule:** increasing k or changing a threshold manually is not “learning”. Learned authority requires learned state, a signal/objective, mutation, consumer and falsifier.

### M3 — learned Flow correction

Freeze Units where possible and change only relations/correction between them.

Required observation:
- same Unit capabilities;
- changed Flow state only;
- better input→output correction or relation selection;
- ordinary consumer actually reads the learned Flow;
- retention of previously correct Unit behavior.

A document-model LoRA gain does not prove Flow learning.

### M4 — asynchronous Orchestrator path selection

Measure router/path quality independently from solver quality.

Include:
- dependency order;
- branch admission/cancellation;
- cost of failed branches;
- selected path vs available lawful alternatives;
- same solvers with different learned routing when possible.

A better solver result cannot be relabeled as better orchestration.

### M5 — DynamicGraph higher-order transfer

Use known lower-level components and test unseen lawful compositions.

Required contrast:
- lower-level Units/Flow behavior held fixed where practical;
- new higher-order composition/pattern relation;
- graph-selected or graph-corrected path vs fixed baseline;
- explicit ordinary consumer and rollback.

DynamicGraph is not postponed indefinitely, but it is also not a prerequisite for M0 unless the selected first lesson actually targets graph learning.

### M6 — compact PC continual learning + user feedback

This milestone validates the project’s “learn on PC and continue from user input” objective.

Measure the full system:
- base/reference model;
- adapters/LoRA;
- optimizer/activations;
- tokenizer/encoder/codebook;
- vector index and graph;
- caches;
- snapshots/rollback state;
- failed attempts/reindex;
- evaluation overhead.

Feedback lifecycle:
user input → provenance/trust check → lesson candidate → CODEX/Astra human selection → external Chinese chosen/rejected authoring if DPO is used → CODEX/Astra exact pair judgment → isolated update → ordinary-consumer examination → promote/rollback → retention.

User feedback is not automatically truth.

### M7 — DTC / IDE / native / host delivery completion

All delivery obligations remain.

Examples:
- Design-to-Code behaviors;
- Visual Studio / VS Code / Continue or other confirmed clients;
- Windows/native process/security/resource behavior;
- release packaging;
- host-specific compatibility.

These are capability milestones, not universal prerequisites of unrelated learning experiments.

## 5. Combined deterministic + learned algorithm matrix

| Mechanism | Deterministic law/helper | Learned state | Signal/objective | Ordinary consumer | Falsifier | Milestone |
|---|---|---|---|---|---|---|
| Domain mapper | schema/language/stack admissibility, hard domain rules | classifier/router state | mapping error + downstream utility | domain-specific representation/path selector | oracle-domain works, predicted-domain fails | M2 |
| Token/canonical representation | syntax/Unicode/structural preservation | optional learned projection parameters | preserve law-sensitive distinctions | DPO/kNN/solver consumer | semantically different cases collapse | M0/M2 |
| Vector/projection | shape/version/index compatibility | learned projection/adapter | domain/task loss or preference signal | retrieval/router/solver | same shape but wrong coordinates | M2/M6 |
| kNN candidate/retrieval | lawful pool, exact reference, hard filters | reranker/aggregation/dynamic budget state | neighbor/path utility | solver/router | first-hit/fixed baseline wins under same pool | M2 |
| DPO builder/assessment | rights, exact identity, no-repeat, pair schema | Student policy/adapters | chosen vs rejected preference | ordinary Student inference | pair mutates or update not consumed | M0 |
| LoRA | frozen base + versioned adapter boundaries | low-rank trainable parameters | explicit task/preference loss | named inference consumer | weights change but behavior does not | M0/M2/M6 |
| ILearningUnit | typed domain/input/output contracts | Unit learner state if applicable | unit-local task signal | unit caller | correct result comes only from fixed helper | M0/M2 |
| ILearningFlow | valid Unit compatibility | relation/correction state | transition/path correction | flow consumer | fixed Flow matches learned Flow | M3 |
| ILearningOrchestrator | dependency/admissibility graph | path-selection state | end-to-end utility/cost | orchestration runtime | gain disappears with same route | M4 |
| ILearningDynamicGraph | legal pattern/composition constraints | higher-order relation state | transfer/composition utility | graph-aware path consumer | no transfer to unseen composition | M5 |
| Runtime arbiter | hard safety/domain laws | optional learned ranking input, not final law override | acceptance utility | final runtime path | learned rank violates hard law | M2–M6 |
| Feedback admission | provenance/trust/conflict rules | adaptation state after accepted lesson | post-feedback utility/retention | ordinary consumer | correction causes regression or is untrusted | M6 |

## 6. Learning-first branching queue

### If M0 = IMPROVE
Do not immediately broaden all algorithms. First:
1. reproduce on another independent source family;
2. run M1 on a structurally different domain;
3. then choose whether the limiting factor is domain mapping, representation, retrieval, Flow, routing or higher-order graph transfer.

### If M0 = REGRESS
Return to the exact changed factor:
- pair quality,
- semantic mutation during preparation,
- wrong objective/recipient,
- state not consumed,
- representation collapse,
- unsuitable retrieval/path authority.

Do not restart the entire replanning program.

### If M0 = INCONCLUSIVE
Increase information quality only within the declared budget:
- more independent families,
- stronger hard negatives,
- better uncertainty estimate,
- clearer same-panel control.

Do not change the rubric after seeing the result.

### If M0 = INVALID_EVIDENCE
Repair the validity break first:
- leakage;
- changed pair after judgment;
- incompatible baseline/challenger;
- ordinary consumer not using new state;
- hidden replay;
- missing state identity.

No learning claim is accepted from invalid evidence.

## 7. Old program → capability migration

### S00–S03
Primary disposition: **REUSE / KEEP DETERMINISTIC FOUNDATIONS / SPLIT FROM LEARNING PROOF**.

Representation, contracts and deterministic infrastructure that are already valid remain foundations. They are not repeated merely to increase coverage. Any representation seam that destroys domain-law meaning moves into M0/M2.

### S04–S06
Primary disposition: **REUSE EXISTING LEARNER; CHANGE SMALLEST LEARNING SEAM; MOVE RELEASE/NATIVE GATES LATER**.

DPO/update/LoRA/checkpoint work maps chiefly to M0/M2/M6. Windows/native checks remain under M7 unless the selected learner path actually crosses them.

### S07–S10
Primary disposition: **DOMAIN/RETRIEVAL/GRAPH CAPABILITIES MOVE INTO M2–M5**, not “later optimization forever”.

Domain mapping, kNN, path/graph logic and downstream contracts become explicit causal experiments.

### WorkLeft
Disposition: **BASELINE AUTHORITY / REQUIREMENT SOURCE**.

Existing training/DPO research is not discarded. Each relevant WorkLeft item maps to a capability owner and trigger. Historical conflicts such as replay vs current no-repeat/new-lesson bulk semantics must be resolved by authority, not silently mixed.

### RAG/index families
Disposition: **SOURCE ROUTING, NOT LEARNING EVIDENCE**.

RAG is used to trace requirement → primary source → current mechanism. Presence in an index does not prove implementation or gain.

### DTC / IDE / clients
Disposition: **KEEP / DEFER TO CAPABILITY DELIVERY TRIGGERS**, never silently remove.

### Windows/native
Disposition: **KEEP at M7 or earlier only on proven native dependency**.

## 8. What R14 explicitly removes from the critical path

R14 removes only the assumption that these are universal prerequisites:
- full Windows/Actions validation;
- high coverage;
- all-domain curriculum;
- complete graph rollout;
- every host/client;
- every ANN configuration;
- broad platform polish.

They remain later obligations where relevant.

## 9. Smallest ready future work packet

Current packet is intentionally partly provisional because final R12/R13 exact selection is not published.

**Capability:** M0 first valid useful lesson  
**Source obligation:** R11 first-lesson DAG + R05 candidate seam + R08/R09 learning-evidence rules  
**Current implementation owner:** existing learner/DPO path traced by R05, subject to final R12 confirmation  
**Exact code seam:** PROVISIONAL_PENDING_R12_FINAL  
**Current input/output:** exact pair → prepared representation → objective → state update → accepted state  
**Target:** exact assessed pair remains semantically faithful; explicit response-conditioned recipient/objective; accepted state reaches ordinary consumer  
**Deterministic guards:** provenance, rights, pair identity, no-repeat, state identity, rollback  
**Learned state:** PROVISIONAL_PENDING_R12  
**Functional experiment:** PROVISIONAL_PENDING_R13; must be one E1–E4 recipe with same-panel baseline/challenger/no-update control  
**Role sequence if DPO:** CODEX/Astra lesson selection → external Chinese both sides → CODEX/Astra exact pair judgment → Student update → independent Student examination  
**Decision branches:** IMPROVE / REGRESS / INCONCLUSIVE / INVALID_EVIDENCE  
**Resource observation:** learning time, inference p50/p95, peak RAM/VRAM/disk, persistent footprint  
**Rollback:** previous accepted state remains recoverable  
**Files allowed/forbidden:** to be filled by R18 from final R12 owner trace; no second trainer by default

This is enough for R18 to know the form of the packet, but not enough to claim implementation readiness until exact R12/R13 fields are resolved.

## 10. R14 verdict

**PASS after repair for whole-program capability-DAG reordering.**  
**NOT FINAL for the exact first implementation seam.**

Stable conclusions:
- learning-first capability order;
- preserved delivery obligations;
- explicit hybrid algorithm sequence;
- explicit domain/vector/kNN/LoRA milestones;
- tests/native gates separated from learning efficacy;
- DPO role split locked.

Still dependent:
- exact R12 hybrid design winner;
- exact R13 first recipe;
- exact R18 code file/method packet.

All learning gains, best-k claims, LoRA utility and PC-fit remain **NOT_MEASURED**.
