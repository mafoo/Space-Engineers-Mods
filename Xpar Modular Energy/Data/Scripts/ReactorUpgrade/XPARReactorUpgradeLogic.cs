using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using VRage.Game.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactorUpgrade
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Reactor))]
    public class XPARReactorUpgradeLogic : MyGameLogicComponent
    {
        private IMyReactor m_reactor;
        private VRage.Game.ModAPI.IMyCubeBlock m_parent;
        private MyObjectBuilder_EntityBase m_objectBuilder = null;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            m_reactor = Entity as IMyReactor;
            m_parent = Entity as VRage.Game.ModAPI.IMyCubeBlock;

            m_parent.UpgradeValues.Add("Power", 0f);
	    
            m_objectBuilder = objectBuilder;

            m_parent.OnUpgradeValuesChanged += OnUpgradeValuesChanged;
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return m_objectBuilder;
        }

        private void OnUpgradeValuesChanged()
        {
            m_reactor.PowerOutputMultiplier = m_parent.UpgradeValues["Power"] + 1f;
        }
    }
}
