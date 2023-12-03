using com.csutil.model.immutable;
using com.csutil.ui;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests.Task7 {

    internal static class Task7_Cells {

        public static async Task ShowIn(ViewStack viewStack) {
            // Call model unit tests manually before the UI is shown:
            CellsModelTests.TestFromAndToRowName();
            CellsModelTests.TestDataStoreTransitiveChanges();

            CellsModel model = new CellsModel(ImmutableDictionary<CellPos, Cell>.Empty);
            Middleware<CellsModel> logging = Middlewares.NewLoggingMiddleware<CellsModel>();
            UndoRedoReducer<CellsModel> undoLogic = new UndoRedoReducer<CellsModel>();
            DataStore<CellsModel> store = new DataStore<CellsModel>(undoLogic.Wrap(CellsReducers.MainReducer), model, logging);

            MyPresenter presenter = new MyPresenter();
            presenter.targetView = viewStack.ShowView("7GUIs_Task7_Cells");
            await presenter.LoadModelIntoView(store);

            await TaskV2.Delay(2000);
            Toast.Show("Now simulating some table model changes..");
            // Simulate changes in the model to check if the UI updates correctly:
            CellsModelTests.SimulateSomeChangesInModel(store);
        }

        private class MyPresenter : Presenter<DataStore<CellsModel>> {

            public GameObject targetView { get; set; }

            private GameObject uiRows;
            private DataStore<CellsModel> store;

            public Task OnLoad(DataStore<CellsModel> store) {
                this.store = store;
                var map = targetView.GetLinkMap();
                map.Get<Button>("Undo").SetOnClickAction(delegate {
                    store.Dispatch(new UndoAction<CellsModel>());
                });
                map.Get<Button>("Redo").SetOnClickAction(delegate {
                    store.Dispatch(new RedoAction<CellsModel>());
                });
                map.Get<Button>("AddManyEntries").SetOnClickAction(async delegate {
                    await CellsModelTests.SimulateManyChangesInModel(store);
                });
                uiRows = map.Get<GameObject>("Rows");

                store.AddStateChangeListenerDebounced(m => m.cells, UpdateCellsGridUi, delayInMs: 300);
                return Task.FromResult(true);
            }

            private void UpdateCellsGridUi(ImmutableDictionary<CellPos, Cell> Cells) {
                var cellsGridUiUpdateTiming = Log.MethodEntered();
                SyncUiRowCountWithModelRowCount(Cells);
                SyncUiColumnCountWithModelColumnCount(Cells);
                LinkCellUiToNewAddedCells(Cells);
                Log.MethodDone(cellsGridUiUpdateTiming);
            }

            // For each cell that is new in the model make sure its UI listens to its changes:
            private void LinkCellUiToNewAddedCells(ImmutableDictionary<CellPos, Cell> Cells) {
                var cellsWithUi = GetSubscribedCells(uiRows).Map(c => c.cellPos);
                var newCellsInModel = Cells.Keys.Except(cellsWithUi);
                foreach (var newCellInModel in newCellsInModel) {
                    GetCellUi(newCellInModel).GetComponentV2<CellPresenter>().SubscribeToSetCell();
                }
            }

            private void SyncUiColumnCountWithModelColumnCount(ImmutableDictionary<CellPos, Cell> Cells) {
                // Get the biggest column count in the cells model:
                int maxColumn = Cells.Keys.Map(k => k.columnNr).DefaultIfEmpty(-1).Max() + 1;

                // For each UI row check that they all have the same nr of UI cells:
                var rowNr = 0;
                foreach (var row in uiRows.GetChildren()) {
                    var columnCount = row.GetChildCount() - 1;
                    while (columnCount < maxColumn) {
                        string columnId = CellPos.ToColumnName(columnCount);
                        if (rowNr == 0) {
                            var columnNameUi = row.AddChild(ResourcesV2.LoadPrefab("ColumnName"));
                            columnNameUi.GetComponentInChildren<Text>().text = columnId;
                        } else {
                            var cellGo = row.AddChild(ResourcesV2.LoadPrefab("CellUiEntry"));
                            var cell = cellGo.GetComponentV2<CellPresenter>();
                            cell.cellPos = new CellPos(columnId, rowNr);
                            cell.store = store;
                        }
                        columnCount++;
                    }
                    rowNr++;
                }
            }

            private void SyncUiRowCountWithModelRowCount(ImmutableDictionary<CellPos, Cell> Cells) {
                // The biggest row count in the model:
                int maxRow = Cells.Keys.Map(k => k.rowNr).DefaultIfEmpty(-1).Max() + 1;

                // Add UI rows based on model row count (until UI has same row count):
                while (uiRows.GetChildCount() <= maxRow) {
                    var rowUi = uiRows.AddChild(ResourcesV2.LoadPrefab("Row"));
                    // Set the row nr in the row UIs first cell:
                    rowUi.GetChild(0).GetComponentInChildren<Text>().text = "" + (uiRows.GetChildCount() - 1);
                } // Repeat until the count of UI rows is same as model 

                // Removed UI rows that are no longer in the datamodel:
                while (uiRows.GetChildCount() > maxRow) {
                    var lastRow = uiRows.GetChildrenIEnumerable().Last().Destroy();
                } // Repeat until the count of UI rows is same as model
            }

            /// <summary> Returns all cell presenter in the childs of the passed GameObject that are 
            /// subscribed to a model cell </summary>
            private static IEnumerable<CellPresenter> GetSubscribedCells(GameObject go) {
                return go.GetComponentsInChildren<CellPresenter>().Filter(x => x.isSubscribed);
            }

            private GameObject GetCellUi(CellPos cellPos) {
                try { return uiRows.GetChild(cellPos.rowNr).GetChild(cellPos.columnNr + 1); }
                catch (System.Exception e) { throw Log.e("Could not get cell at " + cellPos, e); }
            }

        }

    }

}