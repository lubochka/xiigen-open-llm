# Покрытие источников, новизна и границы проверки

**Актуальность и границы:** полный корпус ниже относится к planning53/core827. Последняя адресная проверка — core6e3 и planningeaed: [поздняя сверка](11-late-reconciliation-6e3-eaed.md). Объединённый реестр содержит 42 ID, 7 завершённых и 35 будущих; NEXT=N63-INGRESS-LENGTH. Статистика 799/345/157 не пересчитывалась как якобы полный semantic review последнего нового корпуса.

## Снимки и происхождение

| Источник | Закреплённый ref | Что проверялось |
|---|---|---|
| xiigen-planning / llm_plan3 | `53cbcabd3926cda714bab428d294eb666301e9e4` | Все 799 Markdown possibleDifs; адресные RAG/родительские документы; текущий RP01 контекст. |
| llm_mvp_core / codex/dpo-implementation, первый срез | `66947785d1318f7cbbc88118f3156cdc75001e16` | Все 339 protected Markdown, выбранные actual результаты; native DPO/LoRA/attention/translation writers/readers. |
| Та же core-ветка, позднее обновление | `827a1acdbef95d220886e54f9d36bce0b405c8cd` | Полный protected Markdown snapshot 345; delta 41 Markdown, новые результаты E-OPT, очередь 36, актуальные пути/конфликты. |
| language_translate | `f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` | Actual E09/E25/E26/E43 results и их native producers; отдельные E07/E36 материалы для контекста. |
| PrepareToGPL | `31346d8928be4469b01aaf313780e8f950ff91e2` | Деревья docs/Translation/training-data, TranslationTrainedUnits/TranslationMachine; адресное сопоставление унаследованной реализации. |
| Исторический RP01 | `83590a462b2f8aedc9d3ab4cb775193eb9c25ce5` | Actual scientific-result JSON C# и EN→RU, source-chain из ранее опубликованного primary readback. |

Другие перечисленные соседние ветви и старые обзоры использовались как навигация. Построчное независимое чтение всех ветвей, всех binary payloads и всех RAG-разделов не заявляется. `TranslationTrainedUnits.cs` на current669 и PrepareToGPL совпадает по прочитанным bytes; поэтому унаследованный дефект не следует автоматически называть регрессией, внесённой RP01. Подробнее — [историческое evidence](06-historical-evidence.md).

Полный новый [source receipt](evidence/protected827-source-receipt.json) проверяет 414 git blobs: 345 MD, 68 файлов с расширением JSON, 1 JSONL; mismatch 0. Два JSON-файла — LFS pointers, их большие payloads не были получены. Для числовых выводов использованы actual arm JSON; Git blob SHA указателя не выдаётся за checksum большого результата.

## Что значит «покрыто экспериментами»

В новом каталоге 157 карточек, 125 P-семейств и 470 исторически сопоставленных подслучаев. Это разные единицы: не 470 готовых экспериментов, не 157 проверенных hosts и не 125 завершённых опытов. Малый core-реестр содержит 36 ID; объединённый реестр этого пакета — 42, из них 7 завершённых / 35 будущих. [Аудит 14 правил](03-experiment-validity-audit.md) отделяет текстовые markers от семантического и runtime review.

| Проверка | Старый core669 | Новый core827 | Правильная интерпретация |
|---|---:|---:|---|
| Planning paths буквально упомянуты в protected MD/JSON | 747/799 | 750/799 | Это наличие относительного пути в тексте. |
| Paths без буквального упоминания | 52 | 49 | Не доказывает отсутствие идеи: AST/ROPE/BPE уже перенесены. |
| Text-marker screen карточек | 154 | 157 | Есть текст правил; это не оценка правильности каждого бюджета/таймера/CLI. |
| Полный независимый semantic review всех 133 prompts | Не заявлен | Не заявлен | Проверялись активные конфликты и новые промпты, плюс точные source chains. |

Данные доступны как [старый crosswalk](evidence/planning-protected-crosswalk-66947785.json), [новый crosswalk](evidence/protected827-planning-path-crosswalk.json), [старый card screen](evidence/protected-card-screen-66947785.json), [новый card screen](evidence/protected827-card-marker-screen.json). 49 неупомянутых paths: 31 из пакета 883496 и 18 из 63. Это **не утверждение, что 49 гипотез отсутствуют**. Непокрытый буквальный путь и новая смысловая гипотеза проверяются отдельно.

Семь вариантов P54 имеют подтверждённую связь с learned-state consumer, но точные snapshots/tokenization/masks/host ещё не назначены. Они отмечены `NEEDS_INPUT_FOR_RECIPE`, не переданы исполнителю с поручением самому найти место, материал и команду. Референс P54 не блокирует уже точные прямые учебные опыты.

## Дельта новых находок и существующие владельцы

| Находка | Что новое относительно прочитанного корпуса | Protected покрытие | Действие здесь |
|---|---|---|---|
| ATTN-TRANSPOSE-STRIDES-01 | Цена coordinate arrays на scalar активного attention transpose; не correctness reshape | P110 и обзор 47 — соседнее общее покрытие, этого A/B фактора не найдено | Новая отдельная карточка/промпт, source manifest с 36 paths; NOT_RUN. |
| JSON-OPTIONS-REUSE-01 | Lifetime options на каждый Executor lesson | P125 описывает actual memory learning, но этот cost-factor не найден | Новый recipe/host appendix/промпт; NOT_RUN. |
| Verifier checkpoint test без learned rule | Точный helper Pass=true плюс writer !Pass и слабая финальная assertion | P25/P56 уже существуют; этот witness не найден в 799+339 поиске | Evidence addendum, без нового algorithm / run ID и без расширения N60-E1. |
| E-OPT raw labels от R1 | Actual сохранённые factor/path literals не соответствуют source | Свежий EOPT уже существует; raw сохранён; на core6e3 уже добавлен erratum | Additive mapping и ограничение выводов; без rerun/raw edits. |
| E-OPT GC1/2 и overclaim | Пропущены полезные показатели; сильная формулировка шире measured panel | Report и поздний erratum имеются; исходный overclaim ограничен | Восстановлены −5/−3 GC, peak-регрессия и ограниченная формулировка. |
| KN-router window | Изменённый ScoreAsync вне TRAIN таймера | Известный peer C4, остаётся в буквальном рецепте 827 | Исторический observer сохранён; повторно не применять к исправленной core6e3 карточке. Не новая гипотеза. |

ATTN дедуп: [queries/совпадения](ATTN-TRANSPOSE-STRIDES-01-NOVELTY-799.json); [source identity669→827](ATTN-TRANSPOSE-STRIDES-01-LATE-SOURCE-IDENTITY.json). JSON и verifier дедуп/неповторённые идеи — в [историческом обзоре](06-historical-evidence.md). Это проверка оригинальности относительно прочитанного корпуса, не обещание отсутствия идеи во всей истории проекта.

**Ответ на требование о полноте:** утверждение «каждая найденная деталь уже правильно оформлена в protected» не подтверждается. Два новых фактора и новые evidence-уточнения подготовлены в разрешённом possibleDifs; protected в этой сессии не редактировался. Наличие старой P-семьи не заменяет буквальный малый recipe. Проблемные места обозначены адресно, вместо формального полного PASS.

## Применённые проектные навыки

Из `llm_mvp_core/docs/skills` прочитаны и применены: `trainable-architecture-decision-SKILL.md`, `intent-to-plan-SKILL.md`, `plan-self-validation-SKILL.md`, `generated-code-review-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `experiment-design-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `human-communication-GLOSSARY.md`. Их задача здесь — source→writer→state→consumer, один фактор, явные домены/метрики, точная передача исполнителю и понятное человеческое объяснение.

Исторические production-процедуры из skills не назначают обучение, полный CI, repair tests или promotion вопреки прямой нынешней задаче «только документируем». Модель исполнителя остаётся default. Проектная роль CODEX и внешнего автора DPO отделена от native student; вызовы этих моделей ради данного аудита экспериментов не имитировались. Приведённые исторические raw сохраняют собственное фактическое authorship.

## Контроль качества этого пакета

Проведены три независимых направления чтения: текущие neural mechanisms, историческая memory/KNN линия и валидность protected карточек. Новые карточки дополнительно проверены другим агентом по смыслу, составу source-copy, бюджету и scope. Выявленные ошибки рецептов и границ таймеров исправлены до публикации; отчёты reviewers лежат в [reviews](reviews/). Это статическая проверка документации и будущего scaffolding, не native build PASS.

Перед публикацией проверяются JSON, относительные ссылки, уникальность ID/roots, отсутствие повторных completed запусков и служебных подписанных download URLs; изменяемые пути ограничены новым пакетом и добавочной шапкой possibleDifs README. Старый README сохраняется целиком после шапки, старые опыты не удаляются. Публикация выполняется поверх свежего planning head без force; фактический результат подтверждается remote readback.
