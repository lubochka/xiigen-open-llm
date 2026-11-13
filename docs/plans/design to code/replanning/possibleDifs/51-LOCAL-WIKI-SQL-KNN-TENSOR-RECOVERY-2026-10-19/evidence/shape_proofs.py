"""Exact small-number mirrors of two source-index contracts; not a .NET run.

The default run is a standalone mathematical proof. Pass --source-root with
core/ and wiki/ snapshot trees to additionally verify the four pinned Git blobs.
No model, training, checkpoint import, external provider, repository mutation or
CI occurs.
"""

from __future__ import annotations

import argparse
import hashlib
import json
from itertools import product
from pathlib import Path


ROOT = Path(__file__).resolve().parent
CORE = "4945b8e85c17b5f150eb5b840d16bbdc4b506242"
WIKI = "219d7ba89dc3ef43e03e17f1461ad96154d3dd7a"
EXPECTED_SOURCES = [
    {"root": "core", "ref": CORE, "path": "00.TensorCore/Core/TensorOps.cs",
     "blob_sha": "e1a20dca84d76ee85d0c559bd496c1fa3c96ac8b"},
    {"root": "core", "ref": CORE, "path": "00.TensorCore/Core/TensorShape.cs",
     "blob_sha": "0879e0702f210e2dd45358af09bddff2a4ef9874"},
    {"root": "wiki", "ref": WIKI, "path": "00.TensorCore/Core/TensorOps.cs",
     "blob_sha": "b9ca2b1be9498ceb245f78caf2bd72cad69d46be"},
    {"root": "wiki", "ref": WIKI, "path": "00.TensorCore/Core/TensorShape.cs",
     "blob_sha": "09eed499bcc64b6917a1c73eecdbb38bf172e315"},
]


def blob_sha(data: bytes) -> str:
    return hashlib.sha1(b"blob " + str(len(data)).encode() + b"\0" + data).hexdigest()


def coordinates(shape):
    return list(product(*(range(n) for n in shape)))


def flat_index(shape, indices):
    # Source contract: TensorShape.FlatIndex checks indices.Length == Rank.
    if len(indices) != len(shape):
        raise ValueError(f"index count {len(indices)} != rank {len(shape)}")
    result = 0
    for extent, coordinate in zip(shape, indices):
        if not 0 <= coordinate < extent:
            raise IndexError((shape, indices))
        result = result * extent + coordinate
    return result


def mean_forward(values, shape, axis, keep_dims):
    output_shape = tuple(1 if i == axis else n for i, n in enumerate(shape))
    if not keep_dims:
        output_shape = tuple(n for i, n in enumerate(shape) if i != axis)
    output = []
    for output_coordinate in coordinates(output_shape):
        total = 0.0
        for reduced_coordinate in range(shape[axis]):
            if keep_dims:
                source_coordinate = list(output_coordinate)
                source_coordinate[axis] = reduced_coordinate
            else:
                source_coordinate = list(output_coordinate)
                source_coordinate.insert(axis, reduced_coordinate)
            total += values[flat_index(shape, source_coordinate)]
        output.append(total / shape[axis])
    return output, output_shape


def current_mean_backward(source_shape, output_shape, axis, output_grad):
    result = []
    for source_coordinate in coordinates(source_shape):
        # Source: dstIdx = srcIdx.ToArray(); dstIdx[axis] = 0.
        target_coordinate = list(source_coordinate)
        target_coordinate[axis] = 0
        result.append(output_grad[flat_index(output_shape, target_coordinate)] / source_shape[axis])
    return result


def finite_difference_mean(values, shape, axis, upstream, epsilon=1e-5):
    def objective(input_values):
        means, _ = mean_forward(input_values, shape, axis, False)
        return sum(g * value for g, value in zip(upstream, means))

    gradient = []
    for index in range(len(values)):
        plus, minus = list(values), list(values)
        plus[index] += epsilon
        minus[index] -= epsilon
        gradient.append((objective(plus) - objective(minus)) / (2 * epsilon))
    return gradient


def current_scalar_batched_matmul(a, a_leading, b, b_leading):
    # All final matrix dimensions are 1x1: isolates leading-index mapping.
    count = len(a)
    mapping = [batch % len(b) for batch in range(count)]
    output = [a[i] * b[mapping[i]] for i in range(count)]
    grad_a = [b[mapping[i]] for i in range(count)]
    grad_b = [0] * len(b)
    for i in range(count):
        grad_b[mapping[i]] += a[i]
    return {"output_shape": list(a_leading) + [1, 1], "b_flat_indices": mapping,
            "output": output, "grad_a_for_sum_output": grad_a,
            "grad_b_for_sum_output": grad_b}


def broadcast_scalar_reference(a, a_leading, b, b_leading):
    rank = max(len(a_leading), len(b_leading))
    left = (1,) * (rank - len(a_leading)) + tuple(a_leading)
    right = (1,) * (rank - len(b_leading)) + tuple(b_leading)
    if any(x != y and x != 1 and y != 1 for x, y in zip(left, right)):
        raise ValueError("leading dimensions do not broadcast")
    shape = tuple(max(x, y) for x, y in zip(left, right))
    output, grad_a, grad_b, mapping = [], [0] * len(a), [0] * len(b), []
    for coordinate in coordinates(shape):
        ai = flat_index(left, tuple(0 if n == 1 else i for n, i in zip(left, coordinate)))
        bi = flat_index(right, tuple(0 if n == 1 else i for n, i in zip(right, coordinate)))
        output.append(a[ai] * b[bi])
        grad_a[ai] += b[bi]
        grad_b[bi] += a[ai]
        mapping.append(bi)
    return {"output_shape": list(shape) + [1, 1], "b_flat_indices": mapping,
            "output": output, "grad_a_for_sum_output": grad_a,
            "grad_b_for_sum_output": grad_b}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source-root", type=Path,
                        help="Optional root containing core/<repo path> and wiki/<repo path>; exact blobs required")
    parser.add_argument("--output", type=Path, default=ROOT / "shape_proofs.json")
    args = parser.parse_args()
    verified = []
    if args.source_root is not None:
        for entry in EXPECTED_SOURCES:
            payload = (args.source_root / entry["root"] / entry["path"]).read_bytes()
            actual = blob_sha(payload)
            if actual != entry["blob_sha"]:
                raise RuntimeError(f"Source blob mismatch: {entry['ref']}/{entry['path']}")
            source = payload.decode()
            if entry["path"].endswith("TensorOps.cs"):
                for marker in ["batch % bBatchSize", "gradOutShape.FlatIndex(dstIdx)", "dstIdx[axis] = 0;"]:
                    if marker not in source:
                        raise RuntimeError(f"Source mechanism changed: {marker}")
            verified.append({"ref": entry["ref"], "path": entry["path"], "blob_sha": actual})

    values, shape, upstream = [1, 3, 5, 2, 4, 6], (2, 3), [2, 5]
    dropped_output, dropped_shape = mean_forward(values, shape, 1, False)
    kept_output, kept_shape = mean_forward(values, shape, 1, True)
    try:
        current_mean_backward(shape, dropped_shape, 1, upstream)
    except ValueError as error:
        dropped_backward = {"status": "rank_error", "message": str(error)}
    else:
        raise RuntimeError("Discriminating Mean branch did not fail")
    expected_gradient = [2 / 3] * 3 + [5 / 3] * 3
    fd = finite_difference_mean(values, shape, 1, upstream)
    if max(abs(a - b) for a, b in zip(fd, expected_gradient)) > 1e-8:
        raise RuntimeError("Independent finite-difference mean reference disagrees")

    a, b = [1, 2, 3, 4], [10, 100]
    current = current_scalar_batched_matmul(a, (2, 2), b, (2, 1))
    reference = broadcast_scalar_reference(a, (2, 2), b, (2, 1))
    if current["output"] == reference["output"]:
        raise RuntimeError("Discriminating MatMul branch did not diverge")
    controls = []
    for name, av, ashape, bv, bshape in [
        ("equal_leading_shapes", a, (2, 2), [10, 20, 30, 40], (2, 2)),
        ("two_dimensional_right_operand", a, (2, 2), [10], ()),
        ("broadcast_shared_over_leftmost_axis", a, (2, 2), [10, 100], (1, 2)),
    ]:
        left = current_scalar_batched_matmul(av, ashape, bv, bshape)
        right = broadcast_scalar_reference(av, ashape, bv, bshape)
        if left != right:
            raise RuntimeError(f"Control does not match: {name}")
        controls.append({"name": name, "same_output_and_gradient": True, "output": left["output"]})

    result = {
        "kind": "SOURCE_INDEX_MIRROR_AND_MATH_COUNTEREXAMPLE",
        "native_csharp_executed": False,
        "training_executed": False,
        "model_inference_executed": False,
        "quality_effect_measured": False,
        "source_verification_performed": args.source_root is not None,
        "expected_source_blobs": EXPECTED_SOURCES,
        "verified_source_blobs": verified,
        "mean_rank_reduction": {
            "input_shape": list(shape), "values": values, "axis": 1,
            "forward_keep_false": {"shape": list(dropped_shape), "output": dropped_output},
            "forward_keep_true": {"shape": list(kept_shape), "output": kept_output},
            "upstream": upstream,
            "current_keep_false_backward": dropped_backward,
            "current_keep_true_backward": current_mean_backward(shape, kept_shape, 1, upstream),
            "expected_keep_false_gradient": expected_gradient,
            "finite_difference_gradient": fd,
        },
        "matmul_axis_broadcast": {
            "a_shape": [2, 2, 1, 1], "a_values": a,
            "b_shape": [2, 1, 1, 1], "b_values": b,
            "current_modulo_formula": current,
            "right_aligned_broadcast_reference": reference,
            "controls": controls,
            "expanded_shape_case": {
                "a_shape": [2, 1, 1, 1], "a_values": [1, 2],
                "b_shape": [1, 2, 1, 1], "b_values": [10, 100],
                "current": current_scalar_batched_matmul([1, 2], (2, 1), [10, 100], (1, 2)),
                "reference": broadcast_scalar_reference([1, 2], (2, 1), [10, 100], (1, 2)),
            },
        },
    }
    target = args.output
    target.write_text(json.dumps(result, indent=2) + "\n")
    print(json.dumps({"saved": str(target), "verified_blobs": len(verified),
                      "mean_rank_error": dropped_backward["message"],
                      "matmul_current": current["output"], "matmul_reference": reference["output"],
                      "native_csharp_executed": False, "quality_effect_measured": False}, indent=2))


if __name__ == "__main__":
    main()
