# 49/03 · Малые научные проверки вместо большого RP01 run

**Статус:** опытные карточки SPECIFIED, новые native L/U **NOT_RUN**, source-equivalent historical projection **VERIFIED**. Фиксировать inputs/expected result и критерий решения до любого нового обучения. Экономить CPU/GPU: сначала проверка информации, только затем настоящие training/consumer tests.

Используется лестница `R` архивные receipts → `S` source-equivalent/counterexample → `C` настоящий компонент → `L` реальное изменение весов/памяти → `U` реальный потребитель после новой загрузки. Не превращать S в выдуманный результат L/U.

## W1 / wiki routing / **P0** — обучать тип ошибки или выбирать Unit по тексту?

- **Гипотеза:** сохранённый исходный запрос и правильный target route позволяют реальному recipient научиться различать две новые формулировки, тогда как defect-only тройка не содержит требуемого признака.
- **Домен:** оркестрация wiki-проекта; профили: `CodeTraceTraining` vs `AnswerComposerTraining`, `ProjectValidation` vs `WikiQuestionAnswer`, negative control `NotProjectWiki`. Никакой Roslyn feature для чистого routing.
- **Доказательство входа:** old/new methods из [49/01](01-DPO-PAIR-LOSS-AND-HISTORICAL-PROJECTION.md); старые archived 50 train/50 heldout преобразуются по type-only в 4 класса, один объединяет 6 target route. Это уже source-equivalent S, а не native.
- **Fixture C (4 пары):**
  - «Train the answer composer using cited evidence» → `AnswerComposerTraining`.
  - «Train the code trace builder using method calls» → `CodeTraceTraining`.
  - «Validate wiki evidence across projects» → `ProjectValidation`.
  - «How many chairs should I buy?» → `NotProjectWiki`.
  - Для первых двух создать одну и ту же явную категорию `wrong_training_unit`, но **разный chosen target**, модельная строка должна отличаться в restored/source-conditioned arm. Эти фразы — тестовые задания, не proven teacher-authored DPO.
- **S/C:** вывести exact model-visible `prompt/chosen/rejected` без PairId для old/source-conditioned A и current/type-only B. Ожидаемое code property: A различает первые два, B — нет. Native 50 current metric batch обязателен для измерения фактической частоты B; исторический 50→4 не подменяет его.
- **L:** разрешённые и действительно admissible 8–16 DPO пар после one-factor modification, **один recipient** и один initial checkpoint, same tokenizer/budget/optimizer/RNG, 2 seeds если дешево; у A — old exact-pair bridge, B — current surrogate, больше ничего не менять. Честно записать источник обеих сторон DPO и reject admission. 12–24 новых prompts с другими формулировками и проектами; split по prompt family (не только prefix `Held-out remediation:`). Metrics: **route exact accuracy**, macro-F1 per rare route, contrast-sensitive pair accuracy, inference latency/CPU, memory.
- **U:** только если L улучшился — restored snapshot через обычный consumer, где можно наблюдать chosen unit. A/B difference-in-differences (`A1-A0` vs `B1-B0`) отделяет просто кодовое исправление от обучаемости.
- **Pass:** предметный score A выше B на тех же unseen families, хотя бы одна ранее неправильная маршрутизация исправлена именно обучением и сохранена при U. **Fail:** A и B одинаковы, explicit source-conditioned trio не дошла до trainer, или consumer не читает learned state. **Stop:** после 1 source proof + 1 native mini if source loss already disproven.

## W2 / C# CodeTraceBuilder / **P0** — восстановить истинную трассу

- **Гипотеза:** frozen/cold model с feature `missing_callee` не может определить, какой именно callee пропущен; old true `A/chosen/rejected` DPO может исправить его при обновлении принимающего recipient.
- **Домен:** программирование C#, trace и Design-to-Code; AST, caller/callee/value-flow **только здесь**, не в переводе.
- **2 исходных исходника (контроль C):**
  - `int F(IRepo r) { return r.Load().Count; }` → call `IRepo.Load` и `Count` как property read, а не `Save`.
  - `void G(IRepo r) { r.Save(); }` → call `IRepo.Save`, а не `Load`.
  - Примеры иллюстративные: реальный gold сверить Roslyn/ручной экспертной разметкой, особенно method/property semantics. Дополнительная метка: `wrong_caller` должна различать caller scope. У обоих можно использовать `missing_callee` rejected category, но правильные traces отличаются.
- **S/C:** сравнить exact input/target в DPO bridge donor/current. Если source не входит, остановить matrix/LoRA tuning и исправлять *representation*, не выкручивать параметры.
- **L:** 8–16 source-disjoint C# methods, trainer/checkpoint идентичен, A=old exact `A/chosen/rejected`, B=type-only, one factor, shared independent gold; вычислять per-edge precision/recall и wrong extra edges (не только recall на gold, сгенерированном тем же extractor).
- **U:** спросить обычный CodeTrace consumer после reboot, compare before/after names and edges. Не считать `CodeTraceBuilderTrainingRunner` structural accuracy=1 частью neural gain.
- **Pass:** правильные конкретные edges на **необучавшихся методах**, сохранённое learned state. **Fail:** only keyword/defect label learning, no source-dependent improvement, opposite-domain regressions.

## W3 / wiki router publication & reload / **P0 после W1**

- **Гипотеза:** даже новый полезный router state не принимается, потому что `semanticQualityClaimed=false`, или используется static `ProjectWikiRequestRouterUnit` вместо обученного checkpoint.
- **C без обучения:** один текущий runner+snapshot с фактическими `HeldOutGatePassed`, `SemanticQualityClaimed`, `PublishAllowed`, `PublishAsync` calls; отдельно spy на `ProjectWikiTrainingOrchestrator` dependency `_router`.
- **L/U:** не менять guard на true; создать независимый смысловой экзамен (wrong route consequences, required inputs, target compatibility) и подключить явный gate к `ToSnapshot` только по реальному pass. Потом cold-restored exact checkpoint sha и ordinary wiki request. Контроль: `A0` original no learned state, `B0` same state + wiring/gate, `A1` old trained without wiring, `B1` trained + valid wiring.
- **Pass:** on new process learned route changed and correct, no false promotion, gate evidence verifiable. **Fail:** only summary/registry counters improved; U still static.

## D1 / RU↔EN контекст / **уже известная P0 линия, не N49 novelty**

- Опыт из [existing E26 / translation recovery](../44-LANGUAGE_TRANSLATE-SUCCESS-FIRST-RECOVERY-2026-10-19.md): два контекста для одного многозначного слова, POS роли/порядок слов; сравнить baseline deterministic lexicon, typed memory, kNN-aware context, neural recipient в раздельных arms. На **новых контекстах** judge word-sense correctness, negation preservation, direction. Не применять C# AST в этом домене.
- Имеющийся E26 архив **15 fixed cases 0.40→1.00**, поэтому те же строки нельзя использовать как новую независимую проверку. В паре «do not delete» перевод, который удаляет отрицание, — FAIL, даже если required word `файл` присутствует.

## D2 / C# AST structural kNN / **известная P0 линия**

- E09 старый top1 0.2413→0.3507 на разных validation denominators; matched same file-family split и label codec: baseline vs dynamic kNN+AST features, k=1/3/5; отдельно сравнить quality и latency. Rule/order-сигналы полезны для C#, но не автоматически для `ProjectWikiRequestRouter`.

## D3 / continual replay / **известная P0 линия**

- Два непересекающихся доменных семейства `old`/ `new`, same learning recipient, fixed update budget, `new-only` vs `new+replay`. Метрики **old retention**, **new acquisition**, update latency, RAM, serialized state and cold reload, плюс one-step rollback. E43 historical 0.3226→0.3978 не является свидетельством LoRA AdamW moment migration.

## Обязательная форма результата каждого native run

`id, date, commit sha, branch, domain, profile, recipient, exact prompt/pairIds, pair authorship/admission, initialization checkpoint sha, tokenizer hash, negative controls, metric formula, train/eval file-family split, baseline, treatment, trained-state hashes, metric before/after, wall/CPU/RAM, confidence interval or case-by-case table, ordinary consumer path, restart hash, PASS/FAIL/UNKNOWN, falsifier, follow-up`.

Судью нельзя менять после просмотра outputs. Если исходные данные simulation, так и маркировать — не приписывать их Astra/Qwen без доказательства вызова. Для L не использовать proxy Student вместо настоящего native learner; для U не использовать искусственный обводной демонстрационный путь. Full Windows CI не нужен для разрешения этих малых исследовательских различий.
