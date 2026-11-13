# 21. Новые механизмы потери обученного смысла: удаление непокрытых слов, граница whitespace, ложная метрика legacy TrainBatch

**Дата:** 2026-10-16. **Статус:** SOURCE-CONFIRMED MECHANISMS; CAUSAL IMPACT NOT RUN; INDEPENDENT CODEX/ASTRA REVIEW PENDING.  
**Репозиторий кода:** [llm_mvp_core@83590a462b2f8aedc9d3ab4cb775193eb9c25ce5](https://github.com/lubochka/llm_mvp_core/tree/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5), ветка codex/dpo-implementation.  
**Историческая сверка:** [DesignToCode@8f20ceb8a9bfbadab2ad309226c47dd76e714548](https://github.com/lubochka/llm_mvp_core/tree/8f20ceb8a9bfbadab2ad309226c47dd76e714548).  
**Область действия:** trained Translation ExecutorUnit / TranslationMachine. Нельзя автоматически приписать перечисленные дефекты нейронному RP01 Student (DPO/LoRA/TextGenerator), пока не показан его фактический consumer/caller.

## Краткий результат и строгая граница утверждения

Код содержит **три дополнительные механики**, которые способны сделать обученное содержимое бесполезным либо создать иллюзию улучшения качества:
1. **N21-01 / P0 conditional: phrase composition удаляет неизвестные слова из запроса, хотя собирает высокоуверенный перевод из двух выученных фраз.** Это не просто ранее обнаруженный verifier bypass: это самостоятельная **потеря непокрытого source-значения до проверки**. Одно потерянное слово может изменить полярность ответа (never/not), модальность (must/may) или количество.
2. **N21-02 / P1: semantic tokenizer считает границей слова только обычный ASCII пробел**; обучающий ключ и inferencing key могут расходиться для одного предложения, представленного через tab, newline, NBSP. Это исследовательская гипотеза снижения устойчивости, НЕ уже измеренная деградация на пользовательских запросах.
3. **N21-03 / P0 conditional: старый плоский TrainBatchAsync повышает reported AccuracyAfter на +0.01 без обучения Unit**. Если вызывающий код использует этот метод вместо TrainStructuredBatchAsync, метрика может «учиться», пока модель и обучаемая память не меняются. Это подтверждено телом метода, но неизвестно, доходит ли до него RP01/ordinary consumer.

Семантическая истинность и сохранность отрицания важнее количества зелёных unit tests, изменения checkpoint SHA или показателя AccuracyAfter.

## N21-01 — удаление критических токенов при композиции из выученных фраз

### Доказательство по действующему коду (C)

- [TranslationTrainedUnits.cs:756–760](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L756-L760): обучающий Executor сохраняет пары SourcePhrase → TargetPhrase в поле _phraseMap.
- [TranslationTrainedUnits.cs:403–456](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L456): TrainOnExample берёт PhraseMappings из выбранной стороны ExecutorOutput, записывает их; одновременно обучает _exactSourceMap, KNN и word mappings.
- [TranslationTrainedUnits.cs:798–857](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L798-L857): при обходе sourceTokens совпавшая фраза копирует **только** её Target. Неизвестный token, для которого нет словарного отображения, вообще не попадает в parts (ветвь 837–845). Код разрешает результат при двух phrase-span и доле покрытых phrase-токенов ≥0.50, даже если обычный порог покрытия 0.85 не пройден. Порог оперирует **количеством**, а не критичностью выпавших слов.
- [TranslationTrainedUnits.cs:262–280](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L262-L280): Execute возвращает выбранный phrase-translation раньше word/KNN с Confidence=0.86.
- [TranslationTrainedUnits.cs:945–958](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L945-L958): проверка направления у phrase result проверяет алфавит/копирование source, а не сохранность квантора, отрицания или модальности.
- [TranslationDirectionAdapters.cs:115–142](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationDirectionAdapters.cs#L115-L142): trained adapter вызывает Executor и Verifier; phrase candidate с confidence ≥0.85 имеет исключение, допускающее принятие даже при verification.Pass=false. **Само исключение уже известно другим авторам (N16-06); новое здесь — upstream token omission и количественный bypass coverage.**

**Контрпример по формуле, НЕ наблюдённый реальный прогон:** обучены только phrase mappings «the service» → «сервис» и «opens the port» → «открывает порт». При запросе «the service never opens the port» первый и второй span вместе покрывают 5 из 6 source-токенов; never не имеет mapping и не включается в result. Возможный собранный ответ «сервис открывает порт» меняет смысл на противоположный. Если нет другого ранее выученного word map, exact match или доменного фильтра, текущая ветка кода допускает именно эту трансформацию. Проверка должна воспроизвести её в нативном ExecutorUnit, а не принять логический пример за измеренное поведение.

### Связанный второй вариант: word-map omission

[TranslationTrainedUnits.cs:364–400](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L364-L400): TryComposeFromWordMappings пропускает любое неузнанное слово, не вставляя placeholder или отказ; допускает ответ, если ≥2 отображённых слов и mappedRatio≥0.50. Затем [Execute:270–282](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L270-L282) возвращает такой результат с Confidence=0.50 до KNN. Пример-проверка: «do not open files», где выучены только open→открывать и files→файлы, теряет «do not»; следует проверить настоящую классификацию POS и is-target-compatible до утверждения об end-to-end исходе.

### Почему это отдельный новый DIF

В существующих [16 — verifier bypass и source collisions](16-INDEPENDENT-TRANSLATION-UNIT-AND-KNN-LEARNING-GAPS-2026-10-16.md) проверяли допускающую ошибку ветку verifier и нормализацию ключей; [16 — поздние KNN corrections](16-INDEPENDENT-NEW-GAPS-KNN-CORRECTIONS-AND-UNIT-TRUTH-2026-10-16.md) изучал порядок соседей. Здесь ставится **иной различающий вопрос**: что происходит с важной информацией source, для которой нет phrase/word correspondence, но остальное предложение частично выучено? Даже идеальный KNN и корректная LoRA не спасут смысл, если ранняя ветка phrase-map возвратила уже укороченную семантику.

## N21-02 — whitespace несовместимость между tokenizer, exact memory и semantic signatures

**C — прямое свойство реализации:** [TranslationSemanticTokenizer.cs:95–149](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationSemanticTokenizer.cs#L95-L149) использует Split(' ', RemoveEmptyEntries) и строит POS/concepts/signature по этим сегментам. При «the service» → «the<TAB>service» текст уже не разбивается на те же два слова. Между тем [NormalizeMapKey:1008–1024](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L1008-L1024) делит по всем whitespace с помощью Split((char[]?)null,...), поэтому exact-memory key может **остаться одинаковым**, в то время как structural signature, POS и KNN address изменятся. 

**H — влияние:** при отключённом exact-lookup или новом source с общими выученными фразами, разные формы форматирования могут маршрутизироваться в разные KNN buckets и разные Unit ветки. Для кода, JSON или многострочных инструкций это нетривиально. Проверить отдельно, потому что exactSourceMap может маскировать проблему на одном и том же исходном предложении.

**Микро-контроль:** одинаковые source после NormalizeWhitespaceForTokenizer (только замена whitespace одним пробелом), без перемен обучающей памяти и без нормализации знаков C#/операторов, должны иметь одинаковые токены/концепты/structureSignature; изменение gold качества проверяется исключительно на source-disjoint test sentences.

## N21-03 — legacy TrainBatchAsync имитирует прирост accuracy

**C — прямой код:** [TranslationMachine.cs:41–87](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L41-L87) — старый метод для плоских LearningTrainingItem. Он не вызывает ни ITranslationUnitTrainer.TrainOnExample, ни DpoTrainer, ни LoRA, ни KNN.AddPair. Он вычисляет AccuracyAfter = min(AccuracyBefore + 0.01, 1), записывает RowsTrained и training history, увеличивает patch-version и возвращает success. Комментарий прямо называет его legacy scaffold, а не реальным Unit-learning path.

**Контраст с настоящим обновлением:** [TranslationMachine.cs:89–145](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L89-L145) — TrainStructuredBatchAsync вызывает trainer.TrainOnExample на каждом известном UnitId; хотя отдельные огрехи его отчётности уже описаны в [17 — silent dispatch](17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH.md). **Нельзя смешивать flat scaffold, structured memory training и neural DPO.**

**H — причинность:** если какой-либо orchestrator, Graph route, RP01 adapter или отчет ошибочно трактует успешный flat TrainBatchAsync как измеренную adaptive competence, регрессия будет скрыта фиктивной метрикой. Но в этом аудите **не доказан фактический вызов flat метода из RP01**; нужен caller trace. Даже без live caller это важный запрет для будущего learning acceptance.

**Отличающий дешёвый probe:** 2 независимые идентичные TranslationMachine; A получает плоский batch из 2 допустимых записей; B получает соответствующие typed structured examples. Сохранить digest trained maps/KNN и direct Unit + ordinary Flow ответы до/после, отдельно от возвращаемых RowsTrained/AccuracyAfter. Сначала убедиться, что B действительно получил actualTrainInvocation>0. В A ожидается счётчик +0.01 при state digest unchanged. Не объявлять B лучшим, если quality не стала выше на независимом наборе.

## Сверка с соседними ветками: inherited ≠ newly introduced

[DesignToCode/TranslationSemanticTokenizer.cs:95–105](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/27.TranslationDomain/TranslationSemanticTokenizer.cs#L95-L105) уже использует Split(' ', ...). [DesignToCode/TranslationTrainedUnits.cs:793–852](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/27.TranslationDomain/TranslationTrainedUnits.cs#L793-L852) уже содержит phrase coverage/repeated spans; [DesignToCode/TranslationMachine.cs:39–85](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/27.TranslationDomain/TranslationMachine.cs#L39-L85) — плоский +0.01 scaffold.

**Вывод археологии:** эти механизмы **не доказаны регрессией, внесённой RP01**, они исторически наследуются; новая находка — выявление ранее не изолированного причинного механизма и доказательного теста, а не заявление о точном виновном коммите. Ветка language_translate существенно раньше и не содержит этих файлов по путям 27.TranslationDomain; отсутствие точного пути в ветке не доказывает отсутствие аналогичной способности.

## Что эти находки НЕ доказывают

- Не доказывают, почему случайный B0 Student после C#500/ENRU500 всё ещё дал 0/3 и 0/4: путь [RP01 TextGenerator](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) проверяется отдельно.
- Не доказывают, что KNN/phrase learner хуже DPO: сравнение возможно лишь при одном downstream endpoint, frozen ground truth и одинаковой панели.
- Не доказывают, что все фразы «нужно» автоматически запрещать, а все неизвестные слова «надо копировать». Требуется law-aware сохранение смысла, а не механический all-or-nothing в долгосрочной реализации.
- Не доказаны фактическое негативное изменение пользовательской accuracy, распространённость в корпусах или успешность patch. Всё это предмет эксперимента.

## Приоритет для архитектора, reviewer и инженера

**Первая проба (P0, ≤1 компактный запуск):** [21-MICRO-PHRASE-CLAUSE-SURVIVAL-EXPERIMENT-2026-10-16.md](21-MICRO-PHRASE-CLAUSE-SURVIVAL-EXPERIMENT-2026-10-16.md). На **одной фиксированной обученной памяти** сравнить обычную фразовую композицию и shadow policy, запрещающую потерю заранее заданного смыслового инварианта. Измерить law-correct, critical-error rate, coverage/abstention, route origin. Не переписывать продукт до результата.

**Вторая проба (P1, локальная):** tokenizer whitespace parity с двумя формами каждого одного источника; сначала equality tokens/signatures, потом source-disjoint semantic quality.

**Третья проба (P1):** caller proof для flat TrainBatchAsync: найти настоящий ordinary caller; если его нет — классифицировать dead legacy trap и не выдавать за причину RP01. Если есть — доказать changedState=false vs reportedAccuracy +0.01 и заменить измерение только после review.

**Разделение ролей:** новый учебный закон и выбор задачи — CODEX (Astra 6 UltraCode); обе стороны новой настоящей DPO пары — только разрешённая внешняя открытая Chinese author model с корректным provenance; CODEX/Astra независимо проверяет пару и Student. Для **этой маленькой проверки уже сохранённых Unit maps новые DPO не требуются**; не подменять независимый human-review rule ручным придуманным gold. Другие алгоритмы/тесты не запускались здесь.

**Ревью:** code audit ≠ измеренный learning uplift; для повышения до VERIFIED_CAUSAL потребуется actual native before/after, frozen fixture, per-case traces и одобренный semantic oracle. Отчёт не должен запускать очередной Windows CI или 500 DPO по инерции.
