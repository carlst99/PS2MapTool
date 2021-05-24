using CliFx.Extensibility;

namespace PS2MapTool.Validators
{
    public class MaxParallelValidator : BindingValidator<int>
    {
        public override BindingValidationError? Validate(int value)
        {
            if (value < 1)
                return new BindingValidationError("The minimum parallelism level is 1.");
            else if (value > 16)
                return new BindingValidationError("The maximum parallelism level is 16.");
            else
                return null;
        }
    }
}
