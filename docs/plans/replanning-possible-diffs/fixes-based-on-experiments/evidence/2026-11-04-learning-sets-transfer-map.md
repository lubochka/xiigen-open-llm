# Наборы для проверки переносов: доказательная карта 4 ноября 2026

Статус: **READ_ONLY_RESEARCH; подбор будущего покрытия, не результаты обучения**. Исследователь `/root/learning_sets_transfer_map`. Этим исследователем изменён только этот запрошенный Markdown; parent отдельно ведёт план и инструкции. Обучение, inference, Load, сборка, тесты, runner и Git не запускались; исходные наборы и продукт не изменялись.

Главный вывод: в core есть разнообразный предметный материал, но нет готовой независимой EXAM-панели среди draft. Единственный принятый пилот содержит 10 TRAIN-пар и прямо помечен как не готовый к native training. Поэтому ниже — конкретные источники для отдельно подготовленной simulation-проекции и её будущего допуска. Они не заменяют собственные панели уже существующих карточек и не открывают завершённые опыты повторно.

## 1. Что действительно исследовано и как получены числа

Оба корня перечислены `rg --files --hidden`; затем файлы читались через Context Mode, `fs.readFileSync`. Каждый `.json` разобран `JSON.parse` целиком, каждый непустой ряд `.jsonl` — отдельно. Для синтаксического разбора снимался только начальный BOM. Скрипты сборки/приёмки из наборов не исполнялись. SHA256 ниже вычислен по исходным байтам файлов через `crypto.createHash('sha256')`, без нормализации newline/Unicode. Номер строки — физическая строка JSONL, начиная с 1.

Числа файлов в таблице — снимок **до** добавления parent новых инструкции 31 и `simulations/transfer-coverage-2026-11-04/README.md` в этой документной работе. Они не заявляются текущим total после этих записей. Подсчёты существующих JSON/JSONL и их содержимого получены указанным разбором; документационная дельта не создаёт новые учебные строки.

| Проверенная область | Наблюдение |
|---|---|
| `C:/Projects/llm_mvp_core/learning-sets` | 5557 файлов: 2148 JSON, 802 JSONL, 2604 Markdown и 3 прочих |
| `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets` | 850 файлов: 137 JSON, 27 JSONL, 662 Markdown, 19 Python, 2 PowerShell и 3 прочих |
| Синтаксический разбор JSON/JSONL в обоих корнях | Ошибок разбора не обнаружено. Это **не** schema validation, семантическая приёмка или исполнение oracle |
| Все core `draft.jsonl` | 569 файлов, 5729 строк; **5729 TRAIN, 0 DEV, 0 EXAM** |
| Идентичности core draft | 5599 разных `pair_id`; 120 ID повторяются; **67 ID обозначают разные тройки `(prompt,chosen,rejected)`** |
| Точные тройки core draft | 5651 разных тройки; 78 дополнительных вхождений повторяют точное содержимое. Это не число семантически независимых примеров |
| Объявленные семейства | 3194 разных `family_id`; нет пересечений split, поскольку все строки TRAIN. Это не доказательство независимого EXAM |
| Объявленный review в draft | 5719 `metadata.review_status=not_requested`; 10 `independent_review_required`. Последние относятся к draft принятого отдельно пилота; accepted имеет свою квитанцию |
| State-файлы | 325: 320 `DRAFT_CREATED/NOT_REQUESTED`, 3 `PREPARED/NOT_REQUESTED`, 1 `PREPARED` без review, 1 без этих полей. Campaign-state — другой формат; state не заменяет accepted receipt |
| Другие core JSONL | 3620 строк `family-material.jsonl`, 76 строк repair-журнала, по 10 accepted/blind/provenance/export-train. **Не прибавлять их к числу самостоятельных учебных примеров** |
| Planning-root draft | 101 строка старых попыток; наличие зеркала не создаёт новый независимый корпус |

Для повторяющегося ID сравнивалась `JSON.stringify([prompt,chosen,rejected])`; происхождение и иной metadata в эту проверку не входили. Для family пересечений сравнивались точные строки `family_id` и `split`. Перефразирования, общие исходники с разными ID и межъязыковое родство такой подсчёт не обнаруживает; ручная семантическая проверка всего корпуса **не выполнена**. Предметно прочитаны выбранные ниже записи, все 10 accepted-записей, 21 маршрутная пара и 24 routing-reference; остальные строки обработаны структурно.

## 2. Структура, темы, схемы и происхождение

Core содержит legacy-партии `<OBJECTIVE>/<profile>/<run>/`, campaign `campaigns/D1000-20261025/` и закреплённые источники `_sources/xiigen-rag-97d61ed38a20/`. В legacy встречаются request/prompt/commands/state/report/draft; новая campaign имеет `xiigen.domain-campaign-request.v1`, material/family-material/creation/coordination. Квитанция CREATE_ONLY и завершение генерации не являются review или обучением.

Количество draft-строк по фактическому `domain_id`:

| Записанные domain_id | Строки |
|---|---:|
| `tr.en`, `tr.ru`, `tr.he` | 1648 / 928 / 738 |
| `code.csharp`, `code.sql`, `code.angular` | 140 / 142 / 120 |
| `docs.csharp`, `docs.sql`, `docs.angular` | 150 / 162 / 120 |
| `or.functional`, `or.architecture` | 1010 / 380 |
| `or.routing`, `or.qa`, `or.automation` | 21 / 30 / 30 |
| `or.design`, `or.code`, `or.client`, `or.ux` | по 20 |
| `or.database`, `or.server`, `or.mobile` | по 10 |

Это 21 значение поля, **не 21 доказанно обученная предметная способность**. RU→EN и RU→HE — языковые профили перевода; C#/SQL/Angular — технологические специализации; docs/code — разные задачи над предметом. Маршрутный JSON о SQL не является уроком выполнения SQL.

Все draft имеют 15 верхних полей: `schema_version,pair_id,domain_id,topic_id,profile_id,family_id,split,prompt,chosen,rejected,error_tag,rationale,source_refs,provenance,metadata`. [Схема кандидата](C:/Users/lubao/Documents/xiigen/llm%20mvp/learning-sets/preparation/schemas/dpo-candidate.schema.json) задаёт `xiigen.dpo.v1`, split train/dev/exam, обязательные provenance kind/generator_model и metadata origin_family_id/leakage_group_ids. Поле `simulation` необязательно: отсутствует у 1350 draft-строк. Reference-файлы имеют отдельную роль; перечисление `reference` в методике не делает их кандидатами по этой схеме.

В draft встречаются только три записанных provenance-варианта: 4189 `model_generated_simulation` с `Kimi K3 (Kiro)`, неизвестной session и `model_identity_observed=false`; 1530 `authored_by_model` с тем же именем; 10 с `not_disclosed_in_this_environment`. Это заявления файлов, не независимая аутентификация модели. **Не назначать `DeterministicGenerator` только из-за вымышленного сюжета** и не выдумывать native admission issuer.

[Pinned manifest](C:/Projects/llm_mvp_core/learning-sets/_sources/xiigen-rag-97d61ed38a20/rag/manifest.json) содержит 58 source_id. Все 35 разных source_refs из core draft найдены в этом manifest. Проверено совпадение ID, не внешняя лицензия и не истинность каждого ответа. RAG-023/024 описывают методику переводческих пар/токенизации; RAG-027/028 — авторинг документации и версионные ограничения; RAG-040/041 содержат прежние вопросы/примеры. Ссылка на методический источник не является выполненным SQL/C#/Angular oracle.

## 3. Восемь конкретных пакетов для будущей simulation-проекции

Все пути ниже находятся под `C:/Projects/llm_mvp_core/learning-sets/`. Полный путь, SHA256, строка и `pair_id` вместе задают адрес; один `pair_id` недостаточен. У всех восьми пакетов split=train. S1 имеет отдельную data-acceptance; S2–S8 остаются DRAFT.

| ID | Точный относительный путь | Строк / байт | SHA256 исходного файла |
|---|---|---:|---|
| S1 | `TR07-OMISSION/ru-en/M001/accepted.jsonl` | 10 / 23489 | `f4fd0393161f0f65e060b053f8af51d1bf85f84af20bb1cc65cd01be1e2399e2` |
| S2 | `OR-ROUTE-INTENT/base/M004/draft.jsonl` | 21 / 85401 | `d5e718992873c3d9a8b6e7ee1e4e852086f706aae1ec9ef67f2c7a34f65327bc` |
| S3 | `DOC-CS-NULL/base/M002/draft.jsonl` | 10 / 22384 | `d3bbb9942d4a2e7354f26f238d23a8fc6d9d3671cfcb9db0cd6bff07f2ff1d45` |
| S4 | `CODE-CS-NULL/base/M003/draft.jsonl` | 10 / 21924 | `35473ef235e756923284185d4dafafb046b6104131a70495572bf57fe3fb819e` |
| S5 | `CODE-NG-DISABLED/base/T436/draft.jsonl` | 10 / 21509 | `757ff32d2fd467fa18b04ef57a6dafbe1c57d19921868103ef5e8cfbd3742926` |
| S6 | `CODE-SQL-COUNT/base/T434/draft.jsonl` | 10 / 20128 | `05b77584dadb530b4b8ed9b0394c38e59ec3b3fe8c5b81d391f8c20088557b74` |
| S7 | `TR01-NEGATION-SCOPE/ru-he/T446/draft.jsonl` | 10 / 16779 | `1191cb6ba9e208b743d688e7656fa111fea2512fd4d8fec8a5dc99c10522949a` |
| S8 | `TR05-CONTEXT-SENSE/ru-en/T468/draft.jsonl` | 10 / 19016 | `f17e1aff95dd90acf9d5aec4db692b066c33edd0824223e0d28f9f30f9f02446` |

**S1 — восстановление обычного слова при переводе.** [Строка 1](C:/Projects/llm_mvp_core/learning-sets/TR07-OMISSION/ru-en/M001/accepted.jsonl:1), `TR07-OMISSION.ru-en.M001.F0000001.P01`: контекст продуктов на кухне, «Молко стоит в холодильнике»; chosen `The milk is in the fridge.`, rejected `The juice is in the fridge.` Строка 2, `TR07-OMISSION.ru-en.M001.F0000002.P01`: «В парке гуляет обака»; dog против cat. Положительный контроль — выбранный перевод с тем же предметом; отрицательный — правдоподобная подмена предмета. Это материал пары DPO либо SFT `(prompt,chosen)` после допуска, не готовый TokenizerOutput. Квитанция [accepted.acceptance.json](C:/Projects/llm_mvp_core/learning-sets/TR07-OMISSION/ru-en/M001/accepted.acceptance.json): `DATA_REVIEWED_NOT_NATIVE_TRAIN_READY`, `native_admission=NOT_CHECKED`, 10 families, `training=NOT_RUN`; [export.json](C:/Projects/llm_mvp_core/learning-sets/TR07-OMISSION/ru-en/M001/export/export.json): `INTERCHANGE_FORMAT_ONLY`, `native_import_verified=false`, train=10, anti_leakage_against=[]; dev/exam пусты. Review объявляет отдельную сессию и bilingual evidence, но receipt прямо не заявляет provider authentication. State PREPARED/NOT_RUN не отменяет отдельную data-acceptance и не доказывает запуск.

**S2 — выбор предметного получателя.** [Строка 1](C:/Projects/llm_mvp_core/learning-sets/OR-ROUTE-INTENT/base/M004/draft.jsonl:1), `OR-ROUTE-INTENT.base.F0000000.P01`: «Переведи на английский: Счёт отправлен вчера вечером»; chosen route `tr.en`, rejected `tr.ru`. [Строка 21](C:/Projects/llm_mvp_core/learning-sets/OR-ROUTE-INTENT/base/M004/draft.jsonl:21), `OR-ROUTE-INTENT.base.F0000022.P01`: перевод инструкции на русский, затем миграция БД; chosen clarify/null, rejected route tr.ru. Это атомарный router: даже упорядоченный составной запрос требует clarify; новое разбиение нельзя скрыто подставить как gold. Строки 4/7 различают docs.csharp/code.csharp. В пакете 20 route-пар и одна clarify; баланс отрицательных видов для широкой оценки недостаточен. Для KNN/router нужна проекция пользовательского намерения в native routing request и явная карта domain→UnitId/FlowRef; JSON-ответ сам по себе не содержит полного тела Flow.

**S3 — документация C# и сохранность пробелов.** [Строка 1](C:/Projects/llm_mvp_core/learning-sets/DOC-CS-NULL/base/M002/draft.jsonl:1), `DOC-CS-NULL.base.F0000001.P01`: `string Display(string? s) => s ?? "—"`; объяснить `" A "`, null, `""`, `" "`. Chosen сохраняет `" A "`; rejected выдумывает Trim и возвращает `"A"`. Само утверждение проверяемо, но metadata.oracle=`NOT_RUN`, result_observed=false. Будущая admission требует различающего поведения обеих сторон в закреплённом C# 10/.NET 6; это не уже выполненный oracle.

**S4 — создание C# метода по тому же типу ограничения.** [Строка 1](C:/Projects/llm_mvp_core/learning-sets/CODE-CS-NULL/base/M003/draft.jsonl:1), `CODE-CS-NULL.base.F0000000.P01`: Title(null)→`"—"`, остальные строки без изменений; chosen `string Title(string? s) => s ?? "—";`, rejected вызывает `s.Trim()`. Контроль null/empty/space/`" x "` различает ошибку. Это полный code-output для SFT/DPO и конечный subject readout E2E; **не готовая MethodKnn-метка null-guard и не доказательство FIX-NG**. Изменённое имя метода не гарантирует независимость от S3: для будущего EXAM учитывать общее исходное правило и родство DOC/CODE.

**S5 — полярность состояния Angular-кнопки.** [Строка 1](C:/Projects/llm_mvp_core/learning-sets/CODE-NG-DISABLED/base/T436/draft.jsonl:1), `CODE-NG-DISABLED.base.F0000026.P01`: Angular 16.2/TypeScript 5.1, disabled ровно когда cancelling=true; chosen `[disabled]="cancelling"`, rejected `[disabled]="!cancelling"`. Обе boolean точки — положительные/отрицательные наблюдения, не строковое сравнение шаблона. Metadata oracle NOT_RUN; работа TestBed в этой сессии не запускалась. Различает порождение корректного кода и вывод внешне похожей строки.

**S6 — SQL COUNT и NULL.** [Строка 1](C:/Projects/llm_mvp_core/learning-sets/CODE-SQL-COUNT/base/T434/draft.jsonl:1), `CODE-SQL-COUNT.base.F0000021.P01`: T-SQL SQL Server 2019, одна connection/batch, @T содержит четыре NULL; нужен счёт строк. Chosen `SELECT COUNT(*) AS n FROM @T;` даёт ожидаемые 4; rejected `COUNT(v)` — ожидаемый 0. Эти числа пока авторский oracle-контракт, **не измеренные результаты выполнения**. При будущей проверке обе стороны исполняются в одинаковых отдельных fixtures; не добавлять null-ограничение, делающее ответы одинаковыми.

**S7 — область отрицания и не-ASCII выход.** [Строка 1](C:/Projects/llm_mvp_core/learning-sets/TR01-NEGATION-SCOPE/ru-he/T446/draft.jsonl:1), `TR01-NEGATION-SCOPE.ru-he.F0000015.P01`: «Он не думает, что план сработает»; chosen `הוא לא חושב שהתוכנית תצליח.`, rejected `הוא חושב שהתוכנית לא תצליח.` Отрицание матричного убеждения против отрицания вложенного события. Это кандидат bilingual review и различения смысла плюс Unicode readout, не уже принятый gold. Оба ответа содержат отрицание: проверка только наличия `לא` не различит их. Сам этот пример не закрывает историческую панель P55-полярности и не переоткрывает принятую строку реестра.

**S8 — контекстное значение слова.** [Строка 1](C:/Projects/llm_mvp_core/learning-sets/TR05-CONTEXT-SENSE/ru-en/T468/draft.jsonl:1), `TR05-CONTEXT-SENSE.ru-en.F0000024.P01`: экскурсовод в средневековой крепости, исходное «замок»; chosen `a castle`, rejected `a door lock`. Строка 2, `TR05-CONTEXT-SENSE.ru-en.F0000025.P01`: школьник пишет синей пастой, «ручка»; `a pen` против `a door handle`. Семантическое подтверждение контекста находится в prompt; sense_id/competing_sense/evidence_span — evaluator/provenance, не подсказка ученику. Для полного контраста нужны независимые положительные контексты обоих значений. **Не подключать напрямую к ContextualDictionary проекта 12 как доказательство TEST-TCTX:** там свой POS-контекст, а castle/lock имеют одинаковую часть речи. Нужен согласованный тип входа конкретного получателя; отсутствие такого carrier — пробел, а не повод закодировать sense_id как ответ.

### Дополнительные адреса для выбранных в плане 158 Angular/SQL-пакетов

План 158 выбирает для широкой маршрутной панели ещё два конкретных пакета, отличающихся от простых S5/S6. Они прочитаны отдельно после появления плана; их данные не заменяют исходные S5/S6 и не объединяются с ними автоматически. Оба содержат 10 TRAIN-строк, `state.status=DRAFT_CREATED`, `review_status=NOT_REQUESTED`, `training_status=NOT_RUN`; metadata.oracle также NOT_RUN.

| Точный путь от core learning-sets | Байты / SHA256 | RowID и предметный контроль |
|---|---|---|
| `CODE-NG-LATEST/base/D310/draft.jsonl` | 24825; `52fb2392f201a85916575c7b852c0fe2b3f71fc38540f34f94a9fa6bdf53445b` | Строка 1, `CODE-NG-LATEST.base.F0000001.P01`. Angular16/RxJS7.8: X в frame0, Y в 3, inner X выдаёт x спустя 6, Y выдаёт y спустя 2. Chosen `search$.pipe(switchMap(q => fetch(q)))`; rejected mergeMap. Ожидается только y в 5; плохой ответ дополнительно выдаёт старый x в 6. Это авторский trace, не выполненный TestScheduler. Prompt прямо требует switchMap: проверяется соблюдение инструкции, не самостоятельный выбор оператора |
| `CODE-SQL-LEFT/base/D306/draft.jsonl` | 23967; `61fc2221168de4d96e311e5af098af7c822b26cca482aa0dae9d447dd45b1a37` | Строка 1, `CODE-SQL-LEFT.base.F0000000.P01`. SQL Server2019/ANSI_NULLS ON: клиенты 1/2, Open-заказ 10 и Closed-заказ 11 у 1. Chosen LEFT JOIN с `o.Status='Open'` в ON ожидаемо сохраняет `(1,10),(2,null)`; rejected переносит фильтр в WHERE и теряет `(2,null)`. Результаты заданы автором и не были исполнены. T-SQL fixture не заменяется другим диалектом |

## 4. Адресация: реальная коллизия ID

`TR02-ROLE-ORDER.ru-he.F0000012.P01`:

| Файл, строка 1 | Фактическая задача | SHA256 файла |
|---|---|---|
| `TR02-ROLE-ORDER/ru-he/T452/draft.jsonl` | Иван поздравил Марию с победой; chosen Иван→Мария, rejected наоборот | `0852cbd1680c59bd7c481e5a88e3a029d275b635df6118a822a809cd2e14901b` |
| `TR02-ROLE-ORDER/ru-he/T357/draft.jsonl` | Нина поздравила Сергея с юбилеем; chosen Нина→Сергей, rejected наоборот | `94143a914a87f0dfe4e74fe87cd35286d4e82657e3a39200f4cc1478dfa18b1f` |
| `TR02-ROLE-ORDER/ru-he/T357-retry/draft.jsonl` | Точная копия предыдущего файла | `94143a914a87f0dfe4e74fe87cd35286d4e82657e3a39200f4cc1478dfa18b1f` |

Первая пара файлов — конфликт ID с разным содержимым; последние две — точный retry. Не выбирать последний автоматически и не переписывать существующие IDs. В simulation manifest нужен составной source locator и явное решение по каждому конфликту. Это согласуется с действующим [02-DATA-CONTRACT:7](C:/Users/lubao/Documents/xiigen/llm%20mvp/learning-sets/preparation/02-DATA-CONTRACT.md:7).

**Отдельная сверка всех строк семи пакетов §3 плана 158 против всех core draft.** Для DOC-CS-NULL/M002, CODE-CS-NULL/M003, OR-ROUTE-INTENT/M004, CODE-NG-LATEST/D310 и CODE-SQL-LEFT/D306 повторов pair_id в других core draft не обнаружено. У accepted TR07/M001 есть его собственный draft с теми же 10 записями; это один исходный материал, не новый независимый пример. Этот узкий результат не подтверждает семантическую независимость, отсутствие зеркал planning-root или качество пакетов.

В выбранном **TR01-NEGATION-SCOPE/ru-he/T446 все 10 pair_id повторяются в T351**. Семь троек совпадают точно; три различаются: строки 1/8/10, ID `TR01-NEGATION-SCOPE.ru-he.F0000015.P01`, `…F0000022.P01`, `…F0000024.P01`. В строке 1 T351 chosen `הוא לא חושב שהתוכנית תעבוד.`, T446 chosen `הוא לא חושב שהתוכנית תצליח.`; rejected также меняет תעבוד на תצליח. Это разные точные ответы, **не автоматически доказанная смысловая ошибка**. Нужен bilingual review допустимых вариантов и pin именно T446; T351 не добавлять как независимую панель и не сливать по одному ID.

## 5. Какие уже назначенные владельцы получают полезное расширение

Допуск проверен по текущему [реестру](../01-registry.md), не по слову positive в старом отчёте. Таблица ниже предлагает **данные для последующего отдельного расширения**, не меняет порядок очереди, бюджет, исторические TRAIN/EXAM и режим карточки.

| Owner / текущая граница | Подходящие источники | Что расширение действительно способно проверить; что остаётся отдельно |
|---|---|---|
| [FIX-SEP/62](../62-supervised-framing-and-learning-readout.md), registry:104 — логический ремонт диагностики | S1, S3, S4, S7 | Одинаковый framing SFT/readout на переводе, документации, коде и Unicode. Чтение exact ответа и диагностический дефект отделены от приобретения способности. Существующую панель 6 Forward/0 updates не расширять этим текстом автоматически |
| [FIX-N63C/17](../17-dpo-sft-correction-collision.md), registry:105 — ремонт источника урока | S3/S4, S5/S6 после admission | Для каждого prompt сохраняется точная rejected/chosen пара; в отдельно спроектированном конфликте SFT response=старый rejected должен исправляться на chosen. Нужны отдельные no-match/уже-chosen controls, исходные пары не менять |
| [FIX-LC/13](../13-dpo-reference-cache.md), registry:170 — допущен, остаток lifecycle/RNG | S1/S4/S6/S7 после native admission | Разные длины/алфавиты и пары для будущего сохранения предметных ответов при cache hit/miss и законной инвалидации. Ни новые Forward, ни процент экономии по новым данным здесь не назначены; существующий receipt и режим cache остаются основой приёмки |
| [FIX-E2E/20](../20-prompt-domain-e2e.md), registry:109 — отдельное покрытие, не успешный опыт | S2 + конечные S1/S3/S4/S5/S6/S7; reference ниже | Raw prompt→предметное намерение→законный route/unit→фактический subject output→тот же вывод после разрешённого restore. Q1–Q6 карточки 20 сохраняются; новые темы — отдельное запланированное расширение после adapter |
| [FIX-RET/15](../15-prompt-flow-retention.md), registry:118; [FIX-KR/49](../49-router-learned-key-normalization.md), registry:167 | S2 и docs/code intent contrasts | Сохранить прежний маршрут после парафраза; C#/.NET ключи и отсутствие случайного substring-match. S2 не содержит полного Flow body и native training binding — нужны отдельно, не угадывать их из domain_id |
| [FIX-IM/48](../48-learned-route-body-identity.md), registry:166 | S2 лишь как вход маршрутизации | Для full body/title и реального получателя пакет пока **недостаточен**: chosen — только action/domain. Не считать JSON-маршрут доказательством выдачи/исполнения полного плана |
| [T112/132](../codex-132-p112-material-lineage-transfer-2026-11-02.md), registry:107 — logical lineage | Все S1–S8; коллизия выше как source identity control | Точные байты, путь/строка/ID, соответствие admitted материала фактическому reader и невозможность незаметно поменять источник. Schema/ref match сами не доказывают фактическое потребление |
| [TEST-TCTX/18](../18-contextual-dictionary-learning-coverage.md), registry:146 | S8 — предметная идея, **не готовый native fixture** | Сохранить исходную POS-панель; семантический context carrier нового набора требуется отдельно. Одинаковый source с разными контекстами должен различаться положительными ответами, не только rejected |

FIX-PD/22, FIX-TS/28, FIX-HNQ/50, FIX-MC/21, FIX-MS/23 и другие приостановленные общие режимы **не получают новый допуск от разнообразия данных**. Например, registry:123/124/178 сохраняет ограничения PD/TS/HNQ. Снижение количества операций HNQ не доказывает CPU/time; меньше allocations при большем peak/wall не выдаётся за общую пользу. Принятые BW/AST/PQ/P55 и другие пропускаемые строки не переоткрываются этой картой.

## 6. Реальный native consumer: подтверждено по исходникам, не запуском

Подробный существующий договор — [10-NATIVE-CONTRACT-BOUNDARY.md](C:/Users/lubao/Documents/xiigen/llm%20mvp/learning-sets/preparation/10-NATIVE-CONTRACT-BOUNDARY.md). Новая карта его использует, не объявляет adapter реализованным.

* [DpoDataset.cs:84](C:/Projects/llm_mvp_core/11.DPO/DpoDataset.cs:84) вызывает strict field validation; строки 96–101 читают `Prompt/Chosen/Rejected`, `chosenAuthor/rejectedAuthor/admissionIssuerName`. Частичная admission-тройка отклоняется. Lowercase interchange не равен этому контракту; legacy container с Admission=null не даёт train-ready. [DpoTrainerPairAdmission.cs:125](C:/Projects/llm_mvp_core/11.DPO/Models/DpoTrainerPairAdmission.cs:125) отдельно проверяет PaidExternalModel sides. Реальное авторство неизвестно — binding остаётся нерешённым, происхождение нельзя переименовать ради допуска.
* [IncrementalTrainer.cs:403](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/IncrementalTrainer.cs:403) кодирует `string.Concat(example.Prompt,"\n",example.Response)`. Проекция SFT из пары требует явно назначить Response=chosen, точный framing и держать rejected как контраст; это не существующий импорт богатого JSONL.
* [TranslationTrainedUnits.cs:127](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:127) принимает `TranslationLearningExample`, пропускает HeldOut и десериализует `ChosenOutputJson` в `TokenizerOutput`, `InputJson` в `TokenizerInput`. Перевод `The milk...` не является таким JSON. [Там же:31](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:31) ExecutorInput содержит SourceText/SourceLang/TopicDomain. Значит, phrase DPO/SFT и typed memory/KNN projection — разные привязки; need UnitId/source/example identity и настоящий выбранный output carrier, не упаковка произвольного текста.
* [TrainableRoutingFlow.cs:97](C:/Projects/llm_mvp_core/26.TrainableRouter/Training/TrainableRoutingFlow.cs:97) вызывает RouteAsync и возвращает chosen_unit_id/ROUTE_TO_UNIT. Этот метод формирует result маршрутизации, не запускает предметного специалиста. [OrchestrationPlanOrchestrator.cs:32](C:/Projects/llm_mvp_core/42.ProjectWikiDomain/OrchestrationPlanOrchestrator.cs:32) вызывает GeneratePlanAsync, записывает predicted_flow_ref и возвращает PlanText на строках 66/76. Это подтверждает разрыв, уже записанный FIX-E2E, а не существующий E2E dispatcher.
* Полный авторинг нативного DPO набора через repository-owned manifest/pairs/resolver/sealer и специальный CodeDocumentationSimulationDpoExperiment описаны в 10-NATIVE:39–59. Нельзя принять второй за универсальный loader Kiro-кандидатов. Конкретный адаптер для S1–S8 **не верифицирован**.

## 7. Готовые reference для проектирования E2E, не скрытого EXAM

[routing-e2e-reference.jsonl](C:/Users/lubao/Documents/xiigen/llm%20mvp/learning-sets/preparation/schemas/routing-e2e-reference.jsonl) содержит 24 строки, `split=reference`, `training_allowed=false`, `status=authored_simulation_not_measured`; expected fields только для evaluator. Примеры:

| Строка / case_id | Назначение |
|---|---|
| 1 `ROUTE-REF2-TRANSLATE-CS-EN` | «Переведи ... Метод не сохраняет пустую строку» → tr.en. Слово метод не должно превращать перевод в документацию C# |
| 4 `ROUTE-REF2-DOC-CS` / 7 `ROUTE-REF2-CODE-CS` | Объяснить поведение показанного Has против написать Has по контракту: docs.csharp/code.csharp |
| 5 `ROUTE-REF2-DOC-SQL` / 8 `ROUTE-REF2-CODE-SQL` | Объяснение COUNT против создания запроса |
| 21 `ROUTE-REF2-CLARIFY-LANGUAGE` | Целевой язык не указан: clarify, без default языка |
| 22 `ROUTE-REF2-CLARIFY-TASK` | Намерение «сделай что-нибудь» недостаточно определено |
| 23 `ROUTE-REF2-CLARIFY-COMPOUND` | SQL→объяснение→иврит: clarify для атомарного router, а не скрытый новый graph |
| 24 `ROUTE-REF2-UNSUPPORTED` | Пешая прогулка вне закрытого каталога: не случайный программный domain |

[prompt-pipeline-reference-2026-10-26.json](C:/Users/lubao/Documents/xiigen/llm%20mvp/learning-sets/preparation/schemas/prompt-pipeline-reference-2026-10-26.json) — 4 сценария `PIPE26-01…04`, `REFERENCE_BUNDLE_NOT_NATIVE_RUNTIME_IMPLEMENTATION`. PIPE26-01 разделяет объяснение C#/SQL с общими ограничениями; PIPE26-02 связывает SQL→описание→перевод; PIPE26-03 переводит цитату «Удалите все файлы» без исполнения; PIPE26-04 требует уточнения для `שלום`. D22 span должен быть точной непрерывной подстрокой, D23 получает FULL original prompt + span + domain + prior results. У атомарного router и pipeline разные контракты; их expected не смешиваются.

Эти публичные reference и их перефразированные/межъязыковые родственники нельзя выдать за независимый закрытый DEV/EXAM. Они годятся как открытый контроль контракта и материал проектирования. Отдельные новые семейства назначаются до ответов и не попадают к TRAIN generator. Все действующие запреты origin/leakage изложены в [02-DATA-CONTRACT:90](C:/Users/lubao/Documents/xiigen/llm%20mvp/learning-sets/preparation/02-DATA-CONTRACT.md:90).

### Предлагаемое покрытие 24 новых router EXAM-семейств

Это **план ещё не созданных строк**, не копия reference и не готовый экзамен. Шаблон case_id: `LS158.ROUTE.EXAM.01` … `LS158.ROUTE.EXAM.24`; новые family_id: `LS158/route/exam/F01` … `LS158/route/exam/F24`, split=exam. Поле `contract_case_ref` у evaluator ссылается на перечисленное ниже публичное требование, но не означает derivation исходного сюжета. Автор обязан создать новый содержательный сюжет, проверить его независимость от TRAIN/reference/quarantine и сохранить фактический origin/leakage. Простая смена ID, имён, языка, чисел или перефраз reference **не создаёт новое семейство**. Expected domain/profile и эта матрица не подаются ученику.

| Суффикс нового case/family | `contract_case_ref` из routing-e2e-reference | Что обязан покрыть новый самостоятельный сюжет |
|---|---|---|
| 01 | `ROUTE-REF2-TRANSLATE-CS-EN` | Перевод на EN текста о коде; не docs/code |
| 02 | `ROUTE-REF2-TRANSLATE-SQL-RU` | Перевод на RU текста о запросе; не генерация SQL |
| 03 | `ROUTE-REF2-TRANSLATE-UI-HE` | Перевод UI-надписи на HE; не дизайн экрана |
| 04 | `ROUTE-REF2-DOC-CS` | Объяснить данный C#; docs.csharp |
| 05 | `ROUTE-REF2-DOC-SQL` | Объяснить данный SQL; docs.sql |
| 06 | `ROUTE-REF2-DOC-NG` | Объяснить данный Angular; docs.angular |
| 07 | `ROUTE-REF2-CODE-CS` | Создать C# по контракту; code.csharp |
| 08 | `ROUTE-REF2-CODE-SQL` | Создать SQL по контракту; code.sql |
| 09 | `ROUTE-REF2-CODE-NG` | Создать Angular/TypeScript по контракту; code.angular |
| 10 | `ROUTE-REF2-OR-FUNC` | Проверяемое функциональное требование; or.functional |
| 11 | `ROUTE-REF2-OR-ARCH` | Владелец решения и границы ответственности; or.architecture |
| 12 | `ROUTE-REF2-OR-CODE` | Следующий шаг реализации без написания кода; or.code |
| 13 | `ROUTE-REF2-OR-DB` | План изменения/миграции данных без SQL; or.database |
| 14 | `ROUTE-REF2-OR-CLIENT` | Актуальность клиентского состояния; or.client |
| 15 | `ROUTE-REF2-OR-SERVER` | Порядок серверного действия; or.server |
| 16 | `ROUTE-REF2-OR-MOBILE` | Восстановление мобильной операции без дубля; or.mobile |
| 17 | `ROUTE-REF2-OR-DESIGN` | Видимость/редактируемость UI-поверхности; or.design |
| 18 | `ROUTE-REF2-OR-UX` | Действие пользователя и исправление ошибки без потери данных; or.ux |
| 19 | `ROUTE-REF2-OR-QA` | Причинная проверка результата; or.qa |
| 20 | `ROUTE-REF2-OR-AUTO` | Зависимости запуска/существующий handle/ожидание; or.automation |
| 21 | `ROUTE-REF2-CLARIFY-LANGUAGE` | Для перевода отсутствует целевой язык; clarify |
| 22 | `ROUTE-REF2-CLARIFY-TASK` | Недостаточно определено само предметное действие; clarify |
| 23 | `ROUTE-REF2-CLARIFY-COMPOUND` | Несколько самостоятельных действий при атомарном router; clarify |
| 24 | `ROUTE-REF2-UNSUPPORTED` | Ясное, но неподдержанное намерение; clarify |

По одной новой строке на класс — минимальное покрытие состава, а не устойчивая оценка обобщения каждого класса. Positive route и четыре разных причины clarify отчёт показывает отдельно; ошибочный route, ложный clarify и правильный route с неверным downstream-ответом имеют разные исходы. Перефраз того же TRAIN остаётся отдельным retention/perturbation-контролем.

Согласованная плановая квота: для каждого предметного профиля — 4 новых DEV и 4 новых EXAM; для router — **24 новых DEV и 24 новых EXAM** по 20 route-классам и 4 видам clarify из таблицы. DEV получает `LS158.ROUTE.DEV.01…24`, family `LS158/route/dev/F01…F24`; его 24 семьи независимы и от 24 EXAM-семей, и от TRAIN/reference/quarantine. Разные split-метки или ID не заменяют проверку независимости содержания. Учебных файлов с этими ID здесь не создано.

## 8. Непригодные подмены и оставшиеся пробелы

1. **Нет готового held-out:** нельзя переименовать часть существующего train или публичный reference в независимый EXAM и заявить обобщение. Отдельно считать acquisition/exact recall, discrimination, семейно независимое generalization и retention; показывать gained/lost RowIDs, не только общий процент.
2. **Нет полного adapter admission:** сначала документировать exact projection, mechanism (SFT/DPO/memory-KNN), recipient class/method, авторство, source locator, model/tokenizer/state identity. Все неизвестные поля явно `null/ADAPTER_IMPLEMENTATION_REQUIRED`. Metadata/rationale/error_tag/expected_domain не попадают в student input ради удобства.
3. **Draft quality не принята:** code/docs oracle=NOT_RUN; bilingual review S7/S8 не выполнен. Синтаксический parse, RAG-ID и shape schema не доказывают правильный chosen или единственную ошибку rejected.
4. **Коллизии и скрытое родство:** exact conflict map обязателен перед assembly; DOC/CODE одного исходника, reverse translation/typos/layout одного предложения — одна lineage-группа даже при разных F-номерах. Нормализацию и переименование исходных данных не выполнять.
5. **Quarantine не negative gold:** [quarantine/README.md](C:/Users/lubao/Documents/xiigen/llm%20mvp/learning-sets/preparation/quarantine/README.md) сохраняет `שלום` и `book` без разрешающего контекста, мобильный route без заданной мобильности, SQL-GUARD с недостающим правилом изменения. Они не являются отрицательным результатом обучения и не допускаются в TRAIN/DEV/EXAM автоматически.
6. **Механизм требует своего контроля:** rejected пара не заменяет оба положительных контекста; наличие отрицательной частицы не проверяет её scope; строковое равенство SQL/Angular не проверяет поведение; route success не доказывает subject success; сохранённый файл не доказывает восстановленное потребление.
7. **Ресурсный допуск:** в будущем показывать качество/retention, wall, CPU отдельно, allocations, peak/retained memory и полный outer+nested бюджет. CPU — оговорённое исключение; другие ухудшения >5% не скрывать общим улучшением или усреднением. Неизмеренное — NOT_MEASURED, не ноль и не «регрессии нет».

Граница любых новых файлов данных, согласованная с планом 158: `C:/Projects/llm_mvp_core/learning-sets/simulations/transfer-coverage-2026-11-04/<profile>/v1/`. Parent уже создал и исследователь прочитал [README этой области](C:/Projects/llm_mvp_core/learning-sets/simulations/transfer-coverage-2026-11-04/README.md): `SIMULATION_NOT_PRODUCTION / DESIGN_ONLY`, новых учебных пар и checkpoint там не заявлено. Это документационная граница, не готовые `<profile>/v1` данные. Будущий manifest фиксирует source path/line/pair_id/SHA256, derived family/origin/leakage, native projection и статус; оригиналы неизменны. Создание данных, adapter и запуск остаются отдельными назначениями; этот отчёт не выдаёт такого разрешения.

## 9. Честная граница чтения инструкций

Canonical AGENTS.md прочитан последовательно по диапазонам; локальные инструкции даны пользователем и сверялись с файлом. CLAUDE.md лично прочитан лишь частично; затем он и перечисленные здесь skills целиком обработаны инструментом с извлечением применимых правил. В стек входят architect-behavior-classifier, planning-session-startup, task-slice-golden-rule, conversation-intent-and-self-reflection, plan-review, code-execution, code-examination, generated-code-review, prompt-patch-authoring, output-readability-gate, human-readable-architecture-audit, human-communication-GLOSSARY, experiment-design, test-integrity; дополнительно обработаны intent-to-plan, phase-planning, improvement-measurement, trainable-architecture-decision. Context Mode skill и RTK прочитаны. Договор 120 прочитан целиком. **Полная машинная обработка не является полным дословным личным чтением всех skills/CLAUDE; такой claim не делается.**

Покрытие результатов: полная инвентаризация и синтаксический разбор указанных JSON/JSONL; выборочное предметное чтение; статическая сверка названных native consumers. Не проверены все семантические пары, все внешние исходники, runtime/import/обучение/restore. Это один исследовательский пакет, не N циклов review и не новая успешная экспериментальная серия.
