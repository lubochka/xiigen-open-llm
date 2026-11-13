# 12 — Мини-эксперименты для каждого DIF-01…DIF-44: проверка одного отличия на реальном результате
**16.10.2026 · DOCUMENTED PROPOSALS, NOT EXECUTED · NOT INDEPENDENT REVIEW**. Дополнение к [44 DIF](01-PRIORITIZED-DIFFERENCES.md), [11 — готовый микроэксперимент](11-QUICK-CAUSAL-PILOT-UNIT-VS-STUDENT.md) и [RP01 REC01…05](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md).

## Принцип сравнения
**Проверка кода ≠ эффект на качество.** Каждый пункт закрывается по трём последовательным уровням:
- **L0 PATH/MECHANISM:** доказали, что отличие реально есть и вызывается конкретным route на этом input. Для некоторых гипотез L0 уже есть.
- **L1 CONTROLLED INTERVENTION:** при замене **одного** механизма, одинаковых dataset/seed/model/index/training budget/consumer, поведение на новых примерах меняется. Нужен контроль без вмешательства и негативный контроль/false-case.
- **L2 LEARNED UTILITY:** после действительного train-state change **новые не использованные при обучении примеры** выигрывают по предметному закону и пользовательскому ответу (отдельно от train loss, margin, confidence, metadata, слова "trained"). Для pure decoding/validation/checkpoint probes L2 неприменим — такой эксперимент только локализует причину и **не доказывает обучение**.

**Коды стоимости:** `R` — read-only/без новых оптимизаций, `U` — существующие Unit/Flow, 0 LLM-gradients, `N` — один ограниченный новый neutral research Student update, `L` — только **позже**, после не-нулевой исходной способности. Буквы не являются утверждением, что существующий CLI поддерживает указанный размер. Для RP01 [`ValidOptions`](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L224-L230) сейчас допускает только calibration=1 или main=500; никаких фиктивных `train --pairs 12`. Создать/переиспользовать один scoped small research mode, только если N действительно потребуется.

## Приоритет: сначала 3 самых информативных

1. **PD-FAST-01** → DIF-08/25/26/35/36: готовые 8 TRAIN, 8 recombinations, 4 негативных контраста, три состояния Unit (до, после, намеренно неверно), cold restore. [Полный протокол + fixtures](11-QUICK-CAUSAL-PILOT-UNIT-VS-STUDENT.md). Может показать, что потерянный learned memory механизм влияет на **ordinary output**, но не доказывает LoRA gain.
2. **DIF-02/03/24**: без градиентов перетасовать семантические слоты `TeachingVector`. [`VectorSignal`](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L1159-L1166) при неизменной sparsity даст тот же scalar — это объясняет отсутствие coordinate teaching в **конкретном** StepVector, но не доказывает, что новая projection улучшит качество. Для causality нужен последующий learned-consumer A/B.
3. **DIF-01/05/23**: только после safe small-run admission и gradient finite difference — DPO-only vs warm→DPO на том же budget (REC02/03). Измерять train overfit отдельно от new-family free answer; не повторять frozen 500.

## Таблица фальсифицируемых микро-проверок

| DIF | Мини-вмешательство (контроль A vs один B) | Измеримый результат/фальсификатор | Исполнитель и ранняя остановка |
| --- | --- | --- | --- |
| DIF-01 | N: B0 random+DPO vs warmup+DPO | Same frozen total token/update budget, unseen-language law rubric; Δ heldout semantics, separate train overfit | REC03; STOP if no grad/tiny overfit |
| DIF-02 | R→N: permute TeachingVector coordinates, preserve sparsity | Same loss/grad under current scalar confirms semantic coordinates are ignored; only redesigned learned recipient + heldout can show quality gain | RP01/RP02; do not label permutation as training gain |
| DIF-03 | R→U: local root index vs shared fixed vocab | Same 2 prompts represent common token in same slot; then one downstream relevance prediction under same pool | RP02; stop when vector recipient absent |
| DIF-04 | R→N: template-disjoint vs random split | Same Student after update, compare per-family heldout scores and contamination count | RP01; new untouched exam only |
| DIF-05 | N: 8-16 exact new examples, 1-3 updates | Train odds and chosen CE after each step vs before; then new-family free output | REC02; small runner not yet implemented |
| DIF-06 | R: same logits teacher-forced vs greedy first-token | Pre/post target odds grow but greedy token unchanged? record first top1-top2 gap | REC02/03; no training needed for diagnostic |
| DIF-07 | U: first-hit vs fixed-pool lawful rerank | One heldout query returns different lawful output with same memory and k; precision/coverage and latency | RP02; compare exact ANN separately |
| DIF-08 | U: Student-alone vs learned Unit-enabled, Student frozen | Same customer request improved only with trained Unit; trace actual consumer IDs, no DPO assumption | Pilot 11; RP02/03 |
| DIF-09 | N gated: rank8 q/v vs one alternate target set | Same B0/teacher/pairs/token budget; tiny fit + new-family semantics and cost | REC03; after good gradient |
| DIF-10 | R: C# BPE629 ENRU token length/profile | Same source sentence length vs target token count, fragmentation and decoded fidelity; prefix/chosen occupancy | REC03; no faulty-RU claim from roundtrip alone |
| DIF-11 | R: paired train-margin vs free-skill rubric | Same checkpoint and inputs show independent outcome gap; not a cause by itself | REC02/03; do not convert metric to quality |
| DIF-12 | L: replay vs no replay with verified old-domain skill | Same parent and new sample; old-skill retention Δ and new-skill utility Δ | REC05 after nonzero baseline; no consumed pair replay |
| DIF-13 | U: order-reversed query, same hashed bag | Vector identity collision plus divergent correct outcomes vs structure-aware version | RP02; 2-4 contrast cases |
| DIF-14 | U: floor=warn-only vs abstain decision | On same low-confidence heldout, compare error severity, coverage, abstention cost | RP02; calibration outside test |
| DIF-15 | R→U: unfiltered pool vs domain-law eligibility | Admissible neighbor recall, bad forbidden acceptance before/after; same fixed corpus | RP02; prove actual caller |
| DIF-16 | R→U: raw vs normalized under negation/numbers/Cyrillic | Same intents become vector-identical? if yes, actual solver errors differ under same candidates | RP02; other raw channels may protect |
| DIF-17 | R: stack detection before/after normalizer | On C#/TS/SQL markers compare extracted stack and downstream route; pass existing order if correct | RP02; negative control |
| DIF-18 | R: compiler-valid vs template-marked C# 8–12 samples | Independent compile/static semantic check, count false chosen/false rejected; no training | RP01; frozen source read-only |
| DIF-19 | R: simulation vs actual author provenance | One exact pair generated by approved author+judge vs simulation; recipient/prompt parity and lawful admission | RP01; no fake Chinese provenance |
| DIF-20 | R→N: near-equal vs meaningful chosen/rejected | Blinded judge law contrast; only admitted new examples then tiny same-budget DPO train effect | REC02; no retroactive labels |
| DIF-21 | R: exact RP01 mask vs legacy Compact path | Proof prompt response mask, no truncation, identical initial prefix; compare only invoked path | REC02; do not import old defect |
| DIF-22 | R: prompt+response concatenation vs separate-token boundaries | IDs, decoded bytes, masked target positions same intended completion; then 1-sample odds | REC02/03; no forced EOS |
| DIF-23 | N: tiny directional finite-difference | Analytic vs numeric DPO gradient agrees at fixed dropout0 before training | REC02; stop if mismatch |
| DIF-24 | R→N: TeachingVector values/sign vs active-count only | Current derivative invariant on coordinate reorder; prospective projection branch must prove heldout gain | RP02; not proof of neural defect |
| DIF-25 | U: 2 inputs→3 typed outputs with associations | Permute arrival order; actual outputs keep source→target binding, no clone/loss; same downstream judge | RP03; 1→0 case control |
| DIF-26 | R→U: DPO target unit ID to checkpoint binding | One new unit-targeted lesson changes only named state; cold consumer calls correct updated unit | RP01/RP03; no global inference |
| DIF-27 | U: frozen units + changed learned Flow only | Same input/output payloads, changed relation improves new law/composition outcome | RP03; distinguish static wiring |
| DIF-28 | U: frozen units + static vs async candidate selection | Same branches and budget, stable dependencies; law correctness vs cost, no first-finish bias | RP04; no whole Windows suite |
| DIF-29 | L: graph novel motif vs memorized path | Hold out new composition of familiar units, toggle learned edge state only; graph-law accuracy | RP04 after real trained edge |
| DIF-30 | L: C# competence → ENRU continual update | Must demonstrate C# skill >0 before judging retention, then equal heldout before/after | REC05; skip if no skill |
| DIF-31 | R: private vs serving pointer | Same checkpoint SHA/adapter keys/tokenizer through cold ordinary request; first logits and result parity | REC05; no unnecessary retrain |
| DIF-32 | R: policy/reference loaded C50 before ENRU | Hash and diff both copies at start and frozen reference after; compare with intended from-scratch reference semantics | RP01; existing verified source pins |
| DIF-33 | R→N: LoRA layer/topology/RNG | Gradient norm and A/B delta with frozen base, before/after cold logits parity; tiny training only if needed | REC02; control rank alpha dropout |
| DIF-34 | U: dynamic class weight vs constant×10 | Same split, balanced heldout law F1 and forgotten classes; avoid naive weighting | RP02/05; no DPO replay |
| DIF-35 | U: E34V2 reference fusion vs current fixed fusion | Same heldout indices & pool, top1/hit3 and downstream correctness; avoid old mismatched denominators | RP02; frozen alpha before exam |
| DIF-36 | U: positive-only vs positive/negative/warning memory | Same lawful candidates, compare critical errors+coverage; warning ≠ DPO pair | RP02/03; same answer scorer |
| DIF-37 | R→U: domain×intent×language×stack composite | SQL BUG vs C# BUG, RU/EN; domain-aware route vs collapsed tag; actual routed solver | RP02; 6-case micro |
| DIF-38 | U: same Unit outputs, credit to selected vs nonselected path | Update target changes correct branch only; per-stage trace not just aggregate reward | RP03/04; active learned state required |
| DIF-39 | R: train prompt framing vs serving OwnerInput | Same tokens/continuation boundary, first logits, stop-reason; judge 1/8/32/256-token outputs | REC03; no arbitrary EOS ID |
| DIF-40 | L: fixed order vs admissible active curriculum | Equal new example sets and optimizer compute; new-family law gain only after tiny learnability | REC05; no E45 inherited PASS |
| DIF-41 | R→N: sealed new semantic families only | Blind independent post-train verdict vs close template transfer, no source overlap | REC05; no contamination of frozen panel |
| DIF-42 | R: same micro-run stage timers | Reference forward/policy backward/optimizer/checkpoint wall, peak process memory and quality per cost | RP01; no CI benchmarking claim |
| DIF-43 | L: lawful feedback to new row to chosen state | New consented feedback maps once to admitted lesson, private candidate and actual consumer; restart/dedup | RP05; no automatic production promo |
| DIF-44 | U: forbidden/stale neighbor vs explicit unknown fallback | Same input, compare critical errors/abstains/coverage with and without hard-law guard | RP02; correct low-coverage penalty |

## Универсальная карточка для исполнения каждой строки

```yaml
experiment_id: PD-DIF-XX
status: NOT_RUN
hypothesis: "Что должно измениться и почему"
entrypoint: "реальный method -> ordinary consumer; подтверждённая текущая ветка и SHA"
source_inputs: "new, permissioned training or read-only snapshots; never frozen validation in train"
arms:
  A: "original, frozen baseline"
  B: "exactly one changed mechanism"
  C: "negative/label-shuffle/no-learning control where meaningful"
invariants: [same_request, same_domain_law, same_scoring_population, same_checkpoint_except_named_change, same_budget]
primary_metric: "new-case law-correct ordinary answer, with coverage and invalid/abstain"
secondary: [train_odds_if_neural, state_change, count_real_updates, first_token, runtime_cost, process_ram]
positive_example: "input -> expected meaningful property"
negative_example: "near-miss input; one concrete violated law"
stop_condition: "predeclared low budget, math inconsistency, no actual recipient or inadmissible source"
result: NOT_MEASURED
verdict: "LOCAL_CAUSAL_SUPPORT | DISCONFIRMED | INCONCLUSIVE | UNEXECUTABLE"
evidence: [core_sha, plan_sha, source_path_and_lines, fixture_sha, arm_receipts, assessor_id_or_NONE]
```

Никакой один case или `PASS` не даёт статистически убедительную общую модельную способность. Для независимого вывода нужна новизна законов/семейств, повтор на другом дне/корпусе и экспертная оценка без утечки ответов. Сначала один **маленький** probe со всеми снимками, а не 44 запуска подряд. Не включать Windows Actions/coverage; локально компилировать лишь затронутый малый harness. Если первая фальсифицирующая проверка уже показывает отсутствие используемого consumer, не расходовать модельные токены на априори бессмысленное обучение; записать `NOT_INVOCABLE` + конкретный bridge.

**Human-readable итог** в каждом receipt: «Какая деталь отличалась → где отследили → что изменили только в B → какой реальный запрос стал лучше/хуже → сколько это стоило → что это не доказывает». В графовых/translation правилах deterministic safety gate допускается; недостаток — выдавать его за выученный solver. Никакой hidden grade fitting, forced pass или replay already-trained source.
