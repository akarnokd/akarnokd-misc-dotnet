using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Reactive.Streams;
using Reactor.Core;
using System.Threading;
using Reactor.Core.flow;
using Reactor.Core.subscriber;
using Reactor.Core.subscription;
using Reactor.Core.util;
using System.Collections;
using System.Linq.Expressions;


namespace akarnokd_misc_dotnet.linq
{
    public class QueryableFlux<T> : IOrderedQueryable<T>
    {
        public QueryableFlux()
        {
            Provider = new QueryableFluxProvider();
            Expression = Expression.Constant(this);
        }

        public QueryableFlux(QueryableFluxProvider provider, Expression expression)
        {
            Provider = provider;
            Expression = expression;
        }

        public Type ElementType
        {
            get
            {
                return typeof(T);
            }
        }

        public Expression Expression { get; private set; }

        public IQueryProvider Provider { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            return Provider.Execute<IQueryable<T>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Provider.Execute<IEnumerable>(Expression).GetEnumerator();   
        }
    }

    internal static class TypeSystem
    {

        internal static Type GetElementType(Type seqType)
        {

            Type ienum = FindIEnumerable(seqType);

            if (ienum == null) return seqType;

            return ienum.GetGenericArguments()[0];

        }

        private static Type FindIEnumerable(Type seqType)
        {

            if (seqType == null || seqType == typeof(string))

                return null;

            if (seqType.IsArray)

                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());

            if (seqType.IsGenericType)
            {

                foreach (Type arg in seqType.GetGenericArguments())
                {

                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);

                    if (ienum.IsAssignableFrom(seqType))
                    {

                        return ienum;

                    }

                }

            }

            Type[] ifaces = seqType.GetInterfaces();

            if (ifaces != null && ifaces.Length > 0)
            {

                foreach (Type iface in ifaces)
                {

                    Type ienum = FindIEnumerable(iface);

                    if (ienum != null) return ienum;

                }

            }

            if (seqType.BaseType != null && seqType.BaseType != typeof(object))
            {

                return FindIEnumerable(seqType.BaseType);

            }

            return null;

        }

    }

    public class QueryableFluxProvider : IQueryProvider
    {
        public IQueryable CreateQuery(Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(QueryableFlux<>).MakeGenericType(elementType), new object[] { this, expression });
            }
            catch (System.Reflection.TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new QueryableFlux<TElement>(this, expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)Execute(expression);
        }

        public object Execute(Expression expression)
        {
            // TODO this
            throw new NotImplementedException();
        }

    }

    internal class QueryTranslator : ExpressionVisitor
    {
        StringBuilder sb;

        internal string Translate(Expression expression)
        {
            sb = new StringBuilder();
            Visit(expression);
            return sb.ToString();
        }

        static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where")
            {
                sb.Append("SELECT * FROM (");

                Visit(m.Arguments[0]);

                sb.Append(") AS T WHERE ");

                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                this.Visit(lambda.Body);

                return m;
            }

            throw new NotSupportedException(string.Format("The method `{0}' is not supported", m.Method.Name));
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    sb.Append(" NOT ");
                    Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The unary operator `{0}' is not supported", u.NodeType));
            }

            return u;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            sb.Append("(");

            this.Visit(b.Left);

            switch (b.NodeType)
            {
                case ExpressionType.And:
                    sb.Append(" AND ");
                    break;
                case ExpressionType.Or:
                    sb.Append(" OR ");
                    break;
                case ExpressionType.Equal:
                    sb.Append(" = ");
                    break;
                case ExpressionType.NotEqual:
                    sb.Append(" <> ");
                    break;
                case ExpressionType.LessThan:
                    sb.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    sb.Append(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    sb.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    sb.Append(" >= ");
                    break;
                default:
                    throw new NotSupportedException(string.Format("The binary operator `{0}' is not supported", b.NodeType));
            }
            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            IQueryable q = c.Value as IQueryable;
            if (q != null)
            {
                sb.Append("SELECT * FROM ");
                sb.Append(q.ElementType.Name);
            }
            else if (c.Value == null)
            {
                sb.Append("NULL");
            }
            else
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        sb.Append(((bool)c.Value) ? 1 : 0);
                        break;
                    case TypeCode.String:
                        sb.Append("`");
                        sb.Append(c.Value);
                        sb.Append("`");
                        break;
                    case TypeCode.Object:
                        throw new NotSupportedException(string.Format("The constant for `{0}' is not supported", c.Value));
                    default:
                        sb.Append(c.Value);
                        break;
                }
            }
            return c;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                sb.Append(m.Member.Name);
                return m;
            }
            throw new NotSupportedException(string.Format("The member `{ 0 }' is not supported", m.Member.Name));
        }
    }
}
