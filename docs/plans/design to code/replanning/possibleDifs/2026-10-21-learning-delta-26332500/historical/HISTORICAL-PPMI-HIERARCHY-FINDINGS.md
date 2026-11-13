# Историческое обучение: две новые причины для проверки и границы прежних успехов

Дата: 2026-10-21. Текущий core: `26332500f76ea8345256c73ecfd44c0023b7e883`; planning: `14308ff181c5a93d837d92b85b2f35c41b863565`. Статус этого дополнения: **документный аудит; новые обучающие прогоны не выполнялись**. Обе новые карточки: `AUTHOR_WORK_ORDER_ONLY / NOT_IMPLEMENTED / NOT_RUN`.

## Вывод

В исторической `34.Experiments` найдены два конкретных несоответствия между тем, что модель обучает, и тем, как затем читается её состояние:

1. `E10SvdPmiEmbedding` обучает факторизацию **направленных переходов**, но при предсказании следующего токена сравнивает две левые стороны факторизации. Такой результат симметричен и не сохраняет общее направление перехода.
2. `E05HierarchicalPrediction` обучает внутреннюю модель каждого кластера на его собственной подпоследовательности, но при переходе между кластерами передаёт в модель целевого кластера локальный номер токена из исходного кластера.

Оба механизма отсутствуют как отдельные причины/карточки в проверенных документах `possibleDifs` и защищённой `docs/plans/replanning-possible-diffs`. Это новые **кандидаты на пересмотр исторических выводов и восстановление работавшего обучения**, а не доказанные причины регрессии RP01. Код уже был таким в соседних ветках; пути, по которому эти две реализации стали бы учебным механизмом RP01 Student, не найдено. Измеренного выигрыша новых исправлений пока нет.

## 1. Направленная PPMI читается как симметричная похожесть

### Доказанный механизм

В [E10, строки 60–84](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/34.Experiments/Experiments/E10SvdPmiEmbedding.cs#L60-L84) счётчик получает пару `a=train[i], b=train[i+1]`. Это направленная матрица: «после a идёт b». У строк и столбцов разные роли. После PPMI [вызывается SVD](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/34.Experiments/Experiments/E10SvdPmiEmbedding.cs#L86-L97).

Алгоритм действительно вычисляет обе стороны `u` и `v`, затем использует их для удаления найденной компоненты. Однако [строки 177–186](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/34.Experiments/Experiments/E10SvdPmiEmbedding.cs#L169-L186) сохраняют только `u*sqrt(sigma)`. В [ArgmaxDot, строки 189–201](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/34.Experiments/Experiments/E10SvdPmiEmbedding.cs#L189-L201) текущий и предполагаемый следующий токен берутся из одной этой таблицы.

Для исходной матрицы `M ≈ UΣVᵀ` оценка направленного перехода имеет вид `U√Σ · (V√Σ)ᵀ`. Код читает `UΣUᵀ`. Это математический вывод из показанных операций, не измеренный результат. Последняя матрица симметрична; общий цикл `a→b→c→a` направленным способом она представить не может.

**Граница вывода:** левые векторы вполне могут быть полезны для поиска похожих исходных контекстов через kNN. Поэтому опыт «похожесть исходных контекстов как вероятность следующего токена» не опровергает полезность тех же векторов как признаков поиска. Однако конкретная связь с упомянутым пользователем успешным переносом «матрица → kNN» не установлена: нет связанного комплекта старого/нового кода, одинакового набора и двух сырых результатов.

### Где это участвует в обучении

[ExperimentRunner:70–87](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/34.Experiments/Infrastructure/ExperimentRunner.cs#L70-L87) допускает E10 после проверки объёма, обучает BPE словаря 500 через свой helper и передаёт токены в [Dispatcher:60–62](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/34.Experiments/ExperimentsDispatcher.cs#L60-L62). Параметры `vocab=500, rank=30, iterations=10` действительно потребляются этим учеником и его оценкой. Это не только неиспользуемая математическая утилита.

[Исторический E10 в external_domains](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/34.Experiments/Experiments/E10SvdPmiEmbedding.cs) совпадает с текущим после удаления пяти строк добавленного AGPL заголовка. Тот же исторический blob `633b8bea01f19f17936ea1e696c71f91a7aaa45e` прочитан в `language_translate@f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` и `DesignToCode@8f20ceb8a9bfbadab2ad309226c47dd76e714548`. Текущий blob: `bc1d6b1733a615ce8c3b455867e2622fd0eabc29`. Это унаследованная семантика, а не новая потеря при переносе.

### Что проверять

Карточка [SVD-DIRECTED-TARGET-01.md](SVD-DIRECTED-TARGET-01.md) задаёт реальное построение PPMI/SVD до и после добавления уроков. Единственный фактор — какая сторона факторизации служит вектором кандидата. Частотный bigram остаётся отдельным ориентиром. Токенизатор, clamp, число итераций и rank не ремонтируются одновременно.

Первый домен — оркестрация: порядок стадий учебного процесса. Это маленькая причинная проверка направления; она не доказывает результат на C# или переводах. Для кода следующий этап мог бы проверять порядок AST/токенов, для перевода — порядок слов, но только отдельными тематическими наборами и отдельной карточкой после принятого результата.

## 2. Локальный номер токена передаётся в другую модель кластера

### Доказанный механизм

[E05:88–106](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/34.Experiments/Experiments/E05HierarchicalPrediction.cs#L88-L106) создаёт список членов для каждого кластера; `tokenToLocal` — позиция токена в **его собственном** списке. Локальный `0` кластера A и локальный `0` кластера B обозначают разные токены.

[E05:108–123](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/34.Experiments/Experiments/E05HierarchicalPrediction.cs#L108-L123) обучает `withinBigrams[c]` на подпоследовательности, из которой удалены токены других кластеров. Семантика этой таблицы — «какой член c встретится после предыдущего члена c». Такая абстракция сама по себе допустима.

Но [E05:128–147](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/34.Experiments/Experiments/E05HierarchicalPrediction.cs#L128-L147) сначала выбирает `predCluster`, затем вызывает его `withinBigrams[predCluster].Predict(curLocal)`, где `curLocal` получен из текущего `curToken`, а не из истории целевого кластера. При межкластерном переходе обученная строка адресуется чужим локальным номером.

Например, A имеет токены `a0,a1,a2`, B — `b0,b1,b2`. Если наблюдался `b2`, затем `a0`, а следующий кластер — B, таблица B должна читать состояние прошлого `b2`. Текущий код читает B-строку `0`, поскольку текущий `a0` имеет номер `0` в A. Совпадение чисел не является совпадением состояний.

### Где это участвует в обучении и границы исправления

Путь до ученика: [ExperimentRunner:84](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/34.Experiments/Infrastructure/ExperimentRunner.cs#L80-L87) → [Dispatcher:38–40](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/34.Experiments/ExperimentsDispatcher.cs#L38-L40) → kMeans и три уровня обученного состояния в E05. [Нативный BigramMarkov.Train](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/34.Experiments/Core/MarkovChain.cs#L24-L41) строит реальные счётчики; [Predict](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/34.Experiments/Core/MarkovChain.cs#L48-L62) читает именно указанную строку.

[Исторический E05](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/34.Experiments/Experiments/E05HierarchicalPrediction.cs) также совпадает с текущим после удаления пяти строк AGPL заголовка: исторический blob `7475c112d932dde5ea9e1be03f4facf2c1942cf5`, текущий `a5ce1f42d708075a11dd32dced6c4b72a7c821eb`.

Карточка [HIERARCHICAL-LOCAL-HISTORY-01.md](HIERARCHICAL-LOCAL-HISTORY-01.md) сохраняет обучение на фильтрованной подпоследовательности. Меняется только вход для чтения таблицы: последний наблюдённый локальный номер **целевого** кластера. Преобразование обучения на обычные соседние пары было бы другим фактором и здесь запрещено.

Чтобы изолировать причину, первое плечо использует фиксированную разметку кластеров одной темы; это эксперимент с нативной локальной моделью E05, не полный повтор kMeans/BPE-конвейера. Отдельное повторное обучение при перестановке только локальных номеров проверяет независимость семантического результата от произвольной нумерации. Оно не заменяет измерения обучения до/после.

## 3. Соседние ветки содержат реальные положительные результаты

Эти записи — **ранее сохранённые результаты**, заново не исполненные в данной сессии. Их значения нельзя превращать в результаты предлагаемых новых карточек.

| Опыт и источник external_domains | Сохранённый полезный показатель | Другие показатели и предел вывода |
|---|---|---|
| [E25-PATTERN-CLASSIFIER, run001](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json) | accuracy 0.3438 → 0.875; 32 validation из 160 примеров; hard 0.8571 на 28 случаях | time 0.4 → 15.4 ms. [Runner](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/Experiments/E25PatternClassifier.cs) вызывает `SentencePatternClassifier.Train`; это обученный классификатор синтетических шаблонов, не доказательство качества свободного перевода. |
| [E09-STRUCTURAL-KNN, run001](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json) | raw accuracy 0.2413 → 0.3507 | time 14.5 → 763.5 ms. [Runner](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L45-L73) оценивает bigram на всей val, kNN на первых 500 токенах: разные знаменатели. При 5617 токенах это 1123 и 499 переходов соответственно. Сохранённый положительный сигнал остаётся; величина причинного выигрыша требует одинаковых query IDs. Признаки Roslyn/AST относятся к коду. |
| [E26-CONTEXTUAL-DICTIONARY, run001](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-001.json) | 0.4 → 1 на 15 случаях; hard 1 на 6 случаях | time 0.8 → 2.2 ms. [Runner](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/Experiments/E26ContextualDictionary.cs) берёт `BuildDefault()`; это успешный выбор значения по контексту, но в этом runner нет приобретения нового обученного состояния. Повтор tier3 использует те же 15 тестов, поэтому не подтверждает расширение охвата. |
| [E43-REPLAY-BUFFER, run001](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json) | retention accuracy 0.3226 → 0.3978; reference на old до нового fit около 0.344 | time 0.1 → 5.8 ms. [Runner](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs) сравнивает заново построенные bigram на new и new+replay177; это реальный эффект состава обучающего набора на удержание старых пар. Это не продолжение нейросетевого checkpoint и не доказательство LoRA replay. |

Здесь намеренно нет единого вердикта «все успешны» или «все невалидны». E25 и E43 показывают реальные полезные изменения конкретных обучаемых механизмов. E09 сохраняет положительную запись с несопоставимым экзаменом. E26 подтверждает функцию выбора, но не обучение в этом runner. Рост времени показан рядом; он не отменяет успех по метрике качества, но ограничивает решение о переносе.

## 4. Низкий приоритет: StackAndVersion выдаёт проверку правил за обучение

Это **не новая обучающая карточка** и не заявленная причина снижения learning accuracy. В текущем пути нет наблюдённого обновления обученного состояния:

- [DpoGenerator:27–42](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/42.ProjectWikiDomain/StackAndVersionDpoGenerator.cs#L27-L42) получает expected labels от того же `StackAndVersionClassifierUnit.Classify`.
- [TrainingRunner:20–75](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/42.ProjectWikiDomain/StackAndVersionTrainingRunner.cs#L20-L75) проверяет размеры/маркеры train и сравнивает классификатор с его же expected labels на heldout; вызова fit или изменения state нет.
- [Pipeline:38–64](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/42.ProjectWikiDomain/StackAndVersionTrainingPipeline.cs#L38-L64) всё же использует этот gate для публикации артефакта.
- [Publisher:59–65, 96–104](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/42.ProjectWikiDomain/StackAndVersionManifestPublisher.cs#L59-L104) записывает metadata результата в `policy.bin` и выдаёт число train rows за `TrainingStepCount`.

Конкретный пример ограничения oracle: [Classifier:27–32](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/42.ProjectWikiDomain/StackAndVersionClassifierUnit.cs#L27-L32) распознаёт modern .NET через `^net[5-9]`; строка `net10.0` этому выражению не соответствует. Это вывод по исходнику, не выполненный тест. Поскольку generator и проверка используют один классификатор, общий промах не обязан снизить self-accuracy.

Будущая коррекция оригинала, если этот путь назначат обучаемым: независимые labels в generator, действительное обновление и сохранение состояния в runner/publisher, before/after exam с непросмотренными версиями. Если путь остаётся детерминистическим извлечением метаданных, честно переименовать статус и не считать rows обучающими шагами. Выбор архитектуры и интеграция не выполнялись. Расходовать учебный бюджет на одно исправление regex без learner запрещено.

## 5. Новизна, покрытие и порядок

Проверены текущие Markdown всей `possibleDifs` и всей защищённой `docs/plans/replanning-possible-diffs`. Запросы включали точные имена `E10SvdPmiEmbedding|E05HierarchicalPrediction|TruncatedSvd|ArgmaxDot|withinBigrams|curLocal|localSeq`, `PPMI`, русские/английские описания сингулярных факторов и локальных номеров кластеров.

[Уже существующий primary-evidence:111–115](https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/2026-10-20-primary-evidence.md#L111-L115) различает 34/E10, 34/E09 и 35/E09 и фиксирует ненайденный matrix→kNN provenance. Это **уже покрытая** находка. Нынешние SVD01/HCL01 добавляют конкретные ошибки чтения обученного состояния; они не переименовывают P03.3/P27.

| Кандидат | Защищённые эксперименты на cutoff | Новый документ | Запуск в этой сессии |
|---|---|---|---|
| U-left вместо V-right при directed prediction | Родственная provenance тема есть; механизм и причинная карточка не найдены | SVD-DIRECTED-TARGET-01 | NOT_RUN |
| Чужой localId при чтении target-cluster model | Точный механизм/карточка не найдены | HIERARCHICAL-LOCAL-HISTORY-01 | NOT_RUN |
| StackAndVersion self-oracle | Отдельного механизма в проверенном корпусе не найдено; learning update не найден | Только приложение, без учебного запуска | NOT_RUN |
| E25/E09/E26/E43 исторические успехи | Эти семейства уже исследовались; не считать новыми гипотезами | Перепроверенная таблица границ | Только чтение сохранённых результатов |

Общий текущий порядок начинается с `PARENT-ADOPTION-01 → N63-INGRESS-LENGTH-LEARNED-01 → завершение N63C`. После разблокировки текущих нативных обучающих запусков и устранения конфликтов их карточек — новые кандидаты по общему registry. Локальный порядок исторического дополнения: `HIERARCHICAL-LOCAL-HISTORY-01`, затем `SVD-DIRECTED-TARGET-01`. StackAndVersion — после прямых учебных задач. Это дополнение не переписывает защищённый registry и не отменяет прямое указание пользователя о следующем ID; автоматических запусков нет.

Защищённая папка не менялась. Пока эти две новые причины не добавлены в её собственные эксперименты уполномоченной сессией, утверждение «все найденные детали уже описаны там» неверно. Здесь приложены готовые по содержанию карточки-кандидаты; их будущий код ещё не создан, поэтому нельзя выдавать команды как уже существующий runnable host.

