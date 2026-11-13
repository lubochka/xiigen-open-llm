# N41-T2 — отрицательная диагностическая сторона управляет положительной памятью перевода

<!-- XIIGEN-AUDIT-2026-11-11-INSERT -->
<!-- AUDIT-2026-11-11-EV-P52:START -->
## Уточнение baseline и границы P52 — 11 ноября 2026

N41-T2 ниже остаётся самостоятельной гипотезой phrase-mapping provenance, а не результатом P52. Последующий P52 attempt2 на core `009bf226174441fa0fbf0336e74ad2f2b7174d92` сохраняет узкий gain адреса1/4→3/4 при неизменных переводах4/4→4/4 через exact-source adapter. Но его writer ошибочно называет cold0/4 потолком постоянного ответа; независимый максимум по двум gold-строкам равен2/4. Перепроверка10.11 сравнила это поле снова с cold и потому не обнаружила подмену величины.

Для A/B/C/D §2–3 отдельно фиксировать observed cold, максимальный score постоянного ответа по тому же правилу equality и ordinary consumer gain. Ожидание для baseline строится из gold histogram; negative с cold0 и ceiling>0 обязан обнаруживать подмену. Совпадение двух JSON-полей не является независимой проверкой смысла. Сохранить trusted/harmful/no-marker контрасты и исходные chosen bytes; успех адреса не объявлять качеством unseen composition. Ремонт writer и датированная correction без перезаписи raw — [дополнение11.11](2026-11-11-P48-P58-EXPERIMENT-MEASUREMENT-ADDENDUM.md). Новый native запуск не выполнялся.
<!-- AUDIT-2026-11-11-EV-P52:END -->


18.10.2026. **SOURCE-CONFIRMED DATA-FLOW SEAM; QUALITY IMPACT HYPOTHESIS; NATIVE EXPERIMENT NOT_RUN.** Независимый от P52.1 (RuleTags), P52.5 (model-visible diagnostic marker) подслучай: **откуда обучаемый Executor получает trusted phrase mappings**. Не утверждать, что marker обязательно ошибочен; при достоверной аннотации он может быть полезным и это надо сохранять.

## 1. Точная цепь от DPO пары к памяти

1. [TranslationStructuredBatchBuilder.cs L94–104](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L94-L104) строит **phraseMappings = BuildPhraseMappings(pair.Rejected)**, agreement mappings и rule tags также берутся из rejected. Это не парсинг выбранного правильного перевода.
2. [Builder L112–131](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L112-L131) создаёт для Executor **ChosenOutput = new ExecutorOutput(pair.Chosen, 0.93, wordMappings, phraseMappings)**. Поэтому metadata негативной стороны превратилась в позитивную структуру chosen subunit.
3. [Builder L403–416](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L403-L416) ищет любые **[PHRASE: source => target]** из строки rejected, не сравнивая автоматически target с реальной chosen alignment. Это source-level факт об этом parser; внешняя проверка другой стадии может существовать.
4. [ExecutorUnit.TrainOnExample L403–456](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L456) при обучении пишет chosen.Text в exact/KNN, затем **каждый chosen.PhraseMappings в _phraseMap**. Этот файл *одинаков* в PrepareToGPL и current (blob SHA 1742abfc9fb617dfc66e4d618e0c169da01a6ccc).
5. [ExecutorUnit.Execute L263–275](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/27.TranslationDomain/TranslationTrainedUnits.cs#L263-L275) предпочитает phraseMap перед KNN, следовательно marker может влиять на **другой, ранее не виденный source**, где точного урока нет.
6. На **другом** учебном пути [TranslationDpoTrainerBridge.cs L183–203](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationDpoTrainerBridge.cs#L183-L203), [L270–312](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationDpoTrainerBridge.cs#L270-L312) diagnostic markers из model-visible text вырезаются. **Не доказано**, что этот bridge служит upstream именно данного Builder; нельзя вывести loss without caller trace. Но общий контракт «одни только DPO token tensors содержат все данные для typed-Unit training» неверен, если annotation была единственным носителем mapping.

Прямое кодовое свидетельство: из пар с **одинаковыми source/chosen**, но разным текстом rejected-маркера Builder может строить **разные** positive PhraseMappings. Это конкретная возможность нежелательного *annotation-to-memory promotion*. Фактическая порча данных, отсутствие upstream semantic-validator, рост качества от иной политики — **не измерены**.

## 2. Минимальный опыт — DPO-PHRASE-AUTH-01

**Тема:** перевод. **Профиль:** EN→RU, затем RU→EN; отдельно только при наличии верной языковой рубрики HE↔EN. **Получатель:** structured Executor memory и ordinary TranslationRoutingFlow. НЕ Student weights.

Подготовить 3 копии одного TRAIN примера: одинаковые ID с локально изолированными машинами, prompt, source **validate the file**, chosen **проверь файл**, rejected candidate по смыслу одинаковый, различается лишь diagnostic block:
- A (trusted correct annotation): [PHRASE: validate => проверь] [PHRASE: file => файл].
- B (semantically false annotation): [PHRASE: validate => удали] [PHRASE: file => файл].
- C (no phrase annotations): никаких [PHRASE:] при том же chosen и rejected candidate.
Отдельный marker provenance gate (D) допускает phrase mapping только через **независимо проверенный структурированный TRAIN field**, не разбираемый свободный rejected text. На стадии проверки механизма используйте заранее объявленные поля вручную; это не доказательство наличия production authorizer.

Для каждой копии вызвать **именно BuildGeneralBatch(..., signatureMode: ExactPair)**, сохранить построенный ChosenOutputJson и происхождение каждого mapping. На каждой fresh TranslationMachine выполнить **только Executor lesson(s)** через TrainStructuredBatchAsync (semantic/verifier остаются frozen) и пустые validation examples не отправлять в trainer. Снять *реальный* memory checkpoint и ordinary Flow на **unseen recombination** (например validate the cache / delete the file), затем на source с отрицанием, вопросом и соединением двух объектов. Existing explicit P125 fixtures можно использовать только при разрешении на доступ и с pinned gold; не менять их post-hoc.

**Критический negative control:** A/B/C имеют одинаковые chosen-response bytes и direct exact-source output; поэтому exact TRAIN recall может остаться идеальным и скрывать divergence. Изменение unseen output при перестановке **только rejected diagnostic** подтверждает зависимость от отрицательной разметки. В D проверяющий допускает проверенные source/chosen-aligned mappings, отвергает ложный marker и сохраняет ожидаемый честный exact recall.

## 3. Что измерять и когда объявлять причиной потери качества

Сохранять таблицу для каждой группы: parser_mappings, admitted_mappings, rejected_reason, memory_hash, phraseMapEntries, exactEntries, KNN triples, selected_route, selected_lesson, source/chosen strings SHA, correct/all, wrong_accepted/all, answered/all, correct/answered, operator violations, training ms, ordinary latency ms, checkpoint bytes, group RSS. Neural trainable layers N/A; Unit/Flow depths фиксировать отдельно.

**Подтверждение механизма:** B записывает target «удали» в положительную phrase memory при неизменном правильном chosen и влияет на ordinary unseen composition, тогда как D при том же входе не пишет ложный mapping. Если B не проникает из-за upstream validation — поставить SOURCE_CANDIDATE_NOT_REACHABLE, **не баг**. Если divergence есть только в train JSON, но ordinary Flow отказывается — это data-contract риск, не наблюдённая потеря final quality.

**Ограничение доменов.** Особенно существенен для перевода с повторно комбинируемыми action/object phrases и ролями. Не обобщать на C# AST, SQL, orchestration или generic neural LoRA без соответствующего actual builder/recipient. Нельзя по одному качеству фразы научить забывать отрицание: [P125 real 8-lesson result](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md) 19/24 правильных сопровождались 3 опасными принятыми edges.

## 4. Самопроверка новизны

Ранее: [P52 semantic annotation](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P52-semantic-annotation.md) содержит проверки RuleTags из rejected и model-visible diagnostic. Здесь отдельный фокус — **отрицательный свободный комментарий → chosen typed payload → обученное состояние Executor → unseen answer** с независимым harmful annotation A/B/C. Никакие исправления/запуски кодов в core не производились; source review конечного consumer выполнен адресно, не всего проекта.
