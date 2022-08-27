using Forester.Framework.Aggregates;

namespace Forester.Domain.Streams.Log
{
    public interface ILogCommandHandler :
        ICommandHandler<IdentifyTreeCommand>,
        ICommandHandler<CutDownTreeCommand>,
        ICommandHandler<CutLogToLengthCommand>
    { }

    public record IdentifyTreeCommand(string StreamId, string SiteId, int Latitude, int Longitude, string Species) : ICommand;
    public record CutDownTreeCommand(string StreamId, string By) : ICommand;
    public record CutLogToLengthCommand(string StreamId, string LogId, int Latitude, int Longitude, int CutLength, int Volume) : ICommand;
}
