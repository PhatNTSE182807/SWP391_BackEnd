using FluentValidation;
using N_Tier.Application.Models.Search;

namespace N_Tier.Application.Models.Validators;

public class SearchPaperRequestValidator : AbstractValidator<SearchPaperRequestModel>
{
    public SearchPaperRequestValidator()
    {
        RuleFor(x => x.Q)
            .MaximumLength(500)
            .WithMessage("Search query must not exceed 500 characters");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.Size)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Size must be between 1 and 100");

        RuleFor(x => x.From)
            .GreaterThan(1900)
            .LessThanOrEqualTo(2100)
            .When(x => x.From.HasValue)
            .WithMessage("From year must be between 1900 and 2100");

        RuleFor(x => x.To)
            .GreaterThan(1900)
            .LessThanOrEqualTo(2100)
            .When(x => x.To.HasValue)
            .WithMessage("To year must be between 1900 and 2100");

        RuleFor(x => x)
            .Must(x => !x.From.HasValue || !x.To.HasValue || x.From.Value <= x.To.Value)
            .WithMessage("From year must be less than or equal to To year");
    }
}
