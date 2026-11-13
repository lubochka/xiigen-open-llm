# Один длинный исходник останавливает обучение всего домена; исправленный ответ может уступать место старому исходнику

Дата: 2026-10-20. Статус: **проверено по исходникам, эффект обучения ещё не измерен**. Core: `ce890a48efda7f00d0ee040f2383cc2d06428ea6`. Корпус прежних findings: planning `de270758087502a350c4fcf460acdf1638bdd318`. Защищённая папка `docs/plans/replanning-possible-diffs` использована только для чтения.

## Что добавляет этот разбор

| ID | Вывод | Новизна | Приоритет и граница |
|---|---|---|---|
| INGRESS-LENGTH-01 | Слишком длинный **supervised исходник** прекращает весь `IncrementalTrainer` до первого SFT и до DPO, даже если остальные примеры и DPO-пары помещаются в контекст. | Новая отдельно прослеженная комбинация producer → domain batch → ранний отказ; точное прежнее описание в просмотренном корпусе не найдено. Общая проблема длины контекста давно известна. | P0 **внутри вызванного continuous-learning пути**. Не доказанная причина direct RP01 simulation. |
| CORRECTION-COLLISION-01 | После Edited feedback исправленный положительный ответ записывается в cache, но этот pipeline обучает SFT на `indexResult`, а не на содержимом cache. Если индексированный исходник равен rejected, после корректирующего DPO SFT снова усиливает rejected. | **Уточнение** известного ND-DPO-SFT/P87: точное условие совпадения, видимость исправления и правило выбора target. Общий порядок DPO→SFT новым не считается. | После снятия блокеров реальных updates. Только при подтверждённом совпадении input и rejected source; реальная распространённость такого совпадения не измерена. |

На этом срезе нельзя считать доказанным, что любой из этих механизмов ухудшил уже полученные RP01 результаты. У обоих есть конкретный потребитель обучения; перенос на другой вызов требует его собственного trace.

## INGRESS-LENGTH-01: цепочка доказательств

1. `ProjectIndexer` перебирает `.cs`, читает **весь** файл и создаёт `TrainingExample` с prompt `// file: <basename>` и response, равным полному исходнику. Разбиение на методы или окна в этой реализации отсутствует. [ProjectIndexer.cs, 42–78][indexer]
2. `ContinuousLearningPipeline.RunCycleAsync` получает индекс, добавляет его в cache, выводит список доменов из индекса, затем снова берёт `domainExamples` непосредственно из `indexResult`. Поэтому длинный файл попадает в один batch со всеми остальными файлами того же домена. [Pipeline, 57–88 и 169–180][pipeline]
3. В `IncrementalTrainer` до применения LoRA и до обучения кодируется `Prompt + "\n" + Response`. При длине больше `ContextLen + 1` возвращается `TRAINING_EXAMPLE_CONTEXT_EXCEEDED`. Это **возврат из всего TrainCoreAsync**, а не отложенный отдельный row. [IncrementalTrainer, 385–418][length]
4. Ниже расположены `LoraApplicator.Apply`, DPO executor и затем реальный supervised executor. Следовательно, отказ на шаге 3 прекращает все эти последующие действия в данном вызове. Это source consequence, не измеренная нами потеря DPO-качества. [IncrementalTrainer, 502–550, 581–672][updates]
5. Supervised executor действительно вызывает `Trainer.TrainSupervisedNextTokenStep`, где выполняются forward, cross-entropy, backward и AdamW. Это не проверка отчётных полей или только валидатора. [SupervisedTrainingExecution, 19–31][executor]; [Trainer, 102–147][sft]

### Почему это может повлиять на качество

Если хотя бы один такой файл оказывается в активном домене, полезные короткие уроки не создают обновления параметров. Повторный запуск с тем же неразбитым источником воспроизводит блокировку. Предполагаемое улучшение — доставить ограниченную часть законных примеров до реального update, сохранив длинные материалы в явном списке отложенных. Это гипотеза **доступности обучения и покрытия**, а не предложение убрать проверку длины или молча обрезать код.

Качество длинных исходников такой ремонт само по себе не улучшает. Отчёт обязан показывать числители `admitted / deferred / failed / actually_updated`, общее число входных исходников и токенов, а также метрики на отложенном материале, где вычисление определено. Нельзя писать «обучен весь домен», если обучена лишь короткая часть. Малый опыт полностью задан в [карточке INGRESS-LENGTH-01](02-INGRESS-LENGTH-EXPERIMENT.md).

### Достижимость и пределы

`ContinuousLearningPipeline` имеет настоящий вызов `TrainAsync`/`TrainTransactionAsync`, а публичный `IncrementalTrainer(storage, factory, directory, config)` создаёт настоящий SFT исполнитель. Публичный `CreateTp1Trainer` overload с server-owned capability также собирает IncrementalTrainer. Вместе с тем materialized startup использует специальный неактивный guard; нельзя объявить всю production deployment цепь активной лишь по наличию класса. [Pipeline, 195–253][dispatch]; [IncrementalTrainer, 54–68][public-trainer]; [Composition, 254–287 и 350–405][composition]

У первого опыта нет DPO-пар: он обращается к публичному SFT пути с пустым `dpoPairs` и реальным optimizer. Так закрывается именно реальная обучающая часть без подделки teacher provenance или transaction authority. Последствия для корректно допущенного DPO подтверждены порядком исходника; они **не объявляются экспериментально измеренными**. Direct RP01 simulation вызывает свой DPO runner и не содержит следующего indexed-file SFT; эта гипотеза не назначается общей причиной всех RP01 неудач. [Предыдущее разграничение ND-DPO-SFT, строки 55–88][prior-order]

## CORRECTION-COLLISION-01: более узкое условие, чем общее «DPO и SFT конфликтуют»

`FeedbackRouter.RouteEdited` строит chosen=`UserCorrection`, rejected=`ModelResponse`, сохраняет DPO-пару и добавляет исправление как положительный `TrainingExample` в cache. [FeedbackRouter, 130–173][feedback] Но данный `RunCycleAsync` не читает положительные cache-примеры для активного `domainExamples`: берёт только `indexResult`. [Pipeline, 63–88, 175–180][pipeline]

Условие проверяемого столкновения:

```text
pair.Domain == example.Domain
pair.Prompt == example.Prompt
pair.Rejected == example.Response
pair.Chosen != pair.Rejected
```

Сравнение — ordinal точных строк, плюс записываются UTF-8 SHA-256 и фактические encoded IDs. Только совпадение ответов без совпадения input недостаточно для сильного утверждения «тот же conditional target». Если условие выполнено, после DPO выполняется SFT на том же rejected под тем же prompt. Это две противоположные цели для одного условного ответа; насколько итоговая LoRA их компенсирует, покажет только измерение непосредственно после каждого этапа. [IncrementalTrainer, 402–418 и 581–672][updates]

**Это не запрет SFT.** Если исходник уже исправлен либо supervised пример относится к другой задаче, эффект может отсутствовать или быть полезным. Упорядочивание стадий уже исследуется ND-DPO-SFT. Здесь обе руки сохраняют DPO→SFT, одинаковые оптимизаторы и число шагов; меняется только происхождение одного совпавшего supervised target. Отдельная [карточка refinement](03-CORRECTION-COLLISION-REFINEMENT.md) не требует смешивать stage-order и target-selection.

Просто подключить `_cache.GetByDomain` тоже недостаточно: создаваемый feedback-пример не задаёт `SourceId`, тогда как IncrementalTrainer требует canonical provenance через `BuildManifest`. Нужно явное происхождение исходного rejected и исправленного source, а не принудительная отмена проверки. [FeedbackRouter, 171–173][feedback]; [TrainingProvenance, 71–135][provenance]; [IncrementalTrainer, 348–385][manifest]

## Сверка с прежними исследованиями и защищёнными экспериментами

| Деталь | Существующее покрытие | Оценка нынешнего покрытия |
|---|---|---|
| DPO→SFT на одном recipient | ND-DPO-SFT; protected `23-additions-n60-small-learning.md`, 175–185; P87 mixed objectives | Известно. Эксперимент меняет порядок; не изолирует выбор устаревшего rejected как SFT target. |
| Full-sequence SFT prompt targets | ND-SFT-MASK/MINI01; protected тот же файл, 135–172 | Известно. Не смешивать response mask с новым admission опытом. |
| Filename-only input | package51 review, строка 55; package53 review, строка 45 | Известно. Не включено в новизну. |
| Один overlength supervised source блокирует даже короткие уроки | Поиск `TRAINING_EXAMPLE_CONTEXT_EXCEEDED`, `ProjectIndexer`, `IncrementalTrainer`, `overlength`, `indexResult`, русские варианты | Отдельной карточки с этой producer→batch→ранний отказ цепью и real before/after update не найдено. Новая карточка прилагается; в защищённой папке её сейчас **нет**, папка не изменялась. |
| Corrected cache положительный ответ не становится активным SFT target при exact rejected collision | Прежний ND-DPO-SFT, P59 correction visibility, P87 | Общие темы известны; не найдена точная строковая фикстура и отдельное вмешательство только в target при сохранённом порядке. Маркировка refinement. |
| Cold capability restore теряет все KNN examples | Старые restore-гипотезы/P25 | На прочитанном текущем loader восстанавливаются manifest KnnExamples и вызывается Train; старую потерю корпуса не заявлять текущим дефектом. [Loader, 1605–1723][restore] |
| Exact DPO якобы применяется всем callers | Текущий request default legacy; ND-DPO-SFT уже фиксирует различие | Нельзя подменить literal production baseline exact-v2 исследовательским host без маркировки нового профиля. |
| Поздний protected N62-training-context | Core `67abfabe6def8a002984be80b999824601ad273c`, `N62-training-context.md`: POS-массивы при AddEntry → ContextualDictionary EN→RU | Прочитан полный поздний документ. Это другой writer, state и reader; INGRESS-LENGTH-01 не дублирует его. |

Соседние старые P07/P68/P98 относятся соответственно к model-visible DPO material, pretraining source/window pipeline и tokenizer capacity. Они не измеряют конкретный отказ **всего IncrementalTrainer** из-за длинного supervised example перед допустимыми короткими updates. Прежние findings остаются необходимыми отдельными проверками; новая карточка их не заменяет.

Отсутствие найденного точного совпадения в доступном корпусе не доказывает отсутствие идеи во всех ветках/личных заметках. Данный разбор не подменяет поиски успешного обучения в соседних ветках и не заявляет, что прочитаны все их payloads.

## Решение по конфликтам

Все новые карточки исследуют реальное обновление или доказанный вход к нему; одних parser/hash/validation чисел недостаточно для вывода об обучении. Любое измеренное улучшение полезного показателя считается успехом **по этому показателю и домену**; рост accuracy не требуется. Рядом показываются остальные изменения, включая ухудшения, покрытие и затраты. Проверки идентичности состояния являются средствами достоверности опыта, а не самостоятельным «качеством обучения».

Ничего из этих документов не предписывает изменить production, переписать старые эксперименты или прогонять всю цепочку тестов. Исследовательская копия использует реальный вызываемый код, фиксированные inputs и отдельные outputs. Внедрение результатов в продукт остаётся решением пользователя после измерения.

[indexer]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Indexer/ProjectIndexer.cs#L42-L78
[pipeline]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Pipeline/ContinuousLearningPipeline.cs#L57-L180
[length]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Training/IncrementalTrainer.cs#L385-L418
[updates]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Training/IncrementalTrainer.cs#L502-L672
[executor]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Training/SupervisedTrainingExecution.cs#L19-L31
[sft]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/04.TrainingLoop/Trainer.cs#L102-L147
[dispatch]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Pipeline/ContinuousLearningPipeline.cs#L195-L253
[public-trainer]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Training/IncrementalTrainer.cs#L54-L68
[composition]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Training/ContinuousLearningProductionComposition.cs#L254-L405
[prior-order]: https://github.com/lubochka/xiigen-planning/blob/de270758087502a350c4fcf460acdf1638bdd318/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/60-TRAINING-PATH-AND-EXPERIMENT-VALIDITY-2026-10-19/01-DPO-LORA-NEW-TRAINING-DIFFERENCES.md#L55-L88
[feedback]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Feedback/FeedbackRouter.cs#L130-L173
[provenance]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Core/TrainingProvenance.cs#L71-L135
[manifest]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Training/IncrementalTrainer.cs#L348-L385
[restore]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/18.CapabilityTraining/TeachingRegistryPersistence.cs#L1605-L1723
