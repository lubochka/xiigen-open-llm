# Что действительно улучшилось в сохранённых опытах

## Метод и результат проверки

Это повторный анализ **уже записанных** результатов. Прочитаны raw, gold, scores, сведения о шагах и состояниях; прочитаны учебные участки пяти сохранённых native harness и SFT training method. На этих данных заново посчитаны правильные ответы, ошибочно принятые ответы, парные losses, optimizer steps и равенство восстановленного состояния. Несовпадений проверенных raw/score/gold не обнаружено. Новых native запусков, Train, Generate, сборок и finite-difference вычислений — **0**.

Артефакты: [машинный пересчёт](evidence/retained-reanalysis.json), [read-only скрипт](evidence/recheck_retained.py), [источники сохранённых данных](evidence/retained-source-manifest.json). Локальные SHA256 относятся к скачанной текстовой копии; Git blob SHA и immutable ref отдельно обозначают исходный Git-файл. Этот пакет не утверждает наличие всех старых DLL/checkpoints на текущем компьютере.

### Реестр кандидатов для решения пользователя

| Кандидат | Измеренное полезное изменение | Ухудшения/ограничения | Текущий вывод |
|---|---|---|---|
| N-KU-01, метка `do` | После обучения A1=1/4, B1=3/4; лишний ADD_LOOP на трёх кейсах 3→0 | Нужный ADD_LOOP на KQ4 теряется 1→0; B даёт постоянный ADD_METHOD; timing меняет знак в сохранённом BA | **Полезный кандидат по бинарной метрике; не внедрён этим аудитом** |
| embedding-sft8-01 | Средний online training loss 7,0169490625→6,9877417; 7/8 парных losses ниже; embedding получает обновление | Перевод 0/24→0/24; process CPU/RAM выше; это full SFT, не frozen-base RP01 | **Успех по objective реального обучения; не универсальный успех перевода** |
| dpo-mask-live8-01 | Записанные derivative cases того же backward 126/162→162/162; время 8 Step: 36113,99→34532,0568 мс | Средний online loss немного выше; перевод 0/24; CPU/RAM выше; время — одиночное наблюдение | **Полезная численная корректность активного training path; уже известное исправление** |
| Translation ingress | После одинаковых 8 уроков 11/24→19/24; композиции 0/8→8/8 | Ошибочно принятые ответы 1→3, precision среди принятых ниже | **Успех доставки учебных подданных; прежнее исправление не выполнять повторно** |
| P125, память Executor | B:0/24→19/24; no-train A остаётся 0/24; corrupt C даёт 13/24 | В 19 входят 8 точных повторов,3 близких примера, 8 композиций; 5 сложных случаев не решены | **Приобретение памяти/композиции доказано на этой панели** |
| P25, cold restore |24/24 исхода (22 ответа и 2 отказа) и canonical state совпадают; качество 19/24 сохранилось | Сохранились и 3 ложных принятия; нет нового роста знания | **Успешное сохранение результата конкретного обучения** |

Кандидат означает, что полезный результат нельзя потерять в документации. Этот статус не даёт автоматического разрешения менять product code, повторять опыт или запускать полную цепочку тестов. Состояние «исправление уже было внесено раньше» также не является новым поручением его переписать.

## 1. P125: память учится и читает результат через обычный Flow

Три независимые TranslationMachine имеют одинаковый начальный canonical state. A не обучается; B получает 8 правильных уроков; C получает те же 8 с переставленными действиями validate/delete. Каждый arm проходит 24 запроса до и после. Итого 144 исходные строки. Writer — `TrainStructuredBatchAsync`, reader — `GetOrchestratorAsync`→`RunAsync`, без доступа harness к gold. Первичный [native harness](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/harness/Program.cs) прямо содержит эти вызовы; [training receipts](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/training-receipts.json) сохраняет две реальные учебные операции.

| Arm | До обучения | После | Ошибочно принятых после |
|---|---:|---:|---:|
| A, no train |0/24|0/24|0|
| B, правильные уроки |0/24|19/24|3|
| C, испорченные действия |0/24|13/24|9|

Контроль C показывает зависимость от содержания уроков, а не только переключения флага готовности. В B точные повторы 8/8, близкие 3/3, комбинации 8/8, сложные случаи 0/5. Поэтому результат называется памятью и ограниченной композицией, без заявления family-disjoint обобщения нейронной модели. Источники: [raw144](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/raw-outputs.json), [gold](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/panel-gold.json), [scores](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/scores.json), [начальные состояния](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/initial-equivalence.json).

## 2. Потеря phrase_mappings: полноценный урок может не дойти до Unit

Сохранённый harness вызывает реальные `ReadTranslationCorpusRowsAsync` и `BuildTranslationContinueExample`, затем `TrainStructuredBatchAsync` и обычный orchestrator. В исправленном варианте учебные `phrase_mappings` доходят до обучения; это конкретная подготовка материала, от которой зависит выученный ответ. Точка исправления — `27.TranslationDomain.Tools/Program.cs`, названные reader/materializer; переносить поля mappings в `ExecutorOutput`, сохраняя legacy-поведение. Здесь документируется уже выполненный опыт, а не назначается новая правка. [Цепочка загрузки→Train→consumer](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/harness/Program.cs); [текущий materializer](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/27.TranslationDomain.Tools/Program.cs).

После одинаковых 8 уроков A дал 11/24, B — 19/24. Все 8 композиционных ответов добавлены; прежние 11 верных сохранились. Coverage принятых 12/24→22/24, ошибочных принятий 1→3, precision 11/12≈91,7%→19/22≈86,4%. Ошибки cc01/cc04 добавились к прежнему cc03. Это полезный прирост вместе с регрессией. [A raw](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/before/raw-outputs.json); [B raw](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/after/raw-outputs.json).

**Подписи before/after здесь означают разные реализации A/B, уже прошедшие обучение.** Это не хронологическая цепочка одной модели 0→11→19. Полного generated A0/A1/B0/B1 нет; начальная сопоставимость удостоверена сохранёнными state-данными и guard в harness. Этот предел не отменяет matched learning comparison.

## 3. P25: сохранность навыка после холодной загрузки

Сохранённый B-checkpoint P125 загружается новым процессом через `LoadCheckpointAsync`; новый Train не нужен, потому что проверяется использование уже выученного. Source/loaded/after canonical JSON совпадают. Для 24 запросов равны source, answer, verdict, flow, adapter и metrics. Качество 19/24 и 3 ошибочных принятия сохраняются. [Load и обычный reader](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/harness/Program.cs); [load receipt](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/load-receipt.json); [comparison](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/comparison-summary.json).

Это положительный результат восстановления TranslationMachine memory. Он ничего не доказывает о полноте optimizer/RNG/LoRA restore другого learner или совместимости старых schema. Адрес последующей проверки при реальной регрессии — `27.TranslationDomain/TranslationMachine.cs` и checkpoint contracts; нынешний положительный опыт не создаёт потребности менять их.

## 4. SFT8: полезное улучшение training objective при прежней точности

Оба arm стартуют с одного C50 checkpoint, одинаковыми weights/RNG/tokenizer/config и 8 token sequences. Один Trainer и AdamW сохраняются через 8 реальных `TrainSupervisedNextTokenStep`; optimizer steps 1…8. В этом опыте все 55 тензоров разрешены для SFT, включая 39 base и 16 LoRA. Это отличается от RP01 с замороженной базой. [native lifecycle](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/harness/Program.cs); [A initial](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/before/initial-state.json); [B initial](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/after/initial-state.json).

| Показатель | A | B |
|---|---:|---:|
| Средний online pre-update loss по 8 урокам |7,0169490625|6,9877417|
| Loss 8-го урока |6,6029263|6,54443|
| Пары losses: B ниже / равен / выше |—|7 / 1 / 0|
| Изменившихся тензоров |54/55|55/55|
| Верных переводов |0/24|0/24|
| Сумма времени 8 Train, мс |24505,3504|24391,5929|
| CPU всего процесса, мс |141593,75|149609,375|
| Peak working set процесса, байт |1737625600|1826639872|
| Накопленные managed allocations процесса, байт |175607527112|175687971504|

Источник чисел: [A steps](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/before/training-steps.json), [B steps](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/after/training-steps.json), [A cost](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/before/cost.json), [B cost](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/after/cost.json). Малую разницу времени одного запуска не выдавать за устойчивое ускорение; меньший loss всё равно является полезным измеренным результатом.

Loss вычисляется **перед optimizer.Step**, затем возвращается после шага: [TrainSupervisedNextTokenStep](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/04.TrainingLoop/Trainer.cs#L67). Следовательно, это среднее online pre-update loss по одинаковому порядку уроков, не heldout loss и не loss всех примеров на одном конечном checkpoint. Последовательные уроки — не независимые повторения. Пустое заявление «неудача, потому что accuracy 0» неверно; равно неверно обещание переводческой способности из уменьшения loss.

Адрес исходного механизма — `00.TensorCore/Layers/Embedding.cs`, передача backward к embedding weight; actual SFT consumer находится в Trainer. Исправление уже отражено сохранённой парой A/B и текущим исходником. Нынешний результат — документированный кандидат/подтверждение полезности, без повторного редактирования ядра.

## 5. DPO8: численная корректность активного backward

Оба arm выполняют 8 настоящих `DpoTrainer.Step` с 16 LoRA параметрами; 39 base и reference на каждом шаге неизменны. Материал одинаков, reference и optimizer не пересоздаются между уроками. Этот режим использует **legacy fractional response mask**: первый target 0,25. Exact binary RP01 отличается. [native lifecycle](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/harness/Program.cs); [A steps](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/before/training-steps.json); [B steps](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/after/training-steps.json).

По сохранённым численным проверкам того же backward проходит 126/162→162/162 сочетаний координаты и размера шага. Это полезная корректность производной, участвующей в actual training; один такой guard без связи с 8 Step не заменил бы учебный опыт. Источники: [A guard](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/before/component-guard.json), [B guard](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/after/component-guard.json). Здесь пересчитаны сохранённые pass flags, новая численная проба не выполнялась.

Средний online loss 0,69314783875→0,693147945, дельта +0,00000010625; 3 парных loss ниже, 3 выше, 2 равны. Перевод 0/24 в обоих arm. Время 8 Step: 36113,99→34532,0568 мс; CPU процесса 154890,625→157968,75 мс; peak 1958367232→2199101440 байт. Это раздельные scopes и одиночные наблюдения. [A cost](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/before/cost.json); [B cost](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/after/cost.json).

Точка механизма — `11.DPO/DpoTrainer.cs`, нормировка `BackwardLogProbs`; точный путь указан в сохранённом harness provenance. P86 уже описывает этот режим. Не откатывать current исправление и не выдавать известный legacy дефект за новую причину exact RP01.

## 6. N-KU: сохранить успех и прекратить расширение прошлого опыта

Пересчёт четырёх state групп даёт A0=0/4, A1=1/4, B0=0/4, B1=3/4. После обучения A всегда возвращает ADD_LOOP, B всегда ADD_METHOD. Gold KQ1–KQ3 задаёт NOT_ADD_LOOP и считает правильным любой ненулевой ответ, кроме ADD_LOOP; KQ4 требует ADD_LOOP. Поэтому 3/4 означает ровно отсутствие лишнего цикла в трёх случаях, а не правильность совета ADD_METHOD и не структурное обобщение. [A raw](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-A.json); [B raw](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-B.json); [gold](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/gold.json).

Основной time A 195,8347 / B 123,8038 мс меняется на A 98,7049 / B 133,1402 мс в сохранённом B→A контроле. Устойчивое ускорение не доказано. По основному Train аллоцировано 1179032→1169344 байт; BA 1166696→1169344. Process peak A включает дополнительный product-equivalence и не является чистой A/B памятью. [BA A](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/order-check-BA/arm-A.json); [BA B](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/order-check-BA/arm-B.json); [исходный отчёт](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/report.md).

Кандидат: учесть `DoStatementSyntax` в teacher label классификации `FileActionKnnPredictor`, как в сохранённом B. Это реальное изменение обучающей метки перед native Train и Predict. Адрес исходного кода и точная поправка сохранены в [N-KU-карточке](https://github.com/lubochka/llm_mvp_core/blob/65d03a73a76810a0425a41c33d3c7eeaf4cdbc48/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md). Принятие полного исправления остаётся за пользователем, с учётом KQ4 регрессии. Новую панель, повторный запуск, full CI и product fix этот отчёт не назначает.

**Если пользователь говорит «следующий эксперимент», N-KU немедленно закончен: нельзя продолжать его оформление, поиск причин majority, проверки, hashes, публикацию или rerun. При «стоп» прекращается вся работа.** Именно это правило имеет приоритет над незавершённым реестром кандидатов.

## Как повторить только подсчёт старых данных, если отдельно поручено

Скрипт использует стандартную библиотеку Python и читает существующее дерево `experimets/results`; не запускает native код, сеть, модель, обучение или генерацию. Выход записывается в заранее назначенный новый разрешённый файл вне защищённого core каталога. Это воспроизводимость документального аудита, не следующий обучающий опыт и не обязательная новая стадия.

```powershell
python evidence/recheck_retained.py --input-root C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/results --output C:/xg/audit-retained-reanalysis.json
```

Для оформления достаточно данного отчёта и ссылок. Нельзя превращать сверку в цикл новых запусков; язык отчёта русский, модель исполнителя текущая по умолчанию. Любое полезное измеренное изменение сохраняется по своему показателю и домену, остальные изменения показываются рядом.
