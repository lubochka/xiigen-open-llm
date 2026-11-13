# Возможные расхождения RP01 / исследование №12: идентичность corrections и атомарность обучения Units

Дата: 2026-10-16. Статус: **DRAFT; code-source audit; native probe NOT_RUN; quality NOT_MEASURED; independent CODEX/Astra review NOT_DONE**.

Снимок кода, по которому проверены утверждения: `llm_mvp_core@0eccbdc3c4efb90adf1d699806cbfa9f5610998d` (`codex/dpo-implementation`). Соседняя полная работа: [planning: N23 новые гипотезы](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/23-NEW-UNIT-CASE-IDENTITY-AND-NONATOMIC-TRAINING-2026-10-16.md> "Исторический ref llm_plan3; локальный файл может отличаться") и [микроэксперимент](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/23-MICRO-UNIT-IDENTITY-AND-ATOMICITY-2026-10-16.md> "Исторический ref llm_plan3; локальный файл может отличаться").

## Зачем нужна эта проверка

Нельзя приравнивать обучение отдельного Student RP01 посредством DPO/LoRA к прежнему обучаемому переводу через `SemanticTokenizerUnit` + `ExecutorUnit` (exact/phrase/word/KNN) + `QualityVerifierUnit`. Первый route действительно менял LoRA; free generation в сохранённых RP01 exams не улучшилась (C# 0/3→0/3, ENRU 0/4→0/4). Во втором route обученная память могла улучшать конкретные ответы независимо от градиентного Student. Исторические E09/E14/E34 и cross-branch DPO evidence см. [11 actual training results](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md> "Исторический ref llm_plan3; локальный файл может отличаться") и [12 matrix→KNN](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md> "Исторический ref llm_plan3; локальный файл может отличаться").

Здесь намеренно проверяется *ещё два*, более низкоуровневых механизма ухудшения качества Unit, а не повторяются уже описанные missing SourceLang, vector active-count, first-hit kNN, BPE vocabulary drift, no-op-training, false-synthetic accuracy.

## N23.01 — регистр источника теряется в learned correction identity

**Свидетельство по коду:**
- [`27.TranslationDomain/TranslationTrainedUnits.cs` L80–95](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:80> "Исторический ref 0eccbdc3c4efb90adf1d699806cbfa9f5610998d, L80-L95; локальный файл может отличаться"): хранилище `_signatureCorrections` сравнивает ключи `StringComparer.OrdinalIgnoreCase`.
- [там же L124–148](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:124> "Исторический ref 0eccbdc3c4efb90adf1d699806cbfa9f5610998d, L124-L148; локальный файл может отличаться"): `TrainOnExample` присваивает correction по `Normalize(Input.RawText)` (whitespace-only), не сохраняя отдельную case identity.
- [`TranslationCheckpointContracts.cs` L75–95](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationCheckpointContracts.cs:75> "Исторический ref 0eccbdc3c4efb90adf1d699806cbfa9f5610998d, L75-L95; локальный файл может отличаться"): экспорт только одного нормализованного текстового ключа/коррекции.

Следствие: `"US"` и `"us"` не могут сохранить два различных корректирующих `TokenizerOutput` в этом словаре; новая запись заменяет предыдущую. Это **верифицируемая по коду коллизия**. Полезность обоих correction для реального задания зависит от законов домена. **Отдельно от N16:** здесь SourceLang фиксирован одинаковым; расхождение вызвано `OrdinalIgnoreCase`. **Отдельно от N21:** сам generator должен производить oracle-corrected pattern, а не bootstrap self-label, иначе различие невозможно оценить.

Проверка: две строки и порядок A→B / B→A, same-language, контролируемый `SentencePattern`, compare `Tokenize(A/B)` и export corrections; затем shadow composite `(SourceLang, Ordinal normalized text)`. Показать paired semantic gains на фиксированном blind-panel с двумя значимыми по регистру доменными законами, иначе не объявлять улучшением реального обучения.

## N23.02 — частично применённый batch меняет Unit до сообщения об успешном обучении

**Свидетельство по коду:**
- [`TranslationMachine.TrainStructuredBatchAsync` L89–145](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationMachine.cs:89> "Исторический ref 0eccbdc3c4efb90adf1d699806cbfa9f5610998d, L89-L145; локальный файл может отличаться") последовательно вызывает `trainer.TrainOnExample` на том же runtime, но не снимает pre-batch snapshot и не откатывает ранее применённые Unit state при exception.
- [`SemanticTokenizerUnit.TrainOnExample` L124–139](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:124> "Исторический ref 0eccbdc3c4efb90adf1d699806cbfa9f5610998d, L124-L139; локальный файл может отличаться") может бросить `JsonException` из десериализации `ChosenOutputJson` второй строки.
- [`TranslationMachine.ExportCheckpoint` L260–312](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationMachine.cs:260> "Исторический ref 0eccbdc3c4efb90adf1d699806cbfa9f5610998d, L260-L312; локальный файл может отличаться") экспортирует Unit state даже после прежнего частичного мутирования; `TrainingHistory` и `HasStructuredTraining` выставляются после завершения полного batch.
- Отдельный `ImportCheckpoint` [L314–345](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationMachine.cs:314> "Исторический ref 0eccbdc3c4efb90adf1d699806cbfa9f5610998d, L314-L345; локальный файл может отличаться") предварительно валидирует checkpoint; **не переносить** проблему training transaction на весь checkpoint code.

Два примера в чистой локальной машине: A валиден и меняет learned correction; B имеет malformed JSON. Ожидается исключение, но state A уже изменён, хотя batch не опубликован как успешный. Наблюдать `_hasStructuredTraining=false`, отсутствующий успешный batch history и изменённую `SemanticTokenizer.SignatureCorrections`. Контроль: invalid B как первая запись не должен мутировать, два валидных примера должны успешно обучаться. Shadow treatment: preflight всех примеров + clone/copy-on-write apply + atomic adoption; reject leaves identical state hash, но successful arm не деградирует.

**Причинная квалификация:** исходник доказывает возможность последовательной partial mutation; истинную частоту malformed batches, достижимость из публичного API и вклад в плохое качество RP01 НЕ установили. Его DPO StepVector отдельно публикуется транзакционно и не наследует этот риск по умолчанию.

## Минимальная исполнительная программа

| Шаг | Прямое наблюдение | Gate |
| --- | --- | --- |
| 1 | 2 valid same-language/case-different corrections; A/B order swap | Коллизия воспроизведена реальным `SemanticTokenizerUnit` или `NOT_REPRODUCED` |
| 2 | `TranslationMachine` with valid A then malformed B | Exception + post-failure unit state diff vs no diff |
| 3 | Shadow guarded versions only, same fixtures and initial state | Different exact-case retention; no partial mutation; same successful-batch behavior |
| 4 | CODEX/Astra independent blind law review on source-disjoint examples | Отдельно `MECHANISM_REPRODUCED` и `HELDOUT_GAIN_OBSERVED / NO_GAIN_OBSERVED` |
| 5 | Discover real callers/adoption, replay on ordinary consumer | `PRODUCT_CAUSE_SUPPORTED` only if actual call chain and matched user-output delta |

**Время/ресурсы:** адресные native tests (2–3 records), без 500-pair DPO, без Windows CI, без массового переписывания Unit. Сохранить SHA исходного кода, exact serialized inputs, expected/actual outputs, fields changed, evaluation panel, counts of retained valid lessons, CPU/RAM and before/after evidence. Нет кадрового порога «сколько тестов достаточно» вместо предметного качества.

## Формальная задача CODEX — human selector and independent judge

**Human role и обе функции отбора/оценки исполняет CODEX на Astra 6 UltraCode.** Внешняя разрешённая **открытая китайская модель** производит обе стороны (`chosen` и `rejected`) **новых реальных DPO пар**, когда они действительно требуются; CODEX/Astra проверяет правильность пары и отдельно судит реальные Student outputs. Здесь достаточно локального теста с синтетическими контрольными corrections, явно помеченного `SYNTHETIC_MECHANISM_ONLY` — не объявлять synthetic пары выходом китайской модели, не переименовывать исторические данные.

Перед исполнением перечитать `docs/skills/human-readable-architecture-audit-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `generated-code-review-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `learning-path-audit-SKILL.md`. Reviewer должен получить source SHA и raw outputs, посмотреть саму логику подстановки двух строк, отличить no-op/history, и отвергнуть любое claim о RP01 gain без фактической проверки его Student route. **Следующее действие: native MICRO-N23** по linked planning документу, после — независимое ревью и только затем адресная правка кода.

Итог: **два новых проверяемых source-level seams**, quality impact `NOT_MEASURED`, direct RP01 causal link `NOT_PROVED`, publication этой заметки `DRAFT`.
