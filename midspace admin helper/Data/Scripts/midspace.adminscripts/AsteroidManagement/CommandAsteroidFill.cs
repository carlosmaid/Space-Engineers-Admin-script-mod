﻿namespace midspace.adminscripts
{
    using System.Text.RegularExpressions;

    using Sandbox.Definitions;
    using Sandbox.ModAPI;
    using VRageMath;

    /// <summary>
    /// Replaces all ore in an asteroid with the specified ore.
    /// </summary>
    public class CommandAsteroidFill : ChatCommand
    {
        public CommandAsteroidFill()
            : base(ChatCommandSecurity.Admin, "roidfill", new[] { "/roidfill" })
        {
        }

        public override void Help(bool brief)
        {
            MyAPIGateway.Utilities.ShowMessage("/roidfill <name> <material>", "Fill the specified Asteroid <name> will the <material>. ie, \"/roidfill baseasteroid1 gold_01\"");
        }

        public override bool Invoke(string messageText)
        {
            var match = Regex.Match(messageText, @"/roidfill\s+(?<Asteroid>[^\s]+)\s+(?<Material>[^\s]+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var searchAsteroidName = match.Groups["Asteroid"].Value;
                IMyVoxelBase originalAsteroid = null;
                if (!Support.FindAsteroid(searchAsteroidName, out originalAsteroid))
                {
                    MyAPIGateway.Utilities.ShowMessage("Cannot find asteroid", string.Format("'{0}'", searchAsteroidName));
                    return true;
                }

                var searchMaterialName = match.Groups["Material"].Value;
                MyVoxelMaterialDefinition material;
                string suggestedMaterials = "";
                if (!Support.FindMaterial(searchMaterialName, out material, ref suggestedMaterials))
                {
                    MyAPIGateway.Utilities.ShowMessage("Invalid Material specified.", "Cannot find the material '{0}'.\r\nTry the following: {1}", searchMaterialName, suggestedMaterials);
                    return true;
                }

                var boxShape = MyAPIGateway.Session.VoxelMaps.GetBoxVoxelHand();

                // boundaries are in local space, you need to set translation into Transform property
                boxShape.Boundaries = (BoundingBoxD)originalAsteroid.PositionComp.LocalAABB;
                boxShape.Transform = MatrixD.CreateTranslation(originalAsteroid.PositionComp.GetPosition());

                MyAPIGateway.Session.VoxelMaps.PaintInShape(originalAsteroid, boxShape, material.Index);

                MyAPIGateway.Utilities.ShowMessage("Asteroid", "'{0}' filled with material '{1}'.", originalAsteroid.StorageName, material.Id.SubtypeName);
                return true;
            }

            return false;
        }
    }
}
