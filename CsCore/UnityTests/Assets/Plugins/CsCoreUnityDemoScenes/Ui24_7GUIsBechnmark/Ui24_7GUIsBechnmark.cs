using System.Collections;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace com.csutil.tests {

    public class Ui24_7GUIsBechnmark : UnitTestMono {

        public override IEnumerator RunTest() { yield return RunAll7GUIsTests().AsCoroutine(); }

        /// <summary> See https://eugenkiss.github.io/7guis/tasks for a detailed overview of all 7 benchmark tasks </summary>
        private async Task RunAll7GUIsTests() {

            var viewStack = gameObject.GetViewStack();

            var map = gameObject.GetLinkMap();
            Task t1 = map.Get<Button>("Task1_Counter").SetOnClickAction(async delegate {
                await Task1_Counter.ShowIn(viewStack);
            });
            Task t2 = map.Get<Button>("Task2_TemperatureConverter").SetOnClickAction(async delegate {
                await Task2_TemperatureConverter.ShowIn(viewStack);
            });
            Task t3 = map.Get<Button>("Task3_FlightBooker").SetOnClickAction(async delegate {
                await Task3_FlightBooker.ShowIn(viewStack);
            });
            Task t4 = map.Get<Button>("Task4_Timer").SetOnClickAction(async delegate {
                await Task4_Timer.ShowIn(viewStack);
            });
            Task t5 = map.Get<Button>("Task5_CRUD").SetOnClickAction(async delegate {
                await Task5_CRUD.ShowIn(viewStack);
            });
            Task t6 = map.Get<Button>("Task6_CircleDrawer").SetOnClickAction(async delegate {
                await Task6_CircleDrawer.ShowIn(viewStack);
            });
            Task t7 = map.Get<Button>("Task7_Cells").SetOnClickAction(async delegate {
                await Task7_Cells.ShowIn(viewStack);
            });

            SimulateButtonClickOn("Task1_Counter");
            await t1;

            SimulateButtonClickOn("Task2_TemperatureConverter");
            await t2;

            SimulateButtonClickOn("Task3_FlightBooker");
            await t3;

            SimulateButtonClickOn("Task4_Timer");
            await t4;

            SimulateButtonClickOn("Task5_CRUD");
            await t5;

            SimulateButtonClickOn("Task6_CircleDrawer");
            await t6;

            SimulateButtonClickOn("Task7_Cells");
            await t7;

        }

    }

}