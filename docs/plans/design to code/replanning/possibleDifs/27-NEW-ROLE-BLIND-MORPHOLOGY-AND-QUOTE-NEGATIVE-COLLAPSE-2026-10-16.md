# N27 — Новые независимые механизмы порчи учебного сигнала: роль слова → морфология → ложные переводческие пары

**Дата:** 2026-10-16. **Область:** source-backed audit структурных RU→EN translation Units; **не** самостоятельная DPO/LoRA-ветка научного RP01. **Первичный код:** llm_mvp_core/codex/dpo-implementation@8c7d41611aecba98d3d1bbdbf30d69bd784949ae; проверенные blobs: TranslationFrequencyQuoteMap.cs cb4344caeb8f0751dd5df353ea6dccf47254260f, TranslationTrainedUnits.cs 1742abfc9fb617dfc66e4d618e0c169da01a6ccc, TranslationMachine.cs fd0e736de3739236b4dde2d6b9d000f5ebae8416. **Статус:** CODE_CONDITIONS_VERIFIED / MINIMAL_SOURCE_EQUIVALENT_JS_PROXY_EXECUTED / NATIVE_DOTNET_NOT_RUN / HELDOUT_LEARNING_GAIN_NOT_MEASURED / INDEPENDENT_CODEX_ASTRA_REVIEW_NOT_RUN.

## Человекочитаемая причинная цепочка

Частотная обучающая запись несёт явно заданную грамматическую роль слова. При превращении такой записи в Unit-урок роль пропадает. Затем слово и его перевод попадают в словарь без признака «существительное/глагол». Отдельная функция производит «альтернативную форму прошедшего времени» **только по окончанию -л/-ла, независимо от грамматической роли**. В результате обучение правильному существительному **может записать неправильный перевод глагола и затереть ранее выученный закон**.

Пример (искусственный, не найденная реальная пользовательская ошибка):

- Правильная запись V: «пил» (past verb) → «drank».
- Правильная запись N: «пила» (noun, инструмент) → «saw».
- Текущая функция для N создаёт ложный gender-alternate «пил» → «saw».
- После N сохранённая surface memory для «пил» имеет значение «saw». Порядок обучения влияет на ответ. POS-aware вариант не создаёт эту ложную запись.

Это **отдельная** ошибка от ранее найденных FIFO-KNN, потери SourceLang, E26 left/right POS и LastWriteWins для exact-source: здесь *сам обучающий код синтезирует неверный новый пример*, которого не было в chosen DPO/Unit material.

## N27-01 (P0 conditional) — GrammaticalRole есть у источника, но исчезает при создании Unit

| Прямой код | Доказанное свойство | Ограничение |
| --- | --- | --- |
| [FrequencyQuoteEntry:14–22](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationFrequencyQuoteMap.cs#L14-L22) | Вход содержит отдельный GrammaticalRole, SourceWord, SourceLang, SourcePrefix и TargetPrefix. | Правильность этих меток для реального корпуса ещё не проверялась. |
| [BuildBatch:44–96](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationFrequencyQuoteMap.cs#L44-L96) | Весь цикл обращается к SourceWord и SourceLang, но **ни разу не читает GrammaticalRole**; создаёт ExecutorWordMapping(SourceWord, SemanticConcept.UNKNOWN, marker). | В некоторых одно-значных лексемах отсутствующая роль безвредна. |
| [ExecutorUnit.TrainOnExample:403–455](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L455) | Перевод добавляется в WordMappings и общую память; информация о реальной роли уже невосстановима из ExecutorWordMapping. | Сырой SourceText/токены сохраняются отдельно; часть запросов может быть спасена KNN/exact. |
| [AddWordMapping:744–753](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationTrainedUnits.cs#L744-L753) | Ключи — (NormalizeMapKey(source), concept) и surface key; GrammaticalRole отсутствует. | Случаи с разными semantic concepts могут быть разнесены в контекстной карте, но **surface key всё равно общий**. |

**Фальсификатор:** две корректно размеченные записи с одинаковым SourceWord «пила» и противоположными GrammaticalRole=NOUN/PAST_VERB; сохранить current BuildBatch, проверить три Unit-записи каждой, потом фактический state after train с обоими порядками. Возможное исправление не должно превращаться в if-проверку слова «пила»: нужна валидированная role/POS identity и миграционная версия состояния памяти. Это не прямое доказательство причины RP01 Student 0/4.

## N27-02 (P0 conditional) — DeriveGenderAlternateForm создаёт ложный обучающий ярлык из имени существительного

1. [TrainOnExample:424–450](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationTrainedUnits.cs#L424-L450) применяет DeriveGenderAlternateForm ко **всем** WordMappings; перед вызовом нет проверки POS, SourceLang или глагольной роли.
2. [DeriveGenderAlternateForm:760–795](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationTrainedUnits.cs#L760-L795) определяет форму только по Unicode/длине и окончанию -ла / -л. «пила» → «пил» следует из кода независимо от исходной грамматики.
3. [AddWordMapping:744–753](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationTrainedUnits.cs#L744-L753) осуществляет overwrite по ключу, а [TryGetAcceptedWordMapping:860–897](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationTrainedUnits.cs#L860-L897) читает и contextual, и surface mappings при inference.
4. Для RU_EN нет EN_RU-ограничений на такую английскую цель в ShouldAcceptStandaloneWordMapping, см. [538–564](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationTrainedUnits.cs#L538-L564).

**Механический пример:** valid verb урок «пил»→«drank»; затем valid noun урок «пила»→«saw»; в обоих выбранная сторона законна. Текущая функция автоматически устанавливает «пил»→«saw» поверх verb-знания. При POS-gated деривации noun не создаёт alternate; старый перевод verb сохраняется. **Пока это доказательство содержимого memory по формуле, не наблюдённый consumer output**: exact-source/phrase/KNN или отсутствие двух доступных word mappings могут маскировать дефект. Поэтому маленький эксперимент имеет обязательный L0 reachability и L1 ordinary Flow quality.

**Предметно важное следствие:** обучение нового законного слова может ухудшить **retention старого** без DPO-gradient, даже если все тесты правильности JSON, размеров KNN и checkpoint PASS.

## N27-03 (P1, data-validity hypothesis) — TargetPrefix не является подтверждённым выравниванием; negative может стать text-tie

1. [ExtractTargetMarker:132–139](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationFrequencyQuoteMap.cs#L132-L139) берёт **последнее слово TargetPrefix** (или весь prefix при пустом regex), без проверки его соответствия SourceWord/GrammaticalRole.
2. [BuildBatch:47–96](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationFrequencyQuoteMap.cs#L47-L96) сразу назначает этот marker переводом SourceWord и именует изменённый full-target ошибкой «prefix_continuation_lost». Даже если выбранное слово — артикль «a» в TargetPrefix «She bought a», для «пила» оно становится неправильным WordMapping «пила»→«a».
3. [BuildRejectedCandidate:142–155](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationFrequencyQuoteMap.cs#L142-L155) удаляет marker regex-ом из FullTargetSentence. Если marker **не встречается**, rejected **равен** chosen по тексту; если удаление оставило только пустую строку, метод тоже возвращает original. После этого [BuildExample:106–126](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationFrequencyQuoteMap.cs#L106-L126) всё равно создаёт пару — text-tie **не отклонён**.
4. [ExecutorUnit.TrainOnExample:404–411](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationTrainedUnits.cs#L404-L411) читает chosen, а не rejected; поэтому text-tie здесь **не доказывает нулевой DPO-gradient RP01**, но доказывает, что семантика «отрицательного контраста» не обеспечивается producer-ом для последующего потребителя.

**Контрольные примеры:** «She bought a saw» + TargetPrefix «unseen marker» → отрицательная сторона того же текста. «She drank water» + TargetPrefix «She drank» → «She water» (стороны различаются, но независимая проверка *семантической* ошибочности всё равно нужна). Пример «She bought a» → marker «a» показывает возможный misalignment и не является настоящей статистикой корпуса.

## Положительная археология и граница RP01

- Исторический [DesignToCode@8f20ceb: TranslationTrainedUnits](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/27.TranslationDomain/TranslationTrainedUnits.cs#L399-L445) также содержит gender-alternate вызовы, а [исторический метод около L769](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/27.TranslationDomain/TranslationTrainedUnits.cs#L755-L793) показывает, что seam **унаследован**, а не доказан как изменение, привнесённое RP01. У старой ветки DesignToCode уже существовал FrequencyQuoteMap со сходной структурой; источник не даёт нового факта успешного quality-gain от этой функции.
- Успешные исторические кластеры learning включали learned KNN/translation mappings, а не только gradient LoRA. Сопоставление фиксировано в [master](00-MASTER-SYNTHESIS-AND-CONFLICTS.md) и [историческом ledger](26-INDEPENDENT-FULL-GAP-REGISTER-AND-HISTORICAL-LEARNING-LINEAGE-2026-10-16.md). Фактический neural RP01 Student path — **другой получатель**; current source evidence не доказывает вызова FrequencyQuoteMapBuilder в main500/ENRU500.
- Уже найденные другим агентом **жёсткий executor-ru_en для всех FrequencyQuoteEntry** и symbol-blind Batch029 admission: [N21 source audit](21-NEW-GAPS-SYMBOL-BLIND-LESSON-ADMISSION-AND-DIRECTION-MISROUTING-2026-10-16.md). **Не выдавать это за новую N27-находку.** N27 проверяет *роль, ложное морфологическое дополнение и качество target marker/negative*, а не сам UnitId.
- Родственные предыдущие работы: [N23 E26 POS](23-NEW-E26-POS-CONTEXT-TO-LEARNED-UNIT-2026-10-16.md), [DIF-20 pair quality](01-PRIORITIZED-DIFFERENCES.md), [N16 memory visibility](16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md), [N17 dispatch](17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH.md).

## Практический приоритет и verdict

**Первый действительный вопрос:** доходит ли FrequencyQuoteMapBatchBuilder до actual training в интересующем проекте? Если реальных callsites/переданных quote rows нет — записать NON_REACHABLE_FOR_RP01 и не отвлекаться на ремонт этой ветки вместо DPO cold-start/warm SFT. Если доходит — сначала предотвращать **автоматическую генерацию противоположного ложного word mapping**, затем измерить source-disjoint законный перевод после второго урока.

- N27-01: SOURCE_CONFIRMED representation loss; CAUSAL_QUALITY_UNKNOWN.
- N27-02: SOURCE_CONFIRMED unsafe heuristic and conditional map overwrite; CAUSAL_QUALITY_UNKNOWN.
- N27-03: SOURCE_CONFIRMED unvalidated marker / conditional text-tie; CORPUS_OCCURRENCE_UNKNOWN.
- Старый negative matrix→successful KNN **не заменять** рассказом о morph suffix: эти линии требуют отдельной фактической genealogy.
- Никакой .NET модельной тренировки, настоящих китайских teacher calls или CODEX/Astra independent judging в этом аудите не было.

**Малый фальсифицируемый эксперимент:** [27-MICRO-ROLE-GENDER-RETENTION-CAUSAL-EXPERIMENT](27-MICRO-ROLE-GENDER-RETENTION-CAUSAL-EXPERIMENT.md); [reproducible JS source-proxy](27-ROLE-GENDER-QUOTE-MICRO-PROXY.mjs) и [raw results](27-ROLE-GENDER-QUOTE-MICRO-PROXY-RESULT.json). Тестировать actual Unit state, обычный TranslationRoutingFlow, холодный restore и semantic law gold по заранее закрытой панели, а не только изменённый SHA и число обученных строк.
