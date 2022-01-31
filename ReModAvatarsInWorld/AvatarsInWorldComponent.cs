namespace ReModAvatarsInWorld
{

    using System;
    using System.Collections.Generic;

    using ReMod.Core;
    using ReMod.Core.Managers;
    using ReMod.Core.UI;
    using ReMod.Core.Unity;

    using UnityEngine;

    using VRC.Core;

    using Object = UnityEngine.Object;
    
    // Thanks ReMod CE
    using AvatarList = Il2CppSystem.Collections.Generic.List<VRC.Core.ApiAvatar>;

    public sealed class AvatarsInWorldComponent : ModComponent, IAvatarListOwner
    {
        private ReAvatarList worldAvatarList;

        private readonly AvatarList foundAvatarsList = new();

        private readonly HashSet<string> foundAvatarIds = new();

        public override void OnUiManagerInit(UiManager uiManager)
        {
            worldAvatarList = new ReAvatarList("Avatars In World", this, false);

            // Since some worlds might have a fuck-ton of avatars, it's better to manually refresh.
            // Also since re-grabbing all avatars in world each time you open up the avatar menu isn't the best xD
            Object.Destroy(worldAvatarList.GameObject.GetComponent<EnableDisableListener>());
        }

        public override void OnLeftRoom()
        {
            foundAvatarsList.Clear();
            foundAvatarIds.Clear();
        }

        public AvatarList GetAvatars(ReAvatarList avatarList)
        {
            foreach (AvatarPedestal pedestal in Resources.FindObjectsOfTypeAll<AvatarPedestal>())
            {
                var avatar = pedestal.field_Private_ApiAvatar_0;
                if (avatar == null
                    || avatar.releaseStatus.IndexOf("private", StringComparison.OrdinalIgnoreCase) != -1
                    && !avatar.authorId.Equals(APIUser.CurrentUser.id, StringComparison.Ordinal)) continue;

                // add new avatars if they haven't been found before
                if (!foundAvatarIds.Contains(avatar.id))
                {
                    foundAvatarIds.Add(avatar.id);
                    foundAvatarsList.Add(pedestal.field_Private_ApiAvatar_0);
                }
            }

            return foundAvatarsList;
        }

        public void Clear(ReAvatarList avatarList)
        {
            
        }

    }

}