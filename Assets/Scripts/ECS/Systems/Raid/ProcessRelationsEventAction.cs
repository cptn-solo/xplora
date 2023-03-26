using Assets.Scripts.ECS.Data;
using Assets.Scripts.Data;

namespace Assets.Scripts.ECS.Systems
{
    public class ProcessRelationsEventAction : ProcessDialogActionSystem<RelationsEventInfo>
    {
        protected override void ProcessAction(
            RelationsEventInfo currentEventInfo,
            ModalDialogAction<RelationsEventInfo> action)
        { 
            // nothing to do here    
        }
    }
}