#nullable disable
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Guru.Collection.Orm;

namespace Guru.Collection.RealmDB
{
    public class PropertyFilterRequest
    {
        public readonly IList<PropertyFilter> PropertyFilters = new List<PropertyFilter>();
    }

    // This class is used to create a filter request for RealmDB.
    // this class is stateless class and can be used as a static class.
    public static class RealmFilter
    {
        private const int DEFAULT_PAGE_SIZE = 50;

        public static PropertyFilterRequest NewObjectIdFilter(string id, bool useIdObfuscating)
        {
            PropertyFilterRequest filterRequest = new PropertyFilterRequest();
            filterRequest.PropertyFilters.Add(new PropertyFilter
            {
                property = "ID",
                condition = new Condition
                {
                    operatorType = OperatorType.EQ,
                    value = ProcessID(id, useIdObfuscating)
                }
            });
            return filterRequest;
        }

        private static string ProcessID(string id, bool useIdObfuscating)
        {
            if (useIdObfuscating)
            {
                //TODO: Implement ID obfuscation decode here.
            }

            return id;
        }

        public static PropertyFilterRequest NewFilterRequest<T>(T request)
        {
            PropertyFilterRequest filterRequest = new PropertyFilterRequest();
            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                if (property.GetValue(request) is Condition value)
                {
                    filterRequest.PropertyFilters.Add(new PropertyFilter
                    {
                        property = property.Name,
                        condition = value,
                    });
                }
            }

            return filterRequest;
        }

        public static IQueryable<T> ApplyFilterRequest<T>(this IQueryable<T> query, PropertyFilterRequest filterRequest, IList<PropertyFilter> extraFilters = null)
        {
            foreach (var filter in filterRequest.PropertyFilters)
            {
                query = ApplyFilter(query, filter);
                if (extraFilters != null)
                {
                    foreach (var extraFilter in extraFilters)
                    {
                        query = ApplyFilter(query, extraFilter);
                    }
                }
            }

            return query;
        }

        public static IQueryable<T> ApplyProjection<T>(this IQueryable<T> query, string[] projection)
        {
            if (projection == null || projection.Length == 0)
            {
                return query;
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            var bindings = projection.Select(field =>
            {
                var property = Expression.Property(parameter, field);
                return Expression.Bind(typeof(T).GetProperty(field)!, property);
            });

            var body = Expression.MemberInit(Expression.New(typeof(T)), bindings);
            var lambda = Expression.Lambda<Func<T, T>>(body, parameter);

            return query.Select(lambda);
        }

        public static (List<T> Results, PageInfo PageInfo) ApplyPageRequest<T>(this IQueryable<T> query, PageRequest pageRequest)
        {
            // 计算总记录数
            var totalCount = query.Count();

            // 获取所有结果
            var allResults = query.ToList();

            // 在内存中排序
            IOrderedEnumerable<T> orderedList = null;
            if (pageRequest.sorts != null && pageRequest.sorts.Any())
            {
                foreach (var sort in pageRequest.sorts)
                {
                    var property = typeof(T).GetProperty(sort.property);
                    if (property == null)
                    {
                        throw new ArgumentException($"Property {sort.property} not found on type {typeof(T).Name}");
                    }

                    if (orderedList == null)
                    {
                        orderedList = sort.direction == SortDirection.ASC
                            ? allResults.OrderBy(x => property.GetValue(x))
                            : allResults.OrderByDescending(x => property.GetValue(x));
                    }
                    else
                    {
                        orderedList = sort.direction == SortDirection.ASC
                            ? orderedList.ThenBy(x => property.GetValue(x))
                            : orderedList.ThenByDescending(x => property.GetValue(x));
                    }
                }
            }

            // 在内存中分页
            var pagedResults = (orderedList?.ToList() ?? allResults).Skip((int) (pageRequest.number - 1 * pageRequest.size)).Take((int) pageRequest.size).ToList();

            // 填写分页信息
            var pageInfo = new PageInfo
            {
                number = pageRequest.number,
                size = pageRequest.size,
                offset = (pageRequest.number - 1) * pageRequest.size,
                numberOfElements = pagedResults.Count,
                totalElements = totalCount,
                totalPages = (long) Math.Ceiling((double) totalCount / pageRequest.size),
                first = pageRequest.number == 1,
                last = pageRequest.number == (long) Math.Ceiling((double) totalCount / pageRequest.size),
                sorts = pageRequest.sorts
            };

            return (pagedResults, pageInfo);
        }

        public static PageRequest FillDefaultPageRequest(this PageRequest pageRequest)
        {
            pageRequest.number = 1;
            pageRequest.size = DEFAULT_PAGE_SIZE;
            return pageRequest;
        }

        private static IQueryable<T> ApplyFilter<T>(IQueryable<T> query, PropertyFilter filter)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = BuildPropertyExpression(parameter, filter.property);

            Expression condition;
            if (typeof(IEnumerable).IsAssignableFrom(property.Type) && property.Type != typeof(string))
            {
                throw new NotSupportedException($"Unsupported property type: {property.Type.Name}");
            }
            else
            {
                condition = filter.condition.multi != MultiType.NO_MULTI
                    ? BuildMultiConditionExpression(property, filter.condition)
                    : BuildOperatorConditionExpression(property, filter.condition);
            }

            var lambda = Expression.Lambda<Func<T, bool>>(condition, parameter);
            return query.Where(lambda);
        }

        private static Expression BuildPropertyExpression(ParameterExpression parameter, string propertyPath)
        {
            var parts = propertyPath.Split('.');
            Expression property = parameter;
            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                if (int.TryParse(part, out int index))
                {
                    // 处理索引
                    if (property.Type.IsGenericType && typeof(IList<>).IsAssignableFrom(property.Type.GetGenericTypeDefinition()))
                    {
                        // property = Expression.Property(property, "Item", Expression.Constant(index));

                        // 使用 ElementAt 来处理列表索引
                        // var elementType = property.Type.GetGenericArguments()[0];
                        // property = Expression.Call(
                        //     typeof(Enumerable),
                        //     "ElementAt",
                        //     new Type[] { elementType },
                        //     property,
                        //     Expression.Constant(index)
                        // );

                        throw new NotSupportedException("Realm does not support list indexing.");
                    }
                    else if (property.Type.IsArray)
                    {
                        property = Expression.ArrayIndex(property, Expression.Constant(index));
                    }
                }
                else if (property.Type.GetProperty("Item", new[] {typeof(string)}) != null)
                {
                    // 处理字典
                    // property = Expression.Property(property, "Item", Expression.Constant(part));

                    throw new NotSupportedException("Realm does not support dictionary indexing.");
                }
                else
                {
                    property = Expression.Property(property, part);
                }
            }

            return property;
        }

        private static Expression BuildOperatorConditionExpression(Expression property, Condition condition)
        {
            switch (condition.operatorType)
            {
                case OperatorType.EQ:
                    return Expression.Equal(property, Expression.Constant(condition.value));
                case OperatorType.NE:
                    return Expression.NotEqual(property, Expression.Constant(condition.value));
                case OperatorType.GT:
                    return Expression.GreaterThan(property, Expression.Constant(condition.value));
                case OperatorType.GTE:
                    return Expression.GreaterThanOrEqual(property, Expression.Constant(condition.value));
                case OperatorType.LT:
                    return Expression.LessThan(property, Expression.Constant(condition.value));
                case OperatorType.LTE:
                    return Expression.LessThanOrEqual(property, Expression.Constant(condition.value));
                case OperatorType.LIKE:
                    return BuildLikeExpression(property, condition);
                case OperatorType.NOT_LIKE:
                    return Expression.Not(BuildLikeExpression(property, condition));
                case OperatorType.NULL:
                    return Expression.Equal(property, Expression.Constant(null));
                case OperatorType.NOT_NULL:
                    return Expression.NotEqual(property, Expression.Constant(null));
                default:
                    throw new NotSupportedException($"Unsupported operator: {condition.operatorType}");
            }
        }

        private static Expression BuildMultiConditionExpression(Expression property, Condition condition)
        {
            // TODO 要求输入的值必需是一个数组，是否合理？
            if (condition.value is not Array arrayValues)
            {
                throw new InvalidFilterConditionException($"Value for multi condition operation must be Array, but was {condition.value?.GetType().Name ?? "null"}");
            }

            switch (condition.multi)
            {
                case MultiType.NO_MULTI:
                    return Expression.Equal(property, Expression.Constant(condition.value));
                case MultiType.BETWEEN:
                    return Expression.AndAlso(
                        Expression.GreaterThanOrEqual(property, Expression.Constant(arrayValues.GetValue(0))),
                        Expression.LessThanOrEqual(property, Expression.Constant(arrayValues.GetValue(1)))
                    );
                case MultiType.NOT_BETWEEN:
                    return Expression.Or(
                        Expression.LessThan(property, Expression.Constant(arrayValues.GetValue(0))),
                        Expression.GreaterThan(property, Expression.Constant(arrayValues.GetValue(1)))
                    );
                case MultiType.IN:
                case MultiType.NOT_IN:
                    var elementType = property.Type;
                    if (arrayValues.Length == 0)
                    {
                        // 如果值列表为空，IN 操作总是返回 false，NOT IN 操作总是返回 true
                        return condition.multi == MultiType.IN ? Expression.Constant(false) : Expression.Constant(true);
                    }

                    if (arrayValues.Length == 1)
                    {
                        // 如果只有一个值，转换为简单的相等或不相等比较
                        var equalityComparison = Expression.Equal(property, Expression.Constant(arrayValues.GetValue(0), elementType));
                        return condition.multi == MultiType.IN ? equalityComparison : Expression.Not(equalityComparison);
                    }

                    // TODO 这里涉及到装箱操作，性能上有待优化
                    var comparisons = arrayValues.Cast<object>().Select(value =>
                        Expression.Equal(property, Expression.Constant(value, elementType)));
                    var combinedComparison = comparisons.Aggregate(Expression.OrElse);
                    return condition.multi == MultiType.IN ? combinedComparison : Expression.Not(combinedComparison);
                default:
                    throw new NotSupportedException($"Unsupported multi type: {condition.multi}");
            }
        }

        private static Expression BuildLikeExpression(Expression property, Condition condition)
        {
            var pattern = condition.value.ToString();
            MethodInfo stringMethod;

            switch (condition.wildcard)
            {
                case WildcardType.CONTAINS:
                    stringMethod = typeof(string).GetMethod("Contains", new[] {typeof(string)});
                    return Expression.Call(property, stringMethod, Expression.Constant(pattern));

                case WildcardType.STARTS_WITH:
                    stringMethod = typeof(string).GetMethod("StartsWith", new[] {typeof(string)});
                    return Expression.Call(property, stringMethod, Expression.Constant(pattern));

                case WildcardType.ENDS_WITH:
                    stringMethod = typeof(string).GetMethod("EndsWith", new[] {typeof(string)});
                    return Expression.Call(property, stringMethod, Expression.Constant(pattern));

                case WildcardType.NO_WILDCARD:
                default:
                    // 如果没有指定通配符类型,默认使用Contains
                    stringMethod = typeof(string).GetMethod("Contains", new[] {typeof(string)});
                    return Expression.Call(property, stringMethod, Expression.Constant(pattern));
            }
        }

        // public static string GetFilter (PropertyFilterRequest filterRequest, IList<PropertyFilter> extraFilters = null)
        // {
        //     List<string> queryParts = new List<string>();
        //     foreach (var filter in filterRequest.PropertyFilters)
        //     {
        //         string queryPart = BuildFilterQuery(filter);
        //         if (!string.IsNullOrEmpty(queryPart))
        //         {
        //             queryParts.Add(queryPart);
        //         }
        //     }
        //
        //     return string.Join(" AND ", queryParts);
        // }
        //
        // private static string BuildFilterQuery(PropertyFilter filter)
        // {
        //     string property = NormalizePropertyPath(filter.property);
        //     var condition = filter.condition;
        //
        //     switch (condition.operatorType)
        //     {
        //         case OperatorType.EQ:
        //             return BuildEqualityQuery(property, condition.value);
        //         case OperatorType.NE:
        //             return BuildInequalityQuery(property, condition.value);
        //         case OperatorType.GT:
        //             return $"{property} > {FormatValue(condition.value)}";
        //         case OperatorType.GTE:
        //             return $"{property} >= {FormatValue(condition.value)}";
        //         case OperatorType.LT:
        //             return $"{property} < {FormatValue(condition.value)}";
        //         case OperatorType.LTE:
        //             return $"{property} <= {FormatValue(condition.value)}";
        //         case OperatorType.LIKE:
        //             return BuildLikeQuery(property, condition);
        //         case OperatorType.NOT_LIKE:
        //             return $"NOT {BuildLikeQuery(property, condition)}";
        //         case OperatorType.NULL:
        //             return $"{property} == NULL";
        //         case OperatorType.NOT_NULL:
        //             return $"{property} != NULL";
        //         default:
        //             return BuildMultiValueQuery(property, condition);
        //     }
        // }
        //
        // private static string BuildEqualityQuery(string property, object value)
        // {
        //     if (value is IList list && list.Count > 0)
        //     {
        //         // 对于多值，使用 OR 连接
        //         return string.Join(" OR ", list.Cast<object>().Select(v => $"{property} == {FormatValue(v)}"));
        //     }
        //     return $"{property} == {FormatValue(value)}";
        // }
        //
        // private static string BuildInequalityQuery(string property, object value)
        // {
        //     if (value is IList list && list.Count > 0)
        //     {
        //         // 对于多值，使用 AND 连接
        //         return string.Join(" AND ", list.Cast<object>().Select(v => $"{property} != {FormatValue(v)}"));
        //     }
        //     return $"{property} != {FormatValue(value)}";
        // }
        //
        // private static string BuildLikeQuery(string property, Condition condition)
        // {
        //     string value = condition.value.ToString();
        //     switch (condition.wildcard)
        //     {
        //         case WildcardType.CONTAINS:
        //             return $"{property} LIKE '*{value}*'";
        //         case WildcardType.STARTS_WITH:
        //             return $"{property} LIKE '{value}*'";
        //         case WildcardType.ENDS_WITH:
        //             return $"{property} LIKE '*{value}'";
        //         default:
        //             return $"{property} LIKE '{value}'";
        //     }
        // }
        //
        // private static string BuildMultiValueQuery(string property, Condition condition)
        // {
        //     switch (condition.multi)
        //     {
        //         case MultiType.BETWEEN:
        //             var values = (IList)condition.value;
        //             return $"{property} >= {FormatValue(values[0])} AND {property} <= {FormatValue(values[1])}";
        //         case MultiType.NOT_BETWEEN:
        //             values = (IList)condition.value;
        //             return $"{property} < {FormatValue(values[0])} OR {property} > {FormatValue(values[1])}";
        //         case MultiType.IN:
        //             return string.Join(" OR ", ((IList)condition.value).Cast<object>().Select(v => $"{property} == {FormatValue(v)}"));
        //         case MultiType.NOT_IN:
        //             return string.Join(" AND ", ((IList)condition.value).Cast<object>().Select(v => $"{property} != {FormatValue(v)}"));
        //         default:
        //             return string.Empty;
        //     }
        // }
        //
        // private static string NormalizePropertyPath(string propertyPath)
        // {
        //     // 处理嵌套属性
        //     propertyPath = propertyPath.Replace(".", ".");
        //
        //     // 处理列表索引
        //     propertyPath = Regex.Replace(propertyPath, @"\.(\d+)", "[$1]");
        //
        //     // 处理字典键
        //     propertyPath = Regex.Replace(propertyPath, @"\.(\w+)", "['$1']");
        //
        //     return propertyPath;
        // }
        //
        // private static string FormatValue(object value)
        // {
        //     if (value == null)
        //         return "NULL";
        //
        //     if (value is string strValue)
        //         return $"'{strValue.Replace("'", "''")}'";
        //
        //     if (value is DateTime dateTime)
        //         return FormatDateTime(dateTime);
        //
        //     if (value is DateTimeOffset dateTimeOffset)
        //         return FormatDateTime(dateTimeOffset.UtcDateTime);
        //
        //     if (value is bool boolValue)
        //         return boolValue.ToString().ToLower();
        //
        //     if (value is IList list)
        //         return string.Join(", ", list.Cast<object>().Select(FormatValue));
        //
        //     return value.ToString();
        // }
        //
        // private static string FormatDateTime(DateTime dateTime)
        // {
        //     dateTime = dateTime.ToUniversalTime();
        //     return $"DATETIME({dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)})";
        // }
    }
}