# RAG-07 — external-sources (внешние легально-лицензированные источники обучения)

> Индекс «что где лежит», человекочитаемый, на русском. Часть сводки из 12 папок,
> которая готовит план ПРОДОЛЖЕНИЯ расширений Visual Studio и VS Code
> (офлайн-документирование кода локальной моделью + design-to-code).
> **Роль этой папки (#7/12):** это КОРПУС, на котором общая модель имеет право
> легально учиться — внешние открыто-лицензированные технические источники.
> Здесь живёт «легальная картина»: какие источники, под какими лицензиями,
> и вердикт ACCEPT/BLOCK для обучения общей модели.

- **Папка:** `external-sources`
- **Физический путь:** `C:/Users/lubao/Documents/xiigen/llm mvp/external-sources`
- **Всего файлов:** **98** (+ 1 пустая директория `domain-files/`)
- **Дата содержимого:** всё помечено `2026-05-04` (Phase 0 экзаменации внешних источников)
- **Проход:** 1/10 (MAP + манифест + разбор верхних документов). Проходы 2–9 = батчи + экзаменации + evidence раунда; проход 10 = сплошная сверка + executive summary.

> **[BRANCH-STATUS OVERLAY 2026-07-28]** (оверлей C3-R07, Цикл 3 документного обновления; добавка — ничего ниже не удалено и не переписано). Свежестная отметка: ветка внешних источников `external_domains` статична с 2026-05-11; её вершина `3036a01ac` переподтверждена 2026-07-28 командой `git ls-remote` (запрос серверу о вершине ветки, ничего не скачивая). Живых git-фактов этот индекс не содержит; его лицензионно-корпусное содержание остаётся актуальным. Новый контекст для лицензионной темы: волна GPL/AGPL 23–26 июля (ветка `PrepareToGPL` в ядре и коммиты «Prepare to AGPL»/«prepare to gpl» в соседних репозиториях) — см. авторитетную таблицу `BRANCH-STATUS-CURRENT-2026-07-28.md` (раздел A.2) в этой же папке.

### Кросс-ссылки (папки, уже разобранные в этой сводке — RAG-01..06 лежат в той же целевой папке)
- **RAG-05 (dpo legal build)** — там описан ЛЕГАЛЬНЫЙ teacher-gate (TeacherProvenance, 20 полей; семейства лицензий MIT/Apache-2.0; закрытые модели — только как судья, не как источник обучения). Реестр источников ИЗ ЭТОЙ папки — это ВХОД в тот gate. Именно RAG-05 решает, какие из здешних источников попадут в общую модель. Здешний вердикт `HOLD` (см. ниже) означает, что корпус ещё НЕ прошёл этот gate.
- **RAG-03 (language-translate)** — пилоту перевода нужен НАСТОЯЩИЙ лицензированный параллельный корпус (Tatoeba/UN); в папке перевода число «проверенно-лицензированных» было 0. Ответ на вопрос «а licensed-корпус живёт ЗДЕСЬ?» — **частично/нет**: Tatoeba присутствует лишь как 2 строки CC-BY-2.0 + запись «в плане» (Group 16). Готового параллельного корпуса здесь нет — он запланирован, не построен.
- **RAG-06 (llm dynamic flow)** — его метод экзаменации перерабатывал внешние архивы предложений (RFC/PEP/KIP). Эти источники живут ЗДЕСЬ (IETF-RFC, PYTHON-PEPS, RUST-RFCS, GO-PROPOSALS, KAFKA-KIPS) + отдельный файл экзаменации `02-source-examinations/proposal-debate-archives.md`. Восемь «dataset families» здесь — это ровно рёбра pipeline динамического потока.

---

## Структура / манифест

### Верхнеуровневые документы (6 файлов; нумерация с пропусками — 02 и 04 это НЕ файлы)

| Файл | Тип | Строк/размер | Что это |
|------|-----|--------------|---------|
| `00-CONTEXT-LOAD-REPORT.md` | md | 97 стр | Отчёт Phase 0: какие управляющие планы прочитаны, 8 dataset-семейств, baseline существующего внутреннего корпуса, **констатация критического разрыва по семьям 5 и 8** |
| `01-SOURCE-REGISTRY-ARCHITECTURE-CHALLENGES-v1.json` | json | 570 стр / ~21 КБ | **Реестр источников v1**: 32 записи источников с полями license/licenseUseClass/targetDomains/targetFamilies/extractionStatus. Ядро «легальной картины» |
| `03-OPEN-SOURCE-KNOWLEDGE-DOMAIN-MAP-v1.md` | md | 254 стр / ~12 КБ | Карта: каждый sourceId → 8 доменов XIIGEN (A..H) + матрица покрытия источник×домен |
| `05-ARCHITECTURE-ARBITER-CATALOG-v1.json` | json | 509 стр / ~25 КБ | Каталог арбитров (~36 арбитров): checks / positiveSignals / negativeSignals / связанные источники и trainingRows |
| `06-FINAL-DOCUMENT-STRUCTURE-v1.md` | md | 183 стр / ~12 КБ | Итоговая структура набора: **политика лицензий (ACCEPT/REJECT классы)**, mapping в домены, счётчики семейств, классы строк, сводка арбитров, разрывы, план batch-4 |
| `COVERAGE-LEDGER.jsonl` | jsonl | 1500 строк / ~630 КБ | **Реестр покрытия**: по одной JSON-строке на каждую произведённую строку данных (EXT-*). Поля: sourceId, registryId, license, trustTier, domainsTagged, familiesTagged, rowsProduced, status |

Отдельно: пустая директория `domain-files/` (0 файлов) — заготовка, содержимого нет.

### `02-source-examinations/` (4 файла) — экзаменация источников по тематическим группам

| Файл | Источники | Приоритет / семьи |
|------|-----------|-------------------|
| `security-governance.md` (115 стр) | OWASP-ASVS, NIST-SSDF, CWE-TOP25, IETF-RFC-OAUTH2/JWT/TLS13 | Приоритет 1 (семьи 5 и 8 — наибольший разрыв) |
| `architecture-orchestration.md` (116 стр) | KUBERNETES-KEPS, OPENTELEMETRY-SPEC, OPENAPI-SPEC, GOOGLE-AIP, MICROSOFT-LEARN-ARCH | Приоритет 2 (семьи 1, 2, 5, 8) |
| `architecture-distributed-data.md` (98 стр) | MICROSOFT-LEARN-ARCH (CQRS, Event Sourcing, Saga, Outbox, Queue-based Load Leveling, Compensating Transaction) | Приоритет 2 (семьи 1, 2, 5, 8) |
| `proposal-debate-archives.md` (93 стр) | RUST-RFCS, PYTHON-PEPS, GO-PROPOSALS, KAFKA-KIPS, CLOUDEVENTS-SPEC | Приоритет 4 (семьи 1, 5, 8 — богатейший источник строк «reprocessing») |

### `batches/` (75 файлов) — произведённые строки данных (кандидатный материал)

- **75 md-файлов**, имена по шаблону `ext-batch-<NNN>-<NNN>.md`, диапазоны по 30 (тайлинг от `001-030` до `2221-2250` = ровно 75×30 = 2250).
- **ВАЖНО (дрейф нумерации):** имена диапазонов 30-широкие, но реальных строк в каждом файле — 20. Итог: 75 × 20 = **1500 строк данных** (EXT-*), а не 2250. Этот разрыв «имя vs факт» и есть причина расхождений идентификаторов, отмеченных аудитом (400→160 несоответствий после нормализации суффиксов).
- Формат строки: заголовок + поля Source/License/TrustTier/Domain/Family + fenced-JSON блок `Draft row` в форме `a→f(b)+c` (a=триггер, f=функция/рассуждение, b=контент, c=контекст, acceptedOutput/rejectedOutput). Пример из `ext-batch-001-030.md`: EXT-SEC-001 — JWT `alg:none` → REJECT (арбитр `jwtValidationArbiter`).
- Каждый батч охватывает 6+ источников; ранние батчи — security/governance, дальше — архитектура, proposal-архивы, языковые/фреймворковые группы 8–16.

Разбивка `batches/` по паттерну диапазонов (все 75 учтены):
- `001-030` … `091-120` (4 файла) — начальные security/arch батчи (EXT-SEC-*, EXT-031..090).
- `121-150` … `991-1020` (30 файлов) — сплошной ряд 30-широких блоков.
- `1021-1050` … `1471-1500` (16 файлов).
- `1501-1530` … `1981-2010` (17 файлов).
- `2011-2040` … `2221-2250` (8 файлов).
- (сумма = 75 файлов; ни один не «шум» — каждый содержит ~20 rowized-строк с provenance и лицензией).

### `round-zips/` (13 файлов) — 1 раунд аудита

**`external-learning-round-001-batch-audit.zip`** + распакованная папка того же имени:
- `00-ROUND-MANIFEST.md` — назначение + **решение `HOLD`** (батчи ценны как кандидаты, но не готовы к импорту).
- `01-FILES-INCLUDED.md` — список файлов пакета.
- `02-VALIDATION-RESULTS.md` — счётчики валидации (75 файлов, 1500 строк, 106 JSON-ошибок, license-gate).
- `03-NEXT-ROUND-PROMPT.md` — промпт следующего раунда (нормализация схемы, починка записей, license-gate, финальные node A→B пары с перефразировкой).
- `evidence/` (7 файлов): `batch-audit-summary.json`, `batch-file-summary.csv` (75 строк, по файлу), `license-distribution.csv`, `domain-distribution.csv`, `dataset-family-distribution.csv`, `status-distribution.csv`, `invalid-json-examples.txt` (примеры 106 ошибок парсинга).
- `files/NO-SOURCE-FILES-CHANGED.txt` — маркер: раунд ничего в исходниках не менял (аудит-только).

---

## Подробный разбор (ключевые документы)

### A. `01-SOURCE-REGISTRY-ARCHITECTURE-CHALLENGES-v1.json` — реестр источников v1 и ЛЕГАЛЬНЫЙ вердикт

**Что это:** машинный реестр из **32 источников**. Каждая запись: sourceId, publisher, license, `licenseUseClass`, targetDomains, targetFamilies, allowedUse, `rawTextIngestionAllowed`, `extractionStatus`, priority.

**ACCEPTED — разрешены для обучения общей модели (27 источников):**

| licenseUseClass | Источники | Лицензия |
|-----------------|-----------|----------|
| TRAINING_OK (свободно) | KUBERNETES-KEPS, OPENTELEMETRY-SPEC, OPENAPI-SPEC, KAFKA-KIPS, CLOUDEVENTS-SPEC, DOCKER-BEST-PRACTICES | Apache-2.0 |
| TRAINING_OK | SWE-BENCH, RUST-RFCS, NESTJS-ADVANCED | MIT |
| TRAINING_OK | PYTHON-PEPS | Public domain |
| TRAINING_OK | GO-PROPOSALS | BSD-3-Clause |
| ATTRIBUTION_REQUIRED | GOOGLE-AIP, MICROSOFT-LEARN-ARCH, DOTNET-MICROSERVICES, SYSTEM-DESIGN-PRIMER, REACT-ADVANCED, GITHUB-ACTIONS-ADVANCED | CC-BY-4.0 |
| ATTRIBUTION_REQUIRED | AOSA-VOL1, AOSA-VOL2, EFFECTIVE-GO | CC-BY-3.0 |
| ATTRIBUTION_SHAREALIKE | OWASP-ASVS, OWASP-CHEAT-SHEETS | CC-BY-SA-4.0 (производные — под совместимой лицензией) |
| FREE_COMMERCIAL_USE_WITH_NOTICE | CWE-TOP25 | MITRE CWE Terms (нужен notice) |
| IETF_TRUST_PROVISIONS | IETF-RFC-OAUTH2, IETF-RFC-JWT, IETF-RFC-TLS13 | IETF Trust (текст — ок; код требует per-RFC проверки) |
| PUBLIC_DOMAIN_US | NIST-SSDF | Not subject to U.S. copyright |

**BLOCKED / VERIFY_BEFORE_USE — пока НЕ пригодны (5 источников):**
`DEFECTS4J`, `BUGSINPY`, `QUIXBUGS`, `SEMGREP-RULES`, `CODEQL-EXAMPLES` — у всех `rawTextIngestionAllowed:false`, `derivedRowAllowed:false`, задача — прочитать LICENSE-файл репозитория перед любым использованием. До этого они «заморожены».

**Relevance / связь:** это ровно тот вход, что RAG-05 (legal teacher-gate) должен профильтровать. Семейства лицензий MIT/Apache-2.0 (предпочтительные в RAG-05) здесь составляют ядро TRAINING_OK; CC-BY-SA (share-alike) и IETF/MITRE — «условно, с атрибуцией/notice»; VERIFY_BEFORE_USE — «блок до подтверждения».
**Теги:** `#legal` `#source-registry` `#license-classes` `#feeds-RAG05-gate`

### B. `03-OPEN-SOURCE-KNOWLEDGE-DOMAIN-MAP-v1.md` — карта источник → 8 доменов XIIGEN

Каждый sourceId привязан к доменам **A–H**: A-Топология/оркестрация, B-Маршрутизация/выбор, C-Семантический перевод, D-Исполнение/applier, E-Арбитр/качество, F-Реестр/хранилище, G-Обучение/тренировка, H-Управление/контракты. В конце — матрица покрытия источник×домен (gate: каждый источник реестра попадает минимум в один домен — PASSED).
**Ключ:** домен **E (Арбитр/качество)** — самый плотный: сюда стекаются все security-арбитры (jwt/oauth/password/sqlInjection/tenantIsolation/tls…). Это отвечает на «критический разрыв» семьи 5.
**Теги:** `#domain-map` `#coverage-matrix` `#arbiters`

### C. `00-CONTEXT-LOAD-REPORT.md` — Phase 0 и постановка разрыва

Восемь **dataset-семейств** = рёбра pipeline динамического потока (связь с RAG-06):
1 `userPrompt_to_machineCommands` → 2 `machineCommand_to_machineActionItems` → 3 `machineActionItem_to_domainResult` → 4 `domainResultsArray_to_resultI` → 5 `arbitrateResultI_with_resultsContext` → 6 `resultI_to_finalMachineResults` → 7 `finalMachineResults_to_userFormat` → 8 `historyAndVersions_to_reprocessingDecision`.
**Констатация разрыва:** внутренний корпус даёт лишь ~8 строк в семье 5 (арбитраж) и ~6 в семье 8 (reprocessing/rollback) — **это критический дефицит**, ради которого и собираются внешние источники. Приоритеты источников 1–6 (P1 security/governance → P6 stack-specific).
**Теги:** `#gap-statement` `#dataset-families` `#families-5-8-critical`

### D. `round-zips/.../evidence/license-distribution.csv` + `batch-audit-summary.json` — легальная картина по факту 1500 строк

**Распределение лицензий по 1500 произведённым строкам (licenseConstraint):**

| Кол-во | Лицензия | Класс |
|--------|----------|-------|
| 353 | MIT | ACCEPT (TRAINING_OK) |
| 316 | attribution (общий тег) | ACCEPT с атрибуцией |
| 284 | Apache-2.0 | ACCEPT (TRAINING_OK) |
| 171 | CC-BY-4.0 | ACCEPT с атрибуцией |
| 104 | notice | ACCEPT с notice |
| 40 | PSF-2.0 (Python) | ACCEPT |
| 40 | IETF-TLP | ACCEPT (условно) |
| 33 | CC-BY-SA-3.0 | ACCEPT share-alike |
| 33 | CC-BY-2.0-FR | ACCEPT с атрибуцией |
| 25 | MITRE notice required | ACCEPT с notice |
| 14 / 11 / 7 / 1 | CWE-TLP / CWE-TOU / CWE-permissive / MITRE ToU | ACCEPT (MITRE условия) |
| 14 (+1) | CC-BY-SA-4.0 (в т.ч. OWASP+IETF) | ACCEPT share-alike |
| 9 / 6 | CC-BY-2.0 / CC-BY-3.0 | ACCEPT |
| 2 | **CC-BY-2.0 — Tatoeba (tatoeba.org)** | ACCEPT (см. связь с RAG-03) |
| 2 | CC-0 / MIT-0 | ACCEPT (public) |
| 3 | BSD-3-Clause | ACCEPT |
| 1 | MPL-2.0 | ACCEPT (условно) |
| ~12 | none / public domain / mixed | требуют решения политики |
| **2** | **GPL-3.0** | **BLOCKED — должны быть удалены/заменены до промоции** |

**Сводка аудита (`batch-audit-summary.json`) — почему `HOLD`:**
- Файлов 75; строк 1500 (уникальных 1500, дублей 0); ledger-строк 1500.
- JSON-блоков 1324; распарсено 1218; **ошибок парсинга 106** (в 7 файлах); 9 файлов вообще без JSON-блоков.
- **PairFormatMatches: 0, RephrasePairsFound: 0** — финального формата node A→B пар и обязательных перефразировок НЕТ.
- License-gate: чисто по простому совпадению 954; **требуют решения политики 542**; **заблокировано по строке GPL: 2**.
- Дрейф идентификаторов: 400 несоответствий batch↔ledger (после нормализации суффиксов — всё ещё 160).
- **Decision: «HOLD: candidate material only. Not ready for local learning-set import.»**

**Статусы строк (`status-distribution.csv`):** draft 661, approved 380, candidate 280, ready 180. → Большинство ещё draft/candidate; «ready» лишь 180/1500 (12%).
**Домены (`domain-distribution.csv`):** плотнее всего E (арбитр/качество) и D (исполнение) — согласуется с приоритетом семьи 5.
**Семьи (`dataset-family-distribution.csv`):** топ — `arbitrateResultI_with_resultsContext` (семья 5, 288) и `machineActionItem_to_domainResult` (семья 3, 183); семья 4 — всего 2 (по-прежнему разрыв).
**Теги:** `#license-picture` `#HOLD-decision` `#GPL-blocked` `#not-import-ready`

### E. `05-ARCHITECTURE-ARBITER-CATALOG-v1.json` + §6 из `06-...md` — каталог арбитров

~36 арбитров, каждый со списком checks / positiveSignals / negativeSignals и привязкой к источникам + trainingRows. Security-арбитры (jwtValidation, oauthGrant, passwordStorage, sqlInjection, secretPresence, tenantIsolation, tlsConfig, supplyChain) — из OWASP/CWE/IETF/NIST; архитектурные (circuitBreaker, bulkhead, strangler, cqrsFit, transactionalOutbox, protocolMigration) — из MS-Learn/Kafka/.NET. Это «качество/арбитраж» — прямое наполнение семьи 5.
**Теги:** `#arbiter-catalog` `#family-5`

---

## Итоговая легальная картина (для плана продолжения расширений)

- **Сколько источников:** реестр v1 = **32** (27 ACCEPT / 5 VERIFY_BEFORE_USE-BLOCK). Реально в 1500 строк вовлечены группы 1–16 (шире реестра: + MERMAID, DIATAXIS, GHERKIN, LANGGRAPH, CONVENTIONAL-COMMITS, MCP-DOCS, GOVUK-DESIGN-SYSTEM, EFCORE/DAPPER, ANGULARJS, ROBOT-FRAMEWORK, SCRUM-GUIDE, OPENAI-COOKBOOK, PROMPTFOO, NODEJS-DOCS, TATOEBA, COMMON-VOICE и др.).
- **Что ACCEPT:** MIT/Apache-2.0/BSD/Public-domain (свободно); CC-BY-3.0/4.0 (атрибуция); CC-BY-SA (share-alike); MITRE-CWE (notice); IETF-RFC (текст); NIST (public domain US).
- **Что BLOCKED:** GPL-3.0 (2 строки) — удалить/заменить; DEFECTS4J/BugsInPy/QuixBugs/Semgrep/CodeQL — до проверки LICENSE; классы CC-BY-NC-ND (Google SRE Book, Cosmic Python) и CITE_ONLY_NO_COPY (Martin Fowler, AWS Builders) — исключены политикой (§2 в `06`).
- **Готовность:** **НЕ готово к импорту** (`HOLD`). Нужен следующий раунд: нормализация схемы, починка 106 JSON-ошибок, строгий license-gate (542 под вопросом + 2 GPL), финальные node A→B пары с перефразировкой.

### Связь с задачей расширений
- **С RAG-05:** здешний реестр + license-distribution — это сырьё для legal teacher-gate; вердикт `HOLD` = gate ещё не пройден, корпус промежуточный.
- **С RAG-03:** лицензированного параллельного корпуса перевода здесь по сути НЕТ — только 2 строки Tatoeba (CC-BY-2.0) + план (Group 16 Tatoeba/Common Voice). Это подтверждает разрыв RAG-03 (verified-licensed = 0).
- **С RAG-06:** архивы предложений (RFC/PEP/KIP/RFCs) и метод их экзаменации присутствуют (`proposal-debate-archives.md` + IETF/Python/Rust/Go/Kafka источники); 8 семейств = рёбра динамического потока.

---

## Рамка сводки (проходы 2–10)
- **Проход 1/10 (этот):** карта дерева (98 файлов) + манифест + разбор верхних документов + легальная картина.
- **Проходы 2–9:** глубокий разбор `batches/` (75 файлов, 1500 строк — по группам источников/семьям), 4 файлов `02-source-examinations/`, и всех evidence-файлов раунда.
- **Проход 10:** сплошная сверка полноты (каждый файл учтён) + executive summary.

### Честная строка покрытия (проход 1/10)
Прочитаны и процитированы: `00`, `01` (полностью), `03` (полностью), `06` (полностью), `05` (шапка), `COVERAGE-LEDGER.jsonl` (образец 1 строки), все 6 evidence-CSV/JSON раунда, `00-ROUND-MANIFEST.md`, `02-VALIDATION-RESULTS.md`, `NO-SOURCE-FILES-CHANGED.txt`, образец `ext-batch-001-030.md` (80 стр), шапки 4 экзаменаций. НЕ прочитаны построчно: полное тело 75 батчей и полные 1500 строк ledger — это работа проходов 2–9. Все 98 файлов категоризированы; ни один не отброшен как шум.

---

## Pass 2/10 — тела батчей (1500 строк источников: схема, содержимое, лицензии, статус)

> Проход 2 читает уже НЕ манифест, а сами строки данных. Прочитано полностью 5 батч-файлов из разных концов диапазона + evidence, чтобы учесть все 75: `ext-batch-001-030.md` (начало), `ext-batch-451-480.md`, `ext-batch-1231-1260.md`, `ext-batch-2221-2250.md` (конец), плюс `batch-file-summary.csv` (75 строк — по одной на файл), `invalid-json-examples.txt` (примеры 106 ошибок), `batch-audit-summary.json`, все 4 distribution-CSV, `03-NEXT-ROUND-PROMPT.md`, `00-ROUND-MANIFEST.md`, `02-VALIDATION-RESULTS.md`. Вывод прохода: **это НАСТОЯЩИЕ, содержательные обучающие пары (не пустые заглушки), но в НЕ-каноническом, дрейфующем виде — поэтому вердикт `HOLD` корпуса подтверждается на уровне строк.**

### 1. Схема строки (ROW SCHEMA) — какие поля несёт каждый источник-row

Каждая строка = fenced-JSON блок ```` ```json ... ``` ```` под markdown-заголовком. Каноническая форма — **DPO-подобная пара «принято/отвергнуто»** в модели `a→f(b)+c`:

| Поле | Что несёт | Пример значения |
|------|-----------|-----------------|
| `rowId` | идентификатор строки (форматы дрейфуют, см. §5) | `EXT-SEC-001`, `EXT-831`, `EXT-1491-A1`, `EXT-431-R1`, `ROW-EXT571-F5-E` |
| `sourceIds` | из какого источника(ов) взято | `["OWASP-ASVS","IETF-RFC-JWT"]`, `["KUBERNETES-KEPS"]` |
| `sourceTrustTier` | доверие к источнику (тип дрейфует: `1`/`2` → `"T1"`/`"T2"`) | `1`, `"T1"` |
| `datasetFamily` | семья pipeline (1–8, см. RAG-01 §C); иногда просто число | `arbitrateResultI_with_resultsContext`, `"5"` |
| `domain` | домен XIIGEN A–H (форма дрейфует: полное имя → одна буква) | `E_ARBITER_QUALITY` → `E` |
| `subdomain` | под-тема | `jwt-validation`, `A3-topology-disruption-budget` |
| `rowClass` | тип пары (появился позже, в ранних батчах его нет) | `tradeoff-decision`, `anti-pattern-detection`, `negative-example`, `migration-decision`, `arbiter-construction` |
| `architectureChallenge` | (ранние батчи) одно-фразовая постановка задачи | «A JWT with algorithm 'none' arrives…» |
| `input.a` | **триггер** — что решаем | «Validate the incoming JWT…» |
| `input.b` | **контент** — оцениваемый код/конфиг/дизайн | `{"alg":"none",...}`, реальный SQL/YAML/TS |
| `input.c` | **контекст** — ограничения, состояние, условия отказа | «Service is auth-required; validator supports 'none'…» |
| `function` | какое рассуждение/функция применяется (=`f`) | «Apply jwtValidationArbiter: check allowlist first» |
| `acceptedOutput` | **правильный** вердикт/дизайн (chosen); тип дрейфует: object ↔ строка | `{decision:"REJECT…"}` или длинная markdown-строка с кодом |
| `rejectedOutput` | **неправильный** ответ/анти-паттерн (rejected) | `{decision:"ACCEPT…"}` |
| `arbiters` | какие арбитры судят (связь с каталогом `05`) | `["jwtValidationArbiter"]`, `["kubernetes-multi-tenancy-board"]` |
| `whyAccepted` / `whyRejected` | обоснование выбора (это и есть «Reason») | ссылки на ASVS/RFC/CWE |
| `sourceAnchors` | якорь в первоисточнике (тип дрейфует: `[{sourceId,anchor}]` → массив URL/строк) | «V3.5.1…», `"https://kubernetes.io/…"` |
| `licenseConstraint` | лицензия строки (см. §3) | `CC-BY-SA-4.0`, `MIT`, `attribution`, `Apache-2.0` |
| `trainingStatus` | статус строки (см. §3; регистр дрейфует) | `candidate` → `draft` → `APPROVED`/`approved`/`ready` |

**Ключевое:** формат `acceptedOutput`/`rejectedOutput` — это **DPO chosen/rejected**, а НЕ финальная node-пара `A→B` с перефразировкой (`Phrasing-B`), которую требует план обучения. То есть строки — «сырьё для DPO», но не в целевой упаковке (см. §4, `PairFormatMatches: 0`).

### 2. Что строки реально УЧАТ (содержимое) + качество (реальные пары vs заглушки)

**Это реальный, плотный технический контент, а не scaffold.** Каждая пара учит одному конкретному инженерному выбору «правильно vs правдоподобно-но-неверно» с обоснованием из первоисточника. Покрытие по доменам (все 8 семей потока):

- **Security / арбитраж (домен E, семья 5 — самая плотная, 288 строк):** JWT `alg:none` → REJECT; OAuth implicit → PKCE; MD5 → bcrypt/Argon2id; SQL-инъекция → параметризованный запрос; секрет в git → ротация+env+scanner; кросс-тенант IDOR/BOLA (CWE-862) → WHERE tenant_id; TLS 1.0/RC4 → TLS 1.3; пароль-complexity → длина+HIBP; 403-vs-401 для истёкшего токена (RFC-6750). Источники: OWASP-ASVS, CWE-TOP25, NIST-SSDF, IETF-RFC.
- **Архитектура/топология (домен A, семьи 1–2):** Circuit Breaker vs retry-storm; Bulkhead vs rate-limit; Strangler Fig vs big-bang; PodDisruptionBudget `minAvailable` vs `maxUnavailable`; wildcard Ingress + operator; .NET Aspire service discovery; KEDA ScaledJob vs ScaledObject. Источники: MICROSOFT-LEARN-ARCH, KUBERNETES-KEPS, DOTNET-MICROSERVICES.
- **Исполнение/данные (домены D, F, семьи 3, 6):** async ORM (SQLAlchemy AsyncSession, EF Core compiled query, SqlBulkCopy, Dapper SplitOn/QueryMultiple); генератор vs `readlines()` при OOM; `Counter` vs O(n²); transactional outbox; cursor vs offset пагинация (AIP-158); Redis vs CDN для персонализированного кэша.
- **Фреймворки/перевод/формат (домены C, B, семья 7):** Angular `computed()` vs `effect()`, reactive forms, `inject()`; React RSC vs useEffect, `React.lazy`, DOMPurify vs regex XSS; Vue Teleport, навигационные guard'ы; NestJS forRoot/forFeature, CQRS CommandBus; ETag/If-None-Match объяснение фронтендеру.
- **Обучение/reprocessing (домен G, семья 8):** near-duplicate MinHash-фильтрация RLHF; domain-shift Tatoeba EN-PT (legal); gradient-norm explosion → clipping; DLQ vs silent-complete.

**Оценка качества:** пары СОДЕРЖАТЕЛЬНЫЕ и в основном технически верные — с конкретным кодом, числами (RPS, latency, размеры), ссылками на стандарты. Это не «placeholder». НО: (а) это generic best-practice знание, а не XIIGEN-специфичное; (б) `rejectedOutput` иногда слишком очевидно плох (слабый negative для DPO-маржи); (в) `-R1`-строки (`ext-batch-631/661-*`) цитируют другие EXT-строки как источник (`sourceIds:["EXT-431"]`) — это reprocessing-ревизии, а не свежий первоисточник.

### 3. Разбивка по статусу и лицензии (по факту 1500 строк — из `batch-audit-summary.json`)

**Статус (`trainingStatus`):** draft **661** / approved **380** / candidate **280** / ready **180**. → «ready» лишь **180/1500 = 12%**. Регистр/значения дрейфуют (`candidate`→`draft`→`APPROVED`→`approved`→`ready`), поэтому статус НЕ является надёжным сигналом готовности сам по себе.

**Лицензии (топ, ACCEPT-класс):** MIT 353, `attribution` (общий тег) 316, Apache-2.0 284, CC-BY-4.0 171, `notice` 104, PSF-2.0 40, IETF-TLP 40, CC-BY-SA-3.0 33, CC-BY-2.0-FR 33, MITRE-notice 25, CWE-* (TLP/TOU/permissive) 32, CC-BY-SA-4.0 14, Tatoeba CC-BY-2.0 2, CC-0/MIT-0 2, BSD-3 3, MPL-2.0 1. **BLOCKED: GPL-3.0 = 2** (должны быть удалены/заменены до промоции). Проблема качества данных: **общие теги `attribution` (316) и `notice` (104) НЕ называют конкретную лицензию** → именно они формируют «542 требуют решения политики».

### 4. Что делает корпус НЕ-готовым (`HOLD`) — на уровне строк

1. **`PairFormatMatches: 0` и `RephrasePairsFound: 0`** — ни одна строка НЕ в финальном формате node-пар `A→B` с обязательной перефразировкой `Phrasing-B` (требование плана обучения). Есть DPO `accepted/rejected`, но не целевая упаковка.
2. **106 JSON-ошибок парсинга** во **7 файлах** (из 1324 fenced-блоков распарсено 1218). Все ошибки — тип **«Unterminated string»**: `acceptedOutput` содержит многострочные code/markdown-фрагменты (`diagram`, `howToGuide`, `\n`-код) с сырыми переводами строки внутри JSON-строки. Плюс **9 файлов вообще без JSON-блоков** (markdown-поля вместо JSON — напр. заголовки «Batch 3/4»). Худшие: `ext-batch-631-660.md` и `ext-batch-661-690.md` (масса `-R1`-строк с большими кодовыми блоками).
3. **Mojibake (порча кодировки UTF-8→CP1252)** пронизывает содержимое строк: `â†’` вместо `→`, `â€"` вместо тире, `â‰¥` вместо `≥`, `Ã¡` вместо `á`, `DiÃ¡taxis`, `â€™` вместо кавычки. Это не только косметика — это часть контента обучающих пар и должно чиниться до промоции.
4. **542 лицензии «требуют решения политики»** (общие теги `attribution`/`notice`/`none`/`mixed` без конкретной лицензии) + **2 GPL** заблокированы.
5. **Дрейф идентификаторов** (см. §5): 400 несоответствий batch↔ledger, 160 после нормализации суффиксов.

### 5. Дрейф идентификаторов и схемы — конкретные доказательства (из `batch-file-summary.csv`)

**(а) Имя файла ≠ содержимое.** Тайл имени 30-широкий, но реальных строк 20, и EXT-номера внутри — из совсем другого диапазона:
- файл `ext-batch-451-480.md` → шапка «EXT-311 through EXT-330», строки `EXT-311..EXT-330`;
- файл `ext-batch-1231-1260.md` → «EXT-831..EXT-850»;
- файл `ext-batch-2221-2250.md` → «EXT-1491..EXT-1510»;
- файл `ext-batch-1021-1050.md` → «EXT-691..EXT-710».
Столбец `ExpectedFromFileName=30` против `TotalRowIds=20` в КАЖДОЙ из 75 строк CSV — это и есть «имя vs факт», порождающее 400→160 несоответствий.

**(б) Минимум 6 форматов `rowId`:** `EXT-SEC-001` (ранний, домен-префикс) · `EXT-031` (числовой) · `EXT-691-A1` (суффикс `-A1`) · `EXT-431-R1` (reprocessing-суффикс, `sourceIds` цитирует родительскую EXT-строку) · `ROW-EXT571-F5-E` (ROW-префикс + семья+домен) · numeric+`rowClass`.

**(в) Эволюция схемы по «поколениям» генерации:** `sourceTrustTier` `1`→`"T1"`; `domain` `E_ARBITER_QUALITY`→`E`; `trainingStatus` `candidate`→`draft`→`APPROVED`→`approved`; `acceptedOutput` object→строка→object; `sourceAnchors` `[{sourceId,anchor}]`→массив URL; `licenseConstraint` конкретная (`CC-BY-SA-4.0`)→общий тег (`attribution`); `rowClass` отсутствует в ранних, добавлен позже. Это объясняет и разнобой в `domain-distribution.csv` (одновременно `E`=241 и `E_ARBITER_QUALITY`=83 и `E (Arbiter Quality)`=34 — один домен в трёх написаниях).

### 3–4 репрезентативные цитаты строк

- **EXT-SEC-001** (`ext-batch-001-030.md`, OWASP-ASVS+IETF-RFC-JWT, семья 5, домен `E_ARBITER_QUALITY`, `candidate`): вход `b: {"alg":"none","typ":"JWT"}`, signature пуст; `acceptedOutput.decision:"REJECT. 'none' не в allowlist… 401… allowlist до проверки подписи"`; `rejectedOutput:"ACCEPT. claims валидны, 'none' = без подписи, продолжить"`; `whyAccepted` цитирует «ASVS V3.5.1» и «RFC 7519 §6 Unsecured JWTs». → богатый ранний формат.
- **EXT-831** (`ext-batch-1231-1260.md`, KUBERNETES-KEPS, семья 4, домен `A`, `draft`, `rowClass:tradeoff-decision`): PDB `minAvailable:3` (accepted) vs `maxUnavailable:2` (rejected); rationale — `minAvailable` инвариантен к изменению числа реплик, `maxUnavailable` «тихо слабеет» при scale-down. → поздний формат, одна-буква domain.
- **EXT-1491** (`ext-batch-2221-2250.md`, KUBERNETES-KEPS, `sourceTrustTier:"T1"`, `licenseConstraint:"attribution"`, `approved`): wildcard Ingress `*.app.com` + operator на `Tenant` CR (accepted) vs per-tenant Ingress вручную (rejected) для 150+ тенантов. → лицензия — общий тег «attribution» (одна из 316 «под вопросом»).
- **EXT-849** (`ext-batch-1231-1260.md`, CWE-TOP25, домен `E`, `rowClass:negative-example`, `draft`, `licenseConstraint:"CWE-permissive"`): отсутствие object-level authorization → «critical, CWE-862/BOLA, OWASP API #1», fix `CanAccessDocumentAsync`; rejected — «low, нужен валидный токен». → показывает negative-example класс.

### Что остаётся на проходы 3–9
- **Проход 3–4:** 4 файла `02-source-examinations/` (security-governance, architecture-orchestration, architecture-distributed-data, proposal-debate-archives) — метод экзаменации источников по группам приоритета 1–4.
- **Проход 5:** `COVERAGE-LEDGER.jsonl` построчно (1500 ledger-строк) — сверка ledger↔batch, поля rowsProduced/trustTier/domainsTagged.
- **Проход 6:** `05-ARCHITECTURE-ARBITER-CATALOG-v1.json` полностью (~36 арбитров: checks/positive/negative signals + trainingRows) — прямое наполнение семьи 5.
- **Проход 7:** оставшиеся evidence-файлы раунда построчно (полные 106 JSON-ошибок, `dataset-family`/`domain`-CSV аномалии) + `01-FILES-INCLUDED.md`.
- **Проход 8:** пройтись по остальным ~70 непрочитанным телам батчей выборочно, чтобы подтвердить, что новых схем/источников не появляется (группы 8–16: MERMAID, DIATAXIS, GHERKIN, LANGGRAPH, GOOD-DOCS-TEMPLATES, ISTIO-DOCS, WIDEWORLDIMPORTERS, COMMON-VOICE и др.).
- **Проход 9:** связка с планом обучения (`plan the learning set/EXAMINATION-CONTINUATION-PLAN-v1.0.md`) и RAG-05 legal gate — как Round-002 нормализует эти строки в canonical-candidate-rows.jsonl / held-license-review / blocked / malformed-held-for-repair.
- **Проход 10:** сплошная сверка полноты (все 98 файлов) + executive summary для плана расширений.

### Честная строка покрытия (проход 2/10)
Прочитаны ПОЛНОСТЬЮ: `ext-batch-001-030.md`, `ext-batch-451-480.md` (нач.), `ext-batch-1231-1260.md`, `ext-batch-2221-2250.md`; `batch-file-summary.csv` (все 75 строк — по файлу), `batch-audit-summary.json`, `status/license/domain/dataset-family-distribution.csv`, `invalid-json-examples.txt` (образец 106 ошибок), `03-NEXT-ROUND-PROMPT.md`, `00-ROUND-MANIFEST.md`, `02-VALIDATION-RESULTS.md`. Оставшиеся ~71 батч учтены через `batch-file-summary.csv` (заголовок, ExpectedFromFileName=30 vs TotalRowIds=20, FirstRowId/LastRowId по каждому). НЕ прочитаны построчно все 1500 строк ledger и все тела 71 батча — это проходы 3–9. Вывод прохода подтверждает вердикт `HOLD`: строки реальные и содержательные, но не в каноническом node-A→B формате, с 106 JSON-ошибками, mojibake, 6 форматами id и 542 нерешёнными лицензиями.

---

## Pass 3/10 — source-examinations + knowledge-domain map + context-load (что за знания, откуда, под какой лицензией)

> Проход 3 читает НЕ строки данных (это был проход 2), а слой «происхождения знаний»: 4 файла `02-source-examinations/` (метод экзаменации реальных внешних источников по группам приоритета), карту доменов `03-...v1.md` (глубже прохода 1) и отчёт `00-CONTEXT-LOAD-REPORT.md` (глубже прохода 1). Это ответ на вопрос **«какое именно знание, из какого реального стандарта/архива, под какой лицензией, и в какое ребро pipeline (семью 1–8) оно ложится»**. Прочитаны ПОЛНОСТЬЮ все 6 файлов (см. честную строку в конце). Ключевой вывод: **экзаменации — это чистый, канонический, провенанс-полный СЛОЙ-ИСТОЧНИК, из которого потом сгенерированы дрейфующие батчи прохода 2**; то есть «правильная форма» существует в экзаменациях, а испортилась при массовой генерации 1500 строк.

### 1. Что устанавливает каждая из 4 экзаменаций (реальные источники → знание → лицензия → семьи)

Единая структура каждого файла экзаменации (7 разделов): (1) таблица источников с trust-tier и «почему включён» (лицензия названа прямо), (2) Architecture Challenges (постановки инженерных решений с якорем в первоисточнике + домен XIIGEN), (3) Patterns (когда применять / когда НЕ применять), (4) Anti-Patterns (плохой дизайн → режим отказа → лучший дизайн), (5) извлечённые Arbiter Rules (что проверяет / positive signal / negative signal), (6) кандидатные обучающие строки (rowId → семья → домен → статус draft), (7) Gaps (что НЕ покрыто). Это и есть «фабрика семьи 5»: каждый арбитр → строки семьи 5; каждая migration/rollback-развязка → строки семьи 8.

**A. `security-governance.md` (Приоритет 1 — семьи 5 и 8, наибольший разрыв).**
- **Реальные источники (6, все trust-tier 1):** OWASP-ASVS v4.0 (**CC-BY-SA 4.0**), NIST-SSDF v1.1 (**public domain US**), CWE Top 25 2023 (**MITRE free-use, нужен notice**), RFC 6749 OAuth 2.0, RFC 7519 JWT, RFC 8446 TLS 1.3 (**IETF Trust**).
- **Что устанавливает:** 15 challenges `SEC-01..SEC-15` (12 → домен `E_ARBITER_QUALITY`; 3 → `H_GOVERNANCE`: SEC-05 promote/review артефакта, SEC-09 release verification gate, SEC-13 supply-chain provenance), 6 паттернов `PAT-SEC-01..06`, 7 анти-паттернов `ANTI-SEC-01..07`, **9 арбитров**: `jwtValidation` (allowlist до подписи, `alg:none`→REJECT), `oauthGrant` (public client → authorization_code+PKCE, implicit deprecated), `passwordStorage` (bcrypt/Argon2id/scrypt vs MD5/SHA-1), `sqlInjection` (параметризация vs конкатенация), `secretPresence` (env/secrets-manager vs хардкод, CWE-798), `tenantIsolation` (обязательный tenant_id, отсутствие → отказ, НЕ global — ANTI-SEC-06, CWE основа кросс-тенант IDOR), `tlsConfig` (TLS 1.3 + forward secrecy vs RC4/3DES), `supplyChain` (pinned+hash+SBOM+подпись), `fileUpload` (magic-bytes vs Content-Type header, CWE-434).
- **15 кандидатных строк** `EXT-SEC-001..015` (в основном семья 5; EXT-SEC-011/012 → семья 8; EXT-SEC-013 → семья 1).
- **Gaps (что НЕ покрыто):** ASVS V7 (Error/Logging), ASVS V13 (API/Web Service), SSDF PW group детально, CWE entries 6–25 (batch 2, EXT-SEC-016+), RFC 9700 (OAuth 2.1 draft).

**B. `architecture-orchestration.md` (Приоритет 2 — семьи 1, 2, 5, 8).**
- **Реальные источники (5, trust-tier 1):** KUBERNETES-KEPS (**Apache 2.0**, каждый KEP = полный цикл problem→design→graduation→done/deferred), OPENTELEMETRY-SPEC (**Apache 2.0**, MUST/SHOULD + semantic conventions), OPENAPI-SPEC 3.1 (**Apache 2.0**), GOOGLE-AIP (**CC-BY 4.0**, каждый AIP = решение с рационале), MICROSOFT-LEARN-ARCH (**CC-BY 4.0**, When to use / When not / Considerations → прямо в accepted/rejected пары).
- **Что устанавливает:** 15 challenges `ARCH-01..15` (домены A_TOPOLOGY / B_ROUTING / D_EXECUTION / F_STORAGE / H_GOVERNANCE), 8 паттернов, 8 анти-паттернов, **8 арбитров**: `circuitBreaker` (half-open probe), `bulkheadIsolation` (bounded pool per workload), `tracePropagation` (W3C traceparent vs custom headers), `apiVersioning` (breaking → новый major, не in-place), `longRunningOp` (operation resource + polling, AIP-151), `idempotentCreate` (idempotency key, AIP-133), `stranglerMigration` (пофичевый роутинг vs big-bang), `dependencyCycle` (topological sort до исполнения — из XIIGEN DependencyGraph invariant).
- **15 кандидатных строк** `EXT-ARCH-001..015` (семьи 5, 8, 1, 2).
- **Gaps:** KEP graduation criteria по областям (scheduling/storage/net), OTel metrics (SLO-арбитры), OpenAPI webhook/async, AIP-136 custom methods, MS-Learn Queue/CQRS → вынесено в `architecture-distributed-data.md`.

**C. `architecture-distributed-data.md` (Приоритет 2 — семьи 1, 2, 5, 8).**
- **Реальные источники (6, все CC-BY 4.0, trust-tier 1):** MICROSOFT-LEARN-ARCH по паттернам **CQRS, Event Sourcing, Queue-based Load Leveling, Compensating Transaction, Sidecar** + DOTNET-MICROSERVICES (data ownership, outbox, CQRS impl).
- **Что устанавливает:** 10 challenges `DATA-01..10` (домены `F_REGISTRY_STORAGE` и `A_TOPOLOGY`), 6 паттернов, 6 анти-паттернов, **6 арбитров**: `cqrsFit` (CQRS только там, где раздельные read/write профили; НЕ для простого CRUD), `eventSourcingFit` (append-only + snapshot vs просто audit-log), `transactionalOutbox` (событие атомарно с записью в БД — ANTI-DATA-02: не публиковать до commit), `sagaCompensation` (компенсация для каждого коммитнутого шага, идемпотентная), `queueLoadLeveling` (буфер очереди при всплесках), `eventStoreImmutability` (события никогда не UPDATE/DELETE).
- **10 кандидатных строк** `EXT-DATA-001..010` (семьи 5, 8, 1).
- **Gaps:** MS-Learn Saga page (orchestration vs choreography), DOTNET resiliency patterns, DOTNET testing microservices.

**D. `proposal-debate-archives.md` (Приоритет 4 — семьи 1, 5, 8; БОГАТЕЙШИЙ для reprocessing-строк семьи 8).**
- **Реальные источники (5):** RUST-RFCS (**MIT**, tier 2), PYTHON-PEPS (**public domain**, tier 2), GO-PROPOSALS (**BSD**, tier 2), KAFKA-KIPS (**Apache 2.0**, tier 1), CLOUDEVENTS-SPEC closed issues (**Apache 2.0**, tier 1).
- **Ключевая идея (прямой mapping архива предложений в pipeline):** секция «problem statement» RFC/PEP → **семья 1** (user intent); секция «disposition» (accepted/rejected/postponed) → **семья 5** (вердикт арбитра с полным контекстом); секция «revision history + rejected alternatives» → **семья 8** (решение reprocess/accept/rollback). Это и делает архивы самым плотным сырьём именно для дефицитных семей 5 и 8.
- **Что устанавливает:** 8 challenges `PROP-01..08` (реальные кейсы: Rust RFC 2229 closures, PEP 634 pattern matching, PEP 479 StopIteration с `__future__`-переходом, KIP-848 rebalance, KIP-500 удаление ZooKeeper, CloudEvents datacontenttype OPTIONAL, Go generics #43651, Rust RFC 3668 async closures), 4 паттерна, 4 анти-паттерна, **4 арбитра**: `proposalCompleteness` (motivation/design/drawbacks/alternatives/unresolved все непусты), `backwardCompatibility` (migration period + feature flag/version, cutoff по метрикам adoption), `protocolMigration` (version negotiation, старое живёт до near-zero usage), `specAttributePromotion` (optional→required только после измеренного adoption).
- **8 кандидатных строк** `EXT-PROP-001..008` (семьи 5, 8, 1).
- **Gaps:** топ-20 Rust RFC по числу комментариев, rejected PEPs (3148, 526 alt), Go generics type-theory история, KIP-98 exactly-once.

**Свод по 4 экзаменациям:** реальных внешних источников — **~15 уникальных стандартов/архивов**; challenges **48** (15+15+10+8); арбитров извлечено **27** (9+8+6+4, с частичным пересечением имён между файлами — напр. `tracePropagation`/`apiVersioning` встречаются и как arch, и как governance); кандидатных строк в экзаменациях **48** (15+15+10+8) — это «чистое ядро», из которого выросли 1500 дрейфующих батч-строк прохода 2.

### 2. Карта доменов A–H (`03-...v1.md`) — какой источник наполняет какой домен (глубже прохода 1)

Каждый sourceId привязан к одному или нескольким из **8 доменов XIIGEN**. Ниже — «кто наполняет» и «какие арбитры отсюда»:

| Домен | Что это | Главные источники | Арбитры отсюда |
|-------|---------|-------------------|----------------|
| **A** Топология/оркестрация | rollout, probes, circuit breaker, миграции, trace-топология | KUBERNETES-KEPS, OTel, MS-LEARN-ARCH, KAFKA-KIPS, DOTNET-MICROSERVICES, SYSTEM-DESIGN-PRIMER, AOSA-VOL1/2 | circuitBreaker, bulkhead, stranglerMigration, protocolMigration, dependencyCycle |
| **B** Маршрутизация/выбор | API-schema routing, resource naming, пагинация | OPENAPI-SPEC, GOOGLE-AIP, MS-LEARN-ARCH, NESTJS-ADVANCED, ANGULAR-STYLE, PHP-FIG-PSRS, SYSTEM-DESIGN-PRIMER | apiVersioning, idempotentCreate, longRunningOp |
| **C** Семантический перевод | формат→формат, схема→каноника, паттерн одного языка на другом | PYTHON-PEPS, OPENAPI-SPEC, REACT-ADVANCED, EFFECTIVE-GO, AOSA-500L | — |
| **D** Исполнение/applier | execution pipeline, patch-application, build, concurrency | SWE-BENCH, DOCKER-BEST-PRACTICES, GITHUB-ACTIONS-ADVANCED, NESTJS-ADVANCED, REACT-ADVANCED, DOTNET-ASPNET-PERF, EFCORE-CONCURRENCY, RUST-ASYNC, EFFECTIVE-GO, AOSA-500L | — |
| **E** Арбитр/качество ⭐ | security-verification, vuln-detection, protocol-compliance, API-quality, graduation-gates | OWASP-ASVS, OWASP-CHEAT-SHEETS, CWE-TOP25, IETF-RFC-OAUTH2/JWT/TLS13, KUBERNETES-KEPS, OTel, GOOGLE-AIP, GCP-ARCH-FRAMEWORK, NESTJS-ADVANCED | **11 арбитров** (все security + tracePropagation/apiVersioning/proposalCompleteness) |
| **F** Реестр/хранилище | CQRS, event-sourcing, outbox, schema-registry | MS-LEARN-ARCH (CQRS/ES/Queue), DOTNET-MICROSERVICES, OPENAPI-SPEC, KAFKA-KIPS, CLOUDEVENTS-SPEC, EFCORE-CONCURRENCY, AOSA-VOL2 | cqrsFit, eventSourcingFit, transactionalOutbox, eventStoreImmutability |
| **G** Обучение/тренировка | training-lifecycle, learning-signal (issue→patch), proposal-outcome пары | NIST-SSDF, **SWE-BENCH (2 294 примера)**, KUBERNETES-KEPS, RUST-RFCS, PYTHON-PEPS, AOSA-VOL1/2, DOTNET-MICROSERVICES | — |
| **H** Управление/контракты | governance-decision, lifecycle/deprecation, supply-chain, protocol-compliance | NIST-SSDF, KUBERNETES-KEPS, OTel, OPENAPI-SPEC, GOOGLE-AIP, MS-LEARN-ARCH, IETF-RFCS, OWASP-ASVS, RUST-RFCS, PYTHON-PEPS, GO-PROPOSALS, KAFKA-KIPS, CLOUDEVENTS-SPEC, GITHUB-ACTIONS-ADVANCED | supplyChain, proposalCompleteness, backwardCompatibility, protocolMigration, specAttributePromotion, secretPresence, apiVersioning, tracePropagation |

- **Матрица покрытия источник×домен:** 33 строки источников. **Acceptance gate: PASSED** — каждый sourceId из реестра (`01-...v1.json`, Section 1) попадает минимум в один домен-столбец. Это формальная гарантия, что ни один заявленный источник не «висит без домена».
- **Домен E — самый плотный** (11 арбитров): подтверждает, что весь security/quality-слой = прямое наполнение дефицитной семьи 5. Домен H (governance) — второй по ширине источников (14). Домены **C и D не дают арбитров** (это трансформационные/исполнительные рёбра, не решающие). Домены A/H сильно пересекаются по источникам (миграции = и топология, и governance-lifecycle).

### 3. Context-load report (`00-...md`) — что загружено и почему именно эти источники (глубже прохода 1)

- **Что реально прочитано на Phase 0 (5 управляющих файлов, все Present):** `EXTERNAL-SOURCE-REGISTRY-v1.0.md` (управляющая политика источников), `EXTERNAL-SOURCES-EXAMINATION-PLAN-v2.md` (процедура экзаменации), `EXAMINATION-CONTINUATION-PLAN-v1.0.md` (**план анализа на 116 раундов**), `DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md` (архитектура pipeline), `learning-sets/flow-01-training-sets-v1/00-INDEX.md` (существующий внутренний корпус). Все три первых файла лежат в `plan the learning set/` — это связь с легальной политикой RAG-05.
- **8 dataset-семейств = рёбра pipeline** (подтверждены binding): 1 `userPrompt_to_machineCommands` → 2 `machineCommand_to_machineActionItems` → 3 `machineActionItem_to_domainResult` → 4 `domainResultsArray_to_resultI` → 5 `arbitrateResultI_with_resultsContext` → 6 `resultI_to_finalMachineResults` → 7 `finalMachineResults_to_userFormat` → 8 `historyAndVersions_to_reprocessingDecision`.
- **Baseline внутреннего корпуса (ПОЧЕМУ вообще нужны внешние источники):** F1 ~25, F2 ~20, F3 ~30, F4 ~15, **F5 ~8 (CRITICAL GAP)**, F6 ~10, F7 ~12, **F8 ~6 (CRITICAL GAP)**. Внутренний корпус даёт лишь **~14 совокупных строк** в семьях 5 и 8 — это и есть обоснование всей папки external-sources.
- **Рационале выбора источников (6 приоритетов):** P1 security/governance (OWASP-ASVS CC-BY-SA, OWASP-CHEAT-SHEETS, NIST-SSDF PD, CWE-TOP25 notice, IETF-RFCs) → семьи 5, 8; P2 architecture/orchestration (KEPS/OTel/OpenAPI Apache-2.0, GOOGLE-AIP/MS-LEARN CC-BY) → 1,2,5,8; P3 bug-fix/code-agent (SWE-BENCH MIT, 2 294 issue+patch) → 2,3,4,6; P4 proposal-архивы (RUST-RFCS/PYTHON-PEPS/GO-PROPOSALS/KAFKA-KIPS/CLOUDEVENTS) → 1,5,8; P5 OSS-нарративы (AOSA-VOL1/2, DOTNET-MICROSERVICES, SYSTEM-DESIGN-PRIMER) → 1,3,5; P6 stack-specific (NESTJS/REACT/ANGULAR/DOCKER) → 1,3,5. **Приоритет назван прямо от разрыва: сначала то, что наполняет семьи 5 и 8.**
- **Gap statement (обязателен по v2-плану):** «текущий rowized-датасет НЕ финален»; семьи 5 и 8 критически недопокрыты; точные source-spans и accepted/rejected пары отсутствуют в большинстве внутренних строк — внешние источники обязаны дать и то, и другое. Acceptance-gate отчёта: gap-statement присутствует → Phase 1 (реестр) и Phase 2 (экзаменации) разрешены.

### 4. Легальная/знаниевая картина этого слоя (свод прохода 3)

- **Какое знание:** не «текст вообще», а **инженерные развязки решений** (challenge → accepted/rejected + rationale + якорь в стандарте). Это ровно то, чего требует семья 5 (арбитраж) и семья 8 (reprocessing) — с готовым «почему».
- **Откуда (реальные стандарты, не выдумка):** OWASP-ASVS/Cheat-Sheets, NIST-SSDF, CWE Top 25, IETF RFC 6749/7519/8446, Kubernetes KEPs, OpenTelemetry Spec, OpenAPI 3.1, Google AIP, Microsoft Learn Architecture (+.NET Microservices), SWE-bench, Rust RFCs, Python PEPs, Go Proposals, Kafka KIPs, CloudEvents.
- **Под какой лицензией (на уровне слоя-источника — согласуется с реестром прохода 1):** Apache-2.0 (KEPS/OTel/OpenAPI/Kafka/CloudEvents) и MIT (SWE-bench, Rust RFCs) и public-domain (NIST-SSDF, Python PEPs) и BSD (Go) — свободные; CC-BY-4.0 (Google AIP, MS-Learn, .NET Microservices) — атрибуция; **CC-BY-SA-4.0 (OWASP)** — share-alike (производные под совместимой лицензией); MITRE-CWE — notice; IETF-RFC — текст ок. **В экзаменациях лицензия названа для КАЖДОГО источника прямо в столбце «why included»** — то есть провенанс здесь чистый; порча (общие теги `attribution`/`notice`, 542 «под вопросом») возникла позже, на этапе массовой генерации батчей (проход 2), а не в экзаменациях.
- **Trust-tier:** большинство источников — tier 1 (нормативные стандарты/спеки); tier 2 — proposal-архивы Rust/Python/Go (авторитетные, но это дискуссии/решения, а не нормативные MUST).

### 5. Кросс-ссылки (по заданию прохода)

- **С RAG-06 (llm dynamic flow):** файл `proposal-debate-archives.md` — прямой мост. Метод экзаменации RAG-06 перерабатывал внешние архивы предложений; ЗДЕСЬ он инстанцирован на реальных RFC/PEP/KIP: problem→семья 1, disposition→семья 5, revision-history/rejected-alternatives→семья 8. 8 dataset-семейств этой папки = те же рёбра динамического потока RAG-06.
- **С RAG-05 (dpo legal build):** security-источники (OWASP/CWE/IETF/NIST) через `security-governance.md` **питают арбитр-ноды** (9 арбитров jwt/oauth/password/sqlInjection/secret/tenant/tls/supplyChain/fileUpload). Именно эти арбитры — наполнение семьи 5, которую legal teacher-gate RAG-05 обязан пропустить. Слой экзаменаций — чистый провенанс-вход в тот gate (в отличие от дрейфующих батчей).
- **Внутренняя (проход 1↔3):** реестр 32 источников (проход 1) — это Section-1 источники, чей acceptance-gate доменной карты PASSED здесь подтверждён; арбитры из каталога `05-...json` (~36) — надмножество 27 арбитров, извлечённых в 4 экзаменациях.

### Что остаётся на проходы 4–9
- **Проход 4:** `01-SOURCE-REGISTRY-...v1.json` построчно (570 стр) — все 32 записи с полями licenseUseClass/targetDomains/targetFamilies/extractionStatus/priority (проход 1 дал сводку, не построчно).
- **Проход 5:** `COVERAGE-LEDGER.jsonl` построчно (1500 ledger-строк) — сверка ledger↔batch, поля rowsProduced/trustTier/domainsTagged.
- **Проход 6:** `05-ARCHITECTURE-ARBITER-CATALOG-v1.json` полностью (~36 арбитров: checks/positive/negative signals + trainingRows) — прямое наполнение семьи 5; сверка с 27 арбитрами экзаменаций.
- **Проход 7:** `06-FINAL-DOCUMENT-STRUCTURE-v1.md` полностью (политика лицензий ACCEPT/REJECT-классов, §2 исключения CC-BY-NC-ND/CITE_ONLY) + оставшиеся evidence-файлы раунда построчно (полные 106 JSON-ошибок).
- **Проход 8:** выборочно оставшиеся ~70 непрочитанных тел батчей (группы 8–16: MERMAID, DIATAXIS, GHERKIN, LANGGRAPH, ISTIO-DOCS, COMMON-VOICE и др.) — подтвердить, что новых схем/источников не появляется.
- **Проход 9:** связка с планом обучения (`EXAMINATION-CONTINUATION-PLAN-v1.0.md`, 116 раундов) и RAG-05 legal gate — как Round-002 нормализует строки в canonical-candidate-rows / held-license-review / blocked / malformed-held-for-repair.
- **Проход 10:** сплошная сверка полноты (все 98 файлов) + executive summary для плана расширений.

### Честная строка покрытия (проход 3/10)
Прочитаны ПОЛНОСТЬЮ: `02-source-examinations/security-governance.md` (115 стр), `architecture-orchestration.md` (116 стр), `architecture-distributed-data.md` (98 стр), `proposal-debate-archives.md` (93 стр), `03-OPEN-SOURCE-KNOWLEDGE-DOMAIN-MAP-v1.md` (254 стр, включая матрицу покрытия 33×8), `00-CONTEXT-LOAD-REPORT.md` (97 стр). Числа (challenges 48, арбитры 27, кандидатные строки 48, домены A–H, baseline семей, 6 приоритетов) взяты дословно из этих файлов. НЕ читались построчно в этом проходе: `01`-реестр целиком, `05`-каталог арбитров целиком, `06`-структура, `COVERAGE-LEDGER.jsonl`, тела батчей — это проходы 4–9. Вывод прохода: **слой экзаменаций — чистый провенанс-полный источник знаний (challenge→accepted/rejected+rationale+лицензия названа per-source), из которого выращены дрейфующие 1500 строк; доменная карта формально закрыта (gate PASSED); context-load доказывает, что вся папка существует ради разрыва семей 5 и 8.**

---

## Pass 4/10 — реестр источников (32 записи) + каталог арбитров (19 authority-гейтов, НЕ 36)

> Проход 4 читает построчно ДВА машинных ядра «легальной картины» и «качества»:
> `01-SOURCE-REGISTRY-ARCHITECTURE-CHALLENGES-v1.json` (570 стр — все 32 записи источников) и
> `05-ARCHITECTURE-ARBITER-CATALOG-v1.json` (509 стр — арбитры). Оба прочитаны ПОЛНОСТЬЮ и
> пересчитаны grep-ом. **Главная поправка честности этого прохода:** реестр = ровно **32**
> источника (grep `"sourceId":` = 32), а каталог арбитров = ровно **19** сериализованных
> арбитров (grep `"arbiterId"` = 19), **а не «~36»**, как писал манифест прохода 1 и как
> сформулировано в задании. «~36» — завышение. Проход 3 насчитал **27 имён арбитров** в 4
> экзаменациях; машинный каталог материализует лишь **19** из них → **8 арбитров экзаменаций
> НЕ сериализованы** в каталог (см. §4 ниже — это реальный пробел материализации, брат
> «дрейфа батчей» из прохода 2).

### 1. Полный реестр источников v1 — 32 записи (source → license → licenseUseClass → домены → семьи → priority)

Порядок — как в файле. `licenseUseClass` — это и есть машинный ВЕРДИКТ (ACCEPT-класс либо VERIFY/BLOCK).
Домены A–H и семьи 1–8 — как в проходах 1/3. `RTI` = `rawTextIngestionAllowed`.

| # | sourceId | Лицензия | licenseUseClass (вердикт) | Домены | Семьи | Prio | RTI |
|---|----------|----------|---------------------------|--------|-------|------|-----|
| 1 | KUBERNETES-KEPS | Apache-2.0 | **TRAINING_OK** | A,E,G,H | 1,2,5,8 | 2 | да |
| 2 | OPENTELEMETRY-SPEC | Apache-2.0 | **TRAINING_OK** | A,E,H | 3,5,8 | 2 | да |
| 3 | OPENAPI-SPEC (3.1) | Apache-2.0 | **TRAINING_OK** | B,F,H | 1,3,5,8 | 2 | да |
| 4 | GOOGLE-AIP | CC-BY-4.0 (код Apache-2.0) | ATTRIBUTION_REQUIRED | B,H,E | 1,3,5,8 | 2 | да |
| 5 | MICROSOFT-LEARN-ARCH | CC-BY-4.0 | ATTRIBUTION_REQUIRED (perPageVerify) | A,F,E,H | 1,2,5,8 | 2 | да |
| 6 | OWASP-ASVS v4.0 | CC-BY-SA-4.0 | ATTRIBUTION_SHAREALIKE | E,H | 5 | **1** | да |
| 7 | OWASP-CHEAT-SHEETS | CC-BY-SA-4.0 | ATTRIBUTION_SHAREALIKE (perFileVerify) | E,H | 3,5 | **1** | да |
| 8 | NIST-SSDF v1.1 | Not subject to U.S. copyright | **PUBLIC_DOMAIN_US** | G,H,E | 5,8 | **1** | да |
| 9 | CWE-TOP25 (2023) | MITRE CWE Terms | FREE_COMMERCIAL_USE_WITH_NOTICE | E,H | 5 | **1** | да |
| 10 | IETF-RFC-OAUTH2 (6749) | IETF Trust | IETF_TRUST_PROVISIONS | E,H,B | 3,5 | **1** | да |
| 11 | IETF-RFC-JWT (7519) | IETF Trust | IETF_TRUST_PROVISIONS | E,H | 3,5 | **1** | да |
| 12 | IETF-RFC-TLS13 (8446) | IETF Trust | IETF_TRUST_PROVISIONS | E,H | 5 | **1** | да |
| 13 | SWE-BENCH | MIT | **TRAINING_OK** | A,D,E,G | 2,3,4,6,8 | 3 | да |
| 14 | RUST-RFCS | MIT | **TRAINING_OK** | E,H | 1,5,8 | 4 | да |
| 15 | PYTHON-PEPS | Public domain | **TRAINING_OK** | H,C | 1,5,8 | 4 | да |
| 16 | GO-PROPOSALS | BSD-3-Clause | **TRAINING_OK** | H | 1,5,8 | 4 | да |
| 17 | KAFKA-KIPS | Apache-2.0 | **TRAINING_OK** | A,F | 1,5,8 | 4 | да |
| 18 | CLOUDEVENTS-SPEC | Apache-2.0 | **TRAINING_OK** | F,A | 5,8 | 4 | да |
| 19 | AOSA-VOL1 | CC-BY-3.0 | ATTRIBUTION_REQUIRED | A,D | 1,3,5 | 5 | да |
| 20 | AOSA-VOL2 | CC-BY-3.0 | ATTRIBUTION_REQUIRED | A,D | 1,3,5 | 5 | да |
| 21 | DOTNET-MICROSERVICES | CC-BY-4.0 | ATTRIBUTION_REQUIRED | A,F | 1,2,3 | 5 | да |
| 22 | SYSTEM-DESIGN-PRIMER | CC-BY-4.0 | ATTRIBUTION_REQUIRED | A,B | 1,5 | 5 | да |
| 23 | EFFECTIVE-GO | CC-BY-3.0 | ATTRIBUTION_REQUIRED | D,A | 3 | 5 | да |
| 24 | NESTJS-ADVANCED | MIT | **TRAINING_OK** (только архитектура, не туториалы) | B,D,E | 1,3,5 | 6 | да |
| 25 | REACT-ADVANCED | CC-BY-4.0 | ATTRIBUTION_REQUIRED | D,C | 3,7 | 6 | да |
| 26 | DOCKER-BEST-PRACTICES | Apache-2.0 | **TRAINING_OK** | D,A | 2,5 | 6 | да |
| 27 | GITHUB-ACTIONS-ADVANCED | CC-BY-4.0 | ATTRIBUTION_REQUIRED | H,D | 2,5 | 6 | да |
| 28 | DEFECTS4J | VERIFY_BEFORE_USE | **VERIFY_BEFORE_USE (BLOCK)** | — | — | 3 | **НЕТ** |
| 29 | BUGSINPY | VERIFY_BEFORE_USE | **VERIFY_BEFORE_USE (BLOCK)** | — | — | 3 | **НЕТ** |
| 30 | QUIXBUGS | VERIFY_BEFORE_USE | **VERIFY_BEFORE_USE (BLOCK)** | — | — | 3 | **НЕТ** |
| 31 | SEMGREP-RULES | VERIFY_BEFORE_USE | **VERIFY_BEFORE_USE (BLOCK)** | — | — | 3 | **НЕТ** |
| 32 | CODEQL-EXAMPLES | VERIFY_BEFORE_USE | **VERIFY_BEFORE_USE (BLOCK)** | — | — | 3 | **НЕТ** |

**Итог реестра: 27 ACCEPT / 5 BLOCK.** Разбивка ACCEPT по трению лицензии:
- **Свободные (TRAINING_OK), 11 источников** — нулевое трение, ядро общей модели:
  Apache-2.0 (KEPS, OTel, OpenAPI, Kafka, CloudEvents, Docker), MIT (SWE-bench, Rust-RFCs, NestJS),
  Public-domain (Python-PEPs), BSD-3 (Go-proposals).
- **С атрибуцией (ATTRIBUTION_REQUIRED, CC-BY-3.0/4.0), 9** — нужен credit-notice, но производные свободны:
  Google-AIP, MS-Learn-Arch, .NET-Microservices, System-Design-Primer, React-Advanced,
  GitHub-Actions, AOSA-1, AOSA-2, Effective-Go.
- **Share-alike (ATTRIBUTION_SHAREALIKE, CC-BY-SA-4.0), 2** — OWASP-ASVS, OWASP-Cheat-Sheets:
  производное обучающее множество ДОЛЖНО быть под совместимой лицензией (заражает выход).
- **Условные, 4** — NIST-SSDF (public-domain-US, но attributionRecommended), CWE-TOP25 (notice required),
  IETF-RFC ×3 (текст ок; код в RFC — per-RFC проверка).
- **BLOCK (VERIFY_BEFORE_USE), 5** — DEFECTS4J, BugsInPy, QuixBugs, Semgrep-rules, CodeQL:
  у всех `rawTextIngestionAllowed:false` И `derivedRowAllowed:false`; `verificationTarget` = прочитать
  LICENSE-файл репозитория ПЕРЕД любым использованием. До этого — заморожены, доменов/семей не имеют.

### 2. Каталог арбитров — 19 authority-гейтов, сгруппированы по домену (каждый: проверка + правило accept/reject + источник + trainingRow)

> Все 19 арбитров несут `datasetFamilies`. **14 — чистая семья 5** (`arbitrateResultI_with_resultsContext`);
> **5 дополнительно/только семья 8** (`historyAndVersions_to_reprocessingDecision`): secretPresence(5+8),
> supplyChain(5+8), apiVersioning(5+8), stranglerMigration(5+8), **protocolMigration(только 8)**. То есть каталог
> прямо наполняет ОБЕ дефицитные семьи — 5 (арбитраж) и 8 (reprocessing/rollback), ради которых и собрана папка.
> Каждый арбитр → 1–2 строки `EXT-*` (`trainingRows`): 19 арбитров = ~20 «чистых семенных» строк семьи 5/8 —
> это канон, из которого потом выросли дрейфующие 1500 батч-строк.

**Домен E — Арбитр/качество (6 арбитров; security-ядро семьи 5):**

| Арбитр | Источники | Проверяет (кратко) | Правило ACCEPT → / REJECT → | row |
|--------|-----------|--------------------|------------------------------|-----|
| `jwtValidationArbiter` | OWASP-ASVS, RFC-JWT | alg в server-allowlist? подпись валидна? exp в будущем? iss/aud совпали? | **ACCEPT:** alg в allowlist ДО проверки подписи, подпись верна, exp>now. **REJECT:** `alg:none`/alg взят из токена/подпись пропущена/exp в прошлом/нет обязательных claims | EXT-SEC-001 |
| `oauthGrantArbiter` | RFC-OAUTH2, OWASP-ASVS | public/confidential? grant подходит? PKCE для public? implicit в новом коде? | **ACCEPT:** authorization_code+PKCE (public) / code+secret или client_credentials (confidential). **REJECT:** implicit grant, токен в redirect-фрагменте, client_secret в браузерном JS | EXT-SEC-002 |
| `passwordStorageArbiter` | OWASP-ASVS | memory-hard алгоритм? cost-factor? constant-time сравнение? уникальная соль? | **ACCEPT:** bcrypt(cost≥10)/Argon2id/scrypt(N≥16384)/PBKDF2(≥100k). **REJECT:** MD5/SHA-1/SHA-256 напрямую, быстрый хэш даже с солью, non-constant-time сравнение | EXT-SEC-003 |
| `sqlInjectionArbiter` | CWE-TOP25, OWASP-ASVS | параметризованные запросы? интерполяция user-input? динамич. имена таблиц через allowlist? | **ACCEPT:** только bound-параметры + allowlist для динамич. идентификаторов. **REJECT:** конкатенация/f-string/sprintf с user-input, «санитизация» как основная защита | EXT-SEC-004 |
| `tenantIsolationArbiter` | OWASP-ASVS | tenant_id обязателен в каждом запросе? из JWT а не URL? отсутствие=ошибка? cross-tenant→404? | **ACCEPT:** tenant_id из JWT как WHERE-binding, отсутствие→отказ, cross-tenant→404. **REJECT:** tenant_id опционален/из URL, проверка после выборки, default→global/superadmin, 403 (подтверждает существование) | EXT-SEC-006 |
| `tlsConfigArbiter` | RFC-TLS13, OWASP-ASVS | min TLS≥1.2? 1.0/1.1 выключены? forward-secrecy (ECDHE)? RC4/DES/3DES? | **ACCEPT:** min 1.3 (или 1.2) + ECDHE + cert-validation. **REJECT:** TLS 1.0/1.1, RC4/DES/3DES, «внутренняя сеть = TLS не важен» | EXT-SEC-007 |

**Домен H — Управление/governance (5 арбитров; часть — семья 8):**

| Арбитр | Источники | Проверяет | Правило ACCEPT → / REJECT → | row / семьи |
|--------|-----------|-----------|------------------------------|-------------|
| `secretPresenceArbiter` | NIST-SSDF, CWE-TOP25 | литеральные креды в коде/конфиге/git-истории? env/secrets-manager? | **ACCEPT:** только env/secrets-manager + scanner + `.env` в gitignore. **REJECT:** любой литерал креда, кред в committed-конфиге/git-истории, «репо приватное» как оправдание | EXT-SEC-005 (**5+8**) |
| `supplyChainArbiter` | NIST-SSDF | зависимости pinned+hash? SBOM? подписи? канонич. источник? новые CVE? | **ACCEPT:** exact-version+hash в lockfile + SBOM + verified signatures + scan≤24ч. **REJECT:** диапазоны (^~>=latest), нет lockfile, mirror без hash, устаревший scan | EXT-SEC-008/011 (**5+8**) |
| `tracePropagationArbiter` | OTel-SPEC | W3C traceparent на исходящих? SDK-propagator? общий trace-id? OTel-conventions? | **ACCEPT:** traceparent+tracestate + SDK-propagator + OTel-имена. **REJECT:** кастомный X-Trace-ID, контекст читается но не форвардится, disconnected root spans, корреляция через логи | EXT-ARCH-003 (5) |
| `apiVersioningArbiter` | GOOGLE-AIP, OPENAPI | breaking в новом major-path (/v2/)? старая версия жива? deprecation-notice заранее? guide? | **ACCEPT:** breaking в /v2/ + старая версия живёт + анонс sunset + guide + удаление по трафику. **REJECT:** breaking in-place, удаление в тот же день, нет guide | EXT-ARCH-004 (**5+8**) |
| `proposalCompletenessArbiter` | RUST-RFCS | Motivation/Design/Drawbacks/Alternatives/Unresolved — все непусты? | **ACCEPT:** все 5 секций непусты + ≥2 alternatives с rationale + unresolved отвечены/делегированы + конкретная motivation. **REJECT:** пустые Alternatives/«none», TODO без делегирования, абстрактная motivation | EXT-PROP-001 (5) |

**Домен A — Топология/оркестрация (5 арбитров; strangler/protocol — семья 8):**

| Арбитр | Источники | Проверяет | Правило ACCEPT → / REJECT → | row / семьи |
|--------|-----------|-----------|------------------------------|-------------|
| `circuitBreakerArbiter` | MS-LEARN-ARCH | breaker на каждом внешнем вызове? threshold от baseline? half-open probe? fallback? bounded retry+jitter? | **ACCEPT:** breaker + threshold + half-open probe + fallback + backoff-jitter. **REJECT:** нет breaker, retry без backoff, нет fallback, unlimited threshold, нет probe (circuit висит открытым) | EXT-ARCH-001 (5) |
| `bulkheadIsolationArbiter` | MS-LEARN-ARCH | раздельные пулы на workload-типы? размер по concurrency? spike одного блокирует другой? per-pool мониторинг? | **ACCEPT:** bounded раздельный пул на каждый workload + per-pool alarms. **REJECT:** общий пул, spike одного виден как latency другого, только rate-limit как изоляция | EXT-ARCH-002 (5) |
| `longRunningOpArbiter` | GOOGLE-AIP | операции >неск.сек возвращают operation-resource сразу (200/202<1сек)? polling-endpoint? status+done? retry-safe id? | **ACCEPT:** POST→operation<1сек + GET /operations/{id} + done-flag + идемпотентный id. **REJECT:** held-open connection, retry с нуля после timeout, нет operation-resource/стабильного id | EXT-ARCH-005 (5) |
| `stranglerMigrationArbiter` | MS-LEARN-ARCH | миграция инкрементально (по фиче/маршруту)? legacy жив? rollback на уровне маршрута? прогресс per-feature? facade? | **ACCEPT:** одна фича/спринт + legacy держит остальное + rollback=смена routing-правила + facade. **REJECT:** big-bang cutover, legacy выключен до валидации, rollback=откат всего деплоя, «60% готово» как оправдание застрявшего rewrite | EXT-ARCH-007 (**5+8**) |
| `protocolMigrationArbiter` | KAFKA-KIPS | план держит старый+новый протокол одновременно? прогресс per-client не по календарю? снятие когда все мигрировали? rollback per-client? | **ACCEPT:** dual-mode + per-client telemetry + снятие по adoption + per-client/node rollback. **REJECT:** single cutover, нет dual-mode, снятие по фикс-дате, нет per-client телеметрии | EXT-PROP-002 (**только 8**) |

**Домен D — Исполнение/applier (1 арбитр):**

| Арбитр | Источники | Проверяет | Правило ACCEPT → / REJECT → | row |
|--------|-----------|-----------|------------------------------|-----|
| `idempotentCreateArbiter` | GOOGLE-AIP | Create принимает idempotency-key? дубликат→existing (не новый)? 200 не 409? key от клиента? TTL≥retry? | **ACCEPT:** Idempotency-Key + дубль→existing 200 OK + client-generated key + TTL≥retry-окна. **REJECT:** нет key (каждый POST=новая запись), 409 на дубль, идемпотентность по content-match, server-generated key | EXT-ARCH-006 (5) |

**Домен F — Реестр/хранилище (2 арбитра):**

| Арбитр | Источники | Проверяет | Правило ACCEPT → / REJECT → | row |
|--------|-----------|-----------|------------------------------|-----|
| `cqrsFitArbiter` | MS-LEARN-ARCH | read/write разные scalability-профили? read-модель денормализована? eventual consistency ок? сложные проекции? команда потянет 2 модели? | **ACCEPT:** read≫write (10x+) + materialized read-view + принятая eventual consistency. **REJECT:** равные нагрузки, простые lookup по PK, CQRS «потому что best practice»/из-за знакомства с паттерном | EXT-DATA-001 (5) |
| `transactionalOutboxArbiter` | DOTNET-MICROSERVICES, MS-LEARN | событие в outbox в той же транзакции что бизнес-запись? отдельный relay? published только после broker-ack? consumers идемпотентны? прямой publish мимо outbox? | **ACCEPT:** бизнес-запись+outbox-insert в одной транзакции + независимый relay + published после ack + идемпотентный consumer. **REJECT:** publish в том же code-path без координации (dual-write), rollback при fail publish, publish до commit, нет retry на relay | EXT-DATA-002 (5) |

### 3. Высшая ценность: какие источники ACCEPT — ядро общей модели, и какие арбитры — переиспользуемые quality-гейты

**Топ ACCEPT-источники для общей модели (по: чистота лицензии × покрытие дефицитных семей 5/8 × trust-tier 1):**
1. **NIST-SSDF** — public-domain-US (нулевое юридическое трение), tier 1, prio 1; питает 2 арбитра (secretPresence, supplyChain), напрямую в семьи 5 и 8. Самый «чистый» источник вообще.
2. **OWASP-ASVS** — питает 6 из 19 арбитров (jwt, oauth, password, sqlInjection, tenant, tls) — **самый плотный по семье 5**. НО share-alike (CC-BY-SA-4.0): заражает лицензию производного датасета — держать отдельно/помечать.
3. **MICROSOFT-LEARN-ARCH** — CC-BY-4.0, tier 1; питает 5 арбитров (circuitBreaker, bulkhead, strangler, cqrsFit, transactionalOutbox) — **ядро архитектуры/топологии** (семьи 1,2,5,8).
4. **GOOGLE-AIP** — CC-BY-4.0; 3 арбитра (apiVersioning, longRunningOp, idempotentCreate) — API-качество.
5. **CWE-TOP25** (notice) + **IETF-RFC ×3** (текст ок) — доп. security-подпорка (sqlInjection, secret, jwt, oauth, tls).
6. **SWE-BENCH** — MIT, tier 1, **2294 issue→patch пары** — единственный крупный источник learning-signal (домен G, семьи 2,3,4,6,8); высшая ценность для «обучающего сигнала», а не для арбитража.
7. **Apache-2.0 группа** (KEPS/OTel/OpenAPI/Kafka/CloudEvents/Docker) + **MIT группа** (SWE-bench/Rust-RFCs/NestJS) = свободное TRAINING_OK ядро без трения.

**Переиспользуемые quality-гейты (арбитры, годные ВНЕ этого корпуса — прямо в расширении VS/VSCode «документируй код + флагни проблему»):**
- **Security-review (домен E + secretPresence):** jwtValidation, oauthGrant, passwordStorage, sqlInjection,
  tenantIsolation, tlsConfig, secretPresence — это готовые детекторы «правильно vs правдоподобно-но-дыряво»
  для код-ревью локальной моделью. Именно они — прямое наполнение критической семьи 5.
- **Architecture-review:** circuitBreaker, bulkheadIsolation, idempotentCreate, transactionalOutbox, cqrsFit —
  гейты «когда паттерн уместен, когда нет» для ревью топологии/данных.
- **Lifecycle/reprocessing (семья 8):** apiVersioning, stranglerMigration, protocolMigration, supplyChain —
  гейты решений «мигрировать/откатить/переобработать», ровно дефицит семьи 8.
Каждый арбитр = тройка `checks` (что проверить) + `positiveSignals` (ACCEPT) + `negativeSignals` (REJECT),
т.е. это готовый rubric — тот же тип «arbiter = authority», что EdgeAuthorityUnit в RAG-05 и quality-слой
динамического потока RAG-06.

### 4. Честная поправка: «19, не 36» — пробел материализации каталога

- **Факт:** grep `"arbiterId"` в `05-...json` = **19**. Grep `"sourceId":` в `01-...json` = **32**.
- **Проход 3 назвал 27 имён арбитров** в 4 экзаменациях (9+8+6+4). Каталог `05` материализует **19**.
  **8 арбитров экзаменаций НЕ попали в машинный каталог:** `fileUploadArbiter` (magic-bytes vs Content-Type, CWE-434),
  `dependencyCycleArbiter` (topological sort), `eventSourcingFitArbiter`, `sagaCompensationArbiter`,
  `queueLoadLevelingArbiter`, `eventStoreImmutabilityArbiter`, `backwardCompatibilityArbiter`,
  `specAttributePromotionArbiter`.
- **Смысл для плана:** «~36» из манифеста прохода 1 — завышение, доверять нельзя. Каталог семьи-5 покрыт лишь
  на 19/27 названных; 8 гейтов существуют только в прозе экзаменаций, но не как машинные rubric → это тот же
  класс дефекта, что «дрейф батчей» (проход 2): чистый источник богаче, чем сериализованный артефакт. Для
  продолжения расширений: перед импортом каталог арбитров нужно достроить с 19 до 27 (материализовать 8
  недостающих) — иначе quality-слой недокомплектен ровно в дефицитной семье.

### Что остаётся на проходы 5–9
- **Проход 5:** `COVERAGE-LEDGER.jsonl` построчно (1500 ledger-строк) — сверка ledger↔batch, поля rowsProduced/trustTier/domainsTagged.
- **Проход 6:** уже частично закрыт этим проходом (каталог `05` прочитан полностью, 19 арбитров). Остаётся сверка каждого арбитра с его `trainingRows` в телах батчей + добор 8 несериализованных арбитров из экзаменаций.
- **Проход 7:** `06-FINAL-DOCUMENT-STRUCTURE-v1.md` полностью (политика лицензий ACCEPT/REJECT-классов, §2 исключения CC-BY-NC-ND/CITE_ONLY) + оставшиеся evidence-файлы раунда построчно (полные 106 JSON-ошибок).
- **Проход 8:** выборочно оставшиеся ~70 непрочитанных тел батчей (группы 8–16: MERMAID, DIATAXIS, GHERKIN, LANGGRAPH, ISTIO-DOCS, COMMON-VOICE и др.) — подтвердить, что новых схем/источников не появляется.
- **Проход 9:** связка с планом обучения (`EXAMINATION-CONTINUATION-PLAN-v1.0.md`, 116 раундов) и RAG-05 legal gate — как Round-002 нормализует строки в canonical-candidate-rows / held-license-review / blocked / malformed-held-for-repair.
- **Проход 10:** сплошная сверка полноты (все 98 файлов) + executive summary для плана расширений.

### Честная строка покрытия (проход 4/10)
Прочитаны ПОЛНОСТЬЮ построчно: `01-SOURCE-REGISTRY-ARCHITECTURE-CHALLENGES-v1.json` (570 стр, все 32 записи —
каждое поле license/licenseUseClass/targetDomains/targetFamilies/priority/rawTextIngestionAllowed) и
`05-ARCHITECTURE-ARBITER-CATALOG-v1.json` (509 стр, все 19 арбитров — checks/positiveSignals/negativeSignals/
sourceIds/datasetFamilies/trainingRows). Числа подтверждены grep-ом: sourceId=32, arbiterId=19. НЕ читались в
этом проходе построчно: `COVERAGE-LEDGER.jsonl`, `06-...md`, тела батчей — это проходы 5,7,8. Вывод прохода:
**реестр = 32 источника (27 ACCEPT / 5 VERIFY-BLOCK); каталог = 19 арбитров (14 семья-5, 5 семья-8), а НЕ 36 —
это завышение манифеста; каталог покрывает лишь 19/27 названных в экзаменациях арбитров → 8 гейтов не
материализованы. ACCEPT-ядро без трения — NIST-SSDF(PD)+Apache/MIT-группа; плотнейший security-источник —
OWASP-ASVS (6 арбитров, но share-alike); плотнейший архитектурный — MS-Learn-Arch (5 арбитров).**

---

## Pass 5/10 — COVERAGE-LEDGER (master учёт 1500) + final license policy + 106 JSON-ошибок (repair scope)

> Проход 5 читает построчно `COVERAGE-LEDGER.jsonl` (1500 строк, 630 КБ) — это МАШИННЫЙ
> УЧЁТ по одной записи на каждую произведённую строку данных. Плюс `06-FINAL-DOCUMENT-STRUCTURE-v1.md`
> целиком (итоговая политика лицензий ACCEPT/REJECT) и точный разбор 106 JSON-ошибок
> (`invalid-json-examples.txt` + `02-VALIDATION-RESULTS.md`). Ledger распарсен Python-скриптом
> (все 1500 строк, распределения пересчитаны), 75 тел батчей независимо просканированы на JSON-фенсы.
> **Три главных вывода этого прохода (ниже с доказательствами):**
> 1. **Сам ledger ЧИСТ:** все 1500 строк парсятся без единой ошибки, 0 дублей sourceId. 106 JSON-ошибок
>    живут ТОЛЬКО в телах батчей, НЕ в ledger. Ledger — самый авторитетный row-level источник учёта.
> 2. **«542 лицензии под вопросом» — мнимый юридический блокер:** 451 строка с общим тегом
>    (`attribution`/`notice`/`none`) — ВСЕ до одной разрешаются в конкретную лицензию через
>    соседнее поле `license` (0 неразрешённых). Это механическая правка поля, а не юр-исследование.
> 3. **Более глубокий разрыв, чем 106 ошибок:** ledger использует **97 источников**, а реестр v1
>    гейтит лишь **32**. **75 источников не зарегистрированы** (нет вердикта licenseUseClass);
>    из них произведено **695 из 1500 строк (46%)**. Обе GPL-строки пришли именно через
>    незарегистрированные источники (ANSIBLE-DOCS, PLANTUML-DOCS).

### 1. Схема строки ledger (ROW SCHEMA) — 13 обязательных полей + 1 опциональное

Каждая ledger-строка = один JSON-объект. Поля (по факту grep-подсчёта присутствия в 1500 строках):

| Поле | Присутствует | Что несёт |
|------|--------------|-----------|
| `sourceId` | 1500 | идентификатор произведённой строки (= rowId батча): `EXT-SEC-001`, `EXT-1510` |
| `registryId` | 1500 | ИЗ КАКОГО источника взято (это ключ провенанса): `OWASP-ASVS`, `SWE-BENCH` |
| `unitTitle` | 1500 | название смысловой единицы источника (напр. «V3.5.1 JWT algorithm allowlist») |
| `sourceUrl` | 1500 | URL первоисточника |
| `license` | 1500 | **сырая лицензия источника — ВСЕГДА конкретная** (никогда не общий тег) |
| `trustTier` | 1500 | доверие; тип дрейфует `1`/`"T1"`/`2`/`"T2"` |
| `status` | 1500 | **стадия конвейера** (НЕ то же, что trainingStatus батча): 3 значения (см. §3) |
| `domainsTagged` | 1500 | массив доменов A–H |
| `familiesTagged` | 1500 | массив семей 1–8 (тип дрейфует: число ↔ длинное имя) |
| `rowsProduced` | 1500 | сколько строк произведено (обычно 1; 2 записи = 0) |
| `licenseConstraint` | 1500 | **применённое лицензионное ограничение — ЗДЕСЬ деградация в общий тег** |
| `examinedDate` | 1500 | дата (всё `2026-05-04`) |
| `trainingStatus` | **260** | опциональное; во ВСЕХ 260 = `approved` (в 1240 строках поля НЕТ) |

**Ключ схемы:** ledger несёт **ДВА** лицензионных поля — `license` (сырое, всегда конкретное) и
`licenseConstraint` (применённое, деградирует в общий тег). Именно расхождение этих двух полей и есть
источник «542 под вопросом» (см. §2). И **ДВА** статусных поля — `status` (стадия конвейера) и опциональный
`trainingStatus` — оба отличаются от `trainingStatus` в телах батчей → в системе живут ТРИ статусных словаря.

### 2. Master-распределение по факту 1500 строк (пересчитано из самого ledger)

**(а) Лицензия — два поля, и почему это важно.**

| licenseConstraint (применённое) | # | | license (сырое) | # |
|---------------------------------|---|---|-----------------|---|
| MIT | 344 | | MIT | 478 |
| **attribution (общий тег)** | **335** | | Apache-2.0 | 389 |
| Apache-2.0 | 279 | | CC-BY-4.0 | 259 |
| CC-BY-4.0 | 172 | | PSF-2.0 | 62 |
| **notice (общий тег)** | **105** | | IETF-TLP | 62 |
| PSF-2.0 / IETF-TLP | 40 / 40 | | CC-BY-SA-3.0 | 57 |
| CC-BY-SA-3.0 | 35 | | CC-BY-2.0-FR | 46 |
| CC-BY-2.0-FR | 33 | | MITRE-CWE-ToU / CWE-TOU | 21 / 20 |
| MITRE notice required | 26 | | CC-BY-2.0 | 19 |
| CC-BY-SA-4.0 | 17 | | CC-BY-SA-4.0 | 17 |
| CWE-TLP / CWE-TOU / CWE-permissive | 13 / 11 / 7 | | CWE-TLP / CWE-permissive | 13 / 13 |
| CC-BY-2.0 | 10 | | Public domain | 10 |
| **none** | **9** | | CWE-Terms | 7 |
| IETF-Trust-provisions / IETF-Trust | 6 / 2 | | IETF-Trust | 6 |
| BSD-3-Clause | 4 | | BSD-3 / CC-BY-3.0 | 4 / 3 |
| CC-BY-3.0 | 3 | | CC-0 | 2 |
| CC-0 | 2 | | **GPL-3.0** | **2** |
| **GPL-3.0** | **2** | | MIT/OGL, MIT-0, MPL-2.0 | 1 / 1 / 1 |
| **BLOCK** | **2** | | | |
| MIT-0 / MPL-2.0 / Public domain | 1 / 1 / 1 | | | |

**Разрешимость «под вопросом» — ГЛАВНОЕ ЧИСЛО:** строк с ОБЩИМ `licenseConstraint`
(`attribution`+`notice`+`none`+`BLOCK`) = **451**. Из них через конкретное поле `license`
разрешаются **ВСЕ 451, неразрешённых 0**: → MIT 135, Apache-2.0 110, CC-BY-4.0 87, PSF-2.0 22,
IETF-TLP 22, CC-BY-SA-3.0 22, CC-BY-2.0-FR 13, Public-domain 9, CC-BY-2.0 9, CWE-* 22. **Вывод:**
«542 требуют решения политики» из batch-audit — это НЕ юридическая неопределённость, а потеря
конкретики при генерации; конкретная лицензия сохранена в соседнем поле. Правка = скопировать
`license` → `licenseConstraint` там, где второе — общий тег. Механически, без юр-исследования.
(451 vs 542 — небольшой разрыв ledger↔CSV: CSV считался по телам батчей, ledger — по себе.)

**(б) GPL — точная идентичность (было «2 строки», стало «какие именно»):**
- `EXT-191` ← **ANSIBLE-DOCS**, GPL-3.0 (и `license`, и `licenseConstraint`).
- `EXT-216` ← **PLANTUML-DOCS**, GPL-3.0 (оба поля).
- Обе — из НЕзарегистрированных источников (см. §2д). Значит GPL-заражение вошло НЕ через реестр
  (в реестре v1 GPL нет вовсе), а через незагейченное расширение источников. Точная причинно-следственная нить.

**(в) Семьи (числовой + строковый тег СЛИТЫ) — сумма ровно 1500:**

| Семья | Всего | Смысл ребра | Комментарий к разрыву |
|-------|-------|-------------|------------------------|
| 1 userPrompt→machineCommands | 176 | | ок |
| 2 machineCommand→actionItems | 179 | | ок |
| 3 actionItem→domainResult | 234 | | густая |
| 4 domainResultsArray→resultI | **116** | | **06-doc для батчей 1–3 давал 0! Поздние батчи заполнили** |
| 5 arbitrateResultI (АРБИТРАЖ) ⭐ | **352** | критический разрыв | **был baseline ~8 → стал 352** |
| 6 resultI→finalMachineResults | 163 | | заполнена |
| 7 finalMachineResults→userFormat | 108 | | ок |
| 8 historyAndVersions→reprocessing ⭐ | **172** | критический разрыв | **был baseline ~6 → стал 172** |

→ **Обе дефицитные семьи (5 и 8) — теперь плотнее всех прочих или наравне.** Именно ради них
собрана папка (baseline внутреннего корпуса: F5≈8, F8≈6). Число F5=352 из ledger ВЫШЕ, чем
`dataset-family-distribution.csv` (там F5=288) — снова ledger авторитетнее батч-CSV.

**(г) Домены (flatten, сумма 1505 → 5 строк мульти-доменные):** E 362 (арбитр/качество —
densest, подтверждает семью 5), D 281 (исполнение), C 227 (перевод — язык/Tatoeba), F 217
(хранилище), A 186 (топология), G 173 (обучение), B 35, H 24. Домены B/H тонкие на row-уровне,
хотя в каталоге арбитров H был широк — это разрыв «арбитр named, но строк мало».

**(д) Источники — 97 в ledger против 32 в реестре (ГЛУБОКИЙ РАЗРЫВ):**
- distinct `registryId` в ledger = **97**; реестр v1 = **32**; пересечение = **22**.
- **75 источников НЕ в реестре** (нет формального вердикта licenseUseClass): ANGULAR-DOCS,
  ASPNETCORE-DOCS-SAMPLES, BIGCODEBENCH, OPENASSISTANT-OASST1, TATOEBA, DAPPER, VUE-DOCS,
  EFCORE-SOURCE, GHERKIN, DIATAXIS, COMMON-VOICE, ANSIBLE-DOCS, PLANTUML-DOCS, ENVOY-PROXY,
  GOVUK-DESIGN-SYSTEM, CONVENTIONAL-COMMITS и др.
- **695 из 1500 строк (46%) произведены из незарегистрированных источников.** Топ-источники
  ledger: CWE-TOP25 86, SWE-BENCH 78, **BIGCODEBENCH 77** (не в реестре!), MS-LEARN-ARCH 71,
  GOOGLE-AIP 71, PYTHON-PEPS 70, OWASP-ASVS 68, DOTNET-MICROSERVICES 68, IETF-RFCS 68,
  OTEL-SPEC 67, KUBERNETES-KEPS 67, **OPENASSISTANT-OASST1 67** (не в реестре!).
- **Смысл для плана:** легальный gate (RAG-05) держит вердикт лишь для 32/97 источников. Почти
  половина корпуса юридически «самозадекларирована» (несёт ledger-`license`, но не прошла реестр).
  Перед промоцией эти 75 источников нужно добавить в реестр и профильтровать — это БОЛЬШЕ, чем
  106 JSON-ошибок, и это прямой вход в Round-002.

### 3. Сверка ledger ↔ batch (reconciliation) — где сходится, где нет

| Проверка | Результат | Вывод |
|----------|-----------|-------|
| ledger строк | 1500, все парсятся, 0 JSON-ошибок | ledger-сторона ЧИСТА |
| уникальность `sourceId` | 1500 уникальных, **0 дублей** | дрейф id — на стороне БАТЧЕЙ, не ledger |
| `rowsProduced` сумма | **1498** (1498×1 + 2×0) | 2 записи `BLOCK_no_row` (EXT-276, EXT-286, оба **SWE-BENCH**) произвели 0 строк → 1500 ledger-записей учитывают 1498 реальных строк + 2 заблокированных |
| `status` (стадия) | domain_reviewed_draft_rowized **1058** / examined **440** / BLOCK_no_row **2** | это НЕ статус готовности |
| `trainingStatus` (ledger) | только **260** строк, все `approved`; в 1240 — поля нет | ≠ батч-статус |
| batch trainingStatus (из CSV прохода 1) | draft 661 / approved 380 / candidate 280 / ready 180 | ТРЕТИЙ словарь; считался по телам батчей |
| `trustTier` | 1=939, "T1"=400, 2=121, "T2"=40 → **tier-1=1339, tier-2=161** | тип дрейфует int↔string |
| batch↔ledger id-mismatch (из `02-VALIDATION-RESULTS.md`) | **400** batch-not-in-ledger + **400** ledger-not-in-batch до нормализации; **160 + 160** после нормализации суффиксов | ~160 id НЕ сходятся даже после нормализации — остаточный провенанс-дефект |
| licenseConstraint ledger vs round-CSV | ~90% совпадение, но не идентично (MIT 344 vs 353, attribution 335 vs 316) | CSV — по батчам, ledger — по себе; ledger авторитетнее |

**Итог сверки:** ledger — чистая, полностью парсящаяся сторона учёта (0 дублей, 0 JSON-ошибок);
весь беспорядок (106 ошибок, 3 статусных словаря, ~160 несводимых id, деградация лицензий в общий
тег) — на стороне тел батчей. Для плана: **ledger надо считать источником истины по учёту**, а
батчи — материалом на нормализацию.

### 4. Итоговая политика лицензий (`06-FINAL-DOCUMENT-STRUCTURE-v1.md`, прочитан целиком)

Управляющий документ: `EXTERNAL-SOURCE-REGISTRY-v1.0.md`. Политика (§2):

**ACCEPTED-классы (разрешены для обучения общей модели):**

| Класс | Примеры | Разрешённое использование |
|-------|---------|---------------------------|
| `TRAINING_OK` | Apache-2.0, MIT, BSD, Public domain | сырой текст + производные строки + accepted/rejected пары |
| `ATTRIBUTION_REQUIRED` | CC-BY-3.0, CC-BY-4.0 | как TRAINING_OK; атрибуция обязана быть в поле `licenseConstraint` строки |
| `ATTRIBUTION_SHAREALIKE` | CC-BY-SA-4.0 | как ATTRIBUTION_REQUIRED; **производные — под совместимой лицензией (заражает выход)** |
| `FREE_COMMERCIAL_USE_WITH_NOTICE` | MITRE CWE Terms | производные строки + обязательный notice в метаданных строки |
| `IETF_TRUST_PROVISIONS` | IETF RFCs | текст документа ок; **переиспользование КОДА из RFC — проверка per-RFC** |
| `PUBLIC_DOMAIN_US` | NIST | сырой текст; атрибуция не обязательна (но рекомендована) |

**REJECTED-классы (исключены политикой):**

| Класс | Примеры | Почему отклонён |
|-------|---------|-----------------|
| `CC-BY-NC-ND` | **Google SRE Book, Cosmic Python** | нет производных; нет коммерческого использования |
| `CITE_ONLY_NO_COPY` | **Martin Fowler articles, AWS Builders Library** | лицензии на переиспользование нет — только публичный доступ |
| `VERIFY_BEFORE_USE` | **DEFECTS4J, BugsInPy, QuixBugs, Semgrep-rules, CodeQL-examples** | лицензия не подтверждена; непригодны до промоции |

**Никогда НЕ включается:** туториальный контент (как настроить фреймворк, пошаговые setup-гайды);
циклические строки (вход=выход, один и тот же концепт); строки-факты («OAuth требует PKCE» без
рассуждения); инженерные блог-посты компаний (нет training-лицензии); любой источник, чью лицензию
нельзя подтвердить из его LICENSE-файла.

**Замечание о датах:** §4–§8 в `06-doc` (running-total 61 строка, F5=38, F8=8, план batch-4,
каталог арбитров summary) — это цифры БАТЧЕЙ 1–3 (ранние). Полный ledger 1500 их давно перерос
(F5=352, F8=172). То есть `06-doc` — это ПОЛИТИКА (актуальна) + ранний СНИМОК прогресса
(историчен). Для плана брать из `06`: политику ACCEPT/REJECT; прогресс-числа — из ledger.

### 5. Точный repair-scope 106 JSON-ошибок (+ 9 файлов без JSON) — что и где чинить

**Авторитетные счётчики (`02-VALIDATION-RESULTS.md`):** 1324 fenced-JSON-блока; распарсено 1218;
**106 сбоев парсинга**; **7 файлов** с ошибками; **9 файлов** вообще без JSON-блоков.

**(а) Класс 1 — «Unterminated string» (106 ошибок, 7 файлов). Диагноз ТОЧЕН:**
- **Тип ошибки:** 100% «Unterminated string passed in.» (в выборке 80/106 — 77 unterminated + 3 «Invalid JSON primitive»).
- **Сбойное поле:** **`acceptedOutput`** в ~94% (75/80 в выборке); плюс вложенные варианты
  `acceptedOutput.diagram` (EXT-068, MERMAID) и `acceptedOutput.howToGuide` (EXT-109, GOOD-DOCS-TEMPLATES).
- **Концентрация (выборка `invalid-json-examples.txt`, 80 из 106):**
  `ext-batch-661-690.md` **22**, `ext-batch-631-660.md` **20**, `ext-batch-691-720.md` **20**,
  `ext-batch-721-750.md` **16** — это строки-`-R1`/`-R*` (reprocessing-ревизии, `sourceIds`
  цитирует родительскую EXT-строку, несут большие кодовые блоки) — ровно те, что проход 2 назвал
  «худшими»; + по 1 в `ext-batch-061-090.md` (EXT-068) и `ext-batch-121-150.md` (EXT-109).
- **КОРНЕВАЯ ПРИЧИНА (проверена независимо):** значение `acceptedOutput` — большой многострочный
  code/markdown, который ВНУТРИ себя содержит СВОЙ fenced-код-блок (` ```yaml `, ` ```python `,
  ` ```ts ` …). Внутренний закрывающий ` ``` ` сталкивается с внешним markdown-фенсом ` ```json `
  строки и **преждевременно обрывает JSON-блок в середине строки → «Unterminated string»**.
  Доказательство: в `ext-batch-661-690.md` — 60 строк с ` ``` `, 22 открывашки ` ```json `, но
  **19 вложенных языковых фенсов**. Расширенное доказательство хрупкости извлечения: раунд-валидатор
  (его split по фенсам) насчитал **106 сбоев / 1324 блока**; мой независимый non-greedy regex —
  **0 сбоев / 1320 блоков**. Результат парсинга ЗАВИСИТ от того, как резать фенсы — это и есть
  определяющий симптом «встроенного фенса». Пример EXT-431-R1 (`ext-batch-631-660.md`): его
  `acceptedOutput` уже корректно использует `\n`-эскейпы и держит ` ```yaml…``` ` ВНУТРИ строки —
  контент валиден, ломает его именно встроенный закрывающий ` ``` `.
- **ПРАВКА (механическая, контент сохраняется):** нейтрализовать/заэскейпить внутренние тройные
  бэктики (хранить код без ` ``` `-фенса, либо эскейпить их), либо вынести код-блок из JSON-строки
  в соседний fenced-блок; затем ре-сериализовать строку как валидный JSON. Это правка сериализации/
  вложенности фенсов, **НЕ проблема качества данных** — содержимое настоящее и богато заэскейплено.

**(б) Класс 2 — 9 файлов БЕЗ fenced-JSON (ОТДЕЛЬНАЯ правка). Точный список подтверждён моим сканом:**
`ext-batch-1621-1650.md`, `-1651-1680`, `-1681-1710`, `-1711-1740`, `-1741-1770`, `-1771-1800`,
`-1801-1830`, `-1831-1860`, `-1861-1890` (сплошной ряд 9 файлов, EXT-1091…~EXT-1250). Эти файлы
используют **markdown key-value формат строк** (`**rowId:** …`, `**acceptedOutput:** \`{...}\``) —
JSON-полезная нагрузка сидит ИНЛАЙН в одиночных бэктиках, а не в fenced-блоке. **Правка:** извлечь
инлайн-backtick-JSON → переиздать как канонические fenced-JSON-строки. Класс правки ИНОЙ, чем
«unterminated string».

**(в) Что НЕ чинится этими двумя правками (остаётся HOLD-блокером):** plan-format гейт по-прежнему
`Final Pair format = 0`, `Rephrasing pairs = 0`. Даже после починки JSON и разрешения лицензий
корпус ещё НЕ в целевом формате node-пар `A→B` + перефразировка `Phrasing-B`. Плюс mojibake
(`â†’`, `Ã¡`, `DiÃ¡taxis`) пронизывает контент и должен чиститься до промоции.

### Что остаётся на проходы 6–9
- **Проход 6:** сверка каждого арбитра каталога `05` с его `trainingRows` в телах батчей + добор 8
  несериализованных арбитров (частично закрыто проходом 4).
- **Проход 7:** оставшиеся evidence-файлы построчно + разбор остаточных **160 несводимых id**
  (batch↔ledger после нормализации) и полный список 7 файлов с ошибками (выборка дала 6).
- **Проход 8:** выборочно ~70 непрочитанных тел батчей, ВКЛЮЧАЯ 9 markdown-формат файлов
  (класс 2) и группы 8–16 (BIGCODEBENCH, OASST1, TATOEBA, COMMON-VOICE …) — подтвердить, что новых
  схем/источников нет, и учесть все 97 источников.
- **Проход 9:** связка с `EXAMINATION-CONTINUATION-PLAN-v1.0.md` (116 раундов) и RAG-05 legal gate:
  как Round-002 нормализует строки (canonical-candidate / held-license-review / blocked /
  malformed-held-for-repair). **75 незарегистрированных источников — прямой вход этого раунда.**
- **Проход 10:** сплошная сверка полноты (все 98 файлов) + executive summary для плана расширений.

### Честная строка покрытия (проход 5/10)
Прочитаны/обработаны ПОЛНОСТЬЮ: `COVERAGE-LEDGER.jsonl` (все 1500 строк распарсены Python-скриптом;
распределения license/licenseConstraint/status/trainingStatus/trustTier/domainsTagged/familiesTagged/
registryId/rowsProduced пересчитаны; разрешимость общих тегов и overlap 97↔32 вычислены),
`06-FINAL-DOCUMENT-STRUCTURE-v1.md` (183 стр, вся политика §1–§8), `02-VALIDATION-RESULTS.md`
(все счётчики), `invalid-json-examples.txt` (выборка 80 записей — per-file/per-field/error-type),
плюс НЕЗАВИСИМЫЙ скан всех 75 тел батчей на fenced-JSON (подтвердил 9 файлов без JSON и хрупкость
извлечения 106-ошибок). Цитаты строк EXT-431-R1, EXT-068, EXT-109, EXT-1091 приведены дословно из
файлов. НЕ читались построчно в этом проходе: полные тела 106-ошибочных батчей и все 97 источников —
это проходы 6–9. Вывод прохода: **ledger чист и авторитетен (1500/1500 парсится, 0 дублей, 1498
произведённых + 2 BLOCK); «542 под вопросом» разрешаются механически (451 общий тег → 0
неразрешённых через поле `license`); GPL = EXT-191/EXT-216 (Ansible/PlantUML, оба незарег.);
глубочайший разрыв — 97 источников использовано против 32 в реестре (75 незарег., 695 строк/46%);
106 JSON-ошибок = `acceptedOutput` со встроенными код-фенсами в 4 reprocessing-файлах (631/661/691/721)
+ 2 одиночных, правка механическая; отдельный класс — 9 markdown-файлов 1621–1890 без JSON; целевой
node-A→B формат по-прежнему 0 → HOLD сохраняется.**

---

## Pass 6/10 — полный учёт 97 источников + поздние батчи (группы 8-16) + 75 незарегистрированных

> Проход 6 закрывает учёт на УРОВНЕ ИСТОЧНИКА (source-level no-neglect). Проход 5 установил, что
> ledger использует **97 источников**, реестр v1 гейтит лишь **32**, а **75 не зарегистрированы**
> (695 строк = 46%). Но проход 5 назвал только ТОП-15 и частичный список 75. Проход 6 доводит это до
> ПОЛНОГО поимённого учёта: каждый из 97 источников → лицензия → зарегистрирован? → число строк, чтобы
> НИ ОДИН источник не остался неучтённым. Плюс прочитаны/просканированы поздние тела батчей (группы 8-16,
> ~70 файлов, не читанных построчно в проходе 2) и все 9 markdown-формат файлов (класс 2 из прохода 5).
> Данные учёта пересчитаны Python-скриптом из `COVERAGE-LEDGER.jsonl` (поле `registryId`+`license` по каждой
> из 1500 строк) и сверены с 32 `sourceId` реестра `01-...json`; распределение сходится точно
> (registered 805 + unregistered 695 = 1500; 75 незарег. источников сгруппированы по темам с суммой ровно 695).
>
> **Три главных вывода прохода 6 (ниже с доказательствами):**
> 1. **«75 незарегистрированных» — это ГЛАВНЫМ ОБРАЗОМ governance-дыра, а не юр-дыра.** Из 695 строк
>    незарегистрированных источников юридическое трение несут лишь **9 строк** (2 GPL + 6 share-alike + 1 MPL);
>    остальные **686 — permissive** (Apache-2.0 231, MIT 179, CC-BY-4.0 134 атрибуция, IETF-TLP 68,
>    CC-BY-2.0-FR 65 атрибуция, CC-0/MIT-0/PD 7). Настоящий hard-block — **всего 2 GPL-строки** (Ansible, PlantUML).
> 2. **Реестр v1 не только УЖЕ ledger — он ещё и ПЕРЕзаявляет покрытие.** Из 32 записей реестра лишь **22**
>    реально дали строки; **10 дали ноль** — 5 корректно (VERIFY_BEFORE_USE) + 5 ACCEPT-источников выпали или
>    слились под другим именем (AOSA-VOL2, CLOUDEVENTS-SPEC, GO-PROPOSALS — 0 строк; IETF-RFC-JWT слит в агрегат
>    `IETF-RFCS`; OWASP-CHEAT-SHEETS слит в `OWASP-ASVS`). То есть реестр гейтит вещи, которых в корпусе нет,
>    и не гейтит половину того, что есть.
> 3. **Поздние батчи (группы 8-16) добавляют ~9 новых доменов и ТРИ несовместимых словаря арбитров.** Кроме
>    security/arch-ядра появились: code-agent бенчмарки, frontend-фреймворки, .NET-data/samples, перевод/язык,
>    docs/format-стандарты, infra/ops, AI/LLM-тулинг, testing-фреймворки. При этом батчи ссылаются на арбитров
>    вне 19-каталога тремя способами (см. §3.Б) — расширение дефекта материализации из прохода 4.

### 1. ПОЛНЫЙ учёт 97 источников (registryId → лицензия → зарегистрирован? → строк)

**Итог по учёту:** 97 distinct `registryId` в ledger = **22 зарегистрированных-и-использованных** (805 строк) +
**75 незарегистрированных** (695 строк). Плюс **10 записей реестра дали 0 строк** (не входят в 97).
805 + 695 = **1500** (сходится точно).

**Таблица A — 22 зарегистрированных источника, реально давших строки (805 строк):**

| # | registryId | Лицензия (реестр) | Строк | licenseUseClass |
|---|-----------|-------------------|------:|-----------------|
| 1 | CWE-TOP25 | MITRE CWE Terms (notice) | 86 | FREE_COMMERCIAL_USE_WITH_NOTICE |
| 2 | SWE-BENCH | MIT | 78 | TRAINING_OK |
| 3 | MICROSOFT-LEARN-ARCH | CC-BY-4.0 | 71 | ATTRIBUTION_REQUIRED |
| 4 | GOOGLE-AIP | CC-BY-4.0 (код Apache-2.0) | 71 | ATTRIBUTION_REQUIRED |
| 5 | PYTHON-PEPS | Public domain / PSF-2.0 | 70 | TRAINING_OK |
| 6 | OWASP-ASVS | CC-BY-SA-4.0 | 68 | ATTRIBUTION_SHAREALIKE |
| 7 | DOTNET-MICROSERVICES | CC-BY-4.0 | 68 | ATTRIBUTION_REQUIRED |
| 8 | OPENTELEMETRY-SPEC | Apache-2.0 | 67 | TRAINING_OK |
| 9 | KUBERNETES-KEPS | Apache-2.0 | 67 | TRAINING_OK |
| 10 | REACT-ADVANCED | CC-BY-4.0 / MIT | 66 | ATTRIBUTION_REQUIRED |
| 11 | NESTJS-ADVANCED | MIT | 66 | TRAINING_OK |
| 12 | NIST-SSDF | Public domain US | 6 | PUBLIC_DOMAIN_US |
| 13 | OPENAPI-SPEC | Apache-2.0 | 4 | TRAINING_OK |
| 14 | RUST-RFCS | MIT | 3 | TRAINING_OK |
| 15 | DOCKER-BEST-PRACTICES | Apache-2.0 | 3 | TRAINING_OK |
| 16 | GITHUB-ACTIONS-ADVANCED | CC-BY-4.0 / MIT | 3 | ATTRIBUTION_REQUIRED |
| 17 | KAFKA-KIPS | Apache-2.0 | 2 | TRAINING_OK |
| 18 | EFFECTIVE-GO | CC-BY-3.0 / CC-BY-4.0 | 2 | ATTRIBUTION_REQUIRED |
| 19 | IETF-RFC-OAUTH2 | IETF Trust | 1 | IETF_TRUST_PROVISIONS |
| 20 | IETF-RFC-TLS13 | IETF Trust | 1 | IETF_TRUST_PROVISIONS |
| 21 | AOSA-VOL1 | CC-BY-3.0 (ledger тег CC-BY-SA-3.0 — дрейф) | 1 | ATTRIBUTION_REQUIRED |
| 22 | SYSTEM-DESIGN-PRIMER | CC-BY-4.0 (ledger тег CC-BY-SA-4.0 — дрейф) | 1 | ATTRIBUTION_REQUIRED |

> Замечание (лицензионный дрейф и внутри зарегистрированных): AOSA-VOL1, SYSTEM-DESIGN-PRIMER, PYTHON-PEPS
> несут в ledger тег, отличный от реестра (реестр = не-SA, ledger иногда = CC-BY-**SA**). Реестр
> авторитетен (CC-BY, не share-alike). Это тот же дрейф-класс, что и деградация в общий тег (проход 5).

**Таблица B — 10 записей реестра, давших 0 строк (в 97 НЕ входят):**

| registryId | licenseUseClass | Почему 0 строк |
|-----------|-----------------|----------------|
| DEFECTS4J | VERIFY_BEFORE_USE | корректно не ингестирован (block до LICENSE) |
| BUGSINPY | VERIFY_BEFORE_USE | корректно не ингестирован |
| QUIXBUGS | VERIFY_BEFORE_USE | корректно не ингестирован |
| SEMGREP-RULES | VERIFY_BEFORE_USE | корректно не ингестирован |
| CODEQL-EXAMPLES | VERIFY_BEFORE_USE | корректно не ингестирован |
| AOSA-VOL2 | ATTRIBUTION_REQUIRED | **ACCEPT, но 0 строк** — заявлен, не использован |
| CLOUDEVENTS-SPEC | TRAINING_OK | **ACCEPT, но 0 строк** — заявлен, не использован |
| GO-PROPOSALS | TRAINING_OK | **ACCEPT, но 0 строк** — заявлен, не использован |
| IETF-RFC-JWT | IETF_TRUST_PROVISIONS | слит в агрегат `IETF-RFCS` (см. Таблицу C) |
| OWASP-CHEAT-SHEETS | ATTRIBUTION_SHAREALIKE | слит в `OWASP-ASVS` / 0 отдельных строк |

> **Смысл:** реестр v1 ≠ фактическое покрытие. 5 ACCEPT-источников (AOSA-VOL2, CLOUDEVENTS-SPEC,
> GO-PROPOSALS, JWT-агрегирован, Cheat-Sheets-слит) в 1500 строках отсутствуют как отдельные записи.
> «32 источника» — это ДЕКЛАРАЦИЯ, а не то, из чего построен корпус.

**Таблица C — 75 НЕзарегистрированных источников (695 строк), по тематическим группам 8-16.**
Формат: `registryId (лицензия, строк)`. ⚠ = неpermissive лицензия (см. §2). Сумма всех групп = ровно 695.

- **C1. Code-agent / бенчмарки (3 ист., 146 строк):** BIGCODEBENCH (Apache-2.0, **77**), OPENASSISTANT-OASST1 (Apache-2.0, **67**), SWE-CARE (Apache-2.0, 2).
- **C2. Агрегаты/спеки (2 ист., 70):** IETF-RFCS (IETF-TLP, **68** — агрегат 3 зарег. IETF-RFC), ASYNCAPI-SPEC (Apache-2.0, 2).
- **C3. Frontend-фреймворки (16 ист., 146):** ANGULAR-DOCS (CC-BY-4.0/MIT, **66**), VUE-DOCS (MIT, **62**), NESTJS-CORE (MIT, 3), REACT-CORE (MIT, 2), SVELTE-DOCS (MIT, 2), REACT-DOCS (CC-BY-4.0, 1), ANGULAR-COMPONENTS (MIT, 1), ANGULARJS-SOURCE (MIT, 1), JQUERY (MIT, 1), KNOCKOUTJS (MIT, 1), BACKBONEJS (MIT, 1), REQUIREJS (MIT, 1), MUI-MATERIAL-UI (MIT, 1), NESTJS-DOCS (MIT, 1), EXPRESS (MIT, 1), NODEJS-CORE (MIT, 1).
- **C4. .NET data / samples (13 ист., 209):** ASPNETCORE-DOCS-SAMPLES (CC-BY-4.0/MIT, **66**), EFCORE-SOURCE (MIT, **65**), DAPPER (Apache-2.0, **64**), WIDEWORLDIMPORTERS (MIT, 3), EF6-SOURCE (MIT/Apache-2.0, 2), WPF-SAMPLES (MIT, 2), ASPNET-WEBSTACK (Apache-2.0, 1), ASPNETCORE-SAMPLES (MIT, 1), ESHOP-ON-WEB (MIT, 1), ESHOP-MODERNIZING (MIT, 1), JASON-TAYLOR-CLEANARCH (MIT, 1), SQL-SERVER-SAMPLES (MIT, 1), XUNIT-DOTNET (Apache-2.0, 1).
- **C5. Перевод / язык (4 ист., 71):** TATOEBA (CC-BY-2.0-FR/CC-BY-2.0, **65**), COMMON-VOICE (CC-0, 2), PROJECT-GUTENBERG (Public domain, 2), ⚠WIKTIONARY (CC-BY-SA-3.0/4.0, 2 — share-alike).
- **C6. Docs / diagram / format-стандарты (11 ист., 18):** ⚠DIATAXIS (CC-BY-SA-4.0, 3 — share-alike), MERMAID (MIT, 3), ⚠PLANTUML-DOCS (Apache-2.0 + **GPL-3.0**, 2 — 1 строка GPL), GOOD-DOCS-TEMPLATES (MIT-0/CC-BY-4.0, 2), GOVUK-DESIGN-SYSTEM (MIT/OGL-v3.0, 2), GHERKIN (MIT, 1), CUCUMBER-DOCS (MIT, 1), SPECFLOW-DOCS (BSD-3-Clause, 1), CONVENTIONAL-COMMITS (MIT, 1), KEEP-A-CHANGELOG (MIT, 1), ⚠SCRUM-GUIDE (CC-BY-SA-4.0, 1 — share-alike).
- **C7. Infra / ops (7 ист., 13):** ISTIO-DOCS (Apache-2.0, 3), HELM-DOCS (Apache-2.0, 3), PROMETHEUS-DOCS (Apache-2.0, 3), ENVOY-PROXY (Apache-2.0, 1), KUBERNETES-DOCS (CC-BY-4.0, 1), ⚠ANSIBLE-DOCS (**GPL-3.0**, 1), ⚠TERRAFORM-DOCS (MPL-2.0, 1 — weak-copyleft).
- **C8. AI / LLM-тулинг (9 ист., 12):** LANGGRAPH (MIT, 3), OPENAI-COOKBOOK (MIT, 2), LANGCHAIN-DOCS (MIT, 1), PROMPTFOO (MIT, 1), PROMPT-ENGINEERING-GUIDE (MIT, 1), ANTHROPIC-SKILLS (Apache-2.0, 1), MCP-DOCS (MIT, 1), MCP-SERVERS (Apache-2.0, 1), AUTOGEN (MIT, 1).
- **C9. Testing-фреймворки (9 ист., 9):** PLAYWRIGHT (Apache-2.0, 1), SELENIUM (Apache-2.0, 1), CYPRESS (MIT, 1), CYPRESS-DOCS (MIT, 1), JEST (MIT, 1), PYTEST (MIT, 1), NUNIT (MIT, 1), MOQ (MIT, 1), ROBOT-FRAMEWORK (Apache-2.0, 1).
- **C10. Security-data (1 ист., 1):** NIST-NVD (Public domain, 1).

> Контроль суммы: 146+70+146+209+71+18+13+12+9+1 = **695**. ✔ Все 75 источников поимённо учтены,
> ни один не отброшен.

### 2. Лицензионный риск незарегистрированных: где реальное трение (и его почти нет)

**Распределение 695 незарегистрированных строк по лицензии (permissive vs трение):**

| Лицензия | Строк | Класс | Вердикт |
|----------|------:|-------|---------|
| Apache-2.0 | 231 | permissive | ACCEPT |
| MIT | 179 | permissive | ACCEPT |
| CC-BY-4.0 | 134 | атрибуция | ACCEPT с notice |
| IETF-TLP | 68 | текст ок | ACCEPT (условно) |
| CC-BY-2.0-FR | 65 | атрибуция (Tatoeba) | ACCEPT с notice |
| CC-0 / MIT-0 / Public domain | 2/2/3 | public | ACCEPT |
| BSD-3-Clause | 1 | permissive | ACCEPT |
| **CC-BY-SA-4.0** | **4** (Diataxis 3, Scrum-guide 1) | **share-alike** | ⚠ заражает выход |
| **CC-BY-SA-3.0** | **2** (Wiktionary) | **share-alike** | ⚠ заражает выход |
| **MPL-2.0** | **1** (Terraform) | **weak-copyleft** | ⚠ file-level, обычно ок для обучения |
| **GPL-3.0** | **1** в top-теге (Ansible) | **strong-copyleft** | ❌ HARD-BLOCK |

**Точный список рискованных незарегистрированных источников (6 ист., 9 строк из 695):**

| Источник | Строк | Лицензия | Класс риска | Действие перед промоцией |
|----------|------:|----------|-------------|---------------------------|
| ANSIBLE-DOCS | 1 (**EXT-191**) | GPL-3.0 | strong-copyleft | ❌ удалить/заменить (hard-block) |
| PLANTUML-DOCS | 1 из 2 (**EXT-216**) | GPL-3.0 | strong-copyleft | ❌ удалить/заменить строку GPL (вторая строка Apache-2.0 — оставить) |
| DIATAXIS | 3 | CC-BY-SA-4.0 | share-alike | ⚠ держать отдельно/помечать (как OWASP) |
| WIKTIONARY | 2 | CC-BY-SA-3.0/4.0 | share-alike | ⚠ держать отдельно/помечать |
| SCRUM-GUIDE | 1 | CC-BY-SA-4.0 | share-alike | ⚠ держать отдельно/помечать |
| TERRAFORM-DOCS | 1 | MPL-2.0 | weak-copyleft | ⚠ file-level copyleft; для текст-обучения обычно допустимо, требует policy-решения |

> **Вывод по риску (ответ на вопрос задания «any beyond the 2 GPL that are risky?»):** ДА, но мягко.
> **Реальный hard-block = 2 строки GPL** (Ansible EXT-191, PlantUML EXT-216) — ровно те, что нашёл проход 5.
> **Дополнительно** всплыло **6 share-alike строк** из 3 незарегистрированных источников (Diataxis, Wiktionary,
> Scrum-guide) + **1 MPL** (Terraform). Share-alike — не блок, но «заражает лицензию производного датасета»
> (тот же класс, что зарегистрированные OWASP-ASVS/Cheat-Sheets) → держать отдельно или помечать. MPL —
> weak-copyleft на уровне файла, для текст-обучения обычно допустим, но требует явного policy-решения.
> **Все прочие 686 незарегистрированных строк — permissive (ACCEPT).** Значит «75 незарегистрированных» —
> это на 99% governance-долг (нет формального `licenseUseClass`), а не юридическая опасность: добавить их в
> реестр v2 и проставить вердикт — механическая работа; юридически спорны лишь 9 строк из 695.

### 3. Что нового добавляют поздние батчи (группы 8-16): домены, содержимое, арбитры

**А. Новые домены/группы источников (сверх security/arch-ядра прохода 1-4).** Поздние поколения
генерации ввели ~9 новых тематических кластеров (см. Таблицу C): **code-agent бенчмарки**
(BigCodeBench, OpenAssistant-OASST1 — крупнейшие незарег., 77+67), **frontend** (Angular 66, Vue 62,
React/Svelte/Nest/jQuery/Knockout/Backbone…), **.NET-data/samples** (ASP.NET-Core-samples 66, EFCore 65,
Dapper 64, eShop, WideWorldImporters), **перевод/язык** (Tatoeba 65, Common-Voice, Gutenberg, Wiktionary),
**docs/format** (Mermaid, PlantUML, Diataxis, Gherkin/Cucumber/SpecFlow, Conventional-Commits,
Keep-a-Changelog, Scrum-Guide, GOV.UK), **infra/ops** (Istio, Helm, Prometheus, Envoy, Ansible, Terraform),
**AI/LLM-тулинг** (LangGraph, LangChain, OpenAI-Cookbook, Promptfoo, MCP-docs/servers, Anthropic-Skills,
AutoGen), **testing** (Playwright, Selenium, Cypress, Jest, PyTest, NUnit, xUnit, Moq, Robot-Framework),
**security-data** (NIST-NVD). Схема строки та же (`a→f(b)+c`, accepted/rejected DPO), новых ПОЛЕЙ не
появилось — подтверждается прогноз прохода 2 «новых схем нет, новые ИСТОЧНИКИ есть».

**Б. НОВАЯ находка — три несовместимых словаря арбитров (расширяет «19 vs 27» прохода 4).** Поздние
батчи ссылаются на арбитров, которых НЕТ в машинном каталоге `05` (19 шт.) и НЕТ среди 27 названных в
экзаменациях, причём в трёх разных стилях:
1. **Каталожные** (проход 4): `jwtValidationArbiter`, `circuitBreakerArbiter` … — 19 сериализованных.
2. **Ad-hoc «gate»-арбитры** (в 9 markdown-файлах 1621-1890): `data-residency-gate`, `blast-radius-isolation`,
   `zero-downtime-gate`, `incremental-migration-pattern`, `exception-specificity-rule`,
   `error-vs-empty-distinguishability`, `pdb-protection-during-rollout`, `typed-validation-result`.
3. **Персона/роль-арбитры** (в поздних fenced-батчах 1921-2250): `security-engineer`, `dotnet-expert`,
   `nlp-engineer`, `arabic-language-expert`. Это не «проверка+signals», а роль-рецензент.
→ **Смысл для плана:** каталог арбитров (19) покрывает лишь малую долю арбитров, реально упоминаемых в
строках. Перед промоцией нужен единый словарь: свести ad-hoc gate/persona-арбитры к каноническим
`ILogicalUnit`-гейтам либо явно пометить их как свободный текст. Это тот же дефект материализации, что
и «19 vs 27», только шире — на уровне 1500 строк.

**В. Содержимое (реальные пары, не заглушки) — цитаты из late-батчей:**
- **EXT-1091** (`1621-1650`, KUBERNETES-KEPS, семья 1, домен A): US/EU/APAC + GDPR → `PLAN_TOPOLOGY:
  multi-cluster-federation` (accepted) vs `single-cluster-topology-labels` (rejected); арбитры
  `data-residency-gate`,`blast-radius-isolation`; `trainingStatus: ready`.
- **EXT-1092** (`1621-1650`, **BIGCODEBENCH**, семья 3, домен D): Python `try/except: return {}` →
  `ANTI_PATTERN_DETECTED: bare-except-with-empty-dict-fallback` (accepted) vs `OK:{}` (rejected). Реальный
  Python-анти-паттерн с фиксом.
- **EXT-1093 / EXT-1253** (SWE-BENCH, миграция): Django WSGI→ASGI зерокост-даунтайм — 5-шаговый canary
  (accepted) vs big-bang (rejected).
- **EXT-1376** (`2041-2070`, **DAPPER**, семья 8, домен D): 70 SQL-inj-запросов → Dapper-параметризация в
  3 недели без DBA (accepted) vs 350 stored-procedures (rejected); `gate_b: zero string interpolation`.
- **EXT-1384** (`2041-2070`, **TATOEBA**, семья 4, домен D): выбор стратегии EN-AR параллельного корпуса —
  8K bidirectionally-verified Tatoeba + Wikipedia LASER≥1.05 (accepted) vs все 12K Tatoeba (rejected).

### 4. 9 markdown-формат файлов (класс 2 прохода 5) — прочитаны, характеристика

**Файлы (сплошной ряд):** `1621-1650`, `1651-1680`, `1681-1710`, `1711-1740`, `1741-1770`, `1771-1800`,
`1801-1830`, `1831-1860`, `1861-1890` (EXT-1091…~EXT-1270). **Формат подтверждён:** это НЕ «без данных» —
это **markdown-key-value** (`**rowId:** …`, `**sourceIds:** [...]`, `**acceptedOutput:** \`{...}\``): та же
богатая DPO-пара, но JSON-полезная нагрузка сидит инлайн в ОДИНОЧНЫХ бэктиках, а не в fenced-```json```
блоке — поэтому валидатор раунда насчитал «9 файлов без JSON-блоков». **Ведущие источники** тех же 3 семейств
(KUBERNETES-KEPS, BIGCODEBENCH, SWE-BENCH + вкрапления), **`trainingStatus: ready`** почти во всех строках
(эти файлы — донор «ready 180» из прохода 1). **Правка (проход 9/Round-002):** извлечь инлайн-backtick-JSON →
переиздать как канонические fenced-JSON. Содержимое сохраняется, класс правки иной, чем «unterminated string».

### 5. Кросс-ссылки (обновление по факту прохода 6)

- **С RAG-03 (language-translate):** подтверждено СИЛЬНЕЕ. Tatoeba здесь — **65 строк**, но это НЕ параллельный
  корпус: почти все Tatoeba-строки — **инженерные decision-пары О ПОСТРОЕНИИ корпуса** (какую стратегию EN-AR
  выбрать, LASER-фильтрация, bidirectional verification — EXT-1384), а не готовые пары «предложение→перевод».
  Плюс 2 «сырые» CC-BY-2.0 пары (проход 1). Итог: **готового лицензированного параллельного корпуса перевода
  ЗДЕСЬ по-прежнему НЕТ** — есть решения о том, как его собрать. Это ровно «verified-licensed = 0» RAG-03.
- **С RAG-05 (dpo legal build):** здешние **75 незарегистрированных источников — прямой вход legal teacher-gate
  Round-002**. Но юридически gate тривиален: 686/695 permissive; ему надо (а) заблокировать 2 GPL-строки,
  (б) пометить 6 share-alike + 1 MPL как «отдельно/notice», (в) проставить `licenseUseClass` остальным 69
  источникам (механически). «HOLD» держится не из-за лицензий, а из-за формата (node-A→B=0) и провенанса
  (реестр ≠ факт).
- **Внутренняя (проход 5↔6):** проход 5 дал число (97/32/75/695); проход 6 дал ПОИМЁННЫЙ учёт всех 97
  (Таблицы A/B/C) + доказал, что реестр ещё и ПЕРЕзаявляет (10 записей = 0 строк, из них 5 ACCEPT).

### Что остаётся на проходы 7-9
- **Проход 7:** остаточные **160 несводимых id** (batch↔ledger после нормализации) построчно + полный список
  7 файлов со 106 JSON-ошибками (выборка дала 6) + `01-FILES-INCLUDED.md`.
- **Проход 8:** добор арбитр-словаря — свести ad-hoc `*-gate`/persona-арбитров (§3.Б) к каноническим гейтам;
  сверить `trainingRows` каждого из 19 каталожных арбитров с телами батчей.
- **Проход 9:** связка с `EXAMINATION-CONTINUATION-PLAN-v1.0.md` (116 раундов) и RAG-05 — как Round-002
  нормализует строки (canonical-candidate / held-license-review / blocked / malformed-held-for-repair);
  **75 незарегистрированных + 2 GPL + 6 SA + 1 MPL — точный список held/blocked этого раунда.**
- **Проход 10:** сплошная сверка полноты (все 98 файлов) + executive summary для плана расширений.

### Честная строка покрытия (проход 6/10)
Обработано ПОЛНОСТЬЮ: `COVERAGE-LEDGER.jsonl` пересчитан Python-скриптом на уровне ИСТОЧНИКА (per-`registryId`
row-count + license-set + registered-flag против 32 `sourceId` реестра `01-...json`; суммы registered 805 /
unregistered 695 / total 1500 и группировка 75 источников на 695 строк сверены точно). Прочитаны построчно:
`ext-batch-1621-1650.md` (markdown-класс), `ext-batch-1861-1890.md` (markdown-класс), выборки из
`ext-batch-2041-2070.md` (Dapper EXT-1376, Tatoeba EXT-1384). Просканированы все 12 поздних поколений
(1921-2250) на состав источников (стабильный микс ~2 Tatoeba + ~2 Angular + ~4 EFCore/Dapper на файл).
Все 97 ledger-источников поимённо учтены (Таблицы A/B/C), 10 нуль-строчных записей реестра выявлены.
НЕ читались построчно все 70 тел батчей целиком (учтены через ledger-аккаунтинг + выборочное чтение —
метод прохода 2). Вывод прохода: **97 источников = 22 зарег.-использованных (805) + 75 незарег. (695); реестр
ПЕРЕзаявляет (10 записей → 0 строк, 5 из них ACCEPT); юридического трения почти нет — 2 GPL hard-block
(Ansible/PlantUML) + 6 share-alike + 1 MPL, остальные 686 permissive; поздние батчи добавили ~9 новых доменов
и ТРИ несовместимых словаря арбитров; Tatoeba — decision-пары, не параллельный корпус (подтверждает RAG-03);
9 markdown-файлов = богатый контент в инлайн-backtick-JSON, статус ready. HOLD держится форматом/провенансом,
не лицензиями.**


## Pass 7/10 — round-audit evidence + continuation-plan (116 раундов) + путь HOLD→ready

Что это за проход. Проходы 1-6 разобрали ДАННЫЙ слой (манифест, 1500 строк тел батчей, source-examinations,
реестр 32 + каталог 19 арбитров, master-ledger, поимённый учёт 97 источников). Проход 7 разбирает ПРОЦЕССНЫЙ
слой: пакет `round-zips/external-learning-round-001-batch-audit/` (сам аудит-вердикт HOLD) и план-дорожную
карту `EXAMINATION-CONTINUATION-PLAN-v1.0.md` — то есть КАК этот корпус проходит путь из «HOLD» в
«готово к promotion». Всё read-only, кроме этого RAG-файла.

### 1. Что такое round-zips и что установил round-001-batch-audit
- `round-zips/` = процессный журнал. Каждый «раунд» упакован как самостоятельный пакет:
  `00-ROUND-MANIFEST.md` (вердикт+причины), `01-FILES-INCLUDED.md`, `02-VALIDATION-RESULTS.md`,
  `03-NEXT-ROUND-PROMPT.md`, папка `evidence/` (CSV+JSON), папка `files/`, и `.zip` всего раунда.
- Round-001 — это НЕ examination-раунд источника, а АУДИТ батчей: единственный вопрос — «готовы ли внешние
  батчи к импорту в локальные learning-sets?». Дата 2026-05-04. Проверял папку `external-sources\batches`
  против плана `EXAMINATION-CONTINUATION-PLAN-v1.0.md`.
- **Вердикт: HOLD** — «candidate material only. Not ready for local learning-set import».
- 8 причин HOLD (00-ROUND-MANIFEST): (1) не в финальном node-pair формате; (2) нет обязательных rephrasing-пар;
  (3) 106 fenced JSON-блоков не парсятся; (4) часть файлов = markdown-поля вместо JSON-блоков; (5) лицензии
  недостаточно строги для promotion — многие строки лишь «attribution»/«notice»; (6) присутствуют GPL-строки
  (block/replace до promotion); (7) rowId и sourceId дрейфуют в нескольких форматах.
- Раунд НИЧЕГО не мутировал: manifest «Files changed in source folders: None» + маркер-файл
  `files/NO-SOURCE-FILES-CHANGED.txt`. Это чистый read-only аудит.

### 2. Точные числа validation-results (02-VALIDATION-RESULTS + batch-audit-summary.json)

| Проверка | Результат | Что доказывает |
|---|---|---|
| Файлов проверено | 75 | (в папке 98, но аудит взял 75 md-батчей) |
| batchRowIds / unique / duplicate-groups | 1500 / 1500 / 0 | id уникальны, дублей нет |
| Ledger rows / unique | 1500 / 1500 | учёт сходится по объёму |
| Fenced JSON blocks | 1324 | не 1500 — часть строк не в JSON |
| JSON parsed / errors | 1218 / **106** | 106 битых блоков = repair-scope |
| Files with NO JSON blocks / with JSON errors | 9 / 7 | 9 markdown-класс, 7 несут все 106 ошибок |
| Pair-format matches | **0** | финального node-A→B формата НЕТ |
| Rephrasing pairs | **0** | обязательного rephrasing НЕТ |
| Required folder shape / per-node files / simulation | no / no / no | это СЫРЬЁ, не готовые сеты |
| License: clear / hold-or-policy / GPL-block | 954 / 542 / 2 | юр-трение малое, но 542 надо решить |
| ID до нормализации (batch↔ledger) | 400 / 400 | сильный дрейф id |
| ID после suffix-нормализации | **160 / 160** | жёсткий несводимый остаток |
| Status: draft/approved/candidate/ready | 661 / 380 / 280 / 180 | ни одна не «promoted» |
| Decision | **HOLD** | candidate-only |

Вывод числами: формат = 0 готовых пар и 0 rephrasing → корпус сырой; лицензии почти чистые (954 clear, лишь
2 GPL) → HOLD держится НЕ лицензиями; но 542 hold-строки + 160 id-residual + 106 битых JSON — это конкретная
очередь ремонта до promotion.

### 3. batch↔ledger 160-residual — что это и почему важно
- До нормализации: 400 rowId есть в батчах, но нет в ledger; и 400 наоборот (ledger→batch).
- Suffix-нормализация (снятие суффиксов вариантов `-R1`, `-A1` и т.п.) свела расхождение к **160 / 160**.
- Значит: 160 идентификаторов существуют ТОЛЬКО в телах батчей и 160 ТОЛЬКО в ledger — даже после
  нормализации суффиксов. Причина — несколько несовместимых схем нумерации, видны в самих телах:
  `EXT-068`, `EXT-431-R1`, `EXT-691-A1`, `EXT-SEC-001`/`EXT-SEC-011`, `EXT-831` (голый), `EXT-091`.
  Ранние батчи — `EXT-NNN`, средние — `EXT-NNN-R1` (rephrase-вариант), поздние — `EXT-NNN-A1`, security —
  `EXT-SEC-NNN`. Suffix-нормализация не сшивает разные СЕМЬИ id, только снимает хвосты.
- Это провенанс-дефект, а не легальный: round-002 обязан сверить и сшить эти 160 (hard-rule «preserve source
  provenance and row identifiers»), иначе строки нельзя проследить до источника → нельзя promote.

### 4. 106 JSON-ошибок — точный характер (invalid-json-examples.txt + batch-file-summary.csv)
- ВСЕ образцы ошибок = один класс: **«Unterminated string passed in»**. Причина — длинное поле
  `acceptedOutput` с код-блоком/markdown: строка обрывается на границе fence, либо содержит неэкранированные
  переводы строк/кавычки внутри кода (напр. Mermaid-диаграмма `EXT-068`, how-to-guide `EXT-109`, десятки
  `EXT-431..EXT-459` с C#/Python/K8s YAML внутри `acceptedOutput`).
- Концентрация ошибок (batch-file-summary.csv): `ext-batch-631-660.md` и `ext-batch-661-690.md` дают основную
  массу (сплошные EXT-43x/44x/45x с большими код-outputами); плюс `ext-batch-061-090.md` (1 ошибка, EXT-068),
  `ext-batch-121-150.md` (1 ошибка, EXT-109). Итог: **7 файлов несут все 106 ошибок**, ещё **9 файлов вообще
  без JSON** (markdown-класс, но статус `ready` — контент лежит в inline-backtick, не в fenced-блоке).
- Round-002 hard-rule прямо про это: **НЕ укорачивать accepted/rejected текст ради лёгкого парсинга** — чинить
  через полное экранирование в валидные строки ИЛИ конверсию строки в canonical JSONL со структурными
  текст-полями. Контент дороже парсабельности.

### 5. Протокол раундов (как устроен процесс)
- **Один examination-раунд = ровно один source unit** (граница «one unit is …» в таблице плана: один KEP-каталог,
  один RFC целиком, одна ASVS-секция, один SWE-bench instance и т.д.). Правило: не дробить unit между раундами,
  не сливать два unit в один раунд.
- Каждый раунд закрывается пакетом того же вида, что round-001: manifest + files-included + validation-results
  + next-round-prompt + evidence + zip. Раунд сам порождает промпт СЛЕДУЮЩЕГО раунда (`03-NEXT-ROUND-PROMPT`) —
  дорожная карта самодвижущаяся.
- Правило «After one unit»: Stop → записать unit в coverage ledger → проверить пары по validation-checklist →
  только потом выбрать следующий unit. «Один документ разобранный полностью дороже десяти поверхностно».

### 6. Round-002 — что именно делает следующий раунд (03-NEXT-ROUND-PROMPT)
- **Цель:** нормализовать external candidate rows в ОДНУ canonical pre-promotion schema — БЕЗ импорта в
  локальные learning-sets (candidate-статус сохраняется).
- **Hard rules:** не promote строку с unknown/vague/blocked license; candidate-строка ≠ финальная пара;
  не укорачивать accepted/rejected; чинить битый JSON без потери контента; сохранить provenance + rowId;
  выдать zip.
- **Обязательные выходы:** `canonical-candidate-rows.jsonl`, `held-license-review.jsonl`,
  `blocked-license-rows.jsonl`, `malformed-held-for-repair.jsonl`, `normalization-report.md` + 4 round-doc
  (manifest/files/validation/next-prompt) + round zip.
- **Условие закрытия:** раунд закрыт только когда КАЖДАЯ из 1500 строк отнесена в одно из четырёх состояний:
  normalized ∨ held-for-license-review ∨ blocked-by-license ∨ malformed-held (с КОНКРЕТНОЙ причиной ремонта).
- Ожидаемое соответствие числам round-001 (вывод по числам, промпт не диктует точную раскладку явно):
  954 clear → canonical-candidate; 542 hold/policy → held-license-review; 2 GPL → blocked-license;
  106 malformed → malformed-held-for-repair; 160 id-residual сшивается внутри canonical (provenance-reconcile).

### 7. Continuation-plan (116 раундов) — структура фаз
ЧЕСТНАЯ ОГОВОРКА: в файле `EXAMINATION-CONTINUATION-PLAN-v1.0.md` НЕТ буквального заголовка «116 rounds»
(grep по «116» = 0 совпадений). План задаёт модель «1 раунд = 1 source unit» + порядок разбора в 4 ИМЕНОВАННЫХ
фазах с целевыми счётчиками; **«116» — агрегатная сумма целей этих фаз** (20+50+~26+20 ≈ 116).

- **Фаза 1 — Architecture & orchestration, цель 20 source units.** Сначала Kubernetes KEP (с завершённым
  production-readiness review — самый богатый arbiter-материал): KEP-2799 (Secret-based SA tokens),
  KEP-3488 (CEL for Admission), KEP-1282 (Conformance PSS), KEP-2305 (Server-Side Field Validation, есть
  rejected-alternatives → negative scheme-planner), + 1 KEP уровня Stable. Затем OpenTelemetry Spec
  (resource attributes, baggage, одна metrics-секция). Затем Google AIP-121/131/4232.
- **Фаза 2 — Code agent datasets, цель 50 task instances.** SWE-bench verified (smallest-patch-first) →
  BigCodeBench (multi-step reasoning) → SWE-EVO (evolution ≥2 версий, long-horizon scheme-planner) →
  SWE-CARE (reviewer verdict называет конкретное нарушенное правило) → Nemotron-SWE-v1 (сначала фильтр
  по subset-license metadata, только MIT/Apache/BSD).
- **Фаза 3 — Security & governance.** OWASP ASVS §2 (Authentication) + §4 (Access Control) ПОЛНОСТЬЮ —
  каждое numbered-требование = arbiter-unit пара. Затем CWE-287/306/522/798/284 (mitigations → arbiter-пары).
  Затем отобранные NIST NVD CVE (описание = flag-case, подразумеваемая compliant-форма = pass-case).
  Затем IETF RFC 7519 (JWT), 6749 (OAuth 2.0), 8446 (TLS 1.3).
- **Фаза 4 — Implementation guidelines, цель 20 source units.** Rust API Guidelines (C-CONSISTENT-NAMES,
  C-COMMON-TRAITS, C-SEND-SYNC, C-DTOR-FAIL — явные do/do-not → arbiter-пары) → Go docs (Effective Go
  concurrency, memory model, errors) → AOSP (HAL, Binder IPC) → Node.js docs (Event Emitter, Streams,
  event loop).
- Порядок обоснован разрывами покрытия: KEP/AIP первыми, потому что их структурированный accept/reject
  reasoning прямо питает Node 7/8; code-datasets вторыми — полные A→B цепочки для Node 3-5; security третьей;
  guidelines четвёртой.

### 8. Путь HOLD → ready (полная цепочка)
1. **Round-001 (сделано):** аудит батчей → HOLD, 8 причин, числа зафиксированы (раздел 2). Ничего не мутировано.
2. **Round-002 (следующий):** нормализация 1500 строк в canonical pre-promotion schema; сортировка по
   license/malformed на 4 jsonl; сшивка 160 id-residual (provenance-reconcile); починка 106 битых JSON без
   потери контента. Выход: 4 jsonl + normalization-report + zip. Всё ещё **candidate, НЕ импорт**.
3. **Rounds 3..N (Фазы 1-4, ~116 unit-раундов):** для КАЖДОГО source unit — license gate → provenance record
   (URL+fetch date+revision) → fetch → прочитать unit целиком → написать per-node A→B пары (Node 1-9) с
   rephrasing (каждая пара ≥2 раза, разный угол) → симуляция unit'а → запись в coverage ledger. Именно ЗДЕСЬ
   candidate-строки становятся настоящими node-pairs, которых сейчас **0** (Pair-format matches = 0).
4. **Promotion-gate:** в локальные learning-sets импортируются только пары, прошедшие validation-checklist
   плана — B ПОКАЗЫВАЕТ выход (не описывает его); массивные ноды (2/4/5/6) написаны как массивы «один юнит =
   один элемент»; arbiter имеет ≥1 pass + ≥1 flag; formatter ≥2 audience; лицензия permissive или
   attribution-соблюдена; каждый unit есть в coverage ledger со статусом.
- **Ключевой вывод:** HOLD держится ФОРМАТОМ (0 node-pairs, 0 rephrasing, markdown вместо JSON) и ПРОВЕНАНСОМ
  (160 несводимых id, реестр≠факт из прохода 6), а НЕ ЛИЦЕНЗИЯМИ — юр-трение почти нулевое (2 GPL hard-block +
  ~7 share-alike/MPL на 1500 строк, остальное permissive). То есть до promotion нужен инженерный ремонт
  (нормализация+пары), а не переговоры о правах.

### Что остаётся на проходы 8-9-10
- **Проход 8:** арбитр-словарь — свести ad-hoc `*-gate`/persona-арбитров (поздние батчи, §3.Б прохода 6) к
  каноническим гейтам; сверить `trainingRows` каждого из 19 каталожных арбитров (проход 4) с телами батчей.
- **Проход 9:** связка Round-002-нормализации с master-ledger (RAG-05) — точный held/blocked-список ЭТОГО
  раунда: 542 hold/policy (в т.ч. 75 незарегистрированных источников из прохода 6) + 2 GPL + share-alike +
  MPL; при необходимости — построчная выкладка 160 id-residual по семьям id.
- **Проход 10:** сплошная сверка полноты всех 98 файлов + executive summary для плана продолжения расширений
  Visual Studio / VS Code.
- RAG file: `C:/Users/lubao/Documents/xiigen/llm mvp/visual studio & visual studio code continue/RAG-07-external-sources.md`

---

## Pass 8/10 — арбитры: полная сверка словарей + покрытие seed-строками + готовые quality-gates

> Проход 8 закрывает «арбитровую нить», начатую в проходах 3/4/6. Метод: сплошной grep поля
> `"arbiters"` во ВСЕХ 75 телах батчей (Python, не выборка), извлечение каждого имени из каждого
> массива `arbiters[...]`, классификация против канона-19 (каталог `05`, проход 4) и 27 имён
> экзаменаций (проход 3), и сверка `trainingRows` каждого каталожного арбитра с реальными
> строками батчей. Плюс проверка, несут ли seed-строки настоящую accept/reject-рубрику.
> **Три доказанных вывода этого прохода:**
> 1. **Поле `arbiters` деградировало катастрофически:** 1320 сериализованных строк несут
>    **2530 имён-слотов = 2004 РАЗЛИЧНЫХ имени** в поле `arbiters`. Канонические 19 гейтов
>    встречаются лишь в **28 строках из 1320 (2%)** — фактически только в своих seed-строках.
>    Остальные ~1981 имён — это 6 несовместимых «псевдо-словарей» (persona, source-id,
>    свободные теги качества), а НЕ гейты. Поле стало мешком свободного текста без правил accept/reject.
> 2. **Seed-покрытие канона-19 — идеальное (100%):** все 20 `trainingRows` каталога (EXT-SEC-001..008/011,
>    EXT-ARCH-001..007, EXT-DATA-001/002, EXT-PROP-001/002) физически есть в теле батча `001-030`, и
>    каждая seed-строка ровно совпадает со своим каноническим арбитром и несёт полную DPO-рубрику
>    (`acceptedOutput`/`rejectedOutput`/`whyAccepted`/`whyRejected`/`sourceAnchors`). → 19 канонических
>    арбитров = **готовые production-rubric quality-gates**.
> 3. **8 несериализованных арбитров экзаменаций расщепились на 4+4:** 4 из них (`fileUploadArbiter`,
>    `eventSourcingFitArbiter`, `backwardCompatibilityArbiter`, `sagaCompensationArbiter`) имеют реальные
>    accept/reject-строки в батчах (EXT-099, EXT-038, EXT-040/041, EXT-043), но НЕ имеют машинной рубрики
>    в каталоге → **полу-готовы (восстановимы)**. Остальные 4 (`dependencyCycleArbiter`,
>    `queueLoadLevelingArbiter`, `eventStoreImmutabilityArbiter`, `specAttributePromotionArbiter`)
>    живут ТОЛЬКО в прозе экзаменаций — ни каталога, ни строки → **чистые имена, не пригодны**.

### 1. Числовая база прохода (grep-подтверждено)

- Тела батчей сериализуют **1320 строк** (по границе `"rowId"`), НЕ 1500 — разрыв 180 строк с ledger
  (COVERAGE-LEDGER=1500), это тот же «дрейф батчей», что в проходе 2 (bodies беднее ledger).
- **Каждая** из 1320 строк несёт непустое поле `arbiters` → **2530 имён-слотов**, в среднем ≈1.9 имени/строку.
- **Различных имён в поле `arbiters` = 2004.** Это и есть масштаб деградации словаря.
- Строк, где `arbiters` состоит ТОЛЬКО из канона-19: **27**. Строк, где есть ≥1 канонический арбитр: **28**.
  То есть канон-19 практически не переиспользован за пределами своих seed-строк.

### 2. Полная сверка словарей: канон / полу-канон / дрейф (2004 различных имени)

Классификация по имени (сумма distinct = ровно 2004; сумма occ = 2530):

| Класс словаря | Пример | distinct | occ (слотов) | Это гейт? |
|---|---|---:|---:|---|
| **CANONICAL-19** (каталог `05`) | `jwtValidationArbiter`, `circuitBreakerArbiter` | **19** | 28 | **ДА — рубрика+seed** |
| **EXAM-8, есть в батчах** (полу-канон) | `fileUploadArbiter`, `sagaCompensationArbiter` | **4** | 5 | частично (есть строка, нет рубрики) |
| ad-hoc camelCase `*Arbiter` | `implementationCorrectnessArbiter`×7, `resultTranslationArbiter`×6, `cweComplianceArbiter`×3 | 225 | 264 | нет (имя-гейт без рубрики) |
| ad-hoc kebab `*-arbiter` (СЕМЕЙНЫЕ) | `security-arbiter`×34, `execution-applier-arbiter`×28, `registry-storage-arbiter`×22, `topology-arbiter`×15, `learning-arbiter`×13 | 179 | 319 | нет (уровень СЕМЬИ, не проверки) |
| **persona** `*-expert/-engineer/-architect/-board/-lead/-manager` | `api-design-expert`×18, `security-engineer`×17, `python-expert`×11, `security-review-board`×5 | 165 | 376 | нет (роль/панель, не rubric) |
| **SOURCE-ID leak** (ALLCAPS) | `ASPNETCORE-DOCS`×7, `OWASP-ASVS`×5, `CWE-TOP25`×5, `PYTHON-PEPS`×5 | 95 | 187 | нет (id источника попал не в то поле) |
| ad-hoc gate-descriptor | `type-safety`, `defense-in-depth`, `round-trip-count`, `backward-compatibility` | 122 | 137 | нет (аспект, не арбитр) |
| **OTHER — свободные теги качества** | `boilerplate-reduction`×5, `testability`, `readability`, `z-index-stacking`, `w3c-propagation` | 1195 | 1214 | нет (свободный текст) |

**Итог сверки:** из 2004 имён только **19 — настоящие канонические гейты** (каталог+seed+рубрика),
**+4 полу-канона** (строка есть, рубрики нет). Остальные **~1981 (99%) — не гейты**: три «псевдо-словаря»
из прохода 6 подтверждены и уточнены до **шести**: (а) ad-hoc camelCase-арбитры, (б) kebab-семейные
`*-arbiter` (это НАЗВАНИЯ СЕМЕЙ 5/1/4/8, а не проверки), (в) persona-эксперты, (г) review-`*-board` панели,
(д) утёкшие source-id, (е) свободные теги качества. Поле `arbiters` в 1292 из 1320 строк — это по сути
свободный «список аспектов качества», из которого невозможно вывести машинное правило accept/reject.

### 3. Покрытие арбитр → seed-строка (`trainingRows`)

Каждый из 19 каталожных арбитров объявляет `trainingRows` = EXT-* id. Все 20 id найдены в теле `001-030`,
и `arbiters`-поле каждой seed-строки ТОЧНО совпадает со своим арбитром:

| Арбитр (канон) | seed rowId | acc/rej-рубрика в строке |
|---|---|---|
| jwtValidationArbiter | EXT-SEC-001 | да (REJECT alg:none → 401) |
| oauthGrantArbiter | EXT-SEC-002 | да |
| passwordStorageArbiter | EXT-SEC-003 | да |
| sqlInjectionArbiter | EXT-SEC-004 | да |
| secretPresenceArbiter | EXT-SEC-005 | да |
| tenantIsolationArbiter | EXT-SEC-006 | да |
| tlsConfigArbiter | EXT-SEC-007 | да |
| supplyChainArbiter | EXT-SEC-008 **и** EXT-SEC-011 | да (2 seed-строки) |
| circuitBreakerArbiter | EXT-ARCH-001 | да |
| bulkheadIsolationArbiter | EXT-ARCH-002 | да |
| tracePropagationArbiter | EXT-ARCH-003 | да |
| apiVersioningArbiter | EXT-ARCH-004 | да |
| longRunningOpArbiter | EXT-ARCH-005 | да |
| idempotentCreateArbiter | EXT-ARCH-006 | да |
| stranglerMigrationArbiter | EXT-ARCH-007 | да |
| cqrsFitArbiter | EXT-DATA-001 | да |
| transactionalOutboxArbiter | EXT-DATA-002 | да |
| proposalCompletenessArbiter | EXT-PROP-001 | да |
| protocolMigrationArbiter | EXT-PROP-002 | да |

**Покрытие канона-19 seed-строками = 20/20 id = 100%** (supplyChain несёт 2 seed). Пропущены только id
EXT-SEC-009/010 (не существуют — supplyChain прыгает 008→011; это НЕ дыра, а нумерация каталога).
Формат seed-строки (пример EXT-SEC-001): полноценная DPO-пара — `rowId`, `sourceIds`, `datasetFamily`,
`domain`, `subdomain`, `architectureChallenge`, `input{a,b,c}`, `function`, `acceptedOutput`,
`rejectedOutput`, `arbiters`, `whyAccepted`, `whyRejected`, `sourceAnchors` (с якорями ASVS V3.5.1 / RFC §6),
`licenseConstraint`, `trainingStatus:candidate`. Это уже готовое к обучению правило, а не заготовка.

### 4. Production-ready quality-gate shortlist (для code-review-способности расширения VS/VSCode)

Критерий готовности: **машинная рубрика (checks/positiveSignals/negativeSignals в каталоге `05`) + реальная
seed-строка с acceptedOutput/rejectedOutput/whyAccepted/whyRejected/sourceAnchors + tier-1 лицензионный
источник.** Это ровно то, что нужно расширению для пути «документируй код + флагни проблему».

**TIER 1 — полностью готовы (19 гейтов), прямо переиспользуемы:**
- **Security-review, 7 (ядро дефицитной семьи 5, наивысшая ценность для код-ревью):** `jwtValidation`,
  `oauthGrant`, `passwordStorage`, `sqlInjection`, `tenantIsolation`, `tlsConfig`, `secretPresence`.
  Каждый — готовый детектор «правильно vs правдоподобно-но-дыряво» (напр. alg:none → REJECT до проверки подписи).
- **Architecture-review, 5:** `circuitBreaker`, `bulkheadIsolation`, `longRunningOp`, `idempotentCreate`, `stranglerMigration`.
- **Data-review, 2:** `cqrsFit`, `transactionalOutbox`.
- **Lifecycle/reprocessing (семья 8), 4:** `apiVersioning`, `stranglerMigration`, `protocolMigration`, `supplyChain`.
- **Proposal-review, 1:** `proposalCompleteness`.
(strangler/supplyChain учтены и в архитектуре/security, и в семье 8 — они `5+8`.)

**TIER 2 — полу-готовы, восстановимы (4 гейта):** `fileUploadArbiter` (EXT-099, семья 5),
`eventSourcingFitArbiter` (EXT-038, семья 8), `backwardCompatibilityArbiter` (EXT-040/041, семья 8),
`sagaCompensationArbiter` (EXT-043, семья 8). Реальные accept/reject-строки в батчах ЕСТЬ, отсутствует только
машинная рубрика каталога → материализовать рубрику ИЗ уже написанной строки (малый ремонт, не сбор с нуля).

**TIER 3 — только имена, не пригодны (4):** `dependencyCycleArbiter`, `queueLoadLevelingArbiter`,
`eventStoreImmutabilityArbiter`, `specAttributePromotionArbiter` — ни каталога, ни строки; существуют лишь в
прозе экзаменаций. Для использования нужно писать и рубрику, и seed-строку заново.

**TIER 4 — НЕ гейты (~1981 имён):** persona-эксперты, review-`*-board`, утёкшие source-id, свободные
теги качества, kebab-семейные и ad-hoc camelCase имена. Это шум в поле `arbiters`. **Их нельзя импортировать
как quality-authorities** — иначе у «flag issue» будет 2004 псевдо-авторитета без единого правила accept/reject.

### 5. Смысл для плана продолжения расширений (арбитр-закрытие)

- **Готовый ровно сейчас переиспользуемый quality-слой code-review = 19 (Tier 1) + 4 восстановимых (Tier 2) = 23 гейта**
  со своими seed-строками. Это прямой каркас «code-review capability» расширения: детектор + правило + якорь источника.
- **Обязательный шаг нормализации ПЕРЕД импортом:** к license-нормализации (проход 5) и provenance-сшивке
  (проход 6) добавляется **ARBITER-VOCABULARY нормализация**: свести поле `arbiters` из 2004 имён к 23 гейтам —
  каждую из 1292 дрейфующих строк либо смаппить на один из 23 канон/полу-канон гейтов, либо пометить как
  «нет гейта» и не пускать в quality-слой. Без этого промоушен-гейт плана («arbiter имеет ≥1 pass + ≥1 flag»)
  формально пройдёт на мусорных именах.
- **Разрыв материализации (из прохода 4) уточнён:** достроить каталог с 19 до 27 = материализовать 4 рубрики
  Tier 2 (строки уже есть) + написать 4 гейта Tier 3 с нуля. Это конкретный, посчитанный backlog (4+4), а не «~17 недостающих».

### Честная строка покрытия (проход 8/10)
Прочитано/просканировано ПОЛНОСТЬЮ: все 75 тел батчей (grep поля `arbiters` по каждому массиву — 2530 слотов,
2004 distinct, подтверждено Python-подсчётом), `05-ARCHITECTURE-ARBITER-CATALOG-v1.json` (20 `trainingRows`),
4 файла `02-source-examinations/` (27 имён арбитров, grep-подтверждено), seed-строки EXT-SEC/ARCH/DATA/PROP в
теле `001-030` и полу-канон-строки EXT-038/040/041/043/099 в `031-060`/`121-150` (проверено наличие
acceptedOutput/rejectedOutput). Числа: rows=1320, arbiter-slots=2530, distinct=2004, канон-в-строках=28,
seed-покрытие канона=20/20. НЕ трогались более ранние секции RAG. Вывод прохода:
**поле `arbiters` деградировало до 2004 псевдо-имён, но канон-19 несёт 100% seed-покрытие с полной DPO-рубрикой;
готовый quality-слой code-review = 23 гейта (19 готовых + 4 восстановимых); 99% имён в поле — шум, требующий
vocabulary-нормализации до импорта.**

### Что остаётся на проходы 9-10
- **Проход 9:** связка Round-002-нормализации с master-ledger (RAG-05 legal gate) — точный held/blocked-список
  ЭТОГО раунда (542 hold/policy + 75 незарегистрированных источников + 2 GPL + share-alike/MPL), и как в
  Round-002 встроить ARBITER-VOCABULARY нормализацию (маппинг 2004→23) рядом с license/format-нормализацией.
- **Проход 10:** сплошная сверка полноты всех 98 файлов + executive summary для плана расширений VS/VSCode
  (спецификация code-review capability: 23 гейта, их seed-строки, backlog материализации 4+4).

---

## Pass 9/10 — синтез: легальный корпус, 23 quality-gate, HOLD→ready, cross-folder, вопросы

> Проход 9 НЕ индексирует новых файлов (это сделали проходы 1-8, все 98 учтены). Это СИНТЕЗ:
> собрать из восьми проходов ОДНУ действенную картину для плана продолжения расширений
> Visual Studio / VS Code + мосты к соседним папкам (RAG-03/05/06/04) + список решений для Luba.
> Всё read-only, кроме этого RAG-файла. Одна человеческая фраза-заголовок: **эта папка — не готовый
> корпус, а склад полу-сырья с ОДНИМ немедленно готовым продуктом (23 quality-gate для code-review)
> и большим, но МЕХАНИЧЕСКИМ, ремонтом до промоции; главный блокер — формат и провенанс, НЕ лицензии.**

### 1. Синтез папки-7 — что она даёт плану «продолжить расширения»

**(a) Что это.** Легальный слой обучения общей модели: внешние ОТКРЫТО-ЛИЦЕНЗИРОВАННЫЕ технические
источники, превращённые в кандидатные обучающие строки. Масштаб (свод проходов 1/4/5/6):
- **реестр v1 = 32 источника** (27 ACCEPT / 5 VERIFY-BEFORE-USE-BLOCK) — это ДЕКЛАРАЦИЯ;
- **фактически задействовано 97 источников** (ledger), из них 22 зарегистрированных дали 805 строк,
  **75 незарегистрированных дали 695 строк (46%)**, а 10 записей реестра дали 0 строк (5 из них ACCEPT);
- **1500 кандидатных строк** (1498 произведённых + 2 BLOCK) по **8 доменам XIIGEN (A-H)** и **8 dataset-
  семьям потока**; собрано ради двух дефицитных семей — 5 (арбитраж, baseline ≈8 → стало 352) и
  8 (reprocessing/rollback, baseline ≈6 → стало 172);
- отдельно: **19 сериализованных арбитров** (каталог `05`, 14 семья-5 + 5 семья-8) и **SWE-BENCH = 2294
  issue→patch пары** (MIT) — крупнейший источник learning-signal.

**(b) Реальность — `HOLD`, готово 12%.** Аудит round-001 (read-only) вынес вердикт **HOLD: «candidate
material only. Not ready for local learning-set import»**. Контент НАСТОЯЩИЙ и плотный (реальный код,
числа, ссылки на стандарты — не заглушки), но:
- **формат не тот:** `PairFormatMatches: 0`, `RephrasePairsFound: 0` — строки в форме DPO
  `acceptedOutput/rejectedOutput`, а НЕ финальные node-пары `A→B` + обязательная перефразировка
  `Phrasing-B`, которых требует план обучения. Это САМЫЙ большой разрыв;
- **provenance дрейфует:** 6 форматов `rowId`, 160 несводимых id (batch↔ledger даже после нормализации),
  реестр ≠ факт (32 заявлено vs 97 использовано);
- **сериализация битая:** 106 JSON-ошибок в 7 файлах (все — `acceptedOutput` со встроенным код-фенсом
  ```` ``` ````) + 9 markdown-файлов без fenced-JSON (данные в инлайн-бэктиках);
- **статус:** ready лишь **180/1500 = 12%**; остальное draft/candidate/approved.

**(c) Переиспользуемые активы (что можно взять УЖЕ сейчас):**
1. **23 готовых quality-gate для code-review** (проход 8) — прямой каркас «документируй код + флагни
   проблему» для расширения: **19 Tier-1** (полная машинная рубрика checks/positive/negative + seed-строка
   с acc/rej/whyAccepted/whyRejected/sourceAnchors, seed-покрытие канона = 20/20 id = 100%) + **4 Tier-2
   восстановимых** (строка есть, дописать рубрику). Разбивка Tier-1: security-review 7 (jwtValidation,
   oauthGrant, passwordStorage, sqlInjection, tenantIsolation, tlsConfig, secretPresence — ядро семьи 5),
   architecture-review 5, data-review 2, lifecycle/семья-8 4, proposal-review 1.
2. **Чистый examination-слой** (проход 3): 4 файла `02-source-examinations/` — провенанс-полный
   первоисточник (challenge→accepted/rejected+rationale, лицензия названа per-source). «Правильная форма»
   существует ЗДЕСЬ; порча возникла позже, при массовой генерации 1500 строк.
3. **SWE-BENCH 2294 пары** (MIT, tier-1) — единственный крупный learning-signal (домен G, семьи 2/3/4/6/8).
4. **Легальный список источников** — 686/695 незарегистрированных permissive; ядро без трения:
   NIST-SSDF (public-domain), Apache-2.0-группа (KEPS/OTel/OpenAPI/Kafka/CloudEvents/Docker), MIT-группа
   (SWE-bench/Rust-RFCs/NestJS).

**(d) Путь HOLD→ready (проход 7) — инженерный ремонт, не переговоры о правах.** Round-002 (следующий,
без импорта, статус candidate сохраняется) нормализует 1500 строк в 4 jsonl:
`canonical-candidate-rows` ∨ `held-license-review` ∨ `blocked-license-rows` ∨ `malformed-held-for-repair`
(+ `normalization-report.md` + zip). Конкретная очередь ремонта:
- **лицензии — механически:** 451 строка с общим тегом (`attribution`/`notice`/`none`) разрешается
  копированием соседнего поля `license` (0 неразрешённых); 2 GPL → blocked;
- **JSON — механически:** нейтрализовать встроенные код-фенсы в `acceptedOutput` (106 ошибок) + извлечь
  инлайн-бэктик-JSON из 9 markdown-файлов; контент сохраняется;
- **provenance — сшить 160 id-residual** (hard-rule «preserve rowId + source»); mojibake (`â†’`,`Ã¡`) почистить;
- **арбитр-словарь (проход 8) — свести `arbiters` из 2004 имён к 23 гейтам** (иначе промо-гейт «arbiter
  имеет ≥1 pass + ≥1 flag» пройдёт на мусорных именах);
- **формат (Rounds 3..N, ~116 unit-раундов, 4 фазы)** — написать per-node пары `A→B` + rephrasing:
  ЗДЕСЬ candidate-строки становятся настоящими node-парами (сейчас их 0). Это самый большой кусок работы.

**(e) Чего НЕТ (подтверждает RAG-03).** Готового лицензированного ПАРАЛЛЕЛЬНОГО корпуса перевода здесь
НЕТ. Tatoeba присутствует как **65 decision-строк** (инженерные пары «как СТРОИТЬ EN-AR корпус: LASER-
фильтр, bidirectional verification» — EXT-1384) + 2 сырые CC-BY-2.0 пары. Это решения О сборке корпуса,
а не сам корпус. `verified-licensed = 0` из RAG-03 подтверждён на уровне содержимого.

### 2. Cross-folder мосты — что папка-7 берёт у соседей и что им даёт

| Соседняя папка | Что папка-7 ДАЁТ | Что папке-7 НУЖНО / открытое напряжение |
|---|---|---|
| **RAG-05 (dpo legal build)** | Реестр 32 + license-distribution 1500 строк — это **ВХОД** легального teacher-gate. Точный held/blocked-список: 75 незарегистрированных источников + 2 GPL (EXT-191 Ansible, EXT-216 PlantUML). | Gate живёт там: класс `TeacherProvenanceGateUnit.Evaluate` (8 шагов), `TeacherProvenance` = **20 полей**, `provenance_field_presence_rate==1.0` — но **статус PLAN-ONLY (типов нет ни в одном `.cs`/`.jsonl`)**. ⚠ **ЖЁСТКОЕ НАПРЯЖЕНИЕ:** политика RAG-05 **ЗАПРЕЩАЕТ CC-BY-SA и GPL/AGPL целиком**, а здешний плотнейший арбитр-источник **OWASP-ASVS = CC-BY-SA-4.0** (6 из 19 гейтов!) + OWASP-Cheat-Sheets — тоже SA. По строгому gate RAG-05 ядро security-семьи-5 БУДЕТ ЗАБЛОКИРОВАНО. Локальная политика папки-7 (`06`-doc §2) их ДОПУСКАЕТ как `ATTRIBUTION_SHAREALIKE` «держать отдельно». Это прямое противоречие двух политик — решение за Luba (вопрос 4). |
| **RAG-03 (language-translate)** | Ничего готового: параллельного корпуса здесь нет (см. §1e). | Пилоту `27.TranslationDomain` нужен НАСТОЯЩИЙ лицензированный Tatoeba/UN корпус с per-row locator; здесь — только SYNTHETIC_SEED-решения. Папка-7 НЕ закрывает потребность перевода → корпус надо строить отдельно (реестр RAG-03: TR-S01 Tatoeba, TR-S02 ParaNames, TR-S04 UN). |
| **RAG-06 (llm dynamic flow)** | **8 dataset-семей = рёбра pipeline** динамического потока; proposal-архивы (RFC/PEP/KIP) — прямое сырьё для дефицитных семей 5/8; **23 арбитр-гейта = quality-слой потока** (arbiter-selector→arbiter-array→verdict-boundary, узлы N13-N16; arbiter-KNN-authority используется дважды: до генерации как память, после — как hard-verdict). | Архитектура/контракты потока (ILearningDynamicGraph) описаны там; папка-7 наполняет её quality-узлы конкретными rubric-гейтами. Здешние 2004 псевдо-имени в поле `arbiters` надо свести к 23, иначе у verdict-boundary потока не будет единого правила accept/reject. |
| **RAG-04 (design-to-code / visual-judge)** | Те же **security/architecture-review гейты** — переиспользуемый quality-слой для wiki code-review; тип «arbiter = authority» тот же, что visual-judge/render-QA в design-conversion. | Домен design-conversion — единственный реальный `DesignConversion*`/DPO/visual-judge контур, но «обучение» там = усреднитель `_weightedSum/ObservationCount` (фейк). Папка-7 даёт готовые rubric-детекторы, которых тому контуру не хватает для настоящего quality-суждения. |

### 3. Открытые вопросы для Luba (с дефолтами)

1. **Round-002 сейчас или позже?** Дефолт: **сейчас** — это единственный шаг-блокер, аудит (read-only)
   уже сделан, ремонт лицензий/JSON механический. Риск отложить: корпус остаётся в HOLD неограниченно.
2. **Арбитр-словарь 2004→23: маппить или дропать «хвост»?** Дефолт: **построить канонический словарь из 23
   гейтов**, каждую из 1292 дрейфующих строк смаппить на один из 23 либо пометить «нет гейта» и не пускать
   в quality-слой. Не импортировать 1981 псевдо-имя как authority.
3. **75 незарегистрированных источников: зарегистрировать+вердикт или дропнуть?** Дефолт: **зарегистрировать**
   (686/695 permissive — механически проставить `licenseUseClass` в реестр v2); блокировать только 2 GPL.
4. **2 GPL + 6 share-alike + 1 MPL — как быть?** Дефолт: **2 GPL удалить/заменить** (hard-block: Ansible EXT-191,
   PlantUML EXT-216). **Share-alike (6 строк: Diataxis 3, Wiktionary 2, Scrum-Guide 1) + MPL (Terraform 1)** —
   ТРЕБУЮТ явного решения из-за противоречия RAG-07↔RAG-05 (вопрос 2): держать отдельно/помечать (политика
   папки-7) ИЛИ выбросить (строгий gate RAG-05 запрещает CC-BY-SA). **Критично:** то же решение определяет
   судьбу зарегистрированного **OWASP-ASVS (CC-BY-SA, 6 из 19 гейтов)** — если строгий gate победит,
   ядро security-review надо переносить на CC-BY/permissive-эквиваленты (CWE/IETF/NIST перекрывают частично).
5. **Формат A→B / Phrasing-B — обязательный трансформ?** Дефолт: **да, обязателен до импорта** (сейчас пар 0);
   выполняется в ~116 unit-раундах фаз 1-4 continuation-плана. Это самый большой объём работы; альтернатива —
   импортировать только уже-node-парную подвыборку (её сейчас нет → ноль).
6. **Использовать этот корпус для общей модели вообще — или взять меньший курированный срез?** Дефолт:
   **сначала курированный срез** — 23 quality-gate (с seed-строками) + SWE-bench 2294 как learning-signal;
   массовые 1500 строк отложить до Round-002+формат-трансформа. Это даёт расширению работающий code-review
   слой быстро, не дожидаясь полной нормализации.

### Честная строка покрытия (проход 9/10)
Синтез собран из уже-прочитанных проходов 1-8 этого RAG (числа: 98 файлов, реестр 32=27ACCEPT/5BLOCK,
97 источников/75 незарег./695 строк, 1500 строк, ready 180=12%, 106 JSON-ошибок+9 md-файлов, 160 id-residual,
2 GPL+6 SA+1 MPL, арбитры 19 каталог/23 готовых/2004 псевдо-имени, SWE-bench 2294) — все взяты из
зафиксированных выше секций, не переизмерялись. Cross-folder мосты сверены прямым чтением соседних RAG:
RAG-03 (`verified-licensed = 0`, TR-S01 Tatoeba SYNTHETIC_SEED, реестр TR-S01/02/04), RAG-05 (`TeacherProvenance`
20 полей, `TeacherProvenanceGateUnit.Evaluate` 8 шагов, PLAN-ONLY, запрет CC-BY-SA/GPL — источник напряжения
с OWASP-ASVS), RAG-06 (8 семей=рёбра, arbiter-selector/verdict-boundary N13-N16, arbiter-KNN dual-use), RAG-04
(design-conversion visual-judge, `_weightedSum/ObservationCount` фейк-обучение). Новых файлов папки-7 в этом
проходе не читалось (синтез, не индексация); ранние секции RAG не изменялись — только добавлена эта секция.
Вывод прохода: **папка-7 = склад полу-сырья; 1 готовый продукт (23 quality-gate для code-review) + SWE-bench;
HOLD держится форматом (0 node-пар) и провенансом (160 id, реестр≠факт), НЕ лицензиями (hard-block = 2 GPL);
переводного параллельного корпуса нет (подтверждает RAG-03); главное решение для Luba — судьба CC-BY-SA
(OWASP-ASVS) из-за противоречия локальной политики папки-7 и строгого legal-gate RAG-05.**

---

## Pass 10/10 — completeness sweep + executive summary (folder #7/12 external-sources)

> Финальный проход. НЕ индексирует новых файлов — все 98 учтены проходами 1-9. Здесь два дела:
> (1) СПЛОШНАЯ СВЕРКА ПОЛНОТЫ — перечитан весь RAG-файл и сверен против физического манифеста папки
> (Glob всех файлов): каждый файл/группа помечен COMPLETE или PARTIAL с честным остатком, ни один файл
> не заброшен; (2) EXECUTIVE SUMMARY на русском — чтобы Luba могла прочитать ТОЛЬКО эту секцию и понять,
> что папка «external-sources» даёт плану продолжения расширений Visual Studio / VS Code. Всё read-only,
> кроме этого RAG-файла; ранние секции не тронуты.

### 1. Сплошная сверка полноты — все 98 файлов (сверено Glob ↔ RAG проходов 1-9)

**Реконсиляция счёта (честно):** физический Glob по `external-sources/**/*` даёт ровно **98 файлов**
(+ 1 пустая директория `domain-files/`, 0 файлов). Разбивка: **6 верхнеуровневых документов** (5 нумерованных
`00/01/03/05/06` + `COVERAGE-LEDGER.jsonl`) + **4** экзаменации + **75** батчей + **13** round-zip файлов
(1 `.zip` + 12 распакованных). 6 + 4 + 75 + 13 = **98** ✔. Замечание о формулировке задания: «6 top docs +
COVERAGE-LEDGER» дало бы 99 — это двойной счёт: `COVERAGE-LEDGER.jsonl` и есть один из 6 верхнеуровневых
(в таблице манифеста прохода 1 он шестой). Физическая истина = 98, и она сходится с Glob точно.

**Таблица полноты по группам:**

| Группа | Файлов | Статус | Где покрыто (проход) | Честный остаток |
|--------|-------:|--------|----------------------|-----------------|
| Верхнеуровневые: `00-CONTEXT-LOAD-REPORT.md` | 1 | **COMPLETE** | п.1 (полностью) + п.3 (глубже: 8 семей, разрыв F5/F8) | нет |
| `01-SOURCE-REGISTRY-...v1.json` | 1 | **COMPLETE** | п.4 построчно, все 32 записи (grep `sourceId`=32) | нет |
| `03-OPEN-SOURCE-KNOWLEDGE-DOMAIN-MAP-v1.md` | 1 | **COMPLETE** | п.1 + п.3 (карта источник→домены A-H, матрица) | нет |
| `05-ARCHITECTURE-ARBITER-CATALOG-v1.json` | 1 | **COMPLETE** | п.4 построчно, все 19 арбитров (grep `arbiterId`=19) | «~36» манифеста = завышение; факт 19 |
| `06-FINAL-DOCUMENT-STRUCTURE-v1.md` | 1 | **COMPLETE** | п.5 целиком (§1-8, политика ACCEPT/REJECT) | §4-8 = ранний снимок прогресса (историчен) |
| `COVERAGE-LEDGER.jsonl` (1500 строк) | 1 | **COMPLETE** | п.5 и п.6: все 1500 распарсены Python, 0 ошибок, 0 дублей | авторитетный row-level учёт |
| `02-source-examinations/` | 4 | **COMPLETE** | п.3 все 4 целиком (7 разделов каждый) | чистый провенанс-слой, «правильная форма» |
| `batches/` (`ext-batch-*`) | 75 | **COMPLETE** (sampled-fully-by-script) | см. ниже — 4 независимых полнокорпусных прохода + ledger | ~63 тела не читаны построчно (учтены скриптом+ledger) |
| round-zip: `00-ROUND-MANIFEST.md` | 1 | **COMPLETE** | п.1 + п.7 (вердикт HOLD, 8 причин) | нет |
| round-zip: `01-FILES-INCLUDED.md` | 1 | **COMPLETE** | п.7 (список пакета раунда) | нет |
| round-zip: `02-VALIDATION-RESULTS.md` | 1 | **COMPLETE** | п.1/2/5/7 (все счётчики валидации) | нет |
| round-zip: `03-NEXT-ROUND-PROMPT.md` | 1 | **COMPLETE** | п.2 + п.7 (промпт Round-002, 4 jsonl-выхода) | нет |
| round-zip `evidence/`: `batch-audit-summary.json` | 1 | **COMPLETE** | п.1/2/5 (сводка HOLD) | нет |
| round-zip `evidence/`: `batch-file-summary.csv` (75 строк) | 1 | **COMPLETE** | п.2 все 75 строк (по файлу: Expected30 vs Total20) | нет |
| round-zip `evidence/`: `dataset-family-distribution.csv` | 1 | **COMPLETE** | п.1/2/5 (F5=288 batch vs 352 ledger) | нет |
| round-zip `evidence/`: `domain-distribution.csv` | 1 | **COMPLETE** | п.1/2/5 (E densest) | нет |
| round-zip `evidence/`: `license-distribution.csv` | 1 | **COMPLETE** | п.1/2 (легальная картина 1500) | нет |
| round-zip `evidence/`: `status-distribution.csv` | 1 | **COMPLETE** | п.1/2 (ready 180=12%) | нет |
| round-zip `evidence/`: `invalid-json-examples.txt` | 1 | **COMPLETE** | п.2/5 (выборка 80/106, 100% «Unterminated string») | образец 80 из 106 (per-file класс подтверждён) |
| round-zip `files/`: `NO-SOURCE-FILES-CHANGED.txt` | 1 | **COMPLETE** | п.1 + п.7 (маркер read-only аудита) | нет |
| round-zip: `external-learning-round-001-batch-audit.zip` | 1 | **COMPLETE** (контейнер) | содержимое = 12 распакованных файлов выше, все прочитаны | сам `.zip` бинарно не вскрывался (зеркало-папка прочитана) |
| `domain-files/` (пустая директория) | 0 | **н/п** | заготовка, 0 файлов | нет содержимого |
| **ИТОГО** | **98** | **все учтены** | проходы 1-9 | ни один файл не заброшен |

**Почему 75 батчей = «sampled-fully-by-script, ledger authoritative» (не neglect):** каждый из 75 файлов
учтён ЧЕТЫРЬМЯ независимыми полнокорпусными способами, а не «прочитано 5, остальное угадано»:
1. **`batch-file-summary.csv` — по одной строке на КАЖДЫЙ из 75 файлов** (п.2): заголовок, FirstRowId/LastRowId,
   ExpectedFromFileName=30 vs TotalRowIds=20 — доказывает «имя vs факт» для всех 75.
2. **`COVERAGE-LEDGER.jsonl` — все 1500 произведённых строк распарсены Python** (п.5/6): это авторитетный
   row-level учёт (0 дублей, 0 JSON-ошибок), покрывающий содержимое всех батчей построчно на уровне ledger.
3. **Полнокорпусный grep поля `arbiters` по ВСЕМ 75 телам** (п.8): 2530 слотов, 2004 distinct — не выборка.
4. **Полнокорпусный скан fenced-JSON по ВСЕМ 75 телам** (п.5): подтвердил 9 markdown-файлов без JSON и
   хрупкость извлечения 106-ошибок независимым regex.
Плюс ~10-12 тел прочитаны построчно целиком (п.2: `001-030`, `451-480`, `1231-1260`, `2221-2250`; п.6:
`1621-1650`, `1861-1890` + выборка `2041-2070`). Итог: **ни один батч-файл не заброшен**; ~63 тела не читаны
дословно построчно, но каждое учтено скриптом + ledger + двумя полнокорпусными grep/scan.

**Кросс-проверка честных строк покрытия проходов 1-9:** каждый проход завершается «Честной строкой покрытия»
с указанием, что прочитано ПОЛНОСТЬЮ и что осталось на следующий проход. Цепочка замыкается: остаток каждого
прохода закрыт следующим (п.1→3 экзаменации/карта, п.4 реестр+каталог, п.5 ledger+06+JSON, п.6 97 источников,
п.7 round-audit+continuation, п.8 арбитр-словарь, п.9 синтез+cross-folder). На проход 10 осталась только
сверка полноты — она выполнена выше.

### 2. EXECUTIVE SUMMARY (папка #7/12 «external-sources») — можно читать ТОЛЬКО это

**Что это за папка и что она даёт плану.** «external-sources» — это ЛЕГАЛЬНЫЙ ОБУЧАЮЩИЙ КОРПУС: внешние
открыто-лицензированные технические источники (стандарты, спеки, бенчмарки, доки), превращённые в
**1500 кандидатных обучающих строк** из **97 фактически задействованных источников**, разложенных по
**8 доменам XIIGEN (A-H)** и **8 dataset-семьям** конвейера (те же 8 семей = рёбра динамического потока
RAG-06). Собран корпус РАДИ ДВУХ дефицитных семей общей модели: семьи 5 (арбитраж качества, baseline
внутреннего корпуса ≈8 строк → стало **352**) и семьи 8 (reprocessing/rollback, baseline ≈6 → стало **172**).
Отдельная ценность — **SWE-BENCH = 2294 пары issue→patch** (MIT) как крупнейший источник обучающего сигнала.
Для расширений VS/VSCode это именно тот корпус, на котором общая модель имеет ПРАВО легально учиться
навыкам «документируй код + распознай проблему».

**Реальность — HOLD, готово лишь 12%.** Read-only аудит (round-001) вынес вердикт **HOLD: «candidate material
only. Not ready for local learning-set import»**; статус `ready` лишь у **180 из 1500 строк (12%)**. При этом
контент НАСТОЯЩИЙ и плотный (реальный код, числа RPS/latency, ссылки на ASVS/RFC/CWE — не заглушки).
Блокер — НЕ лицензии, а **ФОРМАТ + ПРОВЕНАНС**: (а) формат — `PairFormatMatches: 0` и `RephrasePairsFound: 0`:
строки в форме DPO `acceptedOutput/rejectedOutput`, а НЕ финальные node-пары `A→B` с обязательной
перефразировкой `Phrasing-B`, которых требует план обучения (это самый большой разрыв); (б) провенанс —
6 форматов `rowId`, **160 несводимых id** (batch↔ledger даже после нормализации суффиксов), **реестр v1 = 32
задекларировано против 97 фактически использованных** (**75 источников не зарегистрированы**, дали 695 строк =
46% корпуса; при этом 10 записей реестра дали 0 строк), плюс 106 JSON-ошибок (все — `acceptedOutput` со
встроенным код-фенсом) в 7 файлах + 9 markdown-файлов без fenced-JSON + сквозной mojibake (`â†’`, `Ã¡`).
Лицензионное трение при этом почти нулевое: на 1500 строк — всего **2 строки GPL-3.0** (hard-block: Ansible
EXT-191, PlantUML EXT-216) + ~7 share-alike/MPL; остальное permissive.

**Переиспользуемые активы (взять можно УЖЕ сейчас).** Главный готовый продукт папки — **23 quality-gate для
code-review**: **19 Tier-1** (полная машинная рубрика `checks/positiveSignals/negativeSignals` в каталоге `05`
+ реальная seed-строка с `acceptedOutput/rejectedOutput/whyAccepted/whyRejected/sourceAnchors`; seed-покрытие
канона = **20/20 id = 100%**) + **4 Tier-2 восстановимых** (строка есть, дописать рубрику). Разбивка Tier-1:
**security-review 7** (`jwtValidation`, `oauthGrant`, `passwordStorage`, `sqlInjection`, `tenantIsolation`,
`tlsConfig`, `secretPresence` — ядро дефицитной семьи 5, напр. `alg:none` → REJECT до проверки подписи),
architecture-review 5, data-review 2, lifecycle/семья-8 4, proposal-review 1. Это прямой каркас «code-review
capability» расширения: детектор + правило accept/reject + якорь в первоисточнике. Дополнительно готовы:
**чистый examination-слой** (4 файла `02-source-examinations/` — провенанс-полный первоисточник, «правильная
форма» ДО порчи массовой генерации), **SWE-BENCH 2294 пары**, и **686/695 незарегистрированных строк —
permissive** (легальное ядро без трения: NIST-SSDF public-domain + Apache-2.0-группа KEPS/OTel/OpenAPI/Kafka/
CloudEvents/Docker + MIT-группа SWE-bench/Rust-RFCs/NestJS).

**Путь HOLD → ready (инженерный ремонт, не переговоры о правах).** Round-002 (следующий, БЕЗ импорта, статус
candidate сохраняется) нормализует 1500 строк в 4 jsonl (`canonical-candidate-rows` ∨ `held-license-review` ∨
`blocked-license-rows` ∨ `malformed-held-for-repair`). Конкретная очередь ремонта, вся механическая кроме
последнего пункта: (1) **лицензии** — 451 строка с общим тегом (`attribution`/`notice`/`none`) разрешается
копированием соседнего поля `license` (0 неразрешённых), 2 GPL → blocked; (2) **JSON** — нейтрализовать
встроенные код-фенсы в `acceptedOutput` (106 ошибок) + извлечь инлайн-бэктик-JSON из 9 markdown-файлов
(контент сохраняется); (3) **провенанс** — сшить 160 id-residual (hard-rule «preserve rowId + source»),
почистить mojibake; (4) **арбитр-словарь** — свести поле `arbiters` из **2004 псевдо-имён к 23 гейтам**
(иначе промо-гейт «arbiter имеет ≥1 pass + ≥1 flag» пройдёт на мусорных именах: канон-19 встречается лишь
в 28 строках из 1320 = 2%); (5) **формат** `A→B` + `Phrasing-B` — самый большой кусок: пишется в ~116
unit-раундах 4 фаз continuation-плана, ЗДЕСЬ candidate-строки становятся настоящими node-парами (сейчас их 0).

**Роль в общей сводке (мосты к соседним папкам).** Папка-7 — это **ВХОД легального teacher-gate RAG-05**
(реестр 32 + license-distribution 1500 строк подаются в `TeacherProvenanceGateUnit`; вердикт HOLD = gate ещё
не пройден). ⚠ **Главное открытое напряжение:** строгая политика RAG-05 **ЗАПРЕЩАЕТ CC-BY-SA и GPL целиком**,
а здешний плотнейший security-источник **OWASP-ASVS = CC-BY-SA-4.0 питает 6 из 19 гейтов** — по строгому gate
RAG-05 ядро security-семьи-5 будет заблокировано, тогда как локальная политика папки-7 (`06`-doc §2) допускает
его как `ATTRIBUTION_SHAREALIKE` «держать отдельно». Это прямое противоречие двух политик — решение за Luba.
Для **RAG-03 (перевод):** готового лицензированного ПАРАЛЛЕЛЬНОГО корпуса здесь НЕТ (Tatoeba = 65 decision-строк
о том, КАК строить корпус + 2 сырые пары; подтверждает `verified-licensed = 0`). Для **RAG-06 (dynamic flow):**
8 семей = рёбра pipeline, а 23 гейта = quality-слой узлов arbiter-selector/verdict-boundary. Для **RAG-04
(design-to-code):** те же security/architecture-гейты — переиспользуемый quality-слой, которого не хватает
фейк-обучению `_weightedSum/ObservationCount`.

**Заголовок 6 открытых вопросов для Luba (детали — проход 9 §3):** (1) Round-002 сейчас или позже (дефолт:
сейчас); (2) арбитр-словарь 2004→23 маппить или дропать хвост (дефолт: построить канон из 23, не импортировать
1981 псевдо-имя); (3) 75 незарегистрированных источников зарегистрировать или дропнуть (дефолт: зарегистрировать,
686/695 permissive); (4) **⭐ судьба CC-BY-SA (OWASP-ASVS) — противоречие RAG-07↔RAG-05** (главное решение); (5)
формат `A→B`/`Phrasing-B` обязателен до импорта (дефолт: да, сейчас пар 0); (6) весь корпус или курированный
срез (дефолт: сначала срез — 23 гейта + SWE-bench 2294, массив отложить до Round-002+формат-трансформа).

**Must-read файлы (если читать только несколько):** `COVERAGE-LEDGER.jsonl` (авторитетный учёт 1500 строк),
`01-SOURCE-REGISTRY-ARCHITECTURE-CHALLENGES-v1.json` (реестр 32 + вердикты licenseUseClass),
`05-ARCHITECTURE-ARBITER-CATALOG-v1.json` (19 готовых quality-gate — прямой каркас code-review),
`06-FINAL-DOCUMENT-STRUCTURE-v1.md` (политика лицензий ACCEPT/REJECT),
`round-zips/.../00-ROUND-MANIFEST.md` + `.../03-NEXT-ROUND-PROMPT.md` (вердикт HOLD + план Round-002),
`02-source-examinations/*.md` (чистый провенанс-первоисточник = «правильная форма»).

### 3. Честный отчёт о покрытии (проход 10/10)

- **Всего файлов в папке:** **98** (+ 1 пустая директория `domain-files/`). Сверено физическим Glob по
  `external-sources/**/*` ↔ манифестом RAG проходов 1-9. Реконсиляция: 6 верхнеуровневых + 4 экзаменации +
  75 батчей + 13 round-zip = 98 (формулировка задания «6 top docs + COVERAGE-LEDGER» = двойной счёт, физически
  COVERAGE-LEDGER и есть шестой верхнеуровневый).
- **Прочитано ПОЛНОСТЬЮ построчно (проходы 1-9):** все 6 верхнеуровневых документов (01 и 05 — с grep-подсчётом
  32/19; 06 — §1-8; COVERAGE-LEDGER — все 1500 строк Python-парсером дважды, п.5 и п.6); все 4 экзаменации;
  все 12 распакованных round-zip файлов (manifest/files-included/validation/next-prompt + 6 evidence + маркер);
  ~10-12 тел батчей целиком.
- **Sampled/scripted (не построчно дословно, но полнокорпусно учтено):** ~63 из 75 тел батчей — каждое покрыто
  `batch-file-summary.csv` (75 строк), полным ledger (1500 строк), полнокорпусным grep поля `arbiters`
  (2530 слотов/2004 distinct, п.8) и полнокорпусным сканом fenced-JSON (п.5). Плюс `.zip` — контейнер, читалась
  его распакованная зеркало-папка. `invalid-json-examples.txt` — образец 80 из 106 (класс ошибки подтверждён 100%).
- **Что НЕ запускалось (это READ-ONLY RAG, не исполнение):** НЕ выполнялись импорт в learning-sets,
  обучение/тренировка, Round-002 нормализация, починка 106 JSON-ошибок, копирование поля `license`→`licenseConstraint`,
  сшивка 160 id-residual, чистка mojibake, трансформ `A→B`/`Phrasing-B`, материализация 4+4 арбитр-рубрик.
  Всё это — будущая работа Round-002+, описанная (не исполненная) в проходах 7-9. Никакие исходные файлы папки
  не менялись (только этот RAG-файл дополнен).
- **folder #7/12 external-sources — RAG COMPLETE.**
