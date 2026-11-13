# N23 · Потеря ранее выученного перевода внутри QualityVerifierUnit

**Дата:** 2026-10-16. **Статус:** DRAFT; source-confirmed механизм / формульный JS proxy; native C# и независимая оценка качества НЕ проводились. **Цель:** найти дополнительные различия, не повторяя уже описанные DIF-01–44, N14, N16, N20–22. **Core snapshot:** [codex/dpo-implementation@0eccbdc3](https://github.com/lubochka/llm_mvp_core/tree/0eccbdc3c4efb90adf1d699806cbfa9f5610998d). Planning branch: llm_plan3.

## Коротко для человека

Структурный переводчик действительно учится: добавляет правила контроля правильности, исправления и языковую память. Однако когда два разных правильных урока попадают в одну *структурную сигнатуру*, проверяющий накапливает их обязательные слова и начинает требовать **все** слова одновременно от **каждого** ответа. После добавления правильного второго урока он способен отклонить ранее правильный ответ. Это не проблема неверной LoRA, не недостаток kNN и не ошибка данных как таковая — это иной способ представления learned constraints.

**Второй независимый дефект границы:** один экземпляр QualityVerifierUnit используется всеми направлениями перевода. В правилах нет ключа translation direction / target language. Обучение одного направления может менять допустимость ответа другого, если сигнатуры или concept-ссылки совпадут.

Это **вероятные причинные механизмы для structured translation path**, но **не доказанные причины конкретного Student-only C#500/ENRU500**, поскольку RP01 training path не показал вызова этих Units.

## N23.01 · Нелокальное накопление противоречащих требований под одной сигнатурой

**Прямая улика C1 — адрес правила.** [TranslationTrainedUnits.cs, 1041–1050](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TranslationTrainedUnits.cs#L1041-L1050): _requiredTermsBySignature — Dictionary<string,List<string>>. Ключ — структурная сигнатура, а не точная исходная фраза, направление или предметный закон.

**Прямая улика C2 — обучение.** [TrainOnExample, 1095–1143](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TranslationTrainedUnits.cs#L1095-L1143) при FAIL-уроках парсит требование и делает requiredTerms.Add(req). Не записывает, является ли req альтернативой, обязательством только при определённом исходном референте, целевым языком или отдельной веткой условия. Дубли возможны.

**Прямая улика C3 — inference.** [Verify, 1051–1090](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TranslationTrainedUnits.cs#L1051-L1090): для любого sourceTokens.Signature совпадения перебирает каждый required и записывает issue при отсутствии этого слова. Условие Pass — issues.Count==0. Следовательно семантика списка здесь **AND**, а не "подходящее требование для выбранной подзадачи" и не набор альтернатив OR.

**Прямая улика C4 — коллизия допустима конструкцией.** [TranslationSemanticTokenizer.Tokenize, 115–159](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TranslationSemanticTokenizer.cs#L115-L159) строит Signature из Pattern + распознанных Concept + RuleTags. RawText, полная identity source, target language и TopicDomain в ключ не входят. Поэтому несколько разных законных фраз могут иметь одну сигнатуру. Частота реальных столкновений **не измерялась**.

**Прямая улика C5 — ответ пользователя.** [TrainedTranslationAdapter.TranslateAsync, 115–143](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TranslationDirectionAdapters.cs#L115-L143) вызывает Verifier и для обычного non-phrase кандидата при FAIL возвращает Bootstrap fallback. Есть особое исключение для phrase-map с confidence>=0.85; этот отдельный известный риск не скрывать и не смешивать с N23.01.

**Проверяемый сценарий:** исходный закон для source A / Signature S требует целевого слова "thread", source B / того же S требует "process", причём оба правильные переводы должны быть разными. После урока A ответ A принимается. После добавления урока B верный A отвергается как не содержащий "process"; верный B отвергается как не содержащий "thread". Бессмысленный ответ с обоими словами проходит эту **конкретную** проверку (хотя другой внешний oracle может отвергнуть). Сценарий **условный**: реальные S и реальные источники должны быть найдены native Tokenize, а не заявлены существующими по умолчанию.

**Отдельная локальная проверка формулы (P, НЕ native).** Исполнена минимальная JS-реплика именно связки requiredTerms.Add + foreach Contains. Строка "start thread": PASS после ["thread"], FAIL после ["thread","process"]; "start process": FAIL после обеих требований; "start thread process": PASS. Это арифметика правила, не тест настоящего Unit и не оценка языковой правильности. Подробный protocol/контроли: [23-MICRO-VERIFIER-CAUSAL-LEARNING-EXPERIMENT](23-MICRO-VERIFIER-CAUSAL-LEARNING-EXPERIMENT-2026-10-16.md).

**Чем отличается от N21:** [21-INDEPENDENT-STRUCTURED-LEARNING-GAPS](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md) обсуждает неверное применение concept context и проверку substring вместо whole word. N23.01 — **самостоятельная семантика накопленных альтернатив**, воспроизводимая даже при идеально распознанных словах и без concept-context ветки. Не называть прежние findings новыми.

## N23.02 · Кросс-языковое распространение правил QualityVerifier

**C1:** [TranslationMachine.CreateRuntime, 717–777](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TranslationMachine.cs#L717-L777): создаётся **один** verifier = new QualityVerifierUnit(), тот же объект передаётся trained adapters RU_EN, EN_RU, HE_EN, EN_HE, ES_EN, EN_ES.

**C2:** [QualityVerifierUnit, 1041–1090](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TranslationTrainedUnits.cs#L1041-L1090): ключи rule dictionaries — (Src,Ctx), Signature, SourceMarker. Нет Direction/TargetLanguage. Verify получает только CandidateText и SemanticTokenSequence. Тренировочные данные могут иметь направление, но здесь оно не присутствует в принятии решения.

**Риск:** правило RU→EN для общей структурной сигнатуры может отклонить lawful ответ EN→RU/HE→EN, если другой язык ожидает иные слова. Даже если первые два домена обладают разными RuleTags, для некоторых пар без этих tags общие сигнатуры остаются возможными. Нужен native collision inventory.

**Отличие от [16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS](16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md):** там смешиваются **stored retrieval targets**, здесь **правила финального проверяющего**, способные отвергнуть результат корректно разделённых ExecutorUnits.

## Археология соседних веток — inherited ≠ RP01 regression

[DesignToCode@8f20ceb:TranslationTrainedUnits.cs, 1032–1138](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/27.TranslationDomain/TranslationTrainedUnits.cs#L1032-L1138) уже имел те же _requiredTermsBySignature/foreach Contains/requiredTerms.Add. Таким образом N23.01 — **уже существовавшая семантика**, не доказанный дефект, впервые добавленный при RP01. Соседние language_translate/external_domains не содержат этот конкретный файл в проверенных путях; их положительные опыты нельзя механически атрибутировать к этому Unit.

## Что проверить без нового 500-pair DPO run

1. Найти два реально разных source с **одинаковыми** Signature и несовместимыми обязательными target terms; зафиксировать exact bytes и независимый ground truth. Собрать отдельную пару с различными сигнатурами для негативного контроля.
2. Native QualityVerifier A: обучить только первое требование; затем B: второе — в том же Unit. Не трогать Student, LoRA, KNN, decoder, sampling.
3. Native QualityVerifier C: те же требования разнести по независимым направлению/условию (test-only shadow dictionary), не переписывая production code; D: та же пара при разных Signature.
4. Измерить для source-disjoint cases **pass of lawful answer**, **false accept of unlawful answer**, количество регрессий A→B, frozen candidate text, exact Signature/Direction, accuracy с явным знаменателем и actual consumer fallback. Отдельно replay после ExportCheckpoint/ImportCheckpoint — чтобы исключить restore.
5. Если нет реальной коллизии или она не затрагивает обычный consumer, сохранять H как риск API, а не RP01 root cause.

Гипотеза опровергнута как *причина деградации на изученном наборе*, если реальные сигнатуры различаются или корректная ветка consumer всегда обходит эти правила; один synthetic proxy не даёт права утверждать обратное.

## Ограничения / права

- **C (code mechanism):** подтверждён по source; **P (formula proxy):** выполнен отдельно; **H (causal impact):** НЕ ПРОВЕРЕН; **M (native paired quality gain):** отсутствует.
- Выбор учебных случаев, смыслового закона и итоговая независимая оценка — CODEX/Astra 6 UltraCode по проектному контракту; создание новых DPO chosen/rejected, если понадобится, — исключительно допустимой внешней открытой китайской моделью. Здесь DPO **не создавались** и вызовов судьи **не было**.
- Не изменять frozen C#500/ENRU500, критерии heldout, публикацию checkpoint или историческую метрику. Не подменять conditional learning evidence зелёным unit-test.
- На момент сравнения просмотренных соседних N14/N16/N20/N21/N22 отдельный механизм conjunction + cross-direction **не был выделен как отдельный причинный contrast**; не заявляется, что все документы RAG или все коммиты истории были прочитаны.
