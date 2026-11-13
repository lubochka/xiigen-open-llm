---
title: "Chapter 16: PAC calibration and non-orthogonal, random, and adversarial growth"
type: source-transcription
language: en
batch: "013"
source_pdf_pages: [628, 630]
book_pages: [592, 594]
---

# Chapter 16: PAC calibration and non-orthogonal, random, and adversarial growth

[Library index](../README.md) · [Source and transcription policy](../SOURCE.md)

<a id="pdf-0628"></a>
## PDF 628 · Book page 592

<a id="table-16-5"></a>
**Table 16.5:** PAC bound verification on experimental data. $m_{\rm PAC}$ is the minimum sample count from the theorem stated in this part with $C_{\rm PAC}=1$ and $\delta=0.05$.

| Task | $d$ | $K^*$ | $\Delta_{\rm rel}$ | $m_{\rm PAC}$ | $m_{\rm train}$ | ratio |
|---|---:|---:|---|---:|---:|---:|
| CoV-2 (8-class) | 128 | 191 | $\sim\theta_w/2$ | $\sim24$ | 320 000 | $>10^4$ |
| SST-2 | 768 | 142 | $\sim\theta_w/10$ | $\sim48\,000$ | 67 349 | 1.40 |

The SST-2 ratio (1.40) is the tighter of the two: the training set is only 40% larger than the PAC requirement. This is consistent with the experimental observation that SST-2 is the harder task (lower accuracy, higher $\kappa_A$, more heads required).

<a id="section-16-10"></a>
## 16.10 Beyond orthogonal deflation

The compressed-sensing bound of the theorem stated in this part assumes that each growth event captures a direction orthogonal to all previous ones. This orthogonality is enforced by the deflation step $Q\leftarrow[Q\mid v_k]$ in the relevant algorithm, but many practical growth strategies — gradient-based NAS, evolutionary search, progressive stacking — do not guarantee it. This section asks: under what conditions on the growth directions $\{v_1,\ldots,v_K\}$ does the universal law still hold?

The answer reveals a sharp phase transition between three regimes, governed by the coherence of the growth directions.

<a id="section-16-10-1"></a>
### 16.10.1 The RIP condition for deflation

Let $Q_K=[v_1\mid\ldots\mid v_K]\in\mathbb R^{d\times K}$ collect the growth directions (not necessarily orthonormal). The *deflation RIP constant* is

<a id="equation-16-53"></a>
$$
\delta_K=\|Q_K^\top Q_K-I_K\|_2,
\tag{16.53}
$$

i.e. the worst-case deviation of the Gram matrix from identity. If $\delta_K=0$, the directions are orthonormal and the theorem stated in this part applies directly. If $\delta_K>0$, each deflation step captures less energy than optimal: the captured energy at step $k$ satisfies

<a id="equation-16-54"></a>
$$
\Delta\Gamma_k=v_k^\top A_{\rm res}^{(k-1)}v_k\ge(1-\delta_K)\,\lambda_{\max}(A_{\rm res}^{(k-1)}),
\tag{16.54}
$$

provided $v_k$ is $\varepsilon$-aligned with the optimal direction $(|v_k^\top w_k-1|\le\varepsilon$ with $\varepsilon=O(\delta_K))$.

<a id="section-16-10-2"></a>
### 16.10.2 Regime 1: $\varepsilon$-orthogonal growth

<a id="theorem-16-37"></a>
**Theorem 16.37 (Non-orthogonal CS bound).** Let the growth directions $\{v_1,\ldots,v_K\}$ satisfy:

- (R1) $|v_i^\top v_j|\le\varepsilon$ for all $i\ne j$ ($\varepsilon$-orthogonality);
- (R2) $|v_k^\top w_k|\ge1-\varepsilon$ for all $k$ ($\varepsilon$-alignment with the greedy optimum);
- (R3) $\delta_K=\|Q_K^\top Q_K-I_K\|_2\le\delta$ for some $\delta<1$ (RIP condition).

Then the terminal head count satisfies

<a id="equation-16-55"></a>
$$
K_\delta^*\le\frac{(1+\delta)^2}{1-\delta}\,\kappa_A^2\log\frac{\Gamma_0}{\theta_w}+O(\delta\,\kappa_A^2).
\tag{16.55}
$$

In particular:

<a id="pdf-0629"></a>
## PDF 629 · Book page 593

1. (i) $\delta=0$: recovers the orthogonal bound $K^*=\Theta(\kappa_A^2\log(\Gamma_0/\theta_w))$.
2. (ii) $\delta=O(1/\kappa_A)$: $K_\delta^*=K^*\cdot(1+O(1/\kappa_A))$.
3. (iii) $\delta=O(1)$: $K_\delta^*=O(K^*)$ with a constant-factor overhead.

*Proof.* Under the RIP-style control condition, the residual energy after $K$ deflation steps satisfies

<a id="equation-16-56"></a>
$$
\Gamma_{\rm res}^{(K)}\le\Gamma_0\cdot\left(1-\frac{1-\delta}{\kappa_A^2}\right)^K.
\tag{16.56}
$$

Setting $\Gamma_{\rm res}^{(K)}\le\theta_w$ and solving for $K$:

<a id="equation-16-57"></a>
$$
K\ge\frac{\log(\Gamma_0/\theta_w)}{\log\bigl(1/(1-(1-\delta)/\kappa_A^2)\bigr)}
\tag{16.57}
$$

<a id="equation-16-58"></a>
$$
\phantom{K}\;=\frac{\kappa_A^2}{1-\delta}\log\frac{\Gamma_0}{\theta_w}\,(1+O(1/\kappa_A^2)).
\tag{16.58}
$$

The $(1+\delta)^2$ factor in the upper bound accounts for the energy “leakage” due to non-orthogonal directions: each deflation step also perturbs previously captured directions by $O(\delta)$. The formal tracking argument uses the Neumann series $(Q_K^\top Q_K)^{-1}=I-(Q_K^\top Q_K-I)+(Q_K^\top Q_K-I)^2-\cdots$, which converges for $\delta<1$.

**Condition (R1) and coherence.** Under $\varepsilon$-orthogonality with $K$ directions, the Gershgorin disc theorem gives $\delta_K\le(K-1)\varepsilon$. For the RIP condition $\delta<1$ to hold, one needs $\varepsilon<1/(K-1)\approx1/K^*$. This is the *coherence condition*: the growth algorithm must produce directions that are nearly orthogonal, with inner products bounded by $O(1/K^*)$. INCRT satisfies this with $\varepsilon=0$ (exact orthogonality); algorithms that use approximate deflation (e.g. NAS with warm-starting) typically achieve $\varepsilon=O(\eta)$ where $\eta$ is the learning rate.

<a id="section-16-10-3"></a>
### 16.10.3 Regime 2: random growth

When the growth directions are chosen independently of the task operator (e.g. random head allocation), the situation is qualitatively different.

<a id="theorem-16-38"></a>
**Theorem 16.38 (Random growth lower bound).** If the growth directions $v_k$ are i.i.d. uniform on $S^{d-1}$ (independent of $\mathcal B$), then with probability $\ge1-\delta$:

<a id="equation-16-59"></a>
$$
K_{\rm random}^*\ge\Omega\!\left(d\cdot\log\frac{\Gamma_0}{\theta_w}\right).
\tag{16.59}
$$

In particular, random growth requires $\Theta(d/\kappa_A^2)$ times more heads than greedy deflation.

*Proof sketch.* The clean argument is stated for the trace residual $T^{(k)}=\operatorname{tr}(A_{\rm res}^{(k)})$, not directly for the Frobenius residual. For a uniformly random direction $v\sim\operatorname{Unif}(S^{d-1})$, $\mathbb E[v^\top A_{\rm res}v]=\operatorname{tr}(A_{\rm res})/d$. Therefore one random one-dimensional deflation removes only a $1/d$ fraction of the trace residual in expectation:

<a id="display-0629-random-trace-recurrence"></a>
$$
\mathbb E[T^{(k+1)}\mid T^{(k)}]\le\left(1-\frac1d\right)T^{(k)}.
$$

Iterating gives the lower scaling $K=\Omega(d\log(T^{(0)}/T^{(K)}))$. Under bounded-effective-rank assumptions relating trace and Frobenius residuals, the same scaling transfers to $\Gamma_{\rm res}$. Thus the result is a trace-based obstruction to random growth, used here as a qualitative comparison with spectrally aligned greedy growth. $\square$

<a id="pdf-0630"></a>
## PDF 630 · Book page 594

**Interpretation.** The random-growth theorem shows that the $\kappa_A^2$ factor in the universal law is not an artifact of the greedy algorithm: it reflects a genuine advantage of spectral alignment. Random growth replaces $\kappa_A^2$ with $d$, which is always worse ($\kappa_A\le d$ by construction, and typically $\kappa_A\ll d$: for SST-2, $\kappa_A\approx13$ vs $d=768$). The ratio $d/\kappa_A^2\approx4.4$ quantifies the efficiency gain of greedy spectral deflation over random allocation on this task.

<a id="section-16-10-4"></a>
### 16.10.4 Regime 3: adversarial growth

For completeness, the discussion notes that adversarial growth directions (chosen to avoid the support of $\mathcal B$) can prevent convergence entirely. If $v_k\perp\operatorname{range}(\mathcal B)$ for all $k$, then $\Delta\Gamma_k=0$ and $\Gamma_{\rm res}^{(K)}=\Gamma_0$ regardless of $K$. This trivial observation confirms that some structural assumption on the growth directions is necessary for the universal law to hold.

<a id="section-16-10-5"></a>
### 16.10.5 Summary

The three regimes are:

<a id="table-16-6"></a>
**Table 16.6:** Non-orthogonal growth: three regimes.

| Regime | Condition on $\{v_k\}$ | Head count $K^*$ |
|---|---|---|
| 1. $\varepsilon$-orthogonal | $\lvert v_i^\top v_j\rvert\le\varepsilon$, $\delta_K<1$ | $\Theta(\kappa^2\log)\cdot(1+O(\delta))$ |
| 2. Random | i.i.d. uniform on $S^{d-1}$ | $\Theta(d\log)$ |
| 3. Adversarial | $v_k\perp\operatorname{range}(\mathcal B)$ | $\infty$ |

The universal law $K^*=\Theta(\kappa_A^2\log(\Gamma_0/\theta_w))$ is robust to small perturbations of orthogonality (Regime 1 with $\delta\ll1$), degrades gracefully to $\Theta(d\log)$ under random growth (Regime 2), and fails under adversarial allocation (Regime 3). The transition from Regime 1 to Regime 2 occurs at $\delta_K\approx1$, i.e. when the coherence exceeds $1/K^*$.

<a id="section-16-11"></a>
## 16.11 Single-time-scale dynamics and Polyak–Ruppert recovery

The bilateral gate of INCRT I converges under a two-time-scale assumption. The task learning rate $\alpha$, with which the network parameters are updated, must be small enough relative to the gate learning rate $\eta^+$, with which the diagnostic eigenvectors $u^+,u^-$ are tracked, that the residual operator $A_{\rm res}$ varies slowly on the time scale of the gate dynamics. Quantitatively, assumption (A3) of the theorem stated in this part of INCRT I requires

<a id="equation-16-60"></a>
$$
\alpha\le\frac{(\Delta^+)^2\eta^+}{2\,\|X\|_F^2\,\|\nabla_{M_a}\ell\|_F},
\tag{16.60}
$$

which, when $\Delta^+$ and $\|\nabla_{M_a}\ell\|_F$ are of comparable magnitude, reduces to the qualitative statement $\alpha\ll\eta^+$.

In modern deep learning practice the assumption is rarely satisfied. Foundation models trained with AdamW typically use a single optimiser to update all parameters, with no distinction between diagnostic and task variables, and the effective learning rates of all parameter groups are within a factor of two of one another [citation]. The two-time-scale separation is, in this regime, collapsed.

The collapse has two consequences for the framework as developed so far. First, the gate convergence proof of the theorem stated in this part of INCRT I no longer applies directly:
