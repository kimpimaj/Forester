namespace Forester.Framework.EventStore
{
    public class SimpleNodeSynchronizer : INodeSynchronizer
    {
        public void Synchronize(IEventStoreClient local, IEventStoreClient remote)
        {
            var localVersion = local.GetCurrentNodeVersion();
            var remoteVersion = remote.GetCurrentNodeVersion();

            var missingFromRemote = local.ReadNewerAndConcurrentTo(remoteVersion);
            var missingFromLocal = remote.ReadNewerAndConcurrentTo(localVersion);

            local.Replicate(missingFromLocal);
            remote.Replicate(missingFromRemote);

            var localVersionMatrix = local.GetCurrentNodeVersionMatrix();
            var remoteVersionMatrix = remote.GetCurrentNodeVersionMatrix();

            local.SynchronizeVersionMatrix(remoteVersionMatrix);
            remote.SynchronizeVersionMatrix(localVersionMatrix);
        }
    }
}
