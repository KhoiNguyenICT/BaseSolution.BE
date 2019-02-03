using System;
using System.ComponentModel.DataAnnotations;
using BaseSolution.Core.Models.Interfaces;

namespace BaseSolution.Core.Models.Entities
{
    public abstract class BaseEntity : IEntity
    {
        private DateTime _createdDate;
        private DateTime _updatedDate;

        [Key]
        public Guid Id { get; set; }

        public DateTime CreatedDate
        {
            get => _createdDate;
            set
            {
                if (value.Kind == DateTimeKind.Utc)
                {
                    _createdDate = value;
                }
                _createdDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }
        }

        public DateTime UpdatedDate
        {
            get => _updatedDate;
            set
            {
                if (value.Kind == DateTimeKind.Utc)
                {
                    _updatedDate = value;
                }
                _updatedDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }
        }
    }
}