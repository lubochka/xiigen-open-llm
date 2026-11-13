# P6 evidence card

## Доказательство

P6 использовал только 2 P5L строки из обучающего разбиения, чтобы создать baseline/checkpoint candidate для PromptToTask. 4 eval/holdout строки записаны в reserved artifact и не использовались для обучения.

SHA-256 checkpoint: `5483f58c3e3dcaf4938b651cad8aa7faf9ad027142d000cb4f482a96e454cd36`

Кандидат сохранен и загружен через существующий проектный код PromptToTask. Semantic quality остается false; source-disjoint evaluation и ablation остаются будущими gates.
