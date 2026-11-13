# Два уточнения уже существующих N60-E1/E2 — конкретные небольшие проекты

**Уточнение исполнения по a822 (20.10):** ссылки на оригинальные файлы ниже обозначают происхождение и адрес предлагаемого последующего переноса. Сначала проверяется точная экспериментальная копия A/B в существующем проекте `34.Experiments` или `35.ExperimentsV2`, с малым host в `experiments-v2/RP01-MICRO/<ID>/`. Оригинальный продукт и его тесты остаются read-only. Копия сохраняет настоящий алгоритм, зависимости, admission и mutation capabilities; их обход, поддельный lease или суррогатный learner недопустимы. До исполнения фиксируются copied-file hashes, A/B diff и actual binding вызовов. Перенос в основной код — отдельное решение после результата. Текущий запрет пользователя менять protected распространяется и на results: для возможного запуска выбирается разрешённый каталог вне protected. Подробнее: [финальная дельта](12-CONCURRENT-UPDATES-AND-NOVELTY.md).

**Дата:** 20 октября 2026. **Источник:** core `f5dd532045716495a2c7668573250e5d7cc46810`; C# код не менялся относительно `bf9b543`. Защищённые карточки и результаты не изменены. Ниже уточняются E1/E2 последнего active23; это не новые номера элементарных дефектов и не дополнительные обязательные прогоны. **Фактических запусков, measured deltas и результатов здесь нет.**

**Исходное задание:** быстро проверить причинные различия, участвующие в обучении или использовании его результата; сначала реальные препятствия обучению. Сохранить доменную применимость, общий материал до/после и показать все полезные изменения и ухудшения. Общий запрет ожидать новое разрешение для каждого паспорта снят; исполнитель сам подготавливает доступный материал в пределах порученной программы. Настоящий внешний блокер указывается после конкретной неудачной попытки получить необходимый ресурс.

Запрошенные исполнитель/reviewer — `gpt-6-astra`, reasoning `ultra`. Нужные навыки: `architect-behavior-classifier`, `phase-planning`, `plan-review`, `code-examination`, `prompt-patch-authoring`, `human-communication-GLOSSARY`. Модель исполнителя не является обучаемой моделью опыта. Обучаемые получатели ниже — обычные C# Units, **нейронных слоёв/optimizer updates нет**. Текущие инструкции пользователя имеют приоритет над старым skill-гейтом обязательного роста accuracy или повторного approval.

## E1-MARKER-MATERIAL — admission ложного Verifier-negative без второго marker-fix

**Статус:** `PREPARABLE / NOT_RUN`. Это конкретизация [существующего N60-E1][existing-e1]. Поле исходных строк заполнено ниже. Исполнитель может сам написать короткий bounded native host и сериализовать данные; полного внешнего артефакта или отдельного решения о смысле для этого проекта не выявлено. Настоящие результаты появятся только при исполнении. Домен — перевод RU→EN, локальное решение Verifier; результат не называется свободным переводом или восстановлением RP01 LoRA.

### Что исправляется в оригинале

В [TranslationStructuredBatchBuilder.BuildGeneralBatch:128–141][builder] при совпадении `rejectedCandidate` с `pair.Chosen` не добавлять **только** false-negative запись `quality-verifier`; остальные записи semantic/executor сохраняются. Сравнение строк — `StringComparison.Ordinal`, без второй нормализации. Причина исключения фиксируется как `NO_DISTINCT_NEGATIVE` в trace подготовки. В A исходный admission, в B только этот фильтр. Исходный [BuildRejectedCandidate:269–288][negative] закономерно создаёт равенство для annotation-only non-omission carrier.

Действительная цепочка: producer создаёт `TranslationLearningExample`; native [QualityVerifierUnit.TrainOnExample:1095–1145][writer] извлекает `expected 'term'`, записывает `_requiredTermsBySourceMarker`; [Verify:1052–1091][verify] читает **ту же** память и выдаёт собственные `Pass/Issues`. Продуктовый dispatch этого класса находится в [TranslationMachine.TrainStructuredBatchAsync:107–121][dispatch]. В минимальном host вызывается сам публичный обучаемый Unit, без симуляции его словарей.

В original active23 `быстро/тихо` не дают заявленного marker-контраста; `окно` дополнительно может стать ложным ADV. Здесь используются уже распознаваемые существующей реализацией `бесшумно/громко` и существительные `дверь/люк`. **Marker-код и tokenizer в обеих сторонах остаются теми же.** Изменить одновременно tokenizer означало бы проверить два фактора. [Классификация ADV][adv]; [запись/извлечение marker][marker]

### Точные входы и начальное состояние

Два независимых новых `QualityVerifierUnit`: пустые словари, одна версия сборки, культура/нормализация и один `TranslationSemanticTokenizer`. `BuildGeneralBatch` вызывается с `heldOut=false`, `signatureMode=Structural`. Все `TranslationTrainReadyPair` имеют `Source="simulation:e1-marker-material"`, `Direction="RU_EN"`, `PairFamily="manner"`, `Prompt=ASide`, `ValidationOutcome="train_ready"`, `ValidationErrors=[]`; остальные поля даны таблицей. `train_ready` здесь воспроизводит реальный carrier с ошибкой materializer, а не утверждает существование отличающегося rejected-текста у V1.

| ID | ASide | Chosen | Rejected |
|---|---|---|---|
| V1 | `Он бесшумно открыл дверь.` | `He silently opened the door.` | `[WRONG:tense]` |
| V2 | `Она громко закрыла люк.` | `She loudly closed the hatch.` | `She closed the hatch.` |

Малый host получает native batch и выбирает **только** его `UnitId == "quality-verifier"` для двух публичных Verifier экземпляров. Это одинаковая локализация во всех состояниях, а не дополнительное исправление producer. A обучается V1 и V2, B — только V2 после единственного admission-фильтра. Разница числа принятых внутренних уроков — действие исследуемого фактора; общий пул из двух raw пар одинаков. Обе стороны реально обучают Verifier. Semantic/executor rows сохраняются для подтверждения projection equality, но не исполняются в этом локальном варианте. Полный Machine-run не добавляется ради вызова всех трёх Units.

**EXAM один и тот же для A0/A1/B0/B1.** Source токенизируется настоящим `TranslationSemanticTokenizer.Tokenize(source,"ru")`, после чего вызывается `Verify(candidate, tokens)` на соответствующем экземпляре. Не использовать `MeasureAccuracy` как источник предметного gold и не вычислять gold через исправленный код.

| ID | Source | Candidate | Независимый gold |
|---|---|---|---|
| E1 | `Он бесшумно закрыл люк.` | `He noiselessly closed the hatch.` | Pass: допустимая синонимическая формулировка |
| E2 | `Он бесшумно закрыл люк.` | `He closed the hatch.` | Fail: потеряно «бесшумно» |
| E3 | `Она громко открыла дверь.` | `She loudly opened the door.` | Pass |
| E4 | `Она громко открыла дверь.` | `She opened the door.` | Fail: потеряно «громко» |

Четыре решения относятся к двум source-комбинациям и двум манерам действия. Они не доказывают общую семантику русского языка. Перед первым Train сохраняются точные UTF8 fixture bytes, hashes, source commit, compiled assembly identity и начальные состояния. Сериализация выполняется из таблиц до просмотра результатов; никакого скрытого отбора более удачной панели после run.

### Вызовы, измерение и вывод

Бюджет минимального варианта: **2 native BuildGeneralBatch** (по одному A/B), **3 native Verifier.TrainOnExample** всего (A2/B1), **16 Verify** (четыре задачи × четыре состояния), ноль Generate и ноль нейронных updates. Это отличается от полного проекта active23 с 11 вызовами всех Units; не складывать два альтернативных бюджета. Не требуется запускать полный Windows CI или повторять исторические 8/24.

Сохранить A0/A1/B0/B1 `Pass`, `Issues`, число записанных требований/ключи и собственное содержимое памяти через одинаковый наблюдатель реального Unit. Если для readback нужен internal host в той же сборке, observer должен только читать состояние; нельзя подставить вычисленный отдельно словарь. Итоговый ordinary Flow здесь не измеряется. Общий корпус/компоненты/конфигурация одинаковы, кроме admission.

Результаты: correct/4, false reject/2, false accept/2; отдельно каждая из четырёх строк; accepted lesson count; wall time подготовки и Train, CPU process time при достаточном разрешении, память/байты learned state, Verify latency, annotation effort. `GC.GetTotalMemory` не именовать пиком RAM, wall clock — CPU. Неизмеренное — `NOT_MEASURED`, а не ноль; микроинтервал ниже разрешения — «неразличимо».

**Правило успеха E1:** любое измеренное полезное улучшение считается успехом по метрике и RU→EN Verifier. Рост общей accuracy не обязателен. Рядом показываются все ухудшения. Аналитически возможны одновременно меньше ложных отказов синониму и больше пропущенных omissions, при неизменной суммарной accuracy. Это прогноз механизма, не полученные числа.

Если V1 реально не пишет marker requirement, первоначальная гипотеза на данном материале не получила контраста; это не повод включить N57-fix задним числом. Если native Train достигнут, но полезного эффекта нет, сообщить честный нулевой эффект. Неверный caller, изменение EXAM после просмотра результатов или имитация памяти делают такое измерение непригодным.

### Промпт исполнителю E1

> Работай сначала на точной экспериментальной копии исходного механизма по правилу a822: A — неизменённый алгоритм, B — одна правка в копии, общие зависимости и состояния одинаковы. Основной код и продуктовые тесты не меняй; оригинальные пути ниже — provenance и будущий адрес переноса. Сохрани исходные admission и mutation authority, не имитируй их. Покажи результаты и точный diff переноса; даже успех не разрешает автоматически менять основной код или запускать продуктовый цикл. Защищённый каталог остаётся read-only; runtime evidence размещай в разрешённом каталоге вне него. 
>
> Выполни в пределах порученной ограниченной программы уточнение N60-E1 E1-MARKER-MATERIAL. До изменения прочти BuildGeneralBatch/BuildRejectedCandidate, полный QualityVerifierUnit и его caller TranslationMachine. Запиши source/assembly/fixture identities. Используй ровно две RU_EN пары и четыре Verify-задачи этой карточки; в A прежний admission, в B исключается только Verifier row при ordinal equality rejectedCandidate==chosen. Не исправляй tokenizer, marker helper, synonyms или Verify. Обучи реальные два QualityVerifierUnit: 3 TrainOnExample всего, затем предъяви 16 собственных Verify на A0/A1/B0/B1; материал и gold одинаковы. Сохрани learned rules, Pass/Issues, false accept/reject, стоимость и невыдуманные ресурсные измерения. Любое измеренное полезное улучшение — успех по этой метрике и домену без обязательного роста accuracy; все остальные изменения и ухудшения покажи рядом. Это обучение Verifier, не свободный перевод и не LoRA. Protected каталог не менять. Доступные входы/host готовь сам; новое согласование каждого SHA не требуется. При недостижимом writer назови конкретную причину, не заменяй native Unit своей таблицей и продолжай независимую готовую работу.

## E2-ORDERED-SUPPORT — конкретный вариант learned writer/state/reader

**Статус:** `PREPARABLE_DESIGN / NOT_IMPLEMENTED / NOT_RUN`. До нашей публикации соседний отчёт уже предложил близкий SourceSpanFrames с теми же двумя уроками; данный вариант — параллельная конкретизация и проверка границ reader, не новая независимая гипотеза. Сравнение и выбор одного варианта — [12](12-CONCURRENT-UPDATES-AND-NOVELTY.md). Это предлагаемое заполнение `OPEN_STATE_AND_READER` [существующего N60-E2][existing-e2], а не уже имеющийся класс или обязательный второй прогон. Получатель — EN→RU Executor memory; тема — технические команды и ограниченность композиции. Исследуется один фактор: какие **порядковые source spans** приобретаются при Train и затем читаются обычным Executor. Два положительных урока не обучают переводу отрицания или вопроса; полезным исходом может быть явный отказ на неподдержанной форме.

### Точная запись, получение признаков и чтение

Предлагаемая версия `OrderedPhraseSupport/v1` содержит для каждого направления: (1) learned соответствия `normalized source phrase → множество порядковых slot IDs`; (2) множество последовательностей slot IDs, наблюдавшихся в исходных уроках; (3) provenance ExampleId для каждой записи. Slot ID — порядковая позиция **размеченного source span в конкретной строке урока**, не вручную заданное общее правило языка и не дерево C#/SQL.

При [ExecutorUnit.TrainOnExample:403–455][executor-train] использовать те же `ExecutorPhraseMapping`, которые уже записываются в `_phraseMap`. Для каждого TRAIN input найти точные целословные неперекрывающиеся spans этих source phrases, отсортировать по началу. Для этого минимального профиля принимается только однозначное полное покрытие всех normalized source tokens; иначе observation `UNRESOLVED_TRAIN_SUPPORT`, шаблон не выдумывается. Слоты нумеруются 0,1; сохранить принадлежность каждой source phrase соответствующему слоту и последовательность `[0,1]`. Нового POS-parser, списка отрицаний, hidden gold или дополнительной языковой эвристики нет.

В [Execute:264][executor-read] применяется **один общий reader до выбора exact/phrase/wordmap/kNN**. Он использует текущие learned source phrases и их slot IDs для полного покрытия query. Для короткого профиля с двумя spans неоднозначные сегментации явно отклоняются; общее backtracking/beam решение не добавляется. Если полный query даёт наблюдавшуюся последовательность `[0,1]`, продолжается существующий Execute. Если источник не поддержан обученной записью, вернуть `TranslationCandidate("",0,"executor-en_ru-unsupported-learned-source")`; обычный [TrainedTranslationAdapter:124–139][adapter] оформляет существующий fallback. Не повышать confidence, не добавлять специальный `if not/did`, не исправлять verifier override.

Reader располагается до всех маршрутов, потому что отказ только в `TryComposeFromPhraseMappings` иначе может обойти `TryComposeFromWordMappings` или kNN. Это часть одного representation-контракта writer→state→reader, а не отдельное изменение порога. Алгоритм детерминистический и использует приобретённые примеры; его нельзя представлять как нейронное понимание операторов. На неразмеченных span-конфликтах или новых формах возможны дополнительные отказы — они обязательны в отчёте.

В одном и том же экспериментальном бинарном коде A выполняет прежнюю запись maps, B дополнительно пишет `OrderedPhraseSupport/v1`. В обеих сторонах reader одинаков, при отсутствии support schema действует прежняя семантика. До обучения обычные maps пусты в A0/B0. Этот A является прежним поведением памяти, B — одна новая обучаемая репрезентация с её согласованным использованием; шаблоны нельзя инициализировать из EXAM.

Оригинальные места для реализации: `TranslationTrainedUnits.cs` поля ExecutorUnit, `TrainOnExample`, начало `Execute`; [ExportCheckpoint/ImportCheckpoint:486–535][state] и `TranslationCheckpointContracts.cs:97` — включить optional versioned support в полный checkpoint. Старый checkpoint без поля означает legacy support absent, не автоматически выученный новый шаблон. Roundtrip входит в чтение состояния этой же постановки, но отдельная большая restore-панель не назначается. Защищённая папка experiments не изменяется.

### Точные уроки и общий экзамен

Два `TranslationLearningExample` только для `executor-en_ru`, `HeldOut=false`, `FlowId=TranslationRoutingFlow.FlowId`, направление EN_RU, `SourceLang="en"`, `TopicDomain="general"`. `InputJson` сериализует `ExecutorInput`; `ChosenOutputJson` — `ExecutorOutput(text,0.93,WordMappings:null,PhraseMappings:...)`. Same native signature строится из source настоящим tokenizer в обоих arms. Остальные provenance/ExampleId фиксируются до первого Train. Rejected не является сигналом этой Unit-memory постановки и не объявляется DPO-обучением.

| ID | Source → chosen | Одни и те же PhraseMappings для A/B |
|---|---|---|
| T1 | `delete the file` → `удали файл` | `delete→удали`; `the file→файл` |
| T2 | `save the cache` → `сохрани кэш` | `save→сохрани`; `the cache→кэш` |

Уроки дают delete/save slot0, the-file/the-cache slot1 и наблюдавшийся порядок `[0,1]`. Это учебное ожидание, получаемое из двух строк, не заранее скрытое состояние.

| ID | Query | Предметный gold и раздельные исходы |
|---|---|---|
| Q1 | `delete the cache` | `удали кэш`; правильность перевода |
| Q2 | `save the file` | `сохрани файл`; правильность перевода |
| Q3 | `do not delete the cache` | `не удаляй кэш`; правильный перевод и безопасный отказ раздельны; `удали кэш` — wrong accepted |
| Q4 | `did you save the file?` | `ты сохранил файл?`; вопрос сохранить; явный отказ отдельно, `сохрани файл` — wrong accepted |

Панель не меняется между четырьмя состояниями. Нельзя добавлять в TRAIN отрицание/вопрос: это другой curriculum-опыт N59-L01. Нельзя автоматически распространять порядковый шаблон EN→RU на RU→EN, POS-classification или AST-функции kNN. Перенос на другие домены требует своего представления и минимального материала.

### Бюджет, результат и промпт

После реализации контрактного участка: **2 TrainStructuredBatchAsync** — по одному batch из двух Executor rows на плечо; **4 вложенных Executor.TrainOnExample**, **16 ordinary orchestrator запросов** A0/A1/B0/B1. Оба плеча реально обучаются. Нейронных updates/Generate нет. Export state до/после — observation того же опыта. Exact host/команда, codec и output directory оформляются исполнителем; готовая CLI здесь не выдумана.

Сохранить learned maps, support templates/provenance, четырёхсторонние собственные ответы, route, unsupported reason, coverage, correct/all, correct/answered, wrong accepted, explicit refusal, semantic ошибки и call counts.

**Точные категории ответа:** accepted означает обычный `OverallVerdict=TRANSLATED` и non-bootstrap adapter; wrong accepted считается только внутри этой категории по общему gold. `BootstrapTranslationAdapter` возвращает `Ok` с текстом `[EN_RU:BOOTSTRAP] ...`, а `TranslationRoutingFlow` помечает его `TRANSLATION_LOW_CONFIDENCE`/`request_more_context`: это fallback, не правильный перевод и не доказанный явный человеческий отказ. Bootstrap/fallback считать отдельной колонкой. Explicit refusal засчитывать только при реально наблюдённом structured или человеческом отказе; не выводить его из пустого Executor candidate. Причина `unsupported-learned-source` теряется при переходе в fallback, поэтому её читает одинаковый read-only observer непосредственно у original Executor, отдельно от ordinary output. Форматтер отказа и Flow в этом A/B не меняются. Источники: [adapter 77–89,124–139](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/27.TranslationDomain/TranslationDirectionAdapters.cs#L77-L139), [Flow 85–121](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/27.TranslationDomain/TranslationRoutingFlow.cs#L85-L121). Измерить стоимость actual Train и source reader, CPU/RAM при доступном разрешении, дополнительные байты maps/checkpoint и ручную стоимость annotations. Повторные участки процесса не складываются дважды. Устаревшее слово `CpuMs` на wall-clock поле не использовать как доказательство CPU-экономии.

**Правило успеха E2:** любое измеренное полезное изменение — успех по соответствующему показателю EN→RU. Accuracy может остаться той же, а wrong accepted уменьшиться; каждый отказ отдельно снижает coverage и не засчитывается как правильный перевод. Все ухудшения привести рядом. Отсутствие state/reader связи нельзя заменить статическим guard, но нулевой эффект настоящего acquisition является валидным результатом.

> Работай сначала на точной экспериментальной копии исходного механизма по правилу a822: A — неизменённый алгоритм, B — одна правка в копии, общие зависимости и состояния одинаковы. Основной код и продуктовые тесты не меняй; оригинальные пути ниже — provenance и будущий адрес переноса. Сохрани исходные admission и mutation authority, не имитируй их. Покажи результаты и точный diff переноса; даже успех не разрешает автоматически менять основной код или запускать продуктовый цикл. Защищённый каталог остаётся read-only; runtime evidence размещай в разрешённом каталоге вне него. 
>
> Выполни в пределах порученной программы точное заполнение N60-E2 вариантом OrderedPhraseSupport/v1. Сначала реализуй один контракт в настоящем ExecutorUnit: source PhraseMappings при Train образуют только наблюдённые порядковые slots/templates; тот же reader проверяет полное покрытие source до всех candidate routes; schema сохраняется в native checkpoint. Не добавляй NLP/AST правила, список not/did, новые уроки или EXAM gold. A/B используют две одинаковые Executor-пары этой карточки, общий пустой старт и четыре одинаковых запроса; отличается acquisition support metadata с её чтением. Выполни 2 native batch/4 внутренних TrainOnExample и 16 ordinary запросов на A0/A1/B0/B1. Не оставляй путь, где wordmap или kNN обходит learned-support условие, и не меняй прежние thresholds/verifier. Покажи own outputs, learned templates, отказы, wrong accepted, correctness, время, память/байты и annotations. Считай TRANSLATED/nonbootstrap, bootstrap fallback и явный отказ раздельно; пустой candidate и bootstrap-текст не являются правильным переводом или доказанным человеческим отказом. Unsupported reason наблюдай отдельно до fallback, форматтер не меняй. Любой измеренный полезный выигрыш — успех по оси и домену без требования роста accuracy; остальные показатели и ухудшения рядом. Если representation не записан/не читается, не заявляй learning effect. Перед запуском оформи доступные входы и команду сам, protected не меняй; отдельное подтверждение каждого SHA не требуется.

[existing-e1]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L188
[existing-e2]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L220
[builder]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L128
[negative]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L269
[writer]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/27.TranslationDomain/TranslationTrainedUnits.cs#L1095
[verify]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/27.TranslationDomain/TranslationTrainedUnits.cs#L1052
[dispatch]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/27.TranslationDomain/TranslationMachine.cs#L107
[adv]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/27.TranslationDomain/TranslationSemanticTokenizer.cs#L413
[marker]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/27.TranslationDomain/TranslationTrainedUnits.cs#L1265
[executor-train]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/27.TranslationDomain/TranslationTrainedUnits.cs#L403
[executor-read]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/27.TranslationDomain/TranslationTrainedUnits.cs#L264
[adapter]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/27.TranslationDomain/TranslationDirectionAdapters.cs#L124
[state]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/27.TranslationDomain/TranslationTrainedUnits.cs#L486
