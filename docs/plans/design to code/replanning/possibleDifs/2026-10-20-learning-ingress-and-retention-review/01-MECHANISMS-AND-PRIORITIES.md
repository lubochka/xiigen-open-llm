# Карта различий: что учится, где теряется сигнал и чем это уже проверяется

Это навигация по существующим гипотезам и новым дополнениям, а не объявление всех перечисленных механизмов дефектными. Факт наличия кода, карточки, исторического результата и измеренного эффекта текущего RP01 — четыре разных утверждения. Полная адресная таблица P01–P125 находится в [аудите карточек](experiments/03-CARD-BY-CARD-AUDIT.md); таблица ниже связывает их с вопросами пользователя.

## Сначала различить три действительных пути

| Путь | Получатель знания | Что доказывает прогресс | Чего он не доказывает |
|---|---|---|---|
| Direct RP01 simulation: materializer → DpoTrainer → project LoRA | Фактические q/v adapter parameters сохранённой нейронной модели | Native update, post-update objective на неизменных targets, собственный ответ, retention, затраты | Успех generic kNN или наличие index entries не доказывают улучшение этой LoRA |
| Continuous learning: ProjectIndexer/feedback → IncrementalTrainer → DPO и SFT | Та же модель/адаптер в конкретной сессии, плюс состояние optimizer | Дошли ли lessons до update; что изменилось после каждого этапа; какие источники реально предъявлены | Порядок и admission этого пути нельзя автоматически приписать direct simulation |
| Typed learning/semantic Units: материализация примеров → Train/AddEntry → learned memory → обычный Flow | kNN, mappings, classifier/semantic state конкретного Unit | Состояние до/после уроков и обычный ответ, использующий именно это состояние | Число optimizer updates 0 не означает отсутствия такого обучения; результат не является neural language learning |

Источники: [RP01 current code map][rp01-map], [IncrementalTrainer][incremental], [TranslationMachine][translation-machine], [Executor training/reader][translation-units]. Source-map RP01 описывает свой исторический execution slice; актуальность отдельных run-status в нём не переносится на сегодняшний весь продукт.

## Семейства риска и существующее покрытие

Номера ведут к карточкам защищённого каталога на позднем SHA. Это ссылки на поставленные проверки, а не доказательство выполненного опыта. Колонка «что проверять» обозначает одно различие за раз; объединять всю строку в большой A/B нельзя.

| Семейство | Возможное отличие, существенное для обучения | Имеющееся покрытие и уточнение этого аудита | Домен/граница |
|---|---|---|---|
| Качество DPO-пар | Chosen/rejected различаются в нужном смысле, общий prompt не съедает contrast, gold и происхождение обеих сторон корректны | P04, P07, P14, P81, P109. Не считать 500 разных JSON независимыми semantic families; не выдавать programmatic materialization за внешний teacher | Код→документация и перевод имеют разные смысловые contrasts |
| DPO objective | Response sum/mean, prompt/pad masks, fractional weights, sign/beta, zero-weight pair, масштаб teaching vector согласованы в forward/backward | P05, P06, P10, P86, P89, P114, P119. Fractional legacy fix имеет сохранённый native результат, но не объясняет binary exact-v2 автоматически | Профиль objective должен совпасть с настоящим caller |
| Получатель LoRA | Где действительно стоят adapters, какие параметры trainable, rank/alpha/dropout, frozen base/reference; способен ли исходный checkpoint решать задачу до preferences | P01, P02, P101, P105, P121. В сохранённом DPO live8 менялись 16 LoRA tensors, поэтому общий тезис «никаких updates» неверен. Embedding SFT8 менял base и не равен этому режиму | Нейронная модель; её способность не заменяется успехом словаря |
| Autograd и optimizer | Gradient доходит до нужного параметра; clipping/LR/AdamW moments/step и state reset не меняют скрыто траекторию | P11–P13, P87, P119; active23 SFT-MOMENT-CONTINUITY. Проверять настоящий update и post-state; само перечисление Parameters недостаточно | Full SFT и frozen-base LoRA требуют разных ожиданий |
| Учебная длина и состав пачки | Целый файл не вмещается, теряется response contrast или одна запись блокирует остальных | P07/P68/P98 относятся к соседним темам. **Новое:** INGRESS-LENGTH-01, общий abort before DPO/SFT. [Доказательства и опыт](ingress/02-INGRESS-LENGTH-EXPERIMENT.md) | Только вызванный indexed-file continuous-learning путь |
| Токенизация и координаты | BPE refit меняет старые IDs, unknown ID исчезает, порядок/роль/Unicode/числа/операторы теряются при vectorization; tokenizer/config checkpoint не совпадают | P16–P18, P61, P98, P99, P113, P122; active13 BPE. Сравнивать actual model-visible IDs/features и результат обучения, а не только длину словаря | Byte/BPE, semantic signature и AST — разные представления |
| Преобразование DPO в подуроки | Chosen превращается в правильные targets каждого Unit; labels, typed metadata, отрицательные требования и исправления не теряются | P52/P53, P85/P92/P94, P104/P106/P108; active20 и 23 composite cases. Не считать запись только верхнего статуса обучением всех дочерних Units | Конкретный Unit/Flow с известными writer и reader |
| Correction feedback | Исправление становится активным target, сохраняет identity/provenance и не уступает устаревшему indexed rejected | P58/P59/P67/P87/P106; **refinement:** CORRECTION-COLLISION-01. Общий DPO→SFT уже известен; новый узкий trace — источник конфликтующего SFT target | Combined incremental caller; exact prompt/domain/response collision обязателен |
| kNN признаки и функции | AST structure, argument order, POS, sentence order, scale каналов и context не заменены неподходящей плоской метрикой | P16/P26/P27/P43/P45/P47/P113, active11 E25, active20 ArgumentOrdinal; late N62 training-context. E09/E25 сохраняются как доменные примеры полезности | AST для программирования; POS/порядок для переводческого профиля. Универсального улучшения всех доменов из них не следует |
| Доступность соседей и агрегация | Сохранённый пример реально reachable; dedup/tier filtering/tie/short-k/eligibility не отбрасывают полезный target; lawful votes не сводятся к hits[0] | P20–P24, P35–P37, P60/P62–P65/P90/P107/P117. Frozen reader допустим, если читает доказанно выученную память | Результат обучения конкретного kNN/Flow; не обязательно новый gradient step |
| Replay и curriculum | Старые lessons сохраняются и реально выбираются; weight class не схлопывается; rejected cohort не отравляет следующий цикл; selection score не устаревает | P15/P24/P48/P74/P78/P89; package53, active15, active23 H60-A/B. **Новое узкое условие:** N63-AR-CAP общий FIFO × same-domain eligibility | Условный public API lifetime; не установлен текущий RP01 production caller |
| Сохранение и продолжение | Восстановлены не только weights, но и нужные typed memories, tokenizer identity, topology, moments/RNG/step; следующий update соответствует обещанному режиму | P18/P22/P25/P59/P67, active10/16/21/23. P25 уже показывает один положительный cold-restore24/24; он не закрывает все модели, schema или следующий optimizer step | Read-only проверка настоящего trained checkpoint допустима без фиктивного переобучения |
| Детерминированные пути | BuildDefault, static labels, SourceLaw, constant oracle или early return могут подменить/ограничить learned consumer | P29–P34/P38–P45/P50/P55/P56/P88/P104/P124. E26 фиксированный contextual dictionary полезен в своей роли, но этот run не учит новый lesson. N-KU показывает реальное обучение детерминированным labels | Deterministic algorithm сам по себе не дефект; важны writer, состояние и причинный вклад |
| Генерация и использование результата | EOS/byte decoding/cache/sampler/route реально участвуют в собственном ответе обученной модели | P08/P09/P75/P76/P100/P102/P118. Для greedy RP01 stochastic-only проблема неприменима. Зависимый reader не исключать только за 0 updates | Второй приоритет после действующего учебного пути; точный consumer обязателен |
| Оценка и split | Одни и те же cases/denominators; нет train-test leakage; accuracy до/после относится к одному recipient и одной метрике | P03/P57/P66/P71/P77/P79/P80/P82/P91/P112. Аудит scorer допустим как проверка сохранённых результатов; самостоятельный report-only запуск не измеряет обучение | Отдельно TRAIN, near transfer, heldout families, retained/lost cases |
| Стоимость реального обучения | Игнорируемый padding, копии градиента, проверки frozen state, scratch allocation, полный export optimizer ради step | P110/P115, N61 reference, late N62 neural-update-costs. Меньше времени/байтов на тот же реальный update — самостоятельная полезная ось; accuracy panel не обязательна для cost-only вопроса | Измеряемый интервал должен включать одинаковую работу и не сравнивать 0 updates с 2 как ускорение |

Все перечисленные карточки доступны в [каталоге][cards]; адресный индекс и фактические статусы — [card-audit.json](experiments/card-audit.json). Политика и active recipes уже уточнялись другими авторами; этот пакет не присваивает себе их правки. Защищённый каталог прочитан, но не изменён.

## Почему исторические успехи нельзя выбросить или перенести целиком

Одинаковая задача на словах ещё не означает одинаковый learner. В E25 действительно обучается POS-память; в E43 строится модель частот с replay; E26 получает заранее подготовленную словарную базу. Эти механизмы могут быть полезными частями функции kNN даже там, где matrix-only вариант был неудачным. Их сохранённый положительный результат не следует отбрасывать из-за отсутствия neural gradients. Но он также не доказывает, что нынешняя q/v-LoRA получила тот же сигнал. [Код, числа и сравнение соседних веток](replay/01-NEW-REPLAY-CAPACITY-AND-HISTORICAL-EVIDENCE.md#исторические-успехи-сохранены-с-правильным-получателем).

Для каждого переноса нужны четыре предметных ответа: что было записано обучением, в каком состоянии это хранится, какой метод читает его в нынешнем запросе и какой показатель изменился. Простое наличие StructuralKnn.cs после merge отвечает только на вопрос о сохранности файла. Глобальное требование «вернуть матрицу» или «всё перенести в kNN» из существующего evidence не следует.

Late N-KU особенно показателен. Исправление распознавания `do` в teacher labels улучшило 3 случая и потеряло 1. Predictor при четырёх уроках и k=3 выдаёт одно большинство A и другое большинство B; это успешная правка учебного сигнала по заявленной оси, а не доказанное структурное обобщение. Следующая проблема отсутствующего `DoStatement` feature уже названа автором нового отчёта и **не записана здесь как наша новая находка**. [Поздний отчёт][nku-report].

## Как измерять до и после без подмены задания

Для обновляющего опыта сохраняются одинаковые исходные A0/B0 и фиксированные TRAIN/EXAM. A обучается текущим механизмом, B — единственным объявленным изменением. Измеряются A1/B1 на той же панели, а также изменения A1−A0/B1−B0, если соответствующий показатель состояния определён. У многослойной цепочки «после обучения» должно быть точно: после DPO, после следующего SFT, после restore, после чтения trained memory. Иначе следующая стадия может скрыть эффект предыдущей.

Для зависимости после обучения допустим один настоящий trained state и два reader/loader варианта. Его training provenance и точный consumed state обязательны; второй Train только ради формального счётчика не нужен. Для сравнения стоимости обучения нужен настоящий update; для стоимости использования результата — настоящий consumer доказанно обученного состояния. Новые генерации нужны только тогда, когда они отвечают поставленному вопросу.

Во всех случаях отдельно отражаются:

- полезная метрика и её направление; числитель/знаменатель, единицы и профиль;
- actual issued/completed calls, реальные optimizer steps или вызовы Train памяти;
- принятые, отложенные и фактически предъявленные строки, токены и источники;
- выигрыши и потери по каждому case/slice, без взаимного стирания;
- изменение состояния и правильное восстановление как evidence исполнения;
- время/allocations/RAM на определённой границе, без превращения cumulative process peak в независимый arm peak.

Это критерии описания и проверки уже нужного опыта. Они не назначают дополнительные тестовые программы, большой grid search, внешнего teacher, полную CI-цепочку или исправление production. Нулевое улучшение по одной оси не отменяет измеренную пользу другой, а failure до update не является «нулевым эффектом обучения».

## Очередь без скрытых блокировок

| Порядок | Конкретная работа | Завершение стадии |
|---|---|---|
| 1 | Проверить действительный caller и устранить обнаруженный admission blocker в изолированном опыте; в новом пакете это INGRESS-LENGTH-01 | Реальный B update и честный per-metric before/after либо точный build/runtime failure, сохранённый без подмены результата |
| 2 | Применить адресный overlay remaining21 карточки; учитывать уже исправленные upstream P51/P94/P110 | Старый blanket-запрет не конфликтует с доказанным learning/learned-result path; unrelated варианты остаются вне очереди |
| 3 | Использовать текущие точные малые рецепты и пользовательские правила; сравнить supplied updates с каталогом | Каждому выбранному опыту назначены inputs/host/commands/output/budget; не требовать пользовательской подписи для рутинной подготовки |
| 4 | Исследовать conditional replay/correction и зависимые restore/consumer seams | Подтверждён конкретный caller или результат честно ограничен research host; отсутствующая связь не останавливает другие опыты |
| 5 | Разбирать только оставшиеся indirect вопросы с доказанной зависимостью | Report-only/constant-oracle проверки не получают учебных токенов |

Перенос успешного изменения в оригинальные файлы, полная цепочка продуктовых тестов и принятие модели — отдельное решение пользователя после результатов. В этом пакете ни один такой перенос не выполнен и не назначен автоматически.

[rp01-map]: https://github.com/lubochka/xiigen-planning/blob/de270758087502a350c4fcf460acdf1638bdd318/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-CURRENT-CODE-MAP.md
[incremental]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Training/IncrementalTrainer.cs#L348-L672
[translation-machine]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/27.TranslationDomain/TranslationMachine.cs#L89-L153
[translation-units]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L455
[cards]: https://github.com/lubochka/llm_mvp_core/tree/67abfabe6def8a002984be80b999824601ad273c/docs/plans/replanning-possible-diffs/experimets/cards
[nku-report]: https://github.com/lubochka/llm_mvp_core/blob/67abfabe6def8a002984be80b999824601ad273c/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/report.md
