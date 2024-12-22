using Cysharp.Threading.Tasks;

namespace OcentraAI.LLMGames.Events
{
    public class GetLocalPlayerEvent : EventArgsBase
    {
        public UniTaskCompletionSource<OperationResult<IHumanPlayerData>> PlayerDataSource { get; }

        public GetLocalPlayerEvent(UniTaskCompletionSource<OperationResult<IHumanPlayerData>> playerDataSource)
        {

            PlayerDataSource = playerDataSource;
        }
    }
}