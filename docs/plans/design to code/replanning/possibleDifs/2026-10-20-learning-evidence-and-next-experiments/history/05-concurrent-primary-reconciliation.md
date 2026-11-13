# Дополнение: новые primary-readback и provenance на planning 53cbcab

**Граница проверки.** Прочитаны только три назначенных файла через `github_fetch_file` на planning `53cbcabd3926cda714bab428d294eb666301e9e4`: `16-RP01-primary-readback.md`, `17-current-dpo-unit-transfer.md` и `evidence/e09-transfer-commit-receipt.json` в `2026-10-20-learning-delta-audit-883496`. Их Git blob SHA локально совпали: соответственно `16a4fbd376439be0f32ad379eae8ac37483e4be3`, `448be5d3d34b340a36f6caf9373c7ecf1cff2801`, `46d80b7f6e6452e08a7f73a10fa1fa5ff66ee3c5`. Исторические JSON результатов, source patch переноса, checkpoint binaries и исходники, на которые ссылаются эти документы, в данном ограниченном дополнении **повторно не загружались**. Документ 16 сообщает о собственном readback actual JSON, а не LFS pointer; мы сохраняем этот уровень provenance, не выдавая его за новую независимую реконструкцию измерений.[1]

## 1. Что уточняется в выводе о переносе matrix→kNN

**Конкретный перенос experimental structural kNN в production теперь имеет commit receipt.** Commit `27ae03266ae2f7300d63e915795c0ff62369fa67`, parent `f8a41b76b26b7de4c53aecb845661d37837d686c`, назван «E09 E10 phase 2: move structural KNN to production». Receipt фиксирует добавление `12.SemanticCompiler/Prediction/StructuralKnn.cs` — 225 строк — и удаление `35.ExperimentsV2/Core/StructuralKnnPredictor.cs` — 50 строк — и `35.ExperimentsV2/Infrastructure/StructuralVectorExtractor.cs` — 98 строк.[2]

Это дополняет раздел 4 нашего исторического отчёта: нельзя оставлять впечатление, что **вообще никакой** точный commit переноса не найден. Но прежний узкий вывод о **неидентифицированном конкретном donor «неуспешная matrix-функция → успешная kNN-функция» сохраняется**. Receipt описывает уже structural kNN→production; он не содержит patch тела функций, названия матричного donor-run, соответствия одной функции между представлениями или сопоставленного before/after на одном корпусе и target. Отождествление его с переходом E07→E09 либо с именно тем историческим случаем, который имел в виду пользователь, остаётся недоказанным. Это ограничение трёх прочитанных файлов, а не заявление об отсутствии такого переноса в истории.

## 2. Какие результаты original RP01 нужно добавить к истории

Документ 16 относится к core `83590a462b2f8aedc9d3ab4cb775193eb9c25ce5`, опытам 15–16 октября. Он указывает первичные `scientific-result.json` с blob `f0947c380cd01ad5885dfd64eedb41e25b87f7b2` для C# и `04f09782aa521c97d5a9cd957408d03fc0dc28ee` для ENRU.[1]

| Ось | C# | EN→RU | Интерпретация для истории |
| --- | --- | --- | --- |
| Обучение и начальное состояние | 500 пар, 50 updates | 500 завершённых IDs, 50 новых updates, итоговый global step 100 | ENRU продолжает C50; имя baseline `C0` в его schema не означает исходную B0 |
| LoRA / frozen base | Изменены 16/16 LoRA, base 0/39 | Изменены 16/16 LoRA, base 0/39 | Свидетельство настоящего обновления параметров; тезис «обучения не было» неверен для этих summaries |
| Средний preference margin | 0,03733304977416992→0,03856058120727539; Δ+0,0012275314331054651 | 0,012293052673339844→0,012403059005737304; Δ+0,00011000633239746093 | **Успех по этой метрике и конкретному профилю**, без требования роста accuracy |
| Разнонаправленные изменения cases | 14 улучшений, 11 ухудшений из 25 | 11 улучшений, 9 ухудшений из 20 | Сохранять регрессии рядом со средним |
| Chosen выше rejected | 16/25→16/25 | 12/20→12/20 | Переходов через нулевой margin не добавилось |
| Правильные свободные ответы | 0/3→0/3; все 3 raw-пары совпали | 0/4→0/4 | Улучшение полезной генерации не измерено |
| Сохранение результата | Полный checkpoint/optimizer/RNG открыт заново | Reopened state равен live, frozen C50 reference неизменён | Нельзя объяснять эти два результата универсальной потерей состояния при restore |

Все числа этой таблицы — документальный перенос из нового readback, без нового запуска.[3] C# retention после ENRU остаётся 0/3→0/3 с одинаковым decoded UTF-8: это сохранение невалидных ответов, а не доказанное удержание исходно полезного навыка. C# preference panel использует новые экземпляры тех же шаблонов; число независимых семантических семейств и значимость не установлены. Раскрытая ENRU diagnostic panel не становится новой слепой панелью.[3]

**Существенная граница initial model:** сохранённая B0 была случайной и необученной; `LoadModels` не создавал её заново на каждом запуске. Это аргумент для уже известных warm-start гипотез, но не доказанная единственная причина слабой генерации. Внутреннее детерминистическое происхождение simulation-пар было разрешено отдельным work order; его нельзя задним числом объявить скрытым внешним teacher либо нарушением этого исследовательского режима.[4]

Стоимость двух доменных этапов не образует скоростной A/B. В C# serialization 16,5683001с и optimizer/RNG save 2,0476702с уже вложены в training step total 6238,0783751с. ENRU 1000 policy и 1000 reference forwards выведены структурно; runtime counters для них отсутствуют. Ни отдельный чистый optimizer kernel, ни полный campaign/API budget из этих сведений не восстанавливаются.[5]

## 3. Что добавляет current DPO→Units readback

Документ 17 — **source evidence на core 883496, NATIVE_NOT_RUN**, не новая measured success. Его следует добавить к причинной карте, сохраняя разделение learners:[6]

- **Structured memory path:** `BuildGeneralBatch` превращает часть rejected-side RULE/PHRASE/AGREEMENT annotations в положительные targets, затем реальные Unit trainers записывают chosen memory и обычный flow её читает. Полезные phrase-repair annotations действительно встречаются в fixture; удаление всего rejected способно удалить полезную supervision. Три Unit examples происходят из одной пары и не равны трём независимым урокам. Данный tokenizer producer воспроизводит собственный pattern, но это не доказательство принципиальной необучаемости самого Unit.
- **Neural translation path:** `TranslationDpoTrainingHandoff` берёт пары из исходных corpus rows, bridge вызывает настоящий `StepMany`; это не обновляет три memory-Units. Текущий featurizer уже удаляет WRONG и PHRASE/AGREEMENT diagnostic blocks; blanket baseline «WRONG всегда попадает в model bytes» неприменим к нему. Annotation-only строка после stripping может быть отклонена до tensor training — это вывод по source, не измеренный run.
- **Граница домена и маршрута:** этот bridge использует regex/hashed IDs и budget truncation, тогда как original RP01 использует BPE exact-v2. Его свойства нельзя автоматически сделать причиной original RP01 0/3 или 0/4. Поддержка шести routing directions не является доказательством успешного обучения всех шести языковых направлений.[6]

Это уточняет существующие P52/P106/P125/P54 и не назначает новых карточек или runtime. Основной документ очереди проверяется отдельно: исторические next-ID и status-фразы из этих источников здесь не превращаются в команды повтора. Старые документы/результаты сохранены, изменения production/protected и обучение не выполнялись.

[1]: https://github.com/lubochka/xiigen-planning/blob/53cbcabd3926cda714bab428d294eb666301e9e4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-delta-audit-883496/16-RP01-primary-readback.md#L3-L11
[2]: https://github.com/lubochka/xiigen-planning/blob/53cbcabd3926cda714bab428d294eb666301e9e4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-delta-audit-883496/evidence/e09-transfer-commit-receipt.json#L2-L25
[3]: https://github.com/lubochka/xiigen-planning/blob/53cbcabd3926cda714bab428d294eb666301e9e4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-delta-audit-883496/16-RP01-primary-readback.md#L15-L31
[4]: https://github.com/lubochka/xiigen-planning/blob/53cbcabd3926cda714bab428d294eb666301e9e4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-delta-audit-883496/16-RP01-primary-readback.md#L35-L44
[5]: https://github.com/lubochka/xiigen-planning/blob/53cbcabd3926cda714bab428d294eb666301e9e4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-delta-audit-883496/16-RP01-primary-readback.md#L48-L61
[6]: https://github.com/lubochka/xiigen-planning/blob/53cbcabd3926cda714bab428d294eb666301e9e4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-delta-audit-883496/17-current-dpo-unit-transfer.md#L3-L72
