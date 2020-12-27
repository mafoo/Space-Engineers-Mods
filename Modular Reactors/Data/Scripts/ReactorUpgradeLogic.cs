// ***********************************************************************
// Assembly         : Modular Reactors
// Created          : 2020-12-08
// ***********************************************************************
// <copyright file="ReactorUpgradeLogic.cs">
//     Copyright (c) 2020 github@mafoo.org. All rights reserved
// </copyright>
// ***********************************************************************

// ReSharper disable once CheckNamespace

namespace ModularReactors
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.ModAPI;
    using VRage.Game.Components;
    using VRage.Game.ModAPI;
    using VRage.ObjectBuilders;

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Reactor), true)]
    public class ReactorUpgradeLogic : MyGameLogicComponent
    {
        private MyObjectBuilder_EntityBase ObjectBuilder { get; set; }
        private IMyCubeBlock Parent { get; set; }
        private IMyReactor Reactor { get; set; }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false) => this.ObjectBuilder;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            this.Reactor = this.Entity as IMyReactor;
            this.Parent = this.Entity as IMyCubeBlock;
            if (this.Parent == null)
            {
                return;
            }

            if (this.Parent.UpgradeValues.ContainsKey("Power"))
            {
                //Another mod is already doing our logic, skipping the setup
                return;
            }

            this.ObjectBuilder = objectBuilder;
            this.Parent.UpgradeValues.Add("Power", 0f);
            this.Parent.OnUpgradeValuesChanged += this.OnUpgradeValuesChanged;
        }

        private void OnUpgradeValuesChanged()
        {
            this.Reactor.PowerOutputMultiplier = this.Parent.UpgradeValues["Power"] + 1f;
        }
    }
}
