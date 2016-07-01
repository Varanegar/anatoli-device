using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.Framework.AnatoliBase
{
    public abstract class Query
    {
        public int Limit = 10;
        public int Index;
    }
    public abstract class QueryParameter
    {

    }
    public class BasicParam : QueryParameter
    {
        public string Name;
        public string Value;
        public BasicParam(string name, string value)
        {
            Name = name;
            Value = value;
        }
        public BasicParam(string name, Guid value)
        {
            Name = name;
            Value = value.ToString().ToUpper();
        }
    }
    public abstract class FilterParam : QueryParameter
    {
        protected string _name;
        protected string _value;
        protected Operator _operator;
        public string Name { get { return _name; } }
        public string Value { get { return _value; } }
        public FilterParam(string Name, string Value)
        {
            _name = Name;
            _value = Value;
        }
    }
    public class SearchFilterParam : FilterParam
    {
        public SearchFilterParam(string Name, string Value)
            : base(Name, Value)
        {

        }
        public SearchFilterParam(string Name, Guid Value)
            : base(Name, Value.ToString().ToUpper())
        {

        }
    }
    public class EqFilterParam : SearchFilterParam
    {
        public EqFilterParam(string Name, string Value)
            : base(Name, Value)
        {

        }
        public EqFilterParam(string Name, Guid Value)
            : base(Name, Value)
        {

        }
    }
    public class GreaterFilterParam : SearchFilterParam
    {
        public GreaterFilterParam(string Name, string Value)
            : base(Name, Value)
        {

        }
        public GreaterFilterParam(string Name, Guid Value)
            : base(Name, Value)
        {

        }
    }
    public class SmallerFilterParam : SearchFilterParam
    {
        public SmallerFilterParam(string Name, string Value)
            : base(Name, Value)
        {

        }
        public SmallerFilterParam(string Name, Guid Value)
            : base(Name, Value)
        {

        }
    }
    public class CategoryFilterParam : FilterParam
    {
        public CategoryFilterParam(string Name, string Value)
            : base(Name, Value)
        {

        }

    }
    public class SortParam : QueryParameter
    {
        string _fieldName;
        SortTypes _sortType;
        public string FieldName { get { return _fieldName; } }
        public SortTypes SortType { get { return _sortType; } }
        public SortParam(string fieldName, SortTypes sortType)
        {
            _fieldName = fieldName;
            _sortType = sortType;
        }
    }
    public class RemoteQuery : Query
    {
        public System.Threading.CancellationTokenSource cancellationTokenSource;
        public TokenType TokenType { get; private set; }
        public HttpMethod Method;
        public bool ResultIsCompressed = false;
        RemoteQuery(HttpMethod method)
        {
            Method = method;
        }
        public RemoteQuery(TokenType token, string WebServiceEndpoint, HttpMethod method, params QueryParameter[] options)
            : this(method)
        {
            this.WebServiceEndpoint = WebServiceEndpoint;
            this.TokenType = token;
        }
        public RemoteQuery(TokenType token, string WebServiceEndpoint, HttpMethod method, List<QueryParameter> options)
            : this(method)
        {
            this.WebServiceEndpoint = WebServiceEndpoint;
            this.TokenType = token;
        }
        public string WebServiceEndpoint { get; set; }
        public List<Tuple<string, string>> Params
        {
            get
            {
                // todo: implement params
                List<Tuple<string, string>> parameters = new List<Tuple<string, string>>();
                return parameters;
            }
        }
    }

    public abstract class DBQuery : Query
    {
        public DBQuery(string DBTableName)
        {
            this.DBTableName = DBTableName;
        }
        public abstract string GetCommand();
        public string DBTableName { get; set; }
    }
    public class StringQuery : DBQuery
    {
        string _query;
        public bool Unlimited { get; set; }
        public StringQuery(string query)
            : base(null)
        {
            _query = query;
        }
        public override string GetCommand()
        {
            if (Unlimited)
            {
                return _query;
            }
            else
                return _query + " LIMIT " + Index + "," + Limit;
        }
    }
    public class SelectQuery : DBQuery
    {
        List<SearchFilterParam> SearchFilters;
        List<CategoryFilterParam> CategoryFilters;
        List<SortParam> Sorts;
        /// <summary>
        /// Not Recommended for big result sets
        /// </summary>
        public bool Unlimited { get; set; }
        public SelectQuery(string dataTableName, params QueryParameter[] options)
            : base(dataTableName)
        {
            SearchFilters = new List<SearchFilterParam>();
            CategoryFilters = new List<CategoryFilterParam>();
            Sorts = new List<SortParam>();
            Index = 0;
            Limit = 10;
            foreach (var item in options)
            {
                if (item.GetType() == typeof(SearchFilterParam))
                    SearchFilters.Add(item as SearchFilterParam);
                if (item.GetType() == typeof(EqFilterParam))
                    SearchFilters.Add(item as EqFilterParam);
                if (item.GetType() == typeof(GreaterFilterParam))
                    SearchFilters.Add(item as GreaterFilterParam);
                if (item.GetType() == typeof(SmallerFilterParam))
                    SearchFilters.Add(item as SmallerFilterParam);
                if (item.GetType() == typeof(CategoryFilterParam))
                    CategoryFilters.Add(item as CategoryFilterParam);
                else if (item.GetType() == typeof(SortParam))
                    Sorts.Add(item as SortParam);
            }
        }
        public SelectQuery(string dataTableName, List<QueryParameter> options)
            : base(dataTableName)
        {
            SearchFilters = new List<SearchFilterParam>();
            CategoryFilters = new List<CategoryFilterParam>();
            Sorts = new List<SortParam>();
            Index = 0;
            Limit = 10;
            foreach (var item in options)
            {
                if (item.GetType() == typeof(SearchFilterParam))
                    SearchFilters.Add(item as SearchFilterParam);
                if (item.GetType() == typeof(EqFilterParam))
                    SearchFilters.Add(item as EqFilterParam);
                if (item.GetType() == typeof(GreaterFilterParam))
                    SearchFilters.Add(item as GreaterFilterParam);
                if (item.GetType() == typeof(SmallerFilterParam))
                    SearchFilters.Add(item as SmallerFilterParam);
                if (item.GetType() == typeof(CategoryFilterParam))
                    CategoryFilters.Add(item as CategoryFilterParam);
                else if (item.GetType() == typeof(SortParam))
                    Sorts.Add(item as SortParam);
            }
        }

        public override string GetCommand()
        {

            string q = string.Format(" SELECT * FROM  {0} ", DBTableName);
            if (SearchFilters.Count == 1)
            {

                if (SearchFilters.First<FilterParam>().GetType() == typeof(EqFilterParam))
                    q += string.Format(" WHERE {0} = '{1}' ", SearchFilters.First<FilterParam>().Name, SearchFilters.First<FilterParam>().Value);
                else if (SearchFilters.First<FilterParam>().GetType() == typeof(GreaterFilterParam))
                    q += string.Format(" WHERE {0} >= '{1}' ", SearchFilters.First<FilterParam>().Name, SearchFilters.First<FilterParam>().Value);
                else if (SearchFilters.First<FilterParam>().GetType() == typeof(SmallerFilterParam))
                    q += string.Format(" WHERE {0}<='{1}' ", SearchFilters.First<FilterParam>().Name, SearchFilters.First<FilterParam>().Value);
                else
                    q += string.Format(" WHERE {0} LIKE '%{1}%' ", SearchFilters.First<FilterParam>().Name, SearchFilters.First<FilterParam>().Value);
            }
            else if (SearchFilters.Count >= 1)
            {
                q += " WHERE (";
                for (int i = 0; i < SearchFilters.Count - 1; i++)
                {
                    var filter = SearchFilters[i];
                    if (filter.GetType() == typeof(EqFilterParam))
                        q += string.Format(" {0} = '{1}' and ", filter.Name, filter.Value);
                    else if (filter.GetType() == typeof(GreaterFilterParam))
                        q += string.Format(" {0}>='{1}' and ", filter.Name, filter.Value);
                    else if (filter.GetType() == typeof(SmallerFilterParam))
                        q += string.Format(" {0}<='{1}' and ", filter.Name, filter.Value);
                    else
                        q += string.Format(" {0} LIKE '%{1}%' OR ", filter.Name, filter.Value);
                }
                if (SearchFilters.Last<FilterParam>().GetType() == typeof(EqFilterParam))
                    q += string.Format(" {0} = '{1}' )", SearchFilters.Last<FilterParam>().Name, SearchFilters.Last<FilterParam>().Value);
                else if (SearchFilters.Last<FilterParam>().GetType() == typeof(GreaterFilterParam))
                    q += string.Format(" {0}>='{1}' )", SearchFilters.Last<FilterParam>().Name, SearchFilters.Last<FilterParam>().Value);
                else if (SearchFilters.Last<FilterParam>().GetType() == typeof(SmallerFilterParam))
                    q += string.Format(" {0}<='{1}' )", SearchFilters.Last<FilterParam>().Name, SearchFilters.Last<FilterParam>().Value);
                else
                    q += string.Format(" {0} LIKE '%{1}%' )", SearchFilters.Last<FilterParam>().Name, SearchFilters.Last<FilterParam>().Value);
            }

            if (CategoryFilters.Count == 1)
            {
                if (SearchFilters.Count > 0)
                {
                    q += string.Format(" AND {0}='{1}' ", CategoryFilters.First<FilterParam>().Name, CategoryFilters.First<FilterParam>().Value);
                }
                else
                    q += string.Format(" WHERE {0}='{1}' ", CategoryFilters.First<FilterParam>().Name, CategoryFilters.First<FilterParam>().Value);
            }
            else if (CategoryFilters.Count > 1)
            {
                if (SearchFilters.Count == 0)
                {
                    q += " WHERE (";
                }
                else
                    q += " AND ( ";
                for (int i = 0; i < CategoryFilters.Count - 1; i++)
                {
                    var filter = CategoryFilters[i];
                    q += string.Format(" {0}='{1}' or ", filter.Name, filter.Value);
                    // todo : and should be added
                }
                q += string.Format(" {0}='{1}' )", CategoryFilters.Last<FilterParam>().Name, CategoryFilters.Last<FilterParam>().Value);
            }

            if (Sorts.Count == 1)
            {
                if (Sorts.First<SortParam>().SortType == SortTypes.ASC)
                    q += string.Format(" ORDER BY {0} ASC ", Sorts.First<SortParam>().FieldName);
                else
                    q += string.Format(" ORDER BY {0} DESC ", Sorts.First<SortParam>().FieldName);
            }
            else if (Sorts.Count > 1)
            {
                q += " ORDER BY ASC ";
                for (int i = 0; i < Sorts.Count - 1; i++)
                {
                    var sort = Sorts[i];
                    if (sort.SortType == SortTypes.ASC)
                        q += string.Format(" {0} ASC ,", sort.FieldName);
                    else
                        q += string.Format(" {0} DESC ,", sort.FieldName);
                }
                if (Sorts.Last<SortParam>().SortType == SortTypes.ASC)
                    q += string.Format(" {0} ASC", Sorts.Last<SortParam>().FieldName);
                else
                    q += string.Format(" {0} DESC", Sorts.Last<SortParam>().FieldName);
            }
            // todo : add filters and order by on several fields
            if (!Unlimited)
                q += string.Format(" LIMIT {0},{1}", Index.ToString(), Limit.ToString());
            return q;
        }


    }
    public class DeleteCommand : DBQuery
    {
        public DeleteCommand(string DBTableName, params SearchFilterParam[] options)
            : base(DBTableName)
        {
            _parameters = new List<SearchFilterParam>();
            foreach (var item in options)
            {
                _parameters.Add(item);
            }
        }
        List<SearchFilterParam> _parameters;
        public override string GetCommand()
        {
            string q = string.Format(" DELETE FROM {0} ", DBTableName);
            if (_parameters.Count == 0)
                return q;
            q += " WHERE ";
            if (_parameters.Count == 1)
            {
                if (_parameters.First<SearchFilterParam>().GetType() == typeof(EqFilterParam))
                    q += String.Format("{0}='{1}'", _parameters.First<SearchFilterParam>().Name, _parameters.First<SearchFilterParam>().Value);
                else if (_parameters.First<SearchFilterParam>().GetType() == typeof(GreaterFilterParam))
                    q += String.Format("{0}>='{1}'", _parameters.First<SearchFilterParam>().Name, _parameters.First<SearchFilterParam>().Value);
                else if (_parameters.First<SearchFilterParam>().GetType() == typeof(SmallerFilterParam))
                    q += String.Format("{0}<='{1}'", _parameters.First<SearchFilterParam>().Name, _parameters.First<SearchFilterParam>().Value);
                else
                    q += String.Format("{0} LIKE '%{1}%'", _parameters.First<SearchFilterParam>().Name, _parameters.First<SearchFilterParam>().Value);
                return q;
            }
            if (_parameters.Count > 1)
            {
                if (_parameters.First<SearchFilterParam>().GetType() == typeof(EqFilterParam))
                    q += String.Format("{0}='{1}'", _parameters.First<SearchFilterParam>().Name, _parameters.First<SearchFilterParam>().Value);
                else if (_parameters.First<SearchFilterParam>().GetType() == typeof(GreaterFilterParam))
                    q += String.Format("{0}>='{1}'", _parameters.First<SearchFilterParam>().Name, _parameters.First<SearchFilterParam>().Value);
                else if (_parameters.First<SearchFilterParam>().GetType() == typeof(SmallerFilterParam))
                    q += String.Format("{0}<='{1}'", _parameters.First<SearchFilterParam>().Name, _parameters.First<SearchFilterParam>().Value);
                else
                    q += String.Format("{0} LIKE '%{1}%' ", _parameters.First<SearchFilterParam>().Name, _parameters.First<SearchFilterParam>().Value);
                for (int i = 0; i < _parameters.Count - 1; i++)
                {
                    if (_parameters[i].GetType() == typeof(EqFilterParam))
                        q += String.Format(" AND {0}='{1}'", _parameters[i].Name, _parameters[i].Value);
                    else if (_parameters[i].GetType() == typeof(GreaterFilterParam))
                        q += String.Format(" AND {0}>='{1}'", _parameters[i].Name, _parameters[i].Value);
                    else if (_parameters[i].GetType() == typeof(SmallerFilterParam))
                        q += String.Format(" AND {0}<='{1}'", _parameters[i].Name, _parameters[i].Value);
                    else
                        q += String.Format(" AND {0} LIKE '%{1}%' ", _parameters[i].Name, _parameters[i].Value);
                }
            }
            return q;
        }
    }
    public class InsertCommand : DBQuery
    {
        List<BasicParam> _parameters;
        public InsertCommand(string DBTableName, List<BasicParam> options)
            : base(DBTableName)
        {
            _parameters = options;
        }
        public InsertCommand(string DBTableName, params BasicParam[] options)
            : base(DBTableName)
        {
            _parameters = new List<BasicParam>();
            foreach (var item in options)
            {
                _parameters.Add(item);
            }
        }
        public override string GetCommand()
        {

            string q = string.Format(" INSERT INTO {0} (", DBTableName);
            string v = " VALUES (";
            if (_parameters.Count == 1)
            {
                q += String.Format("{0}) VALUES ({1})", _parameters.First<BasicParam>().Name, _parameters.First<BasicParam>().Value);
                return q;
            }
            q += String.Format("{0}", _parameters.First<BasicParam>().Name);
            v += String.Format("'{0}'", _parameters.First<BasicParam>().Value);
            for (int i = 1; i < _parameters.Count - 1; i++)
            {
                q += String.Format(",{0}", _parameters[i].Name);
                v += String.Format(",'{0}'", _parameters[i].Value);
            }
            q += String.Format(",{0})", _parameters.Last<BasicParam>().Name);
            v += String.Format(",'{0}')", _parameters.Last<BasicParam>().Value);
            return q + v;

        }
    }
    public class InsertAllCommand : DBQuery
    {
        List<List<BasicParam>> _rows;
        public InsertAllCommand(string DBTableName, List<List<BasicParam>> rows)
            : base(DBTableName)
        {
            _rows = rows;
        }
        public override string GetCommand()
        {

            string q = string.Format(" INSERT INTO {0} (", DBTableName);
            string v = " VALUES ";
            var parameters = _rows.First<List<BasicParam>>();
            if (parameters.Count == 1)
            {
                q += String.Format("{0}) ", parameters.First<BasicParam>().Name);
            }
            else
            {
                q += String.Format("{0}", parameters.First<BasicParam>().Name);
                for (int i = 1; i < parameters.Count - 1; i++)
                {
                    q += String.Format(",{0}", parameters[i].Name);
                }
                q += String.Format(",{0})", parameters.Last<BasicParam>().Name);
            }

            for (int i = 0; i < _rows.Count - 1; i++)
            {
                parameters = _rows[i];
                v += String.Format(" ('{0}'", parameters.First<BasicParam>().Value);
                for (int j = 1; j < parameters.Count - 1; j++)
                {
                    v += String.Format(",'{0}'", parameters[j].Value);
                }

                v += String.Format(",'{0}'),", parameters.Last<BasicParam>().Value);
            }

            parameters = _rows.Last<List<BasicParam>>();
            v += String.Format(" ('{0}'", parameters.First<BasicParam>().Value);
            for (int j = 1; j < parameters.Count - 1; j++)
            {
                v += String.Format(",'{0}'", parameters[j].Value);
            }

            v += String.Format(",'{0}')", parameters.Last<BasicParam>().Value);
            return q + v;

        }
    }
    public class UpdateCommand : DBQuery
    {
        public UpdateCommand(string DBTableName, params QueryParameter[] parameters)
            : base(DBTableName)
        {
            _searchParameters = new List<SearchFilterParam>();
            _valueParameters = new List<BasicParam>();
            foreach (var item in parameters)
            {
                if (item.GetType() == typeof(SearchFilterParam))
                    _searchParameters.Add(item as SearchFilterParam);
                if (item.GetType() == typeof(EqFilterParam))
                    _searchParameters.Add(item as EqFilterParam);
                if (item.GetType() == typeof(BasicParam))
                    _valueParameters.Add(item as BasicParam);
            }
        }
        public UpdateCommand(string DBTableName, List<SearchFilterParam> searchParameters, List<BasicParam> valueParameters)
            : base(DBTableName)
        {
            _searchParameters = searchParameters;
            _valueParameters = valueParameters;
        }
        List<SearchFilterParam> _searchParameters;
        List<BasicParam> _valueParameters;
        public override string GetCommand()
        {

            string q = string.Format(" UPDATE {0} SET ", DBTableName);
            if (_valueParameters.Count == 1)
            {
                q += String.Format("{0}='{1}'", _valueParameters.First<BasicParam>().Name, _valueParameters.First<BasicParam>().Value);
            }
            else if (_valueParameters.Count > 1)
            {
                q += String.Format("{0}='{1}'", _valueParameters.First<BasicParam>().Name, _valueParameters.First<BasicParam>().Value);
                for (int i = 1; i < _valueParameters.Count - 1; i++)
                {
                    q += String.Format(",{0}='{1}'", _valueParameters[i].Name, _valueParameters[i].Value);
                }
            }
            if (_searchParameters.Count == 1)
            {
                if (_searchParameters.First<SearchFilterParam>().GetType() == typeof(EqFilterParam))
                    q += String.Format(" WHERE {0}='{1}'", _searchParameters.First<SearchFilterParam>().Name, _searchParameters.First<SearchFilterParam>().Value);
                else
                    q += String.Format(" WHERE {0} LIKE '%{1}%'", _searchParameters.First<SearchFilterParam>().Name, _searchParameters.First<SearchFilterParam>().Value);
                return q;
            }
            else if (_searchParameters.Count > 1)
            {
                if (_searchParameters.First<SearchFilterParam>().GetType() == typeof(EqFilterParam))
                    q += String.Format(" WHERE {0}='{1}'", _searchParameters.First<SearchFilterParam>().Name, _searchParameters.First<SearchFilterParam>().Value);
                else
                    q += String.Format(" WHERE {0} LIKE '%{1}%' ", _searchParameters.First<SearchFilterParam>().Name, _searchParameters.First<SearchFilterParam>().Value);
                for (int i = 1; i < _searchParameters.Count - 1; i++)
                {
                    if (_searchParameters[i].GetType() == typeof(EqFilterParam))
                        q += String.Format(" and {0}='{1}'", _searchParameters[i].Name, _searchParameters[i].Value);
                    else
                        q += String.Format(" and {0} LIKE '%{1}%'", _searchParameters[i].Name, _searchParameters[i].Value);
                }
            }
            return q;
        }
    }
    public enum SortTypes
    {
        ASC = 1,
        DESC = 2
    }

    public enum Operator
    {
        And = 1,
        Or = 2,
        Equals = 3,
        Greater = 4,
        Smaller = 5,
        Like = 6
    }
}
