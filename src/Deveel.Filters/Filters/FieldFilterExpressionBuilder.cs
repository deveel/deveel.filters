namespace Deveel.Filters;

/// <summary>
/// A builder that provides comparison and function operations
/// for a specific variable in a filter expression.
/// </summary>
public sealed class FieldFilterExpressionBuilder {
    private readonly FilterExpressionBuilder _parent;
    private readonly VariableFilterExpression _variable;
    private readonly FilterExpressionType? _combineWith;

    internal FieldFilterExpressionBuilder(FilterExpressionBuilder parent, VariableFilterExpression variable, FilterExpressionType? combineWith = null) {
        _parent = parent;
        _variable = variable;
        _combineWith = combineWith;
    }

    private FilterExpressionBuilder Compare(FilterExpressionType type, object? value) {
        var expression = FilterExpression.Binary(_variable, FilterExpression.Constant(value), type);
        _parent.SetExpression(expression, _combineWith);
        return _parent;
    }

    /// <summary>
    /// Creates an equality comparison with the specified value.
    /// </summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>Returns the parent builder for further chaining.</returns>
    public FilterExpressionBuilder IsEqualTo(object? value)
        => Compare(FilterExpressionType.Equal, value);

    /// <summary>
    /// Creates an inequality comparison with the specified value.
    /// </summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>Returns the parent builder for further chaining.</returns>
    public FilterExpressionBuilder IsNotEqualTo(object? value)
        => Compare(FilterExpressionType.NotEqual, value);

    /// <summary>
    /// Creates a greater-than comparison with the specified value.
    /// </summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>Returns the parent builder for further chaining.</returns>
    public FilterExpressionBuilder IsGreaterThan(object? value)
        => Compare(FilterExpressionType.GreaterThan, value);

    /// <summary>
    /// Creates a greater-than-or-equal comparison with the specified value.
    /// </summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>Returns the parent builder for further chaining.</returns>
    public FilterExpressionBuilder IsGreaterThanOrEqualTo(object? value)
        => Compare(FilterExpressionType.GreaterThanOrEqual, value);

    /// <summary>
    /// Creates a less-than comparison with the specified value.
    /// </summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>Returns the parent builder for further chaining.</returns>
    public FilterExpressionBuilder IsLessThan(object? value)
        => Compare(FilterExpressionType.LessThan, value);

    /// <summary>
    /// Creates a less-than-or-equal comparison with the specified value.
    /// </summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>Returns the parent builder for further chaining.</returns>
    public FilterExpressionBuilder IsLessThanOrEqualTo(object? value)
        => Compare(FilterExpressionType.LessThanOrEqual, value);

    /// <summary>
    /// Creates a null equality check for the variable.
    /// </summary>
    /// <returns>Returns the parent builder for further chaining.</returns>
    public FilterExpressionBuilder IsNull()
        => Compare(FilterExpressionType.Equal, null);

    /// <summary>
    /// Creates a null inequality check for the variable.
    /// </summary>
    /// <returns>Returns the parent builder for further chaining.</returns>
    public FilterExpressionBuilder IsNotNull()
        => Compare(FilterExpressionType.NotEqual, null);

    /// <summary>
    /// Creates a function call expression on the variable.
    /// </summary>
    /// <param name="functionName">The name of the function to invoke.</param>
    /// <param name="arguments">The arguments to pass to the function.</param>
    /// <returns>Returns the parent builder for further chaining.</returns>
    public FilterExpressionBuilder HasFunction(string functionName, params object?[] arguments) {
        var args = arguments.Select(a => a is FilterExpression expr
            ? expr
            : (FilterExpression)FilterExpression.Constant(a)).ToArray();

        var expression = FilterExpression.Function(_variable, functionName, args);
        _parent.SetExpression(expression, _combineWith);
        return _parent;
    }
}