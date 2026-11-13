---
title: "160. Checkpoint marginality: ранняя роль головы и пределы pruning evidence"
source_pdf_pages: [478, 482]
book_pages: [442, 446]
batch: "010"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 160. Checkpoint marginality: ранняя роль головы и пределы pruning evidence

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Что измеряет автор

В §§13.5.4 и Definition 13.20 сравниваются signed loss deltas от удаления **той же головы в фиксированном checkpoint**, без переобучения после удаления. Endpoint functional использует последний checkpoint; trajectory functional берёт максимум или взвешенный интеграл по времени. Модели автора имеют три слоя, восемь голов на слой, head dimension 24, model dimension 192 и FFN width 768. [Постановка](../source/0476-0478-ch13-log-centred-subadditivity.md#section-13-5-4), [Definition 13.20](../source/0479-0484-ch13-trajectory-values-and-redundancy-hierarchy.md#definition-13-20).

$$
\mathcal V_{\mathrm{traj}}(h;I)=\max_{t\in I}\Delta L_h(t),\qquad
\mathcal V_{\mathrm{end}}(h)=\Delta L_h(T).
$$

Если $T\in I$, максимум не меньше endpoint; это определение, а не theorem о положительном значении. На конечном наборе checkpoint максимум существует, для непрерывного $I$ без компактности/непрерывности корректнее использовать supremum. Интеграл зависит от нормировки $w(t)$ и длительности интервала; он не имеет автоматически тот же scale, что max.

## Что видно в числах

| Checkpoint | Oriented accuracy | Максимальная oriented marginal value | Максимальная antisymmetric sensitivity |
|---|---:|---:|---:|
| 8000 | 0.631 | 0.0457 | 0.0125 |
| 9000 | 0.666 | 0.00220 | 0.000610 |
| 10000 | 0.995 | 0.00864 | 0.00778 |
| 22000 | 1.000 | 0.000000830 | 0.0000000612 |

Это авторские значения из [Table 13.4](../source/0479-0484-ch13-trajectory-values-and-redundancy-hierarchy.md#table-13-4). Они поддерживают различие endpoint и earlier-checkpoint чувствительности. Однако максимумы обеих колонок среди перечисленных checkpoint находятся на **8000**, не на самом переходе 9000→10000. Поэтому фраза о наибольшем сигнале «near transition» требует широкого определения этого интервала.

На шаге 10000 mixed accuracy 0.998 превышает и symmetric 0.994, и oriented 0.995. Для единой непересекающейся смеси на той же выборке weighted average не может превышать обе компоненты. Возможно, использованы разные наборы или определения метрик, но это не пояснено; числа сохранены буквально.

## Причинный смысл и пределы

Нулевая single-head delta в конце не различает распределённое представление, взаимозаменяемость, насыщение выбранной loss и другие механизмы. Высокая delta раньше показывает локальную роль головы **при фиксированных остальных параметрах**; она не доказывает, что обучение без этой головы не смогло бы найти альтернативное решение. Совместное удаление половины голов — другая операция, не сумма single-head deltas.

Figure 13.9 показывает важный предел собственной protection policy: на checkpoint 10000 столбец crowding + protection около 2.9 выше crowding около 2.4. Следовательно, график не подтверждает универсальное улучшение от добавления protection. Сам автор называет directional-tail protection необходимой, но недостаточной и обсуждает разные failure modes. Наблюдение о столбцах уточняет предел применимости конкретной изображённой policy, согласуясь с оговоркой автора. Подписи разных правил и stage-dependent ошибки полезны как авторская иллюстрация, но без seeds, разброса и полного implementation нельзя установить её переносимость. [Figures 13.7–13.9](../source/0479-0484-ch13-trajectory-values-and-redundancy-hierarchy.md#figure-13-7).

Для проекта сохраняйте checkpoint, evaluation distribution, способ абляции, signed delta и uncertainty. Пороговый S–A tail, antisymmetric intervention и loss marginality — разные показатели. Figure 13.6 подчёркивает, что наиболее заметная oriented голова может иметь умеренное значение $\alpha_h$.
