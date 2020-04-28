namespace com.csutil.model {

    public interface IFeatureFlag {

        string id { get; set; }
        int rolloutPercentage { get; set; }
        int requiredXp { get; set; }
        IFeatureFlagLocalState localState { get; set; }

    }

    public interface IFeatureFlagLocalState {
        int randomPercentage { get; set; }
    }

}