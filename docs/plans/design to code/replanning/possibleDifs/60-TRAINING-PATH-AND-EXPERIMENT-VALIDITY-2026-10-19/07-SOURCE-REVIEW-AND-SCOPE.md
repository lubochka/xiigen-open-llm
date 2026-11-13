# Источники, независимая проверка и пределы полноты

## 1. Снимки источников

| Материал | Проверенная версия | Объём и способ проверки |
|---|---|---|
| Core source и исходный protected | `8e020052fae7efa694b8d33f90768648d6b67af9` | Полный protected tree:1096 blobs;904 текстовых Git files прочитаны и совпали по blob SHA;192 binaries — metadata only |
| Первая внешняя правка protected | `c3f1d0d4fc7fb7949c51d2251e89d2fb0e590d72` | 145 версий файлов прочитаны отдельно,145 exact blob matches; код продукта не менялся |
| Вторая внешняя правка protected | `f50b710a5eaa6a8f0468b07d5b2510342a1de7ba` | Metadata149 изменений,147 modified+2new; адаптивно перечитаны10 ключевых документов,10 exact matches; код продукта не менялся |
| Начальный possibleDifs | `993ac5cf5803d9903a58b71eca7c85b94d3116f4` | 525 Markdown выгружены; полный поиск новизны, чтение близких механизмов и исходных ссылок |
| Первая planning delta | `5333d9efa76982330e8be0b59facf304105e300b` | 11 Markdown:9modified+2new; union527 |
| Вторая planning delta | `47e6f1be824e53e1d78094ba9e18126c37117c22` | Четыре новых59-* документа; union531 |
| Третья planning delta | `878900e7a854db477c40f430cdcf14e0dc428e6a` | Два новых59-* документа и обновлённый README; новые документы прочитаны полностью, union533 |
| Дополнение при проверке публикации | `a1b480aeb67fb6ee7657fa3bde7b43ad30d8da84` | Три новых60-* документа, проверены предметы/близкие механизмы; union536 |

Source ссылки в01/02/04 закреплены на прочитанных commit SHA. Сравнения core [8e020→c3f1](https://github.com/lubochka/llm_mvp_core/compare/8e020052fae7efa694b8d33f90768648d6b67af9...c3f1d0d4fc7fb7949c51d2251e89d2fb0e590d72) и [c3f1→f50](https://github.com/lubochka/llm_mvp_core/compare/c3f1d0d4fc7fb7949c51d2251e89d2fb0e590d72...f50b710a5eaa6a8f0468b07d5b2510342a1de7ba) показывают только документационные изменения внутри protected. **Это внешние изменения во время нашей работы; мы не писали в core.** Не следует утверждать, что защищённая папка глобально оставалась неподвижной.

Последняя адаптивная проверка не равна полному повторному чтению149 файлов. Union f50 арифметически содержит908 текстов,282 Markdown и22 small-probe документа;125 P-карточек сохраняются. Успешная проверка blob bytes удостоверяет точность полученного текста, а не истинность всех утверждений внутри него.

## 2. От RAG-индексов к первичным документам

Прочитаны три индекса из `llm mvp/design to code session`: `09.09.26WorkLeft-RAG-01-dpo-legal-build.ndjson` (201 строка), `09.09.26WorkLeft-RAG-02-design-to-code.ndjson` (140), `09.09.26WorkLeft-RAG-03-merge-and-branches.ndjson` (33). Это374 индексные записи, **не374 независимо прочитанных первичных документа**.

Из них и RP01 pack адресно проверены:

- [RP01 SOURCE-AND-ARCHITECTURE](https://github.com/lubochka/xiigen-planning/blob/993ac5cf5803d9903a58b71eca7c85b94d3116f4/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/SOURCE-AND-ARCHITECTURE.md), [CURRENT-CODE-MAP](https://github.com/lubochka/xiigen-planning/blob/993ac5cf5803d9903a58b71eca7c85b94d3116f4/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-CURRENT-CODE-MAP.md), [recovery/code audit](https://github.com/lubochka/xiigen-planning/blob/993ac5cf5803d9903a58b71eca7c85b94d3116f4/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md).
- Родительские [DPO data/training/measurement guide](https://github.com/lubochka/xiigen-planning/blob/993ac5cf5803d9903a58b71eca7c85b94d3116f4/llm%20mvp/dpo%20legal%20build/active-codex-execution-05-dpo-data-training-measurement-guide.md) и [training lifecycle](https://github.com/lubochka/xiigen-planning/blob/993ac5cf5803d9903a58b71eca7c85b94d3116f4/llm%20mvp/dpo%20legal%20build/active-codex-execution-plan-10-training-lifecycle.md), а также [KNN authority amendment](https://github.com/lubochka/xiigen-planning/blob/993ac5cf5803d9903a58b71eca7c85b94d3116f4/llm%20mvp/llm%20dynamic%20flow/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md).
- Далее конкретные native callers и persisted receipts, перечисленные в01/02/04. Индекс или старое намерение не использованы как доказательство того, что нужная функция действительно вызывается в нынешнем обучении.

Успех matrix-компонента, последующего KNN и конкретного RP01 сверялся раздельно. Ни исторический статус FAILED, ни одинаковое имя класса не являются достаточной причиной отвергнуть функцию. Архивные ветки и source SHA: DesignToCode `8f20ceb8a9bfbadab2ad309226c47dd76e714548`; DesignToCode-clean `e346b88a3cc1fcce48aefa626bfc7b00509eefd6`; PrepareToGPL `31346d8928be4469b01aaf313780e8f950ff91e2`; language_translate `f6032aff81f1f83de7c09cf9b9efe6b058dd12b9`; external_domains `3036a01acec938f0b83fa4ffbda26202cc35d639`; local_wiki_planning `219d7ba89dc3ef43e03e17f1461ad96154d3dd7a`; orchestrator-extensions `b293045bd89b1eb96d3e32fedd341f1a4170adbc`. Степень адресного чтения различалась; полного поиска каждого commit каждой ветки не было.

## 3. Как проверялась новизна

Поиск выполнялся по всем525 начальным Markdown, всем125 P-карточкам и дополнительным protected материалам; близкие совпадения читались по смыслу. Затем проверены11 файлов первой delta,4 новых59-* файла и2 заключительных59-* документа на878900e7. Эти поздние59-* описывают memory continuation/feedback, scheduler DidRun, conditional AST channel и orchestration payload/task-type кандидаты. Они не описывают наши конкретные DPO→full-file SFT order, exact side-prefix cost и E14 ForEachVariable/Do training-label cases. Два заключительных документа (`59-CAUSAL-LEARNING-PASSPORTS-AND-EXECUTOR-PROMPTS...` и `59-PROTECTED-EXPERIMENT-VALIDITY-AND-NONLEARNING-SCORER-GATES...`) также проверены: новый scope касается orchestration OR/AND/learned_pattern и panel-wide scorer; три наших механизма и два точных SFT-подслучая ими не покрыты. Общий методический протокол с ними согласован. Перед обновлением ветки дополнительно проверены три новых60-* документа наa1b480ae: атлас истории, memory homograph/corrected-tokenizer/feedback взаимодействия и приоритетная карта. Конкретных трёх ND/E14 механизмов и двух SFT-подслучаев они не содержат. Эти чужие файлы сохраняются при публикации; общий числовой префикс60 не означает авторство одного пакета.

Для ND-DPO-SFT ближайшие темы — P02 warm base и SFT objective. Для ND-DPO-PAD — P09 serving cache, P68 shards/padding и P110 numeric kernel. Для N-KU-01 — MethodKnn.CanonicalType, E14 List/Task feature alias и null-guard; это другие producer/target/factor. Два SFT уточнения честно остаются KNOWN/MISSING_PRECISE_SUBCASE. Отсутствие строки с новым названием само по себе не доказывает новизну; проверена разница механизмов и задуманных A/B.

Число536 означает union исходных planning Markdown до нашего пакета на последнем указанном срезе, а не536 различных гипотез. Отсутствие фактора в прочитанной карте не исключает неизвестный старый commit или неиндексированный документ.

## 4. Независимая проверка выявила и исправила существенные детали

| Замечание рецензента | Что исправлено в конечном проекте опыта |
|---|---|
| Production DPO wrapper после SFT переснимет reference либо потребует FullStatesMatch | E-DPO-SFT-ORDER явно требует отдельный пока отсутствующий native research host и общий R0 до обеих стадий; простой reorder production owner не предлагается |
| Default mixed caller legacy, а proposed recipient exact-v2 | Exact-v2 закреплён как общий исследовательский профиль обоих плеч; буквальное воспроизведение default CODE_Q не заявлено |
| Translation bridge не доказан как exact-v2 consumer | ENRU PAD ограничен отдельно установленным exact research caller; общий переводческий bridge не подменяется |
| После StepVector градиенты уже очищены | PAD instrumentation снимает их до ApplyOptimizer/ZeroGrad внутри тех же updates |
| PAD cost можно спутать с JIT, cumulative peak и временем копии | Заданы равные границы реального update, fresh processes, включение view-copy и предел точности малого опыта |
| E14 Train заменяет весь corpus, а не добавляет только новые строки | В карточке полный accumulated4→6→8,18 предъявлений на плечо/36 всего; label preparation учтена отдельно |
| SFT маска должна совпасть с настоящим shifted target | Уточнены T=tokenIds.Count, input первыеT−1 IDs, target i+1, joint byte spans, одинаковый weight и fixed positive LR |
| При удалении prompt gradients clipping может измениться | Фиксируется только response coefficient до clipping; norm/clip/AdamW deltas измеряются как часть реального эффекта маски |
| Старые претензии к protected уже исправлены другой работой | Текущий03 начинается со снятых претензий наf50; c3f1 разбор помечен историческим |

Содержательная перекрёстная проверка: отдельный исследователь KNN/истории проверил DPO-карточки; исследователь DPO проверил E14, общий контракт и root SFT-mask карточку. Coverage исследован отдельным worker. Root прочитал результаты и интегрировал исправления. [Ревизия DPO до исправлений](reviews/knn-review-dpo.md), [ревизия E14/контракта/SFT-mask](reviews/dpo-review-e14-and-protocol.md) содержат source refs и hashes рассматривавшихся **промежуточных** редакций. Их нельзя выдавать за hashes окончательных файлов после интеграции. Число источников и строк не названо числом независимых reviews.

## 5. Применённые навыки проекта

Использованы навыки из `llm_mvp_core/docs/skills` на закреплённом core source SHA: архитектурный `trainable-architecture-decision-SKILL.md`; планирование `phase-planning-SKILL.md`; проверка плана `plan-review-SKILL.md`; проверка кода `generated-code-review-SKILL.md`; учебный контур `learning-path-audit-SKILL.md`; дизайн опыта `experiment-design-SKILL.md`; промпты `prompt-patch-authoring-SKILL.md`; понятный архитектурный отчёт `human-readable-architecture-audit-SKILL.md` и `human-communication-GLOSSARY.md`. [Каталог навыков на source snapshot](https://github.com/lubochka/llm_mvp_core/tree/8e020052fae7efa694b8d33f90768648d6b67af9/docs/skills).

Практическое применение: настоящее состояние и его потребитель; один фактор; текущая воля пользователя выше старых порогов и чужих ограничений; согласованные карточка/промпт; собственный domain scorer; разделение предмета, направления, механизма и Unit. В частности, архитектурный review не позволил принять простую перестановку owners за чистый order experiment. Skill-файлы в этой работе не редактировались. Вызов конкретного CODEX/Astra или внешнего китайского teacher при анализе документов не приписывался: модельные роли описаны для будущего исполнения, не как уже выполненные вызовы.

## 6. Машиночитаемые доказательства и их пределы

В `inventory/` сохранены компактные формы исходных inventories:

- `source-card-crosswalk.json`:525 исходных planning документов, upstream Git blob IDs, точные пути/строки найденных ссылок и явный `semantic_coverage=UNPROVEN_BY_REFERENCE_SCAN`. Числа278/108/139 относятся к поисковой адресуемости наc3f1. Они не являются числом покрытых/непокрытых гипотез, а послеf50 не являются актуальным filename-gap count.
- `protected-corpus-integrity.csv`: исходные1096 blobs с размером, SHA, признаком прочитанного текста;145 exact версийc3f1 и10 выбранных exact версийf50 с SHA256. Это проверка содержимого источников, не эксперимент обучения.
- `card-structure-and-late-review.json`:125 P-карточек, структурные признаки и строки промптов наc3f1; полные149 metadata-изменений и содержательный поздний review10 файлов. Наличие `Train` в тексте не присваивает `LEARNING_VALID`.
- `unread-lfs-payloads.json`: четыре comparison JSON оказались Git LFS pointers; сохранены OIDs и ожидаемые размеры, фактические payload не материализованы. Остальные447 из451 JSON/JSONL синтаксически прочитаны; это не проверка правильности всех численных выводов в них.

Семантически исследованы конкретные семейства механизмов и подслучаи, приведённые в документах; утверждения «все125 карточек полностью валидны», «все536 документа исчерпаны по смыслу» и «найдены все возможные причины деградации» не делаются. Для новых трёх гипотез здесь есть собственные конкретные эксперименты, но их полезный эффект ещё не измерен. Для уже исправленных исторических ошибок сохранены доказательства успеха и отрицательные оси.

## 7. Граница публикации

Разрешённый результат этой задачи — новые документы данного пакета в `xiigen-planning`, ветка `llm_plan3`, каталог `possibleDifs`. Исторические отчёты и protected core не перезаписываются. Публикация выполняется отдельным commit поверх проверенного актуального head с проверкой ожидаемого родителя; соседние новые файлы сохраняются. Состав фактического commit и его SHA виден в истории GitHub и в итоговом ответе. Commit Markdown не подтверждает запуск эксперимента, принятие модели или изменение её качества.
