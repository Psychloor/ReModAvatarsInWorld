namespace ReModAvatarsInWorld
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using HarmonyLib;

    using MelonLoader;

    using ReMod.Core;
    using ReMod.Core.Managers;
    using ReMod.Core.UI;

    using VRC.Core;

    // Thanks ReMod CE
    using AvatarList = Il2CppSystem.Collections.Generic.List<VRC.Core.ApiAvatar>;

    public sealed class AvatarsInWorldComponent : ModComponent, IAvatarListOwner
    {

        private static readonly HashSet<string> FoundAvatarIds = new();

        private static readonly AvatarList FoundAvatarsList = new();

        private ReAvatarList worldAvatarList;

        public AvatarsInWorldComponent()
        {
            Harmony harmonyInstance = new ("AvatarsInWorldReMod");

            // Thanks Knah
            foreach (MethodInfo methodInfo in typeof(AvatarPedestal)
                                              .GetMethods(
                                                  BindingFlags.DeclaredOnly | BindingFlags.Instance
                                                                            | BindingFlags.Public).Where(
                                                  it => it.Name.StartsWith("Method_Private_Void_ApiContainer_")
                                                        && it.GetParameters().Length == 1))
                harmonyInstance.Patch(
                    methodInfo,
                    GetType().GetMethod(
                        nameof(AvatarPedestalPatch),
                        BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).ToNewHarmonyMethod());
        }

        public void Clear(ReAvatarList avatarList)
        {
        }

        public AvatarList GetAvatars(ReAvatarList avatarList)
        {
            return FoundAvatarsList;
        }

        public override void OnLeftRoom()
        {
            FoundAvatarsList.Clear();
            FoundAvatarIds.Clear();
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            worldAvatarList = new ReAvatarList("Avatars In World", this, false);
        }

        private static void AvatarPedestalPatch(ApiContainer __0)
        {
            if (__0.Code != 200) return;
            ApiAvatar model = __0.Model?.TryCast<ApiAvatar>();
            if (model == null) return;

            if (model.releaseStatus.IndexOf("private", StringComparison.OrdinalIgnoreCase) != -1
                && !model.authorId.Equals(APIUser.CurrentUser.id, StringComparison.Ordinal)) return;

            if (!FoundAvatarIds.Contains(model.id))
            {
                FoundAvatarIds.Add(model.id);
                FoundAvatarsList.Add(model);
            }
        }

    }

}