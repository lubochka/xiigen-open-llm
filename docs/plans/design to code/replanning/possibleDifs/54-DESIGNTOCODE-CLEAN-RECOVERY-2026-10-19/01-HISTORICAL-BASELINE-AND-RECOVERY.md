# DesignToCode-clean: подтверждённые положительные следы и порядок восстановления

Дата source readback: 2026-10-19. Этот документ — исследование источников и дизайн следующих проверок; новых native-сборок, обучения, inference и измерения качества в нём нет.

## 1. Что следует восстановить в первую очередь

Пользователь указывает **DesignToCode-clean** как ветку с хорошо работавшим обучением. Основной зафиксированный эталон для этого исследования — **`e346b88a3cc1fcce48aefa626bfc7b00509eefd6`**. Текущее сравнение — **`358af351e22a92a3f7938d1f9221d5aeb7ddb053`**. Положительные исторические эксперименты следует сначала связать с конкретной исполнявшейся функцией, обученным состоянием и обычным потребителем. Название ветки и наличие отчёта не заменяют эту связь.

Первыми поставлены проверки восстановления полезного поведения из указанного пользователем эталона. Это приоритет повторной проверки, **не утверждение, что все перечисленные функции отсутствуют в текущей ветке**. Если код совпадает, различие нужно искать в составе обучающей памяти, параметрах, восстановлении состояния или вызове функции, а не объявлять одинаковый код регрессией. Конкретные новые разрывы checkpoint и контекста получателя приведены в [02-NEW-CHECKPOINT-AND-CONTEXT-GAPS.md](02-NEW-CHECKPOINT-AND-CONTEXT-GAPS.md); корректное восстановление состояния проверяется до измерения прироста от нового обучения.

| Порядок | Восстанавливаемое поведение | Положительный след в clean | Первая короткая проверка | Граница переноса |
|---|---|---|---|---|
| **R0** | Exact state и действительный caller | Нужен конкретный сохранённый артефакт и подтверждённый consumer, а не одна итоговая `Accuracy` | Закрепить checkpoint/corpus/tokenizer hashes, реального получателя и обычный запрос до изменения алгоритма | Техническая исходная точка; отсутствие состояния требует сначала адресовать блокирующую зависимость |
| **R1** | Структурные признаки как запрос и расстояние KNN | E09: `top1_accuracy .2413 → .3507` | Один frozen holdout для frequency baseline и structural KNN, одинаковые token IDs; затем отключение только структурных признаков | C# AST и домены, где смысл задачи действительно зависит от проверенного дерева |
| **R2** | Использование обученной памяти POS-шаблонов в KNN | E25: `top1_accuracy .3438 → .875`, 32 validation-примера | Один и тот же набор входов через rules, обученный `SentencePatternClassifier(k:3)`, затем save/load и штатного потребителя | POS-классификация в задачах перевода; не доказательство качества полного предложения |
| **R3** | Сохранение полезной старой памяти при поступлении новых знаний | E43: `retention_accuracy .3226 → .3978` | Одна старая компетенция, один новый набор, фиксированный old/new holdout, одинаковый бюджет, явно разрешённая политика памяти | Исторический опыт — таблица частот переходов; точный replay DPO-пар не разрешается его результатом |
| **R4** | Совместимое восстановление checkpoint | Новая точная цепочка разбирается в [02](02-NEW-CHECKPOINT-AND-CONTEXT-GAPS.md) | Отдельное сравнение restore при сохранении обязательной provenance | Не разрешение ослабить admission или скопировать runtime authority |
| **R5** | Одинаковые значимые условия обучения и запроса | Новая цепочка контекста разбирается в [02](02-NEW-CHECKPOINT-AND-CONTEXT-GAPS.md) | Один контролируемый контраст conditioning и проверенный consumer | Обобщение на другой домен требует собственного контекста и gold |

Обозначения R0–R5 совпадают с общей очередью верхнего документа пакета54. Значения E25/E09/E43 взяты из raw JSON, присутствующих именно в pinned clean: [E25 receipt][e25-result], [E09 receipt][e09-result], [E43 receipt][e43-result]. Рецепты ниже раскрывают, что измерялось и что нужно удержать постоянным.

## 2. Три положительных результата: что действительно было измерено

### 2.1. E25: память POS-биграмм действительно обучалась

В [E25 исходнике][e25-source] создаются 160 синтетических POS-последовательностей при `tier=1`, `seed=42`, `perPattern=20`. Первые 80% идут в train, последние 20% — в validation. Затем вызываются `new SentencePatternClassifier(k:3)` и `Train(train)`; rules и KNN оцениваются **на одном** `val`. Raw receipt от 2026-05-01 сообщает `.3438 → .875`, `hardAcc=.8571` на 28 hard cases. [Данные генерации и вызов Train][e25-source], [числа receipt][e25-result].

Сам [классификатор][pattern-source] хранит обучающие записи и голосует по ближайшим соседям; расстояние использует POS-биграммы. Следовательно, здесь есть положительный исторический след обучения памяти, который нельзя переименовать в «только неудачный матричный эксперимент». Воспроизведение должно проверить не только наличие класса в репозитории, но и реально переданные записи, `k`, поддержку классов, вызов обученного экземпляра после восстановления.

**Чего это ещё не доказывает.** Синтетическая классификация POS-шаблона не измеряет лексический выбор, правильность перевода, согласование, сохранение чисел или полноту предложения. Нельзя переносить `87.5%` на качество перевода. EN→RU и RU→EN — направления работы в предметной области перевода; сами по себе они не новые предметные домены.

**Связь с прежними findings.** Потеря E25 при использовании другого rule-only получателя, несовпадение множества классов и ограничения POS-биграмм уже описаны предыдущими аудитами. Этот раздел фиксирует raw evidence в требуемом clean и порядок восстановления; новой гипотезой эти старые находки не объявляются.

### 2.2. E09: положительный результат структурного KNN, требующий одинакового holdout

В [E09][e09-source] C# исходники превращаются в AST-узлы и структурные векторы. KNN обучается парам `(vector(current), next.BpeId)`. Исторический receipt: 5617 токенов, `k=5`, 4492 обучающие пары, `top1 .2413 → .3507`, среднее ближайшее расстояние `.587`. Это свидетельство, что **помещение структуры в поиск соседей** было полезно в конкретном опыте. [Receipt][e09-result].

При этом baseline оценивается на всём `val`, а KNN — на `val.Take(500)`. Это [прямо видно в строках 50–77][e09-source]. Поэтому `+45.33%` — исторически записанное сравнение, которое ещё нужно повторить на одинаковых вопросах, прежде чем приписывать весь gain структурному расстоянию. Кроме того, ID вида узла строится отдельно внутри каждого файла; конкатенация узлов создаёт пары и на межфайловой границе. Эти ограничения уже известны; не выдаём их за новые открытия. [Построение узлов и локальной карты][e09-mapping].

**Доменная применимость.** Структура синтаксического дерева особенно релевантна задачам о коде, где целевое решение зависит от типа узла, вложенности или связей символов. Перенос на языковой перевод или нерегулярный текст требует другой проверяемой структуры; поле с тем же именем в векторе не делает AST-признак полезным для всех доменов.

**Минимальное восстановление.** Сначала повторить trained KNN на общей закрытой панели. Затем дать тому же индексу штатный запрос; заменить только структурную часть запроса нейтральным значением и проверить падение целевой способности на структурных заданиях. Параметры, память и gold не менять. Отдельный отрицательный контроль — задания, где структура не должна различать ответы: gain там не ожидается.

### 2.3. E43: положительное сохранение переходов, не измерение нейронного continual learning

[E43][e43-source] делит **список входных файлов** пополам, называет части old/new, извлекает пары переходов и строит три таблицы частот: `BuildBigram(oldTrain)`, `BuildBigram(newPairs)` и `BuildBigram(newPairs + sampledOldTrain)`. Enhanced не продолжает обучение старой нейронной модели: он строит таблицу из смешанной памяти. Raw receipt сообщает oldPairs=465, newPairs=414, replay=177; no-replay `.3226`, with-replay `.3978`, `baseOnOld=34.4%`. [Исходный алгоритм][e43-source], [receipt][e43-result].

Это реальный положительный след для сохранения старого материала в обучаемой памяти. Он обосновывает проверку потери памяти при переезде в KNN и при восстановлении Unit. Он **не подтверждает** эффективность точного повторного применения уже обновлявшейся DPO-пары, не измеряет q/v LoRA и не устанавливает качество всей генерации. Названия CODE_Q/SECURITY в комментарии не доказывают, что файлы были размечены этими доменами: код делит последовательность по позиции.

В текущем `16.AutonomousLearning/Training/ReplayBuffer.cs` eligibility дополнительно ограничена совпадающим `Domain`; этот участок [совпадает с clean][replay-clean]. Такое ограничение может быть намеренной границей адаптера. Отмена фильтра не является рекомендованным «возвращением E43». Проверку retention надо строить на двух наборах навыков **внутри законного scope получателя** либо на явно оформленном исследовательском варианте, которому действительно разрешена междоменная память.

Ещё один обязательный контроль: извлекать переходы отдельно внутри файлов и показывать качество на source-family-disjoint holdout. Ранее найденные межфайловые ложные пары и точные повторяющиеся переходы не являются новыми гипотезами этого исследования.

## 3. Почему положительный архив нельзя просто запускать на сегодняшнем head

В clean сохраняются отчёты May-серии `INTERNAL-TRAIN-READY-V191`. Сам runner называет `FinalAccuracy` метрику **последнего батча**: `batchSummaries.Last().Accuracy`. Он [создаёт policy и reference отдельно][runner-model] и использует буквальные префиксы `Chosen:` / `Rejected:` при подготовке текста. Поэтому итоговая `Accuracy=1` из такого summary не равна доле правильных ответов на независимом holdout. [Формирование summary][runner-summary], [подготовка пар][runner-input].

Дополнительный архивный crosscheck: в V210 134 пары / 9 батчей, а в V219 374 пары / 24 батча. Последние три батча имеют совпадающие крайние IDs и одинаковые loss/accuracy/margin до опубликованной точности. Это согласуется с уже исследованным риском отсутствия эффекта предшествующих обновлений; само по себе численное совпадение не доказывает причину. [V210, строки 26–28][v210], [V219, строки 41–43][v219].

Отдельно проверенный исходник `11.DPO/DpoTrainer.cs` на предмайском `a52a83bec1be668c8a093f4e7311ab5da715d2cc` отличается от сегодняшнего trainer. При восстановлении historical run нужно фиксировать **commit исполняемого trainer, исходный checkpoint, tokenizer, данные и consumer**, а не только branch head, содержащий более старый отчёт. Ни old runner, ни этот предмайский commit не имеют доказанной связи с каждой успешной сессией пользователя. [Trainer указанной эпохи][era-trainer].

**Ограничение вывода.** Это не опровержение всех исторических успехов. В [предыдущем исследовании wiki/query-planning][wiki-prior] отдельно описаны реальные изменившиеся веса и положительные preference-метрики; их lineage следует сохранять отдельно от May margin-only отчётов. Это ссылка на выводы прежнего исследования: новый binary-readback его checkpoints здесь не заявляется. Повторение уже известной проблемы старого backward не считается новым finding этого документа.

## 4. Evidence ledger для дальнейшего воспроизведения

Для каждого приоритетного восстановления заполнить одну строку до обсуждения ремонта:

| Поле | Что фиксировать |
|---|---|
| Источник исторического утверждения | Raw receipt с SHA/path; пользовательское наблюдение хранить отдельным типом свидетельства |
| Исполняемый алгоритм | Функция и точный commit; если runtime SHA отсутствует, записать `UNKNOWN` |
| Что обучалось | POS-память, AST-KNN, таблица переходов, q/v LoRA, output head — конкретный объект |
| Что было сохранено | Проверяемый артефакт и его hash, corpus/index/version, ключи классов и размерности |
| Кто использовал результат | Обычный consumer, а не существующий рядом экспериментальный класс |
| Что сравнивалось | Одинаковые source IDs, heldout, бюджет, scorer и decoder |
| Домен | Предметная задача, поддержанные входные структуры и направления языка |
| Решение после проверки | `REPRODUCED`, `ARCHIVE_ONLY`, `INHERITED`, `CURRENT_CONSUMER_UNPROVEN` или конкретный подтверждённый разрыв |

Соседние ветки служат источниками восстановления lineage: DesignToCode `8f20ceb8a9bfbadab2ad309226c47dd76e714548`, language_translate `f6032aff81f1f83de7c09cf9b9efe6b058dd12b9`, external_domains `3036a01acec938f0b83fa4ffbda26202cc35d639`. Их наличие не меняет основной приоритет: сначала восстановить полезное поведение указанного пользователем clean и проверить его тем же штатным потребителем.

Адресный primary metadata readback корней этих трёх sibling refs показал совпадающие между ними tree SHA для `12.SemanticCompiler`, `34.Experiments`, `35.ExperimentsV2`. Clean имеет другие tree SHA. Последующая сверка именно трёх положительных линий дала более узкий вывод: в E09/E25/E43 исходниках sibling отличается от clean только пятистрочный copyright/SPDX-заголовок; три May01 JSON совпадают. Поэтому эти reports — сохранённая общая линия, **не три независимых повторных эксперимента**. Сравнение не распространяется на всё содержимое clean subtrees.

| Subtree | Clean git tree SHA | Общий git tree SHA трёх siblings |
|---|---|---|
| `12.SemanticCompiler` | `cae2d9355e803e5b46ec52b7d4fe0f00b524af0f` | `66eb13b71f12d4881b92fe551e24bdd62fcc74a7` |
| `34.Experiments` | `2efeec6d4f5911874b33c548593b7e16fc5550e3` | `82ffd0208fe99ecf1abc280598ecb32b81b67632` |
| `35.ExperimentsV2` | `e1e8f881d6818bb926c806e3ea8fef5b2585528d` | `aa5c7d5d6f3a1ecb1f221755dae61f03141d62da` |

Источники metadata: GitHub contents API для [clean][root-clean], [DesignToCode][root-dtc], [language_translate][root-lang], [external_domains][root-ext]. Содержимое E09/E25/E43 дополнительно прочитано на `language_translate@f6032aff…`, который имеет общий `35.ExperimentsV2` tree с двумя другими siblings.

## 5. Как использованы skills и как ограничен review

Использованы релевантные части `phase-planning-SKILL.md` и `plan-review-SKILL.md` из закреплённого current: evidence до решения, различение истории/гипотезы, проверка recipient, доменные критерии и отрицательные контроли. Это targeted application; полного формального review всех тысяч строк skills или независимого multi-model review данный документ не заявляет. Принятые решения и новые вопросы отдельно приведены в [плане микроэкспериментов](04-DOMAIN-MICRO-EXPERIMENTS.md).

[e25-result]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json#L2-L17
[e09-result]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json#L2-L17
[e43-result]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json#L2-L17
[e25-source]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/Experiments/E25PatternClassifier.cs#L22-L48
[pattern-source]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/12.SemanticCompiler/Translation/PatternClassifier.cs#L39-L87
[e09-source]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L30-L77
[e09-mapping]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L102-L123
[e43-source]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs#L23-L55
[replay-clean]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/16.AutonomousLearning/Training/ReplayBuffer.cs#L40-L68
[runner-summary]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/experiments-v2/INTERNAL-TRAIN-READY-V191/runner/InternalTrainReadyDpoRunner/Program.cs#L140-L154
[runner-input]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/experiments-v2/INTERNAL-TRAIN-READY-V191/runner/InternalTrainReadyDpoRunner/Program.cs#L192-L220
[v210]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/experiments-v2/INTERNAL-TRAIN-READY-V191/results/train-ready-v210-good-external-plus-internal-guide-full-20260511-014712/internal-train-ready-dpo-summary.md#L3-L28
[v219]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/experiments-v2/INTERNAL-TRAIN-READY-V191/results/train-ready-v219-core-plus-broad-chunks-1-3-full-20260511-022213/internal-train-ready-dpo-summary.md#L3-L43
[era-trainer]: https://github.com/lubochka/llm_mvp_core/blob/a52a83bec1be668c8a093f4e7311ab5da715d2cc/11.DPO/DpoTrainer.cs
[runner-model]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/experiments-v2/INTERNAL-TRAIN-READY-V191/runner/InternalTrainReadyDpoRunner/Program.cs#L44-L56
[wiki-prior]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/plans/replanning-possible-diffs/25-historical-neural-learning-donor.md
[root-clean]: https://api.github.com/repos/lubochka/llm_mvp_core/contents?ref=e346b88a3cc1fcce48aefa626bfc7b00509eefd6
[root-dtc]: https://api.github.com/repos/lubochka/llm_mvp_core/contents?ref=8f20ceb8a9bfbadab2ad309226c47dd76e714548
[root-lang]: https://api.github.com/repos/lubochka/llm_mvp_core/contents?ref=f6032aff81f1f83de7c09cf9b9efe6b058dd12b9
[root-ext]: https://api.github.com/repos/lubochka/llm_mvp_core/contents?ref=3036a01acec938f0b83fa4ffbda26202cc35d639

