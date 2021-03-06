using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Hexio.DineroClient.Models;
using Hexio.DineroClient.Models.Contacts;
using Hexio.DineroClient.Models.Products;

namespace Hexio.DineroClient
{
    public enum QueryOperator
    {
        Eq,
        Contains
    }
    
    public class QueryCreator<T> where T : IReadModel, new()
    {
        private IList<QueryFilterValue> Filters { get; set; } = new List<QueryFilterValue>();
        private IList<string> Fields { get; set; } = new List<string>();

        private int PageSize { get; set; } = 100;

        private int Page { get; set; } = 0;
        private DateTimeOffset? ChangesSince { get; set; }

        public QueryCreator()
        {
            Fields = new T().GetDefaultFields();
        }

        public QueryCreator<T> Where(Expression<Func<T, object>> expression, QueryOperator queryOperator, string value)
        {
            var type = new T();
            
            MemberExpression body = expression.Body as MemberExpression;

            if (body == null) {
                UnaryExpression ubody = (UnaryExpression)expression.Body;
                body = ubody.Operand as MemberExpression;
            }

            var parameterName = body.Member.Name;

            if (!type.FieldsToFilter.Contains(parameterName))
            {
                throw new Exception($"Filtering for parameter {parameterName} is not allowed, see the Dinero Api documentation for further explanation");
            }
            
            var filterValue = new QueryFilterValue
            {
                Parameter = parameterName,
                CompareOperator = queryOperator.ToString().ToLower(),
                Value = value
            };
            
            Filters.Add(filterValue);
            

            return this;
        }

        public QueryCreator<T> Include(Expression<Func<T, object>> expression)
        {
            MemberExpression body = expression.Body as MemberExpression;

            if (body == null) {
                UnaryExpression ubody = (UnaryExpression)expression.Body;
                body = ubody.Operand as MemberExpression;
            }

            var parameterName = body.Member.Name;

            if (!Fields.Contains(parameterName))
            {
                Fields.Add(parameterName);
            }
             
            return this;
        }
        
        public QueryCreator<T> Size(int pageSize)
        {
            if (pageSize > 1000 || pageSize < 0)
            {
                throw new Exception("Page size must be between 0 and 1000");
            }
            
            PageSize = pageSize;

            return this;
        }

        public QueryCreator<T> PageNumber(int page)
        {
            Page = page;

            return this;
        }

        public QueryCreator<T> ChangeSince(DateTimeOffset changesSince)
        {
            if ((new T().HasChangesSince()))
            {
                ChangesSince = changesSince;
            }
            else
            {
                throw new Exception($"Class {typeof(T)} does not allow ChangesSince");
            }

            return this;
        }

        public override string ToString()
        {
            var filter = string.Join(";", Filters.Select(x => x.ToString()));

            var fields = string.Join(",", Fields);

            var queryItems = new List<string>();

            if (Fields.Count > 0)
            {
                queryItems.Add($"fields={fields}");
            }

            if (Filters.Count > 0)
            {
                queryItems.Add($"queryFilter={filter}");
            }

            if (ChangesSince != null)
            {
                queryItems.Add($"changesSince={ChangesSince.Value.ToString("yyyy-MM-ddTHH:mm:ssZ")}");
            }

            queryItems.Add($"page={Page}");
            queryItems.Add($"pageSize={PageSize}");

            return string.Join("&", queryItems);
        }
    }

    public class QueryFilterValue
    {
        public string Parameter { get; set; }
        public string CompareOperator { get; set; }
        public string Value { get; set; }
        public override string ToString()
        {
            return $"{Parameter}+{CompareOperator}+'{Value}'";
        }
    }
}