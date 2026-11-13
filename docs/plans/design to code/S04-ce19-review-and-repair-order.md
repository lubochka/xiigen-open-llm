# S04 — проверка ce19f657 и ограниченное задание на исправление

Дата проверки: 7 октября 2026 года.
Кандидат: `ce19f6578e578dd1815707a142ba82b65f4e9e16`.
Предыдущий кандидат для сравнения: `b05af49bcc960878e8350b715619938d8387a6e8`.
План: `lubochka/xiigen-planning`, `llm_plan3 @ 5b9f6844bdc39b1f253466a12962f63bd734a2c9`, пакет `Kiro-Xiigen-S04-Luna-xhigh-v2-Pack`.

## Вердикт

NEEDS_FIX для приёмки текущего кандидата. Новые исходники соответствуют подготовительным частям S04.04A; полная функция урока и весь S04 не завершены. Найдены два конкретных дефекта тестовой инфраструктуры и отдельный пробел Windows-покрытия. Подтверждённого обхода продуктового ORIGINAL-guard этим исследованием не установлено; падающие тесты не позволяют объявить его Windows-интеграцию принятой.

Это результат анализа опубликованных исходников и самостоятельного разбора существующих CI-артефактов. Новые .NET-прогоны не выполнялись. Исходники, план, ветки и настройки GitHub не изменены. Приведённое ниже задание не является отчётом о выполненном исправлении.

## Проверенные результаты

| Кандидат / run | Focused | Impacted | Валидатор |
|---|---|---|---|
| b05af49 / 37628473451 | 61 Passed, 0 Failed | 1291 Passed, 0 Failed | valid=true |
| ce19f657 / 37656036159 | 85 выполнено: 61 Passed, 24 Failed | NOT_STARTED | valid=false |

Сопоставление `testId + testName`: все 61 прежние focused identities сохранены и Passed; ровно 24 новые identities — Failed. Старые десять ordinary recovery-сценариев присутствуют и проходят. Focused и impacted пересекаются: их числа нельзя складывать как уникальное покрытие.

Три успешные jobs другого workflow, `Design-to-Code Focused Verification`, run 37655918233: resolve verification context, Focused verification, main-compatibility. Они не заменяют S03.04 Windows gate.

Отчёт исполнителя в `active.md` указывает WSL 1731 Passed. Это отдельный набор и платформа; здесь этот WSL-прогон не перезапускался и не используется как Windows PASS.

## F1 — P2: общий адрес teaching corpus между независимыми fixture

Источники на точном кандидате:
- `Tests/CodeDocumentationOneLessonSourceSelectionTests.cs`, `GenuineTeaching`, строки 948–992, особенно фиксированный `exclusion-reference-v{index}` и PersistAndSeal на строке 984.
- `Tests/CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.cs`, EndpointTrainingFixture, строки 4500–4525: Linux получает отдельный TempRepositoryRoot, Windows использует actual checkout.
- Там же, `OriginalTeachingExclusion_ReceivingGuardsRejectWithoutOpeningMissingTeaching`, строки 130–178.

Девять Windows-случаев останавливаются на MANIFEST_CONFLICT до нового guard: разные fault-варианты пытаются запечатать разные данные по одинаковым двум адресам. Один foreign-baseline случай останавливается на MANIFEST_INVALID: первый harness удаляет payload, после чего второй harness открывает тот же адрес ещё до проверки чужого baseline. Это также объясняет, почему одной последовательности тестов недостаточно для исправления: два harness могут существовать внутри одного теста.

Минимальный ремонт: отдельное namespace/уникальная допустимая версия teaching corpus для КАЖДОГО экземпляра harness в существующем доверенном repository root; передать идентичности через фактический SOURCE и заново получить реальные pins. Сохранять требуемые source/item ordering и semantic fault. Учесть оба teaching root в точной fixture-owned cleanup, включая отказ конструктора. Не менять production trust-root policy, не удалять общие версии и не игнорировать конфликт sealer.

## F2 — P2: snapshot читает весь checkout вместо принадлежащих fixture файлов

Источник: `Tests/CodeDocumentationPreparedBaselineTests.cs`, OrdinaryDiagnosticHarness.Files/AssertFiles, строки 482–489.

14 Windows-случаев падают в File.ReadAllBytes при обходе постороннего `artifacts/extension-runtime-token-quality-learnability/qsg21-live-runtime-target-probes/claude-live-plugin-hook/latest`. Files() рекурсивно перечисляет весь Fixture.RepositoryRoot. На Windows это checkout, а не отдельный временный repository.

Минимальный ремонт: полный снимок ТОЧНО принадлежащих сценарию каталогов и файлов, включая ORIGINAL, parent/model, DPO/source, обычную историю/claim/baseline и оба teaching corpus; переиспользовать существующие bounded snapshot/operational-lock правила. Проверять membership, bytes и нужные timestamps в этом наборе. Не подменять точную область произвольными исключениями, не проглатывать ошибки чтения обязательного fixture-файла и не удалять посторонние артефакты репозитория.

## F3 — пробел приёмочного покрытия

Четыре строки `OriginalTeachingExclusion_FiniteOwnerAndClosedRechecksRejectLateChangeAndRestoreHooks` расположены в `Tests/CodeDocumentationPreparedBaselineTests.cs`, строки 356–415. Текущие Windows focused/impacted filters S03.04 не выбирают этот класс/метод.

Перед заявлением соответствующего Windows-покрытия добавить эти точные identities в существующий gate (или выполнить отдельный привязанный к тому же SHA native gate) и проверить присутствие каждой строки в TRX. Сохранить весь предыдущий фильтр. При изменении CI синхронизировать `.github/workflows/s03-windows-verification.yml` и `.kiro/xiigen-slices/luna/s03/gate-plan.json`.

Windows-ветка этого теста сама помечает mid-open race и DACL как NOT_PROVEN: она проверяет positive/pre-cancel/replacement, не Linux hook внутри открытия. Нельзя переименовать её PASS в доказательство всех четырёх timing-сценариев. Для такого утверждения нужен реально наблюдаемый соответствующий native механизм; до него сохранить честное ограничение.

## Задание исполнителю

Выполнить ограниченный source/test repair F1 и F2, затем закрыть затронутую проверку. Не начинать новый общий дизайн S04 и не переписывать skills/AGENTS/CLAUDE. До редактирования перечитать актуальные инструкции, применимые навыки и полные затрагиваемые файлы. Сверить текущий HEAD: приведённые номера строк относятся к ce19f657, а не к будущей ветке. Не перезаписывать последующие изменения.

Основная область изменения: три существующих test owner — EndpointTests, PreparedBaselineTests, SourceSelectionTests. Изменение production guard допускается только при отдельном доказанном дефекте; текущие находки такого изменения не требуют. Для F3 — минимальный существующий CI/filter delta либо отдельное надлежащее доказательство, без ослабления валидатора.

Пример правильной проверки: независимо запечатанный teaching corpus с точным ORIGINAL owner-text должен успешно пройти исходный owner readback, после чего receiving Session возвращает именно `ONE_LESSON_TEACHING_ORIGINAL_OVERLAP`, Value отсутствует, дополнительных evaluator/teacher/update effects нет, исходный baseline и принадлежащие fixture файлы неизменны. MANIFEST_CONFLICT, MANIFEST_INVALID и DirectoryNotFoundException до этой границы — не положительное доказательство guard.

Обязательные регрессии: два harness в одном Windows root не делят teaching addresses; foreign-baseline не использует повреждённый корпус первого harness; независимые fault-варианты не конфликтуют при последовательном повторном прогоне; посторонний сломанный путь вне owned scope не влияет на snapshot, но пропажа обязательного owned файла обнаруживается. Все исходные fault-assertions, формы Unicode, полные корпуса/ordering и счётчики эффектов сохранены.

После ремонта сохранить исходные красные доказательства; выполнить свежую обычную сборку, focused и impacted на точном исправленном SHA. Проверить сохранность прежних 61 focused identities и всех 24 новых, плюс добавленные регрессии. Отдельно подтвердить новую область F3 без приписывания отсутствующего native timing. Финальный verdict привязать к точному опубликованному кандидату и фактическому отдельному CODE review по существующему контракту. Не объявлять весь S04 принятым.

## Незавершённые продуктовые требования — не подменять fixture-ремонтом

S04.04A-C01: утверждённый выбор одного задания по слабости и стабильному приоритету. C02: исторические пары только как контекст, не update batch. C03: полная material/certification ancestry и включение защиты в путь учителя, не только ORIGINAL-only descriptive witness. C04: отказ по действительному пределу полного teacher context, без скрытого усечения. Полный task в JSON и байтовый guard сами по себе не доказывают tokenizer-level admission.

В `CodeDocumentationOneLessonEndpoint.cs` обычная ветка необходимости урока по-прежнему возвращает `CODE_DOCUMENTATION_ONE_LESSON_NEEDS_LESSON_UNSUPPORTED` / HTTP 503. Не называть внутренние prerequisites законченной пользовательской функцией или доказанным улучшением Student.

## Состав доказательств

Соседние ZIP — неизменённые скачанные GitHub Actions артефакты. `s04-ci-independent-comparison.json` и `s04-ce19-detailed-failures.txt` — производные результаты самостоятельного XML/JSON-разбора в этой сессии, не новые тестовые исполнения. Цифры и группы падений можно проверить по `focused/focused.trx` и `validation-report.json` внутри оригинальных архивов.
