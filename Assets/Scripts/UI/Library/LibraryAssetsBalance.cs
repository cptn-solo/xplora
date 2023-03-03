using Assets.Scripts.ECS;
using Assets.Scripts.Data;
using Assets.Scripts.Services;
using Zenject;

namespace Assets.Scripts.UI.Library
{
    public partial class LibraryAssetsBalance : BaseEntityView<Team>
    {
        private readonly HeroLibraryService libraryService;

        [Inject]
        public void Construct(HeroLibraryService libraryService)
        {
            libraryService.RegisterEntityView<Team>(this, libraryService.PlayerTeamEntity);
        }

        private void OnDestroy()
        {
            OnGameObjectDestroy();
        }

    }
}