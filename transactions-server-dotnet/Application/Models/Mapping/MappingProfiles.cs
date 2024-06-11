using AutoMapper;
using TransactionService.Domain.Types;

namespace TransactionService.Application.Models;

public class MappingProfiles: Profile {
    public MappingProfiles()
    {
        CreateMap<Account, AccountDto>().ForMember(x => x.Amount, opt => opt.MapFrom(src => Math.Round(src.Amount, 2)));; // no dto -> db for now
        CreateMap<Category, CategoryDto>(); // no dto -> db for now
        CreateMap<Currency, CurrencyDto>(); // no dto -> db for now
        CreateMap<Transaction, TransactionDto>();
        CreateMap<TransactionDto, Transaction>().ForMember(x => x.Amount, opt => opt.MapFrom(src => Math.Round(src.Amount, 2)));
    }
}