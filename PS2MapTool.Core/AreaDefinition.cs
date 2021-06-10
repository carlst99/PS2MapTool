namespace PS2MapTool.Core
{
    /// <summary>
    /// Defines an area, with properties selectively chosen for the purpose of storing no-deploy areas.
    /// </summary>
    public record AreaDefinition
    {
        /// <summary>
        /// The X-coordinate of the area.
        /// </summary>
        public float X { get; init; }

        /// <summary>
        /// The Y-coordinate of the area.
        /// </summary>
        public float Z { get; init; }

        /// <summary>
        /// The radius of the area.
        /// </summary>
        public float Radius { get; init; }

        /// <summary>
        /// The name of the area.
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Specific no-deploy area info
        /// </summary>
        public NoDeployAreaInfo Info { get; set; }

        /// <summary>
        /// Initializes a new <see cref="AreaDefinition"/> object.
        /// </summary>
        public AreaDefinition()
        {
            Name = string.Empty;
            Info = new NoDeployAreaInfo();
        }

        public override string ToString()
        {
            return $"X: {X} | Z: {Z} | Radius: {Radius} | Name: {Name}";
        }
    }
}
