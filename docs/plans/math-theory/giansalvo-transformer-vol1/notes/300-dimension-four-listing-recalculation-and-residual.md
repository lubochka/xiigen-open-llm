---
title: "300. Listing 17.1: воспроизведённый локальный расчёт и оставшийся signed residual"
source_pdf_pages: [712, 713]
book_pages: [676, 677]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 300. Listing 17.1: воспроизведённый локальный расчёт и оставшийся signed residual

## Что именно пересчитано

Выполнен напечатанный Python listing с нормализацией типографических пробелов. Это небольшой deterministic linear-algebra расчёт из заданных $X,M_a$, а не запуск обучения и не воспроизведение Figure 17.6 или Figure 17.7. [Полный Listing 17.1](../source/0710-0713-ch17-tracking-complexity-and-kernel-example.md#listing-17-1).

Из исходных массивов получается

$$
\mathcal B=
\begin{pmatrix}
-2&0.5&0.5&0\\
0.5&2&1&0\\
0.5&1&0&0\\
0&0&0&0
\end{pmatrix}.
$$

Матрица симметрична, имеет нулевой trace и rank 3; она indefinite, а не PSD. Локальный пересчёт даёт:

| Величина | Пересчитанное значение |
|---|---:|
| $\lambda_1$ | $2.5093840543$ |
| $\lambda_2$ | $0$ |
| $\lambda_3$ | $-0.3730777403$ |
| $\lambda_4$ | $-2.1363063141$ |
| $\theta_{\mathrm{rank}}$ | $3.7392736291\cdot10^{-8}$ |
| Relative gap top | $1.1486730338$ |
| Relative gap bottom | $0.8253631804$ |
| Shape и rank $Q$ | $4\times3$, rank 3 |
| $\|Q^\top Q-I\|_F$ | $4.4440791914\cdot10^{-16}$ |

Эти величины согласуются с напечатанными результатами в пределах округления. Python index 1 соответствует математическому $\lambda_2$; знак выбранного eigenvector несущественен, kernel space равен $\operatorname{span}\{e_4\}$. [(17.47) и numerical results](../source/0710-0713-ch17-tracking-complexity-and-kernel-example.md#equation-17-47).

## Какие части Algorithm 7 listing заменяет

Source сам допускает full eig для $d=4$, и code действительно вызывает полную symmetric eigendecomposition. Он сразу берёт точные найденные крайние eigenvectors; собственных Lanczos iterations, RQI solve, dispatch по всем ветвям и convergence checks здесь нет. Финальный вызов QR пересчитывает decomposition данной маленькой матрицы; он не реализует incremental update из (17.34), хотя комментарий называет его incremental QR.

Listing также не строит факторизацию motor в обучаемые attention parameters и не запускает дальнейший optimisation loop. Поэтому воспроизведены его matrix outputs; утверждение «following Algorithm 7 step by step» шире реально исполняемого code.

## После одной head остаётся ненулевой residual

Редакторский дополнительный расчёт для $P=I-QQ^\top$ даёт спектр $P\mathcal BP$:

$$
\{-0.3730777403,\ 0,\ 0,\ 0\}
$$

с обычными rounding deviations около нуля, и

$$
\|P\mathcal BP\|_F=0.3730777403.
$$

То есть оставшаяся $v_3$ не является kernel direction: её энергия просто меньше крупнейших удалённых компонентов. Для используемого в Algorithm 7 default $\theta_w=0.4\Gamma_0$ имеем $\Gamma_0=\|\mathcal B\|_F=\sqrt{11}\approx3.31662$ и $\theta_w\approx1.32665$, так что остаток действительно ниже этого конкретного growth scale. Это допустимое threshold stopping, а не полное восстановление $\mathcal B$ или исчезновение всех non-kernel modes.

Здесь особенно видно ограничение критерия только по $\lambda_{\max}$: отрицательный остаток имеет maximum eigenvalue около нуля независимо от своей отрицательной амплитуды. Для общего signed task такая проверка может пропустить большую negative direction; нужен критерий, учитывающий обе стороны spectrum либо согласованную residual norm.

Наконец, rank $Q=3$ для одной head и одной kernel direction подтверждает различие counters из [карточки 299](299-structural-heads-deflation-dimensions-and-complexity-accounting.md). Этот пример не доказывает универсальный complexity law, экономию heads на других задачах или качество обученной модели.
