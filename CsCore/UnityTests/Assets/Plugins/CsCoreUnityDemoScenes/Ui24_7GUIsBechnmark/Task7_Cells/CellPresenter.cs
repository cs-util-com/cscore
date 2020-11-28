using com.csutil.model.immutable;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests.Task7 {

    /// <summary> Connects a cell UI with a cell model </summary>
    internal class CellPresenter : MonoBehaviour {

#pragma warning disable 0649 // Variable is never assigned to, and will always have its default value
        public Text value;
        public InputField formulaInput;
        public GameObject inputUi;

        public bool isSubscribed = false;
        public DataStore<CellsModel> store;
        public CellPos cellPos;
#pragma warning restore 0649 // Variable is never assigned to, and will always have its default value

        private void OnEnable() {
            formulaInput.SetOnValueChangedActionThrottled(SetFormulaOfCell);
        }

        /// <summary> Will be called by the UI button attached to the same GameObject </summary>
        public void OnCellClicked() {
            var t = Log.MethodEnteredWith(cellPos);
            // If the cell was not used before, initialize it with 0:
            if (!store.GetState().cells.ContainsKey(cellPos)) { SetFormulaOfCell("0"); }
            store.Dispatch(new MyActions.SelectCell(cellPos));
            formulaInput.SelectV2();
            Log.MethodDone(t);
        }

        private void SetFormulaOfCell(string newFormula) {
            var t = Log.MethodEntered(newFormula);
            try {
                store.Dispatch(new MyActions.SetCell(cellPos.columnId, cellPos.rowNr, newFormula));
                // Show latest value via toast while formula is being edited:
                var cell = store.GetState().cells[cellPos];
                if (cell.formula != "" + cell.value) { Toast.Show($"{cellPos}: {cell.value}"); }
            }
            catch (Exception e) {
                Toast.Show("Invalid input: " + e.Message); // Or indicate error in cell input UI
            }
            Log.MethodDone(t);
        }

        internal void SubscribeToSetCell() {
            if (store == null) { throw Log.e("store not set"); }
            if (cellPos.columnId.IsNullOrEmpty()) { throw Log.e("cellPos not set"); }
            if (isSubscribed) { throw Log.e("Already setup", gameObject); }
            isSubscribed = true;
            this.SubscribeToStateChanges(store, s => s.cells.GetValue(cellPos, null), UpdateUi);
        }

        private void UpdateUi(Cell newCellValue) {
            value.text = "" + newCellValue?.value;
            formulaInput.text = newCellValue?.formula;
            inputUi.SetActiveV2(newCellValue != null && newCellValue.isSelected);
        }

    }

}