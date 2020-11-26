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
            // Call some unit tests manually before the UI is shown:
            // TODO use xunit runner instead to automatically call these tests here?
            CellsModelTests.TestDataTable();
            CellsModelTests.TestFromAndToRowName();
            CellsModelTests.TestDataStoreTransitiveChanges();

            CellsModel model = new CellsModel(ImmutableDictionary<CellPos, Cell>.Empty);
            Middleware<CellsModel> logging = Middlewares.NewLoggingMiddleware<CellsModel>();
            UndoRedoReducer<CellsModel> undoLogic = new UndoRedoReducer<CellsModel>();
            DataStore<CellsModel> store = new DataStore<CellsModel>(undoLogic.Wrap(MyReducers.MyReducer), model, logging);

            MyPresenter presenter = new MyPresenter();
            presenter.targetView = viewStack.ShowView("7GUIs_Task7_Cells");
            await presenter.LoadModelIntoView(store);
        }

        private class MyPresenter : Presenter<DataStore<CellsModel>> {

            private GameObject uiRows;

            public GameObject targetView { get; set; }
            public Task OnLoad(DataStore<CellsModel> store) {

                var map = targetView.GetLinkMap();
                uiRows = map.Get<GameObject>("Rows");
                store.AddStateChangeListener(m => m.cells, Cells => {
                    SyncUiRowCountWithModelRowCount(Cells);
                    SyncUiColumnCountWithModelColumnCount(store, Cells);
                    LinkCellUiToNewAddedCells(Cells);
                });

                // Simulate changes in the model to check if the UI updates correctly:
                store.Dispatch(new MyActions.SetCell("C", 3, "1 + 1"));
                store.Dispatch(new MyActions.SetCell("D", 4, "1 + C3"));
                store.Dispatch(new MyActions.SetCell("E", 5, "1 + D4"));
                store.Dispatch(new MyActions.SetCell("C", 3, "2"));

                return Task.FromResult(true);
            }

            // For each cell that is new in the model make sure its UI listens to its changes:
            private void LinkCellUiToNewAddedCells(ImmutableDictionary<CellPos, Cell> Cells) {
                var cellsWithUi = GetSubscribedCells(uiRows).Map(c => c.cellPos);
                var newCellsInModel = Cells.Keys.Except(cellsWithUi);
                foreach (var newCellInModel in newCellsInModel) {
                    GetCellUi(newCellInModel).GetComponent<CellPresenter>().SubscribeToSetCell();
                }
            }

            private void SyncUiColumnCountWithModelColumnCount(DataStore<CellsModel> store, ImmutableDictionary<CellPos, Cell> Cells) {
                // Get the biggest column count in the cells model:
                int maxColumn = Cells.Keys.Map(k => k.columnNr).Max() + 1;
                // For each UI row check that they all have the same nr of UI cells:
                var rowNr = 0;
                foreach (var row in uiRows.GetChildren()) {
                    var columnCount = row.GetChildCount();
                    while (columnCount < maxColumn) {
                        string columnId = CellPos.ToColumnName(columnCount);
                        if (rowNr == 0) {
                            var columnNameUi = row.AddChild(ResourcesV2.LoadPrefab("7GUIs_Task7_ColumnName"));
                            columnNameUi.GetComponentInChildren<Text>().text = columnId;
                        } else {
                            var cellGo = row.AddChild(ResourcesV2.LoadPrefab("7GUIs_Task7_CellUiEntry"));
                            var cell = cellGo.GetComponent<CellPresenter>();
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
                int maxRow = Cells.Keys.Map(k => k.rowNr).Max() + 1;

                // Add UI rows based on model row count (until UI has same row count):
                while (uiRows.GetChildCount() <= maxRow) {
                    var rowUi = uiRows.AddChild(ResourcesV2.LoadPrefab("7GUIs_Task7_Row"));
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
                try { return uiRows.GetChild(cellPos.rowNr).GetChild(cellPos.columnNr); }
                catch (System.Exception e) { throw Log.e("Could not get cell at " + cellPos, e); }
            }

        }

    }

}