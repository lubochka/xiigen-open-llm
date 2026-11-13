# 09 · Ten operational refinements, repairs 37–46

**2026-10-15 · DRAFT · SOURCE/DESIGN EXTENSION · INDEPENDENT_ASTRA_NOT_PERFORMED.**

Continuation of [the published intermediate checkpoint](08-CONTINUATION-CHECKPOINT.md), not a restart of R01–R25 and not another product dispatcher. These ten refinements extend [repairs 17–36](05-ADDITIONAL-REPAIRS.md) and [work orders A–E](02-REVIEW-WORK-ORDERS.md). Select only the discriminator relevant to the next effect; equivalent current behavior returns `REUSE_NO_CHANGE`. Ten entries do not mean ten newly discovered product bugs or ten required runs.

The main distinctions are **a verified instruction problem**, **a source-supported conditional implementation risk**, **a correction already present in a newer slice and reused here**, and **a proposed examination detail**. Those categories must not collapse into a single DONE/FAILED count.

## Evidence and authority

Planning source snapshot: `b356120fdc35579366821dc21cd0317c2446d172`, the actual interim publication commit. Core snapshot: `97e4af3eadd3981320f698d104134a01984a2542`, independently resolved from the connected branch during this continuation.

**DPO roles remain exact:** CODEX with requested `astra 6 ultracode` performs delegated human case selection; an approved external open Chinese model authors BOTH final chosen and rejected; a separate CODEX invocation with requested `astra 6 ultracode` judges the exact unchanged pair. Actual model identity, requested identity and invocation receipts are separate. Student examination and independent review are separate effects. No such invocation occurred in this document task.

All examples below are public author illustrations, not Chinese-authored final DPO material, sealed examination items or product runtime results. Source-only/prompt/examination-design work adds **zero product tests**. Real changes to a deterministic integrity, state, type, permission or cancellation boundary retain their smallest relevant existing check; add a narrow check only for a demonstrated uncovered changed risk. No Windows coverage workflow is introduced.

### Sources actually examined

| Key | Source and bounded evidence |
|---|---|
| A | `02-REVIEW-WORK-ORDERS.md`, blob `6ea9992facb30f04ed9ee2b1064723f409208dba`: A–E, including their extensions and examples. These are authored instructions, not implementation receipts. |
| B | `05-ADDITIONAL-REPAIRS.md`, blob `0729eeb7d095dd01264475a00f5f4cddd791731d`: all items 17–36; `06-SPECIALIZED-REVIEW-PACKETS.md`, blob `2399af5d4ecf68248976f5cbf60e126e2a0705f4`: shared contract and SR1–SR6. |
| R | [RP02 stable-filename runbook](../Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/RP02-V7.2-LEARN-FIRST-RUNBOOK.md), blob `6d177e82549cf8cd9657681e6e9060517f524df5`: actual content is **revision 7.3**, not 7.2. Read the returned complete contract, equation, dynamic-search and evidence sections. Several refinements below reuse this existing correction. |
| W | [WorkLeft RESEARCH-03](../../09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md), blob `08c1cc7f92ba61ab05da521be516d105b366885f`: lines 1–110, especially current R16 exact-pair, no-repeat, state/verdict and functional-examination sections. Older commands were not executed. |
| F | [Arbiter/KNN primary amendment](../../../llm%20dynamic%20flow/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md), blob `0408bddbcd595f2023dec5001b52bac68af7b8c5`: opening through the priority/compiler distinction and start of required fields. Historical proof/thresholds are not new experiments or universal calibration. |
| X | [03 domain examination](03-DOMAIN-EXAMS-AND-FUNCTIONAL-CROSSWALK.md): mechanism matrix, law passport and C#/RU/SQL examples. The later full functional crosswalk was not freshly audited. |
| K1 | [KnnLogicalUnit.cs](https://github.com/lubochka/llm_mvp_core/blob/97e4af3eadd3981320f698d104134a01984a2542/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs), blob `2957816a88aa9cdcd3020bb81be56b5dfff165e0`: complete file, read in consecutive ranges. |
| K2 | [HnswIndex.cs](https://github.com/lubochka/llm_mvp_core/blob/97e4af3eadd3981320f698d104134a01984a2542/06.VectorSearch/HnswIndex.cs), blob `03287c05bcd6c4422d679a3b2e37a03f418918d6`: complete file, including Add, Search, persistence and private helpers. This closes the earlier atlas's explicit gap about this implementation's duplicate-ID storage; it does not prove production input frequency. |

The attached 1,481-line prompt is the requirement baseline. Its older answers and commit-title list remain historical leads. This continuation does not claim that every historical commit, RAG family, slice card or production caller was reviewed.

### Applied skill sections, not a whole-skill or pass-count claim

All paths below are under core `docs/skills` at the pinned K snapshot.

| Key | Skill, inspected section and actual application | Blob |
|---|---|---|
| ARCH | `trainable-architecture-decision-SKILL.md`, opening work-order completeness and deterministic/trainable authority; name signal, state, reader and falsifier. Historical 30/50-pass readiness rules are not imposed on this scoped reference repair. | `81e96fb8981d4eeb4a3f25f57001e74c453ffdbd` |
| PLAN | `phase-planning-SKILL.md`, Live Authority/Quoted Example and Implementation Evidence Map; current publication request is live, pasted runtime commands are not. | `27ba212c829148158511c7a5a69bc418abfc276c` |
| REVIEW | `plan-review-SKILL.md`, Candidate Binding and Work-Order Completeness; durable DRAFT is not independent acceptance. | `b99f4ce280f48d0242a35f2ba421a2dc1996399f` |
| CODE | `generated-code-review-SKILL.md`, Exact Final SHA and Code-First Claim Review; join the actual producer, index and lookup instead of deriving a bug from a manifest. | `eb27d6267115cc96b17a2cd77b601f84f2e6f0b6` |
| EXAMINE | `code-examination-SKILL.md`, Core Problem and protocol through Step 6; read complete targets and separate source hypothesis from test assertions. Step 5 applicability is repaired in item 37. | `5f6a9eb87686dfc67ef2eda0086c43e7dce47184` |
| PROMPT | `prompt-patch-authoring-SKILL.md`, Rules 0–2; literal observed problem and replacement, one actual bottleneck. Its generic score bands are not a ceiling on semantic correctness or requirement fidelity. | `063fe9ca26b2a7921279dd803db91ac4e2551c2c` |
| HUMAN | `conversation-intent-and-self-reflection-SKILL.md`, Purpose and Pre-Action Review; publish intermediate work, continue substantive analysis and distinguish existing versus freshly performed work. | `3cea7832d4a0e5d447f12f3f010d5fccd7f2759b` |
| SCIENCE | `experiment-design-SKILL.md`, Experiment Structure, Failure Classification and later rules; retain named comparisons, but reconcile historic replay, escalation and correctness claims with W/R. | `5987ff02ea7866c69c87b7efffa5658251419de8` |

## Ordered repair summary

| ID | Evidence classification | Owner / local trigger | Operational addition |
|---|---|---|---|
| 37 | Source-verified inherited-instruction risk | A/B/D; when loading the conflicting skill section | A contradicting test is evidence to investigate, not automatic scientific truth. |
| 38 | Overbroad generic instruction; corrected slice detail reused | B/C; when choosing the intervention | Freeze non-treated factors, not the very mechanism being examined; P is not a compulsory unsafe rerun. |
| 39 | Existing RP02 7.3 correction propagated | B/C; informative first update | Separate preference gradient from regularization-only or otherwise non-informative state change. |
| 40 | Source-supported conditional implementation risk | B; actual KnnLogicalUnit producer | Prevent one logical ID from joining an older indexed vector to a newer output record. |
| 41 | Source fact plus proposed experiment detail | B/C; learned-k or ANN economics | Separate k, paid probe information and search effort; do not assume ANN results are nested prefixes. |
| 42 | Proposed domain-law discrimination | B/C; next-token/action or representation claim | Distinguish prefix viability from final validity, and relations from token-count similarity. |
| 43 | Proposed examination accounting | C; quality, domain breadth or retrieval diagnosis | Declare denominators and locate the stage losing a lawful correct option. |
| 44 | Proposed work-reduction and validity rule | C/E; repeated bulk/examination work | Reuse compatible C0 evidence without hidden training replay or pretend-independent repeated holdouts. |
| 45 | Cross-contract vocabulary reconciliation | B/C/E; after a trial or no-update outcome | Map scientific observations to existing transaction actions, never invent promotion from a label. |
| 46 | Proposed typed/async and cost discriminator | B/C; many-to-many, orchestration or PC claim | Preserve association identity and count actual effects/work without rewarding output loss or double-counting shared work. |

---

## 37. Do not turn existing tests into an unquestionable learning oracle

**Extends 29 and work orders B/D. Source-verified instruction risk, not a finding that all tests are wrong.**

**Observed wording:** code-examination Step 5 says that when a test contradicts the reviewer's model, “your model is wrong” and it must be fixed first. Applied literally to this learning-first task, that pre-decides the result before consulting the requirement, law or production consumer.

**Replacement instruction:** treat the disagreement as a hypothesis. Compare the test assertion, implementation, exact active requirement and independent domain-law source. Classify `CODE_DEFECT`, `TEST_EXPECTATION_DEFECT`, `REVIEWER_MISREAD`, `STALE_CONTRACT` or `UNRESOLVED`; these are review labels, not new runtime enums. Preserve a still-valid deterministic safety invariant. Do not modify Student behavior, an exam rubric or a test expectation merely to obtain a green result.

**Steps/context:** use the selected B/D packet, the whole reached target and test only if relevant, the primary requirement span, and EXAMINE/CODE/ARCH. State the source-based prediction before looking at the assertion. Record a literal before/after only after identifying which contract is wrong. If no current test is implicated, do not create one.

**GOOD:** a test expects every low-confidence envelope to be a failure, while K1 returns `Ok` with a warning; the reviewer traces the caller and active acceptance requirement before deciding what is defective. **BAD:** the test is treated as proof that warning and rejection are identical. **EDGE:** a guard protects rights or once-only mutation; scientific uncertainty cannot waive it. An unresolved source conflict limits that change, not unrelated learning.

**Recovery/falsifier:** no correction is justified without a controlling requirement and affected consumer. Reuse valid checks; route a stale assertion to its owner. Apply PROMPT's specificity technique without importing its generic 0.80–0.90 band as an upper bound on correctness. **New product tests in this review: 0.**

## 38. Make the frozen-factor contract specific to the intervention

**Extends 21/24/35 and C. The generic C step 3 freezes the candidate pool and features without first naming the factor; R already supplies a more precise design.**

**Replacement:** first state treatment and estimand—the exact effect being measured. Freeze only non-treated factors. Any unavoidable coupled change is recorded as part of the intervention, not silently attributed to one parameter.

| Intervention | Keep fixed | May legitimately change |
|---|---|---|
| Theta ranker | Same query families, eligible candidate identities, realized feature values, representation/index, decoder and budget | Theta only, with its declared update state |
| k/search-effort policy | Same authorized corpus/index, representation, scorer and external workload/budget envelope | Probe/action sequence, search effort, returned/consumed subset, abstention policy within its contract |
| Exact versus ANN | Same eligible vectors, metric, requested task and downstream scorer | Retrieval implementation, effort and actual retrieved set |
| Representation | Same authorized raw examples and tasks; compatible downstream controls | Features/coordinates and the coherently rebuilt dependent index; report the coupled representation/index effect |
| Memory adaptation | Encoder/schema/scorer and evaluation policy | Authorized memory records/index contents; do not describe this as theta-only learning |
| Student DPO / Flow / E3 / E4 | The non-target recipients and source contracts | Only the explicitly named recipient and necessary transaction state |

P, the historical implementation, is useful engineering context only when safe and available. C0 versus T can answer the learning question without reviving unsafe historical execution. A unavailable P is `NOT_MEASURED`, not a universal blocker.

**GOOD:** compare learned-k with fixed-k over the same eligible index while measuring the different subsets and total effort. **BAD:** demand identical consumed neighbors while claiming to examine changing k; change corpus and theta together and credit theta alone. **EDGE:** a new representation necessarily requires reindexing; name the combined intervention instead of pretending the old index remains compatible.

**Recovery/falsifier:** split the causal question or downgrade attribution, not the law rubric. Sources A/R/K2; skills SCIENCE/ARCH/PLAN. Apply one row, not the entire table as a new experiment suite.

## 39. Prove that the pair can inform the selected recipient

**Extends 19/21. REUSE_FROM_ACTIVE_RP02_7.3, not a newly missing RP02 requirement.**

R defines `d = psi(q,z+) - psi(q,z-)`, `L = softplus(-theta·d) + lambda*||theta||²/2` and its bounded update. It already says that equal feature deltas and regularization-only movement are not preference-learning success.

**Operational detail:** record exact text integrity, usable transformed targets/features, data-gradient contribution, regularization contribution, clipping and actual numerical state change separately. A valid pair can be useless for this representation; a changed checkpoint can still contain no preference information.

**Public arithmetic illustration, not a product run:** with `d=(0,0)`, `theta0=(1,-1)`, `lambda=0.2`, `eta=0.1` and inactive clipping, the data gradient is zero while regularization gives `theta1=(0.98,-0.98)`. The state changes; the pair taught no distinguishing preference. This calculation is conditional on the declared objective/settings, not a universal optimizer prescription.

**GOOD:** a source-backed contrast survives tokenization/feature projection and contributes a finite recipient-appropriate signal. **BAD:** regularization, optimizer counters or a new checkpoint hash are counted as a successful lesson. **EDGE:** truncated DPO sides, identical rank features, nonfinite values, a tie, or an already optimal choice need different outcomes. Standard Student DPO uses its actual policy/reference and response targets, not this ranker formula.

**Recovery:** before an update, route the non-informative contrast to the representation/material owner; changed final text needs genuine Chinese authorship and a fresh exact-pair CODEX judgment. After an update, preserve its applied-once record even if utility is zero; no retry merely to force movement. Sources R/W/B; skills ARCH/SCIENCE/CODE. Neither a nonzero gradient nor a better margin proves held-out improvement.

## 40. Bind index identity to the same example version used for output

**Extends item 31's explicitly unverified HNSW question. Fresh bounded source join: K1.Train → K2.Add/Search → K1.ExecuteAsync.**

K1 trims each ExampleId, assigns `exampleMap[trimmedId] = example`, and calls `index.Add` for every row. K2.Add appends vectors and IDs to lists without logical-ID replacement. Search returns the external string ID; K1 subsequently reads `_examples[best.id]`.

**Source-supported conditional failure:** two rows with the same trimmed ID and different input/output payloads can leave two indexed vectors but only the last dictionary record. If search selects the older vector, the returned output/evidence comes from the later record. The nearest-vector score therefore need not describe the output actually returned. This is a source-level counterexample, not a measured production incident or proof that upstream callers supply duplicate IDs.

**Public walkthrough:** rows `case-7` and ` case-7 ` normalize to one ID. Let their distinct vectors be vA/vB and outputs A/B. The map holds B; the index holds both ID-bearing vectors. A hit on vA still resolves to B. `Manifest.TrainingExampleCount` reflects map count, not necessarily indexed vector count.

**Minimal proposed repair:** define the producer's logical identity contract before building this local index. Conflicting normalized IDs should be refused or explicitly versioned by the actual owner; identical repeats can only be reused under an authorized novelty/idempotency rule. Do not globally change shared HNSW Add into upsert without examining other consumers. Do not silently deduplicate text in a way that removes negation or code punctuation.

**GOOD:** one stable ID maps to one matching vector/payload/version and lawful source. **BAD:** a last-write dictionary is combined with append-only ID storage and called coherent training. **EDGE:** whitespace aliases, identical repeats, changed payloads, cross-tenant IDs and renewed corpus versions need explicit owner decisions.

**Verification/recovery:** inspect the reached producer and its upstream identity guarantee; a guaranteed-unique input contract can make this a defended local risk rather than a current production blocker. Only a future actual producer repair needs a minimal changed-risk identity check. Source review adds no tests and does not repair core. Skills CODE/EXAMINE/ARCH.

## 41. Learn search effort from information actually available at decision time

**Extends 24/30/35; R's two-stage search contract is reused and K2 supplies a concrete coupling.**

K2.Search calls `SearchLayer` with `Math.Max(_efSearch, topK)` and then takes topK results. K1 constructs the index with efSearch=32. Increasing topK past that setting also changes beam effort; reducing the returned count does not necessarily reduce traversal. K2 also uses random levels during construction, so independently rebuilding an index is not automatically a controlled no-change comparator.

**Instruction:** distinguish pre-query features from observations available only after a paid probe. Record `eligible_index_version`, requested action, probe/ef effort, candidates returned, candidates consumed, law-filter outcomes, decision time and total resource use. Do not let a no-search decision use neighbor margins it would have had to retrieve first. A diagnostic full candidate pool is not free production information.

**GOOD:** a bounded probe is charged, then density/ambiguity selects more effort or justified abstention. **BAD:** k=1 is announced as faster despite identical measured traversal; a policy chooses k using the uncharged full answer set. **EDGE:** ANN results at different k/ef need not be assumed nested prefixes. Freeze a realized index where appropriate; a rebuild is a declared additional factor.

**Recovery/falsifier:** compare the actual policy and retrieval path, not just result-list length. For a scorer-only study, use the same realized admissible pool; for search policy, expose changing opportunities and total cost. Preserve exact/ANN compatible-metric comparison. Law-dependent nonmetric scores can remain a reranker; R explicitly warns against silently using them as HNSW geometry. Skills SCIENCE/CODE/ARCH. Full learned-k remains separate from the first theta lesson.

## 42. Check prefix viability, final validity and relation preservation separately

**Extends 20/23/33. Proposed law-examination detail, anchored in the attachment, F/X/R and K1's actual representation.**

The attachment specifically asks whether the next word/token fits domain laws and syntax. An unfinished expression and an invalid completed answer are different objects. A final-answer checker cannot silently become a hard veto on every incomplete prefix.

**Instruction:** label the evaluated object: next token, partial AST/action prefix, completed typed candidate or final user answer. State which source-established conditions apply at that stage. Preserve `KNOWN_VIOLATION`, `VIABLE_PREFIX`, `COMPLETE_VALID` and `UNKNOWN` as examination concepts, not invented public enums. Viability is not final acceptance; uncertainty is not a license to bypass the arbiter.

K1.Vectorize adds token counts into modulo slots and predicate counts into another tier, then normalizes. With the same encoded-token multiset and predicates, this operation has no token-order signal. Different natural-language word orders must not be asserted to collide until the actual tokenizer output is inspected; BPE segmentation itself may differ.

**GOOD:** an expression-building prefix `s?.` is recognized as incomplete where continuation is expected, while completed `s?.Length ?? 0` is examined for its actual nullable meaning. RU agreement is attached to the same referent; an entity-ID renaming preserves the relation. **BAD:** all incomplete prefixes are trained as semantically wrong; a global feminine flag rejects `он` referring to `стол`; identical dimensions are called meaningful alignment.

**EDGE/recovery:** ambiguous parses retain hypotheses; an unavailable stage-specific checker becomes a bounded source/feature gap, not a demand for a universal parser before learning. Use a minimal runtime-computable relation feature, preserve version/units/missing masks, and compare paraphrase versus meaning-changing contrast. LoRA and semantic projection retain separate actual recipients/readers. Skills ARCH/EXAMINE/SCIENCE/PROMPT.

## 43. Make denominator changes and lost candidate opportunities visible

**Extends 22/24/28/32. Proposed accounting schema, not a new global benchmark quota.**

**Instruction:** before outcomes, define the enrolled workload and the denominator for each reported metric. Give every family/case a final outcome such as correct, wrong, abstained, timed out, rights-refused, unsupported, unresolved or not run. Diagnostic stage flags are separate; do not count them again as independent cases. Multiple losers of one source input are not independent families.

For a declared N-case workload, report correct/N, answer coverage, and correctness among answered cases separately. Also report the supported/authorized stratum when useful, without hiding the counts outside it. `UNSUPPORTED` is not a fabricated 0% solver score, but it still limits a claimed domain-coverage result. Critical-law violations and worst-domain retention remain visible.

**Public numerical illustration, not measured evidence:** C0 answers all 100 offered cases with 80 correct; T answers only 20 with 18 correct. Answered-case accuracy rises 80%→90%, while coverage falls 100%→20% and correct-per-offered-case falls 80%→18%. Calling this simply an accuracy improvement hides the trade-off.

For the actual domain, trace whether a lawful acceptable option exists in the authorized reference corpus, is retrieved, survives applicable law filtering, is ranked/selected, materializes as the same typed payload, and passes final arbitration. Missing option, false hard veto, bad ranking and wrong payload lookup have different owners.

**GOOD:** stage counts show retrieval opportunity was unchanged and selection improved. **BAD:** absent candidates are blamed on theta; leaked gold answers are inserted into serving to make opportunity perfect. **EDGE:** the oracle cannot determine whether an acceptable option exists; mark that stage unknown rather than inventing a denominator. Skills SCIENCE/ARCH/REVIEW; sources A/X/R. This is domain examination, not snapshot-string coverage.

## 44. Reuse compatible baselines without replaying lessons or contaminating confirmation

**Extends 28/34/35. Proposed work-reduction rule preserving W's no-repeat policy.**

**Instruction:** identify an examination by recipient/checkpoint, relevant code and input-transform versions, eligible corpus/index/candidate contract, panel/source-family version, evaluator/rubric/model identity, decoder/stochastic protocol and relevant hardware/cache/budget conditions. Reuse a genuine C0 observation when the fields relevant to the claimed outcome remain compatible. A documentation-only commit does not require fresh Student training or another full baseline run.

Distinguish three operations: reusing an already recorded score; reevaluating a model without optimizer effects; and training again. They have different permissions, costs and once-only semantics. Repeated inspection of a panel for checkpoint/hyperparameter selection makes it development evidence; it does not create a fresh blind confirmation each time.

**GOOD:** the same compatible C0 raw outputs support another predeclared comparison, with the shared observations acknowledged. **BAD:** the old pair is trained again to regenerate a baseline; changing teacher/judge version or candidate pool is hidden under the old receipt; repeated holdout peeks are counted as independent trials. **EDGE:** quality evidence may remain reusable while latency evidence is not, because hardware/load/cache conditions changed.

**Recovery:** invalidate only the affected measurement or claim. For a genuine final confirmation, preserve an untouched family panel under the current owner. Multiple comparisons and stopping rules are declared; a larger sample is not automatic correctness. Novel retention evaluation is not hidden replay training. Sources W/A/R; skills PLAN/SCIENCE/REVIEW. No universal sample size, replay ratio or Windows rerun is imposed.

## 45. Map observations to existing state actions before a trial can be promoted

**Extends 21/27/34/36. A/W use related but non-identical verdict vocabularies; reconcile at the boundary.**

W's current R16 section specifies IMPROVE, REGRESS, INCONCLUSIVE and INVALID_EVIDENCE. A also uses NO_GAIN as a scientific description. This does not authorize inventing a new transaction enum or a default success branch.

| Observation | Required interpretation/action under the current owner contract |
|---|---|
| Invalid or unjudged exact pair before update | No unauthorized update; material/rights/judgment recovery. Not a measured Student regression. |
| No informative recipient signal | Diagnose the representation/material boundary; preserve any already-applied once record. Not a reason to replay. |
| NO_GAIN / equal or uncertain quality | Preserve the accepted parent; use the existing no-promotion/inconclusive route, recording the more specific observation separately. |
| REGRESS | Preserve/restore the coherent parent through the actual transaction owner. |
| INVALID_EVIDENCE after an update | Preserve the safe accepted state; repair the measurement. The update did not become unapplied merely because evidence failed. |
| IMPROVE | Eligible for the authorized acceptance decision only after applicable integrity/state/rights checks; not automatic promotion on save. |
| Cold isolated trial loaded | Demonstrates a trial reader, not accepted-state promotion or causal improvement. |
| Accepted pointer published and ordinary cold reader observed | Adoption evidence for that exact coherent version, not a retrospective proof of all domains. |

**GOOD:** scientific NO_GAIN is retained without promoting a challenger or fabricating another update. **BAD:** an unfamiliar verdict falls through to ACCEPT; a trial checkpoint overwrites the serving parent before examination. **EDGE:** a crash has unknown mutation status; journal reconciliation precedes any retry.

**Recovery/falsifier:** name the actual state owner and concrete mapping, or return a bounded unresolved seam. These are planning interpretations, not verified enum/method names for every runtime. Do not claim all coordinator/trainer/rollback paths audited; this continuation did not reread those files. Skills ARCH/PLAN/CODE; sources W/A/R.

## 46. Attribute many-to-many, asynchronous effects and PC cost to the same served result

**Extends 25/26/35. Proposed operational trace for one actual Unit/Flow/E3/E4 consumer, not a new framework.**

**Instruction:** retain request/input identity, output identity, association/port, producing state/route, invocation/effect ID, completion/cancellation status and materialization snapshot. Record which associations the next consumer actually read. A query-like interface name alone does not prove lazy execution, stable ordering or once-only enumeration; inspect the reached implementation.

For learning, distinguish observed semantic failure from timeout, cancellation and an unexecuted counterfactual. A completed law-correct answer may be unchanged while the learned route uses less work. Conversely, dropping requested outputs can make execution faster without making the algorithm better.

Count wall latency on the user path and resource work across actual unique invocations separately. Parallel branch durations are not simply added to obtain wall latency. Shared subwork is charged once in the total ledger, with allocation rules for component views; retries and distinct executions remain distinct costs. Include already-spent cancelled work, cold load, base/adapter/index/optimizer/cache/rollback storage and external author/judge calls. Local Student feasibility does not establish an offline end-to-end loop.

**GOOD:** the same complete law-correct typed result and associations arrive faster at no hidden quality/coverage loss, with every executed branch accounted for. **BAD:** one output repeated N times is counted as many-to-many; cancelled branches are invented as semantic losers; overlapping branch durations and shared work inflate or hide cost. **EDGE:** late completion after cancellation, duplicated feedback delivery and multiple enumerations are separate observed effects, not automatically identical lessons.

**Recovery/falsifier:** restore association/custody evidence or correct the cost attribution before claiming a learned gain. Hold Student, Flow, kNN and graph neighbors fixed according to the selected factor; never claim Student weights are Flow W. A trace/algorithm gap goes to its actual owner; an actual side-effect/cancellation repair retains its narrow safety check. Sources attachment/A/B/X/F; skills ARCH/CODE/SCIENCE. No fresh RP03/RP04 runtime/caller audit is claimed here.

---

## Complete bounded example: issue the identity trace, not the whole catalogue

The following is an authored example of one complete read-only work order. It has not been sent to or completed by an independent model. The issuer must reconcile changed relevant blobs before use on a later head.

```text
ASSIGNMENT: ATLAS-40 / B / KnnLogicalUnit local index-to-output identity.
CURRENT TASK: verify the conditional duplicate-ID join and return one
  producer-scoped repair proposal or REUSE_NO_CHANGE with source evidence.
NOT THE TASK: repair core, run training, invoke models, change HNSW globally,
  dispatch all RP slices, rerun R01-R25, or create product tests.
SNAPSHOTS: P=b356120fdc35579366821dc21cd0317c2446d172;
  K=97e4af3eadd3981320f698d104134a01984a2542.
READ:
  K/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs, whole file;
  K/06.VectorSearch/HnswIndex.cs, whole file;
  current RP02 runbook, computation/first-learner/identity sections;
  only the actual constructing caller needed to establish whether producer
  inputs are guaranteed unique. If not found, CALLER_UNVERIFIED, not invented.
SKILLS:
  generated-code-review / Code-First Claim Review;
  code-examination / complete file, upstream/downstream, hypothesis first;
  trainable-architecture-decision / state and consumer ownership;
  phase-planning / current authority versus quoted evidence.
  A test contradiction is investigated against the governing source, not
  automatically treated as proof that either implementation or test is right.
ALLOWED: connected repository reads, source reasoning, an output proposal.
WRITES/MODEL/RUNTIME EFFECTS: none. No external calls, optimizer, checkpoint,
  accepted-pointer, corpus or sibling-registry changes.

STEPS:
1. Follow Train's trimmedId through dictionary assignment and index.Add.
2. Follow Add storage and Search's internal-index-to-external-ID conversion.
3. Follow ExecuteAsync's _examples[best.id] to output/evidence/manifest count.
4. Walk two same-trimmed-ID rows with different vectors and payloads.
   Explain what happens IF the older vector is returned; do not assert a
   measured nearest-neighbor result without executing an authorized run.
5. Locate the actual uniqueness guarantee, or state exactly what is unknown.
6. Compare local reject/version/reuse choices against the current input and
   once/novelty contract. Propose the smallest producer-owned change only.
7. Name the future minimal identity risk check if code is later changed;
   do not add or run it in this source assignment.

GOOD: one ID/version consistently refers to matching vector and payload.
BAD: a last-write dictionary is joined to older append-only vector records.
EDGE: whitespace alias; identical lawful repeat; conflicting payload;
  cross-tenant identity; upstream guaranteed uniqueness; absent caller.
RECOVERY: retain the current safe parent/state, return the specific identity
  guarantee or source gap; do not invent a global deduplication policy.
FALSIFIER: if actual reached inputs are unique by an enforced contract,
  downgrade the production-risk claim; the local source behavior still stands.
RETURN: exact files/blobs/methods; source fact versus conditional inference;
  two-row trace; actual caller or gap; literal proposed instruction;
  preserved novelty/rights obligations; minimal diff owner; GOOD/BAD/EDGE;
  no-change/repair reason; one next permitted owner; SOURCE_ONLY;
  runtime NOT_RUN; independent Astra NOT_PERFORMED.
```

## Continuation outcome and remaining boundaries

C1–C8 in the interim checkpoint are addressed through the evidence classification, items 37–46 and the complete example above. They were refinement questions, not eight bugs to mark fixed. Existing provisions were reused, especially the current RP02 7.3 equation, paid-probe distinction, safe optional P and isolated trial/promotion boundary.

The requested work-order file should incorporate the applicable wording repairs and route to this detail; publication of this file alone does not claim that integration happened. The separate continuation receipt records the actual changed files, commits and remote readback.

`PLANNING_DRAFT; SOURCE_REVIEW_BOUNDED; CORE_UNCHANGED; DATA_AND_WEIGHTS_UNCHANGED; GLOBAL_SKILLS_UNCHANGED; NO_NEW_PRODUCT_TESTS; TRAINING_NOT_RUN; DOMAIN_EXPERIMENTS_NOT_RUN; PC_NOT_MEASURED; INDEPENDENT_ASTRA_NOT_PERFORMED`.
