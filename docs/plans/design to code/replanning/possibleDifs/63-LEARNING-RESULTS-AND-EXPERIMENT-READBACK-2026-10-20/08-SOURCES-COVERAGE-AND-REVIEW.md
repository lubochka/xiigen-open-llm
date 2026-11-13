# Источники, границы покрытия и независимая проверка

## Закреплённые источники

| Репозиторий / область | Снимок | Как используется |
|---|---|---|
| llm_mvp_core, codex/dpo-implementation | `0b76ab950d82534f5904005dda096545505ae307` | Текущие native методы, protected карточки, уже сохранённые raw20.10 |
| Защищённый subtree | `c63ba2d6253c42bc949b9d9732fa812111318718` | Контроль точного исходного содержания; новых записей туда не делалось |
| xiigen-planning, llm_plan3 | `4b950599a04388941f56b41e634233ebb7dc17c2` | Предшествующие исследования, RP01 pack и родительские RAG |
| possibleDifs исходный subtree | `dedec969669909f6fbef3e068d54b7c7d81ec24f` | Базовый корпус поиска дубликатов до добавления этого пакета |

[Машиночитаемый scope](evidence/snapshots-and-scope.json) содержит полные SHA и границы. Соседние ветки имеют собственные pinned commits и blob receipts в [историческом разделе](03-HISTORICAL-KNN-AND-UNITS.md). Дата исторического запуска не заменена датой нынешнего аудита. Для свежих BPE/transpose raw исходная версия запуска записана как62bcc8f2368840e22d2ea98ef16d9bd526ad9a39;0b76 — снимок хранения и проверки текущего кода. У N-KU raw используются file/assembly hashes; Git commit не выдуман.

## Что охвачено и что не следует из чисел

Из закреплённого possibleDifs получены все **734 Markdown** из1298 Git blobs. [Полный исходный Markdown manifest](evidence/prior-planning-markdown-manifest.json) хранит путь и blob SHA каждой записи. По этому корпусу выполнены целевые поиски новых факторов с чтением релевантных совпадений. Это не последовательное смысловое чтение всех734 документов и не доказательство новизны во всех непроверенных ветках.

Из protected получены1007 текстовых файлов из1207 blobs, включая330 Markdown. Выполнены структурный census всех153 карточек и сопоставление470 P-идентификаторов с125 семействами, затем адресные source/semantic проверки и отдельная проверка конкретных32 строк очереди. Общие текстовые маркеры не являются сертификатом готовности опыта: например, сам заголовок может содержать слово Train, а реальная зависимость устанавливается по caller и изменяемому состоянию. [Census](card-census.json) и [смысловая проверка](10-QUEUE-SEMANTIC-REVIEW.md) намеренно различаются.

Восемь comparison.json в Git являются LFS pointer blobs; их содержимое как JSON-сравнение не прочитано. Для свежих трёх A/B доступны raw arm-A/arm-B и A/B по профилям, поэтому фактические дельты пересчитаны без повтора learner. Сравнение сохранённых state hash strings не выдано за повторное вычисление tensor hashes из весов. Один исторический N-KU build.log декодирован connector; он не является доказательством побайтной тождественности. Исходные Git blob SHA относятся к Git-объектам, а не обещают отсутствие newline-normalization в локальной исследовательской копии.

## Родительские RAG и исходная цель

В [SOURCE-AND-ARCHITECTURE RP01](https://github.com/lubochka/xiigen-planning/blob/4b950599a04388941f56b41e634233ebb7dc17c2/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/SOURCE-AND-ARCHITECTURE.md) разрешены префиксы D/G/F/V. Из G и F прочитаны соответствующие архитектурные и учебные источники, а не только записи RAG-индекса:

- [active-codex-execution-03-ilearningmachine-pattern-notes.md](https://github.com/lubochka/xiigen-planning/blob/4b950599a04388941f56b41e634233ebb7dc17c2/llm%20mvp/dpo%20legal%20build/active-codex-execution-03-ilearningmachine-pattern-notes.md)
- [active-codex-execution-05-dpo-data-training-measurement-guide.md](https://github.com/lubochka/xiigen-planning/blob/4b950599a04388941f56b41e634233ebb7dc17c2/llm%20mvp/dpo%20legal%20build/active-codex-execution-05-dpo-data-training-measurement-guide.md)
- [ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md](https://github.com/lubochka/xiigen-planning/blob/4b950599a04388941f56b41e634233ebb7dc17c2/llm%20mvp/llm%20dynamic%20flow/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md)

Из этих источников взяты предметное предпочтение chosen/rejected, разделение ролей и реального training owner, source-disjoint контроль, разные метрики обучения и runtime, а также различение KNN памяти/доменного отбора/последующего arbiter verdict. Ссылки RAG на201 документа и33 branch-материала использовались как навигация; не заявляется, что все первичные201+33 документа прочитаны целиком. Старые жёсткие thresholds, выбранные модели и product gates не переносятся поверх текущих прямых14 правил пользователя.

Дополнительные исходные материалы из рабочей папки (May skills/session guidance и WHAT-TO-DO/WHAT-NOT-TO-DO) использованы как исторический контекст и словарь. Их старые blocked-статусы не объявлены текущим состоянием октябрьской реализации. Производные assistant-протоколы использованы для поиска ссылок; источник факта о коде или запуске — pinned code/raw, а не уверенность старого ответа.

## Применённые навыки

Использованы актуальные проектные навыки по архитектуре, планированию, проверке плана/кода, промптам, смысловому пути обучения и человеческой коммуникации. Прочитаны относящиеся к задаче разделы; полное последовательное чтение каждого большого SKILL не заявляется.

- [architect-behavior-classifier-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/skills/architect-behavior-classifier-SKILL.md)
- [phase-planning-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/skills/phase-planning-SKILL.md)
- [plan-review-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/skills/plan-review-SKILL.md)
- [plan-self-validation-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/skills/plan-self-validation-SKILL.md)
- [code-examination-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/skills/code-examination-SKILL.md)
- [prompt-patch-authoring-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/skills/prompt-patch-authoring-SKILL.md)
- [output-readability-gate-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/skills/output-readability-gate-SKILL.md)
- [human-communication-GLOSSARY.md](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/skills/human-communication-GLOSSARY.md)
- [experiment-design-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/skills/experiment-design-SKILL.md)
- [learning-path-audit-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/skills/learning-path-audit-SKILL.md)

Их применение дало: exact producer→learner→state→consumer trace, отдельный статус гипотезы и измеренного результата, один A/B-фактор, заранее заданные данные/команды/бюджет, русский краткий отчёт и явные STOP/NEXT. Семантика «домен» взята из human-communication-GLOSSARY: программирование/перевод/документация — предметы; BPE/KNN/LoRA — механизмы; EN→RU — профиль. Старое причинное утверждение skill об E44×10 отмечено как конфликт с уже найденными confounds, а не использовано как доказательство.

## Новизна

- [RoPE novelty record](rope-novelty.json):734 planning Markdown и330 protected Markdown;17/12 файлов с основными совпадениями. Ранее уже были постоянные RoPE frequencies, offset/cache parity, PAD и backward. Новый фактор — однозаписочное повторное использование таблиц **внутри реальных SFT updates** с измерением цены и learned-state parity.
- [AST novelty record](evidence/ast-sibling-novelty-search.json): поиск существующих sibling/ordinal/структурных факторов с чтением близких P26/P27 и аргументного порядка. Новый фактор — устранение повторного перечисления детей одного родителя при построении **настоящей обучаемой памяти**, без добавления весов distance и без изменения codec.

Повторно не предложены уже известные gender aliases, first-hit KNN, tokenizer collisions, `%200` AST codec, reference score reuse, frozen transpose, RMS allocations, lm-head rows и old top3/cold-corpus mismatch. Отдельные подозрительные static tags без доказанного learning consumer не превращены в runtime задания.

## Независимая проверка и выпуск

Автор RoPE и автор AST разделены с reviewer карточек; root отдельно прочитал ключевые текущие методы, первичные исторические E09 tier1/code и raw E09/E26/E43/E07, а также проверил числовую трактовку новых результатов. Проверка документов не исполняла experiment hosts, dotnet build, training, генерацию или продуктовые тесты.

Конкретные замечания и их закрытие фиксируются в [evidence/independent-review.md](evidence/independent-review.md). Технический контроль выпуска ограничен ссылками, полнотой файлов пакета, реестром, отсутствием вымышленных результатов и diff только по разрешённым planning-путям. Он не выдаётся за проверку сборки новых hosts. Перед публикацией головы веток сверяются повторно; изменение planning публикуется обычным commit с expected-head lease без force и без изменения core.

[Документный контроль ссылок, JSON и порядка реестра](evidence/document-validation.json) фиксирует границы технической проверки выпуска.

## Поздняя дельта публикации

После завершения основного аудита появился planning commit `6bc4b6fba8f14870f5dc4966cf5a344754962d66`, прямой потомок4b95:15 новых файлов соседнего пакета, из них13 Markdown, и добавление навигации в README. Эти13 Markdown получены отдельно; выполнены поиск пересечений новых факторов и адресная сверка NG01/P54/очереди/передачи промпта. Исходный manifest734 и его pin не переписаны. Новая база публикации —6bc4, core остаётся0b76. [Reconciliation](12-CONCURRENT-RECONCILIATION.md) и [receipt дельты](evidence/concurrent-update.json) сохраняют эту границу.

## Последующий срез core883496

Во время подготовки публикации другой участник добавил core commit `883496bca18113bf37e0afe683c75b05a0075048`, прямого потомка0b76. Наши действия не меняли core. Полная дельта812 путей ограничена экспериментальными N62-R1 папками и protected документами; продуктовые исходники вне этих папок не изменены. Старый protected subtree c63ba2d остаётся исходным снимком аудита, новый — `41587e4a86b38928e4f4d642a8f1a0f2d35bfcf1`. [Полная metadata-дельта](evidence/core-883496-complete-changed-files.json) подтверждает границу. Изменения новых результатов и правил прочитаны адресно в [13](13-LATEST-N62-R1-READBACK.md) и [14](14-CORE883-PROTOCOL-DELTA.md); новый счётчик334 Markdown и исходные1007 полученных текстовых файлов не выдаются за новый полный смысловой аудит. В protected на883496 осталось153 карточки;330 исходных Markdown +4 добавленных =334, все125 P-семейства и470 подслучаев не превратились в новые готовые hosts.
