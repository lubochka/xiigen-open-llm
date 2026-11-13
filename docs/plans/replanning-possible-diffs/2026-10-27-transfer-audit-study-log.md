# Журнал исследования переносов: P02, P08, P09

27 октября 2026. Текущий запрос — аудит исполнения переносов и правка планов/карточек. Фаза READ_ONLY_RESEARCH + Markdown. Новые C#, runner, dataset, state, сборки, тесты и модельные вызовы не создавались и не запускались; git commit/push не выполнялись.

## Прочитанные источники и выводы

* HEAD dd5d786879ea0e92d471eb84eb4fa0b252458127; P02 reporting commits8370a1f70a9/be1c9e56da1, P08 f4dcf691cf2, P09 239d7557f46. Это конкретный срез, не обещание актуальности будущего HEAD.
* P0201 Program, действующий PreferenceSeparationReport, DpoMetrics, tests и сохранённый test log: alias/builder действительно добавлены, но внешний writer путает единицу pair/step и пишет неизвестное SFT время0. Сохранённые103tests — чужой ранее выполненный запуск. Математический контрпример [+1,-3] разделяет pair rate и positive-step-mean rate без новых model calls.
* P0801 source/raw и native Sampler/TextGenerator: полезный T1 probability эффект сохранён, consumer A/B через генераторы ещё не выполнен; cost не измерен. Граница r=0 нарушает support статически. Независимый работник Astra ultra `/root/p08_transfer_audit` записал [source audit](fixes-based-on-experiments/reviews/2026-10-27-p08-generation-transfer-source-audit.md), проверил source/state hashes, затем рецензировал план85. Это один адресный worker, не полный повтор всех аудитов.
* P0901 Program/report/metrics/arms-logits и CacheAwareForwardTests/LlmModel: повторное сопоставление сохранённых21 504чисел дало0различий, без inference. Existing empty/full и chunked tests уже есть; нужно learned fixture усиление. Allocations window включает wrapper и не задаёт константу одного Forward.

## Документный результат и границы

[Аудит84](fixes-based-on-experiments/84-reporting-sampling-cache-transfer-audit-2026-10-27.md), [ремонт85](fixes-based-on-experiments/85-reporting-sampling-cache-repair-plan-2026-10-27.md), [применимость86](fixes-based-on-experiments/86-reporting-sampling-cache-card-coverage-2026-10-27.md). Во всех56 файлах-владельцах62 ID добавлены требования применимой категории; это покрытие требований документации, не62 runtime проверки. Исправлены общий договор, навигация, P02 и шесть H64-RD карточек. Исторические attempts сохранены.

Доступные context-mode MCP отсутствовали; чтение/обработка выполнены Node filesystem и read-only git/rg, записи — native apply_patch. Частичные чтения governance/skills не называются полной загрузкой. Прочитаны относящиеся к задаче части architect-behavior-classifier, planning-session-startup, plan-review, generated-code-review, prompt-patch-authoring, output-readability-gate, human-readable-architecture-audit и communication glossary; применение ограничено исследованием и документами.

Независимое документное ревью `/root/p08_transfer_audit` подтвердило P08-разделы85 и соответствие всех56 owner blocks карте62 ID. Последнее уточнение P75/P100 — прямые prefill без Sample — внесено родителем и прочитано обратно вместе с таблицей36. Проверены467 локальных ссылок выбранных новых поверхностей без отсутствующих целей; git diff --check выбранных tracked docs exit0. Это статическая проверка документации, не product tests.

Открыты операционные работы85 и прежние80/82/83. Они не выполняются без отдельного поручения. Следующий исследовательский триггер — новые исходные изменения/receipts исполнителя; отсутствующие тестовые результаты или неизмеренные resource controls не объявлены PASS.
