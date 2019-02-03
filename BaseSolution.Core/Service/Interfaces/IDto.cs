using System;

namespace BaseSolution.Core.Service.Interfaces
{
    public interface IDto
    {
        Guid Id { get; set; }
        DateTime CreatedDate { get; set; }
        DateTime UpdatedDate { get; set; }
    }
}