using PS2MapTool.Census.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PS2MapTool.Census;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(IEnumerable<LatticeLink>))]
[JsonSerializable(typeof(IEnumerable<MapHex>))]
[JsonSerializable(typeof(IEnumerable<MapRegion>))]
internal partial class JsonContext : JsonSerializerContext;
