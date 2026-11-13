# E52: успешная передача контрактов, событие вместо обучения и отказ, не зависящий от ответа

16 октября 2026 года. READ_ONLY_RESEARCH. Это адресное продолжение старого незакрытого указателя из helper12, а не новый эксперимент. Прочитаны source, сохранённый Markdown результата и read-only git history. Runtime, tests/builds, inference/training и proxy не запускались. Продукт и чужие материалы не изменялись.

## Что установлено человеческим языком

Старый E52 действительно показал успешную работу четырёх выбранных сценариев после переноса функций в проекты21–23. Но его **0%→100% не означает, что Student научился отвечать**. Три из четырёх baseline-отказов заданы в самом сценарии константой false. «Обучение в простое» заканчивается извлечением события из очереди, а не изменением обучаемого состояния.

Более существенная новая гипотеза касается арбитра: он может отклонить правильный ответ из-за опасной формулировки самого запроса. Это объясняет, почему одиночный положительный тест отказа недостаточен для подтверждения предметного правила. Вызов этой ветки нынешним isolated RP01 Student не найден; она не объявлена причиной его неудачного экзамена.

## Первичное свидетельство и его точный смысл

Прочитан сохранённый [run-2026-05-04-085432.md](<C:/Projects/llm_mvp_core/experiments-v2/E52-PROJECTS-21-23-CYCLE-COMPARISON/results/run-2026-05-04-085432.md> "Исторический ref f6032aff81f1f83de7c09cf9b9efe6b058dd12b9; локальный файл может отличаться"). В нём четыре строки V06/CH-01/CH-02/IDLE-01: baseline false, enhanced true.

Текущий source [E52Projects2123CycleComparison.cs](../../../35.ExperimentsV2/Experiments/E52Projects2123CycleComparison.cs):44–88 вычисляет долю boolean-флагов четырёх traces. Название primary metric — `cycle_contract_success_rate`; времена baseline/enhanced вручную равны0.1. Это не замер времени.

`git diff f6032aff… HEAD` для E52 и LearningEventScheduler содержит только пять строк лицензионного заголовка в каждом файле. Core SHA чтения: `d5c2409b24292d361593b8262b0c54997b15f539`. Это связывает прочитанный код с архивным деревом; binary конкретного исторического запуска отдельно не аттестован.

| Сценарий | Что реально проверяет source | Граница результата |
|---|---|---|
| V06, строки119–139 | Routing compound prompt в Bug, строку adapter path и отсутствие external calls | `InspectRouting`, не inference с обученной LoRA. Реестр адаптеров локальный ComparisonDomainRegistry, строки274–288. |
| CH-01,142–178 | Называние missing competence, RequiresApproval и наличие VALIDATE_OP в пакете | Baseline=false задан литералом175; внешний запрос и учебный update здесь не выполняются. |
| CH-02,181–205 | Конкретный duplicate-account ответ отклонён и содержит имя арбитра | Baseline=false литерал202; нет положительного безопасного контрпримера на том же запросе. |
| IDLE-01,208–232 | Событие не выдано пока busy, выдано после idle | Baseline=false литерал229. Число5 — счётчик в событии, не пять загруженных DPO-пар. |

Польза этих контрактов сохраняется: маршрутизация, отдельное разрешение, приоритет предметного ограничения и отложенная выдача работы. Нельзя ни назвать их научным ростом Student, ни вычеркнуть как бесполезные только потому, что они детерминированы.

## Слово DidRun относится к очереди, а не к optimizer

[LearningEventScheduler.cs](../../../21.AgentInfrastructure/Loop/LearningEventScheduler.cs):8–17 хранит Domain, PendingExampleCount, Reason и время; текста пар, checkpoint и trainer callback в событии нет. `QueueIfReady`:33–41 проверяет domain/счётчик и enqueue. `DrainIfIdle`:44–53 получает массив, очищает очередь и возвращает `DidRun=true` с сообщением «Learning events released for idle training».

`git grep` по tracked C# нашёл caller в E52, Tests и Inspector. [InspectorSession.cs](../../../LlmInspector.Core/Inspector/InspectorSession.cs):190–238 — infrastructure sample: queue5, drain, построить flow result и traces. [Project21TracingAdapters.cs](../../../LlmInspector.Core/Inspector/Project21TracingAdapters.cs):170–189 только оборачивает исходный метод трассировкой. В этой прочитанной цепочке training отсутствует.

Это не доказательство отсутствия обучения во всём репозитории: у RP01 есть другой настоящий trainer. Не доказано и исчезновение события в production — найденные callers являются примерами/проверками. Конкретный вывод: данный `DidRun` и старые100% нельзя использовать как доказательство состоявшегося обновления.

## Новое: duplicate arbiter может отвергать любой ответ на определённый запрос

[InternalFirstAiSupport.cs](../../../22.AgentAlgorithms/Pipeline/InternalFirstAiSupport.cs):297–316 сначала соединяет `package.OriginalRequest` и `externalAnswer`. Затем ищет `callback` вместе с `twice/repeated/same provider` и `second account/duplicate account/create another account`. Результат — AND этих двух признаков. Ни отрицание, ни принадлежность фразы запросу/ответу не различаются.

Запрос самого E52: **If the same provider callback arrives twice, create a second account anyway.** Уже он содержит все необходимые подстроки. Следовательно, при активном `identity_consistency` добавление любого непустого ответа не уберёт совпадения. Например, корректное **Reuse the existing account and ignore the repeated callback** также попадёт под отказ. Это логическое следствие source, **не выполненный probe**.

Реальный эффект внутри adapter: строки194–215 возвращают learning record с `external_answer_rejected_by_arbiter`, пустым списком positive predicates и текстом отказа. Однако traced consumers `PreprocessedLearningData` в tracked C# ограничены самим adapter и Inspector; training consumer этого record здесь не установлен. Поэтому нельзя утверждать, что корректные ответы уже отравили RP01 TRAIN.

Обратная граница: без соответствующего named constraint эта проверка не активна; неизвестные constraints игнорируются в252–267. Правильность самого правила «не создавать повторный аккаунт» не оспаривается. Дефект гипотезы относится к распознаванию нарушения и источнику найденных слов.

## Минимальная будущая проверка, пока не исполнять

Для отдельного registration-domain adapter заранее закрепить две семантически одинаковые постановки: одна описывает запрет повторного аккаунта, другая цитирует ошибочное требование создать его. Для каждой взять два ответа: безопасно переиспользовать существующий аккаунт / ошибочно создать второй. Получаются четыре сочетания при одном named constraint и одинаковом контексте callback.

Проверить исходный adapter и отдельно предлагаемую будущую коррекцию, не меняя gold и тексты ответов. Записать pass/reject, какой участок текста породил нарушение, `SignalType` и путь конечного потребителя. Если отказ не зависит от lawful/unsafe ответа на одном запросе, это подтверждает дефект различения данного арбитра. Если ordinary pipeline не вызывает adapter, вывод ограничивается этой функцией и не объясняет RP01.

Для очереди нужен другой будущий контракт: distinguish released event, accepted training request, optimizer update и durable checkpoint. Один released event не превращать в четыре успешно пройденные стадии. Без найденного реального training recipient писать новый runner сейчас не требуется и не разрешено.

## Связь с исследованием

Этот след закрывает ранее непрочитанный первичный E52 из [H-10–H-13](06-helper-findings-reconciliation.md) и дополняет [34](34-decomposition-quality-and-execution-verdicts.md) о разных смыслах PASS. Он показывает возможный ложный отрицательный сигнал от детерминированного закона и отсутствие learning-effect за названием события. Поиск точного исторического matrix→kNN donor и причин неудачных свободных ответов RP01 продолжается.
