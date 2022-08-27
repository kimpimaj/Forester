using Forester.Framework.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forester.Framework.Policies
{
    internal class PolicyCatalog
    {
        private List<Func<IList<ICommand>>> _policies = new();

        internal List<ICommand> Trigger()
        {
            return _policies
                .SelectMany(p => p.Invoke())
                .ToList();
        }

        public void RegisterPolicy<TProjectionQueryHandler>(IPolicyRehydrator rehydrator)
            where TProjectionQueryHandler : IPolicy, new()
        {
            Func<IList<ICommand>> forwarder = () =>
            {
                var policy = new TProjectionQueryHandler();
                var query = new PolicyTrigger();
                var result = rehydrator.RehydrateAndTrigger(policy, query);
                return result;
            };

            _policies.Add(forwarder);
        }
    }
}
