using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.FluentValidation;

namespace SlaytonNichols.Common.ServiceStack.Auth;
public class CustomRegistrationValidator : RegistrationValidator
{
    public CustomRegistrationValidator()
    {
        RuleSet(ApplyTo.Post, () =>
        {
            RuleFor(x => x.DisplayName).NotEmpty();
            RuleFor(x => x.ConfirmPassword).NotEmpty();
        });
    }
}