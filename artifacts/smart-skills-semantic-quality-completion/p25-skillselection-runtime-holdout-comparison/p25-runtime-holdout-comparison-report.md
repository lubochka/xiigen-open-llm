# P25 - side-by-side SkillSelection runtime holdout comparison

Вердикт: COMMON_AND_CANDIDATE_TIE_ON_P24_ROWS_PROMOTION_STILL_BLOCKED.

## Что проверялось

P25 взял только четыре accepted строки из P24 runtime holdout ledger и прогнал каждую строку через один и тот же runtime метод `SkillSelectionMachine.DecideFromTrainedState`.

Сравнивались два checkpoint-а:

- common: `artifacts/smart-skills-semantic-quality-completion/p25-skillselection-runtime-holdout-comparison/isolated-model-inputs/common-checkpoint.json`
- candidate P17E production export: `artifacts/smart-skills-semantic-quality-completion/p25-skillselection-runtime-holdout-comparison/isolated-model-inputs/candidate-checkpoint.json`

Runtime input для выбора skill состоял из redacted task summary, списка доступных skill ids и нейтральных host facts P25. Exact prompt, source id, path, filename, pair id и candidate order не использовались как authority.

## Итоги по строкам

- `P24-SS-HOLDOUT-E2E-MATRIX-001`: expected [e2e-test-matrix]; common [implementation-integrity, test-integrity, plan-review] pass=false; candidate [] pass=false.
- `P24-SS-HOLDOUT-NO-PRODUCT-DECISION-001`: expected [no-product-decisions]; common [claim-verification] pass=false; candidate [] pass=false.
- `P24-SS-HOLDOUT-OUTPUT-SKEPTICISM-001`: expected [output-skepticism]; common [verification-before-completion, plan-review] pass=false; candidate [] pass=false.
- `P24-SS-HOLDOUT-SELF-VERIFICATION-001`: expected [self-verification]; common [test-integrity] pass=false; candidate [] pass=false.

## Агрегат

- Всего строк: 4
- Common совпал с expected skill: 0
- Candidate совпал с expected skill: 0
- Threshold P25 пройден: false

## Что это доказывает

Это доказывает только наблюдаемый side-by-side результат на accepted P24 runtime holdout rows. Модели загружались в отдельные `SkillSelectionMachine` instances, а файлы common и candidate checkpoint/manifest остались byte-identical после сравнения.

## Что это НЕ доказывает

P25 не доказывает semantic quality, SkillSelection readiness, full API readiness, runtime promotion или право заменить common model. P25 не запускал training, не мутировал checkpoint, не делал promotion и не обращался к внешнему AI или сети.
