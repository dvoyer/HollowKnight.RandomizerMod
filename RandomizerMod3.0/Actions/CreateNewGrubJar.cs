﻿using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using SereCore;
using UnityEngine;
using RandomizerMod.FsmStateActions;
using static RandomizerMod.GiveItemActions;

namespace RandomizerMod.Actions
{
    internal class CreateNewGrubJar : RandomizerAction
    {
        public const float GRUB_JAR_ELEVATION = 0.1f;

        private readonly string _newGrubJarName;
        private readonly string _sceneName;
        private readonly float _x;
        private readonly float _y;
        private readonly string _item;
        private readonly string _location;

        public CreateNewGrubJar(string sceneName, float x, float y, string newGrubJarName, string item, string location)
        {
            _sceneName = sceneName;
            _x = x;
            _y = y;
            _newGrubJarName = newGrubJarName;
            _item = item;
            _location = location;
        }

        public override ActionType Type => ActionType.GameObject;

        public override void Process(string scene, Object changeObj)
        {
            if (scene != _sceneName)
            {
                return;
            }

            GameObject GrubJar = ObjectCache.GrubJar;
            GrubJar.name = _newGrubJarName;

            // Move the jar forward so it appears in front of any background objects
            GrubJar.transform.position = new Vector3(_x, _y, GrubJar.transform.position.z - 0.1f);
            var grub = GrubJar.transform.Find("Grub");
            grub.position = new Vector3(grub.position.x, grub.position.y, GrubJar.transform.position.z);

            FixBottleFSM(GrubJar, _item, _location);
            
            GrubJar.SetActive(true);
        }

        public static void FixBottleFSM(GameObject jar, string item, string location)
        {
            var fsm = FSMUtility.LocateFSM(jar, "Bottle Control");
            var init = fsm.GetState("Init");
            init.RemoveActionsOfType<BoolTest>();
            init.AddFirstAction(new RandomizerExecuteLambda(() => fsm.SendEvent(RandomizerMod.Instance.Settings.CheckLocationFound(location) ? "ACTIVATE" : null)));
            // The bottle FSM already takes care of granting the grub and playing happy grub noises
            // We have to add the GiveItem action before incrementing the grub count so the RecentItems
            // correctly notes the grub index
            fsm.GetState("Shatter").AddFirstAction(new RandomizerExecuteLambda(() => GiveItem(GiveAction.None, item, location)));
        }
    }
}
