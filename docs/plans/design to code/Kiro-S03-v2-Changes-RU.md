# S03 v2 — что заменено и почему

Это не заявленная семантически эквивалентная компрессия. Продуктовые условия
сохранены; операционные правила намеренно изменены по общему разрешению владельца.

| v1 / накопившаяся проблема | v2 |
|---|---|
| Запрет Git в каждом task конфликтует с передачей CI-кандидата | Один принятый grant; candidate commit != acceptance |
| Только локальный runner при SAC-block | Существующий отдельно разрешённый CI; никакого отключения SAC |
| Разовый запуск, новая просьба на cancel/rerun | Decision table и общий лимит серии |
| Всегда стартовать S03.00 | Возобновить earliest unmet requirement с сохранением milestone |
| Proposed identity class при восьми уже написанных EndpointTests | Case mapping к existing owner, без дубликата |
| tasks.json имел NOT_EXECUTED как потенциальный новый live state | Catalogue без execution-state; current truth только в existing state/evidence |
| Четыре роли повторяются на обычное управление CI | Роли для product/delta, manager для обычных API/Git; финальный reviewer отдельно |
| Общий blocked flag | Implementation, verification, acceptance и domain blocker раздельно |
| Два одинаковых paid test sets | Один полный run с mapping, если повтор не экспериментальное требование |
| Status-only commit запускает весь reload/build | Relevant identity checks и сохранённый tested SHA |
| Отсутствует packet субагента | Замена/разрешённый явно помеченный fallback, без self-approval |
| Хаотичные уточнения после исходного ZIP | Весь active prompt tree согласован с v2, не ещё одна contradicting наклейка |

## Что сохранено буквально

REQUIREMENTS.md, PROPOSED-CONTRACTS.md и context-manifest.json;
для каждой из 13 задач — product scope/allowlist seed, positive/negative examples,
таблица test scenarios/observations, task-specific role responsibility table и
edge-case list. В validation/preservation.json — fingerprints каждого блока по чтению записанных
файлов; при сравнении исключены только внешние переводы строк.
Source tables сохранены с переобозначением их как исторических v1 references.
Task IDs и acceptance dependencies не переименованы.

## Что не выполнено

Нет свежего private-repository audit, CI query, .NET execution, платежей,
репозиторных записей или отдельного reviewer-агента. Новый workflow/runner не
поставляется. Проверки validation оценивают текст/структуру/сохранность блоков,
не доказывают поведение Luna или приложение. Новые signing/cleanup проекты
в задачи S03 не добавлялись.
