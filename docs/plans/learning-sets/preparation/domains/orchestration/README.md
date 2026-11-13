# Оркестрация, включая router: материал для DPO

Каталог: 26 родительских тем и 21 микроцелей. Полный текущий порядок и NEXT — [EXECUTION-QUEUE.md](../../EXECUTION-QUEUE.md); прямое указание ID выше очереди. [Общий промпт Kiro](../../14-GENERATION-CAMPAIGN.md) и [контракт формата](../../02-DATA-CONTRACT.md). Подготовка нового дополнения не означает выполненную генерацию, native oracle или обучение.

Отдельные поддомены: functional, architecture, code, database, client, server, mobile, design, ux, qa, automation; routing отдельно. Набор обучает конкретному оркестрационному решению. Разные классы и контроли внутри одной задачи нужны; они не означают смешение обучения переводам и программированию.

## Родительские темы

| ID | Тема | Домены |
|---|---|---|
| [OR-ROUTE-01](OR-ROUTE-01.md) | Выбор домена по намерению, а не по ключевому слову | or.routing |
| [OR-FUNC-01](OR-FUNC-01.md) | Один проверяемый критерий функционального требования | or.functional |
| [OR-ARCH-01](OR-ARCH-01.md) | Владелец решения и граница зависимости | or.architecture |
| [OR-CODE-01](OR-CODE-01.md) | Следующий шаг реализации по готовым зависимостям | or.code |
| [OR-DB-01](OR-DB-01.md) | Порядок изменения данных при заданных ограничениях | or.database |
| [OR-CLIENT-01](OR-CLIENT-01.md) | Состояние клиента соответствует ответу сервера | or.client |
| [OR-SERVER-01](OR-SERVER-01.md) | Обязательное условие перед серверным изменением | or.server |
| [OR-MOBILE-01](OR-MOBILE-01.md) | Синхронизация офлайн-изменения без потери данных | or.mobile |
| [OR-DESIGN-01](OR-DESIGN-01.md) | Выбор UI-поверхности по пользователю и состоянию | or.design |
| [OR-UX-01](OR-UX-01.md) | Понятное следующее действие при затруднении | or.ux |
| [OR-QA-01](OR-QA-01.md) | Проверка меняющегося учебного поведения | or.qa |
| [OR-AUTO-01](OR-AUTO-01.md) | Ожидание и продолжение одного запуска | or.automation |
| [OR-FUNC-02](OR-FUNC-02.md) | Настраиваемое значение и обязательный инвариант | or.functional |
| [OR-ARCH-02](OR-ARCH-02.md) | Передача точного контракта между двумя узлами | or.architecture |
| [OR-CODE-02](OR-CODE-02.md) | Ограниченное поручение на исправление одного дефекта | or.code |
| [OR-DB-02](OR-DB-02.md) | Ключ идемпотентности отражает логическую операцию | or.database |
| [OR-CLIENT-02](OR-CLIENT-02.md) | Запоздалый ответ не заменяет актуальное состояние | or.client |
| [OR-SERVER-02](OR-SERVER-02.md) | Публикация опирается на подтверждённое сохранение | or.server |
| [OR-MOBILE-02](OR-MOBILE-02.md) | Продолжение после возврата приложения из фона | or.mobile |
| [OR-DESIGN-02](OR-DESIGN-02.md) | Видимость и редактирование по роли внутри одного экрана | or.design |
| [OR-UX-02](OR-UX-02.md) | Ошибка одного поля без потери остального ввода | or.ux |
| [OR-QA-02](OR-QA-02.md) | Сравнение, которое выделяет один учебный фактор | or.qa |
| [OR-AUTO-02](OR-AUTO-02.md) | Следующий шаг графа после завершения всех зависимостей | or.automation |
| [OR-QA-03](OR-QA-03.md) | Предметный состав проверки одного учебного решения | or.qa |

## Узкие задания

| ID | Учебная граница | Reference |
|---|---|---:|
| [OR-ROUTE-INTENT](../../curriculum/cards/OR-ROUTE-INTENT.md) | Маршрут по действию при явном закрытом каталоге | 25 |
| [OR-AUTO-TERMINAL](../../curriculum/cards/OR-AUTO-TERMINAL.md) | Следующее действие по состоянию одного длительного запуска | 4 |
| [OR-CODE-PROMPT-CONTEXT](../../curriculum/cards/OR-CODE-PROMPT-CONTEXT.md) | Готовность передачи задачи исполнителю по фактически отправляемому контексту | 4 |
| [OR-ARCH-FIFO](../../curriculum/cards/OR-ARCH-FIFO.md) | Назначение гарантии порядка в компоненте очереди | 4 |
| [OR-CLIENT-RUN-STATE](../../curriculum/cards/OR-CLIENT-RUN-STATE.md) | Переход UI по подтверждению своего запуска | 4 |
| [OR-DESIGN-ROLE-PANEL](../../curriculum/cards/OR-DESIGN-ROLE-PANEL.md) | Одна панель по роли, объекту и области доступа | 4 |
| [OR-UX-RECOVERY](../../curriculum/cards/OR-UX-RECOVERY.md) | Одно действие восстановления по известной причине | 4 |
| [OR-QA-CAUSAL](../../curriculum/cards/OR-QA-CAUSAL.md) | Один совместимый parent для сравнения учебного изменения | 4 |
| [OR-FUNC-ACCEPTANCE](../../curriculum/cards/OR-FUNC-ACCEPTANCE.md) | Наблюдаемый критерий без изменения требования | 6 |
| [OR-DB-ATOMIC-CLAIM](../../curriculum/cards/OR-DB-ATOMIC-CLAIM.md) | Следующий шаг по доказанному владению операцией | 6 |
| [OR-SERVER-OUTBOX](../../curriculum/cards/OR-SERVER-OUTBOX.md) | Серверный переход между сохранением и доставкой события | 6 |
| [OR-MOBILE-ACK](../../curriculum/cards/OR-MOBILE-ACK.md) | Закрытие локальной операции только по подходящему подтверждению | 6 |
| [OR-AUTO-FANIN](../../curriculum/cards/OR-AUTO-FANIN.md) | Сбор группы только по её составу и политике завершения | 6 |
| [OR-ARCH-DECOMPOSE](../../curriculum/cards/OR-ARCH-DECOMPOSE.md) | Полезная декомпозиция одного узла с сохранением обязательств | 8 |
| [OR-QA-REVIEWER-COVERAGE](../../curriculum/cards/OR-QA-REVIEWER-COVERAGE.md) | Выбор недостающей предметной проверки для одного учебного шага | 8 |
| [OR-AUTO-TIME-WINDOW](../../curriculum/cards/OR-AUTO-TIME-WINDOW.md) | Ограничить автоматическую агрегацию заданным временным окном | 8 |
| [OR-AUTO-RESULT-COVERAGE](../../curriculum/cards/OR-AUTO-RESULT-COVERAGE.md) | Не завершать агрегацию по усечённой выдаче | 8 |
| [OR-AUTO-EVENT-GATE](../../curriculum/cards/OR-AUTO-EVENT-GATE.md) | Следующий шаг по точному событийному условию | 8 |
| [OR-DB-CAPACITY-ATOMIC](../../curriculum/cards/OR-DB-CAPACITY-ATOMIC.md) | Выбор плана сохранения ограниченного ресурса | 8 |
| [OR-ARCH-EVENT-CONTRACT](../../curriculum/cards/OR-ARCH-EVENT-CONTRACT.md) | Передача канонического имени события через границу компонентов | 8 |
| [OR-QA-SCORE-PRESENCE](../../curriculum/cards/OR-QA-SCORE-PRESENCE.md) | Сохранение наблюдённой оценки без подмены отсутствия нулём | 8 |

Код, документация, перефразирование и перевод одного источника сохраняют общие origin/leakage до split. Публичные reference и их потомки не являются закрытым EXAM. Точные first-run/dataset/B1 пути задаёт [pilot-queue.json](../../curriculum/pilot-queue.json); метрики, восстановление и достаточный ненулевой старт — [05](../../05-LEARNING-AND-CHECKPOINTS.md). Выбор/оценка выполняются CODEX, генерация — китайской моделью Kiro по [10](../../10-CODEX-AND-CHINESE-ROLES.md). NEXT/STOP соблюдаются немедленно, старые версии сохраняются.

