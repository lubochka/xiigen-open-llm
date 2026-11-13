# Восстановление external_domains: пять новых гипотез и локализация открытого механизма

**19 октября 2026. Исследование по первичным исходникам и сохранённым результатам.** Главная задача — вернуть конкретные полезные функции успешной ветки `external_domains`, а затем проверить новые риски нынешних consumers. Новые native обучение и генерация в этой работе не выполнялись; три Python probes подтверждают только отдельные операции из source.

**Финальная сверка перед публикацией:** [689 уникальных Markdown; пять новых конкретных гипотез и одна локализация ранее открытого duplicate lead](08-FINAL-CORE-DOCS-NOVELTY-CLARIFICATION.md). Этапы основного review и двух параллельных planning-публикаций сохранены в [главе 07](07-PUBLICATION-RECONCILIATION.md). Соседние файлы и записи индекса сохранены.

## Читать первым: восстановление успешной ветки

**[00 — Приоритетная глава восстановления external_domains](00-EXTERNAL-DOMAINS-RECOVERY-FIRST.md)**

Она содержит шесть групп малых опытов P0–P5: сохранённое состояние→обычный consumer→cold load; structural C# KNN; learned POS order и применение перестановки; contextual dictionary; neural target-set contrast; полный resume и ранняя польза переноса. Восстановление начинается с нужного реального artifact и одного предметного закона. Если artifact недоступен, реконструкция обучения явно отделена от загрузки сохранённой модели.

Повторная первичная сверка сохранила полезные результаты, не смешивая их смысл: ранний E09 имеет небольшой сопоставимый положительный эффект; E25 обучает POS-память; E26 использует заранее наполненный контекстный словарь; E27 правильно преобразует заданную структуру. E13B показывает точную память и отказ на неизвестном. У E48 ранний положительный эффект остаётся важным при равенстве финальных точек. Положительный ProjectWiki DPO найден в соседней DesignToCode, но его preference accuracy не равна точности свободных ответов. Все числа и ограничения — с direct pinned evidence в первой главе.

## Содержание

| Документ | Для чего нужен |
|---|---|
| [01 — Общая карта различий](01-MECHANISM-CROSSWALK.md) | DPO, LoRA, tokenizer/vector, KNN, deterministic selection, DPO→под-юниты, restore; известные owners и быстрые различающие проверки |
| [01B — Пять перепроверенных Unit seams](01B-KNOWN-UNIT-TRANSFORMATION-GAPS.md) | 64-token embedding, generic label projection, translation keys/RuleTags, phrase index и morphology; **KNOWN**, не новые гипотезы |
| [02A — KU-01: агрегация сегментов](02A-NEW-SEGMENT-AGGREGATION-GAP.md) | Уникальные source IDs превращают согласованный learned target в конфликт |
| [02B — KU-02: eligibility после cutoff](02B-NEW-CANDIDATE-ELIGIBILITY-GAP.md) | Запретный для ребра кандидат поднимает global floor и выключает learned inference |
| [03 — PT-01/PT-02: память PromptToTask](03-NEW-PROMPT-PATTERN-GAPS.md) | Правильный повтор обнуляет margin; substring gate создаёт ложный missing_gates |
| [04 — AUT-WEIGHT-01/AUT-REPLAY-02](04-AUTONOMOUS-LEARNING-CONDITIONAL-GAPS.md) | Схлопывание provider weights и допуск replay до quality verdict; **P2 conditional**, другой путь, не direct RP01 |
| [05 — Малые эксперименты и задание CODEX](05-SMALL-EXPERIMENTS-AND-WORK-ORDER.md) | Минимальные объёмы, факторы A/B, доменные outcomes, контроли и готовый следующий work order |
| [06 — Источники, новизна и review](06-SOURCES-NOVELTY-AND-REVIEW.md) | Pinned refs, этапы поиска и review, статус skills; окончательное уточнение 689 — в главе 08 |
| [08 — Финальное уточнение новизны](08-FINAL-CORE-DOCS-NOVELTY-CLARIFICATION.md) | Поздние protected core документы: known generic lead для PT-01 и отдельный owner/trigger KU-02 |
| [Evidence](evidence/README.md) | Скрипты/receipts, source manifests, поисковый ledger и независимые замечания |

## Что нового и насколько это связано с RP01

Описаны шесть конкретных цепочек: **пять новых гипотез и PT-01 — локализация ранее открытого общего предположения**. Каждая имеет точный owner, consumer/precondition, ancestry, контрсвидетельства и малый falsifiable experiment. Это source findings с ограничениями применимости; причины одного неудачного запуска RP01 ими пока не установлены.

KU-01/KU-02 и PT-01/PT-02 способны лишить систему пользы уже имеющегося решения/памяти на своих путях. AUT-WEIGHT/AUT-REPLAY способны изменить будущий supervised учебный поток, но нынешний автономный pipeline не имеет non-test composition root и не обслуживает direct RP01. Два AUT механизма в существенной части унаследованы от donor. Исправление их политики нельзя выдавать за возвращение найденного старого успешного исправления.

**Практический первый шаг:** выбрать домен и P0, удостовериться в реальном loaded state и обычном ответе. Затем проверить одно применимое отличие из P1–P5. Для neural RP01 сохранять современную исправленную DPO/backward/mask цепь; отдельно сравнить набор обучаемых параметров и пригодность исходной базы. Для структурного KNN и порядка предложения нужны свои представления и метрики.

Core snapshot: [`44507823…`](https://github.com/lubochka/llm_mvp_core/tree/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76). Donor: [`external_domains@3036a01…`](https://github.com/lubochka/llm_mvp_core/tree/3036a01acec938f0b83fa4ffbda26202cc35d639). [Исходный RP01 pack](../../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/README.md). [Вернуться в общий possibleDifs](../README.md).
