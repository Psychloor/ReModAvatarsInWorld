namespace ReModAvatarsInWorld
{

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using BestHTTP;

    using Harmony;

    using MelonLoader;

    using ReMod.Core;
    using ReMod.Core.Managers;
    using ReMod.Core.UI;
    using ReMod.Core.Unity;

    using UnityEngine;

    using VRC.Core;

    using Object = UnityEngine.Object;
    
    // Thanks ReMod CE
    using AvatarList = Il2CppSystem.Collections.Generic.List<VRC.Core.ApiAvatar>;
    using Harmony = HarmonyLib.Harmony;

    public sealed class AvatarsInWorldComponent : ModComponent, IAvatarListOwner
    {
        private ReAvatarList worldAvatarList; 

        private static readonly AvatarList FoundAvatarsList = new();

        private static readonly HashSet<string> FoundAvatarIds = new();

        private readonly Harmony harmonyInstance;

        public override void OnUiManagerInit(UiManager uiManager)
        {
            worldAvatarList = new ReAvatarList("Avatars In World", this, false);

            // Since some worlds might have a fuck-ton of avatars, it's better to manually refresh.
            // Also since re-grabbing all avatars in world each time you open up the avatar menu isn't the best xD
            // Object.Destroy(worldAvatarList.GameObject.GetComponent<EnableDisableListener>());
        }

        public AvatarsInWorldComponent()
        {
            harmonyInstance = new Harmony("AvatarsInWorldReMod");
            
            // Thanks Knah
            foreach (var methodInfo in typeof(AvatarPedestal).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public).Where(it => it.Name.StartsWith("Method_Private_Void_ApiContainer_") && it.GetParameters().Length == 1))
            {
                harmonyInstance.Patch(
                    methodInfo,
                    GetType().GetMethod(
                        nameof(AvatarPedestalPatch),
                        BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).ToNewHarmonyMethod());
            }
        }
        
        private static void AvatarPedestalPatch(ApiContainer __0)
        {
            if (__0.Code != 200) return;
            var model = __0.Model?.TryCast<ApiAvatar>();
            if (model == null) return;

            if (model.releaseStatus.IndexOf("private", StringComparison.OrdinalIgnoreCase) != -1
                && !model.authorId.Equals(APIUser.CurrentUser.id, StringComparison.Ordinal)) return;

            if (!FoundAvatarIds.Contains(model.id))
            {
                FoundAvatarIds.Add(model.id);
                FoundAvatarsList.Add(model);
            }
        }

        public override void OnLeftRoom()
        {
            FoundAvatarsList.Clear();
            FoundAvatarIds.Clear();
        }

        public AvatarList GetAvatars(ReAvatarList avatarList)
        {
            /*foreach (AvatarPedestal pedestal in Resources.FindObjectsOfTypeAll<AvatarPedestal>())
            {
                // Patch into set of it and if not null add to founds?
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
            }*/

            return FoundAvatarsList;
        }

        public void Clear(ReAvatarList avatarList)
        {
            
        }

    }

}