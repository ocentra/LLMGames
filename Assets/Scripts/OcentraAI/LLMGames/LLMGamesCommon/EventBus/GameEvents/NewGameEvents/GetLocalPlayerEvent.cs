using Cysharp.Threading.Tasks;

namespace OcentraAI.LLMGames.Events
{
    public class GetLocalPlayer : EventArgsBase
    {
        public UniTaskCompletionSource<OperationResult<IPlayerData>> PlayerDataSource { get; }

        public GetLocalPlayer(UniTaskCompletionSource<OperationResult<IPlayerData>> playerDataSource)
        {

            PlayerDataSource = playerDataSource;
        }
    }



}