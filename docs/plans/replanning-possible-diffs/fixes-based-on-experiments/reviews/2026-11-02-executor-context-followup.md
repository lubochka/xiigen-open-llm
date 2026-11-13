# Проверка адресного контекста и P108 — 2 ноября 2026

Исследователь: **/root/post107_card_context**, единственный worker родителя /root; вложенные агенты не использовались. Фаза: READ_ONLY_RESEARCH и разрешённые Markdown-изменения. Этот пакет фиксирует проверку документов и сохранённых данных, не выполненную продуктовую приёмку.

В живые P17, P19 и P59 добавлены три разных контракта по их собственным PRIMARY. Новое обучение, вызовы Unit/Flow и расширение доз не назначены. В проекте переноса P108 закрыты два найденных замечания к измерителю и независимому expected. Полезный локальный результат P108 подтверждается сохранёнными ответами; готовность продукта, штатного Flow и восстановления не подтверждена.

## Основания и границы среза

- Основной срез P108: 066725d8f3e92d692a95f5566f353ce9053f4095. Дополнительный результат P109: 126c093201f187a409efef814a8ab3df582f8652.
- Новый аудит: 43afaefe7eee6daddccd25787012d1cfe4131bf7; полностью прочитаны приложение пользователя из attachments/5f394afd-a3e7-41bf-9dd6-3a1afc4e0724/Pasted text.txt, [находки126](../codex-126-postmerge-transfer-and-experiment-audit-2026-11-02.md), [ремонт127](../codex-127-postmerge-addressed-repair-plan-2026-11-02.md), [охват128](../codex-128-postmerge-card-coverage-2026-11-02.md) и исходная версия [проверок42](../../experimets/codex-42-postmerge-executor-checks-2026-11-02.md). Поздние изменения родителя в общих договорах — отдельный слой, а не старое доказательство.
- Во время readback внешний HEAD сменился на 5883e55ed7553dbc0f9b355c4b540f9ef30fe98a, затем на 605ac4c69491357822629055a47f3d46fc533f46. Наши вставки оказались в live файлах уже без рабочего diff. Worker не выполнял commit, merge или push и не подтверждал удалённую публикацию. Срез исследования не расширялся вслед за каждым внешним commit.
- Чужие изменения Tests/ExperimentFixes/AnswerReadingRejectsIncompleteCharacterIdentifiersTests.cs, TranslationPairTailSignatureLearningTests.cs, gate scripts и P11001 не редактировались и не приписываются этой работе. P110 здесь не аттестован; его наличие не разрешает повторный запуск.

## Три применённые адресные правки

| Живая карточка | Найденный пробел | Внесённое различение | Защищённая доза PRIMARY |
|---|---|---|---|
| [P17-tokenizer-refit](../../experimets/cards/P17-tokenizer-refit.md), P17-REENCODED-CONTENT | Новый generation, count=9 и width=64 совместимы со старым vector | Сопоставить девять actual vectors с ID и сохранённым перекодированием B. На копии заменить один vector отличающимся старым A при прежней форме и согласованном digest; comparator должен достигнуть сравнения содержания. Нет различия — NOT_DISCRIMINATING; нет сохранённых массивов — NOT_OBSERVED | 10 предъявлений, 3 BPE fit, 2 rebuild, 50 запросов; Load не добавлен |
| [P19-mutable-output](../../experimets/cards/P19-mutable-output.md), P19-FROZEN-EXPECTED-CONTENT | Expected может разделять изменяемый DTO/list с actual; одинаковая длина Steps не различает содержание | До мутации закрепить u1: review-first и [review, implement]. Копия с delete в первом шаге при той же длине должна нарушить comparator; expected и original observation неизменны. Native alias-защита доказывается назначенными чтениями Unit, не JSON-калибровкой | 16 предъявлений, 60 запросов; P19.2 и reload не добавлены |
| [P59-correction-visibility](../../experimets/cards/P59-correction-visibility.md), P59-IMPORTED-TARGET-READS | Успешный re-export не связывает обычный ответ с imported target | Cold target получает GetOrchestratorAsync и RunAsync; квитанция связывает instance/invocation, checkpoint и полный ответ. Подмена reader или отличающегося старого ответа при сохранном re-export обнаруживается. Если A уже равен expected, это NOT_DISCRIMINATING. Отсутствующая identity/branch evidence отмечается NOT_OBSERVED | 1 новый урок, 3 import, 54 Flow в назначении; исторические фактические 80 Flow сохранены отдельно |

P59 не превращён в чужой KNN-only опыт: exact-ответ допустим для его exact-запроса, please-вариант сохраняет прежний action/object oracle. Будущее native отключение применения loaded map при сохранном re-export принадлежит M03c плана127; его выполнение здесь не заявляется.

До записи все три карточки прочитаны целиком: P17 — 510 строк, P19 — 518, P59 — 380 в исходном снимке. Усечённые фрагменты P17 и P19 прочитаны отдельно. Правки помещены перед EXECUTOR-CONTEXT-120:END, не в PRIMARY recipes. Перед каждой записью проверялся текущий hash; изменение чужого текста не откатывалось.

После записи удаление только собственного нового блока восстановило исходный SHA256 каждого файла — 3/3. Это проверка сохранности bytes, не три научных аудита. В собственных вставках выровнены три CRLF на LF в каждом файле; остальной файл не нормализован. Исходные SHA:

| Файл | SHA256 до собственных вставок |
|---|---|
| P17 | 96ec4ebf69fe88b0f30cc01158b62be30934bd8bb3ec9d8de3ced197b9a1a8c0 |
| P19 | 35514fee8f21640031da5a3bf928937d402c4120ae0d7b97edb4efa4cb14e566 |
| P59 | 3a98eb00dc743d267c3aab8c13e87b506af2cefad1a03294b0210dce21602aae |

Поздний prompt wrapper родителя является отдельной согласованной дельтой. Приведённое восстановление относится к снимку до неё; будущий общий hash не обязан совпадать с исходным.

## Соседние карточки: почему повторные блоки не добавлены

Это адресное чтение применимых разделов, не полное повторное чтение всех перечисленных файлов и не научная аттестация всего каталога.

| Карточка | Уже имеющийся договор, закрывающий рассмотренный риск |
|---|---|
| [P18-state-identity.md](../../experimets/cards/P18-state-identity.md) | Consumed bytes/identity, отсутствие payload как NOT_VERIFIED, собственная размерность запроса и запрет нового обучения |
| [P21-hnsw-ingress-atomicity.md](../../experimets/cards/P21-hnsw-ingress-atomicity.md) | Полный граф и отношения после Load, отказ malformed до публикации, native failure до внешнего restore |
| [P22-hnsw-rng-continuation.md](../../experimets/cards/P22-hnsw-rng-continuation.md) | Состояние RNG вместо одного seed, continuation Add, раздельные graph/output и точная доза |
| [P67-correction-identity.md](../../experimets/cards/P67-correction-identity.md) | Порядок old→correct→control на том же экземпляре против обратного; authority не выводится из одной метки |
| [P75-sampler-residual.md](../../experimets/cards/P75-sampler-residual.md) | Единственный residual factor, CDF0 отдельно, фактический consumer и расход; уже перенесённое не назначается заново |
| [P78-loss-plateau.md](../../experimets/cards/P78-loss-plateau.md) | Реальная cadence/ShouldStop origin; реконструкция не называется live; бюджет не расширяется |
| [P89-density-stop.md](../../experimets/cards/P89-density-stop.md) | Фактический масштаб 13/12 и 3/3 updates, отсутствие выдуманных 40% savings; producer/consumers и mixed-signal oracle раздельны |
| [P96-async-orchestration.md](../../experimets/cards/P96-async-orchestration.md) | Scheduled/entered/completed/consumed IDs различаются; независимый denominator, ошибки/потери и реальный async отделены от source-only |
| [N61-mode1-retention-and-identity.md](../../experimets/cards/N61-mode1-retention-and-identity.md) | RETENTION и IDENTITY отдельно, полный PlanText не считается выполнением Flow; собственный контроль чужого body при правильном header |

Имена выше обозначают семейства просмотренных карточек; утверждение об отсутствии иных дефектов в непрочитанных разделах не делается. Точечные основания были в верхних контрактах/PRIMARY: P18 1–126, P21 1–137, P22 15–56, P67 1–49, P75 1–50, P78 1–75, P89 1–72, P96 1–68; у N61 — верхние контракты 1–32 и 46–65. Усечённые промежутки соседей не засчитаны как прочитанные целиком.

## Независимая проверка P108 и проекта126

Проверены [arms.json P108](../../../../../experiments-v2/RP01-MICRO/P10801/results/attempt-0001/arms.json) и точные литералы [Program.cs](../../../../../experiments-v2/RP01-MICRO/P10801/Program.cs):169–201. Сопоставлены сами route и оба ordered Steps, не только сохранённый флаг оба_точны. Для u1–u4/e1–e2 источник задаёт review-first/[review, implement]; для остальных — build-first/[implement, review]. Все сохранённые gold совпали с этими литералами.

| Панель | Полный ответ совпал с независимыми source literals | Сохранённый флаг доставки признаков | Средняя confidence |
|---|---:|---:|---:|
| Учебные контроли | 8/8 | 8/8 | 1 |
| A, экзамен | 12/12 | 4/12 | 0,6029833333333331 |
| B, экзамен | 12/12 | 12/12 | 0,8772499999999998 |

Счёт доставки использует сохранённые признаки/флаги; он не заменяет отсутствующую инструментированную квитанцию consumed input. Первый служебный пересчёт worker содержал ошибочный литерал implement-first вместо build-first; он был исправлен после точного чтения источника. Итоговая таблица относится к исправленному аналитическому сопоставлению. Продукт, raw и gold опыта не менялись.

Прочитанные участки [KnnLogicalUnit.cs](../../../../../21.AgentInfrastructure/Logical/KnnLogicalUnit.cs):351–390 показывают ограничение predicateObject is string[] и обычный путь Vectorize→Search. SHA256 этого файла при проверке: 3dfc1e9f40f0800e6cb8b431efc1609dbce1d08a9fc35d1e8a5ea298b2b5d70a. Участки Program.cs:938–969 и 998–1087 показывают различие продуктового A и экспериментального B через доступ к состоянию/приватным методам. Участок 870–897 подтверждает неполную композицию итогового статуса: orderInsensitive вычислен отдельно, а exit0 сам по себе её не доказывает. Весь Program.cs целиком в этой проверке не перечитывался.

**Вывод:** есть узкий полезный результат чтения обученной памяти и явная ошибка обработки носителя. Нет измеренного роста точности, отдельной ресурсной эквивалентности A/B, product Flow или Save→новый объект→Load. Confidence не является калиброванной вероятностью. Порядок множества предикатов не тождественен порядку выходных Steps. Другая постановка Q127 не поглощается этим переносом.

[Проект126](../126-p108-predicate-carrier-transfer-2026-11-02.md) первоначально прочитан целиком; после редакции повторно прочитаны изменённые условия допуска, M06, oracle и бюджет. Найденные замечания и readback:

1. Не хватало обязательной связи с M06-P108. Теперь §2 связывает приёмку с планом127 §8.2: saved-only required-set/finalizer, полный бюджет двух процессов, terminal/deadline и NOT_OBSERVED/HOLD при отсутствии consumed input. Новые Train/query не выдаются за ремонт старого отчёта.
2. Фраза о непередаче эталона оценщику была неверной. Теперь §5 передаёт evaluator отдельно frozen expected и actual; gold не идёт learner/consumer, mutation name не служит подсказкой. Правильный raw false не является check failure; meta finalizer не требует собственного PASS во входе.
3. §1 теперь разделяет 40 завершённых наблюдений, 24 наблюдения прерванного процесса и 8 суммарных cold: 64 наблюдения, 2 Train/16 предъявлений; 42 Execute и 41 source-derived Search различаются по основанию. Это уточнение исторического учёта M06, не новый инструментированный замер worker.
4. Доза будущего класса арифметически согласована: 8+12+3+4+2+2+2+2=35 Execute на руку, 1 Train/8 уроков. Red/green — два физических исполнения. Отдельная таблица восьми regression methods даёт 16 Execute; вместе cap 86. Итог 12 входов Train, 9 успешных/3 ранних отказа, 29 принятых/4 отказанные строки и 9 BPE fit следует из указанной родителем таблицы. Тела всех восьми существующих test methods worker независимо заново не пересчитывал; это ограничение проверки бюджета, не выполненный замер.

Устранение двух замечаний принято как редакторское улучшение плана. Это не READY всей программы. Сохраняются M06, конкретный штатный caller, ресурсы, нативная реализация и её разрешённая будущая приёмка. Пробный Rp01MechanismProbe не объявляется production Flow; стык prompt.compiled.predicate_ids→predicate_ids остаётся у E2E владельца20. S0 не получает выдуманный native Load; восстановление остаётся у владельца101. Текущих падений тестов worker не заявляет.

## P109 как граница отбора новых переносов

По [arms.json P109](../../../../../experiments-v2/RP01-MICRO/P10901/results/attempt-0001/arms.json) отдельно прочитаны 16 строк: до/после × A/B × TR/E1/E2/T1. Независимые арифметические gold: 2+3×4=14, 3+2×5=13, (3+2)×5=25; trained знак «+» проверяется отдельно. Сохранённые ответы обоих плеч не решают три численных задания: 0/3→0/3; «+» сохранён 1/1→1/1.

После обучения отношение B/A по p(gold): TR=1,230070993536098; E1=1,0675435442558563; E2=0,7808802224620457; T1=1,0665149278805002. Следовательно, локальные положительные teacher-forced оси TR/E1 сохраняются, но E2 ухудшен на 21,91197775%. При критерии пользователя «без другой регрессии больше 5%» и отсутствии роста generation это не новый назначаемый перенос.

[metrics.json P109](../../../../../experiments-v2/RP01-MICRO/P10901/results/attempt-0001/metrics.json) прочитан как сохранённая квитанция: 4 updates на руку, 8 всего, 16 generation, 16 scoring, 8 training forward и 34 evaluator calls. Worker не запускал эти операции и не проводил полный аудит всех исходников P109. Подтверждена указанная граница отбора, не вся техническая приёмка опыта.

## Проверка копируемых промптов родителя

На первом этапе проверена первоначальная пофайловая выборка 41 строки [отчёта129](../codex-129-executor-context-followup-2026-11-02.md), а не окончательная полнота каталога. В каждой из 41 вставок найдены точный собственный полный путь, договор120, роли40 и проверки42. Носители этих вставок: 28 blockquote, 12 fenced text (включая ~~~), один plain «Промпт» в83. Представительные исходные блоки 13 и29 и абзац83 прочитаны с окружением. Более строгая последующая проверка выявила предел этого вывода: вставка внутри вводного blockquote ещё не означает её присутствия в отдельно копируемом actual prompt ниже. Первоначальная проверка не доказывала самодостаточность всех 41 поручений; обнаруженный разрыв исправлен на завершающем этапе ниже.

Отдельный read-only diff этой выборки к 126c093 показал 40 новых полных entry, вставку в83 и шесть замен только фразы no-reread. Сопоставление общих префикса/суффикса замен подтвердило сохранность остального текста строк. Иных содержательных удалений/замен в этой выборке не было; recipes, команды и бюджеты не изменены этими операциями. Точное обратное SHA-сопоставление 41/41 сообщено родителем; worker не выдаёт его за собственную byte-проверку. Собственная проверка bytes выполнена для трёх своих карточек.

Родитель затем обнаружил дополнительные заголовки «Промпт и будущие свидетельства», «Самодостаточный промпт» и расширил работу. Эти поздние блоки не включались в первое 41; их последующая ограниченная проверка описана ниже. Дополнительные 11 замен no-reread пересекаются с 41, поэтому сумма 52 не является числом уникальных файлов. Отсутствие ссылки внутри передаваемого блока исправляется документно; будущее фактическое чтение агентом из этого не следует.

## Фактическое свежее чтение правил и навыков

Полный запрошенный reload завершён **до содержательной проверки и записи трёх карточек**. Прочитаны local AGENTS (163 строки), canonical AGENTS (1–8705) и CLAUDE (1–4373). Усечённые участки AGENTS 1583–1642 и 8333–8354 восстановлены точными отдельными чтениями. Это подготовка одного worker, не число циклов/независимых ревью.

Все файлы следующей таблицы находятся в C:/Projects/llm_mvp_core/docs/skills. Диапазоны относятся к прочитанному снимку, не к обещанию неизменности файлов после внешних commit.

| Группа | Полный файл | Прочитанные строки |
|---|---|---:|
| Основной | architect-behavior-classifier-SKILL.md | 1–1967 |
| Основной | planning-session-startup-SKILL.md | 1–2372 |
| Основной | task-slice-golden-rule-SKILL.md | 1–344 |
| Основной | conversation-intent-and-self-reflection-SKILL.md | 1–490 |
| Основной | plan-review-SKILL.md | 1–4273 |
| Основной | code-execution-SKILL.md | 1–366 |
| Основной | code-examination-SKILL.md | 1–376 |
| Основной | generated-code-review-SKILL.md | 1–859 |
| Основной | prompt-patch-authoring-SKILL.md | 1–242 |
| Основной | output-readability-gate-SKILL.md | 1–1013 |
| Основной | human-readable-architecture-audit-SKILL.md | 1–442 |
| Основной | human-communication-GLOSSARY.md | 1–47 |
| Ролевой | intent-to-plan-SKILL.md | 1–224 |
| Ролевой | phase-planning-SKILL.md | 1–1231 |
| Ролевой | improvement-measurement-SKILL.md | 1–213 |
| Ролевой | experiment-design-SKILL.md | 1–103 |
| Ролевой | test-integrity-SKILL.md | 1–788 |

Усечения startup 255–258 и 1910–1940 восстановлены; чтение plan-review доведено непрерывно до конца. Context-mode не смог открыть canonical repo за пределами своего project root; использованы ограниченные точные host-чтения, без изменения настроек или обхода ограничения. Инвентаризация, byte-processing и поиск по словам не засчитывались вместо полного чтения. Полное чтение родителя этим не заявляется. После steering внутри того же поручения уже свежо прочитанное не выдавалось за второй reload.

Применение навыков видно в дельте: PRIMARY и одна переменная сохранены; expected отделён от actual/learner; положительный и отрицательный случаи различают сам проверяемый механизм; missing evidence не превращено в PASS; native/saved-only, научный результат, бюджет и техническое завершение разделены. Historical runtime/commit указания навыков не расширяют прямое исследовательское поручение пользователя.

## Что этот пакет не утверждает

Ни одного build/test/Train/inference/Flow/replay worker не запускал. Аналитические чтения JSON и арифметические пересчёты не являются вызовами модели. Продуктовые исходники, hosts, datasets, receipts и Git state не изменялись worker. Ни 277 карточек, ни первоначальные 41 промпт не называются 277/41 независимыми научными аудитами. Содержательная дельта worker — три адресные карточки; отдельная механическая дельта завершения — 143 actual prompt-вставки в 143 файлах и этот review. P17/P19/P59 входят в эти 143, поэтому числа уникальных файлов не складываются. Общие входы, исходные 268 вставок контекста, индекс и отчёт129 принадлежат родителю.

Readback собственных трёх содержательных блоков: каждый присутствует ровно один раз, CR внутри вставок отсутствует, различающие WRONG/CORRECT сохранены. На первом этапе для review проверены 23 относительные файловые ссылки — все пути существуют; Markdown anchors и runtime-поведение этим не проверяются. Ограниченный git diff --check по трём карточкам и пути review завершился с exit0; это проверка документного diff, не запуск продуктовых тестов. Новый untracked review дополнительно проверяется как текст, поскольку git diff не включает untracked содержимое.

## Завершающая проверка фактической границы копирования

Это продолжение того же документного задания, без нового reload, runtime или внешнего аудита P110. Пересчитана [полная карта родителя](../codex-129-executor-prompt-coverage-2026-11-02.md): 185 различных путей, из них 147 experimental cards и 38 transfer/general docs. В ней ровно 41+135+92=268 исходных вставок контекста; последние 92 находятся в 83 файлах. Суммы и соответствие существующим путям подтверждены, но первоначальное число вставок само по себе не доказывало границу передачи.

Показательный WRONG: E-DPO-SFT-ORDER:231 содержал новый вход внутри вводного blockquote, а отдельный самодостаточный абзац «Полная карточка…» на239 его не содержал. При копировании только последнего терялись точные120§1/40§2/42. То же найдено у 129 plain абзацев в кавычках. Ещё 14 самостоятельных блоков имели разрыв между вводными правилами и поручением: 13 quote и один plain. Всего исправлены 143 actual фрагмента в 143 файлах внутри исходной области185.

CORRECT: короткий «Самодостаточный вход» помещён непосредственно в абзац поручения — после открывающей « либо после Markdown-префикса >, либо в начало plain-абзаца. Собственный абсолютный путь уже имеющегося текста сохранён; где его не было в том же фрагменте, он добавлен. Все прежние слова, команды, PRIMARY, числа и бюджеты сохранены. Вводные wrappers не считаются заменой этой проверки.

Перед каждой записью зафиксированы SHA256 и точная строка, затем повторно проверен hash непосредственно перед apply. Readback сверял полный записанный текст с ожидаемым; удаление только новой вставки и восстановление исходной кодировки дали каждый исходный SHA bytes — **143/143**. Сохранены исходные BOM и LF/CRLF, включая SILU. Это проверка обратимости документа, не 143 запуска или научных ревью. Пофайловый исходный SHA и строка находятся в приложении ниже.

Итоговый extractor проверял только содержимое реально передаваемого фрагмента: для новых actual/inline — соответствующий абзац; для остальных — непрерывный blockquote или полный замкнутый fenced text, отдельно plain83. Соседние заголовки и вводные абзацы не подмешивались. В 33 оставшихся main prompts дополнительно проверено присутствие самого поручения внутри того же блока, а не одного общего вступления.

| Проверенная часть | Фрагментов | Результат |
|---|---:|---|
| Исправленные actual абзацы | 143 | Exact self-path,120§1,40§2,42 и граница авторизации находятся внутри |
| Ранее дополненные inline-параграфы | 92 | Те же обязательные поля внутри каждого абзаца |
| Остальные main prompts | 33 | 18 blockquote,14 fenced text,1 plain83; обязательные поля внутри фактического блока |
| **Точный проверенный набор** | **268** | **Незащищённых фрагментов в этом наборе: 0** |

Различающий документный контроль: из копии каждого извлечённого фрагмента поочерёдно удалялись self/120/40/42 либо граница «документальное поручение не разрешает запуск/исполнение». Подмена self указывала на WRONG-CARD. Все полные копии прошли проверку полей; ни одна повреждённая копия не была принята. Исходные файлы при этих контрольных преобразованиях не менялись. Это статическая чувствительность проверки передачи текста, не выполненная проверка поведения модели, выполнения навыков или качества эксперимента.

268 исходных вставок контекста и 143 исправления actual границы не являются 411 опытами или 411 независимыми промптами. Число268 в итоговой таблице получено заново из actual набора 143+92+33, а не автоматически перенесено из числа первоначальных вставок. Охват ограничен именованными185 файлами и указанными копируемыми фрагментами; произвольные исторические цитаты и вторичные продолжения инструкций не объявляются новыми самостоятельными заданиями.

Также прочитан новый абзац «Последующие ошибки переноса» договора120:39. Он прямо включает42 для standalone executor-файлов через120, задаёт40§2 и применимость по собственному PRIMARY, запрещает наследовать чужие fixtures/calls/budget и закрепляет PM-C10. Это отдельное центральное документное изменение родителя, не ещё25 выполненных проверок и не часть143. Новая 25-кратная вставка не требовалась.

Финальный readback review: 200 относительных файловых ссылок, отсутствующих путей0; trailing whitespace и conflict markers0. Документный diff-check по каталогу cards и этому review прошёл с exit0 при разовом core.whitespace с cr-at-eol; Git config не записывался, исходный CRLF не нормализовался. Непроверенные Markdown anchors и фактическое выполнение будущим агентом остаются за пределами этой статической приёмки.

### Адреса 143 исправлений actual границы

Строка относится к снимку записи; вставки не добавляют переводов строк. SHA — bytes непосредственно перед этой вставкой, после ранее внесённых родителем контекстных дополнений. Для каждой строки независимо выполнены compare-before-apply, readback и inverse SHA. Таблица — карта одной документной операции, не число исследовательских циклов.

| Файл | Строка | Носитель | SHA256 до вставки |
|---|---:|---|---|
| [AST-SIB-CACHE-01.md](../../experimets/cards/AST-SIB-CACHE-01.md) | 299 | quote | 23da7c6b8541899e649bd6102e00622caf6034cf3a7cc559ad72171677d49ccc |
| [E-DPO-SFT-ORDER.md](../../experimets/cards/E-DPO-SFT-ORDER.md) | 239 | guillemet | b80bf70d0dc869919f3eedcce7b86ccf3288ea5676f72ea5907f96d3d783bd9f |
| [E-DPO-VALID-LENGTH.md](../../experimets/cards/E-DPO-VALID-LENGTH.md) | 258 | guillemet | e49b9aaf2c7e0237d487c9d8836dac1881ea7b46a8eecbbf60cd517f86cea35d |
| [E-SFT-MOMENT-CONTINUITY.md](../../experimets/cards/E-SFT-MOMENT-CONTINUITY.md) | 233 | guillemet | 6b92027e9d84ff04183c36152bbdbad776bdae0981393e6fb1c04332d3b9eb58 |
| [E-SFT-RESPONSE-MASK.md](../../experimets/cards/E-SFT-RESPONSE-MASK.md) | 232 | guillemet | fd9e1bbf9a6c23ddaae84785eb475bd7fc05bd1d8365b9911f15f41a82eb2d6b |
| [FROZEN-BROADCAST-MAP-01.md](../../experimets/cards/FROZEN-BROADCAST-MAP-01.md) | 183 | quote | e2a7fefebe33c49ef73fbdf86e96d8ebd43f5a0746217388acf22cf10f90798e |
| [GRAD-ACCUM-COPY-01.md](../../experimets/cards/GRAD-ACCUM-COPY-01.md) | 225 | quote | b1afdcd090d28e800b2dba706ee2d8a890eaec76560baa4950464799ff2f014f |
| [METHOD-KNN-TARGET-SCOPE-01.md](../../experimets/cards/METHOD-KNN-TARGET-SCOPE-01.md) | 223 | guillemet | a57273ebf67967873779ecf4e4ec9e8f4f5b8f4d0940a3a171a495fb655cf4b3 |
| [N-KU-01-do-loop-label.md](../../experimets/cards/N-KU-01-do-loop-label.md) | 745 | quote | b701834b46d85eefdd60f2aa110f33a4952c53b39b8f4ae990f129a925628457 |
| [N60-e1-verifier-admission.md](../../experimets/cards/N60-e1-verifier-admission.md) | 371 | quote | e920e1a980a2f3eaf3a46bedee03c106d1e54f71e8a8c3e8bbf4ca36a08d7a0b |
| [N61-dpo-reference-reuse.md](../../experimets/cards/N61-dpo-reference-reuse.md) | 383 | quote | d82ec0bd0c85da2dd56ce89b0a30dc9563eb3ec06390dce145a07ecdfa50a87b |
| [P01-trainable-recipient.md](../../experimets/cards/P01-trainable-recipient.md) | 475 | guillemet | ccc8b9dfa0833bc209338e67109f3b9fdf7600c62d46edfc678e42e0e4c728e6 |
| [P02-warm-base.md](../../experimets/cards/P02-warm-base.md) | 485 | guillemet | 10ab02f1eb94e212536f430a1e295979ccf6ea4f0196c7ce5e40812fa460a64c |
| [P03-historical-score-lineage.md](../../experimets/cards/P03-historical-score-lineage.md) | 401 | guillemet | a98507f69079962f990d9eb176bd61ea54257168538ecf1e22a7865df68d31c0 |
| [P04-preference-contrast.md](../../experimets/cards/P04-preference-contrast.md) | 469 | guillemet | 294057d156c71890162fbec79dba8855ec5c6453adc4dd06f3bc3022c8a19428 |
| [P05-response-length-objective.md](../../experimets/cards/P05-response-length-objective.md) | 455 | guillemet | d218ad31f2d34378c62b21f054eed9cd73710b77dff938e1c36fef54a00bc912 |
| [P06-teaching-vector-adamw.md](../../experimets/cards/P06-teaching-vector-adamw.md) | 455 | guillemet | 31adc12f9a82c2710e9e1b9ff0b347072cca809dc84ba9e35e60ebaa71dffd4d |
| [P07-materialized-token-contrast.md](../../experimets/cards/P07-materialized-token-contrast.md) | 478 | guillemet | 649cfd772bf260f1d77f7b868ddcccb13a5c22b056f2965696f1e766fbeced26 |
| [P08-answer-boundary.md](../../experimets/cards/P08-answer-boundary.md) | 470 | guillemet | adb947a7594c9825f9e3b726d9e2010ece2a04533556dfbb2d089b7789df9745 |
| [P09-cached-forward.md](../../experimets/cards/P09-cached-forward.md) | 459 | guillemet | 2bfda3cd7f396406b7b09e4ef326b26fa2cdeb55abbc56a8dee1da0f8f1ae3cd |
| [P10-composed-gradient.md](../../experimets/cards/P10-composed-gradient.md) | 449 | guillemet | e3e078dc4a16784c3946d45026fd0577ebcaeb02d14139e0e2ab837ea8e7b49d |
| [P100-topk-ties.md](../../experimets/cards/P100-topk-ties.md) | 500 | guillemet | cc915ca60709c83344d5692ba9e6b9d170b261ec5dd204cba3d7691bd5b373b8 |
| [P101-learning-budget.md](../../experimets/cards/P101-learning-budget.md) | 501 | guillemet | da3d3b48c40f9aafd6a9fe6668700a975f974f3fddecdff2bd85e2b695483efb |
| [P102-teacher-forced-decoding.md](../../experimets/cards/P102-teacher-forced-decoding.md) | 493 | guillemet | 51d835b0f5afed106b31f70fd391731d042b0dcf1c0e132e8b91cc11a5894816 |
| [P103-stack-before-normalization.md](../../experimets/cards/P103-stack-before-normalization.md) | 413 | guillemet | 23e4f58479cfb0afe57f06968abe1e7399b5d5216faae5b6e8473a1b52c34265 |
| [P104-law-eligibility.md](../../experimets/cards/P104-law-eligibility.md) | 579 | guillemet | 7be143c075f07194b5e87e5abe9377942893f8e1080b09df16f2561b4d936295 |
| [P105-frozen-reference.md](../../experimets/cards/P105-frozen-reference.md) | 495 | guillemet | 9f03c24265407cf994000a0f9b469cb1d5e85be95e4e2cb60c508d18b7d8e985 |
| [P106-corrected-semantic-target.md](../../experimets/cards/P106-corrected-semantic-target.md) | 447 | guillemet | 41ce2baf1449f07d3427816744ef706f901168faf43ad2c1f5d1c80aa09aeb78 |
| [P107-lawful-neighbor-aggregation.md](../../experimets/cards/P107-lawful-neighbor-aggregation.md) | 577 | guillemet | 4f29d4c3df9d8f95e8740de31dac33aa276f178d2dcb139a832e3789e06bdda1 |
| [P108-metadata-carrier.md](../../experimets/cards/P108-metadata-carrier.md) | 482 | guillemet | d1119a8d437e702daaeb7e0bfd1509a14d5bd8a232889a186e08e2f255339be0 |
| [P109-gold-validity.md](../../experimets/cards/P109-gold-validity.md) | 573 | guillemet | 8f1c0d89f8bda140b74313b8ef915cd4daec5511f7b9ab7df55b883cafd970c8 |
| [P11-autograd-reachability.md](../../experimets/cards/P11-autograd-reachability.md) | 499 | guillemet | fab751499b944804c251d7b4bcc37831f83fa9d9ff3c8f616c2d30ff28454e6d |
| [P110-numeric-matrix-cost.md](../../experimets/cards/P110-numeric-matrix-cost.md) | 504 | guillemet | 4e7817ccf16d2cb55214c537ddc237416e718c89b7e1e1beaf737845ec5ee0fd |
| [P111-reward-credit.md](../../experimets/cards/P111-reward-credit.md) | 328 | guillemet | 338aadcfa901f88e1fead1536ae2337d9f9d9b09d736d41d78776a9c25e46781 |
| [P112-family-disjoint-evaluation.md](../../experimets/cards/P112-family-disjoint-evaluation.md) | 537 | guillemet | 0dbb3516a52fe2bf907173cf255399fdba21874f857340d637842ea3a6fdbdfa |
| [P113-feature-energy.md](../../experimets/cards/P113-feature-energy.md) | 537 | guillemet | 52cedaf08a553c783b45630e1fc56bccea438f29fa1660101f6cc194c3bb6fa9 |
| [P114-zero-weight-pair.md](../../experimets/cards/P114-zero-weight-pair.md) | 474 | guillemet | ef99b90b9452ed0585b894d8ed85ea2c8013a60db57f6edce400a9087de18285 |
| [P115-bm25-statistics-cache.md](../../experimets/cards/P115-bm25-statistics-cache.md) | 523 | guillemet | 692986a3bc55dac7c1b0b6aa173562eec5005aa26f84dbe093b1d63e4a9c8db5 |
| [P116-e43-file-boundary.md](../../experimets/cards/P116-e43-file-boundary.md) | 383 | guillemet | d902ba04821e23a11b6ee884e4e57f9ab21deb8ef204d6e6e2599984e091d4eb |
| [P117-translation-direction-memory.md](../../experimets/cards/P117-translation-direction-memory.md) | 403 | guillemet | 76ca6e6b7660979a6fe19b363b9d7f3ad5c2c44de5ab8a56ccec9372379ecbca |
| [P118-projectwiki-content.md](../../experimets/cards/P118-projectwiki-content.md) | 340 | guillemet | 195acc95b0f6fdfd25c46421c7e1fac96c9f141515b398c0eea69f116e9fb221 |
| [P119-learning-dynamics.md](../../experimets/cards/P119-learning-dynamics.md) | 479 | guillemet | 8b0fb10f7016739bc40e56a7b218c50a6cb5e6fa617d2710ebbdeefaa032b928 |
| [P12-optimizer-boundaries.md](../../experimets/cards/P12-optimizer-boundaries.md) | 476 | guillemet | e1fe29d7027028acb3242c0b21fb6047f4ead896511fb61fb06d6f420484e6ce |
| [P120-semantic-compression-laws.md](../../experimets/cards/P120-semantic-compression-laws.md) | 374 | guillemet | 0aa9ad35603be9ca439b7e025763f2a346aae12b17b5656bac933f93c4961b2c |
| [P121-project-adapter-selection.md](../../experimets/cards/P121-project-adapter-selection.md) | 384 | guillemet | 4a374114803730480b93c857223b492611d85f74d67e107a80663b3a81256de1 |
| [P122-root-channel-unicode-digits-operators.md](../../experimets/cards/P122-root-channel-unicode-digits-operators.md) | 552 | guillemet | e54588feac936f3fb1cd4cb9a9eb3b8a52818b3f67669350c2fc00c5d3d320f9 |
| [P123-design-conversion-critic-evidence.md](../../experimets/cards/P123-design-conversion-critic-evidence.md) | 326 | guillemet | bacb46aca9c1cb13bdc27b94e9f41a63b62a6bfe1eab19a9fe3a2f389ad5190d |
| [P124-p3-fixed-discriminator-contract.md](../../experimets/cards/P124-p3-fixed-discriminator-contract.md) | 366 | guillemet | a27347ee75b4c485c96e41ee792d1a635cea93c134f7a566f9cbbc04245aa815 |
| [P125-executor-memory-learning.md](../../experimets/cards/P125-executor-memory-learning.md) | 453 | guillemet | bd672190aff3bbdfd266beeb363c324764c204239b7e358e6fbf535c1415de33 |
| [P13-finite-ingress.md](../../experimets/cards/P13-finite-ingress.md) | 439 | guillemet | 66c16eea9e53c062107911e6bef889ce47a1db2031a499c6c1ad39132e262af2 |
| [P14-teacher-provenance.md](../../experimets/cards/P14-teacher-provenance.md) | 477 | guillemet | 056aa4bd3ba67f28a55aae2b9cc975444a27e418a052ca145367870ff97f9346 |
| [P15-retention-replay.md](../../experimets/cards/P15-retention-replay.md) | 579 | guillemet | 77acc33b1dc1fafee280089a73bcf050a40b90dfbcf9b8e3ee28b0edd20d3c1c |
| [P16-semantic-order-roles.md](../../experimets/cards/P16-semantic-order-roles.md) | 650 | guillemet | 93d32d761afcea2c80614d19e32d8bc0502bcff5aa9cbcb219ed9bb0b1f7525e |
| [P17-tokenizer-refit.md](../../experimets/cards/P17-tokenizer-refit.md) | 512 | guillemet | bce597d3f6c8821ac3a135e8545a259782f3533731100df0e59b7f553932534f |
| [P18-state-identity.md](../../experimets/cards/P18-state-identity.md) | 568 | guillemet | e1534b1ab2f53d58c77c485a5e203c4378c5a3c341b9372fb81eafee54e71d2e |
| [P19-mutable-output.md](../../experimets/cards/P19-mutable-output.md) | 520 | guillemet | 5ccfd89d7cbdac9a5516a99f0c5ca1186a5bd31c9493d1d91f8aee0e9faa94be |
| [P20-hnsw-reachability.md](../../experimets/cards/P20-hnsw-reachability.md) | 531 | guillemet | 86b8b3d998bcc89d6c9bc7f370b403c90494483272b76402d64d0886c740810a |
| [P21-hnsw-ingress-atomicity.md](../../experimets/cards/P21-hnsw-ingress-atomicity.md) | 584 | guillemet | a5d6b2e87dc823edee2bb01e2a917ee5f7608d270ceddc07e82a2a9d52533947 |
| [P22-hnsw-rng-continuation.md](../../experimets/cards/P22-hnsw-rng-continuation.md) | 501 | guillemet | d2546da70e6fc4711296fb65f3be34b63e8cf276929332d33409b35e8334446d |
| [P23-retrieval-fusion.md](../../experimets/cards/P23-retrieval-fusion.md) | 515 | guillemet | bf8992c89ad74568e6d5ddc9949440cb071a9d39b851d6565ab1c2931d204259 |
| [P24-replay-selection.md](../../experimets/cards/P24-replay-selection.md) | 541 | guillemet | 8cf85dce2bc5b53fcfc193817f04611eada5297bac61b7a887224d9121084922 |
| [P25-cold-restore.md](../../experimets/cards/P25-cold-restore.md) | 604 | guillemet | ec4b56a95417802ef07be02255527e6ea1c3e983fd11e6409e8e7184f1a32882 |
| [P26-structural-features.md](../../experimets/cards/P26-structural-features.md) | 336 | guillemet | fe727e683edcd0e28be7c154fb1548f38a63af5ae8e41e0c70fe6543ee6868af |
| [P27-structural-codec.md](../../experimets/cards/P27-structural-codec.md) | 346 | guillemet | ef81cf7ba170cb42044d4c68f6b6798d3910c200ad5cd9489a3a4fa3787dd1f2 |
| [P28-morphology-family.md](../../experimets/cards/P28-morphology-family.md) | 378 | guillemet | e6926ccf9ff8978ef5da64ab8f1b0df0505f1c819ba333ef3ccdece56bdbe3e2 |
| [P29-formula-memory.md](../../experimets/cards/P29-formula-memory.md) | 293 | guillemet | 153d1c5f246a1498da0c409e78840bcaf7b4b10e7ad72ec096ff9edbdf7581f8 |
| [P30-learned-fusion-alpha.md](../../experimets/cards/P30-learned-fusion-alpha.md) | 372 | guillemet | b7b8f9255514dce15d9a59171bea1cd8db2e4b393f97fd7ef9502aabc2366512 |
| [P31-source-trust.md](../../experimets/cards/P31-source-trust.md) | 387 | guillemet | 55e0aa76c9c585da9810185b1bbb5c3c4e3299ebe9039a9711e2ebcf8073dbb7 |
| [P32-skill-selection.md](../../experimets/cards/P32-skill-selection.md) | 311 | guillemet | 3c50633184b7a1eb1e1636bde1bed67c69df428a755a6f764255a723650ffa54 |
| [P33-domain-classifier.md](../../experimets/cards/P33-domain-classifier.md) | 469 | guillemet | 2b18764c8b53e20b1c6094bae921f3c9cdec69badfcffc97ebad31788b3e4d70 |
| [P34-router-labels.md](../../experimets/cards/P34-router-labels.md) | 353 | guillemet | 0e8bea4563450941ed14cf907bb587a65f5c586fb732f57803ff74276bead7ef |
| [P35-actual-neighbor-votes.md](../../experimets/cards/P35-actual-neighbor-votes.md) | 356 | guillemet | 8ff1283b477b965306c6ed2ca36a14b59938ddbdeaff93544926f212d5d7c78b |
| [P36-incremental-router.md](../../experimets/cards/P36-incremental-router.md) | 353 | guillemet | 3db54d55e1cb9b70e1c85d8d10b1a372d6adddebad92e00411f06c167e9d09ae |
| [P37-empty-router-query.md](../../experimets/cards/P37-empty-router-query.md) | 385 | guillemet | f5e50f2a2fc6f8c0873483603c7c6f5e9f7ac5f9953969a702da15006b7c903d |
| [P38-learned-segmentation.md](../../experimets/cards/P38-learned-segmentation.md) | 377 | guillemet | c8ca53f8e301c8f85674e5463350f259353a51bf579449a2ffcff574386615ab |
| [P39-oracle-output-separation.md](../../experimets/cards/P39-oracle-output-separation.md) | 356 | guillemet | 2145b0d7b52c63ecfeacc9269907b73b9ea6e13e94284440af3ca0936a2b8e5b |
| [P40-router-feedback.md](../../experimets/cards/P40-router-feedback.md) | 324 | guillemet | bd1ff5abe2336b63bcbbc1ccd29be483e8c31ea6e4712f1548cb470454d1e971 |
| [P41-stop-and-split.md](../../experimets/cards/P41-stop-and-split.md) | 306 | guillemet | b076dcb0ac4e0b3374da8322ec9cbf2c92c63abaf91b4fef5fccf992e1079379 |
| [P42-checker-arbiter.md](../../experimets/cards/P42-checker-arbiter.md) | 314 | guillemet | 09302653a9368eed75ae43ed3e2c1d1796032b9f7d2be4b3468ba1960a2cb862 |
| [P43-contextual-pos.md](../../experimets/cards/P43-contextual-pos.md) | 543 | guillemet | 4abb2b39b46922cc8bc83dad384ec430798fd73526f33deab5037131fa779ab1 |
| [P44-template-projection.md](../../experimets/cards/P44-template-projection.md) | 368 | guillemet | e2d53b202c96c687c241baed851d8c0d382955816964ebc69063348549c47217 |
| [P45-learned-reordering.md](../../experimets/cards/P45-learned-reordering.md) | 383 | guillemet | c95738a7c24ee9607f96bde8747c7c69a7e4efd630d21613fd9c0ca65f1863bd |
| [P46-pair-value.md](../../experimets/cards/P46-pair-value.md) | 362 | guillemet | fd64726a9b63f1e494b29e6ea322b3d0a789054ee1605fe282c6189991d607b6 |
| [P47-file-vector-context.md](../../experimets/cards/P47-file-vector-context.md) | 386 | guillemet | 80af44d324fa4ac859d96facf37682e014e852d560a8c7866a502d455903e0ed |
| [P48-weighting-curriculum.md](../../experimets/cards/P48-weighting-curriculum.md) | 352 | guillemet | 880df9284850ff644f41f286fe5acd6d01ca50a49d9dda9e087452462ff72432 |
| [P49-probe-warm-start.md](../../experimets/cards/P49-probe-warm-start.md) | 402 | guillemet | 69422a98b0428313ee05638b701b7d29ecb7ec754dfbf59fa995236f66b5da18 |
| [P50-graph-dependencies.md](../../experimets/cards/P50-graph-dependencies.md) | 357 | guillemet | f594452e50d167262fe339802320a80b2a1d766a7e64af093c574ec76dc70666 |
| [P51-translation-context.md](../../experimets/cards/P51-translation-context.md) | 504 | guillemet | 60dac65952e3e2f398dd75ac148c0617a60334e7cca6bca408dc6b60a9daf886 |
| [P52-semantic-annotation.md](../../experimets/cards/P52-semantic-annotation.md) | 587 | guillemet | 6e779405f4fa0e8faa5b209eb3b86dcec16ac4907e37719e177dfc34e2bddf44 |
| [P53-batch-atomicity.md](../../experimets/cards/P53-batch-atomicity.md) | 471 | guillemet | 3bbab46a178b0951fc796bbca4c73fe4058abed7c0b8357325ed325db5f6c594 |
| [P54-phrase-composition.md](../../experimets/cards/P54-phrase-composition.md) | 591 | guillemet | acb0e145745adb8f6dbc19eb3d5f3948d005d247d2466354e74569f44d189fef |
| [P55-translation-laws.md](../../experimets/cards/P55-translation-laws.md) | 396 | guillemet | afc03539f3b4214bf64dc8f83c1449eb20fa3e721dae563066450a72c8a613da |
| [P56-verifier-context.md](../../experimets/cards/P56-verifier-context.md) | 450 | guillemet | 4540db0ace3b7321b3134b9f8642acc94d8a73aca359e400c9f8bc197d8433ee |
| [P57-semantic-measurement.md](../../experimets/cards/P57-semantic-measurement.md) | 491 | guillemet | b2ca0b47fe86ce9e264eda48ba17e7814b205357f73117527ef086aad62daef1 |
| [P58-feedback-learning.md](../../experimets/cards/P58-feedback-learning.md) | 400 | guillemet | 8358a5d7c93c81487f1c9ebb6c17ac6c036d1afd49dd7dfc64f65a945864f609 |
| [P59-correction-visibility.md](../../experimets/cards/P59-correction-visibility.md) | 383 | guillemet | 559a1c6abac158a8526cce76f568d6e82de757989dba054623abc2093c301be3 |
| [P60-signature-duplicate.md](../../experimets/cards/P60-signature-duplicate.md) | 368 | guillemet | c5241f3580332e1c6e8575419bd689619b3942c225f49845c281455fd56de6c3 |
| [P61-unknown-tail.md](../../experimets/cards/P61-unknown-tail.md) | 373 | guillemet | 3521cc54839c5af07fc921b15cd813fd2ec7eee27475aecd56bf50c5a01945c3 |
| [P62-signature-monotonicity.md](../../experimets/cards/P62-signature-monotonicity.md) | 384 | guillemet | 5fa372425efbd755953bb70d2840c94a5f0c64095067c9ca569eaa44ca917983 |
| [P63-source-containment.md](../../experimets/cards/P63-source-containment.md) | 386 | guillemet | fbda2ddeb55611691b8a464a68bc9779bf06056148acd953ed6b2676c0144525 |
| [P64-tier-postfilter.md](../../experimets/cards/P64-tier-postfilter.md) | 384 | guillemet | c877c8b0187a366d2e6e17f7363c88506c63bb7e8c7ee4608bea6c91074b9510 |
| [P65-history-admission.md](../../experimets/cards/P65-history-admission.md) | 378 | guillemet | c976e3981e9cc5b77d1fb9411d17c82699b3a30c16e9cc18084cef6e3f70c63b |
| [P66-heldout-contrast.md](../../experimets/cards/P66-heldout-contrast.md) | 365 | guillemet | c48cca6e8e532eab1aeb0fd6eadf399165b3aeaa7372198c3e267e65cd028e72 |
| [P67-correction-identity.md](../../experimets/cards/P67-correction-identity.md) | 366 | guillemet | b53508b244d9539ffd0a8c05b575d73889e11ced0be7457572b53476e965a4cc |
| [P68-pretrain-pipeline.md](../../experimets/cards/P68-pretrain-pipeline.md) | 450 | guillemet | be560a1ebdfb13919cbdc7726ea7040357baac688acd76df6e7a94bcf94bf3e5 |
| [P69-graphrag-context.md](../../experimets/cards/P69-graphrag-context.md) | 481 | guillemet | 0b5d4870c0248ec6edeb41ddf549c78688ad18d49be0bc610d2f51f36ad32870 |
| [P70-wikitrace-context.md](../../experimets/cards/P70-wikitrace-context.md) | 290 | guillemet | 7b134d3fd6d53c7b3ac8751b15f2409b614ce907e979c8a74a9f42ae361d1333 |
| [P71-perplexity-alphabet.md](../../experimets/cards/P71-perplexity-alphabet.md) | 321 | guillemet | 94698ecc9ebf2ac040d70228104228a361d3e32fa198044fb53f04fb4d5d78f6 |
| [P72-project-conditioning.md](../../experimets/cards/P72-project-conditioning.md) | 335 | guillemet | 3f29911c837336b0b5c04d41f2275d8277a33f07881a66f9a0304ab26c6da392 |
| [P73-domain-conditioning.md](../../experimets/cards/P73-domain-conditioning.md) | 370 | guillemet | a1faf5ca0f83152b35fcfc685a1bae45bdf3061183420f46d0804605d77abab8 |
| [P74-memory-eviction.md](../../experimets/cards/P74-memory-eviction.md) | 507 | guillemet | 4c21bae1ef1b52af25b41c364fd31c6204c4bff284bc0c136371dd372e287caf |
| [P75-sampler-residual.md](../../experimets/cards/P75-sampler-residual.md) | 450 | guillemet | 056cd53948f17760c9b4ad2a8e1c2222b91ff85d288852cbc6efdd3716e89389 |
| [P76-utf8-boundary.md](../../experimets/cards/P76-utf8-boundary.md) | 458 | guillemet | 00fbd763e91b24c09e168b3584ef9b3c1b46207f4bafa239aa2097afdfe9d558 |
| [P77-rare-class-support.md](../../experimets/cards/P77-rare-class-support.md) | 491 | guillemet | 1868e4a49e6708cbfff4ffaa8caa8f8c92add6557f98a7163076eb98da3fb9cb |
| [P78-loss-plateau.md](../../experimets/cards/P78-loss-plateau.md) | 463 | guillemet | d466ff45eceebddb3cfcf3876f296ee71ccace3c79b1956cd256362ce4187542 |
| [P79-grammar-leakage.md](../../experimets/cards/P79-grammar-leakage.md) | 300 | guillemet | 0b98ddc98ab115f4eae17035d51fe3584c05d5cfa069b838119104294c2e36c3 |
| [P80-marker-denominator.md](../../experimets/cards/P80-marker-denominator.md) | 275 | guillemet | f5f327382259c21039b4b65a00a8eb9d23ee1266f7cbcb8841f36ea42d462081 |
| [P81-input-conditioning.md](../../experimets/cards/P81-input-conditioning.md) | 428 | guillemet | abe3d37c523128f4d30d0fa792f5f7f09f58c3d79cd7bc03acfd28f59cec0e38 |
| [P82-file-split.md](../../experimets/cards/P82-file-split.md) | 311 | guillemet | 4eac77064fe5b218823c37e0b55b293e692fcb346de07e887126ac2e66eeb3cf |
| [P83-expression-body.md](../../experimets/cards/P83-expression-body.md) | 359 | guillemet | 30ede022902e981e1dbe40dbb481042970fd15beb1b2af5b9a51948a333eac49 |
| [P84-lambda-family.md](../../experimets/cards/P84-lambda-family.md) | 348 | guillemet | 506089b4ca618899a7882f373e2c4a12e912717ff37f78325b357e969945bee4 |
| [P85-composite-obligations.md](../../experimets/cards/P85-composite-obligations.md) | 387 | guillemet | db750d4e641ee5dfd4c40325775b24f0614cbe1904e632003fd8e33ef14d07c0 |
| [P87-mixed-objectives.md](../../experimets/cards/P87-mixed-objectives.md) | 457 | guillemet | daccfdebca8a772d171e4617b56df4cfea5b7d8c0c6be4861aad697d6766b08b |
| [P88-unit-context-consumption.md](../../experimets/cards/P88-unit-context-consumption.md) | 311 | guillemet | 73acfc3a3a395c43eabe5c6c6bb9c4c268e1596b58a2e1be260d33b991d7fe6a |
| [P89-density-stop.md](../../experimets/cards/P89-density-stop.md) | 490 | guillemet | 9fb92a5bc874568ef1c542ea520e72e3ee978b482a52e5f7484e28c3e3c22ac7 |
| [P90-tagged-search.md](../../experimets/cards/P90-tagged-search.md) | 324 | guillemet | 9f6000b352bec43fb3109f8c9fdc07ca8f9b38d1d7ff7376b2e4dc98e9017168 |
| [P91-method-pair-split.md](../../experimets/cards/P91-method-pair-split.md) | 335 | guillemet | 629dd239a86f146a97b681087831c24c8edd1a836527a2abab9a3afedf8a41bb |
| [P92-dpo-memory-flow-bridge.md](../../experimets/cards/P92-dpo-memory-flow-bridge.md) | 487 | guillemet | 7d884da22f113ac529ec75e4e50120d3535ecbc4e09e5b8bb97e12b058ed22ff |
| [P93-confidence-abstention.md](../../experimets/cards/P93-confidence-abstention.md) | 543 | guillemet | 9fafd4c596c81e0c37d71ebeebba992a4ef5a3c65f9d7df4636b4f2fce80a77a |
| [P94-typed-many-results.md](../../experimets/cards/P94-typed-many-results.md) | 427 | guillemet | c5e69fcf860c4e20065d9cf58717f070e370c86d3ed7493273e88d78f3b0f86e |
| [P95-learned-flow-edge.md](../../experimets/cards/P95-learned-flow-edge.md) | 318 | guillemet | b163cd679554a2bb62315559fb2088e4eb866e90c8e7de3e997d8a16402417ca |
| [P96-async-orchestration.md](../../experimets/cards/P96-async-orchestration.md) | 342 | guillemet | a03aa9e1db2b85d381adb86f82189d22b8160ecdafdb306fbb32cdafb5362982 |
| [P97-learned-graph-order.md](../../experimets/cards/P97-learned-graph-order.md) | 342 | guillemet | 41cfb362d45298e46fa7d7f8cdd12a18a6140535a56ad476db19d88405cffd2c |
| [P98-tokenizer-capacity.md](../../experimets/cards/P98-tokenizer-capacity.md) | 470 | guillemet | 740e33ab6ad6eb6726bddcb6fd8ec038fde137b29f82d49fdde26ee988275e0c |
| [P99-unknown-token-id.md](../../experimets/cards/P99-unknown-token-id.md) | 483 | guillemet | 141bad11dc0d054b32a155084800637b33283f5b94e3574423d5b9b37e95717b |
| [ROPE-TRAIN-TABLE-01.md](../../experimets/cards/ROPE-TRAIN-TABLE-01.md) | 191 | plain | 310fc7904bd9f97e7f8f23045fc5570eeb6820da29fb011047d63aa6ebca9629 |
| [BPE-NO-MATCH-COPY-01.md](../../experimets/cards/BPE-NO-MATCH-COPY-01.md) | 233 | quote | c2383c3076e2d9b734d7586c4d9c8169188aa3de3ad8051ad8d4c2670f9b79b4 |
| [NEURAL-FROZEN-TRANSPOSE-01.md](../../experimets/cards/NEURAL-FROZEN-TRANSPOSE-01.md) | 239 | quote | 0b0938db60efec39be1b79d435a43e08e4e3a9b3bf892b41753840e2e3f84681 |
| [SILU-SIGMOID-REUSE-01.md](../../experimets/cards/SILU-SIGMOID-REUSE-01.md) | 366 | quote | ddfa5006d8b669f9440c93284d0ed88e4a86020a31585bc3d9a636bcfcc3deb1 |
| [WIKI-FEATURE-KEYS-01.md](../../experimets/cards/WIKI-FEATURE-KEYS-01.md) | 200 | quote | 5ffaa213be54b22c2620487cacd8559590c7e581733d75f3bc7288b1422caca6 |
| [WIKI-FEATURES-ONCE-01.md](../../experimets/cards/WIKI-FEATURES-ONCE-01.md) | 238 | quote | 2bb6040d08fe68d9bb10ea2e0c531d6ede1311638b5dda53a62b26479bcbf179 |
| [WIKI-MARGIN-01.md](../../experimets/cards/WIKI-MARGIN-01.md) | 234 | quote | 5ec953866fb9b05003ec0f5fcf56745956e54c7bbe3e1bf7529b6de5abcf4a6b |
| [WIKI-TAXONOMY-01.md](../../experimets/cards/WIKI-TAXONOMY-01.md) | 227 | quote | 2bb5815786d250814359033e11a7c2716d69e94073cc2a8696764028f522a8e6 |

### Остальные 33 main prompts, проверенные без новой вставки

92 inline-фрагмента перечислены по multiplicity в карте родителя; здесь приведены остальные фактические блоки. Проверка включает замкнутую границу fence/quote и наличие поручения в том же фрагменте.

| Файл | Входная строка | Носитель |
|---|---:|---|
| [N63-correction-collision.md](../../experimets/cards/N63-correction-collision.md) | 242 | quote |
| [03-test-and-execution-contract.md](../03-test-and-execution-contract.md) | 364 | fence |
| [114-post88-addressed-repair-plan-2026-11-01.md](../114-post88-addressed-repair-plan-2026-11-01.md) | 214 | quote |
| [118-post101-addressed-repair-plan-2026-11-02.md](../118-post101-addressed-repair-plan-2026-11-02.md) | 154 | quote |
| [12-bpe-no-match-copy.md](../12-bpe-no-match-copy.md) | 165 | quote |
| [13-dpo-reference-cache.md](../13-dpo-reference-cache.md) | 213 | quote |
| [16-replay-domain-retention.md](../16-replay-domain-retention.md) | 202 | quote |
| [18-contextual-dictionary-learning-coverage.md](../18-contextual-dictionary-learning-coverage.md) | 163 | quote |
| [27-predicate-dataset-router-learning-coverage.md](../27-predicate-dataset-router-learning-coverage.md) | 410 | quote |
| [28-dpo-tokenizer-snapshot.md](../28-dpo-tokenizer-snapshot.md) | 287 | quote |
| [29-matmul-right-gradient-locality.md](../29-matmul-right-gradient-locality.md) | 199 | fence |
| [30-broadcast-coordinate-allocation.md](../30-broadcast-coordinate-allocation.md) | 185 | fence |
| [31-bpe-weighted-chunks.md](../31-bpe-weighted-chunks.md) | 216 | fence |
| [34-pos-bigram-learning-coverage.md](../34-pos-bigram-learning-coverage.md) | 222 | quote |
| [36-dpo-scored-row-head.md](../36-dpo-scored-row-head.md) | 239 | fence |
| [37-structural-knn-learning-coverage.md](../37-structural-knn-learning-coverage.md) | 219 | quote |
| [38-structural-knn-sibling-cache.md](../38-structural-knn-sibling-cache.md) | 253 | fence |
| [39-null-guard-training-label.md](../39-null-guard-training-label.md) | 302 | fence |
| [40-bpe-realized-merge-objective.md](../40-bpe-realized-merge-objective.md) | 225 | fence |
| [41-grad-accum-copy.md](../41-grad-accum-copy.md) | 220 | fence |
| [42-frozen-broadcast-map.md](../42-frozen-broadcast-map.md) | 213 | fence |
| [44-rms-row-reduce.md](../44-rms-row-reduce.md) | 234 | fence |
| [45-attention-transpose-strides.md](../45-attention-transpose-strides.md) | 225 | quote |
| [46-training-json-options-reuse.md](../46-training-json-options-reuse.md) | 267 | quote |
| [47-silu-sigmoid-reuse.md](../47-silu-sigmoid-reuse.md) | 250 | quote |
| [48-learned-route-body-identity.md](../48-learned-route-body-identity.md) | 246 | fence |
| [49-router-learned-key-normalization.md](../49-router-learned-key-normalization.md) | 280 | quote |
| [51-pos-query-bigram-reuse.md](../51-pos-query-bigram-reuse.md) | 260 | fence |
| [52-translation-training-identity-index.md](../52-translation-training-identity-index.md) | 322 | quote |
| [53-hierarchical-local-history.md](../53-hierarchical-local-history.md) | 236 | fence |
| [62-supervised-framing-and-learning-readout.md](../62-supervised-framing-and-learning-readout.md) | 328 | quote |
| [83-learned-bpe-boundary-transfer-addendum.md](../83-learned-bpe-boundary-transfer-addendum.md) | 100 | plain83 |
| [codex-127-postmerge-addressed-repair-plan-2026-11-02.md](../codex-127-postmerge-addressed-repair-plan-2026-11-02.md) | 212 | quote |
