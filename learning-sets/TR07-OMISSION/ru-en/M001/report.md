# Отчёт подготовки партии TR07-OMISSION/ru-en, запуск M001

Статус партии: **ПРИНЯТА** после независимой слепой проверки (DATA_REVIEWED_NOT_NATIVE_TRAIN_READY). Черновик сгенерирован сессией генерации; слепая проверка выполнена отдельной независимой сессией; приёмка и экспорт выполнены; обучения не было.

## Scope и вход

- Scope: DATA_PREPARATION_ONLY (по request.json). Обучение и production-изменения не выполнялись. Стадии blind/review/accept/export после генерации выполнены — фактические статусы в разделе «Чего не было (честные статусы)».
- Поток: TR07-OMISSION/ru-en; run: M001; split: train; размер: 10 пар, 10 независимых семей (по одной паре на семью, pairs_per_family=1).
- Вход: request.json, prompt.md этого запуска; карточка `curriculum/cards/TR07-OMISSION.md`; назначенные источники RAG-024 и RAG-023.
- RAG восстановлен из коммита `97d61ed38a20fe0a6128ca5648b97fb9520dc17a` репозитория `C:/Users/lubao/Documents/xiigen` в `learning-sets/_sources/xiigen-rag-97d61ed38a20`: 58 источников + manifest, каждый сверен по SHA-256 с `sources.lock.json` до записи, 0 пропусков. Первоначальный пин `48566132e35d` заменён решением родителя: он устарел на один коммит и покрывал 57 из 58 источников (нет RAG-DATA-001).

## Фактически наблюдаемая модель и среда

- Генератор: текущая выбранная модель этой Kiro-сессии; наблюдаемый селектор — «Kimi K3» (записан как `Kimi K3 (Kiro)`).
- Внутренняя identity/версия модели средой не раскрыта: `model_identity_observed=false`, `generation_session_id=not_disclosed_in_this_environment`. Конкретный провайдер не утверждается.

## Содержательный выбор материала

- Учебная граница одна: ровно одна пропущенная буква (single-character deletion) внутри известного русского слова; контекст оставляет одно разумное восстановление; canonical-слово в видимом prompt не раскрывается.
- Единственный различающий признак chosen/rejected: восстанавливаемый предмет. Rejected подменяет его другой правдоподобной лексемой (молоко→сок, собака→кошка, окно→дверь, карандаш→ручка, учитель→ученик, врач→медсестра, телефон→кошелёк, сметана→творог, поезд→автобус, библиотека→музей); действия, роли, время, число и регистр совпадают.
- Механика зафиксирована в metadata: canonical_token, raw_token, mutation.index в Unicode code points; проверено механически: canonical_token без указанного code point равен raw_token для всех 10 строк. Повреждённые формы не являются самостоятельными словами (коллизий valid-word нет).
- Покрытие осей: position first/middle/last = 2/6/2; context one_short_sentence/two_short_sentences = 8/2; word_length 4_to_6/7_to_10 = 5/5.
- Сюжеты 10 разных: кухня, прогулка с животным, дождь и комната, школьные принадлежности, урок, поликлиника, городской транспорт, молочный отдел, ж/д расписание, часы учреждений. Публичный reference-сюжет (билет на столе) не переиспользован.

## Фактические результаты

- draft.jsonl: 10 строк, schema `xiigen.dpo.v1`; pair_id уникальны; split=train у всех; source_refs=[RAG-024, RAG-023] у всех; меток вида [WRONG] нет; rationale/error_tag только в метаданных; lex:* ID только в metadata.lexical_family_ids и не дублируются в leakage_group_ids.
- Структурная проверка `tools/batches.py check`: **STRUCTURE_ONLY**, rows=10, errors=[] (приложен вывод). Была одна ограниченная итерация исправления черновика (индексы удаления и регистр букв в трёх строках), после неё проверка повторена.
- state.json инструмента не редактировался вручную и по-прежнему содержит PREPARED / generation NOT_RUN — фактическая генерация зафиксирована этим отчётом и наличием draft.jsonl.
- Приёмка `tools/batches.py accept` (команда из commands.md этого запуска): `{"accepted": "...\\accepted.jsonl", "rows": 10, "status": "DATA_REVIEWED_NOT_NATIVE_TRAIN_READY"}`, код выхода 0. Инструмент сверил победителей из review.json с закрытым ключом blind.key.json — расхождений нет.
- Экспорт `tools/batches.py export`: `{"out": "...\\export", "rows": 10, "status": "INTERCHANGE_FORMAT_ONLY"}`, код выхода 0. Проверка результата: dpo.train.jsonl — ровно 10 строк, в каждой только ключи prompt/chosen/rejected; prompt/chosen/rejected в экспорте байт-в-байт совпадают с accepted.jsonl (0 расхождений).

## Чего не было (честные статусы)

- blind: DONE (`blind.jsonl` + закрытый `blind.key.json`). review (независимая слепая bilingual-проверка отдельной сессией): DONE — 10/10 валидных, tie/invalid = 0, ключ проверяющему не показывался (`review.json`: `saw_label_key=false`, `independence=separate_session`). accept: DONE — `accepted.jsonl` + `accepted.acceptance.json`, статус DATA_REVIEWED_NOT_NATIVE_TRAIN_READY; инструмент сверил победителей review с закрытым ключом. export: DONE — `export/dpo.train.jsonl` (10 строк, только prompt/chosen/rejected), `provenance.jsonl`, `export.json` со статусом INTERCHANGE_FORMAT_ONLY, `native_import_verified=false`; это обменный формат, не нативный вход обучения. training: NOT_RUN.
- Экспорт создал `dpo.dev.jsonl` и `dpo.exam.jsonl` пустыми (0 байт) — партия только train; наполненных dev/exam не существует.
- Семантическое качество данных: установлено независимым bilingual review (10/10); это не обучение и не проверка модели.
- Метрики обучения: NOT_MEASURED — обучение НЕ запускалось, качество модели НЕ измерялось.
