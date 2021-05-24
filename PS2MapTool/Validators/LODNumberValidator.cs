using CliFx.Extensibility;
using System.Collections.Generic;

namespace PS2MapTool.Validators
{
    public class LODNumberValidator : BindingValidator<IEnumerable<int>>
    {
        public override BindingValidationError? Validate(IEnumerable<int> value)
        {
            foreach (int lod in value)
            {
                if (lod < 0)
                    return new BindingValidationError("LOD values cannot be lower than 0.");
            }

            return null;
        }
    }
}
