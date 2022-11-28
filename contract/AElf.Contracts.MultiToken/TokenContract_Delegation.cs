using System;
using System.Collections.Generic;
using System.Linq;
using AElf.Standards.ACS1;
using AElf.Standards.ACS10;
using AElf.CSharp.Core;
using AElf.Sdk.CSharp;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;
using AElf.Contracts.MultiToken;

namespace AElf.Contracts.MultiToken;

public partial class TokenContract
{
     
    public override SetTransactionFeeDelegationsOutput SetTransactionFeeDelegations(
        SetTransactionFeeDelegationsInput input)
    {
         
        var Delegatees = State.DelegateesMap[input.DelegatorAddress].Delegatees;
        if (input.Delegations.First(x => x.Value > 0).Key == null)
        {
            
        }
           if(Delegatees[Context.Sender.ToString()] != null)
            {
                if (input.Delegations.First(x => x.Value > 0).Key == null)
                {
                    Delegatees.Remove(Context.Sender.ToString());
                    Context.Fire(new TransactionFeeDelegationCancelled()
                    {
                        Caller = Context.Sender,
                        Delegatee = Context.Sender,
                        Delegator = input.DelegatorAddress
                    });
                }
                Delegatees[Context.Sender.ToString()] = new TransactionFeeDelegations()
                {
                    Delegations = { input.Delegations}
                };
            }
           else
           {
               Assert(input.Delegations.First(x => x.Value > 0).Key != null, "Invalid input");
               Assert(Delegatees.Count() < 128, "delegate count reach limit");
               Delegatees[Context.Sender.ToString()] = new TransactionFeeDelegations()
               {
                   Delegations = { input.Delegations}
               };
               Context.Fire(new TransactionFeeDelegationAdded()
               {
                   Caller = Context.Sender,
                   Delegatee = Context.Sender,
                   Delegator = input.DelegatorAddress
               });
            
           }
           
        return new SetTransactionFeeDelegationsOutput()
        {
            Success = true
        };
    }
    
    
    
    public override Empty RemoveTransactionFeeDelegator(
        RemoveTransactionFeeDelegatorInput   input)
    {

        var Delegatees = State.DelegateesMap[Context.Sender].Delegatees;
        Assert(Delegatees[input.DelegatorAddress.ToString()] != null, "Invalid input");
        Delegatees.Remove(input.DelegatorAddress.ToString());
        Context.Fire(new TransactionFeeDelegationCancelled()
        {
            Caller = Context.Sender,
            Delegatee = Context.Sender,
            Delegator = input.DelegatorAddress
        });
        return new Empty();
    }
    
    public override Empty RemoveTransactionFeeDelegatee(
        RemoveTransactionFeeDelegateeInput input)
    {
        var Delegatees = State.DelegateesMap[input.DelegateeAddress].Delegatees;
        Assert(Delegatees[Context.Sender.ToString()] != null, "Invalid input");
        Delegatees.Remove(Context.Sender.ToString());
        Context.Fire(new TransactionFeeDelegationCancelled()
        {
            Caller = Context.Sender,
            Delegatee = input.DelegateeAddress,
            Delegator = Context.Sender
        });
        return new Empty();
    }
   

   
}