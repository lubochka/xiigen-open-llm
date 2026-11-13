# Независимый адресный review root-пакета

Дата: 20 октября 2026. Проверяющий: `/root/neural_audit`; автор проверяемых документов: `/root`. Проверен пакет `deliverables/2026-10-20-learning-delta-audit-883496`: документы02,03,05,10 и пять файлов `prompts/`. Это проверка чтением, без сборок, Train/Forward/Encode/Predict, продуктовых тестов или изменения output. Собственную BPE-карточку этот review не оценивает; её независимо проверяют root и experiment_review.

**Текущий вердикт после адресных исправлений: PASS для проверенной документационной передачи; runtime-ready не утверждается.** Исходный verdict был NEEDS_FIX по R-01/R-02 с двумя дополнительными правками R-03/R-04; история и закрытие сохранены ниже. Числа и причинные ограничения N62-R1 подтверждены; NG01 сохранён; пять адресных prompts содержат before/after, current-default executor model, русский отчёт и STOP/NEXT. Исправления относятся только к добавочному planning-пакету, не к protected.

## Идентичность просмотренных документов

SHA256 сняты с уже прочитанных Markdown, чтобы verdict не относился к будущему изменённому тексту. Это идентификаторы документационного review, не новые проверки результатов экспериментов.

| Файл | SHA256 |
|---|---|
| 02-learning-results-and-candidates.md | f19d6224f9bd90f3923c29b4735e9cdbabf377067607826dca3c3b9b5edfb918 |
| 03-N62-R1-measurement-erratum.md | c7083091ba80ee61674c8bfa6c940ad83bc5f3947dfe6bc9fda7b48bb8e86956 |
| 05-execution-registry.md | 144e093152c8303134ac988f4f75cf0d37473879d1c7e7667c3c27f3ce4534e5 |
| 10-protocol-and-skill-addenda.md | bce8b34f7f1181d46ca95579b70abf57dc2feccaeab6b23de645c012cdfe2dcf |
| prompts/E-DPO-SFT-ORDER-addendum.md | ebf61b104587ce80ae0b675790ffe8b4bac4c9c023a449eda308ddf2de13870d |
| prompts/E-DPO-VALID-LENGTH-addendum.md | 336bc771a3cc56c4d1052b9196a1e0219b1a94a1a145a66284af1904ebc9d7e6 |
| prompts/E-SFT-MOMENT-CONTINUITY-addendum.md | a9ff270014d8a1db152413399b7245d30a40281eafbf0760c5e1563af1c3f949 |
| prompts/E-SFT-RESPONSE-MASK-addendum.md | f13cde11b8a4878c393a51379ea909cba04ed79a960eff95e9d14f3a12c18ba5 |
| prompts/METHOD-KNN-TARGET-SCOPE-01-addendum.md | 5e87bc8289b4ffdc36f3aaf2862e1429b18277f48017683a9134f71d2b2bb69e |

## R-01. Не закрыт result-root конфликт для NEXT05 и других прежних назначенных рецептов

**Существенная незавершённая правка.** `10-protocol-and-skill-addenda.md:3,52,62` задаёт protected read-only и сообщает о переносе result-root, но перенос описан только в пяти prompts18/19/20/21/31. `05-execution-registry.md:13–15,47–49` одновременно передаёт N62-R2/E-OPT-STEP/LF-03 как готовые рецепты без такого добавочного mapping.

Непосредственный конфликт: [N62-neural-update-costs.md](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md) в строках9/31 требует candidate в `../../required-changes.md`, а таблица149–154 и дальнейшие команды назначают `docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R2-01/` и соседние protected roots. Историческая строка15 той карточки также разрешает изменение protected в другом поручении. При текущем запрете новый исполнитель не должен решать сам, какие пути менять и какой запрет обходить.

**Точная правка root-пакета:** задать единый явный write-location override для ещё не выполненных рецептов либо минимум законченные добавления05/06/07 перед объявлением их готовыми. Указать новые абсолютные roots, candidate.md внутри соответствующего root, и замену location allowlists/CLI/observer/checkpoint/output только в experimental copy. Чтение исходников, учебных материалов и сохранённых результатов из protected разрешено; их input-paths автоматически не переносить. Не отключать custody/identity/shape guards. Старые результаты01–04 не перемещать и не переписывать. Обновить правило передачи05 и ссылку из10 на этот override. Это документное устранение препятствия, оно не требует запуска.

Если выбран общий mapping, он должен однозначно разрешать именно output prefix и не требовать от исполнителя поиска каталога. Уже заданные пять roots и новые C:/xg пакеты можно сохранить явными исключениями. Простое «protected не менять» без replacement paths оставляет этот конфликт открытым.

## R-02. Обещанная таблица цены N-KU отсутствует, часть ухудшений остаётся только во внешнем отчёте

`02-learning-results-and-candidates.md:22` утверждает: «Подробная числовая таблица этих наблюдений включена ниже». В просмотренной версии после этой строки идут candidate C-01 и следующий опыт; такой таблицы нет.

В [первичном N-KU report, раздел «Цена основного сравнения»](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/report.md) уже есть нужные сохранённые числа. Их достаточно перенести без runtime и без запроса LFS payload:

| Наблюдение | A | B | Разница B−A |
|---|---:|---:|---:|
| Разметка+признаки+Train, мс | 195,8 | 123,8 | −72,0 |
| Тот же участок CPU, мс | 109,4 | 93,8 | −15,6 |
| Тот же участок allocated bytes | 1 179 032 | 1 169 344 | −9 688 |
| Четыре ответа после Train, мс | 22,4 | 26,1 | **+3,7** |
| Те же ответы CPU, мс | 15,6 | 31,3 | **+15,7** |
| Те же ответы allocated bytes | 45 168 | 45 152 | −16 |
| Process peak, млн байт | 45,8 | 45,4 | −0,4; округлённые данные отчёта |
| Выученная память, строки/payload bytes | 4/496 | 4/496 | 0/0; payload не весь managed heap |
| Весь процесс, мс | 3155 | 945 | −2210 |

**Точная правка:** добавить таблицу рядом с качеством N-KU и подписать её «сохранённые данные первичного отчёта; causal resource effect не установлен». Иначе убрать неверное «включена ниже» и прямо назвать readout latency/CPU regressions в соседнем абзаце. Предпочтителен первый вариант: пользователь просит показывать ухудшения рядом. Не превращать однократный timing или асимметричный product-equivalence контроль в причинный speedup. Не приписывать дополнительные Train/queries основному бюджету; существующее разделение в02 корректно.

## R-03. Unit/Flow ошибочно названы механизмами

`10-protocol-and-skill-addenda.md:19`: «LoRA, kNN, Unit и Flow — разные механизмы». [human-communication-GLOSSARY.md](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/skills/human-communication-GLOSSARY.md) явно различает механизмы и архитектурные сущности Unit/Flow.

**Точная замена:** «LoRA и kNN — механизмы; Unit и Flow — архитектурные сущности. Эти понятия не заменяют предметную тему обучения». Остальной смысл абзаца сохранить. Это терминологическая правка, не новый архитектурный фактор.

## R-04. Строка BPE32b расширяет предметную применимость за literal panel

`05-execution-registry.md:42` называет «Токенизация кода/документации и языковых материалов». В новой BPE-карточке фиксированы только lambda, comment и neutral C#-профили; EN→RU или другой языковой корпус не назначен. Кроме того, токенизация — механизм, не предметная тема.

**Точная замена колонки:** «Код/документация C#; lambda и комментарии». Непроверенные языковые материалы можно назвать только возможной дальнейшей применимостью вне текущего опыта, без расширения его бюджета и без новой claim об успехе.

## Что прошло проверку

| Проверка | Доказательство и вывод |
|---|---|
| N62-R1 числа | Независимо прочитаны оба локальных raw `A/arm-result.json` и `B/arm-result.json`, полученные с core883496. ApplyOptimizer sums1873,2654/378,6029; allocated3764969984/3764879480; GC188/12/9 и189/13/10; elapsed40977,3226/24298,8544; CPU32578,125/22140,625; wholeRun53,307086/31,494634; peak996343808/962547712; changed adapters16/base0 и final hash7676e8b1 совпадают с03. Вычитания в03 корректны. |
| Причинные границы N62-R1 | 03:27–40 явно отделяет наблюдение от causal speedup, timing всего ApplyOptimizer от отдельного сравнения arrays, два разных урока от before/after одной панели. Три ухудшения GC показаны. Удачный ресурсный кандидат сохранён без требования accuracy gain. |
| LFT/BPE сохранённые результаты | Прочитаны соответствующие primary reports; числа02 совпадают с опубликованными таблицами. Не заявлена новая независимая raw-реконструкция LFS payload. LFT sanity400 отделён от четырёхшагового A/B; неизвестный total двух попыток не выдуман. Старый Windows failure не заменяет завершённый BPE результат. |
| N-KU качество и предел | 0/4→1/4 A,0/4→3/4 B; false ADD_LOOP3→0, нужный совет1→0; сохранено объяснение majority collapse k3 из4. Основной Train/query budget отделён от дополнительных вызовов. |
| NG01/очередь | 05:5,39–43,49 сохраняет прежний NG01:31→32 NG01→32a SKNN→32b BPE→33 KN-ROUTER. Старый protected номер32 reader не теряется. NEXT05, завершённые01–04 не повторяются. |
| Пять новых prompts: path override | В каждом:9–11 заданы exact host и result-root вне protected, переназначены CLI/observer/checkpoint/output; report/candidate только новый root. Нет требования менять protected источник карточки. Нерешённая область — остальные карточки, см.R-01. |
| Before/after и бюджет | Prompts:23 сопоставлены с protected24-neural-learning-signal-protocol:27–30: MS/MC D-T1/D-T2 и8 forwards, PD PAD1/PAD2 full-length и8, ORDER D-T+S1/S2 и16. MKSC2 Train/16 Predict соответствует его native lifecycle:129–138. Панель фиксирована, её стоимость отделена от training, обновления не увеличены. |
| Wrapper и no replay | Prompts:19–21 сохраняют один внешний launcher, старые команды — внутренние шаги, новые hosts не объявлены реализованными. Существующие paths/markers не удаляются и attempt не выбирается автоматически. Для MS/MC/PD/OR прочитан referenced protocol:58–62, который задаёт ранний execution.claim и запрещает duplicate creation result-root. |
| Модель, язык, успех, STOP/NEXT | Все пять prompts:7,25,27,29,31 задают текущую default model, отделяют Student, русский отчёт, полезный успех по любой оси, все regressions, experimental exceptions и немедленный переход/stop без дописывания прошлого. Прежний NG01 — next для31, глобальный NEXT05 сохраняется. |

## Граница review

Применены ранее прочитанные адресные code-examination/plan-review/claim-verification/human-language правила и repo glossary. Проверялись конкретные claims и действия этих девяти документов, не работоспособность будущих CLI и не полный корпус153 карточек. Изменять output проверяющему не поручалось; замечания переданы root. Никакие новые обучения, метрики модели или продуктовые изменения для закрытия этих замечаний не нужны.

## Адресный повторный review исправлений root

Root внёс изменения после исходного review. Повторно прочитаны только исправленные участки02/05/10 и новый `15-output-path-contract.md`/`output-path-map.json`; прежние хэши выше относятся к исходной версии. R-02 закрыт: в02:24–36 появилась полная N-KU cost table с readout latency/CPU regressions и округлениями; native/guard query counts уточнены. R-03 закрыт в10:19: Unit/Flow названы архитектурными сущностями. R-04 закрыт в05:42: BPE profile ограничен C# lambda/comment.

R-01 закрыт в части назначения путей: новый15 содержит31 уникальный pending ID и31 уникальный result-root вне protected. Все31 назначения совпадают с машинной картой; report/candidate лежат под своим root; новые пять prompts совпадают с mapping; NEXT=N62-R2. NG01 сохранён,01–04 не перемещаются. Входные sources/B0/ancestor/raw явно не перенаправляются.

**Осталось одно уточнение lifecycle в15:7.** Формулировка «для всех записей» может перенести ранний `execution.claim` и source/data manifests в result-root. Referenced protected24-neural-learning-signal-protocol:58–60 намеренно хранит preparation manifests/blueprint и `execution.claim` в host-root; claim создаётся до build, а result-root создаёт единственная execution/prepare chain. Перенос claim внутрь result-root преждевременно создаст этот каталог и может снова вызвать self-block на проверке existing output.

Точная достаточная правка15: явно исключить **host-owned** source/data manifests, blueprint и ранний execution.claim из result-root mapping; сохранить их прежние host/source-root и create-new owner/lifecycle. Arm `training.claim` остаётся внутри нового arm-out по исходному протоколу. Result-root не создавать на подготовке документации или ради раннего claim; его единственный владелец — назначенная execution/prepare chain. Это не требует второго launcher или новых native calls. После этого адресного readback оставшихся blocking handoff-противоречий в проверенной области не выявлено; работоспособность hosts по-прежнему не аттестуется.

## Закрытие последнего замечания

Повторно прочитаны15:7–11: mapping теперь применяется только к данным, которым исходный рецепт уже назначал results-root; ранний execution.claim, blueprint/source manifests и другие host-owned артефакты прямо исключены. Сохранены один владелец execution/prepare и запрет создавать result-root во время подготовки. Последнее замечание lifecycle закрыто.

**PASS по проверенному документационному handoff:** R-01…R-04 устранены. Это не разрешение запуска, не проверка ABI/build, не результат обучения и не аттестация всего153-card корпуса. Ничего дополнительно запускать ради этого verdict не требуется.

| Исправленный файл | Идентичность принятой версии, SHA256 |
|---|---|
| 02-learning-results-and-candidates.md | 9bdadc088f805b70ba9e1bd35f158fa3ef53f4857205a24fbbcb50b413e2f311 |
| 05-execution-registry.md | 1a6ed3e607fc69f3a464a5570f486d0d4e7584673716ba83f41ce425904c2ba0 |
| 10-protocol-and-skill-addenda.md | 15e49c614832024541beef6fad888905194a37b5d01259f68c913d898480a247 |
| 15-output-path-contract.md | a5103f98c6cfbf2822950ede304022d3bad81107a2f9dd0197391be36feb9e77 |
| output-path-map.json | 451ae9e28bfb09c773ca19e3e44519b13e6e9a08988dc74e333fafbba757e315 |

Документ03 и пять prompts проверены в версиях исходной таблицы; дальнейшая семантическая правка этих файлов требует адресного readback, а не автоматического переноса текущего PASS. Сам review-файл записан в notes, проверяемый output этим агентом не менялся.
