# CODEX: один короткий восстановительный запуск

## Цель

Сначала удостовериться, что сохранённое успешное состояние `DesignToCode-clean` действительно загружается правильным получателем и используется обычным запросом. Выдать доказательство конкретного восстановления либо точную точку разрыва. Не начинать новый большой DPO-цикл по одному старому статусу PASS.

Это **спецификация будущего выполнения**, а не отчёт о запуске. Новые изменения product code и обучение в рамках подготовки пакета не выполнялись.

## Закреплённые источники и предел изменения

Эталон: core `e346b88a3cc1fcce48aefa626bfc7b00509eefd6`; проверенная реализация: `358af351e22a92a3f7938d1f9221d5aeb7ddb053`. После появления новых commits сперва сравнить только затрагиваемые методы; не считать старый анализ автоматически актуальным для другого HEAD. Доказательства: [00](00-DESIGNTOCODE-CLEAN-RESTORATION-FIRST.md), [01 / DC-01b-R](01-NEW-DESIGN-CORRECTION-SEMANTICS.md), [04](04-SMALL-DOMAIN-EXPERIMENTS.md).

`docs/plans/replanning-possible-diffs` в core не изменять. Исторические checkpoints/results не перезаписывать. Исследовательские результаты сохранять отдельным пакетом в разрешённом planning possibleDifs. Любой будущий product patch должен иметь отдельный явный scope; этот документ не выдаёт ревью/допуск несуществующему patch.

## Первый запуск: R0-SNAPSHOT, без новых DPO

1. Найти настоящий `policy_final.bin` / `reference_base.bin` CodeTrace019 по точным LFS OID из главы00. Проверить payload SHA-256 и размер 1,188,744 bytes каждого. Сам 132-byte pointer не передавать loader-у как модель.
2. Установить model config, input representation и reference identity именно этого scorer. Clean wiki runner использует hash-word ID pipeline; не присоединять его веса к произвольному BPE Student. Сохранить declared и фактически используемые identities.
3. Получить **P** — чтение final019 без update. Пересчитать те же 50 heldout pairs: relative margin, absolute policy ordering и per-case result. Ожидаемый архивный ориентир 49/50 относится только к relative preference metric; несовпадение сначала объяснить exact input/model/runner-version разницей.
4. Получить **C0** — новый процесс, та же загрузка, тот же набор, ноль update. Сравнить hashes, selected logits и решения; если они расходятся, learning contrast недостоверен.
5. Проследить обычный consumer этого состояния. Сохранить 8–20 новых source-family-disjoint C# запросов и реальные ответы. Если API scorer не выдаёт свободный ответ, назвать его ranking-получателем и записать, как downstream caller применяет ranking; не выдавать score за генерацию Student.
6. Вернуть один краткий evidence report: где state найден, какой payload прочитан, какой recipient вызван, что совпало, что не измерено. Если payload или совместимый loader недоступен, закончить этот запуск с конкретной причиной `RESTORE_NOT_PROVEN`; не создавать случайную B0 под именем «восстановили019».

Здесь нет teacher-authoring и новых optimizer шагов. Повторное оценивание архивных pairs не переименовывается в обучение. Восстановленный scorer и отдельный neural Student остаются разными получателями.

## Параллельно пригодный малый repair probe: DC-01b-R

Этот опыт становится первым для **DesignConversion**, если в его реальном clean export найден `correction_category:generic_visual_defect`. Сначала проверить наличие ключа, не предполагать его по одному source-контрпримеру.

Пять otherwise-valid checkpoint cases: known spacing, только underscore generic, только hyphen generic, обе формы вместе, unrelated forbidden key. Сравнить clean/current без новых уроков. Снять exact `Import/Load` verdict и состояние до/после отказа. Source предсказывает, что current отвергает весь checkpoint с запрещённым ключом через `CHECKPOINT_LEARNED_WEIGHTS_INVALID`; native факт ещё надо получить.

**Единственное вмешательство T:** узкая нормализация разрешённой canonical identity при импорте/сохранении, с явным правилом для alias collision. Сохраняются текущий whitelist, source/policy/payload/shape проверки и остальные weights. Нельзя молча суммировать два observations одного урока, выкинуть весь state или разрешить произвольные feature keys. Known control должен остаться совместимым; unrelated forbidden key — остаться отвергнутым. После успешной миграции проверить ordinary improver/Flow и cold load.

Этот опыт доказывает совместимость state. Для утверждения о качестве дополнительно нужна новая layout-панель с реальными renders; старый underscore weight мог не читаться generic runtime и в clean. Полезный эффект возможен через сохранность остальных weights, но это отдельное наблюдение.

## Только после восстановления — один учебный contrast

Выбрать ровно один R1/R2/R3 или KU-01 по реальному предмету и найденному consumer. Для этого пакета наиболее дешёвые кандидаты: RU→EN concept/gate на 12 новых инструкциях или C# KNN ± AST на 8–12 файлах. Не объединять representation, target mappings, k, objective и base weights в одну правку.

Настоящие DPO: внешняя открытая китайская модель создаёт обе финальные стороны; **CODEX на Astra 6 ultracode** выбирает случаи в human-role и независимо оценивает exact final pair и обычные ответы. Сохранить фактические model/authoring/judge receipts. Автор диагностического скрипта не становится teacher посредством подписи. Если нужная модель/конфигурация недоступна, этот этап не запускался; это не отрицательный learning result.

Для KU-01 заморозить chosen, WordMappings и PhraseMappings bytes; иначе смена token.Concept меняет также ранний word-map output. Все addresses соответствующего плеча строятся одним encoder, train/eval source families разделены. Не отключать early paths ради получения искусственного KNN gain. Полная спецификация — [02](02-NEW-TRANSLATION-REPRESENTATION-GAPS.md).

## Обязательный ответ исполнителя и независимое review

Вернуть факты в таком порядке: предметная задача; фактический recipient и ordinary caller; единственный changed factor; P/C0/T raw outputs; paired wins/losses/ties и ошибки смысловой рубрики; state identity и cold-load наблюдения; время/память/число реальных model calls; verdict `IMPROVE / REGRESS / INCONCLUSIVE / INVALID_EVIDENCE` и точная граница вывода.

Независимый reviewer проверяет, что исправилась заявленная причина: сохранено ли checkpoint содержимое, не поменялись ли targets, действительно ли вызван Unit, не пришёл ли ответ из раннего exact map, не заменён ли смысл local score. Отдельная review-оценка качества требует фактического вызова оценщика; наличие этого work order её не доказывает.

Принятие полезного изменения требует ordinary gain на новых случаях и допустимой сохранности старых. Ни число изменённых LoRA тензоров, ни успешная сериализация, ни зелёный helper test не заменяют эти наблюдения. Для неясного результата сохраняется parent и формулируется следующий один различающий вопрос.
