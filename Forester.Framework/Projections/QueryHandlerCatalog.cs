using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forester.Framework.Projections
{
    internal class QueryHandlerCatalog
    {
        private Dictionary<Type, Func<IQuery, object>> _projections = new();

        internal bool IsSupported<TResult>(IQuery<TResult> query) 
        {
            return _projections.ContainsKey(query.GetType());
        }

        internal TResult Forward<TResult>(IQuery<TResult> query) 
        {
            return (TResult)_projections[query.GetType()](query);
        }

        public void RegisterProjectionQueryHandler<TProjectionQueryHandler, TQuery, TResult>(IProjectionQueryRehydrator<TProjectionQueryHandler> rehydrator)
            where TProjectionQueryHandler : IProjectionQueryHandler<TQuery, TResult>, new()
            where TQuery : IQuery<TResult>
        {
            Func<IQuery, object> forwarder = (query) =>
            {
                var projection = new TProjectionQueryHandler();
                var result = rehydrator.RehydrateAndQuery<TProjectionQueryHandler, TQuery, TResult>(projection, (TQuery)query);
                return result;
            };

            _projections.Add(typeof(TQuery), forwarder);
        }
    }
}
