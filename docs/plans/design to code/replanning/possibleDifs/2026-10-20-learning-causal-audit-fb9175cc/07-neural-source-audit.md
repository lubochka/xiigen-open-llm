# Native learning: проверенные результаты, границы измерений и новый кандидат

> **Граница источников:** основной текст ниже относится к core `fb9175cc`. Перед публикацией отдельно проверен новый commit `d962f958`: [позднее уточнение](10-late-core-delta-d962f958.md), [актуальный реестр](04-registry.md). Исторические номера/статусы и счётчики 163 cards / 8 prompts не являются повторной аттестацией нового среза. NEXT сейчас — №10 PARENT-ADOPTION-01; старые raw и попытки сохраняются.

Дата аудита: 20 октября 2026. Core: fb9175cc752d41e1cb525e0609b7c96140940217, ветка codex/dpo-implementation. Planning: fc0dd1b04d61a963906cdc1ab29dffd2ee9e520c, ветка llm_plan3.

## Вывод

Найдена одна новая недублирующаяся гипотеза **SILU-SIGMOID-REUSE-01**: сохранять вычисленный в forward знаменатель SiLU для backward, чтобы проверить затраты полного native SFT update. Сейчас это source-supported candidate, **не измеренное улучшение**. Карточка с manifest 36 исходников, точным host, бюджетом, данными, before/after, правилами результата и встроенным промптом подготовлена со статусом AUTHOR_SETUP_REQUIRED. Готового пригодного H64 checkpoint не найдено.

Уже существующие native результаты нельзя объединять в один вердикт «модель не обучается». LEARN-SANITY-PROBE действительно получил ненулевые однотокенные правильные ответы; RESTORE-MINIMAL-SET восстановил веса, сообщил успешный импорт optimizer state/step и показал продолжение обучения по NLL/токенам; N63R-PROBE сохранил больше токенов старого урока и близкого EXAM с replay. Каждый результат имеет конкретный домен и отдельные ограничения. Улучшение полезного показателя сохраняется как успех по этому показателю; ухудшения и неодинаковые бюджеты показываются рядом.

В этой работе ни один эксперимент не запускался, production не менялся, защищённая папка docs/plans/replanning-possible-diffs не редактировалась. Загружены и прочитаны сохранённые источники и JSON; общий реестр находится в 04, сведения о публикационном review — в 09.

## 1. Покрытие DPO, LoRA, tokenizer, математики и ролей учителя

Это адресный current-source аудит с дедупликацией, а не утверждение, что каждый путь всего репозитория исполнен или каждую из 929 карточек прочитали содержательно. Результат источника, вызванный обучающий путь и измеренное обучение различаются.

| Область | Текущий source и вывод | Уже существующие ID / покрытие | Что остаётся доказывать |
|---|---|---|---|
| DPO objective и gradient | [DpoTrainer.cs, 878–924][dpo-step] сохраняет policy graphs обоих ответов; [1014–1029][dpo-graph] делает training=true forward; [1072–1120][dpo-backward] использует derivative oneHot−softmax и backward по сохранённым logits. Нельзя утверждать «LoRA не обучается, потому что logits оторваны» для этого текущего пути. | P10-composed-gradient; P11-autograd-reachability; E-DPO-VALID-LENGTH; E-DPO-SFT-ORDER | Конечность/направление gradients, корректные masks и полезный свободный ответ на конкретных исходных парах; source-правильность не заменяет native learning before/after. |
| Семантический DPO-вектор | [VectorSignal, 1160–1167][vector-signal] читает только число слотов с abs(value)>1e-6 и возвращает clamp(1+count/60,1,2). Это scalar multiplier, не полноценное использование значения и структуры каждой координаты. | P06-teaching-vector-adamw; ранее описанные RP01/P122/atlas материалы | Путь реального потребителя и сравнение matched lessons; нельзя повторно регистрировать эту старую гипотезу как новую. |
| LoRA-механизм | [LoraLinear.Forward, 113–131][lora] действительно добавляет scaled x·Aᵀ·Bᵀ к base. [LoraApplicator, 398–480][lora-apply] использует target modules и trainable/frozen состояние. q/v-only и frozen lm_head в LEARN-SANITY — проверенные настройки, не доказательство полного отсутствия градиента. | P02-warm-base; P11-autograd-reachability; N-HEAD-01; NEURAL-FROZEN-TRANSPOSE-01 | Полезность иных target modules или trainable head зависит от данных/домена и требует отдельного равного native опыта. |
| SFT, warmup, optimizer continuation | [Trainer, 117–147][trainer] делает настоящий CE/backward/clip/AdamW. Общая warmup проблема не переносится автоматически на incremental path: [IncrementalTrainingSession.cs:405][session] прямо ставит perCallConfig.WarmupSteps=0. Моменты и шаг экспортирует [AdamW.cs, 188–289][adamw]. | E-SFT-MOMENT-CONTINUITY; E-SFT-RESPONSE-MASK; P25-cold-restore; 56/59 atlas материалы | Конкретный восстановленный S0 и влияние продолжения оптимизатора; проверять scheduler вместе с caller, а не один default. |
| Tokenizer и длина входа | [BpeTokenizer.Train/Encode, 42–87][bpe-train] обучает merges/vocab и затем использует их; [MergeChunk/ApplyMerges, 548–572][bpe-merge] участвуют в реальном tokenizer training/encoding. [EosToken:38][bpe-eos] трактует последний vocab ID как EOS, но caller должен действительно использовать это значение. | BPE-NO-MATCH-COPY-01; BPE-REALIZED-MERGE-COUNT-01; BPE-WEIGHTED-CHUNKS-01; DPO-TOKENIZER-SNAPSHOT-01; N63-INGRESS-LENGTH | Не смешивать tokenizer compression/Train cost и downstream KNN/neural accuracy. Риск EOS не объявлять причиной RP01 без фактического generation parameter. |
| Tensor/autograd/math | [TensorOps.Silu][silu], [SwiGLU][ffn], [TransformerBlock][block] соединены с native training. Повторный Exp в SiLU — новая candidate; сохранённый backward в current — полезное уже сделанное исправление. | P11-autograd-reachability; RMS-COORD-ALLOC-01; RMS-ROW-REDUCE-01; ROPE-TRAIN-TABLE-01; SFT-EXP-REUSE-01; NEURAL-FROZEN-TRANSPOSE-01 | Измерение полного native Train, float/state parity и цена дополнительных массивов; отдельный операторный stopwatch не даёт training result. |
| DPO pair construction и качество учителя | [SimulationDpoInputRow и validation][sim-input] передают prompt/chosen/rejected, отсекают ties/duplicates; [simulation material, 114–124][sim-material] сохраняет typed deterministic authorship и prepared token counts. Это проверенный simulation route; он не доказывает Chinese-authored, независимо оценённое качество обоих ответов. | P04-preference-contrast; P109-gold-validity; P102-teacher-forced-decoding; REC02/REC03 | Качество и source-disjointness обеих сторон, пригодность B0, distinction teacher-forced и свободного ответа. Экспериментальные исключения пользователя допустимы, но simulation нельзя переименовать в результат внешнего учителя. |

Назначенные пользователем роли сохраняются буквально: CODEX (Astra 6 ultracode) выбирает/оценивает DPO, внешняя открытая китайская модель генерирует пары, исполнитель работает на default/inherited модели текущей сессии. Ни source audit, ни новая численная SiLU карточка не меняют эти роли. В SiLU карточке новые DPO не генерируются и все teacher calls равны нулю.

## 2. Результаты, сверенные непосредственно с raw

Ниже приведены сохранённые native артефакты. Числа не получены новым запуском. Исторический core_commit внутри raw может отличаться от SHA репозитория, на котором файл сейчас сохранён; эти provenance не сливаются.

### 2.1. LEARN-SANITY-PROBE: положительная обучаемость H64

[Program.cs][sanity-source] использует native Trainer.TrainSupervisedNextTokenStep: H64/V256/L1/Q4/KV2/FF176, q/v LoRA rank4/alpha8/dropout0; frozen lm_head; fixed LR0.001, seeds62001/62002. Две учебные строки и две близкие EXAM строки, ответ один byte token. [probe-result.json][sanity-raw] записан с internal core_commit=62bcc8f2368840e22d2ea98ef16d9bd526ad9a39.

| Метрика | До, 0 updates | После, 400 updates | Оценка |
|---|---:|---:|---|
| TRAIN exact | 0/2 | 1/2 | Измеренное улучшение |
| TRAIN mean answer NLL | 7.960885496452843 | 1.8879445998230966 | Измеренное улучшение |
| TRAIN mean gold rank | 206.5 | 1.5 | Измеренное улучшение |
| EXAM exact | 0/2 | 1/2 | Измеренное улучшение на двух близких строках |
| EXAM mean answer NLL | 8.01533412366764 | 2.7988595276200154 | Измеренное улучшение |
| EXAM mean gold rank | 215 | 2.5 | Измеренное улучшение |

Это не 2/2: TRAIN после 400 отвечает + обеим строкам, EXAM отвечает - обеим. На точках 4/20/100 exact ещё 0/2. Loss обучает всю последовательность, а ответ занимает одну позицию. Учебные и EXAM результаты не равны доказательству общей способности писать код.

**Критичный результат проверки артефакта:** Program сохраняет JSON, а не веса/optimizer; в проверенном дереве семь текстовых файлов и нет H64 checkpoint. Поэтому новый опыт получает честный статус AUTHOR_SETUP_REQUIRED и фиксированный план подготовки S0 за 400 updates. Нельзя назвать random H64 «почти дообученной моделью» и нельзя скрыть эти 400 updates из бюджета.

### 2.2. RESTORE-MINIMAL-SET: restore работает, продолжение измеримо, полных ответов нет

[Исходник][restore-source] использует SaveCheckpoint/LoadCheckpoint и ExportPortableState/TryImportPortableState. [Raw][restore-raw] подтверждает bit-exact восстановление named weights: hashAfterPriorTraining=hashAfterLoad=db44f1a437653299b345dc83c85523aad1ba1c55d3285865fe92501e5e78e386. Новый объект до загрузки отличался. Import=true, optimizer step1000→1000 после restore и 1200 после continuation; readout сразу после restore совпал.

| Показатель | До continuation, restored | После 200 native updates |
|---|---:|---:|
| TRAIN correct response tokens | 3/40 | 4/40 |
| TRAIN mean response NLL | 4.766696420187372 | 4.711598363502413 |
| TRAIN mean response gold rank | 51.825 | 50.05 |
| TRAIN full teacher-forced response exact | 0/2 | 0/2 |
| EXAM correct response tokens | 0/43 | 0/43 |
| EXAM Three response NLL | 5.527751288639532 | 5.4992039045038466 |
| EXAM Four response NLL | 5.769119468000703 | 5.677866738414823 |
| CONTROL correct response tokens | 0/28 | 0/28 |
| CONTROL response NLL | 5.8079633717111525 | 5.857194149707198 |

Улучшения NLL и train response tokens сохраняются как полезные показатели. CONTROL NLL ухудшился. Полноценный ответ не появился, и эта H16 модель не является пригодным H64 donor для новой карточки. Заявление raw о проблеме adoption/parent checkpoints не превращается в доказательство, что native restore сломан: конкретный механизм в этой пробе восстановил веса, сообщил успешный import optimizer и сохранил step. Отдельное побайтовое сравнение post-import m/v export не выполнялось; наличие import=true не подменяет его.

В tree имена weights.bin/optimizer.bin имеют размеры 130/129 bytes, соответствующие Git LFS pointers; payload нами не загружен и не верифицирован. JSON доказывает запись результатов запуска, а не сегодняшнюю возможность открыть эти бинарные payload.

### 2.3. N63-INGRESS-LENGTH: достижимость настоящего обучения улучшилась

Проверены [N63I/Program.cs][n63i-source], [arms.json][n63i-arms] и [per-row.json][n63i-rows]. A-full передаёт три строки штатному IncrementalTrainer; B-bounded заранее исключает только over-context train-long по точному условию 2≤encoded_length≤ContextLen+1. Ни prompt, ни response не обрезается. Общий S0 действительно одинаков по hash, но это random base, не заранее выученный checkpoint.

| Метрика | A-full | B-bounded |
|---|---:|---:|
| Реальные optimizer updates | 0 | 2 |
| Реальные shifted targets | 0 | 70 |
| Изменение weights | Нет | Есть |
| Результат | TRAINING_EXAMPLE_CONTEXT_EXCEEDED | COMPLETED |
| Отложено оригинальных строк | 0; весь batch отказал | 1, train-long |
| native elapsed_ms | 279.9091 | 111.3761 |
| process CPU_ms | 62.5 | 93.75 |
| allocated bytes | 81992 | 3330408 |

Это успех по разблокированию 2 настоящих updates и 70 targets; меньший elapsed наблюдается рядом с ростом CPU/allocations, но A не обучалось, поэтому эти числа не обозначают стоимость одинаковых updates.

В B response NLL train-one уменьшился 6.554350670659838→6.550180174020116, train-two6.636790247616372→6.634059538608386. EXAM Three6.699461780801719→6.697845993180661; Four6.528147523227716→6.5275596130608395; CONTROL6.566935850077769→6.565852480334515. Во всех пяти scored rows correct response tokens остались 0, full exact=false. train-long имеет NOT_SCORED_OVER_CONTEXT и не может получать выдуманный NLL=0. Произошло 20 scoring forwards, их стоимость записана отдельно.

Итак, пользовательское правило полезных метрик сохраняет успех admission/updates и NLL. Правило 15 о ненулевом минимальном обучении не позволяет объявить этот запуск доказательством освоенного ответа. №08 consumed; не повторять его в попытке привести результат к другому вердикту.

### 2.4. N63R-PROBE: replay помог старому уроку, одновременно изменились затраты и новые уроки

Оригинальный №09 N63-AR-CAP имел бюджет 9 updates и 32 scoring forwards с 0 correct response tokens до/после; его нельзя объявлять полноценным before/after обучением. Отдельный [N63R-PROBE/Program.cs][n63r-source] и [n63r-probe.json][n63r-raw] содержат другой уже выполненный диагностический запуск:1000 prior CODE_Q updates,400 BUG updates, затем A800/B1200/C1200, всего 4600 updates и 40 scoring forwards. Его нельзя задним числом приписать бюджету карточки№09.

Перед плечами old lessons имеют 45/92 правильных response tokens, но 0/2 full teacher-forced exact. Это ненулевое частичное знание. A теряет A07 из replay; B сохраняет ему место; C содержит новые уроки какA, но получает 1200 updates какB.

| Правильные response tokens | Обученное WA | A, no replay | B, reserved replay | C, equal updates без replay |
|---|---:|---:|---:|---:|
| Старый A07,49 tokens | 22 | 2 | 15 | 4 |
| EXAM EO02,49 tokens | 15 | 1 | 15 | 3 |
| Старый A01,43 tokens | 23 | 2 | 1 | 2 |
| Новый N01,26 tokens | 0 | 22 | 18 | 24 |
| Новый N02,31 tokens | 0 | 28 | 25 | 28 |

Наблюдаемый успех B: сохранено больше правильных токенов A07 и близкого EXAM EO02, чем уA/C. Одновременно B хужеA по новым N01/N02 и по старому A01. Даже внутри предметной области программирования результат различается по уроку; полный ответ ни из этих token counts не следует.

| Затраты | A | B | C |
|---|---:|---:|---:|
| Updates | 800 | 1200 | 1200 |
| TrainedTokens в raw, полные encoded sequences | 63600 | 118800 | 95400 |
| Shifted next-token targets, derived = TrainedTokens−Updates | 62800 | 117600 | 94200 |
| Allocated bytes | 5863987776 | 12124886728 | 8795992464 |
| Wall_ms | 12217.5506 | 28327.0492 | 17791.1449 |
| CPU_ms | 10406.25 | 22125 | 16484.375 |

B/A: updates+50%, encoded tokens+86.79%, allocations+106.77%, wall+131.86%, CPU+112.61%. C уравнивает число updates, но не tokens и не состав/порядок уроков. Поэтому это честный положительный replay result по удержанию A07/EO02 с затратами и потерями рядом; измерения не изолируют «одинаковое обучение за меньшую цену». Один процесс и порядокA→B→C также не дают независимых peak/time экспериментов. Эти ограничения прямо согласуются с raw resources.axisHonesty.

№09 уже consumed. Нельзя продолжать его, исправлять старую карточку или повторять diagnostic tool после команды NEXT10.

## 3. Новая гипотеза и почему она не повтор

В [TensorOps.Silu:1139][silu] forward вычисляет Exp(−x). Backward в 1152 вычисляет его второй раз. Native [Trainer][trainer]→[LlmModel][model]→[TransformerBlock][block]→[SwiGLU][ffn] достигает этого backward при q/v LoRA даже с frozen FFN weights: градиент нужен для расположенных раньше адаптеров.

B сохраняет denominator=1f+MathF.Exp(−x) в graph-local float[]; forward остаётся x/denominator; backward sigmoid=1f/denominator. Extra arrays могут увеличить allocations и RAM. Математическая равносильность сама по себе не обещает speedup; нужны фактические full Train interval и float/weight/optimizer parity.

Дедупликация выполнена по всему 929 Markdown possibleDifs и 372 Markdown protected. По адресным patterns найдено 8 совпадающих строк planning: упоминания достижимости SiLU/autograd, SHA manifests и соседний SFT-EXP-REUSE. Последний относится к softmax в Trainer.BackwardLoss, а не к SiLU forward/backward. Новый factor не совпадает с RMS-COORD-ALLOC, ROPE-TRAIN-TABLE или frozen Linear transpose. Поиск не выдаётся за полнотекстовый semantic review каждого файла.

Карточка назначает новый короткий host SILU01 и точное исходное замыкание:36.cs из LFT01/src tree e448640bd1f2fe2c65adcc0b5ffcb02e7f5539e6. Target TensorOps blob e1a20dca84d76ee85d0c559bd496c1fa3c96ac8b совпадает с текущим core. Проверенные Tensor/Transformer/Trainer/AdamW/LoRA файлы также совпадают, кроме Linear с experiment cache switch; этот switch фиксируется false. Это не новая реализация альтернативного learner.

S0 не выдумано: автора просят подготовить его ровно 400 updates по положительному recipe, сохранить weights и optimizer, подтвердить минимум 1/2 TRAIN, finite state и optimizer step400. EXAM показать полностью без gate допуска; известный исторический 1/2 EXAM не превращается в обязательный критерий отбора. При неуспехе — stop at fixed bound. Потом A4/B4 updates из одних S0 bytes, optimizer400→404; общий budget408 updates/8568 targets/24 scoring forwards. Нет LR/grid, wrapper-only probe, нового DPO teacher или автоматической интеграции в production.

Статус AUTHOR_SETUP_REQUIRED означает конкретно: host и command ещё предстоит реализовать автору по назначенному месту и source manifest; runtime executor до readiness ничего не ищет и не запускает. Это не повторный запрос GitHub permissions и не остановка уже разрешённого документирования.

## 4. Соблюдённые правила review и ограничения результата

Полностью прочитан docs/skills/generated-code-review-SKILL.md. Применены code-first claims, exact file/method/call-chain ссылки, разделение source/runtime/quality, проверка actual reader/writer вместо выводов по storage receipt, отсутствие production edits у reviewer и честный русский отчёт. Прочитаны релевантные AGENTS положения текущей planning-only фазы и OP50/OP53/OP73: реальный training evidence, человеческое объяснение, сохранение результата, немедленный stop/next. Остальной пользовательский skill stack распределён родительским заданием; эта часть не утверждает чтение всех skills или всего огромного AGENTS без нужды.

Новая карточка содержит отдельный обязательный запрет продолжения старой задачи после STOP/NEXT **в основном тексте и встроенном executor prompt**. В ней назначены модель default/inherited, язык, роли DPO, host/source closure, author task, data/model recipe, S0 gate, budget, before/after/ресурсные метрики, report paths, report sections, no-delete/no-production правила.

Не заявляется:

- что SiLU — доказанная причина исторического ухудшения RP01;
- что legacy/current графы идентичны во всех соседних ветках;
- что 0/2 full responses равны успешно освоенной многотокенной задаче;
- что inference-copy parity или schema validation заменяет изменение learned state;
- что improvement одного урока переносится на все домены;
- что найденный в Git LFS pointer уже скачан и восстанавливается;
- что прошёл новый runtime, build, unit test или production review.

Ненулевые улучшения сохранённых native результатов документируются без требования обязательного роста точности. Новая SiLU гипотеза остаётся немеренной до отдельного разрешённого experiments-only исполнения и решения пользователя об интеграции.

[dpo-step]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/11.DPO/DpoTrainer.cs#L878-L924
[dpo-graph]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/11.DPO/DpoTrainer.cs#L1014-L1029
[dpo-backward]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/11.DPO/DpoTrainer.cs#L1072-L1120
[vector-signal]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/11.DPO/DpoTrainer.cs#L1160-L1167
[lora]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/10.LoRA/LoraLinear.cs#L113-L131
[lora-apply]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/10.LoRA/LoraApplicator.cs#L398-L480
[trainer]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/04.TrainingLoop/Trainer.cs#L117-L147
[session]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/14.ContinuousLearning/Training/IncrementalTrainingSession.cs#L405
[adamw]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/04.TrainingLoop/Optimizers/AdamW.cs#L188-L289
[bpe-train]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/01.Tokenizer/BpeTokenizer.cs#L42-L87
[bpe-merge]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/01.Tokenizer/BpeTokenizer.cs#L548-L572
[bpe-eos]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/01.Tokenizer/BpeTokenizer.cs#L38
[silu]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/00.TensorCore/Core/TensorOps.cs#L1129-L1162
[ffn]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/03.TransformerModel/Layers/SwiGLUFeedForward.cs#L40-L51
[block]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/03.TransformerModel/TransformerBlock.cs#L45-L65
[model]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/03.TransformerModel/LlmModel.cs#L219-L240
[sim-input]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L30-L69
[sim-material]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L114-L124
[sanity-source]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/Program.cs
[sanity-raw]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/probe-result.json
[restore-source]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs
[restore-raw]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/restore-minimal-set.json
[n63i-source]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/experiments-v2/RP01-MICRO/N63I/Program.cs
[n63i-arms]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/experiments-v2/RP01-MICRO/N63I/results/arms.json
[n63i-rows]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/experiments-v2/RP01-MICRO/N63I/results/per-row.json
[n63r-source]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/experiments-v2/RP01-MICRO/N63R-PROBE/Program.cs
[n63r-raw]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/experiments-v2/RP01-MICRO/N63R-PROBE/results/n63r-probe.json


Поисковые patterns и найденные ближайшие упоминания сохранены в [SiLU novelty receipt](evidence/silu-dedup.json).
