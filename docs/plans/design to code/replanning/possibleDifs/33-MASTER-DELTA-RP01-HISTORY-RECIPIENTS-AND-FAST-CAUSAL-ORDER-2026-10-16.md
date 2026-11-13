# N33 MASTER DELTA — RP01: старая успешная обучаемость, 26 осей потери качества и новый блокер нейронного warm-start

**Дата:** 16.10.2026. **Статус:** новые доказательства по исходникам + интеграция чужого evidence, без native-обучения и без independent CODEX review. Исходный core snapshot: `32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7` (`codex/dpo-implementation`). Planning writing branch: `llm_plan3`. Единственное место новых файлов: `llm mvp/design to code session/replanning/possibleDifs`. Запрещённая зона `llm_mvp_core/docs/plans/replanning-possible-diffs` прочитана выборочно, **изменений нет**.

## 1. Где именно могло потеряться обучение — не смешивать получателей

У обучения здесь **нет одного универсального получателя**. Для каждого прежнего успешного урока нужно установить: исходный запрос → кто выбрал lesson → кто породил `chosen/rejected` → кто отдельно оценил пригодность → какие реальные ids/vectors/subunits получились → какой **конкретный** объект тренируется → как он сохранён и восстановлен → кто потребляет выученное состояние в обычном ответе → что изменилось на невиденных семьях. «Прошёл pipeline» и «не ошибается на этой строке» не равны «появилась обобщённая способность».

**Обязательный контракт DPO:** роль человека в выборе урока исполняет **CODEX с Astra 6 UltraCode**. **Разрешённая внешняя открытая китайская модель** создаёт **обе окончательные стороны DPO: chosen И rejected**. Затем **CODEX с Astra 6 UltraCode** независимо проверяет именно обе стороны пары, качество контраста и результат обычного ответа. Без реальных вызовов фиксировать `NOT_RUN`, не объявлять synthetic self-authored DPO эталоном.

Три архитектурные ветви не взаимозаменяемы:

| Recipient | Реальное изменение и потребитель | Что НЕ следует заключать |
| --- | --- | --- |
| **Neural Student / RP01** | Exact DPO loss → q/v-LoRA → saved policy checkpoint → `TextGenerator` | LoRA tensors changed ≠ syntax learned, ≠ KNN learned |
| **Typed translation/structural Units** | DPO decomposition → Tokenizer / Executor / Verifier / POS/word/phrase memory → direction adapter / Flow | Stored pair/Unit count ≠ consumer видел learned chosen |
| **KNN / Graph / episodic route** | Learned examples + stable identity / embeddings + ANN / ranking + scope / arbiter → actual route answer | `kNN top1` и oracle features ≠ correctness on unseen family; E09 structural ≠ LoRA gain |

Для текущих RP01 источников: [causal pack](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-CAUSAL-LEARNING-DIAGNOSTIC-PACK.md) и [recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md). Результаты **C#500/50 updates: 0/3→0/3**, **ENRU500/ещё 50 updates: 0/4→0/4**; LoRA изменилась. Экзамены слишком малы для статистического вывода по семействам, но достаточны, чтобы не называть обучение успешным на тех опубликованных заданиях. B0 случайная (не pretrained). Любое объяснение должно пройти причинный эксперимент на реальном consumer.

## 2. Полнота известных семейств: ссылки на реальные владельцы доказательств

Это **индекс 26 уже описанных классов из [N30 master](30-MASTER-QUALITY-LOSS-COVERAGE-AND-SOURCE-LINEAGE-2026-10-16.md)**, **не** заявление о 26 доказанных первопричинах. Дополнительные независимые дельты [N29](29-CONSOLIDATED-EVIDENCE-ROUTING-AND-NOVELTY-REGISTER-2026-10-16.md), [N31](31-INTEGRATED-POSSIBLE-DIFFS-NEW-HYPOTHESES-AND-PRIORITY-2026-10-16.md), [N32](32-INCREMENTAL-NOVELTY-REGISTER-AND-DECISION-ORDER-2026-10-16.md) не стирать и не переименовывать.

| Evidence family | Риск / отличающее измерение | Домены, где прежде всего искать |
| --- | --- | --- |
| **Neural base P0 — #01** | Random B0; SFT tiny overfit и new-family свободный ответ | все языковые / кодовые neural |
| **DPO teacher/provenance/independence — #02–03** | Правильный human selector, authorized обе sides, независимая judge, 25 semantic families на 500 rows; split by source family | все DPO |
| **Exact pair / gradient / optimizer — #04–05** | raw sides → IDs+response masks, loss direction/ref, numeric gradient; vector signal scalarization и AdamW scale | exact DPO / StepVector **различать** |
| **LoRA/bpe/decoder/restore — #06–09** | q/v/rank/target и volume, BPE vocab/fingerprint/EOS, KV/full parity, greedy first token, cold correct policy | neural; отдельные issues multilingual |
| **True consumer / decomposition — #10–12** | `TextGenerator` не читает kNN; DPO→3 typed lessons, silent unknown Unit, Tokenizer self-test/Verifier negative-only | structured translation, code documentation |
| **Исторические POS/AST/context — #13–14** | Потеря E26 preceding concept / left-right context / syntactic roles; E07 matrix feature не равно фактическому KNN feature | RU/EN/HE translation; **AST отдельно C#** |
| **KNN identity/eligibility/ranking — #15–18** | HNSW hit count ≠ reachable, predicate scope, language direction, negation, topK before filter, BM25 score nonlocal | retrieval/translation/code |
| **Verifier/composition — #19–23** | positive/rejected/warning conflict, unknown clause omission, leftmost phrase greedy, `can't/can`/operator `->` collapse | translation/DSL/graph, не универсально |
| **Replay/flow/graph — #24–25** | wrong state restore/recipient, replay old-law forgetting, static fallback, graph/Flow bypass actual learned Unit | continual, project orchestration |
| **Quality gate — #26** | pair margin, Unit pass, product contract pass vs independently judged **ordinary** unseen answer; negative controls | **все** домены |

Для конкретных кейсов открывать исходные owners: [DPO качество и objective](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [LoRA и vector space](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [checkpoint и restore](06-CHECKPOINT-RESTORE-ADOPTION.md), [матрица → KNN misreads](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [история переноса](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md), [units, flows, graph](05-UNITS-FLOW-GRAPH-DETERMINISM.md), [базовый retrieval](04-KNN-HYBRID-RETRIEVAL.md), [branch history](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md). Каждый owner уже содержит более подробные paths, figures, microdesign и разграничения; вместо повторной переписи нужен native/semantic counterexample.

## 3. Новое: четыре ранее не выделенных source seams и одна условная

[Точное исследование N33-A..E](33-NEW-AUTOGRAD-EMBEDDING-BIAS-AND-SUPERVISED-WARMUP-GAPS-2026-10-16.md), [пять сверхмалых проверок](33-MICRO-AUTOGRAD-WARMUP-AND-LEARNING-IMPACT-2026-10-16.md):

1. **N33-A SOURCE_CONFIRMED / P0 при neural warm-start:** `Embedding.Forward` через `Array.Copy` создаёт не связанный с весами выход; токенный `Weight.Grad` не пополняется. [Embedding.cs](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/00.TensorCore/Layers/Embedding.cs#L35-L56). Это потенциально исключает обновление лексических представлений через обычный SFT/pretraining, **не** прямой баг frozen-base q/v LoRA.
2. **N33-B SOURCE_CONFIRMED / P2:** `Linear.ExpandBias` отсоединяет bias от графа и не редуцирует градиент по batch/seq, вопреки комментарию. [Linear.cs](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/00.TensorCore/Layers/Linear.cs#L37-L84). Основной RP01 Transformer `useBias:false`; только адресные custom trainable layers.
3. **N33-C SOURCE_CONFIRMED / P0 для supervised warm-up:** default `WarmupSteps=100`; `CosineWithWarmup.GetLr(0)=0`; `Trainer.TrainSupervisedNextTokenStep` требует LR>0, падает до первого optimizer update. [Trainer.cs](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/04.TrainingLoop/Trainer.cs#L124-L147). RP01 DpoTrainer использует **другой** путь.
4. **N33-D SOURCE_CONFIRMED / P1 для очень короткого pretraining:** `RunPretrainingCoreAsync` допускает `lr=0` на первом шаге, увеличивает `CompletedSteps` и AdamW moment counters без изменения весов. [Trainer.cs](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/04.TrainingLoop/Trainer.cs#L233-L264). Не объявлять массовой потерей всех следующих updates.
5. **N33-E CONDITIONAL / P2:** `_supervisedStep` живёт в instance `Trainer` без показанного cold state restore; новый объект может сбросить LR schedule. До факта нового экземпляра на serving/continuation — только гипотеза.

**Новизна и история:** непосредственный пофайловый readback веток `DesignToCode` и `language_translate` выявил **тот же raw-copy Embedding**. Это inherited deficiency, а не regression только RP01. В историческом `DesignToCode Trainer.cs` не найден новый `TrainSupervisedNextTokenStep`, поэтому N33-C не следует переносить на старый runner. В обследованных сводных и нескольких read-only core reports эти точные seams не обнаружены; **полный exhaustiveness claim не делается**. Отрицательная data-mining находка не равна доказательству отсутствия у другого агента.

## 4. Что действительно сработало исторически, включая «FAILED» эксперименты

| Архив / ветка / donor | Опубликованный позитивный сигнал | Лимит доказательства / следующий fair compare |
| --- | --- | --- |
| **E26 ContextualDictionary** | 15 fixed context examples, 0.40→1.00 при известном POS слева/справа | `BuildDefault()` и маленький known panel; проверить trained/new-family POS Unit, не универсальный retrieval |
| **E09 structural KNN** | 0.2413→0.3507 top1 с 4492 train pairs | сравнение baseline/enhanced на **разных** validation subsets; 8–16 same-family fixed C# AST tasks, exact-KNN vs AST |
| **E14 ContextAction KNN Tier2** | 0.75→1.00 на 4 validation (оба [30.04](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/35.ExperimentsV2/results/E14-CONTEXT-ACTION-KNN/results/run-2026-04-30-001.json) и [01.05](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/35.ExperimentsV2/results/E14-CONTEXT-ACTION-KNN/results/run-2026-05-01-001.json)) | **Tier3 30.04:** 0.8393→0.875, `passed=false`, 56 val; **Tier3 01.05:** 0.8824→0.8676, `passed=false`, 68 val. Это разные и сильно несбалансированные corpora, поэтому и знак delta меняется; не сравнивать как same-panel regression. [Первичные JSON 30.04](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/35.ExperimentsV2/results/E14-CONTEXT-ACTION-KNN/results/run-2026-04-30-002.json), [01.05](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/35.ExperimentsV2/results/E14-CONTEXT-ACTION-KNN/results/run-2026-05-01-002.json) |
| **E43 replay** | удержание 0.3226→0.3978 | другой bigram learner/panel; проверять old-law retention в actual RP01 recipient |
| **DesignToCode V210** | real `StepMany` и checkpoint, 134 pairs / 9 batches, train accuracy .8333 / margin +.005703 | не доказано free-answer generalization; иные tokenizer, prompts, repetitions |
| **E13B** | known expression completion 100%, unseen0%, осознанное abstain100% | полезная learned exact memory, **не** нейронное понимание |
| **E52** | 4 contract scenarios 0→100% | часть baseline=false жёстко задана; не student quality |

Первичные results и source refs уже собраны в [N30 historical table](30-MASTER-QUALITY-LOSS-COVERAGE-AND-SOURCE-LINEAGE-2026-10-16.md), [historical corpus](02-HISTORICAL-EXPERIMENTS.md), [siblings](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md), [core read-only E52 adjudication](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/docs/plans/replanning-possible-diffs/40-e52-contract-success-and-answer-independent-veto.md). Заявленный пользователем **конкретный** перенос неудачной matrix функции в успешно обучившийся KNN **не идентифицирован доказательно по SHA+feature+caller+matched receipt**: нельзя выдумывать donor по близости заголовков. Сопоставить `E07`, `E09`, `E14`, исторические `33.MatrixAlgorithms`, `language_translate`, `DesignToCode` по коммитам и фактическим аргументам фичи — отдельный evidence gate.

**Доменная применимость обязательна:** синтаксическое дерево/типы операндов и связи AST — преимущественно C#/SQL/DSL; словопорядок, negation, agreement, subject gender и POS — лингвистические направления; E14 action sequence — routing; unicode/mixed script — многиязычный ввод; source-family disjoint, restore и quality judge универсальны. Правило может улучшать один домен и вредить другому, поэтому включать feature только при положительном same-panel causal result и отдельной регрессионной оценке соседних доменов.

## 5. Реальный быстрый порядок работ — не очередная 500-run матрица

| Приоритет | Один проверяемый вопрос | Малый опыт и принятие решения |
| --- | --- | --- |
| **G0** | Какой recipient был у фактически успешных старых уроков? | 3–5 frozen historical case IDs: original source → changed state → ordinary consumer; дополнить matrix→KNN SHA. Без source lineage — `UNRESOLVED`. |
| **G1 P0** | Есть ли работающий **первый** нейронный supervised шаг и gradient lookup? | [MICRO-N33 M0.A/B](33-MICRO-AUTOGRAD-WARMUP-AND-LEARNING-IMPACT-2026-10-16.md), два маленьких native probe, gradient finite difference + LR. Если нет — **не тратить 500 DPO** на random B0. |
| **G2 P0** | Исправление N33 даёт возможность усвоить хотя бы 8 законов? | M1 frozen A/B 8–32 SFT updates, heldout by family + retention, обычный TextGenerator; `GRAD_FIXED` ≠ `QUALITY_GAIN`. |
| **G3 P0** | Не перепутан ли semantic DPO contract? | 4–8 одобренных разных пар: CODEX выбрал, authorized open Chinese authored обе стороны, CODEX независимо rated; IDs/mask/finite difference и actual DPO gradient; не использовать simulation как настоящий teacher. |
| **G4 P0/P1** | Старый AST/POS/KNN структурный gain вообще присутствует в consumer? | отдельные 2×2 A/B: no learned Unit vs learned Unit, feature off/on, same memory, frozen pretrained embeddings, exact cosine before ANN; blind source-disjoint C#/RU grammar, domain-specific. |
| **G5 P1** | Сохранённое обучение реально читается после cold reload и новых уроков? | before / warm after / cold after, exact state SHA, index/encoder version, all old laws, new laws, conditional replay; не проверять одно лишь Count. |
| **G6** | Важно ли улучшение в обычном ответе? | CODEX/Astra independent blind law comparison, false positive, abstain, latency/resources, no broad regressions, report real tested denominator. |

Работы G1+G2 должны открывать возможность реально учиться; G3+G4 идут как две **разные** ветви, а не конкурируют за одно metric score. Не переносить вывод `SFT repairs worked` на `existing RP01 LoRA now works` без нового end-to-end сравнения.

## 6. Skills / review / prompt-contract

Прочитаны `llm_mvp_core/docs/skills` для architect (`trainable-architecture-decision-SKILL.md`), phase planning (`plan-self-validation-SKILL.md`), plan review (`plan-review-SKILL.md`), code inspection (`code-examination-SKILL.md`), prompt patch (`prompt-patch-authoring-SKILL.md`), human communication (`human-readable-architecture-audit-SKILL.md`), experiment design и claim verification. Проверочный принцип: **показать WRONG и CORRECT для одного bottleneck**, связать каждый результат с exact reviewed commit и caller, разделить `SOURCE_CONFIRMED`, `HISTORICAL_RESULT`, `QUALITY_CAUSAL_PASS`, `NOT_RUN`. Чужой report — evidence-only, не новая команда на изменение core.

**Готовый bounded prompt для CODEX, модель Astra 6 UltraCode (ещё НЕ ЗАПУЩЕН):** «Прочти названные skills, N33-A..E, source at SHA32ac6e7, related RP01 diagnostic. Проверь `Embedding` scatter-add в native finite difference и первый supervised learning-rate без иных изменений; сверяй independent readback. На независимых examples и без self-authored DPO проведи source-family disjoint обучающий A/B. Если нужны новые DPO: сначала в роли человека выбери case; external authorized open Chinese model генерирует chosen/rejected; отдельно CODEX/Astra независимо оценивает пару до admission. Сохрани exact state IDs, trained consumer trace, before/after warm+cold answers, false accepts и regression; верни source:line, один reviewed commit SHA, `PASS / NO_GAIN / BLOCKED / NOT_RUN`. Не изменяй `core/docs/plans/replanning-possible-diffs`.»

**Финальный research verdict:** новая доказанная конструкция source существует (N33-A/B/C/D), но её влияние на уже завершённый RP01 и общий user learning quality **ещё не доказано**. Это существеннее очередного декоративного теста: алгоритмический path может быть не способен выполнять предполагаемое supervised обновление, тогда как другие исторические механизмы работают независимо. Минимальные причинные пробы выше специально отделяют эти факты.
