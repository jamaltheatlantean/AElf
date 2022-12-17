using AElf.Sdk.CSharp;
using AElf.Standards.ACS7;
using AElf.Types;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.CA;

public partial class CAContract
{
    public override Empty ValidateCAHolderInfoWithManagersExists(ValidateCAHolderInfoWithManagersExistsInput input)
    {
        Assert(input != null);
        Assert(input.CaHash != null);
        Assert(input.Managers != null);

        var holderInfo = State.HolderInfoMap[input.CaHash];
        Assert(holderInfo != null, $"Holder by ca_hash: {input.CaHash} is not found!");
        
        Assert(holderInfo.Managers.Count == input.Managers.Count, "Managers set is out of time! Please GetHolderInfo again.");

        foreach (var manager in input.Managers)
        {
            if (!CAHolderContainsManager(holderInfo.Managers, manager))
            {
                Assert(false, 
                    $"Manager(address:{manager.ManagerAddresses},device_string{manager.DeviceString}) is not in this CAHolder.");
            }
        }
        
        return new Empty();
    }

    public override Empty SyncHolderInfo(SyncHolderInfoInput input)
    {
        var originalTransaction = Transaction.Parser.ParseFrom(input.TransactionBytes);
        Assert(originalTransaction.MethodName == nameof(ValidateCAHolderInfoWithManagersExists), $"Invalid transaction method.");
        
        var originalTransactionId = originalTransaction.GetHash();
        HolderInfoTransactionVerify(originalTransactionId, input.ParentChainHeight, input.FromChainId, input.MerklePath, input.CaContractAddress);
        var transactionInput =
            ValidateCAHolderInfoWithManagersExistsInput.Parser.ParseFrom(originalTransaction.Params);
        
        var holderId = transactionInput.CaHash;
        var holderInfo = State.HolderInfoMap[holderId] ?? new HolderInfo();

        holderInfo.CreatorAddress = Context.Sender;
        holderInfo.Managers.AddRange(transactionInput.Managers);
        
        State.HolderInfoMap[holderId] = holderInfo;

        return new Empty();
    }
    
    private void HolderInfoTransactionVerify(Hash transactionId, long parentChainHeight, int chainId, MerklePath merklePath, Address address)
    {
        var verificationInput = new VerifyTransactionInput
        {
            TransactionId = transactionId,
            ParentChainHeight = parentChainHeight,
            VerifiedChainId = chainId,
            Path = merklePath
        };

        var verificationResult = Context.Call<BoolValue>(address,
            nameof(ACS7Container.ACS7ReferenceState.VerifyTransaction), verificationInput);
        Assert(verificationResult.Value, "CAHolder validatation transaction verification failed.");
    }


    private bool CAHolderContainsManager(RepeatedField<Manager> managers, Manager targetManager)
    {
        foreach (var manager in managers)
        {
            if (manager.ManagerAddresses == targetManager.ManagerAddresses
                && manager.DeviceString == targetManager.DeviceString)
            {
                return true;
            }
        }

        return false;
    }
}