---
title: "301. Near-degenerate benchmark: разные targets, бюджеты и смысл rate"
source_pdf_pages: [713, 715]
book_pages: [677, 679]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 301. Near-degenerate benchmark: разные targets, бюджеты и смысл rate

## Что именно сообщает автор

На спектре $(2,1.99,1,0.5,0.2,0.1,0.05,0)$ source сравнивает Oja, restarted Lanczos с $m=2$ и regularised block method. Table 17.5 сообщает конечные ошибки $0.95$, $0.94$ и $4.6\cdot10^{-5}$; только block method достигает указанного tolerance в испытанном бюджете. Это **авторский численный результат**, не воспроизведённый здесь эксперимент. Заданы eigenvalues и некоторые параметры, но не сама матрица в выбранной basis, полный initial state, realization perturbation, implementation retraction и trajectory. [Setup и (17.49)](../source/0710-0713-ch17-tracking-complexity-and-kernel-example.md#section-17-6-3), [Figure 17.7 и Table 17.5](../source/0714-0717-ch17-empirical-diagnostics-and-signed-gates.md#figure-17-7).

Полезный вывод — устойчивой целью при тесном cluster может быть весь subspace. Данные из этого единственного описания не устанавливают одинаковую вычислительную стоимость или универсальное превосходство solver.

## У методов разные задачи восстановления

Single-vector methods оцениваются по углу к конкретному $v_1$. Block method оценивается по расстоянию двумерного пространства к $V_2=\operatorname{span}\{v_1,v_2\}$. Для данного spectrum:

$$
\lambda_1-\lambda_2=0.01,\qquad
\frac{\lambda_1-\lambda_2}{\lambda_1}=0.005,\qquad
\lambda_2-\lambda_3=0.99.
$$

То есть block target отделён от complement gap, в 99 раз большим внутреннего gap. Малый subspace angle не требует определения индивидуальной ориентации $v_1$ внутри $V_2$. Например, basis $\{(v_1+v_2)/\sqrt2,(v_1-v_2)/\sqrt2\}$ восстанавливает $V_2$ точно, хотя первый столбец имеет angle sine $1/\sqrt2$ к $v_1$.

Сравнение разумно для решения «восстанавливать head direction или cluster», но не является сравнением трёх методов на одном error target. Для общей accuracy/compute диаграммы нужен одинаковый конечный объект: например, projector при одинаковом rank или остаточная captured energy. Source честно различает метрики в setup; эту оговорку надо переносить вместе с таблицей.

## Iteration не всегда равна одному matvec

В source заданы 500 Oja iterations, 20 Lanczos restarts примерно по два matvec и 200 block iterations с двумя столбцами. Применение $A$ ко всему block требует эквивалента двух vector matvec, помимо orthogonalisation и прочих операций. Подпись оси “Iterations (matvec count)” не задаёт единого способа конвертации этих budgets.

Фраза “same initial vector $u_0$” также не полностью определяет initial $W_0\in\operatorname{St}(2,8)$: нужен второй direction и его ортогонализация. Нельзя воспроизвести или проверить равенство условий по одному такому указанию.

## Gap и contraction factor имеют разный смысл

На PDF 715 скорость около $0.95$ сопоставлена с gap $0.99$. Эти числа не обязаны совпадать: contraction дискретного flow зависит и от шага, и от нормировки gradient. При $\eta=0.05$ простая линейная модель даёт

$$
1-\eta(\lambda_2-\lambda_3)=0.9505,
$$

а при gradient с factor 2 — $1-2\eta(\lambda_2-\lambda_3)=0.901$. Поэтому $0.95$ может быть совместимо с одним соглашением, но само значение gap не доказывает наблюдаемый rate. Кроме того, $0.99$ — gap **исходного** заданного spectrum; perturbed gap может отличаться, хотя при малом perturbation лишь немного. [Connection to the Lyapunov argument](../source/0714-0717-ch17-empirical-diagnostics-and-signed-gates.md#pdf-0715).

Figure caption связывает 185 iterations с ошибкой $5\cdot10^{-5}$, а Table 17.5 — с пересечением $10^{-4}$; final error $4.6\cdot10^{-5}$ относится к полному budget 200. Это разные milestones, которые source не позволяет точно восстановить без raw trajectory. Их нельзя молча считать одним и тем же threshold.

## Что означает finite-budget saturation

Большая ошибка в данном запуске не означает невозможности сходимости single-vector method при положительном gap. Начальное overlap, бюджет, шаг и solver details имеют значение; даже exact dominant vector как initial condition уже даёт нулевую angular error. В Type A individual dominant basis не уникальна, но это не запрещает сходимость к одному вектору или к доминирующему subspace.

Поэтому operational вывод ограничен: этот reported benchmark иллюстрирует смысл cluster extraction при тесном spectrum. Он не обосновывает universal iteration barrier, одинаковый compute budget или точность одной конкретной head после восстановления только projector.
