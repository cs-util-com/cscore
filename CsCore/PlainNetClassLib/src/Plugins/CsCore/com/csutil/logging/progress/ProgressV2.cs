using System;

namespace com.csutil.progress {

    public class ProgressV2 : Progress<double>, IProgress {

        public readonly string id;
        public bool disposed { get; private set; }

        private double _totalCount;
        public double totalCount {
            get { return _totalCount; }
            set {
                if (disposed) { throw new ObjectDisposedException($"Progress {id} already disposed!"); }
                if (value != _totalCount) {
                    var latestCount = this.GetCount();
                    _totalCount = value;
                    this.SetCount(latestCount);
                }
            }
        }

        private double _percent;
        public double percent {
            get { return _percent; }
            set {
                AssertV3.IsTrue((int)_percent <= (int)value, () => $"Warning: current {_percent}% > new {value}%");
                if (disposed) { throw new ObjectDisposedException($"Progress {id} already disposed!"); }
                if (value != _percent) {
                    _percent = value;
                    if (totalCount > 0) { ((IProgress<double>)this).Report(this.GetCount()); }
                }
            }
        }

        public override string ToString() {
            return $"{id} at {Math.Round(percent, 2)}% ({this.GetCount()}/{totalCount})";
        }

        public ProgressV2(string id, double totalCount) {
            this.id = id;
            this.totalCount = totalCount;
        }

        protected override void OnReport(double count) {
            this.SetCount(count);
            base.OnReport(count);
        }

        public void Dispose() {
            percent = 100;
            disposed = true;
        }

    }

}