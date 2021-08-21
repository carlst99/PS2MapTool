using PS2MapTool.Areas;

namespace PS2MapTool.Cli
{
    public static class Formatter
    {
        public static string Success(string value) => $"[lightgreen]{ value }[/]";
        public static string Error(string value) => $"[red]Error: { value }[/]";
        public static string Warning(string value) => $"[darkorange]Warning: { value }[/]";
        public static string World(AssetZone value) => $"[yellow]{ value }[/]";
        public static string World(string value) => $"[yellow]{ value }[/]";
        public static string Lod(Lod value) => $"[fuchsia]{ value }[/]";
        public static string Lod(string value) => $"[fuchsia]{ value }[/]";
        public static string NdzType(NoDeployType value) => $"[lime]{ value }[/]";
        public static string NdzType(string value) => $"[lime]{ value }[/]";
        public static string Path(string value) => $"[aqua]{ value }[/]";
    }
}
