# Независимый review Units и главы восстановления

19 октября 2026. Reviewer: отдельная подзадача `knn_history`. Проверка исходников, происхождения и формулировок; нативный C# запуск и обучение модели не проводились. Чужой draft и удалённые репозитории не изменялись.

## Зафиксированные объекты

| Объект | SHA256 рассмотренной версии |
|---|---|
| `agents/units/units-findings.md` | `463c1563c7945a7fdd6952e5f3df4de1c1379cfebd0828837926ca9a7e3925a9` |
| `agents/units/dtc_source_probe.py` | `50da2fbd5876601909ed40c34cf6c4562e4b43087ebaa6a12b34d94638870b25` |
| `agents/units/dtc-source-probe-result.json` | `ca53b87b8bf6032ed809407c3bc4e5a43d9acf5f00ab76782fab60d9ea4befb9` |
| `deliverables/2026-10-19-designtocode-clean-causal-audit/00-RESTORE-DESIGNTOCODE-CLEAN-FIRST.md` | `e9c843a7e6185f487537a4cee4388a55654b64fdb58c25b3e111a4f744b4a034` |

Все 23 локальных файла Units совпали с Git blob SHA и SHA256 из manifest. Дополнительно независимо получены GitHub contents-directory metadata на обоих закреплённых commit: current Machine=`947383043f9b40c352bdf28ab492febc2ed7ca5e`, clean Machine=`bad9b74f153afdd00ca6c7d42e185e312b9634a3`; TrainedUnits в обеих ветках=`3ef9f245d70df2c6dee673f7b9af5a15b3584701`. Current TrainingPipeline=`d48cb0bf58b3b6107c72500f8d68d5918d6848b3`, GradientBridge=`85bbc3e68c565807b5872fcb1191ed82503d7891` также сверены с GitHub. Это подтверждает привязку исследованных текстов; исполнявшаяся сборка этим не аттестована.

Собственный языковой draft reviewer зафиксирован без изменений: `NEW-STRUCTURAL-KNN-DONOR-BOUNDARIES.md`, SHA256 `9f31b0e21ce43cc8401e9eb19d84d1b5c57a868e8a06816a514bde5e1e2b0958`.

## Вердикт по Units: PASS_BOUNDED

Четыре узких source-сигнала подтверждены. Ни один не является доказанной причиной RP01 neural regression или измеренным улучшением visual quality. Именно такие границы уже поставлены автором. Нужна одна конкретизация будущего U-DTC-3 перед предметным выполнением; она не отменяет опубликованный арифметический контрпример.

### U-DTC-1a — PASS_BOUNDED: Critic update не использует полярность точной пары

Проверен путь [Pipeline→admitted Machine](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionTrainingPipeline.cs#L970-L1043), а не только имя wrapper. [Подготовка локального batch и margin](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L2909-L3003) разделяет `Deltas` и `AverageRewardMargin`. `AddCorrectionCategoryDeltas` возвращается для Critic; chosen/rejected категории воздействуют на Improver. [Commit](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L349-L406) применяет только deltas, а margin помещает в результат. Он не умножает вклад на знак margin.

Условие доказательства: фиксированы prompt, batch/source identity, исходное состояние, прочие входы и выбран локальный Critic recipient; обе стороны остаются допустимыми для разобранной JSON-проекции. Перестановка `.95/.20` меняет отчётный margin, но не mean/dimension/source-kind contributions. Configurable bridge получает [те же сгруппированные signals](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L3117-L3163), без raw pair; внешние данные собственного bridge остаются исключением. Это не утверждение, что admission допустит произвольную перевёрнутую учебную пару.

Clean whole method иной: дополнительные pair-family/hash-diversity признаки и другая проверка отсутствующего score. Рассматриваемый независимый от candidate-content вклад присутствует [в clean](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/44.DesignConversionDomain/DesignConversionMachine.cs#L2755-L2819). Автор не смешивает эти веса с отдельно подключаемой DPO policy и не объявляет разницу evaluator metadata обучением.

### U-DTC-1b — PASS_BOUNDED: evaluator и обычный Critic имеют разные проекции

Подтверждены [evaluator mean/dimension](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L3408-L3430), [runtime только source-kind и пересечение ключей](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L4339-L4385) и фактический [wrapper](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L4082-L4125). Последний сохраняет baseline `Passed`. Значения `1.0` и `.731059` в приложении — разные непрерывные формулы с искусственной baseline, а не разные измеренные accuracy.

Ключевое контрдоказательство сохранено корректно: [EvaluateAsync](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L443-L475) явно пишет `scope=sanitized local checkpoint feature-fit held-out evaluation; not trained visual QA authority`. Называть этот API лживым или уже обещающим визуальное качество нельзя. Подтверждён ограниченный разрыв представления; риск ошибочного вывода последующего автора зависит от такого потребителя отчёта.

### U-DTC-2 — PASS_BOUNDED: отрицательный вес не ограничивает fallback

Подтверждена полная цепочка [chosen +1 / rejected-only −1](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L3008-L3048) → runtime correction features → [wrapper передаёт preferences](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L4241-L4254) → [фильтр Weight>0 и fallback all available](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionTrainedUnits.cs#L313-L352) → [изменение CSS](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionTrainedUnits.cs#L161-L181). Это реальный consumer, а не только выведенное поле metadata.

Отрицательные числа являются относительной накопленной preference по категории. Они не доказывают абсолютную недопустимость всякой spacing-коррекции. Поэтому авторский mobile/desktop control и условие наличия scope необходимы. Желательная редакционная точность: вместо «вес исчезает» писать «отрицательный вес не исключает правило из fallback» — само хранимое число не стирается. Не считать применение padding доказанным вредом без render и не приписывать final-quality gain, если последующая QA и так всегда отклоняет кандидат.

### U-DTC-3 — PASS_BOUNDED: warm/cold drift вытекает из настоящего восстановления суммы

Помимо формул проверен настоящий путь сохранения/восстановления: [ExportLearnedWeights вызывает ToCheckpoint; StageLearnedWeights вызывает FromCheckpoint](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L2722-L2795), затем [Import заменяет словарь accumulator](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L731-L745). При обычном update существующий accumulator сохраняет сумму; само создание snapshot её не заменяет округлённым средним.

Класс accumulator current/clean идентичен; SHA256 выделенного полного класса=`95044b5ad7823b9f978822e73bb09ea70a3985f6ab5422a3c5508bf69f490eab`. При искусственных `Weight=.25, Count=4` и двух вкладах `.250002` точная арифметика даёт:

* После первого update: `1.250002/5=.2500004`, сохраняется `.25`; cold восстанавливает сумму `1.25`.
* Warm после второго: `1.500004/6=.250000666…`, round6=`.250001`.
* Cold после второго: `1.500002/6=.250000333…`, round6=`.250000`.

Здесь нет midpoint tie: разница не зависит от выбора tie-breaking именно для этих значений. Это независимая проверка рациональной формулы, не .NET исполнение. Стресс с двумя миллионами initial observations явно искусственный. Состояние может расходиться без отличия выбранного правила или пользовательского ответа.

**P2 — уточнение будущего опыта, не опровержение находки.** В штатном `AddCorrectionCategoryDeltas` вклады равны ±1; `.250002` не является полученным там correction signal. `internal_vector:mean` может быть дробным, но обычный Critic его не читает (U-DTC-1b). Перед предметной фазой нужно назвать реально обновляемый и читаемый feature owner. Один допустимый дизайн арифметического условия для category-memory: initial `Count=5, Weight=+1`, два законных урока с rejected-only данной категорией, то есть `−1,−1`. Warm даёт `3/7→.428571`; cold после первого восстанавливает `round(4/6,6)×6=4.000002`, итог `.428572`. Это также предложенный, **не выполненный** опыт; без смены реального решения его итог — только `STATE_CONTINUATION_DRIFT_CONFIRMED / QUALITY_EFFECT_NOT_OBSERVED`.

## Новизна относительно общего корпуса

В поиске участвовали все 689 Markdown обоих SHA-verified реестров; дополнительно прочитаны P123, N52 и исходный DIF-1008 контекст. Точные имена `BuildLearnedWeightDeltas`, `BuildCriticRuntimeFeatures`, `BuildLocalImprovementPlan`, `rejected_correction_category_penalty`, `_weightedSum` в прежних MD не дали описания этих механизмов. Поиск служит навигацией; вывод основан на смысле ближайших карточек.

* [P123](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/plans/replanning-possible-diffs/experimets/cards/P123-design-conversion-critic-evidence.md) устанавливал scaffold aggregator и оставлял actual learned wrapper/train contract условным. Новое здесь — точный admitted caller, локальный state owner и две проекции. Это уточнение того же семейства, а не впервые обнаруженный scaffold.
* [N52-02](https://github.com/lubochka/xiigen-planning/blob/b0308fbaa783758104987e6544d057062ccf6fc3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/52-ORCHESTRATOR-NEW-GAPS-AND-EVIDENCE-2026-10-19.md) объяснял слабость count-предиката через теоретический неизменный Weight. U-DTC-3 находит иной причинный механизм: округлённый mean становится исходной суммой после настоящего import. Не повторяет N52-01 sampling и N52-03 authority-gate.
* Общая потеря отрицательных уроков уже известна в переводе. Новая область U-DTC-2 ограничена конкретным learned CSS fallback; её нельзя продавать как вновь открытую универсальную проблему DPO.

## Быстрый review главы00

**PASS_BOUNDED по логике восстановления и объёму опытов.** Оговорки E26 (заранее заполненный словарь), E27 (даны POS/pattern), разных экзаменов E07/E09 и непереносимости kNN успеха на свободные ответы Student корректны. Два урока/четыре новых задания — разумный локальный сигнал; автор не превращает шесть вариантов в обязательную матрицу gates. ДР-Н в своей начальной части — проверка learnability текущего learner; восстановление clean следует заявлять лишь после отдельного донорского различия и сопоставимого consumer.

**P2 — происхождение двух строк таблицы исправить перед публикацией.** Рассмотренная таблица называется «Источник в DesignToCode-clean», но строки continuation-memory и ProjectWiki ссылаются только на current docs/78 и docs/25. В самом [78](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/plans/replanning-possible-diffs/78-translation-restored-memory-positive-receipt.md) baseline закреплён на `5fa0f242`, в [25](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/plans/replanning-possible-diffs/25-historical-neural-learning-donor.md) source lineage — `dd22eb10/195840c`; эти тексты сами по себе не устанавливают receipt на закреплённом clean. Вынести строки в «Дополнительные исторические свидетельства» или дать прямые raw clean blob links с пределами бинарной аттестации. Содержательные выводы строк уже достаточно узкие.

**P3 — приоритет реально обучавшегося clean classifier.** Добавить E25 в первую положительную таблицу: runner действительно вызывает `Train` на synthetic train split, в отличие от preset E26 и fixed E27. Root уже сообщил о добавлении E25/E43; настоящий review hash относится к версии до этой интеграции. Не выдавать позднее изменение главы за повторно проверенное без нового hash.

Дальнейший широкий source audit, новые native quality прогоны или повторение всей экспериментальной матрицы этим review не требуются. Units пригоден к интеграции как ограниченный исследовательский пакет; глава00 требует указанной точности происхождения.
