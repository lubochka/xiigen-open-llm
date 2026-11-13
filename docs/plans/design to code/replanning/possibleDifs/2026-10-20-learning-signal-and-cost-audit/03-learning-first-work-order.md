# Сначала разблокировать реальные малые уроки

**Это конкретизация текущей серии, не ещё одна очередь поверх неё.** Защищённые карточки остаются источником исходных рецептов. Здесь перечислены необходимые изменения их исполнения и недостающие связи. Код, веса, конфигурация и эксперименты в этом проходе не изменялись и не запускались.

**Текущая политика:** [редакция от 20 октября](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/00-execution-policy-2026-10-20.md) уже сняла общую блокировку карточек и повторные личные одобрения. Неоформленные доступные поля и малый caller — `PREPARABLE`: исполнитель готовит их сам. `NEEDS_INPUT` означает конкретный обязательный недоступный вход после адресной попытки его получить. Ниже перечислена инженерная подготовка, а не новый approval gate.

**Последнее адресное уточнение7c5718:** [P01, строки3–25](https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/cards/P01-trainable-recipient.md#L3-L25) допускает простой host с throw, try/catch, void и DTO; продуктовый OperationResult, оформление и полная цепочка внедрения не prerequisite. Ошибку сохранять с этапом, фактическими счётчиками и ненулевым exit code. Старые эксперименты, исходники, результаты и bin/obj сохраняются. Новая редакция Markdown не назначает перенос, пересборку или повтор уже начатого опыта. Это правило относится ко всем новым LF-prompts ниже; права менять protected-каталог оно нашему заданию не даёт.

## 1. Что требуется подготовить для нейронной части

Общая [карточка D][d] содержит шесть групп полей, исторически помеченных NEEDS_INPUT: точный фактор; bounded native caller/admission; материал 2TRAIN/4EXAM; реальные B0/reference; optimizer/RNG/settings; разрешённые paths/команда. Поэтому формулировка «DPO/LoRA покрыто экспериментами» допустима только как наличие тем, не как готовность исполнимого опыта.

Но [поздняя карточка 23][r23] уже задаёт четыре конкретных фактора — MASK, STATE, PAD, ORDER. Их нельзя снова потерять в общем меню D. Подробные budgets и ограничения уже описаны; требуется замкнуть указанные ниже связи между записью состояния и его потребителем.

Сам существующий RP01 host не является произвольным micro-runner. [Program.cs][cli] фиксирует canonical root и dataset pins; принимает calibrate для одной пары или main500. [ValidOptions][options] закрепляет 1/500 и конкретный ENRU C50→C100 режим. [Issued marker][issued] зависит от mode+input SHA и переживает новый run-id. Повтор calibrate под новым ID не создаёт законный A/B.

### B1. Ограниченный host на точной экспериментальной копии

**Актуальное уточнение 62bcc имеет приоритет над прежним предложением product dispatch:** сначала используется экспериментальная копия необходимых исходников; расширять production runner для первого малого опыта не требуется. [Точная политика](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L3-L26). Ссылки на original файлы задают происхождение и будущий адрес переноса; сам перенос в продукт — отдельное решение после результатов.

| Контраст | Копии механизма относительно корня core | Отдельный host |
|---|---|---|
| LF-02, C# |35.ExperimentsV2/Experiments/Rp01Micro/LF-02-CS/ |experiments-v2/RP01-MICRO/LF-02-CS/Runner.csproj и Program.cs |
| LF-03, C# |35.ExperimentsV2/Experiments/Rp01Micro/LF-03-CS/ |experiments-v2/RP01-MICRO/LF-03-CS/Runner.csproj и Program.cs |
| LF-04 / E1, RU→EN |35.ExperimentsV2/Experiments/Rp01Micro/LF-04-RUEN/ |experiments-v2/RP01-MICRO/LF-04-RUEN/Runner.csproj и Program.cs |

Это будущие пути реализации в исходном проекте, не утверждение о наличии готовых файлов в нынешней среде. Для существующей N-KU использовать её уже описанный host, не новый dispatcher. Результаты нашего документного задания остаются в разрешённом possibleDifs: чужое указание писать results в защищённый каталог не отменяет прямой запрет пользователя менять этот каталог.

**Для уже начатой N-KU:** [последняя карточка, строка73](https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md#L71-L73) прямо сохраняет предыдущие NKu01/VariantA, VariantB и N-KU-01/runner. Новая схема предназначена для ещё не начатого варианта. Опубликованный [preregistration](https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/preregistration.md) фиксирует прежний паспорт, а не receipt выполненного обучения. Его роли, warmup и последующий контроль не переносятся автоматически в наши новые LF-бюджеты; этот аудит не переназначает участников текущего запуска.

1. Закрепить один source snapshot и manifest конкретного опыта: exact source files/dependencies, входы, состояния, A/B factor, scalar settings, native counts и output namespace. A — точная копия, B — копия с единственной правкой. Не копировать весь репозиторий и не подключать общий ExperimentsV2.csproj. Чужие Markdown-коммиты не требуют перечитывать весь проект.
2. Для LF-02/03 включить настоящие DpoTrainer/LoRA/Tensor/model/AdamW и необходимые material/recipient dependencies. Сохранить exact IDs/masks, реальное связывание разрешённых параметров, frozen reference и checks, от которых зависит эквивалентность обучения. Зависимости включаются явно; арифметическая имитация, поддельное состояние или ожидаемый ответ вместо learner не подходят. Если используемый factory требует действительный typed recipient/capability, включить его законный provider, а не fabricated token.
3. Загрузить две независимые policy/optimizer/RNG копии одинакового B0 и R0. Существующие calibrate/main и issued markers не вызываются для имитации нового A/B. Изолированный эксперимент не является повтором старого production run или разрешением изменять его данные.
4. Выполнить только native budget выбранного контраста: LF-02/LF-03 — один single-pair update на плечо; MASK/STATE/PAD/ORDER имеют свои отдельные бюджеты ниже. Один нужный host и предусмотренные A/B процессы; без дополнительных warmup, CI, Main500, повторов до успеха или автоматического расширения панели.
5. Одинаковый observer работает на actual training thread. Gradients наблюдать до ApplyOptimizer/ZeroGrad. Возвращённые Step-метрики — pre-update forward; post-update scoring не скрывается в бюджете. Времена вложенных стадий не складываются как независимые.
6. Для resource-only actual update сохранять state и затраты; Generate не нужен. Для ответного опыта использовать тот же обученный candidate и реальный reader из его карточки. Отдельно показать ошибки materialization/reachability и честный нулевой эффект достигнутого обучения.

**Готовый результат подготовки:** конкретные source copies, короткий проект с явным составом файлов, A/B diff, один material/state manifest и точная исполнимая команда после materialization. Это PREPARABLE работа, не новый круг разрешений. Готовая сборка или доступные B0 runtime bytes в этом документном аудите не заявляются. Предыдущее предложение менять tools/Rp01SimulationDpo/Program.cs/CodeDocumentationSimulationDpoExperiment.cs может быть рассмотрено только как будущая продуктовая интеграция; **оно не prerequisite малого опыта**.

### B2. Четыре нейронных фактора — что именно закрывать

| Текущий ID | Причинный вопрос | Original seam и конкретный ремонт | Минимальный бюджет из активной карточки | Что не смешивать |
|---|---|---|---|---|
| E-SFT-RESPONSE-MASK | Не расходуются ли updates на prompt вместо ответа? | Trainer.TrainSupervisedNextTokenStep: prompt+LF target mask0; response mask1; одинаковые joint BPE IDs и denominator T−1 в loss/backward. [Исходный метод](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/04.TrainingLoop/Trainer.cs#L102-L148). Нужен bounded supervised caller и совместимое сохранение candidate | По 2 SFT update;4 всего; при заявленной оценке 16 generated answers и 32 candidate-side scores | Не включать response-only normalization, новую BPE сегментацию или DPO |
| SC-SFT-STATE | Меняет ли следующий урок reset SFT moments? | [IncrementalTrainingSession](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/IncrementalTrainingSession.cs#L403-L419): fresh AdamW против carry m/v/step после первого урока через реальные keyed bindings; фиксированный положительный LR и те же weights/RNG | По 2 SFT update;4 всего;16 answers при выбранной output-оси | Не reset всей модели, scheduler, mask или DPO reference |
| ND-DPO-PAD | Сколько стоит нулевой хвост actual update? | После проверки original exact material сделать side-specific IDs+mask view по ChosenLength/RejectedLength; сохранить texts/digest и target i+1 | По 2 DPO update;4 всего;0 Generate в resource-варианте | Не искать конец по ID0; не менять уроки, objective или dropout |
| ND-DPO-SFT | Не подавляет ли последняя стадия эффект первой? | Компоновать существующие DPO и SFT owners с R0 и двумя stage optimizers, созданными до обоих stages | На плечо 1 DPO batch из 2 пар +2 SFT update;6 optimizer updates суммарно | Не recapture reference после SFT; research exact A не равен whole production legacy caller |

[Источники всех четырёх карточек и точные 2TRAIN/4EXAM][r23]. Общий нейронный профиль в них: vocab629, context2048, H512,4 layers, q/v LoRA rank8/alpha16/dropout0; это [конфигурация][config], не доказательство языковой компетентности B0. Два почти зеркальных subtraction примера — быстрый близкий контраст, не четыре независимых закона.

**Порядок:** сначала materialize выбранный прямой training path и фактический native budget. MASK и STATE требуют supervised owner; PAD — exact DPO owner; ORDER — их согласованную композицию, поэтому имеет больше зависимостей. LF-02/LF-03 могут использовать тот же bounded DPO owner, но не должны вытеснить semantic learning questions своей простотой. Не назначать серию всех опытов автоматически.

### B3. Малые KNN/Unit уроки

- N-KU-01: сначала учитывать уже существующий [4-row runner proposal](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md), не создавать дубликат. Для отдельного структурного вопроса подготовить шестистрочную [LF-01](01-new-findings-and-small-experiments.md). Ограничение 4/k3 уже признано в f5dd; исходные четыре строки допустимы для эффекта смены постоянного ответа с явным tradeoff.
- N60-E1: сначала применить [LF-04](01-new-findings-and-small-experiments.md) к материалу обеих ветвей, чтобы урок достигал Verifier marker writer; затем сравнивать только admission совпавшего candidate. N57 code repair одновременно не добавлять.
- Нативный FileAction facade требует минимум 4 обучающих файла; Structural facade имеет другой guard. Два урока допустимы для прямого predictor, но это другой уровень потребителя. Не обещать ordinary facade result от не достигнутого predictor.
- Translation memory: сохранённый ingress fix уже доставляет phrase_mappings. Сохранить его как положительный контроль; не назначать повторный ремонт уже изменённого reader.
- DPO→Units: проверить конкретный projected input/chosen/rejected каждой роли, а не только parent pair. В BuildGeneralBatch один pair создаёт semantic/executor/verifier уроки с разной целью; арбитр смыслового качества должен соответствовать роли. Список unit IDs и count3 сами по себе не доказывают правильность преобразования.

У этих задач разные native learners; их можно исследовать независимо после подготовки собственной короткой карточки. Успех таблицы не является необходимым предварительным gate для DPO и наоборот.

E2 не требует ещё одного абстрактного проекта: в позднем [planning overlay](https://github.com/lubochka/xiigen-planning/blob/fb9aadf69e0669d36b820d61f76fe3ecc8697a7d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-current-learning-audit/coverage/02-N60-CONCRETE-OVERLAYS.md) уже предложен конкретный SourceSpanFrames writer/state/reader. Это доступная подготовка для выбранного опыта; она не объявляется внедрённым кодом. Указанные там отдельные larger ordinary-retention данные E1 не добавляются скрыто к нашему малому direct-Verify бюджету.

## 2. Какие присланные правила внесены в дополнение

| Правило пользователя | Как оно реализовано здесь |
|---|---|
| Малый проверяемый опыт по предметному домену | У новых LF-карточек exact factor, native writer/consumer, материал/бюджет и фальсификатор; активные рецепты разобраны в 02 |
| Любой полезный измеренный выигрыш — успех по показателю/домену | Правило есть в каждой новой карточке и её полном prompt; tradeoffs обязательны; старый accuracy-only порог не применяется |
| Измерение должно касаться обучения | TRAIN_UPDATE/INPUT требуют reached update; standalone math/copy/benchmark не заменяет опыт |
| Входы и потребление результата тоже могут влиять на обучение | Введён единый LEARNED_OUTPUT с writer/state/reader/code-to-fix; не blanket запрет всех frozen/cold |
| Сначала всё, что разблокирует learning | B1–B3 выше; остальные readout/serving карточки ниже; unrelated проверки не запускаются |

**Не нужно заново добавлять одну и ту же фразу во все protected P-карточки:** у всех 125 она уже есть. Изменяются рекомендации и конкретные постановки в этом разрешённом planning-каталоге. LF-02/LF-03 отсутствуют и в проверенной поздней защищённой программе; эта незакрытая строка покрытия сохранена явно.

## 3. Затем — доступ к уже обученному состоянию

Текущая f5dd-политика уже устранила общий запрет «нет нового Train — невалидно» в соответствии с пунктом 4 задания. Следующие пути могут быть допустимы, когда результат зависит от реального trained state:

| Семейство | Требуемая связь | Конкретная граница вывода |
|---|---|---|
| Cold restore TranslationMachine | Ранее выполненный TrainStructuredBatchAsync → сохранённая память → LoadCheckpointAsync/ImportCheckpoint → обычный TranslationRoutingFlow | Доступность/сохранение выученных ответов; не новое приобретение способности |
| T POS query / RX preprocessing | Конкретная обученная таблица → тот же native Predict с одной изменённой query transformation | Использование выученной памяти, не изменение самого Train |
| OrchestrationReasoning v1→v2 | Проверенный historical trained state → честная миграция к настоящему loader → actual API consumer | Не выдумывать provenance и не менять schemaVersion вместо миграции |
| DTC learned preference reader | Названный actual writer preference state → Improver/ordinary output | Исправление чтения trained preferences; report-only reward без чтения не learning gain |
| Decoder/EOS/cache | Один verified trained Student checkpoint → одна изменённая часть generation path | Эффект получения выученного результата, не LoRA optimizer repair |

Если restore блокирует **само продолжение обучения**, он возвращается в B1 как prerequisite, а не откладывается механически. Если существует только файл или названный API без доказанного writer/consumer, статус OPEN_LINK; сначала статически установить связь. Регистрация неиспользуемого пути, счётчики отчёта, Windows/coverage церемонии сюда не входят.

## 4. Полный исполнительский prompt для одного выбранного пункта

> Работай по конкретному ID из текущей малой серии с поправками этого дополнения. Сначала предъяви native путь: input → фактический writer/update → изменяемый state → настоящий consumer. Закрой именно недостающие поля caller/state/material/output; не меняй docs/plans/replanning-possible-diffs. Для малого опыта используй точную экспериментальную копию выбранного механизма и отдельный host по текущей policy. Production source/tests и старые calibrate/main не менять; не подменять learner имитацией. Сохранить exact material, реальный recipient и reference semantics. В копии измени только фактор выбранной карточки; никаких скрытых changes labels/features/k/optimizer/decoder одновременно. При разрешённом исполнении соблюдай её actual budget, сохрани до/после и сырые результаты. Любое измеренное улучшение полезного показателя — успех именно по этому показателю и домену; рост accuracy не обязателен, все ухудшения рядом. Для resource-опыта actual update обязателен, Generate не обязателен. Для LEARNED_OUTPUT докажи trained-state dependency и не заявляй acquisition. Не запускай unrelated проверки, не увеличивай Tier и не повторяй опыт ради красивого результата. Передай результаты, точный A/B diff и кандидат будущего переноса; product cycle и перенос не запускай автоматически. Заверши ограниченным выводом и точным оставшимся вопросом, если он есть.

Это prompt конкретной подготовки и последующего исполнения в пределах порученной программы. Существующая авторизация сохраняется; повторное личное одобрение каждого паспорта не требуется. В данном аудите новых запусков и измеренных результатов не было.

[d]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/04-documentation-neural.md#L62-L89
[r23]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md
[cli]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/tools/Rp01SimulationDpo/Program.cs#L15-L20
[options]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L224-L231
[issued]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L78-L90
[config]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/config/code-documentation/generation-v1.json
