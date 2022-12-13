using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.CA;

public partial class CAContract
{
    public override Empty AddVerifierServerEndPoints(AddVerifierServerEndPointsInput input)
    {
        Assert(Context.Sender.Equals(State.Admin.Value), 
            "Only Admin has permission to add VerifierServerEndPoints");
        Assert(input != null);
        return new Empty();
    }

    public override Empty RemoveVerifierServerEndPoints(RemoveVerifierServerEndPointsInput input)
    {
        Assert(Context.Sender.Equals(State.Admin.Value), 
            "Only Admin has permission to remove VerifierServerEndPoints");
        return new Empty();
    }

    public override Empty RemoveVerifierServer(RemoveVerifierServerInput input)
    {
        Assert(Context.Sender.Equals(State.Admin.Value), 
            "Only Admin has permission to remove VerifierServer");
        return new Empty();
    }

    public override GetVerifierServersOutput GetVerifierServers(GetVerifierServersInput input)
    {
        var output = new GetVerifierServersOutput();
    }
}