namespace PS2MapTool.Core.Areas
{
    /// <summary>
    /// Stores no-deploy area specific data, with properties selectively chosen for our use-case.
    /// </summary>
    public record NoDeployAreaInfo
    {
        /// <summary>
        /// The type of deployable object that must be in use to activate this no-deploy zone.
        /// </summary>
        public NoDeployType Requirement { get; init; }

        /// <summary>
        /// The facility that this zone is tied to.
        /// </summary>
        public int FacilityId { get; init; }

        public override string ToString()
        {
            return $"Requirement: {Requirement} | Facility ID: {FacilityId}";
        }
    }
}
