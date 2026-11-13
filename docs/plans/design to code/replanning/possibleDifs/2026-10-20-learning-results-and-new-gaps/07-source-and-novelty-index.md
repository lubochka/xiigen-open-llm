# Источники, проверка новизны и пределы аудита

## Зафиксированные версии

| Источник | Immutable commit / tree |
|---|---|
| llm_mvp_core, codex/dpo-implementation | `0b76ab950d82534f5904005dda096545505ae307` |
| xiigen-planning, llm_plan3 до публикации этого пакета | `4b950599a04388941f56b41e634233ebb7dc17c2` |
| Защищённая docs/plans/replanning-possible-diffs | tree `c63ba2d6253c42bc949b9d9732fa812111318718` |
| Исходная possibleDifs | tree `dedec969669909f6fbef3e068d54b7c7d81ec24f` |

Утверждения о коде/старых документах привязаны к этим commit, а не к подвижному имени ветки. Перед публикацией проверяются актуальные heads; конкурентные изменения не перезаписываются. Итоговый commit пакета показывается в ответе пользователю. Ни одному локальному source snapshot не приписывается runtime выполнение.

## Охват, который действительно предъявлен

1. **Весь исходный текстовый каталог possibleDifs:734 Markdown и 1 TXT,735/735 доступных файлов.** Корпус материализован из Git blobs; использован для поиска известных формулировок, устранения повторных «новых» гипотез и чтения пересекающихся доказательств. Это не означает построчное семантическое рецензирование всех 734 документов.
2. **Все 153 карточки и отдельный execution index.** Проверены presence rules STOP/NEXT, модель по умолчанию и успех по полезной метрике; [card-inventory.json](card-inventory.json) содержит каждое имя/ссылку. Семантический разбор механизмов выполнялся по causal paths и адресным подслучаям, перечисленным в 03/04. Счётчик 125 P-семейств / 470 подслучаев означает состав каталога, не 470 выполненных или сертифицированных hosts.
3. **Защищённые Markdown, прочитанные как локальные source snapshots: 330.** Окончательный перечень — [source-manifest.json](source-manifest.json); там отдельно отмечена проверка bytes по Git blob. [Проверены 133 встроенных P-промпта](09-prompt-handoff-audit.md): 124 общих и девять доменных P86. Полезный успех есть в каждом; STOP/NEXT и default-модель обязательны через полную карточку, но теряются при передаче одной короткой цитаты. Это не 133 самодостаточных задания. В 09 даны точные строки, готовая оболочка и исправление конфликтующей фразы «сам выбирает и готовит опыт».
4. **Native source:** DpoTrainer/DpoDataset; LoRA; BPE; Trainer/AdamW/scheduler; Linear/TensorOps/RmsNorm; IncrementalTrainer/session/loader; конкретные kNN, preference projectors, replay-to-unit materializer и translation readers. Полное либо адресное чтение каждого kNN/source указано в 03. Просмотр manifest29 модулей не называется полным code review29 модулей.
5. **Raw:** три свежих reports, два frozen-transpose arms, четыре BPE arms и N-KU arms/дополнительные сохранённые controls; выбранные исторические E22/E25/E26/E43 JSON и их actual driver code.400-step Windows artifact доступен как report-level ссылка, не как независимо прочитанный raw.
6. **Соседние ветки:** точное сравнение тела E25 в language_translate, PrepareToGPL, DesignToCode и текущей ветке; сохранённый E25 raw language_translate. Инвентаризировано 15 веток, полный semantic diff15 веток не заявляется.
7. **RP01/RAG и первичные родительские документы:** прочитаны выбранные Source/Architecture/Recovery/Examination/V6 документы, RAG01/RAG03 и algorithm-conformance; выполнен переход к исходной задаче, parent ARBITER/DECISIONS и plan07. Общее число materialized источников сейчас **1220**, несовпадений bytes с объявленными Git blobs — **0**. Это счётчик источников, не число гипотез.

## Проверка ссылок на родительскую архитектуру

[Исходная задача](https://github.com/lubochka/xiigen-planning/blob/4b950599a04388941f56b41e634233ebb7dc17c2/llm%20mvp/design%20to%20code%20session/original%20task.md#L7-L21) задаёт many→many Units и Flow как отношения между ними. В [parent plan07](https://github.com/lubochka/xiigen-planning/blob/4b950599a04388941f56b41e634233ebb7dc17c2/llm%20mvp/dpo%20legal%20build/active-codex-execution-plan-07-dpo-builder.md#L29-L30) прямо различаются graph-context envelope и **unit-level** training row; раздел 232–318 задаёт 14-field contract и sidecars. Это требование/проект, не доказательство, что все эти типы уже существуют в текущем core.

[ARBITER-KNN-AUTHORITY-AMENDMENT](https://github.com/lubochka/xiigen-planning/blob/4b950599a04388941f56b41e634233ebb7dc17c2/llm%20mvp/llm%20dynamic%20flow/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md) сохраняет отрицательные и warning-примеры в памяти, отделяет retrieval signal от финального acceptance и предупреждает о слишком мелкой нарезке корпуса множеством ключей. Эти требования сопоставляются с реальным consumer: наличие chosen-only Executor не доказывает, что corrected rejected изменит его обучение; нужен negative-consuming learner. Соответствующие ограничения уже разобраны в P52/N60-E1 и 03; новая измеренная потеря здесь не выдумана.

[DECISIONS-LOCKED](https://github.com/lubochka/xiigen-planning/blob/4b950599a04388941f56b41e634233ebb7dc17c2/llm%20mvp/llm%20dynamic%20flow/DECISIONS-LOCKED.md) содержит исторические правила 70/30 replay, stratified split, порядок StackDetector/normalization и late acceptance. Его старое accuracy-only install правило не меняет текущее пользовательское правило полезного измеренного успеха и не разрешает автоматический перенос кандидата в production. Численные historical claims внутри этого файла отдельно не перепроверялись и не выданы за новые результаты.

## Новизна: что добавлено и что сознательно не переоткрывается

| Кандидат | Проверка prior | Вывод |
|---|---|---|
| NG01: int relational pattern подавляет null label для другого nullable receiver | Во всём 734MD+1TXT prior поиск `RelationalPatternSyntax`, literal `if(... is >...)`, non-nullable int relational не дал точного прежнего опыта. Broad N48-01, N53 и core65 already describe arbitrary IsPattern/receiver/scope loss | **Новый узкий контрпример и one-factor recipe внутри известного семейства.** Не новая root-cause family, не измеренное улучшение |
| Косинусный LR после horizon | Уже R09/N1, protected09-additions-44-48, P12; найден реальный supervised caller | Дополнительная source reachability, без нового ID/повторного опыта |
| DoStatement отсутствует в features | Уже свежий N-KU report и prior | Не новая находка; регрессия сохранена |
| MethodKnn Next захватывает nested scope | Уже METHOD-KNN-TARGET-SCOPE-01, queue31 | Не новый эксперимент |
| Top1 generic kNN, duplicate IDs, typed metadata, nested replay | Уже P18/P24/P35/P108 и соответствующие prior документы | Карта механизмов и применимости, без повторного объявления |
| Frozen transpose/BPE no-match/selected-head/RMS/gradB | Уже завершённые либо описанные текущие small cards | Не повторять и не переименовывать; измеренные полезные эффекты записаны кандидатами |
| P54 и KH-COST | Содержательные противоречия на актуальном 0b76, адресный source trace | Документные исправления применимости/learner; не новый runtime |
| KH-COST/KH-EXT/GRADB внутренние номера | Сверка 28 small-card файлов с 32-entry index | Три устаревших перехода; действующие номера явно исправлены в 08 |

Поиск отсутствующего слова не доказывает отсутствие близкой идеи. Поэтому ближайшие N48/N53/core65 указаны прямо в NG01, а отличительный controlled factor сформулирован узко. Новизна не используется как повод запускать дополнительные модели или бесконечный перебор.

## Уровни доказательств

- **Измерено в сохранённом run:** raw подтверждает числа и actual learner. Вывод ограничен тем же domain/panel/budget/source version.
- **Сообщено историческим отчётом:** сохранённые слова/цифры без доступного первичного raw отмечаются как report-level, включая 400-step probe и V309 leads.
- **Подтверждено исходником:** существует конкретный branch/caller/state/reader. Влияние на выбранную метрику остаётся гипотезой до сравнения.
- **Рецепт эксперимента:** заранее определены learner, данные, один фактор, budget, команды и report; host не называется собранным до реального build.
- **Конкретный недостающий вход:** P54 требует named learned state/contract. Это не blanket запрет learning, не разрешение придумать checkpoint и не runtime failure.

Пакет не заявляет единственную доказанную причину деградации RP01, полную семантическую готовность 470 подслучаев или полный обзор каждой ветки. Он сохраняет существующие свидетельства, исправляет обнаруженные противоречия, даёт адресную карту различий и готовый новый малый рецепт без новых запусков.

Снятие ошибочного ограничения P54 не снимает автоматически все остальные пометки `OUT_OF_LEARNING_SCOPE`. У каждого такого подслучая требуется собственный доказанный writer/state/consumer. Например, sampling фиксированных logits, пересчёт отчёта либо cold/export без предъявленного обученного состояния не становятся проверкой обучения от добавления формального вызова Train. Связанный с реальным продолжением уроков restore описан отдельно в 04; недостающий learned snapshot или host обозначен конкретно.
