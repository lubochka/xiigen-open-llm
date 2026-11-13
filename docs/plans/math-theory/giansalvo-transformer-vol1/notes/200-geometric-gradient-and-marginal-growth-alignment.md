---
title: "200. Согласование геометрии, градиента и marginal value: недостающие связки"
source_pdf_pages: [558, 562]
book_pages: [522, 526]
batch: "012"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 200. Согласование геометрии, градиента и marginal value: недостающие связки

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Три разных вопроса контроллера

Coverage $\rho_\ell$ показывает расстояние от сигнатур до словаря. (15.35) пытается выделить unresolved gradient $G_\ell^\perp$. (15.37) проверяет фактический выигрыш после добавления кандидата, вычитая size/crowding penalties. [Геометрия и градиент](../source/0560-0563-ch15-gates-growth-criteria-and-budget.md#section-15-15-1), [marginal criterion](../source/0560-0563-ch15-gates-growth-criteria-and-budget.md#equation-15-37).

Propositions 15.11 и 15.15 заменяют полную equivalence более слабым alignment при near-linear, slow-drift и stable-window условиях. Это полезное разграничение, однако доказательствам нужны дополнительные связи между измеряемыми пространствами. [15.11](../source/0556-0559-ch15-stability-cover-preservation-and-termination.md#proposition-15-11), [15.15](../source/0560-0563-ch15-gates-growth-criteria-and-budget.md#proposition-15-15).

## Одинаковая стабильность не создаёт одинаковую геометрию

Проекция $P_{\mathcal D_\ell(t)}G_\ell(t)$ определена только если gradient signal и span signature directions живут в совместимом линейном пространстве либо задано отображение между ними. Сигнатура, параметрический градиент и изменение выходов сети имеют разные типы. Нужен конкретный tangent feature map и метрика.

Простой редакторский пример показывает, чего не обеспечивает постоянный NTK сам по себе. В линейной модели кандидаты изменяют выход вдоль $u_1=e_1$ и $u_2=e_2$, а residual задачи направлен вдоль $e_2$. Тогда первый кандидат даёт нулевой first-order gain, второй — положительный. Если независимая signature metric оценивает uncovered region первого выше, geometric ranking противоположен loss ranking даже при нулевом NTK drift и точных измерениях. Чтобы исключить такую ситуацию, требуется связь signature distance с task-relevant tangent projection. Одна near-linearity этой связи не задаёт.

Скаляр $T_\ell=\sqrt{\rho_\ell}\|G_\ell^\perp\|_F$ также агрегирует величины по слою. Он не содержит направления проекции отдельно для каждого кандидата и не равен автоматически squared gradient projection, о котором говорит proof sketch. Кандидатное ранжирование требует локальных, согласованных по регионам quantities. [Score (15.36)](../source/0560-0563-ch15-gates-growth-criteria-and-budget.md#equation-15-36).

## Положительный loss contribution и положительный net value

(15.37) содержит штрафы. Если validation loss уменьшается на $0.01$, а $\lambda_{\mathrm{size}}\Delta\operatorname{size}=1$ и crowding penalty равен нулю, то

$$
V_{\mathrm{marg},h}=0.01-1=-0.99.
$$

Это совместимо с положительным первым вкладом в **нештрафованный** loss gain. Оно показывает, почему такой вклад ещё не разрешает приёмку кандидата.

В proof sketch 15.15 size/crowding penalties отнесены к ошибкам $O(\eta W+\xi+m^{-1/2})$, но явного предположения об их масштабе нет. Фиксированный complexity penalty не исчезает при $\eta W,\xi\to0$ и $m\to\infty$. Для bound на полный marginal value штраф нужно оставить явным либо отдельно ограничить.

## Условие согласования ranking

Если уже доказано, что кандидатные оценки $s_h$ и $t_h$ имеют одну шкалу и $|s_h-t_h|\le\varepsilon$ для каждого $h$, то gap лучшего $s$-кандидата больше $2\varepsilon$ достаточен для совпадения argmax. Этот стандартный вывод редакторский; он объясняет, какую именно равномерную ошибку и margin требуется установить перед применением авторской фразы о gap.

Для проекта три критерия разумно хранить раздельно: где есть uncovered demand, какой реализуемый tangent direction способен уменьшить ошибку, и что получилось после безопасного вмешательства. Работа книги на этих страницах не доказывает уменьшения числа тренировочных примеров или времени обучения.
