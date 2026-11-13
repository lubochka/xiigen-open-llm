# 03. Короткие доменные эксперименты

19 октября 2026. **Все native/learning опыты ниже — проект, NOT_RUN.** Единственное новое исполненное наблюдение пакета — арифметический NumPy FP32 расчёт N48-02, который не является native обучением.

## 1. Порядок, позволяющий быстро получить причинный ответ

Начать с **R25-MEMORY**, затем проверить **R25-CONSUMER**; далее R26, R09 и R43 по соответствующему предметному получателю. Отдельно выполнить R-GRANULARITY/R-CORPUS, если actual logical route использует данный corpus builder. После появления полезного состояния — R-RESTORE. Новые N48 остаются ниже этой recovery-очереди; N48-02 требует численного следа до предметного A/B.

Размеры наборов далее — стартовые небольшие бюджеты, а не обязательная квота или порог приёмки. Один раздельный вопрос, один меняемый фактор, одна замороженная панель. Не запускать полный E01–E53 sweep, Main500 или Windows CI ради первого ответа.

### Общий контракт измерения

- У каждой стороны A/B одна версия входных данных, один EXAM и сопоставимое исходное состояние. Если меняется объём обучающих примеров, это прямо раскрывается; equal-budget contrast при необходимости отдельный.
- TRAIN/EXAM делить по исходному файлу, предложению, шаблонному или смысловому семейству **до** извлечения POS, AST, пар и перефразов. Переводы одной исходной фразы в разные направления не расходятся между TRAIN/EXAM.
- Экзаменационный gold фиксируется до ответов обеих сторон; raw outputs сохраняются без регенерации «неудачных» случаев. Знакомые архивные панели служат диагностике, новые выводы о переносе требуют новых семейств.
- Писать фактического получателя, конкретную entry point, изменившееся state и того, кто его прочитал. Базовый результат, A/B improvement и отрицательные контроли выражать числами правильных/всех, а не только margin/PASS.
- Новые DPO: внешняя открытая китайская модель создаёт chosen **и** rejected; CODEX с astra 6 ultracode выполняет человеческий отбор и отдельное независимое review точных пар. Зафиксировать реальные model/provider identifiers при исполнении; не приписывать старой simulation другой источник.
- Время и память измерять для actual local process; времени подготовки/API при отсутствии учёта не присваивать ноль. Поиск конкретного механизма не требует отчёта о производительности всей платформы.

Эти границы следуют из разделения уровней материала/обучения/оценки в [RAG measurement guide][ragmetrics], [training lifecycle][raglife], [translation plan][ragtranslation] и из ограничений прочитанных [исторических результатов](01-MECHANISMS-AND-EVIDENCE.md).

## 2. R25-MEMORY — вернуть научение POS-памяти из language_translate

**Домен:** перевод технических текстов. EN→RU и RU→EN — отдельные профили. **Получатель:** SemanticCompiler.PatternClassifier проекта 12. **Вопрос:** дают ли сами сохранённые примеры полезный прогноз структуры на новых семьях?

**Малый набор.** Около 24 TRAIN и 16 EXAM: утверждения, вопросы, императивы, переставленная тема/объект. Для каждого зафиксировать исходное предложение, POS sequence и независимо проверенный gold из поддерживаемых классов. Не считать правильный класс доказательством полного правильного перевода.

| Arm | Создание состояния | Вызов на общей EXAM |
|---|---|---|
| A | Новый PatternClassifier(k:3), без Train | Predict; штатный rule fallback |
| B | Такой же объект; единственное изменение — Train(trainPosAndLabels) | Тот же Predict |
| C, отрицательный контроль | Те же TRAIN POS с заранее переставленными labels | Тот же Predict |

Снимать train count, сохранённые метки, top neighbors, prediction и confusion question↔imperative / SVO↔SOV. Основной показатель — правильная структура на новых семьях; macro accuracy и по классам. Простые императивы, где rules уже верны, — контроль отсутствия ненужной регрессии. При B≈A/C не считать память восстановившей полезное обучение; если B лучше A и C хуже B, перейти к consumer. Маленькая панель даёт диагностический вывод, а не точную оценку универсального процента.

**Повтор старого sanity:** публичный dispatcher можно вызвать из отдельного небольшого harness с ProjectReference; 35.ExperimentsV2 — библиотека net9.0, подтверждённого CLI --exp нет.

```csharp
var dispatcher = new ExperimentsV2.ExperimentsV2Dispatcher();
var donorReceipt = dispatcher.RunE25(tier: 1);
```

Это проект вызова, не выполненная команда. Не увеличивать tier для видимости масштаба; отдельный результат старой synthetic panel сохраняется под собственным именем. [PatternClassifier][pc12], [E25 driver][e25driver], [dispatcher][dispatcher], [project][experimentproject].

## 3. R25-CONSUMER — вернуть полезный структурный сигнал в actual KNN

**Получатель:** TransformerEmbeddingKnnLogicalUnit, который сейчас добавляет structural bonus по SemanticCompiler29.PatternClassifier. **Вопрос:** улучшает ли trained POS result выбор нужного сохранённого ответа?

До A/B зафиксировать один общий text→POS ingress и payload, доступный обеим сторонам. Нынешний rules-classifier получает words, donor — POS: если вместе с заменой classifier обучить новый POS extractor, это два фактора. Общая подготовка ingress учитывается отдельно; её качество не приписывается замене classifier.

A — нынешний fixed classifier в том же scoring slot. B — обученный PatternClassifier с той же замороженной POS памятью, что прошла R25-MEMORY. Vector/lexical scores, список кандидатов, corpus rows, thresholds, k и модель embedding одинаковы. Не повышать одновременно structural weight.

Для примерно 12 EXAM запросов предусмотреть 2–3 кандидата с близкими словами и разной требуемой структурой/ролью. Сначала фиксировать полезный raw ответ получателя; если все кандидаты возвращают одинаковый общий capability-текст, эта панель проверяет лишь routing и не годится для claims о переводе. Записать old/new structural label, .20 contribution, selected example, raw output и ordinary consumer result. Отрицательный контроль — одинаковая структура при разных значениях слов: POS не должен заменять lexical/semantic различение.

Опровержение интеграции: B меняет локальный label, но consumer не вызывается или output не меняется. Это LOCAL_SIGNAL_ONLY. Следующее действие — исправить установленный call path, а не объявить KNN memory бесполезной. [Current classifier][pc29], [fusion][fusion], [previous scope][prevpos].

## 4. R26-CONTEXT — вернуть левый и правый контекст значения

**Домен:** перевод технических терминов; профиль RU→EN удобен для прямого donor. **Получатель:** ContextualDictionary, затем отдельно конкретный translation Unit.

Взять около 12 TRAIN entries для трёх неоднозначных терминов и 12 новых sentence families. Например, различать stream/thread для «поток» в контексте ввода-вывода и выполнения; конкретный выбранный смысл проверяется по полной фразе. Не задавать gold только по привычному переводу одного слова.

A и B получают **одинаковые** entries, stemmer, priorities и исходные слова. A использует TranslateBaseline, B — TranslateWithContext с frozen left/right POS. Перестановка или удаление одного из контекстов — диагностический отрицательный контроль. Снимать shortlist, выбранное значение, отдельно unknown/неподдерживаемые слова. Сам selector не обязан уметь abstain; его fallback явно учитывать как answer, если он выдан. Добавить empty-both, left-only, right-only и полный контекст; произвольное specific значение при неизвестном контексте не назначать gold. Соседний агент уже описал этот конкретный риск и потерю порядка в Any-overlap — [N44 E26][latee26].

Такой A/B проверяет пользу функции. Отдельный follow-up «до/после AddEntry нового значения» проверяет приобретение знания из нового урока. Не смешивать эти два вывода.

В integration trace вывести поля, реально дошедшие до Unit. Если ему передаётся только prevConcept, а правый контекст потерян, сначала фиксируется контракт данных; не менять одновременно память, scorer и tokenizer. Для документации терминов это может быть полезно, для C# next-AST-node нужны другие признаки. [Donor dictionary][dict12], [E26 driver][e26driver], [current tokenizer][token27].

```csharp
var donorReceipt = dispatcher.RunE26(tier: 1);
```

У E26 tier не расширяет сохранённую 15-case панель; этот вызов — только donor sanity.

## 5. R09-CONSUMER — вернуть структурный KNN для C#

**Домен:** программирование C#. **Получатель:** StructuralKnnPredictor и публичный SemanticCompilerFacade.Compile через private PredictStructuralNextTokens; далее подтверждённый caller поля CompilationResult.PredictedNextTokens.

Подготовить примерно 8 TRAIN и 4 EXAM целых файлов из разных source families. Единый codec SyntaxKind target, пары строго внутри файла, одинаковая EXAM для всех вариантов. Это устраняет известные ограничения исторического driver, не меняя архивный E09 JSON.

**L0, функция:** baseline next-kind predictor и actual StructuralKnnPredictor получают тот же TRAIN/EXAM. Зафиксировать реально используемые DistanceTo признаки. SiblingCount и FileId хранятся в StructuralVector, но этим расстоянием не читаются; не добавлять их в метрику внутри consumer A/B. Название BpeId не превращает local kind IDs в byte-BPE.

**L1, потребитель:** один corpus и обученный predictor; A — этот прогноз не подключён к выбранному recipient, B — подключён. До оценки проверить flag, C# profile, TrainingSize≥10 и actual read прогнозируемого поля. Выигрыш L0 без read в L1 не означает восстановленное поведение продукта.

Primary local metric — правильный следующий kind на общей панели; downstream — корректность нужного изменения/дополнения программы, если actual consumer его производит. Отрицательные контроли — файлы с тем же bag of kinds, но иным parent/scope/order. Новую scope feature не добавлять одновременно с включением уже обученного predictor.

```csharp
// files: frozen IReadOnlyList<(string Path, string Text)>; не пустой.
var donorReceipt = dispatcher.RunE09(
    source: files[0].Text, files: files, k: 5, tier: 2);
```

Показанный dispatcher воспроизводит старый driver. Для нового L0 scorer нужна описанная общая панель; нельзя приписывать исходному driver исправленную процедуру. [E09 source][e09driver], [predictor][structural12], [gates][facadeconsumer], [dispatcher][dispatcher].

## 6. R43-RETENTION — сохранить старые знания при новом обучении

**Предметные наборы:** например, документация control-flow и документация I/O в C#. Это две темы внутри предметного домена, не два разных нейронных механизма. Получатель должен быть одним фактически обучаемым owner.

Старт: около 8 старых TRAIN и 4 старых EXAM; измерить исходный результат. Новый этап — около 8 новых TRAIN и 4 новых EXAM. A перестраивается/обучается на новых данных; B получает те же новые плюс заранее выбранные 4 старых replay. Заявленный бюджет: new=8, replay=4; увеличение total rows не скрывается. Equal-total-budget вариант, если нужен, имеет отдельное имя и сравнение.

Снимать старые/новые correct counts, конкретные gains/losses, state additions/overwrites и обычный output. Replay не включает EXAM; bigram pairs не соединяются через границы файлов. Shuffle/selection правило фиксировать до запуска.

```csharp
var donorReceipt = dispatcher.RunE43(files: files, tier: 2);
```

Это sanity для исторической bigram memory. Чтобы исследовать neural forgetting, нужен собственный neural recipient, его checkpoint/optimizer и новый A/B; успех E43 эту проверку не заменяет. Полезное retained state затем использовать в R-RESTORE. [E43 source][e43driver], [existing boundary analysis][prevreplay].

## 7. R-GRANULARITY / R-CORPUS — память ветвей и восстановление полного корпуса

### R-GRANULARITY

**Домен:** задачи документации C#; второй отдельный профиль набора — SQL-миграции, если actual recipient их поддерживает. Механизм — corpus projection, не новая метрика KNN.

Около 2 subphases × 2 ветви. A — нынешний grouped corpus. B — те же уроки отдельными permitted rows с уникальными source-bound IDs. Policy/tokenizer/embedding model, fusion, thresholds, outputs и DPO material неизменны. Каждый branch discriminant пометить позициями в **реальных** token IDs, включая случаи до/после maxLen=64.

Панель: новые запросы поздних ветвей, ранних ветвей и нерелевантные запросы. Снимать nearest rank, selected example, правильную subphase и raw output. Если output всех ветвей одной subphase одинаков, смена nearest ID не является subject-quality gain.

Старую ошибку duplicate IDs не воспроизводить как treatment. Current builder — BuildKnnTrainingCorpus(replayManifests, registrationPlans).RuntimeExamples; отличие от baseline branch examples — известный U-N2/PG-07. [Current caller][newgroup], [old caller][oldgroup], [duplicate counterfactor][prevgroup].

### R-CORPUS

Взять разрешённый replay manifest с несколькими nested KnnExamples. A — фактическая нынешняя реконструкция; B — полный corpus closure данного owner с корректными identities. Сначала при одинаковых weights сравнить counts, IDs, outputs и доступность старых запросов. Это без neural updates.

Если другой actual loader уже полностью раскрывает nested examples, гипотеза для этого path отвергнута. Если B возвращает потерянные правильные ответы, причинный эффект относится к serving memory, а не к replay ratio AdamW. [Current builder][newknnrestore].

## 8. R-RESTORE — одинаковый ответ и одинаковое продолжение

Вход — полезное state из предыдущего опыта или законно доступный положительный архив. Не использовать 0/4→0/4 как доказательство сохранения первоначально полезного навыка.

A — live owner. B — новый процесс, восстановивший его штатное состояние. Для Unit сравнить corpus/mappings/rules/keys/feature model/route; для Student — base/LoRA/tokenizer/framing. На одной панели сравнить raw output, correctness, selected route и отказ/принятие. Совпадение всех 24 ответов означает parity, а правильность считается отдельно.

Для continuation — отдельная вторая часть: один и тот же следующий урок/update после live и restored state; проверить m/v/step/RNG для Student либо корректное добавление/замещение memory для Unit. Equality конечных output до обучения не доказывает equality следующего update.

Современный P25 уже сохранил состояние P125 B_after с 24/24 output/route parity и качеством 19/24. Его повтор не нужен как новая находка. Использовать его как положительный контроль процедуры и проверять новый относящийся к гипотезе state. [P125][memory125], [P25][cold25].

## 9. N48 — маленькие причинные пробы новых гипотез

| Опыт | T0: самый дешёвый проверяемый факт | A/B и независимый gold | Отрицательный контроль / остановка |
|---|---|---|---|
| N48-01 E14 | Actual ActionLabelGenerator на 6 source fixtures из [02](02-NEW-HYPOTHESES.md), без обучения | Только labels producer меняется, features/k/corpus остаются; gold внешней защиты и требуемого действия готовится независимо | Invoked Guard защищает x; blanket nested pruning недопустим. Если caller отключён — не заявлять RP01 cause |
| N48-02 AdamW | Read-only actual moments scan; затем один Parameter: Step1(g=1e20f), actual portable roundtrip, затем Step2–4(g=1,−1,1) в новом AdamW | После найденного trigger: same last-good state, только безопасное ограничение gradients; новое/старое качество отдельно | Fresh small-gradient state; finite weights при Inf-v недостаточны. Без trigger — не расширять learning pilot |
| N48-03 recipe | Один single-branch current/replay text triplet: оба vectors, counts, signals, path version | При разных counts — два разных pair gradients, одинаковые actual optimizer/material; затем качество реального output | Равные counts, одна pair scale, одинаковые density arrays. Legacy immediate loss не является сигналом |

Предметный размер для learning части — порядка 8–16 новых закрытых случаев, если T0 подтвердил релевантность. Это предложение стартового бюджета, не обещание статистической мощности. Раздельно использовать C# API/null semantics, технические переводы, subphase routing для документации/миграций. Полные source chains и условия опровержения — [02](02-NEW-HYPOTHESES.md).

## 10. Остальные запрошенные оси: один диагностический контраст до масштабирования

Эти темы **уже присутствовали** в старых исследованиях; таблица связывает их с минимальным опытом, чтобы охват не потерялся.

| Ось | Малый опыт | Что означает результат |
|---|---|---|
| Качество DPO | 8–16 exact pairs по одному предметному закону; обе стороны генерирует заданный teacher, отдельное CODEX review; контролировать одинаковую ошибку при разных формулировках | Ошибочная/эквивалентная пара не должна поступать в training; хороший review не доказывает learning |
| Masks, objective, gradient | Tiny actual recipient: directional numerical-vs-analytic check одного masked response; prompt исключён; reference frozen | Один адресный mathematical defect либо подтверждение проверенного path; не общий production pass |
| Исходная компетенция Student | Same B0: DPO-only против заранее определённого короткого chosen-CE warmup + того же DPO; отдельно раскрыть добавленный compute | Зависимость от начального состояния, не «исправленная LoRA»; trade-off бюджета открыт |
| LoRA targets/rank | Только после tiny overfit проблемы: одна ось q/v targets либо rank, та же база и панель | Actual ΔLoRA/Δlogits плюс правильные outputs; moved weights не sufficiency |
| Tokenizer и длина | Read-only IDs/masks/length для source/chosen/rejected и budget stops; same-byte spellings/операторы | Найденная несовместимость или усечение; UTF-8 roundtrip не semantic suitability |
| DPO→под-Unit | Один правильный полный triplet; A/B одного projection rule: position alignment, keys или context | State правильного Unit и новый ordinary output; не только правильность полной пары |
| Topic/operator scope | Одинаковое слово в двух темах; либо x==y / x!=y при одинаковых остальных полях | Collision только если фактически совпал ключ и изменился answer/route; темы должны допускать перенос общих правил |
| Unit metrics/fallback | Один actual candidate через Verify и отдельно ordinary adapter; valid/invalid, negation/order | Correct accepted и wrong accepted раздельно; output coverage не correctness |
| Flow decomposition/quality | 4–6 запросов с числом требований выше/ниже cap; good/bad/unavailable checker | Сохранены ли требования, выполнен ли checker, какое решение принял следующий consumer |

Источники действующих задач: [RP01 recovery][rpplan], [DPO-quality review][prevquality], [BPE identity][prevbpe], [subunits][prevsubunits], [role/alignment][prevroles], [decomposition/checker][prevdecomp], [confidence][prevconfidence]. Этот пакет не требует одновременно выполнять всю таблицу: выбрать следующий контраст по найденному bottleneck.

## 11. Результат одного опыта: короткая форма

```text
experiment_id / source_commit / actual_entry_point:
subject_domain / profile / recipient:
hypothesis / sole_changed_factor:
train_families / exam_families / exact material hashes:
initial_state_A / initial_state_B:
actual_state_delta / actual_consumer:
correct_before, correct_A, correct_B (numerator/denominator):
retention / wrong_accepted / unsupported:
paired_gains_and_losses_with_raw_examples:
runtime_scope_seconds / memory_bytes / API_cost_known_or_unknown:
verdict: MECHANISM_ONLY | NUMERICAL_DIFFERENCE_ONLY |
         QUALITY_IMPROVED_ON_THIS_PANEL | NOT_SUPPORTED |
         NOT_REACHED | NOT_RUN
next_action:
```

Вердикт формируется по выполненному опыту. Никаких заранее проставленных PASS или обязательных процентов улучшения. После одного достаточного causal result перейти к установленному ремонту либо следующей гипотезе; не наращивать повторения ради отчётности.

[ragmetrics]: https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/dpo%20legal%20build/claude-05-dpo-data-training-measurement-guide.md
[raglife]: https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/dpo%20legal%20build/active-codex-execution-plan-10-training-lifecycle.md
[ragtranslation]: https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/language%20translate%20learning/TRANSLATION-FOUR-LEVEL-LLM-TRAINING-PLAN-v1.md
[pc12]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Translation/PatternClassifier.cs#L29-L86
[e25driver]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E25PatternClassifier.cs
[dispatcher]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/ExperimentsV2Dispatcher.cs
[experimentproject]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/ExperimentsV2.csproj
[pc29]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/29.SemanticCompiler/Translation/PatternClassifier.cs#L24-L65
[fusion]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L205-L300
[prevpos]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/17-positive-pos-knn-versus-production-rules.md
[dict12]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Translation/ContextualDictionary.cs#L22-L110
[e26driver]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E26ContextualDictionary.cs
[token27]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationSemanticTokenizer.cs#L294-L363
[e09driver]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E09StructuralKnn.cs
[structural12]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Prediction/StructuralKnn.cs
[facadeconsumer]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/SemanticCompilerFacade.cs#L295-L334
[e43driver]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs
[prevreplay]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/68-e43-replay-boundaries-and-reachable-quality-question.md
[newgroup]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L952-L969
[oldgroup]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L284-L290
[prevgroup]: https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/PREPARETOGPL-UNITS-RESTORE-2026-10-18.md
[newknnrestore]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L3467-L3508
[memory125]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md
[cold25]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md
[rpplan]: https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md
[prevquality]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/14-data-quality-and-evaluation-alignment.md
[prevbpe]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/12-bpe-retrain-identity-and-consumer-causal-gap.md
[prevsubunits]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/13-domain-context-and-dpo-subunits.md
[prevroles]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/44-structured-lessons-roles-keys-and-word-alignment.md
[prevdecomp]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/34-decomposition-quality-and-execution-verdicts.md
[prevconfidence]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/72-negation-confidence-and-e46-feedback-consumers.md

[latee26]: https://github.com/lubochka/xiigen-planning/blob/3215267c0ee2c2beafebec950b45b7fee326daf7/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/44-E26-UNKNOWN-CONTEXT-AND-ORDER-2026-10-19.md
