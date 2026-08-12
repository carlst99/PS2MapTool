using CliFx.Activation;
using System;
using System.Collections.Generic;

namespace PS2MapTool.Cli.Validators;

public class MaxParallelValidator : InputValidator<int>
{
    public override IEnumerable<InputValidationError> Validate(int value)
    {
        if (value < 1)
            return [new InputValidationError("The minimum parallelism level is 1.")];

        if (value > Environment.ProcessorCount)
            return [new InputValidationError($"The maximum parallelism level is {Environment.ProcessorCount}.")];

        return [];
    }
}
