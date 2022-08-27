using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forester.Framework.EventStore
{
    public interface INodeSynchronizer
    {
        void Synchronize(IEventStoreClient local, IEventStoreClient remote);
    }
}
