namespace PS2MapTool.Cli
{
    public static class Formatter
    {
        public static string Success(string value) => $"[lightgreen]{ value }[/]";
        public static string Error(string value) => $"[red]{ value }[/]";
        public static string World(string value) => $"[yellow]{ value }[/]";
        public static string Lod(string value) => $"[fuchsia]{ value }[/]";
        public static string Path(string value) => $"[aqua]{ value }[/]";
    }
}
