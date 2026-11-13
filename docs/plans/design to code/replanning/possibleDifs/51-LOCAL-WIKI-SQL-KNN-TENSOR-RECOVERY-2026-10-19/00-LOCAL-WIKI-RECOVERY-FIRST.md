# Глава 0. Сначала восстановить алгоритм и поведение local_wiki_planning

19 октября 2026. [Оглавление пакета](README.md) · [Короткие постановки опытов](05-SMALL-DOMAIN-EXPERIMENTS.md).

**Рабочая ветка local_wiki_planning — исходный эталон этого исследования.** Приоритет имеют изменения, которые возвращают её конкретный учебный сигнал и обычный путь ответа. Новые дефекты, уже существовавшие в этой ветке, вынесены ниже и не названы причиной регрессии.

Донор W: **219d7ba89dc3ef43e03e17f1461ad96154d3dd7a**. Проверенная реализация C: **4945b8e85c17b5f150eb5b840d16bbdc4b506242**. Более свежий core **649b9343cfda5b81cc54396a3840ad5db5e0b179** изменил только исследовательские документы; программные источники остались прежними. Все факты ниже имеют первичные или адресные прежние доказательства. Новых обучающих запусков здесь нет.

## Что именно надо возвращать

Слово «обучение» в архиве относится к нескольким получателям: neural policy, memory/KNN, обучаемому маршрутизатору и композиции Unit/Flow. Высокий результат одного из них не доказывает, что тот же урок выучила LoRA нынешнего RP01. Поэтому восстановление проверяется по цепочке **исходная задача → реальная модельно-видимая пара → изменяемое состояние → сохранение/загрузка → фактически вызванный Unit → его собственный ответ**. [Точная таблица neural получателей](01-LEARNING-INTERNALS-BEFORE-AFTER.md), [прежний разбор Wiki scorer/holders](../49-LOCAL-WIKI-DONOR-CAUSAL-RECOVERY-2026-10-19/README.md).

| Очередь | Восстановительная проверка | Доказательство отличия | Короткое вмешательство и решающий результат |
|---|---|---|---|
| **R0 — ноль обновлений** | Одинаковы ли donor state и реальный consumer? | Исторические policy/reference и Unit manifests имеют разные роли; нынешний default continuation reset уже описан в N49. Состояние из receipt ещё не доказано загруженным бинарником | Зафиксировать конкретный архив, совместимый tokenizer/schema и вызываемый Unit. Старый и новый процесс с одним состоянием отвечают на те же 6–8 source-disjoint запросов. Если расходятся уже здесь, локализовать загрузку/маршрут до новых уроков |
| **R1 — P0, известная потеря содержания** | Вернуть исходные prompt/chosen/rejected в CodeTrace/Router metric path | N49 проследил замену исходной тройки маркерами типа ошибки; source-equivalent проекция 50 разных Router prompts сводилась к 4 тройкам. Это прежняя находка, не новая | На одном состоянии сравнить текущую и сохраняющую содержание проекцию. До optimizer проверить actual IDs/masks; затем одинаковый небольшой update budget и отдельные обычные ответы. Не сравнивать разные архитектуры |
| **R2 — P0 для Wiki, новое SQL-различие AC-SQL01** | Вернуть проверяемое read/write представление позднего SQL-донора | В W есть условные WRITES_NONE_CONFIRMED / READ_ONLY_PERSISTENCE_SCOPE_PRESENT и особые признаки result/contract; в C условный блок удалён. Count-признаки в C сохранены | Сначала A00↔A10: одни SQL source families, общий parent/reference, только условное представление TRAIN chosen; EXAM побайтно одинаков. Затем отдельно curriculum 10↔35 table-effects строк на 50. Успех — правильные read/write различия на закрытых процедурах, а не наличие слова GOOD |
| **R3 — P0 для neural RP01, известное различие** | Проверить место адаптации: выходная проекция против только q/v-LoRA | Wiki metric learner по обычному graph path обновлял lm_head; RP01 обучает 16 q/v-LoRA tensors, head заморожена | Один совместимый B0/BPE/reference; текущий правильный DPO. Менять только trainable set/место адаптации. Показать число параметров, величину обновления и собственные ответы C# или EN→RU. Старые ошибки знака/autograd не возвращать |
| **R4 — P0, известный путь исполнения Unit/Flow** | Проверить, что обычный запрос потребляет обученную единицу и её state | Прежние N49/G-W описали отдельный trainable router, статический обычный router, gate semanticQualityClaimed=false и потерянный ClientServerBusinessFlow | При нуле новых уроков сравнить identity реально вызванного Unit, вход/выход каждого ребра и конец ответа. Восстановить совместимое состояние и корректный вызов; законный quality gate удовлетворить доказательством, а не отключать |
| **R5 — P1, новое адресное уточнение AC-EV02** | Дать neural AnswerComposer доступ к принятым evidence и версии | В W и C rich pair содержит evidence/request, ToDpoMetricPair оставляет в prompt только instruction+вопрос. Structural evaluator видит evidence. Это inherited seam, не новая регрессия | Один вопрос, две допустимые версии SQL, противоположные read/write факты. A — нынешний prompt; B — вопрос+минимальные принятые факты. Общий достаточный контекст в обоих плечах; до train доказать видимость фактов в actual prefix IDs и снять обе оценки при нуле updates |

Прямые владельцы R0/R1/R4: [потеря тройки](../49-LOCAL-WIKI-DONOR-DELTA-2026-10-19/01-DPO-PAIR-LOSS-AND-HISTORICAL-PROJECTION.md), [continuation policy/reference](../49-LOCAL-WIKI-DONOR-DELTA-2026-10-19/06-NEW-CONTINUATION-RESET-POLICY-REFERENCE.md), [golden Unit/Flow recovery](../49-LOCAL-WIKI-GOLDEN-RECOVERY-2026-10-19/README.md). R2/R5: [новый SQL-разбор с кодом и архивами](02-NEW-SQL-UNIT-GAPS.md). R3: [формы матриц, objective и состояние optimizer](01-LEARNING-INTERNALS-BEFORE-AFTER.md).

R0 не означает необходимость долго воспроизводить весь старый продукт до любого полезного опыта. Если точного старого бинарника или его совместимой среды нет, можно выполнить ограниченный matched A/B на нынешнем получателе и явно назвать это переносом механизма, а не точным повтором donor run. Не подменять отсутствующее состояние случайной моделью под прежним именем.

## Положительные результаты донора сохранены, но у каждого свой смысл

| Источник в W | Сохранённый результат | Что результат подтверждает | Чего по нему не установлено |
|---|---|---|---|
| CodeTrace run064 | Structural primary 1.00, secondary .94; DPO heldout .92 | Положительный archived gate конкретного CodeTrace обучения/оценивания | Свободное порождение current RP01 и независимый вклад одной исправленной функции |
| QueryPlanning run134 | Structural primary/secondary 1.00; DPO heldout 1.00 | Положительный результат указанного planning scorer | Универсальное planning-умение: прежний аудит нашёл всего 8 уникальных prompts на split и постоянные answer targets |
| **AnswerComposer SQL110/111/112** | **47/50 = .94 heldout DPO**, 9/10 table-effects; loop summary passed | Поздние успешные preference receipts реального SQL-донора | .94 не равно .94 обычных grounded ответов;111→112 — continuation на том же corpus, не независимая реплика |
| AnswerComposer SQL106 | 37/50 = .74; loop summary failed, publication skipped | Отрицательный этап с focused curriculum и противоречивыми read/write signals | Структурная надпись GATE_PASSED не превращает этот этап в полный успех;70% focus нельзя автоматически считать лучшим |
| AnswerComposer SQL113 | 44/50 = .88; table-effects 7/10 | Более поздний результат после global expansion chosen, сохранившийся как passed | Это не чистый эффект «слишком длинного сигнала»: policy продолжалась от112, нет matched контроля |
| BaseWiki session01/batch003 | Final DPO accuracy1.00, margin .021053 | Завершение своего DPO runner gate | Сам отчёт требует отдельного heldout Q/A перед BASE_MODEL_ACCEPTED |

Источники: [CodeTrace064][ct064], [QueryPlanning134][qp134], [BaseWiki003][base003]; полный SQL lineage, raw pairs и раздельные gates — [AC-SQL01](02-NEW-SQL-UNIT-GAPS.md). Кардинальность QueryPlanning и предупреждения о quality claim — [прежний donor receipts audit](../49-LOCAL-WIKI-DONOR-CAUSAL-RECOVERY-2026-10-19/01-DONOR-RECEIPTS-AND-PAIR-CARDINALITY.md).

**Почему SQL выделен первым среди новых находок.** Он содержит одновременно source-confirmed W→C удаление и поздние положительные архивы. При этом архив прямо мешает удобному, но неверному объяснению: .94 уже было в110, до read-only correction111. Хорошая постановка обязана раздельно проверить условное представление, состав отрицательных примеров и стартовое состояние. Число маркеров, название batch и прежний PASS не заменяют этот опыт.

## Что переносится из «неудачной матрицы» в функцию KNN

Прежние исследования E07/E09/E25/E26/E43 уже установили, что неудача одного представления или места в вычислении не отменяет полезность признака в другом consumer. Для структурного C# поиска порядок AST, роль операнда и отношения parent/child проверяются **в функции отбора соседей**. Для перевода важны смысл, отрицание, направление и порядок слов; числовой Roslyn kind не является универсальным языковым вектором. Для order-insensitive lookup добавление линейного порядка может оказаться лишним. [Сохранённая историческая линия](../43-PREPARETOGPL-RESTORATION-AND-NEURAL-DIFFS-2026-10-18/03-HISTORICAL-LINEAGE.md), [соседний language_translate recovery](../2026-10-19-language-restore/README.md).

Новые NK-01/NK-02/NK-03 проверяют другой конкретный адаптер, AstRelevanceRanker/skill context. Нельзя назвать его полным воспроизведением StructuralKnnPredictor E09 только по комментарию в коде.

| Условная очередь | Когда ставить опыт | Минимальный опыт | Почему не первая причина RP01 |
|---|---|---|---|
| NK-01, repeated Index | Установлен actual caller с повторной записью ChunkId | Те же 2 документа, несколько Index того же ID; df и правильный top1 должны оставаться стабильны | Свойство унаследовано от W; комментарий класса предусматривает once-per-file initialization |
| NK-02, structural relation | В обычном C# запросе действительно используется AstRelevanceRanker | if(x==null), if(x!=null), return null; trace query/index keys, затем retrieval@1 | Возможна компенсация обычным kind; native Roslyn и product wiring пока не проверены |
| NK-03, skill eligibility | Реально установлен KnnSkillFilter с общим индексом | Global topK→intersection против eligible scope→topK на одинаковых scores | Default ContextWindowManager использует PassThrough; общий принцип уже prior P104.1 |
| N-SHAPE-01/02 | Найден вызывающий обучение код с удалённой Mean осью или mixed batch/head broadcast | Микротензоры и независимые производные, затем только relevant caller | В проверенном RP01 Mean keepDims=true, MatMul right operand2D либо leading shapes совпадают |

Доказательства и выполненные proxy: [KNN](03-NEW-KNN-CONSUMER-GAPS.md), [тензорное приложение P2](04-TENSOR-CONDITIONAL-GAPS.md). Эти условия входят в очередь только после подтверждения использования; общий откат файлов W их не исправляет.

## Как признать восстановление состоявшимся

Для выбранного получателя надо получить **два разных вида доказательства**. Сначала — причинный след: конкретный факт дошёл до входа, обновилось нужное состояние, оно восстановилось и было прочитано обычным вызовом. Затем — предметный результат: собственный ответ на новом семействе правильно сохраняет нужный закон и не ломает старый.

Для SQL это точное различение читаемых и изменяемых таблиц, transaction/result contract, отсутствие выдуманных evidence и корректное воздержание при неизвестных facts. Для C# — null/ветвь/роль/направление вызова. Для EN→RU — сохранённые участники, отрицание и порядок отношений. Отдельно записываются coverage, критические ошибки, удержание прежнего материала, cold reload и вычислительная цена. Факт изменения весов или margin нужен для диагностики, но не заменяет предметный ответ. [Полный небольшой протокол и роли](05-SMALL-DOMAIN-EXPERIMENTS.md).

[ct064]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-code-trace-builder-high-contrast-terminal-chain-promotion-confirmation-064/project-wiki-training-loop-result.json
[qp134]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-134/project-wiki-training-loop-result.json
[base003]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/base-wiki-runs/session-01-batch-003/base-wiki-dpo-runner-report.json
