﻿using com.csutil.ui;
using System.Collections;
using UnityEngine.UI;

namespace com.csutil.tests.ui {

    public class Ui12_ActionMenu : UnitTestMono {

        public override IEnumerator RunTest() {

            var map = gameObject.GetLinkMap();

            // A minimalistic simple example to show an action menu:
            map.Get<Button>("ShowMenu1").SetOnClickAction(async delegate {

                // Icons copied from https://shanfan.github.io/material-icons-cheatsheet/
                var menu1 = new ActionMenu(id: "MyMenu1");
                menu1.entries.Add(new ActionMenu.Entry(MenuIcons.Edit, name: "Rename"));
                menu1.entries.Add(new ActionMenu.Entry(MenuIcons.Copy, name: "Dublicate"));
                menu1.entries.Add(new ActionMenu.Entry(MenuIcons.Delete, name: "Delete"));

                var selectedEntry = await gameObject.GetViewStack().ShowActionMenu(menu1);
                Toast.Show("User selected entry '" + selectedEntry?.name + "'");

            });

            map.Get<Button>("ShowMenu2").SetOnClickAction(async delegate {

                var menu2 = new ActionMenu(id: "MyMenu2") {
                    isMenuFavoriteLogicEnabled = false,
                    onCancel = delegate { return false; } // Dont allow cancel
                };
                menu2.title = "I am a generic action menu, please select one of the following actions:";
                menu2.entries.Add(new ActionMenu.Entry(MenuIcons.Edit, name: "Rename") {
                    descr = "Rename the current thing. Renaming is highly recommended in case the name is not what you want it to be!",
                });
                menu2.entries.Add(new ActionMenu.Entry(MenuIcons.Copy, name: "Dublicate") {
                    descr = "Dublicate the current thing.",
                    onClicked = delegate { Log.d("Dublicate action clicked!"); }
                });
                menu2.entries.Add(new ActionMenu.Entry(MenuIcons.Delete, name: "Delete") {
                    descr = "Delete all the things! If you click here nothing will happen, deleting things is never a good idea :(",
                    isEnabled = false
                });

                var selectedEntry = await gameObject.GetViewStack().ShowActionMenu(menu2);
                Toast.Show("User selected entry '" + selectedEntry?.name + "'");

            });

            yield return null;
        }

    }

}