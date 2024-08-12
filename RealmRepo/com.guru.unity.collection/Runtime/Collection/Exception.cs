#nullable disable
using Guru.Collection.Orm;

namespace Guru.Collection
{
    public class QueryException : Exception
    {
        public QueryException(string message, Exception innerException = null)
            : base(message, innerException) { }
    }

    public class PrimaryKeyQueryException : QueryException
    {
        public PrimaryKeyQueryException(string id, Exception innerException = null)
            : base($"\nFailed to query by primary key: {id}", innerException) { }
    }

    public class ConditionQueryException : QueryException
    {
        public ConditionQueryException(string details, Exception innerException = null)
            : base($"\nFailed to execute condition query: {details}", innerException) { }
    }

    public class PropertyPathException : QueryException
    {
        public PropertyPathException(string path, Exception innerException = null)
            : base($"\nInvalid property path: {path}", innerException) { }
    }

    public class InvalidFilterConditionException : QueryException
    {
        public InvalidFilterConditionException(OperatorType optionType, Exception innerException = null)
            : base($"\nInvalid filter condition: {optionType}", innerException) { }

        public InvalidFilterConditionException(MultiType multiType, Exception innerException = null)
            : base($"\nInvalid filter condition: {multiType}", innerException) { }

        public InvalidFilterConditionException(string details, Exception innerException = null): base(details, innerException) { }
    }

    public class SortingException : QueryException
    {
        public SortingException(string details, Exception innerException = null)
            : base($"\nFailed to apply sorting: {details}", innerException) { }
    }

    public class PagingException : QueryException
    {
        public PagingException(string details, Exception innerException = null)
            : base($"\nFailed to apply paging: {details}", innerException) { }
    }

}