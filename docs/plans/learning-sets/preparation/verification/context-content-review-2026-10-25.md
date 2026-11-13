# Предметный просмотр дополнения — 25.10.2026

Это review подготовленных условий и ответов. Вызов китайской модели, независимый языковой oracle, SQL Server и обучение не выполнялись. Проверка автором и независимым агентом не заменяет будущие CODEX selection/review receipts.

## Проверенные конструкции

| Материал | Строк | Происхождение | Проверено |
|---|---:|---|---|
| Переводы | 48 | 4 исходные семьи | Все 6 направлений; контрфактуал меняет ответ при том же source text; достаточный контекст; баланс рода/позиции и действия/модальности |
| Оркестрация | 16 | 14 объявленных семей | Достаточная policy; one-decision contrast; выбор предметной проверки не смешивает предметные уроки |
| SQL DOC/CODE | 12 | 1 общий origin cluster, 6 вариаций | Полный setup и trace; DOC описывает фактический код; CODE меняет один control statement; отдельная connection без outer transaction |

## Замечания и применённые исправления

1. В 4 прежних CODE-SQL-GUARD prompt отсутствовал явный SET Available := Available - @qty. Активные .v2 его содержат; полные старые строки и происхождение сохранены в quarantine.
2. Начальные переводческие опоры коррелировали gender с позицией референта и действие с модальностью. Добавлены counterfactual варианты во всех направлениях с общей lineage; партия12 содержит4клетки по3строки. Эти требования относятся к составу будущего принятого набора, а не доказывают результат генерации.
3. OR-ARCH-DECOMPOSE REF01 only-if был недостаточным для обязательного split. В REF01 теперь достаточное правило; REF05 явно требует keep при отсутствии самостоятельных контрактов. Повторный независимый просмотр этих двух условий блокеров не нашёл.
4. В MASTER/10/14 расходилось действие до CODEX selection. Новый модельный вызов требует фактический selection; request/prompt и имеющийся draft сохраняются до него как NOT_SELECTED. Отсутствие selection не является живым процессом ожидания.
5. SQL oracle не должен добавлять внешнюю BEGIN/ROLLBACK обёртку: она меняет проверяемый @@TRANCOUNT. Новые cards задают отдельный sqlcmd process/connection на candidate; все ожидаемые результаты остаются NOT_RUN/predicted.

Ни одна из этих проверок не оформлена успешным learning experiment и не добавляет неподтверждённый production-fix candidate.

## Прочитанные предметные файлы

| Карточка | SHA-256 просмотренной версии |
|---|---|
| [TR02-COREFERENCE](../curriculum/cards/TR02-COREFERENCE.md) | `1228d21ff9f3e14523da04880c09038c026833ff887e46728d6c929b96558877` |
| [TR05-MODALITY](../curriculum/cards/TR05-MODALITY.md) | `7cd0b21ecf584bc7f81ec17109cf11784fc0db8368c9e3b832250ecf35d33fb9` |
| [OR-ARCH-DECOMPOSE](../curriculum/cards/OR-ARCH-DECOMPOSE.md) | `46cd69e852f9e97952d4cfca7d0b59eeb8b3f1574e24d1506f419adba79bf439` |
| [OR-QA-REVIEWER-COVERAGE](../curriculum/cards/OR-QA-REVIEWER-COVERAGE.md) | `d45b6077ee104a684fe72b7e166b322eca710144d3e305942c657ba5c21594f0` |
| [DOC-SQL-SAVEPOINT](../curriculum/cards/DOC-SQL-SAVEPOINT.md) | `20eef24b19f22170a8024982842eed5b175b5d9c2a47ad8a96ad0c2b3cbaa136` |
| [CODE-SQL-SAVEPOINT](../curriculum/cards/CODE-SQL-SAVEPOINT.md) | `0a4115724d21f9442482bc0609e17c7d445c81691dbb32c7412ae0354a4a2689` |

Основной исходный snapshot: `llm_plan3@7b5cd885d352f604a763b9a66b44e861b99ca427`; RAG `main@f16e68bc662bc629bc45bd0e6a46011c12865199`. Проверка точного сохранения [652 исходных reference и 55 источников](context-preservation-2026-10-25.json) выполнялась отдельно.
