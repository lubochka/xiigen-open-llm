## Актуальный комплект: 60 источников, одна рабочая ветка

[Новые источники событий, атомарности и оценок](DOMAIN-GENERATION-SOURCES-2026-10-25.md) · [RAG-OR-006](sources/RAG-OR-006.md) · [RAG-OR-007](sources/RAG-OR-007.md).

**Текущая предметная навигация:** [SUBJECT-PACK-MAP.md](SUBJECT-PACK-MAP.md). Точный состав источников — 58; обновление добавляет задания и связи, но не объявляет старые выдержки новыми источниками.

> **CREATE_ONLY — текущая навигация:** [CREATE-ONLY-NAVIGATION.md](CREATE-ONLY-NAVIGATION.md). Все 60 выдержок и 30 выбранных main-оригиналов уже доступны в llm_plan3. Генератор читает готовый prompt с абсолютными путями; checksum/review/oracle не являются предварительными действиями создания.

В llm_plan3 рядом с карточками теперь находятся все 60 выбранных выдержок и 30 полных исходных файлов из main. Для подготовки не нужен отдельный checkout main. Полные оригиналы: [originals/INDEX.md](originals/INDEX.md). [Новая запись RAG-DATA-001](sources/RAG-DATA-001.md) — предупреждение против ложного rejected.

`search.py --query ...` работает без сети и без обязательного checksum-прохода. Исторический `--verify` остаётся необязательной диагностикой происхождения. RAG — материал для чтения, а не новые команды исполнителю.

# Индекс RAG для подготовки учебных наборов

Предыдущее дополнение: **57 источников** (исторический состав). [Новое дополнение и ограничения](CONTEXT-UPDATE-2026-10-25.md). Исторические количества ниже относятся к прежним публикациям.
Включены 55 записей и 55 различных путей исходных файлов из двух разрешённых веток: 25 записей из `main`, 30 из `llm_plan3`. Старые 45 сохранены; объединены четыре добавления RAG-OR/RAG-DC и пять входящих RAG-046–RAG-050 от 25.10.2026. Каждый фрагмент имеет commit, blob SHA, строки и пояснение, что допустимо переносить в новые наборы. Полный репозиторий не объявляется прочитанным.

**Сначала:** [расхождения со старыми правилами](CONFLICTS.md), затем [поиск и добавление источников](RETRIEVAL.md). План и карточки находятся в ветке `llm_plan3`, каталог `llm mvp/learning-sets/preparation`.

| ID | Источник | Ветка | Учебное применение |
|---|---|---|---|
| [RAG-001](sources/RAG-001.md) | GUIDE-B11-FLOW-UI-AUTOMATION-JSON.md | `main` | Сценарий UI-автоматизации: действия, ожидания, результат и связь с состоянием. |
| [RAG-002](sources/RAG-002.md) | GUIDE-B04-QA-COVERAGE-STATE-JSON.md | `main` | Проверяемое покрытие поведения и связь QA с требованиями. |
| [RAG-003](sources/RAG-003.md) | GUIDE-B09-UI-REFLECTION-STATE-JSON.md | `main` | Отражение UI и состояния; материал для контрастных пар о UX. |
| [RAG-004](sources/RAG-004.md) | GUIDE-B12-DESIGN-SIMULATION-R1-MD.md | `main` | Симуляция дизайна до реализации. |
| [RAG-005](sources/RAG-005.md) | GUIDE-B17-IMPLEMENTATION-PLAN.md | `main` | План реализации с конкретными зависимостями и ожидаемыми изменениями. |
| [RAG-006](sources/RAG-006.md) | GUIDE-B28-STEP-8-HANDOFF-CONTRACT.md | `main` | Контракт передачи между шагами и артефактами. |
| [RAG-007](sources/RAG-007.md) | XIIGEN-SIMULATION-METHODOLOGY.md | `main` | Методика построения симуляционных ситуаций. |
| [RAG-008](sources/RAG-008.md) | XIIGEN-GAP-SIMULATION-PROTOCOL.md | `main` | Сценарии обнаружения разрывов и проверка отрицательных исходов. |
| [RAG-009](sources/RAG-009.md) | 2026-02-05-XIIGen-CSharp-Skill-Library-and-10Phase-Prompts-Orchestration.md | `main` | Исторический пример этапов оркестрации и C#. |
| [RAG-010](sources/RAG-010.md) | 2026-02-05-Figma-to-Code-Vision-to-First-Skill-Polyglot-Decision.md | `main` | Переход от дизайна к коду и выбор ответственности. |
| [RAG-011](sources/RAG-011.md) | FLOW-PREP-LIBRARY-README.md | `main` | Вход в библиотеку подготовки flow. |
| [RAG-012](sources/RAG-012.md) | GUIDE-B07-RAG-MD.md | `main` | Состав RAG-свидетельств и связь с конкретным решением. |
| [RAG-013](sources/RAG-013.md) | LEARNING-SETS-JSON-README.md | `llm_plan3` | Разделение Markdown-правил, JSON runtime и train-ready представления. |
| [RAG-014](sources/RAG-014.md) | NODE-01-DOMAIN-SPLITTER.md | `llm_plan3` | Разделение содержательных областей и зависимостей. |
| [RAG-015](sources/RAG-015.md) | NODE-02-DOMAIN-TRANSLATOR.md | `llm_plan3` | Перевод технического доменного описания; не перевод естественных языков. |
| [RAG-016](sources/RAG-016.md) | NODE-03-DECOMPOSER.md | `llm_plan3` | Декомпозиция задачи и переход к исполнимым подзадачам. |
| [RAG-017](sources/RAG-017.md) | FLOW-01-REGISTRATION-RUNTIME-TRAINING-DATA-v2.md | `llm_plan3` | Симулируемая регистрация с runtime-входами и результатами. |
| [RAG-018](sources/RAG-018.md) | 00-INDEX.md | `llm_plan3` | Индекс существующих наборов по узлам flow. |
| [RAG-019](sources/RAG-019.md) | node-03-scheme-planner-set.md | `llm_plan3` | Примеры планирования схемы с входом и контрастными результатами. |
| [RAG-020](sources/RAG-020.md) | 00-INDEX.md | `llm_plan3` | Индекс уже включённых в проект материалов; без внешнего скачивания. |
| [RAG-021](sources/RAG-021.md) | CEXP-B0000-LDS-CORR-0001.yaml | `llm_plan3` | Существующая коррекционная пара: форма provenance и причинного различия. |
| [RAG-022](sources/RAG-022.md) | dpo-pairs-supplement-dpo-quality-v197.jsonl | `llm_plan3` | Пример train-ready JSONL; статус старого набора не равен новой проверке. |
| [RAG-023](sources/RAG-023.md) | DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1.md | `llm_plan3` | Правила смысловых контрастов в переводческих DPO. |
| [RAG-024](sources/RAG-024.md) | TRANSLATION-TOKENIZER-GUIDE-v1.md | `llm_plan3` | Разделение языков, токенизации и смысла. |
| [RAG-025](sources/RAG-025.md) | training-pairs-train-ready.jsonl | `llm_plan3` | Существующие переводческие пары как заготовки для повторной проверки. |
| [RAG-026](sources/RAG-026.md) | TRANSLATION-SOURCE-REGISTRY-v3.md | `llm_plan3` | Список уже исследованных переводческих ресурсов внутри проекта. |
| [RAG-027](sources/RAG-027.md) | CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md | `llm_plan3` | Правила пар документации к коду. |
| [RAG-028](sources/RAG-028.md) | CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md | `llm_plan3` | Версионность стека и доказательство переходов вызовов. |
| [RAG-029](sources/RAG-029.md) | CODE-DOC-DPO-REJECTED-PROMPTS-v1.md | `llm_plan3` | Ошибки описаний и контрасты rejected. |
| [RAG-030](sources/RAG-030.md) | CODE-DOC-SOURCE-REGISTRY-v1.md | `llm_plan3` | Навигация по существующим источникам документации. |
| [RAG-031](sources/RAG-031.md) | ORCHESTRATOR-TEACHING-PLAN-v2.md | `llm_plan3` | Обучение оркестратора и разделение ролей. |
| [RAG-032](sources/RAG-032.md) | TEACHING-MEASUREMENT-GUIDANCE-v3.md | `llm_plan3` | Раздельная оценка до/после и следа обучения. |
| [RAG-033](sources/RAG-033.md) | XIIGEN-EXPORT-1000-CASE-GENERATION-GUIDANCE-v1.md | `llm_plan3` | Массовая генерация ситуаций из существующих project export. |
| [RAG-034](sources/RAG-034.md) | DYNAMIC-FLOW-FIRST-FILE-LEARNING-SET-PLAN-v1.md | `llm_plan3` | План стартового обучения на одном файле и последовательных решениях. |
| [RAG-035](sources/RAG-035.md) | XIIGEN_IMPLEMENTATION_ORCHESTRATION.md | `main` | Историческая карта фаз, skills и зависимостей реализации. |
| [RAG-036](sources/RAG-036.md) | XIIGEN-EXPORT-TEACHING-ADDENDUM-v2.md | `llm_plan3` | Разделение типов A-side: compiler, transition, splitter и другие роли. |
| [RAG-037](sources/RAG-037.md) | ADR-ANGULAR-RUNTIME-CONFIG-HTTP-SSR.md | `main` | Существующие TypeScript-примеры: интерфейс runtime config, initializer, единственный AuthSessionService и lazy route array. |
| [RAG-038](sources/RAG-038.md) | ADR-ANGULAR-I18N-RTL-STRATEGY.md | `main` | Готовая конфигурационная иллюстрация provideTransloco, порядок bundle/overlay и изменение lang/dir. |
| [RAG-039](sources/RAG-039.md) | ADR-ANGULAR-VERSION-STANDALONE-STRATEGY.md | `main` | Историческая проектная стратегия standalone components, route arrays и lazy loading. |
| [RAG-040](sources/RAG-040.md) | CODE_Q-Rows.md | `llm_plan3` | Существующие учебные вопросы SQL window vs GROUP BY и Angular Zone; повторные формулировки в том же файле. |
| [RAG-041](sources/RAG-041.md) | node-04-domain-processor-set.md | `llm_plan3` | Встроенные SQL-примеры SELECT JOIN и ALTER TABLE; контекст, success/failure/wait states. |
| [RAG-042](sources/RAG-042.md) | node-03-scheme-planner-set.md | `llm_plan3` | Существующий сценарий atomic order/inventory и реальное расхождение условия остатка между двумя формулировками. |
| [RAG-043](sources/RAG-043.md) | TRANSLATION-DPO-REJECTED-PROMPTS-v1.md | `llm_plan3` | Библиотека правдоподобных ошибочных переводов: одна основная ошибка и её наблюдаемое последствие. |
| [RAG-044](sources/RAG-044.md) | EXAMINATION-METHOD-TRANSLATION-v1.md | `llm_plan3` | Историческая методика от source entry до проверяемого переводческого контраста; явные примеры устаревших ограничений. |
| [RAG-045](sources/RAG-045.md) | TRANSLATION-ROUTING-FLOW-DPO-GUIDANCE-v1.md | `llm_plan3` | Разделение маршрутизации перевода, предметного переводчика и внутренних Unit; наблюдаемое последствие неверного route. |

## Дополнение по Angular и SQL

[Разбор полезных snippets и ограничений](SQL-ANGULAR-SOURCE-NOTES.md).

## Разделение источника и учебной истины

Фрагменты подтверждают, что записано в конкретной версии документа. Они не подтверждают текущую реализацию, качество нового набора или успешность обучения. `train_ready`, `PASS`, числа и старые команды внутри фрагмента сохраняются как исторические данные. Их нельзя автоматически переносить в новый отчёт.

Внешние URL, встречающиеся внутри исходных документов, служат только указателями. Для этой задачи новый материал берётся из `xiigen-planning` в ветках `main` и `llm_plan3`. Проектные skills в `llm_mvp_core` задают метод работы, но не расширяют источники корпуса.

## Новые источники от 25.10.2026

| ID | Документ main | Учебное применение |
|---|---|---|
| [RAG-OR-001](sources/RAG-OR-001.md) | GUIDE-B21-STEP-1-INVARIANTS.md | Граница настраиваемых параметров и обязательных гарантий; идентичность операций и порядок эффекта. |
| [RAG-OR-002](sources/RAG-OR-002.md) | GUIDE-B48-UX-AUDIT.md | Локализация ошибок формы, сохранение понятного контекста и оценка по роли. |
| [RAG-OR-003](sources/RAG-OR-003.md) | GUIDE-B46-DESIGN-SIM-CLIENT-SCREENS.md | Контракт клиентского экрана: серверный ответ, изменения состояния и матрица ролей. |
| [RAG-DC-001](sources/RAG-DC-001.md) | ANGULAR-ROUTE-MAP-AUDIT.md | Параметризованные пути как темы симуляций документации Angular. |

[Что добавлено и как использовать](UPDATE-2026-10-25.md). Кандидаты создаёт текущая модель Kiro без override; назначенный reviewer применяет предметную рубрику. Точная модель записывается по фактическому selector. Источник не выдаёт себя за измеренное обучение или актуальный runtime.

## Дополнение 25.10.2026: узкие причины ошибок и наблюдаемые состояния

Пять новых фрагментов закреплены на `main@8d3b4c600f458363e4a54ddce83e91000efd0e8a`; старые 45 IDs и их версии сохранены. Они используются в отдельных objective-рецептах плана, а не объединяются в одну учебную тему.

| ID | Источник | Учебное применение |
|---|---|---|
| [RAG-046](sources/RAG-046.md) | `Flow files generator/GUIDE-B18-TEACH-QA-R0.md` | Учебный контраст как одна различающая причина: positiveExample, negativeExample, discriminatingConstraint. |
| [RAG-047](sources/RAG-047.md) | `Flow files generator/GUIDE-B40-LIVE-RUN.md` | Разрыв между исходным контрактом и фактически отправленным модели prompt; привязка результатов к runId. |
| [RAG-048](sources/RAG-048.md) | `Flow files generator/GUIDE-B47-UI-STATE-MAP.md` | Наблюдаемые состояния initiate/in_progress/result/error/next_step и их зависимость от роли. |
| [RAG-049](sources/RAG-049.md) | `Flow files generator/GUIDE-B50-ROLE-SCREEN-MATRIX.md` | Матрица экран/роль и различие HIERARCHY, CONTEXT, TENANT-CONFIG, MUTUAL_EXCLUSIVE, DELEGATION. |
| [RAG-050](sources/RAG-050.md) | `Flow preparation/final results/FLOW-01-ENGINE-GAP-LIST.md` | Контраст гарантий: оба входных пути, FIFO против mutex, временное окно и полнота агрегации. |

[Как объединены поступившие изменения и версии источников](RECONCILIATION-2026-10-25.md). Все девять добавленных ID сохранены; ни один фрагмент не переназначен.

## Дополнение: проверка качества переводных отрицаний

| ID | Источник llm_plan3 | Учебное применение |
|---|---|---|
| [RAG-TR-001](sources/RAG-TR-001.md) | `обучение языку/1st game/curated_ru.py` | Ложные отрицания и неоднозначность director: не переносить ok/bad в gold без проверки видимого контекста. |

[Состав дополнения и границы проверки](EXPANSION-2026-10-25.md). 54 прежние записи сохранены, новый источник закреплён на `llm_plan3@66cd68b247ad9d92570b37594fcb2cb5eb33a6cc`. Актуальный manifest и source lock плана содержат одинаковые 55 IDs и хеши локальных выдержек.

## Дополнительные источники текущего набора

| ID | Источник | Применение |
|---|---|---|
| [RAG-OR-004](sources/RAG-OR-004.md) | GUIDE-B26-STEP-6-CYCLE3-CONTEXT.md | Декомпозиция; конфликтующие signal rules отмечены |
| [RAG-OR-005](sources/RAG-OR-005.md) | GUIDE-B24-STEP-4-CYCLE2-TEMPLATE.md | Предметный состав проверки шага |

