using FluentValidation;
using Users.Business.DTOs;

namespace Users.Business.Validators;

public sealed record UserValidators(
    IValidator<CreateUserDto> Create,
    IValidator<UpdateUserProfileDto> Update,
    IValidator<UserFilterDto> Filter);
