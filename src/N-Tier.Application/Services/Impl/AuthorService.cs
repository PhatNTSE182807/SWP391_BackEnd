using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapster;
using N_Tier.Application.Models.Author;
using N_Tier.DataAccess.Repositories;

namespace N_Tier.Application.Services.Impl;

public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository _authorRepository;

    public AuthorService(IAuthorRepository authorRepository)
    {
        _authorRepository = authorRepository;
    }

    public async Task<List<AuthorResponseModel>> GetAllAuthorsAsync()
    {
        var authors = await _authorRepository.GetAllAsync(a => true);
        return authors.Adapt<List<AuthorResponseModel>>();
    }

    public async Task<AuthorResponseModel> GetAuthorByIdAsync(Guid id)
    {
        var author = await _authorRepository.GetByIdAsync(id);
        return author.Adapt<AuthorResponseModel>();
    }
}
