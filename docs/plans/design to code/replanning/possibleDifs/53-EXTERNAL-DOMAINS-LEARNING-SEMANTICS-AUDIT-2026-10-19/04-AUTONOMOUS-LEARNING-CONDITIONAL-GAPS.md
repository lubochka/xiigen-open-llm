# Две дополнительные гипотезы автономного обучения: веса уроков и admission replay

19 октября 2026. **P2, условный путь `16.AutonomousLearning`.** Новизна относится к двум конкретным цепочкам исходника в сверенном корпусе, а не к уже доказанной причине неудачного RP01. Этот раздел должен идти **после** главы восстановления подтверждённых механизмов `external_domains`.

## Вывод и границы

Обнаружены две причины, по которым автономный цикл мог бы предъявлять ученику другой учебный материал, чем предполагает план: он схлопывает часть входных весов уроков, а следующий цикл может использовать replay из кандидата, который не прошёл последующую оценку. Оба свойства следуют из нормального порядка вызовов, без NaN, повреждённых checkpoint или обхода проверок.

Однако **нынешний исходник прямо фиксирует отсутствие нетестовой точки сборки этого pipeline** — [AutonomousLearningPipeline, 45–47][current-pipeline-root]. Поэтому они не объясняют уже измеренный isolated RP01 автоматически. В RP01 отдельный [Simulation runner создаёт `DpoTrainer` и вызывает `StepVector`, 160–186][rp01-direct]. Здесь рассматривается другой маршрут: `AutonomousLearningPipeline.RunAsync → AutonomousTrainer → IncrementalTrainer → supervised LoRA`. Pipeline передаёт в `PrepareBatch` пустой список DPO — [147–148][current-weight-flow]. Ни в одном из двух пунктов не утверждается дефект точной DPO-формулы.

Оба механизма в существенной части присутствуют и в donor. **Их исправление не является восстановлением найденного старого исправления.** `external_domains` остаётся приоритетным положительным ориентиром для реально работавших learners; это не придаёт каждому его вспомогательному классу статус доказанно успешного алгоритма.

В этой работе выполнен только Python source-equivalent probe: проверены точные Git blob bytes 11 файлов, порядок соответствующих вызовов и восемь коротких численных/событийных случаев. [Скрипт](evidence/autonomous/autonomous_source_probe.py), [результат](evidence/autonomous/autonomous_source_probe_result.json), [manifest](evidence/autonomous/source_manifest.json). **C#, LoRA, DPO, генерация ответов и внешний judge не запускались.** `dotnet` в окружении не найден. Изменений core нет.

## AUT-WEIGHT-01. Domain используется как класс, хотя допустимый batch обязан иметь один Domain

### Что делает текущий код

| Шаг | Наблюдаемое поведение | Источник |
|---|---|---|
| Допуск | Каждый собранный пример обязан иметь `example.Domain == requestedDomain`; иначе возврат `INVALID_TRAINING_EXAMPLE` | [Pipeline, 112–122][current-weight-flow] |
| Оценка дисбаланса | `domainCount = GroupBy(Domain).Count()`, `rareCount = GroupBy(Domain).Min(Count)` | [Pipeline, 124–134][current-weight-flow] |
| Вычисление коэффициента | `maxWeight = N / (numClasses × rareExamples)` | [DatasetWeighter, 26–36][current-weight-formula] |
| Изменение примеров | `Weight = Math.Min(e.Weight * maxWeight, maxWeight)` | [DatasetWeighter, 54–64][current-weight-apply] |
| Передача ученику | Изменённые примеры идут в `PrepareBatch`, replay mix и `TrainAsync`; `IncrementalTrainer` передаёт **сам Weight** в supervised executor | [Pipeline, 147–153][current-weight-flow], [AutonomousTrainer, 98–110 и 155–161][current-trainer], [IncrementalTrainer, 645–662][incremental-weight] |

Для **любого** непустого принятого batch из N строк: `domainCount=1`, `rareCount=N`, следовательно `maxWeight=1`. Условие `rareCount>0 && domainCount>0` истинно. Комментарий ветви `else` «single class … skip weighting» не описывает исполняемое поведение: single-class batch до неё не доходит. Получаем преобразование

\[
w_i' = \min(w_i,1).
\]

Валидный provider с пятью строками одного `CODE_Q`, где четыре обычных урока имеют вес 1, а редкий урок — 4, выдаст `[1,1,1,1,4]`. У ученика будет `[1,1,1,1,1]`: отношение редкого к обычному меняется с **4:1 на 1:1**. Это результат probe, не результат оптимизации модели. Валидатор `AutonomousTrainer` разрешает положительные конечные веса до `float.MaxValue`, поэтому исходный вес 4 не является нарушением контракта — [82–95][current-trainer].

Здесь **класс** означает заранее определённый тип задачи внутри одного предмета обучения: например, обычный результат и исключение в документации C#; сохранение отрицания и утвердительная конструкция в переводе. `Domain` — ключ маршрутизации адаптера, а не автоматически такая категория. Нельзя устранять проблему, подставляя категории в `Domain`: текущий pipeline отклонит строку с другим Domain. Семантика редкой категории должна быть отдельно зафиксирована, если она действительно нужна данному provider.

### Что это может изменить в обучении

**Гипотеза:** если provider использует веса больше 1 для явного приоритета проверенных редких уроков, текущий шаг устраняет этот приоритет до supervised LoRA. При ограниченном числе предъявлений редкие правила могут хуже закрепляться. Это конкретная потеря заданного веса, а не новое общее утверждение «взвешивание всегда полезно».

Контрсвидетельства существенны:

- Встроенный `DatasetPreprocessor` создаёт `Weight=1.0` — [78–87][preprocessor-weight]. Для такого материала изменение является **no-op**. Провайдер с весами больше 1 в работающем пользовательском запуске не установлен.
- Если provider задаёт нормированные веса `[0.25,0.25,0.25,0.25,1]`, то они сохраняются и относительный приоритет 4:1 остаётся. Такая семантика может быть намеренной; текущий публичный контракт её явно не требует.
- Даже сохранение разных чисел не гарантирует изменение ответа. Реальный executor делает отдельные supervised updates; AdamW может почти компенсировать масштаб одного градиента, особенно на первом шаге. Нужны фактические траектории нескольких чередующихся уроков, а не вывод из отношения весов.
- Стандартная `ComputeMaxWeight(1000,5,50)==4` проверяет арифметику helper, но не доказывает, что caller способен передать ему пять классов. Не следует менять саму формулу, не установив контракт весов.

### Сравнение с external_domains

`DatasetWeighter.cs` полностью совпадает с donor после удаления ровно пяти новых строк лицензии: current blob `60547ba2f316ad6ee72afd34b4cccffafdb61a40`, donor `04bdcf541d10dc547f86dc19fe8ed7ea07134219`. [Donor формула/применение, 21–59][external-weighter].

В [donor pipeline, 97–115][external-weight-flow] уже были `GroupBy(Domain)` и эта формула, но отсутствовал нынешний явный отказ на mixed-domain collection. Поэтому single-domain case был вырожденным и раньше; current guard делает эту вырожденность обязательной для каждого принятого batch. **Это усиленное контрактом унаследованное ограничение, а не найденное исчезнувшее улучшение donor.**

### Малый эксперимент EXP-AUT-WEIGHT-01

Сначала тестовый native driver вызывает существующий `RunAsync` с instrumented provider, trainer и корректными identity/provenance fixtures. Пять валидных примеров одного Domain, веса `[1,1,1,1,4]`. Driver сохраняет значения у provider, `PrepareBatch` и supervised executor. Ожидаемая механическая сигнатура текущего A — `[1,1,1,1,1]`. B меняет **только** обработку single-domain случая на сохранение исходных допустимых весов. Все IDs, тексты, порядок, replay state и возвращаемая оценка одинаковы. Отрицательные контроли: все веса 1; нормированные веса не выше 1; пример чужого Domain должен по-прежнему отклоняться.

Только после установленного actual consumer и воспроизведения поля Weight — маленький предметный этап. Для C# возьмите 8 source-disjoint уроков: 6 обычных результатов и 2 исключения/граничных случая. В отдельном опыте перевода — 6 утвердительных и 2 отрицательных конструкции с сохранением лица/времени. Это разные предметы и отдельные runs, не смесь внутри одного domain adapter. Все уроки должны быть независимо признаны корректными до обучения; категории и их веса не выбирать по результату.

A и B стартуют из одного сохранённого компетентного tiny state, с одинаковыми LoRA, tokenizer, optimizer, dropout/RNG и 8 одинаковыми последовательными предъявлениями; replay выключен **одинаково в обоих** для изоляции веса. A получает cap текущего pipeline, B — исходный provider weight. Ни SFT-данные, ни модуль LoRA, ни LR менять нельзя. После каждого шага сохранять реальный weight, loss, нормы обновлений и SHA state. До запуска заморозить 8 новых EXAM задач: 4 редких, 4 обычных, со source-family split. Сначала проверить механическое расхождение градиентов/параметров, затем собственные ответы до/после и после cold restore. Учесть, что одинаковое число предъявлений не означает одинаковый weighted-loss budget: именно этот вклад и является вмешательством.

**Предварительный малый сигнал:** B исправляет хотя бы один новый редкий случай из 4 при отсутствии потери на 4 обычных и при изменившемся реальном update trace. Это основание для следующей проверки, не статистическое доказательство общего выигрыша. **Опровержение/сужение:** реальный provider выдаёт только веса ≤1; intended contract требует нормированных весов; actual consumer не связан; либо matched A/B имеет одинаковые параметры и ответы. При одинаковом качестве записать `NO_OBSERVED_GAIN`, а не объявлять helper исправлением обучения.

## AUT-REPLAY-02. Уроки кандидата становятся replay до решения о его качестве

### Точный порядок вызовов

1. `PrepareBatch` берёт старый replay и замораживает manifest источников — [AutonomousTrainer, 98–110][current-trainer].
2. `TrainPreparedAsync` вызывает реальный `_trainer.TrainAsync`, проверяет успех, adapter path и достаточную provenance — [155–188][current-trainer].
3. Затем **добавляет все `batch.NewExamples` в `_buffer`** и возвращает результат — [190–199][current-replay-commit].
4. Только после возвращения `AutonomousLearningPipeline` оценивает candidate — [230–275][current-evaluate-order].
5. Если `enhanced <= baseline`, pipeline возвращает `ACCURACY_NOT_IMPROVED` — [301–315][current-reject]. Регистрация адаптера находится после этого условия.
6. `ReplayBuffer` имеет Add и Sample, ограничение ёмкости и фильтр Domain, но не хранит статус candidate и не откатывает добавление после такого отказа — [20–68][current-buffer].

**Следствие кода:** успешный и корректно сохранённый training artifact, не прошедший последующую quality evaluation, уже изменил будущий учебный поток. Это происходит и при нормальном `ACCURACY_NOT_IMPROVED`, без исключения, частичного checkpoint или сбоя процесса.

Механический пример: пустой buffer, первый batch — 5 строк, native training successful, candidate accuracy ниже baseline. Адаптер не опубликован; buffer содержит 5 строк. Во втором цикле того же Domain поступают ещё 5 строк. `Round(5×0.3/(1−0.3))=2`, и обе replay-строки неизбежно принадлежат первому не принятому кандидату. Получается 5 новых + 2 прежних строки. Наш probe доказывает количество и принадлежность **cohort**; точные индексы `.NET Random` не эмулировались.

### Почему это гипотеза качества, а не автоматическое доказательство «отравления»

Quality-rejected **адаптер не означает неверные уроки**. Хорошие уроки могут не дать прироста за малый бюджет или временно ухудшить другое знание; повторное предъявление может оказаться полезным. Сохранение таких уроков в replay может быть сознательным алгоритмом. Ошибочные, противоречивые или устаревшие уроки, напротив, могут удерживаться и влиять на последующие updates, хотя потребитель продолжает видеть прежний адаптер.

Новый проверяемый вопрос — **какова семантика admission replay относительно двух отдельных решений: качество урока и качество кандидата**. Сейчас код связывает admission только с успешным выполнением training/provenance, а последующее решение evaluator на него не влияет. Комментарий «prevents polluting … failed adapter» в trainer относится к technical failure, а не к будущему отказу evaluator. Нельзя по одному этому комментарию объявлять нынешний код нарушением явно утверждённой политики.

**Полная память неуспешных примеров должна сохраняться.** Предложение ниже меняет только допуск в положительные supervised targets следующего replay; оно не удаляет историю, исходники, отрицательные уроки, предупреждения или provenance. Независимо подтверждённый неверный пример сохраняется как negative/warning с причиной. Корректный урок из quality-rejected кандидата сохраняет собственный статус и может быть полезен для дальнейшего обучения. Положительный учебный replay и архив всех исходов требуют разных правил использования; запрещать или удалять весь failed cohort по одному статусу адаптера здесь не предлагается.

Контроли: при failed training buffer не пополняется; при другом Domain эти строки не выбираются; при отсутствии живого общего buffer между циклами накопительного эффекта нет. Нынешняя сильная проверка source provenance предотвращает неизвестное происхождение, но не кодирует независимую семантическую пригодность урока или решение о candidate.

### Сравнение с external_domains

В donor [AutonomousTrainer, 59–73][external-replay-commit] те же новые примеры добавляются после training и до возврата. Donor [pipeline, 120–142][external-evaluate-order] оценивает улучшение и принимает решение о регистрации позже. Значит порядок **унаследован**, а не потерян при переносе. Current дополнительно фильтрует replay по Domain и проверяет provenance. Старую выборку без фильтра Domain нельзя возвращать ради одного этого пункта.

Положительный historical E43 изучает replay в другом bigram learner; его успех поддерживает необходимость проверить удержание старого закона, но **не доказывает** пользу или вред этой политики admission в supervised LoRA. Этот пункт не повторяет E43 file-boundary gap или известный top3/newest capability replay: другой owner, другой критерий включения и другой момент появления урока.

### Малый эксперимент EXP-AUT-REPLAY-02

**Механическая стадия:** native test composition с настоящими `AutonomousTrainer`/`ReplayBuffer` и instrumented trainer/evaluator. Два цикла по 5 строк; первый training возвращает корректный artifact, evaluator отвечает baseline 0.8 / candidate 0.6. Проверить фактический статус `ACCURACY_NOT_IMPROVED`, отсутствие promotion, manifest первого batch в памяти и 2 его SourceIds во втором `MixedExamples`. Отдельные контроли — failed training, принятый candidate, другой Domain. Эта стадия проверяет порядок и membership, не обучение.

**Предметная стадия, один фактор — когда/по какому решению допускается новый cohort в replay.** A сохраняет нынешнее добавление до оценки. B держит cohort pending до завершения оценки и разрешает replay только по заранее выбранной политике. Самая простая проверка B — `candidate accepted`; её нельзя заранее объявлять правильной. Начать с 5 заведомо корректных старых replay-уроков и maxCapacity=5 в обоих плечах, затем 5 новых уроков первого кандидата. Это нормальная поддерживаемая малая ёмкость, **не утверждение о default4096**. Если первый кандидат не принят, у A остаются новые5, у B старые5. Следующий цикл получает одинаковые 5 новых уроков, а оба плеча — ровно2 replay, по одному seed и одинаковым позициям. Бюджет ученика одинаков: 7 supervised updates; различается происхождение двух replay-уроков. Ёмкость, old state, RNG, порядок, модель и данные второго цикла фиксированы. Фактические токенные длины подобрать из frozen pool до запуска либо честно отразить различие; не приписывать эффект меньшему/большему числу обработанных токенов.

В обоих плечах полный cohort первого цикла остаётся в отдельном исследовательском журнале с SourceIds, текстами, ролью positive/negative/warning, оценкой урока, оценкой кандидата и историей решений. Малая ёмкость5 выше относится только к выбранной положительной replay-памяти. Такой test-only staging не меняет retention policy всего продукта.

Для C# отдельно используйте правила операций и их исключений; для перевода — отрицание и роли действующих лиц. Проведите **две разные семантические панели**: (I) первый непрошедший cohort содержит независимо выявленные ошибочные/противоречивые target lessons; (II) cohort корректен, но candidate не улучшил качество за первый малый бюджет. Панель II — обязательный контроль того, что gate по успешности candidate сам может мешать полезному накоплению знаний. Нельзя получать статус «плохие уроки» из одной цифры evaluator.

Для каждой панели заранее заморозить 8 EXAM задач: 4 старого закона, 4 нового. Сохранить actual source IDs, отдельные оценки качества урока и candidate, выбранные replay IDs, количество updates/tokens, свободные ответы до/после и cold restore. Малый эффект B можно считать кандидатом на продолжение только если B исправляет хотя бы один старый случай из4 и не теряет ни одного нового из4 в панели I; одновременно показать честный outcome панели II. Если B проигрывает на корректных недоученных уроках, следующий вариант должен решать вопрос admission **уроков**, а не просто приравнивать его к promotion адаптера.

**Опровержение/сужение:** consumer отсутствует; replay не живёт до второго цикла; реальные кандидаты не отвергаются; качество уроков независимо проверено и A не хуже B; либо при одинаковом бюджете ответы/параметры не различаются. `MEMBERSHIP_DIFFERENCE` отдельно от `LEARNING_QUALITY_DIFFERENCE`. В этой сессии получено только первое на уровне source/formula.

## Проверка новизны и отброшенные направления

Полнотекстово проверены **661 Markdown**: 389 planning и 272 protected core. [Поисковый ledger](evidence/autonomous/novelty_search.json) содержит выражения и совпадения. По точным владельцам `AutonomousLearningPipeline`/`AutonomousTrainer`, `DatasetWeighter.Weight` и формулам `numClasses|rareCount|domainCount|maxWeight` совпадений не найдено. Дополнительно проверены русские/английские описания rejected replay. Это ограниченная проверка двух замороженных корпусов; она не доказывает абсолютную новизну во всех черновиках или ветках.

| Кандидат/близкая тема | Решение |
|---|---|
| Две цепочки AUT-WEIGHT-01/AUT-REPLAY-02 | Новые concrete source hypotheses относительно сверенного корпуса; inherited, P2 conditional |
| E44/V2 rare weights, сломанные пары и десять повторений | Уже разобраны protected57/73; не считать новую связку Domain→cap=1 повторной оценкой E44 |
| E43 bigram replay/file boundaries, capability newest/top3 | Уже известны; другой owner и другой критерий admission |
| DPO→CE конфликт и `ProjectIndexer` filename-only context | **Исключены из новизны:** peer draft уже зафиксирован в [package51, строка55][prior-dpo-context] |
| LoRA cold start/q-v vs head, detached old DPO, bias/Embedding gradients, BPE EOF/stride, summed logP FP32 cancellation, optimizer overflow, continuation moment carry | Предыдущие отчёты; новых карточек не создавалось |

Применены проектные `generated-code-review-SKILL` и `training-data-gap-audit-SKILL`: исходник важнее комментария, отдельные статусы field transformation / update / measured learning, обязательный caller и отрицательное свидетельство, точные SHA. Материалы других агентов и historical результаты используются как evidence-only. Нет нового подтверждённого запуска под CODEX/Astra 6 ultracode. Для будущих DPO опытов проектная роль CODEX/Astra выбирает урок и независимо оценивает точную пару; разрешённая внешняя открытая китайская модель генерирует кандидаты; оценка реальных ответов ученика — отдельная процедура. Численные fixtures здесь не являются разрешённым учебным DPO.

## Источники

Все current core ссылки закреплены за `44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76`; donor — за `3036a01acec938f0b83fa4ffbda26202cc35d639`. Реестр `evidence/autonomous/source_manifest.json` покрывает exact Git bytes. Получение pinned источников для повторной проверки описано в evidence/README.md; копии продуктового source в пакет не включены.

[current-pipeline-root]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/16.AutonomousLearning/Pipeline/AutonomousLearningPipeline.cs#L45-L47
[current-weight-flow]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/16.AutonomousLearning/Pipeline/AutonomousLearningPipeline.cs#L112-L153
[current-weight-formula]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/16.AutonomousLearning/Preprocessing/DatasetWeighter.cs#L26-L36
[current-weight-apply]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/16.AutonomousLearning/Preprocessing/DatasetWeighter.cs#L54-L64
[current-trainer]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/16.AutonomousLearning/Training/AutonomousTrainer.cs#L68-L188
[incremental-weight]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/14.ContinuousLearning/Training/IncrementalTrainer.cs#L645-L662
[preprocessor-weight]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/16.AutonomousLearning/Preprocessing/DatasetPreprocessor.cs#L78-L87
[current-replay-commit]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/16.AutonomousLearning/Training/AutonomousTrainer.cs#L190-L199
[current-evaluate-order]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/16.AutonomousLearning/Pipeline/AutonomousLearningPipeline.cs#L230-L275
[current-reject]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/16.AutonomousLearning/Pipeline/AutonomousLearningPipeline.cs#L301-L315
[current-buffer]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/16.AutonomousLearning/Training/ReplayBuffer.cs#L20-L68
[external-weighter]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/16.AutonomousLearning/Preprocessing/DatasetWeighter.cs#L21-L59
[external-weight-flow]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/16.AutonomousLearning/Pipeline/AutonomousLearningPipeline.cs#L97-L115
[external-replay-commit]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/16.AutonomousLearning/Training/AutonomousTrainer.cs#L59-L73
[external-evaluate-order]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/16.AutonomousLearning/Pipeline/AutonomousLearningPipeline.cs#L120-L142
[rp01-direct]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L160-L186
[prior-dpo-context]: https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/51-LOCAL-WIKI-SQL-KNN-TENSOR-RECOVERY-2026-10-19/04-TENSOR-CONDITIONAL-GAPS.md#L55
