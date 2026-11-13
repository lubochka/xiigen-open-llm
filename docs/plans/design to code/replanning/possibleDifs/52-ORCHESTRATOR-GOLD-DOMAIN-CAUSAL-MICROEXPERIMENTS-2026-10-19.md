# GOLD-OR — быстрые причинные микропробы, возвращающие именно способности orchestrator-extensions

**Тип:** EXECUTOR-READY PLAN; реализация/эксперименты на .NET **NOT_RUN**. Основа: [глава Golden](00-ORCHESTRATOR-EXTENSIONS-GOLD-RECOVERY-FIRST-2026-10-19.md) и [точный source audit](52-ORCHESTRATION-REASONING-CHECKPOINT-AND-ADMISSION-GAP-2026-10-19.md). Фиксировать HEAD SHA на момент исполнения, не считать текущее историческое наблюдение вечным.

## Общий протокол для каждого опыта

1. **One question / one toggled factor:** одинаковые input, source snapshot, holdout, domain, judge, exact consumer version там, где алгоритм допускает; менять ровно один механизм в паре arms. Distinct codebases GOLD vs CURRENT — это regression-parity, **не** чистый single-factor causal A/B; для причинности нужен внутренний current B→C.
2. Зафиксировать hashes исходного TRAIN/holdout, независимость семей/источников (не только строк), workflow owner, domain tag, direction, recipient Unit/Flow/Orchestrator, checkpoint+proof SHA, tokenizer+encoder/index versions, kNN features, source rights.
3. В логе разделять training update (weights/patterns changed), validation score, cold restore, *actual ordinary consumer*, heldout usefulness, latency и memory. Если обновился только manifest, score или fallback — **NO_LEARNING_CLAIM**.
4. Human independent adjudication: rubric заморожен до results; минимум один контроль с порчей паттерна/удалением learned state. Отдельно выписывать false-positive/false-negative и цену ошибочного принятия.
5. 0 обязательных Windows CI / broad coverage gates. Единственный native smoke / narrow regression path если затронут настоящий loader/decoder invariant. Source-only/proxy результаты = PROXY_ONLY.

## GOLD-OR-A0 — P0: реальное восстановление 15 learned orchestration patterns

**Hypothesis:** v2-current lost old learned guide reachability due to explicit v1 provenance rejection; authorized v2 provenance rebuild restores accepted scenarios and successful *ordinary* reasoning decisions.

**Source:** Gold [Machine](https://github.com/lubochka/llm_mvp_core/blob/orchestrator-extensions/43.SmartSkillsDomain/OrchestrationReasoningMachine.cs), [named checkpoint](https://github.com/lubochka/llm_mvp_core/blob/orchestrator-extensions/models/orchestration-reasoning/orchestration-reasoning-v1/orchestration-reasoning-v1-checkpoint.json), [manifest](https://github.com/lubochka/llm_mvp_core/blob/orchestrator-extensions/models/orchestration-reasoning/orchestration-reasoning-v1/manifest.json) and [current loader](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/43.SmartSkillsDomain/OrchestrationReasoningMachine.cs#L699-L755).

**Input/output types:** input: OrchestrationReasoningRequest with task type, requirement and constraint signals, prompt/context/risks; output: LoadResult + OrchestrationReasoningResponse {ContractStatus, ScenarioPlan, SelectedLearnedPatterns, RiskFlags, NextGranularizationActions} + API warnings + persisted receipt. Do not substitute only an enum for the actual request body.

**4 arms, in strict order:**
- A — Gold v1 loader + gold manifest/named checkpoint, **cold separate process**. If gold A does not restore/checkpoint hash disagrees, STOP: cannot claim a valid success benchmark until original source is reconciled.
- B — Current v2 loader + exact same v1 files, expected fail with provenance rebuild error; current ordinary API should record asset_loaded=false and **not** pretend trained output.
- C — Current v2 loader + **legitimately reconstructed** v2 checkpoint/manifest/proof bundle, original source authority, per-pattern provenance identity, no weakening admission/security checks. Same exact 15 pattern IDs and 11 accepted references if supported by original lawful source material. If not reconstructable, report BLOCKED_BY_PROVENANCE, not fabricated PASS.
- D — Current machine without training state (negative control). Compare B/C/D actual selectedPattern and answer, not just loader return codes.

**12 microcases:** derive from 15 archived patterns rather than invent a disconnected task. After reading source and de-duplicating families, choose: four already-seen structural signal combinations as smoke; four **held-out paraphrase/family-disjoint** task sets requiring risk/authority gates; four abstention/contradiction probes with missing prerequisite, contradictory requirements and unseen domain. Freeze before C migration. For each case store input source ref, expected *outcome family* and negative controls; avoid exact-text memorization. Use structured not natural-language-only matching as contract requires.

**Acceptance for pilot (exploratory, NOT production):** A reconstructs baseline; B is appropriately rejected; C loads with proof and restores 15 IDs/11 accepted refs **and** matches A's lawful selected scenarios on smoke; on 8 heldout/negative cases C has at least two more independently correct decisions than B/D, without increasing unsafe accepts or hiding abstentions. If difference <2/8 or C fails admission, mark INCONCLUSIVE/FAIL, not PASS. No statistical significance claim from eight cases. Do not cherry-pick golden test items after looking at outputs. Record exact positive/negative counts and raw answer links.

**Failure example:** C loads a four-pattern checkpoint with green hash; AllLoaded=true but previously supported scenario missing. **Good example:** C returns same core dependency/risk boundaries as A on unseen rewording, while D abstains, proof valid, and no model-index override.

**Edges:** v1 manifest vs v2 serializer, duplicate accepted refs, lost continued pattern, source manifest hash changes, nonportable absolute path, proof outside bundle, user adaptive cache shadowing the common asset, manifest points to named 15-state while fallback picks four-state, cache warmed by earlier arm. Control by fresh process+unique asset root+immutable content hashes.

## GOLD-OR-A1 — P0: E53-like graph/operator ability, but honest source scope

**Hypothesis:** graph dependency composition, knowledge-edge abstention and negative-control decisions can regress even if neural DPO preference score improves; restore graph mechanisms separately from learned-guide migration.

**Donor:** [E53 source receipt](https://github.com/lubochka/llm_mvp_core/blob/orchestrator-extensions/experiments-v2/E53-HIGH-LEVEL-ORCHESTRATION-PROOF/results/run-2026-05-04-131603.md): 11→11 nodes, 0 lost edges, 0 dependency violations, 4 learning-gap packets, 0 external calls. This is **deterministic orchestration**, not supervised Student learning.

**Inputs:** 6 compact unseen dependency DAGs (three domain flavours × two variations) with explicit source references: SaaS onboarding (provider trust/account merge/terms), Design-to-Code (design evidence→component schema→UI/server DTO→QA), code review (source→dependencies→change→test→release). Each includes one deliberately missing authority/unknown and one forbidden shortcut. Perturb semantic order, duplicate nodes and input order without modifying dependency truth.

**Arms:** A gold graph policy; B current graph policy under equivalent inputs; C current policy with historical graph edge/tier feature reinstated (one at a time); D current graph with that feature disabled. Compare graph edges before/after dynamic closure and final human result. No external provider calls.

**Measure:** valid edge recall/precision, impossible parallel schedule, unknown→learning-gap packet, forbidden action accepted, final 5-section human output, latency. Pilot: recover at least one lost lawful edge/abstention case with no new forbidden acceptance, otherwise INCONCLUSIVE. **Metamorphic conditions:** permuting node presentation must preserve partial-order; duplicating identical node must not double-admit a learned source; clearing a missing prerequisite must change block/allow appropriately.

## GOLD-OR-A2 — P1: replay/retention E43, не подменить continual learning кешем

**Hypothesis:** donor replay mix improved older-domain retention (historical 0.3226→0.3978) but current learning path may omit replay, pick wrong examples or forget after restart.

**Inputs:** 12 short disjoint examples across two domains: 4 old C# code refactoring rules and 4 old EN→RU/RU→EN order/negation laws, then 4 new corrections in two other families. Reserve 4 paraphrased old holdouts unseen by replay selection. Use frozen tokenizer/vector version, snapshot journal ID and chosen/rejected labels; **translation direction is part of identity**.

**Arms:** current with replay disabled; current with E43-like replay at recorded 70/30 selection where meaningfully applicable; an exact-memory-only control; donor algorithm offline reference. Compare before, after new adaptation and *after cold restore*.

**Measure:** old heldout law accuracy, new corrections acquired, harmful interference, false accepts, update count, replay source ids, actual used recipient. A tiny positive exploratory result requires old-domain retention gain without new-domain material loss. Do not replay the same paraphrase into evaluation or count same teacher record twice; holdout must be source-disjoint. Failure: replay increased DPO margin but ordinary outputs unchanged.

## GOLD-OR-A3 — P1: domain-conditioned kNN/grammar/AST feature relocation

**Hypothesis:** «провалившийся» в матрице feature может улучшить kNN as *query dependent* feature, but not as unconditional token/embedding weight. Separate selector/eligibility, vectorizer, metric, top-k, rerank, memory and consumer.

**Three subdomains, one paired ablation each:**

| Test | Positive feature path | Negative transfer control | Outcome |
| --- | --- | --- | --- |
| C# / Angular / SQL design→code | E09 AST structural vectors + relative order + BM25 semantic context; syntax-aware reordering where AST exists | same source flattened/shuffled tree; no AST or wrong language | source-disjoint hit@k, valid symbol/edge match, compiler-grounded code task correctness |
| EN↔RU / language translation | E25 POS order and E26 context disambiguation then E27 reorder where direction supports it; lexicon domain gate | reorder in wrong direction or treat rare syntax as global, OOV/negation/adversarial homonym | direction-specific BLEU/chrF or edit-exact + human semantic/polarity accuracy on 8–12 cases |
| Orchestration/Wiki | dependency DAG/order & E49/E50 graph closure, Wiki lexical BM25 when authority source text matches | applying POS of language task to graph or AST to plain wiki; graph closure overriding unknown evidence | edge-F1 / evidence citation hit@k / abstention accuracy |

**Arms:** kNN baseline; + single structure/POS/graph feature in relevant domain; + same feature in deliberately irrelevant domain as negative; + lexical-only. Fix corpus/index/neighbor IDs/teacher split across arms, expose feature vector and selected neighbors. If chosen improvements happen only in matching domain and irrelevant domain is neutral/negative, set per-domain policy gate, **not** global deterministic fallback. If feature causes more unseen-domain errors, withdraw.

**Edge cases:** input <3 tokens, repeated nodes, Unicode mixed script, casing, language direction reversal, punctuation/negation scope, graph cycles, no admissible neighbors, cross-domain kNN collisions, index rebuild order.

## Bounded execution prompt for one CODEX/Kiro executor (no broad replan)

**CURRENT TASK:** execute GOLD-OR-A0 read-only baseline + minimal admitted-v2 shadow bundle outside production, capture native cold consumer 12-case result. If A fails or provenance impossible, stop with exact failure receipt; do not proceed A1/A2/A3.

**NOT THE TASK:** retrain RP01 500, replace frozen tests, publish weights, change forbidden llm_mvp_core/docs/plans/replanning-possible-diffs, fake safety proof, Windows CI, blanket skill preload.

**Required skills:** docs/skills/architect-behavior-classifier-SKILL.md; reference-inventory-and-source-map-SKILL.md; phase-planning-SKILL.md; plan-review-SKILL.md; code-examination-SKILL.md; experiment-design-SKILL.md; claim-verification-SKILL.md; prompt-patch-authoring-SKILL.md; human-readable-architecture-audit-SKILL.md and human-communication-GLOSSARY.md. Root AGENTS.md, RP01 local AGENTS/README before any running; source scope locked.

**Context:** pinned gold/current heads above; gold/current OrchestrationReasoningMachine.cs, Trainer.cs, ScenarioUnit.cs, XiigenExtensionApiService.cs, manifest + BOTH checkpoint files, source manifests and accepted pair IDs; read existing 00/52 documents and prior plan content. Do not assume schema v2 can be built from 8 rows; trace 15 patterns to lawful provenance.

**Allowed:** isolated scratch fixture and minimal loader/consumer driver, single affected .NET build/smoke, read-only evidence. **Forbidden:** touching donor/production weights/datasets, disabling provenance, altering gold evaluator after outputs, declaring quality from unit score, editing protected core plans.

**Good:** source-disjoint frozen cases; result status from real native load; C beats D by concrete independent correct decisions with zero safety regression. **Bad:** changing v1 schemaVersion field to 2 without verified proof and calling success; using archived 15 smoke patterns as all the heldout.

**Return packet schema:** source SHA; exact inputs/outputs; arm and model state hashes; proof receipt; trained recipient; status and selected pattern IDs per case; before/after output; source-disjoint coverage; latency/memory; independent judge and rubric; negative control; code diff; honest verdict VERIFIED/REFUTED/INCONCLUSIVE/NOT_RUN; next narrow task. Author evidence in own new file under planning possibleDifs when allowed, no modification of protected directory.

