# Два дополнительных механизма стоимости реального обучения: BPE и exact DPO

**Статус: SOURCE_CONFIRMED / DESIGNED_NOT_RUN.** Найдены два конкретных native механизма, для которых в прочитанном прежнем корпусе не обнаружена такая же экспериментальная гипотеза. Они могут влиять на цену конечного учебного перехода. Влияние на accuracy, причина неудачи первого PR и измеренный выигрыш **не установлены**. Данные новых опытов отсутствуют; оригинальный код и защищённый архив не менялись.

| Новый кандидат | Что происходит сейчас | Что проверит малый A/B | Статус покрытия защищёнными карточками |
|---|---|---|---|
| [BPE-WEIGHTED-CHUNKS-01](BPE-WEIGHTED-CHUNKS-01.md) | BpeTokenizer.Train заново считает и сливает каждый одинаковый regex chunk при каждой итерации | Один список на точную строку + multiplicity, с сохранением исходного tie order. Четыре native Train: A/B на C# и EN→RU материалах | Точного опыта frequency-preserving chunk folding не найдено; прежний BPE-NO-MATCH-COPY-01 меняет другой расход |
| [DPO-TOKENIZER-SNAPSHOT-01](DPO-TOKENIZER-SNAPSHOT-01.md) | Для каждой exact pair полный Save→Load→canonical hash одного tokenizer | Один private lazy frozen snapshot на одно PrepareExecution; один настоящий StepVector с двумя парами в каждом arm | Точного опыта tokenizer snapshot reuse внутри preparation не найдено; N62 оптимизирует другие части update |

Это два независимых опыта. Ни результаты других агентов, ни их новые заявки не заменяются. Их positive candidates должны сохранять свой статус по реально измеренным метрикам.

## 1. Частотная свёртка одинаковых BPE чанков

**Сейчас.** [Train:53–66](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/01.Tokenizer/BpeTokenizer.cs#L53-L66) создаёт отдельный список на каждое вхождение. [CountPairs/MergeAll:526–545](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/01.Tokenizer/BpeTokenizer.cs#L526-L545) повторяют одинаковую работу. Эти процедуры действительно записывают learned merges/vocabulary, а не только готовят отчёт.

**Почему это кандидат.** Если «return» встречается много раз в одинаковом regex chunk, состояние каждого экземпляра после одинаковой последовательности merges совпадает. Можно обновлять одного представителя, но при подсчёте пар учитывать его исходную кратность. Это должно сохранять частоты и сэкономить часть allocations/CPU. Доказательство эквивалентности требует first-occurrence order: [MaxBy:63](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/01.Tokenizer/BpeTokenizer.cs#L63) разрешает равные счётчики через порядок перечисления. Недостаточно просто сделать Distinct.

**Где участвует.** [KnnLogicalUnit.Train:136–153](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L136-L153) обучает тот же BPE перед построением векторов учебных примеров. [DefaultLogicalCapabilityTrainer.BuildTokenizer:4054–4061](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L4054-L4061) обучает его для новой teaching stack. Следовательно, расход находится в реальном учебном writer. Наличие caller не доказывает, что этот этап доминировал во времени конкретного RP01 run.

**Будущее исправление после решения Любы.** Train/CountPairs/MergeAll указанного BpeTokenizer.cs; закрытые локальные WeightedChunk, Ordinal equality, multiplicity и первое появление. Encode/MergeChunk/pair objective/regex не меняются. Отдельный [source-copy design](bpe-source-copy-design.md) задаёт точное преобразование, project/CLI, передаваемые файлы и отказ при конфликте.

**Доменные пределы.** Измерение относится к обучению tokenizer на повторяющихся C# и EN→RU материалах. На почти уникальном корпусе дополнительный словарь может проиграть. Нельзя из token round-trip заключить улучшение перевода, AST retrieval или полного KNN pipeline. AST/порядок предложения не являются фактором этого опыта.

## 2. Повторная полная копия tokenizer на каждую DPO pair

**Сейчас.** [DpoExactPairMaterial.CreateBatch:64–90](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/11.DPO/Models/DpoExactPairMaterial.cs#L64-L90) делает Save, Load и ComputeStateSha256 для одной пары. [PrepareExecution:1686–1745](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L1686-L1745) вызывает его по каждой строке. Snapshot — полезная защита от изменения исходного tokenizer, поэтому удаление защиты не является оптимизацией этого кандидата.

**Почему это кандидат.** При одном owner-exclusive tokenizer две пары могут кодироваться через один private frozen clone с сохранённым hash. Hash/masks/admission каждого примера остаются собственными; повторяется только не нужный второй capture. Экономия может быть мала относительно forward/backward, но useful materialization allocations/CPU — самостоятельная ось успеха.

**Где участвует.** [Exact materializer:14–20](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/18.CapabilityTraining/LogicalDpoPreparedBatchMaterializer.cs#L14-L20) явно выбирает exact version; [реальный owner:106–125](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L106-L125) получает его batches; [тот же owner:175–189](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L175-L189) передаёт их в реальный StepVector. В generic PrepareExecution default остаётся legacy; утверждение «вся DPO-токенизация exact» было бы ошибкой.

**Важное ограничение исправления.** [BuildBatch:2366–2380](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L2366-L2380) сначала выдаёт typed admission и вызывает internal test observer. Freeze до цикла менял бы порядок guard. Дизайн использует lazy capture после первого допустимого текста; при активном observer остаётся per-pair путь, а invocation snapshot сбрасывается. Arbitrary concurrent mutation внешнего tokenizer не покрыта equivalence утверждением. Persistent/global cache запрещён.

**Будущее исправление после решения Любы.** Два source methods — DpoExactPairMaterial.CreateBatch и private BuildBatch/PrepareExecution в CapabilityTraining. Полные копии DPO/CapabilityTraining/ContinuousLearning закрывают настоящие внутренние типы и issuer boundary. [Design](dpo-source-copy-design.md) не подменяет эту границу reflection или новым mutation permit.

**Доменные пределы.** Первое измерение только C# documentation, saved B0. При одной паре второй capture отсутствует; legacy route не затрагивается. Отдельного подтверждения для translation/SQL/design-to-code здесь нет, хотя generic механизм потенциально применим.

## 3. Дедупликация и отвергнутые как новые кандидаты

Проверен полный загруженный Markdown корпус исходной точки: **678 possibleDifs +301 protected =979 файлов**. Planning исходного корпуса — `f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9`, core защищённых документов — `ab583032562d6656de9dd5ca6fa0a325fd4b1f30`. Сверены точные названия/API и фразы о tokenizer capture/serialization, repeated/weighted chunks, multiplicity, CountPairs; смыслово прочитаны ближайшие найденные совпадения. Дополнительно прочитан поздний пакет `2026-10-20-learning-candidates-and-causal-gaps` на planning `a868f46ee3695728406173f8cd945e278ddb0def`, включая new-gradb-locality-card, knn-cost-card и method-memory-card. Exact overlap двух кандидатов не найден.

Это поиск по полному корпусу с адресным чтением ближайших механизмов, **не полный построчный ручной аудит всех 979 документов** и не утверждение абсолютной новизны алгоритмов. Все source выводы привязаны к native commit `ab583032562d6656de9dd5ca6fa0a325fd4b1f30`. Поздний core `65d03a73a76810a0425a41c33d3c7eeaf4cdbc48` проверен общим аудитом как документная delta; эта подзадача не заявляет новый native запуск на нём. Поздние документные уточнения не являются новыми выполненными learning runs.

| Рассмотренная идея | Почему не регистрируется заново |
|---|---|
| Косинусный LR после TotalSteps снова растёт | Уже есть clamp/unclamped horizon опыт: protected [09-additions-44-48.md](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/09-additions-44-48.md). Достижимость actual continuation сверх horizon отдельно не доказана. |
| Нулевой первый warmup LR, reset schedule, overflow grad norm | Уже P12 и [56-autograd-reachability-and-sft-caller-controls.md](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/56-autograd-reachability-and-sft-caller-controls.md); production callers могут переопределять WarmupSteps. |
| Seed512 вытесняет полезные BPE merges | Уже [13-additions-feedback-bpe.md](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/13-additions-feedback-bpe.md). Наш corpus и статистические веса сохраняются. |
| BPE allocation при no-match MergeChunk | Уже [BPE-NO-MATCH-COPY-01](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/BPE-NO-MATCH-COPY-01.md). Наш B оставляет MergeChunk прежним. |
| Tie order BPE меняет token IDs | Уже [12-bpe-retrain-identity-and-consumer-causal-gap.md](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/12-bpe-retrain-identity-and-consumer-causal-gap.md). Здесь tie order — сохраняемый инвариант, не новый фактор. |
| Reuse frozen-reference scores; softmax scratch; optimizer-state export; gradient-copy overhead | Уже N61/N62 и [N62-neural-update-costs.md](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md). Новый snapshot-кандидат меняет другой объект/фазу. |
| Локальность gradB, один parse C# source, sparse MethodKnn replacement | Поздний planning packet прямо добавляет эти идеи. Наши две карточки их не повторяют и не объявляют новым prior art. |

Обнаруженное отсутствие точной карточки в защищённой папке отражено как **coverage gap**; запрещённая пользователем папка не дополнена. Добавлены ровно две самостоятельные карточки в разрешённый possibleDifs packet. Встраивание этих кандидатов в protected collection решается отдельно, а не скрытым write.

## 4. Что готово и чего нет

Готовы source evidence с pin/lines, literal fixtures, измерительный протокол до/после, конечный бюджет, отдельные executor prompts, короткие Windows paths, source-copy manifest с108+2 исходными blob identities и точный design будущих проектов.

- [BPE prompt](prompts/BPE-WEIGHTED-CHUNKS-01.md): четыре native BPE fit, никакой нейросети.
- [DPO prompt](prompts/DPO-TOKENIZER-SNAPSHOT-01.md): одна Prepare(two) + один StepVector на arm, saved B0/реальные guards.
- [Source-copy manifest](source-copy-manifest.json): исходные Git blob SHA1 из pinned repository tree; fixture SHA256 относятся к **добавленным literal files**, не к неизвлечённым бинарным моделям.

**Не созданы** C# runners, экспериментальные libraries, build outputs, trained checkpoints, metrics reports или positive-result receipts. Новые статусы не READY_TO_RUN. Никакие native тесты, model calls или training здесь не запускались.

Критерий будущего результата одинаков: любой измеренный useful gain считается успехом по своей метрике/теме, остальные показатели и регрессии рядом. До реального измерения это candidate hypothesis, не «удачное исправление». Чтение после direct NEXT/STOP не разрешает продолжать старую карточку; это правило встроено отдельно в каждую карточку и каждый prompt.

