# Источники, новизна и независимая проверка

## 1. Зафиксированные срезы и охват

| Объект | Закреплённый commit | Как использован |
|---|---|---|
| `llm_mvp_core@DesignToCode-clean` | `e346b88a3cc1fcce48aefa626bfc7b00509eefd6` | Главный успешный донор пользователя: исходники, raw receipts, непосредственное сравнение с current |
| `llm_mvp_core@codex/dpo-implementation` | `358af351e22a92a3f7938d1f9221d5aeb7ddb053` | Нынешние формулы, callers/consumers, защищённый реестр прежних исследований и уже выполненных исправлений |
| `xiigen-planning@llm_plan3`, исходный срез | `b0308fbaa783758104987e6544d057062ccf6fc3` | RP01 pack, RAG-навигация, первоначальный possibleDifs и требования к исследованию |
| Planning после параллельных публикаций | `64ade14ed5dfb799c5e3edc7ac44acdc957aabcd` | 33 новые/изменённые Markdown-версии, повторная дедупликация N54/53 и сохранение общего указателя |

Обе основные core ветки повторно проверены перед сборкой и сохраняли указанные heads. Ветки `DesignToCode`, `PrepareToGPL`, `external_domains`, `language_translate`, `local_wiki_planning`, `orchestrator-extensions` и другие перечислены в [branch-heads.json](evidence/branch-heads.json). Это инвентаризация доступных линий и источник для адресных сопоставлений, **не заявление о полном построчном аудите каждой ветки**. Соседние положительные результаты имеют отдельную родословную и не переименованы в clean-native запуск.

Получено 1 612 файлов, чьи локальные байты сверены с Git blob SHA; 798 из них — Markdown. Для дедупликации выделены **415 Markdown planning и 274 Markdown protected core: 689 исходных документов**. Перед публикацией сверены ещё **33 изменённые/новые версии** planning. После замены двух старых путей — **720 уникальных Markdown-путей**. Все тексты доступны поиску; первичные исходники и близкие прежние объяснения читались адресно. Этот счётчик не означает 720 полных экспертных reviews каждой реализации. [Manifest исходного корпуса](evidence/prior-corpus-manifest.json), [manifest публикационной дельты](evidence/publication-delta-manifest.json).

Скопированные из рабочих кэшей материалы принимались только при совпадении с заново полученными Git tree/blob metadata. Для несовпадений CRLF/BOM/UTF-16 повторно получены исходные bytes; нормализация текста не использована как доказательство Git-идентичности. Бинарные сборки из прежних опытов заново не исполнялись и не выдаются за нынешний runtime.

Полный GitHub compare clean/current имеет разошедшуюся историю, 806 commits ahead и одну behind; список изменённых файлов ограничен 300 записями. Из него не делался вывод о полноте diff. Для выбранных путей использованы scoped tree, конкретный blob и исходный текст. Например, одинаковый `DesignConversionTrainedUnits` или BPE доказывает равенство конкретного файла, но не всех callers и конфигураций вокруг него.

## 2. Новизна после свежей дельты

| Проверка | Окончательный статус | Связь с успешной веткой |
|---|---|---|
| N-K1: IMPERATIVE_VS удаляет объектные PRON | Новый точный контрпример в известной семье E27; exact предшественник не найден | Унаследован, не новая RP01 регрессия |
| N-K2: перенос одного слова разрывает именную группу | Новый точный контрпример; прежняя слабость IsSVO известна | Унаследован; ограничение переноса положительного helper на более сложные фразы |
| N-K3: While/Do не входят в Loop next-method label | Новая условная проверка покрытия меток | Унаследован; при узком legacy-контракте это расширение, не баг |
| U-DTC-1b: vector mean/dimension отсутствуют в ordinary Critic | Новое точное сравнение feature maps; другая hash-diversity train/serve проблема уже описана N54 | Унаследованный локальный feature learner; отдельная neural policy не охвачена |
| U-DTC-3: округлённый mean становится восстановленной суммой | Новая конкретная операция потери остатка; качество пока не измерено | Унаследован; низкий приоритет до смены реального выбора |
| N-ADJOINT-01: scalar→SIMD backward gradA | Новый точный численный diff, гипотеза влияния, не утверждение об ошибке SIMD | Реальное изменение clean→current в достигнутой neural call-chain |
| U-DTC-1a: пара не меняет локальные Critic deltas | **Подтверждение опубликованного N54; самостоятельная новизна снята** | Унаследованный локальный recipient; добавлены отдельная трасса и scope bridge |
| U-DTC-2: negative correction возвращается через fallback | **Конкретизация уже опубликованного fallback N54; самостоятельная новизна снята** | Унаследован; новый отрицательный контрпример к тому же selector |

Итого — **шесть точечных проверок без найденного точного предшественника в просмотренном корпусе и две подтверждающие/уточняющие проверки**. Это ограниченная заявка на новизну, не глобальное утверждение обо всех скрытых ветках и документах. Название нового файла или новое имя check не считаются доказательством нового механизма.

Полное решение с immutable ссылками на совпавшие строки N54 находится в [независимом publication-delta review](evidence/review/review-publication-delta.md); [поисковые совпадения](evidence/review/review-publication-delta-search.json) сохранены. Reviewer проверил 31 содержательную Markdown-версию, root дополнительно прочитал два навигационных README. Первоначальные reviews честно оставлены с их прежними SHA и выводами на исходном корпусе; окончательная классификация этой главы имеет приоритет в вопросе новизны.

## 3. Что действительно вычислено

| Приложение | Фактическое действие | Результат и предел |
|---|---|---|
| [KNN proxy](evidence/knn/knn_structural_proxy_results.json) | Python-трасса выбранных ветвей списка и label mapping на размеченных fixtures | 2/4 PRON cases теряют нужные слова; 2/4 VSO cases разрывают группу и проходят IsSVO; 2/5 KindBag меняют label при условном расширении. Это не native parser и не measured learning accuracy |
| [Units probe](evidence/units/dtc-source-probe-result.json) | Формулы проекции пары, пересечения признаков, fallback и accumulator | Margin может поменять знак без изменения Critic deltas; feature-fit/ordinary score различаются; negative rule может вернуться; warm/cold mean расходятся. Все fixtures искусственные |
| [DPO arithmetic и archive readback](evidence/dpo/arithmetic-and-retained-receipts.json) | FP32 арифметическая иллюстрация и сопоставление полных сохранённых Wiki triples | Scalar/chunk-round имеют разные остатки в constructed case; raw Wiki023/025 совпадают 50/50, positive 49/50→49/50. .NET Vector.Dot и реальные q/v gradients не исполнялись |
| [Root source verification](evidence/review/root-source-checks.json) | Повтор Git blob/SHA256 сверки 95 source records: 26 KNN, 23 Units, 38 DPO, 6 historical receipts и 2 runners | Все совпали. Дополнительно проверен artificial category пример `+1,count5; −1,−1`: warm .428571/cold .428572. Это state arithmetic, не UI outcome |
| [Независимый повтор DPO приложения](evidence/review/root-dpo-rerun.json) | Root повторил опубликованный переносимый скрипт с проверкой четырёх реально читаемых первичных файлов | Итоговый JSON байт-в-байт совпал. Повтор не добавляет новую выборку и не является обучением |

Поиск/сравнение, вычисление исходной формулы, native execution и прирост предметного качества — разные уровни доказательства. **Новых Student/Teacher/judge вызовов, .NET/native experiments, обучения или inference в этой работе нет.** Сохранённые положительные и отрицательные чужие results процитированы как исторические, с их датой, знаменателем и consumer.

## 4. Независимые проверки и внесённые исправления

Три исследовательские области разрабатывались отдельно; затем KNN и Units проверены не их авторами, DPO часть отдельно перепроверена root. Это фактически выполненный review ограниченного исследовательского пакета, не аттестация всей программы или всех процедур продукта.

| Проверка | Что подтвердилось | Что изменено при интеграции |
|---|---|---|
| [Units reviewer → KNN](evidence/review/review-knn.md) | Primary E25/E27 owner, PRON/span/Loop источники; 26 exact source blobs; повтор исходного proxy | Harness теперь проверяет ожидаемые hashes до вычисления; next-method claim сужен до голосов соседей; N-K2/3 получили явные 2 урока/4 EXAM |
| [KNN reviewer → Units](evidence/review/review-units.md) | Полные admitted/update/runtime/save/load цепи; 23 exact blobs; отсутствие подмены feature-fit визуальной accuracy | Для rounding добавлены реально допустимые ±1 contributions и условие читаемого feature. Negative weight не назван абсолютным запретом. В главе00 E25 включён, supplemental lineage вынесена отдельно |
| [DPO reviewer → README/00/04](evidence/review/review-foundations.md) | Различие memory/neural consumers, сохранение autograd fixes, границы исторических metrics и опыта | Bias-off назван контролем отсутствия регрессии; ссылка fractional mask заменена на настоящий live report; cold restore учтён отдельно в бюджете |
| [Независимая свежая дедупликация](evidence/review/review-publication-delta.md) | 31 delta Markdown сверены с Git blobs и прочитаны по предметным совпадениям | У двух checks снята независимая новизна; U-DTC-1b связан с предшествующим hash-diversity contrast |

Обновление KNN harness прошло [отрицательный контроль](evidence/knn/proxy-harness-verification.json): обе локальные версии намеренно одинаково изменены, то есть parity сохранилась, но ожидаемый blob не совпал. Скрипт завершился до proxy и не создал output. Это проверка доказательного приложения, не новый тест модели. Режим `--repo` использует только `git show`; в этой сессии для KNN он не исполнялся, выполнен режим materialized source-root.

Reviews содержат точные SHA256 рассмотренных drafts. Поздние правки root не названы тайно тем же draft; их содержание и закрытые замечания описаны выше. Подробные артефакты нужны для проверки утверждений, а не для увеличения счётчика проверок.

## 5. Применённые project skills

Использованы навыки из указанного пользователем `llm_mvp_core/docs/skills`, на закреплённом current. У больших файлов прочитаны применимые к исследованию разделы; не заявляется буквальное исполнение всех продуктовых release gates или всех возможных review-раундов.

| Skill | Применённое требование и видимый результат |
|---|---|
| [trainable-architecture-decision](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/trainable-architecture-decision-SKILL.md) | Найти настоящего владельца изменяемого состояния и потребителя; не создавать новый Unit ради одного numerical kernel |
| [human-readable-architecture-audit](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/human-readable-architecture-audit-SKILL.md) | Объяснить, какое пользовательское умение теряется, показать путь урока и контрсвидетельство рядом с выводом |
| [phase-planning](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/phase-planning-SKILL.md) и [plan-review](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/plan-review-SKILL.md) | Один механизм A/B, явные зависимости, новое наблюдение и falsifier; небольшие альтернативные опыты вместо обязательной большой матрицы |
| [generated-code-review](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/generated-code-review-SKILL.md) | Проверить реальный caller, формулу и consumer, а не вывод по имени класса или слову trained |
| [prompt-patch-authoring](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/prompt-patch-authoring-SKILL.md) | Разделить выбор урока, авторство обеих сторон и независимую оценку; точные hashes, конкретное поручение в главе05 |
| [human-communication glossary](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/human-communication-GLOSSARY.md) | Домен — предметная тема; kNN/LoRA/Unit — механизмы; понятная русская формулировка, честные границы положительных и отрицательных результатов |

## 6. Финальная интеграция и последние редакции

[Независимый closing review итоговых README/00/02/05/06](evidence/review/review-final-integrated.md) дал `PASS_BOUNDED` без оставшихся содержательных замечаний и зафиксировал SHA рассмотренных файлов. После него planning обновился до `b95278b25a6697b42ef7025e6965a9f2837a34b7`: ещё шесть редакций уже существующих Markdown и два evidence-файла. Root прочитал точную дельту и сверил обе версии всех восьми blobs. Новых путей и новых пересечений оставшихся шести checks не добавилось; итог **720 уникальных Markdown** сохраняется. [Отдельный ledger и внесённые технические/содержательные уточнения](evidence/review/last-head-delta-review.md). Последняя редакция ДР-Д в главе00 — интеграция уже известного N54, не новый результат обучения.

## 7. Как повторить приложения

Требуются уже доступные локальные исходники, соответствующие pinned refs. Скрипты не скачивают репозиторий, не обходят доступ и не запускают модель. Из корня этого пакета:

```bash
python evidence/knn/knn_structural_proxy.py --repo /path/to/llm_mvp_core
python evidence/units/dtc_source_probe.py --repo /path/to/llm_mvp_core
python evidence/dpo/read_only_evidence.py --source-root /path/to/DesignToCode-clean
```

KNN также принимает `--source-root` с `current/` и `clean/` внутри; DPO читает оригинальные UTF-16LE bytes соответствующих raw batches и проверяет их до decode. Манифесты лежат рядом со скриптами. Выход по умолчанию — аналитический JSON рядом с приложением, отдельно от входных исходников. Для KNN/DPO можно указать собственный `--output`.

Открытые ограничения остаются конкретными: не установлено, что все новые helpers достигаются обычным входом RP01; не измерена семантическая цена малых numerical/state дельт; старый binary SHA не всегда записан в архиве; перенос структуры на иной язык требует собственного контракта. Эти ограничения не удаляют положительные старые результаты. Они определяют, какой из маленьких [восстановительных опытов](00-RESTORE-DESIGNTOCODE-CLEAN-FIRST.md) даст следующий полезный ответ.
