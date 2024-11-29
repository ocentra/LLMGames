using Cysharp.Threading.Tasks;

namespace OcentraAI.LLMGames.Events
{
    public class GetLocalPlayerEvent : EventArgsBase
    {
        public UniTaskCompletionSource<OperationResult<IPlayerData>> PlayerDataSource { get; }

        public GetLocalPlayerEvent(UniTaskCompletionSource<OperationResult<IPlayerData>> playerDataSource)
        {

            PlayerDataSource = playerDataSource;
        }
    }
}