# MICRO-N35 — за один короткий native запуск различить потерю отрицания, повышение confidence и их взаимодействие

<!-- CODEX-POST125-PLANNING:START -->
## Адресное уточнение после опытов P118–P125 — 3 ноября 2026

Применение к этой карточке: **`RESTORE-ACTUAL-CHECKPOINT`**. Источник ошибки проверен на core `9d4d79d27f18066ce3a346fac2a5eea156304f62`; это профилактическое усиление соответствующей границы, не утверждение о новом исполнении исторического опыта этой карточки.

P12501 уже исполнен: 0 → 19/24, после загрузки 19/24; новый процесс и 24/24 буквального совпадения подтверждаются сохранёнными файлами. Это отдельный состоявшийся прогон на тех же уроках и панели P2502; он не разрешает повторять P2502 и не является карточкой P125-executor-memory-learning.

**Хороший пример и действие проверяющего:** Читать существующие checkpoint, memory-trained/restored, source-provenance и 72 ответа. Один и тот же независимый сравниватель проверяет exact key с Topic, phrase key и KNN key/target, версии, полный состав с кратностью записей, происхождение и значимый порядок, затем обычные ответы. Все обязательные результаты должны влиять на reader-status, общий outcome и exit.

**Плохой пример и требуемый отказ:** Поменять только Topic одной exact-записи или KNN target при прежних Source/Target соседних записей и числе 48: тот же независимый сравниватель должен отказать. Проверка наличия строки target где-либо в JSON не подходит.

**Предел вывода:** Не переучивать память. 19/24 — результат именно использованной сборки; source-provenance не подтверждает исполнение свежего HEAD. Восемь уроков не содержат отрицаний и вопросов: способность к ним не измерена. Наблюдённые cc01/cc04 и одинаковая уверенность 0,99 остаются фактами; снятую с очереди продуктовую правку по отрицанию не возвращать.

**Добавить к передаваемому заданию:** выполнить вышеописанную проверку через того же рабочего потребителя и общий finalizer. Сначала полный честный пакет → принятие, затем ровно одна порча → отказ с конкретным полем. Использовать существующие результаты; этот аудит не назначает Train, новый PRIMARY, изменение gold или увеличение cap. Происхождение файла, actual state и исполнявшаяся сборка называются отдельно.

[Сводка сверки](codex-2026-11-03-POST125-TRANSFER-AND-EXPERIMENT-AUDIT.md) · [владелец в рабочем репозитории](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P25-cold-restore.md). Нижние исходные данные, результаты, команды и исторические выводы сохранены.
<!-- CODEX-POST125-PLANNING:END -->

**16.10.2026 · DRAFT / EXPERIMENT PROTOCOL ONLY / .NET NOT_RUN / CODEX/Astra quality grade NOT_RUN.** Подготовлено для `llm_mvp_core` `codex/dpo-implementation` [@216a1f3d](https://github.com/lubochka/llm_mvp_core/tree/216a1f3d6af3f40ceef081dc168b07e4ce3934f9). План и контроль: [N35](35-NEW-NEGATION-OMISSION-WORDMAP-TO-099-CONFIDENCE-2026-10-16.md). **Не запускать 500 DPO, не менять native product из этого файла; сначала исполнить диагностический микроэксперимент.**

## 1. Два разных уровня проверки

**Механическая source проверка (Tier 0):** ожидаемая ветвь `ExecutorUnit.TryComposeFromWordMappings` для source `never delete records`, если ровно два слова получают перевод: `mappedTokens=2`, `translatableTokenCount=3`, `mappedRatio=0.6667`, `Confidence=0.50`; пустой `QualityVerifierUnit` выдаёт `Issues=[]`, `Pass=true`, `Score=1`; adapter устанавливает `Confidence=0.99`. Это _условная арифметика_; продуктовый перевод и его семантика до native не подтверждены.

**Полезность реального механизма (Tier 1):** фиксированный ordinary `TranslationMachine` с одним-двумя легальными `TranslationLearningExample` обучает `executor-en_ru`, но не `quality-verifier`; затем исполняет source-disjoint holdout через `TranslationRoutingFlow` и оценивает пользу по законам «negation preserved», «target language correct», «meaning preserved», «no unknown high-confidence assertion». Дополнительная arm использует shadow test-only policy, не подменяя сохранённое checkpoint.

## 2. Паспорт причинного опыта

| Поле | Фиксация ДО запуска |
| --- | --- |
| ID | `MICRO-N35-WORDMAP-NEGATION-CONFIDENCE` |
| Baseline | `TranslationMachine()` + `TrainStructuredBatchAsync` на паре EN_RU, **без** VerifierUnit обучающих примеров; одинаковый source SHA |
| Минимальные train-only | `delete records`→`удалить записи` с `WordMappings=[delete→удалить, records→записи]`; при необходимости второй грамматически корректный позитивный пример другого source family |
| Контроль tokenizer | вывести `SemanticTokenSequence.Tokens` для `never delete records`; не допускать подгонки токенов/числителя под gold; если токенов или branch иначе — зарегистрировать NOT_TRIGGERED |
| Eval freeze | 12 случаев: 3 утвердительных in-domain, 3 с `never/not/no`, 3 с `without/do not`, 3 с морфологическими или order/scope отрицаниями; как минимум 6 source-family disjoint от train; provenance/хеш/роль/направление |
| 2×2 | A: old unknown-policy + old confidence; B: negator-preserving refusal *only*; C: no-rule confidence calibration *only*; D: оба фактора |
| неизменно | Train data, seed, tokenizer, learned state, KNN store, source/version ID, exact selected same 12 IDs, expected domain labels, latency budget |
| output | `receipt.json` + compact `cases.jsonl` без скрытых ответов экзамена в prompt автора данных; отдельная quality карточка |
| decision | Никакой code/product ACCEPT по native `PASS` без frozen blind quality / regression/coverage и независимого CODEX/Astra |

## 3. Алгоритм (minimal source-owned harness, без реализации новых feature до диагностики)

1. **Source and authority.** Прочитать root/slice `AGENTS.md`, [translation trained Units](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/27.TranslationDomain/TranslationTrainedUnits.cs), [adapter](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/27.TranslationDomain/TranslationDirectionAdapters.cs), [machine](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/27.TranslationDomain/TranslationMachine.cs). Проверить неизменность pinned commit перед запуском и право писать только test/probe artifacts в разрешённом окружении.
2. **First diagnostic witness.** Не вручную «нарисовать» успешно прошедший pipeline: через штатный `TrainStructuredBatchAsync` подготовить one-case map, вывести `UnitId`, chosen payload, `_wordMap` observable snapshot через export, `sourceTokens`. Отдельным unit-level branch fixture можно форсировать 3 semantic tokens **только как структурный контроль**, не считать product quality.
3. **Before / after on same cases.** На baseline до обучения сохранить ответы, adapter ID, confidence, verifier issues; после обученного map сохранить те же поля. Сравнить с полным ответом, а не только с map accuracy. Результат `never delete records` в виде `удалить записи` — заведомое изменение полярности, но окончательное semantic решение принимает независимый judge.
4. **A/B one-factor: lexical loss.** Arm B меняет только low-information unknown-negator rule в shadow `ExecutorUnit`: если в source присутствует критическая частица и она не представлена в candidate, вернуть typed `ABSTAIN_NEGATION_UNSUPPORTED` либо другой допустимый learned negative candidate; сохранять исходный Verifier score/candidate selection metadata. Не «исправлять» строку из gold.
5. **A/B one-factor: authority.** Arm C сохраняет текст и выбор кандидата A, но для нулевых learned checks записывает `NOT_ASSESSED` вместо `Score=1`; downstream confidence запрещено повышать выше исходной .50 на основании отсутствующих правил. This is **calibration**, not semantic repair.
6. **Interaction.** D сочетает B и C, сравнить paired cases и interdependence; исключить ситуацию, когда исправлен только показанный confidence, а ошибочная команда продолжает возвращаться.
7. **Cold restore.** `SaveCheckpointAsync` → dispose/reopen `LoadCheckpointAsync` → те же 12 requests, hash/config + token dump + outputs; новые статусы `RESTORE_MISMATCH` vs `QUALITY_REGRESSION`. Не использовать `ImportAsync` metadata-only как восстановление learned Units.
8. **Blind verdict.** CODEX с **Astra 6 UltraCode** отдельно оценивает truth/failure per domain и per case. Измерить `lawful_correct_and_returned / N`, `unlawful_returned / N`, `abstain / N`, `accepted_wrong_at_confidence>=.9 / N`, target language, worst domain. Не считать `Score=1` ground truth.

## 4. 12 замороженных ролей тестов (схемы, не скрытые экзаменационные ответы)

- Affirmative EN_RU ×3: source, где отрицания нет; обязаны остаться переводимыми.
- `never` / `not` / `no` ×3: надо сохранить область отрицания при сохранённых соседних известных словах.
- `without` / imperative `do not` ×3: составные запретительные конструкции, чтобы не переобучиться на отдельное `never`.
- Sentence order / scope / alternate object ×3: изменение только позиции отрицания, полярности, scope и одного content слова (source-family holdout).
- Негативный контроль: отсутствие выбранных word mappings либо `mappedRatio<.50` должно дать **не тот же** `-wordmap` trace; отдельный phrase-map контроль обязателен для обнаружения короткого замыкания ветки.
- Добавить EN_RU homograph `not on` и domain `not authorized` как расширение после первого falsifier, но **не** подмешивать в frozen primary.

**Возможные переносы:** для code docs C#/Angular/SQL аналог закона `!=`, `!is`, `NOT EXISTS`, `IS NOT NULL`; нужен AST/typed-operator oracle, не общий эвристический переводчик. Для Figma HTML/CSS эта полярность не является обязательным признаком: проверить другие геометрические/структурные constraints в отдельной задаче.

## 5. Accept/Fail и чёткое отрицательное знание

- **Mechanism TRIGGERED:** 3 source tokens, 2 accepted mappings, `-wordmap`, missing negation, empty verifier, adapter confidence `.99`, flow `TRANSLATED` — всё в одном production-ownership trace; иначе `NOT_TRIGGERED` с объяснением на каком звене.
- **Quality harm SUPPORTED:** на замороженных исходных задачах A возвращает незаконную по смыслу полярность; B снижает такие ошибки без недопустимого роста abstain на lawful affirmative. Иначе `NOT_SUPPORTED`.
- **Confidence mismatch SUPPORTED:** C сохраняет те же raw texts, но убирает высокий confidence у неверных ответов; это предотвращает false confidence, но НЕ гарантирует исправления перевода.
- **Repair candidate:** только если D даёт семантическое улучшение с измеренными costs/regressions; независимый review всё ещё обязателен.
- **Regression controls:** two mapped / one mapped, 3+ mapped, no-negation, negation inside quoted identifier, code identifiers, punctuation, RU_EN vs EN_RU, new checkpoint reopened, two more domains.
- **Stop:** если нет learned Unit recipient или `-wordmap` не достигается на фактическом обычном consumer, не переносить ни один статус на RP01 neural Student.

## 6. Важная граница создания DPO

Здесь разрешённые ручные `WordMappings` — тестовый probe для проверки `ExecutorUnit`, **не** DPO-пара от учителя. Если после диагностики нужны реальные новые DPO, **внешняя открытая китайская модель создаёт и chosen, и rejected**; **CODEX (Astra 6 UltraCode)** делегированно выбирает реальные teaching cases, независимо проверяет пару и отдельно оценивает Student/Unit. Ни одна synthetic/proxy пара не должна маркироваться как китайская. DPO обучать только после законной admission/provenance; не брать тексты закрытых exam cases в training.
