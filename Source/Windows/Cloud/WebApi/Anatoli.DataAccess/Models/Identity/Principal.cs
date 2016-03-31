using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anatoli.DataAccess.Models.Identity
{
    public class Principal 
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(200)]
        public string Title { get; set; }
        public virtual ICollection<PrincipalPermission> PrincipalPermissions { get; set; }
        public virtual ICollection<Group> Groups { get; set; }
        //public virtual ICollection<Stock> Stocks { get; set; }
        //[ForeignKey("User")]
        //public string UserId { get; set; }
        public virtual User User { get; set; }
        public virtual Group Group { get; set; }
        [ForeignKey("ApplicationOwner")]
        public Guid ApplicationOwnerId { get; set; }
        public virtual ApplicationOwner ApplicationOwner { get; set; }
    }
}