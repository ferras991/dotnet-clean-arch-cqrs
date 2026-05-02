using Application.UseCases.Clients.RegisterClient;
using FluentValidation.TestHelper;

namespace Unit.Application.UseCases.Clients.RegisterClient;

public sealed class RegisterClientCommandValidatorTests
{
    private readonly RegisterClientCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldHaveNoErrors()
    {
        var command = new RegisterClientCommand("John", "Doe", "john@example.com");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WhenFirstNameIsEmpty_ShouldHaveError(string firstName)
    {
        var command = new RegisterClientCommand(firstName, "Doe", "john@example.com");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.FirstName);
    }

    [Fact]
    public void Validate_WhenFirstNameExceedsMaxLength_ShouldHaveError()
    {
        var command = new RegisterClientCommand(new string('a', 101), "Doe", "john@example.com");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.FirstName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WhenLastNameIsEmpty_ShouldHaveError(string lastName)
    {
        var command = new RegisterClientCommand("John", lastName, "john@example.com");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.LastName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    public void Validate_WhenEmailIsInvalid_ShouldHaveError(string email)
    {
        var command = new RegisterClientCommand("John", "Doe", email);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Email);
    }
}