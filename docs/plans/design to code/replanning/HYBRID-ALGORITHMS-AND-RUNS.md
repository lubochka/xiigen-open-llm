# Гибридные алгоритмы: сохранённая архитектура и предметные поручения R01–R25

**13.10.2026 · DRAFT уточнения действующих 25 поручений. Не новая очередь, не исполнение R01–R25 и не измеренное обучение.**

Назначение: вернуть в каждое поручение связь между устройством алгоритма и наблюдаемым обучением. Система должна не просто хранить пары или проходить Windows-проверки: она должна представлять предмет задачи, выбирать полезные связи между механизмами, получать допустимый ответ, учиться на разрешённой новой информации и становиться точнее/экономнее на независимых задачах.

Этот документ содержит общий технический контракт H01–H09 и **адресную специализацию каждого существующего Rxx**. Dispatcher включает в один готовый prompt полный существующий Rxx, только его специализацию ниже и необходимые разделы H. Пользователь не должен вручную склеивать документы. Чтение всех 25 специализаций каждым исполнителем запрещено как ненужное дублирование контекста. Старые четыре LEARNING-WORK-ORDERS файла сохраняют все остальные исходные обязанности.

## H01. Источники, восстановленные решения и незавершённость исследования

D = `llm mvp/design to code session`; G = `llm mvp/dpo legal build`; F = `llm mvp/llm dynamic flow`; C = `llm mvp/visual studio & visual studio code continue`. Все четыре корня относятся к planning repository. K = `lubochka/llm_mvp_core`, branch `codex/dpo-implementation`.

| Ключ | Точный маршрут | Как использовать |
|---|---|---|
| P01 | [original task](<../original task.md>), раздел «Роли каждого блока» и следующий абзац | Прямой источник четырёх ролей, many→many, разных комбинаций одного входа, собственного DPO Builder. Исторический provider shortlist не является сегодняшним допуском. |
| P02 | [Алгоритм создания ДПО](<../Алгоритм создания ДПО.md>), [task prompt](<../task prompt.md>), [planning of testing](<../planning of testing.md>) | Восстановить алгоритм, DTC-цепь и домены; старый порядок «DPO последним» заменяется текущей learning-first целью, не удалением конечной функциональности. |
| P03 | [предыдущие раунды](<previous rounds thinking results.md>) | История исправлений, не текущий dispatcher; разные старые R03 и разные числа проходов не суммируются. |
| P04 | [R01 findings](results/R01/FINDINGS.md), [R02 findings](results/R02/FINDINGS.md), [R03 findings](results/R03/FINDINGS.md), их SOURCES.csv и DECISIONS.md | Использовать содержательные находки и точные spans. На прочитанном срезе все три результата IN_PROGRESS. Их существование не завершает раунды. Новые публикации читать как дельты. |
| P05 | [SOURCE-ROUTES](SOURCE-ROUTES.md), [RAG usage guide](<../09.09.26WorkLeft-RAG-00-usage-guide.md>), MASTER-INDEX и все RAG-01..04 MD/NDJSON, DELTAS | Полная навигационная область; разрешать source.file относительно объявленного корня, проходить вложенные карты с visited-set. |
| P06 | `G/active-codex-execution-original-task.md`; `G/active-codex-execution-plan-12-no-hidden-determinism.md` | Первичные маршруты, восстановленные R03. Сверять границу вычислительного helper, baseline и обучаемой семантической власти. Не выдавать пересказ R03 за собственное чтение всего G. |
| P07 | `C/RAG-06-llm-dynamic-flow.md` → `F/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md` → `F/DECISIONS-LOCKED.md` | История domain-scoped памяти, положительных/отрицательных/предупреждающих случаев и независимого арбитража. Старый порог 0.50 и старые цифры не назначаются современным baseline. |
| P08 | `D/09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md`; `D/09.09.26WorkLeft-RESEARCH-01-training-quality-speed-accuracy-program.md`; owner decisions/COVERAGE/RESEARCH-02 | Существующая база исследования, а не поручение заново написать такой же трактат. Читать активные разделы и необходимые первичные решения; исторические 20/30/50 проходов не добавлять. |
| P09 | [LEARNING-EVALUATION-CONTRACT](LEARNING-EVALUATION-CONTRACT.md), LF01–LF12 из LEARNING-EXAMINATION-CONTRACT, DOMAIN-EXPERIMENT-RECIPES/PLAYBOOK | Сохранять существующий метод измерений и уникальные предметные детали. Этот документ уточняет архитектурную адресацию, не обнуляет их. |

Уже видны вопросы, которые нельзя потерять: четыре роли против смешения с текущими ILogical* именами; старый Qwen-as-judge против нового CODEX selector/pair judge; replay 70/30 против one-pair/no-repeat; семь учебных доменов против дополнительных типов задач и языков; deterministic baseline против learned authority. R02/R03 продолжают их первичную трассировку; R12 принимает архитектурные решения, R16 применяет разрешённые изменения. До решения спорное не объявляется отсутствующим или уже реализованным.

## H02. Четыре функциональные роли и границы их соединения

Это **сохраняемые функциональные ожидания**, а не объявление, что нынешний код уже имеет приведённые сигнатуры.

| Роль | Что обязана делать | Что в ней исследовать | Как наблюдать полезность |
|---|---|---|---|
| ILearningUnit | Получить domain/schema входных векторов и требуемого выхода; сопоставить несколько входов нескольким типизированным результатам. | Матрица/параметры, представление, локальный выбор, условия допустимости, owner обновления, способ перечисления результатов. | Для одного набора входов сохранить существенные различия и выдать допустимое множество результатов; качество на новых семействах после урока. |
| ILearningFlow | Описать отношения между units и преобразование/коррекцию промежуточных входов в выходы. | Совместимость типов и координат, mapping/adapter между портами, fan-out/fan-in, learned edge choice и фиксированные ограничения. | Правильный перенос значений и причинно полезная композиция; фиксированные units + меняемая связь дают различающий опыт. |
| ILearningOrchestrator | Асинхронно организовать обработку одного входа несколькими допустимыми комбинациями и получить выход. | Выбор маршрутов, бюджеты, зависимости, конкуренция, порядок результатов, остановка и изоляция состояния. | Качество и полная latency относительно последовательного/фиксированного маршрута; late/partial/cancelled ответы не превращаются в ложное улучшение. |
| ILearningDynamicGraph | Представлять отношения паттернов разного порядка и выбирать/комбинировать полезные подструктуры. | Типы узлов/рёбер и порядок паттерна, learned selection, память отказов, credit assignment, pruning и Builder. | Полезная новая композиция на незнакомых сочетаниях, а не рост числа рёбер или повтор маршрута из обучающего примера. |

`Iquearable`, `IlearnyngFlow` и `IlearningOrchestrator` остаются исходными aliases. Намерение queryable/перечислимого typed many→many результата сохраняется. R04 обязан сверить реальный C# контракт, включая IQueryable/IEnumerable/IAsyncEnumerable, ленивость, материализацию и повторное перечисление; нельзя молча назначить LINQ-provider по опечатке или сократить результат до scalar. Если текущие ILogical* и generic interfaces представляют те же роли, нужна таблица соответствия с кодом, не переименование по похожему имени.

Функциональная роль ≠ отдельная модель ≠ физический trainer. Текущий план называет владельцем обучения ILearningMachine и train-ready GraphDpoPair с Level="unit" и UnitId. R05/R12 проверяют это по producer→optimizer→consumer. Оценка flow/path/graph необходима уже сейчас; произвольный новый Train у каждого уровня либо graph-level payload без миграции не разрешён. Если существующий unit-level механизм не может обучить нужное отношение, это **явный функциональный gap**, а не основание вычеркнуть отношение из требований.

Для каждой связи сохранять: producer/consumer; input/output domain и schema version; shape и смысл осей; forward mapping и необходимость обратимости; loss/информационные потери; fan-out/fan-in; порядок и identity результата; confidence отдельно от допустимости; owner состояния; версия checkpoint/index; наблюдение ошибки; разрешённое действие при несовместимости. Один и тот же vector length не доказывает одно пространство.

## H03. Паспорт гибридного алгоритма и управляемые вмешательства

Реальный паспорт отвечает человеческими словами: что алгоритм получает, что выбирает/вычисляет, что в нём меняется после урока и почему следующий ответ должен стать лучше.

Обязательные технические поля: `mechanism_id; role; domain/task/intent; source_refs; code_producer→method→consumer; input/output schema; deterministic operations; learned authority; persistent state; objective; update owner; signal/provenance; dependencies; baseline; intervention; observable/falsifier; time/memory accounting; failure/recovery`.

Не смешивать две оси: **обученное/необученное состояние** и **детерминированное/стохастическое исполнение**. Обученная матрица может исполняться воспроизводимо; случайность сама по себе не доказывает обучение. Фиксированный parser, проверка единиц, вычисление расстояния или scheduler resource cap могут быть нужной частью гибрида, но не заменяют learned semantic choice.

Минимальная цепь исследования:

`вход → domain+intent → typed representation → допустимые соседи → кандидаты unit/flow/path → исполнение → арбитраж результата → диагностика слабого звена → допустимая пара → изменение состояния → повторная независимая оценка → принятие/откат → обычный consumer`.

Управлять связями через отдельно именованные изменения: (1) port/projection; (2) разрешённые рёбра и learned weights/selector; (3) выбор подграфа/порядка; (4) scheduling и resource cap. Логи фиксируют, что реально изменено. Нельзя приписывать весь итоговый reward каждому ребру одинаково без обоснования. Для pair signal указывать, какой unit/machine/selector мог причинно повлиять на различие кандидатов и как получена эта атрибуция.

Первое сравнение — существующий пригодный reference против одного минимального изменения. Затем только различающие ablations: fixed units/changing flow; fixed flow/changing unit; sequential/fixed route против разрешённой adaptive orchestration; flat patterns против higher-order composition. При подозрении на взаимодействие — ограниченное 2×2 сравнение, например representation×kNN. Не требовать полный декартов перебор всех настроек до первого урока.

«Лучший путь» означает лучший **среди исследованных допустимых альтернатив при объявленных условиях**, не доказанный глобальный оптимум. Сначала допустимость и достаточное качество, затем экономия. Быстрый неправильный ответ не выигрывает. Сравнение correctness-equivalent путей использует заранее зафиксированный допуск качества, одинаковые данные, попытки и ресурсные ограничения.

## H04. Правильная DPO-пара, сигнал и действительное обновление

Одна пара содержит один неизменный problem/context, domain+intent, входную/выходную схему, два сравнимых кандидата, обоснование предпочтения, provenance, версии Student/reference/evaluator и lineage исходной задачи. `chosen` и `rejected` — предпочтение в этом контексте, не универсальные классы истинности.

Хороший negative правдоподобен и отличается существенной ошибкой: потеря отрицания; неверная единица; зависимость выполнена раньше prerequisite; SQL JOIN теряет требуемые строки; корректный ответ получен недопустимой/слишком дорогой комбинацией при заранее заданном бюджете. Нельзя делать rejected заведомой бессмыслицей, добавлять метки «плохой ответ», менять ему исходную задачу, использовать закрытый certification target или принимать оба неверных ответа как учебную пару. TIE/BOTH_BAD/UNJUDGEABLE отправляются на отдельное разрешение, а не в принудительное предпочтение.

При нескольких корректных решениях качество рассматривается до стоимости; preference на экономию допустимо только в пределах заданной эквивалентности качества. Отрицательная история kNN полезна как предупреждение, но не превращается автоматически в градиентный replay.

Для стандартного DPO нужен прослеживаемый differentiable policy/log-probability путь для обоих кандидатов и reference policy. Проверить objective, reduction/masks, общий контекст, beta, frozen reference и owner градиента. Формальная опора — [Rafailov et al., DPO](https://arxiv.org/abs/2305.18290). Отсутствие вероятностного пути нельзя скрыть произвольным similarity score: pairwise ranking/metric learning может быть альтернативой, но называется своим именем, получает отдельное решение и сравнение.

Не всякая preference-пара уровня пути автоматически является пригодным GraphDpoPair. R05 описывает текущий сериализуемый объект; R07 — сигнал отношений; R12 — точное отображение в существующего training owner или предметный проект миграции. Анализ графа не служит разрешением менять Level/UnitId молча.

Сохраняются актуальные one-pair/no-repeat и полный state snapshot. Bulk — группа наблюдений новых разрешённых уроков. Не повторять gradient на прежней паре через новый ID, replay 70/30 или «batch optimization» без явного изменения политики. R12 обязан определить семантику one-pair на уровне sample/optimizer step/episode, если источники расходятся; не додумывать число steps. Recovery неизвестного завершения сначала разрешает состояние операции, затем принимает решение об обновлении/откате.

Роли по восстановленной пользовательской дельте: CODEX в назначенной Astra Ultra роли выбирает учебную задачу и оценивает exact pair; допущенный внешний open-weight generator либо разрешённый собственный Builder создаёт содержимое; XIIGEN Student обучается. Оценка полезности Student и итоговая certification отдельно адресуются владельцам. Judge comments/закрытые эталоны не копируются в targets через generator. Requested model/effort не равны actual invocation. Эта публикация не запускает внешние модели.

## H05. Домен, законы, токены и dynamic kNN

Паспорт домена разделяет предмет, intent, язык/направление, задачу, уровень композиции, данные и пользовательские ограничения. Семь WorkLeft доменов — базовый минимум, не закрытый enum: EN↔RU, оркестрация, Figma→HTML+CSS, C#/Angular/SQL documentation, Builder для этих шести. Сохранить также server/client code, stored procedures, другие языковые направления и прочие исходные требования, обнаруженные R02/R03.

Для каждого «закона» указать источник/версию и тип: строгий invariant; модель предметной области с предпосылками; policy пользователя; экспертное предпочтение; неизвестное. Высокая статистическая близость не отменяет строгий invariant. Закон предметной области не равен случайному привычному формату корпуса. Для меняющихся технических фактов фиксировать версию среды/языка, а не объявлять вечное правило.

Representation path: raw input → parser/tokenizer → typed features/relations → embedding/projection → optional codebook/token IDs → алгоритм/decoder. Для каждого перехода назвать сохраняемый смысл и допустимую потерю, schema/coordinate version, bytes и downstream criterion. Числа, единицы, отрицания, типы, причинность и связи не должны исчезать ради меньшей размерности. Не путать обычные текстовые tokens, доменные символы и vector coordinates.

Dynamic kNN задаёт отдельно: candidate pool по domain/правам/версии; feature groups и нормировку; distance/similarity; k/radius; порог доверия/abstention; роль negative/warning memory; что обучается и каким сигналом. Кандидаты могут использовать semantic, structural, numeric/unit и relation features с domain-specific весами. Это гипотезы для выбора, не универсальная выбранная формула.

Сначала пригодный exact-search reference на объявленном масштабе; ANN добавляется по профилю цены и допустимой потери retrieval. Если выбранная функция не является метрикой, явно проверить применимость конкретного индекса; не предполагать её по имени kNN. При изменении encoder/projection/metric рассмотреть invalidation/re-embedding/reindex, не сравнивать несовместимые координаты. Полную цену включить в результат.

Измерять отдельно domain mapping (known/mixed/unknown), retrieval и solver. Сравнить actual router с экспертно допустимым route при том же solver; затем actual retrieval с обоснованным reference pool. Это диагностические сравнения, а не право использовать экспертный ответ в production или обучении. High similarity не переопределяет финальное решение арбитра.

## H06. LoRA и перевод результата в пригодное векторное представление

LoRA адаптирует конкретные матрицы низкоранговым обновлением; не является сама по себе tokenizer или обещанием правильной семантики векторов. Опора: [Hu et al., LoRA](https://arxiv.org/abs/2106.09685). Назвать matrix owner, input/output dimensions, rank/scaling, frozen/trainable state, objective, checkpoint и downstream consumer. Для rectangular projection явно описать пространство назначения и утраты; для discrete tokens нужен отдельный decoder/codebook, если это действительно требуется.

Проверка смысла: сохраняет ли представление различия, нужные следующему unit/flow/kNN/graph; улучшается ли end-task behavior относительно frozen projection/no-update. Хороший пример — после адаптации не смешиваются «разрешено» и «запрещено» при сохранении корректного downstream ответа. Плохой — loss снизился и shape совпал, поэтому объявить vectors правильными. Рассмотреть collapse, rotation/scale drift, OOV, domain switching и устаревший index.

## H07. Как измерять обучение от bulk к bulk без coverage-проекта

Во время этих 25 документных сессий не создавать и не запускать product tests/coverage/Windows runs. Проверка документов — смысловая трассировка, чтение источников, сценарное сопоставление good/bad, согласованность ссылок и diff; она не доказывает качество модели. Будущие учебные опыты используют реальные задачи, независимые панели, измерения и предметную экспертизу.

До опыта определить domain/task families, lineage split, baseline/challenger, единственный меняемый фактор, shared evaluation panel, metric rubric, допустимый риск/стоимость и правило решения. Обе версии состояния получают одну панель; новая панель также оценивается обеими. Разные по сложности bulks не сравниваются как будто это один эксперимент. Paraphrases одного источника не считать независимыми наблюдениями. Operational/dev panel, которая влияет на обновления, не называется нетронутой certification.

Минимальные наблюдения: per-domain correct/acceptable outcomes; domain mapping и abstention; качество соседей и конечного решения; допустимость/цена пути; DPO validity отдельно от update utility; representation/LoRA downstream effect; качество новых семейств; retention; feedback до обучения на текущем запросе. Показывать число независимых семейств, распределение/неопределённость, worst-domain и неудачи, а не только средний score.

Цена: wall time и latency quantiles по доступному объёму; inference/update/teacher/judge/retrieval/reindex/snapshot/rollback/failed-attempt time; peak RAM/VRAM и disk footprint модели, adapters, activations/optimizer, индекса, графа, cache и состояния. No-update control отличает эффект update от изменённого retrieval/контекста. Teacher-assisted качество не выдавать за автономное качество Student.

Исходы: IMPROVE, REGRESS, INCONCLUSIVE, INVALID_EVIDENCE. Численные thresholds/число семейств фиксирует R13 до результатов, с предметным обоснованием; не подгонять после опыта и не обещать статистическую определённость одной пары. Сохранённая конечная цель 95% к эталону не равна текущему score или confidence; ранний отрицательный опыт допустим и полезен как опровержение гипотезы.

Неизвестность «будет ли прирост» разрешается опытом. Неизвестность «какое состояние меняем, какой objective, какие права на данные, кто принимает результат» — design blocker, который должен быть решён до соответствующего опыта. Закрытие всех release-поставок не является универсальным условием первого урока.

## H08. ПК, непрерывное обучение и безопасная асинхронность

Профиль PC устанавливать по фактической конфигурации, не по давней памяти о компьютере пользователя. Временно неизвестные RAM/VRAM/CPU/storage отмечать как параметры, а не заявлять, что модель уже помещается. Предпочесть максимальное переиспользование существующего working owner и минимальный отсутствующий dynamic seam; не вводить новый параллельный trainer ради демонстрации.

Feedback stateflow: ответ на новый запрос до обучения → проверка допустимости коррекции и её provenance → новая пара → изолированное обновление → оценка → выбор состояния → обычное применение. Неверная, противоречивая, приватная или вредоносная коррекция не становится истиной автоматически. Retention проверяется наблюдением прежних способностей без скрытого повторного gradient на прежних парах.

Разделить inference snapshot и training candidate; определить consistency для units, adapters, graph и index. Не публиковать смесь версий и не начинать rollback, пока update имеет неизвестный terminal state. Квоты, backpressure, timeout и сохранение результата — ограничения исполнения, а не обучаемая семантическая власть. При росте memory/index/graph требуется измеряемая политика pruning/forgetting, которая не уничтожает нужную negative history без анализа.

## H09. Пакет модели, подзадачи и условия завершения

Исполнитель получает один заполненный prompt: назначенный Rxx; вопрос/не-задача; нужные H-разделы; точные source refs/spans; роль и реально применимые skills; current→target; конкретные шаги; результат; good/bad/edge; сегодняшняя проверка; будущий опыт/владелец; разрешённые файлы и запрещённые действия; ограничение нагрузки и handoff. Не отправлять только ссылки, все 25 задач или «сделай всё по архитектуре».

Skills в K/docs/skills: startup=`planning-session-startup-SKILL.md`; intent=`architect-behavior-classifier-SKILL.md`; sources=`reference-inventory-and-source-map-SKILL.md`; plan=`phase-planning-SKILL.md` и `plan-review-SKILL.md`; architecture=`trainable-architecture-decision-SKILL.md`; code=`generated-code-review-SKILL.md` и применимый `code-examination-SKILL.md`; prompt=`prompt-patch-authoring-SKILL.md`; human=`output-readability-gate-SKILL.md` и `human-readable-architecture-audit-SKILL.md`; experiments=`experiment-design-SKILL.md`/`improvement-measurement-SKILL.md` после проверки доступности нужных разделов. Список не считается чтением. Общие применимые зависимости сохраняются; исторические 30/50-review и чужая последовательность не перебивают нынешние 25 и параллельные R01–R03.

Для большого Rxx допустимы A и B **внутри того же номера**, не скрытые дополнительные раунды. Родитель разворачивает каждую часть в полный пакет выше. A получает свой source subset, один предметный вопрос, алгоритм работы и конкретную таблицу/цепь на выходе. B получает immutable результат A и первичные spans, проверяет отдельный контрпример и собирает итог. Good/bad/edge конкретного Rxx ниже обязательны обоим в относящейся части; одинаковая модель не становится независимым reviewer от буквы B. Нельзя принять пустой шаблон вместо заполненного поручения.

До чтения фиксировать границу новой информации и ёмкость: источники/диапазоны, новые code seams, число конфликтов, объём проверки/передачи. При превышении делить по этим единицам; не переносить весь остаток в R07/R10/R25. Не считать передачу прочтением. Сохранить five-carrier формат в собственном results/Rxx; не создавать пустые измерения. Настоящий дополнительный запуск отражается как изменение бюджета, а не прячется под «проверкой».

---

# Предметные дополнения ко всем 25 промптам

Ниже не сокращённая замена исходных поручений, а обязательная конкретизация алгоритмической части. **Контекст каждого пункта = его существующий полный Rxx + указанные H/P и результаты предшественников.** Все действия здесь документные; измерительные слова описывают будущий опыт, а не разрешение выполнить training сейчас.

## R01 — карта ролей и источников, независимо от готовности R02/R03

**Роль/skills:** sources, plan, human. **Контекст:** H01/H02/H09, P03–P05, деревья planning/core; существующий results/R01.

**Промпт A:** продолжи фактическую инвентаризацию; добавь каждую из четырёх ролей, её исторические aliases и текущие ILogical*/generic кандидаты как разные колонки. Для source family укажи конкретный путь, semantic span и владельца. Распредели source/code/measurement глубину, не объявляй найденный interface рабочим алгоритмом.

**Промпт B:** сопоставь карту с R02/R03 snapshot, когда он доступен; отсутствующий sibling не блокирует A. Новые независимые корни `llm dynamic flow`, orchestration и domain libraries назначь владельцам; не бери их полный аудит на R01.

**Выход/проверка:** source→роль→ожидание→читатель; сравнение с деревом и явные unread/unavailable. **Хорошо:** найден отдельный источник learned edge authority и передан R06. **Плохо:** четыре имени найдены, значит архитектура покрыта. **Edge:** переименованный файл, вложенный RAG-цикл, разные refs. **Далее:** R04–R06 получают точные code-search seeds; метрики H07 принадлежат R13.

## R02 — восстановить намерение без подмены API

**Роль/skills:** plan, architecture, human. **Контекст:** H01–H04, P01/P02/P08 и свой существующий source ledger; R01/R03 advisory, не prerequisite.

**Промпт A:** по исходным словам восстанови Unit/Flow/Orchestrator/Graph, many→many, разные combinations одного input, Builder и local feedback. Для каждого требования раздели функциональное ожидание, старую реализационную гипотезу и текущий code contract. Сохрани дополнительные домены/намерения/языки и endpoint consumers.

**Промпт B:** восстанови lineage конфликтов teacher/judge, one-pair/replay, train-ready unit-level/graph evaluation и aliases. Не решай спор сменой названия или стиранием старого требования.

**Выход/проверка:** trace matrix с первичным source, сохранённым ожиданием и наблюдаемым неуспехом. **Хорошо:** отношение Flow остаётся обязательством при другой текущей C# сигнатуре. **Плохо:** приравнять исходную опечатку к готовому IQueryable provider. **Edge:** оба кандидата корректны, но путь разный; домен×intent. **Далее:** R12 adjudication, R16 migration; R04 проверяет API.

## R03 — восстановить связи, а не только словарь

**Роль/skills:** sources, plan, architecture. **Контекст:** H01/H03–H05, P05–P08, свой CP; не ждать завершения R01/R02.

**Промпт A:** пройди текущие RAG/NDJSON и вложенные маршруты к первичным правилам domain filtering, arbiter, negative memory, deterministic baseline/learned authority, replay и графовым порядкам. Храни visited key repo/ref/path и необходимость прочтения конкретного span.

**Промпт B:** для каждого восстановленного решения покажи источник→действующий преемник/конфликт→алгоритм/стык→владелец. Укажи, где нужна свежая core trace вместо июльского READY/CODE-ABSENT.

**Выход/проверка:** первичные маршруты и conflict list; достаточно проследить используемые утверждения, нельзя объявлять остальной подграф прочитанным. **Хорошо:** RAG→amendment→граница final arbiter. **Плохо:** threshold из истории назначен текущим optimum. **Edge:** moved source, циклическая ссылка, исторический единственный носитель. **Далее:** R06 получает kNN/graph; R07 connections; R12 policy conflicts.

## R04 — типизированное представление и many→many

**Роль/skills:** code, architecture, plan. **Контекст:** H02/H05/H06, P01, source spans R01–R03 и S00–S03.

**Промпт A:** проследи реальные producers, schemas, tokenizer/parser, encoder/projection, output enumerator и consumer. Зафиксируй реальные методы, материализацию и координатные версии. Сопоставь исходные Learning* роли с текущим кодом; не создай новое API из текста плана.

**Промпт B:** составь различающий опыт frozen representation против одного изменения на смысловых контрастах. Отдели набор результатов от ranking и от превращения многих input в один усреднённый vector.

**Выход/проверка:** current/gap map и контракт портов. **Хорошо:** отрицание и числовая единица доходят до consumer. **Плохо:** одинаковая shape названа смысловой совместимостью. **Edge:** empty input/output, OOV, variable cardinality, duplicate enumeration, incompatible schema. **Далее:** R05 получает adapter boundary; R06 coordinate/index constraints; будущие метрики — downstream accuracy, relation fidelity, bytes/latency.

## R05 — от валидной пары до причинного изменения ответа

**Роль/skills:** code, architecture, experiments. **Контекст:** H02/H04/H06/H08, R04 и S04–S06, P08.

**Промпт A:** проследи GraphDpoPair producer, owner update, objective/reference, LoRA target matrices, snapshot, promotion и следующий inference consumer. Для каждого звена различи хранение/память/градиент/наблюдаемую пользу. Проверь, какие relation signals реально достижимы через текущий unit-level owner.

**Промпт B:** спроектируй no-update/update сравнение при одинаковом representation/retrieval; добавь causal attribution и retention. Выпиши точный отсутствующий seam, если trace разрывается.

**Выход/проверка:** code chain + матрицы/shape/objective/state boundary + минимальный будущий опыт. **Хорошо:** показано, как learned state меняет ordinary consumer. **Плохо:** изменился hash, значит DPO работает. **Edge:** BOTH_BAD/TIE, missing reference, repeated pair, unknown terminal update, LoRA/index version mismatch. **Далее:** R07 connections, R12 objective choice, R18 smallest implementation order.

## R06 — домен, динамические соседи и паттерны разных порядков

**Роль/skills:** code, architecture, experiments. **Контекст:** H02/H03/H05, R03 P07, R04/R05 contracts, S07–S10.

**Промпт A:** укажи domain×intent representation, допустимый candidate pool, distance features/weights, k/radius, learned selector и final arbiter. Проследи graph relation orders, node ports и actual consumers. Нельзя назначить один cosine всем доменам без сравнения.

**Промпт B:** подготовь actual/oracle-domain diagnostic, fixed/dynamic kNN и fixed/higher-order graph comparisons с frozen остальными частями. Раздели retrieval memory, learned metric и learned routing.

**Выход/проверка:** паспорта mechanism/edge authority и конкретные экспериментальные контрасты. **Хорошо:** близкая, но ранее запрещённая регистрация остаётся negative/warning. **Плохо:** nearest text принят независимо от домена и правил. **Edge:** mixed/unknown domain, nonmetric score, empty compatible pool, stale index, graph cycle. **Далее:** R07 credit assignment; R08 hard negatives; R13 quality/cost comparison.

## R07 — repaired hybrid architecture: deterministic + learned + DPO + domain laws

Полный обязательный work order: `R07-REPAIR-WORK-ORDER.md`.

**Роль/skills:** architecture + planning + plan/code review + prompt engineering + human-readable architecture + experiment design. Не принимать sibling summary за direct evidence.

**Prompt A — architecture reconstruction.** Пройди relevant RAG/WorkLeft/original decisions и current code. Соедини Unit→Flow→Orchestrator→DynamicGraph с domain mapping, domain-sensitive token/vector representation, BM25+kNN, dynamic k/radius/weights, DPO/LoRA state, feedback и ordinary consumer. Для каждой связи: schema/version/multiplicity/association, deterministic constraint, learned authority, mutable state, consumer, fallback, cost и falsifier.

**Prompt B — domain representation / kNN / DPO.** Для каждого baseline domain сформулируй domain law, необходимые tokens/features, hard negatives, OOD/unknown behavior и candidate dynamic kNN rule. Проследи exact DPO pair до response tokens/objective/LoRA state/accepted checkpoint и обратно до ordinary caller. CODEX/Astra 6 ultracode выполняет human lesson selection и exact-pair judgment; external open Chinese model authors both chosen/rejected.

**Prompt C — causal measurement.** Подготовь no-update/update, fixed-candidates/changed-selector, fixed-selector/changed-domain-pool, frozen-Unit/changed-Flow, flat/higher-order graph, representation A/B, kNN A/B и interaction 2×2. Мерить semantic accuracy, domain mapping, valid path/association, time, RAM/VRAM, persistent bytes, latency, retention. Tests/coverage/Windows CI не являются learning efficacy evidence.

**Good:** deterministic admissibility frozen; learned selector changes; unseen-family quality and cost improve, ordinary caller consumes accepted child. **Bad:** одновременно поменять tokenizer+kNN+LoRA+graph и приписать reward всем. **Edges:** mixed domain, TIE/BOTH_BAD, stale checkpoint, shape-compatible semantic mismatch, async reorder/cancel, 1→0/1→N/N→M, adversarial feedback, no useful neighbor.

**Выход:** repaired R07 five-carrier packet + exact R12/R13/R18 handoffs. Current status до этого: R07_REPAIR_REQUIRED.

## R08 — данные, законы и трудные контрпримеры

**Роль/skills:** sources, experiments, architecture. **Контекст:** H04/H05/H07, P08/P09, R02 domain×intent catalog и R05–R07 constraints.

**Промпт A:** дополни существующий WorkLeft research паспорта предметов, источники законов/предпочтений, data rights, lineage families и known/mixed/new случаи. Для каждого proposed vector/kNN feature назови сохраняемый смысл и исходный пример.

**Промпт B:** наметь positive/plausible-negative/TIE/BOTH_BAD/unknown случаи и путь независимой оценки. Проверяй, что near-duplicates одного источника не распределены между train и certification. Не генерируй новый обучающий корпус в этом раунде.

**Выход/проверка:** domain-law-feature-negative-observation matrix. **Хорошо:** SQL×BUG и SQL×DOC различены без создания взаимоисключающих доменов. **Плохо:** семь названий выданы за семь готовых корпусов. **Edge:** contradictory feedback, multilingual negation, multi-solution, changing domain rules. **Далее:** R13 thresholds; R21 curriculum; Builder оценивается будущей пользой Student.

## R09 — стоимость полезной информации, а не число jobs

**Роль/skills:** sources, code review, plan. **Контекст:** H03/H07/H08, WorkLeft process и существующие commits/logs.

**Промпт A:** на обоснованной выборке раздели time-to-baseline/update/evaluation, queue/build/Windows fixture repair, повторное чтение и реальные data/algorithm gaps. Для подозрения о test-fitting прочитай requirement→assertion→implementation diff.

**Промпт B:** у каждого дорогостоящего действия назови защищаемое свойство и минимальное достаточное evidence; сравни варианты reuse/defer/retain без запуска нового job.

**Выход/проверка:** причинная cost/dependency таблица с границами выборки. **Хорошо:** release-only блокер отделён от snapshot corruption риска. **Плохо:** все checks объявлены вредными по одному названию коммита. **Edge:** unknown duration, retries, teacher latency, billing not rollbackable. **Далее:** R11/R17; будущий показатель — полное время до достоверного learning delta, а не green jobs.

## R10 — сведение с сохранением незакрытых решений

**Роль/skills:** plan, architecture, human. **Контекст:** H01–H09 по дельтам, точные результаты R01–R09.

**Промпт A:** сведи source→functional role→mechanism→state→signal→observation и competing hypotheses. Сопоставь разные snapshots R01–R03; перечитай только противоречащие изменённые spans.

**Промпт B:** адресно закрой назначенные gaps либо запиши точный blocker/owner/capacity. Не превращай R10 в повтор всех исследований; отсутствие эмпирического gain не является отсутствием спроектированного опыта.

**Выход/проверка:** единый evidence brief для R11–R15, включая rejected hypotheses. **Хорошо:** replay conflict передан с обеими первичными нормами. **Плохо:** latest file wins без смысловой сверки. **Edge:** один sibling ещё IN_PROGRESS; конфликт влияет лишь на конкретный seam. **Далее:** R12 получает решения, не ещё один запрос «исследовать всё».

## R11 — causal first lesson vs full delivery — REPAIRED

**Governing executable prompt:** [R11-REPAIR-WORK-ORDER.md](R11-REPAIR-WORK-ORDER.md). Не выполнять прежнюю короткую карточку отдельно.

R11 строит **два причинных графа**, а не общий checklist: FIRST-LESSON до ordinary consumer + semantic before/after examination и FULL-DELIVERY для всех сохранённых DPO/DynamicGraph/DTC/IDE/domain obligations. Он обязан найти minimal cut-set реальных blockers первого learning signal и у каждой отложенной обязанности сохранить source/ID, owner, trigger и later examination.

В R11 явно сохраняются:
- CODEX + Astra 6 UltraCode = human lesson selector **и** отдельный exact DPO pair judge;
- внешняя открытая китайская модель = автор обеих final chosen/rejected sides;
- ILearningUnit → ILearningFlow → ILearningOrchestrator → ILearningDynamicGraph, ILearningMachine training owner;
- domain mapping отдельно от solver; dynamic kNN eligibility/metric/k/neighbor-consumer отдельно; vector/token meaning/version отдельно; LoRA parameter target/consumer отдельно;
- trial evaluator reload отдельно от ordinary accepted-state adoption;
- deterministic hard guards отдельно от nondeterministic learning efficacy.

**R11 не требует coverage/Windows/Actions как proof of learning.** Future functional examination: frozen baseline/challenger, independent domain families, no-update control, hard-law semantic metrics, mapping/path/retrieval diagnostics, retention, time/latency/peak RAM-VRAM/persistent size и bulk-to-bulk curves новых уроков.

Полные context/skills/RAG/WorkLeft refs, prompts A–E, good/bad examples, edge cases, bounded child prompts, acceptance и return schema находятся в governing work order и обязательны для dispatcher. R12 получает только concrete architecture decisions; R13 — causal experiment variables; R14 — preserved backlog ordering; R17 — acceptance migration; R18 — minimal implementation seams.

## R12 — выбрать минимальную архитектуру, а не придумать её заново

**Роль/skills:** architecture, plan review, code review. **Контекст:** H02–H08, R04–R11 и обе стороны source conflicts.

**Промпт A:** сравни существующий путь, минимальный missing-seam repair и более широкий redesign. Реши mapping Learning*/ILogical*, owners, port schemas, learned edge authority, dynamic kNN state, LoRA target/objective и DPO/reference path. Для каждой альтернативы укажи что переиспользуется и какой вывод её опровергнет.

**Промпт B:** явно разреши replay/one-pair semantics и unit-level/graph-signal gap; не меняй owner policy без полномочия. Выбери безопасный обратимый design там, где это техническое решение; только настоящую product/risk choice вынеси отдельно. Составь one-input/two-combinations сценарий.

**Выход/проверка:** solved architecture с current→target, отвергнутыми альтернативами и remaining implementation seams. **Хорошо:** минимальный adapter со стабильным consumer вместо второй системы. **Плохо:** «всё dynamic» без state/objective. **Edge:** нельзя выразить preference через текущий owner; non-differentiable score. **Далее:** R13/R16/R18.

## R13 — causal experiment compiler, not generic protocol

**Governing prompt:** [R13-REPAIR-WORK-ORDER.md](R13-REPAIR-WORK-ORDER.md).

R13 MUST combine the latest R01–R06 findings into experiment consequences, reopen the relevant primary WorkLeft/RAG/original DPO/dynamic-flow sources, and fill one real first-experiment card. E1–E4 labels alone are insufficient.

The selected experiment names domain/families, deterministic guards, learned owner/state/signal, actual ordinary consumer, baseline/challenger/no-update control, frozen factors, one changed factor, pair/signal lineage, representation/index/objective versions, panel/metric, falsifier, retention, full cost and branch after each outcome.

The queued interactions must explicitly cover:
- exact DPO pair integrity/objective/adoption;
- domain mapping vs solver;
- token/vector semantics and index compatibility;
- dynamic-kNN/reranking with actual consumer;
- LoRA/projection when causally relevant;
- frozen-Unit Flow relation learning;
- Orchestrator path/branch learning with all-branch cost;
- DynamicGraph unseen-composition transfer;
- compact PC continual learning and feedback.

Learning efficacy is examined by functional before/after evidence and bulk-to-bulk new lessons, never by coverage/Windows/CI/loss/hash alone. Current status is **R13_REPAIR_REQUIRED / LEARNING_NOT_MEASURED**.

## R14 — capability-first whole-program order

**Governing prompt:** [R14-REPAIR-WORK-ORDER.md](R14-REPAIR-WORK-ORDER.md). It supersedes the older short R14 wording.

R14 now reconstructs the entire program as M0–M7 capabilities: first valid lesson; second structurally different domain; domain-aware mapping/representation/retrieval; Flow; Orchestrator; DynamicGraph transfer; compact PC continual learning/feedback; then retained DTC/IDE/native/host delivery.

It explicitly preserves the combined deterministic+learned design, domain-sensitive vector/token semantics, dynamic-kNN hypotheses, LoRA/projection, Unit→Flow→Orchestrator→DynamicGraph, and the DPO role split: **CODEX GPT-6 Astra UltraCode = human lesson selector + separate exact-pair judge; external open Chinese model = author of both final chosen/rejected sides.**

R14 does not use Windows/coverage as learning evidence and does not delete deterministic/native safeguards. Exact R12/R13-dependent first-seam fields remain provisional; this does not block whole-program capability migration.

Published result: [results/R14/FINDINGS.md](results/R14/FINDINGS.md) + four sibling carriers.

## R15 — независимая предметная экспертиза направления

**Роль/skills:** отдельный назначенный Astra Ultra reviewer, architecture/experiments/plan review. **Контекст:** exact R12–R14 candidate и первичные spans H01, не пересказ автора.

**Промпт A:** самостоятельно разбери один полный путь Unit→Flow→Orchestrator→Graph и один правдоподобный failing/retention случай. Ищи потерю функциональных ролей, неподходящий DPO objective, leakage, неправильную kNN власть и необоснованную компактность.

**Промпт B:** оспорь наиболее важную гипотезу альтернативным объяснением; реши, какой минимальный опыт различит их. Замечание содержит consequence, precise repair и affected consumer, не «улучшить качество».

**Выход/проверка:** candidate-bound review: допустить следующий опыт/исправить конкретный design blocker. **Хорошо:** false gain объяснён сменой retrieval. **Плохо:** авторское B названо независимым Ultra. **Edge:** reviewer недоступен — NOT_PERFORMED; данные ещё не измерены — не выдумывать. **Далее:** R16–R20 исправляют только найденное, без автоматических 30/50 reviews.

## R16 — применить сохранённые функциональные требования

**Роль/skills:** plan, prompt, architecture. **Контекст:** R02/R12/R13/R15, H02–H08, точные live requirements.

**Промпт A:** применяй source→old→new изменения со стабильными IDs: четыре роли, typed many→many, connection learning, domain laws, representations/kNN, DPO/update, PC и feedback. Отдели readiness for experiment, observed effect и release.

**Промпт B:** проверь обратную трассировку каждого изменения и его consumer. Для policy/payload migration зафиксируй explicit decision, compatibility и какие downstream документы должны измениться; исторические слова владельца не переписывай.

**Выход/проверка:** реальные применённые требования и obligation map. **Хорошо:** unit-level gap явно отражён до разрешённой миграции. **Плохо:** graph learning удалено ради существующего payload. **Edge:** unresolved genuine owner policy — блокирует зависимую миграцию, не все независимые документы. **Далее:** R17–R20; функциональная приёмка привязана к R13.

## R17 — заменить неверные критерии прогресса

**Роль/skills:** plan review, code review, prompt. **Контекст:** R09/R11/R13/R16, H07/H08, live gates.

**Промпт A:** перепиши именно planning clauses, где coverage/Windows green подменяет learning quality. Для каждого типа изменения назови предметное наблюдение и необходимую среду. Не удаляй существующие product tests/workflows.

**Промпт B:** проверь, что no-repeat/state/provenance ограничения сохранены, а повторное измерение требует новой гипотезы и лимита общей стоимости. Реальные изменения CI вне scope обозначь отдельным незавершённым engineering task.

**Выход/проверка:** применённая acceptance matrix, показывающая что снимается с первого критического пути и что защищается иначе. **Хорошо:** renderer release не блокирует unit learning. **Плохо:** пропущенный check назван PASS. **Edge:** production correctness constraint нужен для доменной допустимости. **Далее:** R18/R19; никакого нового coverage-проекта.

## R18 — точный work order минимальной реализации

**Роль/skills:** architecture, code review, prompt/human. **Контекст:** R04–R07/R12–R17, H02–H08, фактический code owner.

**Промпт A:** сначала выпусти один smallest work order: repo/ref/file/class/method, current→target, source obligation, input/output, learned/deterministic seam, changed state, actual consumer, allowed files и прежний code reuse. Не сочиняй команду trainer и не создавай duplicate pipeline.

**Промпт B:** приложи заполненный implementation prompt и отдельный examination prompt на R13 recipe: initial state, ожидаемое наблюдение, falsifier, ресурсный stop и rollback. Следующие seams дели по unit projection, flow relation, router/kNN, orchestration/graph, а не одним «внедрить AI».

**Выход/проверка:** первая готовая пара implementation/examination поручений плюс зависимая очередь. **Хорошо:** один отсутствующий port adapter открывает опыт. **Плохо:** все функции переписать до первого урока. **Edge:** code уже эквивалентен — reuse, не повтор. **Далее:** исполнение только при текущем разрешении; R19 применяет очередь, R22 расширяет опыты.

## R19 — применить learning-first roadmap без потери поставок

**Роль/skills:** plan, prompt, human. **Контекст:** R14/R16–R18, все live queues/start entries, H08.

**Промпт A:** примени reused/necessary-now/deferred/replaced для всех исходных обязательств и слайсов. Введи milestones первого baseline, разрешённой пары/update, достоверной оценки, иного домена, связей, compactness/feedback, затем необходимых delivery features.

**Промпт B:** пройди реальные entrypoints и dependency graph: новый START должен вести к минимальному work order, не старому blanket S04 repair. Deferred API/DB/render/IDE имеет owner и trigger.

**Выход/проверка:** изменённый действующий roadmap и no-cycle trace. **Хорошо:** готовый первый опыт не ждёт заполнения всего curriculum. **Плохо:** R25 стал универсальным запретом учиться даже после разрешённого достаточного R18 packet. **Edge:** REGRESS после урока возвращает к конкретной гипотезе. **Далее:** R20 consumer prompts.

## R20 — собрать исполнимые prompts и исключить конфликтующие инструкции

**Роль/skills:** prompt, plan review, human, architecture. **Контекст:** H09, R16–R19, actual startup/continuation/role templates.

**Промпт A:** собери на каждый следующий шаг один полностью заполненный model packet; включи конкретные context spans, skills, RAG root, current→target, good/bad/edge, permitted files, результат и проверку. Убери повторную обязательную загрузку одинаковых карточек, не содержание.

**Промпт B:** вручную смоделируй выбор задачи для параллельного R03, минимальной реализации R18 и отрицательного учебного результата. Убедись, что каждый получает правильного owner и не default Windows rerun.

**Выход/проверка:** live consumer edits и три смысловые dispatch traces. **Хорошо:** child получает заполненные refs и отдельный seam. **Плохо:** «прочитай весь репозиторий» или ссылки без контекста. **Edge:** недоступный skill, ref changed, stale installed overlay. **Далее:** R21–R25; вне-scope compiler/skill changes не объявлять применёнными.

## R21 — предметный curriculum и качество Builder

**Роль/skills:** experiments, architecture, human. **Контекст:** R08/R13/R16, P08/P09, H04/H05.

**Промпт A:** дополни существующие domain passports: смысловой закон, representation, distance candidate, простая/неоднозначная/составная/новая задача и plausible negative. Сохрани семь baseline доменов и дополнительные исходные задачи.

**Промпт B:** для Builder спроектируй как измерять не только accept-rate/разнообразие пар, но пользу Student на независимых семействах при одинаковом update бюджете. Слабая пара не становится полезной из-за красивого rationale.

**Выход/проверка:** конкретный curriculum с rights/lineage и минимальным начальным подмножеством. **Хорошо:** EN→RU negative теряет отрицание; SQL negative нарушает нужную JOIN семантику. **Плохо:** rejected бессмыслица. **Edge:** multi-correct, BOTH_BAD, mixed-domain, feedback conflict. **Далее:** R22; первый узкий опыт не ждёт полного каталога.

## R22 — различающие эксперименты алгоритмов и их соединений

**Роль/skills:** experiments, architecture, code review. **Контекст:** R13 initial recipe/R18 real seam/R21, H03–H08.

**Промпт A:** уточни первый recipe реальными entrypoints и версиями данных/state. Затем задай условные следующие сравнения: representation; fixed/dynamic kNN; no-update/DPO; frozen/LoRA; fixed/adaptive connection; higher-order path; feedback/retention. Не запускай их сейчас.

**Промпт B:** назначь для каждого один меняемый фактор, одинаковую панель и quality/time/memory decision; interaction 2×2 только при причинной необходимости. Составь очередь по результату предыдущего опыта, не по полному перебору.

**Выход/проверка:** executable experiment cards либо exact prerequisite code gap. **Хорошо:** gain исчез без learned edge — полезный falsifier. **Плохо:** одновременно сменены tokenizer, k, dataset и LoRA. **Edge:** insufficient evidence, invalid baseline, nondeterministic variance. **Далее:** R23 resources/R24 users; все measured поля пока NOT_MEASURED.

## R23 — компактность и обучение на пользовательском вводе

**Роль/skills:** architecture, experiments, code review. **Контекст:** R09/R12/R22, H04/H08, реальные PC/state manifests.

**Промпт A:** рассчитай структуру бюджета, не выдумывая фактические значения: модель/adapters/optimizer/activations/index/graph/snapshots/cache; peak и growth при feedback. Назови, кто владеет quota/pruning/consistency.

**Промпт B:** распиши prequential feedback lifecycle, conflict admission, isolated update, unknown termination, promotion/rollback и retention без скрытого replay. Сравни полную цену с reference, включая teacher/judge и неудачные попытки.

**Выход/проверка:** ресурсный envelope и состояние переходов. **Хорошо:** small adapter не скрывает большой index. **Плохо:** «помещается на ПК» по размеру весов. **Edge:** memory pressure, poisoned correction, interrupted update, conflicting concurrent inputs. **Далее:** R24/R25; необратимые расходы/раскрытие данных не считаются откатанными.

## R24 — реальный пользовательский результат четырёх ролей

**Роль/skills:** human, architecture, experiments. **Контекст:** R02/R06/R16/R21–R23, H02/H07/H08, исходные DTC/IDE promises.

**Промпт A:** опиши end-to-end сценарии observation: request→domain→representation→units/flow/path→answer→feedback→новый ответ, плюс ordinary consumer выбранного checkpoint. Для DTC сохрани API, дерево/HTML/CSS/images, разрешения, DB, публикацию и user-tier budget.

**Промпт B:** сопоставь ожидание с будущими per-domain metrics и неуспехом пользователя; software test suite не создавай. Отдели headless learning evidence от host delivery evidence.

**Выход/проверка:** человеческие scenario cards с current/expected и owning code seam. **Хорошо:** правильный более дешёвый путь сохранил качество на новом запросе. **Плохо:** файл весов опубликован, значит IDE уже использует его. **Edge:** stale checkpoint, partial async output, new language, tariff cap. **Далее:** R25 independent handoff.

## R25 — независимая финальная передача, не новый круг исследования

**Роль/skills:** отдельный Astra Ultra reviewer, plan/architecture/experiments. **Контекст:** exact final candidate, actual R01–R24 records, R15 repairs, H01–H09 адресно.

**Промпт A:** пройди source→четыре роли→алгоритм/state→пара/objective→опыт→consumer; проверь, что потерянные решения R02/R03 действительно применены, а не только процитированы. Обнаружь unresolved design blockers и необоснованные implementation claims.

**Промпт B:** воспроизведи по документам один первый implementation/examination packet и отрицательный сценарий. Итог должен назвать конкретное следующее действие, а не «нужен ещё полный аудит»; эмпирическая неизвестность с готовым опытом не блокирует постановку опыта.

**Выход/проверка:** candidate-bound verdict, limitations и точный handoff. **Хорошо:** next seam + same-panel experiment + rollback/decision. **Плохо:** 25 файлов названы 25 выполненными сессиями. **Edge:** missing reviewer или unfinished prerequisite — честный частичный статус, никакой фиктивной независимости. R26 автоматически не возникает.

## Граница подготовки этого уточнения

Прочитаны доступные результаты R01/R02/R03, исходная формулировка ролей, разделы предыдущих раундов, действующие входы/очередь/маршруты и адресные разделы восьми core skills (startup, classifier, plan review, generated-code review, prompt patch, trainable architecture, human architecture, output readability). Это не полный skill load, не полный обход всех WorkLeft/RAG и не новый аудит всего core. Источники P06/P07, восстановленные R03, требуют первичной проверки назначенными владельцами там, где на них опирается новое решение.

Эта редакция публикует уточнённые поручения. Она не меняет results/R01–R03, код, tests, workflows, datasets, weights или core skills; не выполняет training/Windows/Actions и не заявляет отдельную Astra Ultra рецензию. Все будущие gains, latency и PC-fit остаются NOT_MEASURED.
