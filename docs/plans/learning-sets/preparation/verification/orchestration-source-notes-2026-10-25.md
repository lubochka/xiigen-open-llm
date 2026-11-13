# Оркестрация: дополнение подготовки DPO от 25.10.2026

Созданы 11 новых карточек `-02`, по 6 reference-пар: всего 66. Они дополняют прежние карточки `-01`; старые материалы не изменялись. Статус — подготовка, без запуска китайской модели и без измерений обучения.

## Что изменено

| ID | Поддомен | Узкая тема |
|---|---|---|
| OR-FUNC-02 | or.functional | Настраиваемое значение и обязательный инвариант |
| OR-ARCH-02 | or.architecture | Передача точного контракта между двумя узлами |
| OR-CODE-02 | or.code | Ограниченное поручение на исправление одного дефекта |
| OR-DB-02 | or.database | Ключ идемпотентности отражает логическую операцию |
| OR-CLIENT-02 | or.client | Запоздалый ответ не заменяет актуальное состояние |
| OR-SERVER-02 | or.server | Публикация опирается на подтверждённое сохранение |
| OR-MOBILE-02 | or.mobile | Продолжение после возврата приложения из фона |
| OR-DESIGN-02 | or.design | Видимость и редактирование по роли внутри одного экрана |
| OR-UX-02 | or.ux | Ошибка одного поля без потери остального ввода |
| OR-QA-02 | or.qa | Сравнение, которое выделяет один учебный фактор |
| OR-AUTO-02 | or.automation | Следующий шаг графа после завершения всех зависимостей |

## Конфликты источников

- **RAG-033:** Only real session prompts; skip when no compiler predicate fires. Текущее решение: Current explicit instruction requests simulations. Mark synthetic_from_repo; do not relabel synthetic as historical. Semantic targets reviewed by CODEX, not generated from keyword predicates.
- **RAG-031:** Compare against deterministic baseline; this section alone does not define a learned-before baseline. Текущее решение: New quality evaluation needs actual learned-state consumer, independent family-held-out exam, compatible trained baseline and no-update control. Historical recipe is not a current ready runner.
- **RAG-032:** A row succeeds only if all three layers acceptable. Текущее решение: Current explicit rule records success for each improved useful metric and domain, with regressions reported; successful metric does not mean universal integration readiness.
- **skill_dpo:** GENERATED requires different sources and real AI pipeline cycle. Текущее решение: Preparation pairs do not claim GENERATED threshold evidence; one Chinese model may create both candidate sides and independent CODEX review accepts/rejects. Reference examples are not a Kiro run.
- **RAG-006:** Nest decorators, MicroserviceBase, tenant context rules and store-before-enqueue are shown as project rules. Текущее решение: Use methodology only. All simulated local guarantees are stated in prompt; order alone never proves atomicity or exactly-once.

## Что проверить при интеграции

Parent добавляет единые команды подготовки/валидации, местоположение checkpoint и рабочей папки, глобальные order/next_id. Новый JSON индекс сохраняет действующие поля схемы, но order/next_id намеренно не назначает локально, чтобы избежать расхождения с другими доменами. Новые source RAG-OR-001..003 включить в общий manifest/lock; остальные IDs уже существуют.
