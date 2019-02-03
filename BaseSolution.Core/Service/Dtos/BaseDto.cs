using System;
using BaseSolution.Core.Service.Interfaces;

namespace BaseSolution.Core.Service.Dtos
{
    public abstract class BaseDto : IDto
    {
        public Guid Id { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }
    }
}