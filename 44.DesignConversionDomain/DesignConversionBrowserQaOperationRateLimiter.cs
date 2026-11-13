// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

internal sealed class DesignConversionBrowserQaOperationRateLimiter
{
    private readonly object _sync = new();
    private readonly int _maxPermitsPerWindow;
    private readonly TimeSpan _window;
    private readonly Func<DateTimeOffset> _utcNow;
    private DateTimeOffset _windowStartedAtUtc;
    private int _usedPermits;

    public DesignConversionBrowserQaOperationRateLimiter(
        int maxPermitsPerWindow,
        TimeSpan window,
        Func<DateTimeOffset>? utcNow = null)
    {
        if (maxPermitsPerWindow <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxPermitsPerWindow), "Rate limit permits must be positive.");
        if (window <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(window), "Rate limit window must be positive.");

        _maxPermitsPerWindow = maxPermitsPerWindow;
        _window = window;
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        _windowStartedAtUtc = NormalizeUtc(_utcNow());
    }

    public int MaxPermitsPerWindow => _maxPermitsPerWindow;

    public TimeSpan Window => _window;

    public bool TryAcquire(int permits = 1)
    {
        if (permits <= 0)
            throw new ArgumentOutOfRangeException(nameof(permits), "Rate limit permits must be positive.");

        lock (_sync)
        {
            var now = NormalizeUtc(_utcNow());
            if (now < _windowStartedAtUtc || now - _windowStartedAtUtc >= _window)
            {
                _windowStartedAtUtc = now;
                _usedPermits = 0;
            }

            if (permits > _maxPermitsPerWindow || _usedPermits > _maxPermitsPerWindow - permits)
                return false;

            _usedPermits += permits;
            return true;
        }
    }

    private static DateTimeOffset NormalizeUtc(DateTimeOffset value)
        => value.ToUniversalTime();
}
