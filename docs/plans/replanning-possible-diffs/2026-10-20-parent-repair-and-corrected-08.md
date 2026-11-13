# №08 остаётся незавершённым: ремонт parent и исправленная учебная постановка

Это срез исправления20 октября. Ремонт parent и открытая цель №08 сохраняются; позднее добавление SFT-EXP и очередь47 описаны в [журнале21 октября](2026-10-21-study-log.md). Число46 ниже относится к прежнему срезу.

20 октября 2026. Изменения только в `C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs`. READ_ONLY_RESEARCH + документы; никаких build/test/inference/training, materialization host или изменения продукта.

## Исправлена ошибка предыдущего решения

Предыдущее утверждение «карточки исправлены» было слишком широким: сохранённую попытку №08 сняли с незавершённой работы, а необходимый ремонт принятия обученного состояния оставили в P02.PARENT со статусом schema gap и без entry. Это **не выполняло запрос пользователя**. Наличие native DPO Publish/Load не доказывает работоспособность SFT-цепочки №08.

В первичном N63I/Program.cs:48–50 создаётся свежая база; FrozenFactory:363–371 загружает эту базу, устанавливает новые adapters и создаёт новый AdamW. Обученный кандидат не выбирается в качестве родителя. Это конкретный разрыв caller integration. Он не доказывает универсальную поломку всех native loaders или то, что step1/parent=null всегда означает необученность.

## Теперь назначены два последовательных work order

| ID и карточка | Что автор назначил исполнителю | Что ещё не является результатом |
|---|---|---|
| [PARENT-ADOPTION-01](experimets/cards/PARENT-ADOPTION-01.md), №10 | H=PA01, typed SFT accepted writer/selector/session factory, H16 donor после1000, private repositories A/B, full optimizer comparison,4новых updates/29Forward/4Backward, один Run-PA01.ps1 | Модуль ещё не реализован/не собран. Механизм ремонта не объявлен работающим |
| [N63-INGRESS-LENGTH-LEARNED-01](experimets/cards/N63-INGRESS-LENGTH-LEARNED-01.md), №11 | H=N63IL; обе руки выбирают immutable accepted000002/step1000; A native length refusal, B200SFT updates;20scoring Forward; общий потолок220Forward/200Backward; один run.py | Учебная цель№08 открыта до фактического до/после; ожидаемые3/40→4/40 не выданы за новый результат |

Оригинальная [карточка№08](experimets/cards/N63-ingress-length.md) теперь `ATTEMPT_CONSUMED / LEARNING_OBJECTIVE_OPEN`. Старые raw/claim сохраняются. Отозванный старый entry не запускается, а новый ID явно описывает исправленную постановку этой незавершённой задачи. Repair receipt обязан иметь technicalStatus=COMPLETED и handoffReady=true; зависимый опыт не подменяет его случайным стартом.

PC01/H32/entry=null отменён как действующий handoff. Выбран H16, совместимый с исходной ingress fixture. SFT snapshot не получает выдуманную DPO lineage: новый schema честно обозначает imported reviewed snapshot и неизвестного исторического training parent. Adoption ordinal и optimizer step разделены. Две continuation updates parent-пробы не публикуются вместо исходного donor1000; corrected08 получает строго согласованный старт.

## Поправки к постановке опытов

- Факт сохранения результата, наличие update и достижение учебной цели — разные состояния.0→0 по правильным токенам/ответам не закрывает обучение. Частичный токенный прирост не называется освоением полного ответа; NLL и ресурсы остаются самостоятельными наблюдениями.
- Число2в старом№08 назначил автор, не пользователь. В corrected08 выбран конечный continuation budget200 по совместимой сохранённой диагностике, без обещания минимальности и без подбора доPASS. Предыдущие1000updates не повторяются тайно как подготовка.
- Старт задан до запуска: конкретный bundle, topology, tokenizer, полный AdamW m/v/step и applicable RNG. Сравнивается фактическое импортированное состояние; одинаковый первый loss не заменяет сравнение m/v.
- Repair parent и admission — разные факторы, поэтому два ID. В corrected08 parent одинаков для A/B; меняется только admission. Длинный материал явно deferred, не обрезан и не учтён как выученный.
- Сначала claim, затем prepare/build/model calls. Подготовка, scorer и gradient probes имеют отдельный явный бюджет. Никаких новых attempts из-за незаконченного отчёта или неудобного результата.

Эти правила внесены в [контракт29](experimets/29-learned-start-and-parent-continuation.md), P02, исходный№08, новые cards/prompts, входные README, [очередь](experimets/cards/00-execution-index.md), [output/entry](experimets/26-output-and-entry-contract.md) и machine registry. Модель исполнителя — default его текущей сессии; Student назначен отдельно. Текущая исследовательская Astra runtime не исполняет.

## Поступление dd150aa: что учтено

Входной пакет planning2d174 содержит35файлов плюс один pointer:28Markdown,7JSON,1CSV. [Поимённый intake](2026-10-20-card-reconciliation/dd150aa-intake.json) содержит bytes/hash, область чтения и решение для каждого. JSON/CSV прочитаны как структурированные metadata; это не новое независимое прочтение всех1571транзитивных файлов или одобрение161старой карточки. Шесть neural/review документов прочитаны worker `/root/rp01_primary_evidence`; остальные Markdown — родителем, включая synthesis/reviews и конкретные KNN recipes.

Внешние формулировки «№08 завершён, следующий№09» не переносятся в текущую очередь: №09 имеет уже более поздние сохранённые результаты, а пользователь явно вернула учебную цель№08 в работу. Исторические внешние review относятся к своим bytes; они не проверяли нынешние изменения. Внешний запрет изменять protected-папку относился к чужому поручению; здесь действует прямое разрешение пользователя на указанную внутреннюю папку.

Добавлены два отдельных фактора из поступления: [DPO-SHIFTED-CONTEXT-01](experimets/cards/DPO-SHIFTED-CONTEXT-01.md) и [IDENTITY-TRAIN-INDEX-01](experimets/cards/IDENTITY-TRAIN-INDEX-01.md). У SC01 учтены Prepare gradient-probes и ранний bridge guard, full target carrier при inputT−1, fixed calibration/comparison и один внешний entry. У IDX сохранён TrainOnExample перед identity-index, явная цена restore+Train,520предъявлений только8содержаний и post-output scorer. Оба имеют внутренние roots и собственные prompts; runtime отсутствует.

Всего46ID;9сохранённых attempts,37будущих постановок. Это **не46готовых или проверенных runtime**. Активное начало parent10→corrected08-11→N63C12. Хвост JSON42→SC0143→IDX44→IDENTITY45→KN46. Независимые resource-гипотезы сохраняются; несовместимый H16 не навязывается всемH64/H512карточкам. Их старые малые бюджеты не получают подтверждения learning-quality от этого ремонта; вывод о достаточности каждой панели остаётся ограниченным её собственными данными.

## Проверка и границы

Проверена документная согласованность:46уникальных ID, NEXT, таблица37future roots/entries; разобраны666JSON; проверены1240local link targets в65изменённых Markdown, broken0. Git status на этом срезе содержит70изменённых/новых файлов, все в разрешённой папке. Это статическая проверка документов, не tests/runtime и не проверка всех старых рецептов по существу.

Worker `/root/rp01_primary_evidence` вернул [ограниченный source/readback](2026-10-20-parent-adoption-repair-evidence.md) по parent/SC01 и исправленным участкам corrected08/IDX. Его замечания о native assembly identity, clean refusal наstep1000 и phase claims применены; у PA01 study.claim теперь до common model/load/panel. Это не независимое одобрение всего каталога. Исходники N63I/RESTORE/IncrementalTrainer/TranslationMachine читались адресно; binary load, tests и обучение не выполнялись. Предлагаемые команды остаются спецификацией будущих host. По разрешённой документации работа применена; механизм runtime ещё предстоит реализовать и проверить отдельным поручением.
