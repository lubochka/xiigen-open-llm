# Идентичность Unit и незавершённый учебный пакет

16 октября 2026. READ_ONLY_RESEARCH. Прочитанные sources — core `0eccbdc3c4efb90adf1d699806cbfa9f5610998d`. Дополнения помощников — planning `9ef3402ffdbd`. Код, dataset, runtime и checkpoints не менялись; сценарии ниже не запускались.

Входы: [helper о KNN state/confidence](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/22-INDEPENDENT-KNN-STATE-IDENTITY-AND-CONFIDENCE-2026-10-16.md>), [helper о case/atomicity](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/23-NEW-UNIT-CASE-IDENTITY-AND-NONATOMIC-TRAINING-2026-10-16.md>). Предложения этих файлов не меняют текущую фазу и не задают новые labels происхождения данных: используется пользовательское `simulation`.

## СТ-01. Manifest generic kNN описывает устройство, но не полное выученное состояние

[KnnLogicalUnit.cs](../../../21.AgentInfrastructure/Logical/KnnLogicalUnit.cs):23–35,160–178 содержит UnitId/role/domain, количество примеров, размерность, k, confidence floor, время/allocations и названия ingredients. В нём нет hashes tokenizer merges, значений indexed vectors и output-map. Два разных набора с одинаковыми метаданными не обязаны означать одну выученную функцию.

Это недостаточность **данного manifest для доказательства тождества**, а не уже найденная ошибка конкретного restore. Внешний владелец может сохранять дополнительные артефакты; его контракт нельзя заменить предположением по одному DTO. Отдельный RP01 full checkpoint имеет более строгие readback проверки — [12](12-student-update-to-generation-path.md).

Будущий небольшой контроль: при одинаковых логических метаданных сравнить фактические tokenizer/vector/label identities и ответы. Изменение state hash при том же manifest показывает границу происхождения, но не само по себе ухудшение качества. Для восстановления нужны проверяемые state artifacts, а не только повторное число строк.

## СТ-02. Полная строка input не означает case-sensitive адрес памяти

[TranslationTrainedUnits.cs](../../../27.TranslationDomain/TranslationTrainedUnits.cs):85–100,124–147 хранит `_signatureCorrections` в `OrdinalIgnoreCase` dictionary. `Normalize` сжимает whitespace; training пишет по normalized raw text, inference ищет по нему. У языка нет отдельного поля в ключе. Две разные исправленные метки на текстах, отличающихся только регистром, заменят друг друга.

Причинное условие: domain oracle должен действительно требовать разные коррекции. Для case-insensitive прозы нормализация может быть полезна. Для case-sensitive идентификаторов и отдельных языковых различий она может стирать значение. Пример «May/may» без полного контекста не является достаточным gold; безопаснее заранее выбрать однозначное предметное различие. Текущий общий builder нередко подставляет собственную исходную метку — тогда опыт проверит self-label вместо исправления, см. [13](13-domain-context-and-dpo-subunits.md).

Будущий контроль: два различающихся по регистру входа с независимыми корректными метками, затем порядок A→B и B→A. Сравнить сохранённые keys, выбранную correction и ответ. Единственный изменяемый фактор — правило идентичности; язык, Unit и закон фиксированы. Если oracle не различает значения, коллизия не считается ошибкой качества.

## СТ-03. Structured batch может изменить часть памяти до исключения

[TranslationMachine.cs](../../../27.TranslationDomain/TranslationMachine.cs):89–145 проверяет наличие batch и строк, затем непосредственно вызывает каждый `trainer.TrainOnExample`. Общего preflight всех JSON или rollback в показанном методе нет. Identity завершённого примера добавляется сразу после его trainer; общие version/history/hasStructuredTraining обновляются после всего цикла.

Поэтому helper-сценарий «валидный A, затем неразбираемый B» имеет конкретную границу: память A и его identity уже могут быть записаны, но history завершённого batch ещё отсутствует. Не следует писать, что вообще все identities обязательно пропали — source показывает поштучную запись.

**Более узкое условие внутри одного примера.** `SemanticTokenizerUnit.TrainOnExample:130–140` сначала разбирает chosen, затем записывает `_patternCorrections`, и только после этого разбирает `InputJson`. При валидном chosen и malformed input исключение может возникнуть **после** изменения pattern-map, но до signature-map и до добавления machine identity. Это прямой порядок операций, не выполненный fault injection.

`ExecutorUnit.TrainOnExample:409–418`, напротив, разбирает input и chosen до первых изменений exact/kNN. Ошибка именно этих JSON не создаёт того же частичного состояния одного Executor-примера. Уже завершённые предыдущие примеры batch остаются отдельным вопросом. Нельзя обобщать один Unit на все реализации.

`ExportCheckpoint:260–302` снимает живые карты и текущую историю. `ImportCheckpoint:308–337` имеет отдельную предварительную валидацию структуры. Поэтому найденная возможность частичного **training batch** не означает автоматически неисправность import или транзакционного DPO owner RP01. Actual outer caller может иметь собственный rollback; он ещё должен быть прослежен для рассматриваемого запуска.

**Будущий малый контраст.** На отдельном временном Unit-runtime один валидный пример и один заведомо повреждённый только в тестовом объекте. Отдельно failure до первой записи и после первой записи; снимок memory/history/identities до/после, один предметный запрос. Нельзя делать это на принятом checkpoint. Выбор минимального ремонта — preflight, staged state или явная частичная квитанция — зависит от фактического caller/recovery-контракта. Сейчас никакой patch не сделан и частота таких ошибок в исторических runs неизвестна.

## СТ-04. Confidence transformer-kNN — преобразование score, не измеренная вероятность правильности

[LlmIngredientLogicalUnits.cs](../../../18.CapabilityTraining/LlmIngredientLogicalUnits.cs):191–208 возвращает выбранный ответ с `Round(Clamp(.50 + fusedScore*.49, 0, .99), 2)` и пустым списком warnings. При обычных конечных неотрицательных компонентах fusion это даёт минимум .50 для любого непустого пула. Никакая проверка предметного закона в самом этом методе не участвует. Обработка NaN/неисправного численного input в данном выводе не рассматривается.

Вывод ограничен методом: это не калиброванное `P(correct)`. Внешний arbiter может законно отклонить ответ; его trace и threshold надо найти прежде, чем говорить о пропуске ошибочного ответа пользователю. Дополнительный UNKNOWN bonus разобран в [15](15-memory-growth-fusion-and-bpe.md).

Будущий контроль — неизменная память и небольшая заранее подготовленная панель in-domain/out-of-domain, раздельно правильность и confidence, затем selective risk при одинаковой coverage. Отказ от большего числа запросов нельзя выдавать за приобретение новой способности. Эта ветка по-прежнему не привязана к прямому RP01 `TextGenerator` как причина его результата.

## Следующий поиск

Для трёх типов памяти требуется разная проверка: identity выученной функции, атомарность приёма урока и authority ответа. Продолжается поиск actual caller, исторических partial failures и положительных source-bound результатов; перечисление условий не закрывает причинную проверку.
