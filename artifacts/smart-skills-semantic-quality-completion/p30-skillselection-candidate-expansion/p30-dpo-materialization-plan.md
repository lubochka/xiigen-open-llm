# P30 DPO materialization plan - human-readable no-code

Вердикт: это план превращения P30 candidate materials в будущие DPO pairs. Это не реализация, не обучение и не проверка checkpoint.

## 1. Как candidate становится DPO парой

1. Берем одну строку из `p30-candidate-expansion-ledger.jsonl`.
2. Проверяем, что source family не используется одновременно как train и eval.
3. Переписываем source signal в универсальную формулировку: без путей, имен файлов, языка программирования, host-specific hooks, exact class names и private text.
4. Создаем chosen behavior: какой skill должен быть выбран и почему именно по смыслу задачи.
5. Создаем rejected behavior: правдоподобная ошибка, например выбрать соседний общий skill или выбрать skill по file/path/title shortcut.
6. Создаем quarantine row: пример, который выглядит похожим, но содержит stack-specific или private leakage и поэтому не должен идти в common training.
7. Добавляем teaching point: какую границу должна выучить модель.

## 2. Как избежать stack-specific details

Для каждой строки reviewer обязан ответить по-человечески:

- Какая универсальная задача выбора скила здесь учится?
- Что удалено как стековая или проектная механика?
- Какой rejected пример проверяет, что модель не выучила shortcut?
- Что должно сломаться в ablation, если оставить только filename/path/class/id?

Если после удаления stack-specific details смысл строки исчезает, строка остается evidence-only и не материализуется в DPO.

## 3. Как тестировать leakage

Будущие тесты должны быть названы явно:

- `SkillSelectionDpoMaterializationTests.P30Rows_DoNotUsePathOrFilenameAsChosenSignal`
- `SkillSelectionDpoMaterializationTests.P30Rows_HaveChosenRejectedAndQuarantineExample`
- `SkillSelectionSourceDisjointTests.P30Rows_DoNotMixTrainAndEvalFamilies`
- `SkillSelectionLeakageAblationTests.P30Rows_FailWhenOnlySkillIdOrFileNameRemains`
- `SkillSelectionDashboardQaTests.P30Rows_AppearAsCandidateOnlyNotTrainingReady`

## 4. Какие sets потом перестраивать

Affected future sets:

- `artifacts/smart-skills-semantic-quality-completion/p31-skillselection-dpo-materialized-candidates`
- future local-private SkillSelection repair set derived from P30 rows
- future dashboard QA scenario set for candidate-only visibility

Rebuild rule: если меняется source family, chosen skill, rejected skill, quarantine reason или universal-vs-stack summary, строка получает новый row hash и старый candidate отправляется в quarantine ledger.

## 5. Жизненный цикл будущего обучения

P30 не обучает. Будущая training phase должна:

1. загрузить user/cache checkpoint first;
2. если user/cache отсутствует, загрузить repo/common checkpoint;
3. применить только разрешенный materialized training batch;
4. сохранить candidate checkpoint отдельно от common;
5. сделать backup/export/import/share artifact;
6. проверить fresh-load;
7. проверить continue-training from existing checkpoint, а не retrain-from-zero;
8. только после held-out + ablation + dashboard evidence обсуждать semantic quality.
