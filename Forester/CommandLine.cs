using Forester.Domain.Streams.CompleteOrder;
using Forester.Domain.Streams.GetCompletedOrders;
using Forester.Domain.Streams.Log;
using Forester.Domain.Streams.LogLocations;
using Forester.Domain.Streams.Order;
using Forester.Framework;
using Forester.Framework.EventStore.InMemory;
using System.Globalization;
using System.Text;

namespace Forester
{
    public class ControlledClock : IClock
    {
        private DateTime _time;

        public void Set(DateTime time)
        {
            _time = time;
        }

        public void Increment(TimeSpan by)
        {
            _time = _time.Add(by);
        }

        public DateTime Now()
        {
            return _time;
        }
    }

    public class CommandLine
    {
        private Dictionary<string, string> _knownNodes = new Dictionary<string, string>();
        private Dictionary<string, string> _nodesToInit = new Dictionary<string, string>();
        private Dictionary<string, INode> _nodes = new Dictionary<string, INode>();

        private ControlledClock _clock = new ControlledClock();
        private IClock _eventStoreClock;

        public void Run()
        {
            while (true)
            {
                Console.Write("Input > ");
                var input = Console.ReadLine();
                HandleInput(input.Split(" "));
            }
        }

        private string HandleInput(string[] args)
        {
            string result = null;

            try
            {
                var cmd = args[0];

                if (cmd == "help")
                {
                    Console.WriteLine("help -- Shows help");
                    Console.WriteLine("read <file> -- Reads inputs from a file");
                    Console.WriteLine("init-add-node <node> <address> -- Registers an in-process node with an address. Address not needed for in-memory implementation.");
                    Console.WriteLine("init-add-known-node <node> <address> -- Registers an external known node with an address. Address not needed for in-memory implementation.");
                    Console.WriteLine("init -- Initializes registered nodes by 'init-add-node' command.");
                    Console.WriteLine("cmd-order-issue -- ");
                    Console.WriteLine("init -- ");
                    Console.WriteLine("init -- ");
                    Console.WriteLine("init -- ");
                    Console.WriteLine("init -- ");
                    Console.WriteLine("init -- ");
                    Console.WriteLine("init -- ");

                } 

                if (cmd == "read")
                {
                    var file = args[1];
                    ReadInput(file);
                }
                else if (cmd == "init-add-node")
                {
                    var node = args[1];
                    var address = args.Count() == 3 ? args[2] : node;
                    _knownNodes[node] = address;
                    _nodesToInit[node] = address;
                }
                else if (cmd == "init-add-known-node")
                {
                    var node = args[1];
                    var address = args[2];
                    _knownNodes[node] = address;
                }
                else if (cmd == "init")
                {
                    var clockType = args[1]; // --use-controlled-clock or use-normal-clock
                    
                    if (clockType == "--use-controlled-clock")
                    {
                        _eventStoreClock = _clock;
                    } 
                    else
                    {
                        _eventStoreClock = new DefaultClock();
                    }

                    foreach (var (node, address) in _nodesToInit)
                    {
                        _nodes[node] = InitNode(node, address);
                    }
                }
                else if (cmd == "time-set")
                {
                    var time = Parse(args[1]);
                    _clock.Set(time);
                }
                else if (cmd == "time-increment-m")
                {
                    var time = Int32.Parse(args[1]);
                    _clock.Increment(TimeSpan.FromMinutes(time));
                }
                else if (cmd == "time-increment-h")
                {
                    var time = Int32.Parse(args[1]);
                    _clock.Increment(TimeSpan.FromHours(time));
                }
                else if (cmd == "cmd-order-issue")
                {
                    var node = args[1];
                    var order = args[2];
                    var site = args[3];
                    var salesPerson = args[4];
                    _nodes[node].Handle(new IssueOrderCommand(order, site, salesPerson));
                }
                else if (cmd == "cmd-order-assign")
                {
                    var node = args[1];
                    var order = args[2];
                    var harvester = args[3];
                    _nodes[node].Handle(new AssignToHarvesterCommand(order, harvester));
                }
                else if (cmd == "cmd-order-harvested")
                {
                    var node = args[1];
                    var order = args[2];
                    var salesPerson = args[3];
                    _nodes[node].Handle(new ReportOrderSiteHarvestedCommand(order, salesPerson));
                }
                else if (cmd == "cmd-order-completed")
                {
                    var node = args[1];
                    var order = args[2];
                    var report = args[3];
                    _nodes[node].Handle(new CompleteOrderCommand(order, report));
                }
                else if (cmd == "cmd-identify-tree")
                {
                    var node = args[1];
                    var siteId = args[2];
                    var treeId = args[3];
                    var lat = Int32.Parse(args[4]);
                    var lon = Int32.Parse(args[5]);
                    var species = args[5];
                    _nodes[node].Handle(new IdentifyTreeCommand(treeId, siteId, lat, lon, species));
                }
                else if (cmd == "cmd-cut-down")
                {
                    var node = args[1];
                    var treeId = args[2];
                    var by = node;
                    _nodes[node].Handle(new CutDownTreeCommand(treeId, by));
                }
                else if (cmd == "cmd-cut-to-length")
                {
                    var node = args[1];
                    var treeId = args[2];
                    var logId = args[3];
                    var lat = Int32.Parse(args[4]);
                    var lon = Int32.Parse(args[5]);
                    var cutLength = Int32.Parse(args[6]);
                    var volume = Int32.Parse(args[7]);
                    _nodes[node].Handle(new CutLogToLengthCommand(treeId, logId, lat, lon, cutLength, volume));
                }
                else if (cmd == "cmd-pick")
                {
                    var node = args[1];
                    var siteId = args[2];
                    var by = node;
                    var lat = Int32.Parse(args[3]);
                    var lon = Int32.Parse(args[4]);
                    _nodes[node].Handle(new PickBunchCommand(siteId, by, lat, lon));
                }
                else if (cmd == "cmd-ground")
                {
                    var node = args[1];
                    var siteId = args[2];
                    var by = node;
                    var lat = Int32.Parse(args[3]);
                    var lon = Int32.Parse(args[4]);
                    _nodes[node].Handle(new GroundBunchCommand(siteId, by, lat, lon));

                }
                else if (cmd == "cmd-load-to-truck")
                {
                    var node = args[1];
                    var siteId = args[2];
                    var toTruck = args[3];
                    var lat = Int32.Parse(args[4]);
                    var lon = Int32.Parse(args[5]);
                    _nodes[node].Handle(new LoadBunchToTruckCommand(siteId, toTruck, lat, lon));

                }
                else if (cmd == "cmd-unload-to-warehouse")
                {
                    var node = args[1];
                    var siteId = args[2];
                    var fromTruck = args[3];
                    var toWarehouse = args[4];
                    _nodes[node].Handle(new StoreToWarehouseCommand(siteId, fromTruck, toWarehouse));

                }
                else if (cmd == "query-completed-orders")
                {
                    var node = args[1];
                    var asAt = args.Length >= 3 ? Parse(args[2]) : Future();
                    var asOf = args.Length == 4 ? Parse(args[3]) : Future();
                    var query = new GetCompletedOrdersQuery()
                    {
                        AsAt = asAt,
                        AsOf = asOf,
                    };
                    result = _nodes[node].Query(query).ToString();
                    Console.WriteLine(result);
                }
                else if (cmd == "query-issued-orders")
                {
                    var node = args[1];
                    var asAt = args.Length >= 3 ? Parse(args[2]) : Future();
                    var asOf = args.Length == 4 ? Parse(args[3]) : Future();
                    var query = new GetIssuedOrdersQuery()
                    {
                        AsAt = asAt,
                        AsOf = asOf,
                    };
                    result = _nodes[node].Query(query).ToString();
                    Console.WriteLine(result);
                }
                else if (cmd == "cmd-sync")
                {
                    var node = args[1];
                    var remote = args[2];
                    var conflicts1 = _nodes[node].Synchronize(remote);
                    var exceptions1 = _nodes[node].TriggerPolicyChecks();
                    var exceptions2 = _nodes[remote].TriggerPolicyChecks();
                    var conflicts2 = _nodes[node].Synchronize(remote);

                    var conflicts = conflicts1.Union(conflicts2).ToHashSet();

                    var resultBuilder = new StringBuilder();

                    foreach (var c in conflicts)
                    {
                        resultBuilder.Append($"Conflict at {c}.");
                    }
                    foreach (var e in exceptions1)
                    {
                        Console.WriteLine($"Exception during policy checks ({node}): '{e.Message}'.");
                    }
                    foreach (var e in exceptions2)
                    {
                        Console.WriteLine($"Exception during policy checks ({remote}): '{e.Message}'.");
                    }

                    result = resultBuilder.ToString();
                    Console.WriteLine(result);
                }
                else if (cmd == "check-policies")
                {
                    var node = args[1];
                    _nodes[node].TriggerPolicyChecks();
                }
                else if (cmd == "validate-output")
                {
                    var outFile = args[1];
                    var res = HandleInput(args.Skip(2).ToArray());

                    var expected = ReadOutput(outFile);

                    if (res != expected)
                        throw new InvalidOperationException("Output was not valid");

                    Console.WriteLine("Output was valid.");
                }
                else
                {
                    Console.WriteLine("Unknown command");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }

        private DateTime Future()
        {
            return DateTime.UtcNow.AddYears(1);
        }

        private DateTime Parse(string dateTime)
        {
            return DateTime.ParseExact(dateTime, "yyyy-MM-dd|HH:mm:ss", CultureInfo.InvariantCulture);
        }

        private INode InitNode(string node, string address)
        {
            var builder = NodeBuilder.Builder();

            builder.AddThisNode(node, address);

            foreach (var (knownNode, knownAddress) in _knownNodes.Where(k => k.Key != node))
            {
                builder.AddKnownNode(knownNode, knownAddress);
            }

            builder
                .RegisterSerializer(new GenericEventSerializer<TreeCutDownEvent>())
                .RegisterSerializer(new GenericEventSerializer<LogCutToLengthEvent>())
                .RegisterSerializer(new GenericEventSerializer<BunchPickedByForwarderEvent>())
                .RegisterSerializer(new GenericEventSerializer<BunchGroundedByForwarderEvent>())
                .RegisterSerializer(new GenericEventSerializer<BunchLoadedToTruckEvent>())
                .RegisterSerializer(new GenericEventSerializer<OrderIssuedEvent>())
                .RegisterSerializer(new GenericEventSerializer<AssignedToHarvesterEvent>())
                .RegisterSerializer(new OwnershipTransferredEventSerializer())
                .RegisterSerializer(new GenericEventSerializer<SiteHarvestedEvent>())
                .RegisterSerializer(new GenericEventSerializer<OrderCompletedEvent>())
                .UseEventstore((context, address) => new InMemoryEventStoreAdapter(address, _eventStoreClock))
                .RegisterSimpleAggregate<LogEventProcessor>((context) => new LogEventProcessorRepository(), (registrar) =>
                {
                    registrar.RegisterCommand<IdentifyTreeCommand, LogEventProcessor>();
                    registrar.RegisterCommand<CutDownTreeCommand, LogEventProcessor>();
                    registrar.RegisterCommand<CutLogToLengthCommand, LogEventProcessor>();
                })
                .RegisterSimpleAggregate<LogLocationsEventProcessor>((context) => new LogLocationsEventProcessorRepository(), (registrar) =>
                {
                    registrar.RegisterCommand<PickBunchCommand, LogLocationsEventProcessor>();
                    registrar.RegisterCommand<GroundBunchCommand, LogLocationsEventProcessor>();
                    registrar.RegisterCommand<LoadBunchToTruckCommand, LogLocationsEventProcessor>();
                    registrar.RegisterCommand<StoreToWarehouseCommand, LogLocationsEventProcessor>();
                })
                .RegisterDynamicallyOwnedAggregate<OrderAggregate>((context) => new OrderAggregateRepository(context.NodeName), (registrar) =>
                {
                    registrar.RegisterCommand<IssueOrderCommand, OrderAggregate>();
                    registrar.RegisterCommand<AssignToHarvesterCommand, OrderAggregate>();
                    registrar.RegisterCommand<ReportOrderSiteHarvestedCommand, OrderAggregate>();
                    registrar.RegisterCommand<CompleteOrderCommand, OrderAggregate>();
                })
                .RegisterProjectionQueryHandler<CompletedOrdersProjection, GetCompletedOrdersQuery, GetCompletedOrdersQueryResult>()
                .RegisterProjectionQueryHandler<IssuedOrdersProjection, GetIssuedOrdersQuery, GetIssuedOrdersQueryResult>()
                .RegisterPolicy<CompleteOrderAfterHarvestPolicy>();

            return builder.Build();
        }

        public void ReadInput(string file)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), file);

            foreach (var line in File.ReadAllLines(path))
            {
                if (line.StartsWith("#") || line.Length == 0)
                    continue;
                Console.WriteLine($"input > {line}");
                this.HandleInput(line.Split(" "));
            }
        }

        public string ReadOutput(string file)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), file);
            return File.ReadAllText(path);
        }
    }
}
