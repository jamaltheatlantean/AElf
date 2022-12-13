using System.Linq;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.CA;

public partial class CAContract
{
    public override Empty AddCAServer(AddCAServerInput input)
    {
        Assert(Context.Sender == State.Admin.Value,"No permission.");
        Assert(input.Name != null && input.EndPoints != null,"Invalid input.");
        var existServer = State.CaServerList.Value.CaServers.FirstOrDefault(s => s.Name == input.Name);
        if (existServer != null)
        {
            existServer.EndPoint = input.EndPoints;
        }
        else
        {
            State.CaServerList.Value.CaServers.Add(new CAServer
            {
                Name = input.Name,
                EndPoint = input.EndPoints
            });
        }
        return new Empty();
    }

    public override Empty RemoveCAServer(RemoveCAServerInput input)
    {
        Assert(Context.Sender == State.Admin.Value,"No permission.");
        Assert(input.Name != null,"Invalid input.");
        var existServer = State.CaServerList.Value.CaServers.FirstOrDefault(s => s.Name == input.Name);
        if (existServer != null)
        {
            State.CaServerList.Value.CaServers.Remove(existServer);
        }
        return new Empty();
    }

    public override GetCAServersOutput GetCAServers(Empty input)
    {
        var caServers = State.CaServerList.Value.CaServers;
        return new GetCAServersOutput
        {
            CaServers = { caServers }
        };
    }
}