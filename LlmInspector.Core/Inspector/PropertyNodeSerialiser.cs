// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmInspector.Inspector;

using System.Collections;
using System.Globalization;
using System.Reflection;

// This helper converts runtime objects into a stable property tree for station inspection.
public static class PropertyNodeSerialiser
{
    // This constant prevents recursive object graphs from expanding without bound.
    private const int MaxDepth = 6;

    // Serialise converts any runtime object into the root property-node list shown by the inspector.
    public static IReadOnlyList<PropertyNode> Serialise(object? value, int depth = 0)
    {
        // Route null values to a leaf so the inspector can show absence explicitly.
        if (value is null)
            return [Leaf("value", "(null)", null)];

        // Stop at the depth cap so nested framework objects cannot overwhelm the UI.
        if (depth > MaxDepth)
            return [Leaf("value", "(max depth)", null)];

        // Return scalar values as one leaf because they have no meaningful inspectable children.
        if (TrySerialiseScalar("value", value, out var scalarNode))
            return [scalarNode];

        // Return enumerable values as indexed children so arrays remain easy to inspect.
        if (TrySerialiseEnumerable(value, depth, out var enumerableNode))
            return enumerableNode.Children;

        // Read public instance properties because records and envelopes expose their shape there.
        var properties = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Fall back to a leaf when reflection finds no readable public properties.
        if (properties.Length == 0)
            return [Leaf("value", value.ToString() ?? string.Empty, value.GetType().Name)];

        // Convert every readable property into one node in declaration order.
        return properties
            .Where(property => property.GetIndexParameters().Length == 0)
            .Select(property => SerialiseProperty(value, property, depth))
            .ToArray();
    }

    // SerialiseProperty converts one reflected property into either a scalar leaf or child subtree.
    private static PropertyNode SerialiseProperty(object owner, PropertyInfo property, int depth)
    {
        // Read the property value once so scalar checks and subtree checks see the same data.
        var propertyValue = property.GetValue(owner);

        // Render null property values as explicit null leaves.
        if (propertyValue is null)
            return Leaf(property.Name, "(null)", property.PropertyType.Name);

        // Render scalar property values as leaves.
        if (TrySerialiseScalar(property.Name, propertyValue, out var scalarNode))
            return scalarNode;

        // Render collection property values as a branch with indexed children.
        if (TrySerialiseEnumerable(propertyValue, depth + 1, out var enumerableNode))
            return enumerableNode with { Name = property.Name, TypeHint = property.PropertyType.Name };

        // Render complex property values as a branch with reflected child properties.
        return new PropertyNode(
            property.Name,
            null,
            property.PropertyType.Name,
            Serialise(propertyValue, depth + 1));
    }

    // TrySerialiseScalar handles values that should always be shown as a single text value.
    private static bool TrySerialiseScalar(string name, object value, out PropertyNode node)
    {
        // Render strings directly because they are enumerable but should not be split into characters.
        if (value is string text)
        {
            node = Leaf(name, text, "string");
            return true;
        }

        // Render booleans with invariant text for stable tests and display.
        if (value is bool boolean)
        {
            node = Leaf(name, boolean.ToString(CultureInfo.InvariantCulture), "bool");
            return true;
        }

        // Render date-time offsets in round-trip format so evidence can be compared exactly.
        if (value is DateTimeOffset dateTimeOffset)
        {
            node = Leaf(name, dateTimeOffset.ToString("O", CultureInfo.InvariantCulture), "DateTimeOffset");
            return true;
        }

        // Render date-time values in round-trip format for the same reason as DateTimeOffset.
        if (value is DateTime dateTime)
        {
            node = Leaf(name, dateTime.ToString("O", CultureInfo.InvariantCulture), "DateTime");
            return true;
        }

        // Render guids as stable strings.
        if (value is Guid guid)
        {
            node = Leaf(name, guid.ToString("D", CultureInfo.InvariantCulture), "Guid");
            return true;
        }

        // Render enums by name because numeric values hide the human meaning.
        if (value.GetType().IsEnum)
        {
            node = Leaf(name, value.ToString() ?? string.Empty, value.GetType().Name);
            return true;
        }

        // Render primitive and decimal values using invariant culture for stable evidence.
        if (value.GetType().IsPrimitive || value is decimal)
        {
            node = Leaf(name, Convert.ToString(value, CultureInfo.InvariantCulture), value.GetType().Name);
            return true;
        }

        // Report that complex values need normal subtree handling.
        node = null!;
        return false;
    }

    // TrySerialiseEnumerable renders arrays and lists as indexed children.
    private static bool TrySerialiseEnumerable(object value, int depth, out PropertyNode node)
    {
        // Avoid treating strings as enumerable values because they are handled as scalar text.
        if (value is string)
        {
            node = null!;
            return false;
        }

        // Non-enumerable values must be handled by the caller.
        if (value is not IEnumerable enumerable)
        {
            node = null!;
            return false;
        }

        // Materialize the sequence once so empty collections can be represented explicitly.
        var items = enumerable.Cast<object?>().ToArray();

        // Render empty collections as a single empty marker.
        if (items.Length == 0)
        {
            node = new PropertyNode("value", null, value.GetType().Name, [Leaf("count", "0", "int")]);
            return true;
        }

        // Render each collection item under its numeric index.
        var children = items
            .Select((item, index) => SerialiseIndexedItem(item, index, depth))
            .ToArray();

        // Return the collection branch to the caller.
        node = new PropertyNode("value", null, value.GetType().Name, children);
        return true;
    }

    // SerialiseIndexedItem converts one collection item into an indexed branch or leaf.
    private static PropertyNode SerialiseIndexedItem(object? item, int index, int depth)
    {
        // Render null collection items as indexed null leaves.
        if (item is null)
            return Leaf($"[{index}]", "(null)", null);

        // Render scalar collection items as indexed scalar leaves.
        if (TrySerialiseScalar($"[{index}]", item, out var scalarNode))
            return scalarNode;

        // Render complex collection items as indexed branches.
        return new PropertyNode(
            $"[{index}]",
            null,
            item.GetType().Name,
            Serialise(item, depth + 1));
    }

    // Leaf creates one terminal property node.
    private static PropertyNode Leaf(string name, string? value, string? typeHint) =>
        new(name, value, typeHint, []);
}
