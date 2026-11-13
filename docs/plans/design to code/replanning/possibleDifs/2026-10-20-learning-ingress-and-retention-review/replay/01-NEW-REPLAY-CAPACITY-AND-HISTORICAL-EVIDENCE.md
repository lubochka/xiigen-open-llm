# Новая условная гипотеза: общая ёмкость replay и возврат к прежнему домену

Дата: 20.10.2026. Область: `16.AutonomousLearning`, supervised next-token обучение LoRA. Статус: **SOURCE_CONFIRMED / CONDITIONAL_PUBLIC_API_SCENARIO / NATIVE_NOT_RUN**. Приоритет P2 после разблокирования доказанно действующего RP01. Это не установленная причина неудачного RP01 и не результат нового обучения.

## Что добавлено после проверки новизны

В текущем `ReplayBuffer` история ограничена **общим** FIFO, а выбираются из неё только строки запрошенного домена. Если один экземпляр обслуживает последовательно домены A, B, A, поток B способен удалить всю память A. Возвращение к A тогда получает 100% новых уроков вместо ожидаемого replay; оставшийся адаптер A сам по себе не восстанавливает удалённые примеры. Возможный эффект — худшее удержание уже усвоенных правил A при следующем обучении A. Польза резервирования памяти пока не измерена.

Две первоначально рассматривавшиеся соседние гипотезы — схлопывание `Weight > 1` и добавление replay раньше оценки кандидата — **уже описаны** в [пакете 53, AUT-WEIGHT-01/AUT-REPLAY-02][prior53] и в [защищённой программе, строки 154–172][protected-aut]. Они не включены в число новых находок. Их уточнения вынесены в отдельный документ, поскольку исходная папка экспериментов запрещена для изменений.

Полнотекстовый поиск охватил 662 Markdown planning и 291 Markdown защищённой программы, а также JSON/JSONL/CSV/TSV/TXT/CS/PY в этих снимках: 1111 + 768 файлов выбранных типов. [Поисковая квитанция](evidence/novelty-receipt.json) сохраняет все выражения, совпадения и hashes корпуса. Это проверка новизны по доступному зафиксированному корпусу, не доказательство, что никто никогда не рассматривал эту идею. Поиски `domainCount/rareCount/AUT-WEIGHT/AUT-REPLAY` сняли новизну двух прежних пунктов. Поиски `DefaultMaxCapacity/eligibleIndices/global FIFO/domain capacity/reservation` и русских эквивалентов не нашли этой конкретной связи владельцев. Общая идея domain-aware eviction уже есть у **другого получателя**, E42 disk memory [N37-D][prior-e42]; она не проверяет потерю supervised replay вследствие нынешнего same-domain filter.

## Точная цепочка до изменения обучения

Все текущие ссылки ниже зафиксированы на `llm_mvp_core@ce890a48efda7f00d0ee040f2383cc2d06428ea6`.

| Участок | Наблюдение | Почему это влияет на обучение |
|---|---|---|
| [ReplayBuffer,18–37][buffer-cap] | Default4096; один `_buffer`; при переполнении удаляются самые ранние строки независимо от Domain | История одного домена может быть вытеснена другим |
| [ReplayBuffer,46–56][buffer-filter] | `eligibleIndices` включает только Domain из новых примеров; при нуле возвращаются только `newExamples` | Сохранённые чужие строки не заменяют утраченные уроки своего домена |
| [AutonomousTrainer,32–46][trainer-owner] | Один readonly buffer передан в конструктор и хранится между вызовами | Публичный объект допускает такую общую историю, если его lifetime общий |
| [AutonomousTrainer,82–110][trainer-prepare] | Один вызов принимает только один Domain; `Sample` непосредственно формирует `MixedExamples` и provenance manifest | Отбор не ограничивается диагностикой: он задаёт реальные входы следующего learner |
| [AutonomousTrainer,155–161][trainer-update] | `MixedExamples` передаются в `IIncrementalTrainer.TrainAsync` | Память влияет на фактически предъявленные supervised targets |
| [IncrementalTrainer,645–662][incremental] → [SupervisedTrainingStepExecutor,22–32][executor] | Для каждого примера вызывается native `Trainer.TrainSupervisedNextTokenStep`, с его Weight | Переход от массива примеров к настоящим градиентным обновлениям |
| [Trainer,111–147][native] | Forward, weighted CE, backward, clipping, AdamW update | Это реальное обучение; изменение только membership без этого участка не является проверкой улучшения обучения |

**Арифметический вывод из исходника, не эксперимент:** при `capacity=10`, десяти добавленных A, затем десяти B и пяти новых A, нынешний buffer вернёт пять новых A и ноль replay. При резерве 5 для каждого из двух доменов останутся пять старых A; неизменная формула `Round(5×0.3/0.7)` запросит два replay. В эксперименте надо подтвердить фактические IDs и затем измерить настоящее обучение, а не тратить отдельный запуск на эту арифметику.

Домен здесь имеет два значения, которые нельзя смешивать: `Domain=CODE_Q/BUG` — ключ адаптера и фильтра; предмет первой проверки — объяснение C# с охранными условиями и изменение таких объяснений. `BUG` служит другой очередью обучения с отдельной моделью. Результат не переносится автоматически на EN→RU, AST kNN или общую языковую модель. Для перевода нужна отдельная программа с собственными уроками и оценкой роли/отрицания.

## Ограничение достижимости: общей нетестовой сборки не найдено

[AutonomousLearningPipeline,45–46][no-root] прямо отмечает отсутствие нетестового composition root. Прочитанный `25.ModelServer/Program.cs` того же SHA не содержит создания `ReplayBuffer`, `AutonomousTrainer` или `AutonomousLearningPipeline`. В [Tests/AutonomousLearningTests.cs,297–389][tests] есть отдельные проверки ratio, domain isolation и FIFO capacity, но это test composition. Они не показывают реальную продуктовую последовательность A→B→A; их успешность в этой работе не проверялась.

Следовательно, можно утверждать **поддерживаемый публичным API сценарий**, но нельзя утверждать, что общая память действительно живёт между продуктовыми доменами. Если фактический будущий caller создаёт отдельный buffer для каждого адаптера, эта причина междоменной потери неприменима. Если продукт вообще не вызывает этот путь, она не объясняет его нынешнее качество. Малый исследовательский host явно создаёт общую память для проверки механизма и вызывает тот же native learner; он не выдаётся за существующую продуктовую сборку.

## История: не возвращать прежнюю смесь чужих доменов

В `external_domains@3036a01acec938f0b83fa4ffbda26202cc35d639` [ReplayBuffer,13–31][donor-cap] уже имел общий FIFO4096. Но [Sample,34–53][donor-sample] выбирал из всей истории без нынешнего Domain filter. Поэтому обнаружена **связь унаследованного удаления с нынешним ограничением допустимости**, а не исчезнувшая найденная функция резервирования. Возврат к смешиванию чужих доменов не предлагается. В treatment меняется только выбор удаляемой записи при том же общем лимите и неизменном фильтре; B-примеры не поступают в A-градиенты.

## Исторические успехи сохранены с правильным получателем

Тела E25/E26/E43 в `language_translate@f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` совпадают с текущими после удаления только пяти строк лицензии и начального BOM. [Квитанция сравнения](evidence/history-body-parity.json). Это независимая повторная сверка сохранности кода, **не новая находка** и не повтор исторических прогонов.

| Механизм | Что подтверждает сохранённое свидетельство | Предел вывода |
|---|---|---|
| E09 structural kNN | Уже разобранная [история и gold/current parity51][prior51] сохраняет положительные AST результаты | Здесь E09 заново не запускался и не пересчитывался; известное различие denominator не превращается в новую находку. Нельзя заменять AST prediction свободным языковым ответом |
| E25 POS kNN | [Сохранённый JSON][e25-result]: top1 `0.3438→0.875`, 160 synthetic examples, validation32; hard-pattern accuracy `.8571`. [Код,22–48][e25-code] действительно вызывает Train перед Predict | Это обучение таблицы POS-паттернов, не LoRA и не доказательство улучшения перевода. Train находится до двух timers: `.4→15.4ms` — сравнение evaluation timing, не цены обучения |
| E26 contextual dictionary | [Код,22–47][e26-code] использует `BuildDefault` и 15 фиксированных контекстов; учитывает соседние POS | Сохраняем пользу контекстной функции. Этот run не показывает усвоение нового урока: словарь заранее построен. Нельзя требовать от него neural evidence или объявлять его неудачным только из-за отсутствия gradient updates |
| E43 replay | [Сохранённый JSON][e43-result]: old-panel accuracy `.3226→.3978`, relative improvement23.33%, retention ratio1.1562. [Код,38–55][e43-code] строит словари частот | Это реальный положительный результат памяти примеров для данного bigram learner. Три словаря строятся заново; это не продолжение одной модели с LoRA/AdamW. E43 не проверяет общий bounded `16.ReplayBuffer`, его Domain filter или возврат A→B→A |

AST/порядок/POS могут быть полезной функцией kNN именно в домене, которому нужны эти различия. Неудачный matrix-only опыт не опровергает успех такой функции внутри kNN. В этом пакете не добавляется повтор уже описанного общего тезиса: новая гипотеза относится к составу **последующих supervised updates**, а не к замене AST признаков.

## Следующая работа и возможное исправление

[Карточка N63-AR-CAP](02-N63-AR-CAP-EXPERIMENT.md) определяет единый новый host, fixture, native learner, A/B и контроль одинакового числа обновлений. Она не требует запуска всей продуктовой цепочки или фиктивных admission receipts. Все будущие изменения ограничены новым экспериментом; production source и защищённая папка остаются неизменными. После подтверждённой пользы пользователь решает, переносить ли reservation в `ReplayBuffer.Add`, либо устанавливать per-domain buffer lifetime в реальном composition root. Второй путь пока не имеет установленного нетестового адреса и не должен выдумываться.

### Ссылки

[prior53]: https://github.com/lubochka/xiigen-planning/blob/de270758087502a350c4fcf460acdf1638bdd318/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/53-EXTERNAL-DOMAINS-LEARNING-SEMANTICS-AUDIT-2026-10-19/04-AUTONOMOUS-LEARNING-CONDITIONAL-GAPS.md
[protected-aut]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/15-additions-designtocode-learning-semantics.md#L154-L172
[prior-e42]: https://github.com/lubochka/xiigen-planning/blob/de270758087502a350c4fcf460acdf1638bdd318/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/37-NEW-E42-KNOWLEDGE-RETENTION-METRIC-WITHOUT-RETRIEVABLE-LAWS-2026-10-16.md
[prior51]: https://github.com/lubochka/xiigen-planning/blob/de270758087502a350c4fcf460acdf1638bdd318/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/51-GOLD-BRANCH-EXPERIMENT-CODE-PARITY-AND-E26-DETERMINISTIC-LOOKUP-2026-10-19.md
[buffer-cap]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/16.AutonomousLearning/Training/ReplayBuffer.cs#L18-L37
[buffer-filter]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/16.AutonomousLearning/Training/ReplayBuffer.cs#L46-L56
[trainer-owner]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/16.AutonomousLearning/Training/AutonomousTrainer.cs#L32-L46
[trainer-prepare]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/16.AutonomousLearning/Training/AutonomousTrainer.cs#L82-L110
[trainer-update]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/16.AutonomousLearning/Training/AutonomousTrainer.cs#L155-L161
[incremental]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Training/IncrementalTrainer.cs#L645-L662
[executor]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Training/SupervisedTrainingExecution.cs#L22-L32
[native]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/04.TrainingLoop/Trainer.cs#L111-L147
[no-root]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/16.AutonomousLearning/Pipeline/AutonomousLearningPipeline.cs#L45-L46
[tests]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/Tests/AutonomousLearningTests.cs#L297-L389
[donor-cap]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/16.AutonomousLearning/Training/ReplayBuffer.cs#L13-L31
[donor-sample]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/16.AutonomousLearning/Training/ReplayBuffer.cs#L34-L53
[e25-code]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/35.ExperimentsV2/Experiments/E25PatternClassifier.cs#L22-L48
[e25-result]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json
[e26-code]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/35.ExperimentsV2/Experiments/E26ContextualDictionary.cs#L22-L47
[e43-code]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs#L38-L55
[e43-result]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json
