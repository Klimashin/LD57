using System;
using Cysharp.Threading.Tasks;

namespace Game.Gameplay
{
    public class EndGameTileController : FieldTileController
    {
        public async UniTask PlayAnimation()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
        }
    }
}
