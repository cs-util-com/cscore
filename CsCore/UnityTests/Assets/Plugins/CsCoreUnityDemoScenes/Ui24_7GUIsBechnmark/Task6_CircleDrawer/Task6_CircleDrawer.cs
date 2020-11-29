using com.csutil.model.immutable;
using com.csutil.ui;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Xunit;

namespace com.csutil.tests {

    internal static class Task6_CircleDrawer {

        public static async Task ShowIn(ViewStack viewStack) {
            TestDataStore();

            DataStore<MyModel> store = NewDataStore();
            MyPresenter presenter = new MyPresenter();
            presenter.targetView = viewStack.ShowView("7GUIs_Task6_CircleDrawer");
            await presenter.LoadModelIntoView(store);
        }

        private static DataStore<MyModel> NewDataStore() {
            MyModel model = new MyModel(null, ImmutableList<MyCircle>.Empty);
            Middleware<MyModel> exampleMiddleware = Middlewares.NewLoggingMiddleware<MyModel>();
            UndoRedoReducer<MyModel> undoLogic = new UndoRedoReducer<MyModel>();
            return new DataStore<MyModel>(undoLogic.Wrap(MyReducer), model, exampleMiddleware);
        }

        private static void TestDataStore() {
            var store = NewDataStore();
            store.Dispatch(new AddCircleAction() { newCircle = new MyCircle("1", 20, 20, 1) });
            store.Dispatch(new AddCircleAction() { newCircle = new MyCircle("2", 40, 10, 2) });
            store.Dispatch(new ChangeDiameterAction() { targetCircleId = "1", newDiameter = 4 });
            // Check that the diameter change automatically selected circle 1:
            Assert.Equal("1", store.GetState().selectedCircle);
            // Check that cicrle 1 now has the new diameter:
            Assert.Equal(4, store.GetState().circles.First(x => x.id == "1").diameter);
        }

        private class MyPresenter : Presenter<DataStore<MyModel>> {

            public GameObject targetView { get; set; }
            private GameObject circleCanvas;
            private Dictionary<string, GameObject> circleGOs;

            private DataStore<MyModel> store;

            public Task OnLoad(DataStore<MyModel> store) {
                this.store = store;
                var map = targetView.GetLinkMap();

                circleCanvas = map.Get<GameObject>("CircleCanvas");
                // React to model changes for both the selected object and the list of circles:
                circleCanvas.SubscribeToStateChanges(store, model => model, RebuildCircleCanvas);

                map.Get<CircleCanvas>("CircleCanvas").OnCicleCreated = (MyCircle newCircle) => {
                    store.Dispatch(new AddCircleAction() { newCircle = newCircle });
                };

                // Setup undo and redo actions for the store:
                map.Get<Button>("Undo").SetOnClickAction(delegate {
                    store.Dispatch(new UndoAction<MyModel>());
                });
                var redoUsedOnce = map.Get<Button>("Redo").SetOnClickAction(delegate {
                    store.Dispatch(new RedoAction<MyModel>());
                });
                return redoUsedOnce;
            }

            private void RebuildCircleCanvas(MyModel model) {
                Log.MethodEntered();

                // Create a lookup dictionary for all canvas children to quickly get the GOs for each circle id:
                circleGOs = circleCanvas.GetChildren().ToDictionary(go => go.GetComponent<CircleUi>().circleId, go => go);

                // Check which entries from the UI are no longer in the model, which means they were removed in the 
                // model and now have to be removed in the UI too:
                var ciclesToBeRemovedFromUi = circleGOs.Keys.Except(model.circles.Map(c => c.id));
                foreach (var circleToDelete in ciclesToBeRemovedFromUi) {
                    circleGOs[circleToDelete].Destroy();
                }

                // Check that new added circles to the model also get a new UI GO representing them:
                var selectedCircleId = model.selectedCircle;
                foreach (var circle in model.circles) {
                    if (circleGOs.TryGetValue(circle.id, out GameObject circleGO)) {
                        // The cicle from the model is already in the UI, just update its position and size:
                        SetCircleUiPosAndSizeFromCircleModel(circle, circleGO.GetComponent<RectTransform>());
                        circleGO.GetComponent<CircleUi>().VisualizeCircleAsSelected(circle.id.Equals(selectedCircleId));
                    } else { // The circle in the model is not yet in the UI, add a new GO:
                        var newCircleGo = CreateNewCircleUi(circle);
                        circleCanvas.AddChild(newCircleGo); // Add to UI
                        circleGOs.Add(circle.id, newCircleGo); // Add to lookup dictionary (see above)
                    }
                }

            }

            /// <summary> Creates a new UI circle based on the circle values in the model </summary>
            private GameObject CreateNewCircleUi(MyCircle circle) {
                Log.MethodEnteredWith(circle);
                var newCircleUi = ResourcesV2.LoadPrefab("CircleUi");
                var map = newCircleUi.GetLinkMap();
                CircleUi circleUi = map.Get<CircleUi>("CircleUi");
                circleUi.circleId = circle.id;
                circleUi.OnCircleClicked = (string clickedCircleId, bool wasRightClick) => {
                    store.Dispatch(new SelectCicleAction() { selectedCircleId = clickedCircleId });
                    if (wasRightClick) { ShowDiameterEditUi(); }
                };
                circleUi.VisualizeCircleAsSelected(circle.id.Equals(store.GetState().selectedCircle));
                SetCircleUiPosAndSizeFromCircleModel(circle, circleUi.GetComponent<RectTransform>());
                return newCircleUi;
            }

            /// <summary> Will show a diameter edit UI that can modify the current selected circle </summary>
            private void ShowDiameterEditUi() {
                Log.MethodEntered();

                var editUi = targetView.GetViewStack().ShowView("DiameterEditUi");
                var map = editUi.GetLinkMap();

                // Get a selector that always returns the current selected circle:
                var selectedCircleId = store.SelectElement(s => s.selectedCircle);
                // Get a selector that always returns the latest state of the current selected circle:
                var selectedCicrle = store.SelectListEntry(s => s.circles, c => c.id == selectedCircleId());

                Text text = map.Get<Text>("CircleInfoText");
                text.SubscribeToStateChanges(store, x => x.selectedCircle, (_) => {
                    text.text = $"Adjust diameter of circle at ({selectedCicrle().x}, {selectedCicrle().y})";
                });

                Slider slider = map.Get<Slider>("DiameterSlider");
                slider.SubscribeToStateChanges(store, model => selectedCicrle().diameter);

                // Instantly render the changed diameter in the UI:
                slider.SetOnValueChangedAction((newDiameter) => {
                    // Use the GameObject lookup dictionary to access the UI GO efficiently:
                    var selectedCircleGo = circleGOs[selectedCircleId()];
                    SetSizeOfCircleUi(selectedCircleGo.GetComponent<RectTransform>(), newDiameter);
                    return true;
                });

                // Only persist the diameter change after a delay to avoid spamming the store with dispatched actions:
                slider.AddOnValueChangedActionThrottled((newDiameter) => {
                    store.Dispatch(new ChangeDiameterAction() {
                        targetCircleId = selectedCircleId(),
                        newDiameter = newDiameter
                    });
                });
            }

            /// <summary> Takes the position and size from the model and applies them in the UI </summary>
            private static void SetCircleUiPosAndSizeFromCircleModel(MyCircle circleModel, RectTransform circleUi) {
                SetSizeOfCircleUi(circleUi, circleModel.diameter);
                // The canvas should have 0,0 at bottom left so that its is aligned with the input system coordinates:
                circleUi.SetAnchorsBottomLeft();
                circleUi.localPosition = new Vector2(circleModel.x, circleModel.y);
            }

            private static void SetSizeOfCircleUi(RectTransform circleUi, float diameter) {
                circleUi.localScale = new Vector3().SetXYZ(diameter);
            }

        }

        #region Actions that can be send to the Redux data store

        private class ChangeDiameterAction {
            public string targetCircleId;
            public float newDiameter;
        }

        private class AddCircleAction {
            public MyCircle newCircle;
        }

        private class SelectCicleAction {
            public string selectedCircleId;
        }

        #endregion

        #region Reducers of the Redux data store that will process the actions

        private static MyModel MyReducer(MyModel previousState, object action) {
            bool modelChanged = false;
            var selectedCircle = previousState.MutateField(previousState.selectedCircle, action, CircleSelectionReducer, ref modelChanged);
            var circles = previousState.MutateField(previousState.circles, action, CirclesReducer, ref modelChanged);
            if (modelChanged) { return new MyModel(selectedCircle, circles); }
            return previousState;
        }

        private static string CircleSelectionReducer(MyModel parent, string oldCircleSelection, object action) {
            if (action is SelectCicleAction sel) { return sel.selectedCircleId; }
            if (action is AddCircleAction add) { return add.newCircle.id; } // Auto-select newly added circles
            if (action is ChangeDiameterAction d) { return d.targetCircleId; }
            return oldCircleSelection;
        }

        private static ImmutableList<MyCircle> CirclesReducer(MyModel parent, ImmutableList<MyCircle> circles, object action) {
            circles = circles.MutateEntries(action, CircleReducer);
            if (action is AddCircleAction a) { circles = circles.AddOrCreate(a.newCircle); }
            return circles;
        }

        private static MyCircle CircleReducer(MyCircle c, object action) {
            if (action is ChangeDiameterAction a && a.targetCircleId == c.id) {
                return new MyCircle(c.id, c.x, c.y, a.newDiameter);
            }
            return c;
        }

        #endregion

        #region Model which is immutable and can only be changed through the Redux datastore

        internal class MyModel {

            public readonly string selectedCircle;
            public readonly ImmutableList<MyCircle> circles;

            public MyModel(string selectedCircle, ImmutableList<MyCircle> circles) {
                this.selectedCircle = selectedCircle;
                this.circles = circles;
            }

        }

        internal class MyCircle {

            public readonly string id;
            public readonly float x;
            public readonly float y;
            public readonly float diameter;

            public MyCircle(string id, float x, float y, float diameter) {
                this.id = id;
                this.x = x;
                this.y = y;
                this.diameter = diameter;
            }

            public override string ToString() { return $"Circle at ({x},{y}) with diameter={diameter}"; }

        }

        #endregion

    }

}