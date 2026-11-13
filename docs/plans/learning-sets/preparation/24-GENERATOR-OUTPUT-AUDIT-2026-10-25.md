# Аудит созданных учебных данных и поведения генератора

**Уточнение последующей проверкой:** [аудит новых партий и исправление пропуска в VALIDWORD](25-GENERATOR-FOLLOWUP-AUDIT-2026-10-25.md). В прежнем чтении десяти строк `TR07-VALIDWORD/ru-en/M006` не был выявлен уход от назначенной цели в строках 6–10. Исходный срез и остальные доказательства ниже сохранены; фраза об отсутствии дополнительных дефектов для этой партии исправлена в §5.

25 октября 2026. **Это аудит созданных DPO-кандидатов и поведения генератора, не аттестация обученных моделей.** Фаза — READ_ONLY_RESEARCH, этот отчёт и перечисленные в §8 плановые Markdown-исправления. Автор участка: `/root/learning_set_quality`; это авторский исследовательский пакет, не независимый review собственного отчёта.

**Основа исправления по уточнению Любы — хорошие и плохие примеры плюс простая понятная логика, а не усиление запретов.** Адресная шпаргалка: [TYPO-AUTHORING-EXAMPLES-RU](prompts/TYPO-AUTHORING-EXAMPLES-RU.md). Примеры должны помогать автору получить правильную строку сразу; отдельный reviewer, oracle или новый процесс не становятся входным условием CREATE_ONLY.

## 1. Срез, происхождение копий и реальные количества

Инвентарный срез: **2026-10-25 15:30:49 UTC**. Во время чтения исполнитель добавил одну партию, `TR07-VALIDWORD/ru-en/M006`; она дочитана и включена в ограниченный участок. Новые партии после неё не разыскивались. Это не мониторинг живого исполнителя.

| Поверхность | Что установлено |
|---|---|
| `C:/Projects/llm_mvp_core/learning-sets` | 225 файлов дерева на момент среза; 17 draft-файлов, **181 строка = 171 CREATE_ONLY + 10 legacy accepted** |
| Библиотечные outputs в `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets` | 9 запусков, 101 строка, 81 файл; побайтно совпали с соответствующими core-копиями |
| Split | Все 181 draft-строка имеют `train`; в имеющемся legacy export DEV/EXAM пусты |
| Объявленные семьи | 71 различный `family_id` в закрытом участке; это **не** 71 проверенное независимое семейство |
| Состояния CREATE_ONLY | `DRAFT_CREATED`, смысловое качество не установлено, review не запрошен, обучение не запускалось |

Библиотечные 101 строка **не прибавляются** к 181: это сохранённые копии, а не дополнительные независимые результаты. Девять библиотечных запусков: шесть профилей TR07-OMISSION, CODE-CS-NULL/base/M003, DOC-CS-NULL/base/M002 и OR-ROUTE-INTENT/base/M004.

Коллизия пути сохранена явно: библиотечный `TR07-OMISSION/ru-en/M001` соответствует core-папке `TR07-OMISSION/ru-en/M001-create-only`. Core `TR07-OMISSION/ru-en/M001` занят другой, более ранней accepted-партией. Это объяснено в [TRANSFER-NOTE:24](C:/Projects/llm_mvp_core/learning-sets/TRANSFER-NOTE.md:24); старый результат не перезаписан.

Плановая база исходного аудита — `e9489da5ea9ce36c4f96a91f5f8a78062f26e3d6`, сообщённая родителем как проверенная локально и удалённо. Во время подготовки отчёта родитель сообщил о локальной актуализации до `746910ce71` (`learning sets update`), выполненной внешним параллельным процессом. Это не новый срез данных и не утверждение о неизменности HEAD. Автор и родитель не приписывают себе этот commit/push; автор отчёта Git-записей не выполнял.

## 2. P2: пять строк нарушают описание операции

Это дефекты кандидатных данных, а не утверждение об уже выполненном обучении на них. Четыре строки не соответствуют обещанной единственной операции над исходным предложением; ещё одна содержит неправильный индекс при правильном фактическом удалении.

| Где | Pair ID | Наблюдение и эффект |
|---|---|---|
| TR07-OMISSION/ru-he/M001, строка2: [core](C:/Projects/llm_mvp_core/learning-sets/TR07-OMISSION/ru-he/M001/draft.jsonl:2), [библиотека](<C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-he/M001/draft.jsonl:2>) | `TR07-OMISSION.ru-he.F0000002.P01` | `письмо → пильмо`: длина6→6. Это замена «с» на «л», а не удаление. Metadata и rationale называют `delete_one_codepoint`, index2; фактическое удаление дало бы `пиьмо`. Сам prompt обещает «пропущена ровно одна буква», поэтому задача противоречит своему условию. |
| TR07-OMISSION/ru-he/M001, строка1: [core](C:/Projects/llm_mvp_core/learning-sets/TR07-OMISSION/ru-he/M001/draft.jsonl:1), [библиотека](<C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-he/M001/draft.jsonl:1>) | `TR07-OMISSION.ru-he.F0000001.P01` | `Окно → Кно`: удаление index0 даёт `кно`; дополнительно изменён регистр. Проверка только длины эту вторую операцию не заметит. |
| TR07-OMISSION/en-he/M001, строка4: [core](C:/Projects/llm_mvp_core/learning-sets/TR07-OMISSION/en-he/M001/draft.jsonl:4), [библиотека](<C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/en-he/M001/draft.jsonl:4>) | `TR07-OMISSION.en-he.F0000005.P01` | `market → maket` действительно удаляет букву, но индекс «r» равен2 при нумерации с нуля. Указанный3 даёт `maret`. Предпочтение переводов не перевёрнуто; ошибочны описание операции и индекс. |
| [TR07-TRANSPOSE/ru-en/M005/draft.jsonl:6](C:/Projects/llm_mvp_core/learning-sets/TR07-TRANSPOSE/ru-en/M005/draft.jsonl:6) | `TR07-TRANSPOSE.ru-en.M005.P06` | Prompt содержит `Возьми рукча и запиши адрес.`, canonical source — `Возьми ручку и запиши адрес.`, а операция применена к словарной форме `ручка`. Для формы `ручку` swap[2,3] даёт `рукчу`. До заявленного правильного предложения требуется ещё изменить окончание. |
| [TR07-TRANSPOSE/ru-he/M005/draft.jsonl:6](C:/Projects/llm_mvp_core/learning-sets/TR07-TRANSPOSE/ru-he/M005/draft.jsonl:6) | `TR07-TRANSPOSE.ru-he.M005.P06` | Та же потеря падежной формы: `ручка → рукча` механически верно само по себе, но не является единственной перестановкой в предложении с `Возьми ручку`. |

У последних двух строк chosen переводит предмет как pen/עט, а rejected — как pencil/עיפרון. Эта предметная граница понятна, но вход уже содержит дополнительную грамматическую ошибку. Следовательно, finding касается не только служебной metadata: студенту предъявлена задача, не соответствующая обещанному одному повреждению.

Минимальная плановая коррекция: на положительном и отрицательном примере показать выбор реальной формы слова из предложения, затем применение одной операции. Для удаления полезно сначала сравнить длины, затем сам результат удаления указанного code point. Для перестановки — равная длина и точное совпадение после обмена двух соседних code points. Проверка длины необходима, но недостаточна: `market/maket` с неправильным индексом и дополнительная смена регистра показывают почему.

Данные, их IDs и история в рамках аудита не исправлялись. Авторская шпаргалка не превращает эти строки в принятые или обученные.

## 3. P2: номер семьи ошибочно подменяет общий сценарий

Одинаковый origin в TR07-OMISSION объединяет различные сюжеты. Это назначено requests, поэтому finding нельзя приписать только самовольному выбору IDs генератором.

| Request и строка результата | Общий назначенный origin | Фактический канонический сценарий |
|---|---|---|
| [en-ru/M001/request.json:24](C:/Projects/llm_mvp_core/learning-sets/TR07-OMISSION/en-ru/M001/request.json:24), [draft:1](C:/Projects/llm_mvp_core/learning-sets/TR07-OMISSION/en-ru/M001/draft.jsonl:1), pair `TR07-OMISSION.en-ru.F0000001.P01` | `v3/tr/TR07-OMISSION/F0000001` | `The window is open.` |
| [en-he/M001/request.json:24](C:/Projects/llm_mvp_core/learning-sets/TR07-OMISSION/en-he/M001/request.json:24), [draft:1](C:/Projects/llm_mvp_core/learning-sets/TR07-OMISSION/en-he/M001/draft.jsonl:1), pair `TR07-OMISSION.en-he.F0000001.P01` | Тот же | `The bread is on the shelf.` |
| [he-en/M001/request.json:24](C:/Projects/llm_mvp_core/learning-sets/TR07-OMISSION/he-en/M001/request.json:24), [draft:1](C:/Projects/llm_mvp_core/learning-sets/TR07-OMISSION/he-en/M001/draft.jsonl:1), pair `TR07-OMISSION.he-en.F0000001.P01` | Тот же | `הכסא שבור.` — стул сломан |

В каждом из этих requests строки25–29 назначают одинаковые origin/leakage и правило `New independent scenario, not a renamed public/prior example.`. У contract только три поля: `origin_family_id`, `leakage_group_ids`, `scenario_rule`; общего канонического сюжета или ссылки на него нет. [en-ru/prompt.md:65](C:/Projects/llm_mvp_core/learning-sets/TR07-OMISSION/en-ru/M001/prompt.md:65) велит копировать origin/leakage из request и сохранять родственность переводов.

Под тем же F0000001 в других профилях находятся машина на улице и молоко в холодильнике. Это не переводы одной ситуации. Вместе с тем готовый номер сам по себе не даёт генератору недостающего общего содержания.

Положительный контрпример уже есть в TR07-TRANSPOSE: F0000000 во всех шести профилях относится к одной ситуации «Купи молоко по дороге домой». Такая связь соответствует семантике происхождения.

Минимальная плановая коррекция — передавать вместе с назначенной семьёй её общий сценарий или явную связь с исходником, используя существующий family handoff при будущем назначении. Пример должен отличать общий origin от совпавшего номера. Нельзя временно «оздоровить» статистику произвольным переименованием семей без восстановления родства.

## 4. P2: один C#-исходник и его объяснение объявлены независимыми

Три chosen-метода из code-партии дословно присутствуют как исходный код в docs-prompts. При этом их origin/leakage groups разделены по темам:

| Code row / pair ID | Docs row / pair ID | Общий код |
|---|---|---|
| [M003:4](C:/Projects/llm_mvp_core/learning-sets/CODE-CS-NULL/base/M003/draft.jsonl:4), `CODE-CS-NULL.base.F0000003.P01` | [M002:3](C:/Projects/llm_mvp_core/learning-sets/DOC-CS-NULL/base/M002/draft.jsonl:3), `DOC-CS-NULL.base.F0000004.P01` | `string Name(string? s) { if (s is null) return "гость"; return s; }` |
| [M003:5](C:/Projects/llm_mvp_core/learning-sets/CODE-CS-NULL/base/M003/draft.jsonl:5), `CODE-CS-NULL.base.F0000004.P01` | [M002:4](C:/Projects/llm_mvp_core/learning-sets/DOC-CS-NULL/base/M002/draft.jsonl:4), `DOC-CS-NULL.base.F0000006.P01` | `string Coalesce(string? s, string alt) => s is null ? alt : s;` |
| [M003:7](C:/Projects/llm_mvp_core/learning-sets/CODE-CS-NULL/base/M003/draft.jsonl:7), `CODE-CS-NULL.base.F0000008.P01` | [M002:2](C:/Projects/llm_mvp_core/learning-sets/DOC-CS-NULL/base/M002/draft.jsonl:2), `DOC-CS-NULL.base.F0000003.P01` | `string Cell(string? v, string fb) => v ?? fb;` |

Библиотечные копии этих [code](<C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/CODE-CS-NULL/base/M003/draft.jsonl>) и [docs](<C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/DOC-CS-NULL/base/M002/draft.jsonl>) побайтно совпадают с core. В [контракте данных](02-DATA-CONTRACT.md) общий исходный код, его пересказ и варианты переименования относятся к одной source-family.

Минимальная плановая коррекция — пример, где pair/topic/domain IDs различаются, но исходный метод и его объяснение сохраняют общий source-origin. Разные номера из двух requests не доказывают независимость.

**Фактическая train/exam leakage не установлена:** все рассматриваемые строки train. Здесь выявлены неверные связи происхождения и риск ненадёжного будущего split, а не уже измеренная утечка в EXAM.

## 5. Удачные образцы для простой авторской проверки

### Иврит: видимая перестановка без изменения длины

[TR07-TRANSPOSE/he-en/M005/draft.jsonl:1](C:/Projects/llm_mvp_core/learning-sets/TR07-TRANSPOSE/he-en/M005/draft.jsonl:1), pair `TR07-TRANSPOSE.he-en.M005.P01`:

- canonical token: `חלב` — последовательность code points [ח, ל, ב];
- raw token: `לחב` — [ל, ח, ב];
- swap indices [0,1], длины **3→3**;
- исходный текст: `קנה לחב בדרך הביתה.`;
- chosen: `Buy milk on the way home.`;
- rejected: `Buy bread on the way home.`.

Здесь считаются Unicode code points, не UTF-8 bytes. Строка демонстрирует правильную механическую операцию и понятное различие переводов. Это образец авторинга, не результат обучения.

### C#: условие и контрпример видны прямо в prompt

[CODE-CS-NULL/base/M003/draft.jsonl:1](C:/Projects/llm_mvp_core/learning-sets/CODE-CS-NULL/base/M003/draft.jsonl:1), pair `CODE-CS-NULL.base.F0000000.P01`:

- prompt требует заменять только null и сохранять точное `" x "`;
- chosen: `string Title(string? s) => s ?? "—";`;
- rejected: `string Title(string? s) => s is null ? "—" : s.Trim();`;
- простой различающий вход: `" x "` должен остаться с пробелами.

Контракт и дефект rejected ясны из исходника. Metadata oracle честно сообщает `NOT_RUN`; аудит не выдаёт чтение кода за исполнение.

В 20 code/docs-парах и 21 routing-паре явного переворота chosen/rejected не установлено. Десять строк обнаруженной партии VALIDWORD были дочитаны, но прежний вывод об отсутствии дополнительных подтверждённых дефектов оказался неполным: повторная адресная проверка установила, что строки 6–10 требуют восстановления повреждения вместо назначенного сохранения правильного слова. Это пять ранее пропущенных нарушений цели; они не были созданы позднее. Точные ID, неизменный SHA256 и сопоставление с сохранённым заданием — в [последующем аудите](25-GENERATOR-FOLLOWUP-AUDIT-2026-10-25.md).

## 6. Структура, источники, модель и legacy acceptance

### Что проверено чтением

Собственный read-only код разобрал JSON/JSONL, проверил требования текущей candidate-схемы и сопоставил 17 партий с requests: count, family IDs, topic/domain/profile/split, learning objective и разрешённые source refs. Расхождений не найдено. В core нет повторных pair IDs или равных chosen/rejected. Это структурный результат, не допуск качества.

Точный prompt сравнен с 854 строками файлов, найденных по шаблону `*reference*.jsonl` в preparation, включая исторические/reference материалы. Совпадений нет. Семантическая независимость всех этих 854 строк не проверялась.

Все 58 закреплённых RAG-фрагментов в `_sources/xiigen-rag-97d61ed38a20/rag/sources` совпали с соответствующими `local_excerpt_sha256` текущего [sources.lock.json](sources.lock.json). Использованные IDs: RAG-006, RAG-023, RAG-024, RAG-027, RAG-028, RAG-031, RAG-033, RAG-040. Наличие методического источника не является доказательством выполнения C# oracle или правильности новой симуляции.

### Identity — заявленная, не подтверждённая провайдером

171 CREATE_ONLY строка декларирует `generator_model="Kimi K3 (Kiro)"`, `generation_session_id="unknown"`, `model_identity_observed=false`. В legacy10 session задан как `not_disclosed_in_this_environment`, observed также false. State сообщает `provider_identity_verified=false`.

По этим файлам нельзя независимо подтвердить фактический provider/model, отдельный вызов или конкретную сессию генерации. Отчёт не превращает текстовое имя селектора в доказательство model identity.

### Ранняя принятая партия отделена от CREATE_ONLY

У [TR07-OMISSION/ru-en/M001](C:/Projects/llm_mvp_core/learning-sets/TR07-OMISSION/ru-en/M001/accepted.acceptance.json) есть 10 accepted-строк. Чтением установлено:

- draft и accepted равны; accepted равен `blind.key.json/input_rows`;
- экспорт точно проецирует prompt/chosen/rejected;
- все 10 winners в review совпадают с key и имеют valid=true;
- blind payload содержит A/B, prompt, context и lineage; явных chosen/rejected labels в нём нет.

Это не доказывает фактическую независимость reviewer. В [review.json](C:/Projects/llm_mvp_core/learning-sets/TR07-OMISSION/ru-en/M001/review.json) она заявлена как `separate_session`, но модель и session ID не раскрыты. Вызов отдельной сессии и отсутствие у неё доступа к key данным аудитом не подтверждались.

Acceptance ограничен статусом `DATA_REVIEWED_NOT_NATIVE_TRAIN_READY`, scope DATA_ONLY; export имеет `INTERCHANGE_FORMAT_ONLY`, native import не проверен, training NOT_RUN. Эти границы не следует сокращать до «готово обучать».

### Наборы не являются моделями

В 225 файлах инвентарного среза не найдено checkpoint/weight-артефактов с расширениями .bin, .pt, .pth, .safetensors, .onnx, .gguf или .ckpt. Содержимое — данные, requests/reports/state и источники. Это не утверждение об отсутствии моделей вне проверенного дерева. Нет основания называть созданные JSONL обученной моделью или заявлять измеренное улучшение обучения.

## 7. Стабильность доказательств и пределы

Файлы четырёх mutation-находок и двух code/docs-находок повторно сверены по SHA-256 перед завершением пакета; изменений между чтениями нет. Соответствующие библиотечные копии остались побайтно идентичны.

| Core-файл относительно learning-sets | SHA-256 | mtime_ns прочитанного файла |
|---|---|---:|
| TR07-OMISSION/ru-he/M001/draft.jsonl | `83620477fd2b2f28e1d06c69635a51e8e5b52e0221b2d55c18f2719104737b06` | 1792937957771099000 |
| TR07-OMISSION/en-he/M001/draft.jsonl | `408085bcaec2fccac519bcaa6881536245b784261c7c4a0a64033fab36e9f0c7` | 1792938547147356400 |
| TR07-TRANSPOSE/ru-en/M005/draft.jsonl | `cf339a1e5ce5287de9d12d53018edffb86ba6987076e43a654bd1ea3413126e2` | 1792940283513113400 |
| TR07-TRANSPOSE/ru-he/M005/draft.jsonl | `c7b57142d2cb8bc8e15b317fc41540486c1886e087c0fcefcda43e347b6df047` | 1792940459007005400 |
| CODE-CS-NULL/base/M003/draft.jsonl | `35473ef235e756923284185d4dafafb046b6104131a70495572bf57fe3fb819e` | 1792937036995024100 |
| DOC-CS-NULL/base/M002/draft.jsonl | `d3bbb9942d4a2e7354f26f238d23a8fc6d9d3671cfcb9db0cd6bff07f2ff1d45` | 1792936942723149500 |

Семантический просмотр охватил source_raw/canonical/chosen/rejected всех 140 переводных строк, 20 code/docs-пар и 21 routing-пару. Полные prompt/rationale адресно разобраны для находок; code/docs прочитаны с условиями и заявленными oracle cases; routing — с общим каталогом и конкретными запросами. Это не полная независимая bilingual-аттестация каждого контекста и не runtime-проверка.

Родитель дополнительно сопоставил 120 строк 12 CREATE_ONLY-файлов OMISSION/TRANSPOSE собственным read-only кодом. Literal-проверка `canonical_token` также отметила `погода` против `Погода` у P10 двух русских TRANSPOSE-профилей, но полная операция `Погода → Поогда` сохраняет регистр и корректно меняет позиции [2,3]. Это флаг разметки, а не ещё две ошибки видимой задачи; число подтверждённых дефектных строк остаётся пять. Совет брать точную форму из предложения устраняет неоднозначность будущей разметки, не превращая каждый старый literal-флаг в доказанный дефект.

AGENTS/CLAUDE и применимые skills прочитаны адресно; полной загрузки больших документов не заявляется. Context-mode для большого чтения не вернул результат за ограниченное ожидание; дальнейший анализ выполнялся собственным кодом с ограниченным выводом. Проектные Python-модули не импортировались и tools/*.py не запускались.

Не выполнялись builds/tests, oracle, inference, обучение, native import, product/runner/dataset/weights/Git writes. Данные сохранены без исправлений. Не проверялись provider authentication, реальные сессии генератора/reviewer, свободная генерация обученной модели, результат учебного шага, семантическая независимость полного reference-корпуса и результаты после закрытого участка M006.

## 8. Внесённые плановые исправления

Применены изменения только к следующим документам; runner и кандидатные данные не исправлялись:

| Документ | Что изменено и для чего |
|---|---|
| [TYPO-AUTHORING-EXAMPLES-RU](prompts/TYPO-AUTHORING-EXAMPLES-RU.md) | Good/bad для удаления, индекса и перестановки реальной формы слова; добавлены Hebrew `חלב → לחב`, 3→3, и C# code→docs общий origin. Обязанность подготовить общий сюжет остаётся у автора задания; старый request без сюжета не останавливает CREATE_ONLY. |
| [BATCH-GENERATE](prompts/BATCH-GENERATE-PROMPT-RU.md) | Встроенная простая самопроверка и ссылка на шпаргалку; сохранение IDs и кандидатного статуса при неподтверждённом родстве без нового review-гейта. |
| [02-DATA-CONTRACT](02-DATA-CONTRACT.md) | Точный контракт отдельно назначенной batch-приёмки по BATCH-REVIEW вместо старых language_review/evidence.oracle полей; конкретные примеры общего сценария и исходника. |
| [04-QUALITY-AND-SPLITS](04-QUALITY-AND-SPLITS.md) | Прямое поручение пользователя позволяет аудитору читать draft сейчас. Открытый аудит с labels отделён от blind/accept/export/обучения; добавлен пример длины и обратного восстановления. |
| [MASTER](prompts/MASTER-PROMPT-RU.md), [DPO-TASK-RESET](prompts/DPO-TASK-RESET-RU.md) | Короткие авторские примеры и абсолютный путь шпаргалки для уже начатой сессии. Обновление документа не объявлено измеренным ростом качества. |
| [REPAIR](prompts/REPAIR-PROMPT-RU.md) | Будущие отдельно назначенные команды используют quoted абсолютные пути реального библиотечного M001; Python и tools script читаются из request.paths. Сохранены версии, derived_from, split и исходные файлы. Текущий аудит не разрешает эти команды или oracle. |
| [README](README.md), [00-START-HERE](00-START-HERE.md) | Прямые ссылки на этот аудит и шпаргалку без повторного объявления новой стадии. |

Новый batch-prompt включает актуальный BATCH-GENERATE через [batches.py:189](<C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/batches.py:189>). Уже сохранённые prompt.md прежних запусков автоматически не изменились. Для начатой сессии предназначены MASTER/reset с явным путём примеров; обновление документа не является доказательством, что исполнитель его прочитал.

### Открытая часть назначения семей

Default-ветвь `current_families` в [batches.py:113–121](<C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/batches.py:113>) назначает всем переводным профилям общий namespace и origin по F-номеру, но передаёт только общее `scenario_rule`, без содержания общего сюжета. Этот allocator **не исправлен** данной работой. Существующая ветвь [--family-plan:98–111](<C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/batches.py:98>) читает назначенный план, выбирает семьи по topic/profile/split/objective и передаёт полные записи в `family_contracts`; она позволяет передать заранее подготовленное семантическое назначение.

Будущее ограниченное поручение **автору подготовки**, после отдельного назначения: описать конкретные общие сюжеты для cross-profile переводов и общие source-origin для точных CODE/DOC исходников; сохранить связи производных, указать применимые topic/profile/objective/split и передать готовые контракты через существующий `--family-plan` в `family_contracts`. Это задача назначения материала, а не требование генератору искать чужие сессии или угадывать смысл F-номера. Новые номера сами по себе не устраняют старое ложное объединение или ложную независимость.

Пример содержательного `scenario_rule`: «Один сюжет — просьба купить молоко по дороге домой. Во всех назначенных языковых профилях сохраняются покупка, молоко и дорога домой; меняются только язык и назначенное повреждение одного слова». Для CODE/DOC: «Один исходник — метод Name(string? s), возвращающий "гость" только при null и точное s иначе; code-задача создаёт этот метод, docs-задача объясняет тот же исходник; обе сохраняют общий source-origin». Это поясняющие примеры для автора будущего назначения, не созданный family-plan, новый набор или команда запуска.

В этой работе не создано нового family-plan, dataset или runtime-файла. Уже назначенные IDs и исходные файлы сохранены. Найденные дефектные CREATE_ONLY-партии остаются `DRAFT_CREATED`; их готовность к нативному обучению — `NOT_NATIVE_TRAIN_READY`, а родство без общего сюжета не подтверждено. Исторический accepted-статус отдельной legacy-партии описан в §6 и не присваивается новым кандидатам. Текущая очередь создания не остановлена ради нового review.

### Что даёт простая самопроверка

Длина, точное обратное восстановление и сверка слова с предложением достаточны, чтобы заметить показанные механические ошибки: замену вместо удаления, неверный индекс, лишнюю смену регистра и подмену падежной формы. Это доступная автору проверка без отдельного процесса. Она не гарантирует единственность восстановления, естественность перевода, корректность C# исполнения, независимость семей или общий успех обучения. Предметные вопросы остаются видимыми; новый oracle/review-гейт, threshold или квота не добавлены. Новая генерация для измерения эффекта этих подсказок не запускалась.
