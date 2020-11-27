using com.csutil.model.immutable;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace com.csutil.tests.Task7 {

    public class CellsModel {
        public readonly ImmutableDictionary<CellPos, Cell> cells;
        public CellsModel(ImmutableDictionary<CellPos, Cell> cells) { this.cells = cells; }
    }

    public struct CellPos {
        public readonly string columnId;
        public readonly int rowNr;
        internal readonly int columnNr;

        public CellPos(string columnId, int rowNr) {
            this.columnId = columnId;
            this.rowNr = rowNr;
            this.columnNr = ToColumnNr(columnId);
        }

        public override string ToString() { return $"{columnId}{rowNr}"; }

        public static int ToColumnNr(IEnumerable<char> columnId) {
            var offset = 0;
            var result = 0;
            foreach (char c in columnId.Reverse()) { // Start with most right char
                int index = char.ToUpper(c) - 'A';
                result = result + offset + index;
                offset += 26; // 26 different letters
            }
            return result;
        }

        public static string ToColumnName(int columnNr) {
            string rowString = "";
            do {
                var n = columnNr % 26;
                var character = (char)(n + 'A');
                rowString = character + rowString;
                columnNr = columnNr - 26 - n;
            } while (columnNr >= 0);
            return rowString;
        }

    }

    public class Cell {

        public readonly CellPos pos;
        /// <summary> eg "(1 + A1) * B3 / 2" </summary>
        public readonly string formula;
        public readonly double value;
        public readonly ImmutableHashSet<CellPos> dependentCells;
        public readonly bool isSelected;

        public Cell(CellPos pos, string formula, ImmutableHashSet<CellPos> dependentCells, double value, bool isSelected) {
            this.pos = pos;
            this.formula = formula;
            this.dependentCells = dependentCells;
            this.value = value;
            this.isSelected = isSelected;
        }

        public static double Calculate(string formula, ImmutableDictionary<CellPos, Cell> allCells, int depth = 0) {
            if (depth > 100) { throw new MyActions.SetCell.SelfRefException("Resolution loop in " + formula); }
            IEnumerable<Match> matches = GetVariables(formula);
            foreach (Match m in matches) {
                var cell = allCells[ToCellPos(m.Value)];
                double valToInsert = Calculate(cell.formula, allCells, depth + 1);
                formula = formula.Replace(m.Value, "" + valToInsert);
            }
            return Numbers.Calculate(formula);
        }

        public static IEnumerable<CellPos> CalcDependentCells(string formula) {
            return GetVariables(formula).Map(m => ToCellPos(m.Value));
        }

        private static IEnumerable<Match> GetVariables(string formula) {
            var regex = "([A-Za-z]+[0-9]+)"; // See https://regex101.com/r/uS6cH4/18
            return Regex.Matches(formula, regex).Cast<Match>();
        }

        /// <summary> Takes a string like AA34 and returns the CellPos for it </summary>
        private static CellPos ToCellPos(string cellPosString) {
            var regex = "([A-Za-z]+)([0-9]+)"; // See https://regex101.com/r/uS6cH4/19
            var match = Regex.Match(cellPosString, regex);
            AssertV2.AreEqual(3, match.Groups.Count);
            if (match.Groups.Count != 3) { throw Log.e("Invalid CellPos: " + cellPosString); }
            Group g0 = match.Groups[1];
            Group g1 = match.Groups[2];
            return new CellPos(g0.Value, int.Parse(g1.Value));
        }

    }

    public class MyActions {
        /// <summary> Allows to add or update a cell s</summary>
        public class SetCell {
            /// <summary> The cell entry to update </summary>
            public readonly CellPos pos;
            /// <summary> The new formula to set for the cell </summary>
            public readonly string newFormula;
            public SetCell(string columnId, int rowNr, string value) {
                this.pos = new CellPos(columnId, rowNr);
                this.newFormula = value;
            }

            public class SelfRefException : Exception {
                public SelfRefException(string message) : base(message) { }
            }

        }

        public class SelectCell {
            public readonly CellPos pos;
            public SelectCell(CellPos pos) { this.pos = pos; }
        }
    }

    public class MyReducers {

        public static CellsModel MyReducer(CellsModel previousState, object action) {
            var changed = false;
            var newCells = previousState.MutateField(previousState.cells, action, CellsReducer, ref changed);
            if (changed) { return new CellsModel(newCells); }
            return previousState;
        }

        private static ImmutableDictionary<CellPos, Cell> CellsReducer(CellsModel parent, ImmutableDictionary<CellPos, Cell> oldCells, object action) {
            var newCells = oldCells.MutateEntries(action, (cell, _) => {
                return ReduceCell(oldCells, cell, action);
            });

            // MyActions.SetCell can also be used to add new cells to the dictionary:
            if (action is MyActions.SetCell s && !newCells.ContainsKey(s.pos)) {
                newCells = newCells.Add(s.pos, NewCell(s, newCells, isSelected: false));
            }

            // Update all cells that depend on the changed cell:
            int maxDependencySteps = 100;
            for (int i = 0; i < maxDependencySteps; i++) {
                if (oldCells == newCells) { return oldCells; }
                // As long as there are cells changed because their dependent cells changed repeat updating
                var c = newCells;
                newCells = newCells.MutateEntries(action, (cell, _) => {
                    return UpdateDependentCell(cell, oldCells, newCells);
                });
                oldCells = c;
            }
            throw new StackOverflowException("Cells have to many transitive dependencies, might be a loop");
        }

        private static Cell ReduceCell(ImmutableDictionary<CellPos, Cell> allCells, Cell cell, object action) {
            if (action is MyActions.SetCell s && Equals(s.pos, cell.pos)) {
                return NewCell(s, allCells, cell.isSelected);
            }
            if (action is MyActions.SelectCell c) {
                bool select = Equals(c.pos, cell.pos);
                if (cell.isSelected != select) {
                    return new Cell(cell.pos, cell.formula, cell.dependentCells, cell.value, select);
                }
            }
            return cell;
        }

        private static Cell NewCell(MyActions.SetCell s, ImmutableDictionary<CellPos, Cell> allCells, bool isSelected) {
            var formula = s.newFormula;
            var deps = ImmutableHashSet.CreateRange(Cell.CalcDependentCells(formula));
            return new Cell(s.pos, formula, deps, Cell.Calculate(formula, allCells), isSelected);
        }

        private static Cell UpdateDependentCell(Cell cell, ImmutableDictionary<CellPos, Cell> oldCells, ImmutableDictionary<CellPos, Cell> newCells) {
            foreach (CellPos d in cell.dependentCells) {
                /* If a cell changed that the current cell depends on, then also the current cell must 
                 * be recalculated so that all cells that are influenced by the change update correctly */
                if (oldCells[d] != newCells[d]) {
                    return new Cell(cell.pos, cell.formula, cell.dependentCells, Cell.Calculate(cell.formula, newCells), cell.isSelected);
                }
            }
            return cell;
        }

    }

}