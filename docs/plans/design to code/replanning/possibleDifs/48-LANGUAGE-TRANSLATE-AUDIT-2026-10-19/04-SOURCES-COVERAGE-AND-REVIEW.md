# 04. Покрытие, происхождение доказательств и review

19 октября 2026. Этот файл отвечает на два вопроса: какие части запроса действительно исследованы и насколько сильны выводы. Начало списка действий — [восстановление language_translate](../00-LANGUAGE-TRANSLATE-RECOVERY-FIRST-2026-10-19.md).

## 1. Зафиксированные версии и соседние ветки

| Источник | Snapshot | Как использован |
|---|---|---|
| core language_translate | f6032aff81f1f83de7c09cf9b9efe6b058dd12b9 | Прямое сравнение кода; root/subtree inventory; E07/E09/E25/E26/E43 первичные JSON |
| core codex/dpo-implementation | a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6 | Прямые consumer/state/source checks; защищённые исследования только чтение; RP01 scientific result; P125/P25/ingress reports |
| planning llm_plan3, начало аудита | 9cea3930067f216ccb812ecc5e412aec673f5058 | RP01 pack, RAG, parent documents, основной previous-diffs corpus |
| planning llm_plan3, первая сверка чужих добавлений | 3215267c0ee2c2beafebec950b45b7fee326daf7 | 15 новых файлов других агентов и новая версия README прочитаны/просмотрены для согласования; чужие материалы сохраняются |
| planning llm_plan3, повторная сверка после конкурентного commit | e5310a41ab5c79dce2f913563b7557cc27c6f272 | Ещё 11 файлов пакета 47 и свежий README; их новые механизмы сохранены под исходным авторством |

Core branch HEAD при поздней проверке остался тем же. Будущий commit этого пакета создаётся поверх актуального planning parent, без изменения pinned исходников анализа. Основные source links используют commit SHA, а не движущееся имя ветки.

Инвентаризированы соседние heads: PrepareToGPL 31346d8928be4469b01aaf313780e8f950ff91e2; DesignToCode 8f20ceb8a9bfbadab2ad309226c47dd76e714548; DesignToCode-clean e346b88a3cc1fcce48aefa626bfc7b00509eefd6; external_domains 3036a01acec938f0b83fa4ffbda26202cc35d639; orchestrator-extensions b293045bd89b1eb96d3e32fedd341f1a4170adbc; main f84c67418de8e58a07dc9dc640ee6632a9411a5b. Это **не полный audit каждой ветки**. Положительные соседние линии проверялись адресно по доступным материалам/истории; полная аттестация каждого старого бинарника не выполнена. Имена и SHA сохранены в [inventory](06-CORPUS-INVENTORY.json).

В language_translate отсутствует 27.TranslationDomain. Старые R1/R2 routing contracts находятся в других проектах, и наличие таких flows не превращает современные TranslationMachine state в артефакты того baseline. У современных positive memory results собственная lineage. [Baseline tree][langtree], [поздний routing diff другого агента][laterouting], [P125][memory125].

## 2. Объём документного корпуса

| Раздел | Извлечённые текстовые файлы |
|---|---:|
| planning/possibleDifs, верхний уровень исходного snapshot | 391 |
| core/docs/plans/replanning-possible-diffs, верхний уровень | 92 |
| Два вложенных planning recovery-пакета | 14 Markdown |
| core/experimets, вложенные карточки/отчёты | 168 Markdown |
| Основной корпус | **665** |
| Новые добавления в planning, первая сверка | **15** |
| Новый соседний пакет 47, повторная сверка | **11** |
| Различные файлы в объединённом перечне | **691** |

README дополнительно прочитан в новой редакции и не считается вторым отдельным файлом. 34 файла RP01 pack, project skills, RAG/parent plans, исходники и отдельно полученные первичные historical JSON **не входят** в этот счётчик 691. Вложенные бинарные результаты и все model payloads не объявляются прочитанными.

Для каждого включённого файла в [06-CORPUS-INVENTORY.json](06-CORPUS-INVENTORY.json) указаны repository, commit, relative path и Git blob SHA. Корпус использован для полнотекстового тематического поиска и адресного ручного чтения. Отрицательный поиск ограничен этими файлами; новизна не означает отсутствия похожей мысли во всём GitHub или неопубликованной работе.

## 3. Как применены запрошенные проектные skills

| Запрошенная роль | Применённые skills | Что изменилось в результате |
|---|---|---|
| Архитектор | [trainable architecture][skillarch], [human-readable audit][skillaudit] | Разделены training owner, stored state и actual consumer; найден путь от конкретного источника урока к ответу |
| Планирование | [phase-planning][skillplan], [experiment-design][skillexperiment] | Приоритет успешных доноров; сначала один дешёвый discriminating experiment, потом необходимая интеграция |
| Проверка планов | [plan-review][skillplanreview] | Один фактор, достижимость, independent gold, отрицательные контроли, stop/falsifier |
| Проверка кода | [generated-code-review][skillcode], [code-examination][skillcodeexam] | Exact source comparison, реальные имена методов и signatures, учёт guards и counterexamples |
| Prompt engineering | [prompt-patch-authoring][skillprompt], [planning-dpo-authoring][skilldpo] | Конкретная правильная/неправильная сторона по одному закону; отдельный review точных пар; отсутствуют подсказки EXAM внутри TRAIN |
| Человеческая коммуникация | [glossary][skillhuman], [readability gate][skillreadability] | Русское объяснение механизма; domain/profile/Unit/LoRA не смешиваются; local score, обучение памяти и free generation названы раздельно |

Применялись релевантные разделы больших project skills, а не утверждение о построчном чтении всех исторических workflow-инструкций. Старые указания исследовательских файлов о другой папке записи не меняют текущий прямой заказ пользователя: **публикация только в planning/possibleDifs; защищённый core каталог остаётся read-only**.

## 4. RAG и ссылки на родительские документы

Прочитаны указатели WorkLeft-RAG-01 dpo-legal-build, WorkLeft-RAG-02 design-to-code, WorkLeft-RAG-03 merge-and-branches; по ним подняты связанные parent plans. Главные требования, использованные в сопоставлении:

| Документ | Существенный для аудита контракт |
|---|---|
| [DPO builder plan 07][ragbuilder] | Unit-level envelope, NodeOrEdgeId, общий InputContext, ordered pattern IDs и происхождение строки; разложение победителя относительно losers |
| [Training lifecycle plan 10][raglife] | Материал → владелец обучения → состояние → продолжение/публикация → потребитель |
| [Measurement guide][ragmetrics] | Pair preference, routing, branch answer, retained behavior и стоимость — разные показатели |
| [Four-level translation plan][ragtranslation] | Синтаксис/negation, morphology/register, robustness; EN/RU/HE направления — profiles; план сам по себе не execution receipt |
| [Dynamic flow learning-set plan][ragflow] | Примеры для узлов/flow и смысловой адрес обучения; прохождение графа не доказывает качество каждого Unit |
| [RP01 recovery/code audit][rpplan] | Начальная компетенция Student, tiny overfit, LoRA sufficiency, качество пар и фактический learning curve вместо обязательного нового Main500 |

Историческая teacher-vs-teacher формулировка одного DPO плана не применяется вместо текущего пользовательского контракта. Новые пары должны создаваться **обеими сторонами внешней открытой китайской модели**; CODEX с astra 6 ultracode осуществляет человеческий отбор и отдельное независимое quality review. Нельзя переименовать фактическое происхождение старых simulation rows. В этом аудите новые teacher/evaluator calls не выполнялись.

## 5. Покрытие всех запрошенных осей

| Требование | Где разобрано в пакете | Состояние / прежние исследования |
|---|---|---|
| Восстановить именно language_translate | [Глава 0](../00-LANGUAGE-TRANSLATE-RECOVERY-FIRST-2026-10-19.md), [03 §§2–6](03-SMALL-DOMAIN-EXPERIMENTS.md) | Exact SHA + положительные E25/E26/E09/E43; первые experiments |
| Качество DPO chosen/rejected | [01 §§6–7](01-MECHANISMS-AND-EVIDENCE.md), [03 §10](03-SMALL-DOMAIN-EXPERIMENTS.md) | Semantic families, pair identity/masks, роли авторов; [existing review][prevquality] |
| LoRA forward/backward/targets | [01 §7](01-MECHANISMS-AND-EVIDENCE.md) | Current repairs сохранить; sufficiency остаётся гипотезой |
| Vector/tokenization/parameters | [01 §8](01-MECHANISMS-AND-EVIDENCE.md), N48-03 | BPE vs embedding vs semantic vector vs AST/POS; [existing identity gaps][prevbpe] |
| KNN functions/структура/порядок | [01 §§1–4](01-MECHANISMS-AND-EVIDENCE.md), R25/R26/R09, N48-01 | Domain-specific features; same formula, changed corpus/consumer |
| Скрытые фиксированные алгоритмы | [01 §10](01-MECHANISMS-AND-EVIDENCE.md) | Labels, fixed classifier, decomposer/quality bridge, confidence/fallback |
| Model/optimizer/Unit restore | [01 §9](01-MECHANISMS-AND-EVIDENCE.md), R-RESTORE, N48-02 | Полное состояние и consumer parity; positive controls сохранены |
| DPO → subunits | [01 §6](01-MECHANISMS-AND-EVIDENCE.md) | Pair→key/target/mapping, chosen-only state, contexts, role parser |
| Unit/Flow implementation | [01 §§6,9–10](01-MECHANISMS-AND-EVIDENCE.md) | Actual registry/node/order/consumer; RAG envelope против actual projection |
| Новые гипотезы сверх других агентов | [02](02-NEW-HYPOTHESES.md) | Две новые конкретные цепочки + один upstream refinement; bounded novelty |
| Быстрая причинная проверка по доменам | [03](03-SMALL-DOMAIN-EXPERIMENTS.md), [05](05-CODEX-EXPERIMENT-WORK-ORDER.md) | Малые панели, один фактор, реальный output, falsifier; NOT_RUN |

Дополнительные старые направления не исчезли: corpus leakage/семейства [69][prevcorpus], attention/positions [47][prevattention], decode/answer boundaries [31][prevdecode], history/trace recipient [76][previnput], feedback [75][prevfeedback], HNSW partial state [51][prevadmission], arbitration/decomposition [34][prevdecomp]. Это карта существующих возможных причин, а не повторное объявление всех новыми дефектами.

## 6. Что добавили другие агенты, пока шёл аудит

В ходе двух поздних сверок в planning появились ещё 26 файлов. Первый подготовленный commit совпал по времени с публикацией чужого пакета 47; обновление ref не было принято. Документы перенесены поверх свежего parent, README сохранён. Чтобы избежать конфликта номеров, итоговый номер этого пакета — **48**.

- [N44 E26][latee26]: отсутствующий контекст может получать нулевую дистанцию к specific entry; Any-overlap теряет порядок/кратность POS. Эти гипотезы и их proxy принадлежат соседнему пакету; наш R26 учитывает empty/partial context и не объявляет эти открытия своими.
- [N44 routing contracts][laterouting]: old raw-text R1/R2 против нового запроса с обязательными SourceLanguage/TopicDomain; возможная потеря RejectedAdapters/evidence/Flow IDs. Actual upstream/caller для причинного эффекта остаётся условием. Эти элементы дополнены к карте, не переписаны.
- [N45 ProductHeldOut][lateeval]: ProductMatch может принять неправильный по смыслу перевод при совпавших markers, а UTF-16 char predicate пропускает supplementary emoji из части метрики. Это другой evaluator, не RP01 scientific examiner; native/quality outcome не заявлен.
- [N46 micro-experiments][lateexperiments] уже ставит language_translate recovery первым. Наши R25/R26/R09/R43 — совместимая детализация вызовов/контролей, а не претензия на новизну этих доноров.

- [Пакет 47][late47] добавил шесть других механизмов: конфликт generic type с безымянными AST features E14, next-method pairs через границы классов, накопление веса повторным Compile, переиспользование каталога export, изменение morphology ranking при росте словаря и повторный рост cosine LR за горизонтом. Это соседние гипотезы LT-N01–LT-N06; наша метка E14 deferred guard касается другого producer condition, а AdamW Inf-v — другого численного состояния. Их source-formula scripts не запускались этим аудитом.

Точная новая цепочка E14 deferred guard, сохранённого AdamW Inf-v и смены recipe одного triplet current→replay в этих поздних документах не обнаружена. Общие темы context/scope/density/restore остаются связанными. Мы не запускали чужие proxy/runners и не изменяли их результаты.

## 7. Независимое review и принятые исправления

После первичных тематических исследований отдельный последовательный reviewer проверил source chains N48-01/N48-03 и итоговые документы 00–03. Главный составитель сверил новые numerical/source assertions и результаты, затем внёс следующие содержательные поправки:

| Найденная неточность или необходимый контроль | Итог в документах |
|---|---|
| Неверное имя current corpus builder из старых заметок | Используется BuildKnnTrainingCorpus(replayManifests, registrationPlans).RuntimeExamples; flat rows подтверждены L3467–3508 |
| Неверное имя API структурного прогноза | Указан public Compile → private PredictStructuralNextTokens → PredictedNextTokens |
| Сохранённые structural fields ошибочно можно прочесть как used features | SiblingCount/FileId явно отделены от полей DistanceTo |
| Same text triplet смешивался с same admitted pair | Вектор входит в digest; новый recipe требует штатного reissue до admission |
| Density автоматически приписывалась loss/stop | Подтверждён legacy caller; immediate loss scale только exact; active-count falsifier обязателен |
| Generic BuildChosen смешивался с предметным ответом | Для него измеряется subphase retrieval/retention; объяснение закона требует actual answer consumer |
| Portable roundtrip после всех Step не проверяет continuation | Step1 → restore → Step2–4; тот же восстановленный weight/m/v/step |
| Uncalled guard предлагал слишком широкое исключение nested syntax | Добавлен реально вызванный Guard; label-producer и feature changes разделены |
| Языковой пример мог менять субъекта | Для одного source сохраняется субъект; корректный парафраз с первым For всё равно показывает positional risk |
| Архивный cold-run можно спутать с ingress | P25 явно относится к P125 B_after; quality 19/24 и parity 24/24 разделены |

Независимо сверены числа E25/E26/E09/E43, RP01 scientific-result, четыре header-only code comparisons и actual dispatcher signatures. Это source/document review, без нового native исполнения. Содержательные замечания внесены до публикации. Проверена целостность восьми deliverables: относительные ссылки, reference definitions, JSON и 691 уникальных corpus paths. Для 139 GitHub source references проверены pinned paths по сохранённым tree/blob metadata и адресным чтениям; это проверка документов, не новый product test.

## 8. Какого доказательства здесь нет

- Нет нового native C# запуска, обучения, inference или performance benchmark. В текущем окружении .NET runtime не был доступен; установка не входила в исследовательскую задачу.
- Нет доказательства, что N48-01/N48-02/N48-03 действовали в actual RP01 run. Для них нужны указанные trigger/consumer условия.
- Нет полной привязки source→бинарник для каждого старого E-result. JSON на фиксированном commit подтверждает архив результата, не воспроизводит процесс.
- Нет новых слепых оценок качества DPO, сгенерированных запрошенным Chinese teacher. Роли исполнителя сформулированы для следующих опытов.
- Нет доказанной потери WordMap на двух изученных phase38 checkpoints: все word_map/surface_word_map там пусты. [Before][phase38before], [continued state][phase38state].
- Нет утверждения, что все старые эксперименты ложны или что весь current KNN/restore сломан. Положительные результаты памяти, context selector и сохранения состояния остаются положительными в своей области.

Открытый вопрос, который продолжает иметь значение: какой именно прежний consumer/user workflow давал наблюдавшийся пользователем хороший результат. Найденные exact language_translate donors и реальные современные positives дают проверяемые начальные точки; они не подменяют ещё не установленный сквозной historical run.

[langtree]: https://github.com/lubochka/llm_mvp_core/tree/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9
[laterouting]: https://github.com/lubochka/xiigen-planning/blob/3215267c0ee2c2beafebec950b45b7fee326daf7/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/44-NEW-LANGUAGE-ROUTING-CONTRACT-DIFF-2026-10-19.md
[memory125]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md
[skillarch]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/skills/trainable-architecture-decision-SKILL.md
[skillaudit]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/skills/human-readable-architecture-audit-SKILL.md
[skillplan]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/skills/phase-planning-SKILL.md
[skillexperiment]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/skills/experiment-design-SKILL.md
[skillplanreview]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/skills/plan-review-SKILL.md
[skillcode]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/skills/generated-code-review-SKILL.md
[skillcodeexam]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/skills/code-examination-SKILL.md
[skillprompt]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/skills/prompt-patch-authoring-SKILL.md
[skilldpo]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/skills/planning-dpo-authoring-SKILL.md
[skillhuman]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/skills/human-communication-GLOSSARY.md
[skillreadability]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/skills/output-readability-gate-SKILL.md
[ragbuilder]: https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/dpo%20legal%20build/active-codex-execution-plan-07-dpo-builder.md
[raglife]: https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/dpo%20legal%20build/active-codex-execution-plan-10-training-lifecycle.md
[ragmetrics]: https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/dpo%20legal%20build/claude-05-dpo-data-training-measurement-guide.md
[ragtranslation]: https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/language%20translate%20learning/TRANSLATION-FOUR-LEVEL-LLM-TRAINING-PLAN-v1.md
[ragflow]: https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/plan%20the%20learning%20set/DYNAMIC-FLOW-FIRST-FILE-LEARNING-SET-PLAN-v1.md
[rpplan]: https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md
[prevquality]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/14-data-quality-and-evaluation-alignment.md
[prevbpe]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/12-bpe-retrain-identity-and-consumer-causal-gap.md
[prevcorpus]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/69-pretraining-data-boundaries-and-consumer-scope.md
[prevattention]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/47-current-attention-backprop-and-position-contract.md
[prevdecode]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/31-answer-boundary-and-decoding-scope.md
[previnput]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/76-wiki-trace-donor-input-loss-and-checkpoint-identity.md
[prevfeedback]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/75-translation-feedback-learning-consumer.md
[prevadmission]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/51-vector-index-admission-and-partial-state.md
[prevdecomp]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/34-decomposition-quality-and-execution-verdicts.md
[latee26]: https://github.com/lubochka/xiigen-planning/blob/3215267c0ee2c2beafebec950b45b7fee326daf7/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/44-E26-UNKNOWN-CONTEXT-AND-ORDER-2026-10-19.md
[lateeval]: https://github.com/lubochka/xiigen-planning/blob/3215267c0ee2c2beafebec950b45b7fee326daf7/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/45-NEW-PRODUCT-HELDOUT-SEMANTIC-GATE-2026-10-19.md
[lateexperiments]: https://github.com/lubochka/xiigen-planning/blob/3215267c0ee2c2beafebec950b45b7fee326daf7/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/46-MICRO-LANGUAGE-TRANSLATE-RESTORATION-AND-EVAL-2026-10-19.md
[phase38before]: https://github.com/lubochka/llm_mvp_core/blob/5fa0f242b9f24c2045f4ca07742bfe6baaf48bb5/models/translation/translation-v1/translation-v1-checkpoint.json
[phase38state]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/artifacts/phase38/translation-v1-continued-probe/translation-v1-continued-checkpoint.json

[late47]: https://github.com/lubochka/xiigen-planning/blob/e5310a41ab5c79dce2f913563b7557cc27c6f272/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/47-LANGUAGE-TRANSLATE-INTERNALS-AND-NEW-HYPOTHESES-2026-10-19/README.md
