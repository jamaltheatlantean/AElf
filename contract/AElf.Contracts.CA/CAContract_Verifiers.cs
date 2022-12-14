using System;
using System.Linq;
using AElf.Sdk.CSharp;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.CA;

public partial class CAContract
{
    public override Empty AddVerifierServerEndPoints(AddVerifierServerEndPointsInput input)
    {
        Assert(Context.Sender.Equals(State.Admin.Value), 
            "Only Admin has permission to add VerifierServerEndPoints");
        Assert(input != null, "invalid input");
        CheckVerifierServerInputName(input.Name);
        Assert(input.EndPoints != null && input.EndPoints.Count != 0, "invalid input EndPoints");

        if (State.VerifiersServerList.Value == null)
        {
            State.VerifiersServerList.Value = new VerifierServerList();
        }
        
        var verifierServerList = State.VerifiersServerList.Value;
        var server = verifierServerList.VerifierServers
            .FirstOrDefault(server => server.Name == input.Name);

        if (server == null)
        {
            State.VerifiersServerList.Value.VerifierServers.Add(new VerifierServer()
            {
                Name = input.Name,
                EndPoints = { input.EndPoints }
            });
        }
        else
        {
            foreach (var endPoint in input.EndPoints)
            {
                if (!server.EndPoints.Contains(endPoint))
                {
                    server.EndPoints.Add(endPoint);
                }
            }
        }
        
        Context.Fire(new VerifierServerEndPointsAdded()
        {
            VerifierServer = new VerifierServer()
            {
                Name = input.Name,
                EndPoints = { input.EndPoints }
            }
        });
        
        return new Empty();
    }

    public override Empty RemoveVerifierServerEndPoints(RemoveVerifierServerEndPointsInput input)
    {
        Assert(Context.Sender.Equals(State.Admin.Value), 
            "Only Admin has permission to remove VerifierServerEndPoints");
        Assert(input != null, "invalid input");
        CheckVerifierServerInputName(input.Name);
        Assert(input.EndPoints != null && input.EndPoints.Count != 0, "invalid input EndPoints");
        if (State.VerifiersServerList.Value == null) return new Empty();
        
        var server = State.VerifiersServerList.Value.VerifierServers
            .FirstOrDefault(server => server.Name == input.Name);
        if (server != null)
        {
            foreach (var endPoints in input.EndPoints)
            {
                if (server.EndPoints.Contains(endPoints))
                {
                    server.EndPoints.Remove(endPoints);
                }
            }
        }
        
        Context.Fire(new VerifierServerEndPointsRemoved()
        {
            VerifierServer = new VerifierServer()
            {
                Name = input.Name,
                EndPoints = { input.EndPoints }
            }
        });
        
        return new Empty();
    }

    public override Empty RemoveVerifierServer(RemoveVerifierServerInput input)
    {
        Assert(Context.Sender.Equals(State.Admin.Value), 
            "Only Admin has permission to remove VerifierServer");
        Assert(input != null, "invalid input");
        CheckVerifierServerInputName(input.Name);
        if (State.VerifiersServerList.Value == null) return new Empty();
        
        var server = State.VerifiersServerList.Value.VerifierServers
            .FirstOrDefault(server => server.Name == input.Name);
        if (server != null)
        {
            State.VerifiersServerList.Value.VerifierServers.Remove(server);
        }
        
        Context.Fire(new VerifierServerRemoved()
        {
            VerifierServer = server
        });
        
        return new Empty();
    }

    public override GetVerifierServersOutput GetVerifierServers(GetVerifierServersInput input)
    {
        var output = new GetVerifierServersOutput();
        var verifierServerList = State.VerifiersServerList.Value;
        
        if (verifierServerList != null)
        {
            output.VerifierServers.Add(verifierServerList.VerifierServers);
        }

        return output;
    }

    private void CheckVerifierServerInputName(string name)
    {
        Assert(name != null && !String.IsNullOrEmpty(name), "invalid input name");
    }
}