# LC-REF-CACHE — повторное использование оценки frozen reference внутри реального DPO-обучения

Дата проверки: 20.10.2026. Идентификатор рабочий; итоговую нумерацию назначает сводный аудит.

**Вывод:** при повторном обучении на тех же точных парах текущий `DpoTrainer` снова вычисляет две неизменные оценки reference для каждой пары на каждом шаге. Можно проверить, уменьшит ли ограниченный кэш этих двух чисел время и CPU реального обучения. Само повторное вычисление подтверждено кодом; ускорение, влияние на память и сохранение результатов после исправления **ещё не измерены**. Это кандидат на улучшение стоимости обучения, а не установленная причина падения качества ответов RP01.

**Правило успеха:** любое измеренное улучшение полезного показателя считается успехом по этому показателю и предметной теме. Рост точности не обязателен. Рядом показываются изменения остальных показателей, включая ухудшения. Для данного опыта основными являются время и CPU одинаковых настоящих DPO-обновлений; уменьшение числа вызовов само по себе не заменяет измерение времени.

Статус: `SOURCE_CONFIRMED / EFFECT_UNMEASURED / NATIVE_HOST_NOT_RUN`. Тексты входов, исходные файлы, конкретный фактор и расписание определены. Малый native host, фактические B0/optimizer/RNG байты и runtime-допуск не получены в этом аудите. Защищённая папка core не менялась; действующей CLI-команды для этого опыта здесь не заявлено.

## 1. Доказательства и место в обучении

Все ссылки на код ниже закреплены на core commit `bf9b543e149ef197da85a767821d98c9b67916d2`.

| Проверяемый факт | Источник | Значение для гипотезы |
|---|---|---|
| Один и тот же `executionBatches` используется на нескольких шагах одного learner | [LogicalDpoTrainingUnit:1342–1391](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L1342-L1391) | `batches = executionBatches`, затем цикл и `trainer.StepVector(batches, step)`; повторяемый материал существует в настоящем вызывающем коде |
| Этот learner поддерживает от 1 до 5 шагов, по умолчанию 1 | [request:457–487](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L457-L487), [границы:584–589](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L584-L589) | Кэш полезен при повторении внутри одного trainer; при единственном шаге reuse может отсутствовать |
| На каждом шаге обе стороны каждой пары снова проходят reference | [DpoTrainer:832–895](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L832-L895) | Строки 879 и 882 безусловно вызывают `BatchLogProbs(_reference,...)` |
| Reference вызывается в режиме оценки, обычным полным Forward | [DpoTrainer:1007–1030](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L1007-L1030), [LlmModel:142–158](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/03.TransformerModel/LlmModel.cs#L142-L158) | Это не `ForwardWithCache` и не кэш KV из декодирования |
| Dropout при `training:false` не выбирает случайную маску | [Dropout:53–65](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/00.TensorCore/Layers/Dropout.cs#L53-L65), [LoRA:107–126](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/10.LoRA/LoraLinear.cs#L107-L126) | При одинаковых входах, reference и численном режиме повторный score детерминирован; это следует из показанного пути, не является измерением всей runtime-системы |
| Reference и base защищены проверками реальных параметров | [recipient:198–259](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L198-L259), [guards:1208–1245](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L1208-L1245) | Кэш нельзя использовать для удаления этих проверок или разрешения изменять reference |
| Оценки reference входят прямо в DPO loss и производную | [DpoTrainer:890–924](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L890-L924) | После них выполняются два policy backward на пару и один optimizer update на весь набор |
| Материал уже имеет проверяемый digest | [DpoExactPairMaterial:30–60](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/Models/DpoExactPairMaterial.cs#L30-L60), [123–137](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/Models/DpoExactPairMaterial.cs#L123-L137), [183–198](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/Models/DpoExactPairMaterial.cs#L183-L198) | Digest связывает версии, тексты, tokenizer, длины, token IDs и маски; вход валидируется до lookup |

Цепочка, на которой измеряется изменение: допущенные пары → штатный exact materializer → `LogicalDpoTrainingUnit` → `DpoTrainer.StepVector` → reference score → DPO margin/loss → policy backward → AdamW → настоящий сохранённый кандидат. Меняется только способ получить reference score. Учится тот же Student; меняются LoRA A/B и AdamW m/v/step.

## 2. Конкретное изменение оригинального кода

В экспериментальной ветке изменить `11.DPO/DpoTrainer.cs` около строк 878–883: заменить повторное вычисление двух reference log-probabilities на вызов небольшого helper с ограниченным кэшем **на один экземпляр trainer**. Cache miss выполняет существующий `BatchLogProbs` без изменения формулы. Cache hit возвращает сохранённые `float` выбранной и отвергнутой стороны.

Требования к этому единственному фактору:

1. Разрешать reuse только для успешно проверенного `ExactMaterial` в проектном LoRA пути с замороженным reference. Сначала остаются `CaptureBatch`, admission и `DpoProjectLoraRecipient.RequireValid`. Legacy-путь в первом опыте проходит исходный код без кэша.
2. Ключ связывает `DigestSha256`, identity полного reference с его конфигурацией/LoRA, tokenizer identity и численный режим вычисления. Половина пары не может быть подменена другой; выбранная и отвергнутая оценки хранятся отдельно. Текст prompt или object identity тензора недостаточны.
3. Не вычислять новый полный hash reference дополнительно для каждого lookup: исходный recipient уже хранит identity и проверяет её при mutation permit/update. Подключить cache lifetime к этому же неизменному reference, сохранив **все** существующие проверки. Новый trainer/новый reference означает новый пустой кэш; обнаруженная мутация по-прежнему вызывает исходный отказ.
4. В первом опыте cap = **2 пары**, без внешней записи и процесса-глобального состояния. Заполнить после успешного получения обеих конечных reference оценок. При отсутствии места или допустимого ключа выполнить обычный Forward. Фактическую память кэша измерить; «два float» не является оценкой всей памяти словаря.
5. Не кэшировать policy logits/графы, не сокращать policy forwards, backward, optimizer, checkpoint/publication и не пропускать их проверки. Не менять beta, LR, маски, tokenizer, batch weights, teaching vector, dropout, порядок пар или количество шагов.
6. Наблюдение времени reference должно включать lookup/key cost. Helper выполняет **один lookup на целую пару** и возвращает два scores. `pair_cache_hits` и `pair_cache_misses` считаются в lookup пары; `reused_reference_scores` — в отдельных переиспользованных scores сторон, то есть2 на один pair hit; `actual_reference_forward_calls` — в реально выполненных полных Forward одной стороны. Cap также измеряется парами. В A кэш отключён: pair hits/misses и reused scores равны0, обычные вызовы не называются cache misses. Существующее число reference phase observations нельзя переименовать в фактическое число полных Forward: после изменения на hit наблюдение всё ещё возможно.
7. При численной неодинаковости A/B записать её и источник; не выбирать другой dtype, приблизительный key или rounded score ради скорости. Для первого сравнения оставить один backend и один thread/runtime режим.

Это рекомендация исправления, а не утверждение о внесённом изменении. В аудите исходный код не редактировался.

**Уточнение общего native A/B owner после параллельного пакета 61.** В [`CodeDocumentationSimulationDpoExperiment:85–87,173`](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L85-L87) долговечный `issuedPath` зависит от domain/mode/InputSha256, а не от нового RunId. Два плеча на одном TRAIN нельзя реализовать повтором того же one-shot run, удалением markers или обходом lease. Малый native owner должен заранее связать один study/factor/исходные identities с двумя отдельными arm-journals, каждое arm исполняется один раз с существующей server-issued capability. Неизвестный эффект незавершённого arm требует reconciliation, а не нового ID. [Параллельный дизайн общего paired owner](https://github.com/lubochka/xiigen-planning/blob/426385f91e7cb454e7e6827d9102d1e0d1284534/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/61-LEARNING-FIRST-AUDIT-2026-10-20/03-NEURAL-LEARNING-AND-MICRO-CALLER.md) — источник этого уточнения; его PAD-фактор и batch1 one-pass не применяются здесь. Для LC остаётся строго наш повторяемый двухпарный batch и один trainer на arm.

## 3. Очень маленький опыт: две пары, два настоящих обновления

### Предметные темы и готовые исходные строки

Основная панель — объяснение поведения C# кода. Вторая, отдельная панель — перевод EN→RU. Это две разные предметные темы; механизм кэша общий. Нельзя переносить измеренное число миллисекунд или вывод о качестве ответов из первой во вторую без её выполнения.

Берём существующие строки без перефразирования и без создания новых учителей:

| Панель | Пары | Источник на закреплённом commit |
|---|---|---|
| C# documentation | `simulation-csharp-doc-f06-008` — postfix increment; `simulation-csharp-doc-f21-001` — zero-based array index | [train500.jsonl, строки 1–2](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/training-data/dpo/csharp-code-documentation/simulation-csharp-500-v1/train500.jsonl#L1-L2) |
| EN→RU, отдельный опыт | `enru-0047` — agent/recipient; `enru-0013` — negation | [строка 5](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/training-data/dpo/translation-en-ru/simulation-enru-500-v1/train500.jsonl#L5), [строка 22](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/training-data/dpo/translation-en-ru/simulation-enru-500-v1/train500.jsonl#L22) |

Raw учебные тексты приложены в `fixtures/csharp-two-source-rows.jsonl` и `fixtures/enru-two-source-rows.jsonl`; source selection manifest содержит source commit/path/line/blob SHA и SHA256 нового двухстрочного файла. Это извлечённые simulation данные, **не** готовое новое разрешение на изменение модели. Само наличие строки в Git не означает, что новый run уже admitted.

Использовать существующий source-governance и typed-author bridge: [CodeDocumentationSimulationDpoExperiment:114–128](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L114-L128). Он сохраняет `DeterministicGenerator` для обеих сторон и вызывает настоящий materializer. Не создавать фиктивные `Human`/`ExternalModel` identities. Двухстрочный input получает свой digest и scoped admission; это не разрешение повторить старый dataset/run ID.

### Общее исходное состояние A и B

Для основной C# панели использовать сохранённый B0 проектного `rp01-csharp-small/v1`, два независимых загруженных экземпляра policy/reference и два независимых плеча A/B. Конфигурация прочитана в [generation-v1.json](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/config/code-documentation/generation-v1.json): vocab629/context2048/hidden512/layers4/heads8/KVheads2/FFN1408; LoRA rank8/alpha16/dropout0, q_proj/v_proj.

Начальный B0 **не подменять случайно созданной моделью**. Source pins в D04 не равны открытому и перепроверенному файлу B0. Перед исполнением нужен конкретный разрешённый путь и фактический hash весов, manifest и tokenizer, затем одинаковые выгруженные optimizer/RNG states. Это незакрытая runtime-зависимость данного документа.

Настройки текущего project trainer: LR=1e-5, DPO beta=0.1, AdamW beta1=0.9, beta2=0.95, decay=0.01, eps=1e-8 ([DpoTrainer:328–379](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L328-L379), [AdamW:38–53](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/04.TrainingLoop/Optimizers/AdamW.cs#L38-L53)). Будущий host должен прочитать их из actual trainer. Выбранный начальный optimizer должен быть один и тот же в A/B; если это cold start, фиксируются реальный step0 и пустые m/v, а не подразумеваются из имени B0. Если это продолжение, восстановить конкретные m/v/step/RNG через существующий continuation proof.

EN→RU не следует запускать автоматически с C# tokenizer/B0: эта дополнительная панель требует собственного разрешённого совместимого baseline/continuation. Нынешний ENRU runner связан с C50→100 и500 строками; двухстрочный новый режим из этих данных пока не существует.

### Расписание

1. Один раз получить два exact material через `LogicalDpoPreparedBatchMaterializer.Prepare`; валидировать токены/маски штатным `DpoExactPairMaterial.CaptureAndValidate`. Они одинаковы в A/B. Сохранить их digest и реальные длины. Подготовка вне измеряемого обучения.
2. A: текущий механизм без кэша. B: тот же код с единственным reference-score reuse. У обоих `MaxStepCount=2`, `ConvergenceWindowSteps=3`; цикл не достигает проверки early-stop. Порядок пары1,пары2 фиксирован.
3. На каждом из двух шагов передать **обе те же пары** одному и тому же trainer данного плеча. В одном плече получается4 предъявления и2 optimizer updates, всего A+B8 предъявлений и4 updates. Модель/optimizer не сбрасывать между шагами.
4. До шага1 и после шага2 снять реальные состояния policy/reference/optimizer/RNG. Показать, что actual LoRA изменилась, base/reference остались исходными, optimizer step увеличился на2 и сохранённый кандидат соответствует in-memory. Эти сведения подтверждают участие в обучении; они не заменяют time/CPU измерение.
5. В первом опыте **нет дополнительных Train/StepVector updates для разминки**: бюджет ровно2 updates на плечо. Сборка и запуск host находятся вне таймера, начальная оценка ниже одинакова в A/B. Не прогретый к моменту первого шага JIT/backward код остаётся внутри измерения; это стоимость малого cold run, не обещание steady-state throughput. Сам кэш B перед шагом1 пуст. Полный reference miss первого шага входит в результат B.
6. Измерять весь интервал двух `StepVector` с его штатной транзакцией/сохранением, отдельно reference phase. Процессный CPU — разность вокруг тех же двух шагов. Lifetime peak RSS нельзя выдавать за память только обучения: либо брать отдельные одинаково запущенные процессы A/B и честно подписать whole-process peak, либо измерять RSS в окне с одинаковым sampling interval.
7. Для before/after вызвать существующий [`DpoTrainer.EvaluateMany(IReadOnlyList<DpoBatch>, int):767–809`](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L767-L809) ровно один раз до шага1 и один раз после шага2 каждого плеча, передав `batches.Select(x => x.Batch).ToArray()`. Итого4 evaluation calls A+B на тех же2 парах, вне training time window. Не подключать cache factor в `EvaluateMany`: он не заполняет тренировочный кэш и отдельно добавляет16 reference и16 policy forwards суммарно для двух плеч; эти затраты записываются отдельно. Его loss — среднее двух evaluation objectives без teaching-vector scaling, не переименовывать его в weighted StepVector loss. Метрики, возвращённые `StepVector`, вычислены **до** `ApplyOptimizer` данного шага; они не являются post-update оценкой конечного состояния.

**Расчёт из кода, не результат запуска:** при двух парах и двух шагах A ожидаемо вычисляет reference8 раз. У B ожидаются `pair_cache_misses=2` на первом шаге, `pair_cache_hits=2` на втором, `reused_reference_scores=4` и `actual_reference_forward_calls=4`. Это2 удачных lookup пары, а не4 pair hits. В A ожидаются0 hits/0 misses/0 reused scores, поскольку кэш выключен. У обоих ожидаются8 policy forwards одной стороны,8 backward одной стороны и2 optimizer updates. Для m пар и U повторов B имеет m pair misses, m(U−1) pair hits и2m(U−1) reused scores; полных reference forwards2mU в A против2m в B, если всё поместилось в кэш и все ключи совпали. Из этого не следует «обучение быстрее вдвое»: backward, policy, проверки, serialization и I/O остаются.

### Что показывать в before/after отчёте

| Показатель | A: до/после обучения | B: до/после обучения | Сравнение |
|---|---|---|---|
| Полное время двух настоящих updates, ms | actual, не build/startup | actual, включая холодное заполнение кэша | B−A и процент |
| CPU в том же окне, ms | actual | actual | B−A |
| Working set/allocated bytes/cache bytes | метод измерения и actual | тот же метод и actual | ухудшение памяти не скрывать |
| `actual_reference_forward_calls` / `pair_cache_hits` / `pair_cache_misses` / `reused_reference_scores` | actual; cache counters0 при отключённом кэше | actual | единицы: Forward стороны / lookup пары / lookup пары / score стороны; phase observations отдельно |
| Pair presentations / policy forwards / backward / updates | actual | actual | одинаковая выполненная работа |
| Loss и margin на тех же парах | `EvaluateMany` до/после2 updates | те же2 вызова `EvaluateMany` | всего4 вызова A+B; evaluation average, не weighted training loss |
| LoRA delta / optimizer m/v/step / reference identity | before/after | before/after | сравнить A/B конечные состояния и указать допуск |
| Свободные ответы на новых заданиях | `NOT_MEASURED`, если не включены явно | `NOT_MEASURED` | отсутствие оценки не превращается в «качество сохранено» |

Любое actual снижение времени/CPU или иное полезное улучшение — локальный успех соответствующего показателя. Одинаковые weight/loss результаты укрепляют тезис об эквивалентности, но не измеряют качество свободной генерации. Если оно не проверено, так и написать. Если добавлена согласованная оценка ответа Student, использовать один фиксированный evaluator/decoder и показать ухудшения рядом; не требовать роста accuracy для признания ресурсного успеха.

## 4. Границы применимости и ложные переносы

Нынешний `CodeDocumentationSimulationDpoExperiment` обходит500 уникальных строк ровно одним проходом: [175–190](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L175-L190). На такой схеме нет гарантированных повторов ключей: кэш может дать только накладные расходы. Это не подтверждённое исправление уже завершённого main500.

Кэш на trainer не переносит результат между разными вызовами, где reference или экземпляр trainer созданы заново. Отдельная идея глобального/дискового кэша здесь не предлагается.

Синтаксическое дерево и порядок слов могут быть важны для самой учебной пары, но этот фактор их не заменяет и не обрабатывает. Он возвращает ровно те же reference scores для ровно тех же последовательностей. Доменная универсальность механизма не доказывает одинаковую экономию при разных длинах, размере модели и числе повторений.

## 5. Покрытие в защищённых экспериментах и отличие от старых находок

Поиск выполнен по всем555 Markdown baseline `possibleDifs` на planning `1ad622fe1f6d1e8b0860265d8c208aeada3fefb0`, а также cached protected библиотеке, с терминами reference/cache/memoization/logprob/кэш/предвычисление. Отдельного фактора reuse неизменных reference log-probabilities в повторных реальных updates не найдено. Это результат адресного поиска по snapshot, а не утверждение, что каждый документ был перечитан целиком или все будущие версии не содержат идеи.

Близкие темы отличаются: P105 проверяет frozen reference; N60 PAD — длину дополненных последовательностей; P115 — BM25 statistics; inference/KV cache — генерацию; checkpoint-cache alias — изменяемость загруженной модели. Они не заменяют данную проверку стоимости reference внутри DPO.

Защищённый [D04](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/04-documentation-neural.md) уже правильно содержит правило любого полезного улучшения и требование настоящего обучения. Его factor B1 и малый host B2 пока не заполнены. Исходное расписание D04 — две разные пары по одному предъявлению — не проверяет reuse. Данную карточку нельзя молча объявить выполнением D04: у неё отдельная ресурсная цель и явно повторяемый batch.

Защищённая папка не менялась. Здесь подготовлено дополнительное описание для `possibleDifs`; включение в native очередь потребует реализации указанного малого host и заполнения фактических входов исполнения. Нативного обучения, теста C# или измеренного ускорения в этом аудите не было.
