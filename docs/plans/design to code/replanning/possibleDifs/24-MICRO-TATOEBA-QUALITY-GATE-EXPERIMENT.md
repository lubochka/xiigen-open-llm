# MICRO-N23 — 12-кейсный native scorer + двухцикловый причинный quality-learning pilot

**2026-10-16 · План опыта, НЕ выполненный .NET-тест и НЕ доказанный learning gain.** Перед применением перечитать [N23 source audit](23-INDEPENDENT-TATOEBA-QUALITY-SCORER-LEXICON-CACHE.md), исходный core `0eccbdc3c4efb90adf1d699806cbfa9f5610998d`, действующий `AGENTS.md` и навыки `trainable-architecture-decision`, `phase-planning`, `plan-review`, `generated-code-review`, `human-readable-architecture-audit`, `prompt-patch-authoring`. Независимый reviewer CODEX/Astra ещё НЕ назначен/не запускался.

## Человеческий вопрос

Когда система получает новые правильные примеры, действительно ли её пользовательские переводы становятся точнее, или грубая проверка `ProductMatch` ошибочно засчитывает противоположную инструкцию / адресата / число как успех и меняет дальнейший выбор, чему учиться? **Один фактор:** правило domain-law correctness в scorer и вызванные им решения. Код обучения Units, данные, seed/checkpoint и сеть остаются теми же.

### Исполняемые ступени

| Ступень | Где | Ресурс | Правильный фальсификатор | Решение |
|---|---|---|---|---|
| L0-A source shadow | Node 22+ | synthetic 12 строк, 0 оптимизатора | Existing JS source mirror не воспроизводит заявленные значения | Проверка арифметики, НЕ .NET |
| L0-B native parity | actual `TatoebaPilotLoopRunner.BuildRowResult` C# (private helper из test assembly через reflection либо *минимальный internal seam*, без изменения production decision) | 12 synthetic records; `0 trainer steps` | На оригинальном C# неверные переводы **не** получают `ProductMatch=true` или точный Hebrew не отвергается | Если falsified — исправить source claim, остановить проект ремонта |
| L1 learned-decision trace | Actual `TatoebaPilotLoopRunner` + `TranslationMachine`; два одинаковых входных snapshot; A=current scorer, B=shadow scorer с independent law oracle, переиспользовать реальные outputs | 2 цикла; ≤20 lawful train, 8 dev & 8 blinded heldout, 0 Student DPO | Идентичны decisions/accepted snapshots; различается только reporting → гипотеза о влиянии на learning selection не доказана | Report decision delta отдельно от quality |
| L2 outcome proof (условно) | train 2 branches от одного cold checkpoint, A=current gate, B=law-aware gate, повторение 2 разных source families | тот же ограниченный корпус, одинаковые вызовы; никаких дорогих CI | Нет source-family-disjoint и law-correct прироста при равных/установленных coverage и бюджете | NO_GAIN, даже если L0 нашёл баг |

L0-B дешевле нового DPO run, выполняется **первым**. Никаких Windows GitHub Actions для этого исследования не требовать; native dotnet нужен в рабочей среде CODEX, не симулировать его JS.

## Frozen L0 fixture

Источник synthetic direct mirror: [23-MICRO-SCORER-LAW-PROXY.mjs](23-MICRO-SCORER-LAW-PROXY.mjs), [raw output](23-MICRO-SCORER-LAW-PROXY-RESULT.json). Контракт полей: `id,direction,source,expected,actual,oracleCorrect`, плюс source-derived `exact,scriptOk,copied,similarity,extraNegation,productMatch`.

Обязательные law-opposites: EN_RU negative deletion, RU_EN negative deletion, prohibited vs permitted, recipient substitution, number change, action inversion, nonlocal extra tail; controls: exact correct EN/RU, lawful rephrase; EN_HE correct Hebrew target как отдельное направление. Корпус synthetic, без приватных Tatoeba и без квазислучайной оценки процента реальной ошибки.

**L0-B executable work order:** в новом test helper сформировать соответствующие `TatoebaPrivateSentencePair` с заданной `Direction/SourceText/TargetText`, вызвать настоящий `BuildRowResult` (при необходимости reflection только в test, без расширения public product API), убедиться в равенстве каждого source intermediate (TokenSimilarity, negation, script, copied). Если private helper недоступен — создать локальный тестовый wrapper в разрешённом test project и независимым review проверить, что он вызвал ORIGINAL метод, а не переписанную копию. Никакой фиктивной замены настоящего метода прокси-кодом.

**Предварительный запрет ложного успеха:** предсказано ≥1 false-accept missing-negation и ≥1 false-reject EN_HE. Если не воспроизвелось, `N23-01/02` понизить до `INVALID_EVIDENCE`, не переносить repair. При совпадении ещё не объявлять learning gain.

## L1/L2: настоящий минимальный тест влияния на обучение

### Изоляция и предотвращение leakage
1. **C0:** один frozen исходный checkpoint `TranslationMachine`, tokenizer/Unit state, dedup/history, corpus sources, ordered examples, target direction; каждый arm начинает из *своего* холодного `LoadCheckpointAsync`. Заморозить exogenous raw translations/evaluator; сравнивать одинаковые `sourceId` и `expected`. Создать SHA всех входов, checkpoints, scoring версии, caller trace и политику рекурсивного использования примеров.
2. **Selection:** 8 DEV-case для calibration (не для окончательного результата), 8 source/family-disjoint SEALED-case для финальной оценки, плюс *не менее 2* counterfactual law-critical cases (negative+recipient/number) в blind панель. Одна source family не делится между train и heldout. Не использовать selected heldout answer или source text при определении law-aware gate.
3. **A:** текущий `BuildRowResult` и текущая последовательность `PassesTarget/BestRuntimeProductAccuracy/PromotionThresholdsFromMaxAccuracy/ShouldPromoteToTraining`.
4. **B:** тот же Output/IDs, те же threshold/gates/Units, но только `ProductMatch` подменён shadow-result независимого CODEX/Astra 6 UltraCode judge на DEV и online decision cases. Одно изменение; недопустимо параллельно менять KNN, LoRA, словарь, BPE или selected examples. Для исследовательской минимизации цена judge и calls учитываются отдельно. Вариант полностью детерминированного typed law checker допустим лишь для заранее объявленных simple negation/number/recipient кейсов, **не** для общего утверждения о семантике.
5. **L1:** сравнить `score→gate→promotion decision→accepted rows→new learned state SHA`. Если разные решения не дошли до actual training/adoption, hypothesis quality-effect остаётся неподтверждённой. Особый контроль: при одинаковой promotion decision ошибка scorer влияет только на отчёт, а не на обучение.
6. **L2:** после реального отдельного обучения/публикации в двух arms сделать COLD restart и вызвать реальный `TranslationRoutingFlow`, без test bypass. Собрать *все* ответы на 8 sealed queries; CODEX/Astra 6 UltraCode judging без доступа к arm identity. Запрет output-first calibration на blind; output без правильного закона считается wrong даже если token recall высокий.

### Predeclared outcome
- `law_correct_count/8` — primary на blind; отдельно `exact, ProductMatch, wrong-negation, wrong-recipient, wrong-number, false-accept, false-reject, target-script, coverage, abstain, newly-correct, newly-wrong, old-law-retention`.
- `IMPROVE` только если L0 native parity, L1 изменился реальный gate+training decision, L2 branch B не ухудшил ни одного критического закона и дал ≥2 новых law-correct ответа из восьми при **той же coverage**. Это практический порог pilot, не статистическая значимость. Второй независимый source-family run обязателен до general recommendation.
- `REGRESS`: критический закон испорчен, dropped correct old answer, или corruption/leakage. `NO_GAIN`: эффекта на корректность нет. `INCONCLUSIVE`: мало или неодинаковая coverage. `INVALID_EVIDENCE`: native proxy mismatch, train/holdout leakage или same checkpoint mutated.
- Возможные альтернативы: quality-scoring bug существует, но не влияет на selection; gating меняется, но downstream Unit не обучается; Unit учится, но consumer не вызывает новое состояние; исходные DPO Student 0/4 не меняются вообще. Это **разные результаты**, не складывать в один PASS.

## Критерии review и handoff

**CURRENT TASK:** доказать/опровергнуть N23-01/02; затем только при native подтверждении причинно проверить влияние на Unit-learning gate. **NOT THE TASK:** новый RP01 Student-LoRA запуск, модельная архитектурная перестройка, production fix без измерения, утверждение о настоящем Chinese teacher по синтетическим строкам.

**Allowed initially:** read/review, один bounded local test в `Tests`, frozen synthetic fixture, local native code execution при наличии среды; документировать hash/outputs. **Forbidden:** rewrite old results, update historical checkpoints, менять learned KNN/LoRA, запустить широкий Windows CI, тестировать реальную пользовательскую модель без прав и отдельного контракта, превращать JS в native evidence.

**Required skill checks:** архитектор — конкретный caller/learned state; planning — one-variable gate; plan reviewer — источники/negative controls/unseen splits; code reviewer — exact source SHA, original method, no hidden simulator; prompt engineering — неизменные инструкции и разграничение assessor/proxy; human communication — на русском по 12 кейсам почему прав/не прав без ложного PASS. Ментально проиграть GOOD/BAD/EDGE (missing negation, added negation, unknown token, identical output, Hebrew). Review verdict: `DRAFT_REVIEW_PENDING`, пока отдельный CODEX/Astra invocation не вернул верифицируемый пакет с `reviewed_sha`.

**RACI DPO (не нарушать):** человеческий выбор task/recipient/acceptance делает **CODEX с Astra 6 UltraCode**; обе DPO sides **chosen и rejected производит разрешённая внешняя открытая китайская модель**; смысловое качество пары независимо оценивает **CODEX с Astra 6 UltraCode**. Этот опыт может не использовать DPO вообще; в таком случае не изобретать DPO provenance. Manual synthetic fixtures ниже не имеют teacher/assessor authority.

**Output packet:**
`{pinned_core_sha, native_test_project, fixture_sha, per_case_original_values, native_vs_proxy_diff, A_B_same_input_proof, C0_checkpoint_sha, scoring_only_delta, promotion_decision_delta, trained_state_delta, cold_ordinary_consumer_results, per_domain_law_correct, per_case_failures, verdict, judge_identity_and_method, review_candidate_sha_or_NOT_PERFORMED, unverified, publication_sha}`.

**Status as authored:** JS source-aligned synthetic counterexample EXECUTED; native original BuildRowResult NOT_RUN; actual two-cycle learned-quality counterfactual NOT_RUN; reviewer NOT_RUN. Work order is ready for a CODEX implementer, not a claimed success.

## Дополнительная дешёвая проверка L0-C: неверный смысл sub-Unit при корректных парах

До L1/L2 (или независимо от них) выполнить [N23-03A four-row fixture](23-MICRO-LEXICON-COOCCURRENCE-PROXY.mjs): source `dog barks …` x4, targets `собака лает …` x3 и `пёс тихо лает` x1. [Raw synthetic result](23-MICRO-LEXICON-COOCCURRENCE-RESULT.json): `dog→лает` score=1.00 выигрывает у `dog→собака` score=.75, поскольку это co-occurrence, а не alignment. Это отдельный фактор от `ProductMatch`. **Native L0-C:** один real `InduceLexicon` вызов в test harness с четырьмя original record и ассертом результата; затем сверка `ToTrainReadyPair` / concrete Unit store, поскольку source-level выбор map не тождествен его фактическому использованию. **Learning L1-C (условно):** 2 одинаковых frozen Unit snapshots, A=induced lexicon, B=lexicon disabled/verified aligned, неизменные phrase mappings; 8 fresh source-family-disjoint queries, score law correctness при одинаковой coverage. Null downstream difference = `MECHANISM_ONLY`, даже если L0-C PASS. Независимая CODEX/Astra 6 UltraCode проверка gold и точного candidate SHA обязательна до слова `QUALITY_IMPROVED`.
