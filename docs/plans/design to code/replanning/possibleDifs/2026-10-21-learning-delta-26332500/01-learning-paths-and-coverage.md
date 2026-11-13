# Где мог потеряться учебный эффект: карта механизмов и доказательств

Срез: core `26332500f76ea8345256c73ecfd44c0023b7e883`, planning `14308ff181c5a93d837d92b85b2f35c41b863565`, 21 октября 2026. Это адресная проверка исходников, сохранённых результатов и документов. Нового обучения в этой сессии не было.

## Главный вывод

Нет одного подтверждённого объяснения всех неудач RP01. Найденные различия принадлежат нескольким разным путям обучения. Нынешний exact DPO действительно доходит до q/v-LoRA; старые успехи kNN, POS-памяти, таблиц переходов и переводческих Units не доказывают, что та же функция присутствует в этом Student. Сравнивать нужно конкретный приобретаемый навык, сохраняемое состояние и получателя результата. Иначе улучшение выбора шаблона легко принять за способность генерировать ответ, а потерю направления в исторической матрице — за непригодность её признаков для kNN.

Новые узкие гипотезы этого пакета: **HIERARCHICAL-LOCAL-HISTORY-01**, **SVD-DIRECTED-TARGET-01**, **ATTN-CAUSAL-SCORES-01**. Первые две могут изменить интерпретацию старых алгоритмов; третья проверяет стоимость настоящего нейронного обучения. Причинное объяснение провала RP01 для них пока не доказано. [Исторические первичные доказательства](historical/HISTORICAL-PPMI-HIERARCHY-FINDINGS.md), [нейронный путь](neural/01-neural-causal-score-audit.md).

## 1. Три пути, которые нельзя оценивать одной метрикой

| Путь | Что получает урок | Что действительно меняется | Где читается результат | Что доказывает полезный результат |
|---|---|---|---|---|
| RP01 exact DPO | Сохранённые prompt/chosen/rejected, tokenizer snapshot, response masks и веса | Разрешённые q/v-LoRA A/B через общий AdamW step | Тот же Student: score и свободный ответ из фактического итогового checkpoint | Score показывает предпочтение; правильный ответ, удержание навыка и цена шага — отдельные показатели |
| Структурированный перевод | Пара → builder → отдельные уроки tokenizer/executor/verifier | Память и правила соответствующего Unit | Обычный перевод/Verify после Train, включая restore | Правильный перевод или меньше ложных решений verifier; одновременно считать противоположную ошибку |
| Исторические kNN/таблицы/классификаторы | Признаки/пары/последовательности конкретной темы | Соседи, счётчики, локальные таблицы, веса классификатора | Тот же classifier/predictor на неизменённой панели | Улучшение конкретного выбора или удержания; оно не переносится автоматически на свободную генерацию нейросети |

Первичный exact DPO: [StepVector и optimizer, строки 864–923][dpo-step], [роль teaching vector, строки 1160–1166][vector]. Первичный переводческий путь: [builder][builder], [Units][units]. Исторические пути и сохранённые успехи разобраны с отдельными source/run ссылками в [историческом отчёте](historical/HISTORICAL-PPMI-HIERARCHY-FINDINGS.md).

## 2. Покрытие вопросов пользователя

Ниже перечислены существующие владельцы гипотез. Наличие P-карточки означает документное покрытие вопроса, а не доказанный эффект и не готовый runnable host. P01–P125 — семейства; единственная действующая очередь старых назначений — [канонический индекс][index].

| Ось проверки | Различие или риск | Доказательства и существующее покрытие | Что остаётся открытым / граница домена |
|---|---|---|---|
| Качество DPO | Содержательный контраст chosen/rejected, общий префикс, дубли вопросов, корректность gold и учителя; annotation вроде `[WRONG:tense]` может не быть настоящим отвергнутым ответом | [Разбор C#500/ENRU500][data]; P04/P14/P66/P81/P109/P112. Реальный N60-E1 показывает, как annotation превращается в ложный verifier-урок: [required-changes][changes], [builder:128–141][verifier-builder] | Контроль целостности файла не доказывает смысловую независимость примеров. Проверять один закон одной темы; не смешивать несколько предметов ради объёма |
| LoRA и область обучения | Старый head-only/full-parameter опыт, текущие q/v-only adapters, случайная база и готовая база — разные постановки | [Сопоставление DPO/LoRA][dpo-state]; [current gradient chain][gradient]; P01/P02/P10/P11/P101/P105 | Изменение адаптеров доказано сохранёнными отчётами; правильность составной производной и достаточность этой параметризации для нужного навыка — отдельные вопросы. Нет нового доказательства, что LoRA «вообще не подключена» |
| Векторизация и BPE | Exact byte-BPE сохраняет prompt/response иначе, чем старый hashed-word runner. Вектор преподавания в текущем DPO сворачивается в число активных координат | [Exact material][material], [VectorSignal][vector], [BPE identity][bpe-identity]; P06/P07/P16/P17/P18/P98/P99/P122 | Semantic vector не превращается автоматически в обучаемую функцию расстояния или отдельный Unit. BPE cost/identity и качество конечного ответа измеряются раздельно |
| kNN и предметные признаки | AST, тип узла, scope и следующий statement полезны для кода; POS, порядок и контекст слова — для языка. Нельзя считать их универсальным улучшением | [POS donor][pos], [structural kNN transfer][structural], [structural/source lineage][historical-scope]; P26/P27/P28/P38/P43/P44/P45/P79/P82/P83/P84/P85/P91 | Положительный E09 raw имеет разные панели baseline/kNN; это требует одинаковых query IDs. E25 действительно обучает классификатор; E26 BuildDefault в данном runner не приобретает новое состояние |
| Чтение исторически обученного состояния | Направленная PPMI читается через U/U вместо U/V; локальный ID чужого кластера подаётся в target-cluster bigram | Новые [HIERARCHICAL-LOCAL-HISTORY-01](historical/HIERARCHICAL-LOCAL-HISTORY-01.md), [SVD-DIRECTED-TARGET-01](historical/SVD-DIRECTED-TARGET-01.md) | В защищённых карточках этот причинный фактор не найден. Наследован в соседних ветках; связи с RP01 recipient не установлено. Это кандидаты на восстановление и переоценку старого обучения |
| Восстановление модели/памяти | Веса, tokenizer, adapter topology, AdamW moments/step, RNG, индекс и адрес урока должны соответствовать друг другу | [State identity][state-identity], [HNSW continuation][hnsw-state], [cold restore/learned memory][restore-memory]; P17/P18/P22/P25/P59/P67/P121; E-SFT-MOMENT-CONTINUITY | Текущие C50 continuation receipts опровергают всеобщую версию «ничего не восстановилось», но не доказывают правильность любого owner. У N63C остаётся source/binary custody, см. аудит результатов |
| DPO → под-юниты | Часть смыслового сигнала может исчезнуть в семантической разметке, phrase mappings, required terms или при выборе роли Unit | [Domain/subunits][subunits], [roles/keys/alignment][roles], [builder][builder]; P51–P56/P92/P106/P108/P125. Translation-memory-ingress и N60-E1 уже имеют измеренные результаты | Прямое исправление конца Flow нельзя приписывать обучению tokenizer. В уроке фиксировать producer, изменённое состояние и конкретный consumer; измерять before/after после настоящего Train |
| Сами Units/Flow и детерминистические обходы | Trained router может не использоваться обычным overload; rule/classifier может выдать PASS без обучения; learned output может стать только style/context metadata | [Reachability/context][reachability], [routing boundaries][routing], [unit metrics][unit-metrics]; P33/P39/P40/P42/P58/P88/P92/P94/P95/P97/P111/P118 | StackAndVersion self-oracle вынесен в приложение без новой учебной карточки: фактический fit не найден. Проверку одного regex нельзя считать экспериментом обучения |
| Цена обучения и бюджет | Лишние transpose/copies/exp/feature recomputation уменьшают доступный бюджет; нулевой acquisition не становится полезным учебным результатом из-за изменения margin | [Текущие учебные ремонты][repairs], [контракт валидности][validity]. Новый [ATTN-CAUSAL-SCORES-01](neural/ATTN-CAUSAL-SCORES-01.md) | Любая измеренная экономия полезного ресурса — успех по этому ресурсу при выполненном учебном контуре; ускорение не заявляется до измерения. Для новой карточки требуются ненулевой trained S0, несколько классов и сохранение функции |

## 3. Что больше нельзя повторять как текущую неисправность

1. **«LoRA не участвует в текущем RP01».** Старые recon-документы действительно описывали более ранние разрывы. Нынешний caller, recipient и backward отличаются; сохранённые результаты показывают изменения разрешённых адаптеров. Это не означает, что получен полезный свободный ответ. [Текущий путь][gradient], [границы старого сравнения][dpo-state].
2. **«Все нынешние входы всё ещё hash/обрезанные слова старого runner».** Simulation caller требует exact material с сохранённым BPE и response mask. Legacy-ветка в том же репозитории не доказывает её использование этим caller. [Material][material], [сопоставление][dpo-state].
3. **«13 сохранённых попыток = 13 закрытых учебных целей» или «50 пунктов = 50 готовых экспериментов».** Это разные состояния. Current registry содержит 13 saved/partial attempts и 37 future designs. Подробная сверка — [аудит](audit/README.md).
4. **«WIKI-MARGIN обучил различение маршрутов».** B повторяет единственную TRAIN-метку на всей EXAM панели; 2/4 совпадает с постоянным ответом. Число правильных строк сохраняется как факт, но способность различать маршруты не показана. [Исправление состава данных и вывода][dataset-corrections].
5. **«Отсутствующий лог доказывает, что процесс не запустился».** Нужны живой handle/status и корректное ожидание. Поздний сбой summary не обнуляет уже завершённые arms; N60-E1 — конкретный пример. [Контракт ожидания][wait], [аудит](audit/README.md).

## 4. Как использовать родительские RAG-документы

RAG и реконструкции — карта намерений и вход в первичные ссылки. Их утверждения привязаны ко времени. В этом проходе дополнительно прочитаны адресные материалы из `llm mvp/dpo legal build`, `compare branches`, `Small model racional` и `local wiki learning`: DPO measurement guide, ILeaningMachine/ILearningDynamicGraph notes, старые train-loop/LoRA/experiment recon, rationale малой модели и протоколы Wiki. [Опись прочитанных родительских файлов](sources-and-scope.json).

Сильный ориентир этих материалов сохранён: сравнение должно показывать, какой конкретно механизм приобрёл навык и какой результат потребляет обычный Flow. Но старую фразу «Apply не подключён» нельзя переносить через изменения live LoRA graph; старый PASS нельзя расширять с выбора шаблона на свободную генерацию. Поэтому каждый новый вывод этого пакета подтверждён текущим кодом, историческим pinned кодом либо сохранённым raw-result, а неподтверждённые мосты названы открытыми.

Не заявляется полный смысловой разбор всех RAG, всех 976 Markdown `possibleDifs`, всех веток и всех 171 карточек. Проведены полный адресный/структурный поиск в указанном корпусе, механическая сверка текущего реестра и углублённое чтение перечисленных механизмов. Эта граница оставляет место дальнейшим гипотезам; она не превращает отсутствие совпадения строки в доказательство абсолютной новизны.

[dpo-step]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/11.DPO/DpoTrainer.cs#L864-L923
[vector]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/11.DPO/DpoTrainer.cs#L1160-L1166
[builder]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/27.TranslationDomain/TranslationStructuredBatchBuilder.cs
[verifier-builder]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L128-L141
[units]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/27.TranslationDomain/TranslationTrainedUnits.cs
[index]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md
[data]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/14-data-quality-and-evaluation-alignment.md
[changes]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/required-changes.md
[dpo-state]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/02-dpo-lora-tokenization-and-state.md
[gradient]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/41-current-dpo-gradient-chain-source-review.md
[material]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/11.DPO/Models/DpoExactPairMaterial.cs
[bpe-identity]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/12-bpe-retrain-identity-and-consumer-causal-gap.md
[pos]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/17-positive-pos-knn-versus-production-rules.md
[structural]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/38-e09-transfer-labels-and-opt-in-consumer.md
[historical-scope]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/50-historical-e05-e20-e21-source-lineage.md
[state-identity]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/19-unit-state-identity-and-partial-learning.md
[hnsw-state]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/63-hnsw-random-state-and-continuation-consumers.md
[restore-memory]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/78-translation-restored-memory-positive-receipt.md
[subunits]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/13-domain-context-and-dpo-subunits.md
[roles]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/44-structured-lessons-roles-keys-and-word-alignment.md
[reachability]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/45-memory-reachability-and-context-consumption.md
[routing]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/26-routing-profiles-and-generated-flow-boundary.md
[unit-metrics]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/58-unit-metrics-and-generation-consumer-boundaries.md
[repairs]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/2026-10-21-14308-training-repairs.md
[validity]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/30-failure-recovery-and-learning-validity.md
[wait]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/31-tool-discovery-and-wait.md
[dataset-corrections]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/2026-10-21-training-set-corrections.md
