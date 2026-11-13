# Нейронный путь: что действительно изменилось после language_translate

Дата аудита: 19 октября 2026. Режим: чтение исходников и сохранённых результатов. Этот документ не сообщает о новых запусках обучения, inference, build или CI. Исходники и защищённый каталог llm_mvp_core не изменялись.

## 1. Что поставить в начало программы восстановления

**Сначала необходимо вернуть успешный путь доставки и использования выученного урока, а затем выяснять дополнительный вклад нейронного Student.** Сохранённые результаты от 18 октября уже различают эти механизмы: память перевода после восьми уроков дала 19/24, холодная загрузка сохранила эти же ответы, а исправление доставки частей урока подняло результат с 11/24 до 19/24. Отдельные нейронные сравнения исправленного autograd пока дали 0/24 → 0/24. Это разные получатели обучения; результат памяти нельзя приписывать изменению LoRA или отрицать из-за результата генератора. [Сохранённый индекс результатов, строки 9–16](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/README.md#L9-L16).

Локальный порядок проверок взаимодействия памяти и Student; общий приоритет исторических R1–R5 дан в [README](README.md):

| Приоритет | Действие | Статус и смысл |
|---|---|---|
| Восстановление 1 | Проверить на текущем пользовательском маршруте сохранение и чтение всех частей принятого урока, включая mappings, обычным Flow | Существующий результат ingress 0→16 mappings и 11/24→19/24 уже сохранён. Это повторная проверка переноса известного исправления, не новая гипотеза этого аудита |
| Восстановление 2 | Проверить обычный вызов выученной памяти и холодное восстановление в той точке продукта, где наблюдалось ухудшение | Существующие P92.1/P92.3 и P25; одинаковые ответы после загрузки подтверждают восстановление, но не новый прирост качества |
| Разделение вкладов | Выполнить согласованный S0M1↔S1M1 replay по P92.2: один корпус, frozen Student до/после DPO, оба индекса построены заново своими encoder | Уже предложено в защищённом корпусе. Ниже уточнены реальные методы; новизна не заявляется |
| Новая проверка нейронного механизма | Проверить масштаб факторов LoRA при одинаковой начальной функции и нулевых моментах оптимизатора | Узкая, ещё не выполненная гипотеза N-LORA-FACTOR-01; отдельная карточка. Она не воспроизводит историческое обучение памяти и не заменяет первые два пункта |

Исходная карточка P92 прямо требует обычного consumer и отдельно задаёт факторное сравнение Student × память. [P92.1–P92.3](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/cards/P92-dpo-memory-flow-bridge.md#L15-L28).

## 2. Фиксация источников и границ

Сравнены две неизменяемые версии:

| Роль | Ветка | SHA |
|---|---|---|
| Исторический ориентир успешного поведения | language_translate | f6032aff81f1f83de7c09cf9b9efe6b058dd12b9 |
| Текущий код аудита | codex/dpo-implementation | a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6 |

Проверены конкретные пути: применение и forward LoRA; вызовы DpoTrainer из LogicalDpoTrainingUnit; materialization prompt/chosen/rejected; response masks и производная log-prob; current Embedding/Linear; параметры генерации; обучение и исполнение TransformerEmbeddingKnnLogicalUnit. Проверка не охватывает каждую соседнюю ветку и каждый возможный внешний caller. Сохранённые model receipts прочитаны как исторические доказательства; DLL, веса и хеши всех их бинарных артефактов в этом аудите повторно не проверялись. Три neural comparison.json представлены в Git как LFS pointers: их большие payload не получены; численные выводы ниже сверены по доступным report/validation и independent-review, а не объявлены собственным пересчётом всех массивов.

Применены заданные пользователем навыки code-examination, generated-code-review, dpo-triple-format и prompt-patch-authoring: вывод привязан к реальному получателю, корректность производной отделена от качества ответа, старые дефекты отделены от текущих. Термины следуют human-communication-GLOSSARY: «перевод» и «документация» — темы обучения, EN→RU — профиль, KNN — механизм, Student/Unit — получатели. Планирование и архитектурные выводы сведены к проверяемым вмешательствам, без переписывания архитектуры.

## 3. Нельзя восстанавливать успешную ветку откатом DPO/LoRA

В language_translate конкретная связка LogicalDpoTrainingUnit → LoraApplicator → DpoTrainer не доказывает успешное нейронное обновление. Она содержит два независимых обрыва. Это не опровержение успешного поведения всей ветки: другие механизмы могли учиться и возвращать правильные ответы.

### 3.1. В старом пути адаптеры создавались отдельно от используемого forward

Старый LoraApplicator замораживает параметры модели, создаёт список LoraLinear и возвращает его. Он не присоединяет адаптеры к projection slots модели. Затем LogicalDpoTrainingUnit сохраняет этот список для отчёта, но передаёт самому DpoTrainer исходную PolicyModel. Комментарий о включении adapter tier не является реализацией такого включения. [Старый Apply, 6–40](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/10.LoRA/LoraApplicator.cs#L6-L40), [старый caller, 338–375](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L338-L375).

В текущем CausalSelfAttention полный forward и cached inference используют общий ForwardProjection. Если адаптер установлен, выполняется его Forward; q/v больше не являются отдельными неиспользованными объектами. [Полный forward, 138–170](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/03.TransformerModel/Layers/CausalSelfAttention.cs#L138-L170), [cached forward и dispatch, 194–259](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/03.TransformerModel/Layers/CausalSelfAttention.cs#L194-L259).

**Классификация:** исторический дефект конкретного consumer, исправленная текущая связь. Повторное обнаружение, не новая гипотеза. Нельзя делать текущий адаптер снова внешним, чтобы «как в language_translate».

### 3.2. Старый BackwardLogProbs не запускал backprop по графу

Старый метод создаёт новый logits, записывает logits.Grad и заканчивается. В нём отсутствует logits.Backward. Кроме того, для переданного dLoss/d(log-prob) записан знак softmax−oneHot, противоположный производной log-softmax. [Старый BackwardLogProbs, 219–263](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/11.DPO/DpoTrainer.cs#L219-L263).

Текущий метод работает с тем же policy graph, который дал log-prob, использует oneHot−softmax, учитывает вес target mask и вызывает logits.Backward. [Текущий BackwardLogProbs, 1072–1120](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L1072-L1120). В StepVector два backward на chosen/rejected накапливаются до одного ApplyOptimizer; обучающий loss не заменяет оценку отложенных ответов. [StepVector, 875–926](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L875-L926).

**Классификация:** исправленный исторический разрыв. Данный аудит не нашёл здесь нового простого sign/transpose дефекта. Утверждение «текущий DPO вообще не двигает LoRA» противоречило бы и исходнику, и сохранённым native receipts ниже.

### 3.3. Историческая успешность должна переноситься через тот же механизм ответа

Current DefaultLogicalCapabilityTrainer сначала передаёт policyModel в ExecuteDpoTeachingAsync, затем передаёт эту же модель в TransformerEmbeddingKnnLogicalUnit.Train вместе с корпусом примеров. Это отдельный путь построения retrieval unit; он не тождественен прямой генерации Student. [Обучение Student, 658–675](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L658-L675), [построение retrieval unit, 961–969](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L961-L969).

Отсюда проверяемое следствие: восстановление этого пути требует, чтобы реальный вызов дошёл до обученного Unit, тот прочитал собственную память и вернул её output. Счётчики DPO steps, наличие LoRA tensors или успешная загрузка Student сами по себе этого не доказывают. Указанный вывод уже соответствует P92, поэтому он включён как проверка переноса известного механизма.

## 4. Состояние возможных различий в DPO и токенизации

| Узел | Старый код | Текущий код и достижимость | Вывод для качества |
|---|---|---|---|
| Формирование пары | Compact(prompt), пробел, Compact(answer); mask всей строки; средний log-prob | Exact carrier кодирует prompt и каждый response отдельно, сохраняет границу ответа и response masks; loss использует sum targets. Legacy carrier с Compact всё ещё существует | Потеря whitespace/хвоста через Compact возможна только у legacy caller. Нельзя переносить этот дефект на RP01 exact без доказанного materialVersion |
| Форма objective | Старый generic path использовал среднее по последовательности | DpoExactPairMaterial фиксирует response-sum-softplus-signal-weighted-v3; legacy остаётся отдельным objective | Различие установлено. Возврат legacy full-sequence mean не является доказанным способом восстановить историческое полезное обучение |
| TeachingVector | Число активных координат превращается в scalar signal | Тот же способ: число abs(value)>1e−6, затем ограниченный scalar 1…2; вектор меняет относительный вес пары | Вектор не передаёт направления/знаки своих координат как отдельную семантическую задачу. Это уже известный gap, не новое открытие |
| EOS | EosToken исторически выводится из последнего vocab ID | В текущем CodeDocumentationStudentGeneration передано verifiedEosToken:null | Общая tokenizer-опасность не является текущей причиной преждевременного EOS в этом RP01 consumer |
| Параметры Student | Исторические продукты могли использовать другую конфигурацию и другой получатель | Текущий generation-v1: H512, 4 layers, vocab629, context2048, q/v rank8 alpha16, dropout0, greedy | Это описание конфигурационного файла, не доказательство того, что любой сохранённый checkpoint соответствует ему или что он хорошо обучен |

Источники таблицы: [старый BuildBatch, 418–437](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L418-L437); [текущая ветка exact/legacy, 2378–2400](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L2378-L2400); [Compact, 2423–2428](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L2423-L2428); [exact constants и masks, 19–46](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/Models/DpoExactPairMaterial.cs#L19-L46); [сегменты и зафиксированный tokenizer, 64–89](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/Models/DpoExactPairMaterial.cs#L64-L89); [VectorSignal, 1160–1166](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L1160-L1166); [generation EOS, 61–76](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/CodeDocumentationStudentGeneration.cs#L61-L76); [generation-v1 config, 7–48](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/config/code-documentation/generation-v1.json#L7-L48).

Текущий RP01 server-owned executor явно задаёт MaterialVersion=exact; simulation materializer также вызывает PrepareExecution с этой version. Это подтверждает границу применимости legacy Compact в указанных callers. [Server-owned executor, 379–402](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/CodeDocumentationServerOwnedDpoTrainingExecutor.cs#L379-L402), [materializer, 10–22](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/LogicalDpoPreparedBatchMaterializer.cs#L10-L22).

## 5. Новые сохранённые доказательства уже закрывают часть старых диагнозов

Три исправления от 18 октября теперь присутствуют в текущем SHA. Нужно обновлять их статус, а не заново рекомендовать уже внесённую правку.

| Проверка 18 октября | Установленный эффект | Ответы модели и предел результата | Текущее действие |
|---|---|---|---|
| Embedding scatter-add | При полном SFT embedding стал достижимым: 54/55→55/55 tensors; при decay=0 изменились 38 400 координат; численные строки 24/24 после | 8 updates на плечо; 21 из 24 текстов изменился, правильные переводы 0/24→0/24 | Исправление сохранить. Не объявлять его достаточным для полезной генерации; у frozen-base DPO embedding вообще не обучается |
| Linear broadcast bias | Связный reduce-sum, численная проверка 30/51→51/51 | В C50 bias выключен; loss, checkpoint и все 24 ответа совпали; 0/24→0/24 | Неактивный в C50 узел не считать причиной её качества. Активный bias исправлен компонентно |
| Fractional DPO mask | Производная weighted mean согласована с forward: finite difference 126/162→162/162; изменились LoRA weights и все 16 moment keys | По 8 updates; 0/24→0/24, все ответы совпали. База/reference неизменны. Experimental legacy carrier с fractional masks, не production exact CLI | Исправление сохранить. Не выдавать экспериментальный fractional consumer за поддержанный exact путь |

Текущая реализация: [Embedding, 43–80](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/00.TensorCore/Layers/Embedding.cs#L43-L80), [Linear, 83–100](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/00.TensorCore/Layers/Linear.cs#L83-L100), [DPO, 1083–1120](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L1083-L1120).

Прочитанные сохранённые доказательства: [Embedding report](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/report.md), [Linear report](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/linear-bias-sft8-01/report.md), [DPO mask report](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/report.md), [независимый review, 7–37](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/independent-neural-eight-lesson-review/review.md#L7-L37).

В сохранённом пакете **пять** новых завершённых процессов обучения, **40** updates и **120** ответов. Linear BEFORE переиспользовал предыдущий Embedding AFTER и не считается шестым запуском. Независимый обзор этих трёх пакетов один. Этот аудит прочитал его, но не повторял его model runs и бинарные проверки. [Правила подсчёта и границы review, 29–49](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/independent-neural-eight-lesson-review/review.md#L29-L49).

Ноль на ограниченной панели после восьми уроков не доказывает невозможность обучить эту архитектуру. Он опровергает более узкое утверждение «исправленная производная уже дала полезный перевод в этих измерениях». Движение весов и рост правильности — разные наблюдения.

## 6. Уточнение P92.2: DPO может менять пригодность embedding для поиска

**Статус: уже известная программа, уточнение исполнения; не новая гипотеза.** Первоначальная идея этого аудита совпала с P92.2, где прямо требуются согласованные document/query embeddings каждого Student. Поэтому новый идентификатор для неё не вводится. [P92.2, 21–23](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/cards/P92-dpo-memory-flow-bridge.md#L21-L23).

### Минимальная исполнимая часть

A: один immutable корпус выбранных уроков и encoder W0 до DPO. B: тот же корпус и encoder W1 после одного уже сохранённого DPO budget. Для каждого плеча вызвать TransformerEmbeddingKnnLogicalUnit.Train заново, затем ExecuteAsync для той же заранее сохранённой панели. Все query и document vectors внутри плеча должны происходить от одного encoder. Корпус, IDs, порядок insertion, tokenizer, pooling, maxLen, HNSW и fusion одинаковы. Дополнительных DPO updates в replay нет.

Если подходящие W0/W1 с доказанным общим исходным состоянием отсутствуют, результат CONDITIONAL; отсутствие такой пары нельзя заменить произвольными unrelated checkpoint. Эффект несовместимого W1 query против W0 index здесь специально исключён: это отдельная известная проблема устаревшего индекса.

Причинная цепочка в текущем исходнике: DPO меняет policyModel → Train вычисляет новые corpus vectors → Execute вычисляет query vector → ранкер выбирает example → потребитель получает сохранённый Output. Данная цепочка присутствует в capability path, но не доказана автоматически для endpoint прямой генерации Student. [Train/Execute, 98–210](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L98-L210).

Сохранять полный top-ID list, vector/lexical/pattern score каждого кандидата, фактический output и правильность. Current fusion смешивает vector, lexical и pattern; в основном сравнении его коэффициенты менять нельзя. В дополнительном read-only анализе можно показать, скрывает ли lexical component сдвиг embedding, но улучшение одного vector score не объявлять улучшением конечного ответа. [Fusion, 217–245](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L217-L245).

Для перевода взять отдельную открытую панель отрицания, модальности и обмена ролями; для документации — переименование без изменения поведения и похожие по токенам методы с разной веткой/условием. В каждом профиле держать пары короче одинакового embedding maxLen, чтобы этот опыт не превратился в известную проверку обрезки хвоста. Отдельно считать exact TRAIN recall и новые комбинации. Число модельных layers неизменно; updates=0; измеряются inference/index latency и память индекса, а не sec/update.

Если после согласованного перестроения индекса W1 ухудшает смысловой выбор, получено основание держать retrieval encoder отдельно замороженным при данном DPO objective. Если ID/output/качество не меняются или улучшаются, ухудшение из-за objective interference на этой панели не показано. Оба исхода сохраняются; независимое утверждение о «неправильной геометрии после DPO» заранее не принимается.

## 7. Реестр выводов без повторного открытия старых ошибок

| Вывод | Статус |
|---|---|
| Старый generic LoRA не подключён к policy forward; старый DPO не вызывает graph backward | RECONFIRMED_HISTORICAL_FIXED; не текущая регрессия |
| Current q/v adapter участвует и в полном, и в cached forward | CURRENT_SOURCE_CONFIRMED |
| Current exact carrier сохраняет raw segments и response masks | CURRENT_SOURCE_CONFIRMED; legacy Compact нельзя обобщать |
| Embedding/bias/fractional derivative | CURRENT_FIXED; сохранённые native результаты предметного прироста не показали |
| TeachingVector как scalar, prefix/pooling, stale encoder/index | KNOWN; не переименовывать в новые открытия |
| Согласованный encoder replay | PRIOR_P92_2_REFINEMENT |
| A-scale при B0=0 и optimizer0 | NEW_NARROW_HYPOTHESIS_NOT_RUN; отдельная карточка |

Следующий конкретный нейронный эксперимент описан в [04a-lora-factor-experiment.md](04a-lora-factor-experiment.md). Он проверяет ещё не установленную причину ограниченного эффекта обновлений, сохраняя текущие исправления DPO, autograd и загрузки.
