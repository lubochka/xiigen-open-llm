# Адресные гипотезы и подтверждения: Critic/Improver и продолжение весовой памяти Design-to-Code

19 октября 2026. **Исследовательский пакет для независимого review; продуктовый код, модель, admission и обучение не изменялись.**

Тема обучения — **разработка интерфейсов**, профиль задачи — преобразование дизайна в HTML/CSS и исправление визуальных дефектов. `DesignConversionDomain`, `Critic`, `Improver`, `Unit` — имена программных получателей и механизмов, а не новые предметные домены.

## Что найдено

Рассмотрены **четыре адресные проверки в трёх семействах**. После сверки параллельной публикации N54 независимую новизну сохраняют U-DTC-1b и U-DTC-3. U-DTC-1a подтверждает уже опубликованный механизм; U-DTC-2 конкретизирует опубликованный fallback новым отрицательным контрпримером. Они также уточняют прежний P123, который оставлял реальный обученный wrapper и его API условными. Здесь прослежена цепочка `TrainingPipeline → TrainAdmittedBatchAsync → локальные feature weights → CreateRuntimeRoutingFlow → LearnedWeightCriticUnit / LearnedWeightImproverUnit`.

| ID | Новый механизм | Классификация относительно DesignToCode-clean | Приоритет маленькой проверки |
|---|---|---|---|
| U-DTC-1a | Содержательное предпочтение пары не входит в локальные обновления Critic: вычисленная разница visual score остаётся в отчётном RewardMargin | **Inherited, источник подтверждён.** Точные методы различаются, рассматриваемая потеря присутствует в обеих ветках | 2 урока: проследить независимость веса от предметного ответа до попыток масштабировать обучение |
| U-DTC-1b | Векторные признаки, которыми локальный evaluator измеряет feature fit, отсутствуют во входном представлении ordinary Critic | **Inherited projection mismatch.** Это ограничение конкретного локального feature learner | 4 новых входа: два реальных визуальных контраста при одном source kind |
| U-DTC-2 | Отрицательный выученный вес CSS-коррекции не исключает правило из fallback: когда положительных доступных правил нет, выполняются все доступные, включая отвергнутое | **Inherited, байт-в-байт одинаковый consumer в обеих ветках** | Наиболее прямой предметный A/B: исправление или сохранение полезного отрицательного урока |
| U-DTC-3 | Checkpoint сохраняет округлённое среднее и число наблюдений, а после загрузки заново строит сумму; частые перезапуски теряют накопленный остаток | **Inherited, одинаковый класс accumulator.** Небольшая численная дельта; пока низкий приоритет влияния на качество | 2 малых updates, сравнение warm/cold; затем только при различии настоящего выбора |

**Эти находки не объявляются причинами деградации RP01 C# / EN→RU.** RP01 neural Student имеет другого получателя обновлений. Они также не доказывают, что вся ветка `DesignToCode-clean` обучалась плохо: отдельная policy LoRA, локальная память выбора коррекций и scaffold имеют разные состояния и потребителей.

Исходники закреплены на `codex/dpo-implementation@358af351e22a92a3f7938d1f9221d5aeb7ddb053` и `DesignToCode-clean@e346b88a3cc1fcce48aefa626bfc7b00509eefd6`. [source-manifest.json](evidence/units/source-manifest.json) содержит Git blob SHA, SHA256, размер и URL 23 прочитанных файлов. Локальные байты проверены именно против Git blob SHA.


**Уточнение перед публикацией:** на planning `64ade14ed5dfb799c5e3edc7ac44acdc957aabcd` [N54 уже описывает одинаковые deltas при swap](https://github.com/lubochka/xiigen-planning/blob/64ade14ed5dfb799c5e3edc7ac44acdc957aabcd/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/54-DESIGNTOCODE-CLEAN-EVIDENCE-REVIEW-2026-10-19.md#L21-L38) и [fallback при неположительных весах](https://github.com/lubochka/xiigen-planning/blob/64ade14ed5dfb799c5e3edc7ac44acdc957aabcd/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/54-DESIGNTOCODE-CLEAN-SOURCE-PARITY-AND-NEW-GAPS-2026-10-19.md#L27-L37). Поэтому U-DTC-1a/2 не посчитаны новыми независимыми находками. Для U-DTC-1b ранее известен другой [train/serve contrast hash-diversity](https://github.com/lubochka/xiigen-planning/blob/64ade14ed5dfb799c5e3edc7ac44acdc957aabcd/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/54-DESIGNTOCODE-CLEAN-EVIDENCE-REVIEW-2026-10-19.md#L9-L14); новый вклад здесь — две конкретные vector keys в нынешнем ordinary runtime. [Полное решение независимой дельта-проверки](evidence/review/review-publication-delta.md).

## 1. Реальная цепочка получателя

[TrainingPipeline, 970–1052](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionTrainingPipeline.cs#L970-L1052) формирует Machine admission и вызывает внутренний `TrainAdmittedBatchAsync`. Публичный `TrainBatchAsync` теперь требует иной authority-маршрут; успешный путь pipeline уже мигрирован, поэтому это **не новая находка о потерянном caller**.

[Machine, 217–266](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L217-L266) строит `PreparedTrainingBatch`. При отсутствии внешнего `_gradientBridge` локальные deltas сразу коммитятся. При наличии bridge передаются сгруппированные разрешённые признаки. [Контракт bridge, 11–40](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachineGradientBridge.cs#L11-L40) содержит `CanonicalUnitId`, `AllowedFeatureKey`, `SignalValue`; сырая пара или отдельная целевая разметка Critic в нём отсутствует.

[CommitLearningBatchAsync, 349–408](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L349-L408) применяет feature deltas и записывает `RewardMargin`. [CreateRuntimeRoutingFlowSnapshot, 1498–1531](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L1498-L1531) действительно оборачивает Critic/Improver сохранёнными весами. Значит это конкретный путь изменения и чтения состояния, а не предположение по слову «learned» в классе.

**Граница:** при установленном прямом DPO policy scorer Critic выбирает отдельную policy-ветку. Локальные feature weights не являются полным описанием всех возможностей DesignConversionMachine. Сам код честно маркирует локальную feature-коррекцию как `NOT_TRAINED_MODEL`/`NOT_TRAINED_AUTHORITY`. Исследование не снимает эти метки и не приписывает им visual intelligence.

## 2. U-DTC-1a — подтверждение N54: локальный Critic получает статистику входа без предметного предпочтения

### Доказательство

[BuildLearnedWeightDeltas, 2965–3003](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L2965-L3003) вычисляет `chosenScore - rejectedScore` и отдаёт это значение в `rewardMargin`. Но список весовых contributions строит из:

- среднего `internal_vector`;
- числа координат `internal_vector`;
- признака `source_kind` со значением 1;
- категорий коррекций, которые последующий метод применяет **только к Improver**.

Последнее условие явно находится в [3008–3017](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L3008-L3017). Для Critic ни значения `visual_score`, ни HTML/CSS выбранной стороны, ни противоположное предметное действие не меняют эти contributions при одинаковом prompt. [Сигналы configurable bridge, 3117–3140](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L3117-L3140) также выводятся из тех же deltas. У стороннего bridge могут быть собственные внешние данные, но приведённый request не передаёт ему утраченное содержание пары.

В clean [2755–2819](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/44.DesignConversionDomain/DesignConversionMachine.cs#L2755-L2819) этот же разрыв существует. Там дополнительно были признаки pair family и разнообразия source hashes. Нынешняя версия удаляет эти identity-derived признаки и допускает различные JSON-стороны без `visual_score`, записывая margin=1. **Целые методы не идентичны.** Возврат старых признаков не восстановит собственно содержательное предпочтение.

### Выполненная проверка и предел вывода

В [Python-разборе](evidence/units/dtc_source_probe.py) два искусственных JSON-ответа `.95/.20` поменяны сторонами при одном prompt. Расчётный RewardMargin меняется `+0.75 → −0.75`, а contributions остаются одинаковыми: mean `.5`, dimension `4`, source-kind `1`. Это демонстрация проекции по прочитанным формулам. Перевёрнутая пара **не подавалась в admitted training**, не одобрена как учебный материал и не использовалась для изменения модели.

**Гипотеза влияния:** локальный Critic с таким представлением не сможет выучить предметное различение двух кандидатов, которое меняет предпочтение, но не эти признаки. Это конкретное ограничение получателя; оно не опровергает обучение отдельной neural policy.

### Маленький будущий эксперимент

Два корректных урока по интерфейсам: сохранение видимой кнопки и сохранение нужной интерактивной области при изменении CSS. Четыре новых входа из других макетов: два с аналогичным дефектом, два допустимых контроля. Один исходный Machine checkpoint, один набор разрешённых lesson bytes, один source encoder и один runtime consumer.

Сначала проверить фактически созданные `LearningTrainingItem` и downstream `SanitizedSignals`, не обходя admission. На **искусственной diagnostic fixture** отдельно проверить содержательную перестановку при одинаковых derived features. Предметное A/B допускается только после определения конкретного pair-conditioned Critic contract: A — текущая input-only проекция; B — та же ограниченная модель с независимо заданными `φ(source,candidate_chosen)−φ(source,candidate_rejected)` и тем же допустимым source representation. Это предложенное изменение дизайна, **не уже существующий API**; не маскировать его как одну строку «умножить на margin».

Снимать: какие сведения пары дошли, настоящий delta state, обе оценки кандидатов и 4 обычных ответа, cold restore и false accept/false reject. Если используется другая neural policy, её результат вести отдельной строкой. Если B меняет лишь статистический RewardMargin без полезного обычного решения — гипотеза исправления качества не подтверждена.

## 3. U-DTC-1b — локальный экзамен и обычный Critic читают разные признаки

### Доказательство

[BuildHeldOutEvaluationFeatures, 3408–3430](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L3408-L3430) создаёт `internal_vector:mean`, `internal_vector:dimension` и source-kind. [3360–3401](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L3360-L3401) по этим признакам вычисляет feature-fit score и сопоставляет его с ожидаемым accepted.

В ordinary path [BuildCriticRuntimeFeatures, 4339–4350](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L4339-L4350) получает только source-kind; `baselineVerdict` явно не читается. [BuildLocalCheckpointFeatureContributions, 4368–4385](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L4368-L4385) исключает веса с отсутствующим runtime feature. Поэтому выученные vector mean/dimension **не влияют на ordinary Critic** этой ветки.

В clean evaluator [3223–3260](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/44.DesignConversionDomain/DesignConversionMachine.cs#L3223-L3260) также имел vector features; runtime [4334–4370](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/44.DesignConversionDomain/DesignConversionMachine.cs#L4334-L4370) их также не имел, хотя включал hash-diversity. Это унаследованный разрыв проекций.

На искусственных весах предыдущего примера и искусственном baseline `.5` arithmetic probe даёт `feature_fit=1.0`, ordinary feature-adjusted score `.731059`. Это два различных вычисления по исходным формулам, **не измеренная accuracy модели**. Непрерывный score и `Passed` также различаются: локальный wrapper сохраняет baseline `Passed`, [4095–4125](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L4095-L4125).

**Существенное контрдоказательство к overclaim:** evaluator явно обозначает scope как sanitized local feature fit, а не trained visual QA. Поэтому это не обвинение существующего API в ложной метрике; риск состоит в неверном переносе его результата на обычное качество и в отсутствии общего meaningful representation.

### Маленький будущий эксперимент

На том же состоянии и четырёх новых интерфейсных входах одновременно снять именно штатный `EvaluateAsync` и обычный `GetOrchestratorAsync`→Critic. Сохранить одинаковые source-kind/QA input и независимо описать различающую геометрию. Проверить отображение каждого model feature до обоих потребителей. Это **readout experiment без нового обучения**, если нужное состояние уже есть.

B — диагностическая оценка через **тот же настоящий runtime consumer**, вместо отдельного feature-fit суррогата; train state не менять. Если нужно добавить общий semantic encoder в runtime, это отдельное последующее A/B с заранее выбранным source-only encoder, который не читает gold, QA expected verdict или teacher labels. Первое сравнение восстанавливает достоверность измерения и выявляет требуемый consumer; оно само по себе не объявляется обучением визуального навыка.

## 4. U-DTC-2 — конкретизация N54: отвергнутая коррекция возвращается через deterministic fallback

### Доказательство

Обучение Improver действительно учитывает обе стороны: [Machine, 3019–3048](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L3019-L3048) добавляет `+1` выбранным категориям и `−1` категориям, присутствующим только в rejected. [LearnedWeightImproverUnit, 4241–4262](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L4241-L4262) передаёт эти preference weights реальному consumer.

Но [BuildLocalImprovementPlan, 312–352](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionTrainedUnits.cs#L312-L352) оставляет для learned ranking только доступные правила с `Weight > 0`. Когда таких нет, он возвращает `SelectedRules = availableRules`. Правило с отрицательным весом не исключается из fallback.

Для категории spacing consumer реально добавляет `gap:12px;padding:16px`, [373–379](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionTrainedUnits.cs#L373-L379); [160–175](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionTrainedUnits.cs#L160-L175) использует выбранные правила для изменения HTML/CSS.

`DesignConversionTrainedUnits.cs` имеет **одинаковый Git blob SHA `3ef9f245d70df2c6dee673f7b9af5a15b3584701`** в обеих ветках. [Тот же consumer на clean](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/44.DesignConversionDomain/DesignConversionTrainedUnits.cs#L312-L352).

### Выполненная source-проверка

Искусственные weights `contrast=+1, spacing=−1` дают:

| Доступные правила | Выбранное правило | Что это устанавливает |
|---|---|---|
| contrast + spacing | contrast | Положительная память работает в подходящей ветке |
| только spacing | spacing | Отрицательный урок не запрещает ту же коррекцию |
| только spacing, памяти нет | spacing | Fallback с отрицательной памятью совпал с отсутствием памяти |

DOM/render не запускались. Само применение spacing rule не доказывает ошибку конкретного макета. Отрицательная category-memory также может быть недостаточно обусловлена контекстом, поэтому превращать каждый глобальный отрицательный вес в вечный запрет было бы неоправданным расширением.

### Маленький будущий A/B

Два CODEX-проверенных урока: один учит полезную contrast-коррекцию, второй отвергает именно увеличение padding в тесном мобильном макете. Четыре новых входа: два тесных макета, где увеличение padding нарушает заданное ограничение; один desktop control, где spacing поправка полезна; один случай без spacing-дефекта. Gold задаётся видимостью/попаданием элементов в контейнер и правильным HTML/CSS, а не названием выбранного rule.

A — текущий выбор. B — **одна предложенная политика fallback**: для применимого learned scope не возвращать явно отвергнутое правило; если положительного допустимого действия нет, сохранить исходного кандидата и вернуть необходимость другой коррекции/QA. Не добавлять новый CSS rule, новый tokenizer или новую policy. Если существующее состояние не хранит достаточного scope для различения mobile/desktop, это конкретная зависимость: сначала ограничить набор строго одинаковым контрактом или спроектировать scoped memory, не принимать глобальный ban.

Снять learned weight, available/selected rules, фактический HTML/CSS, новые дефекты и abstention/coverage; повторить после cold load. Успех требует уменьшения неправильных CSS-изменений **при сохранении полезных коррекций**, а не только роста числа отказов. Если current downstream QA уже всегда отклоняет плохой кандидат, эффект может ограничиться числом бесполезных итераций; это тоже записывается отдельно от итогового качества.

## 5. U-DTC-3 — округлённое среднее становится новым исходным состоянием суммы

### Доказательство и величина эффекта

[LearnedWeightAccumulator.Apply/ToCheckpoint, 4026–4044](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L4026-L4044) хранит внутреннюю сумму contributions, но записывает только `Round(sum/count,6)` и count. [FromCheckpoint, 4046–4054](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L4046-L4054) восстанавливает внутреннюю сумму как `checkpoint.Weight * checkpoint.ObservationCount`. Дробный остаток до округления потерян.

В clean [4037–4074](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/44.DesignConversionDomain/DesignConversionMachine.cs#L4037-L4074) тот же механизм; весь класс accumulator совпал при source comparison.

Выполнен арифметический разбор с **искусственным** начальным `Weight=.25, Count=4` и двумя contributions `.250002`:

| Продолжение | Итоговый Weight | Count |
|---|---:|---:|
| Два updates без промежуточной загрузки | .250001 | 6 |
| Save/load после каждого update | .250000 | 6 |
| Предложенный перенос точной суммы через load | .250001 | 6 |

Более близкий к штатной category-memory пример, дополнительно проверенный арифметически при интеграции: начальные `Weight=+1, Count=5`, два rejected-only вклада `−1,−1`. Warm даёт `.428571`; cold между уроками — `.428572`, потому что после первого load сумма восстанавливается как `round(4/6,6)×6=4.000002`. Такой знак вклада присутствует в настоящем category consumer, но данный начальный state и два урока здесь искусственно заданы. [Расчёт и source verification](evidence/review/root-source-checks.json). Это также не измеренный native или предметный эффект.

Дополнительный искусственно заданный stress-state `Count=2,000,000, Weight=0` и 1000 численных `+1` даёт warm `.0005`, cold-per-step `0`. Это **не два миллиона реально проведённых уроков**, не найденный production checkpoint и не native run. Такой стресс показывает условие накопления малых дельт при большом числе наблюдений.

Реальная цена пока неизвестна. На маленьких корпусах дельта порядка `10^-6` может не менять пользовательского решения. Ошибка не стирает весь checkpoint и не доказывает провал продолжения LoRA. Она относится к локальной feature/correction памяти и частоте cold restart.

### Маленький будущий A/B

Один допустимый начальный checkpoint и **два одинаковых разрешённых урока в каждом isolated diagnostic arm**. A — warm; B — current cold между уроками; C — предложенная сохранённая сумма/residual при неизменных остальных полях. Упаковка C должна иметь явную новую версию/совместимую миграцию; старые округлённые checkpoint не содержат потерянного остатка, его нельзя объявлять восстановленным из воздуха.

Сначала сравнить сам native accumulator. Значение .250002 — artificial contribution; штатные category deltas обычно ±1, а vector mean ordinary Critic не читает. Предметную фазу запускать только при найденном реально изменяемом и читаемом весе. Тогда сравнить четыре обычных входа, реально читающих затронутую correction feature. Если все четырe outputs одинаковы, verdict — `STATE_CONTINUATION_DRIFT_CONFIRMED / QUALITY_EFFECT_NOT_OBSERVED`. Подставленный искусственный tie-break не считать общей предметной пользой. Во время этой работы native/.NET обучение **не выполнялось**.

## 6. Дополнение для главы восстановления DesignToCode-clean: прямой policy scorer сменил вход

Это **source difference и контроль восстановления, не пятая новая гипотеза**.

В clean [Machine, 3728–3740](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/44.DesignConversionDomain/DesignConversionMachine.cs#L3728-L3740) direct scorer строил `preferred` с score=1 и `baseline` из текущего QA verdict. Это сравнение синтетических описаний состояния. Current [3865–3904](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L3865-L3904) требует точную pair, runtime encoding и реальные generated/incumbent HTML/CSS.

Новая pair переносится во внутреннем, не JSON поле [RoutingFlow, 11–24](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionRoutingFlow.cs#L11-L24). [Coordinator, 1279–1388](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionImprovementLoopCoordinator.cs#L1279-L1388) создаёт её только при наличии teacher-authored candidate, независимого evidence source/receipt и runtime evaluator. При отсутствии pair [4154–4175](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L4154-L4175) возвращается baseline. Это происходит после выбора policy-ветки и до local weight scoring, [4092–4108](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L4092-L4108).

Поэтому restore-проверка должна отдельно перечислять **loaded policy**, **valid runtime pair**, **actual policy forward**, **local feature fallback**, **final candidate accepted**. Успешный load сам по себе не доказывает достижимость прежнего scorer. Старую synthetic-label ветку не следует автоматически переносить ради совпадения старого score; восстановление полезной способности требует реального сравнения кандидатов через текущий разрешённый coordinator.

Мини-проверка: frozen policy и два реальных generated/incumbent comparisons; четыре readouts — clean historical score, current ordinary input без pair, current coordinator pair, current scaffold control. Это **разные измеряемые семантики**, а не four-arm accuracy benchmark. Новое обучение не нужно. Если coordinator path уже всегда подаёт валидную pair, гипотеза потери через отсутствие pair для этого consumer закрывается.

## 7. Дедупликация, роли и требования независимой проверки

Таблица ниже описывает первоначальный корпус из 689 Markdown. Решение после свежих публикаций N54 приведено в начале главы и имеет приоритет в вопросе новизны.

Оба прежних реестра прочитаны через общий SHA-verified corpus root: `xiigen-planning/.../possibleDifs` и read-only `llm_mvp_core/docs/plans/replanning-possible-diffs`. Ближайшие прежние находки:

| Прежний документ | Что в нём уже было | Что здесь добавлено |
|---|---|---|
| [P123](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/plans/replanning-possible-diffs/experimets/cards/P123-design-conversion-critic-evidence.md) | Scaffold QA aggregator; actual learned wrapper и его update/render contract условны | Найдены реальный wrapper, admitted caller, точная содержательная потеря и evaluator/runtime projection |
| [N52-02](https://github.com/lubochka/xiigen-planning/blob/b0308fbaa783758104987e6544d057062ccf6fc3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/52-ORCHESTRATOR-NEW-GAPS-AND-EVIDENCE-2026-10-19.md) | ObservationCount не доказывает изменение weights/quality; пример неизменного Weight теоретический | Найден конкретный roundtrip-код, способный потерять остаток и подавить последующие малые updates |
| N52-01/03 | Cap10 probe и старый публичный caller требуют отдельной проверки | Здесь не повторяются; действующий TrainingPipeline уже использует admitted API |
| Прежние translation negative/state findings | Потеря отрицательных уроков, область правил и cold restore в других владельцах состояния | Не объявляются новыми; U-DTC-2 — другой точный consumer, который после `Weight>0` выполняет отрицательно оценённое CSS-правило |

Для будущих содержательных DPO **CODEX с `astra 6 ultracode` выполняет человеческий выбор урока и отдельную независимую оценку качества обеих сторон; внешняя открытая китайская модель только создаёт chosen/rejected**. Эта работа не вызвала ни teacher, ни модель арбитра и не создаёт фиктивных отметок approval.

Применены относящиеся к задаче требования навыков phase-planning / plan-review: свежая source-bound карта получателя, явное разделение исторического успеха/исходной формулы/native исполнения/предметного эффекта, один меняемый механизм, заранее указанный falsifier и проверка обычного потребителя. Общие требования архитектуры и human communication: называть конкретный state owner и объяснять пользу пользователю; не называть KNN/Unit новым доменом и не объявлять счётчик артефактов обучением. Текущая работа — research packet, не readiness-вердикт всей программы.

**Независимому reviewer:** проверить U-DTC-1a по deltas и gradient request; U-DTC-1b по обеим проекциям и оговорке feature-fit; U-DTC-2 по `+1/−1`→wrapper→fallback→CSS; U-DTC-3 по `ToCheckpoint/FromCheckpoint` и native rounding limitation. Отклонить любое утверждение о confirmed RP01 cause, visual gain, полностью одинаковых Machine methods или вызванном CODEX UltraCode. Сверить P123/N52 по полному смыслу, а не одному поисковому совпадению.

## 8. Воспроизведение только арифметического приложения

Рядом нужны `dtc_source_probe.py` и `source-manifest.json`. В существующем локальном clone с обоими закреплёнными commit:

```bash
python evidence/units/dtc_source_probe.py --repo /path/to/llm_mvp_core
```

Скрипт вызывает только `git show` для трёх исходников, проверяет Git blob SHA и считает artificial fixtures. При локально загруженных snapshot в `src/current`/`src/clean` параметр `--repo` не нужен. Выход — [dtc-source-probe-result.json](evidence/units/dtc-source-probe-result.json). Это не C# тест, не DPO update и не измерение качества UI.
