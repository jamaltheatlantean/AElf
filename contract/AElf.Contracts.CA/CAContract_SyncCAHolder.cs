using System.Linq;
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
                    $"Manager(address:{manager.ManagerAddress},device_string{manager.DeviceString}) is not in this CAHolder.");
            }
        }
        
        return new Empty();
    }

    public override Empty SyncHolderInfo(SyncHolderInfoInput input)
    {

        var originalTransaction = MethodNameVerify(input.VerificationTransactionInfo, nameof(ValidateCAHolderInfoWithManagersExists));
        var originalTransactionId = originalTransaction.GetHash();
        
        TransactionVerify(originalTransactionId, input.VerificationTransactionInfo.ParentChainHeight, input.VerificationTransactionInfo.FromChainId, input.VerificationTransactionInfo.MerklePath, input.VerificationTransactionInfo.CaContractAddress);
        var transactionInput =
            ValidateCAHolderInfoWithManagersExistsInput.Parser.ParseFrom(originalTransaction.Params);
        
        var holderId = transactionInput.CaHash;
        var holderInfo = State.HolderInfoMap[holderId] ?? new HolderInfo();

        holderInfo.CreatorAddress = Context.Sender;
        var managersToAdd = ManagersExcept(transactionInput.Managers, holderInfo.Managers);
        var managersToRemove = ManagersExcept(holderInfo.Managers, transactionInput.Managers);
        
        holderInfo.Managers.AddRange(managersToAdd);
        SetDelegators(holderId, managersToAdd);
        foreach (var manager in managersToRemove)
        {
            holderInfo.Managers.Remove(manager);
        }
        
        RemoveDelegators(holderId, managersToRemove);
        
        State.HolderInfoMap[holderId] = holderInfo;

        return new Empty();
    }

    private RepeatedField<Manager> ManagersExcept(RepeatedField<Manager> set1, RepeatedField<Manager> set2)
    {
        RepeatedField<Manager> resultSet = new RepeatedField<Manager>();
        
        foreach (var manager1 in set1)
        {
            bool theSame = false;
            foreach (var manager2 in set2)
            {
                if (manager1.ManagerAddress == manager2.ManagerAddress)
                {
                    theSame = true;
                    break;
                }
            }

            if (!theSame)
            {
                resultSet.Add(manager1);
            }
        }

        return resultSet;
    }

    private Transaction MethodNameVerify(VerificationTransactionInfo info, string methodNameExpected)
    {
        var originalTransaction = Transaction.Parser.ParseFrom(info.TransactionBytes);
        Assert(originalTransaction.MethodName == methodNameExpected, $"Invalid transaction method.");

        return originalTransaction;
    }
    
    private void TransactionVerify(Hash transactionId, long parentChainHeight, int chainId, MerklePath merklePath, Address address)
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
        Assert(verificationResult.Value, "transaction verification failed.");
    }


    private bool CAHolderContainsManager(RepeatedField<Manager> managers, Manager targetManager)
    {
        foreach (var manager in managers)
        {
            if (manager.ManagerAddress == targetManager.ManagerAddress
                && manager.DeviceString == targetManager.DeviceString)
            {
                return true;
            }
        }

        return false;
    }
}