using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using N_Tier.Application.Models.Author;

namespace N_Tier.Application.Services;

public interface IAuthorService
{
    Task<List<AuthorResponseModel>> GetAllAuthorsAsync();
    Task<AuthorResponseModel> GetAuthorByIdAsync(Guid id);
}
