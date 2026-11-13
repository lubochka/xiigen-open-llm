---
title: "324. Runtime results: арифметика, CI против SD и предел curriculum explanation"
source_pdf_pages: [749, 750]
book_pages: [713, 714]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 324. Runtime results: арифметика, CI против SD и предел curriculum explanation

## Вопрос

Насколько сильны reported runtime improvements и выделяет ли опыт именно closed-loop capacity curriculum как причину?

**Table 18.8 сообщает большой эффект в исследованных runs, особенно на длинном горизонте, но часть summary несовместима по метрике, а уникальная причинная интерпретация не установлена.** Ни обучение SAC, ни plant simulation здесь не воспроизводились. [Table 18.8](../source/0749-0750-ch18-runtime-control-results-curriculum-and-prediction-p5.md#table-18-8).

## Пересчёт displayed means

| $\tau_z$ | Static $\Delta\%$ | Runtime $\Delta\%$ | Разность runtime − static | Напечатано |
|---:|---:|---:|---:|---:|
| 1 | −40.0 | −45.8 | −5.8 pp | −5.8 |
| 2 | −35.0 | −50.9 | −15.9 pp | −15.8 |
| 5 | +5.0 | −54.2 | −59.2 pp | −59.1 |

Расхождения по 0.1 pp могут возникнуть при расчёте из неокруглённых исходных mean; raw values отсутствуют, поэтому таблицу не исправляем молча. Число “forty-four points” для worst runtime seed согласуется с $-38.8-5.0=-43.8$ pp после округления. Однако заявление, что matched improvement 15.8 pp превосходит **любое** static advantage над Transformer, расходится с предыдущими 12 и **19** pp: 15.8 меньше 19.

В Table 18.7 matched static cell имеет $0/5$ seeds, а Table 18.8 — $0/10$ при том же mean −35.0. Возможно расширение серии, но нужно явно идентифицировать обновлённую выборку; слово “matched” не объясняет замену.

## Confidence interval не равен standard deviation

Таблица обозначает $\pm7.1,\pm8.4,\pm5.4$ как $\mathrm{CI}_{95}$, а проза сообщает per-seed SD $9.9,11.7,5.4$. При стандартном two-sided Student interval для $n=10$

$$
\text{half-width}=t_{0.975,9}\,s/\sqrt{10},
\qquad t_{0.975,9}\approx2.2622.
$$

Первые два half-width соответствуют SD около 9.93 и 11.74, согласующимся с прозой. Третий 5.4 соответствует SD около **7.55**, а при SD 5.4 half-width был бы **3.86**. Другой, например bootstrap, способ построения CI не исключён, но не указан; использовать 5.4 одновременно как обе метрики без clarification нельзя.

Наблюдение $0/10$ runtime failures против $4/10$ static не доказывает elimination общей failure probability. Для заранее заданного непарного сравнения Fisher exact даёт one-sided $p\approx0.04334$, two-sided $p\approx0.08669$; выбор направления и парности существенно влияет на интерпретацию. При matched seeds нужны сами пары исходов. Это проверка counts, не reported confidence interval tracking mean и не воспроизведение исходного statistical analysis.

## Что поддерживает curriculum hypothesis

На длинном горизонте обе схемы заканчивают с $K=8$, но runtime начинает с $K=1$. Это мотивирует проверку роли траектории. На других горизонтах static retained $K=4,7$, поэтому сравнение дополнительно меняет конечную capacity. В основной runtime серии prune не срабатывает и все runs достигают cap; experiment не демонстрирует индивидуальную terminal architecture для каждого $\tau_z$.

Проза называет schedule одинаковой, но Figure 18.13 показывает различающиеся моменты переходов у трёх representative seeds. Общий monotone-to-cap pattern не тождественен одинаковому timing. Cap ablation сообщает лучшие mean при $K_{\max}=2$, что дополнительно ослабляет интерпретацию $K=8$ как необходимого optimum. [Figure 18.13 и interpretation](../source/0749-0750-ch18-runtime-control-results-curriculum-and-prediction-p5.md#figure-18-13).

Cooldown sweep даёт −41.2, −51.7, −36.6% для 1000, 2000, 5000 steps; default −51.7 отличается от −50.9 основной таблицы на 0.8 pp. Это может быть другая серия, но её seed budget и связь с основной требуют уточнения. Наличие оптимального timing совместимо с curriculum, однако само по себе не отделяет residual-driven feedback от заранее заданного growth schedule с теми же event times.

Наконец, одна голова тоже может выдавать постоянную или неинформативную policy. Отсутствие proper subset из нескольких silenced heads не доказывает отсутствие всех bad attractors при $K=1$, а exact continuity вставки не доказывает уход из их basin. Полезное empirical ядро — reported улучшение при постепенной активации capacity; объяснение через конкретную геометрию attractors остаётся проверяемой гипотезой.
