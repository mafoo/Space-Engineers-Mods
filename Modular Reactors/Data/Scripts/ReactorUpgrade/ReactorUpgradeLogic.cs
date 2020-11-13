using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;

namespace ModularReactors.Data.Scripts.ReactorUpgrade
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Reactor), true)]
    public class ReactorUpgradeLogic : MyGameLogicComponent
    {
        private MyObjectBuilder_EntityBase m_objectBuilder = null;
        private IMyCubeBlock m_parent;
        private IMyReactor m_reactor;

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return this.m_objectBuilder;
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            this.m_reactor = this.Entity as IMyReactor;
            this.m_parent = this.Entity as IMyCubeBlock;
            if (this.m_parent == null)
            {
                return;
            }

            this.m_objectBuilder = objectBuilder;
            this.m_parent.UpgradeValues.Add("Power", 0f);
            this.m_parent.OnUpgradeValuesChanged += this.OnUpgradeValuesChanged;
        }

        private void OnUpgradeValuesChanged()
        {
            this.m_reactor.PowerOutputMultiplier = this.m_parent.UpgradeValues["Power"] + 1f;
        }
    }
}
